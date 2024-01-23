using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Service.Rest
{
    internal class SendCallback
    {

        [JsonProperty(PropertyName = "signStr")]
        public string signStr { get; set; }

        [JsonProperty(PropertyName = "orderNo")]
        public string orderNo { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string type { get; set; }
    }
}
