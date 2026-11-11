using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace Accounting.Utilities.Internal
{
    public class XMLUtility
    {
        public static string Serialize(System.Type pObjectType, Object pTargetObject)
        {
            StringWriter output = new StringWriter(new StringBuilder());
            XmlSerializer xmlSer = new XmlSerializer(pObjectType);
            xmlSer.Serialize(output, pTargetObject);
            StringBuilder bldrContent = output.GetStringBuilder();
            bldrContent.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
            bldrContent.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
            bldrContent.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            return bldrContent.ToString().Trim();
        }
        public static object DeSerialize(System.Type pObjectType, string pSourceValue)
        {
            StringReader output = new StringReader(pSourceValue);
            XmlSerializer xmlSer = new XmlSerializer(pObjectType);

            return xmlSer.Deserialize(output);
        }
        internal static bool IsValidXML(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }

}
