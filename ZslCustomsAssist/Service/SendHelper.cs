using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Server;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Jobs;
using ZslCustomsAssist.Server.Rest;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using com.szcport;
using System.Net;
using System.Xml;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.MQ.MQQueueProxy;
using IBM.WMQ;
using System.Reflection.Metadata;

namespace ZslCustomsAssist.Service
{
    public class SendHelper : AbstractHandle, IServiceHandle
    {
        public string privateKeyStr;
        public string publicKeyStr;
        public static readonly string msgTypes = "SZCPORTDR";

        public SendHelper() 
        {
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(1024, new CspParameters()
            {
                Flags = CspProviderFlags.UseMachineKeyStore
            });
            this.privateKeyStr = cryptoServiceProvider.ToXmlString(true);
            this.publicKeyStr = cryptoServiceProvider.ToXmlString(false);
        }

        public bool SendReport(string fullPath, string content)
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
            string str1 = strArray[0];
            ReceiveMessageType messageTypeByCode = ServerCore.GetMessageTypeByCode(str1);
            if (messageTypeByCode == null)
            {
                ReportSendJob.MoveToFailDirectory(fullPath, "报文类型【" + str1 + "】未开通业务申请");
                AbstractLog.logger.Error((object)(withoutExtension + "，报文类型【" + str1 + "】未开通业务申请，已移动到【发送失败目录】!"));
                ServerCore.AddMainLog(withoutExtension + "，报文类型【" + str1 + "】未开通业务申请，已移动到【发送失败目录】!", EnumLogLevel.Error);
                return false;
            }
            string Sender = strArray[1];
            string str2 = strArray[2];
            byte[] numArray = (byte[])null;
            try
            {
                if (fullPath.EndsWith(".xml") && !str1.Equals("SZCPORTACMP", StringComparison.Ordinal) && !str1.Equals("SZCPORTEDOC", StringComparison.Ordinal))
                {
                    using (StreamReader streamReader = new StreamReader(fullPath, Encoding.UTF8))
                        numArray = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
                }
                else
                    numArray = FileHelper.GetBytes(fullPath);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("【" + fullPath + "】文件读取异常！"), ex);
                ServerCore.AddMainLog(withoutExtension + "文件读取异常(" + ex.Message + ")", EnumLogLevel.Error);
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
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + ".xml】报文内容加载,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + ".xml】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
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
                        ReportSendJob.MoveToFailDirectory(fullPath, "报文类型【" + str1 + "】<SignInfo>节点必填！");
                        AbstractLog.logger.Info((object)(withoutExtension + "，报文类型【" + str1 + "】<SignInfo>节点必填，已移动到【发送失败目录】!"));
                        ServerCore.AddMainLog(withoutExtension + "，报文类型【" + str1 + "】<SignInfo>节点必填，已移动到【发送失败目录】!", EnumLogLevel.Error);
                        return false;
                    }
                    try
                    {
                        xmlNode3.InnerText = SPSecureAPI.SpcSignData(xmlNode3.InnerText);
                    }
                    catch
                    {
                        ReportSendJob.MoveToFailDirectory(fullPath, "报文类型【" + str1 + "】<SignInfo>节点加签失败！");
                        AbstractLog.logger.Info((object)(withoutExtension + "，报文类型【" + str1 + "】<SignInfo>节点加签失败，已移动到【发送失败目录】!"));
                        ServerCore.AddMainLog(withoutExtension + "，报文类型【" + str1 + "】<SignInfo>节点加签失败，已移动到【发送失败目录】!", EnumLogLevel.Error);
                        return false;
                    }
                    numArray = Encoding.UTF8.GetBytes(xmlDocument.InnerXml);
                }
                catch
                {
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填(节点值为空也可)！");
                    AbstractLog.logger.Info((object)(withoutExtension + "，报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填，已移动到【发送失败目录】!"));
                    ServerCore.AddMainLog(withoutExtension + "，报文类型【" + str1 + "】<NqamSign><SignInfo><CertNo>节点必填，已移动到【发送失败目录】!", EnumLogLevel.Error);
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
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + ".xml】报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + ".xml】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
                    return false;
                }
                if (DeclareMessageXmlSign.IsSignatured(xmlDocument.ChildNodes))
                {
                    AbstractLog.logger.Info((object)("当前报文[" + fullPath + "]已加签！"));
                }
                else
                {
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
                                ReportSendJob.MoveToFailDirectory(fullPath, "无卡模式不支持当前报文[" + fullPath + "]加签操作！");
                                AbstractLog.logger.Info((object)("无卡模式不支持当前报文[" + fullPath + "]加签操作！已移动到【发送失败目录】!"));
                                ServerCore.AddMainLog("无卡模式不支持当前报文[" + fullPath + "]加签操作！已移动到【发送失败目录】!", EnumLogLevel.Error);
                                return false;
                            }
                            str3 = DeclareMessageXmlSign.XmlSign(str3, this.privateKeyStr, this.publicKeyStr);
                            AbstractLog.logger.Info((object)("【" + fullPath + "】卡加签成功！"));
                        }
                        numArray = Encoding.UTF8.GetBytes(str3);
                    }
                    catch (Exception ex)
                    {
                        ReportSendJob.MoveToFailDirectory(fullPath, "加签失败:" + ExceptionHelper.GetErrorMsg(ex));
                        AbstractLog.logger.Error((object)("【" + withoutExtension + ".xml】加签失败,已移动到【发送失败目录】\nreportContent：" + LogHelper.CutLongValue(str3)), ex);
                        ServerCore.AddMainLog("【" + withoutExtension + ".xml】加签失败,已移动到【发送失败目录】", EnumLogLevel.Error);
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
                    ReportSendJob.MoveToFailDirectory(fullPath, "报文内容加载失败:" + ExceptionHelper.GetErrorMsg(ex));
                    AbstractLog.logger.Error((object)("【" + withoutExtension + ".xml】报文内容加载失败,已移动到【发送失败目录】！"), ex);
                    ServerCore.AddMainLog("【" + withoutExtension + ".xml】报文内容加载失败,已移动到【发送失败目录】", EnumLogLevel.Error);
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
                ReportSendJob.MoveToFailDirectory(fullPath, "目标报文类型无法找到：" + ExceptionHelper.GetErrorMsg(ex));
                ServerCore.AddMainLog("报文类型（" + str1 + "）业务编号（" + str2 + "）无法匹配，【" + withoutExtension + "】已移动到【发送失败目录】！", EnumLogLevel.Error);
                AbstractLog.logger.Info((object)("报文类型（" + str1 + "）业务编号（" + str2 + "）无法匹配，【" + withoutExtension + "】已移动到【发送失败目录】"), ex);
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
                ReportSendJob.MoveToFailDirectory(fullPath, ex.Message);
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
                    stopwatch.Stop();
                    AbstractLog.logger.Info((object)("报文处理共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
                }
            }
            else
            {
                bool flag = this.SendHttpsMessage(fullPath, str1, xmlMessage, ref jsonResult);
                stopwatch.Stop();
                AbstractLog.logger.Info((object)("报文处理共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
                if (!flag)
                    return false;
            }
            if (jsonResult.result)
            {
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送成功！"));
                ReportSendJob.DeleteFile(fullPath);
                return true;
            }
            if (jsonResult.errorCode != null && jsonResult.errorCode.StartsWith("-"))
            {
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送失败（" + jsonResult.errorCode + "）：" + jsonResult.description + ",进入重发..."));
                ReportSendJob.AppendToUnSendReportList(fullPath, true);
            }
            else
            {
                ReportSendJob.MoveToFailDirectory(fullPath, "报文发送失败，描述：" + jsonResult.description);
                AbstractLog.logger.Info((object)("报文【" + Path.GetFileName(fullPath) + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！"));
                ServerCore.AddMainLog("报文【" + Path.GetFileName(fullPath) + "】发送失败:" + jsonResult.description + ",已移动到【发送失败目录】！", EnumLogLevel.Error);
            }
            return false;
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

        private string GetHttpsUrl()
        {
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? (!(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/client/declare/sendMessage.action" : cloudServicesUrl + "/client/declare/sendMessage.action") : cloudServicesUrl + "/client/declare/sendMessage.action";
        }
    }
}
