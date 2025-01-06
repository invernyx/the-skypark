using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Script.Serialization;
using TSP_Transponder.Models.Connectors;
using WebSocketSharp;

namespace TSP_Transponder.Models.API
{
    public class WSRemote
    {
        private WebSocket ClientConnection = null;
        private MainWindow MW = null;
        internal double LastSent = 0;
        internal double LastReceived = 0;
        private string ClientID = "";
        private string SessionKey = "";
        
        public bool IsConnected
        {
            get
            {
                if (ClientConnection != null)
                {
                    return ClientConnection.ReadyState == WebSocketState.Open && SessionKey != string.Empty;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsAlive
        {
            get
            {
                if (ClientConnection != null)
                {
                    return ClientConnection.IsAlive && ClientConnection.ReadyState == WebSocketState.Open;
                }
                else
                {
                    return false;
                }
            }
        }

        public enum SendMode
        {
            Bounce,
            Server,
            User,
        }

        public WSRemote(MainWindow _MW)
        {
            MW = _MW;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("Opened connection with Remote WS Server");
            if (UserData.Get("privacy_remote_access") == "1")
            {
                App.MW.CreateAPIConnectionIndicator("indicator_remote");
                string json = App.JSSerializer.Serialize(new Dictionary<string, string>() {
                    { "new_host", UserData.Get("token") },
                });
                //Send(json);
                APIBase.IsHost = true;

            }
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            try
            {
                Console.WriteLine("Closed Remote WS Connection " + e.Reason);
                App.MW.DestroyConnectionIndicator(APIBase.ConnectionIndicator);
                SessionKey = "";
                if (!MW.IsShuttingDown)
                {
                    //Connect();
                }
            }
            catch
            {

            }
        }

        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("Failed to connect to Server " + e.Message);
            Thread.Sleep(3000);
        }

        private void OnMessage(object sender, MessageEventArgs e) // From the Server
        {
            CultureInfo.CurrentCulture = App.CI;
            Dictionary<string, dynamic> structure = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(e.Data);

            if(structure.ContainsKey("session_key"))
            {
                SessionKey = structure["session_key"];
                UserData.Set("ws_room", SessionKey, true);
            }
            else
            {
                SocketClient Client = null;
                Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);

                if (structure.ContainsKey("origin"))
                {
                    lock(APIBase.ClientCollection.ConnectedClients)
                    {
                        Client = APIBase.ClientCollection.ConnectedClients.Find(x => x.ID == structure["origin"]);
                    }
                }

                string[] NameSpl = ((string)structure["name"]).Split(':');
                switch (NameSpl[0])
                {
                    case "connect":
                        {
                            switch (NameSpl[1])
                            {
                                case "confirm":
                                    {
                                        if (Client == null)
                                        {
                                            Console.WriteLine("New Remove Client (" + payload_struct["type"] + " on " + payload_struct["name"] + ")");
                                            Client = APIBase.ClientCollection.Add((string)structure["origin"], (string)payload_struct["name"], (string)payload_struct["type"]);

                                            if (Client == null)
                                            {
                                                return;
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
                

                if(Client != null)
                {
                    Client.CommonReceive(structure);
                }

            }
            
        }

        public void Send(string verb, string message, Dictionary<string, dynamic> meta = null, SendMode sendTo = SendMode.Bounce, string User = "")
        {
            SocketClient Client = null;
            
            lock (APIBase.ClientCollection.ConnectedClients)
            {
                Client = APIBase.ClientCollection.ConnectedClients.Find(x => x.ID == User);
            }

            if (ClientConnection.ReadyState == WebSocketState.Open)
            {
                if (Client == null)
                {
                    return;
                }

                Dictionary<string, dynamic> MetaStruct = new Dictionary<string, dynamic>();
                if (meta != null)
                {
                    if (meta.ContainsKey("callback"))
                    {
                        MetaStruct.Add("callback", meta["callback"]);
                        MetaStruct.Add("callbackType", meta["callbackType"]);
                    }
                }

                Dictionary<string, dynamic> SendStructure = new Dictionary<string, dynamic>()
                {
                    { "name", verb },
                    { "origin", ClientID },
                    { "target", User },
                    { "meta", MetaStruct },
                    { "payload", message },
                };

                Client.LastSent = App.Timer.ElapsedMilliseconds;
                ClientConnection.SendAsync(App.JSSerializer.Serialize(SendStructure), (s) =>
                {
                    if (!s)
                    {
                        Console.WriteLine("Failed to send WS Remote message, closing connection");
                        APIBase.ClientCollection.Remove(Client);
                    }
                });

            }
            
            //Thread sendThread = new Thread(() =>
            //{
            //    if (ClientConnection != null)
            //    {
            //        
            //    }
            //});
            //sendThread.Start();

        }

        public void Connect()
        {
            if (!IsConnected && APIBase.ClientCollection.DataReady)
            {
                Thread WSThread = new Thread(delegate ()
                {
                    try
                    {
                        Console.WriteLine("Trying to connect WS Client to API");
                        if(UserData.Get("ws_room") != string.Empty)
                        {
                            ClientConnection = new WebSocket(APIBase.WSEndpoint + "?type=transponder&session_key=" + UserData.Get("ws_room"));
                        }
                        else
                        {
                            ClientConnection = new WebSocket(APIBase.WSEndpoint + "?type=transponder");
                        }
                        ClientConnection.Compression = CompressionMethod.Deflate;
                        ClientConnection.WaitTime = TimeSpan.FromSeconds(3);
                        ClientConnection.OnMessage += OnMessage;
                        ClientConnection.OnOpen += OnOpen;
                        ClientConnection.OnClose += OnClose;
                        ClientConnection.OnError += OnError;
                        ClientConnection.Connect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to connect WS Client to API: " + ex.Message);
                    }
                });
                WSThread.IsBackground = true;
                WSThread.SetApartmentState(ApartmentState.STA);
                //WSThread.Start();
            }
        }

        public void Disconnect()
        {
            if (ClientConnection != null)
            {
                if (IsConnected)
                {

                }
                if (ClientConnection.ReadyState == WebSocketState.Open || ClientConnection.ReadyState == WebSocketState.Connecting)
                {
                    ClientConnection.Close();
                }
                ClientConnection = null;
            }
        }

    }
}
