using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PeaRoxy.Windows
{
    public class WindowsCertManager : PeaRoxy.Platform.CertManager
    {
        public override bool CreateAuthority(string name, string path)
        {
            string currentAddress = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (!File.Exists(Path.Combine(currentAddress, path)))
            {
                System.Diagnostics.Process p = Common.CreateProcess(
                    Path.Combine(currentAddress, "HTTPSCerts\\makecert.exe"),
                    "-pe -n \"CN=" + name + "\" -cy authority -ss my -sr LocalMachine -a sha1 -sky signature -r " + path);
                if (p != null)
                    p.WaitForExit();
                return File.Exists(Path.Combine(currentAddress, path));
            }
            return true;
        }

        public override bool CreateCert(string domainName, string authorityPath, string path)
        {
            domainName = domainName.ToLower().Trim();
            string currentAddress = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (!File.Exists(Path.Combine(currentAddress, path)))
            {
                System.Diagnostics.Process p = Common.CreateProcess(
                    Path.Combine(currentAddress, "HTTPSCerts\\makecert.exe"),
                    "-pe -n \"CN=" + domainName + "\" -ss my -sr LocalMachine -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -ic \"" + authorityPath + "\" -is my -ir LocalMachine -sp \"Microsoft RSA SChannel Cryptographic Provider\" -sy 12 \"" + path + "\""
                    );
                if (p != null)
                    p.WaitForExit();
                return File.Exists(Path.Combine(currentAddress, path));
            }
            return true;
        }

        public override bool RegisterAuthority(string name, string authorityPath, bool firefox = true)
        {
            string currentAddress = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (File.Exists(Path.Combine(currentAddress, authorityPath)))
            {
                System.Diagnostics.Process p = Common.CreateProcess(
                    Path.Combine(currentAddress, "HTTPSCerts\\certmgr.exe"),
                    "-add -c \"" + authorityPath + "\" -s -r LocalMachine root"
                    );
                if (p != null)
                    p.WaitForExit();
                if (!firefox)
                    return true;
                string firefoxProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath))
                    return true;
                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                {
                    System.Diagnostics.Process p1 = Common.CreateProcess(
                        Path.Combine(currentAddress, "HTTPSCerts\\certutil.exe"),
                        "-A -n \"" + name + "\" -t \"TCu,TCu,TCu\" -d \"" + profileAddress + "\" -i \"" + authorityPath + "\""
                        );
                    if (p1 != null)
                        p1.WaitForExit();
                }
                return true;
            }
            return false;
        }
    }
}
