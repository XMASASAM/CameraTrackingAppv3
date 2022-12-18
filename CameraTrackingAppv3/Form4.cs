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
    public partial class Form4: Form,IFormUpdate
    {
        RangeOfMotionProps range;
        Vec2d[] points;

        int step_range_of_motion;
        bool f_done = false;
        bool f_first_event = false;

        bool f_pre_config_range_of_motion = true;
        bool f_pre_congig_control = true;
        string dialog_title = "メッセージ";
        string[] message = new string[] { "左上端を見て下さい", "右上端を見て下さい", "右下端を見て下さい", "左下端を見て下さい" };
        Image[] dialog_icon = new Image[] { Properties.Resources.pc_topleft, Properties.Resources.pc_topright, Properties.Resources.pc_bottomright, Properties.Resources.pc_bottomleft };
        public UserControl1 UserControl { get { return userControl11; } }

        SettingsConfig config;

        Mat frame;
        public Form4(ref SettingsConfig config,bool first_event)
        {
            InitializeComponent();
            this.config = config;
            f_done = false;
            f_first_event = first_event;
            Main.ChangeDisplayCameraForm(this);

            if (this.config.Property.RangeOfMotion.Points == null)
            {
                points = new Vec2d[4];
                step_range_of_motion = 0;
            }
            else
            {
                points = config.Property.RangeOfMotion.Points;
                range = config.Property.RangeOfMotion;
               // range_of_motion = config.Property.RangeOfMotion;
                step_range_of_motion = points.Length;
                f_pre_config_range_of_motion = CursorControl.IsRangeOfMotion;
                f_pre_congig_control = MouseControl.IsControl;
                f_done = true;
            }

            CursorControl.SettingMode();


        }




        private void Form4_Load(object sender, EventArgs e)
        {
            this.Text = "可動域の設定";
            DisplayMessage(0);
        }


        public void FormUpdate(ref Mat frame)
        {
            this.frame = frame;
            Main.Tracker.Update(frame);
            //  Utils.WriteLine("Cursor::" + CursorControl.IsControlMouse.ToString());
            CursorControl.Update(Main.Tracker.IsError, Main.Tracker.CorrectedCenterPoint, Main.Tracker.CorrectedVelocity);

        }

        public void FormDraw()
        {

            if (!f_done)
            {
                if (CursorControl.IsDwellImpulse)
                {
                    points[step_range_of_motion++] = Main.Tracker.CenterPoint;

                    Utils.CloseLoadAlert(dialog_title);

                    if (step_range_of_motion < 4)
                    {
                        DisplayMessage(step_range_of_motion);
                    }


                }

                if (step_range_of_motion >= 4)
                {
                    f_done = true;
                    range = new RangeOfMotionProps(points);

                }
            }

            Main.Tracker.Draw(ref frame);

            if (f_done)
            {
                CursorControl.DisplayRangeOfMotion(ref frame, range);
            }
            else
            {
                for (int i = 0; i < step_range_of_motion; i++)
                {
                    frame.Circle(
                        (int)points[i].Item0,
                        (int)points[i].Item1,
                        4, Scalar.Yellow, 4);
                }
            }

            Main.DisplayCamera(frame);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            Utils.CloseLoadAlert(dialog_title);
            f_done = false;
            step_range_of_motion = 0;
            points = new Vec2d[4];
            DisplayMessage(0);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!f_done)
            {
                Utils.Alert_Note("初期設定が完了していません");
                return;
            }
            CursorControl.Init();


            config.Property.RangeOfMotion = range;


            if (f_first_event)
            {
                if (config.Save())
                {
                    Utils.WriteLine("正常に設定をセーブできました");
                }
                else
                {
                    Utils.WriteLine(Utils.PathResource + "がありませんでした。なので作成しました。セーブしました");
                }

                CursorControl.IsRangeOfMotion = true;


                Utils.MainForm.FormStart(ref Utils.Config);
            }
            else 
            {
                CursorControl.IsRangeOfMotion = f_pre_config_range_of_motion;
                MouseControl.IsControl = f_pre_congig_control;
                Utils.MainForm.FormStart(ref Utils.Config);
            }

            Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (f_first_event)
            {
                var form = new Form1(ref config, false,false);

            }else
            {
                Utils.MainForm.FormStart(ref Utils.Config);

            }
            Close();
        }

        void DisplayMessage(int i)
        {
            var p = Location;
            p.X += Size.Width >> 1;
            Utils.ShowLoadAlert(dialog_title, message[i], dialog_icon[i], p, true);
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var i in dialog_icon)
                i.Dispose();
            Dispose();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Main.SetRotate(trackBar1.Value);
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.CloseLoadAlert(dialog_title);
        }
    }
}
