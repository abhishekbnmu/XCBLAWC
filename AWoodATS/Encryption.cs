using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace AWoodATS
{
    public class Encryption
    {
        private static string securityKey = "XcblWebServiceMERIDNow";

        /// <summary>
        ///     This method used for encrypting password 
        /// </summary>
        /// <param name="stringToEncrypt">string - password </param>
        /// <returns>string - encrypted password</returns>
        public static string Encrypt(string stringToEncrypt, string securekey)
        {
            if (securekey.Equals(securityKey))
            {

                byte[] keyArray;
                byte[] encryptArray = UTF8Encoding.UTF8.GetBytes(stringToEncrypt);

                // This class used for encrypt secure key
                MD5CryptoServiceProvider cryptoServiceProvideMD5 = new MD5CryptoServiceProvider();
                keyArray = cryptoServiceProvideMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
                cryptoServiceProvideMD5.Clear();

                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = keyArray;
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryToTransform = tripleDES.CreateEncryptor();
                byte[] resultArray = cryToTransform.TransformFinalBlock(encryptArray, 0, encryptArray.Length);
                tripleDES.Clear();

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            return "Error: Securitykey does not match";

        }

        /// <summary>
        ///     This method used for decrypting password     
        /// </summary>
        /// <param name="stringToDecrypt">string - encrypted password</param>
        /// <returns>string - decrypted password</returns>
        public static string Decrypt(string stringToDecrypt, string securekey)
        {
            if (securekey.Equals(securityKey))
            {
                byte[] keyArray;
                stringToDecrypt = stringToDecrypt.Replace(" ", "+");
                byte[] decryptArray = Convert.FromBase64String(stringToDecrypt);

                // This class used for encrypt secure key
                MD5CryptoServiceProvider cryptoServiceProvideMD5 = new MD5CryptoServiceProvider();
                keyArray = cryptoServiceProvideMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
                cryptoServiceProvideMD5.Clear();

                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = keyArray;
                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryToTransform = tripleDES.CreateDecryptor();
                byte[] resultArray = cryToTransform.TransformFinalBlock(decryptArray, 0, decryptArray.Length);
                tripleDES.Clear();

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            return "Error: Securitykey does not match";
        }
    }
}