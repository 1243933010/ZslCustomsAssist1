using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class XmlSignType
    {
        [JsonProperty(PropertyName = "signName")]
        public string signName { get; set; }

        [JsonProperty(PropertyName = "encryptorIP1")]
        public string encryptorIP1 { get; set; }

        [JsonProperty(PropertyName = "encryptorPort1")]
        public int encryptorPort1 { get; set; }

        [JsonProperty(PropertyName = "encryptorKeyIndex1")]
        public int encryptorKeyIndex1 { get; set; }

        [JsonProperty(PropertyName = "encryptorIP2")]
        public string encryptorIP2 { get; set; }

        [JsonProperty(PropertyName = "encryptorPort2")]
        public int encryptorPort2 { get; set; }

        [JsonProperty(PropertyName = "encryptorKeyIndex2")]
        public int encryptorKeyIndex2 { get; set; }
    }
}
