using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using Apache.NMS;

namespace ZslCustomsAssist.MQ
{
    internal class HwsbReceiptPushJob
    {
        public const string fileSuffix = ".hwsbReceipt";
        public const string invalidFileSuffix = ".hwsbInvalidReceipt";
        public const string InvalidReceiptDirectoryName = "InvalidReceipt";
        public const string DataFileSuffix = ".xml";
        public bool IsValid;

        public string MessageID { get; set; }

        public string Keyword { get; set; }

        public string FunctionCode { get; set; }

        public string MessageType { get; set; }

        public string SenderID { get; set; }

        public string ReceiverID { get; set; }

        public string SendTime { get; set; }

        public string Version { get; set; }

        public DateTime ReceiptDate { get; set; }

        public DateTime LastTimePushTime { get; set; }

        public int FailureCount { get; set; }

        public string ReceiptDateFilePath { get; set; }

        public string ReceiptContent { get; set; }

        public string GetValidReceiptDirectoryPath()
        {
            string path3 = this.Keyword == null ? "" : this.Keyword;
            return Path.Combine(ServerCore.clientConfig.ReceiptReceiveDir, this.MessageType, path3);
        }

        public string GetInvalidReceiptDirectoryPath() => Path.Combine(ServerCore.clientConfig.ReceiptReceiveDir, "InvalidReceipt");

        public string getLocalFileName()
        {
            string str = this.MessageID == null || !(this.MessageID != "") ? this.ReceiptDate.ToString("yyyyMMddHHmmssff") : this.MessageID;
            return !this.IsValid ? str + ".hwsbInvalidReceipt" : str + ".hwsbReceipt";
        }

        public string GetFileFullPath() => Path.Combine(this.GetValidReceiptDirectoryPath(), this.getLocalFileName());

        public static HwsbReceiptPushJob GetInstanceByMqMessage(ITextMessage mqMessage)
        {
            HwsbReceiptPushJob instanceByMqMessage = new HwsbReceiptPushJob();
            string str1 = mqMessage.Properties.GetString("MessageID");
            string str2 = mqMessage.Properties.GetString("FunctionCode");
            string str3 = mqMessage.Properties.GetString("MessageType");
            string str4 = mqMessage.Properties.GetString("SendTime");
            string str5 = mqMessage.Properties.GetString("SenderID");
            string str6 = mqMessage.Properties.GetString("ReceiverID");
            string str7 = mqMessage.Properties.GetString("Version");
            string str8 = mqMessage.Properties.GetString("Keyword");
            AbstractLog.logger.Info((object)"开始拿回执内容………………………………");
            AbstractLog.logger.Info((object)("得到MessageID:" + (str1 == null || str1 == "" ? "未找到" : str1)));
            AbstractLog.logger.Info((object)("得到FunctionCode:" + (str2 == null || str2 == "" ? "未找到" : str2)));
            AbstractLog.logger.Info((object)("得到MessageType:" + (str3 == null || str3 == "" ? "未找到" : str3)));
            AbstractLog.logger.Info((object)("得到SendTime:" + (str4 == null || str4 == "" ? "未找到" : str4)));
            AbstractLog.logger.Info((object)("得到SenderID:" + (str5 == null || str5 == "" ? "未找到" : str5)));
            AbstractLog.logger.Info((object)("得到ReceiverID:" + (str6 == null || str6 == "" ? "未找到" : str6)));
            AbstractLog.logger.Info((object)("得到Version:" + (str7 == null || str7 == "" ? "未找到" : str7)));
            AbstractLog.logger.Info((object)("得到Keyword:" + (str8 == null || str8 == "" ? "未找到" : str8)));
            if (str1 == null || str3 == null || str6 == null)
            {
                instanceByMqMessage.ReceiptContent = mqMessage.Text;
                instanceByMqMessage.ReceiptDate = DateTime.Now;
                instanceByMqMessage.IsValid = false;
                return instanceByMqMessage;
            }
            instanceByMqMessage.MessageID = str1;
            instanceByMqMessage.FunctionCode = str2;
            instanceByMqMessage.MessageType = str3;
            instanceByMqMessage.SenderID = str5;
            instanceByMqMessage.ReceiverID = str6;
            instanceByMqMessage.SendTime = str4;
            instanceByMqMessage.Version = str7;
            instanceByMqMessage.ReceiptContent = mqMessage.Text;
            instanceByMqMessage.ReceiptDate = DateTime.Now;
            instanceByMqMessage.Keyword = str8;
            instanceByMqMessage.IsValid = true;
            return instanceByMqMessage;
        }

        public bool SaveAsReceiptFile()
        {
            string receiptDirectoryPath = this.GetValidReceiptDirectoryPath();
            AbstractLog.logger.Info((object)("传输客户端系统回执存储目录：" + receiptDirectoryPath));
            string fileContent = JsonConvert.SerializeObject((object)this);
            string localFileName = this.getLocalFileName();
            return FileHelper.SaveAsFile(receiptDirectoryPath, localFileName, fileContent);
        }

        public bool SaveInvalidReceiptAsFile()
        {
            string receiptDirectoryPath = this.GetInvalidReceiptDirectoryPath();
            AbstractLog.logger.Info((object)("传输客户端系统无效回执存储目录：" + receiptDirectoryPath));
            string fileContent = JsonConvert.SerializeObject((object)this);
            string localFileName = this.getLocalFileName();
            return FileHelper.SaveAsFile(receiptDirectoryPath, localFileName, fileContent);
        }

        public bool IsCanPush(int spaceSeconds)
        {
            DateTime t1 = this.ReceiptDate;
            t1 = t1.AddSeconds((double)spaceSeconds);
            return DateTime.Compare(t1, DateTime.Now) == -1;
        }

        public bool DeleteLocalFile()
        {
            try
            {
                this.GetFileFullPath();
                FileHelper.DeleteDirectory(this.GetValidReceiptDirectoryPath());
                return true;
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Info((object)("DeleteLocalFile:\n" + ex.Message));
                return false;
            }
        }

        public bool Push()
        {
            DateTime now = DateTime.Now;
            AbstractLog.logger.Info((object)"开始给企业推送回执");
            try
            {
                string feedbackUrl = ServerCore.clientConfig.FeedbackURL;
                string str1 = "";
                if (feedbackUrl.IndexOf("?") > 0)
                {
                    string str2 = str1 + "&";
                }
                string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(new List<RequestParam>()
                {
                    RequestParam.GetInstance("userName", ServerCore.downloadConfig.entName),
                    RequestParam.GetInstance("passWord", ServerCore.clientConfig.TypistPassword),
                    RequestParam.GetInstance("companyId", ServerCore.downloadConfig.entCode),
                    RequestParam.GetInstance("messageID", this.MessageID),
                    RequestParam.GetInstance("messageType", this.MessageType),
                    RequestParam.GetInstance("receiverID", this.ReceiverID),
                    RequestParam.GetInstance("messageText", this.ReceiptContent)
                });
                AbstractLog.logger.Info((object)("回执推送参数【已URL编码】：" + urlEncodePostStr));
                string str3 = HttpHelper.HttpPost(feedbackUrl, urlEncodePostStr);
                this.LastTimePushTime = DateTime.Now;
                return str3.ToUpper().IndexOf("SUCCESS") == 0;
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Info((object)("Push:\n" + ex.Message));
                return false;
            }
        }

        public bool IsPersisted() => File.Exists(this.GetFileFullPath());
    }
}
