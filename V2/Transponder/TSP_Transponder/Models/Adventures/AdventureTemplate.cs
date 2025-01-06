using System;
using System.Collections.Generic;
using System.Globalization;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Adventures.Actions;
using System.Collections;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using static TSP_Transponder.App;
using static TSP_Transponder.Models.Adventures.AdventuresBase;
using static TSP_Transponder.Models.Adventures.RouteGenerator;
using System.Runtime.Serialization.Formatters.Binary;
using TSP_Transponder.Models.Airports;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using TSP_Transponder.Models.DataStore;
using LiteDB;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Messaging;
using System.Windows;
using TSP_Transponder.Models.PostProcessing;

namespace TSP_Transponder.Models.Adventures
{
    public class AdventureTemplate
    {
        public static long BaseAdventureID = DateTime.UtcNow.Ticks - 637330216710053157;

        public int Version = 0;
        public DateTime ModifiedOn = DateTime.UtcNow;
        public DateTime? IsTesting = null;
        public bool Published = false;
        public bool Activated = false;
        public bool Loaded = false;
        public bool Ready = false;
        public bool Unlisted = false;
        public bool DirectStart = false;
        public bool StrictOrder = false;
        public bool NoMilestones = true;
        public int Instances = 100;
        public int RouteLimit = 100;
        public bool IsCustom = false;
        public List<DateTime> Dates = new List<DateTime>();
        public float TimeToComplete = 24;
        public bool RunningClock = false;
        public bool Regen = true;
        public string Name = "";
        public string FileName = "";
        public string EncryptedFileName = "";
        public string TemplateCode = "";
        public string EndSummary = "";
        public List<string> Tiers = new List<string>() { "discovery", "endeavour" };
        public List<string> Description = null;
        public List<string> DescriptionLong = null;
        public List<string> AircraftRestriction = new List<string>();
        public string AircraftRestrictionLabel = "";
        public List<string> MediaLink = null;
        public List<string> ImageURL = null;
        public List<string> Type = new List<string>();
        public List<string> CompanyStr = new List<string>();
        public List<Company> Company = new List<Company>();
        public List<dynamic[]> POIs = new List<dynamic[]>();
        public string TypeLabel = "";
        public List<Airport> IncludeICAO = new List<Airport>();
        public List<Airport> ExcludeICAO = new List<Airport>();
        public string IncludeAPTName = "";
        public string ExcludeAPTName = "";
        public List<Dictionary<string, dynamic>> DiscountFees = new List<Dictionary<string, dynamic>>();
        public float CustomScore = 0;
        public float RewardBase = 0;
        public float RewardPerNM = 0;
        public float KarmaGainBase = 0;
        public float KarmaMin = -42;
        public float KarmaMax = 42;
        public float ExpireMin = 0;
        public float ExpireMax = 72;
        public float RatingGainSucceed = 5;
        public float RatingGainFail = -8;
        public float RatingMin = 0;
        public float RatingMax = 10;
        public float XPBase = 0;
        public float LvlMin = 0;
        public float LvlMax = float.MaxValue;
        public Dictionary<string, dynamic> InitialStructure = null;
        private RouteGenerator RG = null;

        public List<Adventure> ActiveAdventures = new List<Adventure>();
        public List<Adventure> LiveAdventures = new List<Adventure>();
        public List<Adventure> InactiveAdventures = new List<Adventure>();

        public List<Situation> Situations = new List<Situation>();
        public List<Route> Routes = new List<Route>();
        public List<string> SituationLabels = new List<string>();
        
        public List<KeyValuePair<int, action_base>> ActionsIndex = new List<KeyValuePair<int, action_base>>();

        public List<action_base> Actions = new List<action_base>();
        public List<action_base> SavedActions = new List<action_base>();
        public List<action_base> StartedActions = new List<action_base>();
        public List<action_base> SuccessActions = new List<action_base>();
        public List<action_base> FailedActions = new List<action_base>();

