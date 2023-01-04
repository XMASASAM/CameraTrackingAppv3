using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Drawing;
using OpenCvSharp.Extensions;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Management;


namespace CameraTrackingAppv3
{


    static class Main
    {

        static Mat camera_frame;
        static VideoCapture capture;
        static SettingsConfig config;
        static Connect connect;
        static System.Drawing.Point comform_picture_offset;
        static System.Drawing.Point comform_picture_size;
        static double comform_picture_scale = 1;
        static int rotate = 0;
        static Mat rotate_mat;
        static bool f_set_up = false;


        static bool f_infrared_mode = true;
        static bool f_first_camera = true;

        static UserControl1 current_picture_control;
        static public UserControl1 GetUserControl1
        {
            get { return current_picture_control; }
        }
        // static Form current_form;
        static GeneralTracker mouse_tracker;
        static CountFPS countFPS;

        delegate void FormUpdate(ref Mat frame);
        delegate void FormDraw();
        static FormUpdate form_update = null;
        static FormDraw form_draw = null;
        static public GeneralTracker Tracker { get { return mouse_tracker; } }
        static public VideoCapture VideoCapture { get { return capture; } }
        static Vec2d[] range_of_motion = new Vec2d[4];

        static System.Diagnostics.Stopwatch stopwatch;
        static long interval_wait_time;
        static bool f_active = false;
        static public bool IsActive { get{return f_active; } }
        static Control control;
        static public Control Control { get { return control; } }
        static public double ComformPictureScale { get { return comform_picture_scale; } }
        static public System.Drawing.Point ComformOffset { get { return comform_picture_offset; } }
        static Main()
        {
            current_picture_control = null;
            countFPS = new CountFPS();
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            SetFPS(1000);
            
        }

        public static void Start(Control cont)
        {
            if (f_active) return;
            f_active  = true;
       //     control = Utils.MainForm.ActiveControl;//cont;
            control = cont;
            Thread thread = new Thread(new ThreadStart(Loop));
            thread.Start();
        }

        public static void End()
        {
            f_active = false;
        }

        static public void SetFPS(int fps)
        {
            interval_wait_time = 1000 / fps;
        }

        static public void SetRotate(int degree)
        {
            if (rotate == degree) return;
            rotate = degree;
            var cp = new Point2f(Utils.CameraWidth *.5f, Utils.CameraHeight * .5f);
            rotate_mat = Cv2.GetRotationMatrix2D(cp, degree, 1);
        }

        static public void SetConnect(Connect co)
        {
            connect = co;
        }

        static public Connect GetConnect()
        {
            return connect;
        }

        static void Loop()
        {
            while (f_active)
            {
               // control.Invoke(new Utils.InvokeVoid(Update));
                Update();
            }
            control.Invoke(new Utils.InvokeVoid(Utils.MainForm.Close));
            
        }

        static void Update()
        {
            if (stopwatch.ElapsedMilliseconds < interval_wait_time)
                return;
            stopwatch.Restart();

            if (!f_set_up)
            {
               // if (current_picture_control != null)
              //      control.Invoke(new Utils.InvokeVoid(current_picture_control.DisplayClear));
                //CurrentPictureControl.VisibleCameraName(false);
                return;
            }

            bool ok;

            if (camera_frame != null)
            {
                lock (camera_frame)
                {
                    camera_frame.Dispose();

                    camera_frame = new Mat();
                    ok = capture.Read(camera_frame);
                }
            }
            else
            {
                camera_frame = new Mat();
                ok = capture.Read(camera_frame);
            }


            if (ok)
            {

               // control.Invoke(new Utils.InvokeShowMat(Cv2.ImShow), "test", camera_frame);
                if (rotate != 0)
                {
                    using(var temp = camera_frame)
                    {
                        camera_frame  = camera_frame.WarpAffine(rotate_mat, camera_frame.Size());
                    }

                }
             //   control.Invoke(new Utils.InvokeShowMat(Cv2.ImShow), "test2", camera_frame);

                countFPS.Update();

                control.Invoke(new Utils.InvokeInt(current_picture_control.SetFPS),countFPS.Get);

             //   f_infrared_mode = DetectColorORGray(camera_frame);
             
                if (f_first_camera)
                    Sub_FirstCamera(camera_frame);


                if (form_update != null)
                    form_update(ref camera_frame);
           //     control.Invoke(new Utils.InvokeShowMat(Cv2.ImShow), "test3", camera_frame);

                if (form_draw != null)
                    control.Invoke(new Utils.InvokeVoid(form_draw));
           //     control.Invoke(new Utils.InvokeShowMat(Cv2.ImShow), "test4", camera_frame);


            }
            else
            {
                f_set_up = false;
                control.Invoke(new Utils.InvokeVoid(current_picture_control.PictureClear));
             //   capture.Release();
             //   capture.Dispose();
                //capture = null;
                Utils.Alert_Error("カメラからの画像読み取りができませんでした");
            }
                
            
        }

        static public void SetUpVideoCapture(VideoCapture cap,int pictureBoxW,int pictureBoxH)
        {
            /*
            if (capture != null && !capture.IsDisposed)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();
                    capture = cap;
                }
            }
            else
            {
                capture = cap;
            }*/
            if (cap == null)
                return;

            if (capture != null)
            {
                lock (capture)
                {
                    capture = cap;
                }
            }
            else
            {
                capture = cap;
            }
                int w = capture.FrameWidth;
                int h = capture.FrameHeight;
                Utils.ZoomFitSize(w, h, pictureBoxW, pictureBoxH, out int ox, out int oy, out int rw, out int rh, out double scale);
                comform_picture_offset.X = ox;
                comform_picture_offset.Y = oy;
                comform_picture_size.X = rw;
                comform_picture_size.Y = rh;
            comform_picture_scale = scale;
            f_set_up = true;
        }

        static public void ReSetVideoCapture()
        {
            f_set_up = false;
        }



        public static void ChangeDisplayCameraForm<T>(T form)where T:IFormUpdate
        {
            //current_form = form;
            form_update = form.FormUpdate;
            form_draw = form.FormDraw;
            current_picture_control = form.UserControl;//userControl1;
         //   config = set_config;
         //   if (capture != set_config.VideoCapture)
         //       SetUpVideoCapture(config.VideoCapture, current_picture_control.PictureWidth, current_picture_control.PictureHeight);
           /* lock (capture)
            {
                capture = config.VideoCapture;
            }*/
        }

        static void Sub_FirstCamera(Mat frame)
        {
            mouse_tracker = new GeneralTracker(frame);
            f_first_camera = false;
        }


        public static void DisplayCamera(Mat frame)
        {
            lock (frame)
            {
                using (Bitmap bitmap = BitmapConverter.ToBitmap(frame))
                    //  using (var resize_bitmap = new Bitmap(bitmap, comform_picture_size.X, comform_picture_size.Y))
                    current_picture_control.DrawImage(bitmap, comform_picture_offset, comform_picture_size);
            }
        }


    }
}
