using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class FindBlob
    {

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

        static bool BlobFlilter(Rect rect, int area, int pre_area, Mat binary, int threshold)
        {
            if (!BlobFilter_1(rect))
                return false;

            if (area < pre_area * 0.5 || (pre_area << 1) < area)
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
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                if (BlobFlilter(rect, binary, threshold))
                {
                    ans = true;
                    blob_rect = rect;
                    ans_ps = contour;
                }
            }

            binary.Dispose();
            erode.Dispose();

            center_point = new Vec2d(0, 0);
            if (ans)
                center_point = GetCenterPoint(ans_ps);


            return ans;
        }

        static public bool Rect(Mat gray, int threshold, int pre_area,out Vec2d center_point, out Rect blob_rect,int iteration = 1)
        {
            blob_rect = new Rect();
            bool ans = false;
            int min_diff = int.MaxValue;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode,iteration);
          //  Cv2.ImShow("binaryyyy", binary);
         //   Cv2.ImShow("erode", erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Point[] ans_ps = new Point[0];
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                var area = rect.Width * rect.Height;
                if (BlobFlilter(rect, area, pre_area, binary, threshold))
                {
                    ans = true;
                    var diff = Math.Abs(area - pre_area);
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
            center_point = new Vec2d(0, 0);
            if (ans)
                center_point = GetCenterPoint(ans_ps);
            
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
