using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.WorldManager;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class scene_load : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> EnterActions = new List<action_base>();
        
        public int LinkID = -1;
        public scene_unload Link = null;
        public Scene Scene = null;
        
        public scene_load(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "scene_load";

            if (Link == null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(scene_unload))
                    {
                        scene_unload TestAction = (scene_unload)action.Value;
                        if (TestAction.LinkID == UID)
                        {
                            Link = TestAction;
                            Link.Link = this;
                            break;
                        }
                    }
                }
            }

            Scene = new Scene(Adventure.SocketID, ((ArrayList)Params["Scene"]["Features"]).Cast<Dictionary<string, dynamic>>().ToList());
            
        }

        public override void Enter()
        {
            lock (Adventure.ActionsWatch)
            {
                if (!Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Add(this);
                    Scene.Show();
                }
            }
            Adventure.Save();
        }

        public override void Process()
        {

        }

        public override void Clear()
        {
            lock (Adventure.ActionsWatch)
            {
                Adventure.ActionsWatch.Remove(this);
                Scene.Clear();
            }
            Adventure.Save();
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            Scene.IsVisible = State["Scene.IsVisible"];
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "Scene.IsVisible", Scene.IsVisible },
            };
        }
    }
}
