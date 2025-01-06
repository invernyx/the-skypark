using LiteDB;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OSGeo.OGR;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TSP_OSM_Loader.Datasets;
using TSP_OSM_Loader.Topography;
using TSP_OSM_Loader.Topography.Utils;
using static TSP_OSM_Loader.ResultFeature;

namespace TSP_OSM_Loader
{
    class OSMImport
    {
        // https://download.geofabrik.de/
        // https://wiki.openstreetmap.org/wiki/Map_features

        public static void ReadData()
        {
#if DEBUG
            string Dir = Path.Combine(Program.AppDataDirectory, "OSM");
            List<string> DoneStrings = new List<string>();

            Console.WriteLine("Checking for *.osm.pbf in " + Dir);
            Console.WriteLine("");
            
            string[] OSMFiles = Directory.GetFiles(Dir, "*.osm.pbf");
            
            List<string> FoundTags = new List<string>();
            
            #region Data Structure
            Dictionary<string, List<SearchEntry>> NodeTagsToMatch = new Dictionary<string, List<SearchEntry>>()
            {
                //{
                //    "rail_cross",
                //    new List<SearchEntry>()
                //    {
                //        new SearchEntry()
                //        {
                //            key = "railway",
                //            value = "railway_crossing",
                //            picker = (geo) =>
                //            {
                //                return geo.Id % 200 == 1;
                //            },
                //            //max_density = 30,
                //            //AttrsReq = new List<List<string>>()
                //            //{
                //            //    new List<string>() { "name", null }
                //            //},
                //            attrs = new List<string>()
                //            {
                //                "name",
                //                "name:en"
                //            }
                //        }
                //    }
                //},

                {
                    "roundabouts",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "highway",
                            value = "mini_roundabout",
                            max_density = 20000,
                            //picker = (geo) =>
                            //{
                            //    return geo.Id % 10 == 1;
                            //},
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },
                
                //{
                //    "landing_sites",
                //    new List<SearchEntry>()
                //    {
                //        new SearchEntry()
                //        {
                //            key = "emergency",
                //            value = "landing_site",
                //            attrs = new List<string>()
                //            {
                //                "name",
                //                "name:en"
                //            }
                //        }
                //    }
                //},

            };

            Dictionary<string, List<SearchEntry>> PolyTagsToMatch = new Dictionary<string, List<SearchEntry>>()
            {
                {
                    "railways",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "railway",
                            value = "rail",
                            is_path = true,
                            coords_src = CoordsSource.Random,
                            min_length = 100,
                            max_density = 15000,
                            picker = (geo) =>
                            {
                                return geo.Id % 10 == 1;
                            },
                            attrs_filter = (attrs) =>
                            {
                                return attrs.Find(x => x.Key == "bridge").Key == null && attrs.Find(x => x.Key == "tunnel").Key == null;
                            },
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null },
                                new List<string>() { "usage", @"main" }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                {
                    "parkings",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "amenity",
                            value = "parking",
                            coords_src = CoordsSource.Center,
                            min_area = 1000,
                            max_density = 20000,
                            picker = (geo) =>
                            {
                                return geo.Id % 10 == 1;
                            },
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "parking", @"surface" }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                {
                    "roads",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "highway",
                            value = "primary",
                            is_path = true,
                            coords_src = CoordsSource.Random,
                            min_length = 150,
                            max_density = 20000,
                            attrs_filter = (attrs) =>
                            {
                                return attrs.Find(x => x.Key == "bridge").Key == null && attrs.Find(x => x.Key == "tunnel").Key == null;
                            },
                            attrs_req = new List<List<string>>()
                            {
                                //new List<string>() { "name", null },
                                //new List<string>() { "lanes", null },
                                new List<string>() { "surface", @"paved" }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                {
                    "beaches",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "natural",
                            value = "beach",
                            coords_src = CoordsSource.Center,
                            //min_area = 500,
                            image_func = (imageb64) =>
                            {
                                string base64String = imageb64.Replace("data:image/png;base64,", "");
                                byte[] imageBytes = Convert.FromBase64String(base64String);

                                SKBitmap bitmap;
                                using (var ms = new MemoryStream(imageBytes))
                                {
                                    bitmap = SKBitmap.Decode(ms);
                                }

                                int p1_x = (int)(bitmap.Width * 0.45);
                                int p1_y = (int)(bitmap.Height * 0.45);

                                int p2_x = (int)(bitmap.Width * 0.5);
                                int p2_y = (int)(bitmap.Height * 0.5);

                                int p3_x = (int)(bitmap.Width * 0.55);
                                int p3_y = (int)(bitmap.Height * 0.55);


                                SKColor c11 = bitmap.GetPixel(p1_x, p1_y);
                                SKColor c12 = bitmap.GetPixel(p1_x, p2_y);
                                SKColor c13 = bitmap.GetPixel(p1_x, p3_y);
                                SKColor c21 = bitmap.GetPixel(p2_x, p1_y);
                                SKColor c22 = bitmap.GetPixel(p2_x, p2_y);
                                SKColor c23 = bitmap.GetPixel(p2_x, p3_y);
                                SKColor c31 = bitmap.GetPixel(p3_x, p1_y);
                                SKColor c32 = bitmap.GetPixel(p3_x, p2_y);
                                SKColor c33 = bitmap.GetPixel(p3_x, p3_y);

                                List<double> a = new List<double>()
                                {
                                    Utils.ColorDifferencePercentage(233, 216, 197, c11.Red, c11.Green, c11.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c12.Red, c12.Green, c12.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c13.Red, c13.Green, c13.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c21.Red, c21.Green, c21.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c22.Red, c22.Green, c22.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c23.Red, c23.Green, c23.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c31.Red, c31.Green, c31.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c32.Red, c32.Green, c32.Blue),
                                    Utils.ColorDifferencePercentage(233, 216, 197, c33.Red, c33.Green, c33.Blue),
                                };

                                var average = a.Average();
                                return average < 30;
                            },
                            //attrs_req = new List<List<string>>()
                            //{
                            //    new List<string>() { "name", null }
                            //},
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                {
                    "ski_slopes",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "piste:type",
                            value = "downhill",
                            is_path = true,
                            coords_src = CoordsSource.Highest,
                            min_length = 500,
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                {
                    "raceways",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "highway",
                            value = "raceway",
                            is_path = true,
                            coords_src = CoordsSource.Random,
                            min_length = 200,
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null },
                                new List<string>() { "sport", @"motor" }
                                
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        }
                    }
                },

                /*
                {
                    "heliports",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "aeroway",
                            Value = "helipad",
                            MinArea = 0,
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        }
                    }
                },
                {
                    "schools",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "amenity",
                            Value = "university",
                            KeepCoords = true,
                            AttrsReq = new List<List<string>>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        }
                    }
                },
                */


                {
                    "open_fields",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "leisure",
                            value = "pitch",
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en",
                                "sport"
                            }
                        },
                    }
                },
                {
                    "golf_courses",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "golf",
                            value = "fairway",
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        },
                        new SearchEntry()
                        {
                            key = "golf",
                            value = "tee",
                            coords_src = CoordsSource.End,
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        },
                        new SearchEntry()
                        {
                            key = "golf",
                            value = "green",
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        },
                    }
                },
                {
                    "prisons",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "amenity",
                            value = "prison",
                            min_area = 600,
                            //max_density = 50,
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en"
                            }
                        },
                    }
                },
                {
                    "hospitals",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            key = "amenity",
                            value = "hospital",
                            min_area = 600,
                            min_density = 1000,
                            max_density = 30000,
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null },
                                new List<string>() { "emergency", @"yes" }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en",
                                "emergency"
                            }
                        },
                        new SearchEntry()
                        {
                            key = "healthcare",
                            value = "hospital",
                            min_area = 600,
                            min_density = 1000,
                            max_density = 30000,
                            attrs_req = new List<List<string>>()
                            {
                                new List<string>() { "name", null },
                                new List<string>() { "emergency", @"yes" }
                            },
                            attrs = new List<string>()
                            {
                                "name",
                                "name:en",
                                "emergency"
                            }
                        },
                    }
                },


                /*
                {
                    "hotels",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "tourism",
                            Value = "hotel",
                            AttrsReq = new List<List<string>>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        },
                        new SearchEntry()
                        {
                            Key = "tourism",
                            Value = "motel",
                            AttrsReq = new List<List<string>>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        },
                        new SearchEntry()
                        {
                            Key = "tourism",
                            Value = "hostel",
                            AttrsReq = new List<List<string>>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        },
                        //new SearchEntry()
                        //{
                        //    Key = "tourism",
                        //    Value = "apartment"
                        //},
                        //new SearchEntry()
                        //{
                        //    Key = "tourism",
                        //    Value = "alpine_hut"
                        //},
                    }
                },
                {
                    "military",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "landuse",
                            Value = "military"
                        }
                    }
                },
                */
                //{
                //    "reefs",
                //    new List<SearchEntry>()
                //    {
                //        new SearchEntry()
                //        {
                //            Key = "natural",
                //            Value = "reef",
                //            Attrs = new List<string>()
                //            {
                //                "name"
                //            }
                //        }
                //    }
                //},
                //{
                //    "fast_food",
                //    new List<SearchEntry>()
                //    {
                //        new SearchEntry()
                //        {
                //            Key = "brand",
                //            Value = "McDonald's",
                //            Attrs = new List<string>()
                //            {
                //                "brand"
                //            }
                //        },
                //    }
                //},
            };
            #endregion




            CountdownEvent Countdown = new CountdownEvent(OSMFiles.Length);
            foreach (var sourcePath in OSMFiles)
            {
                Thread nt = new Thread(() =>
                {
                    DateTime StartTime = DateTime.Now;
                    int PolysCount = 0;
                    FileInfo FI = new FileInfo(sourcePath);
                    Console.WriteLine("Loading file " + FI.Name);

                    #region Create Index
                    List<SearchEntry> PolyTagsToMatchIndex = new List<SearchEntry>();
                    foreach (var entry in PolyTagsToMatch)
                    {
                        PolyTagsToMatchIndex.AddRange(entry.Value.Select(x => x));
                    }

                    List<SearchEntry> NodeTagsToMatchIndex = new List<SearchEntry>();
                    foreach (var entry in NodeTagsToMatch)
                    {
                        NodeTagsToMatchIndex.AddRange(entry.Value.Select(x => x));
                    }
                    #endregion

                    #region "Check if Tags Contains" Function
                    Func<List<SearchEntry>, OsmSharp.OsmGeo, OsmSharp.OsmGeoType, bool> HasTags = (Source, OSMItem, Type) =>
                    {

                        // where ((osmGeo.Tags != null && HasTags(PolyTagsToMatchIndex, osmGeo.Tags)) || osmGeo.Type == OsmSharp.OsmGeoType.Node) && (osmGeo.Tags != null ? !osmGeo.Tags.ContainsKey("id") : true)

                        if (OSMItem.Tags != null) { if (OSMItem.Tags.ContainsKey("id")) return false; }

                        if (OSMItem.Type == OsmSharp.OsmGeoType.Node)
                        {
                            return true;
                        }

                        if (OSMItem.Tags.Count == 0) return false;

                        foreach (var tag in Source)
                        {
                            if(tag.picker != null)
                            {
                                if(!tag.picker(OSMItem))
                                {
                                    return false;
                                }
                            }

                            if (OSMItem.Tags.Contains(tag.key, tag.value))
                            {
                                if(!FoundTags.Contains(tag.key))
                                {
                                    FoundTags.Add(tag.key);
                                    Console.WriteLine("Found first tag for " + tag.key);
                                }
                                return true;
                            };
                        }
                        return false;
                    };
                    #endregion
                                        
                    if (File.Exists(sourcePath))
                    {
                        CountdownEvent Countdown1 = new CountdownEvent(2);
                        
                        Thread lThread = new Thread(() =>
                        {
                            if (PolyTagsToMatch.Count == 0)
                            {
                                Countdown1.Signal();
                                return;
                            }

                            using (var fileStream = File.OpenRead(sourcePath))
                            {
                                using (var stream = new PBFOsmStreamSource(fileStream))
                                {
                                    var filteredPoly = from osmGeo in stream
                                                       where HasTags(PolyTagsToMatchIndex, osmGeo, OsmSharp.OsmGeoType.Node)
                                                       select osmGeo;

                                    #region LineStrings
                                    var features = filteredPoly.ToFeatureSource();
                                    var lineStrings = from feature in features
                                                      where (feature.Geometry is LineString) || (feature.Geometry is MultiLineString) || (feature.Geometry is Polygon) || (feature.Geometry is MultiPolygon)
                                                      select feature;

                                    Console.WriteLine("Starting polygons Query on " + FI.Name + " at " + DateTime.Now.ToLongTimeString());
                                    Parallel.ForEach(lineStrings, new ParallelOptions() { MaxDegreeOfParallelism = 30 }, (feature) =>
                                    {
                                        string[] attr_names = feature.Attributes.GetNames();
                                        ResultFeature p = new ResultFeature(feature, (long)feature.Attributes["id"]);

                                        foreach (var str in attr_names.Where(x => x != "id"))
                                        {
                                            p.attrs.Add(new KeyValuePair<string, string>(str, (string)(feature.Attributes[str])));
                                        }

                                        #region Create Coordinates
                                        foreach (var geo in feature.Geometry.Coordinates)
                                        {
                                            p.coordinates.Add(new Point(Math.Round(geo.Y, 5), Math.Round(geo.X, 5)));
                                        }
                                        #endregion

                                        PolysCount++;

                                        foreach (var entry in PolyTagsToMatch)
                                        {
                                            bool pass = false;
                                            foreach (var se in entry.Value)
                                            {
                                                p.se = se;

                                                if (attr_names.Contains(se.key))
                                                {
                                                    if ((string)feature.Attributes[se.key] == se.value)
                                                    {
                                                        // Check if what we found has the required tags
                                                        foreach (var tag1 in se.attrs_req)
                                                        {
                                                            var f = p.attrs.Find(x => x.Key == tag1[0]);
                                                            if (f.Key == null)
                                                            {
                                                                pass = true;
                                                                break;
                                                            }
                                                            else if (tag1[1] != null)
                                                            {
                                                                Regex regex = new Regex(tag1[1]);
                                                                if (!regex.IsMatch(f.Value))
                                                                {
                                                                    pass = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        if (pass) { break; }


                                                        if (se.attrs_filter != null ? !se.attrs_filter(p.attrs) : false)
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        if (!p.Calculate())
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        if (!p.CalculateDensity())
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        // Compute BSON
                                                        var topo_dataset = Topo.GetNewDataset();
                                                        p.CalculateTopo(topo_dataset);

                                                        Action Finalize = () =>
                                                        {
                                                            // Filter tags to what we want
                                                            p.attrs = p.attrs.Where(x => se.attrs.Contains(x.Key)).ToList();

                                                            var Attrs = BsonMapper.Global.ToDocument(p.attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                                                            var Bson = new BsonDocument()
                                                            {
                                                                ["lat"] = Math.Round(p.central_node[0], 6),
                                                                ["lon"] = Math.Round(p.central_node[1], 6),
                                                                ["alt"] = (int)Topo.GetElevation(new GeoLoc(p.central_node[1], p.central_node[0]), topo_dataset),
                                                                ["relief"] = p.relief,
                                                                ["density"] = p.density,
                                                                ["attrs"] = Attrs,
                                                            };

                                                            if (p.heading != null)
                                                            {
                                                                Bson.Add("heading", p.heading);
                                                            }

                                                            // Add to DB
                                                            lock (LiteDbService.DB)
                                                            {
                                                                var DBCollection = LiteDbService.DB.Database.GetCollection(entry.Key);
                                                                DBCollection.Upsert(p.id, Bson);
                                                            }

                                                            string d = Bson["density"].ToString();
                                                            string a = Bson["alt"].ToString();
                                                            Console.WriteLine((string)("Added " + (se.is_path ? "Path" : "Poly") + " " + entry.Key + " in " + FI.Name + " with " + se.key + "/" + se.value + "/" + p.id + " at " + Environment.NewLine + "    D:" + d + ", A:" + a + ", R:" + p.relief + ", C:" + p.central_node[0] + "," + p.central_node[1] + ""));

                                                            pass = true;
                                                        };

                                                        Console.WriteLine((string)("Processing " + (se.is_path ? "Path" : "Poly") + " " + entry.Key + " in " + FI.Name + ""));
                                                        if (se.image_func != null)
                                                        {
                                                            WSMapImage.GetImage(new Job()
                                                            {
                                                                Id = p.central_node[0] + "_" + p.central_node[0],
                                                                Latitude = p.central_node[0],
                                                                Longitude = p.central_node[1],
                                                                Callback = (img) => {
                                                                    if (!se.image_func(img))
                                                                    {
                                                                        pass = true;
                                                                        Console.WriteLine((string)("Passed " + (se.is_path ? "Path" : "Poly") + " " + entry.Key + " in " + FI.Name + " with " + se.key + "/" + se.value + "/" + p.id + " at " + Environment.NewLine + "   C:" + p.central_node[0] + "," + p.central_node[1] + ""));
                                                                    }
                                                                    else
                                                                    {
                                                                        Finalize();
                                                                    }
                                                                }
                                                            });
                                                        }
                                                        else
                                                        {
                                                            Finalize();
                                                        }

                                                        if (pass) { break; }
                                                        break;
                                                    }
                                                }
                                            }
                                            if (pass)
                                            {
                                                continue;
                                            }
                                        }
                                    });

                                    #endregion

                                    stream.Dispose();
                                }
                            }
                            Countdown1.Signal();
                        });
                        lThread.SetApartmentState(ApartmentState.MTA);
                        lThread.Priority = ThreadPriority.Highest;
                        lThread.Start();

                        Thread nThread = new Thread(() =>
                        {
                            if(NodeTagsToMatch.Count == 0)
                            {
                                Countdown1.Signal();
                                return;
                            }
                            
                            using (var fileStream = File.OpenRead(sourcePath))
                            {
                                using (var stream = new PBFOsmStreamSource(fileStream))
                                {
                                    var filteredPoint = from osmGeo in stream
                                                        where osmGeo.Type == OsmSharp.OsmGeoType.Node && HasTags(NodeTagsToMatchIndex, osmGeo, OsmSharp.OsmGeoType.Node)
                                                        select osmGeo;
                                    #region Nodes
                                    Console.WriteLine("Starting nodes Query on " + FI.Name + " at " + DateTime.Now.ToLongTimeString());
                                    foreach (var osmGeo in filteredPoint.AsParallel())
                                    {
                                        OsmSharp.Node node = (OsmSharp.Node)osmGeo;
                                        string[] attr_names = osmGeo.Tags.Select(x => x.Key).ToArray();

                                        foreach (var entry in NodeTagsToMatch)
                                        {
                                            bool pass = false;
                                            foreach (var se in entry.Value)
                                            {
                                                if (attr_names.Contains(se.key))
                                                {
                                                    if ((string)osmGeo.Tags[se.key] == se.value)
                                                    {
                                                        // Filter tags
                                                        var attrs = node.Tags.Where(x => se.attrs.Contains(x.Key)).ToList();

                                                        // Check if what we found has the required tags
                                                        foreach (var tag1 in se.attrs_req)
                                                        {
                                                            var f = attrs.Find(x => x.Key == tag1[0]);
                                                            if (f.Key == null)
                                                            {
                                                                pass = true;
                                                                break;
                                                            }
                                                            else if(tag1[1] != null)
                                                            {
                                                                Regex regex = new Regex(tag1[1]);
                                                                if(!regex.IsMatch(f.Value))
                                                                {
                                                                    pass = true;
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        // Create ResultFeature
                                                        ResultFeature p = new ResultFeature(null, (long)node.Id);
                                                        p.coordinates.Add(new Point(node.Longitude.Value, node.Latitude.Value));
                                                        p.se = se;

                                                        // Add tags to results
                                                        foreach (var tag in attrs.Where(x => x.Key != "id"))
                                                        {
                                                            p.attrs.Add(new KeyValuePair<string, string>(tag.Key, tag.Value));
                                                        }

                                                        if (se.attrs_filter != null ? !se.attrs_filter(p.attrs) : false)
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        if (!p.Calculate())
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        if (!p.CalculateDensity())
                                                        {
                                                            pass = true;
                                                            break;
                                                        }

                                                        if (pass) { break; }

                                                        // Compute BSON
                                                        var topo_dataset = Topo.GetNewDataset();
                                                        p.CalculateTopo(topo_dataset);

                                                        Action Finalize = () =>
                                                        {
                                                            var Attrs = BsonMapper.Global.ToDocument(p.attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                                                            var Bson = new BsonDocument()
                                                            {
                                                                ["lat"] = Math.Round(p.central_node[0], 6),
                                                                ["lon"] = Math.Round(p.central_node[1], 6),
                                                                ["alt"] = (int)Topo.GetElevation(new GeoLoc(p.central_node[1], p.central_node[0]), topo_dataset),
                                                                ["relief"] = p.relief,
                                                                ["density"] = p.density,
                                                                ["attrs"] = Attrs,
                                                            };

                                                            if (p.heading != null)
                                                            {
                                                                Bson.Add("heading", p.heading);
                                                            }

                                                            topo_dataset.Dispose();

                                                            // Add to DB
                                                            float lat = (float)Math.Round((double)node.Latitude, 5);
                                                            float lon = (float)Math.Round((double)node.Longitude, 5);
                                                            lock (LiteDbService.DB)
                                                            {
                                                                var DBCollection = LiteDbService.DB.Database.GetCollection(entry.Key);
                                                                DBCollection.Upsert(node.Id, Bson);
                                                            }

                                                            string d = Bson["density"].ToString();
                                                            string a = Bson["alt"].ToString();
                                                            Console.WriteLine((string)("Added node " + entry.Key + " in " + FI.Name + " with " + se.key + "/" + se.value + "/" + node.Id + " at " + Environment.NewLine + "    D:" + d + ", A:" + a + ", R:" + p.relief + ", C:" + lat + "," + lon + ""));
                                                            pass = true;
                                                        };

                                                        Console.WriteLine((string)("Processing node " + entry.Key + " in " + FI.Name + " with " + se.key + "/" + se.value + "/" + node.Id + ""));
                                                        if (se.image_func != null)
                                                        {
                                                            WSMapImage.GetImage(new Job()
                                                            {
                                                                Id = p.central_node[0] + "_" + p.central_node[0],
                                                                Latitude = p.central_node[0],
                                                                Longitude = p.central_node[1],
                                                                Callback = (img) => {
                                                                    if (!se.image_func(img))
                                                                    {
                                                                        pass = true;
                                                                        Console.WriteLine((string)("Passed node " + entry.Key + " in " + FI.Name + " with " + se.key + "/" + se.value + "/" + node.Id + " at " + Environment.NewLine + "    C:" + node.Latitude + "," + node.Longitude + ""));
                                                                    }
                                                                    else
                                                                    {
                                                                        Finalize();
                                                                    }
                                                                }
                                                            });
                                                        }
                                                        else
                                                        {
                                                            Finalize();
                                                        }

                                                        if (pass) { break; }
                                                        break;
                                                    }
                                                }
                                            }
                                            if (pass)
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    #endregion

                                    stream.Dispose();
                                }
                            }
                            Countdown1.Signal();
                        });
                        nThread.SetApartmentState(ApartmentState.MTA);
                        nThread.Priority = ThreadPriority.Highest;
                        nThread.Start();

                        Countdown1.Wait();

                        lock (DoneStrings)
                        {
                            DoneStrings.Add("Done on " + FI.Name + " with " + PolysCount + " results in " + (DateTime.Now - StartTime).ToString(@"hh\:mm\:ss"));
                        }
                    }
                    else
                    {
                        lock (DoneStrings)
                        {
                            DoneStrings.Add("Source file doesn't exist: " + sourcePath);
                        }
                    }

                    Countdown.Signal();
                    Console.WriteLine("Done with file " + FI.Name + " in " + (DateTime.Now - StartTime).ToString(@"hh\:mm\:ss"));
                });
                nt.Start();
            }

            Countdown.Wait();

            Console.WriteLine("");
            Console.WriteLine("-Summary--------------------------");
            Console.WriteLine("");

            foreach (string str in DoneStrings)
            {
                Console.WriteLine(str);
            }
#endif
        }
        
    }

    class SearchEntry
    {
        public string key = "";
        public string value = "";
        public Func<OsmSharp.OsmGeo, bool> picker = null;
        public List<string> attrs = new List<string>();
        public List<List<string>> attrs_req = new List<List<string>>();
        public Func<List<KeyValuePair<string, string>>, bool> attrs_filter = null;
        public CoordsSource coords_src = CoordsSource.Center;
        public Func<string,bool> image_func = null;
        public float min_area = 400;
        public float min_length = 500;
        public float max_density = float.MaxValue;
        public float min_density = 0;
        public bool is_path = false;
        public bool keep_coords = false;
    }

    class ResultFeature
    {
        public long id = 0;
        public List<Point> coordinates = new List<Point>();
        public List<List<double>> coordinates_d = null;
        public SearchEntry se = null;
        public List<double> central_node = null;
        public int? heading = null;
        public int? area = null;
        public int? length = null;
        public int? relief = null;
        public int? density = null;
        public IFeature feature = null;
        public List<KeyValuePair<string, string>> attrs = new List<KeyValuePair<string, string>>();
        public ResultFeature(IFeature feature, long id)
        {
            this.feature = feature;
            this.id = id;
        }

        public bool Calculate()
        {
            if (coordinates_d == null)
            {
                coordinates_d = coordinates.Select(x => new List<double>() { x.Coordinate.X, x.Coordinate.Y }).ToList();
                
                if(coordinates.Count == 1)
                {
                    central_node = new List<double>() { coordinates_d[0][1], coordinates_d[0][0] };
                }
                else if (!se.is_path)
                {
                    var area_r = Area.ComputeSignedArea(coordinates_d);
                    area = Convert.ToInt32(Math.Min(Int32.MaxValue, Math.Round(area_r)));

                    if (area < se.min_area) return false;

                    var fa = new float[1][][];
                    fa[0] = PolyLabel.ConvertPolygonToFloatArray(coordinates_d);
                    var cn = PolyLabel.GetPolyLabel(fa);
                    central_node = new List<double>() { cn[0], cn[1] };
                }
                else
                {
                    var half = Convert.ToInt32(Math.Floor(coordinates_d.Count / 2.00));
                    central_node = new List<double>() { coordinates_d[half][1], coordinates_d[half][0] };

                    List<double> previous = null;
                    length = 0;
                    foreach (var leg in coordinates_d)
                    {
                        if(previous != null)
                        {
                            var dist = Utils.MapCalcDist(leg[0], leg[1], previous[0], previous[1], Utils.DistanceUnit.Meters);
                            length += Convert.ToInt32(Math.Round(dist));
                        }
                        previous = leg;
                    }

                    if (length < se.min_length) return false;
                }

                return true;
            }

            return false;

        }

        public bool CalculateDensity()
        {
            density = (int)Math.Round(WorldCities.DC.CalculateDensityAt(central_node[0], central_node[1]));
            return se.max_density >= density && se.min_density <= density;
        }

        public void CalculateTopo(TileDataSet tds)
        {
            if (central_node != null)
            {
                List<float> alts = new List<float>();
                foreach(var coords in coordinates_d)
                {
                    alts.Add((float)Topo.GetElevation(new GeoLoc(coords[1], coords[0]), tds));
                }

                int chosen_index = 0;
                switch(se.coords_src)
                {
                    case CoordsSource.Start:
                        {
                            central_node[0] = coordinates_d[0][0];
                            central_node[1] = coordinates_d[0][1];
                            break;
                        }
                    case CoordsSource.End:
                        {
                            central_node[0] = coordinates_d[coordinates_d.Count - 1][0];
                            central_node[1] = coordinates_d[coordinates_d.Count - 1][1];
                            break;
                        }
                    case CoordsSource.Highest:
                        {
                            chosen_index = alts.IndexOf(alts.Max());
                            central_node[0] = coordinates_d[chosen_index][0];
                            central_node[1] = coordinates_d[chosen_index][1];
                            break;
                        }
                    case CoordsSource.Lowest:
                        {
                            chosen_index = alts.IndexOf(alts.Min());
                            central_node[0] = coordinates_d[chosen_index][0];
                            central_node[1] = coordinates_d[chosen_index][1];
                            break;
                        }
                    case CoordsSource.Random:
                        {
                            chosen_index = Utils.GetRandom(alts.Count);
                            central_node[0] = coordinates_d[chosen_index][0];
                            central_node[1] = coordinates_d[chosen_index][1];
                            break;
                        }
                }

                if(coordinates_d.Count > 0 && se.coords_src != CoordsSource.Center)
                {
                    var previous_node = chosen_index - 1;
                    if(previous_node < 0) { previous_node = chosen_index + 1; }
                    heading = Convert.ToInt32(Math.Round(Utils.GetDirectionInDegrees(coordinates_d[chosen_index][1], coordinates_d[chosen_index][0], coordinates_d[previous_node][1], coordinates_d[previous_node][0])));
                }

                relief = Convert.ToInt32(Math.Round(Topo.GetRelief(new GeoLoc(central_node[1], central_node[0]), tds)));
                
            }
        }

        public enum CoordsSource
        {
            Center,
            Start,
            End,
            Random,
            Highest,
            Lowest,
        }
    }
}
