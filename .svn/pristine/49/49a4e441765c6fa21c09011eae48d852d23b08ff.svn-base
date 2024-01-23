using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;

namespace ZslCustomsAssist.Jobs
{
    internal class CusReceiptPushHandle
    {
        public static List<CusReceiptPushJob> receiptList = new List<CusReceiptPushJob>();

        public static void OnDoHandle(CusReceiptPushJob receipt)
        {
            if (!receipt.SaveAsReceiptFile())
                return;
            CusReceiptPushHandle.JoinToDoReceiptList(receipt);
        }

        public static void JoinToDoReceiptList(CusReceiptPushJob newReceipt) => CusReceiptPushHandle.receiptList.Add(newReceipt);

        private static void DebugInfo(string debugMsg, string invNo = "")
        {
            if (!ServerCore.IsDebugModel)
                return;
            if (invNo != "")
                debugMsg = "【" + invNo + "】" + debugMsg;
            Console.WriteLine(debugMsg);
        }
    }
}
