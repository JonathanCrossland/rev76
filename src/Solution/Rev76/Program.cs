//  ===============================================================
//           Copyright (c) Jonathan Crossland
//                  All Rights Reserved
//  ===============================================================
//
//      ██╗ █████╗ ███╗   ███╗ ██████╗ ███╗   ██╗
//      ██║██╔══██╗████╗ ████║██╔═══██╗████╗  ██║
//      ██║███████║██╔████╔██║██║   ██║██╔██╗ ██║
// ██   ██║██╔══██║██║╚██╔╝██║██║   ██║██║╚██╗██║
// ╚█████╔╝██║  ██║██║ ╚═╝ ██║╚██████╔╝██║ ╚████║
//  ╚════╝ ╚═╝  ╚═╝╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝[]
//
//  This file is part of the proprietary software of Jonathan Crossland.
//  Redistribution or use without explicit permission is prohibited.
//  I have not yet decided on a license.

using Rev76.Core.Logging;
using Rev76.DataModels;
using Rev76.Windows;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace Rev76
{
    internal class Program
    {
        
        private static SystemTrayIcon _SystemTrayIcon = new SystemTrayIcon();

        [STAThread]
        private static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Tracing.StartTracing("rev76.log", TraceEventType.Error);
            Trace.WriteLine($"Rev76.{new AppData().Version}");

            CreateSystemTrayIcon();

            Trace.WriteLine("Handing over to game.");

            bool ret = WindowManager.HandOverToGame();

            if (!ret)
            {
                Console.WriteLine("Game not found");
                
            }

            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(10); // Adjust delay as needed
            }
        }

        private static void CreateSystemTrayIcon()
        {
            Task.Run(() =>
            {

                Icon icon = null;
                ResourceManager rm = new ResourceManager("Rev76.AppResources", typeof(AppResources).Assembly);
                byte[] iconBytes = (byte[])rm.GetObject("rev76"); 

                using (MemoryStream ms = new MemoryStream(iconBytes))
                {
                    icon = new Icon(ms); 
     
                }

                _SystemTrayIcon.AddIcon(icon, "Rev76", () =>
                {
                    Environment.Exit(0);
                });
            });
        }
    }
}
