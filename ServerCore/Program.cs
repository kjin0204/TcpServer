using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static ThreadLocal<string> threadName = new ThreadLocal<string>(() => { return $"My Name Is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAml()
        {
            //threadName.Value = 
            bool repeat = threadName.IsValueCreated;
            if(repeat)
                Console.WriteLine(threadName.Value + "(repeat)");
            else
                Console.WriteLine(threadName.Value );

            //Thread.Sleep(1000);

        }
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            //쓰레드 풀에서 사용 
            Parallel.Invoke(WhoAml, WhoAml, WhoAml, WhoAml, WhoAml, WhoAml, WhoAml, WhoAml);
        }
    }
}
