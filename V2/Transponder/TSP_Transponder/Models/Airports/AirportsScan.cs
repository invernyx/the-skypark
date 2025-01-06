using Orbx.DataManager.Core.Bgl.AirportSubRecords;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using static TSP_Transponder.Models.SimLibrary;
using System.Threading;
using System.Windows;
using TSP_Transponder.Models.Topography;
using TSP_Transponder.Models.Topography.Utils;
using TSP_Transponder.Utilities;
using TSP_Transponder.Models.DataStore;
using LiteDB;

#if DEBUG
using System.Data.SQLite;
#endif

namespace TSP_Transponder.Models.Airports
{
    class AirportsScan
    {
        //private readonly string CacheVersion = "v2.6";
        private List<Airport> CSVAPTList = new List<Airport>();
        private Dictionary<string, Airport> CSVAPTDict = new Dictionary<string, Airport>();
        private Simulator Sim = null;

        internal string CacheDate = "";
        
        internal List<int> StatsValues = new List<int>()
        {
            0,
            0,
            0,
            0,
            0,
            0,
        };
        internal List<TextBlock> StatsFields = new List<TextBlock>();
        internal List<string> StatsParamsName = new List<string>()
        {
            "Airports",
            "Runways",
            "Parkings",
            "Default Apt",
            "Custom Apt",
            "Files",
        };

        internal AirportsScan(AirportsLib _APTLib, Simulator _Sim)
        {
            Sim = _Sim;

        }

