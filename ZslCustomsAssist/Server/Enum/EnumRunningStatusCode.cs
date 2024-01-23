using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Enum
{
    public enum EnumRunningStatusCode
    {
        [Description("已登录")] Login = 1,
        [Description("未登录")] UnLogin = 2,
    }
}
