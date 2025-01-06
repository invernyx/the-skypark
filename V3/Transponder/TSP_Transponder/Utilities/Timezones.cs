using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Utilities
{
    class Timezones
    {
        // https://github.com/treyerl/timezones
        private static Dictionary<float, List<List<GeoLoc>>> TimezonesGeometry = null;

        public static void Startup()
        {
            LoadTimezones();
        }

        public static Dictionary<float, List<List<GeoLoc>>> LoadTimezones()
        {
            if(TimezonesGeometry == null)
            {
                TimezonesGeometry = new Dictionary<float, List<List<GeoLoc>>>();
                string TimezonesJSON = App.ReadResourceFile("TSP_Transponder.Utilities.Timezones.timezones.json");
                Dictionary<string, dynamic> TimezonesRaw = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(TimezonesJSON);

                foreach (var Timezone in TimezonesRaw["features"])
                {
                    //Timezone.Add("GeoPolys", new List<List<List<GeoLoc>>>());

                    if (Timezone["geometry"] == null)
                        continue;

                    var offset = Convert.ToSingle(Timezone["properties"]["name"]);

                    List<List<GeoLoc>> PG = new List<List<GeoLoc>>();
                    TimezonesGeometry.Add(offset, PG);
                    //Timezone["GeoPolys"].Add(PG);

                    switch (Timezone["geometry"]["type"])
                    {
                        case "Polygon":
                            {
                                foreach (var PolyGroup in Timezone["geometry"]["coordinates"])
                                {
                                    List<GeoLoc> NP = new List<GeoLoc>();
                                    PG.Add(NP);
                                    foreach (var Poly in PolyGroup)
                                    {
                                        NP.Add(new GeoLoc(Convert.ToDouble(Poly[0]), Convert.ToDouble(Poly[1])));
                                    }
                                }
                                break;
                            }
                        case "MultiPolygon":
                            {
                                foreach (var PolyGroup in Timezone["geometry"]["coordinates"])
                                {
                                    foreach (var Poly in PolyGroup)
                                    {
                                        List<GeoLoc> NP = new List<GeoLoc>();
                                        PG.Add(NP);
                                        foreach (var Node in Poly)
                                        {
                                            NP.Add(new GeoLoc(Convert.ToDouble(Node[0]), Convert.ToDouble(Node[1])));
                                        }
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                
            }

            return TimezonesGeometry;
        }

        public static Timezone GetTimezone(GeoLoc Location)
        {
            Dictionary<float, List<List<GeoLoc>>> Timezones = LoadTimezones();

            foreach (var Timezone in Timezones)
            {
                foreach (var PolyGroup in Timezone.Value)
                {
                    if (Utils.IsPointInPoly(PolyGroup, (GeoLoc)Location))
                    {
                        return new Timezone() { Offset = Timezone.Key };
                    }
                }
            }
            
            return null;
        }

        public class Timezone
        {
            public float Offset = 0f;
        }
    }
}
