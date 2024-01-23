using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    public class ExitProgramJob : AbstractLog
    {
        public void OnDoJob()
        {
            while (true)
            {
                try
                {
                    if (this.IsAllowToExit())
                    {
                        //ServerCore.MainForm.lblAppState.Text = "退出程序中！";
                        //ServerCore.MainForm.lblAppState.ForeColor = Color.Red;
                        this.QuitApplication();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    AbstractLog.logger.Error((object)"关闭客户端异常!", ex);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public bool IsAllowToExit() => ServerCore.IsExitThread && !ServerCore.isUpdating && ServerCore.ReportSendingCount < 1 && ServerCore.reportReceiptThreads.Count == 0 && ServerCore.UnWriteReceiptReports.Count + ServerCore.WritingReceiptReportsCount < 1;

        public void QuitApplication(string showText = "") => new Thread((ThreadStart)(() =>
        {
            //AbstractLog.logger.Info((object)"关闭客户端中...");
            ServerCore.AddMainLog("关闭客户端中", EnumLogLevel.Important);
            ServerCore.isLogin = false;
            //new Thread((ThreadStart)(() => new SendClientStateJob().SendClientState())).Start();
            ServerCore.DealWithFailIOReports();
            if (!string.IsNullOrWhiteSpace(showText))
            {
                int num = (int)MessageBox.Show(showText, "确认", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (!ServerCore.IsWithOutCard)
                SPSecureAPI.SpcClearEnv();
            string str = IOHelper.ClearData();
            /*if (string.IsNullOrWhiteSpace(str))
                AbstractLog.logger.Info((object)"清空密码成功！");
            else
                AbstractLog.logger.Error((object)("清空密码失败！信息如下：\n" + str));*/
            AbstractLog.logger.Info((object)("本次启动:\n发送报文数\t" + (object)ServerCore.ReportSendSuccessSum + "\n发送失败报文数\t" + (object)ServerCore.ReportSendFailSum + "\n接收回执数\t" + (object)ServerCore.DownLoadedReceiptReportSum + "\n写入回执数\t" + (object)ServerCore.ReceiptWritedSum + "\n程序退出！"));
            Environment.Exit(0);
        })).Start();
    }
}
