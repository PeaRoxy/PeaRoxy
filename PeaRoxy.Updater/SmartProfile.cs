// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmartProfile.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Updater
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    ///     The SmartProfile class is representation of a SmartPear settings profile.
    /// </summary>
    [Serializable]
    public class SmartProfile
    {
        public string AntiBlockPageRule { get; set; }

        public string AntiDnsPoisoningRule { get; set; }

        public List<string> DirectRules { get; set; }

        public int DirectTimeout { get; set; }

        public bool DnsPoisoningDetection { get; set; }

        public bool HttpBlockPageDetection { get; set; }

        public List<string> HttpRules { get; set; }

        public bool IsHttpSupported { get; set; }

        public bool IsHttpsSupported { get; set; }

        public bool IsSocksSupported { get; set; }

        public bool TimeoutDetection { get; set; }

        public bool TreatPort80AsHttp { get; set; }

        public static SmartProfile FromXml(string xmltext)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(SmartProfile));
                using (StringReader reader = new StringReader(xmltext))
                {
                    return ser.Deserialize(reader) as SmartProfile;
                }
            }
            catch
            {
            }
            return null;
        }

        public string ToXml()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(SmartProfile));
                using (StringWriter writer = new StringWriter())
                {
                    ser.Serialize(writer, this);
                    return writer.ToString();
                }
            }
            catch
            {
            }
            return null;
        }
    }
}