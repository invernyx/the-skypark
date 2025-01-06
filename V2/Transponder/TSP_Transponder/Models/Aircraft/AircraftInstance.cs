using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models.Aircraft
{
    public class AircraftInstance
    {
        [BsonField("_id")]
        public int ID { get; set; } = 0;

        [BsonField("Creator")]
        public string Creator { get; set; } = "";

        [BsonField("Manufacturer")]
        public string Manufacturer { get; set; } = "";

        [BsonField("Model")]
        public string Model { get; set; } = "";

        [BsonField("Name")]
        public string Name { get; set; } = "";

        [BsonField("LastLivery")]
        public string LastLivery { get; set; } = "";


        [BsonField("PayloadStations")]
        public List<AircraftPayloadStation> PayloadStations { get; set; } = null;


        [BsonField("WingspanMeters")]
        public double WingspanMeters { get; set; } = 0;

        [BsonField("EmptyWeightKg")]
        public double EmptyWeightKg { get; set; } = 0;
        
        [BsonField("EngineCount")]
        public int EngineCount { get; set; } = 0;


        [BsonField("InsuranceLast")]
        public DateTime? InsuranceLast { get; set; } = null;

        [BsonField("InsuranceExpire")]
        public DateTime? InsuranceExpire { get; set; } = null;

        [BsonField("InsuranceBilled")]
        public float InsuranceBilled { get; set; } = 0;


        [BsonField("TotaledInstances")]
        public List<DateTime> TotaledInstances { get; set; } = new List<DateTime>();
        
        [BsonField("DamagePcnt")]
        public float DamagePcnt { get; set; } = 0;


        [BsonField("DutyFirstFlown")]
        public DateTime? DutyFirstFlown { get; set; } = null;

        [BsonField("DutyLastFlown")]
        public DateTime? DutyLastFlown { get; set; } = null;

        [BsonField("DutyFlightHoursMonth")]
        public float DutyFlightHoursMonth { get; set; } = 0;

        [BsonField("DutyFlightHours")]
        public float DutyFlightHours { get; set; } = 0;

        [BsonField("DutyLastLocation")]
        public double[] DutyLastLocation { get; set; } = null;

        [BsonField("Simulators")]
        public string Simulator { get; set; } = "";

        [BsonField("DirectoryName")]
        public string DirectoryName { get; set; } = "";

        [BsonField("DirectoryFull")]
        public string DirectoryFull { get; set; } = "";

        internal string ConfigFilePath = "";
        internal Point3D LocationFront = new Point3D(0, 0, 5);
        internal Point3D LocationTail = new Point3D(0, 0, -5);
        internal Point3D EngineLocation1 = new Point3D(0, 0, 0);
        internal Point3D EngineLocation2 = new Point3D(0, 0, 0);
        internal Point3D EngineLocation3 = new Point3D(0, 0, 0);
        internal Point3D EngineLocation4 = new Point3D(0, 0, 0);

        internal uint PayloadStationCount = 0;

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

        internal void Update()
        {
            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection<AircraftInstance>("fleet");
                    try
                    {
                        DBCollection.Upsert(this);
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("Failed to update aircraft in fleet. " + ex1.Message);
                        try
                        {
                            DBCollection.Delete(this.ID);
                            DBCollection.Insert(this);
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

        internal Dictionary<string, dynamic> ToListing()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Creator", Creator },
                { "Manufacturer", Manufacturer },
                { "Model", Model },
            };

            return rs;
        }

        internal Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Creator", Creator },
                { "Manufacturer", Manufacturer },
                { "Model", Model },
                { "LastLivery", LastLivery },
                { "InsuranceLast", InsuranceLast != null ? ((DateTime)InsuranceLast).ToString("O") : null },
                { "InsuranceBilled", InsuranceBilled },
                { "TotaledInstances", TotaledInstances },
                { "DamagePcnt", DamagePcnt },
                { "DutyFirstFlown", DutyFirstFlown != null ? ((DateTime)DutyFirstFlown).ToString("O") : null },
                { "DutyLastFlown", DutyLastFlown != null ? ((DateTime)DutyLastFlown).ToString("O") : null },
                { "DutyFlightHours", DutyFlightHours },
                { "Simulator", Simulator },
                { "PayloadStations", PayloadStations != null ? PayloadStations.Select(x => x.ToDictionary()) : null },
            };

            return rs;
        }

        public override string ToString()
        {
            return Creator + " - " + Manufacturer + " " + Model;
        }
    }
}
