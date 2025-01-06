using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    abstract public class action_base
    {
        [ClassSerializerField("id")]
        public int UID = 0;
        [ClassSerializerField("action_name")]
        public string ActionName = "";
        [ClassSerializerField("order")]
        public ushort Order = 0;
        public Adventure Adventure = null;
        public AdventureTemplate Template = null;
        public Situation Situation = null;
        public bool Entered = false;
        public bool Within = false;
        public float LvlMin = 0;
        public float LvlMax = float.MaxValue;
        public float KarmaGain = 0;
        public float KarmaMin = -42;
        public float KarmaMax = 42;
        public float RatingMin = 0;
        public float RatingMax = 10;
        public Dictionary<string, dynamic> Parameters = new Dictionary<string, dynamic>();
        public Dictionary<string, List<action_base>> SubActions = new Dictionary<string, List<action_base>>();


        private bool? _Completed = null;
        public bool? Completed
        {
            get {
                return _Completed;
            }
            set {
                if(_Completed != value)
                {
                    _Completed = value;
                    if (AdventuresBase.AllContracts.Contains(Adventure))
                    {
                        Adventure.SchedulePathBroadcast = true;
                        Adventure.Save();
                    }
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
            
            foreach(var actions in Parameters.ToList().FindAll(x => x.Key.EndsWith("Actions")))
            {
                SubActions.Add(actions.Key, new List<action_base>());
                foreach (int ActionID in actions.Value)
                {
                    action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                    if (ActionObj != null)
                    {
                        SubActions[actions.Key].Add(ActionObj);
                        
                        var set_list = (List<action_base>)ObjAttr.GetObj(ActionObj, this, actions.Key);
                        if(set_list != null)
                        {
                            set_list.Add(ActionObj);
                        }

                    }
                }
            }
            
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
            Interactions.Find(x => x.Verb == (string)interaction["verb"]).Trigger(((Dictionary<string, dynamic>)interaction["data"]).ToDictionary(x1 => x1.Key, x1 => x1.Value));
        }

        public virtual bool PreStart(Action<List<Dictionary<string, dynamic>>> objection_callback)
        {
            return true;
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
            Entered = true;
            Within = true;


        }

        public virtual void EnterPaused()
        {

        }

        public virtual void EnterSit()
        {

        }

        public virtual void Exit()
        {
            Within = false;
        }

        public virtual void ExitPaused()
        {

        }

        public virtual void ExitSit()
        {

        }

        public virtual void PausingPreview()
        {

        }

        public virtual void Pausing()
        {

        }

        public virtual void Process()
        {

        }

        public virtual void ProcessAdv()
        {

        }

        public virtual void ProcessPaused()
        {

        }

        public virtual void ProcessSit()
        {

        }

        public virtual void ProcessLive(TemporalData LastData, TemporalData NewData)
        {

        }

        public virtual void Clear()
        {

        }
       
        public virtual void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
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
        
        public virtual Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<action_base> cs = new ClassSerializer<action_base>(this, fields);
            cs.Generate(typeof(Adventure), fields);

            var result = cs.Get();
            return result;
        }

        public virtual Dictionary<string, dynamic> ToListedActions()
        {
            return null;
        }
    }
}
