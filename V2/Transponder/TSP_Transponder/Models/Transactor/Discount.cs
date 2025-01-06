using LiteDB;
using System.Collections.Generic;
using static TSP_Transponder.Models.Transactor.Invoice;

namespace TSP_Transponder.Models.Transactor
{
    public class Discount
    {
        [BsonField("Code")]
        public string Code { get; set; } = "";
        [BsonField("Amount")]
        public float? Amount { get; set; } = 0;
        [BsonField("Params")]
        public Dictionary<string, dynamic> Params { get; set; } = null;

        public Discount()
        {

        }

        public Discount(Dictionary<string, dynamic> Restore)
        {
            Code = (string)Restore["Code"];
            Amount = (float?)Restore["Amount"];
            Params = Restore.ContainsKey("Params") ? (Dictionary<string, dynamic>)Restore["Params"] : null;
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Code", Code },
                { "Amount", Amount }
            };

            if (Params != null)
            {
                rs.Add("Params", Params);
            }

            return rs;
        }
    }
}
