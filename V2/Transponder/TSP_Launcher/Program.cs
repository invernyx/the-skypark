using System;
using System.Diagnostics;
using System.IO;

namespace TSP_Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Process[] transponders = Process.GetProcessesByName("transponder");

                foreach(Process p in transponders)
                {
                    p.WaitForExit(10000);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "Transponder.exe"), Environment.CommandLine);
                Process.Start(startInfo);
            }
            catch
            {

            }
        }
    }
}
