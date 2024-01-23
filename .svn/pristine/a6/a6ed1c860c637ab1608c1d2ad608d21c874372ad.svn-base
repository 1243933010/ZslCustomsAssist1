using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class Base64Helper
    {
        public static void SaveToFile(string base64Str, string saveFilePath)
        {
            byte[] buffer = Convert.FromBase64String(base64Str);
            using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Flush();
            }
        }

        public static string GetBase64ByPath(string filePath)
        {
            FileStream fileStream = File.OpenRead(filePath);
            long length = fileStream.Length;
            byte[] numArray = new byte[fileStream.Length];
            fileStream.Read(numArray, 0, (int)length);
            fileStream.Close();
            return Convert.ToBase64String(numArray, 0, numArray.Length);
        }

        public static string Base64Encode(string source) => Base64Helper.Base64Encode(Encoding.UTF8, source);

        public static string Base64Encode(Encoding encodeType, string source)
        {
            string empty = string.Empty;
            byte[] bytes = encodeType.GetBytes(source);
            string str;
            try
            {
                str = Convert.ToBase64String(bytes);
            }
            catch
            {
                str = source;
            }
            return str;
        }

        public static string Base64Decode(string result) => Base64Helper.Base64Decode(Encoding.UTF8, result);

        public static string Base64Decode(Encoding encodeType, string result)
        {
            if (string.IsNullOrWhiteSpace(result))
                return "";
            string empty = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            string str;
            try
            {
                str = encodeType.GetString(bytes);
            }
            catch
            {
                str = result;
            }
            return str;
        }
    }
}
