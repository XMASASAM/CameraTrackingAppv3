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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
namespace CameraTrackingAppv3
{
    static class Utils
    {

        public static string UserName;
        static Dictionary<string, Form2> loadalert = new Dictionary<string, Form2>();//   Form2 loadalert = null;
        public delegate void InvokeInt(int send);
        public delegate void InvokeBool(bool send);
        public delegate void InvokeString(string send);
        public delegate void InvokePoint(System.Drawing.Point send);
        public delegate void InvokeShowMat(string name, Mat mat);
        public delegate void InvokeVoid();
        public delegate void InvokeLoadAlert(string title,string message,Image image,System.Drawing.Point start_point,bool bottom);

        public static SettingsConfig Config;
        public static SettingsConfig Temp_Config;

        public static Form3 MainForm;

        public static readonly int PrimaryScreenWidthHalf;
        public static readonly int PrimaryScreenHeightHalf;
        public static int PrimaryScreenWidth { get { return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width; } }
        public static int PrimaryScreenHeight { get { return System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; } }

        public static readonly int AllScreenWidth;
        public static readonly int AllScreenHeight;
        public static readonly int AllScreenWidthHalf;
        public static readonly int AllScreenHeightHalf;

        public const int PortNum = 62355;
        public const string Password = "F6WqdvHwPY0wDiQ2SbTQDx8IlEvwyUhx";


