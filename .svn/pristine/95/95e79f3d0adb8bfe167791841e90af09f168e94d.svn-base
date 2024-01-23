using Newtonsoft.Json;
using Org.BouncyCastle.Tsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Jobs
{
    internal class SendClientStateJob : AbstractLog
    {
        public void OnDoJob()
        {
            try
            {
                while (true)
                {
                    this.SendClientState();
                    Thread.Sleep(ServerCore.sysConfig.GetUploadStateInterval());
                }
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"上传客户端状态异常！", ex);
            }
        }

        public void SendClientState()
        {
            ServerCore.clientConfig = ClientConfig.LoadConfig();
            AbstractLog.logger.Info((object)"上传客户端状态中...");
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("MAC地址：" + ServerCore.MacAddress));
            string rawString = "";
            string str1 = "2";
            if (ServerCore.sendClientState != null && ServerCore.sendClientState.GetIsUploadLog())
            {
                FileInfo[] files = new DirectoryInfo(ServerCore.LogFileDirectory).GetFiles("*");
                Array.Sort((Array)files, (IComparer)new FileLastWritedTimeComparer());
                string str2 = "";
                int uploadLines = ServerCore.sendClientState.GetUploadLines();
                for (int index = 0; index < files.Length; ++index)
                {
                    if (files[index].FullName.IndexOf(".log") > -1)
                    {
                        str2 = FileHelper.readLastLine(files[index].FullName, uploadLines, Encoding.UTF8);
                        break;
                    }
                }
                string str3 = FileHelper.readLastLine(Path.Combine(ServerCore.SysFileDirectory, ServerCore.UpdateLogFileName), uploadLines, Encoding.UTF8);
                rawString = str2 + "\n以下为更新日志\n" + str3;
                if (rawString.Length > ServerCore.sysConfig.GetHttpGzipMinsize())
                {
                    rawString = CGZipUtil.GetStringByDataset(rawString);
                    str1 = "1";
                }
                AbstractLog.logger.Info((object)("即将上传" + (object)uploadLines + "行日志..."));
                ServerCore.sendClientState.sendClientStateContent.uploadLog = "2";
            }
            string sendClientStateUrl = this.GetSendClientStateUrl();
            int num = ServerCore.ReportScanedSum - ServerCore.ReportSendFailSum - ServerCore.ReportSendingCount;
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("ip", ServerCore.LocalIP),
                new RequestParam("macAddress", ServerCore.MacAddress),
                new RequestParam("version", ServerCore.GetVersion()),
                new RequestParam("login", ServerCore.isLogin ? "1" : "2"),
                new RequestParam("sendTotalMessage", num.ToString()),
                new RequestParam("receiveMessage", ServerCore.ReceiptWritedSum.ToString()),
                new RequestParam("sendFailureMessage", ServerCore.ReportSendFailSum.ToString()),
                new RequestParam("pushFailureMessage", ServerCore.ReceiptPlusFaillSum.ToString()),
                new RequestParam("entId", ServerCore.userData.szEntId),
                new RequestParam("logs", rawString),
                new RequestParam("zip", str1)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string str4;
            try
            {
                str4 = HttpHelper.HttpPostForType2(sendClientStateUrl, urlEncodePostStr);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AbstractLog.logger.Error((object)("状态上传（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）异常！"), ex);
                return;
            }
            stopwatch.Stop();
            AbstractLog.logger.Info((object)("本次上传状态耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
            SendClientStateResult jsonResult;
            try
            {
                jsonResult = JsonConvert.DeserializeObject<SendClientStateResult>(str4);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("状态上传接口返回结果序列化异常！结果：" + str4), ex);
                return;
            }
            if (jsonResult.result)
            {
                ServerCore.sendClientState = jsonResult;
                AbstractLog.logger.Info((object)jsonResult.description);
                //this.UpdateMonitor(jsonResult);
            }
            else
                AbstractLog.logger.Info((object)("状态上传模块异常(errorCode:" + jsonResult.errorCode + "):" + jsonResult.description));
        }

        private void UpdateMonitor(SendClientStateResult jsonResult)
        {
            if (!jsonResult.GetIsUpdateMonitor() || string.IsNullOrWhiteSpace(jsonResult.GetUpdateMonitorUrl()) || ServerCore.isUpdating)
                return;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Equals("Monitor.exe".Replace(".exe", ""), StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                    break;
                }
            }
            AbstractLog.logger.Info((object)"监控程序已暂时关闭，即将进行更新！");
            string fileName = Path.Combine(ServerCore.SysFileDirectory, "SZCBTServer.Monitor.exe");
            string str1 = Path.Combine(ServerCore.SysFileDirectory, ServerCore.MonitorUpdateTempDirectory);
            if (!Directory.Exists(str1))
                Directory.CreateDirectory(str1);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string str2 = Path.Combine(str1, "Monitor.exe".Replace(".exe", ".zip"));
            stopwatch.Stop();
            AbstractLog.logger.Info((object)("监控更新下载共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
            AbstractLog.logger.Info((object)"准备检查更新监控程序！");
            try
            {
                WebClient webClient = (WebClient)null;
                try
                {
                    webClient = new WebClient();
                    webClient.DownloadFileAsync(new Uri(jsonResult.GetUpdateMonitorUrl()), str2);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("监控程序下载异常!" + LogHelper.GetAttributesNValueFromObject((object)jsonResult, nameof(jsonResult))), ex);
                }
                finally
                {
                    webClient?.Dispose();
                }
                while (IOHelper.CheakFileIsWriting(str2))
                    Thread.Sleep(500);
                ZipUtil.UnZip(str2, str1);
                if (System.IO.File.Exists(Path.Combine(str1, "Monitor.exe")))
                {
                    AbstractLog.logger.Info((object)("监控程序更新文件:" + fileName));
                    DateTime lastWriteTime1 = new FileInfo(fileName).LastWriteTime;
                    FileHelper.MoveAndReplace(Path.Combine(str1, "SZCBTServer.Monitor.exe"), ServerCore.SysFileDirectory);
                    DateTime lastWriteTime2 = new FileInfo(fileName).LastWriteTime;
                    if (Directory.Exists(str1))
                    {
                        if (System.IO.File.Exists(str2))
                            System.IO.File.Delete(str2);
                        Directory.Delete(str1);
                    }
                    AbstractLog.logger.Info((object)("监控程序(版本更新日期" + (object)lastWriteTime1 + "-->" + (object)lastWriteTime2 + ")更新完成:" + fileName));
                }
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("监控程序更新异常!" + LogHelper.GetAttributesNValueFromObject((object)jsonResult, nameof(jsonResult))), ex);
            }
            finally
            {
                AbstractLog.logger.Info((object)"监控程序重启完毕！");
            }
        }

        private string GetSendClientStateUrl()
        {
            if (ServerCore.IsTestModel)
                return "https://localhost:8080/shenzhendtsp_web/client/declare/sendClientState.action";
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/sendClientState.action" : cloudServicesUrl + "/client/declare/sendClientState.action") : cloudServicesUrl + "/client/declare/sendClientState.action";
        }
    }
}
