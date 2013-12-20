// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartPear.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The smart pear.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    ///     The smart pear.
    /// </summary>
    public class SmartPear
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmartPear" /> class.
        /// </summary>
        public SmartPear()
        {
            this.ForwarderHttpList = new List<string>();
            this.ForwarderDirectList = new List<string>();
            this.DetectorHttpPattern = "^HTTP/1.1 403 Forbidden(\r\nConnection:close)*";
            this.DetectorDnsGrabberPattern = "^(10.10.*.*)$";
            this.DetectorTimeout = 10;
            this.DetectorHttpResponseBufferTimeout = 10;
            this.DetectorTimeoutEnable = false;
            this.DetectorHttpEnable = true;
            this.ForwarderDirectPort80AsHttp = false;
            this.DetectorHttpMaxBuffering = 50;
            this.ForwarderHttpEnable = true;
            this.ForwarderHttpsEnable = false;
            this.ForwarderSocksEnable = false;
            this.DetectorDirectPort80AsHttp = true;
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The forwarder list updated delegate.
        /// </summary>
        /// <param name="rule">
        ///     The rule.
        /// </param>
        /// <param name="https">
        ///     The https.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        public delegate void ForwarderListUpdatedDelegate(string rule, bool https, EventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        ///     The forwarder_ list updated.
        /// </summary>
        public event ForwarderListUpdatedDelegate ForwarderListUpdated;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether detector direct port 80 as http.
        /// </summary>
        public bool DetectorDirectPort80AsHttp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector DNS grabber enable.
        /// </summary>
        public bool DetectorDnsGrabberEnable { get; set; }

        /// <summary>
        ///     Gets or sets the detector DNS grabber_ pattern.
        /// </summary>
        public string DetectorDnsGrabberPattern
        {
            get
            {
                return Common.FromRegEx(this.DetectorDnsGrabberRegExPattern);
            }

            set
            {
                this.DetectorDnsGrabberRegExPattern = Common.ToRegEx(value);
                this.DetectorDnsGrabberRegEx = new Regex(
                    this.DetectorDnsGrabberRegExPattern, 
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        ///     Gets the detector DNS grabber detector.
        /// </summary>
        public Regex DetectorDnsGrabberRegEx { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector http enable.
        /// </summary>
        public bool DetectorHttpEnable { get; set; }

        /// <summary>
        ///     Gets or sets the detector http max buffering.
        /// </summary>
        public int DetectorHttpMaxBuffering { get; set; }

        /// <summary>
        ///     Gets or sets the detector http pattern.
        /// </summary>
        public string DetectorHttpPattern
        {
            get
            {
                return Common.FromRegEx(this.DetectorHttpRegExPattern);
            }

            set
            {
                this.DetectorHttpRegExPattern = Common.ToRegEx(value);
                this.DetectorHttpRegEx = new Regex(
                    this.DetectorHttpRegExPattern, 
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        ///     Gets the http detector.
        /// </summary>
        public Regex DetectorHttpRegEx { get; private set; }

        /// <summary>
        ///     Gets the http detector pattern.
        /// </summary>
        public string DetectorHttpRegExPattern { get; private set; }

        /// <summary>
        ///     Gets or sets the detector http response buffer timeout.
        /// </summary>
        public int DetectorHttpResponseBufferTimeout { get; set; }

        /// <summary>
        ///     Gets a value indicating whether detector status DNS grabber.
        /// </summary>
        public bool DetectorStatusDnsGrabber
        {
            get
            {
                return this.DetectorDnsGrabberEnable && this.DetectorDnsGrabberRegExPattern.Trim() != string.Empty;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether detector status_ http.
        /// </summary>
        public bool DetectorStatusHttp
        {
            get
            {
                return this.DetectorHttpEnable && this.DetectorHttpRegExPattern.Trim() != string.Empty;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether detector status_ timeout.
        /// </summary>
        public bool DetectorStatusTimeout
        {
            get
            {
                return this.DetectorTimeoutEnable && this.DetectorTimeout > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the detector_ timeout.
        /// </summary>
        public int DetectorTimeout { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether detector timeout enable.
        /// </summary>
        public bool DetectorTimeoutEnable { get; set; }

        /// <summary>
        ///     Gets or sets the forwarder direct list.
        /// </summary>
        public List<string> ForwarderDirectList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder direct port 80 as http.
        /// </summary>
        public bool ForwarderDirectPort80AsHttp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder http enable.
        /// </summary>
        public bool ForwarderHttpEnable { get; set; }

        /// <summary>
        ///     Gets or sets the forwarder http list.
        /// </summary>
        public List<string> ForwarderHttpList { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder https enable.
        /// </summary>
        public bool ForwarderHttpsEnable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether forwarder socks enable.
        /// </summary>
        public bool ForwarderSocksEnable { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the detector DNS grabber pattern.
        /// </summary>
        private string DetectorDnsGrabberRegExPattern { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The analyze direct_ list.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
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
                    int pl = list.Count(t => Common.IsMatchWildCard(MakeWildCardable(t), rule));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!Common.IsMatchWildCard(MakeWildCardable(list[i]), rule))
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

            // Adding uper level domain rule
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
                    int pl = list.Count(t => Common.IsMatchWildCard(MakeWildCardable(t), ruleP));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
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
        /// The analyze http list.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
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
                                      (rule.IndexOf("/", StringComparison.Ordinal) == -1) ? rule.Length - 1 : rule.IndexOf("/", StringComparison.Ordinal),
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
                    int pl = list.Count(t => Common.IsMatchWildCard(MakeWildCardable(t), ruleP));

                    if (pl < 3)
                    {
                        continue;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
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
        /// The add rule to direct forwarder.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        public void AddRuleToDirectForwarder(string rule)
        {
            if (rule.IndexOf(".", StringComparison.Ordinal) != -1)
            {
                if (this.ForwarderDirectList.Any(t => Common.IsMatchWildCard(rule, t)))
                {
                    return;
                }

                if (this.ForwarderDirectPort80AsHttp && rule.IndexOf(":", StringComparison.Ordinal) != -1
                    && rule.Substring(rule.IndexOf(":", StringComparison.Ordinal) + 1) == "80")
                {
                    string rule2 = rule.Substring(0, rule.IndexOf(":", StringComparison.Ordinal));
                    if (this.ForwarderHttpList.Any(t => Common.IsMatchWildCard(rule2, t)))
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
        /// The add rule to http forwarder.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        public void AddRuleToHttpForwarder(string rule)
        {
            if (rule.IndexOf(".", StringComparison.Ordinal) == -1)
            {
                return;
            }

            if (this.ForwarderHttpList.Any(t => Common.IsMatchWildCard(rule, t)))
            {
                return;
            }

            lock (this.ForwarderHttpList) this.ForwarderHttpList.Add(rule);
            if (this.ForwarderListUpdated != null)
            {
                this.ForwarderListUpdated(rule, false, new EventArgs());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The make wild card test string
        /// </summary>
        /// <param name="str">
        /// The wild card string
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string MakeWildCardable(string str)
        {
            return str.Replace("*", Path.GetRandomFileName().Replace(".", string.Empty));
        }

        /// <summary>
        /// The remove duplicates from list.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static List<string> RemoveDuplicatesFromList(List<string> list)
        {
            foreach (string rule in list)
            {
                if (list.Any(item => Common.IsMatchWildCard(MakeWildCardable(rule), item)))
                {
                    continue;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (!Common.IsMatchWildCard(MakeWildCardable(list[i]), rule))
                    {
                        continue;
                    }

                    list.RemoveAt(i);
                    i--;
                }
            }

            return list;
        }

        #endregion
    }
}