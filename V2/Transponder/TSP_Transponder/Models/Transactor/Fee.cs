using LiteDB;
using System.Collections.Generic;
using System.Linq;
using static TSP_Transponder.Models.Transactor.Invoice;

namespace TSP_Transponder.Models.Transactor
{
    public class Fee
    {
        [BsonField("Code")]
        public string Code { get; set; } = "";
        [BsonField("Amount")]
        public float? Amount { get; set; } = 0;
        [BsonField("ExclIf")]
        public string ExclIf { get; set; } = null;
        [BsonField("InclIf")]
        public string InclIf { get; set; } = null;
        [BsonField("Required")]
        public bool Required { get; set; } = false;
        [BsonField("Refunded")]
        public bool? Refunded { get; set; } = null;
        [BsonField("RefundMoment")]
        public MOMENT RefundMoment { get; set; } = MOMENT.NONE;
        [BsonField("Params")]
        public Dictionary<string, dynamic> Params { get; set; } = null;
        [BsonField("Discounts")]
        public List<Discount> Discounts { get; set; } = null;

        public Fee()
        {

        }

        public Fee(Dictionary<string, dynamic> Restore)
        {
            Code = (string)Restore["Code"];
            Amount = (float?)Restore["Amount"];
            RefundMoment = Restore.ContainsKey("RefundMoment") ? (MOMENT)App.GetEnum(typeof(MOMENT), (string)Restore["RefundMoment"]) : MOMENT.NONE;
            ExclIf = Restore.ContainsKey("ExclIf") ? (string)Restore["ExclIf"] : null;
            InclIf = Restore.ContainsKey("InclIf") ? (string)Restore["InclIf"] : null;
            Required = Restore.ContainsKey("Required") ? (bool)Restore["Required"] : false;
            Refunded = Restore.ContainsKey("Refunded") ? (bool)Restore["Refunded"] : (bool?)null;
            Params = Restore.ContainsKey("Params") ? (Dictionary<string, dynamic>)Restore["Params"] : null;

            if(Restore.ContainsKey("Discounts"))
            {
                Discounts = new List<Discount>();
                foreach(var discount in (List<Dictionary<string, dynamic>>)Restore["Discounts"])
                {
                    Discounts.Add(new Discount(discount));
                }
            }
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Code", Code },
                { "Amount", Amount }
            };

            if (RefundMoment !=  MOMENT.NONE)
            {
                rs.Add("RefundMoment", App.GetDescription(RefundMoment));
            }

            if (Params != null)
            {
                rs.Add("Params", Params);
            }

            if (InclIf != null)
            {
                rs.Add("InclIf", InclIf);
            }

            if (ExclIf != null)
            {
                rs.Add("ExclIf", ExclIf);
            }

            if (Refunded != null)
            {
                rs.Add("Refunded", Refunded);
            }

            if (Required)
            {
                rs.Add("Required", Required);
            }

            if (Discounts != null)
            {
                rs.Add("Discounts", Discounts.Select(x => x.ToDictionary()).ToList());
            }

            return rs;
        }
    }
}
