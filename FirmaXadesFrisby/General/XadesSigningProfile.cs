using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesFrisby.General
{
    public class XadesSigningProfile
    {
        public X509Certificate2 Certificate { get; set; } = default!;
    }
}
