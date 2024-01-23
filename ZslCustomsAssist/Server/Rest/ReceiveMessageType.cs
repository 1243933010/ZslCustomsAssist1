﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class ReceiveMessageType
    {
        [JsonProperty(PropertyName = "hasTransferSign")]
        public string hasTransferSign { get; set; }

        [JsonProperty(PropertyName = "hasSign")]
        public string hasSign { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string code { get; set; }

        [JsonProperty(PropertyName = "sendMessageTypes")]
        public List<SendMessageType> sendMessageTypes { get; set; }
    }
}
