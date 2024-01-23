using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Server.Rest
{
    [JsonObject(MemberSerialization.OptOut)]
    public class QueryDataResourcesResult : AbstractLog
    {
        public string description { get; set; }

        public bool result { get; set; }

        public string errorCode { get; set; }

        [JsonProperty("content")]
        public QueryDataResources queryDataResources { get; set; }
    }
}
