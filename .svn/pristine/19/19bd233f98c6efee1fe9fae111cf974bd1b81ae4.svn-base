using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class ReceiptContentResult
    {
        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }

        [JsonProperty(PropertyName = "result")]
        public bool result { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string errorCode { get; set; }

        [JsonProperty(PropertyName = "content")]
        public List<ReceiptContentItem> receiptList { get; set; }
    }
}
