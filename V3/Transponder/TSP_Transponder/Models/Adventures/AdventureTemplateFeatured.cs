using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models.Adventures
{
    class AdventureTemplateFeaturedService
    {
        internal static int IsFeatured(string TemplateName)
        {
            if (App.MW.IsShuttingDown)
            {
                return 1000;
            }

            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("templates_featured");
                    DBCollection.EnsureIndex("Name");
                    BsonDocument Entry = DBCollection.FindOne("$.Name = '" + TemplateName + "'");

                    if (Entry == null)
                    {
                        Entry = new BsonDocument
                        {
                            ["Name"] = TemplateName,
                            ["Date"] = DateTime.UtcNow,
                            ["Dismissed"] = false,
                        };
                        DBCollection.Insert(Entry);
                    }

                    DateTime LastSeen = Entry["Date"];
                    return !Entry["Dismissed"] ? (DateTime.UtcNow - LastSeen).Days : 1000;
                }
            }
            catch
            {
                return 1000;
            }
        }

        internal static void DismissFeatured(string TemplateFileName)
        {
            try
            {
                lock(LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("templates_featured");
                    BsonDocument Entry = DBCollection.FindOne("$.Name = '" + TemplateFileName + "'");
                    if (Entry != null)
                    {
                        if (!Entry["Dismissed"])
                        {
                            Entry["Dismissed"] = true;
                            DBCollection.Update(Entry);
                        }
                    }
                }
            }
            catch
            {

            }
        }

    }
}
