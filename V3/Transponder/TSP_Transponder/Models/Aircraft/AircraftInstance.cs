using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Aircraft.Cabin;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.WorldManager;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.SimLibrary;

namespace TSP_Transponder.Models.Aircraft
{
    public class AircraftInstance
    {
        [StateSerializerField("_id")]
        [ClassSerializerField("id")]
        public int ID { get; set; } = 0;
        
        [StateSerializerField("DirectoryName")]
        [ClassSerializerField("directory_name")]
        public string DirectoryName { get; set; } = "";

        [StateSerializerField("DirectoryFull")]
        [ClassSerializerField("directory_full")]
        public string DirectoryFull { get; set; } = "";

        [StateSerializerField("DirectoryPackage")]
        [ClassSerializerField("directory_package")]
        public string DirectoryPackage { get; set; } = "";

        [StateSerializerField("Creator")]
        [ClassSerializerField("creator")]
        public string Creator { get; set; } = "";

        [StateSerializerField("Manufacturer")]
        [ClassSerializerField("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [StateSerializerField("Model")]
        [ClassSerializerField("model")]
        public string Model { get; set; } = "";

        [StateSerializerField("Name")]
        [ClassSerializerField("name")]
        public string Name { get; set; } = "";
        
        [StateSerializerField("LastLivery")]
        [ClassSerializerField("last_livery")]
        public string LastLivery { get; set; } = "";

        [StateSerializerField("Location")]
        [ClassSerializerField("location")]
        public double[] Location { get; set; } = null;

        [StateSerializerField("PayloadStations")]
        public List<AircraftPayloadStation> PayloadStations { get; set; } = null;


        [StateSerializerField("Wingspan")]
        [ClassSerializerField("wingspan")]
        public double Wingspan { get; set; } = 0;

        [StateSerializerField("EmptyWeight")]
        [ClassSerializerField("empty_weight")]
        public double EmptyWeight { get; set; } = 0; // KG

        [StateSerializerField("MaxWeight")]
        [ClassSerializerField("max_weight")]
        public double MaxWeight { get; set; } = 0; // KG

        [StateSerializerField("EngineCount")]
        [ClassSerializerField("engine_count")]
        public int EngineCount { get; set; } = 0;

        [StateSerializerField("Size")]
        [ClassSerializerField("size")]
        public int Size { get; set; } = 0;


        [StateSerializerField("InsuranceLast")]
        public DateTime? InsuranceLast { get; set; } = null;

        [StateSerializerField("InsuranceExpire")]
        public DateTime? InsuranceExpire { get; set; } = null;

        [StateSerializerField("InsuranceBilled")]
        [ClassSerializerField("insurance_billed")]
        public float InsuranceBilled { get; set; } = 0;


        [StateSerializerField("TotaledInstances")]
        public List<DateTime> TotaledInstances { get; set; } = new List<DateTime>();
        
        [StateSerializerField("DamagePcnt")]
        [ClassSerializerField("damage_pcnt")]
        public float DamagePcnt { get; set; } = 0;


        [StateSerializerField("DutyLastLivery")]
        [ClassSerializerField("duty_last_livery")]
        public string DutyLastLivery { get; set; } = "";

        [StateSerializerField("DutyFirstFlown")]
        public DateTime? DutyFirstFlown { get; set; } = null;

        [StateSerializerField("DutyLastFlown")]
        public DateTime? DutyLastFlown { get; set; } = null;

        [StateSerializerField("DutyFlightHours")]
        [ClassSerializerField("duty_flight_hours")]
        public float DutyFlightHours { get; set; } = 0;

        [ClassSerializerField("is_duty")]
        public bool IsDuty = false;
        
        
        internal string ConfigFilePath = "";
        internal Point3D LocationFront = new Point3D(0, 0, 5);
        internal Point3D LocationTail = new Point3D(0, 0, -5);
        public Scene_Obj Item = null;

        [StateSerializerField("Cabins")]
        public List<AircraftCabin> Cabins { get; set; } = new List<AircraftCabin>();
        public AircraftCabin Cabin = null;

        internal uint PayloadStationCount = 0;
        
        internal void Startup()
        {
            if(Location != null ? Location.Length < 3 : false)
            {
                Location = new double[] { Location[0], Location[1], 0 };
            }

            if(Item == null)
                Item = new Scene_Obj("aircraft_" + ID, "_fleet", null, SceneObjType.Dynamic)
                {
                    File = DutyLastLivery,
                    CullingDistMin = Convert.ToSingle(Wingspan < 15 ? 15 : Wingspan) / 1000 * 2,
                };
            
            if(Location != null)
                Item.Relocate(new GeoPosition(Location[0], Location[1], Location[2]));
            
        }

        internal void Load()
        {
            if (App.IsDev)
            {
                //if (PayloadStations == null)
                //{
                    //SimConnection.ActiveSim.Connector.CalibratePayloadStations(this, (result) =>
                    //{
                    //    PayloadStations = result;
                    //    Update();
                    //
                    //    Console.WriteLine("Payload");
                    //    foreach (var t in PayloadStations)
                    //    {
                    //        Console.WriteLine(t.X + "\t" + t.Z);
                    //    }
                    //});
                //}
                //else
                //{
                //    Console.WriteLine("Payload");
                //    foreach (var t in PayloadStations)
                //    {
                //        Console.WriteLine(t.X + "\t" + t.Z);
                //    }
                //}

            }
        }

        internal void Save()
        {
            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("fleet");
                    try
                    {
                        DBCollection.Upsert(ID, BsonMapper.Global.ToDocument(ExportState()));
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("Failed to update aircraft in fleet. " + ex1.Message);
                        try
                        {
                            DBCollection.Delete(this.ID);
                            DBCollection.Insert(ID, BsonMapper.Global.ToDocument(ExportState()));
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Failed to fallback update aircraft in fleet. " + ex2.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load aircraft in fleet. " + ex.Message);
            }

        }
        
        internal void SetLocation(double Lon, double Lat, double Hdg)
        {
            Location = new double[] { Lon, Lat, Hdg };
            Item.Relocate(new GeoPosition(Location[0], Location[1], Location[2]));
            this.Save();
        }

        internal void Update()
        {
            bool HasLive = false;
            bool HasDutyChange = false;

            AdventuresBase.GetAllLive((adventure) =>
            {
                if(adventure.IsMonitored)
                {
                    HasLive = true;
                }
            });

            HasDutyChange = IsDuty != HasLive;
            IsDuty = HasLive;

            if (IsDuty)
            {
                SetLocation(SimConnection.LastTemporalData.PLANE_LOCATION.Lon, SimConnection.LastTemporalData.PLANE_LOCATION.Lat, SimConnection.LastTemporalData.PLANE_HEADING_DEGREES);
                Item.IsEnabled = false;
                DutyLastLivery = LastLivery;
            }
            else if(Location != null)
            {
                Item.File = DutyLastLivery;
                Item.IsEnabled = true;
            }

            if(HasDutyChange)
            {
                APIBase.ClientCollection.SendMessage("fleet:update", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "id", ID },
                    { "aircraft", Serialize(new Dictionary<string, dynamic>()
                        {
                            { "is_duty", HasDutyChange },
                            { "nearest_airport", true },
                            { "location", true },
                        })
                    },
                }), null, APIBase.ClientType.Skypad);
            }

        }

