using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
namespace CameraTrackingAppv3
{
    static class MouseControl
    {
        static int wait_time = 10;
        static int wait_time_double = 10;
        static Thread clicking;
        public static bool IsControl { get; set; } = false;
        public static bool CanClick { get; set; } = true;
        static public OpenCvSharp.Point GetLocation { get { return Utils.cvtPointForm2CV(System.Windows.Forms.Cursor.Position); } }

        static bool f_drag = false;
        static bool f_on_form = false;
        static Point2d start_point, end_point;
        static bool f_animation_wait = true;
        public static bool IsCursorOnForm { get { return f_on_form; } set { f_on_form = value; } }
        static System.Diagnostics.Stopwatch stopwatch;
        static long move_interval;

        static public int WaitTimeDouble { get { return wait_time_double; } set { wait_time_double = value; } }

        public static void Start()
        {
            start_point = new Point2d(0, 0);
            end_point = new Point2d(0, 0);
            Thread thre = new Thread(new ThreadStart(MoveAnimation));

            thre.Start();
            stopwatch = new Stopwatch();
            stopwatch.Start();
           // stopwatch.Start();
        }

        static public void Move(int dx, int dy)
        {
            if (!IsControl) return;
            var p = System.Windows.Forms.Cursor.Position;
            p.X += dx;
            p.Y += dy;
            SetLocation(p.X,p.Y);

            //System.Windows.Forms.Cursor.Position = p;
        }


        static public void SetLocationAnimation(Point2d ep)
        {
        //    Utils.WriteLine("stopWatch: " + stopwatch.ElapsedMilliseconds);
            move_interval = Math.Min(28,stopwatch.ElapsedMilliseconds);
         //   move_interval = 6;
            stopwatch.Restart();
            f_animation_wait = false;
           // start_point = sp;
            end_point = ep;
        }

        static public void SetLocationAnimation(Point2d ep,long time)
        {
            move_interval = time;//Math.Min(28, stopwatch.ElapsedMilliseconds);
            //   move_interval = 6;
            stopwatch.Restart();
            f_animation_wait = false;
            // start_point = sp;
            end_point = ep;
        }



        static void MoveAnimation()
        {
            System.Diagnostics.Stopwatch animation_stopwatch = new Stopwatch();
            while (Main.IsActive)
            {

                if (f_animation_wait)
                    continue;
                animation_stopwatch.Restart();
                f_animation_wait = true;
                var temp_end_point = end_point;
                var temp_move_interval = move_interval + 1;

                var diff = temp_end_point - start_point;

                var temp_mag = 1.0 / temp_move_interval;
                for(long t = 0;t<temp_move_interval;t = animation_stopwatch.ElapsedMilliseconds)
                {
                   // Utils.WriteLine("stopwatch: " + t);
                    if (!f_animation_wait) break;
                    var p = start_point + diff * (t+1) * temp_mag;
                    SetLocation((int)p.X, (int)p.Y);
                    Thread.Sleep(1);
                }
                SetLocation((int)temp_end_point.X,(int)temp_end_point.Y);
                start_point = temp_end_point;
                animation_stopwatch.Stop();
            }

        }


        static public void SetLocation(int x,int y)
        {
            if (!IsControl) return;

           // var point = button2.Parent.PointToScreen(button2.Location); // button2の座標取得
            NativeMethods.SetCursorPos(x, y);

            //System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }

        static public void Click(MouseState state)
        {
            if (!IsControl) return;

            if (!CanClick) return;

            if (f_drag)
            {
                f_drag = false;

                if(!f_on_form)
                    return;
            }


            clicking = new Thread(new ParameterizedThreadStart(Clicking));

            var input_state = state;
            if (f_on_form)
            {
                input_state = MouseState.LeftClick;
            }

            clicking.Start(input_state);
        }




        static void Clicking(object state)
        {
            MouseState sub = (MouseState)state;
           // f_clicking_lock = true;

            if (sub == MouseState.LeftClick)
            {
                LeftClick();
            }
            else if(sub == MouseState.LeftDoubleClick)
            {
                LeftClick();
                Thread.Sleep(wait_time_double);
                LeftClick();

            }
            else if (sub == MouseState.LeftDrag)
            {
                f_drag = true;
                MouseInput(NativeMethods.MOUSEEVENT.LEFTDOWN);
                while (f_drag) Thread.Sleep(1);
                MouseInput(NativeMethods.MOUSEEVENT.LEFTUP);
            }

            else if (sub == MouseState.RightClick)
            {
                RightClick();
            }
            else if (sub == MouseState.RightDoubleClick)
            {
                RightClick();
                Thread.Sleep(wait_time_double);
                RightClick();

            }
            else if (sub == MouseState.RightDrag)
            {
                f_drag = true;
                MouseInput(NativeMethods.MOUSEEVENT.RIGHTDOWN);
                while (f_drag) Thread.Sleep(1);
                MouseInput(NativeMethods.MOUSEEVENT.RIGHTUP);
            }

            else if (sub == MouseState.MiddleClick)
            {
                MiddleClick();
            }
            else if (sub == MouseState.MiddleDoubleClick)
            {
                MiddleClick();
                Thread.Sleep(wait_time_double);
                MiddleClick();

            }
            else if (sub == MouseState.MiddleDrag)
            {
                f_drag = true;
                MouseInput(NativeMethods.MOUSEEVENT.MIDDLEDOWN);
                while (f_drag) Thread.Sleep(1);
                MouseInput(NativeMethods.MOUSEEVENT.MIDDLEUP);
            }

            else if (sub == MouseState.ScrollUp)
            {
                while (CursorControl.IsDwell)
                {
                    MouseInput(NativeMethods.MOUSEEVENT.WHEEL, 10);
                    Thread.Sleep(1);
                }
            }

            else if (sub == MouseState.ScrollDown)
            {
              //  f_clicking_lock = true;
                while (CursorControl.IsDwell)
                {
                    //   mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -10, 0);
                    MouseInput(NativeMethods.MOUSEEVENT.WHEEL, -10);

                    Thread.Sleep(1);
                }
            }
        }

        static void MouseInput(NativeMethods.MOUSEEVENT flags,int data=0){
            var inp = new NativeMethods.Input();

            inp.Type = 0;
            inp.ui.Mouse.X = 0;
            inp.ui.Mouse.Y = 0;
            inp.ui.Mouse.Data = data;
            inp.ui.Mouse.Flags = (int)flags | 1 ;
            inp.ui.Mouse.ExtraInfo = (System.IntPtr)69;

            NativeMethods.SendInput(1, ref inp, Marshal.SizeOf(inp));
        }

        static void LeftClick()
        {
            MouseInput(NativeMethods.MOUSEEVENT.LEFTDOWN);
            Thread.Sleep(wait_time);
            MouseInput(NativeMethods.MOUSEEVENT.LEFTUP);
        }

        static void RightClick()
        {
            MouseInput(NativeMethods.MOUSEEVENT.RIGHTDOWN);
            Thread.Sleep(wait_time);
            MouseInput(NativeMethods.MOUSEEVENT.RIGHTUP);
        }

        static void MiddleClick()
        {
            MouseInput(NativeMethods.MOUSEEVENT.MIDDLEDOWN);
            Thread.Sleep(wait_time);
            MouseInput(NativeMethods.MOUSEEVENT.MIDDLEUP);
        }

    }
}
