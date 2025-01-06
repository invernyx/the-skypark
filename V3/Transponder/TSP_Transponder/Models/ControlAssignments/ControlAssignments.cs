using RawInput_Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using TSP_Transponder.Models.Connectors;

namespace TSP_Transponder.Models
{
    internal class Controller
    {
        private static RawInput RawInput = null;        
        private static AutoResetEvent ARE = new AutoResetEvent(false);

        private static List<int> ArePressed = new List<int>();
        internal static List<int> LastCombo = new List<int>();

        internal static Action<List<int>> MonitorFunc = null;
        internal static Action MonitorDoneFunc = null;

        internal static Dictionary<string, string> EventAssignments = new Dictionary<string, string>();
        internal static Dictionary<string, Action> EventActions = new Dictionary<string, Action>()
        {
            { "controls_skypad", () =>
                {
                    if(SimConnection.ActiveSim != null)
                    {
                        SimConnection.GetSimIsFocused();
                        App.MW.ToggleSkypad();
                    }
                }
            },
            { "controls_capture", () =>
                {
                    if(SimConnection.ActiveSim != null)
                    {
                        SimConnection.GetSimIsFocused();
                        if(EventBus.EventManager.Active != null && SimConnection.SimHasFocus)
                        {
                            //EventBus.Blogs.Active.ImageCapture(false);
                        }
                        else
                        {

                        }
                    }
                }
            },
        };
        
        internal static void Startup()
        {
            RawInput = new RawInput(App.MW.Handle);
            RawInput.AddMessageFilter();
            RawInput.KeyPressed += OnKeyPressed;
        }

        public static void AddAssignement(List<int> keys, string Ev, bool save = true)
        {
            RemoveAssignement(Ev, false);
            RemoveAssignement(keys, false);

            if (keys.Count == 0)
            {
                App.MW.PopulateControlAssignment(Ev, "");
            }
            else
            {
                if (EventAssignments.ContainsKey(Ev))
                {
                    EventAssignments[Ev] = GetKeyVCode(keys);
                }
                else
                {
                    EventAssignments.Add(Ev, GetKeyVCode(keys));
                }

                App.MW.PopulateControlAssignment(Ev, GetKeyName(keys));
            }

            if (save)
            {
                SavePrefs();
            }
        }

        public static void RemoveAssignement(string Ev, bool save = true)
        {
            if (EventAssignments.ContainsKey(Ev))
            {
                EventAssignments.Remove(Ev);
            }

            App.MW.PopulateControlAssignment(Ev, "");

            if (save)
            {
                SavePrefs();
            }
        }

        public static void RemoveAssignement(List<int> keys, bool save = true)
        {
            foreach(KeyValuePair<string, string> Ev in EventAssignments)
            {
                if(Ev.Value == GetKeyVCode(keys))
                {
                    EventAssignments.Remove(Ev.Key);
                    App.MW.PopulateControlAssignment(Ev.Key, "");
                    return;
                }
            }

            if (save)
            {
                SavePrefs();
            }
        }

        private static void SavePrefs()
        {
            Dictionary<string, string> config = UserData.GetAll();
            
            List<string> Assignmentkeys = new List<string>(EventAssignments.Keys);
            List<string> configParam = new List<string>(config.Keys);

            // Clean existing assignments
            foreach (string key in configParam)
            {
                if (key.StartsWith("controls_"))
                {
                    UserData.Remove(key);
                }
            }

            // Write new assignments
            foreach (string assignment in Assignmentkeys)
            {
                UserData.Set(assignment, EventAssignments[assignment]);
            }

            UserData.Save();
        }
        
        public static string GetKeyName(List<int> keys)
        {
            string str = "";
            int i = 0;
            foreach (int key in keys)
            {
                string keyName = new System.Windows.Forms.KeysConverter().ConvertToString(key);

                i++;
                if (i == keys.Count)
                {
                    str += keyName;
                }
                else
                {
                    str += keyName + ", ";
                }
            }

            return str;
        }

        public static string GetKeyVCode(List<int> keys)
        {
            string str = "";
            int i = 0;
            foreach (int key in keys)
            {
                i++;
                if (i == keys.Count)
                {
                    str += key.ToString();
                }
                else
                {
                    str += key.ToString() + ", ";
                }
            }
            return str;
        }

