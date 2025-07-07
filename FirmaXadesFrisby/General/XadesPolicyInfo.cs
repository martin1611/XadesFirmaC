using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesFrisby.General
{
    public class XadesPolicyInfo
    {
        public string PolicyIdentifier { get; set; } = string.Empty;
        public string PolicyDescription { get; set; } = string.Empty;
        public string PolicyHashValue { get; set; } = string.Empty;
        public string HashAlgorithm { get; set; } = string.Empty;
    }
}
