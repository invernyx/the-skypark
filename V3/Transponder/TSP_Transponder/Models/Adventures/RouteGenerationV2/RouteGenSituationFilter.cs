using System;
using LiteDB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.DataStore;
using static TSP_Transponder.Models.Adventures.RouteGeneration.RouteGenerator;
using static TSP_Transponder.Models.Connectors.SimConnection;
using Org.BouncyCastle.Asn1.IsisMtt.X509;

namespace TSP_Transponder.Models.Adventures.RouteGenerationV2
{
    class RouteGenSituationFilter
    {
        RouteGen Generator = null;
        RouteGenSituation Situation = null;

        public RouteSituationType SituationType;
        public List<string> SituationTypeParams;
        public Airport Airport;
        public GeoLoc Location;
        public string LocationName;
        public float Height;
        public float DistToNextMinKM;
        public float DistToNextMaxKM;
        public bool RequiresLight;
        public string Query = "";
        public string Country;
        public string SurfaceType;
        public int HeliMin;
        public int HeliMax;
        public int RwyMin;
        public int RwyMax;
        public int ParkMin;
        public int ParkMax;
        public int ParkWidMin;
        public int ParkWidMax;
        public int RwyLenMin;
        public int RwyLenMax;
        public int RwyWidMin;
        public int RwyWidMax;
        public int ElevMin;
        public int ElevMax;
        public uint DensityMin = 0;
        public uint DensityMax = uint.MaxValue;
        public uint ReliefMin = 0;
        public uint ReliefMax = uint.MaxValue;
        public List<GeoLoc> Boundaries = new List<GeoLoc>();
        public List<Airport> BaseAirports = null;

        public RouteGenSituationFilter(RouteGenSituation Situation, RouteGen Generator, Dictionary<string, dynamic> SituationDic)
        {
            this.Situation = Situation;
            this.Generator = Generator;

            if (SituationDic.ContainsKey("Height"))
            {
                Height = SituationDic["Height"];
            }

            if (SituationDic.ContainsKey("DistToNextMax"))
            {
                DistToNextMaxKM = Convert.ToSingle(SituationDic["DistToNextMax"]) * 1.852f;
            }

            if (SituationDic.ContainsKey("DistToNextMin"))
            {
                DistToNextMinKM = Convert.ToSingle(SituationDic["DistToNextMin"]) * 1.852f;
            }

            if (SituationDic.ContainsKey("Boundaries"))
            {
                foreach (var Bound in SituationDic["Boundaries"])
                {
                    Boundaries.Add(new GeoLoc(Convert.ToDouble(Bound[0]), Convert.ToDouble(Bound[1])));
                }
            }

            if (SituationDic.ContainsKey("Query"))
            {
                Query = SituationDic["Query"];
            }

            if (SituationDic.ContainsKey("RequireLights"))
            {
                RequiresLight = SituationDic["RequireLights"];
            }

            switch ((string)SituationDic["SituationType"])
            {
                case "Any":
                case "Country":
                    {
                        SurfaceType = SituationDic["Surface"];
                        HeliMin = SituationDic["HeliMin"];
                        HeliMax = SituationDic["HeliMax"];
                        RwyMin = SituationDic["RwyMin"];
                        RwyMax = SituationDic["RwyMax"];
                        ParkMin = SituationDic["ParkMin"];
                        ParkMax = SituationDic["ParkMax"];
                        ParkWidMin = SituationDic["ParkWidMin"];
                        ParkWidMax = SituationDic["ParkWidMax"];
                        RwyLenMin = SituationDic["RwyLenMin"];
                        RwyLenMax = SituationDic["RwyLenMax"];
                        RwyWidMin = SituationDic["RwyWidMin"];
                        RwyWidMax = SituationDic["RwyWidMax"];
                        ElevMin = SituationDic["ElevMin"];
                        ElevMax = SituationDic["ElevMax"];
                        DensityMin = SituationDic.ContainsKey("DensityMin") ? Convert.ToUInt32(SituationDic["DensityMin"]) : (uint)0;
                        DensityMax = SituationDic.ContainsKey("DensityMax") ? Convert.ToUInt32(SituationDic["DensityMax"]) : uint.MaxValue;
                        ReliefMin = SituationDic.ContainsKey("ReliefMin") ? Convert.ToUInt32(SituationDic["ReliefMin"]) : (uint)0;
                        ReliefMax = SituationDic.ContainsKey("ReliefMax") ? Convert.ToUInt32(SituationDic["ReliefMax"]) : uint.MaxValue;
                        break;
                    }
            }

            switch (SituationDic["SituationType"])
            {
                case "Any":
                    {
                        SituationType = RouteSituationType.Any;
                        break;
                    }
                case "Country":
                    {
                        SituationType = RouteSituationType.Country;
                        Country = (string)SituationDic["Country"];
                        break;
                    }
                case "ICAO":
                    {
                        SituationType = RouteSituationType.ICAO;
                        string[] ICAOs = ((string)SituationDic["ICAO"]).Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (ICAOs.Length > 1)
                        {
                            BaseAirports = new List<Airport>();
                            foreach (var ICAO in ICAOs)
                            {
                                BaseAirports.Add(SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO));
                            }
                        }
                        else
                        {
                            Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO((string)SituationDic["ICAO"]);
                            if (Airport != null)
                            {
                                Location = Airport.Location;
                            }
                        }
                        break;
                    }
                case "Geo":
                    {
                        SituationType = RouteSituationType.Geo;
                        Location = new GeoLoc(Convert.ToDouble(SituationDic["Lon"]), Convert.ToDouble(SituationDic["Lat"]));
                        break;
                    }
                case "Situation":
                    {
                        SituationType = RouteSituationType.Situation;
                        SituationTypeParams = ((ArrayList)SituationDic["SituationTypeParams"]).Cast<string>().ToList();
                        break;
                    }
                case "Location":
                    {
                        SituationType = RouteSituationType.Location;
                        SituationTypeParams = ((ArrayList)SituationDic["SituationTypeParams"]).Cast<string>().ToList();
                        break;
                    }

            }
        }



