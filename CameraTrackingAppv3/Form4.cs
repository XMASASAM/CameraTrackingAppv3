using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{
    public partial class Form4 : Form,IDrawFrameForm
    {
        Form1 form1;
        public Form4(Form1 form1)
        {
            InitializeComponent();
            //Main.current_picture_control = userControl11;
            Main.ChangeDisplayCameraForm(this, userControl11);
            this.form1 = form1;
        }


        private void Form4_Load(object sender, EventArgs e)
        {
            this.Text = "初期設定";
            // form1.BeginControl();
            Main.f_control_active = true;
        }

        public void DrawFrame(ref Mat frame)
        {
            Main.Tracker.Draw(ref frame);
        }



    }
}
