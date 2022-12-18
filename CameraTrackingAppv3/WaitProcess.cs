using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Diagnostics;
namespace CameraTrackingAppv3
{
    public class WaitProcess:IDisposable
    {
        Stopwatch stopwatch;
        bool f_first = true;
        bool f_confirm = false;
        int step_wait = 0;
        Form7 form7 = null;
        bool f_change_machine;
        bool f_active_signal=false;
        public WaitProcess(bool change_machine)
        {
            //   stopwatch = new Stopwatch();
            Main.SetFPS(1);
            CursorControl.SettingMode();
            stopwatch = new Stopwatch();
            f_change_machine = change_machine;

        //    if (change_machine)
        //        System.Windows.Forms.Cursor.Hide();

        }

        public void Dispose()
        {
            if (form7 != null)
            {
                form7.Close();
                form7.Dispose();
            }
        }
        public void ReceiveActiveSignal()
        {
            f_active_signal = true;
        }
        // public void Update(Vec2d tracker_corrected_vel)
        public bool Update(Mat frame)
        {
            if (f_change_machine)
            {
                return f_active_signal;
            }


            Main.Tracker.Update(frame);
            CursorControl.Update(Main.Tracker.IsError,Main.Tracker.CorrectedCenterPoint, Main.Tracker.CorrectedVelocity);


            if (!f_confirm)
            {
                if (CursorControl.IsStay)
                {
                    Utils.WriteLine("cursor is stay");
                    step_wait += 1;
                }
                else
                {
                    step_wait = 0;
                }

                if (step_wait >= 3)
                {
                    Utils.WriteLine("confirm start!!");

                    f_confirm = true;
                    MouseControl.IsControl = true;
                    MouseControl.CanClick = false;
                    CursorControl.IsRangeOfMotion = true;
                    Main.SetFPS(1000);

                    form7 = new Form7(Utils.cvtCV2Form(MouseControl.GetLocation));
                    form7.Show();
                    stopwatch.Start();

                }
            }

            if (f_confirm)
            {
                if (stopwatch.ElapsedMilliseconds < 10 * 1000)
                {
                    form7.SecondRemining((int)(10 - stopwatch.ElapsedMilliseconds / 1000));

                    if (MouseControl.IsCursorOnForm)
                    {
                        MouseControl.CanClick = true;

                        if (CursorControl.IsDwellImpulse)
                        {
                            MouseControl.Click(MouseState.LeftClick);
                        }

                    }
                    else
                    {
                        MouseControl.CanClick = false;
                    }

                    if (form7.IsResume)
                    {
                        return true;
                    }
                }
                else
                {
                    f_confirm = false;
                    stopwatch.Reset();
                    step_wait = 0;
                    form7.Close();
                    form7.Dispose();
                    Main.SetFPS(1);
                    CursorControl.SettingMode();
                }

            }
            

            return false;

        }


    }
}
