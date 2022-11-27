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
        static Thread clicking;
        public static bool IsControl { get; set; }
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

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);



        static public void SetLocation(int x,int y)
        {
            if (!IsControl) return;

           // var point = button2.Parent.PointToScreen(button2.Location); // button2の座標取得
            SetCursorPos(x, y);

            //System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }

        static public void Click(MouseState state)
        {
            if (!IsControl) return;

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

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        private const int MOUSEEVENTF_RIGHTTDOWN = 0x8;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x800;
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


        static void Clicking(object state)
        {
            MouseState sub = (MouseState)state;
           // f_clicking_lock = true;
            if (sub == MouseState.LeftClick)
            {

                // mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                var inp = new NativeMethods.Input();
                inp.Type = 0;
                inp.ui.Mouse.X = 0;
                inp.ui.Mouse.Y = 0;
                inp.ui.Mouse.Data = 0;
                inp.ui.Mouse.Flags = 0x2;
                inp.ui.Mouse.ExtraInfo = (System.IntPtr)68;

                NativeMethods.SendInput(1,ref inp,Marshal.SizeOf(inp));
                Thread.Sleep(50);

                inp.ui.Mouse.Flags = 0x4;
                // mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                NativeMethods.SendInput(1, ref inp, Marshal.SizeOf(inp));

            }
            else if (sub == MouseState.RightClick)
            {
                mouse_event(MOUSEEVENTF_RIGHTTDOWN, 0, 0, 0, 0);
                Thread.Sleep(50);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
            else if(sub == MouseState.DoubleClick)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                Thread.Sleep(50);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                Thread.Sleep(50);

                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                Thread.Sleep(50);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (sub == MouseState.Drag)
            {
                f_drag = true;
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                while (f_drag) Thread.Sleep(1);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (sub == MouseState.ScrollUp)
            {
              //  f_clicking_lock = true;
                while (CursorControl.IsDwell)
                {
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 10, 0);
                    Thread.Sleep(1);
                }
            }

            else if (sub == MouseState.ScrollDown)
            {
              //  f_clicking_lock = true;
                while (CursorControl.IsDwell)
                {
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -10, 0);
                    Thread.Sleep(1);
                }
            }
        }

    }
}
