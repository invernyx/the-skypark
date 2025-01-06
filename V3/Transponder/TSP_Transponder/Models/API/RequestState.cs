using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.App;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.API
{
    public class RequestState
    {
        public STATUS Status = STATUS.FAILED;
        public long? ReferenceID;
        public string Reason = "Unknown";
        public dynamic Data = null;

        public enum STATUS
        {
            [EnumValue("SUCCESS")]
            SUCCESS,
            [EnumValue("FAILED")]
            FAILED
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            var response = new Dictionary<string, dynamic>()
            {
                { "status", GetDescription(Status) },
                { "reference_id", ReferenceID },
                { "reason", Reason },
                { "data", Data },
            };
            return response;
        }
    }
}
