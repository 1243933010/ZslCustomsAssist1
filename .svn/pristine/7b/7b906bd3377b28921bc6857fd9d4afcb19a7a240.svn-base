using System.Text;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.User;
using ZslCustomsAssist.Utils.Http;
using log4net;
using System.Collections;
using System.Reflection;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Server.Rest;
using static Org.BouncyCastle.Math.EC.ECCurve;
using IBM.WMQ;
using ZslCustomsAssist.MQ.MQQueueProxy;
using ZslCustomsAssist.MQ;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Runtime
{
    public class ServerCore
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        public const string FORMAL = "FORMAL";
        public const string TEST = "TEST";
        public const string DEBUG = "DEBUG";
        public const string UpdateNoticeFileName = "updateNotice.notice";
        public const string ReadyToUpdateFileName = "readyToUpdate.notice";
        public static readonly bool IsBindingIp = ResourceSetting.GetValue(nameof(IsBindingIp)).Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        public static readonly bool IsBindingCard = ResourceSetting.GetValue(nameof(IsBindingCard)).Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        public static readonly bool IsVerifySign = ResourceSetting.GetValue(nameof(IsVerifySign)).Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        public static readonly string electSignUrl = ResourceSetting.GetValue("ElectSignUrl").Trim();
        public static readonly string electSignKey = ResourceSetting.GetValue("ElectSignKey").Trim();
        public static readonly string readyToUpdateFilePath = Path.Combine(Environment.CurrentDirectory, "readyToUpdate.notice");
        public static readonly string updateNoticeFilePath = Path.Combine(Environment.CurrentDirectory, "updateNotice.notice");
        public static readonly bool IsWithOutCard = ResourceSetting.GetValue(nameof(IsWithOutCard)).Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        public static readonly int MainLogLinage = ResourceSetting.GetInt32(nameof(MainLogLinage)) < 20 ? 20 : ResourceSetting.GetInt32(nameof(MainLogLinage));
        public static readonly string MainFormName = ResourceSetting.GetValue(nameof(MainFormName)) ?? "数据交换客户端";
        public static readonly string CloudServicesUrl = ResourceSetting.GetValue(nameof(CloudServicesUrl));
        public static readonly string ZslApiServicesUrl = ResourceSetting.GetValue(nameof(ZslApiServicesUrl));
        public static readonly string DataQueryServicesUrl = ResourceSetting.GetValue(nameof(DataQueryServicesUrl));
        public static readonly string RunEnvironment = ResourceSetting.GetValue("RunningEnvironment") ?? nameof(FORMAL);
        public static readonly string HttpServicePort = ResourceSetting.GetValue(nameof(HttpServicePort)) ?? "6666";
        public static readonly int MinSendThreads = ResourceSetting.GetInt32(nameof(MinSendThreads));
        public static readonly int MinReceiveThreads = ResourceSetting.GetInt32(nameof(MinReceiveThreads));
        public static readonly string AESKey = ResourceSetting.GetValue(nameof(AESKey)) ?? "1234567890123456";
        public static readonly string AESIV = ResourceSetting.GetValue(nameof(AESIV)) ?? "6543210987654321";
        public static readonly string UpdateLogFileName = ResourceSetting.GetValue(nameof(UpdateLogFileName)) ?? "CBTUpdateLog.log";
        public static readonly string MonitorUpdateTempDirectory = ResourceSetting.GetValue(nameof(MonitorUpdateTempDirectory)) ?? "MonitorUpdateTemp";
        public static readonly bool EnableAES = ResourceSetting.GetValue(nameof(EnableAES)).ToLower().Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        public static readonly string AssemblyVesion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        //public static readonly string GetMonitorServicePath = Path.Combine(Environment.CurrentDirectory, "SZCBTServer.Monitor.exe");
        public static readonly bool IsDebugModel = ServerCore.RunEnvironment.ToUpper().Equals(nameof(DEBUG));
        public static readonly bool IsTestModel = ServerCore.RunEnvironment.ToUpper().Equals(nameof(TEST));
        public static readonly bool IsFORMALModel = ServerCore.RunEnvironment.ToUpper().Equals(nameof(FORMAL));
        public static readonly string SysFileDirectory = Path.Combine(Environment.CurrentDirectory, "runtimes", "Server");
        public static readonly string LogFileDirectory = Path.Combine(Environment.CurrentDirectory, "Log");
        public static readonly string MacAddress = SystemInfoHelper.GetMacAddress();
        public static readonly string LocalIP = SystemInfoHelper.GetLocalIP();
        public static object lockObj = new object();
        public static object sendLock = new object();
        public static object scanLock = new object();
        public static int CertSize = 2000;
        public static bool IsExitThread = false;
        public static bool isCardRegular = true;
        public static bool isLogin = false;
        public static bool isUpdating = false;
        public static ClientConfig clientConfig = new ClientConfig();
        public static SysConfig sysConfig = new SysConfig();
        public static SendClientStateResult sendClientState = new SendClientStateResult();
        //public static ServiceHost m_Host;
        public static int ReportScanedSum = 0;
        public static int ReportSendingCount = 0;
        public static int ReportSendFailSum = 0;
        public static int ReportSendSuccessSum = 0;
        public static int DownLoadedReceiptReportSum = 0;
        public static int ReceiptWritedSum = 0;
        public static int WritingReceiptReportsCount = 0;
        public static int ReceiptPlusFaillSum = 0;
        public static int AbstractSum = 0;

        public static DateTime LastSPSSignTime = DateTime.Now;
        public static List<ThreadExt> reportReceiptThreads = new List<ThreadExt>();
        public static UserData userData = new UserData();
        //public static HwsbMqConfig hwsbMqCoinfig = new HwsbMqConfig();
        public static DownloadConfig downloadConfig = new DownloadConfig();
        public static List<string> DeleteFailReportSendList = new List<string>();
        public static List<string> MoveFailReportSendList = new List<string>();
        public static List<string> UnSendReportList = new List<string>();
        public static List<string> NeedResendReportList = new List<string>();
        public static List<ReceiptContentItem> UnWriteReceiptReports = new List<ReceiptContentItem>();
        public static List<Hashtable> MainLogs = new List<Hashtable>();
        public static string supportedSignType = "ICCard";
        public static int privateKeyIndex;
        public static SwagentProxy.SwagentProxy swagentProxy = new SwagentProxy.SwagentProxy();
        public static string supportedTransportProtocol = "HTTPS";
        public static MQQueueManager mqQueueManager;
        public static IbmMQQueueProxy mqSendQueueProxy = new IbmMQQueueProxy(IbmMQQueueProxy.Func.SEND);
        public static IbmMQQueueProxy mqReceiveQueueProxy = new IbmMQQueueProxy(IbmMQQueueProxy.Func.RECEIVE);
        public static List<string> scanAllowSendFileStart = new List<string>();
        public static List<string> scanAllowSendFileEnd = new List<string>();

        public static FormMain MainForm { get; set; }

        //public static DataQueryForm DataQueryForm { get; set; }

        public static FormLogin Login { get; set; }

        //public static Config ConfigForm { get; set; }

        public static FormSend SendForm { get; set; }

        public static FormSign SignForm { get; set; }

        public static bool IsSafetyModuleInited { get; set; }

        public static MyActiveMq MyMq { get; set; }

        public static void AddMainLog(string logStr, EnumLogLevel LogLevel = EnumLogLevel.Normal, Exception ex = null)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ServerCore.MainLogs.Add(new Hashtable()
            {
                {
                    (object) (str + " — " + logStr + "\r\n"),
                    (object) LogLevel
                }
            });
            lock (ServerCore.lockObj)
            {
                if (ServerCore.MainLogs.Count <= ServerCore.MainLogLinage)
                    return;
                ServerCore.MainLogs.RemoveAt(0);
            }
        }

        public static void AddAbstractCount(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ServerCore.lockObj)
                ServerCore.AbstractSum += addNum;
        }

        public static void AddReportSendingCount(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ServerCore.lockObj)
                ServerCore.ReportSendingCount += addNum;
        }

        public static void AddReportScanedSum(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ServerCore.lockObj)
                ServerCore.ReportScanedSum += addNum;
        }

        public static void AddReportSendFailSum(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ServerCore.lockObj)
                ServerCore.ReportSendFailSum += addNum;
        }

        public static void AddReportSendSuccessSum(int addNum = 1, bool isRefreshNum = true)
        {
            lock (ServerCore.lockObj)
                ServerCore.ReportSendSuccessSum += addNum;
        }

        public static bool IsAllowToDealWithReport() => ServerCore.isCardRegular && ServerCore.isLogin && !ServerCore.IsExitThread && !ServerCore.isUpdating;

        public static ReceiveMessageType GetMessageTypeByCode(string code) => ServerCore.downloadConfig.receiveMessageTypes?.Find((Predicate<ReceiveMessageType>)(rmt => rmt.code.Equals(code)));

        public static string GetDeleteReportSendFailTips() => ServerCore.GetFileIOTips(ServerCore.DeleteFailReportSendList, true);

        public static string GetMoveReportSendFailTips() => ServerCore.GetFileIOTips(ServerCore.MoveFailReportSendList, false);

        public static string GetFileIOTips(List<string> fileNameList, bool isSendSuccess)
        {
            string str1 = isSendSuccess ? "成功" : "失败";
            string str2 = isSendSuccess ? "移除" : "移动";
            if (fileNameList == null || fileNameList.Count <= 0)
                return (string)null;
            string str3 = "以下报文文件(" + (object)fileNameList.Count + "个)发送" + str1 + "，可能由于读写权限未能" + str2 + "\n建议在下次启动客户端前在报文发送目录下手动" + str2 + "，否则将在重启客户端后重复发送:";
            for (int index = 0; index < fileNameList.Count; ++index)
                str3 = str3 + fileNameList[index] + "、";
            return str3.Substring(0, str3.Length - 1);
        }

        public static void DealWithNotExistFileName(List<string> fileNameList, bool isMove) => fileNameList.ForEach((Action<string>)(name =>
        {
            if (!File.Exists(name) || !isMove)
                return;
            string str = isMove ? "移动" : "删除";
            ServerCore.logger.Info((object)("尝试" + str + name));
            try
            {
                if (isMove)
                {
                    string withoutExtension = Path.GetFileNameWithoutExtension(name);
                    string targetDirectory = Path.Combine(ServerCore.clientConfig.ReportSendFailDir, withoutExtension);
                    FileHelper.MoveAndReplace(name, targetDirectory);
                }
                else
                    File.Delete(name);
            }
            catch (Exception ex)
            {
                ServerCore.logger.Error((object)(str + name + "失败!"), ex);
            }
        }));

        public static void DealWithFailIOReports()
        {
            ServerCore.DealWithNotExistFileName(ServerCore.MoveFailReportSendList, true);
            ServerCore.DealWithNotExistFileName(ServerCore.DeleteFailReportSendList, false);
            if (ServerCore.GetDeleteReportSendFailTips() != null)
            {
                int num1 = (int)MessageBox.Show(ServerCore.GetDeleteReportSendFailTips(), "确认", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (ServerCore.GetMoveReportSendFailTips() == null)
                return;
            int num2 = (int)MessageBox.Show(ServerCore.GetMoveReportSendFailTips(), "确认", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static string GetUrlEncodePostStr(
            List<RequestParam> paramList,
            string encodStr = "UTF-8",
            bool isUrlEncode = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (isUrlEncode)
            {
                if (ServerCore.EnableAES)
                {
                    foreach (RequestParam requestParam in paramList)
                    {
                        stringBuilder.Append("&");
                        string oldValue = Encrypter.DefaultEncodeAES(requestParam.Value);
                        stringBuilder.Append(requestParam.Name + "=" + HttpHelper.UrlEncode(oldValue, encodStr));
                    }
                }
                else
                {
                    foreach (RequestParam requestParam in paramList)
                    {
                        stringBuilder.Append("&");
                        string str = HttpHelper.UrlEncode(requestParam.Value, encodStr);
                        stringBuilder.Append(requestParam.Name + "=" + str);
                    }
                }
            }
            else if (ServerCore.EnableAES)
            {
                foreach (RequestParam requestParam in paramList)
                {
                    stringBuilder.Append("&");
                    stringBuilder.Append(requestParam.Name + "=" + Encrypter.DefaultEncodeAES(requestParam.Value));
                }
            }
            else
            {
                foreach (RequestParam requestParam in paramList)
                {
                    stringBuilder.Append("&");
                    stringBuilder.Append(requestParam.Name + "=" + requestParam.Value);
                }
            }
            if (stringBuilder.ToString().StartsWith("&"))
                stringBuilder = stringBuilder.Remove(0, 1);
            return stringBuilder.ToString();
        }

        public static string GetVersion()
        {
            string oldValue = ServerCore.AssemblyVesion.Split('.')[3];
            if (oldValue.Length != 3)
                return ServerCore.AssemblyVesion;
            string newValue = "0" + oldValue;
            return ServerCore.AssemblyVesion.Replace(oldValue, newValue);
        }

        public static string RunningModelChineseName()
        {
            string str = "";
            string upper = ServerCore.RunEnvironment.ToUpper();
            if (upper.Equals("DEBUG"))
                str += "调试版";
            else if (upper.Equals("TEST"))
                str += "测试版";
            else if (upper.Equals("FORMAL"))
                str += "正式版";
            return str;
        }
    }
}
