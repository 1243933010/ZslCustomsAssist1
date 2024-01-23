using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils;

namespace ZslCustomsAssist.Server.Rest
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class Condition : ICloneable
    {
        [JsonIgnore]
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        [JsonIgnore]
        public static int multipleWidth_1 = 185;
        [JsonIgnore]
        public static int multipleWidth_2 = 512;
        [JsonIgnore]
        public static int multipleWidth_3 = 837;
        [JsonIgnore]
        public static int multipleHeight_1 = 28;

        public string min { get; set; }

        public string dataType { get; set; }

        public string multipleWidth { get; set; }

        public string max { get; set; }

        public string maxLength { get; set; }

        public string multipleHeight { get; set; }

        public string minLength { get; set; }

        public string description { get; set; }

        public string inputType { get; set; }

        public string name { get; set; }

        public string format { get; set; }

        public string required { get; set; }

        public string regexp { get; set; }

        public string message { get; set; }

        public string multipleValuesNumber { get; set; }

        public int GetMultipleWidth()
        {
            try
            {
                int num = int.Parse(multipleWidth);
                if (num > 2)
                    return multipleWidth_3;
                return num == 2 ? multipleWidth_2 : multipleWidth_1;
            }
            catch (Exception ex)
            {
                multipleWidth = "1";
                return multipleWidth_1;
            }
        }

        public int GetMultipleHeight()
        {
            try
            {
                int num = DateHelper.GetUInt(int.Parse(multipleHeight));
                if (num > 0)
                    return num * multipleHeight_1;
                throw new Exception("服务端设置倍高为0！");
            }
            catch (Exception ex)
            {
                multipleHeight = "1";
                return multipleHeight_1;
            }
        }

        public int GetMin()
        {
            try
            {
                return int.Parse(min);
            }
            catch (Exception ex)
            {
                min = string.Concat(0);
                return 0;
            }
        }

        public int GetMax()
        {
            try
            {
                return int.Parse(max);
            }
            catch (Exception ex)
            {
                max = string.Concat(double.MaxValue);
                return int.MaxValue;
            }
        }

        public int GetMaxLength()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(maxLength));
            }
            catch (Exception ex)
            {
                maxLength = string.Concat(int.MaxValue);
                return int.MaxValue;
            }
        }

        public int GetMinLength()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(minLength));
            }
            catch (Exception ex)
            {
                minLength = "0";
                return 0;
            }
        }

        public bool GetRequired() => "1".Equals(required);

        public string GetMessage() => string.IsNullOrWhiteSpace(message) ? "正则表达式：" + regexp : message;

        public int GetMultipleValuesNumber()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(multipleValuesNumber));
            }
            catch (Exception ex)
            {
                multipleValuesNumber = "1";
                return 1;
            }
        }

        public object Clone() => MemberwiseClone();
    }
}
