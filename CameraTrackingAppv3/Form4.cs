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
        Form3 form3 = null;
        Form1 form1 = null;
        
        Vec2d[] range_of_motion;
        Vec2d[] axis = new Vec2d[2];
        int step_range_of_motion;
        bool f_done = false;
        bool f_event1 = false;
        bool f_event2 = false;
        bool f_pre_config_range_of_motion = true;
        bool f_pre_congig_control = true;
        string dialog_title = "メッセージ";
        string[] message = new string[] { "左上端を見て下さい", "右上端を見て下さい", "右下端を見て下さい", "左下端を見て下さい" };
        Image[] dialog_icon = new Image[] { Properties.Resources.pc_topleft, Properties.Resources.pc_topright, Properties.Resources.pc_bottomright, Properties.Resources.pc_bottomleft };
        public UserControl1 UserControl { get { return userControl11; } }


        public Form4(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
            Init();
            f_event1 = true;


        }

        public Form4(Form3 form3,Form1 form1)
        {
            InitializeComponent();
            this.form3 = form3;
            this.form1 = form1;
            f_pre_config_range_of_motion = CursorControl.IsRangeOfMotion;
            f_pre_congig_control = MouseControl.IsControl;
            Init();
            f_event2 = true;
            f_done = true;
            range_of_motion = CursorControl.RangeOfMotion;
            axis = CursorControl.RangeOfMotionNormalAxis;
            step_range_of_motion = 4;
            

        }



        void Init()
        {
            Main.ChangeDisplayCameraForm(this);

            range_of_motion = new Vec2d[4];
            
            step_range_of_motion = 0;

            CursorControl.SettingMode();
        }


        private void Form4_Load(object sender, EventArgs e)
        {
            this.Text = "可動域の設定";
            DisplayMessage(0);
            //CursorControl.Init();

            // form1.BeginControl();
            //    Main.f_control_active = true;
            // Utils.WriteLine("Cursor::" + CursorControl.IsControlMouse.ToString());
        }


        public void FormUpdate(ref Mat frame)
        {
            Main.Tracker.Update(frame);
            //  Utils.WriteLine("Cursor::" + CursorControl.IsControlMouse.ToString());
            CursorControl.Update(Main.Tracker.IsError, Main.Tracker.CenterPoint, Main.Tracker.Velocity);

            if (!f_done)
            {
                if (CursorControl.IsDwellImpulse)
                {
                    range_of_motion[step_range_of_motion++] = Main.Tracker.CenterPoint;

                    Utils.CloseLoadAlert(dialog_title);

                    if (step_range_of_motion < 4)
                    {
                        DisplayMessage(step_range_of_motion);
                    }


                }

                if (step_range_of_motion >= 4)
                {
                    f_done = true;
                    var range = range_of_motion;
                    var axis_x = (range[1] + range[2]) * 0.5 - (range[0] + range[3]) * 0.5;
                    //var axis_y = (range[0] + range[1]) * 0.5 - (range[3] + range[2]) * 0.5;
                    var axis_y = new Vec2d(axis_x.Item1, -axis_x.Item0);//(range[3] + range[2]) * 0.5 - (range[0] + range[1]) * 0.5;

                    var dis_x = Math.Sqrt(Utils.GetDistanceSquared(axis_x.Item0, axis_x.Item1));
                    var dis_y = Math.Sqrt(Utils.GetDistanceSquared(axis_y.Item0, axis_y.Item1));

                    if (dis_x == 0) dis_x += 0.0001;
                    if (dis_y == 0) dis_y += 0.0001;

                    axis[0] = axis_x / dis_x;
                    axis[1] = axis_y / dis_y;

                }
            }

            Main.Tracker.Draw(ref frame);

            if (f_done)
            {
                CursorControl.DisplayRangeOfMotion(ref frame,range_of_motion,axis);
            }
            else
            {
                for (int i = 0; i < step_range_of_motion; i++)
                {
                    frame.Circle(
                        (int)range_of_motion[i].Item0,
                        (int)range_of_motion[i].Item1,
                        4, Scalar.Yellow, 4);
                }
            }
        
            Main.DisplayCamera(frame);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            f_done = false;
            step_range_of_motion = 0;

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
            CursorControl.SetRangeOfMotion(range_of_motion);





            SettingsConfig settings = new SettingsConfig(range_of_motion,form1.GetActiveCameraID);
            if (settings.Save())
            {
                Utils.WriteLine("正常に設定をセーブできました");
            }
            else
            {
                Utils.WriteLine(Utils.PathResource + "がありませんでした。なので作成しました。セーブしました");
            }


            if (f_event1)
            {
                CursorControl.IsRangeOfMotion = true;
                var form = new Form3(form1);
                form.Show();
            }

            if (f_event2)
            {
              //  CursorControl.Init();
                CursorControl.IsRangeOfMotion = f_pre_config_range_of_motion;
                MouseControl.IsControl = f_pre_congig_control;
                Main.ChangeDisplayCameraForm(form3);
            }

            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (f_event1)
            {
                form1.Visible = true;
                Main.ChangeDisplayCameraForm(form1);
            }else if (f_event2)
            {
                Main.ChangeDisplayCameraForm(form3);
            }
            Close();
        }

        void DisplayMessage(int i)
        {
            var p = Location;
            p.X += Size.Width >> 1;
            Utils.ShowLoadAlert(dialog_title, message[i], dialog_icon[i], p, true);
        }
    }
}
