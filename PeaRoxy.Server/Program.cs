using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Controller sController = new Controller();
            sController.Start();
            bool doReDraw = true;
            if (Environment.CommandLine.ToLower().IndexOf("silent") != -1)
                doReDraw = false;
            Console.WriteLine("PeaRoxy Server Started at " + sController.IP.ToString() + ":" + sController.Port.ToString());
            while (true)
            {
                if (doReDraw)
                    Screen.reDraw(sController);
                else
                    Screen.reDraw(sController, false);

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
