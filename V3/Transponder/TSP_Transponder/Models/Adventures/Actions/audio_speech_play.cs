using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Messaging;
using static TSP_Transponder.Attributes.ObjAttr;

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

        [ObjValue("DonePlayingActions")]
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
                    
                    Audio.AudioFramework.GetSpeech(Type, Path.ToLower(), () =>
                    {
                        foreach (action_base action in DonePlayingActions)
                        {
                            action.Enter();
                        }
                    }, (member, route, msg) =>
                    {
                        Chat.SendFromHandleIdent(member, new Message(Adventure)
                        {
                            Content = msg,
                            AudioPath = Type + ":" + route,
                            ContentType = Message.MessageType.Call,
                        });

                        Adventure.ScheduleMemosBroadcast = true;
                        Adventure.Save();

                        //Adventure.Memos.EnsureMember(NameID, Name);
                        //Adventure.Memos.Add(new Chat.Message()
                        //{
                        //    Type = Chat.Message.MessageType.Audio,
                        //    Param = Type + ":" + route,
                        //    Member = member,
                        //    Content = msg,
                        //});
                        //Adventure.ScheduleMemosBroadcast = true;
                        //Adventure.Save();

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
