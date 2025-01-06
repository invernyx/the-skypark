using System;
using System.Collections.Generic;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class audio_effect_play : action_base
    {
        public int Delay = 0;
        public string Path = "";

        public audio_effect_play(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "audio_effect_play";

            Delay = Params.ContainsKey("Delay") ? Params["Delay"] : 0;
            Path = Params.ContainsKey("Path") ? Params["Path"] : "";
        }

        public void Cache()
        {
            Audio.AudioFramework.GetEffect(Path);
        }

        public override void Enter()
        {
            if(Path.Trim() != string.Empty)
            {
                Audio.AudioFramework.GetEffect(Path, true);
            }
        }

        public override void Exit()
        {

        }

        public override void Process()
        {

        }

    }
}
