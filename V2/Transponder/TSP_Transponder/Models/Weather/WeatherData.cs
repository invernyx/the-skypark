using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.App;

namespace TSP_Transponder.Models.WeatherModel
{
    public class WeatherData
    {
        public List<CloudLayer> Clouds = new List<CloudLayer>();
        public Precipitation_Types Precipitation = Precipitation_Types.NO;
        public float IsNearby = 0;
        public string Station = "";
        public DateTime Time = DateTime.UtcNow;
        public short Precipitation_Rate = 0;
        public bool Thunderstorm = false;
        public float Temperature = 0;
        public float DewPoint = 0;
        public float VisibilitySM = 0;
        public float Altimeter = 0;

        public float WindSpeed = 0;
        public float WindGust = 0;
        public float WindHeading = 0;
        
        public enum Precipitation_Amount
        {
            [EnumValue("None")]
            None,
            [EnumValue("Light")]
            Light,
            [EnumValue("Moderate")]
            Moderate,
            [EnumValue("Heavy")]
            Heavy
        }

        public enum Precipitation_Types
        {
            [EnumValue("NO")]
            NO,
            [EnumValue("DZ")]
            DZ,
            [EnumValue("RA")]
            RA,
            [EnumValue("SN")]
            SN,
            [EnumValue("SG")]
            SG,
            [EnumValue("IC")]
            IC,
            [EnumValue("PE")]
            PE,
            [EnumValue("BR")]
            BR,
            [EnumValue("PY")]
            PY
        }

        public enum Cloud_Types
        {
            [EnumValue("None")]
            None,
            [EnumValue("CI")]
            CI,
            [EnumValue("CS")]
            CS,
            [EnumValue("CC")]
            CC,
            [EnumValue("AS")]
            AS,
            [EnumValue("AC")]
            AC,
            [EnumValue("SC")]
            SC,
            [EnumValue("NS")]
            NS,
            [EnumValue("ST")]
            ST,
            [EnumValue("CU")]
            CU,
            [EnumValue("CB")]
            CB,
        }

        public enum Cloud_Coverage
        {
            [EnumValue("CLR")]
            CLR,
            [EnumValue("FEW")]
            FEW,
            [EnumValue("SCT")]
            SCT,
            [EnumValue("BKN")]
            BKN,
            [EnumValue("OVC")]
            OVC,
            [EnumValue("OVX")]
            OVX
        }

        public class CloudLayer
        {
            public Cloud_Types Type = Cloud_Types.None;
            public Cloud_Coverage Coverage = Cloud_Coverage.CLR;
            public int Base = 0;

        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Station", Station },
                { "Altimeter", Altimeter },
                { "Visibility", VisibilitySM },
                { "WindSpeed", WindSpeed },
                { "WindGust", WindGust },
                { "WindHeading", WindHeading },
                { "IsNearby", Math.Round(IsNearby) }
            };

            //if (Completed == true)
            //{
            //    ns.Add("Completed", true);
            //}

            return ns;
        }
    }
    
}