        internal void ScanP3D()
        {
            LoadCSV();

            /*
            Task.Factory.StartNew(() =>
            {
                CultureInfo.CurrentCulture = App.CI;

                App.MW.PopulateUISceneryLists();

                #region Init
                List<string> LookupList = new List<string>();
                Dictionary<string, Airport> APTDict = new Dictionary<string, Airport>();
                Dictionary<int, List<string>> NewCache = new Dictionary<int, List<string>>();
                Dictionary<string, List<string>> GeoFiles = new Dictionary<string, List<string>>();
                Dictionary<string, List<Airport>> FilesDict = new Dictionary<string, List<Airport>>();
                Dictionary<string, List<Airport>> CustomAirportsDirs = new Dictionary<string, List<Airport>>();
                List<Airport> NewCustomAirports = new List<Airport>();
                List<Airport> NewAPTList = new List<Airport>();
                string WorkingDir = Path.Combine(App.AppDataDirectory, "Scenery Library");
                string WorkingSimDir = Path.Combine(WorkingDir, Sim.NameStandard.ToUpper());
                string CacheFile = Path.Combine(WorkingSimDir, "Cache.txt");
                string LookupFile = Path.Combine(WorkingSimDir, "Lookup.txt");
                bool Rescan = false;
                bool UpdateFiles = false;
                int FileAt = 0;
                #endregion

                #region Convert Cache to Airport Library
                if (File.Exists(LookupFile))
                {
                    try
                    {
                        int i = 0;
                        string[] lines = File.ReadAllLines(LookupFile);
                        foreach (string line in lines)
                        {
                            if (i > 1)
                            {
                                try
                                {
                                    string[] lineSpl = line.Split('\t');
                                    string[] LocationInput = lineSpl[4].Split(',');
                                    GeoLoc Location = new GeoLoc(Convert.ToDouble(LocationInput[0]), Convert.ToDouble(LocationInput[1]));

                                    #region Find Geo
                                    List<string> GeoFile = new List<string>();
                                    string GeoFileName = Math.Round(Location.Lon).ToString() + '_' + Math.Round(Location.Lat).ToString();
                                    if (GeoFiles.ContainsKey(GeoFileName))
                                    {
                                        GeoFile = GeoFiles[GeoFileName].ToList();
                                    }
                                    else
                                    {
                                        GeoFile = File.ReadAllLines(Path.Combine(WorkingSimDir, GeoFileName + ".txt")).ToList();
                                        GeoFiles.Add(GeoFileName, GeoFile);
                                    }
                                    #endregion

                                    #region Create Placeholder Data
                                    List<Airport.Runway> rw = new List<Airport.Runway>();
                                    List<Airport.Parking> pk = new List<Airport.Parking>();
                                    List<Airport.TaxiwayPath> tp = new List<Airport.TaxiwayPath>();
                                    List<Airport.TaxiwayNode> tn = new List<Airport.TaxiwayNode>();
                                    List<Airport.ApronGeometry> ap = new List<Airport.ApronGeometry>();

                                    int pkCount = Convert.ToInt16(lineSpl[11]);
                                    int tpCount = Convert.ToInt16(lineSpl[12]);
                                    float RadiusNM = 5;
                                    #endregion

                                    #region Process Geo File
                                    foreach (string l in GeoFile)
                                    {
                                        if (l.StartsWith(lineSpl[0]))
                                        {
                                            string[] sect = l.Split('\t');
                                            if (sect.Length > 1)
                                            {
                                                // Runways
                                                if (sect[1] != string.Empty)
                                                {
                                                    string[] rw1 = sect[1].Split(';');
                                                    foreach (string rw2 in rw1)
                                                    {
                                                        if (rw2 != string.Empty)
                                                        {
                                                            rw.Add(new Airport.Runway(rw2));
                                                            StatsValues[1]++;
                                                        }
                                                    }
                                                }

                                                // Parkings
                                                if (sect[2] != string.Empty)
                                                {
                                                    string[] pk1 = sect[2].Split(';');
                                                    ushort pni = 0;
                                                    foreach (string pk2 in pk1)
                                                    {
                                                        if (pk2 != string.Empty)
                                                        {
                                                            pk.Add(new Airport.Parking(pk2, pni));
                                                            StatsValues[2]++;
                                                            pni++;
                                                        }
                                                    }
                                                }

                                                // Taxi Nodes
                                                if (sect[3] != string.Empty)
                                                {
                                                    string[] tn1 = sect[3].Split(';');
                                                    ushort tni = 0;
                                                    foreach (string tn2 in tn1)
                                                    {
                                                        if (tn2 != string.Empty)
                                                        {
                                                            tn.Add(new Airport.TaxiwayNode(tn2, tni));
                                                            tni++;
                                                        }
                                                    }
                                                }

                                                // Taxi Paths
                                                if (sect[4] != string.Empty)
                                                {
                                                    string[] tp1 = sect[4].Split(';');
                                                    foreach (string tp2 in tp1)
                                                    {
                                                        if (tp2 != string.Empty)
                                                        {
                                                            tp.Add(new Airport.TaxiwayPath(tp2));
                                                        }
                                                    }
                                                }

                                                // Aprons
                                                if (sect[5] != string.Empty)
                                                {
                                                    string[] ap1 = sect[5].Split(';');
                                                    foreach (string ap2 in ap1)
                                                    {
                                                        if (ap2 != string.Empty)
                                                        {
                                                            ap.Add(new Airport.ApronGeometry(ap2));
                                                        }
                                                    }
                                                }

                                                // Radius
                                                if (sect[6] != string.Empty)
                                                {
                                                    RadiusNM = Convert.ToSingle(sect[6]);
                                                }

                                            }

                                            break;
                                        }
                                    }
                                    #endregion

                                    #region Create Airport Object
                                    Airport outputAirport = new Airport()
                                    {
                                        ICAO = lineSpl[0],
                                        Hash = lineSpl[1],
                                        IsCustom = (CustomLevel)Convert.ToInt16(lineSpl[2]),
                                        Location = Location,
                                        Elevation = Convert.ToSingle(lineSpl[5]),
                                        Radius = RadiusNM,
                                        Name = lineSpl[6],
                                        City = lineSpl[7],
                                        State = lineSpl[8],
                                        Country = lineSpl[9],
                                        Runways = rw,
                                        Parkings = pk,
                                        TaxiNodes = tn,
                                        TaxiPaths = tp,
                                        Aprons = ap,
                                        FileName = lineSpl[13],
                                    };
                                    #endregion

                                    #region Add File to File Dictionary
                                    if (lineSpl[13] != "")
                                    {
                                        string parent = Directory.GetParent(lineSpl[13]).FullName;
                                        if (Directory.Exists(parent))
                                        {
                                            if (!FilesDict.ContainsKey(parent))
                                            {
                                                FilesDict.Add(parent, new List<Airport>());
                                            }
                                            FilesDict[parent].Add(outputAirport);
                                        }
                                    }
                                    #endregion

                                    #region Check Custom Airports
                                    if (outputAirport.IsCustom > 0)
                                    {
                                        if (lineSpl[13] != string.Empty)
                                        {
                                            StatsValues[4]++;

                                            string parent1 = Directory.GetParent(lineSpl[13]).FullName;
                                            if (Directory.Exists(parent1))
                                            {
                                                if (!CustomAirportsDirs.ContainsKey(parent1))
                                                {
                                                    CustomAirportsDirs.Add(parent1, new List<Airport>());
                                                }
                                                CustomAirportsDirs[parent1].Add(outputAirport);
                                            }
                                            NewCustomAirports.Add(outputAirport);
                                        }
                                    }
                                    else
                                    {
                                        StatsValues[3]++;
                                    }
                                    #endregion

                                    StatsValues[0]++;
                                    if (!APTDict.ContainsKey(outputAirport.ICAO))
                                    {
                                        NewAPTList.Add(outputAirport);
                                        APTDict.Add(outputAirport.ICAO, outputAirport);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed to load Airport (" + ex.Message + ") on " + line);
                                }
                            }
                            else if (i == 1)
                            {
                                #region Check cache date
                                CacheDate = line;
                                #endregion
                            }
                            else if (i == 0)
                            {
                                if (line != CacheVersion)
                                {
                                    Rescan = true;
                                    break;
                                }
                            }
                            i++;
                        }
                    }
                    catch
                    {
                        Rescan = true;
                    }
                }
                else
                {
                    Rescan = true;
                }

                #endregion

                if (!Rescan)
                {
                    Airports = NewAPTList;
                    CreateDerivatives();
                    //APTLib.CalculateRelevancy();
                    Plans.Startup();
                    Blogs.ReadBlog(0);
                    App.MW.ResetValidateLoop();

                }

                #region Get Current BGLs
                App.MW.UpdateSceneryProcess(Sim, 0, "Getting Scenery files...");
                SortedDictionary<int, List<string>> BGLPaths = Airports_ESP.GatherBGLPaths(Sim);
                List<string> BGLPathsCombined = new List<string>();
                foreach (KeyValuePair<int, List<string>> paths in BGLPaths)
                {
                    BGLPathsCombined.AddRange(paths.Value);
                }
                #endregion

                #region Validate Cache
                if (File.Exists(CacheFile))
                {
                    App.MW.UpdateSceneryProcess(Sim, 0, "Reading Cache...");
                    SortedDictionary<int, List<string>> RemovedFilesFromScan = new SortedDictionary<int, List<string>>();
                    List<string> CacheLines = File.ReadAllLines(CacheFile).ToList();
                    List<string> CacheFiles = new List<string>();
                    List<string> RemoveFromCache = new List<string>();
                    int i = 0;

                    if (!File.Exists(LookupFile))
                    {
                        Rescan = true;
                    }

                    #region List Cache Files
                    foreach (string line in CacheLines)
                    {
                        CacheFiles.Add(line.Split('\t')[0]);
                    }
                    #endregion

                    #region Check for new files
                    App.MW.UpdateSceneryProcess(Sim, 0, "Checking new files...");
                    foreach (string file in BGLPathsCombined)
                    {
                        if (!App.MW.IsShuttingDown && !CacheFiles.Contains(file))
                        {
                            Rescan = true;
                            break;
                        }

                    }
                    #endregion

                    #region Check for removed files
                    if (!Rescan)
                    {
                        App.MW.UpdateSceneryProcess(Sim, 0, "Checking removed files...");
                        while (i < CacheLines.Count && !Rescan && !App.MW.IsShuttingDown)
                        {
                            if (i > 1)
                            {
                                string[] pathSplit = CacheLines[i].Split('\t');
                                if (pathSplit.Length < 4)
                                {
                                    Rescan = true;
                                    break;
                                }

                                // Remove if the path no longer exist
                                if (!BGLPathsCombined.Contains(pathSplit[0]))
                                {
                                    Rescan = true;
                                    break;
                                }
                            }
                            i++;
                        }
                    }
                    #endregion

                    #region Compare Cache vs Real
                    if (!Rescan)
                    {
                        i = 0;
                        foreach (string CacheLine in CacheLines)
                        {
                            if (App.MW.IsShuttingDown)
                            {
                                return;
                            }

                            if (i == 0)
                            {
                                #region Check if the Cache file is valid for the current version
                                if (CacheLine != CacheVersion)
                                {
                                    int index = 0;
                                    while (index < StatsValues.Count)
                                    {
                                        StatsValues[index] = 0;
                                        index++;
                                    }

                                    // Empty the Scenery Library for current sim
                                    string[] filePaths = Directory.GetFiles(WorkingSimDir);
                                    foreach (string filePath in filePaths)
                                    {
                                        try
                                        {
                                            File.Delete(filePath);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Failed to delete file: " + ex.Message);
                                        }
                                    }
                                    break;
                                }
                                #endregion
                            }
                            else
                            if (i == 1)
                            {
                            }
                            else
                            {
                                try
                                {
                                    string[] pathSplit = CacheLine.Split('\t');
                                    int layer = Convert.ToInt32(pathSplit[2]);

                                    #region Check if the cached file exists
                                    if (!File.Exists(pathSplit[0]))
                                    {
                                        if (pathSplit[3] == "1")
                                        {
                                            Rescan = true;
                                            break;
                                        }
                                        else
                                        {
                                            AddToNewCache(Convert.ToInt32(pathSplit[2]), CacheLine, NewCache);
                                            AddFileToLayers(Convert.ToInt32(pathSplit[2]), RemovedFilesFromScan, pathSplit[0]);
                                            continue;
                                        }
                                    }
                                    #endregion

                                    #region Check if the file is scheduled to be scanned
                                    bool IsInLayer = true;
                                    if (BGLPaths.ContainsKey(layer))
                                    {
                                        if (!BGLPaths[layer].Contains(pathSplit[0]))
                                        {
                                            IsInLayer = false;
                                        }
                                    }
                                    else
                                    {
                                        IsInLayer = false;
                                    }

                                    if (!IsInLayer)
                                    {
                                        if (pathSplit[3] == "1")
                                        {
                                            RemoveFromCache.Add(CacheLine);
                                            Rescan = true;
                                            break;
                                        }
                                    }

                                    #endregion

                                    #region Check if the file has changed
                                    if (Utils.TimeStamp(File.GetLastWriteTimeUtc(pathSplit[0])) == Convert.ToInt64(pathSplit[1]) && RemoveFromCache.Count == 0)
                                    {
                                        AddToNewCache(Convert.ToInt32(pathSplit[2]), CacheLine, NewCache);
                                        AddFileToLayers(Convert.ToInt32(pathSplit[2]), RemovedFilesFromScan, pathSplit[0]);
                                        StatsValues[5]++;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    #endregion

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed to Exclude path from Scenery scan: " + CacheLine + " / " + ex.Message);
                                }
                            }
                            i++;
                        }
                    }
                    #endregion

                    #region Rescan -- Filter non-airport files, update RemovedFilesFromScan and new Cache
                    // Rescan every scenery except ones without airports
                    int i2 = 0;
                    foreach (string line in CacheLines)
                    {
                        if (i2 > 1)
                        {
                            string[] pathSplit1 = line.Split('\t');
                            if (pathSplit1.Length < 4)
                            {
                                break;
                            }
                            if (pathSplit1[3] == "0")
                            {
                                if (BGLPathsCombined.Contains(pathSplit1[0]))
                                {
                                    AddToNewCache(Convert.ToInt32(pathSplit1[2]), line, NewCache);
                                    AddFileToLayers(Convert.ToInt32(pathSplit1[2]), RemovedFilesFromScan, pathSplit1[0]);
                                    StatsValues[5]++;
                                }
                            }
                        }
                        i2++;
                    }

                    if (Rescan)
                    {
                        UpdateFiles = true;
                        StatsValues[5] = 0;
                        NewCache.Clear();
                        RemovedFilesFromScan.Clear();

                        App.MW.UpdateSceneryProcess(Sim, 0, "Filtering...");

                        NewCache.Clear();
                        RemovedFilesFromScan.Clear();

                        i = 0;
                        foreach (string CacheLine in CacheLines)
                        {
                            if (i > 1)
                            {
                                try
                                {
                                    string[] pathSplit = CacheLine.Split('\t');
                                    if (pathSplit.Length < 4)
                                    {
                                        break;
                                    }
                                    if (pathSplit[3] == "0")
                                    {
                                        if (BGLPathsCombined.Contains(pathSplit[0]))
                                        {
                                            AddToNewCache(Convert.ToInt32(pathSplit[2]), CacheLine, NewCache);
                                            AddFileToLayers(Convert.ToInt32(pathSplit[2]), RemovedFilesFromScan, pathSplit[0]);
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            i++;
                        }
                    }
                    #endregion

                    #region Remove files from the scan list
                    foreach (KeyValuePair<int, List<string>> layer in RemovedFilesFromScan)
                    {
                        if (BGLPaths.ContainsKey(layer.Key))
                        {
                            foreach (string file in layer.Value)
                            {
                                if (BGLPaths[layer.Key].Contains(file))
                                {
                                    BGLPaths[layer.Key].Remove(file);
                                }
                            }

                            if (BGLPaths[layer.Key].Count == 0)
                            {
                                BGLPaths.Remove(layer.Key);
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                #region Process BGLs
                int FileQty = 0;
                foreach (KeyValuePair<int, List<string>> paths in BGLPaths)
                {
                    FileQty += paths.Value.Count;
                }
                Console.WriteLine(FileQty + " bgl to scan");
                FileAt = 0;
                List<KeyValuePair<int, List<string>>> Sorted = BGLPaths.OrderBy((kvp) =>
                {
                    if (kvp.Key == -1)
                    {
                        return 9999999;
                    }
                    else
                    {
                        return kvp.Key;
                    }
                }).ToList();
                foreach (KeyValuePair<int, List<string>> layer in Sorted)
                {
                    foreach (string FilePath in layer.Value)
                    {
                        if (App.MW.IsShuttingDown)
                        {
                            return;
                        }

                        try
                        {
                            //Console.WriteLine("Now Reading " + FilePath);

                            StatsValues[5]++;
                            bool HasAirport = false;
                            BglFile newBgl = new BglFile(FilePath);
                            newBgl.GetData();

                            IEnumerable<Section> NameSections = newBgl.Sections.Where(q => q is NameListSection);
                            IEnumerable<Section> AirportSections = newBgl.Sections.Where(q => q is AirportSection);

                            List<Icao> AirportsMeta = new List<Icao>();
                            Parallel.ForEach(NameSections, (SectionAbstract) =>
                            {
                                NameListSection Section = (NameListSection)SectionAbstract;
                                Section.GetData(newBgl.Reader);
                                AirportsMeta.AddRange(Section.Icaos);

                            });

                            Parallel.ForEach(AirportSections, (SectionAbstract) =>
                            {
                                AirportSection Section = (AirportSection)SectionAbstract;
                                Section.GetData(newBgl.Reader);

                                foreach (AirportRecord airport in Section.Airports)
                                {
                                    if (App.MW.IsShuttingDown)
                                    {
                                        return;
                                    }

                                    if (airport.Runways.Count == 0 &&
                                        airport.TaxiPoints.Count == 0 &&
                                        airport.Helipads.Count == 0 &&
                                        airport.Starts.Count == 0)
                                    {
                                        continue;
                                    }

                                    double AirportHash = 0;
                                    CustomLevel IsCustom = CustomLevel.Default;
                                    StatsValues[0]++;
                                    HasAirport = true;
                                    AirportHash += airport.Location.Longitude + airport.Location.Latitude + airport.Altitude;

                                    //if (FilePath.ToLower().StartsWith(Path.Combine(Sim.InstallDirectory, Sim.DefaultSceneryPath).ToLower()))
                                    //{
                                    //    StatsValues[3]++; // Default
                                    //}
                                    //else
                                    //{
                                    //    StatsValues[4]++; // Custom
                                    //    IsCustom = CustomLevel.ComplexAirport;
                                    //}

                                    foreach (TaxiPoint tp in airport.TaxiPoints)
                                    {
                                        AirportHash += (tp.Location.Longitude - airport.Location.Longitude) + (tp.Location.Latitude - airport.Location.Latitude);
                                    }

                                    Icao AirportMeta = AirportsMeta.Find(x => x.IcaoIdent == airport.ICAO);

                                    //Airport CSVAirport = new Airport();
                                    //if (CSVAPTDict.ContainsKey(airport.ICAO))
                                    //{
                                    //    CSVAirport = CSVAPTDict[airport.ICAO];
                                    //}

                                    Airport apt = new Airport()
                                    {
                                        Name = airport.Name,
                                        ICAO = airport.ICAO,
                                        Location = new GeoLoc(airport.Location.Longitude, airport.Location.Latitude),
                                        Elevation = (float)airport.Altitude,
                                        Country = Utils.ConvertESPCountry(AirportMeta.CountryName),
                                        State = AirportMeta.StateName,
                                        City = AirportMeta.CityName,
                                        IsCustom = IsCustom,
                                        FileName = FilePath,
                                    };
                                    UpdateFiles = true;

                                    #region Runways
                                    foreach (Runway rw in airport.Runways)
                                    {
                                        if (rw.Length != rw.Width)
                                        {
                                            AirportHash += (rw.Location.Longitude - airport.Location.Longitude) + (rw.Location.Latitude - airport.Location.Latitude);
                                            apt.Runways.Add(new Airport.Runway(apt, rw));
                                        }
                                    }
                                    StatsValues[1] += airport.Runways.Count;
                                    #endregion

                                    #region Parkings
                                    int ParkingPointIter = 0;
                                    foreach (TaxiParking pk in airport.Parking)
                                    {
                                        AirportHash += (pk.Location.Longitude - airport.Location.Longitude) + (pk.Location.Latitude - airport.Location.Latitude);
                                        apt.Parkings.Add(new Airport.Parking(pk, (ushort)ParkingPointIter));
                                        ParkingPointIter++;
                                    }
                                    StatsValues[2] += airport.Parking.Count;
                                    #endregion

                                    #region TaxiPoint
                                    int TaxiPointIter = 0;
                                    foreach (TaxiPoint tp in airport.TaxiPoints)
                                    {
                                        AirportHash += (tp.Location.Longitude - airport.Location.Longitude) + (tp.Location.Latitude - airport.Location.Latitude);
                                        apt.TaxiNodes.Add(new Airport.TaxiwayNode(tp, (ushort)TaxiPointIter));
                                        TaxiPointIter++;
                                    }
                                    #endregion

                                    #region TaxiPath
                                    foreach (TaxiPath tp in airport.Paths)
                                    {
                                        apt.TaxiPaths.Add(new Airport.TaxiwayPath(tp));
                                    }
                                    #endregion

                                    #region Apron
                                    foreach (Apron ap in airport.Aprons)
                                    {
                                        apt.Aprons.Add(new Airport.ApronGeometry(ap));
                                    }
                                    #endregion

                                    #region Check for Customs
                                    if (IsCustom != 0)
                                    {
                                        string parent = Directory.GetParent(FilePath).FullName;
                                        if (!CustomAirportsDirs.ContainsKey(parent))
                                        {
                                            Console.WriteLine("Airport " + apt.ICAO + " is Custom " + apt.IsCustom + " / " + apt.FileName);
                                            CustomAirportsDirs.Add(parent, new List<Airport>());
                                        }
                                        CustomAirportsDirs[parent].Add(apt);
                                    }
                                    #endregion

                                    #region Find Airport Radius
                                    double FarthestNode = 1;
                                    foreach (Airport.TaxiwayNode nd in apt.TaxiNodes)
                                    {
                                        double NDDist = Utils.MapCalcDist(apt.Location, nd.Location, Utils.DistanceUnit.NauticalMiles);
                                        if (FarthestNode < NDDist)
                                        {
                                            FarthestNode = NDDist;
                                        }
                                    }
                                    apt.Radius = (float)Math.Round(FarthestNode, 5) + 0.03f;
                                    #endregion

                                    AirportHash *= 10e10;
                                    AirportHash = Math.Round(AirportHash);
                                    apt.Hash = AirportHash.ToString();

                                    if (APTDict.ContainsKey(airport.ICAO))
                                    {
                                        StatsValues[0]--;
                                        StatsValues[1] -= APTDict[airport.ICAO].Runways.Count;
                                        StatsValues[2] -= APTDict[airport.ICAO].Parkings.Count;

                                        if (APTDict[airport.ICAO].IsCustom > 0)
                                        {
                                            StatsValues[4]--; // Custom
                                        }
                                        else
                                        {
                                            StatsValues[3]--; // Default
                                        }
                                        NewAPTList.Remove(APTDict[airport.ICAO]);
                                        APTDict.Remove(airport.ICAO);
                                    }
                                    APTDict.Add(airport.ICAO, apt);
                                    NewAPTList.Add(apt);
                                }
                            });

                            if (!HasAirport)
                            {
                                AddToNewCache(0, FilePath + "\t" + Utils.TimeStamp(File.GetLastWriteTimeUtc(FilePath)) + "\t" + layer.Key + "\t0", NewCache);
                            }
                            else
                            {
                                AddToNewCache(1, FilePath + "\t" + Utils.TimeStamp(File.GetLastWriteTimeUtc(FilePath)) + "\t" + layer.Key + "\t1", NewCache);
                            }

                        }
                        catch// (Exception ex)
                        {
                            //Console.WriteLine("Failed to process: " + path + " " + ex.Message + " / " + ex.HResult);
                            //if (ex.HResult != -2146233088)
                            //{
                            AddToNewCache(-1, FilePath + "\t" + Utils.TimeStamp(File.GetLastWriteTimeUtc(FilePath)) + "\t" + layer.Key + "\t-1", NewCache);
                            //}
                        }
                        App.MW.UpdateSceneryProcess(Sim, (FileAt / (double)FileQty) * 100, "Updating...");
                        FileAt++;

                        //if(FileAt == 2000)
                        //{
                        //    break;
                        //}
                    }
                }
                #endregion

                if (UpdateFiles)
                {
                    #region Analyse Custom Airports
                    App.MW.UpdateSceneryProcess(Sim, -2, "Analysing results...");
                    foreach (KeyValuePair<string, List<Airport>> dir in CustomAirportsDirs)
                    {
                        CustomLevel CustomLevel = CustomLevel.Default;
                        if (dir.Value.Count < 10)
                        {
                            if (Directory.GetFiles(dir.Key, "*.bgl", SearchOption.TopDirectoryOnly).Length > 5)
                            {
                                CustomLevel = CustomLevel.ComplexAirport; // Custom Complex
                            }
                            else
                            {
                                CustomLevel = CustomLevel.CustomAirport; // Custom Basic
                            }
                        }
                        else
                        {
                            CustomLevel = CustomLevel.Bulk; // Bulk airports
                        }

                        foreach (Airport apt in dir.Value)
                        {
                            if (App.MW.IsShuttingDown)
                            {
                                return;
                            }

                            foreach (string excl in Sim.SceneryCorrections)
                            {
                                if (dir.Key.Contains(excl))
                                {
                                    CustomLevel = CustomLevel.Correction; // Elevation corrections etc.
                                }
                            }

                            apt.IsCustom = CustomLevel;
                        }
                    }
                    #endregion

                    #region Add to Lookup
                    foreach (Airport apt in NewAPTList)
                    {
                        if (App.MW.IsShuttingDown)
                        {
                            return;
                        }

                        string entry = apt.ICAO + '\t';
                        entry += apt.Hash + '\t';
                        entry += Convert.ToInt16(apt.IsCustom).ToString() + '\t';
                        entry += Math.Round(apt.Location.Lon).ToString() + '_' + Math.Round(apt.Location.Lat).ToString() + '\t';
                        entry += apt.Location.Lon + "," + apt.Location.Lat + '\t';
                        entry += apt.Elevation.ToString() + '\t';
                        entry += apt.Name + '\t';
                        entry += apt.City + '\t';
                        entry += apt.State + '\t';
                        entry += apt.Country + '\t';
                        entry += apt.Runways.Count.ToString() + '\t';
                        entry += apt.Parkings.Count.ToString() + '\t';
                        entry += apt.TaxiNodes.Count.ToString() + '\t';
                        entry += apt.FileName;

                        LookupList.Add(entry);
                    }
                    #endregion

                    #region Compile everything (Cache file + Geo)
                    App.MW.UpdateSceneryProcess(Sim, -2, "Saving data...");
                    Dictionary<string, List<string>> APTGeo = new Dictionary<string, List<string>>();

                    if (!Directory.Exists(WorkingSimDir))
                    {
                        Directory.CreateDirectory(WorkingSimDir);
                    }

                    foreach (Airport apt in APTDict.Values)
                    {
                        string GeoFile = Math.Round(apt.Location.Lon).ToString() + '_' + Math.Round(apt.Location.Lat).ToString();
                        if (!APTGeo.ContainsKey(GeoFile))
                        {
                            APTGeo.Add(GeoFile, new List<string>());
                        }

                        string aptLine = "";
                        aptLine += apt.ICAO + '\t';
                        aptLine += apt.RunwaysToString() + '\t';
                        aptLine += apt.TaxiParkingsToString() + '\t';
                        aptLine += apt.TaxiwayNodesToString() + '\t';
                        aptLine += apt.TaxiwayPathsToString() + '\t';
                        aptLine += apt.ApronsToString() + '\t';
                        aptLine += apt.Radius.ToString() + '\t';

                        APTGeo[GeoFile].Add(aptLine);
                    }

                    List<string> CacheOut = new List<string>();
                    foreach (KeyValuePair<int, List<string>> state in NewCache)
                    {
                        foreach (string file in state.Value)
                        {
                            CacheOut.Add(file);
                        }
                    }

                    string Time = Utils.TimeStamp(DateTime.UtcNow).ToString();

                    CacheDate = Time;

                    CacheOut.Insert(0, Time);
                    CacheOut.Insert(0, CacheVersion);

                    LookupList.Insert(0, Time);
                    LookupList.Insert(0, CacheVersion);

                    Parallel.ForEach(APTGeo, (region) =>
                    //foreach (KeyValuePair<string, List<string>> region in APTGeo)
                    {
                        App.MW.UpdateSceneryProcess(Sim, -2, "Saving " + region.Key);
                        string fileName = Path.Combine(WorkingSimDir, region.Key + ".txt");
                        File.WriteAllText(fileName, string.Join(Environment.NewLine, region.Value));
                    });

                    File.WriteAllText(CacheFile, string.Join(Environment.NewLine, CacheOut));
                    File.WriteAllText(LookupFile, string.Join(Environment.NewLine, LookupList));
                    #endregion

                    #region Finalize
                    foreach (Airport apt in NewAPTList)
                    {
                        if (apt.IsCustom > 0)
                        {
                            NewCustomAirports.Add(apt);
                        }
                    }
                    #endregion

                    Airports = NewAPTList;
                }

                CreateDerivatives();
                //APTLib.CalculateRelevancy();

                Dump(Airports);

            }, App.ThreadCancel.Token);
            */
        }


