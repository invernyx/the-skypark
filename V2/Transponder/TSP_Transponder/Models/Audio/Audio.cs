using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.API.APIBase;
using static TSP_Transponder.Models.Audio.AudioLibrary;

namespace TSP_Transponder.Models.Audio
{
    class AudioFramework
    {
        private static string AudioDir = Path.Combine(App.AppDataDirectory, "Sounds");
        private static List<string> CharacterSpeechQueue = new List<string>();
        private static List<string> CharacterSpeechDownloading = new List<string>();
        private static List<KeyValuePair<string, Action>> CharacterSpeechPlayAfterDownload = new List<KeyValuePair<string, Action>>();
        private static WaveOutEvent SpeechAudio = new WaveOutEvent();
        private static List<WaveOutEvent> EffectAudios = new List<WaveOutEvent>();
        private static DateTime Date = DateTime.UtcNow;

        public static void Startup()
        {
            GetEffect("clickmic");

            GetEffect("failed");
            GetEffect("completed");
            GetEffect("kaching");

            GetEffect("cargoload");
            GetEffect("cargounload");


            GetSpeech("characters", "brigit/load");
            GetSpeech("characters", "brigit/good_day");
            GetSpeech("characters", "larry/loaded");
            GetSpeech("characters", "brigit/welcome_big_day");
            GetSpeech("characters", "pablo/deliver_some_not_so_good");
            
        }

