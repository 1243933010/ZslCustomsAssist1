﻿using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Runtime;
using com.szcport;
using ZslCustomsAssist.MQ.MQQueueProxy;
using IBM.WMQ;
using ZslCustomsAssist.Service.Rest;
using ZslCustomsAssist.Service;
using static System.Reflection.Metadata.BlobBuilder;
using ZslCustomsAssist.User;

namespace ZslCustomsAssist.Jobs
{
    internal class ReportSendJob : AbstractLog
    {
        public static int ranNum = 0;
        public static List<ThreadExt> reportSendThreads = new();
        public string privateKeyStr;
        public string publicKeyStr;
        public static string unSendListName = "待发送集合";
        public static readonly string msgTypes = "SZCPORTDR";

        public ReportSendJob()
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
                //AbstractLog.logger.Debug((object)"等待1秒钟开启文件监控，等候中……");
                Thread.Sleep(1000);
                AbstractLog.logger.Debug((object)"报文发送业务线程，开始!");
                this.CtrlReportSendThreads();
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"报文发送线程异常！", ex);
            }
        }

        public void CtrlReportSendThreads()
        {
            AbstractLog.logger.Info((object)("开始处理报文发送业务，预计开启线程数：" + (object)ServerCore.downloadConfig.GetReceiveThread()));
            while (ServerCore.IsAllowToDealWithReport())
            {
                Thread.Sleep(1000);
                try
                {
                    if (ReportSendJob.reportSendThreads.Count < ServerCore.downloadConfig.GetReceiveThread())
                    {
                        CancellationTokenSource TokenSource = new();
                        //ThreadsHelper.AddThreads(ReportSendJob.reportSendThreads, new Action(() => this.OnDoReportSend(TokenSource)), TokenSource);
                       // AbstractLog.logger.Info((object)("成功添加报文发送线程！当前线程数为：" + (object)ReportSendJob.reportSendThreads.Count + "/" + (object)ServerCore.downloadConfig.GetReceiveThread()));
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)ex);
                }
            }
        }

        private void OnDoReportSend(CancellationTokenSource TokenSource)

        {
            AbstractLog.logger.Error((object)("执行了OnDoReportSend！"));
            while (!TokenSource.IsCancellationRequested)
            {
                string mySendReportTask;
                do
                {
                    if (ReportSendJob.reportSendThreads.Count <= ServerCore.downloadConfig.GetReceiveThread())
                    {
                        if (ServerCore.IsAllowToDealWithReport())
                        {
                            Thread.Sleep(ServerCore.downloadConfig.GetReceivePeriod());
                            mySendReportTask = this.GetMySendReportTask();
                           // AbstractLog.logger.Error((object)("mySendReportTask-----！"+ mySendReportTask));
                        }
                        else
                        {
                            goto label_3;
                        }


                    }
                    else {
                        
                        goto label_1;
                    }
                        
                }
                 
                while (string.IsNullOrWhiteSpace(mySendReportTask));
                AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----000！"));
                if (IOHelper.CheakFileIsWriting(mySendReportTask))
                {
                    AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----111！"));
                    if (System.IO.File.Exists(mySendReportTask))
                    {
                        AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----222！"));

                        //AbstractLog.logger.Info((object)("待发送报文【" + mySendReportTask + "】仍在写入中!"));
                        ReportSendJob.AppendToUnSendReportList(mySendReportTask);
                    }
                    else
                    {
                        AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----333！"));

                        ServerCore.AddReportScanedSum(-1);
                        AbstractLog.logger.Error((object)("待发送报文【" + mySendReportTask + "】由于未知原因丢失！已完成对：【" + mySendReportTask + "】的处理！"));
                    }
                }
                else
                {
                    AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----444！"));

                    bool flag = false;
                    try
                    {
                        AbstractLog.logger.Error((object)("while (string.IsNullOrWhiteSpace(mySendReportTask))-----555！"));

                        ServerCore.AddReportSendingCount();
                        flag = this.SendReport(mySendReportTask);
                        
                       // MessageBox.Show(mySendReportTask.ToString());
                        ///AbstractLog.logger.Error((mySendReportTask)str);
                        // AbstractLog.logger.Error((object)("【" + mySendReportTask + "】发送异常!\r\n" + ExceptionHelper.GetErrorMsg(ex)));

                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)("【" + mySendReportTask + "】发送异常!\r\n" + ExceptionHelper.GetErrorMsg(ex)));
                    }
                    finally
                    {

                        if (flag)
                            ServerCore.AddReportSendSuccessSum();
                        else
                            ServerCore.AddReportSendFailSum();
                        ServerCore.AddReportSendingCount(-1);
                        //AbstractLog.logger.Info((object)("已完成对【" + mySendReportTask + "】的处理," + (object)ServerCore.downloadConfig.GetReceivePeriod() + "毫秒后处理下一个任务！"));
                    }
                }
            }
        label_1:
            //AbstractLog.logger.Info((object)("报文发送线程数减少至[" + (object)ServerCore.downloadConfig.GetReceiveThread() + "]，线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ReportSendJob.reportSendThreads, ServerCore.sendLock);
            return;
        label_3:
            AbstractLog.logger.Info((object)("程序退出中/更新中/卡状态异常,不再处理任何报文！线程[" + (object)Thread.CurrentThread.ManagedThreadId + "]从线程池中移除"));
            ThreadsHelper.RemoveCurrentThreadFromThreadList(ReportSendJob.reportSendThreads, ServerCore.sendLock);
        }

     
