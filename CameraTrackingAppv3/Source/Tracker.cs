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
        System.Diagnostics.Stopwatch stopwatch;
        public GeneralTracker(Mat frame)
        {
            f_infrared = DetectInfraredColor(frame);
            if (f_infrared)
                tracker = new TrackerInfrared();
            else
                tracker = new TrackerOpticalFlow();
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }

        public bool Update(Mat frame)
        {

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                stopwatch.Restart();
                //step_detect = 0;
                bool f_pre_infrared = f_infrared;
                f_infrared = DetectInfraredColor(frame);

                if (f_infrared && !f_pre_infrared)
                {
                  //  Utils.ShowMat("test", frame);
                    Utils.WriteLine("赤外線モード!!!!");
                    tracker.Dispose();
                    tracker = new TrackerInfrared();
                    f_first = true;
                }
                else if (!f_infrared && f_pre_infrared)
                {
                    Utils.WriteLine("通常カメラモード!!!!");

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

        public bool IsSettingRect { get { return tracker.IsSettingRect; } }

        public bool DetectInfraredColor(Mat frame)
        {
            using (var resize = frame.Resize(new OpenCvSharp.Size(10, 10)))
            {
                //var bgr = resize.Mean();
                bool ok = true;
                Cv2.Split(resize, out var splits);

                Mat diff = new Mat();

                Cv2.Absdiff(splits[0], splits[1], diff);
                Cv2.MinMaxLoc(diff, out _, out double dis);//.Mean()[0] > 1.5;
                
                ok = dis < 2;

                if (ok)
                {
                    Cv2.Absdiff(splits[1], splits[2], diff);
                    Cv2.MinMaxLoc(diff, out _, out dis);//.Mean()[0] > 1.5;

                    ok = dis < 2;
                }

                if (ok)
                {
                    Cv2.Absdiff(splits[2], splits[0], diff);
                    Cv2.MinMaxLoc(diff, out _, out dis);//.Mean()[0] > 1.5;

                    ok = dis < 2;
                }

                diff.Dispose();
                splits[0].Dispose();
                splits[1].Dispose();
                splits[2].Dispose();

                return ok;
                //   var c_b = splits[0].Mean()[0];
                //   var c_g = splits[1].Mean()[0];
                //   var c_r = splits[2].Mean()[0];
                //   Utils.WriteLine("split:(b,g,r) " + "(" + c_b + "," + c_g + "," + c_r + ")");
                //  Utils.WriteLine("mean:(b,g,r) " + "(" + bgr[0] + "," + bgr[1] + "," + bgr[2] + ")");

             /*   if (ok)//Math.Abs(bgr[0] - bgr[1]) < 1.5 && Math.Abs(bgr[2] - bgr[1]) < 1.5)
                {
                    
                    return true;
                }
                else
                {
                    return false;
                }*/
            }
        }

        public void Reset()
        {
            f_first = true;
            step_detect = 1000;
            tracker.Reset();
        }

        public void SetTrackerRect(Rect rect)
        {
            tracker.SetTrackerRect(rect);
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
        protected bool f_first_clip_rect = false;

        protected Rect first_clip_rect;

        public Vec2d CenterPoint { get { return center_point; } }

        public Vec2d Velocity { get; protected set; }

        public bool IsActive { get { return f_active; } }

        public bool IsError { get { return f_error; } }

        public bool IsSettingRect { get { return f_first_clip_rect; } }
        int debug_count = 0;
        public virtual bool Update(Mat frame)
        {
            bool ok = false;
            Mat gray;
            lock (frame)
            {
                gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY);
            }

            if (f_first)
            {
                CursorControl.Init();
                ok = First(gray);
                f_first = !ok;
            }
            else
            {
                ok = Process(gray);
                f_active = true;
            }

            gray.Dispose();

            if (ok)
            {
                f_error = false;
            }
            else
            {
                debug_count++;
                Init();
                f_error = true;
           //     Utils.WriteLine("errorが起きたぞ!!!"+debug_count);
            }
            return ok;
        }

        protected abstract bool First(Mat gray);

        protected abstract bool Process(Mat gray);

        public abstract void Draw(Mat frame, Scalar color);

        public abstract void Dispose();

        public virtual void Reset()
        {
            f_first = true;
        }

        public void SetTrackerRect(Rect rect)
        {
            Init();
            f_first_clip_rect = true;
            first_clip_rect = rect;
            FindBlob.Init();

        }

        void Init()
        {
            Velocity = new Vec2d(0, 0);
            //f_error = true;
            f_first = true;
        }
    }



    class TrackerInfrared:Tracker
    {
        int wide = 50;
        Rect2d pre_rect;
        double pre_area;
        Vec2d pre_center_point;
        int binary_threshold=180;
        bool f_first_threshold = true;
        public TrackerInfrared()
        {

        }

        protected override bool First(Mat gray)
        {
            Mat mat = gray;
            Vec2d offset = new Vec2d(0, 0);
            if (f_first_clip_rect)
            {
                if (first_clip_rect.Size.Width < 3 || first_clip_rect.Size.Height < 3)
                    return false;

                mat = new Mat(gray, first_clip_rect);
                binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(mat), out _);
                offset = new Vec2d(first_clip_rect.X ,first_clip_rect.Y);
            }

            if (f_first_threshold)
            {
                binary_threshold = Utils.Config.Property.InfraredFirstThreshold;

                if (Utils.Config.IsHaveTargetImage)
                {
                    FindBlob.SetTargetImage(Utils.Config.TrackingTargetImage, Utils.Config.Property.TrackingTargetMean, Utils.Config.Property.TrackingTargetAround);
                    binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(Utils.Config.TrackingTargetImage), out _);
                  //  binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(mat), out _);
                }

                f_first_threshold = false;
            }
            

            bool f = FindBlob.Rect(mat, binary_threshold,offset, out pre_center_point, out var rect);

            if (f)
            {

                pre_rect = Utils.RectAddWide(Utils.Rect2Rect2d(rect), wide, wide);
                pre_area = rect.Width * rect.Height;
                using (var clip = new Mat(mat, Utils.RectGrap(pre_rect.ToRect(), new Rect(new Point(0, 0), mat.Size()))))
                binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(clip),out _);

                if (f_first_clip_rect)
                    pre_rect.Location += first_clip_rect.Location;
                
            }

            if (f_first_clip_rect)
            {
                if (f) f_first_clip_rect = false;
                mat.Dispose();
            }

            return f;
        }

        protected override bool Process(Mat gray)
        {

            bool ok = false;
            pre_rect = Utils.RectGrap(pre_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));
            Rect clip_rect = pre_rect.ToRect();
            using (var roi = new Mat(gray,clip_rect))
            {
                ok = FindBlob.Rect(roi,binary_threshold, pre_area,out center_point ,out var blob_rect , out pre_area,0);

                if (!ok) return false;
                blob_rect.X += clip_rect.X;
                blob_rect.Y += clip_rect.Y;
                center_point.Item0 += clip_rect.X;
                center_point.Item1 += clip_rect.Y;
                pre_rect = Utils.RectAddWide(Utils.Rect2Rect2d(blob_rect),wide,wide);//MakeRect(center_point,wide);

                binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(roi), out _);

            }
           // Utils.WriteLine("threshold: " + binary_threshold);
            pre_rect = Utils.RectGrap(pre_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));



            var vel = center_point - pre_center_point;

            Velocity = new Vec2d(-vel.Item0, vel.Item1);
         //   Utils.WriteLine("Velocity: "+Velocity.ToString());
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
        public override void Reset()
        {
            base.Reset();
        }

        // Rect2d MakeRect(Vec2d cp,double wide)
        //  {
        //      return Utils.RectAddWide();//new Rect2d(cp.Item0 - wide * 0.5 , cp.Item1 - wide * 0.5, wide, wide);
        //  }


    }

    class TrackerOpticalFlow : Tracker
    {
   //     CascadeClassifier face_cas;
  //      CascadeClassifier eye_cas;
   //     CascadeClassifier mouth_cas;
    //    CascadeClassifier pair_eye_cas;
        Mat pre_gray;
        Rect2d face_rect;
        Point2f[] pre_features;
        Point2d face_rect_point;
        double face_rect_width;
        double face_rect_height;
        public TrackerOpticalFlow()
        {
     //       string res_path = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";

      //      face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
      //      eye_cas = new CascadeClassifier(res_path + "\\haarcascade_eye.xml");
      //      mouth_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_mouth.xml");
      //      pair_eye_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_eyepair_big.xml");
        }

        public override void Reset()
        {
            base.Reset();
            face_rect = new Rect2d(0, 0, 0, 0);
            pre_features = new Point2f[0];
        }

        protected override bool First(Mat gray)
        {

            /*     var rects = face_cas.DetectMultiScale(gray,1.1,10);

                 if (rects.Length == 0)
                     return false;

                 face_rect = new Rect2d(0, 0, 0, 0);
                 foreach (var rect in rects)
                 {
                     if (face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                     {
                         face_rect = new Rect2d(rect.X,rect.Y,rect.Width,rect.Height);
                     }
                 }


                 using(var clip = new Mat(gray,face_rect.ToRect()))
                 {
                     var pair_eyes = pair_eye_cas.DetectMultiScale(clip, 1.1, 1);

                     if (pair_eyes.Length <=0)
                         return false;

                     Rect pair_eye_rect = pair_eyes[0];
                     var area = pair_eyes[0].Width * pair_eyes[0].Height;
                     for (int i = 1; i < pair_eyes.Length; i++)
                         if (area < pair_eyes[0].Width * pair_eyes[0].Height)
                         {
                             pair_eye_rect = pair_eyes[i];
                             area = pair_eyes[0].Width * pair_eyes[0].Height; 
                         }

                     var clip_height_half = clip.Height * 0.7;
                     var pair_eye_cp = Utils.RectCenter2Point(pair_eye_rect);


                     if (clip.Width * 0.45 > pair_eye_cp.X || pair_eye_cp.X > clip.Width * 0.55)
                         return false;


                 }*/



            //   var findfeatures_rect = Utils.RectScale2d(zoom_rect, zoom_rate_x, zoom_rate_y).ToRect();

            //    ImageProcessing.FindCentralGoodFeatures(gray,face_rect,.5,.5); 
            /*Utils.RectScale2d(face_rect, .5, .5).ToRect();
            using (var clip = new Mat(gray,findfeatures_rect)) {
                pre_features = GetGoodFeatures(clip);

            }*/

            Mat mat = gray;
            Vec2d offset = new Vec2d(0, 0);
            if (f_first_clip_rect)
            {
                mat = new Mat(gray, first_clip_rect);
                offset.Item0 = first_clip_rect.Location.X;
                offset.Item1 = first_clip_rect.Location.Y;
            }

            if (!ImageProcessing.DetectOneFrontFace(mat, out face_rect))
            {
                if (f_first_clip_rect)
                    mat.Dispose();
                return false;
            }
            var findfeatures_rect = Utils.RectScale2d(face_rect, .5, .5).ToRect();
            pre_features = ImageProcessing.FindGoodFeatures(mat, findfeatures_rect);

            using (var clip = new Mat(mat, face_rect.ToRect()))
                pre_gray = clip.Clone();

            face_rect.X += offset.Item0;
            face_rect.Y += offset.Item1;

            findfeatures_rect.X += (int)offset.Item0;
            findfeatures_rect.Y += (int)offset.Item1;

            center_point = Utils.RectCenter2Vec2d(face_rect);
            face_rect_point = new Point2d(face_rect.X, face_rect.Y);
            face_rect_width = face_rect.Width;
            face_rect_height = face_rect.Height;

            var t = findfeatures_rect.Location - face_rect.Location;
            Point2f dis =  new Point2f((float)(t.X),(float)(t.Y) );
            for(int i = 0; i<pre_features.Length; i++)
            {
                pre_features[i] += dis;

            }

            if (f_first_clip_rect)
            {
                mat.Dispose();
                f_first_clip_rect = false;
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

            if (pre_fp.Count == 0)
            {
              //  Utils.WriteLine("countが0!!!");
                return false;
            }

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
               // Utils.WriteLine("countが5以下!!!");

                return false;
               // pre_features = GetGoodFeatures(pre_gray);
            }
            else
            {
                pre_features = next_ps.ToArray();
            }


            Velocity =new Vec2d( -vel.X , vel.Y );

           // Utils.WriteLine("Velocity:" + Velocity.ToString());

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
          //  if (face_cas != null && !face_cas.IsDisposed)
          //      face_cas.Dispose();

            if (pre_gray != null && !pre_gray.IsDisposed)
                pre_gray.Dispose();
        }


    }

}
    