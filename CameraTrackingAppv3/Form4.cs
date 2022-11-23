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
    public partial class Form4 : Form,IFormUpdate
    {
        Form1 form1;
        Vec2d[] range_of_motion;
        int step_range_of_motion;
        bool f_done = false;
        public UserControl1 UserControl { get { return userControl11; } }

        public Form4(Form1 form1)
        {
            InitializeComponent();

            Main.ChangeDisplayCameraForm(this);
            this.form1 = form1;
            range_of_motion = new Vec2d[4];
            step_range_of_motion = 0;

            CursorControl.IsRangeOfMotion = false;
            MouseControl.IsControl = false;

        }


        private void Form4_Load(object sender, EventArgs e)
        {
            this.Text = "初期設定";

            //CursorControl.Init();
            
            // form1.BeginControl();
            //    Main.f_control_active = true;
           // Utils.WriteLine("Cursor::" + CursorControl.IsControlMouse.ToString());
        }


        public void FormUpdate(ref Mat frame)
        {
            Main.Tracker.Update(frame);
          //  Utils.WriteLine("Cursor::" + CursorControl.IsControlMouse.ToString());
            CursorControl.Update(Main.Tracker.IsError,Main.Tracker.CenterPoint, Main.Tracker.Velocity);

            if (!f_done)
            {
                if (CursorControl.IsStayImpulse)
                {
                    range_of_motion[step_range_of_motion++] = Main.Tracker.CenterPoint;
                }

                if (step_range_of_motion >= 4)
                {
                    f_done = true;

                }
            }

            Main.Tracker.Draw(ref frame);

            for (int i = 0; i < step_range_of_motion; i++)
            {
                frame.Circle(
                    (int)range_of_motion[i].Item0,
                    (int)range_of_motion[i].Item1,
                    4, Scalar.Yellow, 4);
            }

            Main.DisplayCamera(frame);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            f_done = false;
            step_range_of_motion = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!f_done)
            {
                Utils.Alert_Note("初期設定が完了していません");
                return;
            }
            CursorControl.SetRangeOfMotion(range_of_motion);
            CursorControl.IsRangeOfMotion = true;
            var form = new Form3(form1);

            form.Show();

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form1.Visible = true;
            Main.ChangeDisplayCameraForm(form1);
            Close();
        }
    }
}
