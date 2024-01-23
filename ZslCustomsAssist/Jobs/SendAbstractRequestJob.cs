using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Enum;
using ZslCustomsAssist.Service.Rest;
using ZslCustomsAssist.Service;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.SPSecure;

namespace ZslCustomsAssist.Jobs
{
    internal class SendAbstractRequestJob
    {
        public void OnDoJob()
        {
            //if (ServerCore.clientConfig.ReportSendDir != "" && !Directory.Exists(ServerCore.clientConfig.ReportSendDir))
            //{
            //    Directory.CreateDirectory(ServerCore.clientConfig.ReportSendDir);
            //}

            while (true)
            {
                if (!ServerCore.IsExitThread)
                {
                    try
                    {
                        // RequestReportJd();
                        RequestReport();

                    }
                    catch (Exception ex)
                    {
                        AbstractLog.logger.Error((object)"请求报文异常", ex);
                    }
                    Thread.Sleep(5000);
                }
                else
                    break;
            }
        }

        public void RequestReport()
        {
            SPSecureAPI.SpcVerifyPIN(ServerCore.clientConfig.TypistPassword);
            //Stopwatch stopwatch = new Stopwatch();
            // long wholeMilliseconds = 0;
            // stopwatch.Start();

            ApiService apiService = new();
            List<AbstractMessage> reports = null;
            try
            {
                reports = apiService.RequestAbstract(out string msg);

            }
            catch (Exception ex)
            {
               // string str = "获取摘要发生异常！（耗时：" + (object)DateHelper.GetStopWatchTime(ref wholeMilliseconds, stopwatch, true) + "ms）";
                AbstractLog.logger.Error((object)ex);
            }
            //MessageBox.Show(reports.Count.ToString());

            if (reports != null && reports.Count > 0)
            {
                AbstractLog.logger.Error("请求摘要数据=============================");
                List<string> strArr = new List<string>();
                foreach (AbstractMessage report in reports)
                {

                    ServerCore.AddMainLog("摘要加密数据："+"guid:"+report.Guid+",type:"+report.Type+"");
                    ServerCore.AddAbstractCount(1);
                    AbstractLog.logger.Error((object)JsonConvert.SerializeObject(report, Formatting.Indented) + "请求摘要数据=============================000");

                    report.SignInfo = DeclareMessageXmlSign.XmlAbstractSign(report.DigestValue);
                    //对xml字符串做base64处理，防止传递格式出现问题
                    string str = report.DigestValue;
                    byte[] bytes = Encoding.UTF8.GetBytes(str);
                    string base64Str = Convert.ToBase64String(bytes);
                    report.DigestValue = base64Str;

                    string jsonStr = JsonConvert.SerializeObject(report, Formatting.Indented);
                    AbstractLog.logger.Error((object)jsonStr + "请求摘要数据=============================111");
                   
                    apiService.SendDataCallbackAbstract(report);

                }
            }

           // stopwatch.Stop();
            //AbstractLog.logger.Debug((object)("获取订单报文，本次请求耗时：" + (object)wholeMilliseconds + "ms"));
        }

    }
}
