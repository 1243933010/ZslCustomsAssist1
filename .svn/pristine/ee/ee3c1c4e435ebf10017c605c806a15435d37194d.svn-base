using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class ReceiptContentItem
    {
        [JsonProperty(PropertyName = "receiptId")]
        public string receiptId { get; set; }

        [JsonProperty(PropertyName = "messageId")]
        public string messageId { get; set; }

        [JsonProperty(PropertyName = "messageType")]
        public string messageType { get; set; }

        [JsonProperty(PropertyName = "messageText")]
        public string messageText { get; set; }

        [JsonProperty(PropertyName = "messageFileSuffix")]
        public string messageFileSuffix { get; set; }

        [JsonProperty(PropertyName = "messageZiped")]
        public string messageZiped { get; set; }

        public bool GetMessageZiped() => !string.IsNullOrWhiteSpace(messageZiped) && "true".Equals(messageZiped.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
