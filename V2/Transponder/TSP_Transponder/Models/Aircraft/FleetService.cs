using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models.Aircraft
{
    class FleetService
    {        
        internal static AircraftInstance CreateAircraft(
            SimLibrary.Simulator Simulator,
            string Name,
            string Directory, 
            string DirectoryFull,
            string Livery,
            double GrossWeight,
            double EmptyWeight,
            double WingspanMeters, 
            int EngineCount,
            uint PayloadStationCount)
        {

            string Manufacturer = null;
            string Creator = null;
            string Model = null;
            Simulator.Connector.ReadAircraftConfig(Simulator, Directory, DirectoryFull, (rManufacturer, rCreator, rModel) =>
            {
                Manufacturer = rManufacturer;
                Creator = rCreator;
                Model = rModel;
            });

            AircraftInstance AI = null;
            lock(LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection<AircraftInstance>("fleet");
                try
                {
                    AI = DBCollection.FindOne(x => x.Name == Name && x.Creator == Creator);
                }
                catch
                {

                }
            }

            if(AI == null)
            {
                AI = new AircraftInstance()
                {
                    Name = Name,
                    Manufacturer = Manufacturer,
                    Creator = Creator,
                };
            }

            AI.Model = Model;
            AI.DirectoryName = Directory;
            AI.DirectoryFull = DirectoryFull;
            AI.LastLivery = Livery;
            AI.EmptyWeightKg = EmptyWeight;
            AI.WingspanMeters = WingspanMeters;
            AI.EngineCount = EngineCount;
            AI.PayloadStationCount = PayloadStationCount;
            AI.LastLivery = Livery;
            AI.Simulator = Simulator.NameStandard;

            if (App.IsDev)
            {
                AI.Update();
            }
            
            //Simulator.Connector.CaptureImage(ImageFormat.Png, Path.Combine(AI.ConfigFilePath, "Thumbnail.png"));
            return AI;
        }

        internal static List<AircraftInstance> GetAllAircraft()
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection<AircraftInstance>("fleet");
                return DBCollection.FindAll().ToList();
            }
        }

        internal static AircraftInstance GetAircraft(string Name)
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection<AircraftInstance>("fleet");
                return DBCollection.FindOne(x => x.Name == Name);
            }
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "getall":
                    {
                        List<Dictionary<string, dynamic>> rs = new List<Dictionary<string, dynamic>>();
                        foreach(var Acf in GetAllAircraft())
                        {
                            rs.Add(Acf.ToDictionary());
                        }

                        Socket.SendMessage("fleet:getall", App.JSSerializer.Serialize(rs), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "getcurrent":
                    {
                        var t = Connectors.SimConnection.Aircraft;
                        Socket.SendMessage("fleet:getcurrent", t != null ? App.JSSerializer.Serialize(t.ToDictionary()) : null, (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "get":
                    {
                        Socket.SendMessage("fleet:get", App.JSSerializer.Serialize(GetAircraft((string)payload_struct["name"]).ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
    }
}
