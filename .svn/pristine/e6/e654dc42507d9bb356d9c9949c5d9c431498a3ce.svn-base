using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils.Http;
using System.ServiceProcess;
using Application = System.Windows.Forms.Application;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Server.Rest;

namespace ZslCustomsAssist.SwagentProxy
{
    public class SwagentProxy
    {
        private readonly string swagentDir = Environment.CurrentDirectory + "\\swagent\\";
        private readonly string swsdsDir = Environment.CurrentDirectory + "\\Libs\\swsds\\";

        public void InitPublic(XmlSignType signType)
        {
            SwsIniFile swsIniFile = new SwsIniFile(this.swagentDir + "swagent.ini");
            swsIniFile.SetIniValue("VSM1", "ip", signType.encryptorIP1);
            swsIniFile.SetIniValue("VSM1", "port", signType.encryptorPort1.ToString());
            swsIniFile.SetIniValue("VSM2", "ip", signType.encryptorIP2);
            swsIniFile.SetIniValue("VSM2", "port", signType.encryptorPort2.ToString());
            swsIniFile.SetIniValue("Log", "logfile", this.swagentDir + "swagent.log");
            swsIniFile.SetIniValue("Auth", "cert", this.swagentDir + ServerCore.userData.szCardID + ".pfx");
            this.SetSwsdsIni("127.0.0.1", 8008);
            ServerCore.privateKeyIndex = signType.encryptorKeyIndex1;
            swsIniFile.GetIniValue("Auth", "cert");
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("macAddress", ServerCore.MacAddress)
            };
            paramList.Add(new RequestParam("certType", "pfx"));
            string urlEncodePostStr1 = ServerCore.GetUrlEncodePostStr(paramList);
            string path1 = this.swagentDir + ServerCore.userData.szCardID + ".pfx";
            HttpHelper.HttpPostAndDownloadFile(this.GetHttpsUrl(), urlEncodePostStr1, path1);
            paramList.RemoveAt(paramList.Count - 1);
            paramList.Add(new RequestParam("certType", "cer"));
            string urlEncodePostStr2 = ServerCore.GetUrlEncodePostStr(paramList);
            string path2 = Application.StartupPath + "\\" + ServerCore.userData.szCardID + ".cer";
            HttpHelper.HttpPostAndDownloadFile(this.GetHttpsUrl(), urlEncodePostStr2, path2);
            this.CheckAgentService();
        }

        private void SetSwsdsIni(string ip, int port)
        {
            SwsIniFile swsIniFile = new SwsIniFile(this.swsdsDir + "swsds.ini");
            swsIniFile.SetIniValue("HSM1", nameof(ip), ip);
            swsIniFile.SetIniValue("HSM1", nameof(port), port.ToString());
        }

        public void InitPrivate(XmlSignType signType)
        {
            SwsIniFile swsIniFile = new SwsIniFile(this.swsdsDir + "swsds.ini");
            swsIniFile.SetIniValue("HSM1", "ip", signType.encryptorIP1);
            swsIniFile.SetIniValue("HSM1", "port", signType.encryptorPort1.ToString());
            swsIniFile.SetIniValue("ErrorLog", "logfile", this.swsdsDir + "swsds.log");
            ServerCore.privateKeyIndex = signType.encryptorKeyIndex1;
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("macAddress", ServerCore.MacAddress)
            };
            paramList.Add(new RequestParam("certType", "cer"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            string path = Application.StartupPath + "\\" + ServerCore.userData.szCardID + ".cer";
            HttpHelper.HttpPostAndDownloadFile(this.GetHttpsUrl(), urlEncodePostStr, path);
        }

        public void CheckAgentService()
        {
            if (this.CheckServiceRunning("SWagent"))
                return;
            this.StartBat(this.swagentDir, "install.bat");
            if (this.CheckServiceRunning("SWagent"))
                return;
            int num = (int)MessageBox.Show("尝试启动云加签SWagent服务失败（请进入客户端安装目录swagent下运行install.bat手动启动或重新登录客户端），并确保SWAgent服务已启动或正在运行，客户端即将退出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            AbstractLog.logger.Error((object)"尝试启动云加签SWagent服务失败（请进入客户端安装目录swagent下运行install.bat手动启动或重新登录客户端），并确保SWAgent服务已启动或正在运行，客户端即将退出！");

            if (!ServerCore.IsWithOutCard)
                SPSecureAPI.SpcClearEnv();
        }

        private void StartBat(string dir, string fileName)
        {
            Process process = new Process();
            process.StartInfo.WorkingDirectory = dir;
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            Thread.Sleep(5000);
        }

        private bool CheckServiceRunning(string serviceName)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName.ToUpper().Equals(serviceName.ToUpper()))
                {
                    service.Refresh();
                    if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                        return true;
                    service.Start();
                    Thread.Sleep(5000);
                    service.Refresh();
                    if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                    {
                        int num = (int)MessageBox.Show("启动云加签SWagent服务失败（请进入客户端安装目录swagent下运行install.bat手动启动或重新登录客户端），并确保SWAgent服务已启动或正在运行，客户端即将退出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        AbstractLog.logger.Error((object)"启动云加签SWagent服务失败（请进入客户端安装目录swagent下运行install.bat手动启动或重新登录客户端），并确保SWAgent服务已启动或正在运行，客户端即将退出！");
                        ServerCore.IsExitThread = true;
                    }
                    return true;
                }
            }
            return false;
        }

        public static void UpdateSignType(string oldSignVersion, SwagentProxy proxy)
        {
            List<XmlSignType> xmlSignTypes = ServerCore.downloadConfig.xmlSignTypes;
            string xmlSignVersion = ServerCore.downloadConfig.xmlSignVersion;
            List<string> stringList = new List<string>()
            {
                "publicEncryptor",
                "privateEncryptor"
            };
            if (xmlSignTypes != null && xmlSignTypes.Count > 0)
            {
                XmlSignType signType = xmlSignTypes[0];
                if (!stringList.Contains(signType.signName))
                {
                    ServerCore.supportedSignType = "ICCard";
                    AbstractLog.logger.Error((object)"加签方式不符合规范要求,已使用默认IC卡加签方式!");
                }
                else if (string.IsNullOrWhiteSpace(oldSignVersion))
                {
                    if ("publicEncryptor".Equals(signType.signName))
                        proxy.InitPublic(signType);
                    else
                        proxy.InitPrivate(signType);
                    ServerCore.supportedSignType = signType.signName;
                }
                else
                {
                    if (xmlSignVersion.Equals(oldSignVersion))
                        return;
                    if ("publicEncryptor".Equals(signType.signName))
                        proxy.InitPublic(signType);
                    else
                        proxy.InitPrivate(signType);
                    ServerCore.supportedSignType = signType.signName;
                }
            }
            else
                ServerCore.supportedSignType = "ICCard";
        }

        private string GetHttpsUrl()
        {
            if (ServerCore.IsTestModel)
                return "http://localhost:8080/shenzhendtsp_web/client/declare/signCertDownload.action";
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/signCertDownload.action" : cloudServicesUrl + "/client/declare/signCertDownload.action") : cloudServicesUrl + "/client/declare/signCertDownload.action";
        }
    }
}
