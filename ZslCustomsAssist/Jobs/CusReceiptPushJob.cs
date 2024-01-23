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
using ZslCustomsAssist.Server.Rest;

namespace ZslCustomsAssist.Jobs
{
    public class CusReceiptPushJob
    {
        public const string fileSuffix = ".cusReceipt";
        public const string CusResultDirectoryName = "CusResult";
        public const int PUSHERHANDLE_ICA = 1;
        public const int PUSHERHANDLE_UPLOADEDOC = 2;
        public const int PUSHERHANDLE_INSPECTHG = 3;

        public string InvNo { get; set; }

        public int ReceiptType { get; set; }

        public string ReceiptUrl { get; set; }

        public string[] filePaths { get; set; }

        public List<RequestParam> ReceiptParams { get; set; }

        public string HandleFullClassName { get; set; }

        public DateTime ReceiptDate { get; set; }

        public DateTime LastTimePushTime { get; set; }

        public string CusReceiptResult { get; set; }

        public int FailureCount { get; set; }

        public string GetReceiptTypeDesc()
        {
            string receiptTypeDesc;
            switch (this.ReceiptType)
            {
                case 1:
                    receiptTypeDesc = "派生委托协议";
                    break;
                case 2:
                    receiptTypeDesc = "上传随附单证";
                    break;
                case 3:
                    receiptTypeDesc = "报关";
                    break;
                default:
                    receiptTypeDesc = "未知";
                    break;
            }
            return receiptTypeDesc;
        }

        public string GetDirectoryPath() => Path.Combine(ServerCore.clientConfig.ReceiptReceiveDir, "InvNo", this.InvNo, "CusResult");

        public string GetLocalFileName() => this.ReceiptDate.ToString("yyyyMMddHHmmssff") + ".cusReceipt";

        public string GetFileFullPath() => Path.Combine(this.GetDirectoryPath(), this.GetLocalFileName());

        public string ToJsonStr() => JsonConvert.SerializeObject((object)this);

        public bool SaveAsReceiptFile()
        {
            string directoryPath = this.GetDirectoryPath();
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            return FileHelper.SaveAsFile(directoryPath, this.GetLocalFileName(), this.ToJsonStr());
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
                FileHelper.DeleteDirectory(this.GetDirectoryPath());
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
            try
            {
                IServiceHandle pusher = AbstractHandle.GetPusher(this);
                return pusher.CusReceiptPush(pusher, this);
            }
            catch (Exception ex)
            {
                if (!ServerCore.IsDebugModel)
                    ;
                return false;
            }
        }

        public bool IsPersisted() => File.Exists(this.GetFileFullPath());
    }
}
