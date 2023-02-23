using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    static class FindBlob
    {
        static Mat target_mat = null;
        static double target_ave;
        static double target_round;
        static bool f_first = false;
        static Size pre_size = new Size(-1,-1);
        static Size first_target_size = new Size(-1,-1);
        static bool f_first_fitst_target_size = true;
        static public Mat GetTargetImage()
        {
          //  if (target_mat != null && target_mat.Size().Equals(new Size(0, 0))) return null;
            return target_mat;
        }

        static public double GetTargetMean()
        {
            return target_ave;
        }

        static public double GetTargetAround()
        {
            return target_round;
        }

        static public void SetTargetImage(Mat mat , double ave , double around)
        {
            target_mat = mat.Clone();
            target_ave = ave;
            target_round = around;
            f_first = true;
        }


        static FindBlob()
        {
            target_mat = new Mat();
        }

        static bool BlobFilter_1(Rect rect)
        {
            var wh = rect.Width / (double)rect.Height;

            return 0.6 <= wh && wh <= 1.6;
        }

        static bool BlobFilter_2(Mat binary, Rect rect, double threshold,out double ave)
        {
            ave = 0;
            if (rect.Width > 1 && rect.Height > 1)
            {
                rect.X += 1;
                rect.Y += 1;
                rect.Width -= 1;
                rect.Height -= 1;
            }

           // if (rect.Width <= 0 || rect.Height <= 0) return false;

            using (var blob = new Mat(binary, rect))
            {
                ave = Cv2.Mean(blob)[0];
                return ave >= threshold;
            }
        }

        static bool BlobFlilter(Rect rect, Mat binary, int threshold)
        {
            if (!BlobFilter_1(rect))
                return false;

            if (rect.Width * rect.Height <= 11)
                return false;

            return BlobFilter_2(binary, rect, threshold,out _);
        }

        static bool BlobFlilter(Rect rect, double area, double pre_area, Mat binary,int threshold)
        {
            if (!BlobFilter_1(rect))
                return false;

            if (area < pre_area * 0.5 || (pre_area *2) < area)
                return false;

            return BlobFilter_2(binary, rect, threshold,out _);
        }
        static long miss_time = 0;
        static public bool Rect(Mat gray, int threshold,Vec2d offset,Vec2d range_center, out Vec2d center_point, out Point[] blob_contor, int iteration = 1)
        {
         //   Utils.ShowMat("eddeed", gray);

            //  blob_rect = new Rect();
            blob_contor = new Point[0];
            bool ans = false;
            ImageProcessing.Filter_FindBlob(gray,threshold,out Mat binary, out Mat erode,Utils.Config.Property.InfraredFirstErodeIteration);
       //     Utils.ShowMat("eddeed_binary", binary);

            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            Point[] ans_ps = new Point[0];
            double distance = double.MaxValue;
            double score = double.MaxValue;
            Size temp_pre_size = new Size(-1,-1);
            if (first_target_size.Width > 0)
            {
                double k = Utils.Grap(0, miss_time*(1.0/1000), 1);
                temp_pre_size.Width = (int)(first_target_size.Width * k + pre_size.Width * (1-k));
                temp_pre_size.Height = (int)(first_target_size.Height * k + pre_size.Height * (1-k));
            }
            miss_time += Main.ElapsedMilliseconds;
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                if (rect.Width < 3 || rect.Height < 3) continue;

                if(temp_pre_size.Width>0)
                if (rect.Width < temp_pre_size.Width *0.5 || temp_pre_size.Width *2 < rect.Width ||
                    rect.Height < temp_pre_size.Height *0.5 || temp_pre_size.Height *2 < rect.Height)
                    continue;



                if (BlobFlilter(rect, binary, threshold))
                {
                    if (f_first)
                    {

                        KeyPoint[] try_keypoints = new KeyPoint[0];
                        Mat try_discriptors = new Mat();

                        double score_temp;
                        using (var clip = new Mat(gray, Utils.RectGrap(Utils.RectAddWide(Utils.RectScale(rect, 1.1, 1.1), 10+iteration*2, 10+iteration * 2),new Rect(new Point(0,0),gray.Size()))))
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

                        if (score_temp *15 >= target_round) continue;

                       // if (score_temp < score) score = score_temp;
                      //  else continue;

                    }
                    // else
                    //  {
                //    rect.X += (int)offset.Item0;
               //     rect.Y += (int)offset.Item1;

                    var cp = Utils.RectCenter2Vec2d(rect);
                        var temp = Utils.GetDistanceSquared(cp.Item0 + offset.Item0 - range_center.Item0, cp.Item1 + offset.Item1 - range_center.Item1);
                  //  Utils.WriteLine();
                    if (distance > temp) distance = temp;
                    else continue;
                    //  }


                    ans = true;
                    blob_contor = contour;
                   // blob_rect = rect;
                    ans_ps = contour;
                }
            }

            binary.Dispose();
            erode.Dispose();
            
            center_point = new Vec2d(0, 0);
            if (ans)
            {
                var blob_rect = Cv2.BoundingRect(blob_contor);
                if (!f_first)
                {
                    var correct_blob_rect = Utils.RectAddWide(blob_rect, iteration * 2, iteration * 2);
                    using (var clip = new Mat(gray, Utils.RectGrap(Utils.RectAddWide(Utils.RectScale(correct_blob_rect, 1.1, 1.1), 10, 10), new Rect(new Point(0, 0), gray.Size()))))
                    {
                        SetTargetImage(clip, correct_blob_rect, .1, 10);
                        //     target_mat = clip.Clone();
                        //      target_ave = target_mat.Mean()[0];
                        ///      var www = blob_rect.Width * 0.05 + 5;
                        //      var hhh = blob_rect.Height * 0.05 + 5;
                        //      target_round = www * clip.Height * 2 + hhh * clip.Width * 2 - www * hhh * 4;
                        //  Cv2.ImShow("target", clip);
                        //  Cv2.WaitKey(0);
                        //  akaze.DetectAndCompute(clip, null, out target_keypoints, target_discriptors);
                        //  Cv2.ImShow("target", clip);
                        //    Cv2.WaitKey(0);
                    }
                }
                f_first = true;
                center_point = GetCenterPoint(ans_ps) + offset;
                pre_size = blob_rect.Size;
                if (f_first_fitst_target_size)
                {
                    first_target_size = blob_rect.Size;
                    f_first_fitst_target_size = false;
                }
                miss_time = 0;
                //area_mean_threshold = 180;

            }

           // f_rect2_first = true;
            return ans;
        }

        static public bool Rect(Mat gray,Vec2d offset ,int threshold, double pre_area,out Vec2d center_point,out Rect blob_rect, out double blob_area,int iteration = 1)
        {

            blob_rect = new Rect();
            bool ans = false;
            blob_area = pre_area;
            double min_diff = double.MaxValue;
            // ImageProcessing.Filter_FindBlob(gray,threshold,out Mat binary, out Mat erode,iteration);
            ImageProcessing.Filter_FindBlob(gray, out Mat binary, out Mat erode,Utils.Config.Property.InfraredTrackErodeIteration);//iteration);
          //  Cv2.ImShow("binaryyyy", binary);
         //   Cv2.ImShow("erode", erode);
            Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
            Point[] ans_ps = new Point[0];
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);

                if (rect.Width < 2 || rect.Height < 2) continue;



                var area = Cv2.ContourArea(contour);//rect.Width * rect.Height;
                if (BlobFlilter(rect, area, pre_area, binary,threshold))
                {
                    if (rect.Width < pre_size.Width >> 1 || pre_size.Width << 1 < rect.Width ||
                        rect.Height < pre_size.Height >> 1 || pre_size.Height << 1 < rect.Height)
                        continue;

                    ans = true;
                    var diff = Math.Abs(area - pre_area);
                    if (min_diff > diff)
                    {
                        blob_rect = rect;
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
                center_point = GetCenterPoint(ans_ps) + offset;
                pre_size = blob_rect.Size;

                //  center_point.Item0 += blob_rect.TopLeft.X;
                //  center_point.Item1 += blob_rect.TopLeft.Y;
            }
            return ans;
        }

       // static int iteration_correct=1;
        static public bool Rect2(Mat gray ,long eli_time,int threshold,Vec2d offset,Vec2d pre_point,Vec2d pre_correct_vel ,double pre_area,double pre_cross_area,
            out Vec2d point_vel , out Vec2d center_point, out Point[] blob_contor, out double blob_area,out double blob_cross_area, int iteration = 1)
        {
                center_point = pre_point;//new Vec2d(0, 0);
                blob_contor = new Point[0];
                blob_area = pre_area;//0;
                point_vel = pre_correct_vel;//new Vec2d(0, 0);
                blob_cross_area = pre_cross_area;//0;
                bool ans = false;
                ImageProcessing.Filter_FindBlob(gray,threshold ,out Mat binary, out Mat erode,iteration);

                Cv2.FindContours(erode, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);


                if (contours.Length == 0) return false;

            var pre_vel = pre_correct_vel;// * Main.ElapsedMilliseconds;

            var correct = pre_point + pre_vel;

            double min_dis = double.MaxValue;
            Size ans_rect = pre_size;//new Size(-1,-1);
            //     blob_contor = contours[0];
            double next_threshold = 10;
            for (int i = 0; i < contours.Length; i++)
            {
                var rect = Cv2.BoundingRect(contours[i]);
                if (rect.Width < 1 || rect.Height < 1) continue;

                if (rect.Width * .4 > rect.Height || rect.Height > rect.Width * 2.5)
                {
                    Utils.WriteLine("break:over_plane_frame");
                    continue;
                }

                if (pre_size.Width > 0)
                    if (rect.Width < pre_size.Width * 0.33 || pre_size.Width * 3 < rect.Width ||
                        rect.Height < pre_size.Height * 0.33 || pre_size.Height * 3 < rect.Height)
                        continue;

                double temp_next_threshold;
                if (!BlobFilter_2(binary, rect, 169,out temp_next_threshold))
                {
                    Utils.WriteLine("break:under_threshold");
                //    Utils.ShowMat("vredddd",binary);
                    continue;
                }

                var area = Cv2.ContourArea(contours[i]);
                double dif_area = Math.Abs(pre_area - area);

           /*     if (pre_area * 2 + 10 < dif_area)
                {
                    Utils.WriteLine("break:over_area");

                    continue;
                }*/

                Vec2d center = GetCenterPoint(contours[i]) + offset;
                double diff_area = dif_area * dif_area;

                double diff_length = Utils.GetDistanceSquared(center, correct);
                double dis = (diff_area + 0.001) * (diff_length + 0.001);

                if (dis < min_dis)
                {
                    ans = true;

                    min_dis = dis;
                    blob_contor = contours[i];
                    center_point = center;
                    next_threshold = temp_next_threshold;
                    ans_rect = rect.Size;
                }
            }

            if (ans)
            {

                blob_area = Cv2.ContourArea(blob_contor);
                point_vel = (center_point - pre_point);//* Main.InvElapsedMilliseconds;
                blob_cross_area = Math.Abs(point_vel.Item0 * pre_correct_vel.Item1 - point_vel.Item1 * pre_correct_vel.Item0);
                //   Utils.WriteLine("around_are_threshold:" + area_mean_threshold.ToString());
                //area_mean_threshold = next_threshold - 10;
                pre_size = ans_rect;


            }
            return ans;
                
        }

        /*
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
        }*/

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

        static void SetTargetImage(Mat mat,Rect inaround_rect,double k,double b)
        {
            if (target_mat != null)
            {
                target_mat.Dispose();
            }
           // Utils.ShowMat("setrrrrrr", mat);
            target_mat = mat.Clone();
            target_ave = target_mat.Mean()[0];
            var www = (inaround_rect.Width) * k*.5 + b*.5;
            var hhh = (inaround_rect.Height) * k*.5 + b*.5;
            target_round = www * mat.Height * 2 + hhh * mat.Width * 2 - www * hhh * 4;
        }

        static public void Init()
        {
            f_first = false;
            pre_size = new Size(-1, -1);
            first_target_size = new Size(-1, -1);
            f_first_fitst_target_size = true;
        }


    }
}
