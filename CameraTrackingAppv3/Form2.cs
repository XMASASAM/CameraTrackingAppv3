using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace CameraTrackingAppv3
{
    public partial class Form2 : Form
    {
        string title;
        string message;
        Point point;
        Image image;
        bool bottom = false;
        bool f_wait_hide = false;
        long time_wait_hide;
        System.Diagnostics.Stopwatch stopwatch;
        Thread thre = null;
        public Form2(string title,string message,Image image,Point point)
        {
            this.title = title;
            this.message = message;
            this.image = image;
            this.point = point;
            InitializeComponent();
            ControlBox = false;
            stopwatch = new System.Diagnostics.Stopwatch();
        }

        public Form2(string title, string message, Image image, Point point,bool bottom)
        {
            this.title = title;
            this.message = message;
            this.image = image;
            this.point = point;
            InitializeComponent();
            ControlBox = false;
            this.bottom = bottom;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = title;
            label2.Visible = false;
            label1.Text = message;
            pictureBox1.Image = image;

            if (bottom)
            {
                this.Location = new Point(
                    point.X - (Size.Width>>1),
                    point.Y - (Size.Height));
            }
            else
            {
                this.Location = new Point(
                    point.X - (Size.Width >> 1),
                    point.Y - (Size.Height >> 1));//center_point + new Point(Size.Width>>1 , Size.Height>>1);
            }

            
        }

        public void SetTitle(string title)
        {
            this.title = title;
            Text = this.title;
        }

        public void SetMessage(string message)
        {
            this.message = message;
            label1.Text = this.message;
        }
        public void SetRemainTime(int time)
        {
            label2.Text = "あと" + time + "秒で消えます";
        }

        public void SetImage(Image image)
        {
            this.image = image;
            pictureBox1.Image = this.image;
        }

        public void SetLocation(Point point)
        {
            Location = point;
        }

        public void HideAfter(long milliseconds)
        {
            label2.Visible = true;
            SetRemainTime((int)Math.Ceiling(milliseconds / 1000.0));//"あと" + (int)Math.Ceiling(milliseconds / 1000.0) + "秒で消えます";
            //Thread thre = new Thread(new ParameterizedThreadStart(Await_Hide));
            if(thre!=null && thre.IsAlive)
            {
                Utils.WriteLine("待機時間上書き");
                time_wait_hide = milliseconds;
                stopwatch.Restart();
            }
            else
            {
                Utils.WriteLine("待機時間のスレッドを生成");

                time_wait_hide = milliseconds;
                stopwatch.Restart();
                thre = new Thread(new ThreadStart(Await_Hide));
                thre.Start();
            }

            //thre = new Thread(new ParameterizedThreadStart(Await_Hide));
           // thre.Start(milliseconds);
            
        }

        void Await_Hide()
        {
            // int remain_time = (int)Math.Ceiling((time) / 1000.0);
            // stopwatch.Start();
            long time_remain = (int)Math.Ceiling(time_wait_hide / 1000.0);
            for (long i = 0; i < time_wait_hide; i = stopwatch.ElapsedMilliseconds)
            {
                long time_pre_remain = time_remain;
                time_remain = (int)Math.Ceiling((time_wait_hide -i) * (1/ 1000.0));
                if (time_pre_remain != time_remain && InvokeRequired)
                    Invoke(new Utils.InvokeInt(SetRemainTime), (int)time_remain);
                System.Threading.Thread.Sleep(1);
            }
            
            Invoke(new Utils.InvokeVoid(Hide));
               
        }



    }
}
