using solum.extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    /// <summary>
    /// Static helpers and Singleton (Current) implementation
    /// </summary>
    partial class Server
    {
        #region Current (singleton) implementation
        public static object _locker = new object();
        public static Server _instance;
        public static Server Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadConfig(DEFAULT_SERVER_CONFG);
                        }
                    }
                }

                return _instance;
            }
        }
        #endregion
        /// <summary>
        /// Load a server using a configuration file
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        public static Server LoadConfig(string configPath)
        {
            var json = File.ReadAllText(configPath);
            var server = json.FromJson<Server>();

            return server;
        }
        public static void RunDefaultServer(bool promptToStart = PROMPT_TO_START)
        {
            using (var server = Server.Current)
            {
                server.Load();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("==> Server LOADED.");
                Console.ResetColor();

                // ** Add hook to cleanup storage if the program quits unexpectedly
#if LINUX
                Console.CancelKeyPress += delegate
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("***********************************");
                    Console.WriteLine("********* CTRL+C Detected *********");
                    Console.WriteLine("***********************************");
                    Console.ResetColor();

                    // This must cleanup all database resources
                    // TODO: Send "HALT" signal to all services
                    server.Storage.Close();
                };
#endif
#if !LINUX
                // ** Add hook to cleanup storage if the program quits unexpectedly                                
                ProgramExit.SetConsoleCtrlHandler(new solum.extensions.ProgramExit.HandlerRoutine(ctrl =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("***********************************");
                    Console.WriteLine("********* CTRL+C Detected *********");
                    Console.WriteLine("***********************************");
                    Console.ResetColor();

                    // This must cleanup all database resources
                    // TODO: Send "HALT" signal to all services
                    server.Storage.Close();

                    return true;
                }), true);
#endif
                // ** Prompt the user to start the server
                if (promptToStart)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("==> Press <ENTER> to start the server <==");
                    Console.ResetColor();

                    Console.ReadLine();
                }

                server.Start();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("==> Server STARTED.");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("==> Press <ENTER> to ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("STOP");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" the server <==");
                Console.ResetColor();

                Console.ReadLine();
                server.Stop();
            }
        }        
    }
}
