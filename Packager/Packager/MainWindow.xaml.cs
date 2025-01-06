using Packager.Models;
using Packager.Models.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Packager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _LockPack = false;
        public bool LockPack
        {
            get { return _LockPack; }
            set
            {
                _LockPack = value;
                Dispatcher.Invoke(() =>
                {
                    pack_app_btn.IsEnabled = !_LockPack;
                    pack_adv_btn.IsEnabled = !_LockPack;
                });
            }
        }
        public string TargetVersion = "2";
        public bool NewVersion = false;
        public List<string> Processes = new List<string>();
        private bool _IsIdle = false;
        private bool IsIdle
        {
            get { return _IsIdle; }
            set
            {
                _IsIdle = value;
                Dispatcher.Invoke(() =>
                {
                    if(!_LockPack)
                    {
                        pack_app_btn.IsEnabled = _IsIdle;
                        pack_adv_btn.IsEnabled = _IsIdle;
                    }
                    settings_btn.IsEnabled = _IsIdle;
                    foreach (var Btn in ActionAppButtons.Children)
                    {
                        ((Button)Btn).IsEnabled = _IsIdle;
                    }
                    foreach (var Btn in ActionAdvButtons.Children)
                    {
                        ((Button)Btn).IsEnabled = _IsIdle;
                    }
                    option_version.IsEnabled = _IsIdle;
                    option_newVersion.IsEnabled = _IsIdle;
                });
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            // Init UI
            foreach(var item in option_version.Items)
            {
                if((string)((ComboBoxItem)item).Tag == TargetVersion)
                {
                    option_version.SelectedItem = item;
                    return;
                }
            }

        }

        internal void SetLock(string p, bool s)
        {
            lock (Processes)
            {
                if(s)
                {
                    Processes.Add(p);
                }
                else
                {
                    Processes.Remove(p);
                }
                IsIdle = Processes.Count == 0;
            }
        }

        internal void Log(string id, string Line, Color? BgColor = null, bool IsTitle = false)
        {
            Dispatcher.Invoke(() =>
            {
                var ConsoleEl = GetConsole(id);

                string OutputLine = "";
            
                if (IsTitle)
                {
                    OutputLine += "--- ";
                }
                OutputLine += Line;
                if (IsTitle)
                {
                    OutputLine += " ---";
                }
                
                Run TextRun = new Run(OutputLine + Environment.NewLine);
                if (IsTitle)
                {
                    TextRun.FontWeight = FontWeights.Bold;
                }

                if (BgColor != null)
                {
                    TextRun.Foreground = new SolidColorBrush((Color)BgColor);
                }

                if (ConsoleEl.Key.CaretPosition == ConsoleEl.Key.Selection.End)
                {
                    ConsoleEl.Key.ScrollToEnd();
                }
            
                double CharWidth = TextRun.FontSize * TextRun.Text.Length;
                if(ConsoleEl.Key.Document.PageWidth < CharWidth)
                {
                    ConsoleEl.Key.Document.PageWidth = CharWidth;
                }
                
                ConsoleEl.Value.Inlines.Add(TextRun);
            });
        }

        internal KeyValuePair<RichTextBox,Paragraph> GetConsole(string name)
        {
            RichTextBox rtb = null;

            foreach(RichTextBox Child in ConsoleBox.Children)
            {
                if(Child.Uid == name)
                {
                    rtb = Child;
                    break;
                }
            }

            Paragraph p = null;

            if (rtb == null)
            {
                //p = new Paragraph(new Run("New Console"));

                rtb = new RichTextBox()
                {
                    Background = new SolidColorBrush(Color.FromArgb(0xff, 0x11, 0x11, 0x22)),
                    Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF)),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(5,0,5,0),
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 16,
                    IsReadOnly = true,
                    Uid = name,
                };

                rtb.PreviewMouseRightButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                {
                    int ind = ConsoleBox.Children.IndexOf(rtb);
                    ConsoleBox.Children.Remove(rtb);
                    ConsoleBox.ColumnDefinitions.RemoveAt(0);
                    while (ind < ConsoleBox.Children.Count)
                    {
                        Grid.SetColumn(ConsoleBox.Children[ind], ind);
                        ind++;
                    }
                };

                rtb.Document.PageWidth = 250;
                //rtb.Document.Blocks.Add(p);

                ConsoleBox.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(rtb, ConsoleBox.ColumnDefinitions.Count - 1);

                ConsoleBox.Children.Add(rtb);
            }

            p = ((Paragraph)rtb.Document.Blocks.LastBlock);

            return new KeyValuePair<RichTextBox, Paragraph>(rtb, p);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] Actions = ((Button)sender).Uid.Split('_');

            Thread ActionThread = new Thread(() =>
            {
                switch (Actions[0])
                {
                    case "clear":
                        {
                            Dispatcher.Invoke(() =>
                            {
                            //ConsoleBox.Document.Blocks.Clear();
                            //ConsoleBox.Document.Blocks.Add(new Paragraph());
                            App.ReadSettings();
                            });
                            LockPack = false;
                            break;
                        }
                    case "settings":
                        {
                            #region Settings
                            switch (Actions[1])
                            {
                                case "edit":
                                    {
                                        string IniPath = Path.Combine(App.AssemblyPath, "settings.ini");
                                        App.Exec("", IniPath, "");
                                        App.ReadSettings();
                                        break;
                                    }
                                case "output":
                                    {
                                        Process.Start("explorer.exe", App.BasePath);
                                        break;
                                    }
                            }
                            #endregion
                            break;
                        }
                    case "pub":
                        {
                            #region Publish
                            //SetLock("pub", true);
                            Dispatcher.Invoke(() =>
                            {
                                ((Button)sender).IsEnabled = false;
                            });

                            App.ExecWithConsole("pub_" + Actions[1] + Actions[2], "cmd", "gcloud config configurations activate p42cdn");

                            List<string> Packs = null;

                            switch (Actions[1])
                            {
                                case "app":
                                    {
                                        Packs = new List<string>()
                                        {
                                            "manifests",
                                            "transponder",
                                            "skypad",
                                            "skyos",
                                            //"content"
                                        };
                                        break;
                                    }
                                case "adv":
                                    {
                                        Packs = new List<string>()
                                        {
                                            "adventures",
                                            "adventures_manifests",
                                        };
                                        break;
                                    }

                            }

                            // Publish all packages
                            Parallel.ForEach(Packs, (package) =>
                            {
                                switch (Actions[2])
                                {
                                    default:
                                        {
                                            string Source = Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], package + ".zip");
                                            string Output = Path.Combine(App.PublishPaths[TargetVersion], Actions[2], package + ".zip");

                                            App.CopyFile("pub_" + Actions[1] + Actions[2], Source, Output);

                                            switch (package)
                                            {
                                                case "skyos":
                                                    {
                                                        App.CopyFile("pub_" + Actions[1] + Actions[2], Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], "changes.json"), Path.Combine(App.PublishPaths[TargetVersion], Actions[2], "changes.json"));
                                                        break;
                                                    }
                                                case "transponder":
                                                    {
                                                        App.CopyFile("pub_" + Actions[1] + Actions[2], Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], "v.txt"), Path.Combine(App.PublishPaths[TargetVersion], Actions[2], "v.txt"));
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case "prod":
                                        {
                                            string Source = Path.Combine(App.PublishPaths[TargetVersion], "rc", package + ".zip");
                                            string Output = Path.Combine(App.PublishPaths[TargetVersion], Actions[2], package + ".zip");

                                            App.CopyFile("pub_prod", Source, Output);

                                            switch (package)
                                            {
                                                case "skyos":
                                                    {
                                                        App.CopyFile("pub_prod", Path.Combine(App.PublishPaths[TargetVersion], "rc", "changes.json"), Path.Combine(App.PublishPaths[TargetVersion], Actions[2], "changes.json"));
                                                        break;
                                                    }
                                                case "transponder":
                                                    {
                                                        App.CopyFile("pub_prod", Path.Combine(App.PublishPaths[TargetVersion], "rc", "v.txt"), Path.Combine(App.PublishPaths[TargetVersion], Actions[2], "v.txt"));
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }

                            });

                            switch (TargetVersion)
                            {
                                case "2":
                                    {
                                        if (App.ExecWithConsole("pub_" + Actions[1] + Actions[2], "cmd", "gsutil -m rsync -r \"" + Path.Combine(App.PublishPaths[TargetVersion], Actions[2]) + "\" gs://gilfoyle/the-skypark/v1/" + Actions[2]) == 0)
                                        {
                                        };
                                        break;
                                    }
                                case "3":
                                    {
                                        if (App.ExecWithConsole("pub_" + Actions[1] + Actions[2], "cmd", "gsutil -m rsync -r \"" + Path.Combine(App.PublishPaths[TargetVersion], Actions[2]) + "\" gs://gilfoyle/the-skypark/v3/" + Actions[2]) == 0)
                                        {
                                        };
                                        break;
                                    }
                            }

                            Dispatcher.Invoke(() =>
                            {
                                ((Button)sender).IsEnabled = true;
                            });
                            //SetLock("pub", false);
                            #endregion
                            break;
                        }
                    case "pack":
                        {
                            #region Pack
                            SetLock("pub", true);
                            SetLock("pack", true);

                            List<string> Packs = null;
                            switch (Actions[1])
                            {
                                case "app":
                                    {
                                        Packs = new List<string>()
                                        {
                                            "manifests",
                                            "topo",
                                            "transponder",
                                            "skypad",
                                            "skyos",
                                            //"content"
                                        };
                                        break;
                                    }
                                case "adv":
                                    {
                                        Packs = new List<string>()
                                        {
                                            "adventures_manifests",
                                            "adventures",
                                        };
                                        break;
                                    }
                            }

                            Parallel.ForEach(Packs, (package) =>
                            {
                                string Source = Path.Combine(App.BasePath, "Dist", TargetVersion, "Output", package);
                                string Output = Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], package + ".zip");
                                string Lock = Path.Combine(Source, "lock");
                                if ((!File.Exists(Lock) || !File.Exists(Output) || package.Contains("manifest")) && Directory.Exists(Source))
                                {
                                    Log("pack_" + Actions[1], "Compressing " + package + "...");
                                    try
                                    {
                                        if (File.Exists(Lock)) { File.Delete(Lock); }
                                        if (File.Exists(Output)) { File.Delete(Output); }
                                        ZipFile.CreateFromDirectory(Source, Output, CompressionLevel.Optimal, false);
                                        File.CreateText(Path.Combine(Source, "lock"));
                                    }
                                    catch
                                    {
                                    }

                                    if (Actions[1] == "app" && package == "skyos")
                                    {
                                        App.CopyFile("pack_" + Actions[1], Path.Combine(App.BasePath, "Dist", TargetVersion, "Output", "changes.json"), Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], "changes.json"));
                                    }

                                    if (Actions[1] == "app" && package == "transponder")
                                    {
                                        App.CopyFile("pack_" + Actions[1], Path.Combine(App.BasePath, "Dist", TargetVersion, "Output", "v.txt"), Path.Combine(App.BasePath, "Dist", TargetVersion, "Zip", Actions[1], "v.txt"));
                                    }
                                    Log("pack_" + Actions[1], "Done compressing " + package + ".");
                                }
                            });

                            Log("pack_" + Actions[1], "DONE", Color.FromRgb(0, 255, 0), true);

                            SetLock("pack", false);
                            SetLock("pub", false);
                            #endregion
                            break;
                        }
                    case "compile":
                        {
                            #region Compile
                            SetLock("compile-" + Actions[1], true);
                            ActionBase Script = null;
                            switch (Actions[1])
                            {
                                case "tsp-topo":
                                    {
                                        Script = new tsp_topo();
                                        break;
                                    }
                                case "tsp-transponder":
                                    {
                                        Script = new tsp_transponder();
                                        break;
                                    }
                                case "tsp-content":
                                    {
                                        Script = new tsp_content();
                                        break;
                                    }
                                case "tsp-skypad-electron":
                                    {
                                        Script = new tsp_skypad_electron();
                                        break;
                                    }
                                case "tsp-skyos":
                                    {
                                        Script = new tsp_skyos();

                                        ActionBase Script1 = new tsp_images();
                                        Script1.Process();
                                        break;
                                    }
                                case "tsp-adventures":
                                    {
                                        ActionBase Script1 = new tsp_sounds();
                                        Script1.Process();

                                        ActionBase Script2 = new tsp_images();
                                        Script2.Process();

                                        Script = new tsp_adventures();
                                        break;
                                    }
                                case "tsp-airports":
                                    {
                                        Script = new tsp_airports();
                                        break;
                                    }
                                case "tsp-images":
                                    {
                                        Script = new tsp_images();
                                        break;
                                    }
                                case "tsp-sounds":
                                    {
                                        Script = new tsp_sounds();
                                        break;
                                    }
                            }
                            Script.Process();
                            SetLock("compile-" + Actions[1], false);
                            #endregion
                            break;
                        }
                    case "download":
                        {
                            switch (Actions[1])
                            {
                                case "holidays":
                                    {
                                        Log("download_holidays", "Starting up...");
                                        int year = 2021;
                                        int countriesLeft = 9999;
                                        int timeout = 1300;

                                        string h_countries_path = Path.Combine(App.AssemblyPath, "holidays", "holiday_countries.json");
                                        string h_countries_json = null;
                                        dynamic h_countries = null;

                                        Func<string, float> getMulti = (type) =>
                                        {
                                            switch (type)
                                            {
                                                case "National holiday":
                                                    {
                                                        return 1.1f;
                                                    }
                                                case "Local holiday":
                                                    {
                                                        return 1.05f;
                                                    }
                                                default:
                                                    {
                                                        return 0;
                                                    }
                                            }
                                        };

                                        List<Dictionary<string, dynamic>> output = new List<Dictionary<string, dynamic>>();

                                        new FileRequest(
                                            (percent, total, received) =>
                                            {
                                            },
                                            () =>
                                            {
                                                Log("download_holidays", "Countries Downloaded.");

                                                h_countries_json = string.Join(" ", File.ReadAllLines(h_countries_path));
                                                h_countries = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(h_countries_json);

                                                countriesLeft = h_countries["response"]["countries"].Count;

                                            // Check existing
                                            foreach (var country in h_countries["response"]["countries"])
                                                {
                                                    string h_country_path = Path.Combine(App.AssemblyPath, "holidays", (string)country["iso-3166"] + ".json");
                                                    if (File.Exists(h_country_path))
                                                    {
                                                        countriesLeft--;
                                                    }
                                                }

                                                var tDiff = DateTime.Now.AddMilliseconds(timeout * countriesLeft);
                                                Log("download_holidays", "Will be done by " + tDiff.ToString());

                                                if (countriesLeft > 0)
                                                {
                                                    Thread.Sleep(timeout);
                                                }

                                                foreach (var country in h_countries["response"]["countries"])
                                                {
                                                    string h_country_path = Path.Combine(App.AssemblyPath, "holidays", (string)country["iso-3166"] + ".json");
                                                    if (!File.Exists(h_country_path))
                                                    {
                                                        Thread.Sleep(timeout);
                                                        Log("download_holidays", "Downloading " + country["country_name"], Color.FromRgb(0, 0, 255));
                                                        new FileRequest(
                                                          (percent, total, received) =>
                                                          {
                                                          },
                                                          () =>
                                                          {
                                                              tDiff = DateTime.Now.AddMilliseconds(timeout * countriesLeft);
                                                              Log("download_holidays", "Downloaded " + country["country_name"] + ". Will be done in " + (tDiff - DateTime.Now).TotalSeconds + " sec", Color.FromRgb(255, 0, 255));
                                                              countriesLeft--;
                                                          },
                                                          (ex) =>
                                                          {
                                                              Log("download_holidays", "Failed to download " + (string)country["iso-3166"] + "... " + ex.Message, Color.FromRgb(255, 0, 0));
                                                          },
                                                          "https://calendarific.com/api/v2/holidays?api_key=15430f7f3db50800776da11a60fb4435a7ba6afd&country=" + (string)country["iso-3166"] + "&year=" + year,
                                                          h_country_path
                                                      );
                                                    }
                                                }

                                            },
                                            (ex) =>
                                            {
                                                Log("download_holidays", "Failed to download Countries... " + ex.Message, Color.FromRgb(255, 0, 0));
                                            },
                                            "https://calendarific.com/api/v2/countries?api_key=15430f7f3db50800776da11a60fb4435a7ba6afd",
                                            h_countries_path
                                        );

                                        while (countriesLeft > 0)
                                        {
                                            Thread.Sleep(100);
                                        }

                                        if (h_countries != null)
                                        {
                                            foreach (var country in h_countries["response"]["countries"])
                                            {
                                                string h_country_path = Path.Combine(App.AssemblyPath, "holidays", (string)country["iso-3166"] + ".json");

                                                var h_country_json = string.Join(" ", File.ReadAllLines(h_country_path));
                                                var h_country = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(h_country_json);

                                                foreach (var holiday in h_country["response"]["holidays"])
                                                {
                                                    bool skip = false;
                                                    foreach (string type in ((ArrayList)holiday["type"]))
                                                    {
                                                        switch (type)
                                                        {
                                                            // DONT
                                                            case "Season":
                                                            case "Clock change/Daylight Saving Time":
                                                            case "Common local holiday":
                                                            case "Worldwide observance":
                                                            case "United Nations observance":
                                                            case "Sporting event":
                                                            case "Weekend":
                                                            // Maybes
                                                            case "De facto holiday":
                                                            case "Half-day holiday":
                                                            case "Hinduism":
                                                            case "Christian":
                                                            case "Muslim":
                                                            case "Hebrew":
                                                            case "Flag day":
                                                            case "Observance":
                                                            case "Local observance":
                                                            case "Orthodox":
                                                            case "Optional holiday":
                                                                {
                                                                    break;
                                                                }
                                                            // DO
                                                            case "Local holiday":
                                                            case "National holiday":
                                                                {
                                                                    var d = DateTime.Parse((string)holiday["date"]["iso"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                                                                    Dictionary<string, dynamic> newEntry = null;

                                                                    if (output.Find(x => x["Name"] == holiday["name"] && x["Country"] == ((string)holiday["country"]["id"]).ToUpper() && x["DayOfYear"] == d.DayOfYear) == null)
                                                                    {
                                                                        newEntry = new Dictionary<string, dynamic>()
                                                                        {
                                                                            { "Name", holiday["name"] },
                                                                            { "Multiplier", getMulti(type) },
                                                                            { "Type", type },
                                                                            { "Country", ((string)holiday["country"]["id"]).ToUpper() },
                                                                            { "DayOfYear", d.DayOfYear }
                                                                        };
                                                                    }

                                                                    if (newEntry != null)
                                                                    {
                                                                        foreach (var existing in output.FindAll(x => x["Country"] == ((string)holiday["country"]["id"]).ToUpper()))
                                                                        {
                                                                            var matches1 = Regex.Matches((string)existing["Name"].ToLower(), Regex.Escape((string)holiday["name"].ToLower()));
                                                                            var matches2 = Regex.Matches((string)holiday["name"].ToLower(), Regex.Escape((string)existing["Name"].ToLower()));

                                                                            if (matches1.Count > 0 || matches2.Count > 0)
                                                                            {
                                                                                if (newEntry["Name"] == existing["Name"])
                                                                                {
                                                                                    if (newEntry["DayOfYear"] == existing["DayOfYear"])
                                                                                    {
                                                                                        output.Remove(existing);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (((string)newEntry["Name"]).Length > ((string)existing["Name"]).Length)
                                                                                    {
                                                                                        skip = true;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        output.Remove(existing);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        if (!skip)
                                                                        {
                                                                            output.Add(newEntry);
                                                                        }
                                                                    }

                                                                    skip = true;
                                                                    break;
                                                                }
                                                            default:
                                                                {
                                                                    break;
                                                                }
                                                        }
                                                        if (skip)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    if (skip)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }

                                            using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\holidays.json", false))
                                            {
                                                writer.WriteLine(App.JSSerializer.Serialize(output));
                                            }


                                        }

                                        Log("download_holidays", "Done!", Color.FromRgb(0, 255, 0));

                                        break;
                                    }
                            }
                            break;
                        }
                }
            });
            ActionThread.Start();

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string[] Actions = ((CheckBox)sender).Uid.Split('_');
            switch (Actions[0])
            {
                case "option":
                    {
                        switch (Actions[1])
                        {
                            case "newVersion":
                                {
                                    NewVersion = (bool)((CheckBox)sender).IsChecked;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] Actions = ((ComboBox)sender).Uid.Split('_');
            switch (Actions[0])
            {
                case "option":
                    {
                        switch (Actions[1])
                        {
                            case "version":
                                {
                                    TargetVersion = (string)((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag;
                                    switch (TargetVersion)
                                    {
                                        case "2":
                                            {
                                                App.CGNPath = "gs://gilfoyle/the-skypark/v1";
                                                break;
                                            }
                                        case "3":
                                            {
                                                App.CGNPath = "gs://gilfoyle/the-skypark/v3";
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
    }
}
