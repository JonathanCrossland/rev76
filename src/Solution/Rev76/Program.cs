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


using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Rev76.Core.Logging;
using Rev76.DataModels;
using Rev76.DataModels.Listeners;
using Rev76.Windows;
using Rev76.Windows.Widgets;

using Rev86.Core.Config;

namespace Rev76
{
    internal class Program
    {
        
        private static SystemTrayIcon _SystemTrayIcon = new SystemTrayIcon();
        private static ACCListener _SharedMemClient = new ACCListener();
        private static ACCBroadcastListener _ACCBroadcastListener = new ACCBroadcastListener();
        private static CancellationTokenSource cts = new CancellationTokenSource();

        [STAThread]
        private static async Task Main(string[] args)
        {
           

            Tracing.StartTracing("rev76.log", TraceEventType.Error);
            Trace.WriteLine($"Rev76.{new AppData().Version}");

            Icon icon = GetIcon();

          
            
            WidgetFactory.LoadWidgets(icon);

            CreateSystemTrayIcon(icon);

            SharedMemory(cts);
            Udp(cts);

            Trace.WriteLine("Handing over to game.");

            WindowManager.WindowClosed += WindowManager_WindowClosed;       

            bool ret = WindowManager.HandOverToGame();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_ProcessExit;
           

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            if (!ret)
            {
                Console.WriteLine("Game not found");
            }

            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(10); // Adjust delay as needed
            }
        }

        private static void WindowManager_WindowClosed(object sender, string e)
        {
            _SystemTrayIcon.SetMenuItemChecked(e, false);
        }

        private static void CreateSystemTrayIcon(Icon icon)
        {
            Task.Run(() =>
            {

                _SystemTrayIcon.AddIcon(icon, "Rev76", () =>
                {

                });

                foreach (OverlayWindow window in WindowManager.Windows.Values)
                {

                    _SystemTrayIcon.AddMenuItem(window.Title, true, () =>
                    {

                        Task.Run(() =>
                        {
                            OverlayWindow w = WindowManager.Windows.Values.FirstOrDefault(e => e.Title == window.Title);
                            if (w == default(OverlayWindow))
                            {
                                WidgetConfig wconfig = RevConfig.Instance.Widgets.Find(wc => wc.Name == window.GetType().Name);
                                
                                w = WidgetFactory.CreateWidget(wconfig, icon);

                                w.FPS = wconfig.FPS;
                               
                                _SystemTrayIcon.SetMenuItemChecked(w.Title, true);
                                w.Show();
                            }
                            else
                            {
                                WindowManager.RemoveWindow(w);
                                _SystemTrayIcon.SetMenuItemChecked(w.Title, false);
                            }
                        });

                    });

                }
           
               

                _SystemTrayIcon.AddMenuItem("Settings",false, () =>
                {
                    var widget = new Rev76Widget((WindowManager.Screen.PrimaryScreen.CX / 2) - 42, (WindowManager.Screen.PrimaryScreen.CY / 2) - 42, 84, 84, icon);
                    widget.FPS = 4;
                    widget.Show();
                });

                _SystemTrayIcon.AddMenuSeparator();

                _SystemTrayIcon.AddMenuItem("Quit", false, () =>
                {
                    Win32.PostQuitMessage(0);
                    cts.CancelAfter(100);
                });

                _SystemTrayIcon.Show();

            });
         
        }

        private static void Udp(CancellationTokenSource cts)
        {
            Task.Run(async () =>
            {
                await _ACCBroadcastListener.Listen(cts);

            }).Wait();
        }

        private static void SharedMemory(CancellationTokenSource cts)
        {
            Task.Run(async () =>
            {
                await _SharedMemClient.Listen(cts.Token);
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

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            cts.Cancel();
            
            _ACCBroadcastListener?.Dispose();
            _SharedMemClient.Dispose();
            SystemTrayIcon.RemoveIcon();
        }
    }
}
