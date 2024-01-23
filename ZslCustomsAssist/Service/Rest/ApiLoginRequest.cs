using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Service.Rest
{
    public class ApiLoginRequest
    {
        [JsonProperty(PropertyName = "appid")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "appsecret")]
        public string AppSecret { get; set; }
    }
}
