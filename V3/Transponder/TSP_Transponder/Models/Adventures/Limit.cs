using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures
{
    public class Limit
    {
        private Adventure Adv = null;
        public int UID = 0;
        public bool Visible = false;
        public bool IsPreStart = false;
        public bool InSummary = true;
        public dynamic[] Params = null;
        public string Label = "";
        public string Type = "";

        private bool _Enabled = false;
        public bool Enabled {
            get
            {
                return _Enabled;
            }
            set
            {
                if(_Enabled != value)
                {
                    _Enabled = value;
                    if(Visible)
                    {
                        Adv.ScheduleLimitsBroadcast = true;
                    }
                }
            }
        }

        public Limit(int UID, Adventure Adv)
        {
            this.UID = UID;
            this.Adv = Adv;
        }

        public Dictionary<string, dynamic> GetStruct()
        {
            return new Dictionary<string, dynamic>()
            {
                { "uid", UID },
                { "enabled", Enabled },
                { "is_prestart", IsPreStart },
                { "in_summary", InSummary },
                { "type", Type },
                { "label", Label },
                { "params", Params },
            };
        }
    }
}
