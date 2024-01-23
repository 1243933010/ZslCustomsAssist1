using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;

namespace ZslCustomsAssist.Server.Rest
{
    public class SendClientStateResult : AbstractLog
    {
        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "result")]
        public bool result { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string errorCode { get; set; }

        [JsonProperty(PropertyName = "content")]
        public SendClientStateContent sendClientStateContent { get; set; }

        public SendClientStateContent GetSendClientStateContent() => sendClientStateContent == null ? new SendClientStateContent() : sendClientStateContent;

        public int GetUploadLines()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(GetSendClientStateContent().uploadLines));
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
                return 0;
            }
        }

        public bool GetIsUploadLog()
        {
            try
            {
                if ("1".Equals(GetSendClientStateContent().uploadLog))
                    return true;
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
            }
            return false;
        }

        public bool GetIsUpdateMonitor()
        {
            try
            {
                if ("1".Equals(GetSendClientStateContent().updateMonitor))
                    return true;
            }
            catch (Exception ex)
            {
                if (!ServerCore.isLogin)
                    ;
            }
            return false;
        }

        public string GetUpdateMonitorUrl() => string.IsNullOrWhiteSpace(GetSendClientStateContent().updateUrl) ? "" : GetSendClientStateContent().updateUrl;
    }
}
