using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Windows
{
    public class WindowsModule : PeaRoxy.Platform.ClassRegistry
    {
        public override void RegisterPlatform()
        {
            RegisterClass<Platform.CertManager>(new Windows.WindowsCertManager());
            RegisterClass<Platform.ConnectionInfo>(new Windows.WindowsConnection());
        }
    }
}
