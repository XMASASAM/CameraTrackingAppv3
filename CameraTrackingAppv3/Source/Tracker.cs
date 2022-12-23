﻿using System;
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

        public void Reset()
        {
            f_first = true;
            step_detect = 1000;
            tracker.Reset();
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
             //   Utils.WriteLine("Error!!!!!!");
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

        public virtual void Reset()
        {
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
        public TrackerInfrared()
        {

        }

        protected override bool First(Mat gray)
        {


            if (FindBlob.Rect(gray, binary_threshold, out pre_center_point, out var rect))
            {

                pre_rect = MakeRect(pre_center_point, wide);//Utils.RectWide(Utils.Rect2Rect2d(rect), wide, wide);
                pre_area = rect.Width * rect.Height;
                using(var clip = new Mat(gray,pre_rect.ToRect()))
                binary_threshold = ImageProcessing.GetOtsuThreshold(ImageProcessing.GetHistList(clip),out _);
                return true;
            }

            return false;
        }

        protected override bool Process(Mat gray)
        {

            bool ok = false;
            pre_rect = Utils.RectGrap(pre_rect, new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));
            Rect clip_rect = pre_rect.ToRect();
            using (var roi = new Mat(gray,clip_rect))
            {
                ok = FindBlob.Rect(roi,binary_threshold, pre_area,out center_point ,out pre_area);

                if (!ok) return false;
                center_point.Item0 += clip_rect.X;
                center_point.Item1 += clip_rect.Y;
                pre_rect = MakeRect(center_point,wide);

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

        Rect2d MakeRect(Vec2d cp,double wide)
        {
            return new Rect2d(cp.Item0 - wide * 0.5 , cp.Item1 - wide * 0.5, wide, wide);
        }

    }

    class TrackerOpticalFlow : Tracker
    {
        CascadeClassifier face_cas;
        CascadeClassifier eye_cas;
        CascadeClassifier mouth_cas;
        CascadeClassifier pair_eye_cas;
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
            pair_eye_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_eyepair_big.xml");
        }

        public override void Reset()
        {
            base.Reset();
            face_rect = new Rect2d(0, 0, 0, 0);
            pre_features = new Point2f[0];
        }

        protected override bool First(Mat gray)
        {

            var rects = face_cas.DetectMultiScale(gray,1.1,10);

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
                var mouth = mouth_cas.DetectMultiScale(clip, 1.1, 30);
                var pair_eyes = pair_eye_cas.DetectMultiScale(clip, 1.1, 1);
                

                if (mouth.Length <=0 || pair_eyes.Length <=0)
                    return false;

                var area = mouth[0].Width * mouth[0].Height;
                Rect mouth_rect = mouth[0];
                for (int i = 1; i < mouth.Length; i++)
                    if (area < mouth[i].Width * mouth[i].Height)
                    {
                        mouth_rect = mouth[i];
                        area = mouth[i].Width * mouth[i].Height;
                    }

                Rect pair_eye_rect = pair_eyes[0];
                area = pair_eyes[0].Width * pair_eyes[0].Height;
                for (int i = 1; i < pair_eyes.Length; i++)
                    if (area < pair_eyes[0].Width * pair_eyes[0].Height)
                    {
                        pair_eye_rect = pair_eyes[i];
                        area = pair_eyes[0].Width * pair_eyes[0].Height; 
                    }

                var clip_height_half = clip.Height * 0.7;
                var mouth_cp = Utils.RectCenter2Point(mouth_rect);
                var pair_eye_cp = Utils.RectCenter2Point(pair_eye_rect);
                if (mouth_cp.Y < clip_height_half)
                    return false;

                if(clip.Width * 0.45 > mouth_cp.X || mouth_cp.X > clip.Width * 0.55)
                {
                    return false;
                }

                if (clip.Width * 0.45 > pair_eye_cp.X || pair_eye_cp.X > clip.Width * 0.55)
                    return false;


            }

            var findfeatures_rect = Utils.RectScale2d(face_rect, .5, .5).ToRect();
            using (var clip = new Mat(gray,findfeatures_rect)) {
                pre_features = GetGoodFeatures(clip);

            }


            face_rect = Utils.RectGrap(Utils.RectScale2d(face_rect, 0.7, 0.8), new Rect2d(0, 0, Utils.CameraWidth, Utils.CameraHeight));
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
            if (face_cas != null && !face_cas.IsDisposed)
                face_cas.Dispose();

            if (pre_gray != null && !pre_gray.IsDisposed)
                pre_gray.Dispose();
        }


    }

}
    