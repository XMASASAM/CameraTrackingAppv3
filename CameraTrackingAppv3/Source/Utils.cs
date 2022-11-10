﻿using System;
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


        public static void ShowLoadAlert(string title,System.Drawing.Point start_locate)
        {
            if (loadalert.ContainsKey(title))
                return;

            loadalert.Add(title, new Form2(title,start_locate));
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
            var cp = RectCenter(rect);

            rect.X = cp.X - (wideX >> 1);
            rect.Y = cp.Y - (wideY >> 1);
            rect.Width = wideX ;
            rect.Height = wideY ;
            return rect;
        }

        public static OpenCvSharp.Point RectCenter(OpenCvSharp.Rect rect)
        {
            var ans = new OpenCvSharp.Point();
            ans.X = rect.X + (rect.Width >> 1);
            ans.Y = rect.Y + (rect.Height >> 1);
            return ans;
        }

        public static OpenCvSharp.Point cvtPointForm2CV(System.Drawing.Point point)
        {
            return new OpenCvSharp.Point(point.X, point.Y);
        }

        public static System.Drawing.Point cvtCV2Form(OpenCvSharp.Point point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static double GetDistanceSquared(double x,double y)
        {
            return x*x + y*y;
        }
    }
}
