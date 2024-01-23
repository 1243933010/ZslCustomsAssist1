using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class StringLengthComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            string fullName1 = ((FileSystemInfo)x).FullName;
            string fullName2 = ((FileSystemInfo)y).FullName;
            if (fullName1.Length > fullName2.Length)
                return -1;
            return fullName1.Length < fullName2.Length ? 1 : 0;
        }
    }
}
