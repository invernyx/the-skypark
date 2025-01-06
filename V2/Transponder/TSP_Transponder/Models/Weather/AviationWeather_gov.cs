using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.WeatherModel;
using static TSP_Transponder.Models.Airports.AirportsLib;
using static TSP_Transponder.Models.WeatherModel.WeatherData;

namespace TSP_Transponder.Models.Weather
{
    class AviationWeather_gov
    {
        public static Dictionary<string, string[]> METARs = new Dictionary<string, string[]>();
        public static Dictionary<string, WeatherData> WeatherStations = new Dictionary<string, WeatherData>();

        public static void GetWeatherData(Action Callback)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                    {
                        if(e.Error == null)
                        {
                            List<string> Rows = new List<string>();
                            if (e.Result != string.Empty)
                            {
                                Rows = e.Result.Split("\n".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                            }

                            bool Start = false;
                            foreach (string Row in Rows)
                            {
                                try
                                {
                                    if (!Start)
                                    {
                                        if (Row.StartsWith("raw_text"))
                                        {
                                            Start = true;
                                        }
                                    }
                                    else
                                    {
                                        string[] RowSpl = Row.Split(",".ToArray());
                                        if(RowSpl.Length > 1)
                                        {
                                            if (!METARs.ContainsKey(RowSpl[1]))
                                            {
                                                METARs.Add(RowSpl[1], RowSpl);
                                            }
                                            else
                                            {
                                                METARs[RowSpl[1]] = RowSpl;
                                            }

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                            WeatherStations.Clear();
                        }
                        Callback();
                    };
                    wc.DownloadStringAsync(new System.Uri("https://aviationweather.gov/adds/dataserver_current/current/metars.cache.csv"));

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get Weather data " + ex.Message);
            }
        }

        public static WeatherData GetWeatherStation(string Station, GeoLoc Location)
        {
            WeatherData wd = null;
            Action<string[]> Decode = (WeatherSplit) =>
            {

                wd = new WeatherData();

                wd.Time = DateTime.UtcNow;
                wd.Station = WeatherSplit[1];
                wd.Altimeter = WeatherSplit[11] == "" ? 0 : (float)Math.Round(Convert.ToSingle(WeatherSplit[11]), 2);
                wd.DewPoint = WeatherSplit[6] == "" ? 0 : Convert.ToSingle(WeatherSplit[6]);
                wd.WindHeading = WeatherSplit[7] == "" ? 0 : Convert.ToSingle(WeatherSplit[7]);
                wd.WindSpeed = WeatherSplit[8] == "" ? 0 : Convert.ToSingle(WeatherSplit[8]);
                wd.WindGust = WeatherSplit[9] == "" ? 0 : Convert.ToSingle(WeatherSplit[9]);
                wd.Temperature = WeatherSplit[5] == "" ? 0 : Convert.ToSingle(WeatherSplit[5]);
                wd.VisibilitySM = WeatherSplit[10] == "" ? -1 : (long)(Convert.ToSingle(WeatherSplit[10]) * 1609.344);


                #region Cloud Layers
                List<KeyValuePair<int, string>> Clouds = new List<KeyValuePair<int, string>>();
                if(WeatherSplit.Length > 28)
                {
                    if (WeatherSplit[22] != string.Empty)
                    {
                        try
                        {
                            switch (WeatherSplit[22])
                            {
                                case "CLR":
                                case "CAVOK":
                                case "SKC":
                                    {
                                        break;
                                    }
                                default:
                                    {
                                        wd.Clouds.Add(new CloudLayer()
                                        {
                                            Base = Convert.ToInt32(WeatherSplit[23]),
                                            Coverage = (Cloud_Coverage)App.GetEnum(typeof(Cloud_Coverage), WeatherSplit[22]),
                                        });
                                        Clouds.Add(new KeyValuePair<int, string>(Convert.ToInt32(WeatherSplit[23]), WeatherSplit[22]));
                                        break;
                                    }
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (WeatherSplit[24] != string.Empty)
                    {
                        try
                        {
                            wd.Clouds.Add(new CloudLayer()
                            {
                                Base = Convert.ToInt32(WeatherSplit[25]),
                                Coverage = (Cloud_Coverage)App.GetEnum(typeof(Cloud_Coverage), WeatherSplit[24]),
                            });
                        }
                        catch
                        {
                        }
                    }

                    if (WeatherSplit[26] != string.Empty)
                    {
                        try
                        {
                            wd.Clouds.Add(new CloudLayer()
                            {
                                Base = Convert.ToInt32(WeatherSplit[27]),
                                Coverage = (Cloud_Coverage)App.GetEnum(typeof(Cloud_Coverage), WeatherSplit[26]),
                            });
                        }
                        catch
                        {
                        }
                    }

                    if (WeatherSplit[28] != string.Empty)
                    {
                        try
                        {
                            wd.Clouds.Add(new CloudLayer()
                            {
                                Base = Convert.ToInt32(WeatherSplit[29]),
                                Coverage = (Cloud_Coverage)App.GetEnum(typeof(Cloud_Coverage), WeatherSplit[28]),
                            });
                        }
                        catch
                        {
                        }
                    }
                }
                #endregion

                #region Precipit
                if (WeatherSplit.Length > 21)
                {
                    if (WeatherSplit[21] != "")
                    {
                        if (WeatherSplit[21].Contains("RA"))
                        {
                            wd.Precipitation = Precipitation_Types.RA;
                            wd.Precipitation_Rate = 2;
                        }
                        if (WeatherSplit[21].Contains("SN"))
                        {
                            wd.Precipitation = Precipitation_Types.SN;
                            wd.Precipitation_Rate = 2;
                        }
                        if (WeatherSplit[21].Contains("-"))
                        {
                            wd.Precipitation_Rate = 1;
                        }
                        else if (WeatherSplit[21].Contains("+"))
                        {
                            wd.Precipitation_Rate = 3;
                        }
                        if (WeatherSplit[21].Contains("TS"))
                        {
                            wd.Thunderstorm = true;
                        }
                    }
                }
                #endregion
            };

            if (WeatherStations.ContainsKey(Station))
            {
                return WeatherStations[Station];
            }
            else
            {
                if (METARs.ContainsKey(Station))
                {
                    try
                    {
                        Decode(METARs[Station]);
                        WeatherStations.Add(Station, wd);
                        return wd;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    List<KeyValuePair<double, Airport>> Apts = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(Location, 27.78f);
                    foreach (KeyValuePair<double, Airport> Apt in Apts)
                    {
                        if (WeatherStations.ContainsKey(Station))
                        {
                            return WeatherStations[Station];
                        }
                        else if(Apt.Value != null)
                        {
                            if (METARs.ContainsKey(Apt.Value.ICAO))
                            {
                                try
                                {
                                    Decode(METARs[Apt.Value.ICAO]);
                                    wd.IsNearby = (float)Utils.MapCalcDist(Location, Apt.Value.Location, Utils.DistanceUnit.NauticalMiles, true);
                                    WeatherStations.Add(Station, wd);
                                    return wd;
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            
            return wd;
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "get":
                    {
                        WeatherData wd = GetWeatherStation(payload_struct["icao"], new GeoLoc(Convert.ToDouble(payload_struct["lon"]), Convert.ToDouble(payload_struct["lat"])));
                        Socket.SendMessage("weather:get", wd != null ? App.JSSerializer.Serialize(wd.ToDictionary()) : "null", (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }

    }
}