        internal List<BsonDocument> FilterLocations(int RouteSeedIndex, string Collection, Dictionary<RouteGenSituation, List<float>> Restrictions)
        {
            lock (LiteDbService.DBLocations)
            {
                // _id, lat, lon, area, attrs
                //var DBCollection = LiteDbService.DBLocations.Database.GetCollection(Collection);
                lock (Generator.LocationsLib)
                {
                    if (!Generator.LocationsLib.ContainsKey(Collection))
                        Generator.LocationsLib.Add(Collection, LiteDbService.DBLocations.Database.GetCollection(Collection).FindAll().ToList());
                }


                var count = Generator.LocationsLib[Collection].Count;
                var index = Utils.GetRandom(count);

                List<BsonDocument> locations = new List<BsonDocument>();
                IEnumerable<BsonDocument> locations_raw = null;

                foreach (var restriction in Restrictions.Where(x => x.Key != null ? (x.Key.Location != null) : false))
                {
                    GeoLoc North = Utils.MapOffsetPosition(restriction.Key.Location, restriction.Value[1], 0);
                    GeoLoc East = Utils.MapOffsetPosition(restriction.Key.Location, restriction.Value[1], 90);
                    GeoLoc South = Utils.MapOffsetPosition(restriction.Key.Location, restriction.Value[1], 180);
                    GeoLoc West = Utils.MapOffsetPosition(restriction.Key.Location, restriction.Value[1], 270);

                    if (locations_raw == null) 
                        locations_raw = Generator.LocationsLib[Collection].AsEnumerable();

                    locations_raw = locations_raw.Where(x => (x["lat"] > South.Lat && x["lat"] < North.Lat) && (x["lon"] < East.Lon && x["lon"] > West.Lon));
                    locations_raw = locations_raw.Where(x => Utils.MapCalcDistFloat(Convert.ToSingle((double)x["lat"]), Convert.ToSingle((double)x["lon"]), (float)restriction.Key.Location.Lat, (float)restriction.Key.Location.Lon, Utils.DistanceUnit.Meters) > restriction.Value[0]);
                }

                if (locations_raw == null && Situation.Index == RouteSeedIndex)
                    locations_raw = Generator.LocationsLib[Collection].AsEnumerable();

                if (locations_raw != null ? locations_raw.Count() > 0 : false)
                {
                    locations.Clear();
                    foreach (var location_raw in locations_raw)
                    {
                        locations.Add(location_raw);
                    }
                }

                return locations;
            }
        }

