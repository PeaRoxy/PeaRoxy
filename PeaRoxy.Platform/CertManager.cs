using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Platform
{
    public abstract class CertManager : ClassRegistry.PlatformDependentClassBaseType
    {
        public abstract bool CreateAuthority(string name, string path);
        public abstract bool CreateCert(string domainName, string authorityPath, string path);
        public abstract bool RegisterAuthority(string name, string authorityPath, bool firefox = true);
    }
}
