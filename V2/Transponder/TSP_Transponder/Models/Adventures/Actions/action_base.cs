using System;
using System.Collections.Generic;
using System.Linq;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    abstract public class action_base
    {
        public int UID = 0;
        public ushort Order = 0;
        public Adventure Adventure = null;
        public AdventureTemplate Template = null;
        public Situation Situation = null;
        public string ActionName = "";
        public float LvlMin = 0;
        public float LvlMax = float.MaxValue;
        public float KarmaGain = 0;
        public float KarmaMin = -42;
        public float KarmaMax = 42;
        public float RatingMin = 0;
        public float RatingMax = 10;
        public Dictionary<string, dynamic> Parameters = new Dictionary<string, dynamic>();

        private bool? _Completed = null;
        public bool? Completed
        {
            get {
                return _Completed;
            }
            set {
                _Completed = value;
                if (AdventuresBase.AllContracts.Contains(Adventure))
                {
                    Adventure.SchedulePathBroadcast = true;
                }
            }
        }

        public action_base(Adventure Adventure, AdventureTemplate Template, Situation Situation, int UID, Dictionary<string, dynamic> Parameters, action_base Parent)
        {
            this.UID = UID;
            this.Adventure = Adventure;
            this.Template = Template;
            this.Parameters = Parameters;
            this.Situation = Situation;
            this.Install();
        }

        public List<Interaction> Interactions = new List<Interaction>();

        public Limit Limit = null;
        
        public void Install()
        {
            if(Adventure != null)
            {
                Adventure.ActionsIndex.Add(new KeyValuePair<int, action_base>(UID, this));
                Adventure.Actions.Add(this);
                if (Situation != null)
                {
                    Situation.ChildActions.Add(this);
                }
            }
            else
            {
                Template.ActionsIndex.Add(new KeyValuePair<int, action_base>(UID, this));
                Template.Actions.Add(this);
                if (Situation != null)
                {
                    Situation.ChildActions.Add(this);
                }
            }
        }

        public virtual void UpdateInteractions(bool Broadcast)
        {

        }

        public virtual void Interact(Dictionary<string, dynamic> interaction)
        {
            Interactions.Find(x => x.Verb == (string)interaction["Verb"]).Trigger(((Dictionary<string, dynamic>)interaction["Data"]).ToDictionary(x1 => x1.Key, x1 => x1.Value));
        }

        public virtual void Starting()
        {
        }

        public virtual void Resuming()
        {
        }

        public virtual void Ending()
        {
        }
        
        public virtual void Enter()
        {

        }

        public virtual void EnterPaused()
        {

        }

        public virtual void EnterSit()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void ExitPaused()
        {

        }

        public virtual void ExitSit()
        {

        }

        public virtual void Pausing()
        {

        }

        public virtual void Process()
        {

        }

        public virtual void ProcessPaused()
        {

        }

        public virtual void ProcessSit()
        {

        }

        public virtual void LiveProcess(TemporalData LastData, TemporalData NewData)
        {

        }

        public virtual void Clear()
        {

        }
       
        public virtual void ChangedAircraft()
        {

        }
        
        public virtual void ImportState(Dictionary<string, dynamic> State)
        {

        }
        
        public virtual Dictionary<string, dynamic> ComputeForTempate(Route route, Dictionary<string, dynamic> Params)
        {
            return Params;
        }
        

        public virtual Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>();
        }

        public virtual Dictionary<string, dynamic> ToDictionary()
        {
            return new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Action", ActionName },
                { "Params", Parameters }
            };
        }

        public virtual Dictionary<string, dynamic> ToListedActions()
        {
            return null;
        }
    }
}
