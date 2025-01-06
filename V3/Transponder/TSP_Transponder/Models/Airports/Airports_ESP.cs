using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TSP_Transponder.Models.Airports;
using static TSP_Transponder.Models.SimLibrary;

namespace TSP_Transponder.AddonsDetector
{
    class Airports_ESP
    {
        public static int Count = 0;
    
        // Method that retrieve BGL path from scenery.cfg in List<string>
        public static SortedDictionary<int, List<string>> GatherBGLPaths(Simulator sim)
        {
            Dictionary<string, int> PathLayerMatch = new Dictionary<string, int>();
            SortedDictionary<int, List<string>> BGLList = new SortedDictionary<int, List<string>>();

            if (sim.NameStandard == "msfs2020")
            {

                // Retreive all APX*.bgl files
                string[] FolderBGLList = Directory.GetFiles(@"B:\FS\MSFS\Official\OneStore\fs-base\scenery", "*.bgl", SearchOption.AllDirectories);

                Count += FolderBGLList.Count();

                // Save to bglPathList
                BGLList = AirportsScan.AddFilesToLayers(0, BGLList, FolderBGLList);
                //B:\FS\MSFS\Official\OneStore\fs-base\scenery
            }
            else
            {
                #region Check if file exist
                /*
                if (!File.Exists(CleanFileName(sim.P3D_SceneryCFGPath)))
                {
                    return BGLList;
                }
                */
                #endregion

                #region Scenery.cfg
                /*
                // Read the scenery.cfg file
                string[] sceneryFile = File.ReadAllLines(CleanFileName(sim.P3D_SceneryCFGPath), GetEncoding(CleanFileName(sim.P3D_SceneryCFGPath)));
                List<string> RawSceneryDirectories = new List<string>();
                SortedDictionary<int, List<string>> sceneryDirectories = new SortedDictionary<int, List<string>>();

                // Scan scenery.cfg for paths
                for (int lineNumber = 0; lineNumber < sceneryFile.Count(); lineNumber++)
                {
                    // Check if entering a new Area line
                    if (Regex.IsMatch(sceneryFile[lineNumber].ToLower(), @"\[area.\d+\]"))
                    {
                        #region Process the scenery.cfg Areas
                        string currentAreaSceneryDirectory = "";
                        string currentAreaTitle;
                        bool currentAreaActive = false;
                        int layer = -1;

                        // Process the current area
                        for (int areaLineNumber = lineNumber + 1; areaLineNumber < sceneryFile.Count(); areaLineNumber++)
                        {
                            // Check for the Title line
                            if (Regex.IsMatch(sceneryFile[areaLineNumber].ToLower(), @"title(\s|\=)"))
                            {
                                // Retreive the title
                                currentAreaTitle = sceneryFile[areaLineNumber].Split('=')[1];
                            }
                            // Check for the Local line
                            else if (Regex.IsMatch(sceneryFile[areaLineNumber].ToLower(), @"local(\s|\=)"))
                            {
                                // Retreive the scenery directory path
                                currentAreaSceneryDirectory = CleanFileName(sceneryFile[areaLineNumber].Split('=')[1]);
                            }
                            // Check for the Active line
                            else if (Regex.IsMatch(sceneryFile[areaLineNumber].ToLower(), @"active(\s|\=)"))
                            {
                                // Retreive the boolean value
                                if (sceneryFile[areaLineNumber].Split('=')[1].ToLower() == "true")
                                {
                                    currentAreaActive = true;
                                }
                            }
                            // Check for the Active line
                            else if (Regex.IsMatch(sceneryFile[areaLineNumber].ToLower(), @"layer(\s|\=)"))
                            {
                                int inLayer = -1;
                                Int32.TryParse(sceneryFile[areaLineNumber].Split('=')[1], out inLayer);
                                layer = inLayer;
                            }
                            // Check if we exited the area or if end of file
                            else if (Regex.IsMatch(sceneryFile[areaLineNumber].ToLower(), @"\[area.\d+\]") || areaLineNumber == sceneryFile.Count() - 1)
                            {
                                lineNumber = areaLineNumber - 1; // Reset the next area line

                                if (currentAreaActive && currentAreaSceneryDirectory != "")
                                {
                                    // Check if path is absolute
                                    //Console.WriteLine("Checking scenery cfg path " + currentAreaSceneryDirectory);
                                    try
                                    {
                                        if (!Path.IsPathRooted(currentAreaSceneryDirectory))
                                        {
                                            currentAreaSceneryDirectory = Path.Combine(sim.InstallDirectory, currentAreaSceneryDirectory);
                                        }

                                        // Add the current area to sceneryDirectories list
                                        AirportsScan.AddFileToLayers(layer, sceneryDirectories, currentAreaSceneryDirectory);
                                        RawSceneryDirectories.Add(currentAreaSceneryDirectory);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Error while processing scenery cfg path " + currentAreaSceneryDirectory);
                                    }

                                }
                                break; // Exit the for loop
                            }
                        }
                        #endregion
                    }
                }
                */
                #endregion

                #region Add-ons.cfg
                /*
                foreach (string addon in sim.P3D_Addons)
                {
                    if (File.Exists(addon))
                    {
                        Console.WriteLine("Getting " + addon);

                        #region Retreive the Addons path from Add-ons.CFG
                        string[] addonsCFG = File.ReadAllLines(addon);
                        string pathExtract = "";
                        bool activeExtract = false;
                        bool hasEnteredSection = false;
                        int i = 0;
                        while (i <= addonsCFG.Length)
                        {
                            string line = "";

                            if (i < addonsCFG.Length)
                            {
                                line = addonsCFG[i];
                            }

                            // Check if the line contain package
                            if (Regex.IsMatch(line.ToLower(), @"(^|\s)\[package") || i == addonsCFG.Length)
                            {
                                #region Process previous segment
                                if (hasEnteredSection && activeExtract)
                                {
                                    // If path exist, find the add-on.xml file and process it
                                    if (Directory.Exists(pathExtract) && !RawSceneryDirectories.Contains(pathExtract))
                                    {

                                        if (File.Exists(pathExtract + @"\add-on.xml"))
                                        {
                                            XmlReaderSettings preferences = new XmlReaderSettings
                                            {
                                                DtdProcessing = DtdProcessing.Parse
                                            };
                                            XmlReader addonsXML = XmlReader.Create(pathExtract + @"\add-on.xml", preferences);

                                            string inEntry = "";
                                            string category = "";
                                            string path = "";
                                            int layer = -1;
                                            bool InCategory = false;
                                            while (addonsXML.Read())
                                            {
                                                switch (addonsXML.NodeType)
                                                {
                                                    case XmlNodeType.Element:
                                                        {
                                                            string name = addonsXML.Name.ToLower();
                                                            if (InCategory)
                                                            {
                                                                inEntry = name;
                                                            }
                                                            else if (name == "addon.component")
                                                            {
                                                                InCategory = true;
                                                            }
                                                            break;
                                                        }
                                                    case XmlNodeType.Text:
                                                        {
                                                            switch (inEntry)
                                                            {
                                                                case "category":
                                                                    {
                                                                        category = addonsXML.Value.ToLower();
                                                                        break;
                                                                    }
                                                                case "path":
                                                                    {
                                                                        path = addonsXML.Value;
                                                                        break;
                                                                    }
                                                                case "layer":
                                                                    {
                                                                        layer = Convert.ToInt32(addonsXML.Value);
                                                                        break;
                                                                    }
                                                            }
                                                            inEntry = "";
                                                            break;
                                                        }
                                                    case XmlNodeType.EndElement:
                                                        {
                                                            if (InCategory)
                                                            {
                                                                if (addonsXML.Name.ToLower() == "addon.component")
                                                                {
                                                                    if (category != "scenery")
                                                                    {
                                                                        path = string.Empty;
                                                                    }

                                                                    if (path != string.Empty)
                                                                    {
                                                                        try
                                                                        {
                                                                            if (!Path.IsPathRooted(path))
                                                                            {
                                                                                string testPath = Path.Combine(pathExtract, CleanFileName(path.Trim('.')));
                                                                                if (Directory.Exists(testPath))
                                                                                {
                                                                                    AirportsScan.AddFileToLayers(layer, sceneryDirectories, testPath);
                                                                                    RawSceneryDirectories.Add(testPath);
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                if (Directory.Exists(CleanFileName(path)))
                                                                                {
                                                                                    AirportsScan.AddFileToLayers(layer, sceneryDirectories, CleanFileName(path));
                                                                                    RawSceneryDirectories.Add(CleanFileName(path));
                                                                                }
                                                                            }
                                                                        }
                                                                        catch
                                                                        {
                                                                            Console.WriteLine("Error while processing add-ons cfg path " + addonsXML.Value);
                                                                        }
                                                                    }

                                                                    category = "";
                                                                    path = "";
                                                                    layer = -1;
                                                                    InCategory = false;
                                                                }
                                                            }
                                                            break;
                                                        }
                                                }
                                            }

                                            addonsXML.Dispose();

                                        }

                                        // Add the path to the SimObjectPath Array
                                        //sceneryDirectories.Add(path);
                                    }

                                }
                                #endregion

                                pathExtract = "";
                                activeExtract = false;
                                hasEnteredSection = true;
                            }
                            else
                            {
                                #region Get Path in section
                                // Check if the line contain path
                                if (Regex.IsMatch(line.ToLower(), @"(^|\s)path(\s|=)"))
                                {
                                    // Retreive the path (after the =)
                                    string path = CleanFileName(line.Split('=')[1]);

                                    if (path.Contains('"'))
                                    {
                                        // Remove any " from path
                                        path = path.Replace("\"", "");
                                    }

                                    // Check if path is absolute
                                    if (!Path.IsPathRooted(path))
                                    {
                                        path = Path.Combine(sim.InstallDirectory, path);
                                    }

                                    pathExtract = path;
                                }
                                #endregion

                                #region Get Active in section
                                // Check if the line contain path
                                if (Regex.IsMatch(line.ToLower(), @"(^|\s)active(\s|=)"))
                                {
                                    // Retreive the path (after the =)
                                    string IsActiveStr = CleanFileName(line.Split('=')[1]);

                                    if (IsActiveStr.Contains('"'))
                                    {
                                        // Remove any " from path
                                        IsActiveStr = IsActiveStr.Replace("\"", "");
                                    }

                                    bool IsActive = false;
                                    bool.TryParse(IsActiveStr.ToLower(), out IsActive);

                                    activeExtract = IsActive;
                                }
                                #endregion

                                #region Get Layer in section
                                // Check if the line contain path
                                if (Regex.IsMatch(line.ToLower(), @"(^|\s)layer(\s|=)"))
                                {
                                    // Retreive the path (after the =)
                                    string LayerStr = line.Split('=')[1];

                                    if (LayerStr.Contains('"'))
                                    {
                                        // Remove any " from path
                                        LayerStr = LayerStr.Replace("\"", "");
                                    }

                                    int IsActive = 0;
                                    Int32.TryParse(LayerStr, out IsActive);

                                    Console.WriteLine("Add-on Layer " + IsActive);
                                }
                                #endregion
                            }
                            i++;
                        }
                        #endregion
                    }
                }
                */
                #endregion

                #region Console Paths
                //foreach (var path in sceneryDirectories)
                //{
                //Console.WriteLine(path);
                //}
                #endregion

                #region Retreive BGLs files path
                /*
                foreach (KeyValuePair<int, List<string>> sceneryDirLayer in sceneryDirectories)
                {
                    int layer = sceneryDirLayer.Key;
                    foreach (string sceneryDir in sceneryDirLayer.Value)
                    {
                        try
                        {
                            // Check if not in exclusion
                            bool isExcluded = false;
                            foreach (string exluc in sim.SceneryExclusions)
                            {
                                if (sceneryDir.ToLower().Contains(exluc.ToLower()))
                                {
                                    isExcluded = true;
                                }
                            }

                            if (isExcluded)
                            {
                                continue;
                            }

                            //if (!sceneryDir.ToLower().Contains("montreal"))
                            //{
                            //    continue;
                            //}

                            // Check if directory exist
                            if (!Directory.Exists(sceneryDir))
                            {
                                Console.WriteLine("Scenery directory does not exist: " + sceneryDir);
                                continue;
                            }


                            // Retreive all APX*.bgl files
                            string[] FolderBGLList = Directory.GetFiles(sceneryDir, "*.bgl", SearchOption.AllDirectories);

                            Count += FolderBGLList.Count();

                            // Save to bglPathList
                            BGLList = AirportsScan.AddFilesToLayers(layer, BGLList, FolderBGLList);

                            // Check for duplicates
                            foreach (string bgl in FolderBGLList)
                            {
                                if (PathLayerMatch.ContainsKey(bgl))
                                {
                                    BGLList[PathLayerMatch[bgl]].Remove(bgl);
                                    PathLayerMatch[bgl] = layer;
                                }
                                else
                                {
                                    PathLayerMatch.Add(bgl, layer);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Unable to scan folder for BGL: " + ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
                */
                #endregion

                #region Sort
                BGLList = new SortedDictionary<int, List<string>>(BGLList.OrderBy((kvp) => {
                    if (kvp.Key == -1)
                    {
                        return 9999999;
                    }
                    else
                    {
                        return kvp.Key;
                    }
                }).ToDictionary(pair => pair.Key, pair => pair.Value));
                #endregion

                #region Check Duplicates
                int i2 = 0;
                foreach (KeyValuePair<int, List<string>> layer in BGLList)
                {
                    int i1 = 0;
                    foreach (KeyValuePair<int, List<string>> layer1 in BGLList)
                    {
                        if (i1 > i2)
                        {
                            foreach (string path in layer1.Value)
                            {
                                if (layer.Value.Contains(path))
                                {
                                    BGLList[layer.Key].Remove(path);
                                }
                            }
                        }
                        i1++;
                    }
                    i2++;
                }
                #endregion
            }

            return BGLList;
        }
        
    }
}