private bool SendReport(string fullPath)
        {
            AbstractLog.logger.Error((object)("执行了SendReport！"));
            SPSecureAPI.SpcVerifyPIN(ServerCore.clientConfig.TypistPassword);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            byte[] numArray = (byte[])null;
            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            ReportMessage report = null;
            string sourceFullFileName = fullPath;

            try
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    using StreamReader streamReader = new(fullPath, Encoding.UTF8);
                    string reportContent = Encrypter.DefaultDecodeAES(streamReader.ReadToEnd());
                    report = JsonConvert.DeserializeObject<ReportMessage>(reportContent);
                    fullPath = Path.Combine(Path.GetFullPath(fullPath), withoutExtension + ".xml");
                    numArray = Encoding.UTF8.GetBytes(report.XmlMessage);
                    AbstractLog.logger.Info((string)("【" + JsonConvert.SerializeObject(report) + "】解密后得report数据-------------！"));
                    if (report == null)
                    {
                        throw new Exception("文件为空或内容有误");
                    }
                }
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("【" + fullPath + "】文件读取异常！"), ex);
                ServerCore.AddMainLog(withoutExtension + extension + "文件读取异常(" + ex.Message + ")", EnumLogLevel.Error);
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, withoutExtension + extension + "文件读取异常(" + ex.Message + ")");
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "文件读取异常！\n" + ex.Message + "\n" + ex.StackTrace);
                return false;
            }

            string[] strArray = withoutExtension.Split('_');
            if (strArray.Length < 4)
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, "报文命名格式不正确");
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文命名格式不正确");
                ServerCore.AddMainLog(withoutExtension + extension + "报文命名格式不正确，已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀", EnumLogLevel.Error);
                AbstractLog.logger.Error((object)(withoutExtension + extension + "报文命名格式不正确，已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀"));
                return false;
            }
            for (int index = 0; index < 4 && index < strArray.Length; ++index)
            {
                if (index < 3 && strArray[index].IndexOf(" ") != -1)
                {
                    if (sourceFullFileName.EndsWith(".report"))
                    {
                        SendReportStatus(report.ID, report.Type, -1, "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！");
                    }
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！");
                    AbstractLog.logger.Error((object)(withoutExtension + extension + "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀"));
                    ServerCore.AddMainLog(withoutExtension + extension + "报文命名【报文类型】/【发送人标识】/【接收人标识】中不允许带有空格！已移动到【发送失败目录】!, 正确格式：报文类型+”_” + 发送人标识（企业EDI编号）+”_” +接收人标识+”_” + 总署公告业务报文命名规则.文件后缀", EnumLogLevel.Error);
                    return false;
                }
            }
            string str1 = strArray[0];
            ReceiveMessageType messageTypeByCode = ServerCore.GetMessageTypeByCode(str1);
            if (messageTypeByCode == null)
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, "报文类型【" + str1 + "】未开通业务申请");
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文类型【" + str1 + "】未开通业务申请");
                AbstractLog.logger.Error((object)(withoutExtension + extension + "，报文类型【" + str1 + "】未开通业务申请，已移动到【发送失败目录】!"));
                ServerCore.AddMainLog(withoutExtension + extension + "，报文类型【" + str1 + "】未开通业务申请，已移动到【发送失败目录】!", EnumLogLevel.Error);
                return false;
            }
            string Sender = strArray[1];
            string str2 = strArray[2];

            try
            {
                if (!sourceFullFileName.EndsWith(".report"))
                {
                    if (fullPath.EndsWith(".xml") && !str1.Equals("SZCPORTACMP", StringComparison.Ordinal) && !str1.Equals("SZCPORTEDOC", StringComparison.Ordinal))
                    {
                        using StreamReader streamReader = new(fullPath, Encoding.UTF8);
                        numArray = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
                    }
                    else
                        numArray = FileHelper.GetBytes(fullPath);
                }
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("【" + fullPath + "】文件读取异常！"), ex);
                ServerCore.AddMainLog(withoutExtension + extension + "文件读取异常(" + ex.Message + ")", EnumLogLevel.Error);
                ReportSendJob.MoveToFailDirectory(fullPath, "文件读取异常！\n" + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
            if (numArray.Length >= 3 && numArray[0] == (byte)239 && numArray[1] == (byte)187 && numArray[2] == (byte)191)
            {
                byte[] destinationArray = new byte[numArray.Length - 3];
                Array.Copy((Array)numArray, 3, (Array)destinationArray, 0, destinationArray.Length);
                numArray = destinationArray;
            }
            if ("SZCPORTNqam".Equals(str1, StringComparison.OrdinalIgnoreCase))
            {
                string xml = Encoding.UTF8.GetString(numArray);
                XmlDocument xmlDocument = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                try
                {
                    xmlDocument.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    if (sourceFullFileName.EndsWith(".report"))
                    {
                        SendReportStatus(report.ID, report.Type, -1, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    }
                    ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + extension + "】报文内容加载,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + extension + "】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                    return false;
                }
                XmlElement documentElement = xmlDocument.DocumentElement;
                try
                {
                    XmlNode xmlNode1 = documentElement.GetElementsByTagName("NqamSign")[0];
                    XmlNode xmlNode2 = ((XmlElement)xmlNode1).GetElementsByTagName("CertNo")[0];
                    XmlNode xmlNode3 = ((XmlElement)xmlNode1).GetElementsByTagName("SignInfo")[0];
                    if (string.IsNullOrEmpty(xmlNode2.InnerText))
                        xmlNode2.InnerText = ServerCore.userData.szCertNo;
                    if (string.IsNullOrEmpty(xmlNode3.InnerText))
                    {
                        if (sourceFullFileName.EndsWith(".report"))
                        {
                            SendReportStatus(report.ID, report.Type, -1, "报文类型【" + str1 + "】<SignInfo>节点必填！");
                        }
                        ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文类型【" + str1 + "】<SignInfo>节点必填！");
                        AbstractLog.logger.Info((object)(withoutExtension + extension + "，报文类型【" + str1 + "】<SignInfo>节点必填，已移动到【发送失败目录】!"));
                        ServerCore.AddMainLog(withoutExtension + extension + "，报文类型【" + str1 + "】<SignInfo>节点必填，已移动到【发送失败目录】!", EnumLogLevel.Error);
                        return false;
                    }
                    try
                    {
                        xmlNode3.InnerText = SPSecureAPI.SpcSignData(xmlNode3.InnerText);
                    }
                    catch
                    {
                        if (sourceFullFileName.EndsWith(".report"))
                        {
                            SendReportStatus(report.ID, report.Type, -1, "报文类型【" + str1 + "】<SignInfo>节点加签失败！");
                        }
                        ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文类型【" + str1 + "】<SignInfo>节点加签失败！");
                        AbstractLog.logger.Info((object)(withoutExtension + extension + "，报文类型【" + str1 + "】<SignInfo>节点加签失败，已移动到【发送失败目录】!"));
                        ServerCore.AddMainLog(withoutExtension + extension + "，报文类型【" + str1 + "】<SignInfo>节点加签失败，已移动到【发送失败目录】!", EnumLogLevel.Error);
                        return false;
                    }
                    numArray = Encoding.UTF8.GetBytes(xmlDocument.InnerXml);
                }
                catch
                {
                    if (sourceFullFileName.EndsWith(".report"))
                    {
                        SendReportStatus(report.ID, report.Type, -1, "报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填(节点值为空也可)！"); 
                    }
                    ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填(节点值为空也可)！");
                    AbstractLog.logger.Info((object)(withoutExtension + extension + "，报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填，已移动到【发送失败目录】!"));
                    ServerCore.AddMainLog(withoutExtension + extension + "，报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填，已移动到【发送失败目录】!", EnumLogLevel.Error);
                    return false;
                }
            }
            if (messageTypeByCode.hasSign == "1" && fullPath.EndsWith(".xml"))
            {
                string str3 = Encoding.UTF8.GetString(numArray);
                XmlDocument xmlDocument = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                try
                {
                    xmlDocument.LoadXml(str3);
                }
                catch (Exception ex)
                {
                    if (sourceFullFileName.EndsWith(".report"))
                    {
                        SendReportStatus(report.ID, report.Type, -1, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    }
                    ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + extension + "】报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + extension + "】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                    return false;
                }
                if (DeclareMessageXmlSign.IsSignatured(xmlDocument.ChildNodes))
                {
                    AbstractLog.logger.Info((object)("当前报文[" + fullPath + "]已加签！"));
                }
                else
                {
                    AbstractLog.logger.Info((object)("【" + ServerCore.supportedSignType+"{{{{}}}}" + fullPath +"------======="+ this.privateKeyStr+"-------=======" + publicKeyStr + "】打印+++++++++++++++！"));
                    try
                    {
                        string supportedSignType = ServerCore.supportedSignType;
                        string str4 = supportedSignType;
                        if (!(str4 == "ICCard"))
                        {
                            if (!(str4 == "publicEncryptor"))
                            {
                                if (str4 == "privateEncryptor")
                                {
                                    str3 = DeclareMessageXmlSign.XmlSignSwxa(str3, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                    AbstractLog.logger.Info((object)("【" + fullPath + "】物理加密机加签成功！"));
                                }
                            }
                            else
                            {
                                str3 = DeclareMessageXmlSign.XmlSignSwxa(str3, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                AbstractLog.logger.Info((object)("【" + fullPath + "】云加签成功！"));
                            }
                        }
                        else
                        {
                            if (ServerCore.IsWithOutCard)
                            {
                                if (sourceFullFileName.EndsWith(".report"))
                                {
                                    SendReportStatus(report.ID, report.Type, -1, "无卡模式不支持当前报文[" + fullPath + "]加签操作！");
                                }
                                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "无卡模式不支持当前报文[" + fullPath + "]加签操作！");
                                AbstractLog.logger.Info((object)("无卡模式不支持当前报文[" + fullPath + "]加签操作！已移动到【发送失败目录】!"));
                                ServerCore.AddMainLog("无卡模式不支持当前报文[" + fullPath + "]加签操作！已移动到【发送失败目录】!", EnumLogLevel.Error);
                                return false;
                            }

                            String defalutString = str3;
                            //str3 = DeclareMessageXmlSign.XmlSign(str3, this.privateKeyStr, this.privateKeyStr);
                            str3 = DeclareMessageXmlSign.XmlSign(str3, this.privateKeyStr, this.publicKeyStr);

                           // AbstractLog.logger.Info((object)("【" + fullPath + this.privateKeyStr+ publicKeyStr + "】卡加签成功1！"));

                            AbstractLog.logger.Info((string)("【" + str3+"=========="+report.RequestPropName + "】卡加签成功2！"));

                            //-------------
                           

                            string orderNoXml = new ApiService().MidStrEx(str3, "<ceb:orderNo>", "</ceb:orderNo>");
                            string SignatureValueXml = new ApiService().MidStrEx(str3, "<ds:SignatureValue>", "</ds:SignatureValue>");
                            string ebpCodeXml = new ApiService().MidStrEx(str3, "<ceb:ebpCode>", "</ceb:ebpCode>");
                            string CEBType = "";
                            bool CEB311Message = str3.Contains("CEB311Message");
                            bool CEB621Message = str3.Contains("CEB621Message");
                            // AbstractLog.logger.Info((string)("【" + str3 + "】卡加签成功！"));
                          //   AbstractLog.logger.Info((string)("【" + orderNoXml + SignatureValueXml + CEBType + "】卡加签成功！"+ str4));

                            //  if (report.RequestPropName == "jd")
                            //  {
                            //   ReportMessage1 obj = new ReportMessage1();
                            //   obj.ID = report.ID;
                            //   obj.Type = report.Type;
                            //  obj.Guid = report.Guid;
                            //  obj.OrderNo = report.OrderNo;
                            //  obj.FileName = report.FileName;
                            //  obj.XmlMessage = str3;
                            //   new ApiService().SendDataCallbackJd(obj, str3);
                            //   AbstractLog.logger.Info((string)("返回给公司京东数据！"+ report+"&&&&&&&&&&&"+ str3+"{{}}{{{}}}}{{"));
                            //   return true;
                            //  }
                            if (CEB311Message == true)
                            {
                                CEBType = "1";
                            }
                            else if (CEB621Message == true)
                            {
                                CEBType = "2";
                            }
                            if (ebpCodeXml == "31149679BZ")  //不推送给海关，返回京东数据
                            {

                                byte[] bytes = Encoding.UTF8.GetBytes(str3);
                                string base64Str = Convert.ToBase64String(bytes);
                                new ApiService().SendDataCallback(orderNoXml, SignatureValueXml, CEBType, base64Str);
                                AbstractLog.logger.Info((string)("返回给公司京东数据！" + report + "&&&&&&&&&&&" + str3 + "{{}}{{{}}}}{{" + report.XmlMessage));
                                return false;
                            }

                            if (ebpCodeXml == "3105961682")
                            { //如果是这个字符串就不发送海关数据，发给清关系统

                                try
                                {

                                    new ApiService().SendDataCallback(orderNoXml, SignatureValueXml, CEBType, JsonConvert.SerializeObject(str3));
                                    AbstractLog.logger.Info((string)("返回给公司数据！"));
                                    return false;

                                }
                                catch (Exception ex)
                                {
                                    AbstractLog.logger.Info((string)("【" + ex.ToString() + "】-------错误信息！"));
                                }


                            }


                        }
                        AbstractLog.logger.Info((string)("【" + "----------" + "】给海关发送数据！"));
                        numArray = Encoding.UTF8.GetBytes(str3);
                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Info((object)("【" + str3 + "--------" + this.privateKeyStr + "--------" + this.publicKeyStr + ex.ToString()+ "--------" + "】*********！"));
                        if (report.RequestPropName == "jd") {
                            if (sourceFullFileName.EndsWith(".report"))
                            {
                                SendReportStatus(report.ID, report.Type, -1, "加签失败:" + ExceptionHelper.GetErrorMsg(ex));
                            }
                        }
                        ReportSendJob.MoveToFailDirectory(sourceFullFileName, "加签失败:" + ExceptionHelper.GetErrorMsg(ex));
                        AbstractLog.logger.Error((object)("【" + withoutExtension + extension + "】加签失败,已移动到【发送失败目录】\nreportContent：" + LogHelper.CutLongValue(str3)), ex);
                        ServerCore.AddMainLog("【" + withoutExtension + extension + "】加签失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                        return false;
                    }
                }
            }
            if ("SZCPORTRedEdoc".Equals(str1) && fullPath.EndsWith(".pdf"))
            {
                try
                {
                    byte[] array1 = ((IEnumerable<byte>)numArray).Skip<byte>(numArray.Length - 2048).ToArray<byte>();
                    byte[] array2 = ((IEnumerable<byte>)numArray).Take<byte>(numArray.Length - 2048).ToArray<byte>();
                    string str5 = fullPath.Remove(fullPath.LastIndexOf("."));
                    string str6 = str5.Remove(0, str5.Length - 3);
                    numArray = ((IEnumerable<byte>)Convert.FromBase64String(HttpWebResponseUtils.GetResponseString(HttpWebResponseUtils.CreateJsonPostHttpResponse(ServerCore.electSignUrl, (object)new
                    {
                        file = Convert.ToBase64String(array2),
                        code = str6,
                        checkCode = ServerCore.electSignKey
                    }, new int?(30000), (string)null, Encoding.UTF8, (CookieCollection)null), Encoding.UTF8))).Concat<byte>((IEnumerable<byte>)array1).ToArray<byte>();
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("【" + withoutExtension + ".pdf】电子签章失败->"), ex);
                    ReportSendJob.MoveToFailDirectory(fullPath, "电子签章失败:" + ExceptionHelper.GetErrorMsg(ex));
                    ServerCore.AddMainLog("【" + withoutExtension + ".pdf】电子签章失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                    return false;
                }
            }
            string signFlag = "0";
            if (ServerCore.IsVerifySign)
            {
                string xml = Encoding.UTF8.GetString(numArray);
                XmlDocument xmlDoc = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                try
                {
                    xmlDoc.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    if (sourceFullFileName.EndsWith(".report"))
                    {
                        SendReportStatus(report.ID, report.Type, -1, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    }
                    ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + extension + "】报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + extension + "】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                    return false;
                }
                XmlNode documentElement = (XmlNode)xmlDoc.DocumentElement;
                XmlNode signatured = DeclareMessageXmlSign.getSignatured(documentElement.ChildNodes);
                if (signatured != null)
                {
                    AbstractLog.logger.Info((object)("当前快件报文[" + fullPath + "]已加签！"));
                    try
                    {
                        string pubicKeyFilePath = Application.StartupPath + "\\tpl\\publickey.key";
                        signFlag = !XmlDigitalSignatureVerifier.isXmlDigitalSignatureValid(fullPath, pubicKeyFilePath) ? "2" : "1";
                        documentElement.RemoveChild(signatured);
                        numArray = XmlHelp.AsByteArray(xmlDoc);
                    }
                    catch (Exception ex)
                    {
                        signFlag = "2";
                        AbstractLog.logger.Error((object)("当前快件报文[" + fullPath + "]验签异常->"), ex);
                    }
                }
            }
            SendMessageType msgType;
            try
            {
                msgType = this.FindMsgType(str1, str2);
            }
            catch (Exception ex)
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, "目标报文类型无法找到：" + ExceptionHelper.GetErrorMsg(ex));
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "目标报文类型无法找到：" + ExceptionHelper.GetErrorMsg(ex));
                ServerCore.AddMainLog("报文类型（" + str1 + "）业务编号（" + str2 + "）无法匹配，【" + withoutExtension + extension + "】已移动到【发送失败目录】！", EnumLogLevel.Error);
                AbstractLog.logger.Info((object)("报文类型（" + str1 + "）业务编号（" + str2 + "）无法匹配，【" + withoutExtension + extension + "】已移动到【发送失败目录】"), ex);
                return false;
            }
            string transportProtocol = ServerCore.supportedTransportProtocol;
            string xmlMessage;
            try
            {
                xmlMessage = !ServerCore.IsVerifySign ? this.GetReportStr(fullPath, messageTypeByCode, msgType, Sender, str2, numArray, transportProtocol) : this.GetReportStr(fullPath, messageTypeByCode, msgType, Sender, str2, numArray, transportProtocol, signFlag);
            }
            catch (Exception ex)
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, ex.Message);
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, ex.Message);
                ServerCore.AddMainLog(ex.Message, EnumLogLevel.Error);
                return false;
            }
            ReceiptItemSendResult jsonResult = (ReceiptItemSendResult)null;
            string str7 = transportProtocol;
           
            if (!(str7 == "HTTPS"))
            {
                if (str7 == "MQ")
                {
                    this.SendMQMessage(fullPath, xmlMessage, ref jsonResult);
                    AbstractLog.logger.Info((object)("吊海关接口1-------：" + JsonConvert.SerializeObject(fullPath, Newtonsoft.Json.Formatting.Indented) + "====11111"  + "======" + JsonConvert.SerializeObject(xmlMessage, Newtonsoft.Json.Formatting.Indented) + "=====-----" + JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)));
                    //AbstractLog.logger.Info((object)("吊海关接口1-------："+ fullPath.ToString()+"======"+ xmlMessage.ToString()+"=====-----"+ jsonResult.ToString()));
                    stopwatch.Stop();
                    AbstractLog.logger.Info((object)("报文处理共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
                }
            }
            else
            {
                bool flag = this.SendHttpsMessage(fullPath, str1, xmlMessage, ref jsonResult);
                AbstractLog.logger.Info((object)("吊海关接口2-------：" + JsonConvert.SerializeObject(fullPath, Newtonsoft.Json.Formatting.Indented) + "====11111"+ JsonConvert.SerializeObject(str1, Newtonsoft.Json.Formatting.Indented) + "======" + JsonConvert.SerializeObject(xmlMessage, Newtonsoft.Json.Formatting.Indented) + "=====-----" + JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)));
                stopwatch.Stop();
                AbstractLog.logger.Info((object)("报文处理共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
                if (!flag)
                    return false;
            }
            if (jsonResult.result)
            {
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送成功！"));
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, 1, "报文【" + Path.GetFileName(fullPath) + "】发送成功！");
                }
                ReportSendJob.DeleteFile(sourceFullFileName);
                return true;
            }
            if (jsonResult.errorCode != null && jsonResult.errorCode.StartsWith("-"))
            {
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送失败（" + jsonResult.errorCode + "）：" + jsonResult.description + ",进入重发..."));
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, 2, "报文【" + Path.GetFileName(fullPath) + "】发送失败（" + jsonResult.errorCode + "）：" + jsonResult.description + ",进入重发...");
                }
                ReportSendJob.AppendToUnSendReportList(sourceFullFileName, true);
            }
            else
            {
                if (sourceFullFileName.EndsWith(".report"))
                {
                    SendReportStatus(report.ID, report.Type, -1, "报文发送失败，描述：" + jsonResult.description);
                }
                ReportSendJob.MoveToFailDirectory(sourceFullFileName, "报文发送失败，描述：" + jsonResult.description);
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！"));
                ServerCore.AddMainLog("报文【" + Path.GetFileName(fullPath) + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！", EnumLogLevel.Error);
            }
            return false;
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
                    AbstractLog.logger.Error((object)("报文【" + Path.GetFileName(fullPath) + "】删除失败!"), ex);
                    ServerCore.DeleteFailReportSendList.Add(fullPath);
                }
            }
            else
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】已不存在，无需执行删除操作!"));
        }

        private string GetReportStr(
            string fullPath,
            ReceiveMessageType dmt,
            SendMessageType smt,
            string Sender,
            string Receiver,
            byte[] fileBytes,
            string transportProcotol,
            string signFlag)
        {
            string withoutExtension1 = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string oldValue = dmt.code + "_" + Sender + "_" + Receiver + "_";
            string newValue1 = withoutExtension1.Replace(oldValue, "") + extension;
            StringBuilder stringBuilder1 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/DxpMsg_Nexp_tpl.xml", Encoding.UTF8));
            if (ReportSendJob.ranNum >= 9999)
                ReportSendJob.ranNum = 0;
            string str1 = "";
            lock (ServerCore.sendLock)
            {
                ++ReportSendJob.ranNum;
                str1 = ReportSendJob.ranNum.ToString();
            }
            while (str1.Length < 4)
                str1 = "0" + str1;
            string newValue2 = ServerCore.downloadConfig.dxpId + DateTime.Now.ToString("yyyyMMddHHmmss") + str1;
            StringBuilder stringBuilder2 = stringBuilder1.Replace("${Data:CopMsgId}", newValue2).Replace("${Data:SenderId}", ServerCore.downloadConfig.dxpId).Replace("${Data:ReceiverId}", smt.dxpId).Replace("${Data:CreatTime}", DateTime.Now.ToString("s") + "Z").Replace("${Data:MsgType}", smt.code).Replace("${Data:SignFlag}", signFlag);
            string newValue3 = "";
            if (fileBytes != null && fileBytes.Length != 0)
                newValue3 = Convert.ToBase64String(fileBytes);
            StringBuilder stringBuilder3 = stringBuilder2.Replace("${Data:Data}", newValue3).Replace("${Data:FileName}", newValue1).Replace("${Data:IcCard}", ServerCore.userData.szCardID);
            if ("1".Equals(dmt.hasTransferSign))
            {
                string str2 = stringBuilder3.ToString();
                XmlDocument xmlDocument = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                try
                {
                    xmlDocument.LoadXml(str2);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("【" + withoutExtension1 + ".xml】DXP封装报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    throw new Exception("【" + fullPath + "】DXP封装报文内容加载失败!", ex);
                }
                if (DeclareMessageXmlSign.IsSignatured(xmlDocument.ChildNodes))
                {
                    AbstractLog.logger.Info((object)("当前报文[" + fullPath + "]已加传输签！"));
                }
                else
                {
                    try
                    {
                        string supportedSignType = ServerCore.supportedSignType;
                        string str3 = supportedSignType;
                        if (!(str3 == "ICCard"))
                        {
                            if (!(str3 == "publicEncryptor"))
                            {
                                if (str3 == "privateEncryptor")
                                {
                                    str2 = DeclareMessageXmlSign.XmlSignSwxaTran(str2, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                    AbstractLog.logger.Info((object)("【" + fullPath + "】物理加密机加传输签成功！"));
                                }
                            }
                            else
                            {
                                str2 = DeclareMessageXmlSign.XmlSignSwxaTran(str2, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                AbstractLog.logger.Info((object)("【" + fullPath + "】云加签加传输签成功！"));
                            }
                        }
                        else
                        {
                            if (ServerCore.IsWithOutCard)
                            {
                                AbstractLog.logger.Info((object)("无卡模式不支持当前报文[" + fullPath + "]传输加签操作！已移动到【发送失败目录】!"));
                                throw new Exception("【" + fullPath + "】无卡模式不支持加传输签!");
                            }
                            str2 = DeclareMessageXmlSign.XmlSignTran(str2);
                            AbstractLog.logger.Info((object)("【" + fullPath + "】卡加传输签成功！"));
                        }
                        stringBuilder3 = new StringBuilder(str2);
                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)("【" + withoutExtension1 + ".xml】加传输签失败,已移动到【发送失败目录】\nreportContent：" + LogHelper.CutLongValue(str2)), ex);
                        throw new Exception("【" + fullPath + "】加传输签失败!", ex);
                    }
                }
            }
            if (transportProcotol.Equals("MQ"))
            {
                Encoding encoding1 = Encoding.GetEncoding("UTF-8");
                Encoding encoding2 = Encoding.GetEncoding("GB2312");
                byte[] bytes1 = encoding1.GetBytes(stringBuilder3.ToString());
                byte[] bytes2 = Encoding.Convert(encoding1, encoding2, bytes1);
                return encoding2.GetString(bytes2);
            }
            string withoutExtension2 = Path.GetFileNameWithoutExtension(fullPath);
            StringBuilder stringBuilder4 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/SendReport_tpl.xml", Encoding.UTF8)).Replace("${Data:MessageId}", withoutExtension2).Replace("${Data:MessageType}", dmt.code).Replace("${Data:Sender}", Sender).Replace("${Data:Receiver}", Receiver).Replace("${Data:SendTime}", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string newValue4 = "";
            try
            {
                newValue4 = Base64Helper.Base64Encode(stringBuilder3.ToString());
                stringBuilder4 = stringBuilder4.Replace("${Data:EncryptData}", newValue4);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(stringBuilder4.ToString());
                return XmlHelp.SignXmlDoc(ServerCore.downloadConfig.rsaPrivate, xmlDoc, "");
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)(withoutExtension2 + "传输加签异常!\npoint_tpl：\n" + LogHelper.CutLongValue(stringBuilder3.ToString()) + "\nencryptedReport：\n" + LogHelper.CutLongValue(newValue4) + "\nxmlDoc：\n" + LogHelper.CutLongValue(stringBuilder4.ToString())), ex);
                throw new Exception("【" + fullPath + "】传输加签异常!", ex);
            }
        }

        private string GetReportStr(
            string fullPath,
            ReceiveMessageType dmt,
            SendMessageType smt,
            string Sender,
            string Receiver,
            byte[] fileBytes,
            string transportProcotol)
        {
            string withoutExtension1 = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string oldValue = dmt.code + "_" + Sender + "_" + Receiver + "_";
            string newValue1 = withoutExtension1.Replace(oldValue, "") + extension;
            StringBuilder stringBuilder1 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/DxpMsg_tpl.xml", Encoding.UTF8));
            if (ReportSendJob.ranNum >= 9999)
                ReportSendJob.ranNum = 0;
            string str1 = "";
            lock (ServerCore.sendLock)
            {
                ++ReportSendJob.ranNum;
                str1 = ReportSendJob.ranNum.ToString();
            }
            while (str1.Length < 4)
                str1 = "0" + str1;
            string newValue2 = ServerCore.downloadConfig.dxpId + DateTime.Now.ToString("yyyyMMddHHmmss") + str1;
            StringBuilder stringBuilder2 = stringBuilder1.Replace("${Data:CopMsgId}", newValue2).Replace("${Data:SenderId}", ServerCore.downloadConfig.dxpId).Replace("${Data:ReceiverId}", smt.dxpId).Replace("${Data:CreatTime}", DateTime.Now.ToString("s") + "Z").Replace("${Data:MsgType}", smt.code);
            string newValue3 = "";
            if (fileBytes != null && fileBytes.Length != 0)
                newValue3 = Convert.ToBase64String(fileBytes);
            StringBuilder stringBuilder3 = stringBuilder2.Replace("${Data:Data}", newValue3).Replace("${Data:FileName}", newValue1).Replace("${Data:IcCard}", ServerCore.userData.szCardID);
            if ("1".Equals(dmt.hasTransferSign))
            {
                string str2 = stringBuilder3.ToString();
                XmlDocument xmlDocument = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                try
                {
                    xmlDocument.LoadXml(str2);
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)("【" + withoutExtension1 + ".xml】DXP封装报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    throw new Exception("【" + fullPath + "】DXP封装报文内容加载失败!", ex);
                }
                if (DeclareMessageXmlSign.IsSignatured(xmlDocument.ChildNodes))
                {
                    AbstractLog.logger.Info((object)("当前报文[" + fullPath + "]已加传输签！"));
                }
                else
                {
                    try
                    {
                        string supportedSignType = ServerCore.supportedSignType;
                        string str3 = supportedSignType;
                        if (!(str3 == "ICCard"))
                        {
                            if (!(str3 == "publicEncryptor"))
                            {
                                if (str3 == "privateEncryptor")
                                {
                                    str2 = DeclareMessageXmlSign.XmlSignSwxaTran(str2, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                    AbstractLog.logger.Info((object)("【" + fullPath + "】物理加密机加传输签成功！"));
                                }
                            }
                            else
                            {
                                str2 = DeclareMessageXmlSign.XmlSignSwxaTran(str2, ServerCore.userData.szCardID + ".cer", supportedSignType);
                                AbstractLog.logger.Info((object)("【" + fullPath + "】云加签加传输签成功！"));
                            }
                        }
                        else
                        {
                            if (ServerCore.IsWithOutCard)
                            {
                                AbstractLog.logger.Info((object)("无卡模式不支持当前报文[" + fullPath + "]传输加签操作！已移动到【发送失败目录】!"));
                                throw new Exception("【" + fullPath + "】无卡模式不支持加传输签!");
                            }
                            str2 = DeclareMessageXmlSign.XmlSignTran(str2);
                            AbstractLog.logger.Info((object)("【" + fullPath + "】卡加传输签成功！"));
                        }
                        stringBuilder3 = new StringBuilder(str2);
                    }
                    catch (Exception ex
                    )
                    {
                        AbstractLog.logger.Error((object)("【" + withoutExtension1 + ".xml】加传输签失败,已移动到【发送失败目录】\nreportContent：" + LogHelper.CutLongValue(str2)), ex);
                        throw new Exception("【" + fullPath + "】加传输签失败!", ex);
                    }
                }
            }
            if (transportProcotol.Equals("MQ"))
            {
                Encoding encoding1 = Encoding.GetEncoding("UTF-8");
                Encoding encoding2 = Encoding.GetEncoding("GB2312");
                byte[] bytes1 = encoding1.GetBytes(stringBuilder3.ToString());
                byte[] bytes2 = Encoding.Convert(encoding1, encoding2, bytes1);
                return encoding2.GetString(bytes2);
            }
            string withoutExtension2 = Path.GetFileNameWithoutExtension(fullPath);
            StringBuilder stringBuilder4 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/SendReport_tpl.xml", Encoding.UTF8)).Replace("${Data:MessageId}", withoutExtension2).Replace("${Data:MessageType}", dmt.code).Replace("${Data:Sender}", Sender).Replace("${Data:Receiver}", Receiver).Replace("${Data:SendTime}", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (Encoding.UTF8.GetByteCount(stringBuilder3.ToString()) > ServerCore.sysConfig.GetHttpGzipMinsize())
            {
                int num = 0;
                string rawString = "";
                try
                {
                    num = Encoding.UTF8.GetByteCount(stringBuilder3.ToString());
                    AbstractLog.logger.Info((object)(withoutExtension2 + "内容[" + (object)num + "]B大于[" + (object)ServerCore.sysConfig.GetHttpGzipMinsize() + "]B，报文内容压缩中"));
                    stringBuilder4 = stringBuilder4.Replace("<ZipFlag>2</ZipFlag>", "<ZipFlag>1</ZipFlag>");
                    rawString = stringBuilder3.ToString();
                    stringBuilder3 = new StringBuilder(CGZipUtil.GetStringByDataset(rawString));
                    AbstractLog.logger.Info((object)(withoutExtension2 + "报文内容压缩完毕压缩率（压缩后的大小/压缩前的大小）：[" + (object)((double)stringBuilder3.Length * 100.0 / (double)num) + "%]！"));
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)(withoutExtension2 + "压缩异常!\n原文件内容大小：" + (object)num + "\n系统配置压缩阈值：" + (object)ServerCore.sysConfig.GetHttpGzipMinsize() + "\n原文内容：\n" + LogHelper.CutLongValue(rawString)), ex);
                    throw new Exception(withoutExtension2 + "压缩异常!", ex);
                }
            }
            string newValue4 = "";
            try
            {
                newValue4 = Base64Helper.Base64Encode(stringBuilder3.ToString());
                stringBuilder4 = stringBuilder4.Replace("${Data:EncryptData}", newValue4);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(stringBuilder4.ToString());
                return XmlHelp.SignXmlDoc(ServerCore.downloadConfig.rsaPrivate, xmlDoc, "");
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)(withoutExtension2 + "传输加签异常!\npoint_tpl：\n" + LogHelper.CutLongValue(stringBuilder3.ToString()) + "\nencryptedReport：\n" + LogHelper.CutLongValue(newValue4) + "\nxmlDoc：\n" + LogHelper.CutLongValue(stringBuilder4.ToString())), ex);
                throw new Exception("【" + fullPath + "】传输加签异常!", ex);
            }
        }

        private SendMessageType FindMsgType(string messageType, string receiver)
        {
            List<ReceiveMessageType> receiveMessageTypes = ServerCore.downloadConfig.receiveMessageTypes;
            if (receiveMessageTypes == null || !receiveMessageTypes.Any<ReceiveMessageType>())
                throw new Exception("配置信息中目标报文类型集合为空！");
            ReceiveMessageType receiveMessageType = receiveMessageTypes.First<ReceiveMessageType>((Func<ReceiveMessageType, bool>)(rmt => rmt.code == messageType));
            if (receiveMessageType.sendMessageTypes == null)
                throw new Exception("报文类型【" + messageType + "】未找到对应目标报文类型!");
            return receiveMessageType.sendMessageTypes.First<SendMessageType>((Func<SendMessageType, bool>)(smt => smt.szcportId == receiver)) ?? throw new Exception("未找到对应目标报文类型");
        }

        private string GetHttpsUrl()
        {
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/sendMessage.action" : cloudServicesUrl + "/client/declare/sendMessage.action") : cloudServicesUrl + "/client/declare/sendMessage.action";
        }

        public static void MoveToFailDirectory(string fullPath, string failureCause)
        {
            string str1 = ServerCore.clientConfig.ReportSendFailDir;
            if (!Directory.Exists(str1))
                Directory.CreateDirectory(str1);
            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string name = new FileInfo(fullPath).Directory.Name;
            bool flag = name == new DirectoryInfo(ServerCore.clientConfig.UnReportSendDir).Name || name == new DirectoryInfo(ServerCore.clientConfig.ReportSendDir).Name;
            if (!flag)
                str1 = Path.Combine(str1, name);
            string str2 = Path.Combine(str1, withoutExtension);
            if (str2.Length >= 248)
            {
                System.IO.File.Delete(fullPath);
                AbstractLog.logger.Error((object)("报文发送失败，移到错误目录时由于【" + withoutExtension + "】文件名过长导致无法移动，追加错误日志【" + failureCause + "】异常！"));
            }
            else
            {
                if (!Directory.Exists(str2))
                    Directory.CreateDirectory(str2);
                if (failureCause != "")
                {
                    string fileName = withoutExtension + ".log";
                    try
                    {
                        if (!System.IO.File.Exists(fullPath))
                            failureCause = failureCause + "\r\n在移动文件时文件已不存在,移动失败报文" + fullPath + "不成功";
                        FileHelper.SaveAsFile(str2, fileName, failureCause);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("报文发送失败，移到错误目录时，追加错误日志异常！" + ExceptionHelper.GetErrorMsg(ex));
                    }
                }
                try
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        FileHelper.MoveAndReplace(fullPath, str2);
                        if (System.IO.File.Exists(fullPath))
                            throw new Exception("报文移动失败！");
                    }
                    if (flag)
                        return;
                    DirectoryInfo directory = new FileInfo(fullPath).Directory;
                    if (directory.Exists && directory.GetFiles().Length == 0)
                        directory.Delete();
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)fullPath, ex);
                    ServerCore.MoveFailReportSendList.Add(fullPath);
                }
            }
        }

        public static bool AppendToUnSendReportList(string fullpath, bool isInsertBefore = false)
        {
            AbstractLog.logger.Info((object)("往插入任务"));
            lock (ServerCore.sendLock)
            {
                if (isInsertBefore)
                {
                    ServerCore.UnSendReportList.Insert(0, fullpath);
                    //AbstractLog.logger.Info((object)("往" + ReportSendJob.unSendListName + "插入任务【" + fullpath + "】"));
                }
                else
                {
                    ServerCore.UnSendReportList.Add(fullpath);
                    //AbstractLog.logger.Info((object)("往" + ReportSendJob.unSendListName + "追加任务【" + fullpath + "】"));
                }
            }
            return true;
        }

        public string GetMySendReportTask()
        {
           
            lock (ServerCore.sendLock)
            {
                try
                {
                   // AbstractLog.logger.Info((object)("1111111111111111111"+ ServerCore.UnSendReportList));
                    if (ServerCore.UnSendReportList.Any<string>())
                    {
                        AbstractLog.logger.Info((object)("22222222222222222222222"));
                        string path = ServerCore.UnSendReportList.First<string>();
                        if (!Path.GetFileName(path).StartsWith(msgTypes))

                        {
                            AbstractLog.logger.Info((object)("333333333333333333333"));
                            ServerCore.UnSendReportList.RemoveAt(0);
                            AbstractLog.logger.Info((object)("领取待发送任务【" + path + "】成功"));
                            return path;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"领取待发送任务失败！", ex);
                }
                return "";
            }
        }

        private bool SendHttpsMessage(
            string fullPath,
            string messageType,
            string xmlMessage,
            ref ReceiptItemSendResult jsonResult)
        {
            string httpsUrl = this.GetHttpsUrl();
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("提交地址：" + httpsUrl));
            if (ServerCore.IsDebugModel)
                AbstractLog.logger.Info((object)("MAC地址：" + ServerCore.MacAddress));
            List<RequestParam> paramList = new List<RequestParam>()
            {
                new RequestParam("clientid", ServerCore.downloadConfig.entCode),
                new RequestParam("key", ServerCore.downloadConfig.httpapiKey),
                new RequestParam("icNo", ServerCore.userData.szCardID),
                new RequestParam("macAddress", ServerCore.MacAddress),
                new RequestParam(nameof (messageType), messageType),
                new RequestParam("messageText", xmlMessage)
            };
            string paramsString = "";
            paramList.ForEach((Action<RequestParam>)(param => paramsString = paramsString + param.Name + ":" + param.Value + "\r\n"));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
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
                AbstractLog.logger.Error((object)("报文上传（耗时【" + (object)stopwatch.ElapsedMilliseconds + "ms】）异常！【" + fullPath + "】进入重发..."), ex);
                ReportSendJob.AppendToUnSendReportList(fullPath, true);
                return false;
            }
            if (string.IsNullOrWhiteSpace(str))
            {
                AbstractLog.logger.Info((object)"报文上传接口响应结果为空，接口请求异常!");
                return false;
            }
            try
            {
                jsonResult = JsonConvert.DeserializeObject<ReceiptItemSendResult>(str);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("报文上传接口返回结果序列化异常！结果：" + str), ex);
                return false;
            }
            return true;
        }
        private void SendMQMessage(
            string fullPath,
            string xmlMessage,
            ref ReceiptItemSendResult jsonResult)
        {
            jsonResult = new ReceiptItemSendResult();
            try
            {
                IbmMQQueueProxy mqSendQueueProxy = ServerCore.mqSendQueueProxy;
                MQMessage message = new MQMessage();
                message.Encoding = 1381;
                message.CharacterSet = 1381;
                message.WriteString(xmlMessage);
                message.Format = "MQSTR   ";
                mqSendQueueProxy.HandleMessege(message);
                jsonResult.result = true;
                jsonResult.description = "采用MQ传输协议发送";
            }
            catch (MQException ex)
            {
                LogHelper.Error((Exception)ex, "使用MQ发送报文过程发生异常！");
                jsonResult.errorCode = "-1";
                jsonResult.description = "ReasonCode => " + (object)ex.ReasonCode + "(" + ((Exception)ex).Message + ")";
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "使用MQ传输预处理过程发生异常！");
                jsonResult.errorCode = "-1";
                jsonResult.description = "ReasonDisc => " + ex.Message;
            }
        }

        private void SendReportStatus(string id, int type, int status, string message)
        {
            List<ReportStatus> list = new() { 
                new ReportStatus { ID = id, Type = type, Status= status, Message = message }
            };
            try
            {
                new ApiService().UpdateReportStatus(list, out string msg);
            }
            catch (Exception ex){ }
        }
    }
}
