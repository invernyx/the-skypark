using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfWebView;

namespace TSP_Transponder.Models.Navigraph
{
    public class NavData
    {
        public static void AuthAsync()
        {
#if DEBUG

            //NavigraphAuthenticate.Init(() =>
            //{
                //NavigraphAuthenticate.Logout();
                //NavigraphAuthenticate.Login((test) => {
                //});

                //NavigraphAuthenticate.CallApi("https://fmsdata.api.navigraph.com/v3/packages?package_status=current,outdated&format=skypark-v1", (result) =>
                //{
                //    if (result != null && result != "[]")
                //    {
                //        MessageBox.Show("Hey, Some Navigraph data started coming in...");
                //    }
                //});

                //NavigraphAuthenticate.CallApi(UserData.Get("navigraph_access"));
            //});

#endif


        }
    }
}
