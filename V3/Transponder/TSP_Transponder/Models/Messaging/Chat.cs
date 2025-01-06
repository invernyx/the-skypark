using LiteDB;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using TSP_Transponder.Models.API;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Messaging
{
    public class Chat
    {
        [BsonField("_id")]
        [ClassSerializerField("id")]
        public int Id { get; set; }

        [BsonField("read_at_date")]
        public long ReadAtDate { get; set; }

        [BsonField("handles")]
        public List<int> Handles { get; set; } = new List<int>();
        

        public void Store()
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Chat>("Chats");
                var existing = table.FindOne(x => x.Id == Id);
                if (existing == null)
                {
                    Id = table.Insert(this);
                }
                else
                {
                    table.Update(this);
                }
            }
        }

        public bool AddHandle(int handle)
        {
            if(!Handles.Contains(handle))
            {
                Handles.Add(handle);
                Store();
                return true;
            }
            return false;
        }

        public bool RemoveHandle(int handle)
        {
            if (Handles.Contains(handle))
            {
                Handles.Remove(handle);
                Store();
                return true;
            }
            return false;
        }
        
        public List<Message> GetMessagesFromContract(long contract_id, int limit, int offset = 0)
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Message>("Messages");
                var messages = table.Find(x => x.ChatId == Id && x.Id > offset && x.ContractID == contract_id).OrderByDescending(x => x.Id).Take(limit);
                var messages_list = messages.ToList();
                return messages_list;
            }
        }

        public List<Message> GetMessages(int limit, int offset = 0)
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Message>("Messages");
                var messages = table.Find(x => x.ChatId == Id && x.Id > offset).OrderByDescending(x => x.Id).Take(limit);
                var messages_list = messages.ToList();
                return messages_list;
            }
        }

        public Message GetLastMessage()
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Message>("Messages");
                var message = table.Find(x => x.ChatId == Id).OrderByDescending(x => x.Id).Take(1);
                return message.FirstOrDefault();
            }
        }


        public List<Handle> GetHandles()
        {
            using (var db = new ChatThreadsContext())
            {
                List<Handle> handles_list = new List<Handle>();
                foreach(var handle in Handles)
                {
                    var table = db.Database.GetCollection<Handle>("Handles");
                    var handle_obj = table.FindOne(x => x.Id == handle);
                    handles_list.Add(handle_obj);
                }
                return handles_list;
            }
        }


        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<Chat> cs = new ClassSerializer<Chat>(this, fields);
            cs.Generate(typeof(Chat), fields);

            cs.Get("handles", fields, (f) => GetHandles().Select(x => x.Serialize(f)).ToList());
            cs.Get("messages", fields, (f) => GetMessages(10).Select(x => x.Serialize(f)).ToList());
            cs.Get("last_message", fields, (f) => GetLastMessage().Serialize(f));

            cs.Get("read_at_date", fields, (f) => ReadAtDate != 0 ? new DateTime((long)ReadAtDate, DateTimeKind.Utc).ToString("O") : null);

            var result = cs.Get();
            return result;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
        

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[2])
            {
                case "get-from-id":
                    {
                        var chats = GetFromID((int)payload_struct["id"]);
                        Socket.SendMessage("response", App.JSSerializer.Serialize(chats.Serialize((Dictionary<string, dynamic>)payload_struct["fields"])), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "get-all":
                    {
                        var chats = GetAll((int)payload_struct["limit"], (int)payload_struct["offset"]);
                        chats = chats.OrderByDescending(x => x.ReadAtDate).ToList();
                        Socket.SendMessage("response", App.JSSerializer.Serialize(chats.Select(x => x.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]))), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "read":
                    {
                        MarkAsRead((int)payload_struct["id"], DateTime.Parse((string)payload_struct["date"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                        break;
                    }
            }
        }

        public static Chat GetFromID(int chat_id)
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Chat>("Chats");
                var chat = table.FindOne(x => x.Id == chat_id);
                return chat;
            }
        }

        public static Chat Open(List<int> handles)
        {
            using (var db = new ChatThreadsContext())
            {
                var handles_sorted = handles.OrderBy(x => x).ToList();
                var handles_string = string.Join("|", handles_sorted);
                var table = db.Database.GetCollection<Chat>("Chats");
                var chats = table.FindAll();
                var chat = chats.FirstOrDefault(x => string.Join("|", x.Handles) == handles_string);

                if (chat == null)
                {
                    chat = new Chat()
                    {
                        Handles = handles_sorted,
                    };
                    chat.Store();
                }
                return chat;
            }
        }

        public static Dictionary<Chat, List<Message>> GetChatFromContract(long contract_id, int limit, int offset = 0)
        {
            using (var db = new ChatThreadsContext())
            {
                var results = new List<KeyValuePair<Chat, List<Message>>>();

                var chats_table = db.Database.GetCollection<Chat>("Chats");
                var messages_table = db.Database.GetCollection<Message>("Messages");
                
                foreach(var message in messages_table.Find(x => x.ContractID == contract_id && x.Id > offset).OrderBy(x => x.Id).Take(limit))
                {
                    var existing = results.Find(x => x.Key.Id == message.ChatId);
                    if (existing.Key == null)
                    {
                        var chat = chats_table.FindOne(x => x.Id == message.ChatId);
                        existing = new KeyValuePair<Chat, List<Message>>(chat, new List<Message>());
                        results.Add(existing);
                    }

                    existing.Value.Add(message);
                }

                return results.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public static List<Chat> GetAll(int limit, int offset = 0)
        {
            using (var db = new ChatThreadsContext())
            {
                var results = new List<Chat>();

                var chats_table = db.Database.GetCollection<Chat>("Chats");
                var chats = chats_table.Find(x => x.Id > offset).OrderByDescending(x => x.Id).Take(limit);

                return chats.ToList();
            }
        }

        public static Chat SendFromHandleIdent(string ident, Message message)
        {
            var handle = Handle.GetFromIdent(ident);
            Chat chat = Open(new List<int>() { handle.Id });

            message.Handle = handle.Id;
            message.ChatId = chat.Id;
            message.Store(true);

            return chat;
        }
        
        public static Chat MarkAsRead(int id, DateTime date)
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Chat>("Chats");
                var chat = table.FindOne(x => x.Id == id);
                chat.ReadAtDate = date.Ticks;
                chat.Store();

                return chat;
            }
        }
    }
}
