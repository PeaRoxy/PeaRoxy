using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
namespace PeaRoxy.CommonLibrary
{
    public class ConfigReader
    {
        private static System.Collections.ObjectModel.Collection<User> _users = null;
        public static System.Collections.ObjectModel.Collection<User> GetUsers()
        {
            if (_users != null)
                return _users;
            _users = new System.Collections.ObjectModel.Collection<User>();
            try
            {
                StreamReader st = new StreamReader("users.ini");
                while (!st.EndOfStream)
                {
                    try
                    {
                        string line = st.ReadLine();
                        if (line.IndexOf('#') > -1)
                            line = line.Substring(0, line.IndexOf('#'));
                        if (line.IndexOf(';') > -1)
                            line = line.Substring(0, line.IndexOf(';'));
                        if (line.IndexOf('=') > 2 && line.Trim().Length > 5)
                        {
                            string username = line.Substring(0, line.IndexOf('=')).Trim();
                            string password = line.Substring(line.IndexOf('=') + 1);
                            if (username.Trim().Length > 2 && password.Length > 2)
                                _users.Add(new User() { Username = username, Password = password });
                        }
                    }
                    catch (Exception) { }
                }
                st.Close();
            }
            catch (Exception){}
            return _users;
        }
        private static System.Collections.Generic.Dictionary<string,string> _settings = null;
        public static System.Collections.Generic.Dictionary<string, string> GetSettings()
        {
            if (_settings != null)
                return _settings;
            string[] args = Environment.CommandLine.Split('/');
            _settings = new System.Collections.Generic.Dictionary<string, string>();
            try
            {
                StreamReader st = new StreamReader("settings.ini");
                while (!st.EndOfStream)
                {
                    try
                    {
                        string line = st.ReadLine();
                        if (line.IndexOf('#') > -1)
                            line = line.Substring(0, line.IndexOf('#'));
                        if (line.IndexOf(';') > -1)
                            line = line.Substring(0, line.IndexOf(';'));
                        if (line.IndexOf('=') > 0 && line.Trim().Length > 3)
                        {
                            string option = line.Substring(0, line.IndexOf('=')).Trim().ToLower();
                            string value = line.Substring(line.IndexOf('=') + 1);
                            foreach (string arg in args)
                                if (arg.ToLower().Trim().StartsWith(option))
                                    value = arg.Substring(arg.ToLower().IndexOf(option) + option.Length).Trim();
                            if (option.Trim().Length > 0 && value.Length > 0)
                                _settings.Add(option, value);
                        }
                    }
                    catch (Exception){}
                }
                st.Close();
            }
            catch (Exception){}
            return _settings;
        }

        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public byte[] Hash
            {
                get
                {
                    return MD5.Create().ComputeHash(System.Text.Encoding.ASCII.GetBytes(this.Password));
                }
            }
        }
    }
}
