// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertManager.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The cert manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    /// <summary>
    ///     The cert manager.
    /// </summary>
    public abstract class CertManager : ClassRegistry.PlatformDependentClassBaseType
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
        public abstract bool CreateAuthority(string name, string path);

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
        public abstract bool CreateCert(string domainName, string authorityPath, string path);

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
        public abstract bool RegisterAuthority(string name, string authorityPath, bool firefox = true);

        #endregion
    }
}