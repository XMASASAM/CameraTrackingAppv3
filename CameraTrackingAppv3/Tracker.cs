using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    class Tracker
    {
        bool f_infrared_mode;
        bool f_first = true;
        CascadeClassifier face_cas;
        Mat karnel3;
        Rect pre_rect;
        int pre_area;
        public Tracker()
        {
            string res_path = "C:\\Users\\e1861\\source\\repos\\CameraTrackingAppv3\\CameraTrackingAppv3\\Resources";
            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
            karnel3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3,3));
        }

        public void Update(bool infrared_mode,Mat frame)
        {
            this.f_infrared_mode = infrared_mode;
            using (var gray = frame.CvtColor(ColorConversionCodes.BGR2GRAY))
            {
                if (!f_first)
                {
                    using(var roi = new Mat(gray, pre_rect))
                    {
                        FindBlob(roi,150, out var ok, out var rect);

                        if (ok)
                        {
                            rect.X += pre_rect.X;
                            rect.Y += pre_rect.Y;
                            pre_rect = Utils.RectWide(rect, 50, 50);
                            pre_area = rect.Width * rect.Height;
                        }
                        gray.Rectangle(pre_rect, new Scalar(255, 255, 0), 2);
                    }
                }

                if (f_first)
                {
                    if (infrared_mode)
                    {
                        FindBlob(gray,180, out var ok, out var rect);
                        gray.Rectangle(rect, new Scalar(255, 255, 0), 2);
                        if (ok)
                        {
                            f_first = false;
                            pre_rect = Utils.RectWide(rect, 50, 50);
                            pre_area = rect.Width * rect.Height;
                        }
                    }
                    else
                    {
                        FindBlob_Color(gray);
                    }
                }
                Cv2.ImShow("findblob", gray);

            }
        }

        void FindBlob(Mat gray,int threshold,out bool success, out Rect blob_rect)
        {
            success = false;
            blob_rect = new Rect();
            int min_diff = int.MaxValue;
            using (var otsu = gray.Threshold(0,255,ThresholdTypes.Otsu))
            {
                int ite = 1;
              //  if (f_infrared_mode) ite = 0;
                using (var erode = otsu.Erode(karnel3,null,ite)) { 
                  //  Cv2.ImShow("otsu", erode);
                    Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                    foreach (var contour in contours)
                    {  
                        var rect = Cv2.BoundingRect(contour);
                        int w = rect.Width;
                        int h = rect.Height;
                        var wh = w / (double)h;
                        if (wh < 0.5 || 2 < wh)
                            continue;

                        var area = w * h;
                        if (!f_first)
                        {
                            
                            if(area <pre_area * 0.5 || (pre_area <<1) < area)
                            continue;
                        }
                        else if (w * h <= 11)
                            continue;

                        using (var blob = new Mat(otsu, rect))
                        {
                            var ave = Cv2.Mean(blob)[0];
                           // Utils.WriteLine(ave.ToString());
                            if (ave >= threshold)
                            {
                                // Utils.WriteLine("Success!!!!"+ave.ToString());
                                gray.Rectangle(rect, new Scalar(255, 255, 255), 2);
                                
                                success = true;

                                if (f_first)
                                {
                                    blob_rect = rect;
                                }
                                else
                                {
                                    var diff = Math.Abs( area - pre_area);
                                    if(min_diff > diff)
                                    {
                                        blob_rect = rect;
                                        min_diff = diff;
                                    }
                                }
                                
                            }
                        }
                    }
                }

            }
            //Cv2.ImShow("findblob", gray);
        }

        void FindBlob_Color(Mat gray)
        {
            var rects = face_cas.DetectMultiScale(gray);

            if (rects.Length == 0)
                return;

            var face_rect = new Rect(0,0,0,0);
            foreach(var rect in rects)
            {
                gray.Rectangle(rect, new Scalar(0, 0, 255),2);
                if(face_rect.Width * face_rect.Height < rect.Width * rect.Height)
                {
                    face_rect = rect;
                }
            }

            var roi = new Mat(gray, face_rect);

            FindBlob(roi,180, out var _, out var blob_rect);

         //   Cv2.ImShow("findblob", gray);

        }

    }
}
