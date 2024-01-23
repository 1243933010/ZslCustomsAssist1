using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Jobs
{
    public class ReportQueryJob : AbstractLog
    {
        public static List<ThreadExt> reportQueryThreads = new List<ThreadExt>();
        public string privateKeyStr;
        public string publicKeyStr;
        public static readonly string unQueryListName = "待查询集合";
        public static readonly string xsdPath = "./tpl/DataResource.xsd";
        private static readonly string queryMode = "4";
        public static readonly string messageType = "SZCPORTDRQC";
        public static readonly string msgTypes = "SZCPORTDR";
        public static readonly string receiver = "SZCPORT";

        public ReportQueryJob()
        {
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(1024, new CspParameters()
            {
                Flags = CspProviderFlags.UseMachineKeyStore
            });
            this.privateKeyStr = cryptoServiceProvider.ToXmlString(true);
            this.publicKeyStr = cryptoServiceProvider.ToXmlString(false);
        }

        public void OnDoJob()
        {
            try
            {
                if (!Directory.Exists(ServerCore.clientConfig.ReceiptReceiveDir))
                    Directory.CreateDirectory(ServerCore.clientConfig.ReceiptReceiveDir);
                AbstractLog.logger.Debug((object)"等待1秒钟开启文件监控，等候中……");
                Thread.Sleep(1000);
                AbstractLog.logger.Debug((object)"文件监控，开始!");
                this.CtrlReportQueryThreads();
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"报文查询线程异常！", ex);
            }
        }

        public void CtrlReportQueryThreads()
        {
            AbstractLog.logger.Info((object)("开始处理报文查询业务，预计开启线程数：" + (object)ServerCore.sysConfig.GetClientDataQueryThreadCount()));
            while (ServerCore.IsAllowToDealWithReport())
            {
                Thread.Sleep(1000);
                try
                {
                    if (ReportQueryJob.reportQueryThreads.Count < ServerCore.sysConfig.GetClientDataQueryThreadCount())
                    {
                        CancellationTokenSource TokenSource = new();
                        ThreadsHelper.AddThreads(ReportQueryJob.reportQueryThreads, new Action(() => this.OnDoReportQuery(TokenSource)), TokenSource);
                        AbstractLog.logger.Info((object)("成功添加报文查询线程！当前线程数为：" + (object)ReportQueryJob.reportQueryThreads.Count + "/" + (object)ServerCore.sysConfig.GetClientDataQueryThreadCount()));
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)ex);
                }
            }
        }

        private void OnDoReportQuery(CancellationTokenSource TokenSource)
        {
            while (!TokenSource.IsCancellationRequested)
            {
                string myQueryReportTask;
                do
                {
                    if (ReportQueryJob.reportQueryThreads.Count <= ServerCore.sysConfig.GetClientDataQueryThreadCount())
                    {
                        if (ServerCore.IsAllowToDealWithReport())
                        {
                            Thread.Sleep(ServerCore.downloadConfig.GetReceivePeriod());
                            myQueryReportTask = this.GetMyQueryReportTask();
                        }
                        else
                            goto label_3;
                    }
                    else
                        goto label_1;
                }
                while (string.IsNullOrWhiteSpace(myQueryReportTask));
                if (IOHelper.CheakFileIsWriting(myQueryReportTask))
                {
                    if (File.Exists(myQueryReportTask))
                    {
                        AbstractLog.logger.Info((object)("待查询报文【" + myQueryReportTask + "】仍在写入中!"));
                        ReportSendJob.AppendToUnSendReportList(myQueryReportTask);
                    }
                    else
                    {
                        ServerCore.AddReportScanedSum(-1);
                        AbstractLog.logger.Error((object)("待查询报文【" + myQueryReportTask + "】由于未知原因丢失！已完成对：【" + myQueryReportTask + "】的处理！"));
                    }
                }
                else
                {
                    bool flag = false;
                    try
                    {
                        ServerCore.AddReportSendingCount();
                        flag = this.SendReport(myQueryReportTask);
                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)("【" + myQueryReportTask + "】发送异常!\r\n" + ExceptionHelper.GetErrorMsg(ex)));
                    }
                    finally
                    {
                        if (flag)
                            ServerCore.AddReportSendSuccessSum();
                        else
                            ServerCore.AddReportSendFailSum();
                        ServerCore.AddReportSendingCount(-1);
                        AbstractLog.logger.Info((object)("已完成对【" + myQueryReportTask + "】的处理," + (object)ServerCore.downloadConfig.GetReceivePeriod() + "毫秒后处理下一个任务！"));
                    }
                }
            }
        label_1:
            AbstractLog.logger.Info((object)("报文查询线程数减少至[" + (object)ServerCore.sysConfig.GetClientDataQueryThreadCount() + "]，线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ReportQueryJob.reportQueryThreads, ServerCore.sendLock);
            return;
        label_3:
            AbstractLog.logger.Info((object)("程序退出中/更新中/卡状态异常,不再处理任何报文！线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ReportQueryJob.reportQueryThreads, ServerCore.sendLock);
        }

        private bool SendReport(string fullPath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string[] strArray = withoutExtension.Split('_');
            if (strArray.Length < 4)
            {
                ReportSendJob.MoveToFailDirectory(fullPath, "报文命名格式不正确");
                ServerCore.AddMainLog(withoutExtension + "报文命名格式不正确，已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀", EnumLogLevel.Error);
                AbstractLog.logger.Error((object)(withoutExtension + "报文命名格式不正确，已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀"));
                return false;
            }
            for (int index = 0; index < 4 && index < strArray.Length; ++index)
            {
                if (index < 3 && strArray[index].IndexOf(" ") != -1)
                {
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！");
                    AbstractLog.logger.Error((object)(withoutExtension + "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀"));
                    ServerCore.AddMainLog(withoutExtension + "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀", EnumLogLevel.Error);
                    return false;
                }
            }
            DataResource dataResource = (DataResource)null;
            try
            {
                dataResource = this.ValidateMsgType(fullPath, strArray[0]);
            }
            catch (Exception ex)
            {
                ReportSendJob.MoveToFailDirectory(fullPath, "数据资源类型校验异常：" + ExceptionHelper.GetErrorMsg(ex));
                ServerCore.AddMainLog("查询请求报文【" + withoutExtension + ".xml】" + ex.Message + "，已移动到【发送失败目录】！", EnumLogLevel.Error);
                AbstractLog.logger.Error((object)("查询请求报文【" + withoutExtension + ".xml】" + ex.Message + "，已移动到【发送失败目录】"), ex);
                return false;
            }
            string reportStr;
            try
            {
                reportStr = this.GetReportStr(fullPath, strArray[0]);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("【" + fullPath + "】传输封装异常！"), ex);
                ReportSendJob.MoveToFailDirectory(fullPath, "传输封装异常！\n" + ex.Message + "\n" + ex.StackTrace);
                ServerCore.AddMainLog(withoutExtension + "传输封装异常(" + ex.Message + ")", EnumLogLevel.Error);
                return false;
            }
            QueryDataResourcesResult jsonResult = (QueryDataResourcesResult)null;
            if (!this.SendHttpsMessage(strArray[0], fullPath, reportStr, ref jsonResult))
            {
                AbstractLog.logger.Info((object)("报文处理共耗时：" + (object)stopwatch.ElapsedMilliseconds + "ms"));
                return false;
            }
            if (jsonResult.result)
            {
                AbstractLog.logger.Info((object)("报文【" + withoutExtension + ".xml】查询操作成功！"));
                ReportSendJob.DeleteFile(fullPath);
            }
            else if (jsonResult.errorCode != null && jsonResult.errorCode.StartsWith("-"))
            {
                AbstractLog.logger.Info((object)("报文【" + withoutExtension + "】发送失败（" + jsonResult.errorCode + "）：" + jsonResult.description + ",进入重发..."));
                ReportSendJob.AppendToUnSendReportList(fullPath, true);
            }
            else
            {
                ReportSendJob.MoveToFailDirectory(fullPath, "报文查询失败，描述：" + jsonResult.description);
                AbstractLog.logger.Info((object)("报文【" + withoutExtension + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！"));
                ServerCore.AddMainLog("报文【" + withoutExtension + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！", EnumLogLevel.Error);
                return false;
            }
            if (dataResource == null)
                return false;
            bool isUpdateSuccess = false;
            ServerCore.downloadConfig.dataResourceTypes.ForEach((Action<DataResourceType>)(drt =>
            {
                if (!(drt.classificationCode == dataResource.classificationCode))
                    return;
                drt.dataResource.ForEach((Action<DataResource>)(dr =>
                {
                    if (!(dr.code == dataResource.code))
                        return;
                    dr.remains = jsonResult.queryDataResources.remains;
                    isUpdateSuccess = true;
                }));
            }));
            return isUpdateSuccess;
        }

        private string GetReportStr(string fullPath, string msgType)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string ediCode = ServerCore.downloadConfig.ediCode;
            string SendTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            byte[] numArray = FileHelper.GetBytes(fullPath);
            if (numArray.Length >= 3 && numArray[0] == (byte)239 && numArray[1] == (byte)187 && numArray[2] == (byte)191)
            {
                byte[] destinationArray = new byte[numArray.Length - 3];
                Array.Copy((Array)numArray, 3, (Array)destinationArray, 0, destinationArray.Length);
                numArray = destinationArray;
            }
            Encoding.UTF8.GetString(numArray);
            return this.TranslateSign(withoutExtension, msgType, ediCode, ReportQueryJob.receiver, SendTime, XmlHelp.GetXmlContent(fullPath, Encoding.UTF8));
        }

        private DataResource ValidateMsgType(string fullPath, string msgType)
        {
            List<ReceiveMessageType> receiveMessageTypes = ServerCore.downloadConfig.receiveMessageTypes;
            if (receiveMessageTypes == null || !receiveMessageTypes.Any<ReceiveMessageType>())
                throw new Exception("同步配置信息中目标报文类型为空！");
            if (!receiveMessageTypes.Any<ReceiveMessageType>((Func<ReceiveMessageType, bool>)(rmt => msgType == rmt.code)))
                throw new Exception("报文类型【" + msgType + "】未开通业务申请!");
            List<DataResourceType> dataResourceTypes = ServerCore.downloadConfig.dataResourceTypes;
            if (dataResourceTypes == null || !dataResourceTypes.Any<DataResourceType>())
                throw new Exception("无数据资源查询权限,请联系分中心业务部分开通相关权限！");
            string str1 = XmlHelp.XmlValidationByXsd(fullPath, ReportQueryJob.xsdPath);
            if (!string.IsNullOrWhiteSpace(str1))
                throw new Exception("格式校验不通过：" + str1);
            string xmlContent = XmlHelp.GetXmlContent(fullPath, Encoding.UTF8);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);
            string messageType = "";
            try
            {
                messageType = xmlDocument.GetElementsByTagName("MessageType")[0].InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception("解析失败！" + ex.Message + "\n" + ex.StackTrace);
            }
            if (string.IsNullOrWhiteSpace(messageType))
                throw new Exception("messageType节点内容为空");
            DataResource dataResource = (DataResource)null;
            if (dataResourceTypes.Find((Predicate<DataResourceType>)(drt => drt.dataResource.Any<DataResource>((Func<DataResource, bool>)(dr =>
            {
                if (!(dr.code == messageType))
                    return false;
                dataResource = dr;
                return true;
            })))) == null)
                throw new Exception("对应数据资源类型【" + messageType + "】未开通查询权限!");
            if (!dataResource.GetQueryMode().Contains<char>('2'))
                throw new Exception("对应数据资源【" + dataResource.name + "】,类型【" + messageType + "】不支持报文接口查询方式!");
            if (dataResource.GetRemains() < 1)
                throw new Exception("对应数据资源【" + dataResource.name + "】,类型【" + messageType + "】今日剩余查询次数已用完!");
            AbstractLog.logger.Debug((object)("数据资源【" + dataResource.name + "】,类型【" + messageType + "】今日剩余查询次数为：" + (object)dataResource.GetRemains() + "/" + (object)dataResource.GetDailyQueryCount()));
            try
            {
                string regexTips = "";
                string str2 = "";
                bool flag1 = true;
                bool flag2 = true;
                XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Condition");
                Hashtable requireCondiction = dataResource.GetRequireCondiction();
                Hashtable hashtable = new Hashtable();
                foreach (XmlNode xmlNode in elementsByTagName)
                {
                    string name = xmlNode.FirstChild.InnerText;
                    string innerText = xmlNode.LastChild.InnerText;
                    Condition condition = dataResource.conditions.Find((Predicate<Condition>)(c => c.name == name));
                    if (condition == null)
                        throw new Exception("【" + name + "】条件名无效");
                    requireCondiction[(object)name] = (object)null;
                    if (hashtable[(object)name] == null)
                        hashtable.Add((object)name, (object)1);
                    else if ((int)hashtable[(object)name] < condition.GetMultipleValuesNumber())
                        hashtable[(object)name] = (object)((int)hashtable[(object)name] + 1);
                    else
                        throw new Exception("【" + name + "】条件的多值查询个数上限为【" + (object)condition.GetMultipleValuesNumber() + "】，请勿超过该上限值");
                    if (condition.GetRequired() && string.IsNullOrEmpty(innerText))
                    {
                        str2 = str2 + "【" + condition.description + "(" + condition.name + ")】值未填写";
                        flag1 = false;
                    }
                    if (flag1 && !string.IsNullOrEmpty(innerText))
                    {
                        string dataType = condition.dataType;
                        if (!(dataType == "Date"))
                        {
                            if (!(dataType == "Double"))
                            {
                                if (!(dataType == "Integer"))
                                {
                                    if (dataType == "RegexpString")
                                    {
                                        if (!string.IsNullOrEmpty(innerText + condition.regexp) && !Regex.IsMatch(innerText, condition.regexp))
                                            flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】未符合【" + condition.message + "】");
                                    }
                                    else if (condition.GetMax() < innerText.Length || condition.GetMin() > innerText.Length)
                                        flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】字符长度应在【" + (object)condition.GetMin() + "】-【" + (object)condition.GetMax() + "】之间");
                                }
                                else
                                {
                                    long result;
                                    if (!long.TryParse(innerText, out result))
                                        flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】必须为整数");
                                    else if (result > (long)condition.GetMax() || result < (long)condition.GetMin())
                                        flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】值域应在【" + (object)condition.GetMin() + "】-【" + (object)condition.GetMax() + "】之间");
                                }
                            }
                            else
                            {
                                double result;
                                if (!double.TryParse(innerText, out result))
                                    flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】不符合小数格式");
                                else if (result > (double)condition.GetMax() || result < (double)condition.GetMin())
                                    flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】值域应在【" + (object)condition.GetMin() + "】-【" + (object)condition.GetMax() + "】之间");
                            }
                        }
                        else
                        {
                            DateTime result;
                            if (!string.IsNullOrEmpty(condition.format) && (!DateTime.TryParse(innerText, out result) || result.ToString(condition.format) != innerText))
                                flag2 = ReportQueryJob.GetRegexTips(ref regexTips, innerText, "【" + condition.description + "(" + condition.name + ")】:【" + innerText + "】未符合【" + condition.format + "】日期格式");
                        }
                    }
                }
                foreach (object key in (IEnumerable)requireCondiction.Keys)
                {
                    if (!string.IsNullOrEmpty((string)requireCondiction[key]))
                    {
                        str2 = str2 + "【" + requireCondiction[key] + "(" + key + ")】值未填写";
                        flag1 = false;
                    }
                }
                if (!flag1)
                    throw new Exception("必填校验中" + str2);
                if (!flag2)
                    throw new Exception("正则校验中" + regexTips);
            }
            catch (Exception ex)
            {
                throw new Exception("中查询条件解析失败:" + ex.Message);
            }
            return dataResource;
        }

        private static bool GetRegexTips(ref string regexTips, string value, string tips)
        {
            regexTips += tips;
            return false;
        }

        public string GetMyQueryReportTask()
        {
            lock (ServerCore.sendLock)
            {
                try
                {
                    if (ServerCore.UnSendReportList.Any<string>())
                    {
                        string path = ServerCore.UnSendReportList.First<string>();
                        if (Path.GetFileName(path).StartsWith(ReportQueryJob.msgTypes))
                        {
                            ServerCore.UnSendReportList.RemoveAt(0);
                            AbstractLog.logger.Info((object)("领取待查询任务【" + path + "】成功"));
                            return path;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"领取待查询任务失败！", ex);
                }
                return "";
            }
        }

        private bool SendHttpsMessage(
            string msgType,
            string fullPath,
            string httpQueryReport,
            ref QueryDataResourcesResult jsonResult)
        {
            Stopwatch stopwatch1 = new Stopwatch();
            long wholeMilliseconds = 0;
            stopwatch1.Start();
            string url = this.GetUrl();
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("macAddress", ServerCore.MacAddress),
                new RequestParam("messageType", msgType),
                new RequestParam("messageText", httpQueryReport),
                new RequestParam("queryMode", ReportQueryJob.queryMode)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            string str1;
            try
            {
                str1 = HttpHelper.HttpPostForType2(url, urlEncodePostStr);
            }
            catch (Exception ex)
            {
                string logStr = "报文【" + Path.GetFileNameWithoutExtension(fullPath) + "】查询数据资源异常,请稍后再试！";
                ServerCore.AddMainLog(logStr, EnumLogLevel.Error);
                AbstractLog.logger.Error((object)(logStr + "(" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch1) + "ms，查询操作总耗时：" + (object)wholeMilliseconds + "ms)"), ex);
                ReportSendJob.MoveToFailDirectory(fullPath, ex.Message);
                return false;
            }
            try
            {
                jsonResult = JsonConvert.DeserializeObject<QueryDataResourcesResult>(str1);
            }
            catch (Exception ex)
            {
                string logStr = "报文【" + Path.GetFileNameWithoutExtension(fullPath) + "】查询数据资源结果转换异常,请稍后再试！";
                ReportSendJob.MoveToFailDirectory(fullPath, ex.Message);
                ServerCore.AddMainLog(logStr, EnumLogLevel.Error);
                AbstractLog.logger.Error((object)(logStr + "(" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch1) + "ms，查询操作总耗时：" + (object)wholeMilliseconds + "ms)"), ex);
                return false;
            }
            if (!jsonResult.result)
            {
                string str2 = "报文【" + Path.GetFileNameWithoutExtension(fullPath) + "】查询数据资源异常(errorCode:" + jsonResult.errorCode + "):" + jsonResult.description;
                ReportSendJob.MoveToFailDirectory(fullPath, str2);
                ServerCore.AddMainLog(str2, EnumLogLevel.Error);
                AbstractLog.logger.Error((object)(str2 + "(" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch1) + "ms，查询操作总耗时：" + (object)wholeMilliseconds + "ms)"));
                return false;
            }
            if (jsonResult.queryDataResources == null)
            {
                string str3 = "报文【" + Path.GetFileNameWithoutExtension(fullPath) + "】的返回查询结果为空！";
                AbstractLog.logger.Error((object)(str3 + "(" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch1) + "ms，查询操作总耗时：" + (object)wholeMilliseconds + "ms)"));
                ReportSendJob.MoveToFailDirectory(fullPath, str3);
                ServerCore.AddMainLog(str3, EnumLogLevel.Error);
                return false;
            }
            if (jsonResult.queryDataResources.messageText.Length > 0)
            {
                string message = "查询数据资源成功！(" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch1, true) + "ms)返回描述：" + jsonResult.description;
                ReportReceiptDownloadJob.AddDownLoadedReceiptReportSum();
                AbstractLog.logger.Info((object)message);
            }
            if (jsonResult.queryDataResources.GetMessageZiped())
            {
                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                try
                {
                    jsonResult.queryDataResources.messageText = CGZipUtil.GZipDecompressString(Base64Helper.Base64Decode(jsonResult.queryDataResources.messageText));
                }
                catch (Exception ex)
                {
                    ReportSendJob.MoveToFailDirectory(fullPath, ex.Message);
                    AbstractLog.logger.Error((object)(Path.GetFileNameWithoutExtension(fullPath) + "解压报文异常：" + LogHelper.GetAttributesNValueFromObject((object)jsonResult.queryDataResources, "queryDataResources")), ex);
                }
                stopwatch2.Stop();
                AbstractLog.logger.Info((object)(jsonResult.queryDataResources.messageId + "解压报文耗时：" + (object)stopwatch2.ElapsedMilliseconds + "ms"));
            }
            if (FileHelper.SaveAsFile(ClientConfig.GetDirByRecipType(jsonResult.queryDataResources.messageType), jsonResult.queryDataResources.messageId + ".xml", jsonResult.queryDataResources.messageText))
            {
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileNameWithoutExtension(fullPath) + ".xml】的查询结果回执【" + jsonResult.queryDataResources.messageId + ".xml】保存成功!"));
                ReportReceiptWritingJob.AddReceiptWritedSum();
                ServerCore.AddMainLog("报文【" + Path.GetFileNameWithoutExtension(fullPath) + ".xml】的查询结果回执【" + jsonResult.queryDataResources.messageId + ".xml】保存成功!", EnumLogLevel.Important);
                return true;
            }
            string str4 = "报文【" + Path.GetFileNameWithoutExtension(fullPath) + ".xml】的查询结果回执【" + jsonResult.queryDataResources.messageId + ".xml】保存到【" + ServerCore.clientConfig.ReceiptReceiveDir + "】时发生异常！";
            ReportSendJob.MoveToFailDirectory(fullPath, str4);
            AbstractLog.logger.Error((object)str4);
            ServerCore.AddMainLog(str4, EnumLogLevel.Error);
            return false;
        }

        public string TranslateSign(
            string MessageId,
            string MessageType,
            string Sender,
            string Receiver,
            string SendTime,
            string MessageText)
        {
            StringBuilder stringBuilder = new StringBuilder(XmlHelp.GetXmlContent("./tpl/SendReport_tpl.xml", Encoding.UTF8)).Replace("${Data:MessageId}", MessageId).Replace("${Data:MessageType}", MessageType).Replace("${Data:Sender}", Sender).Replace("${Data:Receiver}", Receiver).Replace("${Data:SendTime}", SendTime);
            if (Encoding.UTF8.GetByteCount(MessageText) > ServerCore.sysConfig.GetHttpGzipMinsize())
            {
                int num = 0;
                try
                {
                    num = Encoding.UTF8.GetByteCount(MessageText);
                    AbstractFormLog.logger.Info((object)(MessageId + "内容[" + (object)num + "]B大于[" + (object)ServerCore.sysConfig.GetHttpGzipMinsize() + "]B，报文内容压缩中"));
                    stringBuilder = stringBuilder.Replace("<ZipFlag>2</ZipFlag>", "<ZipFlag>1</ZipFlag>");
                    MessageText = CGZipUtil.GetStringByDataset(MessageText);
                    AbstractFormLog.logger.Info((object)(MessageId + "报文内容压缩完毕压缩率：[" + (object)((double)stringBuilder.Length * 100.0 / (double)num) + "%]！"));
                }
                catch (Exception ex)
                {
                    AbstractFormLog.logger.Error((object)(MessageId + "压缩异常!\n原文件内容大小：" + (object)num + "\n系统配置压缩阈值：" + (object)ServerCore.sysConfig.GetHttpGzipMinsize() + "\n原文内容：\n" + LogHelper.CutLongValue(MessageText)), ex);
                    throw new Exception(MessageId + "压缩异常!", ex);
                }
            }
            string newValue = "";
            try
            {
                newValue = Base64Helper.Base64Encode(MessageText);
                stringBuilder = stringBuilder.Replace("${Data:EncryptData}", newValue);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(stringBuilder.ToString());
                return XmlHelp.SignXmlDoc(ServerCore.downloadConfig.rsaPrivate, xmlDoc, "");
            }
            catch (Exception ex)
            {
                AbstractFormLog.logger.Error((object)(MessageId + "传输加签异常!\npoint_tpl：\n" + LogHelper.CutLongValue(MessageText) + "\nencryptedReport：\n" + LogHelper.CutLongValue(newValue) + "\nxmlDoc：\n" + LogHelper.CutLongValue(stringBuilder.ToString())), ex);
                throw new Exception("添加传输签异常!", ex);
            }
        }

        public string GetUrl()
        {
            if (ServerCore.IsTestModel)
                return "https://localhost:8080/shenzhendtsp_web/client/declare/queryDataResource.action";
            string queryServicesUrl = ServerCore.DataQueryServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? queryServicesUrl + "/client/declare/queryDataResource.action" : queryServicesUrl + "/client/declare/queryDataResource.action") : queryServicesUrl + "/client/declare/queryDataResource.action";
        }
    }
}
