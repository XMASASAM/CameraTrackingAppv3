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


        static System.Drawing.Point comform_picture_offset;
        static System.Drawing.Point comform_picture_size;

        static bool f_set_up = false;


        static bool f_infrared_mode = true;
        static bool f_first_camera = true;
        static bool f_set_range_of_motion = false;

        static UserControl1 current_picture_control;
       // static Form current_form;
        static GeneralTracker mouse_tracker;
        static CountFPS countFPS;

        delegate void FormUpdate(ref Mat frame);
        static FormUpdate form_update = null;
        static public GeneralTracker Tracker { get { return mouse_tracker; } }

        static Vec2d[] range_of_motion = new Vec2d[4];
        static int step_range_of_motion = 0;

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

                f_infrared_mode = DetectColorORGray(camera_frame);

                if (f_first_camera)
                    Sub_FirstCamera();


                if (form_update != null)
                    form_update(ref camera_frame);

            }
            else
            {
                f_set_up = false;
                current_picture_control.PictureClear();
                capture.Release();
                capture.Dispose();
                capture = null;
                Utils.Alert_Error("カメラからの画像読み取りができませんでした");
            }
        }

        static public void SetUpVideoCapture(VideoCapture cap,int pictureBoxW,int pictureBoxH)
        {
            capture = cap;
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
            if (capture != null)
            {
                lock (capture)
                {
                    capture.Release();
                    capture.Dispose();
                }
            }
            f_set_up = false;
        }

        static public bool DetectColorORGray(Mat frame)
        {
            using (var resize = frame.Resize(new OpenCvSharp.Size(10, 10)))
            {
                var bgr = resize.Mean();
                if (Math.Abs(bgr[0] - bgr[1]) < 1.5 && Math.Abs(bgr[2] - bgr[1]) < 1.5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void ChangeDisplayCameraForm<T>(T form)where T:IFormUpdate
        {
            //current_form = form;
            form_update = form.FormUpdate;
            current_picture_control = form.UserControl;//userControl1;
        }

        public static void SetRangeOfMotion()
        {
            f_set_range_of_motion = true;
            step_range_of_motion = 0;
        }

        public static void CloseSetRangeOfMotion()
        {
            f_set_range_of_motion = false;
            step_range_of_motion = 0;
        }

        static void Sub_FirstCamera()
        {
            mouse_tracker = new GeneralTracker(f_infrared_mode);
            f_first_camera = false;
        }

        static void Sub_ControlActive()
        {

        }

        public static void DisplayCamera(Mat frame)
        {
            using (Bitmap bitmap = BitmapConverter.ToBitmap(frame))
          //  using (var resize_bitmap = new Bitmap(bitmap, comform_picture_size.X, comform_picture_size.Y))
                current_picture_control.DrawImage(bitmap, comform_picture_offset, comform_picture_size);
        }

        
        static void Sub_SetRangeOfMotion()
        {


        }

    }
}
