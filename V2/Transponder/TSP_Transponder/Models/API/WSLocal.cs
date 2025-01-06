using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Threading;
using TSP_Transponder.Models.Connectors;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TSP_Transponder.Models.API
{
    public class WSLocal
    {
        private WebSocketServer Server = null;
        private List<LocalSocketClient> Clients = new List<LocalSocketClient>();
        private MainWindow MW = null;

        public bool IsConnected
        {
            get
            {
                if (Server != null)
                {
                    return Server.IsListening;
                }
                else
                {
                    return false;
                }
            }
        }

        public class LocalSocketClient : WebSocketBehavior
        {
            private List<LocalSocketClient> Clients = null;
            private MainWindow MW = null;
            internal SocketClient Client = null;

            public LocalSocketClient(MainWindow _MW, List<LocalSocketClient> _Clients)
            {
                Clients = _Clients;
                MW = _MW;
            }

            protected override void OnOpen()
            {
                Console.WriteLine("New client: " + this.Context.Host);
                Clients.Add(this);
                base.OnOpen();
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Console.WriteLine("Closed client: " + this.Context.Host);
                Clients.Remove(this);
                APIBase.ClientCollection.Remove(Client);
                base.OnClose(e);
            }

            protected override void OnError(WebSocketSharp.ErrorEventArgs e)
            {
                Console.WriteLine("Error on client: " + this.Context.Host);
                Clients.Remove(this);
                APIBase.ClientCollection.Remove(Client);
                base.OnError(e);
            }

            protected override void OnMessage(MessageEventArgs e) // From locals
            {
                CultureInfo.CurrentCulture = App.CI;

                try
                {
                    Dictionary<string, dynamic> structure = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(e.Data);
                    
                    if (Client == null)
                    {
                        Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                        if (payload_struct != null)
                        {
                            Console.WriteLine("New Local Client (" + payload_struct["type"] + " on " + payload_struct["name"] + ")");
                            Client = APIBase.ClientCollection.Add((string)structure["origin"], (string)payload_struct["name"], (string)payload_struct["type"], this);
                            
                            if (Client == null)
                            {
                                return;
                            }
                        }
                    }

                    Client.CommonReceive(structure);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to fully process WSLocal Message: " + ex.Message + " - "+ ex.StackTrace);
                }
            }

            public void Close()
            {
                foreach (IWebSocketSession sess in Sessions.Sessions)
                {
                    if (sess.ID == this.ID)
                    {
                        Sessions.CloseSession(sess.ID);
                    }
                }
            }

            public void SendMessage(string verb, string message, Dictionary<string, dynamic> meta = null)
            {
                if (State == WebSocketState.Open)
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
                        { "meta", MetaStruct },
                        { "payload", message },
                    };

                    Client.LastSent = App.Timer.ElapsedMilliseconds;
                    lock (this)
                    {
                        SendAsync(App.JSSerializer.Serialize(SendStructure), (s) =>
                        {
                            if (!s)
                            {
                                Console.WriteLine("Failed to send WS message, closing connection");
                                APIBase.ClientCollection.Remove(Client);
                            }
                        });
                    }
                }
                else
                {
                    Console.WriteLine("Failed to send WS message: Connection is closed");
                    APIBase.ClientCollection.Remove(Client);
                }
            }
        }

        public WSLocal(MainWindow _MW)
        {
            MW = _MW;
        }

        public void Disconnect()
        {
            if (Server != null)
            {
                if (IsConnected)
                {
                    foreach (LocalSocketClient cl in new List<LocalSocketClient>(Clients))
                    {
                        cl.Close();
                        Clients = new List<LocalSocketClient>();
                    }

                    Server.Stop();
                    Server = null;
                }
            }
        }

        public void Connect()
        {
            if (!IsConnected && APIBase.ClientCollection.DataReady)
            {
                Console.WriteLine("Starting Local Websocket");
                Thread WSThread = new Thread(delegate ()
                {
                    try
                    {
                        Server = new WebSocketServer(8163);
                        //Server.SslConfiguration.ServerCertificate = new X509Certificate2(@"C:\Program Files\OpenSSL-Win64\bin\certificate.pfx", "6s72AZ014R693C8j");
                        Server.AddWebSocketService("/", () => new LocalSocketClient(MW, Clients));
                        Server.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to start WS Server: " + ex.Message);
                    }
                });
                WSThread.IsBackground = true;
                WSThread.SetApartmentState(ApartmentState.STA);
                WSThread.CurrentCulture = CultureInfo.CurrentCulture;
                WSThread.Start();
            }
        }

    }
}
