using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class FindBlob
    {
        static Mat target_mat;
        static double target_ave;
        static double target_round;
        static bool f_first = false;
        static Size pre_size;
        
        static FindBlob()
        {
            target_mat = new Mat();
        }

        static bool BlobFilter_1(Rect rect)
        {
            var wh = rect.Width / (double)rect.Height;

            return 0.6 <= wh && wh <= 1.6;
        }

        static bool BlobFilter_2(Mat binary, Rect rect, int threshold)
        {

            using (var blob = new Mat(binary, rect))
            {
                var ave = Cv2.Mean(blob)[0];
                return ave >= threshold;
            }
        }

        static bool BlobFlilter(Rect rect, Mat binary, int threshold)
        {
            if (!BlobFilter_1(rect))
                return false;

            if (rect.Width * rect.Height <= 11)
                return false;

            return BlobFilter_2(binary, rect, threshold);
        }

        static bool BlobFlilter(Rect rect, double area, double pre_area, Mat binary, int threshold)
        {
            if (!BlobFilter_1(rect))
                return false;

            if (area < pre_area * 0.5 || (pre_area *2) < area)
                return false;

            return BlobFilter_2(binary, rect, threshold);
        }

        static public bool Rect(Mat gray, int threshold, out Vec2d center_point, out Rect blob_rect, int iteration = 1)
        {
            blob_rect = new Rect();
            bool ans = false;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode,iteration);

            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Point[] ans_ps = new Point[0];
            double distance = double.MaxValue;
            double score = double.MaxValue;
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);

                if (BlobFlilter(rect, binary, threshold))
                {
                    if (f_first)
                    {
                        if (rect.Width < pre_size.Width >> 1 || pre_size.Width << 1 < rect.Width ||
                            rect.Height < pre_size.Height >> 1 || pre_size.Height << 1 < rect.Height)
                            continue;

                        KeyPoint[] try_keypoints = new KeyPoint[0];
                        Mat try_discriptors = new Mat();

                        double score_temp;
                        using (var clip = new Mat(gray, Utils.RectGrap(Utils.RectAddWide(Utils.RectScale(rect, 1.1, 1.1), 10, 10), Utils.CameraFrame)))
                        {
                            using (var resize = clip.Resize(target_mat.Size()))
                            {
                                Mat diff = new Mat();
                                //Utils.WriteLine("resiz: " +resize.ToString());

                                Mat resize_correct = resize * (float)(target_ave / clip.Mean()[0]);

                                Cv2.Absdiff(target_mat, resize_correct, diff);
                                score_temp = diff.Mean()[0];
                                diff.Dispose();
                                resize_correct.Dispose();
                            }
                        }

                        //   Utils.WriteLine("Score:" + score_temp + " Round:" +target_round);

                        if (score_temp * 9 >= target_round) continue;

                        if (score_temp < score) score = score_temp;
                        else continue;

                    }
                    else
                    {
                        var cp = Utils.RectCenter2Vec2d(rect);
                        var temp = Utils.GetDistanceSquared(cp.Item0 - Utils.CameraWidth * .5, cp.Item1 - Utils.CameraHeight * .5);

                        if (distance > temp) distance = temp;
                        else continue;
                    }
                    ans = true;
                    blob_rect = rect;
                    ans_ps = contour;
                }
            }

            binary.Dispose();
            erode.Dispose();

            center_point = new Vec2d(0, 0);
            if (ans)
            {

                if (!f_first)
                    using (var clip = new Mat(gray, Utils.RectGrap(Utils.RectAddWide(Utils.RectScale(blob_rect, 1.1, 1.1), 10, 10), Utils.CameraFrame)))
                    {
                        target_mat = clip.Clone();
                        target_ave = target_mat.Mean()[0];
                        var www = blob_rect.Width * 0.05 + 5;
                        var hhh = blob_rect.Height * 0.05 + 5;
                        target_round = www * clip.Height * 2 + hhh * clip.Width * 2 - www * hhh * 4;
                        //  Cv2.ImShow("target", clip);
                        //  Cv2.WaitKey(0);
                        //  akaze.DetectAndCompute(clip, null, out target_keypoints, target_discriptors);
                        //  Cv2.ImShow("target", clip);
                        //    Cv2.WaitKey(0);
                    }

                f_first = true;
                center_point = GetCenterPoint(ans_ps);
                pre_size = blob_rect.Size;

            }


            return ans;
        }

        static public bool Rect(Mat gray, int threshold, double pre_area,out Vec2d center_point, out double blob_area,int iteration = 1)
        {
           // blob_rect = new Rect();
            bool ans = false;
            blob_area = pre_area;
            double min_diff = double.MaxValue;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode,iteration);
          //  Cv2.ImShow("binaryyyy", binary);
         //   Cv2.ImShow("erode", erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
            Point[] ans_ps = new Point[0];
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                var area = Cv2.ContourArea(contour);//rect.Width * rect.Height;
                if (BlobFlilter(rect, area, pre_area, binary, threshold))
                {
                    ans = true;
                    var diff = Math.Abs(area - pre_area);
                    if (min_diff > diff)
                    {
                        // blob_rect = rect;
                        blob_area = area;
                        min_diff = diff;
                        ans_ps = contour;
                    }
                }
            }

            binary.Dispose();
            erode.Dispose();
            center_point = new Vec2d(0, 0);
            if (ans)
            {
                center_point = GetCenterPoint(ans_ps);
              //  center_point.Item0 += blob_rect.TopLeft.X;
              //  center_point.Item1 += blob_rect.TopLeft.Y;
            }
            
            return ans;
        }

        static public bool Rect(Mat gray, int threshold, int pre_area,Point pre_point,Point clip_topleft,out Vec2d center_point, out Rect blob_rect, int iteration = 1)
        {
            blob_rect = new Rect();
            bool ans = false;
            double min_diff = double.MaxValue;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode, iteration);
       //     Cv2.ImShow("binaryyyy", binary);
      //      Cv2.ImShow("erode", erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Point[] ans_ps = new Point[0];
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                var area = rect.Width * rect.Height;
                if (BlobFlilter(rect, area, pre_area, binary, threshold))
                {
                    ans = true;
                    var p = new Point(rect.X + clip_topleft.X + (rect.Width>>1), 
                                      rect.Y + clip_topleft.Y + (rect.Height>>1));
                    var dis_p = p - pre_point;
                    var diff = Utils.GetDistanceSquared(dis_p.X,dis_p.Y);//Math.Abs(area - pre_area);
                    if (min_diff > diff)
                    {
                        blob_rect = rect;
                        min_diff = diff;
                        ans_ps = contour;
                    }
                }
            }

            binary.Dispose();
            erode.Dispose();

            center_point = new Vec2d(blob_rect.Location.X, blob_rect.Location.Y);
            if (ans)
                center_point = GetCenterPoint(ans_ps);


            return ans;
        }

        static Vec2d GetCenterPoint(Point[] ps)
        {
            Vec2d ans = new Vec2d(0, 0);
            foreach (var i in ps)
            {
                ans.Item0 += i.X;
                ans.Item1 += i.Y;
            }
            var div_l = 1 / (double)ps.Length;
            ans.Item0 *= div_l;
            ans.Item1 *= div_l;

            return ans;
        }

    }
}
