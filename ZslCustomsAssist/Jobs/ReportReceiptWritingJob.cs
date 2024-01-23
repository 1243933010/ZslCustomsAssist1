using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Service;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Jobs
{
    internal class ReportReceiptWritingJob : AbstractLog
    {
        public static List<ThreadExt> writingReportThreads = new List<ThreadExt>();
        private static ReportReceiptWritingJob lockObj = new ReportReceiptWritingJob();

        public void OnDoJob()
        {
            try
            {
                this.CtrlReportWriteThreads();
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"回执写入线程异常！", ex);
            }
        }

        public void CtrlReportWriteThreads()
        {
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    if (ReportReceiptWritingJob.writingReportThreads.Count < ServerCore.downloadConfig.GetSendThread())
                    {
                        CancellationTokenSource TokenSource = new();
                        ThreadsHelper.AddThreads(ReportReceiptWritingJob.writingReportThreads, new Action(() => this.ReportReceiptWrite(TokenSource)), TokenSource);
                        //AbstractLog.logger.Info((object)("成功添加回执写入线程！当前线程数为：" + (object)ReportReceiptWritingJob.writingReportThreads.Count + "/" + (object)ServerCore.downloadConfig.GetSendThread()));
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)ex);
                }
            }
        }

        private void ReportReceiptWrite(CancellationTokenSource TokenSource)
        {
            while (!TokenSource.IsCancellationRequested)
            {
                if (ReportReceiptWritingJob.writingReportThreads.Count <= ServerCore.downloadConfig.GetSendThread())
                {
                    ReceiptContentItem downloadReportTask = ReportReceiptDownloadJob.GetMyDownloadReportTask();
                    if (downloadReportTask == null)
                        Thread.Sleep(ServerCore.downloadConfig.GetSendPeriod());
                    else if (string.IsNullOrWhiteSpace(downloadReportTask.messageText))
                    {
                        AbstractLog.logger.Error((object)("由于回执【" + downloadReportTask.messageId + ".receipt】中的messageText内容为空，取消写入该回执！"));
                    }
                    else
                    {
                        if ("SZCPORTEncrypt".Equals(downloadReportTask.messageType))
                        {
                            try
                            {
                                XmlDocument xmlDocument1 = new XmlDocument()
                                {
                                    PreserveWhitespace = true
                                };
                                xmlDocument1.LoadXml(downloadReportTask.messageText);
                                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument1.NameTable);
                                nsmgr.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
                                nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
                                string aesKey = SPSecureAPI.SpcRSADecryptAsPEM(xmlDocument1.SelectSingleNode("/xenc:EncryptedData/ds:KeyInfo/xenc:EncryptedKey/xenc:CipherData/xenc:CipherValue", nsmgr).InnerXml);
                                string xml = AESUtil.AesDecrypt(xmlDocument1.SelectSingleNode("/xenc:EncryptedData/xenc:CipherData/xenc:CipherValue", nsmgr).InnerXml, aesKey);
                                XmlDocument xmlDocument2 = new XmlDocument()
                                {
                                    PreserveWhitespace = true
                                };
                                xmlDocument2.LoadXml(xml);
                                XmlNode xmlNode = xmlDocument2.GetElementsByTagName("MsgType")[0];
                                if (xmlNode != null && !string.IsNullOrWhiteSpace(xmlNode.InnerXml))
                                    downloadReportTask.messageType = "SZCPORT" + xmlNode.InnerXml;
                                string str = Encoding.UTF8.GetString(Convert.FromBase64String(xmlDocument2.GetElementsByTagName("Data")[0].InnerXml));
                                downloadReportTask.messageText = str;
                            }
                            catch (Exception ex)
                            {
                                AbstractLog.logger.Error((object)("报关单订阅数据解密失败,密文为：\n" + downloadReportTask.messageText), ex);
                            }
                        }
                        string dirByRecipType = ClientConfig.GetDirByRecipType(downloadReportTask.messageType);
                        string saveFileFullPath = Path.Combine(dirByRecipType, downloadReportTask.messageId + ".receipt");
                        ReportReceiptWritingJob.AddWritingReceiptReportsCount();
                        try
                        {
                            this.WriteRportReceipt(downloadReportTask, dirByRecipType, saveFileFullPath);
                        }
                        catch (Exception ex)
                        {
                            AbstractLog.logger.Error((object)("回执写入异常！\n" + LogHelper.GetAttributesNValueFromObject((object)downloadReportTask, "receiptContentItem")), ex);
                            ReportReceiptWritingJob.ReAddToUnWriteList(downloadReportTask, saveFileFullPath);
                            AbstractLog.logger.Info((object)("回执【" + saveFileFullPath + "】已重新添加到未写入队列！"));
                            ServerCore.AddMainLog("回执【" + downloadReportTask.messageId + ".receipt】写入失败将继续尝试重新写入!", EnumLogLevel.Error);
                        }
                        finally
                        {
                            ReportReceiptWritingJob.AddWritingReceiptReportsCount(-1);
                        }
                    }
                }
                else
                    break;
            }
            AbstractLog.logger.Info((object)("回执写入线程数减少至[" + (object)ServerCore.downloadConfig.GetSendThread() + "]，线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]即将从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ReportReceiptWritingJob.writingReportThreads, (object)ReportReceiptWritingJob.lockObj);
        }

        public void WriteRportReceipt(
            ReceiptContentItem receiptContentItem,
            string directoryName,
            string saveFileFullPath)
        {
            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            if (receiptContentItem.GetMessageZiped())
            {
                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                try
                {
                    receiptContentItem.messageText = CGZipUtil.GZipDecompressString(Base64Helper.Base64Decode(receiptContentItem.messageText));
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("解压报文异常：" + LogHelper.GetAttributesNValueFromObject((object)receiptContentItem, nameof(receiptContentItem))), ex);
                }
                stopwatch2.Stop();
                AbstractLog.logger.Info((object)("本次解压报文耗时：" + (object)((double)stopwatch2.ElapsedMilliseconds / 1000.0) + "s"));
            }

            receiptContentItem.messageText = receiptContentItem.messageText.Trim(new char[] {' ', '\r', '\n'});
            FileHelper.SaveAsFile(directoryName, receiptContentItem.messageId + ".xml", receiptContentItem.messageText);

            //TODO：回执推送至中商旅辅助平台
            try
            {
                bool sendResult = new ApiService().SendReceipt(receiptContentItem.messageText, out string msg);
                if (sendResult)
                {
                    AbstractLog.logger.Info((object)("回执【" + saveFileFullPath + "】推送成功"));
                }
                else 
                { 
                    if (FileHelper.SaveAsFile(directoryName, receiptContentItem.messageId + ".receipt", Encrypter.DefaultEncodeAES(receiptContentItem.messageText)))
                    {
                        ServerCore.AddMainLog("回执推送中商旅辅助平台失败，原因：" + msg + "，写入文件【" + saveFileFullPath + "】成功", EnumLogLevel.Error);
                        AbstractLog.logger.Info((object)("回执推送中商旅辅助平台失败，原因：" + msg + "，写入文件【" + saveFileFullPath + "】成功"));
                    }
                    else
                    {
                        AbstractLog.logger.Info((object)("回执【" + saveFileFullPath + "】回执文件写入失败"));
                        throw new Exception("回执文件写入失败！");
                    }
                }
            }
            catch (Exception ex)
            {
                if (FileHelper.SaveAsFile(directoryName, receiptContentItem.messageId + ".receipt", Encrypter.DefaultEncodeAES(receiptContentItem.messageText)))
                {
                    ServerCore.AddMainLog("回执推送出错，原因：" + ex.Message + "，写入文件【" + saveFileFullPath + "】成功", EnumLogLevel.Error);
                    AbstractLog.logger.Info((object)("回执推送出错，原因：" + ex.Message + "，写入文件【" + saveFileFullPath + "】成功"));
                }
                else
                {
                    ServerCore.AddMainLog("回执推送出错，回执【" + saveFileFullPath + "】回执文件写入失败", EnumLogLevel.Error);
                    AbstractLog.logger.Info((object)("回执推送出错，回执【" + saveFileFullPath + "】回执文件写入失败"));
                    throw new Exception("回执文件写入失败！");
                }
            }

            ReportReceiptWritingJob.AddReceiptWritedSum();
            stopwatch1.Stop();
            //AbstractLog.logger.Info((object)("本次写入耗时：" + (object)((double)stopwatch1.ElapsedMilliseconds / 1000.0) + "s；剩余待写入回执：" + (object)ServerCore.UnWriteReceiptReports.Count + "/" + (object)ServerCore.DownLoadedReceiptReportSum));
        }

        public static void AddWritingReceiptReportsCount(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ReportReceiptWritingJob.lockObj)
                ServerCore.WritingReceiptReportsCount += addNum;
        }

        public static void AddReceiptWritedSum(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ReportReceiptWritingJob.lockObj)
                ServerCore.ReceiptWritedSum += addNum;
        }

        public static void ReAddToUnWriteList(
          ReceiptContentItem receiptContentItem,
          string saveFileFullPath)
        {
            lock (ReportReceiptWritingJob.lockObj)
                ServerCore.UnWriteReceiptReports.Add(receiptContentItem);
        }
    }
}
