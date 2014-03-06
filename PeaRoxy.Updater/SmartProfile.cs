namespace PeaRoxy.Updater
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;

    #endregion

    [Serializable]
    public class SmartProfile
    {
        #region Public Properties

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

        #endregion

        #region Public Methods and Operators

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

        #endregion
    }
}