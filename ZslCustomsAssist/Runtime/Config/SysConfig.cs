using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils;

namespace ZslCustomsAssist.Runtime.Config
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SysConfig
    {
        [JsonIgnore]
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        public string uploadStateInterval { get; set; }

        public string clearTimeOutFileInterval { get; set; }

        public string logsKeepDays { get; set; }

        public string receivedReportKeepDays { get; set; }

        public string reportSendFailKeepDays { get; set; }

        public string reportSendEmptyDirKeepDays { get; set; }

        public string updateInterval { get; set; }

        public string synchronousConfigurationInterval { get; set; }

        public string bottomLink { get; set; }

        public string bottomLinkDescrption { get; set; }

        public string clientSystemConfigInterval { get; set; }

        public string httpGzipMinsize { get; set; }

        public string clientDataQueryPagesize { get; set; }

        public string clientDataQueryThreadCount { get; set; }

        public string clientDataQueryDisclaimer { get; set; }

        public string clientScanAllowSendFileStart { get; set; }

        public string clientScanAllowSendFileEnd { get; set; }

        public int GetUploadStateInterval()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(uploadStateInterval));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                uploadStateInterval = "300000";
                return 300000;
            }
        }

        public int GetLogsKeepDays()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(logsKeepDays));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                logsKeepDays = "7";
                return 7;
            }
        }

        public int GetReceivedReportKeepDays()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(receivedReportKeepDays));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                receivedReportKeepDays = "7";
                return 7;
            }
        }

        public int GetReportSendFailKeepDays()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(reportSendFailKeepDays));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                reportSendFailKeepDays = "7";
                return 7;
            }
        }

        public int GetReportSendEmptyDirKeepDays()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(reportSendEmptyDirKeepDays));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                reportSendEmptyDirKeepDays = "7";
                return 7;
            }
        }

        public int GetClearTimeOutFileInterval()
        {
            try
            {
                return DateHelper.getMillisecond(DateHelper.GetUInt(int.Parse(clearTimeOutFileInterval)));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                clearTimeOutFileInterval = string.Concat(DateHelper.getMillisecond(24, "h"));
                return DateHelper.getMillisecond(24, "h");
            }
        }

        public int GetSynchronousConfigurationInterval()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(synchronousConfigurationInterval));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                synchronousConfigurationInterval = string.Concat(DateHelper.getMillisecond(10, "s"));
                return DateHelper.getMillisecond(10, "s");
            }
        }

        public int GetUpdateInterval()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(updateInterval));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                updateInterval = string.Concat(DateHelper.getMillisecond(10));
                return DateHelper.getMillisecond(10);
            }
        }

        public string GetBottomLink() => string.IsNullOrWhiteSpace(bottomLink) ? "" : bottomLink;

        public string GetBottomLinkDescrption()
        {
            if (string.IsNullOrWhiteSpace(bottomLinkDescrption))
                return "";
            return bottomLinkDescrption.Length > 65 ? bottomLinkDescrption.Substring(0, 65) + "..." : bottomLinkDescrption;
        }

        public int GetClientSystemConfigInterval()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(clientSystemConfigInterval));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                clientSystemConfigInterval = string.Concat(DateHelper.getMillisecond(10, "s"));
                return DateHelper.getMillisecond(10, "s");
            }
        }

        public int GetHttpGzipMinsize()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(httpGzipMinsize));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                httpGzipMinsize = "2147483647";
                return int.MaxValue;
            }
        }

        public int GetClientDataQueryPagesize()
        {
            int dataQueryPagesize = 15;
            try
            {
                return DateHelper.GetUInt(int.Parse(clientDataQueryPagesize));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                clientDataQueryPagesize = string.Concat(dataQueryPagesize);
                return dataQueryPagesize;
            }
        }

        public int GetClientDataQueryThreadCount()
        {
            int queryThreadCount = 2;
            try
            {
                return DateHelper.GetUInt(int.Parse(clientDataQueryThreadCount));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                clientDataQueryThreadCount = string.Concat(queryThreadCount);
                return queryThreadCount;
            }
        }

        public string GetClientDataQueryDisclaimer() => string.IsNullOrWhiteSpace(clientDataQueryDisclaimer) ? "查询结果仅供参考，不保证所提供的资料和服务没有任何瑕疵，海关对公布内容拥有修改和解释权，用户应以实际通关、办理海关手续时相关海关的解释和要求为准!" : clientDataQueryDisclaimer;

        public string GetClientScanAllowSendFileStart()
        {
            if (string.IsNullOrWhiteSpace(clientScanAllowSendFileStart))
                return "SZCPORT";
            return clientScanAllowSendFileStart == "*" ? null : clientScanAllowSendFileStart;
        }

        public string GetClientScanAllowSendFileEnd()
        {
            if (string.IsNullOrWhiteSpace(clientScanAllowSendFileEnd))
                return "xml,zip,pdf";
            return clientScanAllowSendFileEnd == "*" ? null : clientScanAllowSendFileEnd;
        }
    }
}
