using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils.Log;
using Formatting = Newtonsoft.Json.Formatting;

namespace ZslCustomsAssist.Utils.sm
{
    internal class Sm3Crypto
    {
        public static byte[] ToSM3byte(string data, string key)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            KeyParameter parameters = new KeyParameter(Encoding.Default.GetBytes(key));
            HMac hmac = new HMac((IDigest)new SM3Digest());
            hmac.Init((ICipherParameters)parameters);
            hmac.BlockUpdate(bytes, 0, bytes.Length);
            byte[] numArray = new byte[hmac.GetMacSize()];
            hmac.DoFinal(numArray, 0);
            return Hex.Encode(numArray);
        }

        public static byte[] ToSM3byte(string data)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            SM3Digest sm3Digest = new SM3Digest();
            sm3Digest.BlockUpdate(bytes, 0, bytes.Length);
            byte[] numArray = new byte[sm3Digest.GetDigestSize()];
            sm3Digest.DoFinal(numArray, 0);
            return Hex.Encode(numArray);
        }

        public static string ToSM3Base64Str(string data)
        {
            AbstractLog.logger.Info((string)("【" + "】byte0-------！"));
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            SM3Digest sm3Digest = new SM3Digest();
            sm3Digest.BlockUpdate(bytes, 0, bytes.Length);
            byte[] numArray = new byte[sm3Digest.GetDigestSize()];
            string jsonStr = JsonConvert.SerializeObject(numArray,Formatting.Indented);
            AbstractLog.logger.Info((string)("【" + jsonStr + "】byte-------！"));
            sm3Digest.DoFinal(numArray, 0);

            return Convert.ToBase64String(numArray);
        }

        public static string ToSM3HexStr(string data, string key)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);
            KeyParameter parameters = new KeyParameter(Sm3Crypto.HexStringToBytes(key));
            HMac hmac = new HMac((IDigest)new SM3Digest());
            hmac.Init((ICipherParameters)parameters);
            hmac.BlockUpdate(bytes, 0, bytes.Length);
            byte[] numArray = new byte[hmac.GetMacSize()];
            hmac.DoFinal(numArray, 0);
            return new UTF8Encoding().GetString(Hex.Encode(numArray));
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            hexString = Regex.Replace(hexString, ".{2}", "$0 ");
            string[] strArray = hexString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
                bytes[index] = Convert.ToByte(strArray[index], 16);
            return bytes;
        }
    }
}
