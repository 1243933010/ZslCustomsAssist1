using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;
using ZslCustomsAssist.Utils.Http;

namespace ZslCustomsAssist.Utils
{
    public class SystemInfoHelper
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        public static string GetUserName()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
                    empty = instance["UserName"].ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetMacAddress()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
                {
                    if ((bool)instance["IPEnabled"])
                        empty = instance["MacAddress"].ToString();
                }
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string getMacAddr_Local()
        {
            string str = (string)null;
            try
            {
                foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
                {
                    if (Convert.ToBoolean(instance["IPEnabled"]))
                    {
                        str = instance["MacAddress"].ToString();
                        str = str.Replace(':', '-');
                    }
                    instance.Dispose();
                }
                return str ?? "unknown";
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetClientLocalIPv6Address()
        {
            string empty = string.Empty;
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetClientLocalIPv4Address()
        {
            string empty = string.Empty;
            try
            {
                return Dns.Resolve(Dns.GetHostName()).AddressList[0].ToString();
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static List<string> GetClientLocalIPv4AddressList()
        {
            List<string> localIpv4AddressList = new List<string>();
            try
            {
                foreach (IPAddress address in Dns.Resolve(Dns.GetHostName()).AddressList)
                {
                    if (!localIpv4AddressList.Contains(address.ToString()))
                        localIpv4AddressList.Add(address.ToString());
                }
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
            }
            return localIpv4AddressList;
        }

        public static string GetClientInternetIPAddress()
        {
            string empty = string.Empty;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string input = webClient.DownloadString("http://www.coridc.com/ip");
                    Regex regex = new Regex("[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}");
                    if (!regex.Match(input).Success)
                    {
                        input = SystemInfoHelper.GetClientInternetIPAddress2();
                        if (!regex.Match(input).Success)
                            input = "unknown";
                    }
                    return input;
                }
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetClientInternetIPAddress2()
        {
            try
            {
                WebRequest webRequest = WebRequest.Create("http://iframe.ip138.com/ic.asp");
                Stream stream = (Stream)null;
                string str = (string)null;
                try
                {
                    stream = webRequest.GetResponse().GetResponseStream();
                    StreamReader streamReader = (StreamReader)null;
                    try
                    {
                        streamReader = new StreamReader(stream, Encoding.Default);
                        streamReader.ReadToEnd();
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
                int startIndex = str.IndexOf("[") + 1;
                int num = str.IndexOf("]", startIndex);
                return str.Substring(startIndex, num - startIndex);
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetDiskID()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject instance in new ManagementClass("Win32_DiskDrive").GetInstances())
                    empty = instance.Properties["Model"].Value.ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetCpuID()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject instance in new ManagementClass("Win32_Processor").GetInstances())
                    empty = instance.Properties["ProcessorId"].Value.ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetSystemType()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
                    empty = instance["SystemType"].ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetSystemName()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("root\\CIMV2", "SELECT PartComponent FROM Win32_SystemOperatingSystem").Get())
                    empty = managementBaseObject["PartComponent"].ToString();
                foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption FROM Win32_OperatingSystem").Get())
                    empty = managementBaseObject["Caption"].ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetTotalPhysicalMemory()
        {
            try
            {
                string empty = string.Empty;
                foreach (ManagementBaseObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
                    empty = instance["TotalPhysicalMemory"].ToString();
                return empty;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetMotherBoardID()
        {
            try
            {
                ManagementObjectCollection instances = new ManagementClass("Win32_BaseBoard").GetInstances();
                string motherBoardId = (string)null;
                using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = instances.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        motherBoardId = enumerator.Current.Properties["SerialNumber"].Value.ToString();
                }
                return motherBoardId;
            }
            catch (Exception ex)
            {
                SystemInfoHelper.logger.Info((object)ex.Message);
                return "unknown";
            }
        }

        public static string GetAllUsersDesktopFolderPath() => SystemInfoHelper.OpenRegistryPath(Registry.LocalMachine, "/software/microsoft/windows/currentversion/explorer/shell folders").GetValue("Common Desktop").ToString();

        public static string GetAllUsersStartupFolderPath() => SystemInfoHelper.OpenRegistryPath(Registry.LocalMachine, "/software/microsoft/windows/currentversion/explorer/shell folders").GetValue("Common Startup").ToString();

        private static RegistryKey OpenRegistryPath(RegistryKey root, string s)
        {
            for (s = s.Remove(0, 1) + "/"; s.IndexOf("/") != -1; s = s.Remove(0, s.IndexOf("/") + 1))
                root = root.OpenSubKey(s.Substring(0, s.IndexOf("/")));
            return root;
        }

        public static string GetLocalIP()
        {
            Match match = Regex.Match(SystemInfoHelper.RunApp("route", "print", true), "0.0.0.0\\s+0.0.0.0\\s+(\\d+.\\d+.\\d+.\\d+)\\s+(\\d+.\\d+.\\d+.\\d+)");
            if (match.Success)
                return match.Groups[2].Value;
            try
            {
                return SystemInfoHelper.GetExternalNetworkIpAddress();
            }
            catch (Exception ex)
            {
                return "0.0.0.0";
            }
        }

        public static string GetExternalNetworkIpAddress22()
        {
            string networkIpAddress22 = (string)null;
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = CredentialCache.DefaultCredentials;
                networkIpAddress22 = Encoding.UTF8.GetString(webClient.DownloadData("http://www.3322.org/dyndns/getip")).Trim();
            }
            if (string.IsNullOrWhiteSpace(networkIpAddress22))
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = CredentialCache.DefaultCredentials;
                    string input = Encoding.UTF8.GetString(webClient.DownloadData("http://pv.sohu.com/cityjson?ie=utf-8"));
                    webClient.Dispose();
                    networkIpAddress22 = Regex.Match(input, "\\d{2,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").Value;
                }
            }
            return networkIpAddress22;
        }

        public static List<string> GetLocalIpAddress(string netType)
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            List<string> localIpAddress = new List<string>();
            if (netType == string.Empty)
            {
                for (int index = 0; index < hostAddresses.Length; ++index)
                    localIpAddress.Add(hostAddresses[index].ToString());
            }
            else
            {
                for (int index = 0; index < hostAddresses.Length; ++index)
                {
                    if (hostAddresses[index].AddressFamily.ToString() == netType)
                        localIpAddress.Add(hostAddresses[index].ToString());
                }
            }
            return localIpAddress;
        }

        public static string GetExternalNetworkIpAddress() => SystemInfoHelper.GetLocalIpAddress("InterNetwork")[0];

        public static string GetExternalNetworkIpAddress3()
        {
            string networkIpAddress3 = "";
            HttpWebResponse getHttpResponse;
            try
            {
                getHttpResponse = HttpWebResponseUtils.CreateGetHttpResponse("http://pv.sohu.com/cityjson?ie=utf-8", new int?(100000), (string)null, (CookieCollection)null);
            }
            catch (Exception ex1)
            {
                SystemInfoHelper.logger.Warn((object)"无法访问{ http://pv.sohu.com/cityjson?ie=utf-8 }", ex1);
                try
                {
                    getHttpResponse = HttpWebResponseUtils.CreateGetHttpResponse("http://ip-api.com/json/?lang=zh-CN", new int?(100000), (string)null, (CookieCollection)null);
                }
                catch (Exception ex2)
                {
                    SystemInfoHelper.logger.Warn((object)"无法访问{ http://ip-api.com/json/?lang=zh-CN }", ex2);
                    getHttpResponse = HttpWebResponseUtils.CreateGetHttpResponse("https://ip.cn/api/index?ip=&type=0", new int?(100000), (string)null, (CookieCollection)null);
                }
            }
            if (getHttpResponse != null)
                networkIpAddress3 = Regex.Match(HttpWebResponseUtils.GetResponseString(getHttpResponse, Encoding.UTF8), "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").Value;
            return networkIpAddress3;
        }

        public static string InternalNetworkIpAddress() => string.Join<IPAddress>(", ", ((IEnumerable<IPAddress>)Dns.GetHostAddresses(Dns.GetHostName())).Where<IPAddress>((Func<IPAddress, bool>)(ip => ip.GetAddressBytes().Length == 4)).Select<IPAddress, IPAddress>((Func<IPAddress, IPAddress>)(iPAddress => iPAddress)));

        public static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                    Trace.WriteLine(filename + " " + arguments);
                Process process = new Process();
                process.StartInfo.FileName = filename;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                StreamReader streamReader = (StreamReader)null;
                try
                {
                    using (streamReader = new StreamReader(process.StandardOutput.BaseStream, Encoding.Default))
                    {
                        Thread.Sleep(100);
                        if (!process.HasExited)
                            process.Kill();
                        string end = streamReader.ReadToEnd();
                        if (recordLog)
                            Trace.WriteLine(end);
                        return end;
                    }
                }
                finally
                {
                    streamReader?.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine((object)ex);
                return ex.Message;
            }
        }
    }
}
