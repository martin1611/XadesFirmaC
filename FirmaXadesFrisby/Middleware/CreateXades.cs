using FirmaXadesFrisby.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesFrisby.Middleware
{
    public class CreateXades
    {
        public static XmlElement CreateXadesElements(X509Certificate2 cert, string idSignature, string idSignedProps)
        {
            TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTimeOffset colombiaNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, colombiaTimeZone);

            // Formato XSD con offset: yyyy-MM-ddTHH:mm:sszzz
            string signingTimeSignature = colombiaNow.ToString("yyyy-MM-ddTHH:mm:sszzz");

            XmlDocument xadesDoc = new XmlDocument { PreserveWhitespace = false };
            XmlElement objectNode = xadesDoc.CreateElement("ds", "Object", "http://www.w3.org/2000/09/xmldsig#");

            XmlElement qualifyingProperties = xadesDoc.CreateElement("xades", "QualifyingProperties", "http://uri.etsi.org/01903/v1.3.2#");
            qualifyingProperties.SetAttribute("Target", "#" + idSignature);

            XmlElement signedProperties = xadesDoc.CreateElement("xades", "SignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
            signedProperties.SetAttribute("Id", idSignedProps);

            XmlElement signedSignatureProperties = xadesDoc.CreateElement("xades", "SignedSignatureProperties", "http://uri.etsi.org/01903/v1.3.2#");

            XmlElement signingTime = xadesDoc.CreateElement("xades", "SigningTime", "http://uri.etsi.org/01903/v1.3.2#");
            signingTime.InnerText = signingTimeSignature; //Insercion de la hora actual de Colombia

            XmlElement signingCertificate = xadesDoc.CreateElement("xades", "SigningCertificate", "http://uri.etsi.org/01903/v1.3.2#");

            //Creacion de los bloques <xades:Cert>
            for (int i = 0; i < 1; i++)
            {
                XmlElement certElement = CreateCert.CreateCertElement(xadesDoc, cert, i);
                signingCertificate.AppendChild(certElement);
            }

            signedSignatureProperties.AppendChild(signingTime);
            signedSignatureProperties.AppendChild(signingCertificate);

            // Agregar SignaturePolicyIdentifier
            XmlElement signaturePolicyIdentifier = xadesDoc.CreateElement("xades", "SignaturePolicyIdentifier", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement signaturePolicyId = xadesDoc.CreateElement("xades", "SignaturePolicyId", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement sigPolicyId = xadesDoc.CreateElement("xades", "SigPolicyId", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement identifier = xadesDoc.CreateElement("xades", "Identifier", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement descryption = xadesDoc.CreateElement("xades", "Description", "http://uri.etsi.org/01903/v1.3.2#");
            identifier.InnerText = AppSettings.UrlPoliticaFirma;
            descryption.InnerText = "Política de firma para facturas electrónicas de la República de Colombia";
            sigPolicyId.AppendChild(identifier);
            sigPolicyId.AppendChild(descryption);
            signaturePolicyId.AppendChild(sigPolicyId);

            XmlElement sigPolicyHash = xadesDoc.CreateElement("xades", "SigPolicyHash", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement digestMethod = xadesDoc.CreateElement("ds", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement digestValue = xadesDoc.CreateElement("ds", "DigestValue", "http://www.w3.org/2000/09/xmldsig#");
            digestValue.InnerText = CalculatePolicyHashAsync.CalculatePolicyHash().Result;
            //digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
            digestMethod.SetAttribute("Algorithm", SignedXml.XmlDsigSHA256Url);
            sigPolicyHash.AppendChild(digestMethod);
            sigPolicyHash.AppendChild(digestValue);
            signaturePolicyId.AppendChild(sigPolicyHash);
            signaturePolicyIdentifier.AppendChild(signaturePolicyId);
            signedSignatureProperties.AppendChild(signaturePolicyIdentifier);

            // Agregar SignerRole
            XmlElement signerRole = xadesDoc.CreateElement("xades", "SignerRole", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement claimedRoles = xadesDoc.CreateElement("xades", "ClaimedRoles", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement claimedRole = xadesDoc.CreateElement("xades", "ClaimedRole", "http://uri.etsi.org/01903/v1.3.2#");
            claimedRole.InnerText = (AppSettings.Emisor == Convert.ToInt32(RolFirmante.EMISOR) ? "supplier" : "third party"); // Cambiar según el rol del firmante
            claimedRoles.AppendChild(claimedRole);
            signerRole.AppendChild(claimedRoles);

            signedSignatureProperties.AppendChild(signerRole);

            signedProperties.AppendChild(signedSignatureProperties);
            qualifyingProperties.AppendChild(signedProperties);
            objectNode.AppendChild(xadesDoc.ImportNode(qualifyingProperties, true));

            return objectNode;
        }
    }
}
