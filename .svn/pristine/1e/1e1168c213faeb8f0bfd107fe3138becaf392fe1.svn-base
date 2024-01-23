using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Runtime.Config;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    internal class GetSystemConfigJob : AbstractLog
    {
        public void OnDoJob()
        {
            while (!ServerCore.IsExitThread)
            {
                try
                {
                    this.GetSystemConfig();
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"下载系统配置线程异常：", ex);
                }
                finally
                {
                    Thread.Sleep(ServerCore.sysConfig.GetClientSystemConfigInterval());
                }
            }
            //AbstractLog.logger.Info((object)"已结束本次下载系统配置！");
        }

        public bool GetSystemConfig()
        {
            string Url = ServerCore.CloudServicesUrl + "app/json/loadClientSysConfig.action";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                string str = HttpHelper.HttpPostForType2(Url, "");
                string allowSendFileStart1 = ServerCore.sysConfig.GetClientScanAllowSendFileStart();
                string allowSendFileEnd1 = ServerCore.sysConfig.GetClientScanAllowSendFileEnd();
                ServerCore.sysConfig = JsonConvert.DeserializeObject<SysConfig>(str);
                string allowSendFileStart2 = ServerCore.sysConfig.GetClientScanAllowSendFileStart();
                if (allowSendFileStart1 != allowSendFileStart2)
                {
                    lock (ServerCore.scanLock)
                        ServerCore.scanAllowSendFileStart = new List<string>((IEnumerable<string>)allowSendFileStart2.Split(','));
                }
                string allowSendFileEnd2 = ServerCore.sysConfig.GetClientScanAllowSendFileEnd();
                if (allowSendFileEnd1 != allowSendFileEnd2)
                {
                    lock (ServerCore.scanLock)
                        ServerCore.scanAllowSendFileStart = new List<string>((IEnumerable<string>)allowSendFileEnd2.Split(','));
                }
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AbstractLog.logger.Error((object)("云平台连接（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）异常！"), ex);
                return false;
            }
        }
    }
}
