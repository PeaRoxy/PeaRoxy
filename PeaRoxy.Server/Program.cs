// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    #region

    using System;
    using System.Threading;

    #endregion

    /// <summary>
    /// The program.
    /// </summary>
    internal static class Program
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main()
        {
            Controller sController = new Controller();
            sController.Start();
            bool doReDraw = Environment.CommandLine.ToLower().IndexOf("silent", StringComparison.Ordinal) == -1;

            Console.WriteLine("PeaRoxy Server Started at " + sController.Ip + ":" + sController.Port);
            while (true)
            {
                if (doReDraw)
                {
                    Screen.ReDraw(sController);
                }
                else
                {
                    Screen.ReDraw(sController, false);
                }

                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        #endregion
    }
}