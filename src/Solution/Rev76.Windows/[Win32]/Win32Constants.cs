using System;

namespace Rev76.Windows
{
    public partial class Win32
    {
        // messages
        public const uint WM_NULL = 0x0000;
        public const int WM_USER = 0x0400;
        public const int WM_DESTROY = 0x0002;
        public const int WM_CLOSE = 0x0010;
        public const int WM_COMMAND = 0x0111;
        public const uint WM_SETFOCUS = 0x0007;
        public const uint WM_PAINT = 0x000F;
        public const uint WM_QUIT = 0x0012;

        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_RBUTTONDOWN = 0x0204;

        public const int WM_NCHITTEST = 0x84;
        public const uint WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_EXITSIZEMOVE = 0x0232;
        
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        public const int HTCLIENT = 1;
        public const int HTCAPTION = 0x02;

        public const uint SWP_NOACTIVATE = 0x0010; 
        public const uint SWP_NOMOVE = 0x0002;   
        public const uint SWP_NOSIZE = 0x0001;

        
        public const uint PM_REMOVE = 0x0001;

        // ShowWindow
        public const int SW_HIDE = 0;  
        public const int SW_SHOW = 5;  
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;


        // Window Styles
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        // UpdateLayeredWindow
        public const byte AC_SRC_OVER = 0x00; // Blend source and destination based on alpha
        public const byte AC_SRC_ALPHA = 0x01; // Use alpha channel in the source bitmap
        public const int LWA_ALPHA = 0x2;

        //cursor
        public const int IDC_ARROW = 32512;


        // Constants for window styles
        public const int GWL_EXSTYLE = -20;


        // Constants for GetSystemMetrics
        public const int SM_CXSCREEN = 0; // Width of the primary screen
        public const int SM_CYSCREEN = 1; // Height of the primary screen
        public const int SM_XVIRTUALSCREEN = 76; // X-coordinate of the virtual screen
        public const int SM_YVIRTUALSCREEN = 77; // Y-coordinate of the virtual screen
        public const int SM_CXVIRTUALSCREEN = 78; // Width of the virtual screen
        public const int SM_CYVIRTUALSCREEN = 79; // Height of the virtual screen


        #region System Tray

        public const int NIF_ICON = 0x00000002;
        public const int NIF_MESSAGE = 0x00000001;
        public const int NIF_TIP = 0x00000004;
        public const int NIM_ADD = 0x00000000;
        public const int NIM_DELETE = 0x00000002;


        #endregion

        public const int TPM_RIGHTBUTTON = 0x0002;
        public const int TPM_RETURNCMD = 0x0100;
        public const uint TPM_NONOTIFY = 0x0080;


        public const int MF_STRING = 0x00000000;
        public const uint MF_SEPARATOR = 0x0800;
        public const uint MF_CHECKED = 0x00000008;


    }
}
