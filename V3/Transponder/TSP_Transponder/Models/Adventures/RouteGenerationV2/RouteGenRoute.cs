using System;
using System.Linq;
using System.Collections.Generic;
using static TSP_Transponder.Models.Adventures.RouteGeneration.RouteGenerator;
using static TSP_Transponder.Models.Connectors.SimConnection;
using TSP_Transponder.Models.Airports;

namespace TSP_Transponder.Models.Adventures.RouteGenerationV2
{
    internal class RouteGenRoute
    {
        int Attempts = 0;
        RouteGen Generator = null;

        internal string Hash = "";
        internal float Distance = 0;
        internal int RouteSeedIndex = 0;
        internal bool IsImpossible = false;
        internal List<RouteGenSituation> Situations = new List<RouteGenSituation>();

        internal double? RouteHeading = null;

        internal List<sbyte> RecommendedAircraft = new List<sbyte>() { 0, 0, 0, 0, 0, 0 };

        internal RouteGenRoute(RouteGen Generator)
        {
            this.Generator = Generator;

            // Copy source situations for this route
            foreach (var Situation in Generator.SourceSituations)
            {
                var new_situation = Situation.Copy();
                new_situation.Route = this;

                Situations.Add(new_situation);
            }

            // Init all situations
            foreach (var situation in Situations)
                situation.Init();

            // Set the seed index. This is the index of the situation where the route generation will begin
            if(Generator.GenSeedIndex != null)
            {
                RouteSeedIndex = (int)Generator.GenSeedIndex;
            }
            else
            {
                lock(Generator.SuccessIndex)
                {
                    int sucessfuls = Generator.SuccessIndex.Sum(x => x.FindAll(x1 => x1 == true).Count);
                    if (sucessfuls > 30)
                    {
                        var sums = Generator.SuccessIndex.Select(x => x.FindAll(x1 => x1 == true).Count).ToList();
                        RouteSeedIndex = sums.IndexOf(sums.Max());

                        //WeightedRandom<int> wr = new WeightedRandom<int>();
                        //foreach (var SituationIndex in Generator.SuccessIndex)
                        //{
                        //    wr.AddEntry(Generator.SuccessIndex.IndexOf(SituationIndex), SituationIndex.FindAll(x1 => x1 == true).Count);
                        //}
                        //RouteSeedIndex = wr.GetRandom();

                        Generator.SuccessIndex.Clear();
                        Generator.GenSeedIndex = RouteSeedIndex;
                    }
                    else
                    {
                        RouteSeedIndex = Utils.GetRandom(Situations.Count);
                    }
                }
            }
        }

        internal bool Find()
        {
            // Loop until we find everything
            int max_pass = 1;
            for (var i = 0; i <= max_pass; i++)
            {
                // Go thought all situations for this index
                foreach (var Situation in Situations.Where(x => x.ProcessOnLoopIndex == i))
                {
                    Situation.Find(i);

                    // if IsImpossible was set to true, kill the process
                    if (IsImpossible)
                    {
                        UpdateSuccessIndex(false);
                        return false;
                    }
                }

                // Find the highest index in all situations and update the max pass var
                max_pass = (int)Situations.Max(x => x.ProcessOnLoopIndex);

                // If we go above 20 iterations, kill the process
                if (max_pass > 20)
                {
                    UpdateSuccessIndex(false);
                    IsImpossible = true;
                    return false;
                }
            }

            // Calculate distance
            Distance = Situations.Sum(x =>
            {
                if (x.Previous != null)
                    return Utils.MapCalcDistFloat((float)x.Previous.Location.Lat, (float)x.Previous.Location.Lon, (float)x.Location.Lat, (float)x.Location.Lon);

                return 0;
            });

            // Generate recommended aircraft
            if(Generator.Template.AircraftRecommendation == null)
            {
                RecommendedAircraft = GenerateRecommendedAircraft();
                if (RecommendedAircraft.Max() < 10)
                {
                    UpdateSuccessIndex(false);
                    IsImpossible = true;
                    return false;
                }
            }
            else
                RecommendedAircraft = Generator.Template.AircraftRecommendation.ConvertAll(i => (sbyte)i);

            // Generate the Hash identifier for this route. Allows us to prevent duplicates
            Hash = String.Join("_", Situations.Select(x => x.ToHash()));

            // Check if we don't already have the same route using generated hash
            lock(Generator.Routes)
                if (Generator.Routes.Find(x => x.Hash == Hash) != null) { return false; }

            // Keep a record of where we started
            UpdateSuccessIndex(true);

            return true;
        }

