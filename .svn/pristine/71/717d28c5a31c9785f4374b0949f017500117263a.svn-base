using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Http;

namespace ZslCustomsAssist.Utils
{
    public class Encrypter
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        private static byte[] DES_IV = new byte[8]
        {
          (byte) 18,
          (byte) 52,
          (byte) 86,
          (byte) 120,
          (byte) 144,
          (byte) 171,
          (byte) 205,
          (byte) 239
        };
        //private static string AESKey = "Si62Em771w5PEUY3";
        //private static string AESIV = "hH4Ve6MacXEyeBt1";
        private static string AESKey = "X29YEm7U1w5N32IV";
        private static string AESIV = "MeiVo6POiX8NyFtZ";

        public static string EncryptByMD5(string input)
        {
            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));
            return stringBuilder.ToString();
        }

        public static string EncryptBySHA1(string input) => BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(input)));

        public static string EncryptByDES(string input, string key)
        {
            byte[] bytes1 = Encoding.UTF8.GetBytes(input);
            byte[] bytes2 = Encoding.UTF8.GetBytes(key);
            byte[] inArray = Encrypter.EncryptByDES(bytes1, bytes2, bytes2);
            using (DES des = (DES)new DESCryptoServiceProvider())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            streamWriter.Write((object)bytes1);
                    }
                }
            }
            return Convert.ToBase64String(inArray);
        }

        public static byte[] EncryptByDES(byte[] inputBytes, byte[] key, byte[] IV)
        {
            DESCryptoServiceProvider cryptoServiceProvider = new DESCryptoServiceProvider();
            cryptoServiceProvider.Key = key;
            cryptoServiceProvider.IV = IV;
            DES des = (DES)cryptoServiceProvider;
            string empty = string.Empty;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                return memoryStream.ToArray();
            }
        }

        public static string DecryptByDES(string input, string key)
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] bytes = Encoding.UTF8.GetBytes(key);
            return Encoding.UTF8.GetString(Encrypter.DecryptByDES(inputBytes, bytes, bytes));
        }

        public static byte[] DecryptByDES(byte[] inputBytes, byte[] key, byte[] iv)
        {
            DESCryptoServiceProvider cryptoServiceProvider1 = new DESCryptoServiceProvider();
            cryptoServiceProvider1.Key = key;
            cryptoServiceProvider1.IV = iv;
            DESCryptoServiceProvider cryptoServiceProvider2 = cryptoServiceProvider1;
            using (MemoryStream memoryStream = new MemoryStream(inputBytes))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, cryptoServiceProvider2.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        return Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
                }
            }
        }

        public static string EncryptString(string input, string sKey)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using (DESCryptoServiceProvider cryptoServiceProvider = new DESCryptoServiceProvider())
            {
                cryptoServiceProvider.Key = Encoding.ASCII.GetBytes(sKey);
                cryptoServiceProvider.IV = Encoding.ASCII.GetBytes(sKey);
                return BitConverter.ToString(cryptoServiceProvider.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
            }
        }

        public static string DecryptString(string input, string sKey)
        {
            string[] strArray = input.Split("-".ToCharArray());
            byte[] inputBuffer = new byte[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
                inputBuffer[index] = byte.Parse(strArray[index], NumberStyles.HexNumber);
            using (DESCryptoServiceProvider cryptoServiceProvider = new DESCryptoServiceProvider())
            {
                cryptoServiceProvider.Key = Encoding.ASCII.GetBytes(sKey);
                cryptoServiceProvider.IV = Encoding.ASCII.GetBytes(sKey);
                return Encoding.UTF8.GetString(cryptoServiceProvider.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
            }
        }

        public static string EncodeAES(string text, string key, string iv)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Mode = CipherMode.CBC;
            rijndaelManaged.Padding = PaddingMode.Zeros;
            rijndaelManaged.KeySize = 128;
            rijndaelManaged.BlockSize = 128;
            byte[] bytes1 = Encoding.GetEncoding("GBK").GetBytes(key);
            byte[] destinationArray = new byte[16];
            int length = bytes1.Length;
            if (length > destinationArray.Length)
                length = destinationArray.Length;
            Array.Copy((Array)bytes1, (Array)destinationArray, length);
            rijndaelManaged.Key = destinationArray;
            rijndaelManaged.IV = Encoding.GetEncoding("GBK").GetBytes(iv);
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
            byte[] bytes2 = Encoding.GetEncoding("GBK").GetBytes(text);
            return Convert.ToBase64String(encryptor.TransformFinalBlock(bytes2, 0, bytes2.Length));
        }

        public static string DecodeAES(string text, string key, string iv)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Mode = CipherMode.CBC;
            rijndaelManaged.Padding = PaddingMode.Zeros;
            rijndaelManaged.KeySize = 128;
            rijndaelManaged.BlockSize = 128;
            byte[] inputBuffer = Convert.FromBase64String(text);
            byte[] bytes1 = Encoding.GetEncoding("GBK").GetBytes(key);
            byte[] destinationArray = new byte[16];
            int length = bytes1.Length;
            if (length > destinationArray.Length)
                length = destinationArray.Length;
            Array.Copy((Array)bytes1, (Array)destinationArray, length);
            rijndaelManaged.Key = destinationArray;
            rijndaelManaged.IV = Encoding.GetEncoding("GBK").GetBytes(iv);
            byte[] bytes2 = rijndaelManaged.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Encoding.GetEncoding("GBK").GetString(bytes2);
        }

        public static string DefaultEncodeAES(string text) => string.IsNullOrEmpty(text) ? "" : Encrypter.EncodeAES(text, Encrypter.AESKey, Encrypter.AESIV);

        public static string DefaultDecodeAES(string text) => string.IsNullOrEmpty(text) ? "" : Encrypter.DecodeAES(text, Encrypter.AESKey, Encrypter.AESIV);

        public static void EncodeRequestParamByAES(List<RequestParam> requestParams)
        {
            int count = requestParams.Count;
            for (int index = 0; index < count; ++index)
                requestParams[index].Value = Encrypter.EncodeAES(requestParams[index].Value, Encrypter.AESKey, Encrypter.AESIV);
        }

        public static string EncryptByRSA(string plaintext, string publicKey)
        {
            byte[] bytes = new UnicodeEncoding().GetBytes(plaintext);
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(publicKey);
                return Convert.ToBase64String(cryptoServiceProvider.Encrypt(bytes, false));
            }
        }

        public static string DecryptByRSA(string ciphertext, string privateKey)
        {
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(privateKey);
                byte[] rgb = Convert.FromBase64String(ciphertext);
                byte[] bytes = cryptoServiceProvider.Decrypt(rgb, false);
                return unicodeEncoding.GetString(bytes);
            }
        }

        public static string HashAndSignString(string plaintext, string privateKey)
        {
            byte[] bytes = new UnicodeEncoding().GetBytes(plaintext);
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(privateKey);
                return Convert.ToBase64String(cryptoServiceProvider.SignData(bytes, (object)new SHA1CryptoServiceProvider()));
            }
        }

        public static bool VerifySigned(string plaintext, string SignedData, string publicKey)
        {
            using (RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider())
            {
                cryptoServiceProvider.FromXmlString(publicKey);
                byte[] bytes = new UnicodeEncoding().GetBytes(plaintext);
                byte[] signature = Convert.FromBase64String(SignedData);
                return cryptoServiceProvider.VerifyData(bytes, (object)new SHA1CryptoServiceProvider(), signature);
            }
        }

        public static KeyValuePair<string, string> CreateRSAKey()
        {
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            string xmlString = cryptoServiceProvider.ToXmlString(true);
            return new KeyValuePair<string, string>(cryptoServiceProvider.ToXmlString(false), xmlString);
        }

        public static byte[] GetBytes(string input)
        {
            string[] strArray = input.Split("-".ToCharArray());
            byte[] bytes = new byte[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
                bytes[index] = byte.Parse(strArray[index], NumberStyles.HexNumber);
            return bytes;
        }

        public static string TypistPasswordEncrypter(string typistPassword, string secretKey = "Sd@6Y4Si") => Encrypter.EncryptString(typistPassword, secretKey);

        public static string TypistPasswordDecrypter(string typistPassword, string secretKey = "Sd@6Y4Si") => typistPassword.Length != 8 ? Encrypter.DecryptString(typistPassword, secretKey) : typistPassword;

        public static string SystemDataEncrypter(string data, string secretKey = "6mK!y*9R") => Encrypter.EncryptByDES(data, secretKey);

        public static string SystemDataDecrypter(string data, string secretKey = "6mK!y*9R") => Encrypter.DecryptByDES(data, secretKey);
    }
}
