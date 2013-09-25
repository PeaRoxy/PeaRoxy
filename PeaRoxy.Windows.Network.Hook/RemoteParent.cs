using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Windows.Network.Hook
{
    public class RemoteParent : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("FileMon has been installed in target {0}.\n", InClientPID);
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported an error:\n" + InInfo.ToString());
        }

        public void LogToScreen(string message)
        {
            Console.WriteLine(message);
        }

        public void Ping()
        {
        }
    }
}
