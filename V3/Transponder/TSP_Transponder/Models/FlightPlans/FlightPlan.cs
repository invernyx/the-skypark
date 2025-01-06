﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Airports.AirportsLib;

namespace TSP_Transponder.Models.FlightPlans
{
    
    public class FlightPlan
    {
        [ClassSerializerField("id")]
        public int UID = 0;
        [ClassSerializerField("hash")]
        public string Hash = "";
        [ClassSerializerField("name")]
        public string Name = "";
        [ClassSerializerField("file")]
        public string File = "";
        [ClassSerializerField("origin_directory")]
        public string OriginDirectory = "";
        [ClassSerializerField("distance")]
        public double Distance = 0;

        public DateTime LastModified = DateTime.Now;
        public List<Waypoint> Waypoints = new List<Waypoint>();

        public FlightPlan(int UID)
        {
            this.UID = UID;
        }

        public void CalculateHash()
        {
            if(File != string.Empty)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(Path.Combine(OriginDirectory, File)))
                    {
                        Hash = Encoding.Default.GetString(md5.ComputeHash(stream));
                    }
                }
            }
        }

        public void Delete()
        {
            lock (Plans.PlansList)
            {
                Plans.PlansList.Remove(this);
            }
        }

        public void CalculateDistances()
        {
            Waypoint PreviousWP =null;
            foreach (Waypoint Waypoint in Waypoints)
            {
                if (PreviousWP != null)
                {
                    PreviousWP.DistToNext = (float)Utils.MapCalcDist(PreviousWP.Location, Waypoint.Location, Utils.DistanceUnit.Kilometers, true);
                    Distance += PreviousWP.DistToNext;
                }
                PreviousWP = Waypoint;
            }
        }

        public void AddWaypoint(Waypoint wp)
        {
            Waypoints.Add(wp);
        }

        public bool Export(string Path, ExportType Type)
        {
            try
            {
                FileInfo FI = new FileInfo(Path);
                if (!FI.Directory.Exists)
                {
                    Directory.CreateDirectory(FI.Directory.FullName);
                }

                switch (Type)
                {
                    case ExportType.PLN:
                        {
                            string Template = App.ReadResourceFile("TSP_Transponder.Models.FlightPlans.WriteTemplates.PLN.txt");

                            Template = Template.Replace("%TITLE%", Name);
                            Template = Template.Replace("%FLIGHTRULE%", "VFR");
                            Template = Template.Replace("%ROUTETYPE%", "VOR");
                            Template = Template.Replace("%ALTITUDE%", "10000");

                            Template = Template.Replace("%DEPARTUREICAO%", Waypoints[0].Apt == null ? "----" : Waypoints[0].Apt.ICAO);
                            Template = Template.Replace("%ARRIVALICAO%", Waypoints[Waypoints.Count - 1].Apt == null ? "----" : Waypoints[Waypoints.Count - 1].Apt.ICAO);

                            Template = Template.Replace("%DEPARTURELLA%", SexagesimalAngle.FromDouble(Waypoints[0].Location.Lat).ToString("NS") + "," + SexagesimalAngle.FromDouble(Waypoints[0].Location.Lon).ToString("WE") + "," + "+000000.00");
                            Template = Template.Replace("%ARRIVALLLA%", SexagesimalAngle.FromDouble(Waypoints[Waypoints.Count - 1].Location.Lat).ToString("NS") + "," + SexagesimalAngle.FromDouble(Waypoints[Waypoints.Count - 1].Location.Lon).ToString("WE") + "," + "+000000.00");

                            Template = Template.Replace("%DESCRIPTION%", "Generated by The Skypark");

                            Template = Template.Replace("%DEPARTURENAME%", Waypoints[0].Apt == null ? "----" : Waypoints[0].Apt.Name);
                            Template = Template.Replace("%ARRIVALNAME%", Waypoints[0].Apt == null ? "----" : Waypoints[0].Apt.Name);

                            string WPs = "";
                            foreach (Waypoint WP in Waypoints)
                            {
                                WPs += "<ATCWaypoint id=\"" + WP.Code + "\">" + Environment.NewLine;
                                WPs += "<ATCWaypointType>" + WP.Type + "</ATCWaypointType>" + Environment.NewLine;
                                WPs += "<WorldPosition>" + SexagesimalAngle.FromDouble(WP.Location.Lat).ToString("NS") + "," + SexagesimalAngle.FromDouble(WP.Location.Lon).ToString("WE") + "," + "+000000.00" + "</WorldPosition>" + Environment.NewLine;

                                WPs += "<ICAO>" + Environment.NewLine;
                                WPs += "<ICAOIdent>" + WP.Code + "</ICAOIdent>" + Environment.NewLine;
                                WPs += "</ICAO>" + Environment.NewLine;

                                WPs += "</ATCWaypoint>" + Environment.NewLine;
                            }
                            Template = Template.Replace("%WAYPOINTS%", WPs);

                            using (StreamWriter writer = new StreamWriter(Path, false, new UTF8Encoding(true)))
                            {
                                writer.WriteLine(Template);
                            }

                            /*
                            <?xml version="1.0" encoding="UTF-8"?>
                                <SimBase.Document Type="AceXML" version="0,0">
                                <Descr>AceXML Document</Descr>
                                <FlightPlan.FlightPlan>
                                    --<Title>%TITLE%</Title>
                                    --<FPType>%FLIGHTRULE%</FPType>
                                    --<RouteType>%ROUTETYPE%</RouteType>
                                    --<CruisingAlt>%ALTITUDE%</CruisingAlt>
                                    --<DepartureID>%DEPARTUREICAO%</DepartureID>
                                    --<DepartureLLA>%DEPARTURELLA%</DepartureLLA>
                                    --<DestinationID>%ARRIVALICAO%</DestinationID>
                                    --<DestinationLLA>%ARRIVALLLA%</DestinationLLA>
                                    --<Descr>%DESCRIPTION%</Descr>
                                    --<DeparturePosition>0</DeparturePosition>
                                    --<DepartureName>%DEPARTURENAME%</DepartureName>
                                    --<DestinationName>%ARRIVALNAME%</DestinationName>
                                    <AppVersion>
                                        <AppVersionMajor>0</AppVersionMajor>
                                        <AppVersionMinor>0</AppVersionMinor>
                                        <AppVersionRevision>0</AppVersionRevision>
                                        <AppVersionBuild>0</AppVersionBuild>
                                    </AppVersion>
                                    %WAYPOINTS%
                                </FlightPlan.FlightPlan>
                                </SimBase.Document>
                            */

                            break;
                        }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create directory for plan " + Path + " because " + ex.Message);
                return false;
            }
        }

        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<FlightPlan> cs = new ClassSerializer<FlightPlan>(this, fields);
            cs.Generate(typeof(FlightPlan), fields);
            
            cs.Get("modified_on", fields, (f) => LastModified.ToString("O"));
            cs.Get("waypoints", fields, (f) => Waypoints.Select(x => x.Serialize(f)));
            
            var result = cs.Get();
            return result;
        }

        public override string ToString()
        {
            return Name + " / " + Distance;
        }

        
        public class Waypoint
        {
            [ClassSerializerField("type")]
            public string Type = "";
            [ClassSerializerField("airway")]
            public string Airway = "";
            [ClassSerializerField("dist_to_next")]
            public float DistToNext = 0;
            [ClassSerializerField("airway_index")]
            public int AirwayIndex = -1;
            [ClassSerializerField("airway_first")]
            public bool AirwayFirst = false;
            [ClassSerializerField("airway_last")]
            public bool AirwayLast = false;
            [ClassSerializerField("code")]
            public string Code = "";
            public GeoLoc Location = null;
            public Airport Apt = null;

            public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
            {
                ClassSerializer<Waypoint> cs = new ClassSerializer<Waypoint>(this, fields);
                cs.Generate(typeof(Waypoint), fields);

                cs.Get("location", fields, (f) => Location.ToList(5));
                cs.Get("airport", fields, (f) => Apt != null ? Apt.Serialize(
                        new Dictionary<string, dynamic>()
                        {
                            { "icao", true },
                            { "name", true },
                            { "country_name", true },
                            { "country", true },
                            { "elevation", true },
                        }
                    ) : null);
                
                var result = cs.Get();
                return result;
            }

            public override string ToString()
            {
                if(Apt != null)
                {
                    return Type + " / " + Apt.ICAO;
                }
                else
                {
                    return Type + " / " + Code;
                }
            }
        }

        public enum ExportType
        {
            PLN
        }

    }
}