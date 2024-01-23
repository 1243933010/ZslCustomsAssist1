using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils.Http
{
    public class HttpWebResponseUtils
    {
        public static HttpWebResponse CreateGetHttpResponse(
            string url,
            int? timeout,
            string userAgent,
            CookieCollection cookies)
        {
            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            if (!string.IsNullOrEmpty(userAgent))
                httpWebRequest.UserAgent = userAgent;
            if (timeout.HasValue)
                httpWebRequest.Timeout = timeout.Value;
            if (cookies != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(cookies);
            }
            httpWebRequest.KeepAlive = false;
            httpWebRequest.UseDefaultCredentials = true;
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

        public static HttpWebResponse CreatePostHttpResponse(
            string url,
            IDictionary<string, string> parameters,
            int? timeout,
            string userAgent,
            Encoding requestEncoding,
            CookieCollection cookies)
        {
            if (requestEncoding == null)
                requestEncoding = Encoding.UTF8;
            HttpWebRequest httpWebRequest;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            if (!string.IsNullOrEmpty(userAgent))
                httpWebRequest.UserAgent = userAgent;
            if (timeout.HasValue)
                httpWebRequest.Timeout = timeout.Value;
            if (cookies != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(cookies);
            }
            if (parameters != null && parameters.Count != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 0;
                foreach (string key in (IEnumerable<string>)parameters.Keys)
                {
                    if (num > 0)
                        stringBuilder.AppendFormat("&{0}={1}", key, parameters[key]);
                    else
                        stringBuilder.AppendFormat("{0}={1}", key, parameters[key]);
                    ++num;
                }
                byte[] bytes = requestEncoding.GetBytes(stringBuilder.ToString());
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);
            }
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

        public static HttpWebResponse CreateJsonPostHttpResponse(
            string url,
            object jsonParams,
            int? timeout,
            string userAgent,
            Encoding requestEncoding,
            CookieCollection cookies)
        {
            if (requestEncoding == null)
                requestEncoding = Encoding.UTF8;
            HttpWebRequest httpWebRequest;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.ProtocolVersion = HttpVersion.Version10;
            }
            else
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json;charset=utf-8";
            if (!string.IsNullOrEmpty(userAgent))
                httpWebRequest.UserAgent = userAgent;
            if (timeout.HasValue)
                httpWebRequest.Timeout = timeout.Value;
            if (cookies != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(cookies);
            }
            if (jsonParams != null)
            {
                string s = JsonConvert.SerializeObject(jsonParams);
                byte[] bytes = requestEncoding.GetBytes(s);
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                    requestStream.Write(bytes, 0, bytes.Length);
            }
            return httpWebRequest.GetResponse() as HttpWebResponse;
        }

        public static string GetResponseString(HttpWebResponse webresponse, Encoding responseEncoding)
        {
            if (responseEncoding == null)
                responseEncoding = Encoding.UTF8;
            using (Stream responseStream = webresponse.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, responseEncoding))
                    return streamReader.ReadToEnd();
            }
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
