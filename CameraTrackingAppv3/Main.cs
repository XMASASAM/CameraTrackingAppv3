using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Drawing;
using OpenCvSharp.Extensions;
using System.Windows.Forms;
namespace CameraTrackingAppv3
{


    static class Main
    {

        static Mat camera_frame;
        static VideoCapture capture;
        static SettingsConfig config;

        static System.Drawing.Point comform_picture_offset;
        static System.Drawing.Point comform_picture_size;

        static bool f_set_up = false;


        static bool f_infrared_mode = true;
        static bool f_first_camera = true;

        static UserControl1 current_picture_control;
       // static Form current_form;
        static GeneralTracker mouse_tracker;
        static CountFPS countFPS;

        delegate void FormUpdate(ref Mat frame);
        static FormUpdate form_update = null;
        static public GeneralTracker Tracker { get { return mouse_tracker; } }
        static public VideoCapture VideoCapture { get { return capture; } }
        static Vec2d[] range_of_motion = new Vec2d[4];

        static Main()
        {
            current_picture_control = null;
            countFPS = new CountFPS();
        }

        public static void Update()
        {

            if (!f_set_up)
            {
                if (current_picture_control != null)
                    current_picture_control.DisplayClear();
                //CurrentPictureControl.VisibleCameraName(false);
                return;
            }


            if (camera_frame != null)
                camera_frame.Dispose();

            camera_frame = new Mat();



            if (capture.Read(camera_frame))
            {
                countFPS.Update();

                current_picture_control.SetFPS(countFPS.Get);

             //   f_infrared_mode = DetectColorORGray(camera_frame);

                if (f_first_camera)
                    Sub_FirstCamera(camera_frame);


                if (form_update != null)
                    form_update(ref camera_frame);

            }
            else
            {
               // f_set_up = false;
                current_picture_control.PictureClear();
             //   capture.Release();
             //   capture.Dispose();
                //capture = null;
             //   Utils.Alert_Error("カメラからの画像読み取りができませんでした");
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
                Utils.ZoomFitSize(w, h, pictureBoxW, pictureBoxH, out int ox, out int oy, out int rw, out int rh, out double _);
                comform_picture_offset.X = ox;
                comform_picture_offset.Y = oy;
                comform_picture_size.X = rw;
                comform_picture_size.Y = rh;
            
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
            using (Bitmap bitmap = BitmapConverter.ToBitmap(frame))
          //  using (var resize_bitmap = new Bitmap(bitmap, comform_picture_size.X, comform_picture_size.Y))
                current_picture_control.DrawImage(bitmap, comform_picture_offset, comform_picture_size);
        }


    }
}
