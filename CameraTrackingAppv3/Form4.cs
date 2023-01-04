using OpenCvSharp;
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
    public partial class Form4: Form,IFormUpdate
    {
        RangeOfMotionProps range;
        Vec2d[] points;

        int step_range_of_motion;
        bool f_done = false;
        bool f_first_event = false;
        bool f_dialog_move = false;
        bool f_pre_config_range_of_motion = true;
        bool f_pre_congig_control = true;
        string dialog_title = "メッセージ";
        string[] message = new string[] { "左上端を見て下さい", "右上端を見て下さい", "右下端を見て下さい", "左下端を見て下さい" };
        Image[] dialog_icon = new Image[] { Properties.Resources.pc_topleft, Properties.Resources.pc_topright, Properties.Resources.pc_bottomright, Properties.Resources.pc_bottomleft };
        public UserControl1 UserControl { get { return userControl11; } }
        Form2 form2 = null;
        SettingsConfig config;
        Mat frame;

        bool f_mouse_press = false;
        bool f_setting_rect = false;
    //    bool f_abort_animation = false;
        Vec2i start_point;
        Vec2i middle_point;
        Vec2i end_point;
        Rect setting_rect;
        double picture_scale;
        int dialog_move_index;

        public Form4(ref SettingsConfig config,bool first_event)
        {
            InitializeComponent();
            this.config = config;
            f_done = false;
            f_first_event = first_event;
            Main.ChangeDisplayCameraForm(this);
            picture_scale = 1/Main.ComformPictureScale;
            SettingsConfig.Adapt(config);
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

            form2 = new Form2(dialog_title, message[0], null,new System.Drawing.Point(Utils.AllScreenWidthHalf, Utils.AllScreenHeightHalf));

        }




        private void Form4_Load(object sender, EventArgs e)
        {
            this.Text = "可動域の設定";
            DisplayMessage(0);

            trackBar1.Value = config.Property.CameraAngle;

            userControl11.GetPictureBox().MouseDown += new MouseEventHandler(picturebox_MouseDown);
            userControl11.GetPictureBox().MouseMove += new MouseEventHandler(picturebox_MouseMove);
            userControl11.GetPictureBox().MouseUp += new MouseEventHandler(picturebox_MouseUp);

            Thread thre = new Thread(new ThreadStart(DialogMove));
            thre.Start();

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
            if (frame.IsDisposed)
                return;

            if (!f_done)
            {
                if (CursorControl.IsDwellImpulse)
                {
                    points[step_range_of_motion++] = Main.Tracker.CenterPoint;

                   // Utils.CloseLoadAlert(dialog_title);

                    if (step_range_of_motion < 4)
                    {
                        DisplayMessage(step_range_of_motion);
                    }


                }

                if (step_range_of_motion >= 4)
                {
                    form2.Hide();
                   // Utils.CloseLoadAlert(dialog_title);
                    f_done = true;
                    range = new RangeOfMotionProps(points);
                    CalibratedHorizontally(ref range);
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

            if (f_mouse_press)
            {

                var middle_rect = MakeRectOnPicture(start_point, middle_point);
                
                frame.Rectangle(middle_rect, Scalar.Aqua, 2);
            }

            if (f_setting_rect && Main.Tracker.IsSettingRect)
            {
                frame.Rectangle(setting_rect, Scalar.Aqua, 2);
            }


            Main.DisplayCamera(frame);
        }


        private void button3_Click(object sender, EventArgs e)
        {

           // Utils.CloseLoadAlert(dialog_title);
            f_done = false;
            step_range_of_motion = 0;
            points = new Vec2d[4];
        /*    if (form2 != null)
            {
                form2.Location = new System.Drawing.Point(Utils.AllScreenWidthHalf, Utils.AllScreenHeightHalf);
            }*/
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

            if (FindBlob.GetTargetImage()!=null && !FindBlob.GetTargetImage().Empty())
            {
                bool ok =true;
                if (config.IsHaveTargetImage)
                {
                    ok = false;
                    Mat mat1 = config.TrackingTargetImage;
                    Mat mat2 = FindBlob.GetTargetImage().Resize(mat1.Size());
                    using (Mat mat = Mat.Zeros(config.TrackingTargetImage.Size(), MatType.CV_8UC1))
                    {
                        Cv2.Absdiff(mat1, mat2, mat);
                        ok = mat.Mean()[0] != 0.0;
                    }
                    mat2.Dispose();
                }

                if (ok)
                {
                    config.TrackingTargetImage = FindBlob.GetTargetImage();
                    config.Property.TrackingTargetAround = FindBlob.GetTargetAround();
                    config.Property.TrackingTargetMean = FindBlob.GetTargetMean();
                }
            }

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
            // var p = Location;
            // p.X += Size.Width >> 1;
            /*        if (form2 != null && !form2.IsDisposed)
                    {
                        form2.SetMessage(message[i]);
                        form2.SetImage(dialog_icon[i]);
                        f_dialog_move = true;
                        dialog_move_index = i;
                    }
                    else
                    {
                        var p = new System.Drawing.Point(Utils.AllScreenWidthHalf, Utils.AllScreenHeightHalf);
                        form2 = Utils.GetShowLoadAlert(dialog_title, message[i], dialog_icon[i], p, false);
                        f_dialog_move = true;
                        dialog_move_index = i;
                    }*/
            //  f_abort_animation = true;
            //  Thread thread = new Thread(new ParameterizedThreadStart(DialogMove));
            //  thread.Start(new object[] { form2, i });
            
            form2.SetMessage(message[i]);
            form2.SetImage(dialog_icon[i]);
            f_dialog_move = true;
            dialog_move_index = i;
            form2.Show();

        }


        void DialogMove()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            while (!IsDisposed)
            {
                if (!f_dialog_move)
                {
                    Thread.Sleep(1);
                    continue;
                }
                f_dialog_move = false;

                Vec2d sp = new Vec2d(0, 0);//new Vec2d(form.Location.X , form.Location.Y);
                Vec2d ep = new Vec2d(0, 0);
                if (dialog_move_index == 0)
                {
                    sp.Item0 = Utils.AllScreenWidthHalf - form2.Width*.5;
                    sp.Item1 = Utils.AllScreenHeightHalf - form2.Height*.5;
                    ep.Item0 = 0;
                    ep.Item1 = 0;
                }
                else
                if (dialog_move_index == 1)
                {
                    sp.Item0 = 0;
                    sp.Item1 = 0;
                    ep.Item0 = Utils.AllScreenWidth - form2.Width;
                    ep.Item1 = 0;
                }
                else
                if (dialog_move_index == 2)
                {
                    sp.Item0 = Utils.AllScreenWidth - form2.Width;
                    sp.Item1 = 0;
                    ep.Item0 = Utils.AllScreenWidth - form2.Width;
                    ep.Item1 = Utils.AllScreenHeight - form2.Height;
                }
                else
                if (dialog_move_index == 3)
                {
                    sp.Item0 = Utils.AllScreenWidth - form2.Width;
                    sp.Item1 = Utils.AllScreenHeight - form2.Height;
                    ep.Item0 = 0;
                    ep.Item1 = Utils.AllScreenHeight - form2.Height;
                }

                if (InvokeRequired)
                    Utils.MainForm.Invoke(new Utils.InvokePoint(form2.SetLocation), Utils.cvtCV2Form(Utils.cvtVec2d2Point(sp)));

                var delta = (ep - sp) / 1000.0;
                stopwatch.Restart();
                for(long i=0;i<300;i=stopwatch.ElapsedMilliseconds)
                {
                    if (IsDisposed || f_dialog_move) break;
                    Thread.Sleep(1);
                }

                if (IsDisposed || f_dialog_move) continue;



                stopwatch.Restart();
                for (long i = 0; i < 1000; i = stopwatch.ElapsedMilliseconds)
                {
                    if (IsDisposed || form2.IsDisposed || f_dialog_move) break;

                if(InvokeRequired)
                        Utils.MainForm.Invoke(new Utils.InvokePoint(form2.SetLocation), Utils.cvtCV2Form(Utils.cvtVec2d2Point(sp + delta * (double)i)));

                    Thread.Sleep(1);
                }

                if (InvokeRequired)
                    Utils.MainForm.Invoke(new Utils.InvokePoint(form2.SetLocation), Utils.cvtCV2Form(Utils.cvtVec2d2Point(ep)));


            }
            Utils.MainForm.Invoke(new Utils.InvokeVoid(form2.Close));
        }

       

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var i in dialog_icon)
                i.Dispose();
            Dispose();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
           // form2.Close();
           // Utils.CloseLoadAlert(dialog_title);
        }

      /*  void CloseForm2()
        {
            form2.Visible = 
        }*/

        private void picturebox_MouseDown(object sender, MouseEventArgs e)
        {
            //  Utils.WriteLine("mouse_down");
            f_mouse_press = true;
            start_point.Item0 = e.Location.X;
            start_point.Item1 = e.Location.Y;

            middle_point = start_point;
        }

        private void picturebox_MouseMove(object sender, MouseEventArgs e)
        {
            // Utils.WriteLine("mouse_move");
            //  var p = e.Location;
            //   p.X -= userControl11.GetPictureBox().Location.X;
            //    p.Y -= userControl11.GetPictureBox().Location.Y;

            //   Utils.WriteLine(p.ToString());

            if (!f_mouse_press) return;

            middle_point.Item0 = e.Location.X;
            middle_point.Item1 = e.Location.Y;

        }

        private void picturebox_MouseUp(object sender, MouseEventArgs e)
        {
            // Utils.WriteLine("mouse_up");
            if (!f_mouse_press) return;
            end_point.Item0 = e.Location.X;
            end_point.Item1 = e.Location.Y;

            setting_rect = MakeRectOnPicture(start_point, end_point);
            f_setting_rect = true;

            f_mouse_press = false;

            Main.Tracker.SetTrackerRect(setting_rect);

        }

        Rect MakeRectOnPicture(Vec2i p1,Vec2i p2)
        {
            p1.Item0 -= Main.ComformOffset.X;
            p1.Item1 -= Main.ComformOffset.Y;
            p2.Item0 -= Main.ComformOffset.X;
            p2.Item1 -= Main.ComformOffset.Y;

            p1 *= picture_scale;
            p2 *= picture_scale;
            int x = Math.Min(p1.Item0, p2.Item0);
            int y = Math.Min(p1.Item1, p2.Item1);
            int w = Math.Abs(p1.Item0 - p2.Item0);
            int h = Math.Abs(p1.Item1 - p2.Item1);
            return new Rect(x , y , w, h);
        }

        void CalibratedHorizontally(ref RangeOfMotionProps props)
        {
            double angle_x = Math.Atan2(props.NormalAxi[0].Item1, props.NormalAxi[0].Item0);

            double dis;

            if (angle_x >= 0)
            {
                dis = Math.PI - angle_x;
            }
            else
            {
                dis = -Math.PI - angle_x;
            }


             double sin = Math.Sin(dis);
             double con = Math.Cos(dis);

            var degree = -dis * 180 / Math.PI;
            degree += trackBar1.Value; 

            if (degree < -180)
                degree += 360;
            if (degree > 180)
                degree -= 360;
            
            trackBar1.Value = (int)Utils.Grap(-180,degree,180);

            props.Rotation(sin, con, new Vec2d(Utils.CameraWidth * .5, Utils.CameraHeight * .5));


        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            Main.SetRotate(trackBar1.Value);
            config.Property.CameraAngle = trackBar1.Value;
        }
    }
}
