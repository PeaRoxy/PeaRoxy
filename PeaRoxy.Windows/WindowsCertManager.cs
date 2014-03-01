// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsCertManager.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The windows cert manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using PeaRoxy.Platform;

    #endregion

    /// <summary>
    /// The windows cert manager.
    /// </summary>
    public class WindowsCertManager : CertManager
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create authority.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool CreateAuthority(string name, string path)
        {
            string currentAddress = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentAddress == null) return false;
            if (File.Exists(Path.Combine(currentAddress, "HTTPSCerts", path))) return true;

            Process p = Common.CreateProcess(
                Path.Combine("HTTPSCerts", "makecert.exe"),
                "-pe -n \"CN=" + name + "\" -cy authority -ss my -sr LocalMachine -a sha1 -sky signature -r \"" + path
                + "\"");
            if (p != null) p.WaitForExit();

            return File.Exists(Path.Combine(currentAddress, "HTTPSCerts", path));
        }

        /// <summary>
        /// The create cert.
        /// </summary>
        /// <param name="domainName">
        /// The domain name.
        /// </param>
        /// <param name="authorityPath">
        /// The authority path.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool CreateCert(string domainName, string authorityPath, string path)
        {
            domainName = domainName.ToLower().Trim();
            string currentAddress = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentAddress == null) return false;
            if (File.Exists(Path.Combine(currentAddress, "HTTPSCerts", path))) return true;

            Process p = Common.CreateProcess(
                Path.Combine("HTTPSCerts", "makecert.exe"),
                "-pe -n \"CN=" + domainName
                + "\" -ss my -sr LocalMachine -a sha1 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -ic \"" + authorityPath
                + "\" -is my -ir LocalMachine -sp \"Microsoft RSA SChannel Cryptographic Provider\" -sy 12 \""
                + path + "\"");
            if (p != null) p.WaitForExit();

            return File.Exists(Path.Combine(currentAddress, "HTTPSCerts", path));
        }

        /// <summary>
        /// The register authority.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="authorityPath">
        /// The authority path.
        /// </param>
        /// <param name="firefox">
        /// The firefox.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool RegisterAuthority(string name, string authorityPath, bool firefox = true)
        {
            Process p = Common.CreateProcess(
                Path.Combine("HTTPSCerts", "certmgr.exe"),
                "-add -c \"" + authorityPath + "\" -s -r LocalMachine root");
            if (p != null) p.WaitForExit();

            try
            {
                if (!firefox) return true;

                string firefoxProfilesPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Mozilla\\Firefox\\Profiles");
                if (!Directory.Exists(firefoxProfilesPath)) return true;

                foreach (string profileAddress in Directory.GetDirectories(firefoxProfilesPath))
                {
                    Process p1 = Common.CreateProcess(
                        Path.Combine("HTTPSCerts", "certutil.exe"),
                        "-A -n \"" + name + "\" -t \"TCu,TCu,TCu\" -d \"" + profileAddress + "\" -i \"" + authorityPath
                        + "\"");
                    if (p1 != null) p1.WaitForExit();
                }
            }
            catch { }
            return true;
        }

        #endregion
    }
}