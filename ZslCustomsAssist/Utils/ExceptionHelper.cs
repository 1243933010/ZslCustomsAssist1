using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class ExceptionHelper
    {
        public static string GetErrorMsg(Exception ex) => "错误原因:" + ex.Message + "堆栈信息:" + ex.StackTrace;
    }
}
