using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Server.Enum
{
    public enum DataType
    {
        [Description("字节长度#String")] LimitB,
        [Description("日期#Number")] Date,
        [Description("货币#Number")] Currency,
        [Description("小数#Number")] Double,
        [Description("整数#Number")] Integer,
        [Description("手机号码#String")] Mobile,
        [Description("电话号码#String")] Phone,
        [Description("Email#String")] Email,
        [Description("QQ号码#String")] QQ,
        [Description("身份证号码#String")] IdCard,
        [Description("邮政编码#String")] Zip,
        [Description("数字#Number")] Number,
        [Description("网址#String")] Url,
        [Description("中文#String")] Chinese,
        [Description("英文#String")] English,
        [Description("安全密码#String")] SafeString,
        [Description("重复输入#String")] Repeat,
        [Description("关系比较\tlong,double, java.util.Date  textfield, textarea")] Compare,
        [Description("字符长度#String")] Limit,
        [Description("单|多选按钮组#String")] Group,
        [Description("正则表达式#String")] Custom,
    }
}