        public static List<int> SortKeys(List<int> keys)
        {
            List<int> sorted = new List<int>(keys);
            sorted.Sort((a, b) => {
                switch (a)
                {
                    case 91:
                    case 92:
                    case 160:
                    case 161:
                    case 162:
                    case 163:
                    case 164:
                    case 165:
                        {

                            return -1;
                        }
                }
                return 0;
            });
            return sorted;
        }
        
        private static void OnKeyPressed(object sender, InputEventArg e)
        {
            //Console.WriteLine(e.KeyPressEvent.Name); // Device Name
            //Console.WriteLine(e.KeyPressEvent.KeyPressState); // MAKE|BREAK
            //Console.WriteLine(e.KeyPressEvent.VKey); // Code
            //Console.WriteLine(e.KeyPressEvent.VKeyName); // Display Name
            //Console.WriteLine("---------------------------------------------");
            
            if (MonitorFunc != null)
            {
                if (e.KeyPressEvent.KeyPressState == "MAKE")
                {
                    if (!ArePressed.Contains(e.KeyPressEvent.VKey))
                    {
                        if (ArePressed.Count == 0)
                        {
                            LastCombo.Clear();
                        }

                        ArePressed.Add(e.KeyPressEvent.VKey);

                        if (!LastCombo.Contains(e.KeyPressEvent.VKey))
                        {
                            LastCombo.Add(e.KeyPressEvent.VKey);
                        }
                    }
                }
                else
                {
                    if (ArePressed.Contains(e.KeyPressEvent.VKey))
                    {
                        ArePressed.Remove(e.KeyPressEvent.VKey);
                    }
                }

                ArePressed = SortKeys(ArePressed);
                LastCombo = SortKeys(LastCombo);
                MonitorFunc(LastCombo);
            }
            else
            {
                if (e.KeyPressEvent.KeyPressState == "MAKE")
                {
                    if (!ArePressed.Contains(e.KeyPressEvent.VKey))
                    {
                        ArePressed.Add(e.KeyPressEvent.VKey);
                    }
                }
                else
                {
                    ArePressed = SortKeys(ArePressed);
                    foreach (KeyValuePair<string, Action> ev in EventActions)
                    {
                        if (EventAssignments.ContainsKey(ev.Key))
                        {
                            if (EventAssignments[ev.Key] == GetKeyVCode(ArePressed))
                            {
                                ev.Value();
                            }
                        }
                    }

                    if (ArePressed.Contains(e.KeyPressEvent.VKey))
                    {
                        ArePressed.Remove(e.KeyPressEvent.VKey);
                    }
                }
            }
            
        }

        /*
        private static void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            Thread ctrlHood = new Thread(() =>
            {
                if (MonitorFunc != null)
            {
                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
                {
                    if (!ArePressed.Contains(e.KeyboardData.VirtualCode))
                    {
                        if (ArePressed.Count == 0)
                        {
                            LastCombo.Clear();
                        }

                        ArePressed.Add(e.KeyboardData.VirtualCode);

                        if (!LastCombo.Contains(e.KeyboardData.VirtualCode))
                        {
                            LastCombo.Add(e.KeyboardData.VirtualCode);
                        }
                    }
                }
                else
                {
                    if (ArePressed.Contains(e.KeyboardData.VirtualCode))
                    {
                        ArePressed.Remove(e.KeyboardData.VirtualCode);
                    }
                }
                
                ArePressed = SortKeys(ArePressed);
                LastCombo = SortKeys(LastCombo);
                MonitorFunc(LastCombo);
            }
            else
            {
                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
                {
                    if (!ArePressed.Contains(e.KeyboardData.VirtualCode))
                    {
                        ArePressed.Add(e.KeyboardData.VirtualCode);
                    }
                }
                else
                {
                    ArePressed = SortKeys(ArePressed);
                    foreach (KeyValuePair<string, Action> ev in EventActions)
                    {
                        if (EventAssignments.ContainsKey(ev.Key))
                        {
                            if (EventAssignments[ev.Key] == GetKeyVCode(ArePressed))
                            {
                                ev.Value();
                            }
                        }
                    }

                    if (ArePressed.Contains(e.KeyboardData.VirtualCode))
                    {
                        ArePressed.Remove(e.KeyboardData.VirtualCode);
                    }
                }
            }
            })
            {
                IsBackground = true
            };
            ctrlHood.Start();
            
        }
        */

        public static void Dispose()
        {
            RawInput.ReleaseHandle();
            RawInput.DestroyHandle();
        }
        
    }
}
