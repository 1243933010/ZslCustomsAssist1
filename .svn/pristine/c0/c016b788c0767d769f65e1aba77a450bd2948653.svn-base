using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class FileLastWritedTimeComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            DateTime lastWriteTime1 = ((FileSystemInfo)x).LastWriteTime;
            DateTime lastWriteTime2 = ((FileSystemInfo)y).LastWriteTime;
            if (lastWriteTime1 > lastWriteTime2)
                return -1;
            return lastWriteTime1 < lastWriteTime2 ? 1 : 0;
        }
    }
}
