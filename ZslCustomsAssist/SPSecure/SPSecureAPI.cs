using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.SPSecure
{
    public class SPSecureAPI
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        public static object lockObj = new object();

        public static void CopySignDll()
        {
            string path = "./";
            FileHelper.CopyOldLabFilesToNewLab(Path.GetFullPath(Environment.Is64BitOperatingSystem ? "./Libs/64" : "./Libs/32"), Path.GetFullPath(path));
        }

        public static string SpcSignData(string inData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inData);
            uint length = (uint)bytes.Length;
            uint nSignDataLen = length;
            if (nSignDataLen < 128U)
                nSignDataLen = 128U;
            byte[] numArray1 = new byte[(int)nSignDataLen];
            lock (SPSecureAPI.lockObj)
            {
               
                Thread.Sleep(SPSecureAPI.GetWaitTime());
                AbstractLog.logger.Info((string)("=="+inData.ToString()+"——————————————————————————！" + numArray1.ToString()+"【" + bytes.ToString()+ "】XML字符串加业务签信息！" + length.ToString()+ "】XML字符串加业务签信息！" + nSignDataLen.ToString() + "】XML字符串加业务签信息！"+ Environment.Is64BitOperatingSystem.ToString()));
                uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcSignData(bytes, length, numArray1, out nSignDataLen) : SPSecureAPI64.SpcSignData(bytes, length, numArray1, out nSignDataLen);
                ServerCore.LastSPSSignTime = DateTime.Now;
                SPSecureAPI.SpcGetErrMsg("XML字符串加业务签", errorCode);
                byte[] numArray2 = new byte[(int)nSignDataLen];
                Array.Copy((Array)numArray1, (Array)numArray2, (long)nSignDataLen);
                return Convert.ToBase64String(numArray2, 0, numArray2.Length);
            }
        }

        public static string SpcSHA1Digest(string inData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inData);
            uint length = (uint)bytes.Length;
            byte[] numArray = new byte[20];
            uint nSignDataLen = (uint)numArray.Length;
            uint errorCode = 0;
            lock (SPSecureAPI.lockObj)
            {
                Thread.Sleep(SPSecureAPI.GetWaitTime());
                errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcSHA1Digest(bytes, length, numArray, out nSignDataLen) : SPSecureAPI64.SpcSHA1Digest(bytes, length, numArray, out nSignDataLen);
                ServerCore.LastSPSSignTime = DateTime.Now;
            }
            SPSecureAPI.SpcGetErrMsg("XML字符串取摘要", errorCode);
            return Convert.ToBase64String(numArray, 0, numArray.Length);
        }

        public static string SpcSignDataNoHash(string inData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(inData);
            uint length = (uint)bytes.Length;
            byte[] numArray = new byte[20];
            uint nSignDataLen = (uint)numArray.Length;
            uint errorCode = 0;
            lock (SPSecureAPI.lockObj)
            {
                errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcSignDataNoHash(bytes, length, numArray, out nSignDataLen) : SPSecureAPI64.SpcSignDataNoHash(bytes, length, numArray, out nSignDataLen);
                Thread.Sleep(1000);
            }
            SPSecureAPI.SpcGetErrMsg("Nohash签名", errorCode);
            return Convert.ToBase64String(numArray, 0, numArray.Length);
        }

        public static string GetX509Certificate()
        {
            string signCert = SPSecureAPI.SpcGetSignCert();
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < signCert.Length; ++index)
            {
                stringBuilder.Append(signCert[index]);
                if (index != 0 && (index + 1) % 76 == 0)
                    stringBuilder.Append("\r\n");
            }
            return stringBuilder.ToString();
        }

        public static bool OpenDevice()
        {
            //SPSecureAPI.logger.Info((object)"初始化设备中...");
            uint errorCode = 0;
            lock (SPSecureAPI.lockObj)
                errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcInitEnvEx() : SPSecureAPI64.SpcInitEnvEx();
            SPSecureAPI.SpcGetErrMsg("打开设备", errorCode);
            return errorCode == 0U;
        }

        public static string SpcVerifyPIN(string password)
        {
            uint errorCode = 0;
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                uint length = (uint)bytes.Length;
                lock (SPSecureAPI.lockObj)
                    errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcVerifyPIN(bytes, length) : SPSecureAPI64.SpcVerifyPIN(bytes, length);
                if (errorCode > 0U)
                {
                    try
                    {
                        SPSecureAPI.SpcGetErrMsg("验证密码", errorCode);
                    }
                    catch (Exception ex)
                    {
                        SPSecureAPI.logger.Error((object)ex);
                        return ex.Message;
                    }
                }
            }
            return "";
        }

        public static string SpcGetCardID()
        {
            byte[] numArray = new byte[100];
            uint nCertNoLen = 100;
            uint errorCode = 0;
            lock (SPSecureAPI.lockObj)
                errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetCardID(numArray, out nCertNoLen) : SPSecureAPI64.SpcGetCardID(numArray, out nCertNoLen);
            string str = Encoding.UTF8.GetString(numArray);
            int length = str.IndexOf('\0');
            string cardId = str.Substring(0, length).Replace("03-", "");
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取卡号", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return cardId;
        }

        public static string SpcGetCertNo()
        {
            byte[] numArray = new byte[100];
            uint nCertNoLen = 100;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetCertNo(numArray, out nCertNoLen) : SPSecureAPI64.SpcGetCertNo(numArray, out nCertNoLen);
            string str = Encoding.UTF8.GetString(numArray);
            int length = str.IndexOf('\0');
            string certNo = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取证书号", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return certNo;
        }

        public static bool SpcGetCardState()
        {
            uint nType = 450;
            uint nIndex = 8008;
            uint nState = 1;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetCardState(nType, nIndex, out nState) : SPSecureAPI64.SpcGetCardState(nType, nIndex, out nState);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取插卡状态", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return false;
            }
            return nState.ToString().Equals("300");
        }

        public static string SpcGetUName()
        {
            byte[] numArray = new byte[100];
            uint nUserNameLen = 100;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetUName(numArray, out nUserNameLen) : SPSecureAPI64.SpcGetUName(numArray, out nUserNameLen);
            //注册Encoding
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string str = Encoding.GetEncoding("GBK").GetString(numArray);
            int length = str.IndexOf('\0');
            string uname = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取申请者名称", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return uname;
        }

        public static string SpcGetEntID()
        {
            byte[] numArray = new byte[100];
            uint nEntIDLen = 100;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetEntID(numArray, out nEntIDLen) : SPSecureAPI64.SpcGetEntID(numArray, out nEntIDLen);
            string str = Encoding.UTF8.GetString(numArray);
            int length = str.IndexOf('\0');
            string entId = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取单位代码", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return entId;
        }

        public static string SpcGetEntName()
        {
            byte[] numArray = new byte[100];
            uint nEntNameLen = 100;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetEntName(numArray, out nEntNameLen) : SPSecureAPI64.SpcGetEntName(numArray, out nEntNameLen);
            string str = Encoding.GetEncoding("GBK").GetString(numArray);
            int length = str.IndexOf('\0');
            string entName = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取单位名称", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return entName;
        }

        public static string SpcGetEntMode()
        {
            byte[] numArray = new byte[100];
            uint nEntModeLen = 100;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetEntMode(numArray, out nEntModeLen) : SPSecureAPI64.SpcGetEntMode(numArray, out nEntModeLen);
            string str = Encoding.UTF8.GetString(numArray);
            int length = str.IndexOf('\0');
            string entMode = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取单位类别", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return entMode;
        }

        public static string SpcGetCardUserInfo()
        {
            char[] szInfo = new char[10000];
            uint nInfoLen = 10000;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetCardUserInfo(szInfo, out nInfoLen) : SPSecureAPI64.SpcGetCardUserInfo(szInfo, out nInfoLen);
            string str = new string(szInfo);
            int length = str.IndexOf('\0');
            string cardUserInfo = str.Substring(0, length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取用户信息", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return cardUserInfo;
        }

        public static string SpcGetSignCert()
        {
            byte[] numArray1 = new byte[10000];
            uint nCertLen = 10000;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetSignCert(numArray1, out nCertLen) : SPSecureAPI64.SpcGetSignCert(numArray1, out nCertLen);
            byte[] numArray2 = new byte[(int)nCertLen];
            Array.Copy((Array)numArray1, (Array)numArray2, (long)nCertLen);
            string base64String = Convert.ToBase64String(numArray2, 0, numArray2.Length);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取卡签名证书", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return base64String;
        }

        public static string SpcGetEnvCert()
        {
            byte[] numArray = new byte[ServerCore.CertSize];
            uint nEnvCertLen = (uint)ServerCore.CertSize;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetEnvCert(numArray, out nEnvCertLen) : SPSecureAPI64.SpcGetEnvCert(numArray, out nEnvCertLen);
            string base64String = Convert.ToBase64String(SPSecureAPI.TrimEnd(numArray));
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取卡加密证书", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return base64String;
        }

        public static string SpcGetCardAttachInfo()
        {
            byte[] numArray = new byte[1000];
            uint nAttachInfoLen = 1000;
            uint errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetCardAttachInfo(numArray, out nAttachInfoLen) : SPSecureAPI64.SpcGetCardAttachInfo(numArray, out nAttachInfoLen);
            byte[] inArray = SPSecureAPI.TrimEnd(numArray);
            string cardAttachInfo = inArray.Length == 0 ? "" : Convert.ToBase64String(inArray);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取卡附加信息", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
                return "";
            }
            return cardAttachInfo;
        }

        public static void SpcClearEnv()
        {
            uint errorCode = 0;
            if (Environment.Is64BitOperatingSystem)
            {
                int num1 = (int)SPSecureAPI64.SpcClearEnv();
            }
            else
            {
                int num2 = (int)SPSecureAPI32.SpcClearEnv();
            }
            try
            {
                SPSecureAPI.SpcGetErrMsg("关闭加密设备", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
            }
        }

        public static void SpcGetErrMsg(string operation, uint errorCode)
        {
            AbstractLog.logger.Info((string)("【" + operation+ errorCode.ToString() + "】提示信息！"));
            if (errorCode > 0U)
                throw new Exception(Marshal.PtrToStringAnsi(!Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetErrMsg(errorCode) : SPSecureAPI64.SpcGetErrMsg(errorCode)));
            string message = "E-PortMsg--==：" + operation + "成功！";
            SPSecureAPI.logger.Info((object)message);
        }

        public static byte[] TrimEnd(byte[] byteArray) => Encoding.Unicode.GetBytes(Encoding.Unicode.GetString(byteArray).TrimEnd(new char[1]));

        public static int GetWaitTime()
        {
            long num = 100L - (DateTime.Now.Ticks - ServerCore.LastSPSSignTime.Ticks) / 10000L;
            return num < 0L ? 0 : (int)num;
        }

        public static string SpcRSADecryptAsPEM(string inData)
        {
            byte[] szInData = Convert.FromBase64String(inData);
            uint length1 = (uint)szInData.Length;
            uint length2 = length1 + 2048U;
            byte[] numArray1 = new byte[(int)length2];
            lock (SPSecureAPI.lockObj)
            {
                Thread.Sleep(SPSecureAPI.GetWaitTime());
                SPSecureAPI.SpcGetErrMsg("RSA解密", !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcSignData(szInData, length1, numArray1, out length2) : SPSecureAPI64.SpcRSADecrypt(szInData, length1, numArray1, out length2));
                byte[] numArray2 = new byte[(int)length2];
                Array.Copy((Array)numArray1, (Array)numArray2, (long)length2);
                return Convert.ToBase64String(numArray2, 0, numArray2.Length);
            }
        }

        public static ulong SpcGetDevAsymAlgoFlag()
        {
            ulong algoFlag = 1;
            uint errorCode = 0;
            lock (SPSecureAPI.lockObj)
                errorCode = !Environment.Is64BitOperatingSystem ? SPSecureAPI32.SpcGetDevAsymAlgoFlag(out algoFlag) : SPSecureAPI64.SpcGetDevAsymAlgoFlag(out algoFlag);
            try
            {
                SPSecureAPI.SpcGetErrMsg("获取卡内证书算法标识", errorCode);
            }
            catch (Exception ex)
            {
                SPSecureAPI.logger.Error((object)ex);
            }
            return algoFlag;
        }
    }
}
