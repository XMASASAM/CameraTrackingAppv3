using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class ImageProcessing
    {
        static Mat karnel3;
        static CascadeClassifier face_cas;
        static CascadeClassifier eye_cas;
        static CascadeClassifier mouth_cas;
        static CascadeClassifier pair_eye_cas;
        
        static ImageProcessing()
        {
            karnel3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));

         //   string res_path = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";



        }
        static public void Load()
        {
            string res_path = Utils.SavePath;
            face_cas = new CascadeClassifier(res_path + "\\haarcascade_frontalface_default.xml");
            eye_cas = new CascadeClassifier(res_path + "\\haarcascade_eye.xml");
            mouth_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_mouth.xml");
            pair_eye_cas = new CascadeClassifier(res_path + "\\haarcascade_mcs_eyepair_big.xml");
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
            var f = cvtMat2FloatArray(hist);
            // return cvtMat2IntArray(hist);
          //  float max_v = 0;
         //   foreach (var i in f)
          //      if (max_v < i)
          //          max_v = i;

         //   for (int i = 0; i < f.Length; i++)
         //       f[i] = f[i] * 10/max_v;

            return f;
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

        static public void Filter_FindBlob(Mat gray,int threshold,out Mat binary , out Mat erode, int iteration = 1)
        {
            binary = gray.Threshold(threshold,255,ThresholdTypes.Binary);//gray.Threshold(0, 255, ThresholdTypes.Otsu);
            erode = binary.Erode(karnel3, null, iteration);
        }
        static public void Filter_FindBlob(Mat gray, out Mat binary, out Mat erode, int iteration = 1)
        {
            binary = gray.Threshold(0, 255, ThresholdTypes.Otsu);
            erode = binary.Erode(karnel3, null, iteration);
        }

        static public bool DetectOneFrontFace(Mat gray , out Rect2d face_rect)
        {
            var rects = face_cas.DetectMultiScale(gray, 1.1, 10);
            face_rect = new Rect2d(0, 0, 0, 0);

            if (rects.Length == 0)
                return false;

            face_rect =Utils.Rect2Rect2d(Utils.RectsMax(rects));

            using (var clip = new Mat(gray, face_rect.ToRect()))
            {
                var pair_eyes = pair_eye_cas.DetectMultiScale(clip, 1.1, 1);

                if (pair_eyes.Length <= 0)
                    return false;

                Rect pair_eye_rect = Utils.RectsMax(pair_eyes);

                var pair_eye_cp = Utils.RectCenter2Point(pair_eye_rect);

                if (clip.Width * 0.45 > pair_eye_cp.X || pair_eye_cp.X > clip.Width * 0.55)
                    return false;


            }
            return true;
        } 

        static public Point2f[] FindGoodFeatures(Mat gray , Rect zoom_rect)
        {
            
           // var findfeatures_rect = Utils.RectScale2d(zoom_rect, zoom_rate_x, zoom_rate_y).ToRect();
            using (var clip = new Mat(gray, zoom_rect))
            {
                return GetGoodFeatures(clip);

            }
        }

        static Point2f[] GetGoodFeatures(Mat mat)
        {
            return Cv2.GoodFeaturesToTrack(mat, 20, 0.001, 10, null, 20, true, 1);
        }
    }
}
