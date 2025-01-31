using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rev76.Windows
{

    public class WindowManager
    {
        public static Dictionary<IntPtr, OverlayWindow> Windows = new Dictionary<IntPtr, OverlayWindow>();
        private static readonly object lockObject = new object();

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

        internal static void Remove(OverlayWindow window)
        {
            if (window == null) return;
            if (window.HWND == IntPtr.Zero) return;

            Windows.Remove(window.HWND);
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
