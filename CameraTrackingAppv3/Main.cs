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

        public static bool f_camera_visible = true;
        public static bool f_control_active = false;
        static bool f_infrared_mode = true;
        static bool f_first_camera = true;


        static UserControl1 current_picture_control;
        static Form current_form;
        static GeneralTracker mouse_tracker;
        static CountFPS countFPS;

        delegate void DrawFrame(ref Mat frame);
        static DrawFrame draw_frame = null;
        static public GeneralTracker Tracker { get { return mouse_tracker; } }

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
                {
                    mouse_tracker = new GeneralTracker(f_infrared_mode);
                    f_first_camera = false;
                }

                if (f_control_active)
                {
                    mouse_tracker.Update(camera_frame);
                    CursorControl.Update(mouse_tracker.IsError, mouse_tracker.Velocity);

                }

                if (f_camera_visible)
                {
                    // if(f_control_active)mouse_tracker.Draw(ref camera_frame);
                    //DrawFrame(ref camera_frame);
                    if (draw_frame != null)
                        draw_frame(ref camera_frame);

                    using (Bitmap bitmap = BitmapConverter.ToBitmap(camera_frame))
                    using (var resize_bitmap = new Bitmap(bitmap, comform_picture_size.X, comform_picture_size.Y))
                        current_picture_control.DrawImage(bitmap, comform_picture_offset, comform_picture_size);
                  
                }
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

        public static void ChangeDisplayCameraForm<T>(T form,UserControl1 userControl1)where T:IDrawFrameForm
        {
            //current_form = form;
            draw_frame = form.DrawFrame;
            current_picture_control = userControl1;
        }


    }
}
