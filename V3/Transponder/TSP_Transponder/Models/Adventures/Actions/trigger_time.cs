using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.ObjAttr;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_time : action_base
    {
        [ObjValue("Actions")]
        public List<action_base> Actions = new List<action_base>();
        [ObjValue("AfterActions")]
        public List<action_base> AfterActions = new List<action_base>();
        [ObjValue("BeforeActions")]
        public List<action_base> BeforeActions = new List<action_base>();

        public DateTime? TriggerTime = DateTime.UtcNow;
        public DateTime? Triggered = null;
        
        public trigger_time(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_time";

            TriggerTime = Params["TriggerTime"] != null ? DateTime.Parse((string)Params["TriggerTime"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            
            Limit = new Limit(ID, Adv)
            {
                Visible = true,
                Label = (string)Params["Label"],
                Params = new dynamic[] { TriggerTime != null ? ((DateTime)TriggerTime).ToString("O") : null },
                Type = "trigger_time"
            };
            
        }

        public override void Enter()
        {
            base.Enter();

            lock (Adventure.ActionsWatch)
                if (!Adventure.ActionsWatch.Contains(this))
                    Adventure.ActionsWatch.Add(this);

            Triggered = DateTime.UtcNow;
            Limit.Enabled = true;
            Adventure.Save();

            if (Triggered < TriggerTime)
            {
                foreach (action_base action in BeforeActions)
                    action.Enter();
            }
            else
            {
                foreach (action_base action in AfterActions)
                    action.Enter();
            }

            foreach (action_base action in Actions)
                action.Enter();

        }

        public override void Exit()
        {
            base.Exit();

            if (Triggered < TriggerTime)
            {
                foreach (action_base action in BeforeActions)
                    action.Exit();
            }
            else
            {
                foreach (action_base action in AfterActions)
                    action.Exit();
            }

            foreach (action_base action in Actions)
                action.Exit();
        }

        public override void Process()
        {
            if (Entered)
            {
                if (TriggerTime < DateTime.UtcNow)
                {
                    if (Triggered == null)
                    {
                        Triggered = DateTime.UtcNow;
                        foreach (action_base action in AfterActions)
                            action.Enter();
                    }

                    foreach (action_base action in AfterActions)
                        action.Process();
                }
                else
                {
                    if (Triggered == null)
                    {
                        Triggered = DateTime.UtcNow;
                        foreach (action_base action in BeforeActions)
                            action.Enter();
                    }

                    foreach (action_base action in BeforeActions)
                        action.Process();
                }

                foreach (action_base action in Actions)
                    action.Process();
            }
            
        }

        public override void ProcessLive(TemporalData LastData, TemporalData NewData)
        {
            if (TriggerTime < DateTime.UtcNow)
                if (Triggered != null)
                    foreach (action_base action in AfterActions)
                        action.ProcessLive(LastData, NewData);
        }


        public override void Clear()
        {
            lock (Adventure.ActionsWatch)
                Adventure.ActionsWatch.Remove(this);

            Triggered = null;
            Limit.Enabled = false;
        }


        public override void ImportState(Dictionary<string, dynamic> State)
        {
            Entered = State.ContainsKey("Entered") ? State["Entered"] : false;

            Triggered = State.ContainsKey("Triggered") ? State["Triggered"] != null ? DateTime.Parse((string)State["Triggered"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null : null;
            TriggerTime = State["TriggerTime"] != null ? DateTime.Parse((string)State["TriggerTime"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            Limit.Params[0] = TriggerTime != null ? ((DateTime)TriggerTime).ToString("O") : null;

            if (State.ContainsKey("LiveActionWatch"))
                if (State["LiveActionWatch"])
                    lock (Adventure.ActionsWatch)
                        if (!Adventure.ActionsWatch.Contains(this))
                            Adventure.ActionsWatch.Add(this);
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Entered", Entered },
                { "Triggered", Triggered != null ? ((DateTime)Triggered).ToString("O") : null },
                { "TriggerTime", TriggerTime != null ? ((DateTime)TriggerTime).ToString("O") : null },
            };
            
            if (Adventure.ActionsWatch.Contains(this))
                ns.Add("LiveActionWatch", true);

            return ns;
        }


        public override Dictionary<string, dynamic> ComputeForTempate(Route route, Dictionary<string, dynamic> config)
        {

            if (config["TriggerTime"] == null)
            {
                var dep_time_s = route.Parameters["dep_time"];
                var actual_start = DateTime.UtcNow.Date.AddHours(dep_time_s);
                var tomorrow_start = DateTime.UtcNow.Date.AddDays(1).AddHours(dep_time_s);

                if (actual_start < DateTime.UtcNow)
                    actual_start = tomorrow_start;

                config["TriggerTime"] = actual_start.ToString("O");
            }


            return config;
        }
    }
}
