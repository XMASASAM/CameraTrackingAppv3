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
        Form6 form6;
        SettingsConfig config;
        public UserControl1 UserControl { get { return userControl11; } }
        WaitProcess wait_process;
        public WaitProcess WaitProcess { get { return wait_process; } }
        Connect connect;
        Mat frame;
        HashSet<Keys> down_key = new HashSet<Keys>();
        System.Diagnostics.Stopwatch error_stopwatch;
        System.Diagnostics.Stopwatch error_return_stopwatch;
        bool f_next_error = true;
        bool f_error_move_mouse = false;
        bool f_cursor_update = false;
        
        Form2 error_dialog;
        Vec2d error_move_mouse_vel;
        Vec2d error_move_mouse_start_point;

        Form2 waitmode_dialog;
        Form2 startoperate_dialog;
        public Form3()
        {
            InitializeComponent();
            pre_form_height = this.Size.Height;

            error_stopwatch = new System.Diagnostics.Stopwatch();

            error_return_stopwatch = new System.Diagnostics.Stopwatch();
            form5 = new Form5(true);
            var center = new System.Drawing.Point(Utils.AllScreenWidthHalf, Utils.AllScreenHeightHalf);
            error_dialog = new Form2("追跡対象を見失いました","顔を画面中央へ向けて下さい",Properties.Resources.not_found,center);
            waitmode_dialog = new Form2("通知", "待機モードに変更しました", Properties.Resources.pc,center);
            startoperate_dialog = new Form2("追跡対象が見つかりました", "顔を画面中央へ向けて下さい",Properties.Resources.correct,center);
            // form5.Show();
            Visible = false;
        }

        //ここからスタート
        private void Form3_Load(object sender, EventArgs e)
        {
            SettingsConfig.MakeInitialFolder();
            ImageProcessing.Load();
            //.Visible = true;
            Utils.MainForm = this;
            //Visible = false;

            Main.Start(this);
            MouseControl.Start();
            connect = new Connect();
            Main.SetConnect(connect);

            //var loaded_settings = SettingsConfig.Load(out Utils.Config);
            var loaded_settings = SettingsConfig.Load(out Utils.Config);

            if (loaded_settings)
            {
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

        private void Form3_Shown(object sender, EventArgs e)
        {
            Visible = false;
        }
        public void FormStart(ref SettingsConfig config)
        {
            this.config = config;

            Visible = true;
            Main.ChangeDisplayCameraForm(this);

            CursorControl.Init();
            SettingsConfig.Adapt(config);
           // CursorControl.SetRangeOfMotion(config.Property.RangeOfMotion);
            
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
            Main.SetFPS(1000);
            f_control_active = true;
            MouseControl.IsControl = true;
            FindTrackingTargetMode();
            //    if (form5 != null)
            //     {
            //  form5.Visible = true;
            //     form5.Show();
            //form5 = new Form5();
            //form5.Show();
            //    }
            form5.Visible = false;

            button4.Text = "操作停止";
        }

        public void StopCursorControl()
        {
            f_control_active = false;
            Main.Tracker.Reset();
            MouseControl.IsControl = false;
         //   if (form5 != null)
                form5.Visible = false;
            //form5.Close();
            button4.Text = "操作開始";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (form6!=null && !form6.IsDisposed)
            {
                form6.Focus();
            }
            else
            {
                form6 = new Form6();
                form6.Show();
            }
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
                if (f_cursor_update)
                {
                    CursorControl.Update(Main.Tracker.IsError, Main.Tracker.CorrectedCenterPoint, Main.Tracker.CorrectedVelocity);
                    form5.Update();

                }
                Invoke(new Utils.InvokeBool(TrackingErrorDialogProcess), Main.Tracker.IsError);

            /*    if (!Main.Tracker.IsError)
                {
                    form5.Update();
                }*/
            }


            if (f_wait_mode)
            {
                if (wait_process.Update(frame))
                {
                     Invoke(new Utils.InvokeVoid( ActiveCursor));
                }
                //Main.Tracker.Update(frame);


            }
        }

        public void TrackingErrorDialogProcess(bool error)
        {
            //f_next_error = true , f_update = false
            bool pre_error = f_next_error;
            bool pre_error_move_mouse = f_error_move_mouse;
            f_next_error = error;//Main.Tracker.IsError;


            if(f_next_error && !pre_error)
            {
                error_stopwatch.Restart();
            }

            if(error_stopwatch.ElapsedMilliseconds > 500)
            {
                f_error_move_mouse = true;
                error_stopwatch.Reset();
                error_return_stopwatch.Restart();
                f_cursor_update = false;
                MouseControl.CanClick = false;
                form5.Hide();
            }


            if (f_error_move_mouse && !pre_error_move_mouse)
            {
                FindTrackingTargetMode();
            }

            if (f_error_move_mouse && !f_next_error)// && pre_error)
            {
                f_cursor_update = false;
                f_error_move_mouse = false;
                error_dialog.Hide();
                startoperate_dialog.Show();
                startoperate_dialog.HideAfter(5000);
                error_return_stopwatch.Restart();
                MouseControl.CanClick = false;

            }

            if (error_return_stopwatch.ElapsedMilliseconds > 5000)
            {
                MouseControl.CanClick = true;

                f_cursor_update = true;
                error_return_stopwatch.Reset();
                form5.Show();
            }

        }

        void FindTrackingTargetMode()
        {
            f_error_move_mouse = true;
            f_cursor_update = false;
            MouseControl.SetLocationAnimation(new Vec2d(Utils.AllScreenWidthHalf, Utils.AllScreenHeightHalf), 2000);
            error_dialog.Show();
            error_return_stopwatch.Reset();
            startoperate_dialog.Hide();
            MouseControl.CanClick = false;

        }


        public void FormDraw()
        {
            if (frame.IsDisposed)
                return;

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
            Main.End();
          //  this.Close();
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
            waitmode_dialog.Show();
            waitmode_dialog.HideAfter(2000);

            if (f_camera_visible)
                button2_Click(null, null);

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

        private void Form3_KeyUp(object sender, KeyEventArgs e)
        {
            Utils.WriteLine("Key_Up: " + e.KeyCode.ToString());

            if (down_key.Contains(Keys.Escape)){
                if (down_key.Contains(Keys.Q))
                {
                    Main.End();
                }
                else
                if (down_key.Contains(Keys.A))
                {
                    StopCursorControl();
                }
                else
                if (e.KeyCode.Equals(Keys.S))
                {
                    StartCursorControl();
                }
            }

            down_key.Remove(e.KeyCode);

        }

        private void Form3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            Utils.WriteLine("Key_PreDown: " + e.KeyCode.ToString());
        }

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            Utils.WriteLine("Key_Down: " + e.KeyCode.ToString());
            down_key.Add(e.KeyCode);
        }

        private void Form3_KeyPress(object sender, KeyPressEventArgs e)
        {
            Utils.WriteLine("Key_Press: " + e.KeyChar.ToString());
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var users = Main.GetConnect().GetRecodeUsers();
            if (users.Count == 2)
            {
                var macs = Utils.GetAllPhysicalAddress();
                var index = 0;
                if (macs.Contains(users[index].MACAddress))
                    index = 1;


                if (Main.GetConnect().SendActiveSignal(index))
                {
                    Utils.MainForm.WaitCursor(true);
                }

            }
            else
            {
                Utils.Alert_Error("切り替え相手がいません");
            }
        }

        private void Form3_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            if (Main.IsActive)
            {
                var r = MessageBox.Show("このソフトウェアを終了します", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (r == DialogResult.OK)
                {
                    // form1.Close();
                    Main.End();
                }
                e.Cancel = true;

            }

        }

        private void Form3_DragEnter(object sender, DragEventArgs e)
        {
            Utils.WriteLine("DragEnter_FOrm3");
        }

        private void Form3_DragDrop(object sender, DragEventArgs e)
        {
            Utils.WriteLine("DragDrop_FOrm3");

        }
    }
}
