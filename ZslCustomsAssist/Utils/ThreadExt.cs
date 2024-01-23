using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class ThreadExt
    {
        public Thread Thread { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }
}