        internal IEnumerable<Airport> FilterAirportsMeta(IEnumerable<Airport> SelectedAirports)
        {
            // Template limits
            #region Filter for Template Countries
            if (Country != null)
            {
                SelectedAirports = SelectedAirports.Where(x =>
                {
                    return x.Country == Country;
                });
            }
            #endregion

            #region Filter from Template Search Query
            if (Query.Trim() != "")
            {
                SelectedAirports = SelectedAirports.Where(x =>
                {
                    return x.Name.ToLower().Contains(Query.ToLower());
                });
            }
            #endregion

            #region Filter Density
            SelectedAirports = SelectedAirports.Where(x =>
            {
                return x.Density > DensityMin && x.Density < DensityMax;
            });
            #endregion

            #region Filter Relief
            SelectedAirports = SelectedAirports.Where(x =>
            {
                return x.Relief > ReliefMin && x.Relief < ReliefMax;
            });
            #endregion

            #region Apply Filters for Template Runways
            List<Surface> FilterSurfaceTypes = new List<Surface>();
            switch (SurfaceType)
            {
                default:
                    {
                        break;
                    }
                case "Soft":
                    {
                        FilterSurfaceTypes.Add(Surface.Grass);
                        FilterSurfaceTypes.Add(Surface.Gravel);
                        FilterSurfaceTypes.Add(Surface.Sand);
                        FilterSurfaceTypes.Add(Surface.Clay);
                        FilterSurfaceTypes.Add(Surface.Snow);
                        FilterSurfaceTypes.Add(Surface.Dirt);
                        break;
                    }
                case "Hard":
                    {
                        FilterSurfaceTypes.Add(Surface.Asphalt);
                        FilterSurfaceTypes.Add(Surface.Concrete);
                        FilterSurfaceTypes.Add(Surface.Bituminous);
                        FilterSurfaceTypes.Add(Surface.Tarmac);
                        FilterSurfaceTypes.Add(Surface.Ice);
                        break;
                    }
                case "Dirt":
                    {
                        FilterSurfaceTypes.Add(Surface.Dirt);
                        break;
                    }
                case "Gravel":
                    {
                        FilterSurfaceTypes.Add(Surface.Gravel);
                        FilterSurfaceTypes.Add(Surface.Sand);
                        FilterSurfaceTypes.Add(Surface.Clay);
                        break;
                    }
                case "Grass":
                    {
                        FilterSurfaceTypes.Add(Surface.Grass);
                        break;
                    }
                case "Water":
                    {
                        FilterSurfaceTypes.Add(Surface.Water);
                        break;
                    }
            }

            SelectedAirports = SelectedAirports.Where(x =>
            {
                if (x.Runways.Count >= RwyMin && x.Runways.Count <= RwyMax)
                {
                    bool HasMetaMatch = false;
                    bool FoundLight = !RequiresLight;
                    foreach (Airport.Runway rwy in x.Runways)
                    {
                        if (RequiresLight && (rwy.CenterLight > 0 || rwy.EdgeLight > 0))
                        {
                            FoundLight = true;
                        }

                        if ((FilterSurfaceTypes.Count > 0 ? FilterSurfaceTypes.Contains(rwy.Surface) : true)
                        && rwy.LengthFT >= RwyLenMin
                        && rwy.LengthFT <= RwyLenMax
                        && rwy.WidthMeters >= RwyWidMin
                        && rwy.WidthMeters <= RwyWidMax
                        && rwy.AltitudeFeet >= ElevMin
                        && rwy.AltitudeFeet <= ElevMax)
                        {
                            HasMetaMatch = true;
                        }
                    }

                    return HasMetaMatch && FoundLight;
                }
                return false;
            });
            #endregion

            #region Apply Filters for Template Parkings
            SelectedAirports = SelectedAirports.Where(x =>
            {
                bool Valid = true;

                if (x.Parkings.Count < ParkMin || x.Parkings.Count > ParkMax)
                {
                    Valid = false;
                }

                if (x.Parkings.Count > 0)
                {
                    // Min
                    if (x.Parkings.Find(pk => pk.Diameter * 3.28084f > ParkWidMin) == null)
                    {
                        Valid = false;
                    }

                    // Max
                    if (x.Parkings.Find(pk => pk.Diameter * 3.28084f < ParkWidMax) == null)
                    {
                        Valid = false;
                    }
                }

                return Valid;
            });
            #endregion

            #region Apply Filters for Template Names
            // Apply Filters to exlude Some Names
            List<string> ExcludedNames = new List<string>()
            {
                "Air Base",
                " AFB",
                " AB",
                "Fliegerhorst",
                "close",
                "Close",
                "Ignore"
            };
            SelectedAirports = SelectedAirports.Where(x =>
            {
                foreach (string Name in ExcludedNames)
                {
                    if (x.Name.Contains(Name))
                    {
                        return false;
                    }
                }

                return true;
            });
            #endregion

            // User Limits
            #region Filter for User Runways
                #region Runways
                SelectedAirports = new AirportFilters.FilterRunways()
                {
                    List = SelectedAirports,
                    CountMin = Generator.RwyCount[0],
                    CountMax = Generator.RwyCount[1],
                    LengthMin = Generator.RwyLength[0],
                    LengthMax = Generator.RwyLength[1],
                    RwySurface = Generator.RwySurface,
                }.Get();
                #endregion
            #endregion

            return SelectedAirports;
        }

