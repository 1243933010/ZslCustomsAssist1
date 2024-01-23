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
using ZslCustomsAssist.Jobs;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Server.Rest
{
    public abstract class AbstractHandle
    {
        public string GetCusResultParamsName() => "resultJson";

        public void SecurityValid(string userName, string passWord, string companyId)
        {
            if (ServerCore.IsTestModel)
                return;
            if (ServerCore.clientConfig == null)
                throw new Exception("客户端参数未配置，请进行设置后再调用接口！");
            if (ServerCore.clientConfig == null)
                throw new Exception("客户端未配置正确的参数");
            if (ServerCore.sysConfig == null)
                throw new Exception("客户端未完成系统配置的加载");
            ClientConfig clientConfig = ServerCore.clientConfig;
            DownloadConfig downloadConfig = ServerCore.downloadConfig;
            if (downloadConfig.entName != userName || clientConfig.TypistPassword != passWord || downloadConfig.entCode != companyId)
                throw new Exception("安全校验不通过，请检查单一窗口的登录账号、密码、企业编号是否与客户端预设值相同！");
        }

        public static IServiceHandle GetPusher(CusReceiptPushJob receipt)
        {
            IServiceHandle pusher = null;
            if (receipt.ReceiptType == 1 || receipt.ReceiptType == 2 || receipt.ReceiptType != 3)
                ;
            return pusher;
        }

        public bool CusReceiptPush(IServiceHandle pushHandle, CusReceiptPushJob receipt)
        {
            DateTime now = DateTime.Now;
            if (receipt.ReceiptUrl == "" || receipt.ReceiptParams == null || receipt.ReceiptType == 0)
            {
                LogHelper.Error(new Exception("无法处理海关回执【" + receipt.GetReceiptTypeDesc() + "】，数据异常！"), receipt.InvNo);
                throw new Exception("无法处理海关回执【" + receipt.GetReceiptTypeDesc() + "】，数据异常！");
            }
            string receiptUrl = receipt.ReceiptUrl;
            if (ServerCore.IsDebugModel)
                LogHelper.Info("开始海关回执【" + receipt.GetReceiptTypeDesc() + "】推送，推送地址：" + receiptUrl, receipt.InvNo);
            string str;
            try
            {
                if (receipt.filePaths == null || receipt.filePaths.Length == 0)
                {
                    string postDataStr = ServerCore.GetUrlEncodePostStr(receipt.ReceiptParams) + "&" + pushHandle.GetCusResultParamsName() + "=" + receipt.CusReceiptResult;
                    LogHelper.Info("请求参数【已URL编码】" + postDataStr, receipt.InvNo, 1);
                    str = HttpHelper.HttpPost(receiptUrl, postDataStr);
                }
                else
                {
                    receipt.ReceiptParams.Add(new RequestParam(pushHandle.GetCusResultParamsName(), receipt.CusReceiptResult));
                    LogHelper.Info("请求有附件的回执，参数【未编码】" + JsonConvert.SerializeObject(receipt.ReceiptParams), receipt.InvNo, 1);
                    for (int index = 0; index < receipt.filePaths.Length; ++index)
                        LogHelper.Info("    文件路径：" + receipt.filePaths[index]);
                    str = HttpHelper.HttpPostAndUploadFile(receiptUrl, receipt.filePaths, receipt.ReceiptParams);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, receipt.InvNo, 1);
                return false;
            }
            LogHelper.Info("海关回执推送耗时" + DateHelper.GetDifferTotalMilliseconds(now), receipt.InvNo, 1);
            try
            {
                HWSBResult hwsbResult = JsonConvert.DeserializeObject<HWSBResult>(str);
                if (hwsbResult.ReturnCode == 1)
                {
                    if (receipt.filePaths != null && receipt.filePaths.Length != 0)
                    {
                        foreach (string filePath in receipt.filePaths)
                        {
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                                LogHelper.Info("回执【" + receipt.GetReceiptTypeDesc() + "】推送成功,删除附件【" + filePath + "】", receipt.InvNo, 1);
                            }
                        }
                    }
                    LogHelper.Info("海关回执【" + receipt.GetReceiptTypeDesc() + "】推送成功", indentValue: 1);
                    return true;
                }
                LogHelper.Info("海关回执【" + receipt.GetReceiptTypeDesc() + "】推送失败,原因:" + hwsbResult.GetErrorMessage(), receipt.InvNo, 1);
                return false;
            }
            catch
            {
                LogHelper.Info("海关回执【" + receipt.GetReceiptTypeDesc() + "】推送失败", receipt.InvNo, 1);
            }
            return false;
        }

        public bool ValidCus(string cus) => cus != null && !(cus == "") && (cus.StartsWith("51") || cus.StartsWith("52"));
    }
}
