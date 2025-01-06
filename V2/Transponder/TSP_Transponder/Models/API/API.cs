using System;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Controls;
using static TSP_Transponder.App;
using System.Net.Http;

namespace TSP_Transponder.Models.API
{
    public class APIBase
    { 
        public static string APIHost = "";
        public static string CDNHost = "";
        public static string CDNChannel = "";
        public static string CDNImagesHost = "";
        public static string SkyOSHost = "";
        private static string APIEndpoint = "";
        internal static string WSEndpoint = "";
        public static bool IsHost = false;
        internal static Label ConnectionIndicator = null;
        public static ClientCollection ClientCollection;
        public static HttpClient HTTPCl = null;

        public enum ClientMode
        {
            All,
            Local,
            Remote,
        }

        public enum ClientType
        {
            [EnumValue("All")]
            All,
            [EnumValue("Skypad")]
            Skypad,
            [EnumValue("Overlay")]
            Overlay,
            [EnumValue("Unknown")]
            Unknown,
        }

        public static WSLocal WSLocal = null;
        public static WSRemote WSRemote = null;

        public static void Startup(MainWindow MW)
        {
            WSLocal = new WSLocal(MW);
            WSRemote = new WSRemote(MW);
            ClientCollection = new ClientCollection(MW) { WSLocal = WSLocal, WSRemote = WSRemote };
        }

        //public static async Task LoginAsync(string email, string password, Action<HttpStatusCode> ReturnMethod)
        //{
        /*
        await PostRequest("auth", new Dictionary<string, string>()
        {
            { "email", email },
            { "password", password },
        }, 
        (response) =>
        {
            Dictionary<string, dynamic> Ret = new Dictionary<string, dynamic>();

            if (response.ContainsKey("access_token"))
            {
                Console.WriteLine("Login Succeeded");
                UserData.Set("token", response["access_token"]);

                Ret.Add("status", "success");
                ReturnMethod(Ret);
            }
            else
            {
                Console.WriteLine("Login Failed");
                Ret.Add("status", "failed");
                Ret.Add("descr", response["error_description"]);
                ReturnMethod(Ret);
            }

            return 0;
        });
        */
        //}

        //public static async Task AuthAsync(Action<HttpStatusCode> ReturnMethod)
        public static void AuthAsync(Action<HttpStatusCode> ReturnMethod) // async Task
        {
            ReturnMethod(HttpStatusCode.OK);

            return;

            /*
            try
            {
                Console.WriteLine("Authing in...");
                HttpResponseMessage response = await HTTPCl.GetAsync(APIEndpoint + "me");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            Console.WriteLine("Authed");
                            string responseString = await response.Content.ReadAsStringAsync();
                            Dictionary<string, string> str = App.JSSerializer.Deserialize<Dictionary<string, string>>(responseString);

                            UserData.AccountDetails["account_id"] = str["id"];
                            UserData.AccountDetails["alias"] = str["alias"];
                            UserData.AccountDetails["first_name"] = str["first_name"];
                            UserData.AccountDetails["last_name"] = str["last_name"];
                            UserData.AccountDetails["email"] = str["email"];
                            
                            UserData.dev_available = true;
                            UserData.staging_available = true;
                            UserData.prod_available = true;

                            if (!UserData.dev_available && UserData.Get("channel") == "2")
                            {
                                App.MW.HeldBeta = "0";
                            }

                            if (!UserData.staging_available && UserData.Get("channel") == "1")
                            {
                                App.MW.HeldBeta = "0";
                            }

                            if (App.MW.IsToInit)
                            {
                                AudioFramework.GetSpeech("BRIGIT", "INTRO", "BRIGIT_INTRO_SKYPAD.mp3");
                            }

                            //Loadmaster.Startup();
                            App.MW.ResetValidateLoop();
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }

                ReturnMethod(response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Auth Failed: " + ex.Message);
                ReturnMethod(HttpStatusCode.ServiceUnavailable);
            }
            */
        }

        public static void FindHost()
        {
            APIHost = "theskypark.com";
            APIEndpoint = "http://api." + APIHost + "/";
            WSEndpoint = "ws://192.168.2.32:8080";
            SkyOSHost = "https://skypad." + APIHost;
            CDNHost = "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/";
            CDNChannel = "prod";
            CDNImagesHost = CDNHost + "common/images";

            Console.WriteLine("API: " + APIEndpoint);
        }

        public static string GetLocalIPAddress()
        {
            //if (UserData.Get("channel") != "2")
            //{
            //    string comb = "";
            //    try
            //    {
            //        var host = Dns.GetHostEntry(Dns.GetHostName());
            //        foreach (var ip in host.AddressList)
            //        {
            //            if (ip.AddressFamily == AddressFamily.InterNetwork)
            //            {
            //                comb += ip.ToString() + ",";
            //            }
            //        }
            //        return comb.TrimEnd(',');
            //    }
            //    catch
            //    {
            //        return "localhost";
            //    }
            //}
            //else
            //{
                return "localhost";
            //}
        }
        
    }
    
}
