using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    internal class ReceiptItemSendResult
    {
        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "result")]
        public bool result { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string errorCode { get; set; }
    }
}
