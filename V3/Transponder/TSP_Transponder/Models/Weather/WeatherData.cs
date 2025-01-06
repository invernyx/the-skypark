using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.App;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.WeatherModel
{
    public class WeatherData
    {
        public List<CloudLayer> Clouds = new List<CloudLayer>();
        public Precipitation_Types Precipitation = Precipitation_Types.NO;
        [ClassSerializerField("is_nearby")]
        public float IsNearby = 0;
        [ClassSerializerField("station")]
        public string Station = "";
        public DateTime Time = DateTime.UtcNow;
        [ClassSerializerField("precipitation_rate")]
        public short Precipitation_Rate = 0;
        [ClassSerializerField("thunderstorm")]
        public bool Thunderstorm = false;
        [ClassSerializerField("temperature")]
        public short Temperature = 0;
        [ClassSerializerField("dew_point")]
        public short DewPoint = 0;
        [ClassSerializerField("visibility_sm")]
        public int VisibilitySM = 0;
        [ClassSerializerField("altimeter")]
        public float Altimeter = 0;

        [ClassSerializerField("wind_speed")]
        public short WindSpeed = 0;
        [ClassSerializerField("wind_gust")]
        public short WindGust = 0;
        [ClassSerializerField("wind_heading")]
        public short WindHeading = 0;
        
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
        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<WeatherData> cs = new ClassSerializer<WeatherData>(this, fields);
            cs.Generate(typeof(WeatherData), fields);

            cs.Get("precipitation", fields, (f) => GetDescription(Precipitation));

            var result = cs.Get();
            return result;
        }
    }
    
}
