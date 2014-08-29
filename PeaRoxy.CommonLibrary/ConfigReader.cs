// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigReader.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.CommonLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     The helper class to read the configuration file, user accounts file and blacklist file
    /// </summary>
    public static class ConfigReader
    {
        private static List<string> blackList;

        private static Dictionary<string, string> settings;

        private static Collection<ConfigUser> users;

        private static DateTime blackListLastWrite;

        private static DateTime settingListLastWrite;

        private static DateTime userListLastWrite;

        private static DateTime blackListLastReadTime;

        private static DateTime settingListLastReadTime;

        private static DateTime userListLastReadTime;

        /// <summary>
        ///     The get list of blacklisted addresses
        /// </summary>
        /// <param name="fileAddress">
        ///     The file address to read the list from
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Collection</cref>
        ///     </see>
        ///     of black listed addresses.
        /// </returns>
        public static IEnumerable<string> GetBlackList(string fileAddress)
        {
            if (blackList != null
                && ((DateTime.Now - blackListLastReadTime) < new TimeSpan(0, 1, 0)
                    || blackListLastWrite.Equals(File.GetLastWriteTime(fileAddress))))
            {
                if ((DateTime.Now - blackListLastReadTime) >= new TimeSpan(0, 1, 0))
                {
                    blackListLastReadTime = DateTime.Now;
                }
                return blackList;
            }

            blackList = new List<string>();
            try
            {
                StreamReader st = new StreamReader(fileAddress);
                blackListLastWrite = File.GetLastWriteTime(fileAddress);
                blackListLastReadTime = DateTime.Now;
                while (!st.EndOfStream)
                {
                    try
                    {
                        string line = st.ReadLine();
                        if (line == null)
                        {
                            continue;
                        }
                        if (line.IndexOf('#') > -1)
                        {
                            line = line.Substring(0, line.IndexOf('#'));
                        }

                        if (line.IndexOf(';') > -1)
                        {
                            line = line.Substring(0, line.IndexOf(';'));
                        }
                        line = line.Trim().ToLower();
                        blackList.Add(line.IndexOf(':') == -1 ? line + ":*" : line);
                    }
                    catch (Exception)
                    {
                    }
                }

                st.Close();
            }
            catch (Exception)
            {
            }

            return blackList;
        }

        /// <summary>
        ///     Get list of settings
        /// </summary>
        /// <param name="fileAddress">
        ///     The address for settings file.
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Dictionary</cref>
        ///     </see>
        ///     of key/value for each setting.
        /// </returns>
        public static Dictionary<string, string> GetSettings(string fileAddress)
        {
            if (settings != null
                && ((DateTime.Now - settingListLastReadTime) < new TimeSpan(0, 1, 0)
                    || settingListLastWrite.Equals(File.GetLastWriteTime(fileAddress))))
            {
                if ((DateTime.Now - settingListLastReadTime) >= new TimeSpan(0, 1, 0))
                {
                    settingListLastReadTime = DateTime.Now;
                }
                return settings;
            }

            string[] args = Environment.CommandLine.Split('/');
            settings = new Dictionary<string, string>();
            try
            {
                StreamReader st = new StreamReader(fileAddress);
                settingListLastWrite = File.GetLastWriteTime(fileAddress);
                settingListLastReadTime = DateTime.Now;
                while (!st.EndOfStream)
                {
                    try
                    {
                        string line = st.ReadLine();
                        if (line == null)
                        {
                            continue;
                        }
                        if (line.IndexOf('#') > -1)
                        {
                            line = line.Substring(0, line.IndexOf('#'));
                        }

                        if (line.IndexOf(';') > -1)
                        {
                            line = line.Substring(0, line.IndexOf(';'));
                        }

                        if (line.IndexOf('=') <= 0 || line.Trim().Length <= 3)
                        {
                            continue;
                        }

                        string option = line.Substring(0, line.IndexOf('=')).Trim().ToLower();
                        string value = line.Substring(line.IndexOf('=') + 1);
                        foreach (string arg in args.Where(arg => arg.ToLower().Trim().StartsWith(option)))
                        {
                            value =
                                arg.Substring(arg.ToLower().IndexOf(option, StringComparison.Ordinal) + option.Length)
                                    .Trim();
                        }

                        if (option.Trim().Length > 0 && value.Length > 0)
                        {
                            settings.Add(option, value);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                st.Close();
            }
            catch (Exception)
            {
            }

            return settings;
        }

        /// <summary>
        ///     Get list of acceptable users
        /// </summary>
        /// <param name="fileAddress">
        ///     The address of users file.
        /// </param>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>Collection</cref>
        ///     </see>
        ///     of users.
        /// </returns>
        public static Collection<ConfigUser> GetUsers(string fileAddress)
        {
            if (users != null
                && ((DateTime.Now - userListLastReadTime) < new TimeSpan(0, 1, 0)
                    || userListLastWrite.Equals(File.GetLastWriteTime(fileAddress))))
            {
                if ((DateTime.Now - userListLastReadTime) >= new TimeSpan(0, 1, 0))
                {
                    userListLastReadTime = DateTime.Now;
                }
                return users;
            }

            users = new Collection<ConfigUser>();
            try
            {
                StreamReader st = new StreamReader(fileAddress);
                userListLastWrite = File.GetLastWriteTime(fileAddress);
                userListLastReadTime = DateTime.Now;
                while (!st.EndOfStream)
                {
                    try
                    {
                        string line = st.ReadLine();
                        if (line == null)
                        {
                            continue;
                        }
                        if (line.IndexOf('#') > -1)
                        {
                            line = line.Substring(0, line.IndexOf('#'));
                        }

                        if (line.IndexOf(';') > -1)
                        {
                            line = line.Substring(0, line.IndexOf(';'));
                        }

                        if (line.IndexOf('=') > 2 && line.Trim().Length > 5)
                        {
                            string username = line.Substring(0, line.IndexOf('=')).Trim();
                            string password = line.Substring(line.IndexOf('=') + 1);
                            if (username.Trim().Length > 2 && password.Length > 2)
                            {
                                users.Add(new ConfigUser(username, password));
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                st.Close();
            }
            catch (Exception)
            {
            }

            return users;
        }
    }
}