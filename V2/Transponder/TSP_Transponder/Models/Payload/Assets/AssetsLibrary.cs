using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Payload.Assets
{
    class AssetsLibrary
    {
        internal static List<Cargo> CargoItems = new List<Cargo>();
        internal static Dictionary<string, List<Cargo>> CargoItemsByTag = new Dictionary<string, List<Cargo>>();

        internal static void Init()
        {
            List<string> AssetsCSV = new List<string>();
            AssetsCSV.AddRange(App.ReadResourceFile("TSP_Transponder.Models.Payload.Assets.tsp_assets_clearsky.csv").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            AssetsCSV.AddRange(App.ReadResourceFile("TSP_Transponder.Models.Payload.Assets.tsp_assets_coyote.csv").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            List<string> ColumnsDescr = CSVParser.Split(AssetsCSV[0]).ToList();

            int i = 1;
            while(i < AssetsCSV.Count){
                string[] X = CSVParser.Split(AssetsCSV[i]);
                try
                {
                    if(X[0] == string.Empty)
                    {
                        i++;
                        continue;
                    }

                    Cargo NewCargo = new Cargo();
                    NewCargo.GUID = X[ColumnsDescr.IndexOf("GUID")];
                    NewCargo.Name = X[ColumnsDescr.IndexOf("Name")].Trim('"');
                    NewCargo.CanInsure = Convert.ToBoolean(Convert.ToInt16(X[ColumnsDescr.IndexOf("Insurable")].Trim('"').Replace(",", "")));
                    NewCargo.Value = Convert.ToSingle(X[ColumnsDescr.IndexOf("Value")].Trim('"').Replace(",", ""));
                    NewCargo.DamagePerGPerS = Convert.ToSingle(X[ColumnsDescr.IndexOf("DamageGPerSec")].Trim('"').Replace(",", ""));
                    NewCargo.WeightKG = Convert.ToUInt16(X[ColumnsDescr.IndexOf("WeightKG")].Trim('"').Replace(",", ""));
                    NewCargo.FlightHoursMin = Convert.ToSingle(X[ColumnsDescr.IndexOf("FlightHoursMin")].Trim('"').Replace(",", ""));
                    NewCargo.FlightHoursMax = Convert.ToSingle(X[ColumnsDescr.IndexOf("FlightHoursMax")].Trim('"').Replace(",", ""));
                    NewCargo.Frequency = Convert.ToUInt16(X[ColumnsDescr.IndexOf("Frequency")].Trim('"').Replace(",", ""));
                    NewCargo.GMax = Convert.ToSingle(X[ColumnsDescr.IndexOf("GLimitMax")].Trim('"').Replace(",", ""));
                    NewCargo.GMin = Convert.ToSingle(X[ColumnsDescr.IndexOf("GLimitMin")].Trim('"').Replace(",", ""));
                    NewCargo.Hazard = Convert.ToUInt16(X[ColumnsDescr.IndexOf("HazardRating")].Trim('"').Replace(",", ""));
                    NewCargo.Models = X[ColumnsDescr.IndexOf("Models")].Trim() != string.Empty ? X[ColumnsDescr.IndexOf("Models")].Trim('"').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    NewCargo.KarmaAdjust = Convert.ToSingle(X[ColumnsDescr.IndexOf("KarmaAdj")].Trim('"').Replace(",", ""));
                    NewCargo.KarmaMin = Convert.ToSingle(X[ColumnsDescr.IndexOf("KarmaMin")].Trim('"').Replace(",", ""));
                    NewCargo.KarmaMax = Convert.ToSingle(X[ColumnsDescr.IndexOf("KarmaMax")].Trim('"').Replace(",", ""));
                    NewCargo.Tags = X[ColumnsDescr.IndexOf("Tags")].Trim() != string.Empty ? X[ColumnsDescr.IndexOf("Tags")].Trim('"').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    NewCargo.TransportModels = X[ColumnsDescr.IndexOf("TransportModels")].Trim() != string.Empty ? X[ColumnsDescr.IndexOf("TransportModels")].Trim('"').Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    NewCargo.XPScoreMin = Convert.ToSingle(X[ColumnsDescr.IndexOf("XPScoreMin")].Trim('"').Replace(",", ""));
                    NewCargo.XPScoreMax = Convert.ToSingle(X[ColumnsDescr.IndexOf("XPScoreMax")].Trim('"').Replace(",", ""));

                    if(NewCargo.WeightKG <= 0)
                    {
                        NewCargo.WeightKG = 42;
                    }

                    CargoItems.Add(NewCargo);
                    foreach (string Tag in NewCargo.Tags)
                    {
                        if (!CargoItemsByTag.ContainsKey(Tag))
                        {
                            CargoItemsByTag.Add(Tag, new List<Cargo>());
                        }
                        CargoItemsByTag[Tag].Add(NewCargo);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to load Cargo item " + AssetsCSV[i]);
                }
                i++;
            }
        }

        internal static Cargo GetFromGUID(string GUID)
        {
            return CargoItems.Find(x => x.GUID == GUID);
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "cargotags":
                    {
                        List<string> Tags = CargoItemsByTag.Keys.ToList();
                        Socket.SendMessage("scenr:cargotags", App.JSSerializer.Serialize(Tags), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
    }
}
