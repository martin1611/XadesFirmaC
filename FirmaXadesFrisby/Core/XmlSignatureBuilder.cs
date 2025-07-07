using FirmaXadesFrisby.General;
using FirmaXadesFrisby.Middleware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesFrisby.Core
{
    public class XmlSignatureBuilder
    {
        #region Variables

        private readonly XadesSigningProfile _profile;

        #endregion

        #region Constructores

        public XmlSignatureBuilder(XadesSigningProfile profile)
        {
            _profile = profile;
        }

        #endregion

        #region Metodos

        public byte[] Sign(XmlDocument xmlDoc)
        {
            try
            {
                if (xmlDoc == null) throw new ArgumentNullException(nameof(xmlDoc));
                if (_profile.Certificate == null) throw new ArgumentNullException(nameof(_profile.Certificate));

                xmlDoc.PreserveWhitespace = true;

                //calcular el hash del Documento Completo sin firmar
                string hashDocumento = CalculatePolicyHashAsync.CalculateHash(xmlDoc.OuterXml);

                RSA privateKey = _profile.Certificate.GetRSAPrivateKey();
                if (privateKey == null)
                {
                    throw new CryptographicException("El certificado no tiene una clave privada válida.");
                }

                SignedXmlWithId signedXml = new SignedXmlWithId(xmlDoc)
                {
                    SigningKey = privateKey
                };

                string idSignature = "xmldsig-" + Guid.NewGuid().ToString();
                string idSignatureValue = idSignature + "-sigvalue";
                string idKeyInfo = idSignature + "-keyinfo";
                string idSignedProps = idSignature + "-signedprops";
                string idRefDocumento = idSignature + "-ref0";

                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
                signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl; // Canonicalizacion Inclusiva

                //Referencia al documento completo
                Reference refDocumento = new Reference()
                {
                    Uri = "",
                    Id = idRefDocumento,
                    DigestMethod = SignedXml.XmlDsigSHA256Url //"http://www.w3.org/2001/04/xmlenc#sha256"
                };
                refDocumento.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                signedXml.AddReference(refDocumento);

                Reference refSignedProperties = new Reference()
                {
                    Uri = "#" + idSignedProps,
                    Type = "http://uri.etsi.org/01903#SignedProperties",
                    DigestMethod = SignedXml.XmlDsigSHA256Url //"http://www.w3.org/2001/04/xmlenc#sha256"
                };
                refSignedProperties.AddTransform(new XmlDsigC14NTransform()); // Canonicalizacion Inclusiva
                signedXml.AddReference(refSignedProperties);

                // Crear y referenciar SignedProperties
                XmlElement signedPropertiesElement = CreateXades.CreateXadesElements(_profile.Certificate, idSignature, idSignedProps);
                signedPropertiesElement.SetAttribute("Id", null, idSignedProps);

                signedXml.AddObject(new DataObject { Data = signedPropertiesElement.ChildNodes, Id = idSignedProps });

                // Crear manualmente el nodo KeyInfo con solo X509Certificate y X509IssuerSerial
                XmlDocument doc = xmlDoc;

                XmlElement keyInfoElement = doc.CreateElement("ds", "KeyInfo", SignedXml.XmlDsigNamespaceUrl);
                //keyInfoElement.SetAttribute("Id", idKeyInfo);

                XmlElement x509DataElement = doc.CreateElement("ds", "X509Data", SignedXml.XmlDsigNamespaceUrl);

                // Agregar <ds:X509Certificate>
                XmlElement x509CertificateElement = doc.CreateElement("ds", "X509Certificate", SignedXml.XmlDsigNamespaceUrl);
                x509CertificateElement.InnerText = Convert.ToBase64String(_profile.Certificate.RawData);
                x509DataElement.AppendChild(x509CertificateElement);

                // Agregar <ds:X509IssuerSerial>
                XmlElement x509IssuerSerialElement = doc.CreateElement("ds", "X509IssuerSerial", SignedXml.XmlDsigNamespaceUrl);

                XmlElement x509IssuerNameElement = doc.CreateElement("ds", "X509IssuerName", SignedXml.XmlDsigNamespaceUrl);
                x509IssuerNameElement.InnerText = _profile.Certificate.Issuer;

                XmlElement x509SerialNumberElement = doc.CreateElement("ds", "X509SerialNumber", SignedXml.XmlDsigNamespaceUrl);
                BigInteger serialBigInt = BigInteger.Parse(_profile.Certificate.SerialNumber, NumberStyles.HexNumber);
                x509SerialNumberElement.InnerText = serialBigInt.ToString();

                XmlElement x509SubjectNameElement = doc.CreateElement("ds", "X509SubjectName", SignedXml.XmlDsigNamespaceUrl);
                x509SubjectNameElement.InnerText = _profile.Certificate.Subject;

                x509IssuerSerialElement.AppendChild(x509IssuerNameElement);
                x509IssuerSerialElement.AppendChild(x509SerialNumberElement);
                x509DataElement.AppendChild(x509IssuerSerialElement);
                x509DataElement.AppendChild(x509SubjectNameElement);

                keyInfoElement.AppendChild(x509DataElement);

                // Insertar manualmente en SignedXml
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.LoadXml(keyInfoElement);
                signedXml.KeyInfo = keyInfo;

                // Generar la firma con `KeyInfo` ya referenciado correctamente
                signedXml.ComputeSignature();

                // Obtener la firma
                XmlElement signatureElement = signedXml.GetXml();

                // Re-formatear los nodos <ds:SignatureValue> y <ds:X509Certificate> con saltos de línea cada 76 caracteres
                ReformatearNodos.ReformatearBase64Nodos(signatureElement);

                //Agregar Prefijo :ds
                ReformatearNodos.AgregarPrefijoDs(signatureElement);

                if (signatureElement == null)
                {
                    throw new ApplicationException("No se pudo generar la firma XML. Verifique las referencias en <SignedInfo>.");
                }

                signatureElement.SetAttribute("Id", idSignature);

                // Asignar ID a <ds:SignatureValue>
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

                XmlElement signatureValueElement = signatureElement.SelectSingleNode("//ds:SignatureValue", nsMgr) as XmlElement;
                if (signatureValueElement != null)
                {
                    signatureValueElement.SetAttribute("Id", idSignatureValue);
                }

                // Eliminar el Id del nodo <Object>
                XmlElement objectElement = signatureElement.SelectSingleNode("//ds:Object", nsMgr) as XmlElement;
                if (objectElement != null)
                {
                    objectElement.RemoveAttribute("Id"); // Eliminamos el `Id` del nodo <ds:Object>
                }

                InsertSignature.InsertSignatureIntoXml(xmlDoc, signatureElement);

                using (var memoryStream = new MemoryStream())
                {
                    var settings = new XmlWriterSettings
                    {
                        Encoding = new UTF8Encoding(false),
                        Indent = false,
                        NewLineHandling = NewLineHandling.None
                    };

                    using (var writer = XmlWriter.Create(memoryStream, settings))
                    {
                        xmlDoc.Save(writer);
                    }

                    return memoryStream.ToArray(); // <-- estos bytes son los correctos
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new ApplicationException("El documento XML o el certificado no pueden ser nulos.", ex);
            }
            catch (CryptographicException ex)
            {
                throw new ApplicationException("Error al acceder a la clave privada del certificado.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al firmar el documento XML.", ex);
            }
        }

        #endregion
    }
}
