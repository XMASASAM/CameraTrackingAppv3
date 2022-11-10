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

            return 0.5 <= wh && wh <= 2;
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

        static public bool Rect(Mat gray, int threshold, out Rect blob_rect)
        {
            blob_rect = new Rect();
            bool ans = false;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                if (BlobFlilter(rect, binary, threshold))
                {
                    ans = true;
                    blob_rect = rect;
                }
            }

            binary.Dispose();
            erode.Dispose();
            return ans;
        }

        static public bool Rect(Mat gray, int threshold, int pre_area, out Rect blob_rect)
        {
            blob_rect = new Rect();
            bool ans = false;
            int min_diff = int.MaxValue;
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

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
                    }
                }
            }

            binary.Dispose();
            erode.Dispose();
            return ans;
        }
    }
}
