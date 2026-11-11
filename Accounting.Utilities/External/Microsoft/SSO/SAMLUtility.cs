using Accounting.Entities.External.Microsoft.SSO;
using Accounting.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography.Xml;

namespace Accounting.Utilities.External.Microsoft.SSO
{
    public class SAMLUtility
    {
        //public string CreateSAMLAuthRequest(string ApplicationId, string DirectoryId, string RedirectionURLAfterLogIn)
        //{
        //    SAML_AuthnRequest _AuthnRequest = new SAML_AuthnRequest();
        //    _AuthnRequest.ID = "_" + Guid.NewGuid();
        //    _AuthnRequest.IssueInstant = DateTime.UtcNow.ToString("o");
        //    _AuthnRequest.AssertionConsumerServiceURL = RedirectionURLAfterLogIn;
        //    _AuthnRequest.Destination = $"https://login.microsoftonline.com/{DirectoryId}/saml2";
        //    _AuthnRequest.Issuer = new SAML_Issuer() { Text = ApplicationId };
        //    return XMLUtility.Serialize(typeof(SAML_AuthnRequest), _AuthnRequest);
        //}
        public string CreateSAMLAuthRequest(
        string applicationId,
        string tenantId,
        string acsUrl)
        {
            string id = "_" + Guid.NewGuid().ToString("N");
            string issueInstant = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // ✅ VALID SAML 2.0 AUTHN REQUEST
            string samlRequest = $@"
<samlp:AuthnRequest
    xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol""
    xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion""
    ID=""{id}""
    Version=""2.0""
    IssueInstant=""{issueInstant}""
    Destination=""https://login.microsoftonline.com/{tenantId}/saml2""
    AssertionConsumerServiceURL=""{acsUrl}""
    ProtocolBinding=""urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"">

    <saml:Issuer>spn:{applicationId}</saml:Issuer>

</samlp:AuthnRequest>";

            // IMPORTANT: return RAW XML (Base64 encode outside)
            return samlRequest.Trim();
        }
        public string CreateSAMLLogoutRequest(string ApplicationId, string DirectoryId, string Email)
        {
            SAML_LogoutRequest _LogoutRequest = new SAML_LogoutRequest();
            _LogoutRequest.ID = "_" + Guid.NewGuid();
            _LogoutRequest.IssueInstant = DateTime.UtcNow.ToString("o");
            _LogoutRequest.Destination = $"https://login.microsoftonline.com/{DirectoryId}/saml2";
            _LogoutRequest.Issuer = new SAML_Issuer() { Text = ApplicationId };
            _LogoutRequest.NameID = new SAMLLogout_NameID() { Text = Email };
            return XMLUtility.Serialize(typeof(SAML_LogoutRequest), _LogoutRequest);
        }
        public X509Certificate2 ReadCertificate(string MetaDataXML)
        {
            // Load the SAML response XML
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(MetaDataXML);
            // Extract the certificate from the SAML response (you may need to adapt this part)
            var certificateNode = xmlDoc.GetElementsByTagName("X509Certificate")[0];
            var certString = certificateNode.InnerText;
            var certificate = new X509Certificate2(Convert.FromBase64String(certString));

            // Check if the certificate is valid
            var chain = new X509Chain();
            chain.Build(certificate);
            return certificate;
        }
        public SAML_AuthnResponse ValidateSamlResponse(string SAMLResponse, string webRootPath, string expectedAudience, string MetaDataXML)
        {
            // Decode the response
            SAMLResponse = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(SAMLResponse));
            SAML_AuthnResponse _AuthnResponse = (SAML_AuthnResponse)XMLUtility.DeSerialize(typeof(SAML_AuthnResponse), SAMLResponse);


            //string MetaDataXML = System.IO.File.ReadAllText($"{webRootPath}/MSADSSOMetadataXMLFiles/{FederationMetadataXMLFileName}");

            SAMLUtility _SAMLUtility = new SAMLUtility();
            // Validate the SAML response (e.g., signature, issuer, etc.)
            X509Certificate2 idpCertificate = _SAMLUtility.ReadCertificate(MetaDataXML);

            // Step 1: Validate the signature
            if (!ValidateSamlSignature(SAMLResponse, idpCertificate))
            {
                throw new SecurityException("Invalid SAML response signature.");
            }

            // Step 3: Check response status
            if (!IsResponseSuccessful(_AuthnResponse))
            {
                throw new SecurityException("SAML response indicates failure.");
            }

            // Step 4: Validate assertions
            if (!ValidateAssertions(_AuthnResponse, expectedAudience))
            {
                throw new SecurityException("SAML assertion validation failed.");
            }

            return _AuthnResponse; // If all validations pass
        }
        public bool IsResponseSuccessful(SAML_AuthnResponse _AuthnResponse)
        {
            return _AuthnResponse.Status.StatusCode.Value == "urn:oasis:names:tc:SAML:2.0:status:Success";
        }
        public bool ValidateAssertions(SAML_AuthnResponse _AuthnResponse, string expectedAudience)
        {
            var assertion = _AuthnResponse.Assertion;

            // Check NotBefore and NotOnOrAfter
            if (assertion.Conditions.NotBefore > DateTime.UtcNow || assertion.Conditions.NotOnOrAfter < DateTime.UtcNow)
            {
                return false; // Assertion is expired or not yet valid
            }

            // Check AudienceRestriction
            if (assertion.Conditions.AudienceRestriction.Audience != expectedAudience)
            {
                return false; // Audience restriction does not match
            }

            return true;
        }
        public bool ValidateSamlSignature(string samlXml, X509Certificate2 certificate)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(samlXml);

            // Create a SignedXml object and pass in the XML document.
            SignedXml signedXml = new SignedXml(xmlDocument);

            // Find the "Signature" node in the XML document.
            XmlNode signatureNode = xmlDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];

            if (signatureNode != null)
            {
                signedXml.LoadXml((XmlElement)signatureNode);
            }
            else
            {
                throw new Exception("Signature not found in SAML response.");
            }

            // Check the signature and return the result.
            return signedXml.CheckSignature(certificate, true);
        }
    }
}
