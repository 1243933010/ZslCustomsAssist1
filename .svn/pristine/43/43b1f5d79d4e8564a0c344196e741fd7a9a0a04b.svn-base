using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils;

namespace ZslCustomsAssist.Server.Rest
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class DataResource : ICloneable
    {
        [JsonIgnore]
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        public string code { get; set; }

        public string name { get; set; }

        public string classificationCode { get; set; }

        public string classificationName { get; set; }

        public string dailyQueryCount { get; set; }

        public string remains { get; set; }

        public string pagination { get; set; }

        public string queryMode { get; set; }

        public object results { get; set; }

        public List<Condition> conditions { get; set; }

        public int GetRemains()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(remains));
            }
            catch (Exception ex)
            {
                logger.Error("默认今日剩余调用次数次数为0", ex);
                return 0;
            }
        }

        public int GetDailyQueryCount()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(dailyQueryCount));
            }
            catch (Exception ex)
            {
                logger.Error("默认日查询次数为0", ex);
                return 0;
            }
        }

        public Hashtable GetResultsHashTable()
        {
            try
            {
                return JsonConvert.DeserializeObject<Hashtable>(results.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("默认结果字段键值对为空", ex);
                return new Hashtable();
            }
        }

        public Hashtable GetRequireCondiction()
        {
            Hashtable requireCondiction = new Hashtable();
            foreach (Condition condition in conditions)
            {
                if (condition.GetRequired())
                    requireCondiction.Add(condition.name, condition.description);
            }
            return requireCondiction;
        }

        public string GetQueryMode() => queryMode == null ? "" : queryMode;

        public object Clone() => MemberwiseClone();
    }
}
