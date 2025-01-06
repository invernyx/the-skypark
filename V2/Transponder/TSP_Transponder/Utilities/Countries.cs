using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Utilities
{
    class Countries
    {
        private static List<Dictionary<string, dynamic>> CountriesGeometry = null;

        public static void Startup()
        {
            LoadCountries();
        }

        public static List<Dictionary<string, dynamic>> LoadCountries()
        {
            if(CountriesGeometry == null)
            {
                #region Convert from Source
                /*
                string CountriesNamesPath = @"F:\Documents\Clients\Parallel 42\The Skypark\Git\tsp-transponder-2\Countries\countryInfo.txt";
                List<string> CountriesNames = File.ReadAllLines(CountriesNamesPath).ToList().Where(x => !x.StartsWith("#")).ToList();

                string ShapePath = @"F:\Documents\Clients\Parallel 42\The Skypark\Git\tsp-transponder-2\Countries\shapes_all_low.txt";
                string[] CountryLine = File.ReadAllLines(ShapePath);

                
                int i = 0;
                foreach (var Country in CountryLine)
                {
                    if (i > 0)
                    {

                        string[] Spl = Country.Split('\t');
                        string[] CountryInfo = null;

                        string InfoLine = CountriesNames.Find(x =>
                        {
                            CountryInfo = x.Split('\t');
                            if (CountryInfo[16] == Spl[0])
                            {
                                return true;
                            }
                            return false;

                        });

                        Dictionary<string, dynamic> CountryStruct = new Dictionary<string, dynamic>()
                        {
                            { "ISO", CountryInfo[0] },
                            { "Capital", CountryInfo[5] },
                            { "Continent", CountryInfo[8] },
                            { "Name", CountryInfo[4] },
                            { "Area", CountryInfo[6] },
                            { "Population", CountryInfo[7] },
                            { "Polys", null },
                        };

                        Dictionary<string, dynamic> CountryGeo = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(Spl[1]);

                        switch (CountryGeo["type"])
                        {
                            case "Polygon":
                                {
                                    CountryStruct["Polys"] = new List<dynamic>() { CountryGeo["coordinates"] };
                                    break;
                                }
                            case "MultiPolygon":
                                {
                                    CountryStruct["Polys"] = CountryGeo["coordinates"];
                                    break;
                                }
                        }

                        CountriesGeometry.Add(CountryStruct);

                    }
                    i++;

                }

                string pat_dump_file = Path.Combine(App.AppDataDirectory, "countries.json");
                if (File.Exists(pat_dump_file))
                {
                    File.Delete(pat_dump_file);
                }
                using (StreamWriter sw = File.CreateText(pat_dump_file))
                {
                    sw.WriteLine(App.JSSerializer.Serialize(CountriesGeometry));
                }
                */
                #endregion
                
                string CountriesJSON = App.ReadResourceFile("TSP_Transponder.Utilities.Countries.countries.json");
                List<Dictionary<string, dynamic>> CountriesRaw = App.JSSerializer.Deserialize<List<Dictionary<string, dynamic>>>(CountriesJSON);

                foreach (var Country in CountriesRaw)
                {
                    Country.Add("GeoPolys", new List<List<List<GeoLoc>>>());
                    foreach (var PolyGroup in Country["Polys"])
                    {
                        List<List<GeoLoc>> PG = new List<List<GeoLoc>>();
                        Country["GeoPolys"].Add(PG);
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
                }

                CountriesGeometry = CountriesRaw;
            }

            return CountriesGeometry;
        }

        public static Country GetCountry(GeoLoc Location)
        {
            List<Dictionary<string, dynamic>> Countries = LoadCountries();
            
            foreach (var Country in Countries)
            {
                foreach (var PolyGroup in Country["GeoPolys"])
                {
                    foreach (var Poly in PolyGroup)
                    {
                        if (Utils.IsPointInPoly((List<GeoLoc>)Poly, (GeoLoc)Location))
                        {
                            return new Country()
                            {
                                Name = Country["Name"],
                                Code = Country["ISO"]
                            };
                        }
                    }
                }
            }
            
            // If we didn't find anything, get the closest
            float Dist = float.MaxValue;
            Dictionary<string, dynamic> Closest = null;
            foreach (var Country in Countries)
            {
                foreach (var PolyGroup in Country["GeoPolys"])
                {
                    foreach (var Poly in PolyGroup)
                    {
                        float newDist = Utils.DistPointToPoly((List<GeoLoc>)Poly, Location);
                        if (newDist < Dist)
                        {
                            Dist = newDist;
                            Closest = Country;
                        }
                    }
                }
            }

            if (Closest != null)
            {
                return new Country()
                {
                    Name = Closest["Name"],
                    Code = Closest["ISO"]
                };
            }

            return null;
        }

        public class Country
        {
            public string Name = "";
            public string Code = "";
        }
    }
}
