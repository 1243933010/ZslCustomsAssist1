using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.SwagentProxy
{
    internal class SwsIniFile
    {
        public string iniFilePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(
          string section,
          string key,
          string val,
          string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
          string section,
          string key,
          string def,
          StringBuilder retVal,
          int size,
          string filePath);

        public SwsIniFile()
        {
        }

        public SwsIniFile(string iniFilePath) => this.iniFilePath = iniFilePath;

        public void SetIniValue(string Section, string Key, string Value) => SwsIniFile.WritePrivateProfileString(Section, Key, Value, this.iniFilePath);

        public string GetIniValue(string Section, string Key)
        {
            StringBuilder retVal = new StringBuilder(1024);
            SwsIniFile.GetPrivateProfileString(Section, Key, "", retVal, 1024, this.iniFilePath);
            return retVal.ToString();
        }
    }
}
