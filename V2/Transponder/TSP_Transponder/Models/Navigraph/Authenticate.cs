using IdentityModel.Client;
using IdentityModel.OidcClient;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfWebView;

namespace TSP_Transponder.Models.Navigraph
{
    class NavigraphAuthenticate
    {
        private static HttpClient Client = new HttpClient();
        private static OidcClient OidcClient = null;
        private static string AccessTokenStr = "";

        internal static void Init(Action Done)
        {
            App.MW.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (OidcClient == null)
                {
                    OidcClient = new OidcClient(new OidcClientOptions
                    {
                        Authority = "https://identity.api.navigraph.com",
                        ClientId = "skypark",
                        ClientSecret = "01275121-7f2f-41ea-b1f4-11cd8e37c4d4",
                        Scope = "openid fmsdata offline_access",
                        RedirectUri = "skypark://identity-callback",
                        Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                        ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                        Browser = new WpfEmbeddedBrowser()
                    });

                    Done();
                }
            }));
        }

        internal static void Login(Action<bool> Callback)
        {
            App.MW.Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (UserData.Get("navigraph_refresh") != string.Empty)
                {
                    RefreshToken((state) =>
                    {
                        Callback(state);
                    });
                }
                else
                {
                    try
                    {
                        LoginResult result = await OidcClient.LoginAsync();
                        if (result.IsError)
                        {
                            UserData.Set("navigraph_refresh", "", true);
                        }
                        else
                        {
                            AccessTokenStr = result.AccessToken;
                            UserData.Set("navigraph_refresh", result.RefreshToken, true);
                        }
                        Callback(!result.IsError);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected Error: {ex.Message}");
                        Callback(false);
                        return;
                    }
                }


            }));
        }

        internal static void Logout()
        {
            App.MW.Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (OidcClient != null)
                {
                    LogoutResult result = await OidcClient.LogoutAsync();

                    UserData.Set("navigraph_refresh", "", true);
                }
            }));
        }

        internal static void RefreshToken(Action<bool> Callback)
        {
            App.MW.Dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    var refreshResult = await OidcClient.RefreshTokenAsync(UserData.Get("navigraph_refresh"));
                    if (refreshResult.IsError)
                    {
                        AccessTokenStr = "";
                        UserData.Set("navigraph_refresh", "", true);
                        Login(Callback);
                    }
                    else
                    {
                        AccessTokenStr = refreshResult.AccessToken;
                        UserData.Set("navigraph_refresh", refreshResult.RefreshToken, true);
                        Callback(true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to connect to Navigraph because " + ex.Message);
                    Callback(false);
                }
            }));
        }

        internal static void CallApi(string URL, Action<string> Callback)
        {
            App.MW.Dispatcher.BeginInvoke(new Action(async () =>
            {
                bool Reset = false;
                if(AccessTokenStr == string.Empty)
                {
                    Reset = true;
                }
                else
                {
                    Client.SetBearerToken(AccessTokenStr);
                    HttpResponseMessage response = await Client.GetAsync(URL);

                    if (response.IsSuccessStatusCode)
                    {
                        Callback(await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        Reset = true;
                    }
                }

                if (Reset)
                {
                    Login((state) =>
                    {
                        if (state)
                        {
                            CallApi(URL, Callback);
                        }
                        else
                        {
                            Callback(null);
                        }
                    });
                }
            }));
        }
    }
}
