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
        

        public static bool IsCursorOnForm { get { return f_on_form; } set { f_on_form = value; } }

        static public void Move(int dx, int dy)
        {
            if (!IsControl) return;
            var p = System.Windows.Forms.Cursor.Position;
            p.X += dx;
            p.Y += dy;
            SetLocation(p.X,p.Y);
            //System.Windows.Forms.Cursor.Position = p;
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
            else if (sub == MouseState.RightClick)
            {
                RightClick();
            }
            else if(sub == MouseState.DoubleClick)
            {
                LeftClick();
                Thread.Sleep(wait_time_double);
                LeftClick();

            }
            else if (sub == MouseState.Drag)
            {
                f_drag = true;
                MouseInput(NativeMethods.MOUSEEVENT.LEFTDOWN);
                while (f_drag) Thread.Sleep(1);
                MouseInput(NativeMethods.MOUSEEVENT.LEFTUP);
            }
            else if (sub == MouseState.ScrollUp)
            {
              //  f_clicking_lock = true;
                while (CursorControl.IsDwell)
                {
                    MouseInput(NativeMethods.MOUSEEVENT.WHEEL, 10);
                   // mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 10, 0);
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
            MouseInput(NativeMethods.MOUSEEVENT.RIGHTTDOWN);
            Thread.Sleep(wait_time);
            MouseInput(NativeMethods.MOUSEEVENT.RIGHTUP);
        }

    }
}