        internal void AssignCountries(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Country Assignments");

            Parallel.ForEach(Apts, (newAirport) =>
            {
                Countries.Country c = Countries.GetCountry(newAirport.Location);
                newAirport.Country = c.Code;
                newAirport.CountryName = c.Name;
            });

            Console.WriteLine("Done with Country Assignments");
        }

        internal void AssignRadius(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Radius Assignments");
            
            foreach(Airport Apt in Apts)
            {
                double FarthestNode = 0.25;
                foreach (Airport.Runway rw in Apt.Runways)
                {
                    GeoLoc Rw1 = Utils.MapOffsetPosition(rw.Location.X, rw.Location.Y, ((rw.LengthFT * 0.3048) / 2), rw.Heading);
                    GeoLoc Rw2 = Utils.MapOffsetPosition(rw.Location.X, rw.Location.Y, ((rw.LengthFT * 0.3048) / 2), rw.Heading + 180);

                    double Rw1Dist = Utils.MapCalcDist(Apt.Location, Rw1, Utils.DistanceUnit.NauticalMiles);
                    if (FarthestNode < Rw1Dist)
                    {
                        FarthestNode = Rw1Dist;
                    }

                    double Rw2Dist = Utils.MapCalcDist(Apt.Location, Rw2, Utils.DistanceUnit.NauticalMiles);
                    if (FarthestNode < Rw2Dist)
                    {
                        FarthestNode = Rw2Dist;
                    }
                }

                foreach (Airport.Parking pk in Apt.Parkings)
                {
                    double Pk1Dist = Utils.MapCalcDist(Apt.Location, new GeoLoc(pk.Location.X, pk.Location.Y), Utils.DistanceUnit.NauticalMiles);
                    if (FarthestNode < Pk1Dist)
                    {
                        FarthestNode = Pk1Dist;
                    }
                }

                Apt.Radius = (float)Math.Round(FarthestNode, 5);
            }

            Console.WriteLine("Done with Radius Assignments");
        }

