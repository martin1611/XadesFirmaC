using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FirmaXadesFrisby.Middleware
{
    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml) { }

        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            nsManager.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");

            return document.SelectSingleNode($"//*[@Id='{idValue}']", nsManager) as XmlElement;
        }
    }
}
