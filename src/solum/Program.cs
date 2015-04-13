using NLog;
using solum.core;
using solum.core.dataprocess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum
{
    class Program
    {
        static Logger Log = LogManager.GetLogger("main");        

        static void Main(string[] args)
        {
            //RunKeyValueTest();
            Server.RunDefaultServer();
            // RunProcessTests();
        }

        static void RunKeyValueTest()
        {
            using (var server = new Server())
            {
                var store = server.OpenKeyValueStore("test-db");
                store.Set("name", "Brad Serbu");

                string value;
                store.Get("name", out value);                
            }
        }

        static void RunProcessTests()
        {
            var source = SayHello(10);
            singleChainedProcess(source).Run();
            manyStepParallelProcess(source).Run();
            mixedProcess(source).Run();

            //var count = 0;
            //process.OnEntryProcessed += (_, entry) =>
            //{
            //    Log.Info("Processed entry #{0:N}", ++count);
            //};

            //process.OnFinished += (_, __) =>
            //{
            //    Log.Info("Processed {0} items.", count);
            //};

            //process.Run();
        }

        static DataProcess singleChainedProcess<T>(IEnumerable<T> source)
        {
            var count = 0;
            var process = DataProcess.With(SayHello(10))
                                     .Do(hello => Log.Info(hello))
                                     .Then(_ =>
                                     {
                                         Log.Info("Waiting 100 ms...");
                                         Thread.Sleep(100);
                                     })                                     
                                     .Then(hello =>
                                     {
                                         Log.Info("\t#{0:N}", ++count);

                                         return new
                                         {
                                             message = hello,
                                             count = count
                                         };
                                     })
                                    .Then(hello => Log.Info("\t {0}", hello))
                                    .Process;            

            return process;
        }
        static DataProcess manyStepParallelProcess<T>(IEnumerable<T> source)
        {
            var count = 0;
            var process = DataProcess.With(SayHello(10))
                                     .Do(hello => Log.Info(hello))
                                     .Do(_ =>
                                     {
                                         Log.Info("Waiting 100 ms...");
                                         Thread.Sleep(100);
                                     })
                                     .Do(hello =>
                                     {
                                         Log.Info("\t#{0:N}", ++count);

                                         return new
                                         {
                                             message = hello,
                                             count = count
                                         };
                                     })
                                    .Then(hello => Log.Info("\t {0}", hello))
                                    .Process;

            return process;
        }
        static DataProcess mixedProcess<T>(IEnumerable<T> source)
        {
            var count = 0;
            var process = DataProcess.With(SayHello(10))
                                     .Do(hello => Log.Info(hello))
                                     .Do(_ =>
                                     {
                                         Log.Info("Waiting 100 ms...");
                                         Thread.Sleep(100);
                                     })
                                     .Do(hello =>
                                     {
                                         Log.Info("\t#{0:N}", ++count);

                                         return new
                                         {
                                             message = hello,
                                             count = count
                                         };
                                     })
                                    .Then(hello => Log.Info("\t {0}", hello))
                                    .Process;

            return process;
        }

        static IEnumerable<string> SayHello(int count)
        {
            for (var lcv = 0; lcv < count; lcv++)
                yield return "Hello World!";
        }
    }
}
