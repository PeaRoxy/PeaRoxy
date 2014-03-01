// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLineOptions.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Linq;

    using CommandLine;
    using CommandLine.Text;

    #endregion

    internal class CommandLineOptions
    {
        #region Static Fields

        private static CommandLineOptions defaultObject;

        #endregion

        #region Constructors and Destructors

        private CommandLineOptions()
        {
        }

        #endregion

        #region Public Properties

        public static CommandLineOptions Default
        {
            get
            {
                if (defaultObject == null)
                {
                    defaultObject = new CommandLineOptions();
                    Parser.Default.ParseArguments(Environment.GetCommandLineArgs().Skip(1).ToArray(), defaultObject);
                    Console.WriteLine(string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
                    if (defaultObject.LastParserState != null && defaultObject.LastParserState.Errors.Count > 0)
                    {
                        throw new Exception(defaultObject.LastParserState.Errors[0].ToString());
                    }
                }
                return defaultObject;
            }
        }

        [Option('a', "apps", Required = true, HelpText = "Applications to force.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Apps { get; set; }

        [Option('l', "resolverlevel", HelpText = "Last digit of dummy host resolver ip addresses", DefaultValue = (byte)240)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte DummyIpResolverSupLevel { get; set; }

        [Option('i', "irp", HelpText = "A pattern for recognizing invalid IPv4 addresses returned by DNS resolver",
            DefaultValue = "")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string InvalidResolverPattern { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsDebug { get; set; }

        [ParserState]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IParserState LastParserState { get; set; }

        [Option('p', "proxy", Required = true, HelpText = "Fast proxy IP address and port")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Proxy { get; set; }

        #endregion

        #region Public Methods and Operators

        [HelpOption]
        // ReSharper disable once UnusedMember.Global
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        #endregion
    }
}