using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Jobs;

namespace ZslCustomsAssist.Server.Rest
{
    public interface IServiceHandle
    {
        string GetCusResultParamsName();

        void SecurityValid(string userName, string passWord, string companyId);

        bool CusReceiptPush(IServiceHandle pushHandle, CusReceiptPushJob job);
    }
}
