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
        internal static List<AircraftInstance> Fleet = new List<AircraftInstance>();

        internal static AircraftInstance CreateAircraft(
            SimLibrary.Simulator Simulator,
            string Name,
            string Directory,
            string DirectoryFull,
            string ImagePath,
            string Livery,
            string Type,
            double EmptyWeight,
            double MaxWeight,
            double WingspanMeters,
            int EngineCount,
            uint PayloadStationCount)
        {

            string Manufacturer = null;
            string Creator = null;
            string Model = null;
            string Package = null;
            Simulator.Connector.ReadPackageConfig(Simulator, Directory, DirectoryFull, (rManufacturer, rCreator, rModel, rPackage) =>
            {
                Manufacturer = rManufacturer;
                Creator = rCreator;
                Model = rModel;
                Package = rPackage;
            });

            bool isNew = false;
            AircraftInstance AI = Fleet.Find(x => x.Name == Name && x.Creator == Creator);

            if (AI == null)
            {
                isNew = true;
                AI = new AircraftInstance()
                {
                    ID = Fleet.Count,
                    Name = Name,
                    Manufacturer = Manufacturer,
                    Creator = Creator,
                };
            }

            AI.Model = Model;
            AI.DirectoryName = Directory;
            AI.DirectoryFull = DirectoryFull;
            AI.DirectoryPackage = Package;
            AI.EmptyWeight = EmptyWeight;
            AI.MaxWeight = MaxWeight;
            AI.Wingspan = WingspanMeters;
            AI.EngineCount = EngineCount;
            AI.PayloadStationCount = PayloadStationCount;

            switch (Type)
            {
                case "Airplane":
                    {
                        int i = 1;
                        foreach (var backet in new List<int>()
                        {
                            1700, // Piston
                            2900, // Turboprop
                            30000, // Jet
                            65000, // Narrow
                            9999999,
                        })
                        {
                            if (EmptyWeight < backet)
                            {
                                AI.Size = i;
                                break;
                            }
                            i++;
                        }
                        break;
                    }
                case "Helicopter":
                    {
                        AI.Size = 0;
                        break;
                    }
                default:
                    {
                        AI.Size = -1;
                        break;
                    }
            }

            AI.Startup();
            AI.UpdateLivery(Livery);
            AI.Save();

            if (isNew)
            {
                lock (Fleet)
                {
                    Fleet.Add(AI);
                }
            }

            //Simulator.Connector.CaptureImage(ImageFormat.Png, Path.Combine(AI.ConfigFilePath, "Thumbnail.png"));
            return AI;
        }


        internal static void Startup()
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection("fleet");

                lock (Fleet)
                {
                    foreach (var acf_src in DBCollection.FindAll())
                    {
                        var dd = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(App.JSSerializer.Serialize(BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(acf_src)));
                        Fleet.Add(new AircraftInstance().ImportState(dd));
                    }
                
                    foreach (var aircraft in Fleet)
                    {
                        aircraft.Startup();
                    }
                }
            }
        }

        internal static void Disconnect(AircraftInstance old_instance)
        {
            lock (Fleet)
            {
                foreach (var aircraft in Fleet)
                {
                    aircraft.ChangedAircraft(old_instance, null);
                }
            }
        }
        
        internal static void ChangedAircraft(AircraftInstance old_instance, AircraftInstance new_instance)
        {
            lock (Fleet)
            {
                foreach(var aircraft in Fleet)
                {
                    aircraft.ChangedAircraft(old_instance, new_instance);
                }
            }
            
            APIBase.ClientCollection.SendMessage("fleet:current_aircraft", App.JSSerializer.Serialize(new_instance.Serialize(null)), null, APIBase.ClientType.Skypad);
        }

        internal static List<AircraftInstance> GetAllAircraft()
        {
            return Fleet;
        }

        internal static AircraftInstance GetAircraft(string Name)
        {
            return Fleet.Find(x => x.Name == Name);
        }

        internal static AircraftInstance GetAircraft(int ID)
        {
            return Fleet.Find(x => x.ID == ID);
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "get_all":
                    {
                        Socket.SendMessage("response", App.JSSerializer.Serialize(GetAllAircraft().Select(x => x.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]))), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "get_current":
                    {
                        var t = Connectors.SimConnection.Aircraft;
                        Socket.SendMessage("response", t != null ? App.JSSerializer.Serialize(t.Serialize((Dictionary<string, dynamic>)payload_struct["fields"])) : null, (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "get":
                    {
                        Socket.SendMessage("response", App.JSSerializer.Serialize(GetAircraft((int)payload_struct["id"]).Serialize((Dictionary<string, dynamic>)payload_struct["fields"])), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "interact":
                    {

                        var aircraft = GetAircraft((int)payload_struct["id"]);
                        aircraft.Command(Socket, StructSplit, structure, payload_struct);
                        break;
                    }
            }
        }

        public static void WSConnected(SocketClient Socket)
        {
            Socket.SendMessage("fleet:getall", App.JSSerializer.Serialize(GetAllAircraft().Select(x => x.Serialize(null))));
        }
    }
}
