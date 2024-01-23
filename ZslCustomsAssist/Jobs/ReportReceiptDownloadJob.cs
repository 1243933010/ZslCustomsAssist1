using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ZslCustomsAssist.MQ.MQQueueProxy;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using IBM.WMQ;

namespace ZslCustomsAssist.Jobs
{
    public class ReportReceiptDownloadJob : AbstractLog
    {
        private static ReportReceiptDownloadJob lockObj = new ReportReceiptDownloadJob();

        public void OnDoJob()
        {
            try
            {
                if (!Directory.Exists(ServerCore.clientConfig.ReceiptReceiveDir))
                    Directory.CreateDirectory(ServerCore.clientConfig.ReceiptReceiveDir);
                this.CtrlReportReceiveThreads();
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"下载回执线程异常！", ex);
            }
        }

        public void CtrlReportReceiveThreads()
        {
            while (ServerCore.IsAllowToDealWithReport())
            {
                Thread.Sleep(100);
                try
                {
                    if (ServerCore.reportReceiptThreads.Count < ServerCore.downloadConfig.GetSendThread())
                    {
                        CancellationTokenSource TokenSource = new();
                        ThreadsHelper.AddThreads(ServerCore.reportReceiptThreads, new Action(() => this.ReportReceiptDownload(TokenSource)), TokenSource);
                        //AbstractLog.logger.Info((object)("成功添加回执下载线程！当前线程数为：" + (object)ServerCore.reportReceiptThreads.Count + "/" + (object)ServerCore.downloadConfig.GetSendThread()));
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)ex);
                }
            }
        }

        public void ReportReceiptDownload(CancellationTokenSource TokenSource)
        {
            while (!TokenSource.IsCancellationRequested)
            {
                if (ServerCore.reportReceiptThreads.Count <= ServerCore.downloadConfig.GetSendThread())
                {
                    if (ServerCore.IsAllowToDealWithReport())
                    {
                        Thread.Sleep(ServerCore.downloadConfig.GetSendPeriod());
                        try
                        {
                            this.DownloadRportReceipt();
                        }
                        catch (Exception ex)
                        {
                            AbstractLog.logger.Error((object)"自动下载回执异常！", ex);
                        }
                    }
                    else
                        goto label_3;
                }
                else
                    break;
            }
            //AbstractLog.logger.Info((object)("回执下载线程数减少至[" + (object)ServerCore.downloadConfig.GetSendThread() + "]，线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]即将从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ServerCore.reportReceiptThreads, (object)ReportReceiptDownloadJob.lockObj);
            return;
        label_3:
            //AbstractLog.logger.Info((object)("程序退出中/更新中/卡状态异常,不再处理任何报文！线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]即将从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ServerCore.reportReceiptThreads, (object)ReportReceiptDownloadJob.lockObj);
        }

        public void DownloadRportReceipt()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string paramsString = "";
            ReceiptContentResult jsonResult = (ReceiptContentResult)null;
            string transportProtocol = ServerCore.supportedTransportProtocol;
            if (!(transportProtocol == "HTTPS"))
            {
                if (!(transportProtocol == "MQ"))
                {
                    if (transportProtocol == "NULL")
                    {
                        stopwatch.Stop();
                        LogHelper.Info((object)"传输协议尚未选择，请进行选择配置！");
                        return;
                    }
                }
                else
                {
                    this.ReceiveMQMessage(ref jsonResult);
                    stopwatch.Stop();
                }
            }
            else if (!this.ReceiveHttpsMessage(ref paramsString, ref jsonResult))
            {
                stopwatch.Stop();
                return;
            }
            if (jsonResult.result)
            {
                if (jsonResult.receiptList == null || !jsonResult.receiptList.Any<ReceiptContentItem>())
                    return;
                AbstractLog.logger.Info((object)("回执下载成功,耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s;描述：" + jsonResult.description));
                ReportReceiptDownloadJob.SendReceiptReceived(jsonResult);
            }
            else
                AbstractLog.logger.Info((object)("回执下载失败：(" + jsonResult.errorCode + ")：" + jsonResult.description));
        }

        private string GetHttpsUrl()
        {
            if (ServerCore.IsTestModel)
                return "http://localhost:8080/shenzhendtsp_web/client/declare/receiveMessage.action";
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/receiveMessage.action" : cloudServicesUrl + "/client/declare/receiveMessage.action") : cloudServicesUrl + "/client/declare/receiveMessage.action";
        }

        private bool ReceiveHttpsMessage(ref string paramsString, ref ReceiptContentResult jsonResult)
        {
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("MAC地址：" + ServerCore.MacAddress));
            string httpsUrl = this.GetHttpsUrl();
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("macAddress", ServerCore.MacAddress)
            };
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            string paramsStrLambda = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsStrLambda = paramsStrLambda + param.Name + ":" + param.Value + "\r\n"));
            paramsString = paramsStrLambda;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string str;
            try
            {
                str = HttpHelper.HttpPostForType2(httpsUrl, urlEncodePostStr);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AbstractLog.logger.Error((object)("回执下载请求（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）异常！"), ex);
                return false;
            }
            if (string.IsNullOrWhiteSpace(str))
            {
                AbstractLog.logger.Info((object)"回执下载接口响应结果为空，接口请求异常!");
                return false;
            }
            try
            {
                jsonResult = JsonConvert.DeserializeObject<ReceiptContentResult>(str);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("回执下载接口返回结果序列化异常！结果：" + str), ex);
                return false;
            }
            return true;
        }

        private void ReceiveMQMessage(ref ReceiptContentResult jsonResult)
        {
            jsonResult = new ReceiptContentResult();
            List<MQMessage> messages = new List<MQMessage>();
            List<ReceiptContentItem> receiptContentItemList = new List<ReceiptContentItem>();
            try
            {
                IbmMQQueueProxy receiveQueueProxy = ServerCore.mqReceiveQueueProxy;
                int result;
                int.TryParse(ServerCore.downloadConfig.sendNumber, out result);
                receiveQueueProxy.GetMesseges(messages, result);
                foreach (MQMessage mqMessage in messages)
                {
                    if (mqMessage.MessageLength > 0)
                    {
                        ReceiptContentItem receiptContentItem = new ReceiptContentItem();
                        byte[] bytes = mqMessage.ReadBytes(mqMessage.MessageLength);
                        string xmlContent = Encoding.GetEncoding("UTF-8").GetString(bytes);
                        try
                        {
                            XmlDocument xmlDocument = XmlHelp.ToXmlDocument(xmlContent);
                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                            nsmgr.AddNamespace("ns2", "http://www.chinaport.gov.cn/dxp");
                            receiptContentItem.messageText = Base64Helper.Base64Decode(xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:Data", nsmgr).InnerXml);
                            string innerXml1 = xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:TransInfo/ns2:MsgType", nsmgr).InnerXml;
                            string innerXml2 = xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:TransInfo/ns2:SenderId", nsmgr).InnerXml;
                            receiptContentItem.messageId = "SZCPORT" + innerXml1 + "_" + innerXml2 + "_" + Guid.NewGuid().ToString();
                            receiptContentItem.messageType = "SZCPORT" + innerXml1;
                            receiptContentItem.messageFileSuffix = ".xml";
                            receiptContentItemList.Add(receiptContentItem);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex, "解封接收报文出错,原接收报文为：\n" + xmlContent);
                        }
                    }
                }
                jsonResult.receiptList = receiptContentItemList;
                jsonResult.result = true;
                jsonResult.description = "已成功下载【 " + (object)receiptContentItemList.Count + "】条回执！";
            }
            catch (MQException ex1)
            {
                LogHelper.Error((Exception)ex1, "使用MQ接收报文过程发生异常：" + (object)ex1.ReasonCode + "(" + ((Exception)ex1).Message + ")");
                foreach (MQMessage mqMessage in messages)
                {
                    if (mqMessage.MessageLength > 0)
                    {
                        ReceiptContentItem receiptContentItem = new ReceiptContentItem();
                        byte[] bytes = mqMessage.ReadBytes(mqMessage.MessageLength);
                        string xmlContent = Encoding.GetEncoding("UTF-8").GetString(bytes);
                        try
                        {
                            XmlDocument xmlDocument = XmlHelp.ToXmlDocument(xmlContent);
                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                            nsmgr.AddNamespace("ns2", "http://www.chinaport.gov.cn/dxp");
                            receiptContentItem.messageText = Base64Helper.Base64Decode(xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:Data", nsmgr).InnerXml);
                            string innerXml3 = xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:TransInfo/ns2:MsgType", nsmgr).InnerXml;
                            string innerXml4 = xmlDocument.SelectSingleNode("/ns2:DxpMsg/ns2:TransInfo/ns2:SenderId", nsmgr).InnerXml;
                            receiptContentItem.messageId = "SZCPORT" + innerXml3 + "_" + innerXml4 + "_" + Guid.NewGuid().ToString();
                            receiptContentItem.messageFileSuffix = ".xml";
                            receiptContentItemList.Add(receiptContentItem);
                        }
                        catch (Exception ex2)
                        {
                            LogHelper.Error(ex2, "解封接收报文出错,原接收报文为：\n" + xmlContent);
                        }
                    }
                }
                jsonResult.receiptList = receiptContentItemList;
                jsonResult.result = true;
                jsonResult.description = "已成功下载【 " + (object)receiptContentItemList.Count + "】条回执！";
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "使用MQ传输预处理过程发生异常！");
                jsonResult.errorCode = "MQPrehandleError";
                jsonResult.description = "ReasonDisc => " + ex.Message;
            }
        }

        public static void SendReceiptReceived(ReceiptContentResult jsonResult)
        {
            List<ReceiptItemToSend> receiptItems = new List<ReceiptItemToSend>();
            List<ReceiptContentItem> needSendReceiptNoticeItem = new List<ReceiptContentItem>();
            jsonResult.receiptList.ForEach((Action<ReceiptContentItem>)(receipt =>
            {
                if (receipt == null)
                    return;
                if (ServerCore.downloadConfig.receiptMessageTypes.Contains(receipt.messageType))
                {
                    receiptItems.Add(new ReceiptItemToSend()
                    {
                        receiptId = receipt.receiptId
                    });
                    needSendReceiptNoticeItem.Add(receipt);
                }
                else
                {
                    lock (ReportReceiptDownloadJob.lockObj)
                    {
                        ServerCore.UnWriteReceiptReports.Add(receipt);
                        ReportReceiptDownloadJob.AddDownLoadedReceiptReportSum();
                        AbstractLog.logger.Info((object)("已下载回执【" + receipt.messageId + ".xml】"));
                    }
                }
            }));
            if (receiptItems.Count <= 0)
                return;
            string str1 = JsonConvert.SerializeObject((object)receiptItems);
            string Url = ServerCore.CloudServicesUrl + "/client/declare/sendReceiptReceived.action";
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("messageText", str1)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                string str2 = HttpHelper.HttpPostForType2(Url, urlEncodePostStr);
                stopwatch.Stop();
                AbstractLog.logger.Info((object)("回执接收成功确认上传耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s,结果：" + str2));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                AbstractLog.logger.Error((object)("回执接收成功确认上传（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）异常！"), ex);
            }
            needSendReceiptNoticeItem.ForEach((Action<ReceiptContentItem>)(receipt =>
            {
                lock (ReportReceiptDownloadJob.lockObj)
                {
                    ServerCore.UnWriteReceiptReports.Add(receipt);
                    ReportReceiptDownloadJob.AddDownLoadedReceiptReportSum();
                    AbstractLog.logger.Info((object)("已下载回执【" + receipt.messageId + ".xml】"));
                }
            }));
        }

        public static void AddDownloadReportReceipts(ReceiptContentResult jsonResult) => jsonResult.receiptList.ForEach((Action<ReceiptContentItem>)(receipt =>
        {
            if (receipt == null)
                return;
            lock (ReportReceiptDownloadJob.lockObj)
            {
                ServerCore.UnWriteReceiptReports.Add(receipt);
                ReportReceiptDownloadJob.AddDownLoadedReceiptReportSum();
                AbstractLog.logger.Info((object)("已下载回执【" + receipt.messageId + ".xml】"));
            }
        }));

        public static void AddDownLoadedReceiptReportSum()
        {
            lock (ReportReceiptDownloadJob.lockObj)
                ++ServerCore.DownLoadedReceiptReportSum;
        }

        public static ReceiptContentItem GetMyDownloadReportTask()
        {
            ReceiptContentItem downloadReportTask = (ReceiptContentItem)null;
            lock (ReportReceiptDownloadJob.lockObj)
            {
                try
                {
                    if (ServerCore.UnWriteReceiptReports.Count > 0)
                    {
                        downloadReportTask = ServerCore.UnWriteReceiptReports[0];
                        ServerCore.UnWriteReceiptReports.RemoveAt(0);
                        //AbstractLog.logger.Info((object)("领取待写入任务成功,文件:" + downloadReportTask.messageId));
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"领取待写入任务失败！", ex);
                }
            }
            return downloadReportTask;
        }
    }
}
