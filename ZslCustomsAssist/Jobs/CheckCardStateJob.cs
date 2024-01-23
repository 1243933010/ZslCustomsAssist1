using System.Diagnostics;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Runtime.Config;
using ZslCustomsAssist.SPSecure;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Jobs
{
    internal class CheckCardStateJob : AbstractLog
    {
        public void OnDoJob()
        {
            while (!ServerCore.IsExitThread)
            {
                Thread.Sleep(60000);
                if (ServerCore.isUpdating)
                {
                    AbstractLog.logger.Info((object)"当前为更新状态中，不检查卡状态！");
                    break;
                }
                try
                {
                    this.CheckCardState();
                }
                catch (Exception ex)
                {
                    if (ServerCore.isUpdating)
                    {
                        AbstractLog.logger.Info((object)"当前为更新状态中，不检查卡状态！");
                        break;
                    }
                    ServerCore.isCardRegular = false;
                    AbstractLog.logger.Error((object)"检查插卡状态异常,可能会导致报文收发功能异常！", ex);
                    int num = (int)MessageBox.Show("读卡器运行异常，即将关闭客户端！" + ex.Message + "\n请确认：\n1.读卡器正常运行；\n2.报关卡正确插入；\n3.系统中无其他程序占用读卡器；", "确认", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    ServerCore.IsExitThread = true;
                }
            }
            //AbstractLog.logger.Info((object)"已结束本次检查插卡状态！");
        }

        public bool CheckCardState()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ServerCore.clientConfig = ClientConfig.LoadConfig();
            //AbstractLog.logger.Info((object)"检查插卡状态中");
            for (int index = 1; index <= 3; ++index)
            {
                string message = SPSecureAPI.SpcVerifyPIN(ServerCore.clientConfig.TypistPassword);
                if (message.IndexOf("输入口令错误") > -1)
                    throw new Exception("插卡信息异常！");
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (index < 3)
                    {
                        AbstractLog.logger.Error((object)("第" + (object)index + "次读卡失败,即将重新尝试读卡！"));
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        AbstractLog.logger.Error((object)"最后一次尝试读卡失败！");
                        ServerCore.isCardRegular = false;
                        throw new Exception(message);
                    }
                }
                else
                    break;
            }
            SPSecureAPI.OpenDevice();
            string cardId = SPSecureAPI.SpcGetCardID();
            if (!cardId.Equals(ServerCore.userData.szCardID))
            {
                AbstractLog.logger.Error((object)("卡号校验失败：\n登录卡号：" + ServerCore.userData.szCardID + "\n检测卡号：" + cardId));
                ServerCore.MainForm.labMsg.Text = "插卡状态异常！";
                //ServerCore.MainForm.lblAppState.ForeColor = Color.Red;
                ServerCore.isCardRegular = false;
                throw new Exception("卡号校验失败！");
            }
            AbstractLog.logger.Info((object)"插卡状态正常！" + cardId + "---" + ServerCore.userData.szCardID + cardId.Equals(ServerCore.userData.szCardID));
            ServerCore.isCardRegular = true;
            stopwatch.Stop();
            //AbstractLog.logger.Info((object)("本次检测插卡状态共耗时：" + (object)((double)stopwatch.ElapsedMilliseconds / 1000.0) + "s"));
            return ServerCore.isCardRegular;
        }
    }
}
