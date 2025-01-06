using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using static TSP_Transponder.Models.FlightPlans.FlightPlan;

namespace TSP_Transponder.Models.FlightPlans.Types
{
    class PlanTypePLN : PlanTypeBase
    {
        public PlanTypePLN()
        {
            Ext = ".pln";
        }
        
        public override FlightPlan ReadFile(string path)
        {
            try
            {
                List<string> FileContent = new List<string>();
                int TryAttempt = 0;
                Func<string, List<string>> TryRead = null;
                TryRead = new Func<string, List<string>>((PlanFilePath) =>
                {
                    if (new System.IO.FileInfo(PlanFilePath).Length > 1000)
                    {
                        try
                        {
                            return System.IO.File.ReadAllLines(PlanFilePath).ToList();
                        }
                        catch
                        {
                            Thread.Sleep(200);
                            if (TryAttempt > 10)
                            {
                                return new List<string>();
                            }
                            else
                            {
                                TryAttempt++;
                                return TryRead(PlanFilePath);
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(200);
                        if (TryAttempt > 10)
                        {
                            return new List<string>();
                        }
                        else
                        {
                            TryAttempt++;
                            return TryRead(PlanFilePath);
                        }
                    }
                });

                List<string> PlanContent = TryRead(path);

                FlightPlan Plan = ReadContent(PlanContent);

                FileInfo FI = new FileInfo(path);
                Plan.File = FI.Name;
                Plan.LastModified = FI.LastWriteTimeUtc;
                Plan.OriginDirectory = Directory.GetParent(path).FullName;
                Plan.CalculateHash();
                
                return Plan;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read Flight Plan for " + path + " - " + ex.Message + Environment.NewLine + ex.StackTrace);
                return null;
            }
        }

        public override FlightPlan ReadContent(List<string> PlanContent)
        {
            try
            {
                FlightPlan Plan = new FlightPlan(Utils.GetNumGUID());
                XmlDocument doc = new XmlDocument();
                string result = string.Join("", PlanContent);
                doc.LoadXml(result);

                XmlNode node = doc.DocumentElement.SelectSingleNode("/SimBase.Document/FlightPlan.FlightPlan");

                if(node != null)
                {
                    int AirwayIndex = 0;
                    string AirwayLast = null;
                    Waypoint AirwayWPLast = null;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        switch (node2.Name.ToLower())
                        {
                            case "title":
                                {
                                    Plan.Name = node2.InnerText.Trim();
                                    break;
                                }
                            case "departurename":
                                {
                                    //Plan.StartAirportName = node2.InnerText.ToUpper().Trim();
                                    break;
                                }
                            case "destinationname":
                                {
                                    //Plan.EndAirportName = node2.InnerText.ToUpper().Trim();
                                    break;
                                }
                            case "atcwaypoint":
                                {
                                    Waypoint Waypoint = new Waypoint();

                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        switch (node3.Name.ToLower())
                                        {
                                            case "atcwaypointtype":
                                                {
                                                    Waypoint.Type = node3.InnerText.ToLower().Trim();
                                                    switch (Waypoint.Type.ToLower())
                                                    {
                                                        case "user":
                                                        case "vor":
                                                        case "ndb":
                                                        case "dme":
                                                        case "intersection":
                                                            {
                                                                Waypoint.Code = node2.Attributes[0].Value;
                                                                break;
                                                            }
                                                        case "airport":
                                                            {
                                                                Waypoint.Apt = SimLibrary.SimList[0].AirportsLib.GetByICAO(node2.Attributes[0].Value);
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                break;
                                                            }
                                                    }
                                                    break;
                                                }
                                            case "worldposition":
                                                {
                                                    List<double> ParsedLocation = Utils.ParseDMS(node3.InnerText, 6);
                                                    Waypoint.Location = new GeoLoc(ParsedLocation[1], ParsedLocation[0]);
                                                    break;
                                                }
                                            case "atcairway":
                                                {
                                                    Waypoint.Airway = node3.InnerText.ToUpper().Trim();
                                                    if (AirwayLast != Waypoint.Airway)
                                                    {
                                                        if (AirwayWPLast != null)
                                                        {
                                                            AirwayWPLast.AirwayLast = true;
                                                        }
                                                        AirwayLast = Waypoint.Airway;
                                                        AirwayIndex++;
                                                        Waypoint.AirwayFirst = true;
                                                    }
                                                    Waypoint.AirwayIndex = AirwayIndex;
                                                    AirwayWPLast = Waypoint;
                                                    break;
                                                }
                                            case "icao":
                                                {
                                                    foreach (XmlNode node4 in node3.ChildNodes)
                                                    {
                                                        switch (node4.Name.ToLower())
                                                        {
                                                            case "icaoident":
                                                                {
                                                                    Waypoint.Code = node4.InnerText.ToUpper().Trim();
                                                                    break;
                                                                }
                                                        }
                                                    }
                                                    break;
                                                }
                                        }
                                    }

                                    Plan.AddWaypoint(Waypoint);
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    throw new Exception("Flight plan file is not valid");
                }
                Plan.CalculateDistances();
                return Plan;
            }
            catch
            {
                throw new Exception("Failed to load flightplan");
            }
        }

        public override List<FlightPlan> ReadDirectory(string path)
        {
            try
            {
                List<FlightPlan> Plans = new List<FlightPlan>();
                string[] PLNFilePaths = Directory.GetFiles(path, "*.pln", SearchOption.TopDirectoryOnly);

                foreach (string PLNFilePath in PLNFilePaths)
                {
                    FlightPlan Plan = ReadFile(PLNFilePath);
                    if(Plan != null)
                    {
                        Plans.Add(Plan);
                    }
                }

                return Plans;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read Flight Plans in dir " + path + " - " + ex.Message + Environment.NewLine + ex.StackTrace);
                return null;
            }
        }
    }
}
