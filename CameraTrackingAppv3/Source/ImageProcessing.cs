using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class ImageProcessing
    {
        static Mat karnel3;
        static ImageProcessing()
        {
            karnel3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
        }

        static public int[] cvtMat2IntArray(Mat mat)
        {
            int[] ans = new int[mat.Rows];
            for (int i = 0; i < mat.Rows; i++)
                ans[i] = mat.At<int>(i, 0);

            return ans;
        }

        static public float[] cvtMat2FloatArray(Mat mat)
        {
            float[] ans = new float[mat.Rows];
            for (int i = 0; i < mat.Rows; i++)
                ans[i] = mat.At<float>(i, 0);

            return ans;
        }

        static public float[] GetHistList(Mat gray)
        {
            Mat hist = new Mat();
            Cv2.CalcHist(new Mat[] { gray }, new int[] { 0 }, null, hist, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
            Cv2.Normalize(hist, hist);
           // return cvtMat2IntArray(hist);
            return cvtMat2FloatArray(hist);
        }

        static public Mat GetHist2d(float[] hist_list)
        {
            int w = hist_list.Length;
            int h = 256;
            float max_v = 0;
            foreach (var i in hist_list)
                if (max_v < i)
                    max_v = i;
            Mat ans = new Mat(new Size(w, h), MatType.CV_8UC3);

            for (int i = 0; i < w; i++)
            {
                ans.Line(new Point(i, h - 1), new Point(i, 0), new Scalar(0, 0, 0));

                float f = hist_list[i] * (h - 1) / (float)max_v;
                ans.Line(new Point(i, h - 1), new Point(i, h - f - 1), new Scalar(255, 255, 255));
            }
            return ans;
        }


        static public int GetOtsuThreshold(float[] hist_list,out float[] otsu_hist , int pre_th = -1)
        {
           // Cv2.Normalize(b_hist, b_hist, 0, histImage.Height, NormTypes.MinMax);
            otsu_hist = new float[hist_list.Length];
            var max_v = 0.0;
            int ans = 0;
            int start = 1;
            int end = 255;
            float[] sum_m = new float[hist_list.Length + 1];
            float[] sum_q = new float[hist_list.Length + 1];
            if (pre_th >= 0)
            {
                start = Math.Max(pre_th - 5, 0);
                end = Math.Min(pre_th + 5, 255);
            }

            sum_m[0] = 0;
            sum_q[0] = 0;
            for (int i = 0; i < hist_list.Length; i++)
            {
                sum_m[i + 1] = hist_list[i] * i + sum_m[i];
                sum_q[i + 1] = hist_list[i] + sum_q[i];
            }



            for (int i = start; i <= end; i++)
            {
                var m1 = sum_m[i] - sum_m[0];
                var m2 = sum_m[256] - sum_m[i];

                var q1 = sum_q[i] - sum_q[0];
                var q2 = sum_q[256] - sum_q[i];

                if (q1 == 0)
                    m1 = 0;
                else
                    m1 /= q1;

                if (q2 == 0)
                    m2 = 0;
                else
                    m2 /= q2;

                var fn_v = q1 * q2 * (m1 - m2) * (m1 - m2);
                if (max_v < fn_v)
                {
                    max_v = fn_v;
                    ans = i;
                }
                otsu_hist[i] = fn_v;

            }

            return ans;
        }

        static public void Filter_FindBlob(Mat gray,out Mat binary , out Mat erode, int iteration = 1)
        {
            binary = gray.Threshold(0, 255, ThresholdTypes.Otsu);
            erode = binary.Erode(karnel3, null, iteration);
        }



    }
}
