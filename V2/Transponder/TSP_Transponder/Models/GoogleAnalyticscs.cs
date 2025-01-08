using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleAnalyticsTracker.Simple;
using GoogleAnalyticsTracker.Core.Interface;
using System.Globalization;

namespace TSP_Transponder.Models
{
    public class GoogleAnalyticscs
    {
        internal static SimpleTracker GATrack = null;

        public static bool Startup(MainWindow _MW)
        {
            #region Get GA
            GATrack = new SimpleTracker("REPLACE_WITH_ANALYTICS_CODE", new AnalyticsSession(UserData.Get("ga")), new SimpleTrackerEnvironment(Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString));
            GATrack.ThrowOnErrors = true;

            if (!App.IsDev)
            {
                TrackEvent("Launch", "Transponder", App.BuildNumber);
            }
            #endregion

            return true;
        }

        internal static void TrackEvent(string Category, string Action, string Label, int Value = 1, bool DevOverride = false)
        {
            try
            {
                if (!App.IsDev || DevOverride)
                {
                    GATrack.TrackEventAsync(Category, Action, Label, null, null, Value);
                }
                else
                {
                    var test = GATrack.UserAgent;
                }
            }
            catch
            {
            }
        }

        public class AnalyticsSession : IAnalyticsSession
        {
            internal AnalyticsSession(string session)
            {
                SessionId = session;
            }

            internal string SessionId { get; set; }

            public string GenerateCacheBuster()
            {
                var random = new Random((int)DateTime.UtcNow.Ticks);
                return random.Next(100000000, 999999999).ToString(CultureInfo.InvariantCulture);
            }

            public string GenerateSessionId()
            {
                if (SessionId != string.Empty)
                {
                    return SessionId;
                }
                else
                {
                    SessionId = Guid.NewGuid().ToString();
                    UserData.Set("ga", SessionId);
                    UserData.Save();
                    return SessionId;
                }
            }
        }
    }
}
