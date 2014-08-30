// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandLine.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    using System;
    using System.Linq;

    using global::CommandLine;
    using global::CommandLine.Text;

    public class CommandLine
    {
        private static CommandLine defaultObject;

        private CommandLine()
        {
        }

        [Option("quit", HelpText = "End the program and send a Quit message to all open instances.")]
        public bool Quit { get; set; }

        [Option("autorun", HelpText = "Indicates if this is a AutoRun execution.")]
        public bool AutoRun { get; set; }

        public static CommandLine Default
        {
            get
            {
                return defaultObject
                       ?? (defaultObject = GetByArguments(string.Join(" ", Environment.GetCommandLineArgs().Skip(1))));
            }
        }

        public static CommandLine GetByArguments(string arguments)
        {
            CommandLine commandObject = new CommandLine();
            Parser.Default.ParseArguments(arguments.Split(' '), commandObject);
            if (commandObject.LastParserState != null && commandObject.LastParserState.Errors.Count > 0)
            {
                throw new Exception(commandObject.LastParserState.Errors[0].ToString());
            }
            return commandObject;
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