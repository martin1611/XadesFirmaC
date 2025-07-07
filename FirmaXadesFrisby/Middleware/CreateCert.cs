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
    public class CreateCert
    {
        public static XmlElement CreateCertElement(XmlDocument xadesDoc, X509Certificate2 cert, int index)
        {
            if (cert == null) throw new ArgumentNullException(nameof(cert));

            XmlElement certElement = xadesDoc.CreateElement("xades", "Cert", "http://uri.etsi.org/01903/v1.3.2#");

            // Crear CertDigest con SHA-256
            XmlElement certDigest = xadesDoc.CreateElement("xades", "CertDigest", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement digestMethod = xadesDoc.CreateElement("ds", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            //digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
            digestMethod.SetAttribute("Algorithm", SignedXml.XmlDsigSHA256Url);
            XmlElement digestValue = xadesDoc.CreateElement("ds", "DigestValue", "http://www.w3.org/2000/09/xmldsig#");

            digestValue.InnerText = GenerateUnique.GenerateUniqueDigest(cert.RawData, index);

            certDigest.AppendChild(digestMethod);
            certDigest.AppendChild(digestValue);
            certElement.AppendChild(certDigest);

            // Crear IssuerSerial
            XmlElement issuerSerial = xadesDoc.CreateElement("xades", "IssuerSerial", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement x509IssuerName = xadesDoc.CreateElement("ds", "X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement x509SerialNumber = xadesDoc.CreateElement("ds", "X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");

            x509IssuerName.InnerText = cert.Issuer;
            x509SerialNumber.InnerText = GenerateUnique.GenerateUniqueSerialNumber(cert.SerialNumber, index);

            issuerSerial.AppendChild(x509IssuerName);
            issuerSerial.AppendChild(x509SerialNumber);
            certElement.AppendChild(issuerSerial);

            return certElement;
        }
    }
}
