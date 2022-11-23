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
        public GeneralTracker(bool infrared_mode)
        {
            f_infrared = infrared_mode;
            if (f_infrared)
                tracker = new TrackerInfrared();
            else
                tracker = new TrackerOpticalFlow();//TrackerNormal();

        }

        public bool Update(Mat frame)
        {
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

        public bool IsError { get { return tracker.IsError; } }

    }

    abstract class Tracker
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
            using (Mat gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY))
            {
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
            }

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
        
        
        



    }



    class TrackerInfrared:Tracker
    {
        int wide = 50;
        Rect pre_rect;
        int pre_area;
     //   Vec2d center_point;
        Vec2d pre_center_point;
        protected override bool First(Mat gray)
        {
            if(FindBlob.Rect(gray,180, out var rect))
            {
                pre_rect = Utils.RectWide(rect, wide, wide);
                pre_area = rect.Width * rect.Height;
                pre_center_point = Utils.RectCenter2Vec2d(pre_rect);
                return true;
            }
            return false;
        }

        protected override bool Process(Mat gray)
        {
            bool ok = false;
            using (var roi = new Mat(gray, pre_rect))
            {
                ok = FindBlob.Rect(roi, 200, pre_area, out var rect);

                if (!ok) return false;

                rect.X += pre_rect.X;
                rect.Y += pre_rect.Y;
                pre_rect = Utils.RectWide(rect, wide, wide);
                pre_area = rect.Width * rect.Height;

                //  gray.Rectangle(pre_rect, new Scalar(255, 255, 0), 2);
                // Cv2.ImShow("gray", gray);

            }

            pre_rect = Utils.RectGrap(pre_rect, new Rect(0, 0, Utils.CameraWidth, Utils.CameraHeight));

            center_point = Utils.RectCenter2Vec2d(pre_rect);

            var vel = center_point - pre_center_point;

            Velocity = new Vec2d(vel.Item0, -vel.Item1);//center_point - pre_center_point;

            

            pre_center_point = center_point;

            return true;
        }

        public override void Draw(Mat frame, Scalar color)
        {
            frame.Rectangle(pre_rect, color, 4);
            // OpenCvSharp.Point cp = Utils.RectCenter(pre_rect);
            frame.Circle((int)center_point.Item0, (int)center_point.Item1, 4, color, 4);
        }

        public Vec2d GetCenterPoint { get { return center_point; } }

    }

    class TrackerOpticalFlow : Tracker
    {
        CascadeClassifier face_cas;
        Mat pre_gray;
        Rect face_rect;
        Point2f[] pre_features;
        Point2f face_rect_point;
        int face_rect_width;
        int face_rect_height;
        public TrackerOpticalFlow()
        {
            string res_path = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";

            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
        }

        protected override bool First(Mat gray)
        {
            var rects = face_cas.DetectMultiScale(gray,1.1,10);

            if (rects.Length == 0)
                return false;

            face_rect = new Rect(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                gray.Rectangle(rect, new Scalar(0, 0, 255), 2);
                if (face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                {
                    face_rect = rect;
                }
            }

            face_rect = Utils.RectScale(face_rect, 0.7, 0.8);
            center_point = Utils.RectCenter2Vec2d(face_rect);
            face_rect_point = new Point2f(face_rect.X, face_rect.Y);
            face_rect_width = face_rect.Width;
            face_rect_height = face_rect.Height;


            using (var clip = new Mat(gray, face_rect)) {
                pre_features = GetGoodFeatures(clip);

                foreach(var p in pre_features)
                {
                    clip.Circle((int)p.X,(int)p.Y, 2, Scalar.White, 2);
                }

                pre_gray = clip.Clone();

            }

            return true;
        }

        protected override bool Process(Mat gray)
        {
            Point2f[] features = new Point2f[0];
            List<Point2f> pre_fp = new List<Point2f>();
            List<Point2f> next_ps = new List<Point2f>();
            Point2f vel = new Point2f(0, 0);
            
            using (var clip = new Mat(gray, face_rect)) {
                
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

            face_rect_point += vel;

            face_rect.X = (int)face_rect_point.X;
            face_rect.Y = (int)face_rect_point.Y;

            face_rect.Width = face_rect_width;
            face_rect.Height = face_rect_height;
            face_rect = Utils.RectGrap(face_rect, new Rect(0, 0, Utils.CameraWidth, Utils.CameraHeight));


            center_point = Utils.RectCenter2Vec2d(face_rect);

            using(var clip = new Mat(gray, face_rect))
            {
                pre_gray.Dispose();
                pre_gray = clip.Clone();
            }

            for (int i = 0; i < next_ps.Count; i++)
                next_ps[i] -= vel;


            if (next_ps.Count <= 10)
            {
                return false;
               // pre_features = GetGoodFeatures(pre_gray);
            }
            else
            {
                pre_features = next_ps.ToArray();
            }


            Velocity =new Vec2d( -(double)vel.X , (double)vel.Y );


            return true;
        }

        public override void Draw(Mat frame, Scalar color)
        {
            frame.Rectangle(face_rect,color,4);
            var offset = face_rect_point.ToPoint();
            foreach (var p in pre_features)
                frame.Circle(p.ToPoint() + offset, 4, color, 4);

        }

        Point2f[] GetGoodFeatures(Mat mat)
        {
            return Cv2.GoodFeaturesToTrack(mat, 80, 0.001, 10, null, 20, true, 1);
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
    