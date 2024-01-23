using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class TransportProcotol
    {
        [JsonProperty(PropertyName = "tpName")]
        public string tpName { get; set; }

        [JsonProperty(PropertyName = "mqHostName")]
        public string mqHostName { get; set; }

        [JsonProperty(PropertyName = "mqChannel")]
        public string mqChannel { get; set; }

        [JsonProperty(PropertyName = "mqPort")]
        public int mqPort { get; set; }

        [JsonProperty(PropertyName = "mqQueueManagerName")]
        public string mqQueueManagerName { get; set; }

        [JsonProperty(PropertyName = "mqSendQueueName")]
        public string mqSendQueueName { get; set; }

        public int mqSendQueueNum { get; set; }

        [JsonProperty(PropertyName = "mqReceiveQueueName")]
        public string mqReceiveQueueName { get; set; }

        public int mqReceiveQueueNum { get; set; }

        [JsonProperty(PropertyName = "httpsURL")]
        public string httpsURL { get; set; }

        [JsonProperty(PropertyName = "httpsVisitType")]
        public string httpsVisitType { get; set; }
    }
}
