using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models;
using System.Globalization;
using TSP_Transponder.Models.Adventures;

namespace TSP_Transponder
{
    public class UserData
    {
        public static MainWindow MW = null;
        public static FileSystemWatcher ConfigWatcher = new FileSystemWatcher();
        private static bool IsSaving = false;
        public static bool IsLoaded = false;
        public static bool dev_available = false;
        public static bool staging_available = false;
        public static bool prod_available = false;
        public static string ConfigPath = Path.Combine(App.AppDataDirectory, "config.json");

        private static Dictionary<string, string> ConfigDic = new Dictionary<string, string>()
        {
            { "token", "" },
            { "ga", "" },
            { "navigraph_access", "" },
            { "navigraph_refresh", "" },
            { "install", "" },
            { "channel", "prod" },
            { "llv", "0" },
            { "llb", "0.0.0" },
            { "rcv", "0.0.0" },
            { "ws_room", "" },
            { "illicit", "0" },
            { "tier", "endeavour" },
            { "discord_presence", "1" },
            { "audio_output", "" },
            { "audio_voices", "0.75" },
            { "audio_effects", "0.75" },
            { "privacy_airport_data", "1" },
            { "privacy_remote_access", "1" },
            { "launch_skypad", "1" },
        };

        public static void Set(string param, string value, bool doSave = false, bool broadcase = true)
        {
            if (Monitor.TryEnter(ConfigDic, 1000))
            {
                if (ConfigDic.ContainsKey(param))
                {
                    ConfigDic[param] = value;
                }
                else
                {
                    ConfigDic.Add(param, value);
                }
                Monitor.Exit(ConfigDic);
                if (doSave)
                {
                    Save();
                }
                if(broadcase && APIBase.ClientCollection != null)
                {
                    APIBase.ClientCollection.SendState();
                }

                switch(param)
                {
                    case "tier":
                        {
                            MW.Shutdown(true);
                            break;
                        }
                }
            };
        }

        public static Dictionary<string, string> GetAll()
        {
            return new Dictionary<string, string>(ConfigDic);
        }

        public static string Get(string param)
        {
            if (Monitor.TryEnter(ConfigDic, 1000))
            {
                if (ConfigDic.ContainsKey(param))
                {
                    Monitor.Exit(ConfigDic);
                    return ConfigDic[param];
                }
                else
                {
                    Monitor.Exit(ConfigDic);
                    return string.Empty;
                }
            }
            else
            {
                Monitor.Exit(ConfigDic);
                return Get(param);
            };
        }

        public static void Remove(string param)
        {
            if (Monitor.TryEnter(ConfigDic, 1000))
            {
                if (ConfigDic.ContainsKey(param))
                {
                    ConfigDic.Remove(param);
                }
                Monitor.Exit(ConfigDic);
            };
        }

        public static bool Startup(MainWindow _MW)
        {
            MW = _MW;
            Load(true);

            //if (!File.Exists(ConfigPath))
            //{
            //    MessageBox.Show("Something went wrong, please reinstall The Skypark");
            //    return false;
            //}

            //ConfigWatcher.Path = Path.GetDirectoryName(ConfigPath);
            //ConfigWatcher.NotifyFilter = NotifyFilters.LastWrite;
            //ConfigWatcher.Filter = Path.GetFileName(ConfigPath);
            //ConfigWatcher.EnableRaisingEvents = true;
            //ConfigWatcher.Changed += (object sender, FileSystemEventArgs e) =>
            //{
            //    if (!IsSaving)
            //    {
            //        Thread.Sleep(1000);
            //        Load();
            //    }
            //};

            APIBase.HTTPCl = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };
            APIBase.HTTPCl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigDic["token"]);

