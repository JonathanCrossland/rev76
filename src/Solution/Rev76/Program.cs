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

using Rev76.DataModels;
using Rev76.Logging;
using Rev76.Windows;

namespace Rev76
{
    internal class Program
    {
        private static Tracing Console = new Tracing();

        static void Main(string[] args)
        {
            Console.StartTracing("rev76.log",0,true);

            Console.WriteLine($"Rev76.{new AppData().Version}");

            Console.WriteLine("Handing over to game.");

            bool ret = WindowManager.HandOverToGame();

            if (!ret)
            {
                Console.WriteLine("Game not found");
                
            }
        }
    }
}
