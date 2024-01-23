using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.SPSecure;

namespace ZslCustomsAssist.User
{
    public class UserData
    {
        [JsonProperty(PropertyName = "IC卡号")]
        public string szCardID { get; set; }

        [JsonProperty(PropertyName = "证书号")]
        public string szCertNo { get; set; }

        [JsonProperty(PropertyName = "申请者名称")]
        public string szUserName { get; set; }

        [JsonProperty(PropertyName = "单位代码")]
        public string szEntId { get; set; }

        [JsonProperty(PropertyName = "单位名称")]
        public string szEntName { get; set; }

        [JsonProperty(PropertyName = "单位类别")]
        public string szEntMode { get; set; }

        [JsonProperty(PropertyName = "用户信息")]
        public string szCardUserInfo { get; set; }

        [JsonProperty(PropertyName = "签名证书")]
        public string szSignCert { get; set; }

        [JsonProperty(PropertyName = "加密证书")]
        public string szEnvCert { get; set; }

        [JsonProperty(PropertyName = "附加信息")]
        public string szCardAttachInfo { get; set; }

        [JsonProperty(PropertyName = "证书算法标识")]
        public ulong szAlgoFlag { get; set; }

        public static void GetCardMsg()
        {
            ServerCore.userData.szAlgoFlag = SPSecureAPI.SpcGetDevAsymAlgoFlag();
            ServerCore.userData.szCertNo = SPSecureAPI.SpcGetCertNo();
            ServerCore.userData.szUserName = SPSecureAPI.SpcGetUName();
            ServerCore.userData.szEntId = SPSecureAPI.SpcGetEntID();
            ServerCore.userData.szEntName = SPSecureAPI.SpcGetEntName();
            ServerCore.userData.szEntMode = SPSecureAPI.SpcGetEntMode();
            ServerCore.userData.szSignCert = SPSecureAPI.SpcGetSignCert();
        }
    }
}
