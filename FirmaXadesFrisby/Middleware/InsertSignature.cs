using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesFrisby.Middleware
{
    public class InsertSignature
    {
        /// <summary>
        /// Metodo que se encarga de insertar la firma en el nodo Signature
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="xmlSignature"></param>
        /// <exception cref="Exception"></exception>
        public static void InsertSignatureIntoXml(XmlDocument xmlDoc, XmlElement xmlSignature)
        {
            // Crear el administrador de namespaces
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsMgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            nsMgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#"); // Namespace para la firma digital

            // Seleccionar el último nodo <ext:UBLExtension> existente
            XmlNode lastExtensionNode = xmlDoc.SelectSingleNode("//ext:UBLExtensions/ext:UBLExtension[last()]", nsMgr);

            if (lastExtensionNode != null)
            {
                // Crear el nuevo nodo <ext:UBLExtension> para la firma
                XmlElement newExtension = xmlDoc.CreateElement("ext", "UBLExtension", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                XmlElement newContent = xmlDoc.CreateElement("ext", "ExtensionContent", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                // Importar y agregar la firma digital al nuevo nodo
                XmlNode importedSignature = xmlDoc.ImportNode(xmlSignature, true);
                newContent.AppendChild(importedSignature);
                newExtension.AppendChild(newContent);

                // Insertar el nuevo nodo después del último nodo existente
                lastExtensionNode.ParentNode.InsertAfter(newExtension, lastExtensionNode);
            }
            else
            {
                throw new Exception("No se encontró ningún nodo <ext:UBLExtension> existente en el XML.");
            }
        }
    }
}
