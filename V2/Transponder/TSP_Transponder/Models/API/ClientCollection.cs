using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using static TSP_Transponder.Models.API.APIBase;

namespace TSP_Transponder.Models.API
{
    public class ClientCollection
    {
        private bool _DataReady = false;
        public bool DataReady
        {
            get
            {
                return _DataReady;
            }
            set {
                if(_DataReady != value && value)
                {
                    Console.WriteLine("Data Ready");
                    _DataReady = value;
                    SendReady();
                }
            }
        }
        public WSLocal WSLocal = null;
        public WSRemote WSRemote = null;
        private MainWindow MW = null;
        public List<SocketClient> ConnectedClients = new List<SocketClient>();
        internal JavaScriptSerializer JSSerializer = new JavaScriptSerializer();
        private int WSExpireTime = 80000;

        public ClientCollection(MainWindow _MW)
        {
            MW = _MW;
        }

        public List<SocketClient> GetClients()
        {
            lock (ConnectedClients)
            {
                return new List<SocketClient>(ConnectedClients);
            }
        }

        public void ValidateClients()
        {
            try
            {
                List<SocketClient> toRemove = new List<SocketClient>();

                // Local Skypads
                //if (!WSLocal.IsConnected)
                //{
                //    foreach (SocketClient client in ConnectedClients)
                //    {
                //        if (client.IsLocal)
                //        {
                //            if (!toRemove.Contains(client))
                //            {
                //                toRemove.Add(client);
                //            }
                //        }
                //    }
                //}

                // Remote Skypads
                if (!WSRemote.IsConnected)
                {
                    lock(ConnectedClients)
                    {
                        foreach (SocketClient client in ConnectedClients)
                        {
                            if (!client.IsLocal)
                            {
                                if (!toRemove.Contains(client))
                                {
                                    toRemove.Add(client);
                                }
                            }
                        }
                    }
                }

                // Check Timeout
                lock (ConnectedClients)
                {
                    foreach (SocketClient client in ConnectedClients)
                    {
                        if (client.LastSeen + WSExpireTime < App.Timer.ElapsedMilliseconds && !client.IsLocal)
                        {
                            if (!toRemove.Contains(client))
                            {
                                toRemove.Add(client);
                            }
                        }
                    }
                }

                // Remove from the list
                foreach (SocketClient client in toRemove)
                {
                    Console.WriteLine("Removing client " + client.ID + " (" + App.GetDescription(client.Type) + ")");
                    Remove(client);
                }
            }
            catch
            {
                Console.WriteLine("Failed to validate clients");
            }

        }

        private void SendReady()
        {
            lock(ConnectedClients)
            {
                foreach (SocketClient Client in ConnectedClients)
                {
                    Client.SendReady();
                }
            }
        }

        public void SendState()
        {
            lock (ConnectedClients)
            {
                foreach (SocketClient Client in ConnectedClients)
                {
                    Client.SendState();
                }
            }
        }

        public void SendMessage(string verb, string message, Dictionary<string, dynamic> meta = null, ClientType ct = ClientType.All)
        {
            try
            {
                List<SocketClient> SelectedSockets = new List<SocketClient>();
                lock(ConnectedClients)
                {
                    if (ct != ClientType.All)
                    {
                        SelectedSockets = ConnectedClients.FindAll(x => x.Type == ct);
                    }
                    else
                    {
                        SelectedSockets = new List<SocketClient>(ConnectedClients);
                    }
                }
                
                foreach(var Socket in SelectedSockets)
                {
                    if (Socket.IsLocal)
                    {
                        Socket.LocalClient.SendMessage(verb, message, meta);
                    }
                    else
                    {
                        WSRemote.Send(verb, message, meta, WSRemote.SendMode.User, Socket.ID);
                    }
                }
            }
            catch
            {

            }
        }

        public SocketClient Add(string id, string device, string type, WSLocal.LocalSocketClient LocalClient = null)
        {
            // Check for existing clients
            SocketClient client = null;

            lock (ConnectedClients)
            {
                client = ConnectedClients.Find(x => x.Device == device);
            }

            if (client == null)
            {
                ClientType ct = ClientType.Unknown;
                switch (type)
                {
                    case "skypad":
                        {
                            ct = ClientType.Skypad;
                            break;
                        }
                }

                client = new SocketClient(id, device)
                {
                    Type = ct
                };

                if (ct == ClientType.Skypad)
                {
                    if (LocalClient != null)
                    {
                        App.MW.CreateWifiConnectionIndicator("indicator_wifi", client);
                    }
                    else
                    {
                        App.MW.CreateLTEConnectionIndicator("indicator_lte", client);
                    }
                }

                lock (ConnectedClients)
                {
                    ConnectedClients.Add(client);
                }
            }

            if (!client.IsLocal && LocalClient != null)
            {
                LocalClient.Client = client;
                client.IsLocal = true;
                client.LocalClient = LocalClient;
            }

            return client;
        }

        public void Remove(SocketClient client)
        {
            if (client != null)
            {
                App.MW.DestroyConnectionIndicator(client.Indicator);
                lock (ConnectedClients)
                {
                    ConnectedClients.Remove(client);
                }
                client.Disconnected();
            }
        }

    }
}
