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
        static public void Move(int dx, int dy)
        {
            if (!IsControl) return;
            var p = System.Windows.Forms.Cursor.Position;
            p.X += dx;
            p.Y += dy;
            System.Windows.Forms.Cursor.Position = p;
        }

        static public void SetLocation(int x,int y)
        {
            if (!IsControl) return;
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }

        static public void Click(MouseState state)
        {
            if (!IsControl) return;

            if (clicking != null && clicking.IsAlive)
                clicking.Abort();


            clicking = new Thread(new ParameterizedThreadStart(Clicking));
            clicking.Start(state);
        }

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;
        private const int MOUSEEVENTF_RIGHTTDOWN = 0x8;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x800;
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        static bool f_clicking_lock = false;
        static void Clicking(object state)
        {
            MouseState sub = (MouseState)state;
            f_clicking_lock = true;
            if (sub == MouseState.LeftClick)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                Thread.Sleep(10);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (sub == MouseState.RightClick)
            {
                mouse_event(MOUSEEVENTF_RIGHTTDOWN, 0, 0, 0, 0);
                Thread.Sleep(10);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
            else if (sub == MouseState.Drag)
            {
                
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                while (f_clicking_lock) Thread.Sleep(1);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (sub == MouseState.ScrollUp)
            {

                while (f_clicking_lock)
                {
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 10, 0);
                    Thread.Sleep(1);
                }
            }
            else if (sub == MouseState.ScrollDown)
            {

                while (f_clicking_lock)
                {
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -1, 0);
                    Thread.Sleep(1);
                }
            }
        }

    }
}
