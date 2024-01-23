using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    internal class UnifiedDeclareResult
    {
        [JsonProperty(PropertyName = "time")]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "returnCode")]
        public int ReturnCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "busiSign")]
        public string BusiSign { get; set; }

        [JsonProperty(PropertyName = "dxpSign")]
        public string DxpSign { get; set; }

        [JsonProperty(PropertyName = "signType")]
        public string SignType { get; set; }

        [JsonProperty(PropertyName = "dxpMessage")]
        public string DxpMessage { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<object> data { get; set; }
    }
}
