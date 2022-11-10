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
        static protected Mat karnel3;
        protected Rect pre_rect;
        protected int pre_area;
        OpenCvSharp.Point center_point;
        public OpenCvSharp.Point GetCenterPoint { get { return center_point; } }

        static Tracker()
        {
            karnel3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
        }

        public virtual bool Update(Mat frame)
        {
            bool ok = false;
            using (Mat gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY))
            {
                if (f_first)
                {
                    ok = First(gray);
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

        protected bool BlobFlilter(Point[] contour, Mat binary, int threshold, bool f_first, out Rect blob_rect, out int area)
        {
            var rect = Cv2.BoundingRect(contour);
            blob_rect = rect;
            int w = rect.Width;
            int h = rect.Height;
            var wh = w / (double)h;
            area = w * h;
            if (wh < 0.5 || 2 < wh)
                return false;

            if (!f_first)
            {

                if (area < pre_area * 0.5 || (pre_area << 1) < area)
                    return false;
            }
            else if (w * h <= 11)
                return false;

            using (var blob = new Mat(binary, rect))
            {
                var ave = Cv2.Mean(blob)[0];
                if (ave >= threshold)
                {
                    blob_rect = rect;
                    return true;

                }
            }
            return false;
        }


        protected void FindBlob(Mat gray, int threshold, out bool success, out Rect blob_rect)
        {
            success = false;
            blob_rect = new Rect();
            int min_diff = int.MaxValue;
            using (var otsu = gray.Threshold(0, 255, ThresholdTypes.Otsu))
            {
                int ite = 1;
                //  if (f_infrared_mode) ite = 0;
                using (var erode = otsu.Erode(karnel3, null, ite))
                {
                    //  Cv2.ImShow("otsu", erode);
                    Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                    foreach (var contour in contours)
                    {
                        if (BlobFlilter(contour, otsu, threshold, f_first, out Rect rect, out int area))
                        {
                            success = true;
                            if (f_first)
                            {
                                blob_rect = rect;
                            }
                            else
                            {
                                var diff = Math.Abs(area - pre_area);
                                if (min_diff > diff)
                                {
                                    blob_rect = rect;
                                    min_diff = diff;
                                }
                            }
                        }

                    }
                }

            }
            //Cv2.ImShow("findblob", gray);
        }

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
            FindBlob(gray, 180, out var ok, out var rect);
            //gray.Rectangle(rect, new Scalar(255, 255, 0), 2);
            if (ok)
            {
                f_first = false;
                pre_rect = Utils.RectWide(rect, 50, 50);
                pre_area = rect.Width * rect.Height;
            }
            return ok;
        }

        protected override bool Process(Mat gray)
        {
            using (var roi = new Mat(gray, pre_rect))
            {
                FindBlob(roi, 150, out var ok, out var rect);

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
            string res_path = "C:\\Users\\dhwp1\\Source\\Repos\\CameraTrackingAppv3\\CameraTrackingAppv3\\Resources";
               // "C:\\Users\\e1861\\source\\repos\\CameraTrackingAppv3\\CameraTrackingAppv3\\Resources";
            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
           
        }

        protected override bool First(Mat gray)
        {
            FindBlob_Color(gray);
            return false;
        }

        protected override bool Process(Mat gray)
        {
            return false;

        }

        void FindBlob_Color(Mat gray)
        {
            var rects = face_cas.DetectMultiScale(gray);

            if (rects.Length == 0)
                return;

            var face_rect = new Rect(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                gray.Rectangle(rect, new Scalar(0, 0, 255), 2);
                if (face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                {
                    face_rect = rect;
                }
            }

            var roi = new Mat(gray, face_rect);

            FindBlob(roi, 180, out var _, out var blob_rect);

        }


    }
}
