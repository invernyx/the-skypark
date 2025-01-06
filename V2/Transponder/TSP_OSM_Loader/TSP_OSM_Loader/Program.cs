using System;
using System.IO;
using System.Threading;

namespace TSP_OSM_Loader
{
    class Program
    {
        internal static string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\");

        static void Main(string[] args)
        {
            DateTime StartTime = DateTime.Now;
            LiteDbService.Startup();
            OSMImport.ReadData();
            LiteDbService.DisposeAll();
            Console.WriteLine("");
            Console.WriteLine("We're done here! Took " + (DateTime.Now - StartTime).ToString(@"hh\:mm\:ss"));
            Console.ReadLine();
        }
    }
}
