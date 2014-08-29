// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Threading;

    using CommandLine;

    internal static class Program
    {
        private static void Main()
        {
            try
            {
                if (Settings.Default.LastParserState != null)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
                return;
            }
            Controller sController = new Controller();
            sController.Start();
            Console.WriteLine("PeaRoxy Server Started at " + sController.Ip + ":" + sController.Port);
            do
            {
                while (!Console.KeyAvailable)
                {
                    Screen.ReDraw(sController, Settings.Default.ShowConnections);
                    Thread.Sleep(1000);
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.X);
        }
    }
}