        internal IEnumerable<Airport> FilterAirportsDistance(IEnumerable<Airport> SelectedAirports, GeoLoc Location, int MinKM, int MaxKM)
        {
            //const float LatitudeDistanceM = 111100;
            //float LimitDist = (float)Max * 1000;
            //float LatRange = LimitDist / LatitudeDistanceM;
            //double North = Location.Lat + LatRange;
            //double South = Location.Lat - LatRange;
            //GeoLoc West = Utils.MapOffsetPosition(Location, LimitDist, 270);
            //GeoLoc East = Utils.MapOffsetPosition(Location, LimitDist, 90);

            //List<Airport> Boxed = SelectedAirports.FindAll(x =>
            //{
            //    if(x.Location.Lon > West.Lon && x.Location.Lon < East.Lon)
            //    {
            //        if (x.Location.Lat > South && x.Location.Lat < North)
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //});

            //List<Airport> Sel = Boxed.FindAll(x =>
            //{
            //    float Dist = (float)Utils.MapCalcDist(Location, x.Location, Utils.DistanceUnit.Kilometers, true);
            //    if (Dist > Min && Dist < Max)
            //    {
            //        return true;
            //    }
            //    return false;
            //});

            float LatitudeRangeN = (float)Utils.MapOffsetPosition(Location, MaxKM * 1000, 0).Lat;
            float LatitudeRangeS = (float)Utils.MapOffsetPosition(Location, MaxKM * 1000, 180).Lat;

            IEnumerable<Airport> SelTot = SelectedAirports.Where(x =>
            {
                float Lat = (float)x.Location.Lat;
                if (LatitudeRangeN > Lat && LatitudeRangeS < Lat)
                {
                    float Dist = (float)Utils.MapCalcDist(Location, x.Location, Utils.DistanceUnit.Kilometers, true);
                    if (Dist > MinKM && Dist < MaxKM)
                    {
                        return true;
                    }
                }
                return false;
            });

            return SelTot;
        }

        internal IEnumerable<Airport> FilterAirportsBounds(IEnumerable<Airport> SelectedAirports)
        {
            if (Boundaries.Count > 2)
            {
                IEnumerable<Airport> InBounds = SelectedAirports.Where(x =>
                {
                    return Utils.IsPointInPoly(Boundaries, x.Location);
                });
                return InBounds;
            }
            else
            {
                return SelectedAirports;
            }
        }

    }
}
