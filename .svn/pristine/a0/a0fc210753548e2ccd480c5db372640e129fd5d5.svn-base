using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Jobs
{
    internal class SynchronousConfigurationJob : AbstractLog
    {
        public void OnDoJob()
        {
            while (!ServerCore.IsExitThread)
            {
                Thread.Sleep(ServerCore.sysConfig.GetSynchronousConfigurationInterval());
                try
                {
                    if (!this.SynchronousConfiguration())
                    {
                        ServerCore.IsExitThread = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"自动同步客户端异常,可能会导致报文收发功能异常！", ex);
                }
                //AbstractLog.logger.Info((object)"已结束本次同步客户配置！");
            }
        }

        public bool SynchronousConfigurationStart()
        {
            ServerCore.clientConfig = ClientConfig.LoadConfig();
            //AbstractLog.logger.Info((object)"同步企业配置中...");
            if (string.IsNullOrWhiteSpace(ServerCore.userData.szCardID) && ServerCore.IsWithOutCard)
                ServerCore.userData.szCardID = ServerCore.clientConfig.IcNo;
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("MAC地址：" + ServerCore.MacAddress));
            string configurationUrl = this.GetSynchronousConfigurationUrl();
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("ip", ServerCore.LocalIP),
                new RequestParam("macAddress", ServerCore.MacAddress)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string str;
            try
            {
                str = HttpHelper.HttpPostForType2(configurationUrl, urlEncodePostStr);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("同步配置（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）失败,可能会导致报文收发功能异常！"), ex);
                throw new Exception("同步配置失败,可能会导致报文收发功能异常！");
            }
            stopwatch.Stop();
            AbstractLog.logger.Info((object)("本次同步配置共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
            if (string.IsNullOrWhiteSpace(str))
            {
                AbstractLog.logger.Info((object)"同步配置接口响应结果为空，接口请求异常!");
                throw new Exception("同步配置接口响应结果为空，接口请求异常!");
            }
            DownloadConfigResult downloadConfigResult;
            try
            {
                downloadConfigResult = JsonConvert.DeserializeObject<DownloadConfigResult>(str);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("同步配置接口返回结果序列化异常！结果：" + str), ex);
                throw new Exception("同步配置接口返回结果序列化异常！结果：" + str, ex);
            }
            if (downloadConfigResult.result)
            {
                //AbstractLog.logger.Info((object)"同步配置成功！");
                string tranVersion = ServerCore.downloadConfig.tranVersion;
                string xmlSignVersion = ServerCore.downloadConfig.xmlSignVersion;
                ServerCore.downloadConfig = downloadConfigResult.downloadConfig;
                /*try
                {
                    IbmMQQueueProxy.UpdateTransportProcotol(tranVersion, ServerCore.mqSendQueueProxy, ServerCore.mqReceiveQueueProxy);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"MQ连接异常,即将退出！", ex);
                    int num = (int)MessageBox.Show("MQ连接失败，原因如下：\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    new ExitProgramJob().QuitApplication();
                    return false;
                }
                try
                {
                    SwagentProxy.SwagentProxy.UpdateSignType(xmlSignVersion, ServerCore.swagentProxy);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"网络连接异常（加签配置同步失败）,即将退出！", ex);
                    int num = (int)MessageBox.Show("网络连接异常（加签配置同步失败），原因如下：\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    new ExitProgramJob().QuitApplication();
                    return false;
                }*/
                return true;
            }
            ServerCore.clientConfig.TypistPassword = (string)null;
            IOHelper.InputConfigFile(ServerCore.clientConfig);
            AbstractLog.logger.Error((object)("同步配置失败(" + downloadConfigResult.errorCode + ")：" + downloadConfigResult.description));
            int num1 = (int)MessageBox.Show("同步配置失败，原因(" + downloadConfigResult.errorCode + ")如下：\n" + downloadConfigResult.description, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        public bool SynchronousConfiguration()
        {
            ServerCore.clientConfig = ClientConfig.LoadConfig();
            //AbstractLog.logger.Info((object)"同步企业配置中...");
            if (string.IsNullOrWhiteSpace(ServerCore.userData.szCardID) && ServerCore.IsWithOutCard)
                ServerCore.userData.szCardID = ServerCore.clientConfig.IcNo;
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("MAC地址：" + ServerCore.MacAddress));
            string configurationUrl = this.GetSynchronousConfigurationUrl();
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("ip", ServerCore.LocalIP),
                new RequestParam("macAddress", ServerCore.MacAddress)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string str;
            try
            {
                str = HttpHelper.HttpPostForType2(configurationUrl, urlEncodePostStr);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("同步配置（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）失败,可能会导致报文收发功能异常！"), ex);
                throw new Exception("同步配置失败,可能会导致报文收发功能异常！");
            }
            stopwatch.Stop();
            //AbstractLog.logger.Info((object)("本次同步配置共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
            if (string.IsNullOrWhiteSpace(str))
            {
                AbstractLog.logger.Info((object)"同步配置接口响应结果为空，接口请求异常!");
                throw new Exception("同步配置接口响应结果为空，接口请求异常!");
            }
            DownloadConfigResult downloadConfigResult;
            try
            {
                downloadConfigResult = JsonConvert.DeserializeObject<DownloadConfigResult>(str);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("同步配置接口返回结果序列化异常！结果：" + str), ex);
                throw new Exception("同步配置接口返回结果序列化异常！结果：" + str, ex);
            }
            if (downloadConfigResult.result)
            {
                //AbstractLog.logger.Info((object)"同步配置成功！");
                string tranVersion = ServerCore.downloadConfig.tranVersion;
                string xmlSignVersion = ServerCore.downloadConfig.xmlSignVersion;
                ServerCore.downloadConfig = downloadConfigResult.downloadConfig;
                /*try
                {
                    IbmMQQueueProxy.UpdateTransportProcotol(tranVersion, ServerCore.mqSendQueueProxy, ServerCore.mqReceiveQueueProxy);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"MQ连接异常,即将退出！", ex);
                    int num = (int)MessageBox.Show("MQ连接失败，原因如下：\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    new ExitProgramJob().QuitApplication();
                    return false;
                }
                try
                {
                    SwagentProxy.SwagentProxy.UpdateSignType(xmlSignVersion, ServerCore.swagentProxy);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"网络连接异常（加签配置同步失败）,即将退出！", ex);
                    int num = (int)MessageBox.Show("网络连接异常（加签配置同步失败），原因如下：\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    new ExitProgramJob().QuitApplication();
                    return false;
                }*/
            }
            return true;
        }

        private string GetSynchronousConfigurationUrl()
        {
            if (ServerCore.IsTestModel)
                return "https://localhost:8080/shenzhendtsp_web/client/declare/receiveDeploy.action";
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/receiveDeploy.action" : cloudServicesUrl + "/client/declare/receiveDeploy.action") : cloudServicesUrl + "/client/declare/receiveDeploy.action";
        }
    }
}
