using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Accounting.Entities.External.Microsoft.SSO
{
    [XmlRoot(ElementName = "LogoutRequest", Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    public class SAML_LogoutRequest
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAML_LogoutRequest()
        {
            Version = "2.0";
            xmlns.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        }

        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public string IssueInstant { get; set; }

        [XmlAttribute("Destination")]
        public string Destination { get; set; }

        [XmlElement("Issuer", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public SAML_Issuer Issuer { get; set; }

        [XmlElement("NameID", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public SAMLLogout_NameID NameID { get; set; }

        [XmlAttribute(AttributeName = "samlp", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Samlp { get; set; }
    }


    [XmlRoot(ElementName = "NameID", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public class SAMLLogout_NameID
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAMLLogout_NameID()
        {
            xmlns.Add("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        }
        [XmlText]
        public string Text { get; set; }
    }
}
