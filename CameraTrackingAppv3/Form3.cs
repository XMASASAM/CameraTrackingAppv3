﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
namespace CameraTrackingAppv3
{
    public partial class Form3 : Form,IFormUpdate
    {
        Form1 form1;
        int pre_form_height;
        bool f_camera_visible = true;
        bool f_control_active = false;
        bool f_tracker_visible = true;
        bool f_range_of_motion_visible = true;

        public UserControl1 UserControl { get { return userControl11; } }

        public Form3(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

            CursorControl.Init();
            MouseControl.IsControl = true;
            

        }
        private void Form3_Load(object sender, EventArgs e)
        {
            //  form1.SetCameraOutPut(pictureBox1.CreateGraphics());
            //  form1.SetResizeParams(pictureBox1.Width, pictureBox1.Height);
            // Main.current_picture_control = userControl11;
            Main.ChangeDisplayCameraForm(this);
            
            pre_form_height = this.Size.Height;
        }
        private void button2_Click(object sender, EventArgs e)
        {
//            if (form1.IsCameraVisible)
            if (f_camera_visible)
            {
             //   this.Size = new Size(this.Size.Width,pre_form_height - pictureBox1.Height - 5);
                this.Size = new System.Drawing.Size(this.Size.Width,pre_form_height - userControl11.PictureHeight - 5);
                // form1.IsCameraVisible = false;
                f_camera_visible = false;
                //  pictureBox1.Visible = false;
                userControl11.VisibleCameraName(false);
                userControl11.VisiblePictureBox(false);
                button2.Text = "カメラ表示";
            }
            else
            {
                this.Size = new System.Drawing.Size(this.Size.Width,pre_form_height);
                //  form1.IsCameraVisible = true;
                f_camera_visible = true;
                //pictureBox1.Visible = true;
                userControl11.VisibleCameraName(true);
                userControl11.VisiblePictureBox(true);
                button2.Text = "カメラ非表示";
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //form1.BeginControl();
            f_control_active = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            var r = MessageBox.Show("このソフトウェアを終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if(r == DialogResult.OK)
            {
                form1.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void FormUpdate(ref Mat frame)
        {

            if (f_control_active)
            {
                Main.Tracker.Update(frame);
                CursorControl.Update(Main.Tracker.IsError,Main.Tracker.CenterPoint ,Main.Tracker.Velocity);
            }

            if (f_camera_visible)
            {
                if(f_control_active&&f_tracker_visible)
                    Main.Tracker.Draw(ref frame);

                if (f_range_of_motion_visible)
                    Sub_DisplayRangeOfMotion(ref frame);

                Main.DisplayCamera(frame);
            }
        }

        public void Sub_DisplayRangeOfMotion(ref Mat frame)
        {
            var ps = CursorControl.RangeOfMotion;
            for (int i = 0; i < ps.Length; i++)
            {
                frame.Circle((int)ps[i].Item0,(int)ps[i].Item1, 4, Scalar.Yellow, 4);
            }
            
            var ax = CursorControl.RangeOfMotionNormalAxis;

            var a_x = Utils.cvtVec2d2Point(ax[0] * 100);
            var a_y = Utils.cvtVec2d2Point(ax[1] * 100);
            var cp = Utils.cvtVec2d2Point(CursorControl.RangeOfMotionCenterPoint);

            frame.Line(cp - a_x , cp, Scalar.Yellow, 4);
            frame.Line(cp + a_x , cp, Scalar.Yellow, 4);

            frame.Line(cp - a_y, cp, Scalar.Yellow, 4);
            frame.Line(cp + a_y, cp, Scalar.Yellow, 4);

            frame.Circle(cp, 5, Scalar.Yellow, 4);

        }

        /*  public void DisplayFPS(int fps)
          {
              label1.Text = "FPS:" + fps.ToString();
          }*/

    }
}
