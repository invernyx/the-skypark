using LiteDB;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Geo;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSP_OSM_Loader
{
    class OSMImport
    {
        //https://wiki.openstreetmap.org/wiki/Map_features
        
        public static void ReadData()
        {
#if DEBUG
            string Dir = Path.Combine(Program.AppDataDirectory, "OSM");
            List<string> DoneStrings = new List<string>();

            Console.WriteLine("Checking for *.osm.pbf in " + Dir);
            Console.WriteLine("");

            string[] OSMFiles = Directory.GetFiles(Dir, "*.osm.pbf");




            #region Data Structure
            Dictionary<string, List<SearchEntry>> NodeTagsToMatch = new Dictionary<string, List<SearchEntry>>()
            {
                {
                    "landing_sites",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "emergency",
                            Value = "landing_site",
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        }
                    }
                },
            };

            Dictionary<string, List<SearchEntry>> PolyTagsToMatch = new Dictionary<string, List<SearchEntry>>()
            {
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
                            AttrsReq = new List<string>()
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
                {
                    "open_fields",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "leisure",
                            Value = "pitch",
                            Attrs = new List<string>()
                            {
                                "name",
                                "sport",
                            }
                        },
                        new SearchEntry()
                        {
                            Key = "golf",
                            Value = "driving_range",
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        }
                    }
                },
                {
                    "prisons",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "amenity",
                            Value = "prison",
                            KeepCoords = true,
                            AttrsReq = new List<string>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
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
                            Key = "amenity",
                            Value = "hospital",
                            KeepCoords = true,
                            AttrsReq = new List<string>()
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
                            Key = "healthcare",
                            Value = "hospital",
                            KeepCoords = true,
                            AttrsReq = new List<string>()
                            {
                                "name"
                            },
                            Attrs = new List<string>()
                            {
                                "name"
                            }
                        },
                    }
                },
                {
                    "hotels",
                    new List<SearchEntry>()
                    {
                        new SearchEntry()
                        {
                            Key = "tourism",
                            Value = "hotel",
                            AttrsReq = new List<string>()
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
                            AttrsReq = new List<string>()
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
                            AttrsReq = new List<string>()
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
                    Func<List<SearchEntry>, OsmSharp.Tags.TagsCollectionBase, bool> HasTags = (Source, Tags) =>
                    {
                        foreach (var tag in Source)
                        {
                            if (Tags.Contains(tag.Key, tag.Value)) { return true; };
                        }
                        return false;
                    };
                    #endregion

                    if (File.Exists(sourcePath))
                    {
                        CountdownEvent Countdown1 = new CountdownEvent(2);
                        Thread lThread = new Thread(() =>
                        {
                            using (var fileStream = File.OpenRead(sourcePath))
                            {
                                var filteredPoly = from osmGeo in new PBFOsmStreamSource(fileStream)//.AsParallel()
                                                   where ((osmGeo.Tags != null && HasTags(PolyTagsToMatchIndex, osmGeo.Tags)) || osmGeo.Type == OsmSharp.OsmGeoType.Node) && (osmGeo.Tags != null ? !osmGeo.Tags.ContainsKey("id") : true)
                                                   select osmGeo;
                                #region LineStrings
                                var features = filteredPoly.ToFeatureSource();
                                var lineStrings = from feature in features
                                                  where feature.Geometry is LineString
                                                  select feature;

                                Console.WriteLine("Starting polygons Query on " + FI.Name + " at " + DateTime.Now.ToLongTimeString());
                                foreach (var feature in lineStrings)
                                {
                                    string[] attr_names = feature.Attributes.GetNames();
                                    Poly p = new Poly((long)feature.Attributes["id"]);

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
                                        foreach (var tag in entry.Value)
                                        {
                                            if (attr_names.Contains(tag.Key))
                                            {
                                                if ((string)feature.Attributes[tag.Key] == tag.Value)
                                                {
                                                    // Check if what we found has the required tags
                                                    foreach (var tag1 in tag.AttrsReq)
                                                    {
                                                        if (p.attrs.Find(x => x.Key == tag1).Key == null)
                                                        {
                                                            pass = true;
                                                            break;
                                                        }
                                                    }
                                                    if(pass) { break; }

                                                    p.Calculate();

                                                    if (p.area > tag.MinArea)
                                                    {
                                                        // Filter tags to what we want
                                                        p.attrs = p.attrs.Where(x => tag.Attrs.Contains(x.Key)).ToList();

                                                        // Compute BSON
                                                        var Bson = BsonMapper.Global.ToDocument(p.ToDictionary(tag.KeepCoords));

                                                        // Add to DB
                                                        lock (LiteDbService.DB)
                                                        {
                                                            var DBCollection = LiteDbService.DB.Database.GetCollection(entry.Key);
                                                            DBCollection.Upsert(p.id, Bson);
                                                        }

                                                        Console.WriteLine("Added poly " + entry.Key + " with " + tag.Key + "/" + tag.Value + "/" + p.id + " at " + p.centralNode[0] + "," + p.centralNode[1] + " ");
                                                        pass = true;
                                                        break;
                                                    }
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
                            }
                            Countdown1.Signal();
                        });
                        lThread.Start();

                        Thread nThread = new Thread(() =>
                        {
                            using (var fileStream = File.OpenRead(sourcePath))
                            {
                                var filteredPoint = from osmGeo in new PBFOsmStreamSource(fileStream)//.AsParallel()
                                                    where osmGeo.Tags != null && osmGeo.Type == OsmSharp.OsmGeoType.Node && HasTags(NodeTagsToMatchIndex, osmGeo.Tags)
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
                                        foreach (var tag in entry.Value)
                                        {
                                            if (attr_names.Contains(tag.Key))
                                            {
                                                if ((string)osmGeo.Tags[tag.Key] == tag.Value)
                                                {
                                                    // Filter Tags
                                                    var attrs = node.Tags.Where(x => tag.Attrs.Contains(x.Key)).ToList();

                                                    // Check if what we found has the required tags
                                                    foreach (var tag1 in tag.AttrsReq)
                                                    {
                                                        if (attrs.Find(x => x.Key == tag1).Key == null)
                                                        {
                                                            pass = true;
                                                            break;
                                                        }
                                                    }
                                                    if (pass) { break; }

                                                    // Add to DB
                                                    lock (LiteDbService.DB)
                                                    {
                                                        var Bson = BsonMapper.Global.ToDocument(attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                                                        var DBCollection = LiteDbService.DB.Database.GetCollection(entry.Key);
                                                        DBCollection.Upsert(node.Id, new BsonDocument()
                                                        {
                                                            ["lat"] = Math.Round((double)node.Latitude, 5),
                                                            ["lon"] = Math.Round((double)node.Longitude, 5),
                                                            ["attrs"] = Bson
                                                        });
                                                    }

                                                    Console.WriteLine("Added node " + entry.Key + " with " + tag.Key + "/" + tag.Value + "/" + node.Id + " at " + node.Latitude + "," + node.Longitude + " ");
                                                    pass = true;
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
                            }
                            Countdown1.Signal();
                        });
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
        public string Key = "";
        public string Value = "";
        public List<string> Attrs = new List<string>();
        public List<string> AttrsReq = new List<string>();
        public float MinArea = 400;
        public bool KeepCoords = false;
    }

    class Poly
    {
        public long id = 0;
        public List<Point> coordinates = new List<Point>();
        public float area = 0;
        public float[] centralNode = null;
        public List<List<float>> coordinatesFloat = null;
        public List<KeyValuePair<string, string>> attrs = new List<KeyValuePair<string, string>>();
        public Poly(long id)
        {
            this.id = id;
        }

        public void Calculate()
        {
            if(coordinatesFloat == null)
            {
                coordinatesFloat = coordinates.Select(x => new List<float>() { Convert.ToSingle(Math.Round(x.Coordinate.X, 4)), Convert.ToSingle(Math.Round(x.Coordinate.Y, 4)) }).ToList();
                area = Convert.ToSingle(Math.Round(Area.ComputeSignedArea(coordinatesFloat), 3));

                var fa = new float[1][][];
                fa[0] = PolyLabel.ConvertPolygonToFloatArray(coordinatesFloat);
                centralNode = PolyLabel.GetPolyLabel(fa);
            }
        }

        public Dictionary<string, dynamic> ToDictionary(bool withCoords)
        {
            Dictionary<string, dynamic> d = new Dictionary<string, dynamic>()
            {
                { "lat", centralNode[0] },
                { "lon", centralNode[1] },
                { "area", area },
                { "attrs", attrs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) }
            };

            if(withCoords)
            {
                d.Add("coords", coordinatesFloat);
            }
            return d;
        }
    }
    
}
