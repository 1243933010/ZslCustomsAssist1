using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Service.Rest;
using ZslCustomsAssist.Utils;

namespace ZslCustomsAssist.Runtime.Config
{
    public class ClientConfig
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        public const string fileName = "sys.data";
        public static string DefaultReportSendDir = Path.Combine(ServerCore.SysFileDirectory, "Send");
       // public static string DefaultReportSendDirJD = Path.Combine(ServerCore.SysFileDirectory, "JDSend");

        public static string DefaultReportSendFailDir = Path.Combine(ServerCore.SysFileDirectory, "SendFail");
        public static string DefaultReceiptReceiveDir = Path.Combine(ServerCore.SysFileDirectory, "Receipt");
        public static string DefaultReceiptPlusFailDir = Path.Combine(ServerCore.SysFileDirectory, "ReceiptFail");
        public Hashtable ReceiptsDirHashMap = new Hashtable();
        public readonly string IsWithoutCard = ServerCore.IsWithOutCard ? "1" : "2";

        public string TypistPassword { get; set; }

        public string ReportSendDir { get; set; }
       // public string JDReportSendDir { get; set; }


        public string UnReportSendDir => ReportSendDir + "ing";
        //public string UnJDReportSendDir => JDReportSendDir + "ing";

        public string ReportSendFailDir { get; set; }

        public string ReceiptReceiveDir { get; set; }

        public string ReceiptsDir { get; set; }

        public string ReceiptPlusFailDir { get; set; }

        public string ReceiptMessageTypes { get; set; }

        public string FeedbackURL { get; set; }

        public string FeedbackEmailEmail { get; set; }

        public string IcNo { get; set; } = string.Empty;

        public string ApiAppId { get; set; }

        public string ApiAppSecret { get; set; }

        public ApiTokenData ApiToken { get; set; }

        public static ClientConfig LoadConfig()
        {
            ClientConfig configInfo = new ClientConfig();
            string configFilePath = GetConfigFilePath();
            if (!File.Exists(configFilePath))
            {
                string? path = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(Path.GetFullPath(path)))
                {
                    Directory.CreateDirectory(path);
                }
                File.Create(configFilePath).Dispose();
            }
            string str1 = "";
            for (int index = 0; str1 == "" && index < 15; ++index)
            {
                if (IOHelper.CheakFileIsWriting(configFilePath))
                {
                    //logger.Info("配置文件【" + configFilePath + "】正在写入中……!");
                    if (index < 2)
                    {
                        Thread.Sleep(10);
                        //logger.Info("等待10毫秒再检查文件状态（最多检查3次）……!");
                    }
                }
                else
                {
                    //logger.Info("配置文件【" + configFilePath + "】状态正常，跳过等待，准备读取配置文件……!");
                    break;
                }
            }
            
