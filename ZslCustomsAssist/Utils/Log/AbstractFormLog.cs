using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils.Log
{
    public class AbstractFormLog : Form
    {
        public static ILog logger;

        public AbstractFormLog() => logger = LogManager.GetLogger("Log4NetTest.LogTest");
    }
}
