using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Rev76.Windows
{
    public class WindowManager
    {
        public static Dictionary<IntPtr, OverlayWindow> Windows = new Dictionary<IntPtr, OverlayWindow>();
        private static readonly object lockObject = new object();

        public static event EventHandler<string> WindowClosed;

        public class Screen
        {
            public Screen() {
                
            }

            public static Win32.SIZE PrimaryScreen
            {
                get
                {
                    int screenWidth = Win32.GetSystemMetrics(0);  // SM_CXSCREEN
                    int screenHeight = Win32.GetSystemMetrics(1); // SM_CYSCREEN

                    return new Win32.SIZE() { CX = screenWidth, CY = screenHeight };
                }
            }
        }
     
        public static bool HandOverToGame()
        {
            IntPtr gameWindow = GetGameWindow();
            if (gameWindow != IntPtr.Zero)
            {
                return Win32.SetForegroundWindow(gameWindow); 
            }
            else
            {
                Trace.TraceError("FindWindow did not find the ACC window");
            }

            return false;
        }

        private static IntPtr GetGameWindow()
        {
            // AC2 must have these spaces - its silly but thats the title
            // and it may change with an update.
            return Win32.FindWindow("UnrealWindow", "AC2  ");
        }

        internal static void AddWindow(OverlayWindow window)
        {
            lock (lockObject)
            {
                Windows.Add(window.HWND, window);
            }
        }

        public static void RemoveWindow(OverlayWindow window)
        {
            if (window == null) return;
            if (window.HWND == IntPtr.Zero) return;
            if (!Windows.ContainsKey(window.HWND)) return;
            
            Windows.Remove(window.HWND);
            //window.Dispose();
            WindowClosed?.Invoke(null,window.Title);
            
        }

        internal static void CloseWindows()
        {
            foreach (OverlayWindow window in Windows.Values)
            {
                window.Dispose();
            }

            Windows.Clear();
        }

    }
}
