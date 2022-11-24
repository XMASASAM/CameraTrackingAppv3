using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Threading;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
namespace CameraTrackingAppv3
{
    static class Utils
    {

        

        static Dictionary<string, Form2> loadalert = new Dictionary<string, Form2>();//   Form2 loadalert = null;
        public delegate void InvokeInt(int send);
        public delegate void InvokeString(string send);
        public delegate void InvokeVoid();
        public delegate void InvokeLoadAlert(string title,System.Drawing.Point start_point);
        public static readonly int ScreenWidthHalf;
        public static readonly int ScreenHeightHalf;

        static Utils()
        {
            ScreenWidthHalf = ScreenWidth >> 1;
            ScreenHeightHalf = ScreenHeight >> 1;
        }

        public static void WriteLine(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        public static void ZoomFitSize(int in_width,int in_height, int out_width, int out_height,out int offsetX,out int offsetY,out int width,out int height,out double scale)
        {
            var scaleX = out_width / (double)in_width;
            var scaleY = out_height / (double)in_height;

            if(scaleX < scaleY)
            {
                scale = scaleX;
                width = out_width;
                height = (int)(in_height * scale);
            }
            else
            {
                scale = scaleY;
                width = (int)(in_width * scale);
                height = out_height;
            }

            offsetX = (out_width - width) >> 1;
            offsetY = (out_height - height) >> 1;
        }


        public static void ShowLoadAlert(string title,System.Drawing.Point center_locate)
        {
            if (loadalert.ContainsKey(title))
                return;

            loadalert.Add(title, new Form2(title,center_locate));
            loadalert[title].Show();
            WriteLine("Load Start:" + title);
        }

        public static void CloseLoadAlert(string title)
        {
            if (!loadalert.ContainsKey(title))
                return;
            var temp = loadalert[title];
            temp.Close();
            temp.Dispose();
            loadalert.Remove(title);
            WriteLine("Load Finish:" + title);

        }

        public static void Alert_Note(string msg)
        {
            MessageBox.Show(msg, "注意",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            WriteLine("Note:"+msg);
        }

        public static void Alert_Error(string msg)
        {
            MessageBox.Show(msg, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteLine("Error:" + msg);

        }

        public static Rect RectScale(Rect rect,double scaleX,double scaleY)
        {
            int w = (int)(rect.Width * scaleX);
            int h = (int)(rect.Height * scaleY);
            return RectWide(rect, w, h);
        }

        public static Rect RectWide(Rect rect,int wideX,int wideY)
        {
            var cp = RectCenter2Point(rect);

            rect.X = cp.X - (wideX >> 1);
            rect.Y = cp.Y - (wideY >> 1);
            rect.Width = wideX ;
            rect.Height = wideY ;
            return rect;
        }

        public static OpenCvSharp.Point RectCenter2Point(OpenCvSharp.Rect rect)
        {
            var ans = new OpenCvSharp.Point();
            ans.X = rect.X + (rect.Width >> 1);
            ans.Y = rect.Y + (rect.Height >> 1);
            return ans;
        }

        public static OpenCvSharp.Vec2d RectCenter2Vec2d(OpenCvSharp.Rect rect)
        {
            return new Vec2d(rect.X + rect.Width * 0.5, rect.Y + rect.Height * 0.5);
        }


        public static OpenCvSharp.Point cvtPointForm2CV(System.Drawing.Point point)
        {
            return new OpenCvSharp.Point(point.X, point.Y);
        }

        public static System.Drawing.Point cvtCV2Form(OpenCvSharp.Point point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static OpenCvSharp.Point cvtVec2d2Point(Vec2d point)
        {
            return new OpenCvSharp.Point((int)point.Item0, (int)point.Item1);
        }

        public static double GetDistanceSquared(double x,double y)
        {
            return x*x + y*y;
        }

        public static double GetDistanceSquared(Vec2d p1,Vec2d p2)
        {
            var p = p1 - p2;
            return GetDistanceSquared(p.Item0, p.Item1);
        }

        public static List<int> GetInflectionPoint(float[] p)
        {
            List<int> ans = new List<int>();
            var pre_p = p[2];
            var pre_d_p = p[2] - p[1];
            var pre_dd_p = (p[2] - p[1]) - (p[1] - p[0]);
            for (int i = 3; i < p.Length; i++)
            {
                var d_p = p[i] - pre_p;
                

                var dd_p = d_p - pre_d_p;
              //  Utils.WriteLine("p:" + p[i].ToString() + " dp:" + d_p.ToString() + " ddp:" + dd_p.ToString());
                //    Utils.WriteLine("ddp:" + d_p.ToString() + " pddp:" + pre_d_p.ToString());
                if (-1.2< d_p && d_p < -0.5 && dd_p >0  && pre_dd_p > 0)// dd_p > 100 && pre_d_p < -50 && pre_dd_p >50)//&& pre_dd_p < -100)
                {
                   // Utils.WriteLine("p:"+p[i].ToString() + " dp:" + d_p.ToString() + " ddp:" + dd_p.ToString());
                    ans.Add(i);
                }
                pre_p = p[i];
                pre_d_p = d_p;
                pre_dd_p = dd_p;
            }

            return ans;

        }

        public static bool FindJustThreshold(float[] p ,int otsu_threshold,out int out_threshold)
        {
            out_threshold = 0;
            var pre_p = p[2];
            var pre_d_p = p[2] - p[1];
            var pre_dd_p = (p[2] - p[1]) - (p[1] - p[0]);
            for (int i = 3; i < p.Length; i++)
            {
                var d_p = p[i] - pre_p;
                pre_p = p[i];

                var dd_p = d_p - pre_d_p;
                pre_d_p = d_p;

                if (otsu_threshold < i && -1.2 < d_p && d_p < -0.5 && dd_p > 0 && pre_dd_p > 0)
                {
                    out_threshold = i;
                    return true;
                }

                pre_dd_p = dd_p;
            }

            return false;
        }

        public static float[] NormalizArray(float[] a,float max_value = 1f)
        {
            float max_v = 0;
            foreach (var i in a)
                if (max_v < i)
                    max_v = i;

            for (int i = 0; i < a.Length; i++)
                a[i] = a[i] * max_value / max_v;
            return a;
        }

        public static Vec2d NormalizVec2d(Vec2d vec)
        {
            return vec / Math.Sqrt(GetDistanceSquared(vec.Item0, vec.Item1));
        }

        public static double Grap(double min,double value,double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
        public static Rect RectGrap(Rect value , Rect range)
        {
            int right = (int)Grap(range.X, value.Right, range.Right);
            int bottom = (int)Grap(range.Y, value.Bottom, range.Bottom);

            value.X = (int)Grap(range.X, value.X, range.Right);
            value.Y = (int)Grap(range.Y, value.Y, range.Bottom);

            value.Width = right - value.X ;
            value.Height = bottom - value.Y ;
            return value;
        }
        public static int ScreenWidth { get { return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width; } }
        public static int ScreenHeight { get { return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; } }
        

        public static int CameraWidth { get; set; }
        public static int CameraHeight { get; set; }

    }
}
