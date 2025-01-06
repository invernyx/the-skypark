using LiteDB;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Messaging
{
    public class Handle
    {
        [BsonField("_id")]
        [ClassSerializerField("id")]
        public int Id { get; set; }

        [BsonField("ident")]
        [ClassSerializerField("ident")]
        public string Ident { get; set; }

        [BsonField("first_name")]
        [ClassSerializerField("first_name")]
        public string FirstName { get; set; }

        [BsonField("last_name")]
        [ClassSerializerField("last_name")]
        public string LastName { get; set; }

        public void Store()
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Handle>("Handles");
                if (table.FindOne(x => x.Ident == Ident) == null)
                {
                    Id = table.Insert(this);
                }
                else
                {
                    table.Update(this);
                }
            }
        }

        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<Handle> cs = new ClassSerializer<Handle>(this, fields);
            cs.Generate(typeof(Handle), fields);

            var result = cs.Get();
            return result;
        }

        public override string ToString()
        {
            return Id + " - " + FirstName + " " + LastName + " (" + Ident + ")"; 
        }


        public static Handle Open(string ident, string first_name, string last_name)
        {
            var handle = GetFromIdent(ident);
            if (handle == null)
            {
                handle = new Handle()
                {
                    Ident = ident,
                    FirstName = first_name,
                    LastName = last_name
                };
                handle.Store();
            }
            return handle;
        }

        public static List<Handle> GetAll()
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Handle>("Handles");
                var handles = table.FindAll();
                return handles.ToList();
            }
        }

        public static Handle GetFromIdent(string ident)
        {
            using (var db = new ChatThreadsContext())
            {
                var table = db.Database.GetCollection<Handle>("Handles");
                var handle = table.FindOne(x => x.Ident == ident);
                if(handle == null)
                {
                    string[] identSplit = ident.Split('_');
                    handle = new Handle()
                    {
                        FirstName = char.ToUpper(identSplit[0][0]) + identSplit[0].Substring(1),
                        LastName = identSplit.Length > 1 ? char.ToUpper(identSplit[1][0]) + identSplit[1].Substring(1) : null,
                        Ident = ident
                    };
                    handle.Store();
                }
                return handle;
            }
        }
    }
}
