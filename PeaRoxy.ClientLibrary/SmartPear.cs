// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartPear.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using PeaRoxy.CommonLibrary;

    /// <summary>
    ///     An object to control the SmartPear functionality and settings
    /// </summary>
    public class SmartPear
    {
        /// <summary>
        ///     The forwarder list updated delegate.
        /// </summary>
        /// <param name="rule">
        ///     The new rule.
        /// </param>
        /// <param name="isDirect">
        ///     A boolean indicating whether this rule is a direct rule
        /// </param>
        /// <param name="e">
        ///     The event arguments
        /// </param>
        public delegate void ForwarderListUpdatedDelegate(string rule, bool isDirect, EventArgs e);

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmartPear" /> class.
        /// </summary>
        public SmartPear()
        {
            this.ForwarderHttpList = new List<string>();
            this.ForwarderDirectList = new List<string>();
            this.DetectorHttpPattern = "^HTTP/1.1 403 Forbidden(\r\nConnection:close)*";
            this.DetectorDnsPoisoningPattern = "^(10.10.*.*)$";
            this.DetectorTimeout = 10;
            this.DetectorHttpResponseBufferTimeout = 10;
            this.DetectorTimeoutEnable = false;
            this.DetectorHttpCheckEnable = true;
            this.ForwarderTreatPort80AsHttp = false;
            this.DetectorHttpMaxBuffering = 50;
            this.ForwarderHttpEnable = true;
            this.ForwarderHttpsEnable = false;
            this.ForwarderSocksEnable = false;
            this.DetectorTreatPort80AsHttp = true;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether detector should treat direct connections to the port 80 as HTTP
        ///     connections.
        /// </summary>
        public bool DetectorTreatPort80AsHttp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector should detect the DNS poisoning.
        /// </summary>
        public bool DetectorDnsPoisoningEnable { get; set; }

        /// <summary>
        ///     Sets the detector DNS poisoning pattern.
        /// </summary>
        public string DetectorDnsPoisoningPattern
        {
            set
            {
                this.DetectorDnsPoisoningRegExPattern = Common.ToRegEx(value);
                this.DetectorDnsPoisoningRegEx = new Regex(
                    this.DetectorDnsPoisoningRegExPattern,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        ///     Gets the detector DNS poisoning regex.
        /// </summary>
        public Regex DetectorDnsPoisoningRegEx { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector should scan HTTP responses for blockage response
        /// </summary>
        public bool DetectorHttpCheckEnable { get; set; }

        /// <summary>
        ///     Gets or sets the detector's HTTP check max buffering size.
        /// </summary>
        public int DetectorHttpMaxBuffering { get; set; }

        /// <summary>
        ///     Sets the detector's HTTP check pattern.
        /// </summary>
        public string DetectorHttpPattern
        {
            set
            {
                this.DetectorHttpRegExPattern = Common.ToRegEx(value);
                this.DetectorHttpRegEx = new Regex(
                    this.DetectorHttpRegExPattern,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        ///     Gets the detector's HTTP check regex.
        /// </summary>
        public Regex DetectorHttpRegEx { get; private set; }

        private string DetectorHttpRegExPattern { get; set; }

        /// <summary>
        ///     Gets or sets the detector's HTTP check buffer timeout in seconds.
        /// </summary>
        public int DetectorHttpResponseBufferTimeout { get; set; }

        /// <summary>
        ///     Gets a value indicating whether DNS poisoning settings are valid and active
        /// </summary>
        public bool DetectorStatusDnsPoisoning
        {
            get
            {
                return this.DetectorDnsPoisoningEnable && this.DetectorDnsPoisoningRegExPattern.Trim() != string.Empty;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether HTTP check settings are valid and active
        /// </summary>
        public bool DetectorStatusHttp
        {
            get
            {
                return this.DetectorHttpCheckEnable && this.DetectorHttpRegExPattern.Trim() != string.Empty;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether timeout detection settings are valid and active
        /// </summary>
        public bool DetectorStatusTimeout
        {
            get
            {
                return this.DetectorTimeoutEnable && this.DetectorTimeout > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the timeout detection value in seconds.
        /// </summary>
        public int DetectorTimeout { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector should treat timeouts as a blockage sign
        /// </summary>
        public bool DetectorTimeoutEnable { get; set; }

        /// <summary>
        ///     Gets or sets the list of rules for forwarding direct connections
        /// </summary>
        public List<string> ForwarderDirectList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder should treat port 80 as HTTP.
        /// </summary>
        public bool ForwarderTreatPort80AsHttp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder is enable for HTTP requests
        /// </summary>
        public bool ForwarderHttpEnable { get; set; }

        /// <summary>
        ///     Gets or sets the list of rules for forwarding HTTP connections
        /// </summary>
        public List<string> ForwarderHttpList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder is enable for direct HTTPS requests
        /// </summary>
        public bool ForwarderHttpsEnable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder is enable for direct SOCKS requests
        /// </summary>
        public bool ForwarderSocksEnable { get; set; }

        private string DetectorDnsPoisoningRegExPattern { get; set; }

        /// <summary>
        ///     The forwarder list updated event.
        /// </summary>
        public event ForwarderListUpdatedDelegate ForwarderListUpdated;

        /// <summary>
        ///     A method to analyze and optimize the rules for direct connections
        /// </summary>
        /// <param name="list">
        ///     The list of rules
        /// </param>
        /// <returns>
        ///     The analyzed and optimized
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     of rules.
        /// </returns>
        public static IEnumerable<string> AnalyzeDirectList(List<string> list)
        {
            list = RemoveDuplicatesFromList(list);

            // Adding Global Port Rule
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                if (rule.IndexOf(":", StringComparison.Ordinal) > -1)
                {
                    rule = rule.Substring(0, rule.IndexOf(":", StringComparison.Ordinal)) + "*";
                    int pl = list.Count(t => Common.DoesMatchWildCard(MakeWildCardable(t), rule));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!Common.DoesMatchWildCard(MakeWildCardable(list[i]), rule))
                        {
                            continue;
                        }

                        list.RemoveAt(i);
                        i--;
                    }

                    i0 = -1;
                    list.Add(rule);
                }
            }

            // Adding upper level domain rule
            string[] secondUnAcceptableDomainNames =
                {
                    "aero", "asia", "biz", "cat", "com", "coop", "info", "int",
                    "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel",
                    "travel", "xxx"
                };
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                int portSeperator = (rule.IndexOf(":", StringComparison.Ordinal) == -1)
                                        ? rule.Length
                                        : rule.IndexOf(":", StringComparison.Ordinal);
                int startOf = (rule.IndexOf(" | ", StringComparison.Ordinal) == -1)
                                  ? 0
                                  : rule.IndexOf(" | ", StringComparison.Ordinal) + 3;
                string rulepTopDomainPart = rule.Substring(startOf, portSeperator - startOf);
                rulepTopDomainPart = rulepTopDomainPart.Trim(new[] { '*' });
                byte temporaryMicrosoftStupidProgrammersVariable;
                if (byte.TryParse(rulepTopDomainPart, out temporaryMicrosoftStupidProgrammersVariable))
                {
                    continue;
                }

                string ruleApp = rule.Substring(0, rule.IndexOf(" | ", StringComparison.Ordinal));
                rule = rule.Substring(rule.IndexOf(" | ", StringComparison.Ordinal) + 3);
                while (rule.IndexOf(".", StringComparison.Ordinal) > -1)
                {
                    rule = rule.Substring(rule.IndexOf(".", StringComparison.Ordinal) + 1);
                    if (rule.IndexOf(".", StringComparison.Ordinal) == -1)
                    {
                        break;
                    }

                    string rulepDomainPart = rule.Substring(0, rule.IndexOf(".", StringComparison.Ordinal));
                    if (rulepTopDomainPart.Length == 2 && secondUnAcceptableDomainNames.Contains(rulepDomainPart))
                    {
                        break;
                    }

                    rule = "*" + rule;
                    string ruleP = ruleApp + " | " + rule;
                    int pl = list.Count(t => Common.DoesMatchWildCard(MakeWildCardable(t), ruleP));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (Common.DoesMatchWildCard(MakeWildCardable(list[i]), ruleP))
                        {
                            list.RemoveAt(i);
                            i--;
                        }
                    }

                    i0 = -1;
                    list.Add(ruleP);
                }
            }

            return RemoveDuplicatesFromList(list);
        }

        /// <summary>
        ///     A method to analyze and optimize the rules for HTTP connections
        /// </summary>
        /// <param name="list">
        ///     The list of rules
        /// </param>
        /// <returns>
        ///     The analyzed and optimized
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     of rules.
        /// </returns>
        public static IEnumerable<string> AnalyzeHttpList(List<string> list)
        {
            list = RemoveDuplicatesFromList(list);
            string[] secondUnAcceptableDomainNames =
                {
                    "aero", "asia", "biz", "cat", "com", "coop", "info", "int",
                    "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel",
                    "travel", "xxx"
                };
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                int lastSlash = (rule.IndexOf("/", StringComparison.Ordinal) == -1)
                                    ? rule.Length
                                    : rule.IndexOf("/", StringComparison.Ordinal);
                int startOf = (rule.LastIndexOf(".", lastSlash, StringComparison.Ordinal) == -1)
                                  ? rule.Length
                                  : rule.LastIndexOf(
                                      ".",
                                      (rule.IndexOf("/", StringComparison.Ordinal) == -1)
                                          ? rule.Length - 1
                                          : rule.IndexOf("/", StringComparison.Ordinal),
                                      StringComparison.Ordinal) + 1;
                string rulepTopDomainPart = rule.Substring(startOf, lastSlash - startOf);
                rulepTopDomainPart = rulepTopDomainPart.Trim(new[] { '*' });
                byte temporaryMicrosoftStupidProgrammersVariable;
                if (byte.TryParse(rulepTopDomainPart, out temporaryMicrosoftStupidProgrammersVariable))
                {
                    break;
                }

                string ruleApp = rule.Substring(0, rule.IndexOf(" | ", StringComparison.Ordinal));
                rule = rule.Substring(rule.IndexOf(" | ", StringComparison.Ordinal) + 3);
                while (rule.IndexOf(".", StringComparison.Ordinal) > -1)
                {
                    rule = rule.Substring(rule.IndexOf(".", StringComparison.Ordinal) + 1);
                    if (rule.IndexOf(".", StringComparison.Ordinal) == -1
                        || (rule.IndexOf("/", StringComparison.Ordinal) > -1
                            && rule.IndexOf(".", StringComparison.Ordinal) > rule.IndexOf("/", StringComparison.Ordinal)))
                    {
                        break;
                    }

                    string rulepDomainPart = rule.Substring(0, rule.IndexOf(".", StringComparison.Ordinal));
                    if (rulepTopDomainPart.Length == 2 && secondUnAcceptableDomainNames.Contains(rulepDomainPart))
                    {
                        break;
                    }

                    rule = "*" + rule;
                    string ruleP = ruleApp + " | " + rule;
                    int pl = list.Count(t => Common.DoesMatchWildCard(MakeWildCardable(t), ruleP));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (Common.DoesMatchWildCard(MakeWildCardable(list[i]), ruleP))
                        {
                            list.RemoveAt(i);
                            i--;
                        }
                    }

                    i0 = -1;
                    list.Add(ruleP);
                }
            }

            return RemoveDuplicatesFromList(list);
        }

        /// <summary>
        ///     The method to add a rule to the list of active forwarder rules of direct connections
        /// </summary>
        /// <param name="rule">
        ///     The rule to add.
        /// </param>
        public void AddRuleToDirectForwarder(string rule)
        {
            if (rule.IndexOf(".", StringComparison.Ordinal) != -1)
            {
                if (this.ForwarderDirectList.Any(t => Common.DoesMatchWildCard(rule, t)))
                {
                    return;
                }

                if (this.ForwarderTreatPort80AsHttp && rule.IndexOf(":", StringComparison.Ordinal) != -1
                    && rule.Substring(rule.IndexOf(":", StringComparison.Ordinal) + 1) == "80")
                {
                    string rule2 = rule.Substring(0, rule.IndexOf(":", StringComparison.Ordinal));
                    if (this.ForwarderHttpList.Any(t => Common.DoesMatchWildCard(rule2, t)))
                    {
                        return;
                    }
                }

                lock (this.ForwarderDirectList) this.ForwarderDirectList.Add(rule);
                if (this.ForwarderListUpdated != null)
                {
                    this.ForwarderListUpdated(rule, true, new EventArgs());
                }
            }
        }

        /// <summary>
        ///     The method to add a rule to the list of active forwarder rules of HTTP connections
        /// </summary>
        /// <param name="rule">
        ///     The rule to add.
        /// </param>
        public void AddRuleToHttpForwarder(string rule)
        {
            if (rule.IndexOf(".", StringComparison.Ordinal) == -1)
            {
                return;
            }

            if (this.ForwarderHttpList.Any(t => Common.DoesMatchWildCard(rule, t)))
            {
                return;
            }

            lock (this.ForwarderHttpList) this.ForwarderHttpList.Add(rule);
            if (this.ForwarderListUpdated != null)
            {
                this.ForwarderListUpdated(rule, false, new EventArgs());
            }
        }

        private static string MakeWildCardable(string str)
        {
            return str.Replace("*", Path.GetRandomFileName().Replace(".", string.Empty));
        }

        private static List<string> RemoveDuplicatesFromList(List<string> list)
        {
            foreach (string rule in list)
            {
                if (list.Any(item => Common.DoesMatchWildCard(MakeWildCardable(rule), item)))
                {
                    continue;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (!Common.DoesMatchWildCard(MakeWildCardable(list[i]), rule))
                    {
                        continue;
                    }

                    list.RemoveAt(i);
                    i--;
                }
            }

            return list;
        }
    }
}