        public static int GetDeviceIndex(string name)
        {
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                try
                {
                    var caps = WaveOut.GetCapabilities(n);
                    if (caps.ProductName == name)
                    {
                        return n;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return -1;
        }

        public static bool GetIsAudible()
        {
            if (SimConnection.SimHasSound) //  && SimConnection.ActiveSim != null
            {
                return true;
            }
            return false;
        }

        public static void SetSpeechVolume(float volume)
        {
            try
            {
                SpeechAudio.Volume = volume;
            }
            catch
            {

            }
        }

        public static void SetEffectVolume(float volume)
        {
            lock(EffectAudios)
            {
                foreach (WaveOutEvent ev in EffectAudios)
                {
                    try
                    {
                        ev.Volume = volume;
                    }
                    catch
                    {

                    }
                }
            }
        }

        public static WaveOutEvent GetSpeech(string Type, string RootFile, bool Play = false, bool Loop = false, bool Override = false, Action DoneCallback = null, Action<string, string, string> TranscriptCallback = null)
        {
            if(Type == string.Empty || RootFile == string.Empty)
            {
                return SpeechAudio;
            }

            string FileRef = Type + ":" + RootFile;

            try
            {
                string CDNBase = Path.Combine(CDNHost, "common", "sounds");

                int Depth = 0;
                int PlaySlot = 0;
                Action<int, string, string, Action<string>, Action<string>> ReadManifest = null;
                Action<int, string, Action<string>, Action<string>> ManifestDL = null;

                if (Play)
                {
                    lock (CharacterSpeechPlayAfterDownload)
                    {
                        CharacterSpeechPlayAfterDownload.Add(new KeyValuePair<string, Action>(FileRef, DoneCallback));
                    }
                }

                lock(CharacterSpeechDownloading)
                {
                    if(!CharacterSpeechDownloading.Contains(FileRef))
                    {
                        CharacterSpeechDownloading.Add(FileRef);
                    }
                    else
                    {
                        return SpeechAudio;
                    }
                }

                #region Read Manifest
                ReadManifest = new Action<int, string, string, Action<string>, Action<string>>((Index, Route, FilePath, Prep, Run) =>
                {
                    if (Depth > 10)
                    {
                        return;
                    }

                    string manifestJSON = File.ReadAllText(FilePath);
                    Dictionary<string, dynamic> manifestDict = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(manifestJSON);
                    
                    Action<string> PlayFn = (Path) =>
                    {
                        lock (CharacterSpeechPlayAfterDownload)
                        {
                            foreach(var Callback in CharacterSpeechPlayAfterDownload.FindAll(x => x.Key == FileRef).ToList())
                            {
                                CharacterSpeechPlayAfterDownload.Remove(Callback);
                                TranscriptCallback?.Invoke((string)manifestDict["Cast"], Route, (string)manifestDict["Caption"]["EN"]);
                                Run(Path);
                            }
                        }
                    };
                    
                    if(manifestDict != null)
                    {
                        if (manifestDict.ContainsKey("Type"))
                        {
                            switch (manifestDict["Type"])
                            {
                                case "ROUTE":
                                    {
                                        Index = 0;
                                        PlaySlot = Utils.GetRandom(((ArrayList)manifestDict["Paths"]).Count);
                                        foreach (var entry in manifestDict["Paths"])
                                        {
                                            string newRoute = "";
                                            if (entry.StartsWith("../"))
                                            {
                                                newRoute = entry.Replace("../", "");
                                            }
                                            else
                                            {
                                                List<string> RouteSpl = Route.Split("\\/".ToCharArray()).ToList();
                                                RouteSpl.RemoveAt(RouteSpl.Count - 1);
                                                newRoute = string.Join("/", RouteSpl) + "/" + entry;
                                            }

                                            Uri newURL = new Uri(Path.Combine(CDNBase, Type, newRoute + ".json"));
                                            FileInfo newLocalPath = new FileInfo(Path.Combine(AudioDir, Type, newRoute + ".json"));

                                            FileInfo fi = new FileInfo(newLocalPath.FullName);
                                            if (fi.Exists && fi.Length > 0)
                                            {
                                                ReadManifest(Index, newRoute, newLocalPath.FullName, Prep, Run);
                                            }
                                            else
                                            {
                                                ManifestDL(Index, newRoute, Prep, Run);
                                            }
                                            Index++;
                                        }
                                        break;
                                    }
                                case "CAPTION":
                                    {
                                        Uri newURL = new Uri(Path.Combine(CDNBase, Type, Route + ".mp3"));
                                        FileInfo newLocalPath = new FileInfo(Path.Combine(AudioDir, Type, Route + ".mp3"));
                                        Prep(newLocalPath.FullName);

                                        if (!File.Exists(newLocalPath.FullName))
                                        {
                                            new FileRequest((percent, total, received) => { },
                                                () =>
                                                {
                                                    if (PlaySlot == Index && Play)
                                                    {
                                                        PlayFn(newLocalPath.FullName);
                                                    }

                                                    lock (CharacterSpeechDownloading)
                                                    {
                                                        CharacterSpeechDownloading.Remove(FileRef);
                                                    }
                                                },
                                                (ex) =>
                                                {
                                                    Console.WriteLine("Failed to download audio file: " + newLocalPath.FullName + " - " + ex.Message);
                                                    Run(null);
                                                },
                                                newURL.AbsoluteUri + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute,
                                                newLocalPath.FullName.ToLower()
                                            );
                                        }
                                        else
                                        {
                                            if (PlaySlot == Index && Play)
                                            {
                                                PlayFn(newLocalPath.FullName);
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Manifets for " + FilePath + " is empty");
                    }

                    lock (CharacterSpeechDownloading)
                    {
                        CharacterSpeechDownloading.Remove(FileRef);
                    }

                    Depth++;
                });
                #endregion

                #region Download Manifest
                ManifestDL = new Action<int, string, Action<string>, Action<string>>((Index, Route, Prep, Run) =>
                {
                    Uri URL = new Uri(Path.Combine(CDNBase, Type, Route + ".json"));
                    FileInfo LocalPath = new FileInfo(Path.Combine(AudioDir, Type, Route + ".json"));

                    #region Check and Download
                    FileInfo fi = new FileInfo(LocalPath.FullName);
                    if (fi.Exists && fi.Length > 0)
                    {
                        ReadManifest(Index, Route, LocalPath.FullName, Prep, Run);
                    }
                    else
                    {
                        new FileRequest((percent, total, received) => { },
                        () =>
                        {
                            ReadManifest(Index, Route, LocalPath.FullName, Prep, Run);
                        },
                        (ex) =>
                        {

                        },
                        URL.AbsoluteUri + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute,
                        LocalPath.FullName);
                    }
                    #endregion
                });
                #endregion

                #region Run the thing
                Thread SoundThread = null;
                ManifestDL(0, RootFile, (Route) =>
                {
                    if(Route == null)
                    {
                        DoneCallback();
                        return;
                    }

                    CultureInfo.CurrentCulture = App.CI;

                    #region Set Volume
                    float volume = (float)Math.Round(Convert.ToDouble(UserData.Get("audio_voices")), 3);
                    if (Override && volume == 0)
                    {
                        volume = 0.5f;
                    }
                    #endregion

                    #region Prepare Thread
                    SoundThread = new Thread(delegate ()
                    {
                        CultureInfo.CurrentCulture = App.CI;

                        try
                        {
                            DateTime RequestTime = DateTime.UtcNow;
                            while (CharacterSpeechQueue[0] != Route) // && (DateTime.UtcNow - RequestTime < new TimeSpan(0, 0, 30) && SpeechAudio.PlaybackState == PlaybackState.Stopped)
                            {
                                Thread.Sleep(100);
                                if(App.MW.IsShuttingDown)
                                {
                                    return;
                                }
                            }

                            if ((GetIsAudible() && UserData.Get("audio_voices") != "0") || Override)
                            {
                                Console.WriteLine("Playing audio: " + Route);

                                SpeechAudio.Dispose();
                                SpeechAudio = new WaveOutEvent();
                                AudioFileReader AudioFile = new AudioFileReader(Route);
                                SpeechAudio.DeviceNumber = GetDeviceIndex(UserData.Get("audio_output"));
                                SpeechAudio.Init(AudioFile);
                                SpeechAudio.Volume = volume;
                                
                                App.MW.SetSpeechUI(true, "");
                                bool Stopped = false;
                                SpeechAudio.PlaybackStopped += (sender ,ev) =>
                                {
                                    if (!Stopped)
                                    {
                                        GetEffect("clickmic", true);
                                        switch(Type)
                                        {
                                            case "characters":
                                            case "adventures":
                                                {
                                                    var filePathSpl = RootFile.Split('/');
                                                    GoogleAnalyticscs.TrackEvent("Speech", filePathSpl[0], filePathSpl[1]);
                                                    break;
                                                }
                                        }
                                        Stopped = true;
                                        try
                                        {
                                            lock (CharacterSpeechQueue)
                                            {
                                                CharacterSpeechQueue.Remove(Route);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                        App.MW.SetSpeechUI(false);
                                        DoneCallback?.Invoke();
                                    }
                                };
                                Thread.Sleep(300);
                                GetEffect("clickmic", true);
                                Thread.Sleep(300);
                                SpeechAudio.Play();

                                Stopwatch sw = new Stopwatch();
                                sw.Start();
                                while ((SpeechAudio.PlaybackState != PlaybackState.Stopped && sw.ElapsedMilliseconds < (int)AudioFile.TotalTime.TotalMilliseconds + 5000) && !App.MW.IsShuttingDown && !Stopped)
                                {
                                    Thread.Sleep(100);
                                }

                                if (!Stopped)
                                {
                                    Stopped = true;
                                    lock (CharacterSpeechQueue)
                                    {
                                        CharacterSpeechQueue.Remove(Route);
                                    }
                                    App.MW.SetSpeechUI(false);
                                    DoneCallback?.Invoke();
                                }

                            }
                            else
                            {
                                lock (CharacterSpeechQueue)
                                {
                                    CharacterSpeechQueue.Remove(Route);
                                }
                                App.MW.SetSpeechUI(false);
                                DoneCallback?.Invoke();
                            }

                            //APIBase.ClientCollection.SendMessage(App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            //{
                            //    { "name", "messages:show" },
                            //    { "meta", null },
                            //    { "payload", App.JSSerializer.Serialize(new List<Dictionary<string, dynamic>>()
                            //        {
                            //            new Dictionary<string, dynamic>()
                            //            {
                            //                { "user_name", Output_Character.Name },
                            //                { "user_id", Output_Character.ID },
                            //                { "time", Utils.TimeStamp(DateTime.UtcNow) },
                            //                { "type", "audio" },
                            //                { "duration", duration },
                            //                { "message", Output_File.Caption },
                            //                { "hide", false },
                            //            }
                            //        })
                            //    },
                            //    
                            //}), ClientType.Overlay);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to play sound " + ex.Message);
                            lock (CharacterSpeechQueue)
                            {
                                CharacterSpeechQueue.Remove(Route);
                            }
                            App.MW.SetSpeechUI(false);
                            DoneCallback?.Invoke();
                        }
                    })
                    {
                        Priority = ThreadPriority.Highest,
                        CurrentCulture = CultureInfo.CurrentCulture
                    };
                    SoundThread.SetApartmentState(ApartmentState.MTA);
                    #endregion
                },
                (Route) =>
                {
                    // Add to Queue
                    lock (CharacterSpeechQueue)
                    {
                        if (!CharacterSpeechQueue.Contains(Route))
                        {
                            CharacterSpeechQueue.Add(Route);
                        }
                        else
                        {
                            return;
                        }
                    }

                    SoundThread.Start();
                });
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to play audio for " + Type + " / " + RootFile + ": " + ex.Message);

                lock (CharacterSpeechPlayAfterDownload)
                {
                    var waitings = CharacterSpeechPlayAfterDownload.FindAll(x => x.Key == FileRef);
                    foreach(var waiting in waitings)
                    {
                        waiting.Value();
                    }
                }
            }

            return SpeechAudio;
        }

        public static WaveOutEvent GetEffect(string RootFile, bool Play = false, int Delay = 0, bool Loop = false, bool Override = false)
        {
            WaveOutEvent EffectAudio = new WaveOutEvent();

            try
            {
                string CDNBase = Path.Combine(CDNHost, "common", "sounds");

                int Depth = 0;
                int PlaySlot = 0;
                Action<int, string, string, Action<string>, Action<string>> ReadManifest = null;
                Action<int, string, Action<string>, Action<string>> ManifestDL = null;

                #region Read Manifest
                ReadManifest = new Action<int, string, string, Action<string>, Action<string>>((Index, Route, FilePath, Prep, Run) =>
                {
                    if (Depth > 10)
                    {
                        return;
                    }
                    
                    string manifestJSON = File.ReadAllText(FilePath);
                    Dictionary<string, dynamic> manifestDict = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(manifestJSON);

                    if(manifestDict != null)
                    {
                        if (manifestDict.ContainsKey("Type"))
                        {
                            switch (manifestDict["Type"])
                            {
                                case "ROUTE":
                                    {
                                        PlaySlot = Utils.GetRandom(((ArrayList)manifestDict["Paths"]).Count);
                                        foreach (var entry in manifestDict["Paths"])
                                        {
                                            string newRoute = "";
                                            if (entry.StartsWith("../"))
                                            {
                                                newRoute = entry.Replace("../", "");
                                            }
                                            else
                                            {
                                                List<string> RouteSpl = Route.ToLower().Split("\\/".ToCharArray()).ToList();
                                                RouteSpl.RemoveAt(RouteSpl.Count - 1);
                                                newRoute = string.Join("/", RouteSpl) + "/" + entry;
                                            }

                                            Uri newURL = new Uri(Path.Combine(CDNBase, "effects", newRoute + ".json"));
                                            FileInfo newLocalPath = new FileInfo(Path.Combine(AudioDir, "effects", newRoute + ".json"));

                                            FileInfo fi = new FileInfo(newLocalPath.FullName);
                                            if (fi.Exists && fi.Length > 0)
                                            {
                                                ReadManifest(Index, newRoute, newLocalPath.FullName, Prep, Run);
                                            }
                                            else
                                            {
                                                ManifestDL(Index, newRoute, Prep, Run);
                                            }
                                            Index++;
                                        }
                                        break;
                                    }
                                case "EFFECT":
                                    {
                                        Uri newURL = new Uri(Path.Combine(CDNBase, "effects", Route.ToLower() + ".mp3"));
                                        FileInfo newLocalPath = new FileInfo(Path.Combine(AudioDir, "effects", Route.ToLower() + ".mp3"));
                                        Prep(newLocalPath.FullName);

                                        if (!File.Exists(newLocalPath.FullName))
                                        {
                                            new FileRequest((percent, total, received) => { },
                                                () =>
                                                {
                                                    if (PlaySlot == Index && Play)
                                                    {
                                                        Run(newLocalPath.FullName);
                                                    }
                                                },
                                                (ex) =>
                                                {
                                                    Console.WriteLine("Failed to download audio file: " + newLocalPath.FullName + " - " + ex.Message);
                                                    Run(null);
                                                },
                                                newURL.AbsoluteUri + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute,
                                                newLocalPath.FullName
                                            );
                                        }
                                        else
                                        {
                                            if (PlaySlot == Index && Play)
                                            {
                                                Run(newLocalPath.FullName);
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Manifets for " + FilePath + " is empty");
                    }
                    Depth++;
                });
                #endregion

                #region Download Manifest
                ManifestDL = new Action<int, string, Action<string>, Action<string>>((Index, Route, Prep, Run) =>
                {
                    Uri URL = new Uri(Path.Combine(CDNBase, "effects", Route + ".json"));
                    FileInfo LocalPath = new FileInfo(Path.Combine(AudioDir, "effects", Route + ".json"));

                    #region Check and Download
                    FileInfo fi = new FileInfo(LocalPath.FullName);
                    if (fi.Exists && fi.Length > 0)
                    {
                        ReadManifest(Index, Route, LocalPath.FullName, Prep, Run);
                    }
                    else
                    {
                        new FileRequest((percent, total, received) => { },
                        () =>
                        {
                            ReadManifest(Index, Route, LocalPath.FullName, Prep, Run);
                        },
                        (ex) =>
                        {

                        },
                        URL.AbsoluteUri + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute,
                        LocalPath.FullName);
                    }
                    #endregion
                });
                #endregion

                #region Run the thing
                Thread SoundThread = null;
                ManifestDL(0, RootFile, (Route) =>
                {
                    CultureInfo.CurrentCulture = App.CI;

                    if (Route == null)
                    {
                        return;
                    }

                    #region Set Volume
                    string rv = UserData.Get("audio_effects");
                    float volume = (float)Math.Round(Convert.ToDouble(rv), 3);
                    if (Override && volume == 0)
                    {
                        volume = 0.5f;
                    }
                    #endregion

                    #region Prepare Thread
                    SoundThread = new Thread(delegate ()
                    {
                        CultureInfo.CurrentCulture = App.CI;

                        try
                        {
                            Thread.Sleep(Delay);
                            if ((GetIsAudible() && UserData.Get("audio_effects") != "0") || Override)
                            {
                                Console.WriteLine("Playing audio: " + Route);
                                
                                AudioFileReader AudioFile = new AudioFileReader(Route);
                                EffectAudio.DeviceNumber = GetDeviceIndex(UserData.Get("audio_output"));
                                EffectAudio.Init(AudioFile);
                                EffectAudio.Volume = volume;
                                
                                lock(EffectAudios)
                                {
                                    EffectAudios.Add(EffectAudio);
                                }
                                EffectAudio.Play();
                            }

                            //APIBase.ClientCollection.SendMessage(App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            //{
                            //    { "name", "messages:show" },
                            //    { "meta", null },
                            //    { "payload", App.JSSerializer.Serialize(new List<Dictionary<string, dynamic>>()
                            //        {
                            //            new Dictionary<string, dynamic>()
                            //            {
                            //                { "user_name", Output_Character.Name },
                            //                { "user_id", Output_Character.ID },
                            //                { "time", Utils.TimeStamp(DateTime.UtcNow) },
                            //                { "type", "audio" },
                            //                { "duration", duration },
                            //                { "message", Output_File.Caption },
                            //                { "hide", false },
                            //            }
                            //        })
                            //    },
                            //    
                            //}), ClientType.Overlay);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to play sound " + ex.Message);
                        }
                    });
                    SoundThread.Priority = ThreadPriority.Highest;
                    SoundThread.CurrentCulture = CultureInfo.CurrentCulture;
                    SoundThread.SetApartmentState(ApartmentState.MTA);
                    #endregion
                },
                (Route) =>
                {
                    SoundThread.Start();
                });
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to play audio for " + RootFile + ": " + ex.Message);
            }

            return EffectAudio;
        }

        public static void Stop()
        {
            lock (EffectAudios)
            {
                foreach (WaveOutEvent ev in EffectAudios)
                {
                    ev.Stop();
                }
            }
            SpeechAudio.Stop();
        }
    }
}
