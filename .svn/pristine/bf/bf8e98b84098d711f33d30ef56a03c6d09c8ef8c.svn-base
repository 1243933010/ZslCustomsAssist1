using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    internal class SendReportScanJob : AbstractLog
    {
        public static readonly string DECMSGMessageType = "SZCPORTDECMSG";
        public static readonly string EdocCopIdNodeName = "EdocCopId";
        public static readonly List<string> findDECMSGXmlNames = new List<string>()
        {
            "CustomMaster",
            "SeqNo",
            "EntryId",
            "EdocID",
            "EdocFomatType",
            "EdocOwnerCode",
            "CustomMaster",
            "EdocCopId",
            "TypistNo",
            "DeclareName",
            "InputerName",
            "EdocCode",
            "OpNote"
        };

        public void OnDoJob()
        {
            if (ServerCore.clientConfig.ReportSendDir != "" && !Directory.Exists(ServerCore.clientConfig.ReportSendDir))
            {
                Directory.CreateDirectory(ServerCore.clientConfig.ReportSendDir);
            }
            while (true)
            {
                if (!ServerCore.IsExitThread)
                {
                    try
                    {
                        this.WatcherReport(ServerCore.clientConfig.ReportSendDir);
                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)"报文目录扫描异常", ex);
                    }
                    Thread.Sleep(10);
                }
                else
                    break;
            }
        }

        public void WatcherReport(string ReportSendDir)
        {
            foreach (FileInfo file1 in new DirectoryInfo(ReportSendDir).GetFiles())
            {
                string fullPath = file1.FullName;
                string fileName = Path.GetFileName(fullPath);
                if (!this.IsIllegalName(fileName))
                {
                    //AbstractLog.logger.Info((object)("报文发送目录【" + ReportSendDir + "】扫描到【" + fileName + "】(" + file1.CreationTime.ToString("yyyy-MM-dd hh:mm:ss") + "),准备往待处理目录移动"));
                    if (IOHelper.CheakFileIsWriting(fullPath))
                    {
                        //AbstractLog.logger.Info((object)("待发送报文【" + fullPath + "】仍在写入中!"));
                    }
                    else
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        long wholeMilliseconds = 0;
                        stopwatch.Start();
                        if (Path.GetExtension(fullPath).Equals(".zip", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(fullPath).StartsWith(SendReportScanJob.DECMSGMessageType))
                        {
                            List<string> stringList1 = new List<string>();
                            ServerCore.AddReportScanedSum();
                            string str1 = Path.Combine(ServerCore.clientConfig.UnReportSendDir, Path.GetFileNameWithoutExtension(fullPath));
                            try
                            {
                                AbstractLog.logger.Debug((object)("开始解压报关单压缩包【" + Path.GetFileNameWithoutExtension(fullPath) + "】..."));
                                if (!Directory.Exists(str1))
                                {
                                    Directory.CreateDirectory(str1);
                                    ZipUtil.UnZip(fullPath, str1);
                                    AbstractLog.logger.Debug((object)("解压报关单压缩包【" + Path.GetFileNameWithoutExtension(fullPath) + "】成功（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）！"));
                                }
                                else
                                {
                                    string str2 = "【" + fileName + "】的同名压缩包已被处理过！";
                                    AbstractLog.logger.Error((object)str2);
                                    ServerCore.AddMainLog(str2, EnumLogLevel.Error);
                                    ServerCore.AddReportSendFailSum();
                                    ReportSendJob.MoveToFailDirectory(fullPath, str2);
                                    continue;
                                }
                            }
                            catch (DirectoryNotFoundException ex)
                            {
                                string message = "【" + fileName + "】解压异常,请确认压缩包下无其他文件夹结构（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                                AbstractLog.logger.Error((object)message, (Exception)ex);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, message + ":" + ex.Message);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                string message = "【" + fileName + "】解压异常（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                                AbstractLog.logger.Error((object)message, ex);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, message + ":" + ex.Message);
                                continue;
                            }
                            Hashtable hashtable = new Hashtable();
                            DirectoryInfo directoryInfo = new DirectoryInfo(str1);
                            FileInfo[] files = directoryInfo.GetFiles("*.xml");
                            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
                            bool flag = true;
                            while (flag)
                            {
                                flag = false;
                                foreach (FileSystemInfo file2 in directoryInfo.GetFiles())
                                {
                                    if (IOHelper.CheakFileIsWriting(file2.FullName))
                                    {
                                        AbstractLog.logger.Debug((object)("【" + directoryInfo.Name + "】解压内容仍在写入中!"));
                                        Thread.Sleep(1000);
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            try
                            {
                                AbstractLog.logger.Debug((object)("开始解析压缩包中报关单【" + Path.GetFileNameWithoutExtension(fullPath) + "】xml内容..."));
                                if (files.Length != 1 || !Path.GetFileNameWithoutExtension(files[0].Name).Equals(withoutExtension))
                                {
                                    string str3 = "【" + fileName + "】压缩文件中只能有唯一一份与压缩包同名的报关单xml文件！";
                                    AbstractLog.logger.Error((object)str3);
                                    SendReportScanJob.HandleDecMsgGError(fullPath, str1, str3);
                                    continue;
                                }
                                string xmlContent = XmlHelp.GetXmlContent(files[0].FullName, Encoding.UTF8);
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.LoadXml(xmlContent);
                                SendReportScanJob.AddEmptyValueToEmptySelectAbleNode(xmlDocument);
                                DeclareMessageXmlSign.GetXmlInnerText(xmlDocument.ChildNodes, SendReportScanJob.findDECMSGXmlNames, hashtable);
                                stringList1.Add(withoutExtension + ".xml");
                                AbstractLog.logger.Debug((object)("解析压缩包中报关单【" + withoutExtension + "】xml内容成功（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）"));
                            }
                            catch (Exception ex)
                            {
                                string message = "【" + fullPath + "】压缩包中的报关单xml文件解析异常（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                                AbstractLog.logger.Error((object)message, ex);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, message + ":" + ex.Message);
                                continue;
                            }
                            List<string> stringList2 = (List<string>)hashtable[(object)SendReportScanJob.EdocCopIdNodeName];
                            if (stringList2 == null || stringList2.Count == 0)
                            {
                                string str4 = "压缩包中报关单【" + withoutExtension + ".xml】关联的附件数量为0！";
                                AbstractLog.logger.Error((object)str4);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, str4);
                                continue;
                            }
                            if (directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Length != stringList2.Count + 1)
                            {
                                int num = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Length - 1;
                                string str5 = "压缩包中报关单【" + withoutExtension + ".xml】关联的附件数量(" + (object)stringList2.Count + ")与压缩包中实际的附件数量(" + (object)num + ")不一致！";
                                AbstractLog.logger.Error((object)str5);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, str5);
                                continue;
                            }
                            int num1 = 0;
                            string str6 = "";
                            for (int index1 = 0; index1 < stringList2.Count; ++index1)
                            {
                                string str7 = stringList2[index1];
                                string fileName1 = Path.Combine(str1, str7);
                                if (!File.Exists(Path.Combine(str1, str7)))
                                {
                                    str6 = "压缩包中报关单【" + withoutExtension + ".xml】关联的附件原始文件名<EdocCopId>(" + str7 + ")与压缩包中对应的附件关联不上！";
                                    AbstractLog.logger.Error((object)str6);
                                    break;
                                }
                                string key = "";
                                try
                                {
                                    AbstractLog.logger.Debug((object)("开始构造压缩包【" + withoutExtension + ".zip】中附件【" + str7 + "】的随附单据报文..."));
                                    StringBuilder stringBuilder1 = new StringBuilder(XmlHelp.GetXmlContent("./tpl/DataInfo_tpl.xml", Encoding.UTF8));
                                    key = "CustomMaster";
                                    string newValue1 = ((List<string>)hashtable[(object)key])[0];
                                    StringBuilder stringBuilder2 = stringBuilder1.Replace("${" + key + "}", newValue1);
                                    key = "SeqNo";
                                    string newValue2 = ((List<string>)hashtable[(object)key])[0];
                                    StringBuilder stringBuilder3 = stringBuilder2.Replace("${" + key + "}", newValue2);
                                    key = "EntryId";
                                    string newValue3 = ((List<string>)hashtable[(object)key])[0];
                                    StringBuilder stringBuilder4 = stringBuilder3.Replace("${" + key + "}", newValue3);
                                    key = "EdocID";
                                    string newValue4 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder5 = stringBuilder4.Replace("${" + key + "}", newValue4);
                                    key = "EdocFomatType";
                                    string newValue5 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder6 = stringBuilder5.Replace("${" + key + "}", newValue5);
                                    key = "EdocOwnerCode";
                                    string newValue6 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder7 = stringBuilder6.Replace("${" + key + "}", newValue6);
                                    key = "EdocCopId";
                                    string newValue7 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder8 = stringBuilder7.Replace("${" + key + "}", newValue7);
                                    key = "DECL_TIME";
                                    string newValue8 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    StringBuilder stringBuilder9 = stringBuilder8.Replace("${" + key + "}", newValue8);
                                    key = "TypistNo";
                                    string newValue9 = ((List<string>)hashtable[(object)key])[0];
                                    StringBuilder stringBuilder10 = stringBuilder9.Replace("${" + key + "}", newValue9);
                                    StringBuilder stringBuilder11;
                                    if (hashtable[(object)"DeclareName"] == null || ((List<string>)hashtable[(object)"DeclareName"]).Count == 0)
                                    {
                                        key = "InputerName";
                                        string newValue10 = ((List<string>)hashtable[(object)key])[0];
                                        stringBuilder11 = stringBuilder10.Replace("${DECL_NAME}", newValue10);
                                    }
                                    else
                                    {
                                        key = "DeclareName";
                                        string newValue11 = ((List<string>)hashtable[(object)key])[0];
                                        stringBuilder11 = stringBuilder10.Replace("${DECL_NAME}", newValue11);
                                    }
                                    key = "EdocCode";
                                    string newValue12 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder12 = stringBuilder11.Replace("${" + key + "}", newValue12);
                                    key = "OpNote";
                                    string newValue13 = ((List<string>)hashtable[(object)key])[index1];
                                    StringBuilder stringBuilder13 = stringBuilder12.Replace("${" + key + "}", newValue13);
                                    int num2 = 2048 - Encoding.UTF8.GetBytes(stringBuilder13.ToString()).Length;
                                    for (int index2 = 0; index2 < num2; ++index2)
                                        stringBuilder13.Append(" ");
                                    FileHelper.SaveOrApppendContent(str1, str7, stringBuilder13.ToString());
                                    AbstractLog.logger.Debug((object)("压缩包【" + withoutExtension + ".zip】中附件【" + str7 + "】的随附单据报文构造成功！（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）"));
                                }
                                catch (Exception ex)
                                {
                                    string message = "为解压内容【" + str7 + "】构造随附单证的【" + key + "】时发生异常！（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                                    AbstractLog.logger.Error((object)message, ex);
                                    str6 = message + ":" + ex.Message;
                                    break;
                                }
                                try
                                {
                                    string[] strArray = withoutExtension.Split('_');
                                    string str8 = "SZCPORTE";
                                    string str9 = "";
                                    if (strArray[2].StartsWith("S"))
                                    {
                                        str8 += "DOC";
                                        str9 = "S004";
                                    }
                                    else if (strArray[2].StartsWith("Q"))
                                    {
                                        str8 += "doc";
                                        str9 = "Q002";
                                    }
                                    string path2 = str8 + "_" + strArray[1] + "_" + str9 + "_" + str7;
                                    new FileInfo(fileName1).MoveTo(Path.Combine(str1, path2));
                                    stringList1.Add(path2);
                                }
                                catch (Exception ex)
                                {
                                    string message = "在解压内容【" + str7 + "】重命名时发生异常！（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                                    AbstractLog.logger.Error((object)message, ex);
                                    str6 = message + ":" + ex.Message;
                                    break;
                                }
                                ++num1;
                            }
                            if (num1 != stringList2.Count)
                            {
                                AbstractLog.logger.Error((object)str6);
                                SendReportScanJob.HandleDecMsgGError(fullPath, str1, str6);
                                continue;
                            }
                            ServerCore.AddReportScanedSum(-1);
                            foreach (string path2 in stringList1)
                            {
                                ReportSendJob.AppendToUnSendReportList(Path.Combine(str1, path2));
                                ServerCore.AddReportScanedSum();
                                AbstractLog.logger.Debug((object)("成功添加【" + path2 + "】到待发送队列！"));
                            }
                            try
                            {
                                AbstractLog.logger.Debug((object)("开始删除原压缩包【" + fileName + "】..."));
                                if (File.Exists(fullPath))
                                {
                                    File.Delete(fullPath);
                                    AbstractLog.logger.Debug((object)("删除原压缩包【" + fileName + "】成功（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "）..."));
                                }
                                else
                                    AbstractLog.logger.Error((object)("未找到原压缩包【" + fileName + "】！"));
                            }
                            catch (Exception ex)
                            {
                                string message = "删除原压缩文件时发生异常（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "）！";
                                AbstractLog.logger.Error((object)message, ex);
                            }
                            Thread.Sleep(2);
                        }
                        else
                        {
                            string path = Path.Combine(ServerCore.clientConfig.UnReportSendDir, fileName);
                            FileInfo fileInfo = new FileInfo(fullPath);
                            if (File.Exists(path))
                            {
                                string str = "重名文件" + fileName + "(创建时间：" + fileInfo.CreationTime.ToString("yyyy-MM-dd hh:mm:ss") + ")在待处理目录已存在，请勿重复发送！";
                                AbstractLog.logger.Error((object)str);
                                ServerCore.AddMainLog(str, EnumLogLevel.Error);
                                ServerCore.AddReportSendFailSum();
                                ReportSendJob.MoveToFailDirectory(fullPath, str);
                                continue;
                            }
                            try
                            {
                                new Thread((ThreadStart)(() =>
                                {
                                    FileHelper.MoveAndReplace(fullPath, ServerCore.clientConfig.UnReportSendDir);
                                    string str = Path.Combine(ServerCore.clientConfig.UnReportSendDir, Path.GetFileName(fileName));
                                    if (File.Exists(str))
                                    {
                                        ReportSendJob.AppendToUnSendReportList(str);
                                        ServerCore.AddReportScanedSum();
                                    }
                                    else
                                        AbstractLog.logger.Error((object)("移动文件" + fileName + "异常！"));
                                })).Start();
                                Thread.Sleep(2);
                            }
                            catch (Exception ex)
                            {
                                AbstractLog.logger.Error((object)("文件" + fileName + "移动到待发送目录失败（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）"), ex);
                            }
                        }
                        stopwatch.Stop();
                        //AbstractLog.logger.Debug((object)("本次扫描耗时：" + (object)wholeMilliseconds + "ms"));
                    }
                }
            }
        }

        private static void HandleDecMsgGError(
            string fullPath,
            string targetDirectory,
            string errorTips)
        {
            ServerCore.AddMainLog(errorTips, EnumLogLevel.Error);
            ServerCore.AddReportSendFailSum();
            ReportSendJob.MoveToFailDirectory(fullPath, errorTips);
            ClearTimeOutFileJob.ClearFile(0, targetDirectory);
            Directory.Delete(targetDirectory);
        }

        private static void AddEmptyValueToEmptySelectAbleNode(XmlDocument xmlDocument)
        {
            foreach (XmlNode EdocRealationXmlNode in xmlDocument.GetElementsByTagName("EdocRealation"))
            {
                if (EdocRealationXmlNode.HasChildNodes)
                {
                    XmlNode preNode1 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocID", xmlDocument, (XmlNode)null);
                    XmlNode preNode2 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocCode", xmlDocument, preNode1);
                    XmlNode preNode3 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocFomatType", xmlDocument, preNode2);
                    XmlNode preNode4 = SendReportScanJob.AddNode(EdocRealationXmlNode, "OpNote", xmlDocument, preNode3);
                    XmlNode preNode5 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocCopId", xmlDocument, preNode4);
                    XmlNode preNode6 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocOwnerCode", xmlDocument, preNode5);
                    XmlNode preNode7 = SendReportScanJob.AddNode(EdocRealationXmlNode, "SignUnit", xmlDocument, preNode6);
                    XmlNode preNode8 = SendReportScanJob.AddNode(EdocRealationXmlNode, "SignTime", xmlDocument, preNode7);
                    XmlNode preNode9 = SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocOwnerName", xmlDocument, preNode8);
                    SendReportScanJob.AddNode(EdocRealationXmlNode, "EdocSize", xmlDocument, preNode9);
                }
            }
        }

        private static XmlNode AddNode(
            XmlNode EdocRealationXmlNode,
            string nodeName,
            XmlDocument xmlDocument,
            XmlNode preNode)
        {
            XmlNode node = xmlDocument.CreateNode(XmlNodeType.Element, nodeName, "");
            node.InnerText = "";
            bool flag = false;
            if (EdocRealationXmlNode.HasChildNodes)
            {
                foreach (XmlNode childNode in EdocRealationXmlNode.ChildNodes)
                {
                    if (childNode.Name == nodeName)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                if (preNode == null)
                    EdocRealationXmlNode.InsertAfter(node, preNode);
                else
                    EdocRealationXmlNode.AppendChild(node);
                AbstractLog.logger.Debug((object)("已补齐非必填属性【" + nodeName + "】"));
            }
            return node;
        }

        private bool IsIllegalName(string fileName)
        {
            bool flag1 = true;
            bool flag2 = true;
            lock (ServerCore.scanLock)
            {
                if (ServerCore.scanAllowSendFileStart == null)
                {
                    flag1 = false;
                }
                else
                {
                    foreach (string str in ServerCore.scanAllowSendFileStart)
                    {
                        if (fileName.StartsWith(str))
                        {
                            flag1 = false;
                            break;
                        }
                    }
                }
                if (ServerCore.scanAllowSendFileEnd == null | flag1)
                {
                    flag2 = false;
                }
                else
                {
                    foreach (string str in ServerCore.scanAllowSendFileEnd)
                    {
                        if (fileName.EndsWith(".report" , StringComparison.CurrentCultureIgnoreCase) || fileName.EndsWith("." + str, StringComparison.CurrentCultureIgnoreCase))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                }
            }
            return flag1 | flag2;
        }
    }
}