        internal void AssignDensity(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Density Assignments");
            
            Parallel.ForEach(Apts, (Apt) =>
            {
                // Find Distances
                List<KeyValuePair<double, Airport>> Results = new List<KeyValuePair<double, Airport>>();

                Parallel.ForEach(Apts, (Apt1) =>
                {
                    if(Apt1 != Apt)
                    {
                        double aptDist = Utils.MapCalcDist(Apt.Location, Apt1.Location, Utils.DistanceUnit.Kilometers, true);
                        if(aptDist < 4000)
                        {
                            lock (Results)
                            {
                                double DistFactor = Math.Round(Math.Pow((1 / (aptDist / 3000)), 3), 5);
                                Results.Add(new KeyValuePair<double, Airport>(DistFactor, Apt1));
                            }
                        }
                    }
                });

                double Density = 0;
                //Results = Results.OrderByDescending(x => x.Key).ToList();
                foreach (var entry in Results)
                {
                    Density += entry.Key;
                }
                
                //List<Airport> radiused = RouteGenerator.FilterAirportsDistance(Apts, Apt.Location, 0, 1300);
                Apt.Density = (uint)Density;
            });

            //var test = Apts.OrderByDescending(x => x.Density);

            // Density
            Console.WriteLine("Done with Density Assignments");
        }

