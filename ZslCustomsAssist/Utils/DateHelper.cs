using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public class DateHelper
    {
        public static string GetDifferTotalMilliseconds(DateTime dateBegin) => DateHelper.GetDifferTotalMilliseconds(dateBegin, DateTime.Now);

        public static string GetDifferTotalMilliseconds(DateTime dateBegin, DateTime dateEnd) => (dateEnd.Subtract(dateBegin).TotalMilliseconds / 1000.0).ToString() + "秒";

        public static DateTime StampToDateTime(string timeStamp) => TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).Add(new TimeSpan(long.Parse(timeStamp + "0000000")));

        public static int DateTimeToStamp() => DateHelper.DateTimeToStamp(DateTime.Now);

        public static int DateTimeToStamp(DateTime time)
        {
            DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (int)(time - localTime).TotalSeconds;
        }

        public static int getMillisecond(int num, string timeUnit = "min")
        {
            string str = timeUnit;
            if (str == "h")
                return num * 60 * 60 * 1000;
            if (str == "min")
                return num * 60 * 1000;
            return str == "s" ? num * 1000 : num;
        }

        public static int GetUInt(int num)
        {
            if (num < 0)
                num = 0;
            return num;
        }

        public static long GetStopWatchTime(
          ref long wholeMilliseconds,
          Stopwatch stopwatch,
          bool isReset = false)
        {
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            wholeMilliseconds += elapsedMilliseconds;
            if (isReset)
                stopwatch.Reset();
            stopwatch.Start();
            return elapsedMilliseconds;
        }
    }
}
