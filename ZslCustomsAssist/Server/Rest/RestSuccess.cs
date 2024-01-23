using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Rest
{
    public class RestSuccess : RestResult
    {
        public string InvNo { get; set; }

        public string InvPassword { get; set; }

        public string[] MessageText { get; set; }

        public int ReturnCode { get; set; }

        public DateTime Time { get; set; }

        public static string GetResultJsonStr(string invNo = "", string invPwd = "", string[] msgText = null)
        {
            RestSuccess result = GetResult();
            result.InvNo = invNo;
            result.InvPassword = invPwd;
            result.MessageText = msgText;
            return JsonConvert.SerializeObject(result);
        }

        public static RestSuccess GetResult(string invNo = "", string invPwd = "", string[] msgText = null)
        {
            RestSuccess result = new RestSuccess();
            result.ReturnCode = 1;
            result.Time = DateTime.Now;
            result.InvNo = invNo;
            result.InvPassword = invPwd;
            result.MessageText = msgText;
            return result;
        }
    }
}
