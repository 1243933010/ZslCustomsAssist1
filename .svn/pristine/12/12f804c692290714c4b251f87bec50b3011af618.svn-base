using System.Diagnostics;
using System.Text;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Service;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    internal class ReportReceiptScanJob : AbstractLog
    {

        public void OnDoJob()
        {
            if (ServerCore.clientConfig.ReceiptReceiveDir != "" && !Directory.Exists(ServerCore.clientConfig.ReceiptReceiveDir))
            {
                Directory.CreateDirectory(ServerCore.clientConfig.ReceiptReceiveDir);
            }
            while (true)
            {
                if (!ServerCore.IsExitThread)
                {
                    try
                    {
                        this.WatcherReceiptReceive(ServerCore.clientConfig.ReceiptReceiveDir);
                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)"回执目录扫描异常", ex);
                    }
                    Thread.Sleep(5000);
                }
                else
                    break;
            }
        }

        public void WatcherReceiptReceive(string ReceiptReceiveDir)
        {
            Stopwatch stopwatch = new Stopwatch();
            long wholeMilliseconds = 0;
            stopwatch.Start();

            foreach (FileInfo file1 in new DirectoryInfo(ReceiptReceiveDir).GetFiles("*.receipt"))
            {
                string fullPath = file1.FullName;
                string fileName = Path.GetFileName(fullPath);

                AbstractLog.logger.Info((object)("回执接收目录【" + ReceiptReceiveDir + "】扫描到【" + fileName + "】(" + file1.CreationTime.ToString("yyyy-MM-dd hh:mm:ss") + "),准备推送至辅助系统"));
                if (!IOHelper.CheakFileIsWriting(fullPath))
                {
                    new Thread((ThreadStart)(() =>
                    {
                        try
                        {
                            if (File.Exists(fullPath))
                            {
                                string receiptContent = string.Empty;
                                using (StreamReader streamReader = new(fullPath, Encoding.UTF8))
                                {
                                    receiptContent = Encrypter.DefaultDecodeAES(streamReader.ReadToEnd());
                                }
                                bool sendResult = new ApiService().SendReceipt(receiptContent, out string msg);
                                if (sendResult)
                                {
                                    AbstractLog.logger.Info((object)("回执【" + fileName + "】推送成功"));
                                    ReportReceiptScanJob.DeleteFile(fullPath);
                                }
                                else
                                {
                                    AbstractLog.logger.Info((object)("回执【" + fileName + "】推送失败：" + msg));
                                }
                            }
                            else
                            {
                                AbstractLog.logger.Error((object)("找不到回执文件" + fullPath + "！"));
                            }
                        }
                        catch (Exception ex)
                        {
                            AbstractLog.logger.Error((object)("回执文件" + fileName + "推送至辅助平台失败（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）"), ex);
                        }
                    })).Start();
                    Thread.Sleep(100);
                }
            }

            stopwatch.Stop();
            //AbstractLog.logger.Debug((object)("本次扫描耗时：" + (object)wholeMilliseconds + "ms"));
        }

        public static void DeleteFile(string fullPath)
        {
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("回执【" + Path.GetFileName(fullPath) + "】删除失败!"), ex);
                    ServerCore.DeleteFailReportSendList.Add(fullPath);
                }
            }
            else
                AbstractLog.logger.Info((object)("回执【" + Path.GetFileName(fullPath) + "】已不存在，无需执行删除操作!"));
        }
    }
}
