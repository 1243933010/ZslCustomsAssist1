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
                Mutex mutex = new Mutex(true, "�����ñ��ظ����ͻ���", out createdNew);
                FormMain formMain = new FormMain();
                if (createdNew)
                {
                    Application.Run(formMain);
                }
                else
                {
                    formMain.WindowState = FormWindowState.Normal;
                    formMain.Activate();
                    int num = (int)MessageBox.Show("�����ظ���Ӧ�á�");
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error((object)"�����������쳣��", ex);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void LogSystemMsg()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
            Program.logger.Info((object)("��ǰ�ͻ��˰汾�ţ�" + ServerCore.GetVersion()));
            Program.logger.Info((object)("��ǰ���л�����" + registryKey.GetValue("ProductName").ToString() + "(" + (Environment.Is64BitOperatingSystem ? "64" : "32") + ") �汾��:" + registryKey.GetValue("CurrentBuildNumber").ToString()));
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => Program.logger.Error((object)"UT�߳��쳣:", e.Exception);

        private static void CurrentDomain_UnhandledException(
          object sender,
          UnhandledExceptionEventArgs e)
        {
            Program.logger.Error((object)"δ������쳣:", e.ExceptionObject as Exception);
        }
    }
}