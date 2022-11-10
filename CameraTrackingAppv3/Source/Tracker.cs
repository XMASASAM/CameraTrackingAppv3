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
        OpenCvSharp.Point center_point;
        public OpenCvSharp.Point GetCenterPoint { get { return center_point; } }

        public virtual bool Update(Mat frame)
        {
            bool ok = false;
            using (Mat gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY))
            {
                if (f_first)
                {
                    ok = First(gray);
                    f_first = !ok;
                }
                else
                {
                    ok = Process(gray);
                }
            }

            if(ok)
                center_point = Utils.RectCenter(pre_rect);
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

        public OpenCvSharp.Point GetCenterPoint { get { return tracker.GetCenterPoint; } }

    }

    class TrackerInfrared:Tracker
    {


        protected override bool First(Mat gray)
        {
            if(FindBlob.Rect(gray,180, out var rect))
            {
                pre_rect = Utils.RectWide(rect, 50, 50);
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
                ok = FindBlob.Rect(roi, 150, pre_area, out var rect);
                if (ok)
                {
                    rect.X += pre_rect.X;
                    rect.Y += pre_rect.Y;
                    pre_rect = Utils.RectWide(rect, 50, 50);
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

            var roi = new Mat(gray,Utils.RectScale(face_rect,0.7,1.2));

            var hist_list = ImageProcessing.GetHistList(roi);

            ImageProcessing.GetOtsuThreshold(hist_list, out var otsu_list);

            Cv2.ImShow("face", roi);
            Cv2.ImShow("Hist", ImageProcessing.GetHist2d(hist_list));
            Cv2.ImShow("otsu_Hist", ImageProcessing.GetHist2d(otsu_list));
//            FindBlob.Rect(roi, 180, out var blob_rect);
            return false;
        }

        protected override bool Process(Mat gray)
        {
            return false;

        }

        void FindBlob_Color(Mat gray)
        {

        }


    }
}
