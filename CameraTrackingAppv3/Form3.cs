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
        int pre_form_height;
        bool f_camera_visible = true;
        bool f_control_active = false;
        bool f_tracker_visible = true;
        bool f_range_of_motion_visible = true;
        bool f_wait_mode = false;
        Form5 form5;
        SettingsConfig config;
        public UserControl1 UserControl { get { return userControl11; } }
        WaitProcess wait_process;
        public WaitProcess WaitProcess { get { return wait_process; } }
        Connect connect;
        Mat frame;
        public Form3()
        {
            InitializeComponent();
            pre_form_height = this.Size.Height;

            Main.Start(this);
            connect = new Connect();
            Main.SetConnect(connect);
        }

        //ここからスタート
        private void Form3_Load(object sender, EventArgs e)
        {

            Utils.MainForm = this;
            //var loaded_settings = SettingsConfig.Load(out Utils.Config);
            var loaded_settings = SettingsConfig.Load(out Utils.Config);

            if (loaded_settings)
            {
                SettingsConfig.Adapt(Utils.Config);
                var form = new Form1(ref Utils.Config, true, true);
                form.Show();
            }
            else
            {
                Utils.Config = new SettingsConfig();
                var form = new Form1(ref Utils.Config,false,false);
                form.Show();
            }


        }

        public void FormStart(ref SettingsConfig config)
        {
            this.config = config;

            Visible = true;
            Main.ChangeDisplayCameraForm(this);

            CursorControl.Init();
            CursorControl.SetRangeOfMotion(config.Property.RangeOfMotion);

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
                StartCursorControl();
            }
            else
            {
                StopCursorControl();
            }


        }


        void StartCursorControl()
        {
            f_control_active = true;
            MouseControl.IsControl = true;
            if (form5 == null)
            {
                form5 = new Form5();
                form5.Show();
            }
        }

        public void StopCursorControl()
        {
            f_control_active = false;
            MouseControl.IsControl = false;
            if (form5 != null)
                form5.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6();
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
            this.frame = frame;
            if (f_control_active)
            {
                Main.Tracker.Update(frame);
                CursorControl.Update(Main.Tracker.IsError,Main.Tracker.CorrectedCenterPoint ,Main.Tracker.CorrectedVelocity);
                if (form5 != null)
                {
                    form5.Update();
                }
            }


            if (f_wait_mode)
            {
                if (wait_process.Update(frame))
                {
                    ActiveCursor();
                }
                //Main.Tracker.Update(frame);


            }
        }

        public void FormDraw()
        {
            if (f_camera_visible)
            {
                if (f_control_active && f_tracker_visible)
                    Main.Tracker.Draw(ref frame);

                if (f_range_of_motion_visible)
                    CursorControl.DisplayRangeOfMotion(ref frame, config.Property.RangeOfMotion);
                // Sub_DisplayRangeOfMotion(ref frame);

                Main.DisplayCamera(frame);
            }
        }


        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            //form1.Close();
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
              //  form1.Close();
        //    }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            WaitCursor(false);
        }

        public void WaitCursor(bool change_machine)
        {
            f_wait_mode = true;
            StopCursorControl();
            wait_process = new WaitProcess(change_machine);
        }

        void ActiveCursor()
        {

            if (wait_process != null)
                wait_process.Dispose();
            f_wait_mode = false;
            StartCursorControl();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            //Main.Update();
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            Visible = false;

        }
    }
}
