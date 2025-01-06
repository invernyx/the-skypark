using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;

namespace TSP_Transponder.Models
{
    public class SimLibrary
    {
        public static bool HasSimInstalled = false;
        public static List<Simulator> SimList = new List<Simulator>()
        {
            /*
            new Simulator()
            {
                Platform = "Prepar3D",
                Name = "P3Dv4",
                NameStandard = "p3dv4",
                MajorVersion = 4,
                Exe = "prepar3d.exe",
                RegSection = Registry.CurrentUser,
                RegPath = @"Software\Lockheed Martin\Prepar3D v4",
                RegKey = @"AppPath",
                FlightPlansFolders = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Prepar3D v4 Files")
                },
                Connector = new ConnectorInstance_P3D(),
                //P3D_AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Lockheed Martin\Prepar3D v4",
                //P3D_SimObjectsCFGPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Lockheed Martin\Prepar3D v4\simobjects.cfg",
                P3D_SceneryCFGPath = (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Lockheed Martin\Prepar3D v4\scenery.cfg"),
                P3D_Addons = new List<string>()
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Lockheed Martin\Prepar3D v4\add-ons.cfg",
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Lockheed Martin\Prepar3D v4\add-ons.cfg",
                },
                SceneryExclusions = new List<string>()
                {
                    "FTX_VECTOR_AEC",
                    "FTX_VECTOR_APT",
                    "FTX_VECTOR_EXX",
                    "FTX_VECTOR_CVX",
                    "FTX_VECTOR_OBJ",
                    "OLC_EU_LIGHTS",
                    "OLC_NA_LIGHTS",
                    "OLC_SA_LIGHTS",
                    "FTXAU05_ROADS",
                    "FreeMesh",
                    "FSGU_NG",
                    "FSGUX",
                    "ORBXLIBS",
                    "Navigation Data",
                },
                SceneryCorrections = new List<string>()
                {
                    "FTX_VECTOR_FixedAPT",
                    "_elevation_adjustment",
                }
            },
            new Simulator()
            {
                Platform = "Prepar3D",
                Name = "P3Dv5",
                NameStandard = "p3dv5",
                MajorVersion = 5,
                Exe = "prepar3d.exe",
                RegSection = Registry.CurrentUser,
                RegPath = @"Software\Lockheed Martin\Prepar3D v5",
                RegKey = @"AppPath",
                FlightPlansFolders = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Prepar3D v5 Files")
                },
                Connector = new ConnectorInstance_P3D(),
            },
            */
            new Simulator()
            {
                Platform = "Flight Simulator",
                Name = "MSFS",
                NameStandard = "msfs2020",
                Exe = "FlightSimulator.exe",
                FlightPlansFolders = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalState"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft Flight Simulator"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.FlightSimDisc_8wekyb3d8bbwe", "LocalState")
                },
                ConfigsFolders = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft Flight Simulator"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.FlightSimDisc_8wekyb3d8bbwe", "LocalCache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache")
                },
                Connector = new ConnectorInstance_MSFS(),
            },
        };
        
        public static void Startup()
        {
            Console.WriteLine("Populating Simulators");

            // Process each sim from m_simsList in reverse order since we must check FSX after FSX:SE in case of Co-Existance
            foreach (Simulator sim in SimList)
            {
                SimConnection.Connectors.Add(sim.Connector);

                if(sim.RegPath != string.Empty)
                {
                    // Retreive the Simulator Path from Registry
                    RegistryKey key = sim.RegSection.OpenSubKey(sim.RegPath);

                    if (key != null)
                    {
                        // path is in registry so retreive the simulator setup path
                        sim.InstallDirectory = CleanFileName((string)key.GetValue(sim.RegKey)) + @"\";

                        if (sim.InstallDirectory != null)
                        {
                            // Check if the sim .exe exist
                            if (File.Exists(Path.Combine(sim.InstallDirectory, sim.Exe)))
                            {
                                sim.IsInstalled = true;
                                HasSimInstalled = true;
                                Console.WriteLine("Found " + sim.Name + " in " + sim.InstallDirectory);
                                sim.Connector.Configure(sim);
                            }
                        }
                    }
                }
                else
                {
                    sim.IsInstalled = true;
                    HasSimInstalled = true;
                    Console.WriteLine("Found " + sim.Name + " in " + sim.InstallDirectory);
                    sim.Connector.Configure(sim);
                }
            }

        }

        public static string CleanFileName(string fileName)
        {
            if (fileName == null)
            {
                return null;
            }
            else
            {
                return fileName.Replace("\0", "").Trim('\\');
            }
        }

        public static Encoding GetEncoding(string filePath)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }

        public class Simulator
        {
            public string Platform = "";
            public string Name = "";
            public string NameStandard = "";
            public string Exe = "";
            public string ExeComp = "";
            public List<string> ConfigsFolders = new List<string>();
            public List<string> FlightPlansFolders = new List<string>();
            public List<string> AircraftDirs = new List<string>();
            public ConnectorInstance_Base Connector = null;
            public AirportsLib AirportsLib = null;
            public Process Proc = null;
            public bool IsAdmin = false;
            public bool IsRunning = false;
            public bool IsInstalled = false;
            public bool IsAutoLaunched = false;
            public string InstallDirectory = "";
            public RegistryKey RegSection; 
            public string RegPath = "";
            public string RegKey = "";
            public int MajorVersion = 0;
            
            public Simulator()
            {
                AirportsLib = new AirportsLib(this);
            }

            public void Startup()
            {
                AirportsLib.Startup();
            }

            public override string ToString()
            {
                return Platform + " v" + MajorVersion;
            }
        }

    }
}
