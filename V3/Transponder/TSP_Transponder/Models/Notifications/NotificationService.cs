using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Notifications
{
    public class NotificationService
    {
        internal static List<Notification> Actives = new List<Notification>();
        private static List<Notification> DisplayQueue = new List<Notification>();

        public static void Add(Notification NotificationInstance, bool Notify = true)
        {
            if (NotificationInstance.UID == 0)
            {
                NotificationInstance.UID = DateTime.UtcNow.Ticks - 637452913845325300;
            }

            lock (Actives)
            {
                Notification Existing = Actives.Find(x => x.Title == NotificationInstance.Title && x.Message == NotificationInstance.Message || x.UID == NotificationInstance.UID);
                if (Existing != null)
                {
                    Existing.Update(NotificationInstance);
                }
                else
                {
                    Actives.Add(NotificationInstance);
                    lock (DisplayQueue)
                    {
                        DisplayQueue.Add(NotificationInstance);
                    }
                }

                if (Notify)
                {
                    if (APIBase.ClientCollection != null)
                    {
                        var dic = NotificationInstance.ToDictionary();
                        APIBase.ClientCollection.SendMessage("notification:add", App.JSSerializer.Serialize(dic), null, APIBase.ClientType.Skypad);
                    }
                }
            }
        }

        public static void Remove(Notification NotificationInstance, bool Notify = true)
        {
            lock (DisplayQueue)
            {
                DisplayQueue.Remove(NotificationInstance);
            }

            lock (Actives)
            {
                Actives.Remove(NotificationInstance);
            }

            if (Notify)
            {
                if (APIBase.ClientCollection != null)
                {
                    APIBase.ClientCollection.SendMessage("notification:remove", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                    {
                        { "UID", NotificationInstance.UID },
                    }), null, APIBase.ClientType.Skypad);
                }
            }
        }

        public static void RemoveFromID(long UID, bool Trigger = false)
        {
            Notification NotificationInstance = null;
            lock (Actives)
            {
                NotificationInstance = Actives.Find(x => x.UID == UID);
            }

            if (NotificationInstance != null)
            {
                if (Trigger)
                {
                    NotificationInstance.Trigger(true);
                }
                else
                {
                    Remove(NotificationInstance);
                }
            }
        }

        public static int GetQueueCount()
        {
            lock (DisplayQueue)
            {
                return DisplayQueue.Count;
            }
        }

        public static void WSConnected(SocketClient Socket)
        {
            List<Dictionary<string, dynamic>> NotifsList = new List<Dictionary<string, dynamic>>();
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "List", NotifsList },
            };

            lock (Actives)
            {
                foreach (var Notif in Actives)
                {
                    NotifsList.Add(Notif.ToDictionary());
                }
            }

            Socket.SendMessage("notifications:get", App.JSSerializer.Serialize(rs));
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "remove":
                    {
                        RemoveFromID((long)payload_struct["UID"], (bool)payload_struct["Trigger"]);
                        break;
                    }
            }
        }
    }
}

