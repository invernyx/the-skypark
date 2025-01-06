using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures
{
    class AdventuresCompile
    {
        private static string DebugExportPath = Path.Combine(App.DocumentsDirectory, "Debug");

        internal static void ExportToNewFormat()
        {
#if DEBUG

            if (!Directory.Exists(DebugExportPath))
            {
                Directory.CreateDirectory(DebugExportPath);
            }

            foreach (var Template in AdventuresBase.Templates.FindAll(x => x.Activated))
            {
                Dictionary<string, dynamic> TempateDict = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(App.JSSerializer.Serialize(Template.InitialStructure));
                List<Dictionary<string, dynamic>> RoutesDict = new List<Dictionary<string, dynamic>>();

                TempateDict.Remove("_id");
                TempateDict.Remove("Loaded");

                #region Situations
                foreach (var Sit in TempateDict["Situations"])
                {
                    List<List<double>> Bounds = new List<List<double>>();
                    foreach (var Bound in Sit["Boundaries"])
                    {
                        Bounds.Add(new List<double>()
                        {
                            Convert.ToDouble(Bound[1]),
                            Convert.ToDouble(Bound[0])
                        });
                    }

                    Sit["Areas"] = new List<dynamic>()
                    {
                        Bounds
                    };
                    Sit.Remove("Boundaries");
                }
                #endregion

                #region Routes
                foreach (var Route in Template.Routes)
                {
                    Dictionary<string, dynamic> d = Route.ToDictionary();
                    RoutesDict.Add(d);
                }
                #endregion

                #region Exports
                try
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(DebugExportPath, Template.FileName + "_Routes.json"), false))
                    {
                        writer.WriteLine(App.JSSerializer.Serialize(RoutesDict));
                    }
                
                    using (StreamWriter writer = new StreamWriter(Path.Combine(DebugExportPath, Template.FileName + "_Template.json"), false))
                    {
                        writer.WriteLine(App.JSSerializer.Serialize(TempateDict));
                    }
                }
                catch
                {
                
                }
                #endregion
            }

#endif
        }

        internal static void ExportToWebsite()
        {
#if DEBUG
            string WebsiteExportPath = Path.Combine(App.DocumentsDirectory, "Website");
            int PerTemplateLimit = 2000;
            List<Dictionary<string, dynamic>> TemplateEntries = new List<Dictionary<string, dynamic>>();
            
            if (!Directory.Exists(WebsiteExportPath))
            {
                Directory.CreateDirectory(WebsiteExportPath);
            }

            foreach (var Template in AdventuresBase.Templates.FindAll(x => x.Activated))
            {
                #region Get Routes
                // Initial structures
                List<dynamic> routes = new List<dynamic>();
                Dictionary<string, int> Countries = new Dictionary<string, int>();
                Dictionary<string, dynamic> TemplateStruct = new Dictionary<string, dynamic>()
                {
                    { "Routes", routes },
                };
                lock (Template.Routes)
                {
                    foreach (var rte in Template.Routes)
                    {
                        routes.Add(rte.ToSummary(5));
                        foreach (var sit in rte.Situations)
                        {
                            if (sit.Airport != null)
                            {
                                if (!Countries.ContainsKey(sit.Airport.Country))
                                    Countries.Add(sit.Airport.Country, 1);
                                else
                                    Countries[sit.Airport.Country]++;
                            }
                        }
                    }
                }

                // Remove above limit
                while (routes.Count > PerTemplateLimit)
                {
                    routes.RemoveAt(Utils.GetRandom(routes.Count));
                }

                // Add entries
                TemplateEntries.Add(new Dictionary<string, dynamic>()
                {
                    { "Name", Template.Name },
                    { "FileName", Template.FileName },
                    { "Count", routes.Count },
                    { "Countries", Countries.OrderByDescending(x => x.Value).Select(x => x.Key) },
                });
                #endregion

                #region Exports
                try
                {
                    using (StreamWriter writer = new StreamWriter(Path.Combine(WebsiteExportPath, Template.FileName + ".json"), false))
                    {
                        writer.WriteLine(App.JSSerializer.Serialize(TemplateStruct));
                    }
                }
                catch
                {
                }
                #endregion
            }
            
            Dictionary<string, dynamic> TemplatesStruct = new Dictionary<string, dynamic>()
            {
                { "Templates", TemplateEntries },
            };

            #region Exports
            try
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(WebsiteExportPath, "templates.json"), false))
                {
                    writer.WriteLine(App.JSSerializer.Serialize(TemplatesStruct));
                }
            }
            catch
            {
            }
            #endregion

#endif
        }

    }
}
