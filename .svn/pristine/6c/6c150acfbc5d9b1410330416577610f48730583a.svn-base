using log4net;
using log4net.Config;
using Microsoft.Win32;
using System.Net;
using System.Runtime.CompilerServices;
using ZslCustomsAssist.Runtime;

namespace ZslCustomsAssist
{
    internal static class Program
    {
        public static ILog logger = LogManager.GetLogger("Log4NetTest.LogTest");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Program.Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);

            try
            {
                Program.LogSystemMsg();
                ServicePointManager.DefaultConnectionLimit = 512;
                XmlConfigurator.Configure();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                bool createdNew;
                Mutex mutex = new Mutex(true, "中商旅报关辅助客户端", out createdNew);
                FormMain formMain = new FormMain();
                if (createdNew)
                {
                    Application.Run(formMain);
                }
                else
                {
                    formMain.WindowState = FormWindowState.Normal;
                    formMain.Activate();
                    int num = (int)MessageBox.Show("请勿重复打开应用。");
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error((object)"主程序运行异常！", ex);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void LogSystemMsg()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
            Program.logger.Info((object)("当前客户端版本号：" + ServerCore.GetVersion()));
            Program.logger.Info((object)("当前运行环境：" + registryKey.GetValue("ProductName").ToString() + "(" + (Environment.Is64BitOperatingSystem ? "64" : "32") + ") 版本号:" + registryKey.GetValue("CurrentBuildNumber").ToString()));
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => Program.logger.Error((object)"UT线程异常:", e.Exception);

        private static void CurrentDomain_UnhandledException(
          object sender,
          UnhandledExceptionEventArgs e)
        {
            Program.logger.Error((object)"未捕获的异常:", e.ExceptionObject as Exception);
        }
    }
}