        public AdventureTemplate(string FileName, Dictionary<string, dynamic> Structure, bool Activate)
        {
            #region Create Adventure base
            this.FileName = FileName.Replace(".p42adv", "");
            EncryptedFileName = FileName.GetHashCode().ToString("X"); //Utils.Encrypt(FileName, "4adb4a48-a659-4174-bf2f-a233f4deb011");
            Published = Structure.ContainsKey("Published") ? Structure["Published"] : true;
            Version = Structure["Version"];
            Name = Structure["Name"];
            TemplateCode = Structure.ContainsKey("TemplateCode") ? Structure["TemplateCode"] : "";
            ModifiedOn = DateTime.Parse(Structure["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            TypeLabel = Structure.ContainsKey("TypeLabel") ? Structure["TypeLabel"] : "contract";
            Type = ((ArrayList)Structure["Type"]).Cast<string>().ToList();
            Tiers = Structure.ContainsKey("Tiers") ? ((ArrayList)Structure["Tiers"]).Cast<string>().ToList() : Tiers;
            CompanyStr = Structure.ContainsKey("Company") ? ((ArrayList)Structure["Company"]).Cast<string>().ToList() : new List<string>() { "clearsky" };
            Regen = Structure.ContainsKey("Regen") ? Structure["Regen"] : true;
            Instances = Structure.ContainsKey("Instances") ? Structure["Instances"] : 0;
            EndSummary = Structure.ContainsKey("EndSummary") ? Structure["EndSummary"] : "";
            RouteLimit = Structure.ContainsKey("RouteLimit") ? Structure["RouteLimit"] : Instances * 5;
            ImageURL = Structure.ContainsKey("ImageURL") ? ((ArrayList)Structure["ImageURL"]).Cast<string>().ToList() : null;
            Description = Structure.ContainsKey("Description") ? ((ArrayList)Structure["Description"]).Cast<string>().ToList() : null;
            DescriptionLong = Structure.ContainsKey("DescriptionLong") ? ((ArrayList)Structure["DescriptionLong"]).Cast<string>().ToList() : null;
            AircraftRestriction = Structure.ContainsKey("AircraftRestriction") ? ((string)Structure["AircraftRestriction"]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
            AircraftRestrictionLabel = Structure.ContainsKey("AircraftRestrictionLabel") ? Structure["AircraftRestrictionLabel"] : "";
            MediaLink = Structure.ContainsKey("MediaLink") ? ((ArrayList)Structure["MediaLink"]).Cast<string>().ToList() : new List<string>();
            StrictOrder = Structure.ContainsKey("StrictOrder") ? Convert.ToBoolean(Structure["StrictOrder"]) : true;
            RunningClock = Structure.ContainsKey("RunningClock") ? Convert.ToBoolean(Structure["RunningClock"]) : true;
            TimeToComplete = Structure.ContainsKey("TimeToComplete") ? Convert.ToSingle(Structure["TimeToComplete"]) : 24;
            ExpireMin = Structure.ContainsKey("ExpireMin") ? Convert.ToSingle(Structure["ExpireMin"]) : 0;
            ExpireMax = Structure.ContainsKey("ExpireMax") ? Convert.ToSingle(Structure["ExpireMax"]) : 72;
            DiscountFees = Structure.ContainsKey("DiscountFees") ? ((ArrayList)Structure["DiscountFees"]).Cast<Dictionary<string, dynamic>>().ToList() : null;
            InitialStructure = Structure;

            if(Structure.ContainsKey("POIs"))
            {
                foreach (var POI in Structure["POIs"])
                {
                    POIs.Add(new dynamic[] { (double)POI[0], (double)POI[1], (string)POI[2] });
                }
            }

            if (Structure.ContainsKey("Dates"))
            {
                Dates.Add(DateTime.Parse((string)Structure["Dates"][0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                Dates.Add(DateTime.Parse((string)Structure["Dates"][1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
            }
            #endregion

            #region Company
            Company = Companies.List.FindAll(x => CompanyStr.Contains(x.Code));
            #endregion

            #region Process all Situations
            //float CustomScoreTemp = 0;
            int Index = 0;
            foreach (Dictionary<string, dynamic> Sit in Structure["Situations"])
            {
                Situation SitObj = new Situation(Index, this, null, null)
                {
                    UID = Sit["UID"],
                    Height = Sit.ContainsKey("Height") ? (float)Sit["Height"] : 0,
                    TriggerRange = Sit.ContainsKey("TriggerRange") ? (float)Sit["TriggerRange"] : 0,
                    Visible = Sit["Visible"],
                    InitialStructure = Sit,
                };

                if (Sit.ContainsKey("Location"))
                {
                    SitObj.Location = new GeoLoc(Convert.ToDouble(Sit["Location"][0]), Convert.ToDouble(Sit["Location"][1]));
                }
                
                if (Sit.ContainsKey("ICAO"))
                {
                    string[] ICAOs = ((string)Sit["ICAO"]).Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (ICAOs.Length == 1)
                    {
                        SitObj.ICAO = Sit["ICAO"];
                        if (SitObj.ICAO != string.Empty)
                        {
                            try
                            {
                                SitObj.Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO(SitObj.ICAO, SitObj.Location);
                                SitObj.TriggerRange = SitObj.Airport.Radius;
                                SitObj.Location = SitObj.Airport.Location;
                            }
                            catch (Exception ex)
                            {
                                if(Activate)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }
                
                if (Sit.ContainsKey("Label"))
                {
                    SituationLabels.Add(Sit["Label"]);
                }
                else
                {
                    SituationLabels.Add(null);
                }
                
                if(Sit.ContainsKey("Actions"))
                {
                    foreach (int ActionID in Sit["Actions"])
                    {
                        action_base ActionObj = CreateAction(null, this, SitObj, ActionID, null);
                        if (ActionObj != null)
                        {
                            SitObj.Actions.Add(ActionObj);
                        }
                    }
                }

                Situations.Add(SitObj);
                Index++;
            }
            #endregion
        
            if (Activate && Tiers.Contains(UserData.Get("tier")))
            {
                #region Include/Exclude ICAO
                if (Structure.ContainsKey("IncludeICAO"))
                {
                    string[] IncludeICAOStr = ((string)Structure["IncludeICAO"]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string[] ExcludeICAOStr = ((string)Structure["ExcludeICAO"]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string ICAO in IncludeICAOStr)
                    {
                        try
                        {
                            IncludeICAO.Add(SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO));
                        }
                        catch
                        {
                        }
                    }

                    foreach (string ICAO in ExcludeICAOStr)
                    {
                        try
                        {
                            ExcludeICAO.Add(SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO));
                        }
                        catch
                        {
                        }
                    }

                    if (IsDev)
                    {
                        if (IncludeICAOStr.Length > 0 && IncludeICAO.Count == 0)
                        {
                            Notifications.NotificationService.Add(new Notifications.Notification()
                            {
                                Title = "IncludeICAO didn't match any ICAOs in " + FileName,
                                Message = ((string)Structure["IncludeICAO"])
                            });
                        }

                        if (ExcludeICAOStr.Length > 0 && ExcludeICAO.Count == 0)
                        {
                            Notifications.NotificationService.Add(new Notifications.Notification()
                            {
                                Title = "ExcludeICAO didn't match any ICAOs in " + FileName,
                                Message = ((string)Structure["ExcludeICAO"])
                            });
                        }
                    }
                }
                #endregion

                #region Include/Exclude Airport Names   
                if (Structure.ContainsKey("IncludeAPTName"))
                {
                    IncludeAPTName = Structure["IncludeAPTName"];
                    ExcludeAPTName = Structure["ExcludeAPTName"];
                }
                #endregion

                #region Calculate Value, XP and Moral and stuff
                RewardPerNM = (float)Structure["RewardPerNM"];
                RewardBase = (float)Structure["RewardBase"];

                if (Structure.ContainsKey("KarmaMin") && Structure.ContainsKey("KarmaMax") && Structure.ContainsKey("KarmaGain"))
                {
                    KarmaGainBase = Convert.ToSingle(Structure["KarmaGain"]);
                    KarmaMin = Convert.ToSingle(Structure["KarmaMin"]);
                    KarmaMax = Convert.ToSingle(Structure["KarmaMax"]);
                }

                if (Structure.ContainsKey("RatingMin") && Structure.ContainsKey("RatingMax"))
                {
                    RatingMin = Convert.ToSingle(Structure["RatingMin"]);
                    RatingMax = Convert.ToSingle(Structure["RatingMax"]);
                }

                if (Structure.ContainsKey("RatingGainSucceed") && Structure.ContainsKey("RatingGainFail"))
                {
                    RatingGainSucceed = Convert.ToSingle(Structure["RatingGainSucceed"]);
                    RatingGainFail = Convert.ToSingle(Structure["RatingGainFail"]);
                }

                if (Structure.ContainsKey("XPBase"))
                {
                    XPBase = Convert.ToSingle(Structure["XPBase"]);
                }

                if (Structure.ContainsKey("LvlMin") && Structure.ContainsKey("LvlMax"))
                {
                    LvlMin = Convert.ToSingle(Structure["LvlMin"]);
                    LvlMax = Convert.ToSingle(Structure["LvlMax"]);
                }
                #endregion

                #region Adventure related Actions
                if(Structure.ContainsKey("SavedActions"))
                {
                    foreach (int ActionID in Structure["SavedActions"])
                    {
                        action_base ActionObj = CreateAction(null, this, null, ActionID, null);
                        if (ActionObj != null)
                        {
                            SavedActions.Add(ActionObj);
                        }
                    }
                }

                foreach (int ActionID in Structure["StartedActions"])
                {
                    action_base ActionObj = CreateAction(null, this, null, ActionID, null);
                    if (ActionObj != null)
                    {
                        StartedActions.Add(ActionObj);
                    }
                }

                foreach (int ActionID in Structure["SuccessActions"])
                {
                    action_base ActionObj = CreateAction(null, this, null, ActionID, null);
                    if (ActionObj != null)
                    {
                        SuccessActions.Add(ActionObj);
                    }
                }

                foreach (int ActionID in Structure["FailedActions"])
                {
                    action_base ActionObj = CreateAction(null, this, null, ActionID, null);
                    if (ActionObj != null)
                    {
                        this.FailedActions.Add(ActionObj);
                    }
                }
                #endregion
                
                #region Calculate CustomScore
                //CustomScore = CustomScoreTemp / Situations.Count;
                #endregion
                
                #region Init Moral, Rating and stuff            
                if (Structure.ContainsKey("KarmaMin") && Structure.ContainsKey("KarmaMax") && Structure.ContainsKey("KarmaGain"))
                {
                    KarmaGainBase = Convert.ToSingle(Structure["KarmaGain"]);
                    KarmaMin = Convert.ToSingle(Structure["KarmaMin"]);
                    KarmaMax = Convert.ToSingle(Structure["KarmaMax"]);
                }

                if (Structure.ContainsKey("RatingGainSucceed") && Structure.ContainsKey("RatingGainFail"))
                {
                    RatingGainSucceed = Convert.ToSingle(Structure["RatingGainSucceed"]);
                    RatingGainFail = Convert.ToSingle(Structure["RatingGainFail"]);
                }

                if (Structure.ContainsKey("RatingMin") && Structure.ContainsKey("RatingMax"))
                {
                    RatingMin = Convert.ToSingle(Structure["RatingMin"]);
                    RatingMax = Convert.ToSingle(Structure["RatingMax"]);
                }

                if (Structure.ContainsKey("XPBase"))
                {
                    XPBase = Convert.ToSingle(Structure["XPBase"]);
                }

                if (Structure.ContainsKey("LvlMin") && Structure.ContainsKey("LvlMax"))
                {
                    LvlMin = Convert.ToSingle(Structure["LvlMin"]);
                    LvlMax = Convert.ToSingle(Structure["LvlMax"]);
                }
                #endregion

                #region Check if Milestones need to be tracked
                foreach (KeyValuePair<int, action_base> action in ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(adventure_milestone))
                    {
                        NoMilestones = false;
                    }
                }
                #endregion

                IsCustom = CheckIsCustom();

                if (ValidateTemplate())
                {
                    if (!Published && IsDev)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(DocumentsDirectory, "Adventures", FileName + ".p42adv"), false))
                        {
                            writer.WriteLine(JSSerializer.Serialize(Structure));
                        }
                    }
                    Activated = true;
                }
                else
                {
                    Loaded = true;
                    Activated = false;
                }
                
            }
            else
            {
                Loaded = true;
                Activated = false;
            }

#if DEBUG
            //if (IsDev)
            //{
            //    if (!Published)
            //    {
            //        Notifications.NotificationService.Add(new Notifications.Notification()
            //        {
            //            Title = FileName + " is not published",
            //            Message = "Just making sure this is intended"
            //        });
            //    }
            //}
#endif

        }
        
        public bool ValidateTemplate()
        {
            List<int> Steps = new List<int>();
            foreach(Situation Sit in Situations)
            {
                foreach(action_base Action in Sit.ChildActions)
                {
                    if (Action.GetType() == typeof(cargo_pickup))
                    {
                        cargo_pickup ActionTyped = (cargo_pickup)Action;
                        Steps.Add(ActionTyped.UID);
                        //ActionTyped.UID
                    }

                    if (Action.GetType() == typeof(cargo_dropoff))
                    {
                        cargo_dropoff ActionTyped = (cargo_dropoff)Action;
                        if(Steps.Contains(ActionTyped.LinkID))
                        {
                            Steps.Remove(ActionTyped.LinkID);
                        }
                        else
                        {
                            if(IsDev)
                            {
                                NotificationService.Add(new Notification()
                                {
                                    Title = "Failed to load " + FileName,
                                    Message = "Cargo unloads twice (" + ActionTyped.LinkID + ")"
                                });
                            }
                            return false;
                        }
                        //ActionTyped.LinkID
                    }
                }
            }

            if(Steps.Count > 0)
            {
                if (IsDev)
                {
                    NotificationService.Add(new Notification()
                    {
                        Title = "Failed to load " + FileName,
                        Message = "Cargo unloads doesn't match cargo loads (" + string.Join(",", Steps) + ")"
                    });
                }
                return false;
            }
            return true;
        }

        public Adventure CreateAdventure(long ID, Dictionary<string, dynamic> Restore, bool DoSave)
        {
            int V = Version;
            if (AdventureVersion == V)
            {
                try
                {
                    return new Adventure(ID, this, Restore, DoSave);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load adventure " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Failed to load adventure, versions don't match " + V + " while " + Version + " is expected");
            }
            return null;
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);

            //AdventureTemplate AdvTemplate = new List<AdventureTemplate>(Templates).Find(x => x.FileName == payload_struct["File"]);
            switch (StructSplit[0])
            {
                case "scenr":
                    {
                        if(IsDev)
                        {
                            switch (StructSplit[1])
                            {
                                case "adventuretemplate":
                                    {
                                        switch (StructSplit[2])
                                        {
                                            case "query":
                                                {
                                                    switch (StructSplit[3])
                                                    {
                                                        case "airports":
                                                            {
                                                                RouteGenParams rGenParams = new RouteGenParams()
                                                                {
                                                                    Sim = SimLibrary.SimList[0],
                                                                };

                                                                List<Dictionary<string, dynamic>> APTList = new List<Dictionary<string, dynamic>>();
                                                                List<Airport> Airports = new List<Airport>();
                                                                var AdventureStruct = payload_struct["Adventure"];

                                                                foreach (Dictionary<string, dynamic> Sit in AdventureStruct["Situations"])
                                                                {
                                                                    switch (Sit["SituationType"])
                                                                    {
                                                                        case "Any":
                                                                        case "Country":
                                                                        case "ICAO":
                                                                            {
                                                                                List<Airport> SitAirports = new List<Airport>();
                                                                                if (((string)AdventureStruct["IncludeICAO"]).Length > 0)
                                                                                {
                                                                                    foreach (string ICAO in ((string)AdventureStruct["IncludeICAO"]).Split(" ;,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                                                                    {
                                                                                        Airport Found = rGenParams.Sim.AirportsLib.GetByICAO(ICAO);
                                                                                        if (Found != null)
                                                                                        {
                                                                                            SitAirports.Add(Found);
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    SitAirports = rGenParams.Sim.AirportsLib.GetAirportsCopy().FindAll(x => !x.IsClosed && !x.IsMilitary);
                                                                                }

                                                                                switch (Sit["SituationType"])
                                                                                {
                                                                                    case "Any":
                                                                                    case "Country":
                                                                                        {
                                                                                            RouteSituation RSit = MakeSituation(Sit);
                                                                                            SitAirports = FilterAirportsBounds(SitAirports, RSit);
                                                                                            SitAirports = FilterAirportsMeta(SitAirports, new RouteGenParams(), RSit);
                                                                                            break;
                                                                                        }
                                                                                    case "ICAO":
                                                                                        {
                                                                                            SitAirports.Clear();
                                                                                            string[] ICAOs = ((string)Sit["ICAO"]).Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                                                                            foreach (string ICAO in ICAOs)
                                                                                            {
                                                                                                Airport FoundAPT = rGenParams.Sim.AirportsLib.GetByICAO(ICAO);
                                                                                                if (FoundAPT != null)
                                                                                                {
                                                                                                    if (!Airports.Contains(FoundAPT))
                                                                                                    {
                                                                                                        SitAirports.Add(FoundAPT);
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                            break;
                                                                                        }
                                                                                }

                                                                                foreach (Airport APT in SitAirports)
                                                                                {
                                                                                    APTList.Add(APT.ToLocation());
                                                                                }
                                                                                break;
                                                                            }
                                                                    }
                                                                }

                                                                while (APTList.Count > 8000)
                                                                {
                                                                    APTList.RemoveAt(Utils.GetRandom(APTList.Count - 1));
                                                                }

                                                                Dictionary<string, dynamic> ReturnStructure = new Dictionary<string, dynamic>()
                                                                {
                                                                    { "count", APTList.Count },
                                                                    { "list", APTList }
                                                                };

                                                                Socket.SendMessage("scenr:adventuretemplate:query:airports", JSSerializer.Serialize(ReturnStructure), (Dictionary<string, dynamic>)structure["meta"]);
                                                                break;
                                                            }
                                                    }
                                                    break;
                                                }
                                            case "list":
                                                {
                                                    Dictionary<string, bool> appdataList = new Dictionary<string, bool>();
                                                    Dictionary<string, bool> integratedList = new Dictionary<string, bool>();

                                                    Dictionary<string, dynamic> TemplatesList = new Dictionary<string, dynamic>()
                                                    {
                                                        { "path", Path.Combine(DocumentsDirectory, "Adventures") },
                                                        { "appdata", appdataList },
                                                        { "integrated", integratedList }
                                                    };

                                                    if (PersistenceRestored)
                                                    {
                                                        if (Directory.Exists(Path.Combine(DocumentsDirectory, "Adventures")))
                                                        {
                                                            foreach (string TemplateFile in Directory.GetFiles(Path.Combine(DocumentsDirectory, "Adventures"), "*.p42adv", SearchOption.TopDirectoryOnly))
                                                            {
                                                                FileInfo fileInfo = new FileInfo(TemplateFile);
                                                                var fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                                                var existing = Templates.Find(x => x.FileName == fileName);
                                                                if (existing != null)
                                                                {
                                                                    appdataList.Add(fileName, existing.Published);
                                                                }
                                                                else
                                                                {
                                                                    appdataList.Add(fileName, false);
                                                                }
                                                            }
                                                        }

                                                        var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");
                                                        var DBTemplates = DBCollection.FindAll();

                                                        foreach (var Entry in DBTemplates)
                                                        {
                                                            if (!appdataList.ContainsKey(Entry["File"]))
                                                            {
                                                                integratedList.Add(Entry["File"], Entry["Published"]);
                                                            }
                                                        }

                                                        //string ResDir = "TSP_Transponder.Models.Adventures.DefaultAdventures.";
                                                        //foreach (string TemplateFile in ThisApp.GetManifestResourceNames().Where(x => x.StartsWith(ResDir)).ToList())
                                                        //{
                                                        //    if(!TemplatesList["appdata"].Contains(TemplateFile.Replace(ResDir, "")))
                                                        //    {
                                                        //        TemplatesList["integrated"].Add(TemplateFile.Replace(ResDir, ""));
                                                        //    }
                                                        //}

                                                        TemplatesList["appdata"] = appdataList.OrderBy(x => x.Value ? 1 : 0).ToDictionary(x => x.Key, x => x.Value);
                                                        TemplatesList["integrated"] = integratedList.OrderBy(x => x.Value ? 1 : 0).ToDictionary(x => x.Key, x => x.Value);

                                                    }

                                                    Socket.SendMessage("scenr:adventuretemplate:list", JSSerializer.Serialize(TemplatesList), (Dictionary<string, dynamic>)structure["meta"]);
                                                    break;
                                                }
                                            case "delete":
                                                {
                                                    string FileName = ((string)payload_struct["File"]).Replace(".p42adv", "");
                                                    AdventureTemplate Existing = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);

                                                    if (Existing != null)
                                                    {
                                                        Existing.Clear(false);
                                                        Existing.Unload();
                                                        Templates.Remove(Existing);
                                                    }

                                                    string TemplateFile = Path.Combine(Path.Combine(DocumentsDirectory, "Adventures"), FileName + ".p42adv");
                                                    if (File.Exists(TemplateFile))
                                                    {
                                                        File.Delete(TemplateFile);
                                                    }

                                                    var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");
                                                    BsonDocument DBTemplate = DBCollection.FindOne(x => x["File"] == FileName);
                                                    if(DBTemplate != null)
                                                    {
                                                        var test = DBTemplate["_id"];
                                                        DBCollection.Delete(DBTemplate["_id"]);
                                                    }

                                                    Socket.SendMessage("scenr:adventuretemplate:delete", "{}", (Dictionary<string, dynamic>)structure["meta"]);
                                                    break;
                                                }
                                            case "load":
                                                {
                                                    string TemplateJSON = null;
                                                    switch (payload_struct["Type"])
                                                    {
                                                        case "appdata":
                                                            {
                                                                string AppdataDir = Path.Combine(Path.Combine(DocumentsDirectory, "Adventures"), payload_struct["File"] + ".p42adv");
                                                                if (File.Exists(AppdataDir))
                                                                {
                                                                    TemplateJSON = string.Join("\n", File.ReadAllText(AppdataDir));
                                                                }

                                                                FileInfo FI = new FileInfo(AppdataDir);
                                                                var ds = JSSerializer.Deserialize<Dictionary<string, dynamic>>(TemplateJSON);
                                                                ds["File"] = FI.Name.Replace(".p42adv", "");

                                                                Socket.SendMessage("scenr:adventuretemplate:load", JSSerializer.Serialize(ds), (Dictionary<string, dynamic>)structure["meta"]);

                                                                break;
                                                            }
                                                        case "integrated":
                                                            {
                                                                //foreach (string TemplateFile in ThisApp.GetManifestResourceNames().Where(x => x.EndsWith(payload_struct["File"])).ToList())
                                                                //{
                                                                //    TemplateJSON = ReadResourceFile(TemplateFile);
                                                                //}
                                                                
                                                                var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");
                                                                string f = payload_struct["File"];
                                                                var DBTemplate = DBCollection.FindOne(x => x["File"] == f);

                                                                TemplateJSON = JSSerializer.Serialize(BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(DBTemplate));
                                                                
                                                                Socket.SendMessage("scenr:adventuretemplate:load", TemplateJSON, (Dictionary<string, dynamic>)structure["meta"]);

                                                                break;
                                                            }
                                                    }

                                                    string FileName = ((string)payload_struct["File"]).Replace(".p42adv", "");
                                                    AdventureTemplate Existing = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);
                                                    List<List<Dictionary<string, dynamic>>> BroadcastedRoutes = new List<List<Dictionary<string, dynamic>>>();
                                                    if (Existing != null)
                                                    {
                                                        lock (Existing.Routes)
                                                        {
                                                            try
                                                            {
                                                                foreach (Route Rte in Existing.Routes)
                                                                {
                                                                    List<Dictionary<string, dynamic>> RouteStruct = new List<Dictionary<string, dynamic>>();
                                                                    foreach (Route.RouteSituation Sit in Rte.Situations)
                                                                    {
                                                                        RouteStruct.Add(new Dictionary<string, dynamic>()
                                                                        {
                                                                            { "Location", Sit.Location.ToList() }
                                                                        });
                                                                    }
                                                                    BroadcastedRoutes.Add(RouteStruct);
                                                                }
                                                            }
                                                            catch
                                                            {

                                                            }
                                                        }
                                                        Socket.SendMessage("scenr:adventuretemplate:test", JSSerializer.Serialize(BroadcastedRoutes));
                                                    }

                                                    break;
                                                }
                                            case "save":
                                                {
                                                    string SaveDir = Path.Combine(DocumentsDirectory, "Adventures");
                                                    string FileName = payload_struct["File"];
                                                    string AdventureTemplateFile = Path.Combine(SaveDir, FileName + ".p42adv");

                                                    if (!Directory.Exists(SaveDir))
                                                    {
                                                        Directory.CreateDirectory(SaveDir);
                                                    }

                                                    using (StreamWriter writer = new StreamWriter(AdventureTemplateFile, false))
                                                    {
                                                        writer.WriteLine(JSSerializer.Serialize(payload_struct));
                                                    }

                                                    AdventureTemplate Existing = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);
                                                    if(Existing != null)
                                                    {
                                                        Existing.Ready = false;
                                                    }

                                                    FileInfo FI = new FileInfo(Path.Combine(SaveDir, FileName));

                                                    LoadTemplate(true, new KeyValuePair<string, string>(FI.Name, File.ReadAllLines(AdventureTemplateFile)[0]), (Template) =>
                                                    {
                                                        if (Template != null)
                                                        {
                                                            lock (Templates)
                                                            {
                                                                if (Existing != null)
                                                                {
                                                                    Existing.Clear(true);
                                                                    Existing.Unload();
                                                                    Templates.Remove(Existing);
                                                                }
                                                                Templates.Add(Template);
                                                                Template.ImportRoutes(false);
                                                                Template.Ready = true;
                                                                UpgradeProcess.TemplatesValidation(false);
                                                            }
                                                            if (Template.Routes.Count > 0)
                                                            {
                                                                Template.PopulateAdventures(false, true);
                                                            }

                                                        }

                                                        Thread.Sleep(300);
                                                        Socket.SendMessage("scenr:adventuretemplate:save", JSSerializer.Serialize(new Dictionary<string, dynamic>() { { "success", Template != null } }), (Dictionary<string, dynamic>)structure["meta"]);
                                                    });

                                                    break;
                                                }
                                            case "regen":
                                                {
                                                    string FileName = ((string)payload_struct["File"]).Replace(".p42adv", "");
                                                    AdventureTemplate Existing = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);
                                                    if (Existing != null)
                                                    {
                                                        lock (Existing.ActiveAdventures)
                                                        {
                                                            while (Existing.ActiveAdventures.Count > 0)
                                                            {
                                                                Existing.ActiveAdventures[0].Remove();
                                                            }
                                                        }

                                                        Existing.PopulateAdventures(false, true);
                                                        Socket.SendMessage("scenr:adventuretemplate:regen", JSSerializer.Serialize(new Dictionary<string, dynamic>() { { "success", true } }), (Dictionary<string, dynamic>)structure["meta"]);
                                                    }
                                                    break;
                                                }
                                            case "test":
                                                {
                                                    string FileName = ((string)payload_struct["File"]).Replace(".p42adv", "");
                                                    bool State = payload_struct["State"];
                                                    AdventureTemplate Template = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);

                                                    if (Template != null)
                                                    {
                                                        if (State)
                                                        {
                                                            DateTime SendNext = DateTime.UtcNow.AddMilliseconds(500);
                                                            List<List<Dictionary<string, dynamic>>> BroadcastedRoutes = new List<List<Dictionary<string, dynamic>>>();
                                                            DateTime TestTime = DateTime.UtcNow;
                                                            Template.FindRoutes((Route) =>
                                                            {
                                                                if (!Socket.IsConnected)
                                                                {
                                                                    Template.IsTesting = null;
                                                                    return;
                                                                }

                                                                if (Route != null)
                                                                {
                                                                    List<Dictionary<string, dynamic>> RouteStruct = new List<Dictionary<string, dynamic>>();
                                                                    foreach (RouteSituation Sit in Route.Situations)
                                                                    {
                                                                        RouteStruct.Add(new Dictionary<string, dynamic>()
                                                                        {
                                                                            { "Location", Sit.Location.ToList() }
                                                                        });
                                                                    }
                                                                    BroadcastedRoutes.Add(RouteStruct);

                                                                    if (SendNext < DateTime.UtcNow)
                                                                    {
                                                                        SendNext = DateTime.UtcNow.AddMilliseconds(5000);
                                                                        List<List<Dictionary<string, dynamic>>> SendStruct = null;
                                                                        lock (BroadcastedRoutes)
                                                                        {
                                                                            SendStruct = new List<List<Dictionary<string, dynamic>>>(BroadcastedRoutes);
                                                                            BroadcastedRoutes.Clear();
                                                                        }
                                                                        Thread TestThread = new Thread(() =>
                                                                        {
                                                                            Socket.SendMessage("scenr:adventuretemplate:test", JSSerializer.Serialize(SendStruct), (Dictionary<string, dynamic>)structure["meta"]);
                                                                        });
                                                                        TestThread.IsBackground = true;
                                                                        TestThread.CurrentCulture = CultureInfo.CurrentCulture;
                                                                        TestThread.Start();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    structure["meta"]["callbackType"] = 0;
                                                                    Socket.SendMessage("scenr:adventuretemplate:test", JSSerializer.Serialize(BroadcastedRoutes), (Dictionary<string, dynamic>)structure["meta"]);
                                                                    Template.ExportRoutes();
                                                                    Template.ImportRoutes(false);
                                                                    Template.Ready = true;
                                                                    Template.PopulateAdventures(false, true);
                                                                }
                                                            });
                                                        }
                                                        else
                                                        {
                                                            Template.IsTesting = null;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case "memos":
                                                {
                                                    string FileName = ((string)payload_struct["File"]).Replace(".p42adv", "");
                                                    AdventureTemplate Existing = Templates.FindAll(x => x.Activated).Find(x => x.FileName == FileName);
                                                    if (Existing != null)
                                                    {
                                                        audio_speech_play audio = (audio_speech_play)Existing.StartedActions.Find(x => x.GetType() == typeof(audio_speech_play));

                                                        var startActionList = ((ArrayList)Existing.InitialStructure["StartedActions"]).Cast<int>().ToList();
                                                        var speech = ((ArrayList)Existing.InitialStructure["Actions"]).Cast<Dictionary<string, dynamic>>().ToList().FindAll(x => startActionList.Contains(x["UID"]) && x["Action"] == "audio_speech_play").FirstOrDefault();
                                                        
                                                        if (speech != null)
                                                        {
                                                            ChatThread ct = new ChatThread();
                                                            
                                                            ct.EnsureMember(speech["Params"]["NameID"], speech["Params"]["Name"]);
                                                            ct.Add(new ChatThread.Message()
                                                            {
                                                                Type = ChatThread.Message.MessageType.Audio,
                                                                Param = speech["Params"]["Type"] + ":" + speech["Params"]["Path"],
                                                                Member = speech["Params"]["NameID"],
                                                                Content = "🔊 New voice memo from " + speech["Params"]["Name"] + ".",
                                                            });

                                                            ct.Add(new ChatThread.Message()
                                                            {
                                                                Type = ChatThread.Message.MessageType.Audio,
                                                                Param = speech["Params"]["Type"] + ":" + speech["Params"]["Path"],
                                                                Member = null,
                                                                Content = Existing.TemplateCode != string.Empty ? "#" + Existing.TemplateCode.ToUpper() : "Let's do this on The Skypark",
                                                            });

                                                            Socket.SendMessage("scenr:adventuretemplate:memos", JSSerializer.Serialize(ct.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                                                        }                                                        
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }
        
        public void Unload()
        {
            if (RG != null)
            {
                RG.Cancel();
            }
            lock (ActiveAdventures)
            {
                foreach (Adventure Adv in ActiveAdventures)
                {
                    lock (AllContracts)
                    {
                        AllContracts.Remove(Adv);
                    }
                }
                while (ActiveAdventures.Count > 0)
                {
                    ActiveAdventures[0].Remove();
                }
            }
            lock (InactiveAdventures)
            {
                foreach (Adventure Adv in InactiveAdventures)
                {
                    lock (AllContracts)
                    {
                        AllContracts.Remove(Adv);
                    }
                }
                while (InactiveAdventures.Count > 0)
                {
                    InactiveAdventures[0].Remove();
                }
            }
        }

        public void Clear(bool KeepRoutes)
        {
            string Dir = FileName.Replace(".p42adv", "");
            if(Dir != string.Empty)
            {
                string DirPersisted = Path.Combine(PersistDirectory, Dir);
                string DirRoutes = Path.Combine(AppDataDirectory, "Routes", Dir);

                if (!KeepRoutes)
                {
                    if (Directory.Exists(DirPersisted))
                    {
                        Directory.Delete(DirPersisted, true);
                    }

                    if (Directory.Exists(DirRoutes))
                    {
                        Directory.Delete(DirRoutes, true);
                    }
                }
                else
                {
                    if (Directory.Exists(DirPersisted))
                    {
                        string[] DatFiles = Directory.GetFiles(DirPersisted, "*.dat");
                        foreach(string file in DatFiles)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
        }

        private bool CheckIsCustom()
        {
            // Check name
            //if(Name == string.Empty)
            //{
            //    return false;
            //}

            // Check Speech
            if (Actions.Find(x => x.GetType() == typeof(audio_speech_play)) == null)
            {
                return false;
            }

            return true;
        }

        public bool CheckValid()
        {
            if (Published || IsDev)
            {
                #region Check if template meets current user requirements
                if(UserData.Get("tier") == "endeavour" || UserData.Get("tier") == "prospect")
                {
                    int Level = Utils.CalculateLevel(Progress.Progress.XP.Balance);

                    if (LvlMin > Level || LvlMax < Level)
                    {
                        if (IsDev)
                        {
                            //Console.WriteLine(FileName + " doesn't meet the XP requirement");
                        }
                        return false;
                    }

                    if (KarmaMin > Progress.Progress.Karma.Balance || KarmaMax < Progress.Progress.Karma.Balance)
                    {
                        if (IsDev)
                        {
                            //Console.WriteLine(FileName + " doesn't meet the Karma requirement");
                        }
                        return false;
                    }

                    if (RatingMin > Progress.Progress.Reliability.Balance || RatingMax < Progress.Progress.Reliability.Balance)
                    {
                        if (IsDev)
                        {
                            //Console.WriteLine(FileName + " doesn't meet the Reliability requirement");
                        }
                        return false;
                    }
                }
                #endregion

                #region Find Start and End date
                if (DateTime.UtcNow < Dates[0] || DateTime.UtcNow > Dates[1])
                {
                    if(IsDev)
                    {
                        //Console.WriteLine("Current date is outside of what's required for the template " + FileName);
                    }
                    return false;
                }
                #endregion

                return true;
            }
            else
            {
                return false;
            }

        }
        
        public List<Route> GetAvailableRoutes()
        {
            List<Route> RoutesAvailable = null;

            lock (Routes)
            {
                RoutesAvailable = new List<Route>(Routes);
            }
            
            #region Elliminate Available Routes from Available Adventures
            lock (ActiveAdventures)
            {
                foreach (Adventure Adv in ActiveAdventures.FindAll(x => x.State == Adventure.AState.Listed))
                {
                    Route Found = RoutesAvailable.Find(x => x.RouteString == Adv.RouteString);
                    if (Found != null)
                    {
                        RoutesAvailable.Remove(Found);
                    }
                }
            }
            #endregion
            
            return RoutesAvailable;
        }

        public List<Route> GetRoutesFromActives(List<Route> RoutesAvailable)
        {
            List<Route> RoutesRelatedToActive = new List<Route>();
            GetAllLive((Adventure) =>
            {
                Situation LastSit = Adventure.Situations.Last();
                if (LastSit.Airport != null)
                {
                    RoutesRelatedToActive = RoutesAvailable.FindAll(x =>
                    {
                        if (x.Situations[0].Airport != null)
                        {
                            return x.Situations[0].Airport == Adventure.Situations.Last().Airport;
                        }
                        return false;
                    });
                }
            });
            return RoutesRelatedToActive;
        }

        public Adventure CreateAdventureFromRoute(Route NR, bool IncludeRouteCode)
        {
            if ((!Regen || !CheckValid() || !Ready || !Published) && !IsDev)
            {
                Loaded = true;
                return null;
            }

            Adventure NA = null;
            Dictionary<string, dynamic> RestoreStruct = new Dictionary<string, dynamic>()
            {
                { "RecommendedAircraft", new ArrayList(NR.RecommendedAircraft) },
                { "Situations", new List<Dictionary<string, dynamic>>() },
                { "Actions", new Dictionary<string, dynamic>() }
            };

            lock(ActiveAdventures)
            {
                lock(LiveAdventures)
                {
                    if (ActiveAdventures.Find(x => x.RouteString == NR.RouteString) == null && LiveAdventures.Find(x => x.RouteString == NR.RouteString) == null)
                    {
                        #region Route Code
                        if (NR.RouteCode.Length > 0)
                        {
                            RestoreStruct.Add("RouteCode", NR.RouteCode);
                        }
                        #endregion

                        #region Process Situations
                        foreach (var Sit in NR.Situations)
                        {
                            if (Sit.Airport != null)
                            {
                                RestoreStruct["Situations"].Add(new Dictionary<string, dynamic>()
                                {
                                    { "UID", Sit.UID },
                                    { "ICAO", Sit.Airport.ICAO },
                                    { "Location", Sit.Location.ToList() }
                                });
                            }
                            else
                            {
                                RestoreStruct["Situations"].Add(new Dictionary<string, dynamic>()
                                {
                                    { "UID", Sit.UID },
                                    { "Location", Sit.Location.ToList() }
                                });
                            }
                        };
                        #endregion

                        #region Compute for Template 
                        foreach (action_base Action in Actions)
                        {
                            ((Dictionary<string, dynamic>)RestoreStruct["Actions"]).Add(Action.UID.ToString(), Action.ComputeForTempate(NR, JSSerializer.Deserialize<Dictionary<string, dynamic>>(JSSerializer.Serialize(Action.Parameters))));
                        }
                        #endregion

                        if (CheckValid())
                        {
                            NA = CreateAdventure((long)BaseAdventureID++, (Dictionary<string, dynamic>)RestoreStruct, true);
                            if (NA != null)
                            {
                                if (!IncludeRouteCode)
                                {
                                    NA.GenerateTopography(IncludeRouteCode);
                                }

                                lock (AllContracts)
                                {
                                    AllContracts.Add(NA);
                                }
                            }
                        }
                    }
                }
            }
            
            return NA;
        }

        public List<Adventure> PopulateAdventuresFromICAO(List<string> ICAOs, int Limit, Dictionary<string, dynamic> Filters)
        {
            if (!Regen || !CheckValid() || !Ready)
            {
                return new List<Adventure>();
            }

            List<Airport> RequiredAirports = new List<Airport>();
            foreach (string ICAO in ICAOs)
            {
                if(ICAO == "-")
                {
                    RequiredAirports.Add(null);
                }
                else
                {
                    Airport Apt = SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO);
                    if (Apt != null)
                    {
                        RequiredAirports.Add(Apt);
                    }
                }
            }

            RouteGenFilters RouteFilters = null;
            if (Filters != null)
            {
                RouteFilters = new RouteGenFilters();
                if (Filters.ContainsKey("range"))
                {
                    RouteFilters.Range[0] = Convert.ToSingle(Filters["range"][0]);
                    RouteFilters.Range[1] = Convert.ToSingle(Filters["range"][1]);
                }
                if (Filters.ContainsKey("rwyCount"))
                {
                    RouteFilters.RwyCount[0] = Convert.ToInt32(Filters["rwyCount"][0]);
                    RouteFilters.RwyCount[1] = Convert.ToInt32(Filters["rwyCount"][1]);
                }
                if (Filters.ContainsKey("runways"))
                {
                    RouteFilters.RwyLength[0] = Convert.ToSingle(Filters["runways"][0]);
                    RouteFilters.RwyLength[1] = Convert.ToSingle(Filters["runways"][1]);
                }
                if (Filters.ContainsKey("rwySurface"))
                {
                    RouteFilters.RwySurface = Filters["rwySurface"];
                }
            }

            if (RequiredAirports.FindAll(x => x != null).Count > 1)
            {
                FindRoutes((G) => { }, false, RequiredAirports, Limit, RouteFilters);
            }

            List<Route> RoutesAvailableSorted = GetAvailableRoutes();
            List<Route> RoutesAvailable = new List<Route>();
            List<KeyValuePair<int, Route>> RoutesAvailableMatch = new List<KeyValuePair<int, Route>>();

            // Limit based on user filters
            if(RoutesAvailableSorted != null)
            {
                RoutesAvailableSorted = RoutesAvailableSorted.FindAll(x => x.DistanceKM > RouteFilters.Range[0] * 1.852f && x.DistanceKM < RouteFilters.Range[1] * 1.852f).ToList();
            }

            // Random sort
            while (RoutesAvailableSorted.Count > 0)
            {
                Route NR = RoutesAvailableSorted[Utils.GetRandom(RoutesAvailableSorted.Count)];
                RoutesAvailableSorted.Remove(NR);

                lock(ActiveAdventures)
                {
                    if (ActiveAdventures.Find(x => x.RouteString == NR.RouteString) == null && LiveAdventures.Find(x => x.RouteString == NR.RouteString) == null)
                    {
                        RoutesAvailable.Add(NR);
                    }
                }
            }

            // Filter matches
            foreach(Route Rte in RoutesAvailable)
            {
                List<KeyValuePair<Airport, List<bool>>> Fits = CheckFitsICAOS(Rte.Situations.Select(x => x.Location), RequiredAirports, false);

                int Pass = 0;
                foreach(var Airport in Fits)
                {
                    int i = 0;
                    foreach(var slot in Airport.Value)
                    {
                        if(slot)
                        {
                            if(Rte.Situations[i].Airport != null)
                            {
                                if (Rte.Situations[i].Airport == Airport.Key)
                                {
                                    Pass++;
                                }
                            }
                        }
                        i++;
                    }
                }
                
                //List<Route.RouteSituation> Sits = Rte.Situations.FindAll(x => x.Airport != null ? ICAOs.Contains(x.Airport.ICAO) : false).ToList();

                if (Pass > 0)
                {
                    RoutesAvailableMatch.Add(new KeyValuePair<int, Route>(Pass, Rte));
                }
            }
            RoutesAvailableMatch = RoutesAvailableMatch.OrderByDescending(x => x.Key).ToList();
            
            // Generate adventure
            List<Adventure> Advs = new List<Adventure>();
            int C = 0;
            while (Advs.Count < Limit && C < RoutesAvailableMatch.Count)
            {
                KeyValuePair<int, Route> NR = RoutesAvailableMatch[C];
                Adventure NA = CreateAdventureFromRoute(NR.Value, false);

                if(NA != null)
                {
                    Advs.Add(NA);
                }
                else
                {
                    break;
                }
                C++;
            }
            
            return Advs;
        }

        public void PopulateAdventures(bool Slow, bool Override)
        {
            if (!Activated)
            {
                return;
            }

            bool IsValid = CheckValid();
            if ((!Regen || !IsValid || !Ready || !Published) && !Override)
            {
                if (IsDev)
                {
                    Console.WriteLine("Skipped updating Contracts for " + FileName + " because " + Regen + "," + IsValid + "," + Ready + "," + Published + "," + Override);
                }

                Loaded = true;
                return;
            }

            if (KarmaGainBase < 0 && UserData.Get("illicit") == "0")
            {
                if (IsDev)
                {
                    Console.WriteLine("Skipped updating Contracts for " + FileName + " because of illicit setting");
                }

                Loaded = true;
                return;
            }

            List<Route> RoutesAvailable = GetAvailableRoutes();
            List<Route> RoutesRelatedToActive = GetRoutesFromActives(RoutesAvailable);

            if(IsDev)
                Console.WriteLine("Updating Contracts for " + FileName + " (" + ActiveAdventures.Count + " actives)");

            if(IsDev && !MW.IsShuttingDown)
            {
                if (Routes.Count == 0 && Instances > 0)
                {
                    Notifications.NotificationService.Add(new Notifications.Notification()
                    {
                        Title = FileName + " has zero routes",
                        Message = EncryptedFileName
                    });
                }
            }

            int Limit = 2000;
            if(UserData.Get("tier") == "prospect")
            {
                lock (AllContracts)
                {
                    List<Adventure> Listed = AllContracts.FindAll(x => x.State == Adventure.AState.Listed);
                    if(42 - Listed.Count > 0)
                    {
                        Limit = 1;
                    }
                    else
                    {
                        Limit = 0;
                    }
                }
            }
            
            int NewCount = 0;
            while (Routes.Count > 0 && ActiveAdventures.Count < Instances && RoutesAvailable.Count > 0 && Limit > NewCount)
            {
                if (Slow)
                {
                    Thread.Sleep(100);
                }
                
                Route NR = null;
                if (RoutesRelatedToActive.Count > 0)
                {
                    NR = RoutesRelatedToActive[Utils.GetRandom(RoutesRelatedToActive.Count - 1)];
                    RoutesRelatedToActive.Remove(NR);
                }
                else
                {
                    NR = RoutesAvailable[Utils.GetRandom(RoutesAvailable.Count - 1)];
                }

                RoutesAvailable.Remove(NR);

                if (CreateAdventureFromRoute(NR, true) != null)
                {
                    NewCount++;
                }
            }

            if(NewCount > 0)
            {
                if (IsDev)
                {
                    Console.WriteLine("Done Exporting " + NewCount + " new Contracts for " + FileName + " (" + ActiveAdventures.Count + " Total)");
                }
            }

            Loaded = true;
        }

        public void SetTopography(bool Slow)
        {
            Adventure Adv = null;
            int i = 0;
            while(i < ActiveAdventures.Count)
            {
                try
                {
                    Adv = ActiveAdventures[i];
                    Adv.GenerateTopography(true);
                }
                catch
                {
                }
                i++;
            }
        }

        public void FindRoutes(Action<GeneratedRoute> Callback, bool MakeRouteCode = true, List<Airport> RequireICAOs = null, int Limit = -1, RouteGenFilters Filters = null)
        {
            if (KarmaGainBase < 0 && UserData.Get("illicit") == "0")
            {
                Callback?.Invoke(new GeneratedRoute());
                return;
            }

            if(MW.IsShuttingDown)
            {
                return;
            }

            //if(Filters == null)
            //{
            Console.WriteLine("Finding Routes for " + FileName);
            //}

            RouteGenParams RGP = new RouteGenParams()
            {
                Name = FileName,
                Filters = Filters,
                Sim = SimLibrary.SimList[0],
                Limit = Limit > 0 ? Limit : RouteLimit,
                RequireICAO = RequireICAOs != null ? RequireICAOs : null,
                IncludeICAO = IncludeICAO,
                ExcludeICAO = ExcludeICAO,
                Callback = Callback,
            };

            if(RG != null)
            {
                RG.IsCanceled = true;
            }
            
            RG = new RouteGenerator(InitialStructure, RGP, (Percent, Text) =>
            {

            });

            RG.Generate();

            #region Read Routes
            if (!RG.IsCanceled)
            {
                List<GeneratedRoute> NewRoutes = RG.ReadRoutes();
                //if(NewRoutes.Count > 0)
                //{
                //}
                Console.WriteLine("Found " + NewRoutes.Count + " Routes for " + FileName);
                foreach (GeneratedRoute Route in NewRoutes)
                {
                    Route NR = new Route(this)
                    {
                        RecommendedAircraft = Route.RecommendedAircraft,
                    };
                    foreach (var Sit in Route.Situations)
                    {
                        NR.Situations.Add(new Route.RouteSituation()
                        {
                            UID = Sit.UID,
                            Airport = Sit.Airport,
                            Location = Sit.Location
                        });
                    }
                    NR.CalculateDistance();
                    NR.CalculateString();
                    if(MakeRouteCode)
                    {
                        NR.CreateRouteCode();
                    }
                    lock (Routes)
                    {
                        Routes.Add(NR);
                    }
                }
            }
            #endregion

            Callback?.Invoke(null);
        }
        

        public void ExportRoutes()
        {
            if(IsDev)
            {
                try
                {
                    if (Routes.Count > 0)
                    {
                        Task.Factory.StartNew(() => {
                            CultureInfo.CurrentCulture = CI;

                            if (FileName != string.Empty)
                            {
                                Console.WriteLine("Exporting Adventure Routes for " + FileName);
                                string AppdataFolder = PersistDirectory;
                                string DocumentsFolder = Path.Combine(DocumentsDirectory, "Adventures");
                                string SerializedJSon = JSSerializer.Serialize(GetExport());

                                if (!Directory.Exists(AppdataFolder)) { Directory.CreateDirectory(AppdataFolder); }
                                using (StreamWriter writer = new StreamWriter(Path.Combine(AppdataFolder, EncryptedFileName + ".rtl")))
                                {
                                    writer.WriteLine(SerializedJSon);
                                }

                                if(!Published)
                                {
                                    if (!Directory.Exists(DocumentsFolder)) { Directory.CreateDirectory(DocumentsFolder); }
                                    using (StreamWriter writer = new StreamWriter(Path.Combine(DocumentsFolder, FileName + ".rtl")))
                                    {
                                        writer.WriteLine(SerializedJSon);
                                    }
                                }
                            }
                        }, ThreadCancel.Token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to save Adventure Routes for " + FileName + " / " + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
        }

        public void ImportRoutes(bool regen = true)
        {
            if (KarmaGainBase < 0 && UserData.Get("illicit") == "0")
            {
                return;
            }

            if(RouteLimit > 0)
            {
                #region Load from folder
                Action<string> LoadFile = (RouteFile) =>
                {
                    lock(Routes)
                    {
                        if (Routes.Count == 0 && File.Exists(RouteFile))
                        {
                            try
                            {
                                FileInfo FI = new FileInfo(RouteFile);
                                string Del = null;
                                using (StreamReader r = new StreamReader(new FileStream(RouteFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                                {
                                    Dictionary<string, dynamic> RoutesStruct = JSSerializer.Deserialize<Dictionary<string, dynamic>>(r.ReadToEnd());
                                    try
                                    {
#if DEBUG
                                        if(IsDev && Convert.ToInt32(RoutesStruct["Version"]) != RouteVersion)
                                        {
                                            Del = "Version mismatch";
                                        }
#endif

                                        DateTime RouteFileDate = DateTime.Parse((string)RoutesStruct["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                                        if (ModifiedOn.Ticks == RouteFileDate.Ticks && Del == null)
                                        {
                                            List<int> Sits = ((ArrayList)RoutesStruct["Sits"]).Cast<int>().ToList();

                                            foreach (string Route in RoutesStruct["Routes"])
                                            {
                                                if (MW.IsShuttingDown) { return; }
                                                try
                                                {
                                                    Routes.Add(new Route(this, Sits, Route));
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (IsDev)
                                                    {
                                                        Notifications.NotificationService.Add(new Notifications.Notification()
                                                        {
                                                            Title = "A route failed to load for " + FileName,
                                                            Message = ex.Message
                                                        });
                                                    }
                                                }
                                                Thread.Sleep(1);
                                            }
                                        }
                                        else
                                        {
                                            Del = "Modified date is different (" + ModifiedOn.Ticks + " vs " + RouteFileDate.Ticks + ")";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (IsDev)
                                        {
                                            Notifications.NotificationService.Add(new Notifications.Notification()
                                            {
                                                Title = "A route failed to load for " + FileName,
                                                Message = ex.Message
                                            });
                                        }
                                        Del = ex.Message;
                                    }
                                }


                                if (Del != null)
                                {
                                    Console.WriteLine("Invalid Routes for " + FileName + " (" + EncryptedFileName + "), " + Del);
                                    File.Delete(RouteFile);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                };
                #endregion

                LoadFile(Path.Combine(PersistDirectory, EncryptedFileName + ".rtl"));

                if(Routes.Count == 0)
                {
                    LoadFile(Path.Combine(DocumentsDirectory, "Adventures", FileName + ".rtl"));
                    ExportRoutes();
                }

                if(RegenRoutes || IsDev)
                {
                    if(Routes.Count == 0 && regen)
                    {
                        FindRoutes(null);
                        ExportRoutes();
                    }

                    #region List countries
                    List<string> AdvStartCountries = new List<string>();
                    List<string> AdvEndCountries = new List<string>();
                    foreach (var Rte in Routes)
                    {
                        int Index = 0;
                        foreach (var Sit in Rte.Situations)
                        {
                            if (Sit.Airport != null)
                            {
                                if (Index == 0)
                                {
                                    if (!AdvStartCountries.Contains(Sit.Airport.Country))
                                    {
                                        AdvStartCountries.Add(Sit.Airport.Country);
                                    }
                                }
                                else if (Index == Rte.Situations.Count - 1)
                                {
                                    if (!AdvEndCountries.Contains(Sit.Airport.Country))
                                    {
                                        AdvEndCountries.Add(Sit.Airport.Country);
                                    }
                                }
                            }
                            Index++;
                        }
                    }
                    Console.WriteLine("Countries for " + FileName + " Departures: [" + string.Join("|", AdvStartCountries) + "]");
                    Console.WriteLine("Countries for " + FileName + " Arrivals: [" + string.Join("|", AdvEndCountries) + "]");
                    Console.WriteLine("Countries for " + FileName + ": [" + string.Join("|", AdvStartCountries.Union(AdvEndCountries)) + "]");
                    #endregion
                }

                if (Routes.Count == 0)
                {
                    Console.WriteLine(FileName + " has zero routes");
                }

            }
        }
        

        public Dictionary<string, dynamic> GetListing(bool detailed)
        {
            if (detailed)
            {
                Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                {
                    { "Name", Name },
                    { "FileName", FileName },
                    { "IsCustom", IsCustom },
                    { "Description", Description },
                    { "ImageURL", ImageURL.Select(x => x.Replace("%imagecdn%", APIBase.CDNImagesHost)) },
                    { "Type", Type },
                    { "POIs", POIs },
                    { "Company", CompanyStr },
                    { "TypeLabel", TypeLabel },
                    { "TemplateCode", TemplateCode },
                    { "RunningClock", RunningClock },
                    { "TimeToComplete", TimeToComplete },
                    { "AircraftRestrictionLabel", AircraftRestrictionLabel },
                    { "Situations", new List<int>() },
                    { "RequiresLevel", new List<float> { LvlMin, LvlMax } },
                    { "RequiresKarma", new List<float> { KarmaMin, KarmaMax } },
                    { "RequiresReliability", new List<float> { RatingMin, RatingMax } },
                };
                
                foreach (Situation Sit in Situations)
                {
                    ns["Situations"].Add(Sit.UID);
                }

                return ns;
            }
            else
            {
                Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                {
                    { "Name", Name },
                    { "FileName", FileName },
                    { "IsCustom", IsCustom },
                    { "Description", Description },
                    { "RunningClock", RunningClock },
                    { "TimeToComplete", TimeToComplete },
                    { "TemplateCode", TemplateCode },
                    { "AircraftRestrictionLabel", AircraftRestrictionLabel },
                    { "ImageURL", ImageURL.Select(x => x.Replace("%imagecdn%", APIBase.CDNImagesHost)) },
                    { "Type", Type },
                    { "Company", CompanyStr },
                    { "TypeLabel", TypeLabel },
                    { "RequiresLevel", new List<float> { LvlMin, LvlMax } },
                    { "RequiresKarma", new List<float> { KarmaMin, KarmaMax } },
                    { "RequiresReliability", new List<float> { RatingMin, RatingMax } },
                };

                return ns;
            }
        }

        public Dictionary<string, dynamic> GetCarrot()
        {
            List<KeyValuePair<string, List<dynamic>>> Requirements = new List<KeyValuePair<string, List<dynamic>>>();
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Name", Name },
                { "FeatureType", "carrot" },
                { "TypeLabel", TypeLabel },
                { "FileName", FileName },
                { "FileNameE", EncryptedFileName },
                { "ImageURL", ImageURL.Count > 0 ? ImageURL[0].Replace("%imagecdn%", APIBase.CDNImagesHost) : "" },
                { "Company", CompanyStr },
                { "Requirements", Requirements },
            };
            
            Requirements.Add(new KeyValuePair<string, List<dynamic>>("level", new List<dynamic>() { LvlMin, LvlMax, Utils.CalculateLevel(Progress.Progress.XP.Balance) }));
            Requirements.Add(new KeyValuePair<string, List<dynamic>>("karma", new List<dynamic>() { KarmaMin, KarmaMax, Progress.Progress.Karma.Balance }));
            Requirements.Add(new KeyValuePair<string, List<dynamic>>("reliability", new List<dynamic>() { RatingMin, RatingMax, Progress.Progress.Reliability.Balance }));
            Requirements.Add(new KeyValuePair<string, List<dynamic>>("dates", new List<dynamic>() { Dates[0].ToString("O"), Dates[1].ToString("O"), DateTime.UtcNow.ToString("O") }));
            return ns;
        }

        public Dictionary<string, dynamic> GetFeatured()
        {
            List<string> Countries = new List<string>();
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Name", Name },
                { "FeatureType", "featured" },
                { "FileName", FileName },
                { "FileNameE", EncryptedFileName },
                { "ImageURL", ImageURL.Count > 0 ? ImageURL[0].Replace("%imagecdn%", APIBase.CDNImagesHost) : "" },
                { "Type", Type },
                { "Contracts", ActiveAdventures.Count },
                { "Company", CompanyStr },
                { "TypeLabel", TypeLabel },
                { "TemplateCode", TemplateCode },
                { "Countries", Countries },
            };

            Dictionary<string, int> CountriesSorted = new Dictionary<string, int>();

            lock (ActiveAdventures)
            {
                foreach (var Adv in ActiveAdventures)
                {
                    foreach (var Sit in Adv.Situations)
                    {
                        if (Sit.Airport != null)
                        {
                            if (!CountriesSorted.ContainsKey(Sit.Airport.Country))
                            {
                                CountriesSorted.Add(Sit.Airport.Country, 1);
                            }
                            else
                            {
                                CountriesSorted[Sit.Airport.Country]++;
                            }
                        }
                    }
                }
            }

            foreach (var Country in CountriesSorted.OrderByDescending(x => x.Value))
            {
                Countries.Add(Country.Key);
            }

            return ns;
        }

        public Dictionary<string, dynamic> GetExport()
        {
            List<string> RoutesListStr = new List<string>();
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Version", RouteVersion },
                { "ModifiedOn", ModifiedOn.ToString("O") },
                { "File", FileName },
                { "Routes", Routes.Select(x => x.Export()) },
                { "Sits", new List<int>() },
            };
            
            foreach(var Sit in Situations)
            {
                ns["Sits"].Add(Sit.UID);
            }

            return ns;
        }

        public Dictionary<string, dynamic> GetExportFull(Adventure Adv)
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>(InitialStructure);
            ns["ImageURL"] = new List<string>() { ImageURL.Count > 0 ? ImageURL[Adv.ImageIndex].Replace("%imagecdn%", APIBase.CDNImagesHost) : "" };

            return ns;
        }

        public Dictionary<string, dynamic> ToDictFull()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();

            foreach(var k in InitialStructure)
            {
                switch(k.Key)
                {
                    case "ModifiedOn":
                        {
                            ns.Add(k.Key, ModifiedOn.ToString("O"));
                            break;
                        }
                    default:
                        {
                            ns.Add(k.Key, k.Value);
                            break;
                        }
                }
            }


            return ns;
        }
        


        public override string ToString()
        {
            return FileName.ToString();
        }

        public static void DownloadSpeech(string FileName, Dictionary<string, dynamic> source)
        {
            if(IsDev)
            {
                try
                {
                    foreach (var action in source["Actions"])
                    {
                        string SaveExportDir = Path.Combine(AppDataDirectory, "Sounds_Export");
                        string SaveDir = Path.Combine(AppDataDirectory, "Sounds");

                        if (action["Action"] == "audio_speech_play")
                        {
                            #region Find Save location
                            FileInfo manifestFileExportPath = null;
                            FileInfo manifestFilePath = null;

                            switch (action["Params"]["Type"])
                            {
                                case "adventures":
                                    {
                                        SaveExportDir = Path.Combine(SaveExportDir, "adventures", FileName.ToLower());
                                        SaveDir = Path.Combine(SaveDir, "adventures", FileName.ToLower());
                                        break;
                                    }
                                case "characters":
                                    {
                                        SaveExportDir = Path.Combine(SaveExportDir, "characters", action["Params"]["NameID"].ToLower());
                                        SaveDir = Path.Combine(SaveDir, "characters", action["Params"]["NameID"].ToLower());
                                        break;
                                    }
                            }
                            #endregion

                            #region Save new Manifest
                            Action SaveManifest = () =>
                            {
                                try
                                {
                                    string Transcript = action["Params"].ContainsKey("Transcript") ? action["Params"]["Transcript"] : "";

                                    #region Check existing manifest files
                                    string manifestExistingJSON = "";
                                    if (File.Exists(manifestFileExportPath.FullName))
                                    {
                                        manifestExistingJSON = File.ReadAllText(manifestFileExportPath.FullName);
                                        Dictionary<string, dynamic> manifestDict = JSSerializer.Deserialize<Dictionary<string, dynamic>>(manifestExistingJSON);

                                        if (manifestDict["Type"] == "CAPTION")
                                        {
                                            Transcript = action["Params"]["Transcript"] == string.Empty ? manifestDict["Caption"]["EN"] : Transcript;
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    #endregion

                                    #region Save new manifest
                                    if (!Directory.Exists(manifestFileExportPath.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(manifestFileExportPath.Directory.FullName);
                                    }
                                    if (!Directory.Exists(manifestFilePath.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(manifestFilePath.Directory.FullName);
                                    }

                                    string newSerialized = JSSerializer.Serialize(new Dictionary<string, dynamic>()
                                    {
                                        { "Version", "1" },
                                        { "Type", "CAPTION" },
                                        { "Cast", action["Params"].ContainsKey("NameID") ? action["Params"]["NameID"] : "" },
                                        { "Caption", new Dictionary<string, string>() { { "EN", Transcript } } },
                                    });

                                    if(newSerialized != manifestExistingJSON.Trim())
                                    {
                                        using (StreamWriter writer = new StreamWriter(manifestFileExportPath.FullName, false))
                                        {
                                            writer.WriteLine(newSerialized);
                                        }
                                        File.Copy(manifestFileExportPath.FullName, manifestFilePath.FullName, true);
                                    }

                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    if(IsDev)
                                    {
                                        NotificationService.Add(new Notification()
                                        {
                                            Title = "Failed to load caption for " + FileName,
                                            Message = ex.Message + " - " + ex.StackTrace,
                                            Type = NotificationType.Status,
                                        });
                                    }
                                }

                            };
                            #endregion

                            if (action["Params"]["URL"] != null)
                            {
                                string p = action["Params"]["URL"];
                                if (p.Contains("http://") || p.Contains("https://"))
                                {
                                    Uri Uri = new Uri(p);
                                    switch (Uri.Host)
                                    {
                                        case "dl.dropboxusercontent.com":
                                            {
                                                string fileExt = Uri.Segments.Last().Split('.').Last();
                                                string saveFileName = (action["Params"]["Path"].ToLower());

                                                if (!Directory.Exists(SaveExportDir)) { Directory.CreateDirectory(SaveExportDir); }
                                                if (!Directory.Exists(SaveDir)) { Directory.CreateDirectory(SaveDir); }

                                                FileInfo audioFileExportPath = new FileInfo(Path.Combine(SaveExportDir, saveFileName + "." + fileExt));
                                                FileInfo audioFilePath = new FileInfo(Path.Combine(SaveDir, saveFileName + "." + fileExt));
                                                manifestFileExportPath = new FileInfo(Path.Combine(SaveExportDir, saveFileName + ".json"));
                                                manifestFilePath = new FileInfo(Path.Combine(SaveDir, saveFileName + ".json"));

                                                if (!manifestFileExportPath.Exists || !manifestFilePath.Exists || !audioFilePath.Exists || !audioFileExportPath.Exists)
                                                {
                                                    new FileRequest((percent, total, received) => { },
                                                        () =>
                                                        {
                                                            SaveManifest();
                                                            try
                                                            {
                                                                File.Copy(Path.Combine(SaveExportDir, saveFileName + "." + fileExt), Path.Combine(SaveDir, saveFileName + "." + fileExt), true);
                                                            }
                                                            catch
                                                            {
                                                            }
                                                        },
                                                        (ex) =>
                                                        {
                                                            Notifications.NotificationService.Add(new Notifications.Notification()
                                                            {
                                                                Title = "Failed to download audio for " + p + " on " + FileName + "",
                                                                Message = (action["UID"].ToString())
                                                            });
                                                        },
                                                        p,
                                                        Path.Combine(SaveExportDir, saveFileName + "." + fileExt)
                                                    );
                                                }
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    manifestFileExportPath = new FileInfo(Path.Combine(SaveExportDir, (action["Params"]["Path"].ToLower()) + ".json"));
                                    manifestFilePath = new FileInfo(Path.Combine(SaveDir, (action["Params"]["Path"].ToLower()) + ".json"));

                                    SaveManifest();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Notifications.NotificationService.Add(new Notifications.Notification()
                    {
                        Title = "Failed to download audio for " + FileName + " on " + FileName + "",
                        Message = ex.Message + " - " + ex.StackTrace
                    });
                }
            }
        }

    }

}
