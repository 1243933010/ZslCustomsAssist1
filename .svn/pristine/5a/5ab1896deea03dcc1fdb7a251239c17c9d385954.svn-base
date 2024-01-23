using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Service;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Rest;

namespace ZslCustomsAssist.Jobs
{
    internal class BatchSignJob
    {
        public const string fileSuffix = ".signReceipt";
        public static string mainDirectory = "";
        public static string inDirectory = Path.Combine(BatchSignJob.mainDirectory, "in");
        public static string outDirectory = Path.Combine(BatchSignJob.mainDirectory, "out");

        public void onDoJob()
        {
            LogHelper.Debug((object)"BatchSignJob等待2秒钟开启文件监控，等候中……");
            Thread.Sleep(2000);
            LogHelper.Debug((object)"BatchSignJob文件监控，开始!");
            this.WatcherStrat(BatchSignJob.inDirectory, "*.xml");
        }

        private void WatcherStrat(string path, string filter)
        {
            LogHelper.Debug((object)("开始对目录下【" + path + "】的所有【" + filter + "】格式的文件进行侦听"));
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher()
            {
                InternalBufferSize = 65536,
                Path = path,
                Filter = filter
            };
            fileSystemWatcher.Created += new FileSystemEventHandler(this.OnCreated);
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Security;
            fileSystemWatcher.IncludeSubdirectories = true;
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created || !File.Exists(e.FullPath))
                return;
            LogHelper.Debug((object)("发现新加入待加签数据文件，路径：" + e.FullPath));
            Thread.Sleep(100);
            this.StartSign(e.FullPath);
        }

        private void StartSign(string fullPath)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string[] strArray = withoutExtension.Split('_');
            if (strArray.Length < 2)
                return;
            string messageType = strArray[0];
            string receiverID = strArray[1];
            string str1 = strArray.Length <= 2 ? DateTime.Now.ToString("yyyyMMddHHmmssff") : withoutExtension.Replace(messageType + "_" + receiverID + "_", "");
            LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Debug, str1, "【本地批量加签】读取待加签报文,删除报文：" + fullPath);
            string xmlContent = XmlHelp.GetXmlContent(fullPath, (Encoding)null);
            File.Delete(fullPath);
            string str2 = withoutExtension + ".signReceipt";
            try
            {
                string fileContent = new DeclareMessageHandle().OnDoDeclare(ServerCore.downloadConfig.entName, ServerCore.clientConfig.TypistPassword, ServerCore.downloadConfig.entCode, receiverID, messageType, xmlContent, str1);
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Debug, str1, "将加签结果保存至：" + Path.Combine(BatchSignJob.outDirectory, str2));
                FileHelper.SaveAsFile(BatchSignJob.outDirectory, str2, fileContent);
            }
            catch (Exception ex)
            {
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Error, str1, ex.StackTrace);
                FileHelper.SaveAsFile(BatchSignJob.outDirectory, str2, ex.ToString());
            }
        }

        public static void InitBatchSignDirectory()
        {
            LogHelper.Debug((object)"开始检测并初始化加签目录……");
            if (!Directory.Exists(BatchSignJob.inDirectory))
            {
                LogHelper.Debug((object)("加签输入目录【" + BatchSignJob.inDirectory + "】不存在，开始创建……"));
                Directory.CreateDirectory(BatchSignJob.inDirectory);
            }
            if (Directory.Exists(BatchSignJob.outDirectory))
                return;
            LogHelper.Debug((object)("加签输出目录【" + BatchSignJob.outDirectory + "】不存在，开始创建……"));
            Directory.CreateDirectory(BatchSignJob.outDirectory);
        }
    }
}
