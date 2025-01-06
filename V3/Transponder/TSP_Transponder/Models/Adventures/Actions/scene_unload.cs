using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class scene_unload : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> EnterActions = new List<action_base>();

        public int LinkID = -1;
        public scene_load Link = null;

        public scene_unload(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "scene_unload";

            LinkID = Params["Link"];

            foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
            {
                if (action.Value.GetType() == typeof(scene_load))
                {
                    scene_load TestAction = (scene_load)action.Value;
                    if (TestAction.UID == LinkID)
                    {
                        Link = TestAction;
                        Link.Link = this;
                        break;
                    }
                }
            }
            
        }

        public override void Process()
        {

        }
        
        public override void Enter()
        {

            if(Link != null)
            {
                Link.Scene.Hide();
            }

            lock (Adventure.ActionsWatch)
            {
                Adventure.ActionsWatch.Remove(this.Link);
            }

            foreach (action_base action in EnterActions)
            {
                action.Enter();
            }

            Adventure.Save();

        }

    }
}
