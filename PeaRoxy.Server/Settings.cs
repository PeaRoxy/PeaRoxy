// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Screen.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using CommandLine;
    using CommandLine.Text;

    using PeaRoxy.CommonLibrary;

    public class Settings
    {
        private static Settings defaultObject;

        private int authMethod = -1;

        private byte compressionType;

        private byte encryptionType;

        private string httpForwardingIp;

        private int httpForwardingPort = -1;

        private bool? logErrors;

        private string logUsersUsageAddress;

        private int maxAcceptingClock;

        private int maxRoutingClock;

        private int noDataConnectionTimeOut = -1;

        private string peaRoxyDomain;

        private bool? pingMasterServer;

        private int receivePacketSize = -1;

        private int sendPacketSize = -1;

        private string serverIp;

        private int serverPort = -1;

        private int supportedCompressionTypes;

        private int supportedEncryptionTypes;

        private Settings()
        {
        }

        public IEnumerable<ConfigUser> AthorizedUsers
        {
            get
            {
                return ConfigReader.GetUsers(this.UsersFileAddress);
            }
        }

        public IEnumerable<string> BlackListedAddresses
        {
            get
            {
                return ConfigReader.GetBlackList(this.BlackListFileAddress);
            }
        }

        private Dictionary<string, string> Config
        {
            get
            {
                return ConfigReader.GetSettings(this.ConfigFileAddress);
            }
        }

        [Option("ip",
            HelpText =
                "Listening IP. *: All / 127.0.0.1: Current machine only / Interface IP: Only that interface / Default: *",
            MetaValue = "IP")]
        public string ServerIp
        {
            get
            {
                IPAddress ip;
                if (!string.IsNullOrEmpty(this.serverIp))
                {
                    if (this.serverIp == "*")
                    {
                        return IPAddress.Any.ToString();
                    }
                    return IPAddress.TryParse(this.serverIp, out ip) ? this.serverIp : IPAddress.Any.ToString();
                }
                if (!this.Config.ContainsKey("ServerIp") || string.IsNullOrEmpty(this.Config["ServerIp"])
                    || this.Config["ServerIp"] == "*")
                {
                    return IPAddress.Any.ToString();
                }
                return IPAddress.TryParse(this.Config["ServerIp"], out ip)
                           ? this.Config["ServerIp"]
                           : IPAddress.Any.ToString();
            }
            set
            {
                this.serverIp = value;
            }
        }

        [Option("port",
            HelpText =
                "Listening Port. From 0 to 65535 / Likely values: 1080 - 8080 - 8081 - 81 - 80 / Default: 1080 / Recommended: 80",
            MetaValue = "PORT")]
        public int ServerPort
        {
            get
            {
                if (this.serverPort != -1)
                {
                    return this.serverPort;
                }
                if (this.Config.ContainsKey("ServerPort") && !string.IsNullOrEmpty(this.Config["ServerPort"]))
                {
                    int i;
                    if (int.TryParse(this.Config["ServerPort"], out i))
                    {
                        return i;
                    }
                }
                return 1080;
            }
            set
            {
                this.serverPort = value;
            }
        }

        [Option("auth",
            HelpText =
                "Active Method for clients to authenticate, will use users.ini when user & pass mode selected. 0: No Authenticate (Open) / 1: User & Pass / Default: 1",
            MetaValue = "TYPE")]
        public int AuthMethod
        {
            get
            {
                if (this.authMethod != -1)
                {
                    return this.authMethod;
                }
                if (this.Config.ContainsKey("AuthMethod") && !string.IsNullOrEmpty(this.Config["AuthMethod"]))
                {
                    byte i;
                    if (byte.TryParse(this.Config["AuthMethod"], out i))
                    {
                        return i;
                    }
                }
                return 1;
            }
            set
            {
                this.authMethod = value;
            }
        }

        [Option("encryption",
            HelpText =
                "Server side encryption type, will apply to server stream only (Clients must set their own encryption settings). 1: No Encryption / 2: TripleDes Block Constant Key Encryption / 4: SimpleXor Self-Sync Encryption / Default: SimpleXor Self-Sync Encryption",
            MetaValue = "TYPE")]
        public Common.EncryptionTypes EncryptionType
        {
            get
            {
                if (this.encryptionType != 0)
                {
                    return (Common.EncryptionTypes)this.encryptionType;
                }
                if (this.Config.ContainsKey("EncryptionType") && !string.IsNullOrEmpty(this.Config["EncryptionType"]))
                {
                    byte i;
                    if (byte.TryParse(this.Config["EncryptionType"], out i))
                    {
                        Common.EncryptionTypes etype = (Common.EncryptionTypes)i;
                        if ((etype & (etype - 1)) == 0)
                        {
                            return etype;
                        }
                    }
                }
                return Common.EncryptionTypes.SimpleXor;
            }
            set
            {
                this.encryptionType = (byte)value;
                Common.EncryptionTypes etype = (Common.EncryptionTypes)this.encryptionType;
                if ((etype & (etype - 1)) != 0)
                {
                    throw new ArgumentException("Unknown value selected for EncryptionType.");
                }
            }
        }

        [Option("compression",
            HelpText =
                "Server side compression type, will apply to server stream only (Clients must set their own compression settings). 1: No Compression / 2: gZip / 4: Deflate / Default: No Compression",
            MetaValue = "FLAG")]
        public Common.CompressionTypes CompressionType
        {
            get
            {
                if (this.compressionType != 0)
                {
                    return (Common.CompressionTypes)this.compressionType;
                }
                if (this.Config.ContainsKey("CompressionType") && !string.IsNullOrEmpty(this.Config["CompressionType"]))
                {
                    byte i;
                    if (byte.TryParse(this.Config["CompressionType"], out i))
                    {
                        Common.CompressionTypes ctype = (Common.CompressionTypes)i;
                        if ((ctype & (ctype - 1)) == 0)
                        {
                            return ctype;
                        }
                    }
                }
                return Common.CompressionTypes.None;
            }
            set
            {
                this.compressionType = (byte)value;
                Common.CompressionTypes ctype = (Common.CompressionTypes)this.compressionType;
                if ((ctype & (ctype - 1)) != 0)
                {
                    throw new ArgumentException("Unknown value selected for CompressionType.");
                }
            }
        }

        [Option("supencryption",
            HelpText =
                "Acceptable encryptions, limit encryption types that client can use, if you had high load then it is recommended to limit users' options. 1: Only No Encryption / 2: Only TripleDes / 4: Only SimpleXor / Default: All",
            MetaValue = "FLAG")]
        public Common.EncryptionTypes SupportedEncryptionTypes
        {
            get
            {
                if (this.supportedEncryptionTypes != 0)
                {
                    return (Common.EncryptionTypes)this.supportedEncryptionTypes;
                }
                if (this.Config.ContainsKey("SupportedEncryptionTypes")
                    && !string.IsNullOrEmpty(this.Config["SupportedEncryptionTypes"]))
                {
                    int i;
                    if (int.TryParse(this.Config["SupportedEncryptionTypes"], out i))
                    {
                        return (Common.EncryptionTypes)i;
                    }
                }
                return Common.EncryptionTypes.AllDefaults;
            }
            set
            {
                this.supportedEncryptionTypes = (int)value;
            }
        }

        [Option("supcompression",
            HelpText =
                "Acceptable compressions, limit compression types that client can use, if you had high load then it is recommended to limit users' options. 1: Only No Compressions / 2: Only gZip / 4: Only Deflate / Default: All",
            MetaValue = "FLAG")]
        public Common.CompressionTypes SupportedCompressionTypes
        {
            get
            {
                if (this.supportedCompressionTypes != 0)
                {
                    return (Common.CompressionTypes)this.supportedCompressionTypes;
                }
                if (this.Config.ContainsKey("SupportedCompressionTypes")
                    && !string.IsNullOrEmpty(this.Config["SupportedCompressionTypes"]))
                {
                    int i;
                    if (int.TryParse(this.Config["SupportedCompressionTypes"], out i))
                    {
                        return Common.CompressionTypes.AllDefaults;
                    }
                }
                return (Common.CompressionTypes)7;
            }
            set
            {
                this.supportedCompressionTypes = (int)value;
            }
        }

        [Option("sndpktsize",
            HelpText = "Max sending packet size, apply to each connection separately. Default: 1024 (1 KB)",
            MetaValue = "BYTES")]
        public int SendPacketSize
        {
            get
            {
                if (this.sendPacketSize != -1)
                {
                    return this.sendPacketSize;
                }
                if (this.Config.ContainsKey("SendPacketSize") && !string.IsNullOrEmpty(this.Config["SendPacketSize"]))
                {
                    int i;
                    if (int.TryParse(this.Config["SendPacketSize"], out i))
                    {
                        return i;
                    }
                }
                return 1024;
            }
            set
            {
                this.sendPacketSize = value;
            }
        }

        [Option("rcvpktsize",
            HelpText = "Max receiving packet size, apply to each connection separately. Default: 8192 (8 KB)",
            MetaValue = "BYTES")]
        public int ReceivePacketSize
        {
            get
            {
                if (this.receivePacketSize != -1)
                {
                    return this.receivePacketSize;
                }
                if (this.Config.ContainsKey("ReceivePacketSize")
                    && !string.IsNullOrEmpty(this.Config["ReceivePacketSize"]))
                {
                    int i;
                    if (int.TryParse(this.Config["ReceivePacketSize"], out i))
                    {
                        return i;
                    }
                }
                return 8192;
            }
            set
            {
                this.receivePacketSize = value;
            }
        }

        [Option("timeout",
            HelpText =
                "Maximum time to wait for a connection with no data transmission to time out. Default: 6000 (100 Min)",
            MetaValue = "SECOND")]
        public int NoDataConnectionTimeOut
        {
            get
            {
                if (this.noDataConnectionTimeOut != -1)
                {
                    return this.noDataConnectionTimeOut;
                }
                if (this.Config.ContainsKey("NoDataConnectionTimeOut")
                    && !string.IsNullOrEmpty(this.Config["NoDataConnectionTimeOut"]))
                {
                    int i;
                    if (int.TryParse(this.Config["NoDataConnectionTimeOut"], out i))
                    {
                        return i;
                    }
                }
                return 6000;
            }
            set
            {
                this.noDataConnectionTimeOut = value;
            }
        }

        [Option("fwdaddress",
            HelpText =
                "In case you want to use an IP:Port combination for more than one application, like using it for both PeaRoxy and Apache, you can use this value to set the second application's listening IP address. localhost or 127.0.0.1: Current machine /  Another IP or Domain Name / Default: localhost",
            MetaValue = "IP/DOMAIN")]
        public string HttpForwardingIp
        {
            get
            {
                if (!string.IsNullOrEmpty(this.httpForwardingIp))
                {
                    return this.httpForwardingIp;
                }
                if (this.Config.ContainsKey("HttpForwardingIp")
                    && !string.IsNullOrEmpty(this.Config["HttpForwardingIp"]))
                {
                    return this.Config["HttpForwardingIp"];
                }
                return IPAddress.Loopback.ToString();
            }
            set
            {
                this.httpForwardingIp = value;
            }
        }

        [Option("fwdport",
            HelpText =
                "In case you want to use an IP:Port combination for more than one application, like using it for both PeaRoxy and Apache, you can use this value to set the second application's listening port number. From 0 to 65535 / 0: Disable / Default: 0",
            MetaValue = "PORT")]
        public int HttpForwardingPort
        {
            get
            {
                if (this.httpForwardingPort != -1)
                {
                    return this.httpForwardingPort;
                }
                if (this.Config.ContainsKey("HttpForwardingPort")
                    && !string.IsNullOrEmpty(this.Config["HttpForwardingPort"]))
                {
                    int i;
                    if (int.TryParse(this.Config["HttpForwardingPort"], out i))
                    {
                        return i;
                    }
                }
                return 0;
            }
            set
            {
                this.httpForwardingPort = value;
            }
        }

        [Option("domain",
            HelpText =
                "When using an IP:Port combination for more than one application, or in other word when you have Forwarding feature available, you should set this variable to an unique string so clients can connect to the PeaRoxy server. A Domain Name (pearoxy.example.com) or Identifier / Default: (Empty)",
            MetaValue = "DOMAIN/ID")]
        public string PeaRoxyDomain
        {
            get
            {
                if (this.peaRoxyDomain != null)
                {
                    return this.peaRoxyDomain;
                }
                if (this.Config.ContainsKey("PeaRoxyDomain"))
                {
                    return this.Config["PeaRoxyDomain"];
                }
                return string.Empty;
            }
            set
            {
                this.peaRoxyDomain = value;
            }
        }

        [Option("logenable", HelpText = "Create error log file. Default: True", MetaValue = "TRUE/FALSE")]
        public bool? LogErrors
        {
            get
            {
                if (this.logErrors != null)
                {
                    return this.logErrors;
                }
                if (this.Config.ContainsKey("LogErrors") && !string.IsNullOrEmpty(this.Config["LogErrors"]))
                {
                    bool b;
                    if (bool.TryParse(this.Config["LogErrors"], out b))
                    {
                        return b;
                    }
                }
                return true;
            }
            set
            {
                this.logErrors = value;
            }
        }

        [Option("ping", HelpText = "Will notify master server when server starts. Default: True",
            MetaValue = "TRUE/FALSE")]
        public bool? PingMasterServer
        {
            get
            {
                if (this.pingMasterServer != null)
                {
                    return this.pingMasterServer;
                }
                if (this.Config.ContainsKey("PingMasterServer")
                    && !string.IsNullOrEmpty(this.Config["PingMasterServer"]))
                {
                    bool b;
                    if (bool.TryParse(this.Config["PingMasterServer"], out b))
                    {
                        return b;
                    }
                }
                return true;
            }
            set
            {
                this.pingMasterServer = value;
            }
        }

        [Option("maxrouting", HelpText = "Max Routing Clock (Per Second). From 1 to 1000 / Default: 1000 (0.001 Sec)",
            MetaValue = "COUNT")]
        public int MaxRoutingClock
        {
            get
            {
                if (this.maxRoutingClock > 0)
                {
                    return this.maxRoutingClock;
                }
                if (this.Config.ContainsKey("MaxRoutingClock") && !string.IsNullOrEmpty(this.Config["MaxRoutingClock"]))
                {
                    int i;
                    if (int.TryParse(this.Config["MaxRoutingClock"], out i))
                    {
                        return i;
                    }
                }
                return 1000;
            }
            set
            {
                this.maxRoutingClock = value;
            }
        }

        [Option("maxaccepting", HelpText = "Max Accepting Clock (Per Second). From 1 to 1000 / Default: 100 (0.01 Sec)",
            MetaValue = "COUNT")]
        public int MaxAcceptingClock
        {
            get
            {
                if (this.maxAcceptingClock > 0)
                {
                    return this.maxAcceptingClock;
                }
                if (this.Config.ContainsKey("MaxAcceptingClock")
                    && !string.IsNullOrEmpty(this.Config["MaxAcceptingClock"]))
                {
                    int i;
                    if (int.TryParse(this.Config["MaxAcceptingClock"], out i))
                    {
                        return i;
                    }
                }
                return 100;
            }
            set
            {
                this.maxAcceptingClock = value;
            }
        }

        [Option("usagelog",
            HelpText =
                "Will create log file for users logged in per each running process and save information about bandwidth usage. (Empty): Disable / Default: . (Current Folder)",
            MetaValue = "ADDRESS")]
        public string UsersUsageLogAddress
        {
            get
            {
                if (this.logUsersUsageAddress != null)
                {
                    return this.logUsersUsageAddress;
                }
                if (this.Config.ContainsKey("UsersUsageLogAddress"))
                {
                    return this.Config["UsersUsageLogAddress"];
                }
                return ".";
            }
            set
            {
                this.logUsersUsageAddress = value;
            }
        }

        [Option('l', DefaultValue = false, HelpText = "Print a list of all active connections.",
            MetaValue = "TRUE/FALSE")]
        public bool ShowConnections { get; set; }

        [Option('c', DefaultValue = "settings.ini",
            HelpText = "Specify a configuration file to read the settings from it.", MetaValue = "FILENAME")]
        public string ConfigFileAddress { get; set; }

        [Option('u', DefaultValue = "users.ini",
            HelpText = "Specify a file to read the user names and passwords for authorized users from it.",
            MetaValue = "FILENAME")]
        public string UsersFileAddress { get; set; }

        [Option('b', DefaultValue = "blacklist.ini",
            HelpText =
                "Specify a file to read the IP addresses or domains that we should reject connections from and to them.",
            MetaValue = "FILENAME")]
        public string BlackListFileAddress { get; set; }

        public static Settings Default
        {
            get
            {
                if (defaultObject == null)
                {
                    defaultObject = new Settings();
                    Parser.Default.ParseArguments(Environment.GetCommandLineArgs().Skip(1).ToArray(), defaultObject);
                    if (defaultObject.LastParserState != null && defaultObject.LastParserState.Errors.Count > 0)
                    {
                        throw new Exception(defaultObject.LastParserState.Errors[0].ToString());
                    }
                }
                return defaultObject;
            }
        }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        // ReSharper disable once UnusedMember.Global
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}