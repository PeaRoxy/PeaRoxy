// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigUser.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CommonLibrary
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    ///     A class to represent a user name and password
    /// </summary>
    public class ConfigUser
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigUser" /> class.
        /// </summary>
        /// <param name="username">
        ///     The username.
        /// </param>
        /// <param name="password">
        ///     The password.
        /// </param>
        public ConfigUser(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        ///     Gets the hash of the user's password
        /// </summary>
        public byte[] Hash
        {
            get
            {
                return MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(this.Password));
            }
        }

        /// <summary>
        ///     Gets the password of the user
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        ///     Gets the user name of the user
        /// </summary>
        public string Username { get; private set; }
    }
}