        // Find heading average
        public double? GetAvgHdg()
        {
            if(RouteHeading == null)
            {
                var coordinates = Situations.Where(x => x.Location != null).Select(x => x.Location).Take(2).ToList();

                int count = coordinates.Count;
                if (count > 0)
                {
                    double sumX = 0;
                    double sumY = 0;
                    double sumZ = 0;

                    for (int i = 0; i < count; i++)
                    {
                        double lat1 = coordinates[i].Lat * Math.PI / 180;
                        double lon1 = coordinates[i].Lon * Math.PI / 180;

                        sumX += Math.Cos(lat1) * Math.Cos(lon1);
                        sumY += Math.Cos(lat1) * Math.Sin(lon1);
                        sumZ += Math.Sin(lat1);
                    }

                    double avgX = sumX / count;
                    double avgY = sumY / count;
                    double avgZ = sumZ / count;

                    double lon = Math.Atan2(avgY, avgX);
                    double hyp = Math.Sqrt(avgX * avgX + avgY * avgY);
                    double lat = Math.Atan2(avgZ, hyp);

                    double bearing = lon * 180 / Math.PI;
                    if (bearing < 0)
                    {
                        bearing += 360;
                    }

                    RouteHeading = bearing;
                }
            }

            return RouteHeading;
        }

        // Updates the Sucess Index. Allows the route generator to choose the most efficient start point for the route.
        private void UpdateSuccessIndex(bool state)
        {
            if (Generator.GenSeedIndex == null)
            {
                lock(Generator.SuccessIndex)
                {
                    if(Generator.SuccessIndex.Count > 0)
                    {
                        Generator.SuccessIndex[RouteSeedIndex].Add(state);
                    }
                }
            }
        }

