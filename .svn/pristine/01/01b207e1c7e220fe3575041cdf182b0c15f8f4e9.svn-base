using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Service.Rest
{
    public class ApiTokenData
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "validity")]
        public int Exprie { get; set; }

        [JsonProperty(PropertyName = "time")]
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
