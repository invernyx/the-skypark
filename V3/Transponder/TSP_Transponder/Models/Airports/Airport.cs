using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using TSP_Transponder.Models.Weather;
using TSP_Transponder.Models.WeatherModel;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.Airports.AirportsLib;
using TSP_Transponder.Attributes;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Airports
{
    public class Airport
    {
        public int Line = 0;
        [ClassSerializerField("name")]
        public string Name = "Unknown Airport";
        [ClassSerializerField("icao")]
        public string ICAO = "????";
        [ClassSerializerField("city")]
        public string City = "";
        [ClassSerializerField("state")]
        public string State = "";
        [ClassSerializerField("country")]
        public string Country = "";
        [ClassSerializerField("country_name")]
        public string CountryName = "";
        public GeoLoc Location = new GeoLoc(0, 0);
        public float Elevation = 0;
        public float Radius = 1;
        public uint Density = 0;
        public short Relief = 0;
        public bool IsClosed = false;
        public bool IsMilitary = false;
        public string FileName = "";
        public DateTime? LastWelcome = null;
        public CustomLevel IsCustom = CustomLevel.Default;
        public string Hash = "";
        public bool? HasWx = null;
        public WeatherData Wx = null;
        public List<Runway> Runways = new List<Runway>();
        public List<Parking> Parkings = new List<Parking>();
        public List<TaxiwayNode> TaxiNodes = new List<TaxiwayNode>();
        public List<TaxiwayPath> TaxiPaths = new List<TaxiwayPath>();
        public List<ApronGeometry> Aprons = new List<ApronGeometry>();

        public string RunwaysToString()
        {
            string output = "";
            foreach (Runway rw in Runways)
            {
                string rw_out = rw.ToString();
                output += rw_out.TrimEnd(',') + ';';
            }
            return output.TrimEnd(';');
        }
        public string TaxiParkingsToString()
        {
            string output = "";
            foreach (Parking pw in Parkings)
            {
                string pw_out = pw.ToString();
                output += pw_out.TrimEnd(',') + ';';
            }
            return output.TrimEnd(';');
        }
        public string TaxiwayPathsToString()
        {
            List<string> output = new List<string>();
            foreach (TaxiwayPath tp in TaxiPaths)
            {
                string tp_out = tp.ToString();
                output.Add(tp_out.TrimEnd(','));
            }
            return string.Join(";", output);
        }
        public string TaxiwayNodesToString()
        {
            List<string> output = new List<string>();
            foreach (TaxiwayNode tn in TaxiNodes)
            {
                string tn_out = tn.ToString();
                output.Add(tn_out.TrimEnd(','));
            }
            return string.Join(";", output);
        }
        public string ApronsToString()
        {
            string output = "";
            foreach (ApronGeometry ap in Aprons)
            {
                string ap_out = ap.ToString();
                output += ap_out.TrimEnd(',') + ';';
            }
            return output.TrimEnd(';');
        }

        internal string GetTile()
        {
            return Math.Floor(Location.Lon) + "_" + Math.Floor(Location.Lat);
        }

        //internal void CalculateRelevancy()
        //{
        //    const double YearInSeconds = 3.154e+7 * 5;
        //    DateTime CreateDate = System.IO.File.GetCreationTimeUtc(File);
        //    TimeSpan test = DateTime.UtcNow - CreateDate;
        //    Relevancy = ((YearInSeconds - test.TotalSeconds)) / YearInSeconds;
        //}

        public Dictionary<double, TaxiwayNode> GetNodesByDistance(GeoLoc Loc)
        {
            Dictionary<double, TaxiwayNode> Sorted = new Dictionary<double, TaxiwayNode>();

            foreach (TaxiwayNode Node in TaxiNodes)
            {
                Sorted.Add(Utils.MapCalcDist(Node.Location, Loc, Utils.DistanceUnit.Meters), Node);
            }

            return Sorted.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public override string ToString()
        {
            return ICAO;// + " - " + Name;
        }

        public WeatherData GetWx()
        {
            if(HasWx == null)
            {
                Wx = AviationWeather_gov.GetWeatherStation(ICAO, Location);
                HasWx = Wx != null;
            }
            
            if(Wx != null)
            {
                if (Wx.Time < DateTime.UtcNow.AddMinutes(-15))
                {
                    Wx = AviationWeather_gov.GetWeatherStation(ICAO, Location);
                    HasWx = Wx != null;
                }
            }

            return Wx;
        }
        
        public Dictionary<string, dynamic> ToLocation()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
                {
                    { "name", Name },
                    { "icao", ICAO },
                    { "location", new List<double>() { Location.Lon, Location.Lat } },
                };

            return rt;
        }
        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            if(fields == null)
            {
                fields = new Dictionary<string, dynamic>()
                {
                    { "icao", true },
                    { "name", true },
                    { "country_name", true },
                    { "country", true },
                };
            }

            ClassSerializer<Airport> cs = new ClassSerializer<Airport>(this, fields);
            cs.Generate(typeof(Airport), fields);
            
            cs.Get("location", fields, (f) => new List<double>() { Location.Lon, Location.Lat });
            cs.Get("elevation", fields, (f) => Elevation * 0.3048);
            cs.Get("runways", fields, (f) => Runways.Select(x => x.Serialize(f)));
            cs.Get("parkings", fields, (f) => Parkings.Select(x => x.Serialize(f)));
            cs.Get("radius", fields, (f) => Radius * 1.852);

            cs.Get("wx", fields, (f) =>
            {
                GetWx();
                return Wx != null ? Wx.Serialize(f) : null;
            });

            var result = cs.Get();
            return result;
        }


        public void GenerateImage()
        {
#if DEBUG
            lock (AirportRenderQueue)
            {
                AirportRenderQueue.Add(this);
            }

            if (AirportRenderThread == null)
            {
                AirportRenderThread = new Thread(() =>
                {
                    Airport Target = null;
                    while (AirportRenderQueue.Count > 0)
                    {
                        Target = AirportRenderQueue[0];
                        lock (AirportRenderQueue)
                        {
                            AirportRenderQueue.RemoveAt(0);
                        }

                        string ImagesDir = Path.Combine(App.AppDataDirectory, "AirportImages");
                        if (!Directory.Exists(ImagesDir))
                        {
                            Directory.CreateDirectory(ImagesDir);
                        }

                        string ImageFile = Path.Combine(ImagesDir, Target.ICAO + ".png");
                        if (!File.Exists(ImageFile))
                        {

                            GeoLoc NE = Utils.MapOffsetPosition(Target.Location, Target.Radius * 1852, 45);
                            GeoLoc SW = Utils.MapOffsetPosition(Target.Location, Target.Radius * 1852, 225);

                            double z = Utils.GetBoundsZoomLevel(NE, SW, 600, 600);

                            switch (Target.ICAO)
                            {
                                default:
                                    {
                                        try
                                        {
                                            string url = "https://api.mapbox.com/styles/v1/mapbox/satellite-v9/static/" + Target.Location.Lon + "," + Target.Location.Lat + "," + z + ",0/800x600@2x?logo=false&attribution=false&access_token=pk.eyJ1IjoicGFyYWxsZWw0MiIsImEiOiJja2k2azJ1bWYwN3J6MnFqdGluNzY5aXA4In0.pSoIpamvzXKkZm2YSdUc7g";
                                            using (WebClient webClient = new WebClient())
                                            {
                                                webClient.DownloadFile(url, ImageFile);
                                            }
                                            Thread.Sleep(10);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            Thread.Sleep(10000);
                                        }
                                        break;
                                    }
                            }

                        }
                    }

                });
                AirportRenderThread.Start();
            }
#endif
        }

        public class Runway
        {
            [ClassSerializerField("heading")]
            public double Heading = 0;
            public int Identifier = 0;
            public short AltitudeFeet = 0;
            public ushort LengthFT = 0;
            public ushort WidthMeters = 0;
            public ushort EdgeLight = 0;
            public ushort CenterLight = 0;
            public Point Location = new Point(0, 0);
            public Surface Surface = Surface.Unknown;
            public bool PrimaryClosed = false;
            [ClassSerializerField("primary_ils")]
            public bool PrimaryILS = false;
            [ClassSerializerField("primary_name")]
            public string Primary = "";
            public bool SecondaryClosed = false;
            [ClassSerializerField("secondary_ils")]
            public bool SecondaryILS = false;
            [ClassSerializerField("secondary_name")]
            public string Secondary = "";

            public Runway()
            {
            }

            public Runway(string encoded)
            {
                int i = 0;
                string[] data = encoded.Split(',');
                int.TryParse(data[i++], out Identifier);
                double lX = 0;
                double lY = 0;
                double.TryParse(data[i++], out lX);
                double.TryParse(data[i++], out lY);
                short.TryParse(data[i++], out AltitudeFeet);
                double.TryParse(data[i++], out Heading);
                ushort.TryParse(data[i++], out LengthFT);
                ushort.TryParse(data[i++], out WidthMeters);
                Location = new Point(lX, lY);
                Primary = data[i++];
                Secondary = data[i++];
                bool.TryParse(data[i++], out PrimaryClosed);
                bool.TryParse(data[i++], out SecondaryClosed);
                PrimaryILS = data[i++] == "0" ? false : true;
                SecondaryILS = data[i++] == "0" ? false : true;
                int sfc = 0;
                int.TryParse(data[i++], out sfc);
                Surface = (Surface)sfc;
                ushort.TryParse(data[i++], out EdgeLight);
                ushort.TryParse(data[i++], out CenterLight);
            }

            public override string ToString()
            {
                List<string> l = new List<string>()
                {
                    Identifier.ToString(),
                    Math.Round(Location.X, 6).ToString(),
                    Math.Round(Location.Y, 6).ToString(),
                    AltitudeFeet.ToString(),
                    Heading.ToString(),
                    LengthFT.ToString(),
                    WidthMeters.ToString(),
                    Primary.ToString(),
                    Secondary.ToString(),
                    Convert.ToUInt16(PrimaryClosed).ToString(),
                    Convert.ToUInt16(SecondaryClosed).ToString(),
                    Convert.ToUInt16(PrimaryILS).ToString(),
                    Convert.ToUInt16(SecondaryILS).ToString(),
                    Convert.ToString((int)Surface),
                    Convert.ToString(EdgeLight),
                    Convert.ToString(CenterLight)
                };
                return string.Join(",", l);
            }

            public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
            {
                ClassSerializer<Runway> cs = new ClassSerializer<Runway>(this, fields);
                cs.Generate(typeof(Runway), fields);

                cs.Get("location", fields, (f) => new List<double>() { Location.X, Location.Y });
                cs.Get("length", fields, (f) => LengthFT * 0.3048);
                cs.Get("width", fields, (f) => WidthMeters * 0.3048);
                cs.Get("surface", fields, (f) => EnumAttr.GetDescription(Surface));
                cs.Get("lit", fields, (f) => CenterLight > 0 || EdgeLight > 0);
                
                var result = cs.Get();
                return result;
            }
        }

        public class Parking
        {
            public ushort ID = 0;
            [ClassSerializerField("number")]
            public ushort Number = 0;
            [ClassSerializerField("type")]
            public ParkingType Type = ParkingType.Vehicles;
            [ClassSerializerField("name")]
            public ParkingName Name = ParkingName.None;
            public float Diameter = 0;
            [ClassSerializerField("heading")]
            public int Heading = 0;
            public List<double> TeeOffsets = new List<double>();
            public Point Location = new Point(0, 0);
            [ClassSerializerField("airline_codes")]
            public List<string> AirlineCodes = new List<string>();

            public Parking(ushort _ID)
            {
                ID = _ID;
            }

            public Parking(string encoded, ushort _ID)
            {
                ID = _ID;

                string[] data = encoded.Split(',');
                
                int _type = 0;
                int.TryParse(data[0], out _type);
                Type = (ParkingType)_type;
                
                int _name = 0;
                int.TryParse(data[1], out _name);
                Name = (ParkingName)_name;

                ushort.TryParse(data[2], out Number);

                float _diam = 0;
                float.TryParse(data[3], out _diam);
                Diameter = _diam;

                double lX = 0;
                double lY = 0;
                double.TryParse(data[4], out lX);
                double.TryParse(data[5], out lY);
                Location = new Point(lX, lY);

                int.TryParse(data[6], out Heading);
            }

            public override string ToString()
            {
                return string.Join(",", new List<string>()
                {
                    ((int)Type).ToString(), // 0
                    ((int)Name).ToString(), // 1
                    Number.ToString(), // 2
                    Math.Round(Diameter, 3).ToString(), // 3
                    Math.Round(Location.X, 6).ToString(), // 4
                    Math.Round(Location.Y, 6).ToString(), // 5
                    Heading.ToString() // 6
                });
            }

            public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
            {
                ClassSerializer<Parking> cs = new ClassSerializer<Parking>(this, fields);
                cs.Generate(typeof(Parking), fields);

                cs.Get("location", fields, (f) => new List<double>() { Location.X, Location.Y });
                cs.Get("diameter", fields, (f) => Diameter * 0.3048);
                
                var result = cs.Get();
                return result;
            }
        }

        public class TaxiwayNode
        {
            public ushort ID = 0;
            public GeoLoc Location = new GeoLoc(0, 0);
            public TaxiwayPointType Type = TaxiwayPointType.Normal;
            public TaxiwayNode(ushort _ID)
            {
                ID = _ID;
            }
            public TaxiwayNode(string encoded, ushort _ID)
            {
                ID = _ID;
                string[] data = encoded.Split(',');
                double lX = 0;
                double lY = 0;
                double.TryParse(data[0], out lX);
                double.TryParse(data[1], out lY);
                Location = new GeoLoc(lX, lY);
                int _type = 0;
                int.TryParse(data[2], out _type);
                Type = (TaxiwayPointType)_type;
            }
            public override string ToString()
            {
                string output = "";
                output += Math.Round(Location.Lon, 6).ToString() + ','; // 0
                output += Math.Round(Location.Lat, 6).ToString() + ','; // 1
                output += Convert.ToInt32(Type).ToString(); // 2
                return output;
            }
        }

        public class TaxiwayPath
        {
            public string Name = "";
            public int Start = 0;
            public int End = 0;
            public int Width = 0;
            public TaxiwayPathType Type = TaxiwayPathType.Path;

            public TaxiwayPath()
            {
            }

            public TaxiwayPath(string encoded)
            {
                string[] data = encoded.Split(',');

                Name = data[0];

                int.TryParse(data[1], out Start);
                int.TryParse(data[2], out End);
                int.TryParse(data[3], out Width);

                int _type = 0;
                int.TryParse(data[4], out _type);
                Type = (TaxiwayPathType)_type;
            }

            public override string ToString()
            {
                string output = "";
                output += Name + ','; // 0
                output += Start.ToString() + ','; // 1
                output += End.ToString() + ','; // 2
                output += Width.ToString() + ','; // 3
                output += Convert.ToInt32(Type).ToString(); // 4
                return output;
            }
        }

        public class ApronGeometry
        {
            public List<Point> Geometry = new List<Point>();
            public Surface Surface = Surface.Unknown;

            public ApronGeometry()
            {
            }

            public ApronGeometry(string encoded)
            {
                string[] data = encoded.Split(',');

                int i = 0;
                foreach (var point in data)
                {
                    if (i == 0)
                    {
                        int _sfc = 0;
                        int.TryParse(data[2], out _sfc);
                        Surface = (Surface)_sfc;
                    }
                    else
                    {
                        double lX = 0;
                        double lY = 0;
                        double.TryParse(data[0], out lX);
                        double.TryParse(data[1], out lY);
                        Geometry.Add(new Point(lX, lY));
                    }
                    i++;
                }
            }

            public override string ToString()
            {
                string output = "";
                foreach (Point point in Geometry)
                {
                    output += Math.Round(point.X, 6).ToString() + "-" + Math.Round(point.Y, 6).ToString() + ",";
                }

                return output;
            }
        }

        public enum TaxiwayPointType
        {
            Normal = 1,
            HoldShort = 2,
            IlsHoldShort = 4
        }

        public enum TaxiwayPathType
        {
            Taxi = 1,
            Runway,
            Parking,
            Path,
            Closed,
            Vehicle
        }

        public enum ParkingName
        {
            None,
            Parking,
            NParking,
            NEParking,
            EParking,
            SEParking,
            SParking,
            SWParking,
            WParking,
            NWParking,
            Gate,
            Dock,
            GateA,
            GateB,
            GateC,
            GateD,
            GateE,
            GateF,
            GateG,
            GateH,
            GateI,
            GateJ,
            GateK,
            GateL,
            GateM,
            GateN,
            GateO,
            GateP,
            GateQ,
            GateR,
            GateS,
            GateT,
            GateU,
            GateV,
            GateW,
            GateX,
            GateY,
            GateZ
        }

        public enum ParkingType
        {
            RampGA = 1,
            RampGASmall,
            RampGAMedium,
            RampGALarge,
            RampCargo,
            RampMilCargo,
            RampMilCombat,
            GateSmall,
            GateMedium,
            GateHeavy,
            DockGA,
            Fuel,
            Vehicles
        }

    }
}
