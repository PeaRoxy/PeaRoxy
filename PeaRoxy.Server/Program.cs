// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Threading;

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
            PeaRoxyController sController = new PeaRoxyController(Settings.Default);
            sController.Start();
            Console.WriteLine(
                "PeaRoxy Server v{2} Started and now listening to {0}:{1}",
                (sController.Ip.Equals(IPAddress.Any)) ? "All Interfaces" : sController.Ip.ToString(),
                sController.Port,
                Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Press X to Exit");
            Thread.Sleep(2000);
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