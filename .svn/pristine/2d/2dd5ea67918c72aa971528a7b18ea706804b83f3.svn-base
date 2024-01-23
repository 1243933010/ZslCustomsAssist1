using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class DataResourceType : ICloneable
    {
        public string classificationCode { get; set; }

        public string classificationName { get; set; }

        public List<DataResource> dataResource { get; set; }

        public object Clone() => MemberwiseClone();
    }
}
