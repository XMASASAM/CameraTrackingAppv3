using System;
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
        Form5 form5;
        public UserControl1 UserControl { get { return userControl11; } }

        public Form3(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

            CursorControl.Init();
            MouseControl.IsControl = true;
            ControlBox = false;
            
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
            f_control_active = !f_control_active;

            if (f_control_active)
            {
                MouseControl.IsControl = true;
                form5 = new Form5();
                form5.Show();
            }
            else
            {
                MouseControl.IsControl = false;
                if (form5 != null)
                    form5.Close();
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(this,form1);
            form6.Show();
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        /*     private void Form3_FormClosing(object sender, FormClosingEventArgs e)
             {

                 var r = MessageBox.Show("このソフトウェアを終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                 if (r == DialogResult.OK)
                 {
                     form1.Close();
                 }
                 else
                 {
                     e.Cancel = true;
                 }
             }*/

        public void FormUpdate(ref Mat frame)
        {
          /*  MouseControl.IsCursorOnForm = ClientRectangle.Contains(
                Form.MousePosition.X - Location.X,
                Form.MousePosition.Y - Location.Y);*/

        /*    if (temp_mof)
            {
                MouseControl.IsCursorOnForm = true;
            }*/

            if (f_control_active)
            {
                Main.Tracker.Update(frame);
                CursorControl.Update(Main.Tracker.IsError,Main.Tracker.CenterPoint ,Main.Tracker.Velocity);
                if (form5 != null)
                {
                    form5.Update();
                }
            }

            if (f_camera_visible)
            {
                if(f_control_active&&f_tracker_visible)
                    Main.Tracker.Draw(ref frame);

                if (f_range_of_motion_visible)
                    CursorControl.DisplayRangeOfMotion(ref frame);
                   // Sub_DisplayRangeOfMotion(ref frame);

                Main.DisplayCamera(frame);
            }
        }

    /*    public void Sub_DisplayRangeOfMotion(ref Mat frame)
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

        }*/

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.Close();
        }

        private void Form3_MouseLeave(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = false;
        }

        private void Form3_MouseEnter(object sender, EventArgs e)
        {
            MouseControl.IsCursorOnForm = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {

          //  var r = MessageBox.Show("このソフトウェアを終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

         //   if (r == DialogResult.OK)
         //   {
                form1.Close();
        //    }

        }



        /*  public void DisplayFPS(int fps)
          {
              label1.Text = "FPS:" + fps.ToString();
          }*/

    }
}
