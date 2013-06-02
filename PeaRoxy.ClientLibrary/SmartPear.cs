using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.ClientLibrary
{
    public class SmartPear
    {
        public delegate void ForwarderListUpdatedDelegate(string rule, bool https, EventArgs e);
        public event ForwarderListUpdatedDelegate Forwarder_ListUpdated;

        public int Detector_HTTP_MaxBuffering { get; set; }
        public int Detector_HTTP_ResponseBufferTimeout { get; set; }
        public int Detector_Timeout { get; set; }
        public bool Detector_HTTP_Enable { get; set; }
        public bool Detector_Direct_Port80AsHTTP { get; set; }
        public bool Detector_DNSGrabber_Enable { get; set; }
        public bool Detector_Timeout_Enable { get; set; }
        public Regex Detector_HTTP_RegEX { get; private set; }
        public Regex Detector_DNSGrabber_RegEX { get; private set; }
        public string Detector_HTTP_RegEXPattern { get; private set; }
        public string Detector_DNSGrabber_RegEXPattern { get; private set; }
        public string Detector_HTTP_Pattern
        {
            get
            {
                return Common.FromRegEX(Detector_HTTP_RegEXPattern);
            }
            set
            {
                Detector_HTTP_RegEXPattern = Common.ToRegEX(value);
                Detector_HTTP_RegEX = new Regex(Detector_HTTP_RegEXPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }
        public string Detector_DNSGrabber_Pattern
        {
            get
            {
                return Common.FromRegEX(Detector_DNSGrabber_RegEXPattern);
            }
            set
            {
                Detector_DNSGrabber_RegEXPattern = Common.ToRegEX(value);
                Detector_DNSGrabber_RegEX = new Regex(Detector_DNSGrabber_RegEXPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        public bool Forwarder_HTTP_Enable { get; set; }
        public bool Forwarder_HTTPS_Enable { get; set; }
        public bool Forwarder_SOCKS_Enable { get; set; }
        public bool Forwarder_Direct_Port80AsHTTP { get; set; }
        public List<string> Forwarder_HTTP_List { get; set; }
        public List<string> Forwarder_Direct_List { get; set; }

        public bool DetectorStatus_HTTP
        {
            get
            {
                return (Detector_HTTP_Enable && Detector_HTTP_RegEXPattern.Trim() != string.Empty);
            }
        }
        public bool DetectorStatus_Timeout
        {
            get
            {
                return (Detector_Timeout_Enable && Detector_Timeout > 0);
            }
        }
        public bool DetectorStatus_DNSGrabber
        {
            get
            {
                return (Detector_DNSGrabber_Enable && Detector_DNSGrabber_RegEXPattern.Trim() != string.Empty);
            }
        }
        public SmartPear()
        {
            this.Forwarder_HTTP_List = new List<string>();
            this.Forwarder_Direct_List = new List<string>();
            this.Detector_HTTP_Pattern = "^HTTP/1.1 403 Forbidden(\r\nConnection:close)*";
            Detector_DNSGrabber_Pattern = "^(10.10.*.*)$";
            this.Detector_Timeout = 10;
            this.Detector_HTTP_ResponseBufferTimeout = 10;
            this.Detector_Timeout_Enable = false;
            this.Detector_HTTP_Enable = true;
            this.Forwarder_Direct_Port80AsHTTP = false;
            this.Detector_HTTP_MaxBuffering = 50;
            this.Forwarder_HTTP_Enable = true;
            this.Forwarder_HTTPS_Enable = false;
            this.Forwarder_SOCKS_Enable = false;
            this.Detector_Direct_Port80AsHTTP = true;
        }
        public void AddRuleTo_HTTP_Forwarder(string rule)
        {
            if (rule.IndexOf(".") != -1)
            {
                for (int i = 0; i < Forwarder_HTTP_List.Count; i++)
                    if (Common.IsMatchWildCard(rule, Forwarder_HTTP_List[i]))
                        return;
                lock (Forwarder_HTTP_List)
                    this.Forwarder_HTTP_List.Add(rule);
                if (this.Forwarder_ListUpdated != null)
                    this.Forwarder_ListUpdated(rule, false, new EventArgs());
            }
        }
        public void AddRuleTo_Direct_Forwarder(string rule)
        {
            if (rule.IndexOf(".") != -1)
            {
                for (int i = 0; i < Forwarder_Direct_List.Count; i++)
                    if (Common.IsMatchWildCard(rule, Forwarder_Direct_List[i]))
                        return;
                if (Forwarder_Direct_Port80AsHTTP && rule.IndexOf(":") != -1 && rule.Substring(rule.IndexOf(":") + 1) == "80")
                {
                    string rule2 = rule.Substring(0, rule.IndexOf(":"));
                    for (int i = 0; i < Forwarder_HTTP_List.Count; i++)
                        if (Common.IsMatchWildCard(rule2, Forwarder_HTTP_List[i]))
                            return;
                }
                lock (Forwarder_Direct_List)
                    this.Forwarder_Direct_List.Add(rule);
                if (this.Forwarder_ListUpdated != null)
                    this.Forwarder_ListUpdated(rule, true, new EventArgs());
            }
        }
        public static List<string> Analyse_Direct_List(List<string> list)
        {
            list = RemoveDeplicatsFromList(list);
            // Adding Global Port Rule
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                if (rule.IndexOf(":") > -1)
                {
                    rule = rule.Substring(0, rule.IndexOf(":")) + "*";
                    Regex curRegp = new Regex(Common.ToRegEX(rule));
                    int pl = 0;
                    for (int i = 0; i < list.Count; i++)
                        if (Common.IsMatchWildCard(MakeWildCardable(list[i]), rule))
                            pl++;
                    if (pl >= 3)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (Common.IsMatchWildCard(MakeWildCardable(list[i]), rule))
                            {
                                list.RemoveAt(i);
                                i--;
                            }
                        i0 = -1;
                        list.Add(rule);
                    }
                }
            }
            // Adding uper level domain rule
            string[] SecondUnAcceptableDomainNames = new string[] { "aero", "asia", "biz", "cat", "com", "coop", "info", "int", "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel", "travel", "xxx" };
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                int portSeperator = (rule.IndexOf(":") == -1) ? rule.Length : rule.IndexOf(":");
                int startOf = (rule.IndexOf(" | ") == -1) ? 0 : rule.IndexOf(" | ") + 3;
                string rulepTopDomainPart = rule.Substring(startOf, portSeperator - startOf);
                rulepTopDomainPart = rulepTopDomainPart.Trim(new char[] { '*' });
                byte Temporary_Microsoft_Stupid_Programmers_Variable;
                if (byte.TryParse(rulepTopDomainPart, out Temporary_Microsoft_Stupid_Programmers_Variable))
                    continue;
                string ruleApp = rule.Substring(0, rule.IndexOf(" | "));
                rule = rule.Substring(rule.IndexOf(" | ") + 3);
                while (rule.IndexOf(".") > -1)
                {
                    rule = rule.Substring(rule.IndexOf(".") + 1);
                    if (rule.IndexOf(".") == -1)
                        break;
                    string rulepDomainPart = rule.Substring(0, rule.IndexOf("."));
                    if (rulepTopDomainPart.Length == 2 && SecondUnAcceptableDomainNames.Contains(rulepDomainPart))
                        break;

                    rule = "*" + rule;
                    string ruleP = ruleApp + " | " + rule;
                    int pl = 0;
                    for (int i = 0; i < list.Count; i++)
                        if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
                            pl++;
                    if (pl >= 3)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
                            {
                                list.RemoveAt(i);
                                i--;
                            }
                        i0 = -1;
                        list.Add(ruleP);
                    }
                }
            }
            return RemoveDeplicatsFromList(list);
        }
        public static List<string> Analyse_HTTP_List(List<string> list)
        {
            list = RemoveDeplicatsFromList(list);
            string[] SecondUnAcceptableDomainNames = new string[] { "aero", "asia", "biz", "cat", "com", "coop", "info", "int", "jobs", "mobi", "museum", "name", "net", "org", "pro", "tel", "travel", "xxx" };
            for (int i0 = 0; i0 < list.Count; i0++)
            {
                string rule = list[i0];
                int lastSlash = ((rule.IndexOf("/") == -1) ? rule.Length : rule.IndexOf("/"));
                int startOf = ((rule.LastIndexOf(".", lastSlash) == -1) ? rule.Length : rule.LastIndexOf(".", ((rule.IndexOf("/") == -1) ? rule.Length - 1 : rule.IndexOf("/"))) + 1);
                string rulepTopDomainPart = rule.Substring(startOf, lastSlash - startOf);
                rulepTopDomainPart = rulepTopDomainPart.Trim(new char[] { '*' });
                byte Temporary_Microsoft_Stupid_Programmers_Variable;
                if (byte.TryParse(rulepTopDomainPart, out Temporary_Microsoft_Stupid_Programmers_Variable))
                    break;
                string ruleApp = rule.Substring(0, rule.IndexOf(" | "));
                rule = rule.Substring(rule.IndexOf(" | ") + 3);
                while (rule.IndexOf(".") > -1)
                {
                    rule = rule.Substring(rule.IndexOf(".") + 1);
                    if (rule.IndexOf(".") == -1 || (rule.IndexOf("/")>-1 && rule.IndexOf(".")>rule.IndexOf("/")))
                        break;
                    string rulepDomainPart = rule.Substring(0,rule.IndexOf("."));
                    if (rulepTopDomainPart.Length == 2 && SecondUnAcceptableDomainNames.Contains(rulepDomainPart))
                        break;

                    rule = "*" + rule;
                    string ruleP = ruleApp + " | " + rule;
                    int pl = 0;
                    for (int i = 0; i < list.Count; i++)
                        if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
                            pl++;
                    if (pl >= 3)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (Common.IsMatchWildCard(MakeWildCardable(list[i]), ruleP))
                            {
                                list.RemoveAt(i);
                                i--;
                            }
                        i0 = -1;
                        list.Add(ruleP);
                    }
                }
            }
            return RemoveDeplicatsFromList(list);
        }
        private static List<string> RemoveDeplicatsFromList(List<string> list)
        {
            foreach (string rule in list)
            {
                for (int i = 0; i < list.Count; i++)
                    if (Common.IsMatchWildCard(MakeWildCardable(rule), list[i]))
                        goto NextRule;
                for (int i = 0; i < list.Count; i++)
                    if (Common.IsMatchWildCard(MakeWildCardable(list[i]), rule))
                    {
                        list.RemoveAt(i);
                        i--;
                    }
            NextRule: ;
            }
            return list;
        }
        private static string MakeWildCardable(string str)
        {
            return str.Replace("*", System.IO.Path.GetRandomFileName().Replace(".", string.Empty));
        }
    }
}
