using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TSP_OSM_Loader.Datasets;
using TSP_OSM_Loader.Topography;

namespace TSP_OSM_Loader
{
    internal class Program
    {
        internal static string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v3");

        static void Main(string[] args)
        {
            DateTime StartTime = DateTime.Now;

            WSMapImage.Startup();
            WorldCities.Startup();
            LiteDbService.Startup();
            Topo.Startup();
            //TestGeojson.LoadGeojson();
            OSMImport.ReadData();
            LiteDbService.DisposeAll();
            Console.WriteLine("");
            Console.WriteLine("We're done here! Took " + (DateTime.Now - StartTime).ToString(@"hh\:mm\:ss"));
            Console.ReadLine();
        }


        internal static string ReadResourceFile(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
