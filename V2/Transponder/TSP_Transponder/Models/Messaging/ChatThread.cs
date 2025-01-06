using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.App;

namespace TSP_Transponder.Models.Messaging
{
    public class ChatThread
    {
        public Dictionary<string, string> Members = new Dictionary<string, string>();
        public DateTime LastRead = DateTime.UtcNow;
        public List<Message> Messages = new List<Message>();

        public ChatThread()
        {
        }

        public ChatThread(Dictionary<string, dynamic> restore)
        {
            Members = ((Dictionary<string, object>)(restore["Members"])).ToDictionary(k => k.Key, k => k.Value.ToString());
            LastRead = DateTime.Parse((string)restore["LastRead"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            foreach(var msg in ((ArrayList)restore["Messages"]).Cast<Dictionary<string, dynamic>>().ToList())
            {
                Add(new Message(msg));
            }
        }

        public void Send(string Message)
        {
            Add(new Message()
            {
                Member = null,
                Content = Message
            });
        }

        public void EnsureMember(string nameId, string name)
        {
            lock (Members)
            {
                if (!Members.ContainsKey(nameId))
                {
                    Members.Add(nameId, name);
                }
            }
        }

        public void Add(Message msg)
        {
            // Add message
            lock (Messages)
            {
                Messages.Add(msg);
            }
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Members", Members },
                { "LastRead", LastRead.ToString("O") },
                { "Messages", Messages.Select(x => x.ToDictionary()) }
            };


            return ns;
        }

        public class Message
        {
            public string Member;
            public DateTime Sent = DateTime.UtcNow;
            public string Param;
            public string Content;
            public MessageType Type = MessageType.Message;

            public Message(Dictionary<string, dynamic> restore)
            {
                Member = restore.ContainsKey("Member") ? (string)restore["Member"] : null;
                Param = restore.ContainsKey("Param") ? (string)restore["Param"] : null;
                Sent = restore.ContainsKey("Sent") ? DateTime.Parse((string)restore["Sent"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : DateTime.UtcNow;
                Content = restore.ContainsKey("Content") ? (string)restore["Content"] : "";
                Type = restore.ContainsKey("Type") ? (MessageType)GetEnum(typeof(MessageType), (string)restore["Type"]) : MessageType.Message;
            }

            public Message()
            {

            }

            public Dictionary<string, dynamic> ToDictionary()
            {
                Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                {
                    { "Member", Member },
                    { "Sent", Sent.ToString("O") },
                    { "Content", Content },
                    { "Param", Param },
                    { "Type", GetDescription(Type) },
                };
                return ns;
            }
            
            public enum MessageType
            {
                [EnumValue("message")]
                Message,
                [EnumValue("audio")]
                Audio,
                [EnumValue("joined")]
                Joined,
                [EnumValue("left")]
                Left
            }
        }
    }
}
