using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace ZslCustomsAssist.Utils
{
    public class ResourceSetting
    {
        public static string GetValue(string property) => new ResourceManager("ZslCustomsAssist.Properties.Resources", Assembly.GetExecutingAssembly()).GetString(property);

        public static double GetDouble(string property)
        {
            double result = 0.0;
            double.TryParse(ResourceSetting.GetValue(property), out result);
            return result;
        }

        public static int GetInt32(string property)
        {
            int result = 0;
            int.TryParse(ResourceSetting.GetValue(property), out result);
            return result;
        }
    }
}
