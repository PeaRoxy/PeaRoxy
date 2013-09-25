using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Text;
using EasyHook;
namespace PeaRoxy.Windows.Network.Hook
{
    class Injector
    {
        static String ChannelName = null;
        static bool isDebug = false;
        [System.Diagnostics.Conditional("DEBUG")]
        static void isDebugSetter() { isDebug = true; }
        static void Main(string[] args)
        {
            isDebugSetter();
            bool noGAC  = false;
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("seamonkey");
            Array.Reverse(processes);
            try
            {
                Config.Register(
                    "PeaRoxy",
                    isDebug ? "PeaRoxy.Windows.Network.Hook.exe" : System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
            }
            catch (ApplicationException)
            {
                Console.WriteLine("This is an administrative task! No admin privilege. Try to not use GAC");
                noGAC = true;
            }
            RemoteHooking.IpcCreateServer<RemoteParent>(ref ChannelName, WellKnownObjectMode.SingleCall);
            foreach (System.Diagnostics.Process p in processes)
            {
                try
                {
                    RemoteHooking.Inject(
                       p.Id,
                       noGAC    ?   InjectionOptions.DoNotRequireStrongName   : InjectionOptions.Default,
                       isDebug  ?   "PeaRoxy.Windows.Network.Hook.exe"        : "PeaRoxy.Windows.Network.Hook_x86.exe",
                       isDebug  ?   "PeaRoxy.Windows.Network.Hook.exe"        : "PeaRoxy.Windows.Network.Hook_x64.exe",
                       new object[] { ChannelName });
                }
                catch (Exception ExtInfo)
                {
                    Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
                }
            }
            Console.ReadLine();
        }
    }
}
