using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Http;
using ZslCustomsAssist.Utils.Log;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;

namespace ZslCustomsAssist.Server.Rest
{
    public class DeclareMessageHandle : AbstractHandle, IServiceHandle
    {
        public string OnDoDeclare(
            string userName,
            string passWord,
            string companyId,
            string receiverID,
            string messageType,
            string report,
            string declareCode)
        {
            if (!ServerCore.IsTestModel)
                SecurityValid(userName, passWord, companyId);
            string xsdPathByMtype = GetXsdPathByMType(messageType);
            string str = XmlHelp.XmlTextValidateByXsd(report, xsdPathByMtype);
            if (str != "")
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Error, declareCode, "XML格式验证异常:" + str);
            GetDeclareUrl();
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            string xmlString1 = cryptoServiceProvider.ToXmlString(true);
            string xmlString2 = cryptoServiceProvider.ToXmlString(false);
            LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Info, declareCode, "开始第一次调用申报");
            UnifiedDeclareResult delareResult = UnifiedDeclare(userName, passWord, companyId, receiverID, messageType, report, "0", "0", declareCode);
            if (delareResult.BusiSign == "1" || delareResult.DxpSign == "1")
                onDoDeclare(userName, passWord, companyId, receiverID, messageType, report, xmlString1, xmlString2, delareResult, declareCode);
            return RestSuccess.GetResultJsonStr();
        }

        private UnifiedDeclareResult onDoDeclare(
            string userName,
            string passWord,
            string companyId,
            string receiverID,
            string messageType,
            string report,
            string privateKeyStr,
            string publicKeyStr,
            UnifiedDeclareResult delareResult,
            string declareCode)
        {
            if (delareResult.BusiSign == "1")
            {
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Debug, declareCode, "开始加业务签！");
                string report1 = DeclareMessageXmlSign.XmlSign(report, privateKeyStr, publicKeyStr);
                delareResult = UnifiedDeclare(userName, passWord, companyId, receiverID, messageType, report1, "1", "0", declareCode);
                return onDoDeclare(userName, passWord, companyId, receiverID, messageType, report, privateKeyStr, publicKeyStr, delareResult, declareCode);
            }
            if (!(delareResult.DxpSign == "1"))
                return delareResult;
            LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Debug, declareCode, "开始加传输签！");
            string report2 = DeclareMessageXmlSign.XmlSign(delareResult.DxpMessage, privateKeyStr, publicKeyStr);
            delareResult = UnifiedDeclare(userName, passWord, companyId, receiverID, messageType, report2, "0", "1", declareCode);
            return onDoDeclare(userName, passWord, companyId, receiverID, messageType, report2, privateKeyStr, publicKeyStr, delareResult, declareCode);
        }

        private UnifiedDeclareResult UnifiedDeclare(
            string userName,
            string passWord,
            string companyId,
            string receiverID,
            string messageType,
            string report,
            string busiSign,
            string dxpSign,
            string declareCode)
        {
            string declareUrl = GetDeclareUrl();
            if (ServerCore.IsDebugModel)
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Info, declareCode, "调用传输客户端系统统一申报接口，接口地址：" + declareUrl);
            List<RequestParam> paramList = new List<RequestParam>();
            var data = new
            {
                senderID = userName,
                password = passWord,
                companyID = companyId,
                receiverID,
                messageType,
                message = report,
                busiSign,
                dxpSign
            };
            string str1 = JsonConvert.SerializeObject(data);
            paramList.Add(RequestParam.GetInstance("String", str1));
            string urlEncodePostStr = ServerCore.GetUrlEncodePostStr(paramList);
            LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Info, declareCode, "调用传输客户端系统统一申报接口，请求参数【已URL编码】：" + urlEncodePostStr);
            try
            {
                string str2 = HttpHelper.HttpPost(declareUrl, urlEncodePostStr);
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Info, declareCode, "调用传输客户端系统统一申报接口,返回原始结果：" + str2);
                UnifiedDeclareResult unifiedDeclareResult = JsonConvert.DeserializeObject<UnifiedDeclareResult>(str2);
                if (unifiedDeclareResult.ReturnCode == 0)
                {
                    LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Error, declareCode, "调用传输客户端系统统一申报接口异常：" + unifiedDeclareResult.Message);
                    throw new Exception("调用传输客户端系统统一申报接口异常：" + unifiedDeclareResult.Message);
                }
                return unifiedDeclareResult;
            }
            catch (Exception ex)
            {
                LogHelper.SaveReportSignLog(LogType.UnifiedDeclare, LogLevel.Error, declareCode, "调用传输客户端系统统一申报接口(地址" + declareUrl + ")执行异常，错误信息:" + ex.Message);
                throw new Exception("调用传输客户端系统统一申报接口(地址" + declareUrl + ")执行异常，错误信息:" + ex.Message);
            }
        }

        private string GetDeclareUrl()
        {
            if (!ServerCore.IsTestModel)
                ;
            string cloudServicesUrl = ServerCore.CloudServicesUrl;
            return !(ServerCore.RunEnvironment == "FORMAL") ? !(ServerCore.RunEnvironment == "TEST") ? cloudServicesUrl + "/declareMessage.action" : cloudServicesUrl + "/declareMessage.action" : cloudServicesUrl + "/declareMessage.action";
        }

        private string GetXsdPathByMType(string messageType) => Path.GetFullPath("./xsd/" + messageType + ".xsd");
    }
}
