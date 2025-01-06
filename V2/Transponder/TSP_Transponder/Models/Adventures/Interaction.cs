﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures
{
    
    public class Interaction
    {
        public bool Visible = false;
        public bool Enabled = true;
        public bool Essential = true;
        public dynamic Data = null;
        public int SID = 0;
        public int UID = 0;
        public DateTime? Expire = null;
        public string Verb = "";
        public string Image = "";
        public string Style = "";
        public string Label = "";
        public string Description = "";

        public Action<Interaction, Dictionary<string, dynamic>> Triggered = null;

        public void Trigger(Dictionary<string, dynamic> data)
        {
            Triggered?.Invoke(this, data);
        }

        public Dictionary<string, dynamic> GetStruct()
        {
            return new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Enabled", Enabled },
                { "Essential", Essential },
                { "Verb", Verb },
                { "Style", Style },
                { "Image", Image },
                { "Label", Label },
                { "Expire", Expire != null ? ((DateTime)Expire).ToString("O") : null},
                { "Description", Description },
                { "Data", Data }
            };
        }
    }
}