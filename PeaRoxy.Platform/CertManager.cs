// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertManager.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    /// <summary>
    ///     This class is base class of all platform dependent classes about certifications
    /// </summary>
    public abstract class CertManager : ClassRegistry.PlatformDependentClassBaseType
    {
        /// <summary>
        ///     This method will create a Authority certification
        /// </summary>
        /// <param name="name">
        ///     The name of authority.
        /// </param>
        /// <param name="path">
        ///     The path of the file.
        /// </param>
        /// <returns>
        ///     A <see cref="bool" /> value shows if process ended successfully.
        /// </returns>
        public abstract bool CreateAuthority(string name, string path);

        /// <summary>
        ///     This method will create a certification
        /// </summary>
        /// <param name="domainName">
        ///     The domain name.
        /// </param>
        /// <param name="authorityPath">
        ///     The authority certification file path.
        /// </param>
        /// <param name="path">
        ///     The path of the file.
        /// </param>
        /// <returns>
        ///     A <see cref="bool" /> value shows if process ended successfully.
        /// </returns>
        public abstract bool CreateCert(string domainName, string authorityPath, string path);

        /// <summary>
        ///     This method will register the authority certification file in OS directories if needed.
        /// </summary>
        /// <param name="name">
        ///     The name of authority.
        /// </param>
        /// <param name="authorityPath">
        ///     The authority certification file path.
        /// </param>
        /// <param name="firefox">
        ///     This value indicates if we should try and register it in Firefox's trusted certifications directory.
        /// </param>
        /// <returns>
        ///     A <see cref="bool" /> value shows if process ended successfully.
        /// </returns>
        public abstract bool RegisterAuthority(string name, string authorityPath, bool firefox = true);
    }
}