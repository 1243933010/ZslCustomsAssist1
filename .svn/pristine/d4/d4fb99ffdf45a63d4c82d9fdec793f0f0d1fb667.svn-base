using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Server.Rest.HWSB;

namespace ZslCustomsAssist.Server.Rest
{
    public class HWSBResult
    {
        [JsonProperty(PropertyName = "time")]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "returnCode")]
        public int ReturnCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string[] Message { get; set; }

        [JsonProperty(PropertyName = "invNo")]
        public string InvNo { get; set; }

        [JsonProperty(PropertyName = "invPassword")]
        public string InvPassword { get; set; }

        [JsonProperty(PropertyName = "bill")]
        public object Bill { get; set; }

        public string invokeResult { get; set; }

        [JsonProperty(PropertyName = "mqConfigs")]
        public List<HwsbMqConfig> MqConfigs { get; set; }

        public string GetErrorMessage()
        {
            if (Message == null)
                return "";
            string errorMessage = "";
            foreach (string str in Message)
            {
                if (errorMessage != "")
                    errorMessage += ";";
                errorMessage += str;
            }
            return errorMessage;
        }
    }
}
