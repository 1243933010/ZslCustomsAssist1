using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class QueryDataResources
    {
        public string remains { get; set; }

        public string messageId { get; set; }

        public string messageType { get; set; }

        public string messageFileSuffix { get; set; }

        public string messageZiped { get; set; }

        public string messageText { get; set; }

        public bool GetMessageZiped() => !string.IsNullOrWhiteSpace(this.messageZiped) && "true".Equals(this.messageZiped.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
