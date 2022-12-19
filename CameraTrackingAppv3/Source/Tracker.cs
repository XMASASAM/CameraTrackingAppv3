using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{



    class GeneralTracker
    {
        Tracker tracker;
        bool f_infrared = false;
        bool f_first = true;
        int step_detect = 0;
      //  Vec2d pre_vel;
        Vec2d corrected_vel;
        Vec2d corrected_center_point;
        public GeneralTracker(Mat frame)
        {
            f_infrared = DetectColorORGray(frame);
            if (f_infrared)
                tracker = new TrackerInfrared();
            else
                tracker = new TrackerOpticalFlow();

        }

        public bool Update(Mat frame)
        {

            if (step_detect++ > 15)
            {
                step_detect = 0;
                bool f_pre_infrared = f_infrared;
                f_infrared = DetectColorORGray(frame);

                if (f_infrared && !f_pre_infrared)
                {
                    tracker.Dispose();
                    tracker = new TrackerInfrared();
                    f_first = true;
                }
                else if (!f_infrared && f_pre_infrared)
                {
                    tracker.Dispose();
                    tracker = new TrackerOpticalFlow();
                    f_first = true;
                }
            }

            var ok = tracker.Update(frame);

            if (ok)
            {
                if (f_first)
                {
                    corrected_center_point = tracker.CenterPoint;
                    corrected_vel = tracker.Velocity;
                    f_first = false;
                }
                corrected_center_point = corrected_center_point * 0.4 + tracker.CenterPoint * 0.6;
                corrected_vel = corrected_vel * 0.4 + tracker.Velocity * 0.6;
            }
            else
            {
                f_first = true;
            }

            return tracker.Update(frame);
        }

        public void Draw(Mat frame, Scalar color)
        {
            tracker.Draw(frame, color);
        }

        public void Draw(ref Mat frame)
        {
            if (!tracker.IsActive) return;

            if (tracker.IsError)
                tracker.Draw(frame, Scalar.Red);
            else
                tracker.Draw(frame, Scalar.Green);
        }

        public Vec2d CenterPoint{ get { return tracker.CenterPoint; } }

        public Vec2d Velocity { get { return tracker.Velocity; } }

        public Vec2d CorrectedVelocity { get { return corrected_vel; } }

        public Vec2d CorrectedCenterPoint { get { return corrected_center_point; } }

        public bool IsError { get { return tracker.IsError; } }

        public bool DetectColorORGray(Mat frame)
        {
            using (var resize = frame.Resize(new OpenCvSharp.Size(10, 10)))
            {
                var bgr = resize.Mean();
                if (Math.Abs(bgr[0] - bgr[1]) < 1.5 && Math.Abs(bgr[2] - bgr[1]) < 1.5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }

    abstract class Tracker:IDisposable
    {
        bool f_first = true;
      //  bool f_first2 = true;
        protected bool f_active = false;
       // protected Rect pre_rect;
       // protected int pre_area;
        protected Vec2d center_point;
       // protected Vec2d pre_center_point;
        protected bool f_error = true;
        public Vec2d CenterPoint { get { return center_point; } }

        public Vec2d Velocity { get; protected set; }

        public bool IsActive { get { return f_active; } }

        public bool IsError { get { return f_error; } }
        public virtual bool Update(Mat frame)
        {
            bool ok = false;
            Mat gray;
            lock (frame)
            {
                gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY);
            }
          //  Cv2.ImShow("wwww", gray);
           // using (Mat )
          //  {
               // Cv2.ImShow("graydddd", gray);
                if (f_first)
                {
                    CursorControl.Init();
                    ok = First(gray);
                    f_first = !ok;
              //      f_first2 = true;
                }
                else
                {
                    ok = Process(gray);
                    f_active = true;
                }
            //  }
            gray.Dispose();
            if (ok)
            {
                f_error = false;
            }
            else
            {
                Velocity = new Vec2d(0, 0);
                f_error = true;
                f_first = true;
            }
            return ok;
        }

        protected abstract bool First(Mat gray);

        protected abstract bool Process(Mat gray);

        public abstract void Draw(Mat frame, Scalar color);

        public abstract void Dispose();
    }



    class TrackerInfrared:Tracker
    {
        int wide = 50;
        Rect2d pre_rect;
        double pre_area;
     //   Vec2d center_point;
        Vec2d pre_center_point;
        protected override bool First(Mat gray)
        {
            if(FindBlob.Rect(gray,180,out pre_center_point, out var rect))
            {

                pre_rect = Utils.RectWide(Utils.Rect2Rect2d(rect), wide, wide);
                pre_area = rect.Width * rect.Height;
                //pre_center_point = Utils.RectCenter2Vec2d(pre_rect);
                return true;
            }
            return false;
        }

        protected override bool Process(Mat gray)
        {
            bool ok = false;
            pre_rect = Utils.RectGrap(pre_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));
            using (var roi = new Mat(gray, pre_rect.ToRect()))
            {
                ok = FindBlob.Rect(roi, 200, (int)pre_area,out center_point ,out var rect);

                if (!ok) return false;
                var rect2d = Utils.Rect2Rect2d(rect);
                rect2d.X += pre_rect.X;
                rect2d.Y += pre_rect.Y;
                pre_rect = Utils.RectWide(rect2d, wide, wide);
                pre_area = rect.Width * rect.Height;

                //  gray.Rectangle(pre_rect, new Scalar(255, 255, 0), 2);
                // Cv2.ImShow("gray", gray);

            }

            pre_rect = Utils.RectGrap(pre_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));

           // center_point = Utils.RectCenter2Vec2d(pre_rect);

            var vel = center_point - pre_center_point;

            Velocity = new Vec2d(vel.Item0, -vel.Item1);//center_point - pre_center_point;

            

            pre_center_point = center_point;

            return true;
        }

        public override void Draw(Mat frame, Scalar color)
        {
            frame.Rectangle(pre_rect.ToRect(), color, 4);
            // OpenCvSharp.Point cp = Utils.RectCenter(pre_rect);
            frame.Circle((int)center_point.Item0, (int)center_point.Item1, 4, color, 4);
        }

        public Vec2d GetCenterPoint { get { return center_point; } }

        public override void Dispose()
        {

        }

    }

    class TrackerOpticalFlow : Tracker
    {
        CascadeClassifier face_cas;
        CascadeClassifier eye_cas;
        CascadeClassifier mouth_cas;
        Mat pre_gray;
        Rect2d face_rect;
        Point2f[] pre_features;
        Point2d face_rect_point;
        double face_rect_width;
        double face_rect_height;
        public TrackerOpticalFlow()
        {
            string res_path = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";

            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
            eye_cas = new CascadeClassifier(res_path + "\\haarcascade_eye.xml");
            mouth_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_mouth.xml");
        }

        protected override bool First(Mat gray)
        {
          //  Cv2.ImShow("clip1", gray);
          //  Cv2.WaitKey(0);
            var rects = face_cas.DetectMultiScale(gray,1.1,10);

            if (rects.Length == 0)
                return false;

            face_rect = new Rect2d(0, 0, 0, 0);
            foreach (var rect in rects)
            {
              //  gray.Rectangle(rect, new Scalar(0, 0, 255), 2);
                if (face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                {
                    face_rect = new Rect2d(rect.X,rect.Y,rect.Width,rect.Height);
                }
            }


            using(var clip = new Mat(gray,face_rect.ToRect()))
            {
             //   var eyes =  eye_cas.DetectMultiScale(clip, 1.1,20);
                var mouth = mouth_cas.DetectMultiScale(clip, 1.1, 20);

                

                if (mouth.Length <=0)
                    return false;

                var area = mouth[0].Width * mouth[0].Height;
                Rect mouth_rect = mouth[0];
                for (int i = 1; i < mouth.Length; i++)
                    if (area < mouth[i].Width * mouth[i].Height)
                    {
                        mouth_rect = mouth[i];
                        area = mouth[i].Width * mouth[i].Height;
                    }

                var clip_height_half = clip.Height * 0.7;
                if (Utils.RectCenter2Point(mouth_rect).Y < clip_height_half)
                    return false;

             

               // foreach (var i in mouth)
               //     temp1.Rectangle(i, Scalar.Red, 2);
               // Cv2.ImShow("wwwwssss", temp1);
               // Cv2.WaitKey(0);
            }

           /* lock (gray)
            {
                Cv2.ImShow("wwweeee", gray.Clone());
            }*/


            var findfeatures_rect = Utils.RectScale2d(face_rect, .5, .5).ToRect();
            using (var clip = new Mat(gray,findfeatures_rect)) {
                pre_features = GetGoodFeatures(clip);

            /*    foreach(var p in pre_features)
                {
                    clip.Circle((int)p.X,(int)p.Y, 2, Scalar.White, 2);
                }*/
            }

            face_rect = Utils.RectScale2d(face_rect, 0.7, 0.8);
            center_point = Utils.RectCenter2Vec2d(face_rect);
            face_rect_point = new Point2d(face_rect.X, face_rect.Y);
            face_rect_width = face_rect.Width;
            face_rect_height = face_rect.Height;

            using (var clip = new Mat(gray,face_rect.ToRect()))
            {
                pre_gray = clip.Clone();
            }

            var t = findfeatures_rect.Location - face_rect.Location;
            Point2f dis =  new Point2f((float)t.X,(float)t.Y );
            for(int i = 0; i<pre_features.Length; i++)
            {
                pre_features[i] += dis;

            }

            return true;
        }

        protected override bool Process(Mat gray)
        {
            Point2f[] features = new Point2f[0];
            List<Point2f> pre_fp = new List<Point2f>();
            List<Point2f> next_ps = new List<Point2f>();
            Point2f vel = new Point2f(0, 0);

            face_rect = Utils.RectGrap(face_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));
            using (var clip = new Mat(gray, face_rect.ToRect())) {
                
                Cv2.CalcOpticalFlowPyrLK(pre_gray, clip, pre_features, ref features,out var status,out _);

                for(int i = 0; i < pre_features.Length; i++)
                {
                    if (status[i] == 1)
                    {
                        vel += features[i] - pre_features[i];
                        pre_fp.Add(pre_features[i]);
                        next_ps.Add(features[i]);
                    }
                }

            }
            
            if (pre_fp.Count == 0) return false;

            float div_count = 1 / (float)pre_fp.Count;
            vel.X *= div_count;
            vel.Y *= div_count;

            face_rect_point += new Point2d(vel.X,vel.Y);

            face_rect.X = face_rect_point.X;
            face_rect.Y = face_rect_point.Y;

            face_rect.Width = face_rect_width;
            face_rect.Height = face_rect_height;
            face_rect = Utils.RectGrap(face_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));


            center_point = Utils.RectCenter2Vec2d(face_rect);

            using(var clip = new Mat(gray, face_rect.ToRect()))
            {
                pre_gray.Dispose();
                pre_gray = clip.Clone();
            }

            for (int i = 0; i < next_ps.Count; i++)
                next_ps[i] -= vel;


            if (next_ps.Count <= 5)
            {
                return false;
               // pre_features = GetGoodFeatures(pre_gray);
            }
            else
            {
                pre_features = next_ps.ToArray();
            }


            Velocity =new Vec2d( -vel.X , vel.Y );


            return true;
        }

        public override void Draw(Mat frame, Scalar color)
        {
            frame.Rectangle(face_rect.ToRect(),color,4);
            var offset = face_rect_point.ToPoint();
            foreach (var p in pre_features)
                frame.Circle(p.ToPoint() + offset, 4, color, 4);

        }

        Point2f[] GetGoodFeatures(Mat mat)
        {
            return Cv2.GoodFeaturesToTrack(mat, 20, 0.001, 10, null, 20, true, 1);
        }


        public override void Dispose()
        {
            if (face_cas != null && !face_cas.IsDisposed)
                face_cas.Dispose();

            if (pre_gray != null && !pre_gray.IsDisposed)
                pre_gray.Dispose();
        }


    }

    /*
    class TrackerNormal:Tracker
    {
        CascadeClassifier face_cas;
        int wide = 50;
        int threshold;
        public TrackerNormal()
        {
            string res_path = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";

            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
           
        }

        protected override bool First(Mat gray)
        {
            var rects = face_cas.DetectMultiScale(gray);

            if (rects.Length == 0)
                return false;

            var face_rect = new Rect(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                //gray.Rectangle(rect, new Scalar(0, 0, 255), 2);
                if (face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                {
                    face_rect = rect;
                }
            }

            var roi_rect = Utils.RectScale(face_rect, 0.7, 1.2);
            var roi = new Mat(gray,roi_rect);

            var hist_list = ImageProcessing.GetHistList(roi);

            var ots_thre =  ImageProcessing.GetOtsuThreshold(hist_list, out var otsu_list);

            var otus2d = ImageProcessing.GetHist2d(otsu_list);

            otsu_list = Utils.NormalizArray(otsu_list,255);
            var ips = Utils.GetInflectionPoint(otsu_list);

            foreach (var p in ips)
            {
                otus2d.Line(new Point(p, 0), new Point(p, otus2d.Height - 1), Scalar.Red);
            }

         //   Cv2.ImShow("face", roi);
         //   Cv2.ImShow("Hist", ImageProcessing.GetHist2d(hist_list));
          //  Cv2.ImShow("otsu_Hist", otus2d);

            if (Utils.FindJustThreshold(otsu_list,ots_thre,out int thre))
            {

                using(var binary = roi.Threshold(thre, 255, ThresholdTypes.Binary)){

                    if (FindBlob.Rect(binary, 230, out var rect,2))
                    {
                      //  binary.Rectangle(rect, Scalar.White, 3);
                      //  Cv2.ImShow("binar", binary);
                        rect.X += roi_rect.X;
                        rect.Y += roi_rect.Y;
                        pre_rect = Utils.RectWide(rect, wide, wide);//rect;
                        pre_area = rect.Width * rect.Height;
                        threshold = thre;
                        return true;
                    }
                     
                   
                }
            }


//            FindBlob.Rect(roi, 180, out var blob_rect);
            return false;
        }

        protected override bool Process(Mat gray)
        {
          //  return false;
            bool ok = false;
            using (var roi = new Mat(gray, pre_rect))
            {
                using (var binary = roi.Threshold(threshold, 255, ThresholdTypes.Binary))
                {
                    OpenCvSharp.Point p = new Point((int)Math.Round(center_point.Item0), (int)Math.Round(center_point.Item1));
                    ok = FindBlob.Rect(binary, 150, pre_area, p, pre_rect.TopLeft, out var rect, 2);
                    if (ok)
                    {
                        rect.X += pre_rect.X;
                        rect.Y += pre_rect.Y;
                        pre_rect = Utils.RectWide(rect, wide, wide);
                        pre_area = rect.Width * rect.Height;
                    }
                    gray.Rectangle(pre_rect, new Scalar(255, 255, 0), 2);
                    Cv2.ImShow("gray", gray);
                    return ok;
                }
            }

        }

        void FindBlob_Color(Mat gray)
        {

        }


    }*/
}
    