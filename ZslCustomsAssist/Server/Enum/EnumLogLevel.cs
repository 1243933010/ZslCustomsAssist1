using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Enum
{
    public enum EnumLogLevel
    {
        [Description("正常")] Normal = 1,
        [Description("重要")] Important = 2,
        [Description("错误")] Error = 3,
    }
}
