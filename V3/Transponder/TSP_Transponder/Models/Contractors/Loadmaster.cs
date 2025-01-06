using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Transactor;

namespace TSP_Transponder.Models.Contractors
{
    class Loadmaster
    {
        public static DateTime? GetLastPay()
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection("contractor_loadmaster");
                if(DBCollection.Count() > 0)
                {
                    var last_invoice = DBCollection.Find(Query.All(Query.Ascending)).Last();
                    if (last_invoice != null)
                    {
                        return (DateTime)last_invoice["Date"];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static void Pay(DateTime time, float? amount)
        {
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection("contractor_loadmaster");
                DBCollection.Insert(new BsonDocument()
                {
                    ["Date"] = DateTime.UtcNow,
                    ["Amount"] = amount != null ? amount : 0,
                });
            }
        }

        public static void InvoicePaid(Invoice invoice)
        {
            var fees = invoice.Fees.FindAll(x => x.Code == "staff_loadmaster");
            foreach(var fee in fees)
            {
                Pay((DateTime)invoice.PaidDate, fee.Amount);
            }
        }
    }
}
