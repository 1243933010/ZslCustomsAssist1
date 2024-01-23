using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class CGZipUtil
    {
        private static DataSet GetDatasetByString(string Value)
        {
            DataSet datasetByString = new DataSet();
            StringReader reader = new StringReader(CGZipUtil.GZipDecompressString(Value));
            int num = (int)datasetByString.ReadXml((TextReader)reader);
            return datasetByString;
        }

        public static string GetStringByDataset(string rawString) => string.IsNullOrEmpty(rawString) || rawString.Length == 0 ? "" : Convert.ToBase64String(CGZipUtil.Compress(Encoding.UTF8.GetBytes(rawString.ToString())));

        private static byte[] Compress(byte[] rawData)
        {
            MemoryStream memoryStream = new MemoryStream();
            GZipStream gzipStream = new GZipStream((Stream)memoryStream, CompressionMode.Compress, true);
            gzipStream.Write(rawData, 0, rawData.Length);
            gzipStream.Close();
            return memoryStream.ToArray();
        }

        public static string GZipDecompressString(string zippedString) => string.IsNullOrWhiteSpace(zippedString) ? "" : Encoding.UTF8.GetString(CGZipUtil.Decompress(Convert.FromBase64String(zippedString.ToString())));

        public static byte[] Decompress(byte[] zippedData)
        {
            GZipStream gzipStream = new GZipStream((Stream)new MemoryStream(zippedData), CompressionMode.Decompress);
            MemoryStream memoryStream = new MemoryStream();
            byte[] buffer = new byte[1024];
            while (true)
            {
                int count = gzipStream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                    memoryStream.Write(buffer, 0, count);
                else
                    break;
            }
            gzipStream.Close();
            return memoryStream.ToArray();
        }
    }
}
