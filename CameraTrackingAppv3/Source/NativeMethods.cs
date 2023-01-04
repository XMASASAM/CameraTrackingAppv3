using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace CameraTrackingAppv3
{
    public static class NativeMethods
    {
        public enum MOUSEEVENT
        {
            MOVE = 0x1,
            LEFTDOWN = 0x2,
            LEFTUP = 0x4,
            RIGHTDOWN = 0x8,
            RIGHTUP = 0x10,
            MIDDLEDOWN= 0x20,
            MIDDLEUP = 0x40,
            XDOWN = 0x100,
            XUP = 0x800,
            WHEEL = 0x800,
            HWHEEL = 0x1000,
            MOVE_NOCOALESCE = 0x2000,
            VIRTUALDESK = 0x4000,
            ABSOLUTE = 0x8000
        }
     /*   public const int MOUSEEVENTF_LEFTDOWN = 0x2;
        public const int MOUSEEVENTF_LEFTUP = 0x4;
        public const int MOUSEEVENTF_RIGHTTDOWN = 0x8;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_WHEEL = 0x800;*/
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        public extern static void SendInput(int nInputs,ref Input pInputs, int cbsize);

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        public extern static int MapVirtualKey(int wCode, int wMapType);


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public short VirtualKey;
            public short ScanCode;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public int Type;
            public InputUnion ui;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput Mouse;
            [FieldOffset(0)]
            public KeyboardInput Keyboard;
            [FieldOffset(0)]
            public HardwareInput Hardware;
        }

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_SCANCODE = 0x0008;
        public const int KEYEVENTF_UNICODE = 0x0004;

        public const int MAPVK_VK_TO_VSC = 0;
        // private const int MAPVK_VSC_TO_VK = 1;

    }


}
