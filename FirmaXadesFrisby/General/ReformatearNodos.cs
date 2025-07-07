using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesFrisby.General
{
    public class ReformatearNodos
    {
        public static void ReformatearBase64Nodos(XmlElement signatureElement)
        {            
            if (signatureElement == null) return;

            XmlNamespaceManager ns = new XmlNamespaceManager(signatureElement.OwnerDocument.NameTable);
            ns.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            // Reformat SignatureValue
            var signatureValueNode = signatureElement.SelectSingleNode("//ds:SignatureValue", ns);
            if (signatureValueNode != null)
            {
                string raw = signatureValueNode.InnerText.Replace("\n", "").Replace("\r", "").Trim();
                signatureValueNode.InnerText = InsertLineBreaks(raw, 76);
            }

            // Reformat X509Certificate
            var x509CertNode = signatureElement.SelectSingleNode("//ds:X509Certificate", ns);
            if (x509CertNode != null)
            {
                string raw = x509CertNode.InnerText.Replace("\n", "").Replace("\r", "").Trim();
                x509CertNode.InnerText = InsertLineBreaks(raw, 76);
            }
        }

        private static string InsertLineBreaks(string base64, int lineLength)
        {            
            if (string.IsNullOrEmpty(base64)) return base64;

            var sb = new StringBuilder();
            for (int i = 0; i < base64.Length; i += lineLength)
            {
                int len = Math.Min(lineLength, base64.Length - i);
                sb.Append(base64.Substring(i, len));
                sb.Append("\n"); // Solo salto de línea plano
            }
            return sb.ToString().TrimEnd('\n');
        }

        public static void AgregarPrefijoDs(XmlElement elemento)
        {
            if (elemento == null)
                return;

            if (elemento.NamespaceURI == "http://www.w3.org/2000/09/xmldsig#")
            {
                elemento.Prefix = "ds";
            }

            foreach (XmlNode hijo in elemento.ChildNodes)
            {
                if (hijo is XmlElement hijoElemento)
                {
                    AgregarPrefijoDs(hijoElemento);
                }
            }
        }
    }
}
