using System;
using System.Runtime.InteropServices;

namespace Rev76.Windows
{
    public static partial class Win32
    {
        #region WindProc

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );


        [DllImport("user32.dll")]
        public static extern IntPtr RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;                // Handle to the window whose position is changing
            public IntPtr hwndInsertAfter;      // Handle to the window after which to insert
            public int x;                      // New position of the window (x-coordinate)
            public int y;                      // New position of the window (y-coordinate)
            public int cx;                     // New width of the window
            public int cy;                     // New height of the window
            public uint flags;                 // Flags indicating the change
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;             // Specifies the blend operation.
            public byte BlendFlags;          // Must be 0.
            public byte SourceConstantAlpha; // Specifies the transparency level (0 to 255).
            public byte AlphaFormat;         // Specifies whether the source has an alpha channel (AC_SRC_ALPHA).
        }


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UpdateLayeredWindow(
            IntPtr hwnd,                // Handle to the layered window
            IntPtr hdcDest,             // Handle to the screen DC
            ref POINT pptDest,          // New screen position of the layered window
            ref SIZE psize,             // New size of the layered window
            IntPtr hdcSrc,              // Handle to the DC with the layered window content
            ref POINT pptSrc,           // Location of the layered window content
            int crKey,                  // Color key (transparent color)
            ref BLENDFUNCTION pblend,   // Transparency and alpha information
            uint dwFlags                // Options for the layered window
        );

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        #region cursor

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        #endregion

        #region messages

        // Message struct
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

    }
}