        internal List<sbyte> GenerateRecommendedAircraft()
        {
            RecommendedAircraft = new List<sbyte>() { 0, 0, 0, 0, 0, 0 };

            try
            {
                // Helis
                // GA
                // Turboprop
                // Small jets
                // Narrow-body
                // Wide-body
                int segments = 6;
                int count = Situations.Count;

                // Calculators
                Func<float, float, float, float> CalcFromTarget = (TippingPoint, Spread, Value) =>
                {
                    float Dif = -Math.Abs(Value - TippingPoint);
                    float Multiplier = (100 / Spread) * 2;

                    return 100 + (Dif * Multiplier);
                };
                Func<float, float, float, float> CalcRelevance = (TippingPoint, Spread, Value) =>
                {
                    float Dif = Value - TippingPoint;
                    float Multiplier = (100 / Spread);

                    return Dif * Multiplier;
                };

                List<Dictionary<string, float?>> Tracking = new List<Dictionary<string, float?>>();

                while (Tracking.Count < segments)
                {
                    Tracking.Add(new Dictionary<string, float?>()
                        {
                            { "Distance", 0 },
                            { "Surface", 0 },
                            { "Length", 0 },
                            { "Parking", 0 },
                            { "Elevation", 0 },
                        });
                }

                float DistToNextKM = 0;
                foreach (var Sit in Situations)
                {
                    int Index = Situations.IndexOf(Sit);

                    if (Sit.Airport != null)
                    {
                        int Longest = (Sit.Airport.Runways.Select(x => x.LengthFT).Max());

                        if (Index < count - 1)
                            DistToNextKM = (float)Utils.MapCalcDist(Sit.Location, Sit.Next.Location, Utils.DistanceUnit.Kilometers, true);

                        if (DistToNextKM != 0)
                        {
                            float DistToNext = DistToNextKM * 0.539957f;
                            Tracking[0]["Distance"] = Tracking[0]["Distance"] != null ? Tracking[0]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(0, 80, DistToNext)) / (count - 1)) : null; // Heli
                            Tracking[1]["Distance"] = Tracking[1]["Distance"] != null ? Tracking[1]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(0, 800, DistToNext)) / (count - 1)) : null; // Cessna
                            Tracking[2]["Distance"] = Tracking[2]["Distance"] != null ? Tracking[2]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(500, 1200, DistToNext)) / (count - 1)) : null; // Turboprop
                            Tracking[3]["Distance"] = Tracking[3]["Distance"] != null ? Tracking[3]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(1500, 4000, DistToNext)) / (count - 1)) : null; // Jet
                            Tracking[4]["Distance"] = Tracking[4]["Distance"] != null ? Tracking[4]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(1500, 4000, DistToNext)) / (count - 1)) : null; // Narrow
                            Tracking[5]["Distance"] = Tracking[5]["Distance"] != null ? Tracking[5]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(4000, 8000, DistToNext)) / (count - 1)) : null; // Wide
                        }

                        Tracking[0]["Length"] = Tracking[0]["Length"] != null ? Tracking[0]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(300, 6000, Longest)) / count) : null;
                        Tracking[1]["Length"] = Tracking[1]["Length"] != null ? Tracking[1]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(600, 4000, Longest)) / count) : null;
                        Tracking[2]["Length"] = Tracking[2]["Length"] != null && Longest > 2000 ? Tracking[2]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(2000, 800, Longest)) / count) : null;
                        Tracking[3]["Length"] = Tracking[3]["Length"] != null && Longest > 3500 ? Tracking[3]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(3500, 1000, Longest)) / count) : null;
                        Tracking[4]["Length"] = Tracking[4]["Length"] != null && Longest > 5000 ? Tracking[4]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(6000, 1100, Longest)) / count) : null;
                        Tracking[5]["Length"] = Tracking[5]["Length"] != null && Longest > 7000 ? Tracking[5]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(7000, 1100, Longest)) / count) : null;

                        Tracking[0]["Parking"] = Tracking[0]["Parking"] != null ? 100 : (float?)null;
                        Tracking[1]["Parking"] = Tracking[1]["Parking"] != null ? Tracking[1]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 5).Count * 100)) / count) : null;
                        Tracking[2]["Parking"] = Tracking[2]["Parking"] != null ? Tracking[2]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 7).Count * 100)) / count) : null;
                        Tracking[3]["Parking"] = Tracking[3]["Parking"] != null ? Tracking[3]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 12).Count * 100)) / count) : null;
                        Tracking[4]["Parking"] = Tracking[4]["Parking"] != null ? Tracking[4]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 50).Count * 100)) / count) : null;
                        Tracking[5]["Parking"] = Tracking[5]["Parking"] != null ? Tracking[5]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 55).Count * 100)) / count) : null;

                        Tracking[0]["Elevation"] = Tracking[0]["Elevation"] != null ? Tracking[0]["Elevation"] - (CalcRelevance(3000, 15000, Sit.Airport.Elevation) / count) : null;
                        Tracking[1]["Elevation"] = Tracking[1]["Elevation"] != null ? Tracking[1]["Elevation"] - (CalcRelevance(2000, 10000, Sit.Airport.Elevation) / count) : null;
                        Tracking[2]["Elevation"] = Tracking[2]["Elevation"] != null ? Tracking[2]["Elevation"] - (CalcRelevance(4000, 10000, Sit.Airport.Elevation) / count) : null;
                        Tracking[3]["Elevation"] = Tracking[3]["Elevation"] != null ? Tracking[3]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / count) : null;
                        Tracking[4]["Elevation"] = Tracking[4]["Elevation"] != null ? Tracking[4]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / count) : null;
                        Tracking[5]["Elevation"] = Tracking[5]["Elevation"] != null ? Tracking[5]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / count) : null;

                        bool IsSoft = Sit.Airport.Runways.Select(x => x.Surface).ToList().Where(x =>
                        {
                            return x == Surface.Asphalt
                            || x == Surface.Concrete
                            || x == Surface.Bituminous
                            || x == Surface.Tarmac;
                        }).Count() == 0;

                        if (IsSoft)
                        {
                            // Skip Heli
                            Tracking[1]["Surface"] = Tracking[1]["Surface"] != null ? Tracking[1]["Surface"] + (IsSoft ? 10 : 0 / count) : null;
                            Tracking[2]["Surface"] = Tracking[2]["Surface"] != null ? Tracking[2]["Surface"] + (IsSoft ? 5 : 0 / count) : null;
                            Tracking[3]["Surface"] = Tracking[3]["Surface"] != null ? Tracking[3]["Surface"] + (IsSoft ? -50 : 0 / count) : null;
                            Tracking[4]["Surface"] = Tracking[4]["Surface"] != null ? Tracking[4]["Surface"] + (IsSoft ? -80 : 0 / count) : null;
                            Tracking[5]["Surface"] = Tracking[5]["Surface"] != null ? Tracking[5]["Surface"] + (IsSoft ? -100 : 0 / count) : null;
                        }
                    }
                }

                int index = 0;
                foreach (var Track in Tracking)
                {
                    if (!Track.Values.Contains(null))
                    {
                        RecommendedAircraft[index] = (sbyte)Utils.Limiter(-100, 100, (double)((Tracking[index]["Distance"] * 0.5f) + (Tracking[index]["Length"] * 0.5f) + (Tracking[index]["Parking"] * 0.1f) + (Tracking[index]["Elevation"] * 0.2f) + (Tracking[index]["Surface"] * 0.2f)));
                    }
                    else
                    {
                        RecommendedAircraft[index] = -100;
                    }
                    index++;
                }

                sbyte maxValue = RecommendedAircraft.Max();
                if (maxValue > 10)
                {
                    int maxIndex = RecommendedAircraft.IndexOf(maxValue);
                    RecommendedAircraft[maxIndex] = maxValue > 71 ? maxValue : (sbyte)71;
                }
            }
            catch
            {

            }

            return RecommendedAircraft;
        }

    }
}