        static Utils()
        {
            PrimaryScreenWidthHalf = PrimaryScreenWidth >> 1;
            PrimaryScreenHeightHalf = PrimaryScreenHeight >> 1;

            int bottom = 0;
            int right = 0;

            foreach(var screen_data in Screen.AllScreens)
            {
                bottom = Math.Max(screen_data.Bounds.Bottom, bottom);
                right = Math.Max(screen_data.Bounds.Right, right);
            }

            AllScreenWidth = right;
            AllScreenHeight = bottom;

            AllScreenWidthHalf = AllScreenWidth >> 1;
            AllScreenHeightHalf = AllScreenHeight >> 1;

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


        public static void ShowLoadAlert(string title,string message,Image image,System.Drawing.Point center_locate,bool bottom)
        {
            if (loadalert.ContainsKey(title))
                return;
            var form2 = new Form2(title, message, image, center_locate, bottom);
            loadalert.Add(title, form2);
            loadalert[title].Show();
            WriteLine("Load Start:" + title);
            return;
        }

        public static Form2 GetShowLoadAlert(string title, string message, Image image, System.Drawing.Point center_locate, bool bottom)
        {
            Form2 form2;
            if (loadalert.ContainsKey(title))
            {
                form2 = loadalert[title];

                return loadalert[title];
            }
            form2 = new Form2(title, message, image, center_locate, bottom);
            loadalert.Add(title, form2);
            loadalert[title].Show();
            WriteLine("Load Start:" + title);
            return form2;
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
        public static Rect2d RectScale2d(Rect2d rect, double scaleX, double scaleY)
        {
            var w = (rect.Width * scaleX);
            var h = (rect.Height * scaleY);
            return RectWide(rect, w, h);
        }

        public static Rect RectAddWide(Rect rect,int wideX,int wideY)
        {
            return RectWide(rect, rect.Width + wideX, rect.Height + wideY);
        }

        public static Rect2d RectAddWide(Rect2d rect, double wideX, double wideY)
        {
            return RectWide(rect, rect.Width + wideX, rect.Height + wideY);
        }

        public static Rect RectWide(Rect rect,int wideX,int wideY)
        {
            var cp = RectCenter2Point(rect);
            rect.X = cp.X - (wideX >> 1);
            rect.Y = cp.Y - (wideY >> 1);
            rect.Width = wideX;
            rect.Height = wideY;
            return rect;
        }

        public static Rect2d RectWide(Rect2d rect, double wideX, double wideY)
        {
            var cp = RectCenter2Point(rect);

            rect.X = cp.X - (wideX *.5);
            rect.Y = cp.Y - (wideY *.5);
            rect.Width = wideX;
            rect.Height = wideY;
            return rect;
        }

        public static OpenCvSharp.Point RectCenter2Point(OpenCvSharp.Rect rect)
        {
            var ans = new OpenCvSharp.Point();
            ans.X = rect.X + (rect.Width >> 1);
            ans.Y = rect.Y + (rect.Height >> 1);
            return ans;
        }

        public static OpenCvSharp.Point2d RectCenter2Point(OpenCvSharp.Rect2d rect)
        {
            var ans = new OpenCvSharp.Point2d();
            ans.X = rect.X + (rect.Width *.5);
            ans.Y = rect.Y + (rect.Height*.5);
            return ans;
        }

        public static OpenCvSharp.Vec2d RectCenter2Vec2d(OpenCvSharp.Rect rect)
        {
            return new Vec2d(rect.X + rect.Width * 0.5, rect.Y + rect.Height * 0.5);
        }

        public static OpenCvSharp.Vec2d RectCenter2Vec2d(OpenCvSharp.Rect2d rect)
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

        public static Vec2d NormalizVec2d(Vec2d vec,out double distance)
        {
            distance = Math.Sqrt(GetDistanceSquared(vec.Item0, vec.Item1));
            if (distance == 0)
            {
                return new Vec2d(0, 0);
            }
            return vec / distance;
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

        public static Rect2d RectGrap(Rect2d value, Rect2d range)
        {
            var right = Grap(range.X, value.Right, range.Right);
            var bottom = Grap(range.Y, value.Bottom, range.Bottom);

            value.X = Grap(range.X, value.X, range.Right);
            value.Y = Grap(range.Y, value.Y, range.Bottom);

            value.Width = right - value.X;
            value.Height = bottom - value.Y;
            return value;
        }

        public static Rect2d Rect2Rect2d(Rect rect)
        {
            return new Rect2d(rect.X,rect.Y, rect.Width,rect.Height);
        }

        public static Rect RectsMax(Rect[] rects)
        {
            int a = rects[0].Width * rects[0].Height;
            Rect ans = rects[0];
            for(int i = 1; i < rects.Length; i++)
            {
                int b = rects[i].Width * rects[i].Height;
                if( a < b)
                {
                    ans = rects[i];
                    a = b;
                }
            }
            return ans;
        }

        public static Rect RectGrapCameraFrame(Rect rect)
        {
            return Utils.RectGrap(rect,CameraFrame);
        }

        public static Rect2d RectGrapCameraFrame(Rect2d rect)
        {
            return RectGrap(rect,Rect2Rect2d(CameraFrame));
        }

        public static Rect CameraFrame { get { return new Rect(0, 0, CameraWidth, CameraHeight); } }

        public static int CameraWidth { get; set; }
        public static int CameraHeight { get; set; }

        public static readonly string PathResource = System.Reflection.Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\..\\Resources";

        static public byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            
            var bf = new BinaryFormatter();
            using (var  ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        static public object ByteArrayToObject(byte[] arrBytes)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                object obj = binForm.Deserialize(memStream);
                return obj;
                
            }

        }


        public static List<string> GetIPv4Address()
        {
            //IPアドレス用変数
            var ip = new List<string>();

            //自身のIPアドレスの一覧を取得する
            string hostname = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostname);

            //一覧からIPv4アドレスのみ抽出する
            foreach (IPAddress a in ips)
            {
                //IPv4を対象とする
                if (a.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    ip.Add(a.ToString());
                }
            }

            return ip;
        }

        static public List<string> GetActivePhysicalAddress()
        {
            var list = new List<string>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in interfaces)
            {
                if (OperationalStatus.Up == adapter.OperationalStatus)
                {
                    if ((NetworkInterfaceType.Unknown != adapter.NetworkInterfaceType) &&
                        (NetworkInterfaceType.Loopback != adapter.NetworkInterfaceType))
                    {
                        list.Add(adapter.GetPhysicalAddress().ToString());
                    }
                }
            }
            return list;
        }

        static public List<string> GetAllPhysicalAddress()
        {
            var list = new List<string>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var adapter in interfaces)
            {
                list.Add(adapter.GetPhysicalAddress().ToString());
            }
            return list;



        }

        static public void ShowMat(string name , Mat mat)
        {
            MainForm.Invoke(new InvokeShowMat(Cv2.ImShow), name, mat);
        }

    }
}