            return true;
        }

        public static void Load(bool init = false)
        {
            Console.WriteLine("Reading Preferences file");
            bool Locked = false;
            if (File.Exists(ConfigPath))
            {
                try
                {
                    using (StreamReader r = new StreamReader(new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var json = r.ReadToEnd();
                        Dictionary<string, string> items = App.JSSerializer.Deserialize<Dictionary<string, string>>(json);

                        if(items != null)
                        {
                            Monitor.Enter(ConfigDic);
                            Locked = true;
                            foreach (KeyValuePair<string, string> parameter in items)
                            {
                                try
                                {
                                    if (!ConfigDic.ContainsKey(parameter.Key))
                                    {
                                        ConfigDic.Add(parameter.Key, "");
                                    }

                                    // Init
                                    if (parameter.Value != ConfigDic[parameter.Key] && !init)
                                    {
                                        switch (parameter.Key)
                                        {
                                            case "privacy_remote_access":
                                                {
                                                    ConfigDic[parameter.Key] = parameter.Value;

                                                    if (APIBase.WSLocal != null)
                                                    {
                                                        APIBase.WSLocal.Disconnect();
                                                        APIBase.WSLocal.Connect();
                                                    }
                                                    if (APIBase.WSRemote != null)
                                                    {
                                                        APIBase.WSRemote.Disconnect();
                                                        APIBase.WSRemote.Connect();
                                                    }
                                                    continue;
                                                }
                                            case "token":
                                                {
                                                    if (parameter.Value != string.Empty)
                                                    {
                                                        ConfigDic[parameter.Key] = parameter.Value;
                                                        //MW.AuthFromToken();
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        MW.Logout();
                                                    }
                                                    break;
                                                }
                                        }
                                    }

                                    // Control Assignments
                                    if (parameter.Key.StartsWith("controls_"))
                                    {
                                        bool passed = true;
                                        List<int> keys = new List<int>();
                                        string[] spl = parameter.Value.Split(',');
                                        foreach (string key in spl)
                                        {
                                            int num = 0;
                                            if (int.TryParse(key, out num))
                                            {
                                                keys.Add(num);
                                            }
                                            else
                                            {
                                                passed = false;
                                                break;
                                            };
                                        }
                                        if (passed)
                                        {
                                            Controller.AddAssignement(keys, parameter.Key, false);
                                        }
                                    }


                                    switch (parameter.Key)
                                    {
                                        case "tier":
                                            {
                                                if (parameter.Value == "prospect")
                                                {
                                                    ConfigDic[parameter.Key] = "endeavour";
                                                    break;
                                                }

                                                ConfigDic[parameter.Key] = parameter.Value;
                                                break;
                                            }
                                        default:
                                            {
                                                if (ConfigDic.ContainsKey(parameter.Key))
                                                {
                                                    ConfigDic[parameter.Key] = parameter.Value;
                                                }
                                                else
                                                {
                                                    ConfigDic.Add(parameter.Key, parameter.Value);
                                                }
                                                break;
                                            }
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Failed to read " + parameter.Key);
                                }
                            }
                            if(Locked)
                            {
                                Monitor.Exit(ConfigDic);
                            }
                        }
                    }

                    MW.UpdatePreferences();
                    IsLoaded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load config file " + ex.Message);
                    if (Locked)
                    {
                        Monitor.Exit(ConfigDic);
                    }
                }
            }

            if(ConfigDic["install"] == string.Empty)
            {
                DirectoryInfo DI = new DirectoryInfo(App.ThisAssemblyDir);
                ConfigDic["install"] = DI.Parent.FullName;
                Save();
            }
        }

        public static void Save()
        {
            if(!IsSaving)
            {
                IsSaving = true;
                Thread SaveThread = new Thread(() =>
                {
                    if (Monitor.TryEnter(ConfigDic, 1000))
                    {
                        string json = App.JSSerializer.Serialize(ConfigDic);
                        Monitor.Exit(ConfigDic);
                        //write string to file
                        try
                        {
                            App.CreateAppdataFolder();
                            File.WriteAllText(ConfigPath, json);

                            //if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\TheSkyPark\")))
                            //{
                            //    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\TheSkyPark\"));
                            //}
                            //File.Copy(ConfigPath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\TheSkyPark\config.json"), true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while saving the Preferences: " + ex.Message);
                        }
                    }
                    else
                    {
                        Save();
                    }

                    IsSaving = false;
                });
                SaveThread.IsBackground = true;
                SaveThread.CurrentCulture = CultureInfo.CurrentCulture;
                SaveThread.Start();
            }
        }

    }
}
