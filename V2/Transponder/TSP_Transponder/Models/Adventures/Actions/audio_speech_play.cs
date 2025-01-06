using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Messaging;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class audio_speech_play : action_base
    {
        private bool Played = false;

        public int Delay = 0;
        public string Type = "";
        public string URL = "";
        public string Path = "";
        public string Name = "";
        public string NameID = "";
        public List<action_base> DonePlayingActions = new List<action_base>();

        public audio_speech_play(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "audio_speech_play";

            Delay = Params.ContainsKey("Delay") ? Params["Delay"] : 0;
            Path = Params.ContainsKey("Path") ? Params["Path"] : "";
            Type = Params.ContainsKey("Type") ? Params["Type"] : "";
            Name = Params.ContainsKey("Name") ? Params["Name"] : "";
            NameID = Params.ContainsKey("NameID") ? Params["NameID"] : "";
            
            if (Type == "adventures")
            {
                Path = Template.FileName + "/" + Path;
            }

            if(Params.ContainsKey("DonePlayingActions"))
            {
                foreach (int ActionID in Params["DonePlayingActions"])
                {
                    action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                    if (ActionObj != null)
                    {
                        DonePlayingActions.Add(ActionObj);
                    }
                }
            }
        }

        public void Cache()
        {
            Audio.AudioFramework.GetSpeech(Type, Path.ToLower());
        }
        
        public override void Starting()
        {
            //Audio.AudioFramework.GetSpeech(Type, Path);
        }

        public override void Resuming()
        {
            //Loaded = true;
            //Audio.AudioFramework.GetSpeech(Type, Path);
        }

        public override void Enter()
        {
            if (Path.Trim() != string.Empty)
            {
                if(!Played)
                {
                    Played = true;
                    
                    Audio.AudioFramework.GetSpeech(Type, Path.ToLower(), true, false, false, () =>
                    {
                        foreach (action_base action in DonePlayingActions)
                        {
                            action.Enter();
                        }
                    }, (member, route, msg) =>
                    {
                        Adventure.Memos.EnsureMember(NameID, Name);
                        Adventure.Memos.Add(new ChatThread.Message()
                        {
                            Type = ChatThread.Message.MessageType.Audio,
                            Param = Type + ":" + route,
                            Member = member,
                            Content = msg,
                        });
                        Adventure.ScheduleMemosBroadcast = true;
                        Adventure.Save();

                    });
                }
                else
                {
                    foreach (action_base action in DonePlayingActions)
                    {
                        action.Enter();
                    }
                }
            }
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            Played = State.ContainsKey("Played") ? State["Played"] : false;

        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Played", Played },
            };

            return ns;
        }

    }
}
