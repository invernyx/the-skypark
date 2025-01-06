using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Models.Airports.AirportsLib;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Airports
{
    class AirportFilters
    {
        public class FilterRunways
        {
            public IEnumerable<Airport> List;
            public int? CountMin;
            public int? CountMax;
            public float? LengthMin = 0;
            public float? LengthMax = 999999;
            public string RwySurface = null;
            public bool? RequiresLight = null;
            public bool? RequiresILS = null;

            public IEnumerable<Airport> Get()
            {
                int Index = 0;
                IEnumerable<Airport> Filtered = List.Where(Apt =>
                {
                    Index++;
                    if (Apt == null)
                    {
                        return true;
                    }

                    if (CountMin != null ? CountMin > Apt.Runways.Count : false || CountMax != null ? CountMax < Apt.Runways.Count : false)
                    {
                        return false;
                    }

                    bool FoundLit = RequiresLight == null ? true : (bool)!RequiresLight;
                    bool FoundILS = (RequiresILS == null ? true : (bool)!RequiresILS);
                    int RunwayCount = 0;
                    int MatchScore = 0;
                    RunwayCount += Apt.Runways.Count;
                    foreach (var Rwy in Apt.Runways)
                    {
                        if (Rwy.CenterLight > 0 || Rwy.EdgeLight > 0)
                        {
                            FoundLit = true;
                        }

                        if (Rwy.PrimaryILS || Rwy.SecondaryILS)
                        {
                            FoundILS = true;
                        }

                        if (LengthMin != null ? Rwy.LengthFT > LengthMin : true && LengthMax != null ? Rwy.LengthFT < LengthMax : true)
                        {
                            if (RwySurface != null)
                            {
                                #region Match Rwy Type
                                switch (RwySurface)
                                {
                                    case "any":
                                        {
                                            if (Rwy.Surface != Surface.Water)
                                            {
                                                MatchScore += 1;
                                            }
                                            break;
                                        }
                                    case "hard":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Asphalt:
                                                case Surface.Concrete:
                                                case Surface.Tarmac:
                                                case Surface.Ice:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "soft":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Grass:
                                                case Surface.Gravel:
                                                case Surface.Sand:
                                                case Surface.Bituminous:
                                                case Surface.Clay:
                                                case Surface.Dirt:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "dirt":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Dirt:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "gravel":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Gravel:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "grass":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Grass:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "water":
                                        {
                                            switch (Rwy.Surface)
                                            {
                                                case Surface.Water:
                                                    {
                                                        MatchScore += 1;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }
                                #endregion
                            }
                            else
                            {
                                MatchScore += 1;
                            }
                        }
                    }

                    if(Index == 1)
                    {
                        FoundILS = true;
                    }

                    return MatchScore == RunwayCount && FoundLit && FoundILS;
                });
                
                return Filtered;
            }
        }

    }
}
