using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Runtime.Config;

namespace ZslCustomsAssist.Utils
{
    internal class IOHelper
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");
        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 64;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        public static string InputConfigFile(ClientConfig clientConfig)
        {
            string str = (string)null;
            clientConfig.TypistPassword = !string.IsNullOrWhiteSpace(clientConfig.TypistPassword) ? Encrypter.TypistPasswordEncrypter(clientConfig.TypistPassword) : "";
            clientConfig.ApiAppSecret = !string.IsNullOrWhiteSpace(clientConfig.ApiAppSecret) ? Encrypter.TypistPasswordEncrypter(clientConfig.ApiAppSecret) : "";
            try
            {
                lock (typeof(IOHelper))
                    File.WriteAllText(ClientConfig.GetConfigFilePath(), Encrypter.SystemDataEncrypter(JsonConvert.SerializeObject((object)clientConfig)));
            }
            catch (Exception ex)
            {
                IOHelper.logger.Error((object)"IO ExMsg: ", ex);
                str = ex.Message;
            }
            clientConfig.TypistPassword = !string.IsNullOrWhiteSpace(clientConfig.TypistPassword) ? Encrypter.TypistPasswordDecrypter(clientConfig.TypistPassword) : "";
            clientConfig.ApiAppSecret = !string.IsNullOrWhiteSpace(clientConfig.ApiAppSecret) ? Encrypter.TypistPasswordDecrypter(clientConfig.ApiAppSecret) : "";
            return str;
        }

        public static string IOTest(string dir)
        {
            string str = (string)null;
            try
            {
                File.WriteAllText(Path.Combine(dir, "TestObject.txt"), new TestObject().toJsonString());
                File.ReadAllText(Path.Combine(dir, "TestObject.txt"), Encoding.Default);
            }
            catch (Exception ex)
            {
                IOHelper.logger.Error((object)"IO ExMsg: ", ex);
                str = ex.Message.Replace("TestObject.txt", "");
            }
            finally
            {
                try
                {
                    File.Delete(Path.Combine(dir, "TestObject.txt"));
                }
                catch (Exception ex)
                {
                    IOHelper.logger.Error((object)("IO ExMsg: " + ex.Message.Replace("TestObject.txt", "")));
                }
            }
            return str;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public static bool CheakFileIsWriting2222(string fileFullName)
        {
            IntPtr hObject = IOHelper._lopen(fileFullName, 66);
            if (hObject == IOHelper.HFILE_ERROR)
                return true;
            IOHelper.CloseHandle(hObject);
            return false;
        }

        public static bool CheakFileIsWriting(string fileFullName)
        {
            bool flag = false;
            FileStream fileStream = (FileStream)null;
            try
            {
                fileStream = File.Open(fileFullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                flag = true;
            }
            finally
            {
                if (fileStream != null)
                {
                    try
                    {
                        fileStream.Close();
                    }
                    catch (Exception ex)
                    {
                        IOHelper.logger.Error((object)ex);
                    }
                }
            }
            return flag;
        }

        public static string ClearData()
        {
            ClientConfig clientConfig = ClientConfig.LoadConfig();
            clientConfig.ApiAppId = "";
            clientConfig.ApiAppSecret = "";
            clientConfig.ApiToken = null;
            clientConfig.TypistPassword = "";
            clientConfig.IcNo = "";
            return IOHelper.InputConfigFile(clientConfig);
        }
    }
}
