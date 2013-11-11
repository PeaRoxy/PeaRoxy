using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ZARA
{
    static class Program
    {
        public static System.Windows.Forms.NotifyIcon Notify { get; private set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Notify = new System.Windows.Forms.NotifyIcon();
            Notify.Visible = false;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frm_Main());
        }
    }
}
