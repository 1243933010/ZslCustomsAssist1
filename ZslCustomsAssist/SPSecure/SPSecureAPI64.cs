using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.SPSecure
{
    internal class SPSecureAPI64
    {
        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcInitEnvEx();

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcClearEnv();

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcVerifyPIN(byte[] password, uint length);

        [DllImport("SPSecureAPI.dll")]
        public static extern IntPtr SpcGetErrMsg(uint errorCode);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetCertNo(byte[] szCertNo, out uint nCertNoLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetCardID(byte[] szCardNo, out uint nCertNoLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcSignData(
          byte[] szInData,
          uint nInDataLen,
          byte[] szSignData,
          out uint nSignDataLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcSHA1Digest(
          byte[] szInData,
          uint nInDataLen,
          byte[] szSignData,
          out uint nSignDataLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcSignDataNoHash(
          byte[] szInData,
          uint nInDataLen,
          byte[] szSignData,
          out uint nSignDataLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetCardState(uint nType, uint nIndex, out uint nState);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetUName(byte[] szUserName, out uint nUserNameLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetEntID(byte[] szEntID, out uint nEntIDLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetEntName(byte[] szEntName, out uint nEntNameLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetEntMode(byte[] szEntMode, out uint nEntModeLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetCardUserInfo(char[] szInfo, out uint nInfoLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetSignCert(byte[] szCert, out uint nCertLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetEnvCert(byte[] szEnvCert, out uint nEnvCertLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetCardAttachInfo(byte[] szAttachInfo, out uint nAttachInfoLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcRSADecrypt(
          byte[] szInData,
          uint nInDataLen,
          byte[] szOutData,
          out uint nOutDataLen);

        [DllImport("SPSecureAPI.dll")]
        public static extern uint SpcGetDevAsymAlgoFlag(out ulong algoFlag);
    }
}
