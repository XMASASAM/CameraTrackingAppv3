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
    public partial class Form3 : Form,IDrawFrameForm
    {
        Form1 form1;
        int pre_form_height;
        public Form3(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;

        }
        private void Form3_Load(object sender, EventArgs e)
        {
            //  form1.SetCameraOutPut(pictureBox1.CreateGraphics());
            //  form1.SetResizeParams(pictureBox1.Width, pictureBox1.Height);
            // Main.current_picture_control = userControl11;
            Main.ChangeDisplayCameraForm(this, userControl11);
            pre_form_height = this.Size.Height;
        }
        private void button2_Click(object sender, EventArgs e)
        {
//            if (form1.IsCameraVisible)
            if (Main.f_camera_visible)
            {
             //   this.Size = new Size(this.Size.Width,pre_form_height - pictureBox1.Height - 5);
                this.Size = new System.Drawing.Size(this.Size.Width,pre_form_height - userControl11.PictureHeight - 5);
                // form1.IsCameraVisible = false;
                Main.f_camera_visible = false;
                //  pictureBox1.Visible = false;
                userControl11.VisibleCameraName(false);
                userControl11.VisiblePictureBox(false);
                button2.Text = "カメラ表示";
            }
            else
            {
                this.Size = new System.Drawing.Size(this.Size.Width,pre_form_height);
                //  form1.IsCameraVisible = true;
                Main.f_camera_visible = true;
                //pictureBox1.Visible = true;
                userControl11.VisibleCameraName(true);
                userControl11.VisiblePictureBox(true);
                button2.Text = "カメラ非表示";
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //form1.BeginControl();
            Main.f_control_active = true;
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

        public void DrawFrame(ref Mat frame)
        {
            Main.Tracker.Draw(ref frame);
        }



        /*  public void DisplayFPS(int fps)
          {
              label1.Text = "FPS:" + fps.ToString();
          }*/

    }
}
