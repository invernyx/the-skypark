using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using System.Linq;

namespace TSP_OSM_Loader
{
    public class Job
    {
        public string Id { get; set; }
        public bool Completed { get; set; } = false;
        public Client Client { get; set; } 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Action<string> Callback { get; set; }
    }

    public class Client
    {
        public string Id { get; set; }
        public WebSocket Socket { get; set; }
        public bool IsAvailable { get; set; }
    }

    class WSMapImage
    {
        private static WebSocketServer wssv = null;
        private static List<Job> jobQueue = new List<Job>();
        private static Dictionary<string, Client> clients = new Dictionary<string, Client>();

        public static void Startup()
        {
            Thread nt = new Thread(() =>
            {
                wssv = new WebSocketServer("ws://localhost:8898");
                wssv.AddWebSocketService<ClientSocket>("/JobServer");
                wssv.Start();
            });
            nt.Priority = ThreadPriority.Highest;
            nt.SetApartmentState(ApartmentState.MTA);
            nt.IsBackground = true;
            nt.Start();
        }

        public class ClientSocket : WebSocketBehavior
        {
            Client client = null;

            protected override void OnOpen()
            {
                var clientId = Guid.NewGuid().ToString();
                client = new Client()
                { 
                    Id = clientId, 
                    Socket = Context.WebSocket, 
                    IsAvailable = true 
                };
                lock(clients)
                {
                    clients.Add(clientId, client);
                }
                Console.WriteLine($"Client connected: {clientId}");
                AssignJobFromClient(clientId);
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                var msgData = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(e.Data);

                var clientId = GetClientIdBySocket(Context.WebSocket);
                if (clientId != null)
                {
                    if (clients.ContainsKey(clientId))
                    {
                        if (msgData["type"].ToString() == "completed")
                        {
                            lock (jobQueue)
                            {
                                var job = jobQueue.FirstOrDefault(x => x.Id == msgData["id"].ToString());
                                jobQueue.Remove(job);

                                job.Callback(msgData["result"].ToString());
                                //Console.WriteLine($"Completed job {job.Id} from client {clientId}");
                            }

                            clients[clientId].IsAvailable = true;
                            AssignJobFromClient(clientId);
                        }
                    }
                    else
                    {

                    }

                }

            }

            protected override void OnClose(CloseEventArgs e)
            {
                var clientId = GetClientIdBySocket(Context.WebSocket);
                if (clientId != null)
                {
                    lock (jobQueue)
                    {
                        foreach (var job in jobQueue.Where(x => x.Client == clients[clientId]))
                        {
                            job.Client = null;
                            job.Completed = false;
                        }
                    }

                    lock (clients)
                    {
                        clients.Remove(clientId);
                    }
                    Console.WriteLine($"Client disconnected: {clientId}");
                }
            }

            private string GetClientIdBySocket(WebSocket socket)
            {
                lock(clients)
                {
                    foreach (var client in clients)
                    {
                        if (client.Value.Socket == socket)
                        {
                            return client.Key;
                        }
                    }
                }

                return null;
            }

            private void AssignJobFromClient(string clientId)
            {
                Job job;

                lock (jobQueue)
                {
                    job = jobQueue.Where(x => x.Client == null).FirstOrDefault();
                }

                if(job != null)
                {
                    if (clients[clientId].IsAvailable)
                    {
                        clients[clientId].IsAvailable = false;
                        job.Client = clients[clientId];

                        //Console.WriteLine($"Assigned job {job.Id} to client {clientId}");
                        clients[clientId].Socket.Send(
                            JsonSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                            { "type", "assigned" },
                            { "id", job.Id },
                            { "latitude", job.Latitude },
                            { "longitude", job.Longitude },
                            }
                        ));
                    }
                }

            }
        }

        public static void GetImage(Job job)
        {
            lock (jobQueue)
            {
                jobQueue.Add(job);
            }
            AssignJobFromJob(job);
        }

        private static void AssignJobFromJob(Job job)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client.Value.IsAvailable)
                    {
                        client.Value.IsAvailable = false;
                        job.Client = client.Value;

                        //Console.WriteLine($"Assigned job {job.Id} to client {client.Key}");

                        client.Value.Socket.Send(
                            JsonSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                            { "type", "assigned" },
                            { "id", job.Id },
                            { "latitude", job.Latitude },
                            { "longitude", job.Longitude },
                            }
                        ));
                        break;
                    }
                }
            }
        }
    }
}
