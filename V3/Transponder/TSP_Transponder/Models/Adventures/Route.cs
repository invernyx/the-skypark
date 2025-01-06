using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;

namespace TSP_Transponder.Models.Adventures
{
    public class Route
    {
        public string RouteString = "";
        public string RouteCode = "";
        public ushort DistanceKM = 0;
        public AdventureTemplate Template = null;
        public List<RouteSituation> Situations = new List<RouteSituation>();
        public List<sbyte> RecommendedAircraft = null;
        public Dictionary<string, dynamic> Parameters = null;

        public Route(AdventureTemplate Template)
        {
            this.Template = Template;
        }

        public Route(AdventureTemplate Template, List<int> Sits, string Import)
        {
            this.Template = Template;
            string[] ImportSpl = Import.Split('|');

            #region Route Code
            RouteCode = ImportSpl[0];
            #endregion

            #region Route
            string[] RouteSpl = ImportSpl[1].Split(':');
            int i = 0;
            foreach (string RouteStr in RouteSpl)
            {
                RouteSituation rs = new RouteSituation()
                {
                    UID = (uint)Sits[i],
                };

                if (RouteStr.Contains(','))
                {
                    string[] locSpl = RouteStr.Split(',');
                    
                    rs.Location = new GeoLoc(Convert.ToDouble(locSpl[0]), Convert.ToDouble(locSpl[1]));
                    rs.LocationName = locSpl.Length > 2 ? locSpl[2] : "";
                }
                else
                {
                    rs.Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO(RouteStr);
                    rs.Location = rs.Airport.Location;
                }

                Situations.Add(rs);
                i++;
            }
            #endregion

            #region Dist
            DistanceKM = (ushort)Math.Round(Convert.ToSingle(ImportSpl[2]));
            #endregion

            #region RecommendedAircraft
            RecommendedAircraft = new List<sbyte>();
            var recacfspl = ImportSpl[3].Split(':');
            foreach (var v in recacfspl)
            {
                RecommendedAircraft.Add((sbyte)Convert.ToInt16(v));
            }
            #endregion

            CalculateString();
            CalculateDistance();
        }

        public void CalculateString()
        {
            foreach (RouteSituation Sit in Situations)
            {
                if (Sit.Airport != null)
                {
                    RouteString += Sit.Airport.ICAO + ":";
                }
                else
                {
                    RouteString += Template.SituationLabels[Situations.IndexOf(Sit)] + ":";
                }
            }
        }

        public void CalculateDistance()
        {
            DistanceKM = 0;
            double TDist = 0;
            RouteSituation PrevSit = null;
            foreach (RouteSituation Sit in Situations)
            {
                if (PrevSit != null)
                {
                    PrevSit.DistToNextKM = (ushort)Math.Round(Utils.MapCalcDist(PrevSit.Location, Sit.Location, Utils.DistanceUnit.Kilometers, true));
                    TDist += PrevSit.DistToNextKM;
                }
                PrevSit = Sit;
            }
            DistanceKM = (ushort)Math.Round(TDist);
        }

        public void CreateRouteCode()
        {
            int hash = Math.Abs(RouteString.GetHashCode());

            if (hash < 148503455)
            {
                hash += 148503455;
            }

            RouteCode = hash.ToString("X");
            RouteCode = RouteCode.Substring(RouteCode.Length - 5);

        }

        public List<List<double>> ToSummary(int decimals = 15)
        {
            return Situations.Select(x => x.Location.ToList(decimals)).ToList();
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
                {
                    { "RouteString", RouteString },
                    { "RouteCode", RouteCode },
                    { "DistanceKM", DistanceKM },
                    { "Template", Template.FileName },
                    { "Situations", Situations.Select(x => x.Export()) },
                    { "RecommendedAircraft", RecommendedAircraft },
                };

            return rt;
        }

        public string Export()
        {
            List<string> ns = new List<string>
                {
                    RouteCode, // 0
                    string.Join(":", Situations.Select(x => x.Export())), // 1
                    DistanceKM.ToString(), // 2
                    string.Join(":", RecommendedAircraft), // 3
                };

            return string.Join("|", ns);
        }

        public class RouteSituation
        {
            public uint UID = 0;

            public Airport Airport = null;
            public GeoLoc Location = null;
            public ushort DistToNextKM = 0;
            public string LocationName = null;

            public string Export()
            {
                return Airport != null ? Airport.ICAO : (Location.ToString() + (LocationName != null ? "," + LocationName.Replace(":", " ").Replace("|", " ").Replace(",", "") : ""));
            }
        }

    }
}
