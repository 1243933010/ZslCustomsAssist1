using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    internal class ReceiptItemToSend
    {
        [JsonProperty(PropertyName = "receiptId")]
        public string receiptId { get; set; }
    }
}
