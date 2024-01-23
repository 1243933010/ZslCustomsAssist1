using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZslCustomsAssist.Utils
{
    public static class ThreadsHelper
    {
        public static void RemoveCurrentThreadFromThreadList(List<ThreadExt> threads, object lockObj)
        {
            lock (lockObj)
            {
                for (int index = 0; index < threads.Count; ++index)
                {
                    if (Thread.CurrentThread.ManagedThreadId == threads[index].Thread.ManagedThreadId)
                    {
                        threads[index].TokenSource.Cancel();
                        threads.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        public static List<Process> FindProcessByName(string processName)
        {
            string[] strArray = processName.Split(',');
            Process[] processes = Process.GetProcesses();
            List<Process> processByName = new List<Process>();
            foreach (Process process in processes)
            {
                foreach (string str in strArray)
                {
                    if (process.ProcessName.Equals(str, StringComparison.OrdinalIgnoreCase))
                        processByName.Add(process);
                }
            }
            return processByName;
        }

        public static void KillProcessByProcessName(string name)
        {
            List<Process> processByName = ThreadsHelper.FindProcessByName(name);
            if (processByName == null || processByName.Count <= 0)
                return;
            foreach (Process process in processByName)
                process.Kill();
        }

        public static void AddThreads(List<ThreadExt> reportThreads, Action action, CancellationTokenSource TokenSource)
        {
            try
            {
                ThreadExt threadExt = new()
                {
                    Thread = new(new ThreadStart(action.Invoke)),
                    TokenSource = TokenSource
                };
                reportThreads.Add(threadExt);
                threadExt.Thread.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("将继续尝试添加进程！", ex);
            }
        }
    }
}
