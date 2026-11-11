using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Accounting.Entities.External.Microsoft.SSO
{
    public class SAML_AuthnRequest
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAML_AuthnRequest()
        {
            Version = "2.0";
            NameIDPolicy = new SAML_NameIDPolicy { AllowCreate = true };
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

        [XmlAttribute("AssertionConsumerServiceURL")]
        public string AssertionConsumerServiceURL { get; set; }

        [XmlElement("Issuer", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public SAML_Issuer Issuer { get; set; }

        [XmlElement("NameIDPolicy", Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
        public SAML_NameIDPolicy NameIDPolicy { get; set; }

        [XmlAttribute(AttributeName = "samlp", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Samlp { get; set; }
    }


    [XmlRoot(ElementName = "Issuer", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public class SAML_Issuer
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAML_Issuer()
        {
            xmlns.Add("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
        }
        [XmlAttribute(AttributeName = "saml", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Saml { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "NameIDPolicy", Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    public class SAML_NameIDPolicy
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAML_NameIDPolicy()
        {
            xmlns.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        }
        [XmlAttribute(AttributeName = "AllowCreate")]
        public bool AllowCreate { get; set; }
        [XmlText]
        public string Text { get; set; }

    }
}
