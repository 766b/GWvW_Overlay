using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GWvW_Overlay.Keyboard
{
    public class Natives
    {
        public static int GWL_ExStyle = -20;
        public static int WS_EX_Transparent = 0x20;
        public static int WS_EX_Layered = 0x80000;

        public static uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        public static uint WINEVENT_OUTOFCONTEXT = 0;
        public static uint EVENT_SYSTEM_FOREGROUND = 3;
        public static uint WINEVENT_SKIPOWNPROCESS = 2;

        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNew);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmd);

        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);
    }
}