        internal void AssignFeatures(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Relief Assignments");

            Topo.Startup();

            Func<List<PointElevation>, float, float, double> GetAmp = (List<PointElevation> Pos, float Dist, float Base) =>
            {
                return (Pos.Max(x => x.Elevation - Base) - Pos.Min(x => x.Elevation - Base)) / (Dist * 0.2f);
            };


            int Write = 0;
            int Count = 0;
            List<int> Rads = new List<int>() { 5, 10, 15 }; // 111 KM per unit
            Parallel.ForEach(Apts, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 100,
            },(apt) =>
            {
                apt.Relief = 0;

                // Base Altitude
                int Step = 20;
                TileDataSet tds = Topo.GetNewDataset();
                int BaseAlt = (int)Topo.GetElevation(apt.Location, tds);

                // Get radii
                List<int> Samples = new List<int>();
                double Angle = 0;
                int m = 1;
                foreach (var Rad in Rads)
                {
                    while (Angle < 360)
                    {
                        double y = (1f / 111) * Rad * Math.Cos(2 * Math.PI * Angle / 360);
                        double x = (1f / 111) * Rad * Math.Sin(2 * Math.PI * Angle / 360);
                        Angle += Step;
                
                        GeoLoc OffsetLocation = new GeoLoc(apt.Location.Lon + x, apt.Location.Lat + y);
                        int Elev = (int)(Topo.GetElevation(OffsetLocation, tds) - BaseAlt);
                        Samples.Add((int)Math.Round((float)Elev / m));
                    }
                    Angle = 0;
                    m *= 2;
                }

                //int sampleCount = 10;
                float RunwayLine = 0;

                foreach(var Rwy in apt.Runways)
                {
                    float thr = Rwy.LengthFT * 3.28084f * 0.5f;
                    float thr1 = thr * 1.5f;
                    float thr2 = thr * 2.5f;

                    //List<PointElevation> track = Topo.GetElevationAlongPath(Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr * 10, Rwy.Heading), Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr * 10, Rwy.Heading - 180), sampleCount * 2);
                    //List<double> leveledTrack = new List<double>();
                    //double trackFirst = track.GetRange(0, sampleCount).Average(x => x.Elevation - BaseAlt);
                    //double trackLast = track.GetRange(sampleCount, sampleCount).Average(x => x.Elevation - BaseAlt);
                    //double slope = trackFirst - trackLast;
                    //int prog = 1;
                    //foreach(var pt in track)
                    //{
                    //    double slopepcnt = 1f / track.Count * prog;
                    //    double rebase = pt.Elevation - BaseAlt - trackFirst;
                    //    double leveled = rebase + (slope * slopepcnt);
                    //    leveledTrack.Add(leveled > 0 ? leveled : leveled * 0.2);
                    //    
                    //    prog++;
                    //}
                    //double variance = 0;
                    //foreach(double elev in leveledTrack)
                    //{
                    //    variance += Math.Abs(elev);
                    //}
                    //
                    //RunwayLine += Convert.ToSingle(variance / apt.Runways.Count);
                    GeoLoc PrimRwy = Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr, Rwy.Heading);
                    GeoLoc SecRwy = Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr, Rwy.Heading - 180);

                    float PrimB = (float)Topo.GetElevation(PrimRwy, tds);
                    float PrimL = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 3), 5, tds), thr1, PrimB);
                    float PrimC = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr2, Rwy.Heading + 0), 5, tds), thr2, PrimB);
                    float PrimR = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading + 3), 5, tds), thr1, PrimB);

                    float SecB = (float)Topo.GetElevation(SecRwy, tds);
                    float SecL = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 180 - 3), 5, tds), thr1, SecB);
                    float SecC = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr2, Rwy.Heading - 180 + 0), 5, tds), thr2, SecB);
                    float SecR = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 180 + 3), 5, tds), thr1, SecB);

                    float RwySum = 0;
                    RwySum += Math.Abs(PrimB - SecB);

                    RwySum += Math.Abs(PrimL) / apt.Runways.Count;
                    RwySum += Math.Abs(PrimC) / apt.Runways.Count;
                    RwySum += Math.Abs(PrimR) / apt.Runways.Count;
                    
                    RwySum += Math.Abs(SecL) / apt.Runways.Count;
                    RwySum += Math.Abs(SecC) / apt.Runways.Count;
                    RwySum += Math.Abs(SecR) / apt.Runways.Count;
                    
                    RwySum /= ((float)(Rwy.WidthMeters * 0.3048) + Rwy.LengthFT) * 0.001f;
                    
                    RunwayLine += RwySum;
                    
                }


                // Set on airport
                apt.Relief += (short)Math.Abs(Samples.Average() * 0.6); //(short)Math.Round(Convert.ToInt16(Samples.Max() - Samples.Min()) * 0.01f);
                apt.Relief += Convert.ToInt16(RunwayLine);
                
                Count++;
                Write++;
                if (Write > 1000)
                {
                    Write = 0;
                    Console.WriteLine("Progress: " + ((1f / Apts.Count) * Count) * 100);
                }
            });

            //var test = Apts.OrderByDescending(x => x.Relief).ToList();
            //int v = 0;
            //while(v < 100)
            //{
            //    Console.WriteLine(test[v].ICAO + " - " + test[v].Relief + " - " + test[v].Location.Lat + "," + test[v].Location.Lon);
            //    v++;
            //}

            Console.WriteLine("Done with Relief Assignments");
        }


        internal void ScanLittleNavMap()
        {
#if DEBUG
            string filePath = @"F:\Documents\Clients\Parallel 42\The Skypark\Git\External\Navdatareader\navdata.sqlite";

            File.Copy(filePath, filePath + "_001", true);
            
            //SQLITE_OPEN_FULLMUTEX
            SQLiteConnection sqlite_conn_0 = new SQLiteConnection(@"Data Source=" + filePath + ";");

            try
            {
                sqlite_conn_0.Open();

                #region Indices
                Action SetIndices = () =>
                {
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM sqlite_master where type='index'", sqlite_conn_0);
                    List<string> tables = new List<string>();
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(1));
                    }

                    if(!tables.Contains("taxi_path_idx"))
                    {
                        Console.WriteLine("Creating Index: taxi_path_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX taxi_path_idx ON taxi_path(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("apron_idx"))
                    {
                        Console.WriteLine("Creating Index: apron_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX apron_idx ON apron(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("start_idx"))
                    {
                        Console.WriteLine("Creating Index: start_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX start_idx ON start(start_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("runway_end_idx"))
                    {
                        Console.WriteLine("Creating Index: runway_end_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX runway_end_idx ON runway_end(runway_end_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("runway_idx"))
                    {
                        Console.WriteLine("Creating Index: runway_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX runway_idx ON runway(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("parking_idx"))
                    {
                        Console.WriteLine("Creating Index: parking_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX parking_idx ON parking(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                };
                SetIndices();
                #endregion
                
                #region TaxiwayPaths
                Func<int, Action<List<Airport.TaxiwayPath>, List<Airport.TaxiwayNode>>, Task> GetTaxiways = async (int AptID, Action<List<Airport.TaxiwayPath>, List<Airport.TaxiwayNode>> Callback) =>
                {
                    List<Airport.TaxiwayNode> newNodes = new List<Airport.TaxiwayNode>();
                    List<Airport.TaxiwayPath> newTaxiways = new List<Airport.TaxiwayPath>();

                    string Name = "";
                    double Width = 0;
                    Point Start = new Point();
                    Point End = new Point();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM taxi_path WHERE airport_id=" + AptID, sqlite_conn_0);
                    while (await reader.ReadAsync())
                    {
                        Name = reader.GetString(reader.GetOrdinal("name"));
                        Width = reader.GetDouble(reader.GetOrdinal("width"));
                        Start.X = reader.GetDouble(reader.GetOrdinal("start_lonx"));
                        Start.Y = reader.GetDouble(reader.GetOrdinal("start_laty"));
                        End.X = reader.GetDouble(reader.GetOrdinal("end_lonx"));
                        End.Y = reader.GetDouble(reader.GetOrdinal("end_laty"));

                        Airport.TaxiwayNode existingStart = newNodes.Find(x => x.Location.Lon == Start.X && x.Location.Lat == Start.Y);
                        if(existingStart == null)
                        {
                            existingStart = new Airport.TaxiwayNode(Convert.ToUInt16(newNodes.Count))
                            {
                                Location = new GeoLoc(Start.X, Start.Y)
                            };
                            newNodes.Add(existingStart);
                        }

                        Airport.TaxiwayNode existingEnd = newNodes.Find(x => x.Location.Lon == End.X && x.Location.Lat == End.Y);
                        if (existingEnd == null)
                        {
                            existingEnd = new Airport.TaxiwayNode(Convert.ToUInt16(newNodes.Count))
                            {
                                Location = new GeoLoc(Start.X, Start.Y)
                            };
                            newNodes.Add(existingEnd);
                        }

                        newTaxiways.Add(new Airport.TaxiwayPath()
                        {
                             Name = Name.Replace(',', ' ').Replace(';', ' ').Replace('\t', ' '),
                             Start = existingStart.ID,
                             End = existingEnd.ID,
                             Width = Convert.ToInt32(Width)
                        });
                    }

                    Callback(newTaxiways, newNodes);

                };
                #endregion

                #region Apron
                Func<int, Action<List<Airport.ApronGeometry>>, Task> GetApron = async (int AptID, Action<List<Airport.ApronGeometry>> Callback) =>
                {
                    List<Airport.ApronGeometry> geometryList = new List<Airport.ApronGeometry>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM apron WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.ApronGeometry geometry = new Airport.ApronGeometry();
                        byte[] bytes = (byte[])reader.GetValue(reader.GetOrdinal("vertices"));
                        Array.Reverse(bytes, 0, bytes.Length);

                        int offset = 0;
                        while(offset + 4 < bytes.Length)
                        {
                            var lat = BitConverter.ToSingle(bytes, offset);
                            var lon = BitConverter.ToSingle(bytes, offset + 4);
                            geometry.Geometry.Add(new Point(lat, lon));
                            offset += 8;
                        }

                        geometryList.Add(geometry);
                    }

                    Callback(geometryList);
                };
                #endregion

                #region GetStart
                Func<int, string> GetStart = (StartID) =>
                {
                    string Name = "";
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM start WHERE start_id=" + StartID, sqlite_conn_0);
                    while (reader.Read())
                    {
                        Name = reader.GetString(reader.GetOrdinal("runway_name"));
                    }

                    return Name;
                };
                #endregion

                #region GetRunwayEnd
                Action<int, Action<string, bool, bool>> GetRunwayEnd = (EndID, Callback) =>
                {
                    string Name = "";
                    bool Closed = false;
                    bool ILS = false;
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM runway_end WHERE runway_end_id=" + EndID, sqlite_conn_0);
                    while (reader.Read())
                    {
                        Name = reader.GetString(reader.GetOrdinal("name"));
                        Closed = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("has_closed_markings")));
                        ILS = reader.GetValue(reader.GetOrdinal("ils_ident")).GetType() != typeof(DBNull);
                    }

                    Callback(Name, Closed, ILS);
                };
                #endregion

                #region GetRunways
                Func<int, Action<List<Airport.Runway>>, Task> GetRunways = async (int AptID, Action<List<Airport.Runway>> Callback) =>
                {
                    List<Airport.Runway> newRunways = new List<Airport.Runway>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM runway WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.Runway Runway = new Airport.Runway();

                        string sfc = reader.GetString(reader.GetOrdinal("surface"));
                        Thread NT = new Thread(() =>
                        {
                            switch (sfc)
                            {
                                case "C": { Runway.Surface = Connectors.SimConnection.Surface.Concrete; break; }
                                case "G": { Runway.Surface = Connectors.SimConnection.Surface.Grass; break; }
                                case "W": { Runway.Surface = Connectors.SimConnection.Surface.Water; break; }
                                case "A": { Runway.Surface = Connectors.SimConnection.Surface.Asphalt; break; }
                                case "CE": { Runway.Surface = Connectors.SimConnection.Surface.Concrete; break; }
                                case "CL": { Runway.Surface = Connectors.SimConnection.Surface.Clay; break; }
                                case "SN": { Runway.Surface = Connectors.SimConnection.Surface.Snow; break; }
                                case "I": { Runway.Surface = Connectors.SimConnection.Surface.Ice; break; }
                                case "D": { Runway.Surface = Connectors.SimConnection.Surface.Dirt; break; }
                                case "R": { Runway.Surface = Connectors.SimConnection.Surface.Coral; break; }
                                case "GR": { Runway.Surface = Connectors.SimConnection.Surface.Gravel; break; }
                                case "OT": { Runway.Surface = Connectors.SimConnection.Surface.OilTreated; break; }
                                case "SM": { Runway.Surface = Connectors.SimConnection.Surface.SteelMats; break; }
                                case "B": { Runway.Surface = Connectors.SimConnection.Surface.Bituminous; break; }
                                case "BR": { Runway.Surface = Connectors.SimConnection.Surface.Brick; break; }
                                case "M": { Runway.Surface = Connectors.SimConnection.Surface.Macadam; break; }
                                case "P": { Runway.Surface = Connectors.SimConnection.Surface.Planks; break; }
                                case "S": { Runway.Surface = Connectors.SimConnection.Surface.Sand; break; }
                                case "SH": { Runway.Surface = Connectors.SimConnection.Surface.Shale; break; }
                                case "T": { Runway.Surface = Connectors.SimConnection.Surface.Tarmac; break; }
                                case "TR": { Runway.Surface = Connectors.SimConnection.Surface.Unknown; break; }
                            }
                        });
                        NT.CurrentCulture = CultureInfo.CurrentCulture;
                        NT.Start();
                        
                        GetRunwayEnd(reader.GetInt32(reader.GetOrdinal("primary_end_id")), (Name, Closed, ILS) =>
                        {
                            Runway.PrimaryClosed = Closed;
                            Runway.PrimaryILS = ILS;
                            Runway.Primary = Name;
                        });

                        GetRunwayEnd(reader.GetInt32(reader.GetOrdinal("secondary_end_id")), (Name, Closed, ILS) =>
                        {
                            Runway.SecondaryClosed = Closed;
                            Runway.SecondaryILS = ILS;
                            Runway.Secondary = Name;
                        });

                        if(Runway.SecondaryClosed && Runway.PrimaryClosed)
                        {
                            continue;
                        }
                        
                        Runway.AltitudeFeet = (short)reader.GetDouble(reader.GetOrdinal("altitude"));
                        Runway.Location = new Point(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));

                        Runway.LengthFT = Convert.ToUInt16(reader.GetDouble(reader.GetOrdinal("length")));
                        Runway.WidthMeters = Convert.ToUInt16(reader.GetDouble(reader.GetOrdinal("width")));
                        Runway.Heading = reader.GetDouble(reader.GetOrdinal("heading"));

                        var edge_light = reader.GetValue(reader.GetOrdinal("edge_light"));
                        if(edge_light.GetType() != typeof(DBNull))
                        {
                            switch(edge_light)
                            {
                                case "L":
                                    {
                                        Runway.EdgeLight = 1;
                                        break;
                                    }
                                case "M":
                                    {
                                        Runway.EdgeLight = 2;
                                        break;
                                    }
                                case "H":
                                    {
                                        Runway.EdgeLight = 3;
                                        break;
                                    }
                            }
                        }

                        var center_light = reader.GetValue(reader.GetOrdinal("center_light"));
                        if (center_light.GetType() != typeof(DBNull))
                        {
                            switch (center_light)
                            {
                                case "L":
                                    {
                                        Runway.CenterLight = 1;
                                        break;
                                    }
                                case "M":
                                    {
                                        Runway.CenterLight = 2;
                                        break;
                                    }
                                case "H":
                                    {
                                        Runway.CenterLight = 3;
                                        break;
                                    }
                            }
                        }
                        
                        newRunways.Add(Runway);
                    }

                    Callback(newRunways);
                };
                #endregion

                #region GetParkings
                Func<int, Action<List<Airport.Parking>>, Task> GetParkings = async (int AptID, Action<List<Airport.Parking>> Callback) =>
                {
                    List<Airport.Parking> newParkings = new List<Airport.Parking>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM parking WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.Parking Parking = new Airport.Parking((ushort)reader.GetInt32(reader.GetOrdinal("parking_id")));

                        Parking.Type = Airport.ParkingType.RampGAMedium;
                        Parking.Diameter = (float)reader.GetDouble(reader.GetOrdinal("radius")) * 2;
                        Parking.Number = (ushort)reader.GetInt32(reader.GetOrdinal("number"));
                        Parking.Location = new Point(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));
                        Parking.Heading = (int)Math.Round(reader.GetDouble(reader.GetOrdinal("heading")));

                        newParkings.Add(Parking);
                    }

                    Callback(newParkings);
                };
                #endregion


                #region GetAirports
                Func<Action<List<Airport>>, Task> GetAirports = async (Callback) =>
                {
                    List<Airport> newAirports = new List<Airport>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM airport", sqlite_conn_0);

                    int i1 = 0;
                    foreach (string Name in reader.GetValues())
                    {
                        Console.WriteLine(i1 + ": " + Name);
                        i1++;
                    }

                    int ct = 0;
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            int Index = 0;
                            Airport newAirport = new Airport();

                            Index = reader.GetOrdinal("ident");
                            if (!reader.IsDBNull(Index)) { newAirport.ICAO = reader.GetString(Index); }

                            Index = reader.GetOrdinal("name");
                            if (!reader.IsDBNull(Index)) { newAirport.Name = reader.GetString(Index).Replace("	", " "); }

                            Index = reader.GetOrdinal("city");
                            if (!reader.IsDBNull(Index)) { newAirport.City = reader.GetString(Index); }

                            Index = reader.GetOrdinal("state");
                            if (!reader.IsDBNull(Index)) { newAirport.State = reader.GetString(Index); }

                            newAirport.IsClosed = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("is_closed")));
                            newAirport.IsMilitary = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("is_military")));
                            newAirport.Elevation = reader.GetInt32(reader.GetOrdinal("altitude"));
                            newAirport.Location = new GeoLoc(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));
                        
                            await GetRunways(reader.GetInt32(0), (r) =>
                            {
                                newAirport.Runways = r;
                            });
                            await GetParkings(reader.GetInt32(0), (p) =>
                            {
                                newAirport.Parkings = p;
                            });
                            await GetApron(reader.GetInt32(0), (p) =>
                            {
                                newAirport.Aprons = p;
                            });
                            await GetTaxiways(reader.GetInt32(0), (paths, nodes) =>
                            {
                                newAirport.TaxiPaths = paths;
                                newAirport.TaxiNodes = nodes;
                            });

                            lock (newAirports)
                            {
                                newAirports.Add(newAirport);
                                ct++;
                                if (ct >= 500)
                                {
                                    Console.WriteLine("Apt Count " + newAirports.Count);
                                    ct = 0;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    List<Airport> FoundUnknowns = newAirports.FindAll(x => x.Name == "Unknown Airport");
                    foreach (Airport Apt in FoundUnknowns)
                    {
                        Console.WriteLine("XXXXXXXXX " + Apt.ICAO + " / " + Apt.Name);
                    }

                    AssignCountries(newAirports);
                    AssignRadius(newAirports);
                    AssignDensity(newAirports);
                    AssignFeatures(newAirports);
                    Callback(newAirports);
                };
                #endregion
                
                GetAirports((simAirports) =>
                {
                    var newList = simAirports.FindAll(x => x.ICAO != "GLOB" && x.Runways.Count > 0);
                    Dump(newList);
                    CreateReport(newList);
                    App.MW.Shutdown();
                });
                Thread.Sleep(1000);
                //AssignCountries(simAirports);

                sqlite_conn_0.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed SQL scan: " + ex.Message);
            }
#endif
        }
        
#if DEBUG
        private SQLiteDataReader QuerySQLite(string Query, SQLiteConnection Connection)
        {
            SQLiteCommand sqlite_cmd = Connection.CreateCommand();
            sqlite_cmd.CommandText = Query; // Get all Root tables
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            var schema = sqlite_datareader.GetValues();

            return sqlite_datareader;
        }
#endif

        internal void CreateReport(List<Airport> Apts)
        {
#if DEBUG
            var AptsPrev = SimList[0].AirportsLib.GetAirportsCopy();
            string pat_dump_file = Path.Combine(App.AppDataDirectory, "apt_report.csv");
            if (File.Exists(pat_dump_file))
                File.Delete(pat_dump_file);

            using (StreamWriter sw = File.CreateText(pat_dump_file))
            {
                sw.WriteLine("ICAO, Change");

                List<Airport> proc = new List<Airport>(AptsPrev);
                foreach(Airport apt in Apts)
                {
                    Airport aptPrev = AptsPrev.Find(x => x.ICAO == apt.ICAO);
                    if (aptPrev == null)
                    {
                        sw.WriteLine(apt.ICAO + ",Added");
                        continue;
                    }
                    else
                    {
                        proc.Remove(aptPrev);
                        List<string> change = new List<string>();

                        if (apt.IsMilitary != aptPrev.IsMilitary)
                        {
                            change.Add(apt.IsMilitary ? "IsMilitary" : "NotMilitary");
                        }

                        if (apt.IsClosed != aptPrev.IsClosed)
                        {
                            change.Add(apt.IsMilitary ? "IsClosed" : "NotClosed");
                        }

                        if (apt.Location.ToString() != aptPrev.Location.ToString())
                        {
                            change.Add("Location");
                        }

                        if (apt.Country != aptPrev.Country)
                        {
                            change.Add("Country");
                        }

                        if (apt.City != aptPrev.City)
                        {
                            change.Add("City");
                        }

                        if (apt.Name != aptPrev.Name)
                        {
                            change.Add("Name");
                        }

                        if (apt.TaxiParkingsToString() != aptPrev.TaxiParkingsToString())
                        {
                            change.Add("Parking");
                        }

                        if (apt.RunwaysToString() != aptPrev.RunwaysToString())
                        {
                            change.Add("Runway");
                        }

                        if (change.Count > 0)
                        {
                            sw.WriteLine(apt.ICAO + "," + string.Join(",", change));
                        }
                    }                     
                }

                foreach(Airport apt in proc)
                {
                    sw.WriteLine(apt.ICAO + ",Removed");
                }

            }
            Console.WriteLine("------------------------- APT Report Done");
#endif
        }

        internal void Dump(List<Airport> Apts)
        {
#if DEBUG
            #region Compile Airports for Database
            string pat_dump_file = Path.Combine(App.AppDataDirectory, "apt_dump.csv");
            if (File.Exists(pat_dump_file))
            {
                File.Delete(pat_dump_file);
            }
            using (StreamWriter sw = File.CreateText(pat_dump_file))
            {
                sw.WriteLine("ICAO, Name, Lon, Lat, Elevation, Country, CountryName, Province, City, IsClosed, IsMilitary, Density, Relief, Runways, Taxis");

                int aptDumpCount = 0;
                foreach (Airport apt in Apts)
                {
                    aptDumpCount++;
                    sw.WriteLine(string.Join("\t", new List<string>()
                    {
                        apt.ICAO, // 0
                        "\"" + apt.Name + "\"", // 1
                        apt.Location.Lon.ToString(), // 2
                        apt.Location.Lat.ToString(), // 3
                        apt.Elevation.ToString(), // 4
                        apt.Country, // 5
                        apt.CountryName, // 6
                        apt.State, // 7
                        apt.City, // 8
                        Convert.ToInt32(apt.IsClosed).ToString(), // 9
                        Convert.ToInt32(apt.IsMilitary).ToString(), // 10
                        apt.Density.ToString(), // 11
                        apt.Relief.ToString(), // 12
                        apt.Radius.ToString(), // 13
                        apt.RunwaysToString(), // 14
                        apt.TaxiParkingsToString(), // 15
                    }));
                        
                    /*
                    lock (LiteDbService.DBApt)
                    {
                        var DBCollection = LiteDbService.DBApt.Database.GetCollection("airports");

                        var LastLocationBson = new BsonDocument();
                        LastLocationBson["TaxiwayPaths"] = apt.TaxiwayPathsToString();
                        LastLocationBson["TaxiwayNodes"] = apt.TaxiwayNodesToString();
                        LastLocationBson["Aprons"] = apt.ApronsToString();

                        DBCollection.Upsert(apt.ICAO, LastLocationBson);
                    }
                    */

                    //lock (LiteDbService.DBApt)
                    //{
                    //    var DBCollection = LiteDbService.DBApt.Database.GetCollection("airports");
                    //    DBCollection.EnsureIndex("_id", true);
                    //}
                }
                Console.WriteLine("------------------------- APT Dump Count: " + aptDumpCount);
            }
            #endregion
#endif
        }

        internal static SortedDictionary<int, List<string>> AddFileToLayers(int layer, SortedDictionary<int, List<string>> dict, string path)
        {
            if (!dict.ContainsKey(layer))
            {
                dict.Add(layer, new List<string>());
            }
            dict[layer].Add(path);
            return dict;
        }

        internal static SortedDictionary<int, List<string>> AddFilesToLayers(int layer, SortedDictionary<int, List<string>> dict, string[] paths)
        {
            if (!dict.ContainsKey(layer))
            {
                dict.Add(layer, new List<string>());
            }

            dict[layer].AddRange(paths);
            return dict;
        }

        private Dictionary<int, List<string>> AddToNewCache(int state, string line, Dictionary<int, List<string>> list)
        {
            if (!list.ContainsKey(state))
            {
                list.Add(state, new List<string>());
            }

            list[state].Add(line);
            return list;
        }
        
        private void CreateDerivatives()
        {
            //Dictionary<string, List<Airport>> AirportsByTileTemp = new Dictionary<string, List<Airport>>();
            //foreach (Airport apt in Airports)
            //{
            //    string tile = Math.Floor(apt.Location.X).ToString() + "_" + Math.Floor(apt.Location.Y).ToString();
            //    if (!AirportsByTileTemp.ContainsKey(tile))
            //    {
            //        AirportsByTileTemp.Add(tile, new List<Airport>());
            //    }
            //
            //    AirportsByTileTemp[tile].Add(apt);
            //}

            //APTLib.AirportsByTile = AirportsByTileTemp;
        }
        
        private void LoadCSV()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(App.ThisApp.Location), "airports.csv");
            if (File.Exists(fileName))
            {
                try
                {
                    string[] lines = File.ReadAllLines(fileName);
                    foreach (string line in lines)
                    {
                        string[] lineSplit = line.Split(',');

                        Airport NewAirport = new Airport()
                        {
                            Name = lineSplit[3].Trim('"'),
                            ICAO = lineSplit[1].Trim('"').ToUpper(),
                            City = lineSplit[2].Trim('"'),
                            State = lineSplit[5].Trim('"'),
                            Country = lineSplit[12].Trim('"'),
                            Location = new GeoLoc(Convert.ToDouble(lineSplit[8].Trim('"')), Convert.ToDouble(lineSplit[9].Trim('"'))),
                            IsCustom = 0,
                            Hash = "",
                        };

                        CSVAPTList.Add(NewAirport);
                        CSVAPTDict.Add(NewAirport.ICAO, NewAirport);
                    }
                }
                catch
                {
                    Console.WriteLine("Unable to read airports.csv");
                }
            }
        }
    }
}
