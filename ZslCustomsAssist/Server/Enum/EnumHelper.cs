using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Enum
{
    public static class EnumHelper
    {
        public static string EnumNameString(System.Enum tag_enum) => System.Enum.GetName(tag_enum.GetType(), (object)tag_enum);
    }
}
