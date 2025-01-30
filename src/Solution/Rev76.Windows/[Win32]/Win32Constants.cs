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

        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_RBUTTONUP = 0x0205;


        // ShowWindow
        public const int SW_HIDE = 0;  
        public const int SW_SHOW = 5;  
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;

      

        //cursor
        public const int IDC_ARROW = 32512;

        #region System Tray

        public const int NIF_ICON = 0x00000002;
        public const int NIF_MESSAGE = 0x00000001;
        public const int NIF_TIP = 0x00000004;
        public const int NIM_ADD = 0x00000000;
        public const int NIM_DELETE = 0x00000002;

        //menu
        public const int IDM_QUIT = 1000;

        #endregion

        public const int TPM_RIGHTBUTTON = 0x0002;
        public const int TPM_RETURNCMD = 0x0100;

        public const int MF_STRING = 0x00000000;
    }
}
