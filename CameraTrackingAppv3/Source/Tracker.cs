using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{

    abstract class Tracker
    {
        protected bool f_first = true;
        protected bool f_active = false;
        protected Rect pre_rect;
        protected int pre_area;
        protected Vec2d center_point;
        protected bool f_error = false;
        public Vec2d GetCenterPoint { get { return center_point; } }
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
                }
                else
                {
                    ok = Process(gray);
                }
            }

            if (ok)
            {
                center_point.Item0 = pre_rect.X + pre_rect.Width * 0.5;
                center_point.Item1 = pre_rect.Y + pre_rect.Height * 0.5;
                f_error = false;
            }
            else
            {
                f_error = true;
                f_first = true;
            }
            return ok;
        }

        protected abstract bool First(Mat gray);

        protected abstract bool Process(Mat gray);

        public virtual void Draw(Mat frame,Scalar color)
        {
            frame.Rectangle(pre_rect, color, 4);
            OpenCvSharp.Point cp = Utils.RectCenter(pre_rect);
            frame.Circle(cp, 4, color, 4);
        }
        



    }



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
                tracker = new TrackerNormal();
            
        }

        public bool Update(Mat frame)
        {
            return tracker.Update(frame);
        }

        public void Draw(Mat frame,Scalar color)
        {
            tracker.Draw(frame,color);
        }

        public void Draw(Mat frame)
        {
            if (tracker.IsError)
                tracker.Draw(frame, Scalar.Red);
            else
                tracker.Draw(frame, Scalar.Green);
        }

        public Vec2d
            GetCenterPoint { get { return tracker.GetCenterPoint; } }

    }

    class TrackerInfrared:Tracker
    {
        int wide = 50;

        protected override bool First(Mat gray)
        {
            if(FindBlob.Rect(gray,180, out var rect))
            {
                pre_rect = Utils.RectWide(rect, wide, wide);
                pre_area = rect.Width * rect.Height;
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


    }
}