        internal void ChangedAircraft(AircraftInstance old_instance, AircraftInstance new_instance)
        {

            if(old_instance == this && new_instance != this)
            {
                Item.IsEnabled = false;
                Item.File = DutyLastLivery;
                if(Cabin != null)
                {
                    Cabin.EndProcess();
                }
                Cabin = null;
                Update();
            }
            else if(new_instance == this)
            {
                if(Cabin != null)
                {
                    Cabin.EndProcess();
                }

                Cabin = Cabins.Find(x => x.Livery == LastLivery);

                if (Cabin == null)
                {
                    Cabin = new AircraftCabin(this);
                    Cabin.StartProcess();
                    Cabins.Add(Cabin);
                    Save();
                }
                else
                {
                    Cabin.Init(this);
                    Cabin.StartProcess();
                }
            }
        }

        internal void UpdateLivery(string Livery)
        {
            Item.IsEnabled = false;
            LastLivery = Livery;
            Update();
        }

        public void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, Dictionary<string, dynamic> payload_struct)
        {
            switch (StructSplit[1])
            {
                case "interact":
                    {
                        switch (StructSplit[2])
                        {
                            case "cabin":
                                {
                                    var cabin = Cabins.Find(x => x.Livery == (string)payload_struct["livery"]);

                                    if (cabin != null)
                                    {
                                        if (!payload_struct.ContainsKey("verb"))
                                        {
                                            if(cabin.Features.Count > 0)
                                            {
                                                cabin.Features.Find(x => x.GUID == (string)payload_struct["guid"]).Interact(Socket, StructSplit, structure, payload_struct.ToDictionary(x1 => x1.Key, x1 => x1.Value));

                                                Socket.SendMessage("response", App.JSSerializer.Serialize(new RequestState()
                                                {
                                                    Status = RequestState.STATUS.SUCCESS,
                                                    ReferenceID = Convert.ToInt64(payload_struct["id"]),
                                                }.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                                            }
                                            Save();
                                        }
                                        else
                                        {
                                            cabin.Interact(Socket, StructSplit, structure, payload_struct);
                                            Save();
                                        }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        
        internal void Process()
        {
            Update();
        }

        internal string GetImageString()
        {
            string base64 = "";
            Bitmap bitmap = GetImage();
            if(bitmap != null)
            {
                base64 = Convert.ToBase64String(Utils.ImageToByteArray(bitmap));
                bitmap.Dispose();
            }
            return base64;
        }

        internal Bitmap GetImage()
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            if(Model != null)
            {
                string model_clean = rgx.Replace(Model, "");

                string save_dir = Path.Combine(App.AppDataDirectory, "Fleet");
                string save_path = Path.Combine(save_dir, model_clean + ".jpg");

                if (File.Exists(save_path))
                {
                    return new Bitmap(Image.FromFile(save_path));
                }
                else if (SimConnection.ActiveSim != null)
                {
                    return SimConnection.ActiveSim.Connector.GetAircraftBitmap(this, save_path);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }


        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<AircraftInstance> cs = new ClassSerializer<AircraftInstance>(this, fields);
            cs.Generate(typeof(AircraftInstance), fields);
            
            cs.Get("duty_first_flown", fields, (f) => DutyFirstFlown != null ? ((DateTime)DutyFirstFlown).ToString("O") : null);
            cs.Get("duty_last_flown", fields, (f) => DutyLastFlown != null ? ((DateTime)DutyLastFlown).ToString("O") : null);
            cs.Get("insurance_last", fields, (f) => InsuranceLast != null ? ((DateTime)InsuranceLast).ToString("O") : null);
            cs.Get("insurance_expire", fields, (f) => InsuranceExpire != null ? ((DateTime)InsuranceExpire).ToString("O") : null);
            cs.Get("totaled_instances", fields, (f) => TotaledInstances.Select(x => x.ToString("O")));
            cs.Get("cabin", fields, (f) =>
            {
                AircraftCabin LastCabin = Cabins.Find(x => x.Livery == LastLivery);
                return LastCabin != null ? LastCabin.Serialize(f) : null;
            });
            cs.Get("cabins", fields, (f) => Cabins.Select(x => x.Serialize(f)));
            cs.Get("image", fields, (f) => GetImageString() );
            cs.Get("nearest_airport", fields, (f) =>
            {
                if(Location != null)
                {
                    Airport NearestAirport = SimList[0].AirportsLib.GetAirportByRange(new GeoLoc(Location[0], Location[1]), 3).FirstOrDefault().Value;
                    return NearestAirport != null ? NearestAirport.Serialize(
                        new Dictionary<string, dynamic>()
                        {
                            { "icao", true },
                            { "name", true },
                            { "country_name", true },
                            { "country", true },
                        }
                    ) : null;
                }
                else
                {
                    return null;
                }
            });

            var result = cs.Get();
            return result;
        }


        public AircraftInstance ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<AircraftInstance>(this, state);

            ss.Set("_id");
            ss.Set("DirectoryName");
            ss.Set("DirectoryFull");
            ss.Set("DirectoryPackage");
            ss.Set("Creator");
            ss.Set("Manufacturer");
            ss.Set("Model");
            ss.Set("Name");
            ss.Set("LastLivery");
            ss.Set("Location", (v) => v != null ? ((ArrayList)v).Cast<decimal>().Select(x => (double)x).ToArray() : null );
            //ss.Apply("PayloadStations");
            ss.Set("Wingspan");
            ss.Set("EmptyWeight");
            ss.Set("MaxWeight");
            ss.Set("EngineCount");
            ss.Set("Size");
            ss.Set("Size");
            ss.Set("InsuranceLast"); //Date
            ss.Set("InsuranceExpire"); // , (v) => v != null ? DateTime.Parse((string)v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null
            ss.Set("InsuranceBilled");
            ss.Set("TotaledInstances", (v) => ((ArrayList)v).Cast<string>().Select(x => DateTime.Parse(x, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).ToList() );
            ss.Set("DamagePcnt");
            ss.Set("DutyLastLivery");
            ss.Set("DutyFirstFlown"); //Date
            ss.Set("DutyLastFlown"); //Date
            ss.Set("DutyFlightHours");

            ss.Set("Cabins", (v) =>
            {
                List<AircraftCabin> ncs = new List<AircraftCabin>();
                foreach(var x in v) { ncs.Add(new AircraftCabin(this).ImportState((Dictionary<string, dynamic>)x)); }
                return ncs;
            });

            return this;
        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftInstance>(this);

            ss.Get("_id");
            ss.Get("DirectoryName");
            ss.Get("DirectoryFull");
            ss.Get("DirectoryPackage");
            ss.Get("Creator");
            ss.Get("Manufacturer");
            ss.Get("Model");
            ss.Get("Name");
            ss.Get("LastLivery");
            ss.Get("Location");
            //ss.Save("PayloadStations");
            ss.Get("Wingspan");
            ss.Get("EmptyWeight");
            ss.Get("MaxWeight");
            ss.Get("EngineCount");
            ss.Get("Size");
            ss.Get("Size");
            ss.Get("InsuranceLast"); //, (v) => InsuranceLast != null ? ((DateTime)InsuranceLast).ToString("O") : null
            ss.Get("InsuranceExpire"); //Date
            ss.Get("InsuranceBilled");
            ss.Get("TotaledInstances", (v) => TotaledInstances.Select(x => x.ToString("O")));
            ss.Get("DamagePcnt");
            ss.Get("DutyLastLivery");
            ss.Get("DutyFirstFlown"); //Date
            ss.Get("DutyLastFlown"); //Date
            ss.Get("DutyFlightHours");

            ss.Get("Cabins", (v) => Cabins.Select(x => x.ExportState()));
            
            return ss.Get();
        }

        public override string ToString()
        {
            return Creator + " - " + Manufacturer + " " + Model;
        }
    }
}