            string str2 = Encrypter.SystemDataDecrypter(Encoding.UTF8.GetString(File.ReadAllBytes(configFilePath)));
            if (!string.IsNullOrEmpty(str2))
            {
                try
                {
                    configInfo = JsonConvert.DeserializeObject<ClientConfig>(str2);
                    configInfo.TypistPassword = !string.IsNullOrWhiteSpace(configInfo.TypistPassword) ? Encrypter.TypistPasswordDecrypter(configInfo.TypistPassword) : "";
                    configInfo.ApiAppSecret = !string.IsNullOrWhiteSpace(configInfo.ApiAppSecret) ? Encrypter.TypistPasswordDecrypter(configInfo.ApiAppSecret) : "";
                }
                catch (Exception ex)
                {
                    logger.Error("返回配置内容异常！", ex);
                }
            }
            return DefaultClientConfig(configInfo);
        }

        private static ClientConfig DefaultClientConfig(ClientConfig configInfo)
        {
            bool flag = false;
            if (configInfo == null)
                configInfo = new ClientConfig();
           // if (string.IsNullOrWhiteSpace(configInfo.JDReportSendDir))
            //{
            //    configInfo.JDReportSendDir = DefaultReportSendDirJD;
            //    flag = true;
          //  }
            if (string.IsNullOrWhiteSpace(configInfo.ReportSendDir))
            {
                configInfo.ReportSendDir = DefaultReportSendDir;
                flag = true;
            }
            if (string.IsNullOrWhiteSpace(configInfo.ReportSendFailDir))
            {
                configInfo.ReportSendFailDir = DefaultReportSendFailDir;
                flag = true;
            }
            if (string.IsNullOrWhiteSpace(configInfo.ReceiptReceiveDir))
            {
                configInfo.ReceiptReceiveDir = DefaultReceiptReceiveDir;
                flag = true;
            }
            if (string.IsNullOrWhiteSpace(configInfo.ReceiptsDir))
            {
                configInfo.ReceiptsDir = "";
                configInfo.ReceiptsDirHashMap = new Hashtable();
                flag = true;
            }
            else
            {
                try
                {
                    configInfo.ReceiptsDirHashMap = SetReceiptsDirHashMap(configInfo.ReceiptsDir);
                }
                catch (ArgumentException ex)
                {
                    logger.Error("报文回执分类目录内容有误！将重新初始化...", ex);
                }
            }
            if (string.IsNullOrWhiteSpace(configInfo.ReceiptPlusFailDir))
            {
                configInfo.ReceiptPlusFailDir = DefaultReceiptPlusFailDir;
                flag = true;
            }
            if (string.IsNullOrWhiteSpace(configInfo.IcNo) && !string.IsNullOrWhiteSpace(ServerCore.userData.szCardID))
            {
                configInfo.IcNo = ServerCore.userData.szCardID;
                flag = true;
            }
            if (flag)
                IOHelper.InputConfigFile(configInfo);

            return configInfo;
        }

        public static Hashtable SetReceiptsDirHashMap(string receiptsDir)
        {
            Hashtable hashtable = new Hashtable();
            if (receiptsDir.Contains("*"))
            {
                string str1 = receiptsDir;
                char[] chArray = new char[1] { '\n' };
                foreach (string str2 in str1.Split(chArray))
                {
                    if (!string.IsNullOrWhiteSpace(str2))
                    {
                        string[] strArray = str2.Trim().Split('*');
                        if (string.IsNullOrWhiteSpace(strArray[0]))
                            logger.Error("回执分类目录内容存在空报文名称！跳过添加回执分类目录【" + strArray[1] + "】");
                        else if (string.IsNullOrWhiteSpace(strArray[1]))
                        {
                            logger.Error("回执分类目录内容存在空报文目录！跳过添加【" + strArray[0] + "】的回执分类目录");
                        }
                        else
                        {
                            if (!File.Exists(strArray[1]))
                                Directory.CreateDirectory(strArray[1]);
                            FileHelper.InitSaveReportDirectory(strArray[1], strArray[0] + "回执接收目录");
                            if (!ServerCore.IsExitThread)
                            {
                                string str3 = IOHelper.IOTest(strArray[1]);
                                if (!string.IsNullOrWhiteSpace(str3))
                                    throw new IOException("当前 " + strArray[1] + " 读写出错！原因如下\n" + str3);
                            }
                            try
                            {
                                hashtable.Add(strArray[0], strArray[1]);
                            }
                            catch (ArgumentException ex)
                            {
                                logger.Error("回执分类目录内容已存在相同回执分类目录【" + strArray[0] + "】：【" + hashtable[strArray[0]] + "】！跳过添加该回执分类目录【" + strArray[1] + "】");
                            }
                        }
                    }
                }
            }
            return hashtable;
        }

        public static string GetDirByRecipType(string receiptMessageType)
        {
            string dirByRecipType = string.Concat(ServerCore.clientConfig.ReceiptsDirHashMap[(object)receiptMessageType]);
            if (string.IsNullOrWhiteSpace(dirByRecipType))
                dirByRecipType = ServerCore.clientConfig.ReceiptReceiveDir;
            return dirByRecipType;
        }

        public static string GetConfigFilePath() => Path.Combine(ServerCore.SysFileDirectory, "Data", "sys.db");
    }
}
