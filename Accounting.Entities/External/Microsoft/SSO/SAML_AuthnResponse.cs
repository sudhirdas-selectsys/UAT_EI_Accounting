using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Accounting.Entities.External.Microsoft.SSO
{
    [XmlRoot("Response", Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    public class SAML_AuthnResponse
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        public SAML_AuthnResponse()
        {
            xmlns.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        }
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }

        [XmlAttribute("Destination")]
        public string Destination { get; set; }

        [XmlAttribute("InResponseTo")]
        public string InResponseTo { get; set; }

        [XmlElement("Issuer", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public string Issuer { get; set; }

        [XmlElement(ElementName = "Status")]
        public SAML_Status Status { get; set; }

        [XmlElement("Assertion", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public SAML_Assertion Assertion { get; set; }
    }

    public class SAML_Status
    {
        //[XmlNamespaceDeclarations]
        //public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        //public SAML_Status()
        //{
        //    xmlns.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
        //}
        [XmlElement("StatusCode")]
        public SAML_StatusCode StatusCode { get; set; }
    }

    public class SAML_StatusCode
    {
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }

    public class SAML_Assertion
    {
        //[XmlNamespaceDeclarations]
        //public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        //public SAML_Assertion()
        //{
        //    xmlns.Add("xmlns", "urn:oasis:names:tc:SAML:2.0:assertion");
        //}
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlAttribute("IssueInstant")]
        public DateTime IssueInstant { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlElement("Issuer", Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public string Issuer { get; set; }

        [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public SAML_Signature Signature { get; set; }

        [XmlElement("Subject")]
        public SAML_Subject Subject { get; set; }

        [XmlElement("Conditions")]
        public SAML_Conditions Conditions { get; set; }

        [XmlElement("AttributeStatement")]
        public SAML_AttributeStatement AttributeStatement { get; set; }

        [XmlElement("AuthnStatement")]
        public SAML_AuthnStatement AuthnStatement { get; set; }
    }

    public class SAML_Signature
    {
        [XmlElement("SignedInfo")]
        public SAML_SignedInfo SignedInfo { get; set; }

        [XmlElement("SignatureValue")]
        public string SignatureValue { get; set; }

        [XmlElement("KeyInfo")]
        public SAML_KeyInfo KeyInfo { get; set; }
    }

    public class SAML_SignedInfo
    {
        [XmlElement("CanonicalizationMethod")]
        public SAML_CanonicalizationMethod CanonicalizationMethod { get; set; }

        [XmlElement("SignatureMethod")]
        public SAML_SignatureMethod SignatureMethod { get; set; }

        [XmlElement("Reference")]
        public SAML_Reference Reference { get; set; }
    }

    public class SAML_CanonicalizationMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }
    }

    public class SAML_SignatureMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }
    }

    public class SAML_Reference
    {
        [XmlAttribute("URI")]
        public string URI { get; set; }

        [XmlElement("Transforms")]
        public SAML_Transforms Transforms { get; set; }

        [XmlElement("DigestMethod")]
        public SAML_DigestMethod DigestMethod { get; set; }

        [XmlElement("DigestValue")]
        public string DigestValue { get; set; }
    }

    public class SAML_Transforms
    {
        [XmlElement("Transform")]
        public List<SAML_Transform> TransformList { get; set; }
    }

    public class SAML_Transform
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }
    }

    public class SAML_DigestMethod
    {
        [XmlAttribute("Algorithm")]
        public string Algorithm { get; set; }
    }

    public class SAML_KeyInfo
    {
        [XmlElement("X509Data")]
        public SAML_X509Data X509Data { get; set; }
    }

    public class SAML_X509Data
    {
        [XmlElement("X509Certificate")]
        public string X509Certificate { get; set; }
    }

    public class SAML_Subject
    {
        [XmlElement("NameID")]
        public SAML_NameID NameID { get; set; }

        [XmlElement("SubjectConfirmation")]
        public SAML_SubjectConfirmation SubjectConfirmation { get; set; }
    }

    public class SAML_NameID
    {
        [XmlAttribute("Format")]
        public string Format { get; set; }
    }

    public class SAML_SubjectConfirmation
    {
        [XmlAttribute("Method")]
        public string Method { get; set; }

        [XmlElement("SubjectConfirmationData")]
        public SAML_SubjectConfirmationData SubjectConfirmationData { get; set; }
    }

    public class SAML_SubjectConfirmationData
    {
        [XmlAttribute("InResponseTo")]
        public string InResponseTo { get; set; }

        [XmlAttribute("NotOnOrAfter")]
        public DateTime NotOnOrAfter { get; set; }

        [XmlAttribute("Recipient")]
        public string Recipient { get; set; }
    }

    public class SAML_Conditions
    {
        [XmlAttribute("NotBefore")]
        public DateTime NotBefore { get; set; }

        [XmlAttribute("NotOnOrAfter")]
        public DateTime NotOnOrAfter { get; set; }

        [XmlElement("AudienceRestriction")]
        public SAML_AudienceRestriction AudienceRestriction { get; set; }
    }

    public class SAML_AudienceRestriction
    {
        [XmlElement("Audience")]
        public string Audience { get; set; }
    }

    public class SAML_AttributeStatement
    {
        [XmlElement("Attribute")]
        public List<SAML_Attribute> Attributes { get; set; }
    }

    public class SAML_Attribute
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("AttributeValue")]
        public string AttributeValue { get; set; }
    }

    public class SAML_AuthnStatement
    {
        [XmlAttribute("AuthnInstant")]
        public DateTime AuthnInstant { get; set; }

        [XmlAttribute("SessionIndex")]
        public string SessionIndex { get; set; }

        [XmlElement("AuthnContext")]
        public SAML_AuthnContext AuthnContext { get; set; }
    }

    public class SAML_AuthnContext
    {
        [XmlElement("AuthnContextClassRef")]
        public string AuthnContextClassRef { get; set; }
    }
}
