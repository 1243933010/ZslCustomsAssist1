using System.Collections.Specialized;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZslCustomsAssist.Utils.Http
{
    public class RequestParam
    {
        private RequestParam()
        {
        }

        public static RequestParam GetInstance(string name, string value) => new RequestParam()
        {
            Name = name,
            Value = value
        };

        public RequestParam(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class HttpHelper
    {
        public static string HttpGet(string Url, string postDataStr)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            return HttpGet(Url, postDataStr, headers);
        }

        public static string HttpGet(string Url, string postDataStr, Dictionary<string, string> headers)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "text/html;charset=UTF-8";
            httpWebRequest.Proxy = null;

            if (headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    httpWebRequest.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            string str = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream stream = null;
                try
                {
                    stream = httpWebResponse.GetResponseStream();
                    StreamReader streamReader = null;
                    try
                    {
                        streamReader = new StreamReader(stream, Encoding.UTF8);
                        str = streamReader.ReadToEnd();
                    }
                    finally
                    {
                        streamReader?.Close();
                    }
                }
                finally
                {
                    stream?.Close();
                }
            }
            finally
            {
                httpWebResponse?.Close();
            }
            return str;
        }

        public static string HttpPost(string Url, string postDataStr, string encodStr = "UTF-8", string contentType = "application/x-www-form-urlencoded")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            return HttpPost(Url, postDataStr, headers, encodStr, contentType);
        }

        public static string HttpPost(string Url, string postDataStr, Dictionary<string, string> headers, string encodStr = "UTF-8", string contentType = "application/x-www-form-urlencoded")
        {
            if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = GetEncoding(encodStr);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Proxy = null;

            if (headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    httpWebRequest.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            if (!string.IsNullOrEmpty(postDataStr))
            {
                StreamWriter streamWriter = null;
                try
                {
                    streamWriter = new StreamWriter(httpWebRequest.GetRequestStream(), encoding);
                    streamWriter.Write(postDataStr);
                    streamWriter.Flush();
                }
                finally
                {
                    streamWriter?.Close();
                }
            }

            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            string name = response.ContentEncoding;
            if (name == null || name.Length < 1)
                name = encodStr;
            StreamReader streamReader = null;
            string str = null;
            try
            {
                streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(name));
                str = streamReader.ReadToEnd();
            }
            finally
            {
                streamReader?.Close();
            }
            return str;
        }

        public static string HttpPostForType2(string Url, string postDataStr, string encodStr = "UTF-8")
        {
            GC.Collect();
            if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = GetEncoding(encodStr);
            string str = null;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Timeout = 100000;
            httpWebRequest.ReadWriteTimeout = 100000;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Proxy = null;
            byte[] bytes = encoding.GetBytes(postDataStr);
            Stream stream = null;
            try
            {
                httpWebRequest.GetRequestStream();
                stream = httpWebRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                HttpWebResponse httpWebResponse = null;
                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    string name = httpWebResponse.ContentEncoding;
                    if (name == null || name.Length < 1)
                        name = encodStr;
                    StreamReader streamReader = null;
                    try
                    {
                        streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(name));
                        str = streamReader.ReadToEnd();
                    }
                    finally
                    {
                        streamReader?.Close();
                    }
                }
                finally
                {
                    httpWebResponse?.Close();
                }
            }
            finally
            {
                stream?.Close();
            }
            return str;
        }

        public static void HttpPostAndDownloadFile(
          string Url,
          string postDataStr,
          string path,
          string encodStr = "UTF-8")
        {
            string path1 = Path.GetDirectoryName(path) + "\\temp";
            Directory.CreateDirectory(path1);
            string str = path1 + "\\" + Path.GetFileName(path) + ".temp";
            if (File.Exists(str))
                File.Delete(str);
            GC.Collect();
            if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = GetEncoding(encodStr);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Proxy = null;
            byte[] bytes = encoding.GetBytes(postDataStr);
            Stream stream1 = null;
            try
            {
                httpWebRequest.GetRequestStream();
                stream1 = httpWebRequest.GetRequestStream();
                stream1.Write(bytes, 0, bytes.Length);
                stream1.Flush();
                HttpWebResponse httpWebResponse = null;
                Stream stream2 = null;
                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    stream2 = httpWebResponse.GetResponseStream();
                    FileStream fileStream = new FileStream(str, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    byte[] buffer = new byte[1024];
                    int count;
                    while ((count = stream2.Read(buffer, 0, buffer.Length)) > 0)
                        fileStream.Write(buffer, 0, count);
                    fileStream.Close();
                    stream2.Close();
                    if (File.Exists(path))
                        File.Delete(path);
                    File.Move(str, path);
                }
                finally
                {
                    stream2?.Close();
                    httpWebResponse?.Close();
                }
            }
            finally
            {
                stream1?.Close();
            }
        }

        public static string HttpPostAndUploadFile(
          string url,
          string[] files,
          List<RequestParam> paramList,
          string fileParamsName = "files",
          string encodStr = "UTF-8")
        {
            Encoding encoding = GetEncoding(encodStr);
            string str1 = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] bytes1 = Encoding.ASCII.GetBytes("\r\n--" + str1 + "\r\n");
            byte[] bytes2 = Encoding.ASCII.GetBytes("\r\n--" + str1 + "--\r\n");
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + str1;
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
            Stream stream = null;
            try
            {
                using (stream = httpWebRequest.GetRequestStream())
                {
                    string format1 = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    foreach (RequestParam requestParam in paramList)
                    {
                        stream.Write(bytes1, 0, bytes1.Length);
                        string s = string.Format(format1, requestParam.Name, requestParam.Value);
                        byte[] bytes3 = encoding.GetBytes(s);
                        stream.Write(bytes3, 0, bytes3.Length);
                    }
                    string format2 = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                    byte[] buffer = new byte[4096];
                    for (int index = 0; index < files.Length; ++index)
                    {
                        stream.Write(bytes1, 0, bytes1.Length);
                        string s = string.Format(format2, fileParamsName, Path.GetFileName(files[index]));
                        byte[] bytes4 = encoding.GetBytes(s);
                        stream.Write(bytes4, 0, bytes4.Length);
                        using (FileStream fileStream = new FileStream(files[index], FileMode.Open, FileAccess.Read))
                        {
                            int count;
                            while ((count = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                stream.Write(buffer, 0, count);
                        }
                    }
                    stream.Write(bytes2, 0, bytes2.Length);
                }
            }
            finally
            {
                stream?.Close();
            }
            HttpWebResponse httpWebResponse = null;
            string str2 = null;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = null;
                try
                {
                    using (streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        str2 = streamReader.ReadToEnd();
                }
                finally
                {
                    streamReader?.Close();
                }
            }
            finally
            {
                httpWebResponse?.Close();
            }
            return str2;
        }

        public static string UrlDeCode(string str, string encodStr = "UTF-8")
        {
            Encoding encoding = GetEncoding(encodStr);
            return HttpUtility.UrlDecode(str, encoding);
        }

        public static Encoding GetEncoding(string encodStr)
        {
            Encoding utF8 = Encoding.UTF8;
            return !(encodStr.ToUpper() == "UTF-8") ? !(encodStr.ToUpper() == "GB2312") ? Encoding.UTF8 : Encoding.GetEncoding("gb2312") : Encoding.UTF8;
        }

        public static string UrlEncode(string oldValue, string encodStr = "UTF-8") => string.IsNullOrEmpty(oldValue) ? "" : HttpUtility.UrlEncode(oldValue, GetEncoding(encodStr));

        public static NameValueCollection GetNameValueCollectionByRequestParams(
          List<RequestParam> receiptParams,
          bool isUrlEncode = true)
        {
            NameValueCollection collectionByRequestParams = new NameValueCollection();
            if (receiptParams == null || receiptParams.Count == 0)
                return collectionByRequestParams;
            foreach (RequestParam receiptParam in receiptParams)
            {
                string str = receiptParam.Value != null && !(receiptParam.Value == "") ? HttpUtility.UrlEncode(receiptParam.Value, GetEncoding("")) : "";
                collectionByRequestParams.Add(receiptParam.Name, str);
            }
            return collectionByRequestParams;
        }

        private static bool CheckValidationResult(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors errors)
        {
            return true;
        }
    }
}