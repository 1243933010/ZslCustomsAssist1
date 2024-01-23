using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    internal class AESUtil
    {
        public static string AesDecrypt(string str, string aesKey)
        {
            byte[] inputBuffer = Convert.FromBase64String(str);
            byte[] numArray1 = Convert.FromBase64String(aesKey);
            byte[] numArray2 = new byte[16];
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = numArray1;
            rijndaelManaged.Mode = CipherMode.CBC;
            rijndaelManaged.Padding = PaddingMode.ISO10126;
            rijndaelManaged.IV = numArray2;
            byte[] sourceArray = rijndaelManaged.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            byte[] numArray3 = new byte[sourceArray.Length - 16];
            Array.Copy((Array)sourceArray, 16, (Array)numArray3, 0, numArray3.Length);
            return Encoding.UTF8.GetString(numArray3);
        }
    }
}
