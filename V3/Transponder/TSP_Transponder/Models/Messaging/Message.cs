using LiteDB;
using System;
using System.Collections.Generic;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.API;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.Messaging
{
    public class Message
    {
        public static List<Message> Persisted = new List<Message>();

        [BsonField("_id")]
        [ClassSerializerField("id")]
        public int Id { get; set; }

        [BsonField("chat_id")]
        [ClassSerializerField("chat_id")]
        public int ChatId { get; set; }

        [BsonField("contract_id")]
        [ClassSerializerField("contract_id")]
        public long ContractID { get; set; }

        [BsonField("situation_index")]
        [ClassSerializerField("situation_index")]
        public int SituationIndex { get; set; }

        [BsonField("contract_topic_ids")]
        [ClassSerializerField("contract_topic_ids")]
        public List<long> ContractTopicIDs { get; set; }

        [BsonField("handle")]
        [ClassSerializerField("handle")]
        public int Handle { get; set; }

        [BsonField("date")]
        public long? Date { get; set; }

        [BsonField("audio_path")]
        [ClassSerializerField("audio_path")]
        public string AudioPath { get; set; }

        [BsonField("from_self")]
        [ClassSerializerField("from_self")]
        public string FromSelf { get; set; }

        [BsonField("content")]
        [ClassSerializerField("content")]
        public string Content { get; set; }

        [BsonField("content_type")]
        public MessageType ContentType { get; set; } = MessageType.Message;
        
        public Dictionary<string, Action> Actions { get; set; }

        public Message()
        {
            Date = DateTime.UtcNow.Ticks;
        }

        public Message(Adventure adv)
        {
            ContractID = adv.ID;
            Date = DateTime.UtcNow.Ticks;

            int RangeSit = adv.Situations.FindIndex(x => x.InRange);
            SituationIndex = RangeSit > -1 ? RangeSit : adv.SituationAt;
        }

        public void Store(bool broadcast)
        {
            using (var db = new ChatThreadsContext())
            {
                var chat_table = db.Database.GetCollection<Chat>("Chats");
                var chat = chat_table.FindOne(x => x.Id == ChatId);
                if(chat == null)
                {
                    chat = new Chat();
                }
                chat.AddHandle(Handle);

                var messages_table = db.Database.GetCollection<Message>("Messages");
                if (messages_table.FindOne(x => x.Id == Id) == null)
                {
                    Id = messages_table.Insert(this);
                }

                if(broadcast)
                {
                    // Broadcast
                    APIBase.ClientCollection.SendMessage("messaging:add", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                    {
                        { "chat", chat.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "id", true },
                                { "handles", true },
                            })
                        },
                        { "message", this.Serialize(null) }
                    }), null, APIBase.ClientType.Skypad);
                }
            }
            
        }

        public void Persist()
        {
            lock (Persisted)
            {
                var existing = Persisted.Find(x => x.Id == Id);
                if(existing != null)
                {
                    existing.Actions = Actions;
                }
                else
                {
                    Persisted.Add(this);
                }
            }
        }
        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<Message> cs = new ClassSerializer<Message>(this, fields);
            cs.Generate(typeof(Message), fields);

            cs.Get("date", fields, (f) => Date != null ? new DateTime((long)Date, DateTimeKind.Utc).ToString("O") : null);
            cs.Get("content_type", fields, (f) => GetDescription(ContentType));
            cs.Get("actions", fields, (f) => Actions != null ? Actions.Keys : null);
            
            return cs.Get();
        }

        public override string ToString()
        {
            return Id + " - " + Content;
        }

        public enum MessageType
        {
            [EnumValue("Joined")]
            Joined,
            [EnumValue("Left")]
            Left,
            [EnumValue("Message")]
            Message,
            [EnumValue("Call")]
            Call,
            [EnumValue("Audio")]
            Audio,
        }


        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[2])
            {
                case "interact":
                    {
                        Interact((int)payload_struct["id"], (string)payload_struct["id"]);
                        break;
                    }
            }
        }

        public static void Interact(int id, string action)
        {
            lock (Persisted)
            {
                var existing = Persisted.Find(x => x.Id == id);
                existing.Actions[action]();
                Persisted.Remove(existing);
            }
        }

    }
}
