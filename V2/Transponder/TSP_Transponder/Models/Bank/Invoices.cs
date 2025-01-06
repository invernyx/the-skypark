using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models.Transactor
{
    public class Invoices
    {
        public static List<Invoice> GetAll()
        {
            return new List<Invoice>();
        }

        public static List<Invoice> GetAllFromID(long ID)
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection<Invoice>("invoices_user");
                var Results = DBCollection.Find(x => x.Link == ID);
                return Results.ToList();
            }
        }

        public static Invoice GetLastFromTitle(string title)
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection<Invoice>("invoices_user");
                return DBCollection.Find(Query.In("Title", title)).OrderBy(x => x.Date).Last();
            }
        }

        public static bool Upsert(Invoice newInvoice)
        {
            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection<Invoice>("invoices_user");
                    DBCollection.Upsert(newInvoice);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
