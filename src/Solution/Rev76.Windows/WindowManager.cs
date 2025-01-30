using System;
using System.Diagnostics;

namespace Rev76.Windows
{
    public class WindowManager
    {
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
    }
}
