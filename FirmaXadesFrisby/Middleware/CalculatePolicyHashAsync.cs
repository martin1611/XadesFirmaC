using FirmaXadesFrisby.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesFrisby.Middleware
{
    public class CalculatePolicyHashAsync
    {
        /// <summary>
        /// Metodo que se encarga de generar el hash de la politica de la firma de la Dian
        /// </summary>
        /// <returns></returns>
        public static async Task<string> CalculatePolicyHash()
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] policyBytes = await client.GetByteArrayAsync(AppSettings.UrlPoliticaFirma);

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(policyBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        /// <summary>
        /// Metodo para calcular un Hash a partir de un valor
        /// </summary>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        public static string CalculateHash(string xmlContent)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
