// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsModule.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The windows module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using PeaRoxy.Platform;

    #endregion

    /// <summary>
    /// The windows module.
    /// </summary>
    public class WindowsModule : ClassRegistry
    {
        #region Public Methods and Operators

        /// <summary>
        /// The register platform.
        /// </summary>
        public override void RegisterPlatform()
        {
            RegisterClass<CertManager>(new WindowsCertManager());
            RegisterClass<ConnectionInfo>(new WindowsConnection());
        }

        #endregion
    }
}