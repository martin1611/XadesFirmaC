using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirmaXadesFrisby.Middleware
{
    public class GenerateUnique
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GenerateUniqueSerialNumber(string serialNumber, int index)
        {
            BigInteger serialBigInt = BigInteger.Parse(serialNumber, NumberStyles.HexNumber);
            return (serialBigInt + index).ToString();
        }

        /// <summary>
        /// Metodo que se encarga de generar los hash para cada nodo Cert de la politica de la firma
        /// </summary>
        /// <param name="certData"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GenerateUniqueDigest(byte[] certData, int index)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashInput = certData.Concat(BitConverter.GetBytes(index)).ToArray();
                return Convert.ToBase64String(sha256.ComputeHash(hashInput));
            }
        }
    }
}
