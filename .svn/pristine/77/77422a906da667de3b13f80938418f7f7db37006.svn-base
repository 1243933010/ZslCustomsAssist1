using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    internal class ClearTimeOutFileJob
    {
        public void OnDoJob()
        {
            while (!ServerCore.IsExitThread)
            {
                this.ClearTimeOutFile();
                Thread.Sleep(ServerCore.sysConfig.GetClearTimeOutFileInterval());
            }
            //AbstractLog.logger.Info((object)"已结束本次定期清理文件！");
        }

        private void ClearTimeOutFile()
        {
            try
            {
                ClearTimeOutFileJob.ClearFile(ServerCore.sysConfig.GetLogsKeepDays(), ServerCore.LogFileDirectory);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"清除指定保留天数以前的日志文件时发生异常！", ex);
            }
            try
            {
                ClearTimeOutFileJob.DeleteDirectory(ServerCore.sysConfig.GetReportSendEmptyDirKeepDays(), new DirectoryInfo(ServerCore.clientConfig.UnReportSendDir));
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)"清除指定保留天数以前的发送目录空文件夹时发生异常！", ex);
            }
        }

        public static void ClearFile(int keepDays, string directory)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                DirectoryInfo dyInfo = new DirectoryInfo(directory);
                if (!dyInfo.Exists)
                {
                    AbstractLog.logger.Info((object)(dyInfo.ToString() + "不存在，取消清除！"));
                    return;
                }
                ClearTimeOutFileJob.DeleteFile(keepDays, directory, dyInfo);
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)("清除【" + directory + "】目录下" + (object)keepDays + "天前的所有文件时遇到异常!"), ex);
            }
            stopwatch.Stop();
            //AbstractLog.logger.Info((object)("本次清除" + directory + "文件共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
        }

        private static void DeleteFile(int keepDays, string directory, DirectoryInfo dyInfo)
        {
            //AbstractLog.logger.Info((object)("开始检测并清除【" + directory + "】目录下" + (object)keepDays + "天前的所有文件！"));
            foreach (FileInfo file in dyInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.CreationTime.AddDays((double)keepDays) < DateTime.Now)
                {
                    file.Delete();
                    if (File.Exists(file.FullName))
                        AbstractLog.logger.Info((object)(file.FullName + "删除失败！"));
                    else
                        AbstractLog.logger.Info((object)(file.FullName + "已删除成功！"));
                }
            }
            //AbstractLog.logger.Info((object)("【" + directory + "】目录下" + (object)keepDays + "天前的所有文件已清除成功！"));
            ClearTimeOutFileJob.DeleteDirectory(keepDays, dyInfo);
        }

        private static void DeleteDirectory(int keepDays, DirectoryInfo dyInfo)
        {
            //AbstractLog.logger.Info((object)("开始检测并清除【" + dyInfo.FullName + "】目录下" + (object)keepDays + "天前的所有空文件夹！"));
            DirectoryInfo[] directories = dyInfo.GetDirectories("*", SearchOption.AllDirectories);
            Array.Sort((Array)directories, (IComparer)new StringLengthComparer());
            foreach (DirectoryInfo directoryInfo in directories)
            {
                if (directoryInfo.CreationTime.AddDays((double)keepDays) < DateTime.Now && directoryInfo.GetFiles("*", SearchOption.AllDirectories).Length == 0)
                {
                    Directory.Delete(directoryInfo.FullName);
                    if (Directory.Exists(directoryInfo.FullName))
                        AbstractLog.logger.Info((object)(directoryInfo.FullName + "删除失败！"));
                    else
                        AbstractLog.logger.Info((object)(directoryInfo.FullName + "已删除成功！"));
                }
            }
            //AbstractLog.logger.Info((object)("【" + dyInfo.FullName + "】目录下" + (object)keepDays + "天前的所有空文件夹已检测清除！"));
        }
    }
}
