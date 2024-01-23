using Apache.NMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.MQ
{
    internal class HwsbReceiptHandle
    {
        public static List<HwsbReceiptPushJob> receiptList = new List<HwsbReceiptPushJob>();

        public static void OnDoHandle(IMessage mqMessage)
        {
            HwsbReceiptPushJob instanceByMqMessage = HwsbReceiptPushJob.GetInstanceByMqMessage((ITextMessage)mqMessage);
            if (!instanceByMqMessage.IsValid)
            {
                HwsbReceiptHandle.DebugInfo("数据协议不合法，请确保关键数据项：MessageID、MessageType、SendTime、Version是合法的。");
                instanceByMqMessage.SaveInvalidReceiptAsFile();
                mqMessage.Acknowledge();
            }
            else
            {
                HwsbReceiptHandle.DebugInfo("得到的报文回执内容：" + JsonConvert.SerializeObject((object)instanceByMqMessage), instanceByMqMessage.MessageID);
                if (!instanceByMqMessage.SaveAsReceiptFile())
                    return;
                HwsbReceiptHandle.JoinToDoReceiptList(instanceByMqMessage);
                mqMessage.Acknowledge();
            }
        }

        public static void JoinToDoReceiptList(HwsbReceiptPushJob newReceipt)
        {
            bool flag = false;
            foreach (HwsbReceiptPushJob receipt in HwsbReceiptHandle.receiptList)
            {
                if (receipt.MessageID == newReceipt.MessageID)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
                return;
            HwsbReceiptHandle.receiptList.Add(newReceipt);
        }

        private static void DebugInfo(string debugMsg, string invNo = "")
        {
            AbstractLog.logger.Info((object)debugMsg);
            if (!ServerCore.IsDebugModel)
                return;
            if (invNo != "")
                debugMsg = "【" + invNo + "】" + debugMsg;
            Console.WriteLine(debugMsg);
        }
    }
}
