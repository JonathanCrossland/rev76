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
using Rev76.Windows.Widgets;
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

            Icon icon = GetIcon();

            CreateSystemTrayIcon(icon);

            Trace.WriteLine("Handing over to game.");

            bool ret = WindowManager.HandOverToGame();

            Rev76Widget widget = new Rev76Widget(0, 0, 84, 84, icon);
           
            widget.Show();

            if (!ret)
            {
                Console.WriteLine("Game not found");
            }

            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(10); // Adjust delay as needed
            }
        }

        private static void CreateSystemTrayIcon(Icon icon)
        {
            Task.Run(() =>
            {
                _SystemTrayIcon.AddIcon(icon, "Rev76", () =>
                {
                    Environment.Exit(0);
                });
            });
        }

        private static Icon GetIcon()
        {
            Icon icon = null;
            ResourceManager rm = new ResourceManager("Rev76.AppResources", typeof(AppResources).Assembly);
            byte[] iconBytes = (byte[])rm.GetObject("rev76");

            using (MemoryStream ms = new MemoryStream(iconBytes))
            {
                icon = new Icon(ms);

            }

            return icon;
        }
    }
}
