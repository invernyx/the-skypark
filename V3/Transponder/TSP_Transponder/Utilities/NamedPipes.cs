using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static TSP_Transponder.Utilities.NamedPipesServer;

namespace TSP_Transponder.Utilities
{
    class NamedPipesServer
    {
        private StreamWriter Writer = null;
        private StreamReader Reader = null;
        private NamedPipeServerStream Stream = null;
        public Action<string> Response = null;

        internal NamedPipesServer()
        {
            const string pipeName = @"NP_TSPTransponder";

            new Thread(() =>
            {
                RunServer(pipeName);
            }).Start();
            
        }
        
        internal void Send(string message)
        {
            Writer.WriteLine(message);
        }

        private async void GetMessages(NamedPipeServerStream str)
        {
            Reader = new StreamReader(str);
            Writer = new StreamWriter(str)
            {
                AutoFlush = true,
            };

            Send("hello");
            while (!Reader.EndOfStream)
            {
                string line = await Reader.ReadLineAsync();
                if (line != null)
                {
                    //Console.WriteLine(line);
                    Response?.Invoke(line);

                    if (line == "bye")
                    {
                        break;
                    }
                }
            }

        }

        private void RunServer(string pipeName)
        {
            try
            {
                PipeSecurity security = new PipeSecurity();
                security.AddAccessRule(new PipeAccessRule($"{Environment.UserDomainName}\\{Environment.UserName}", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
                Stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, security);

                Stream.BeginWaitForConnection(asyncResult =>
                {
                    using (NamedPipeServerStream Str = (NamedPipeServerStream)asyncResult.AsyncState)
                    {
                        try
                        {
                            Str.EndWaitForConnection(asyncResult);
                        }
                        catch
                        {
                        }

                        Str.WaitForPipeDrain();
                        GetMessages(Str);
                        Stream.Disconnect();
                        Stream.Dispose();
                        RunServer(pipeName);
                    }
                }, Stream);
            }
            catch
            {
                //string pn = Process.GetCurrentProcess().ProcessName;
                //Process[] OldTransponder = Process.GetProcessesByName(pn);
                //foreach (var transponder in OldTransponder)
                //{
                //    if (transponder.Id != Process.GetCurrentProcess().Id)
                //    {
                //        Console.WriteLine("Killing old Transponder");
                //        transponder.Kill();
                //    }
                ////}

#if !DEBUG
                //App.RunUpdater((r) => { return 0; }, false, true, true);
#endif
                return;
            }
        }


    }
    
}
