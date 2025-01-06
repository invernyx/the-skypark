using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.WeatherModel;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Weather : Compute_Base
    {
        WeatherData PreviousWx = null;
        int HighestCloud = -9999;
        
        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!_TemporalNewBuffer.IS_SLEW_ACTIVE && Session.Started && LatestWeatherData != null && Session.Timer.ElapsedMilliseconds > 60000)
            {
                if (PreviousWx == null)
                {
                    PreviousWx = LatestWeatherData;
                    Session.AddEvent(new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.Weather,
                        TemporalRef = null,
                        Params = new Dictionary<string, dynamic>()
                        {
                            { "Precipitation", EnumAttr.GetDescription(PreviousWx.Precipitation) },
                            { "PrecipitationRate", PreviousWx.Precipitation_Rate },
                            { "Thunderstorm", PreviousWx.Thunderstorm },
                            { "Temperature", PreviousWx.Temperature },
                            { "DewPoint", PreviousWx.DewPoint },
                            { "Visibility", PreviousWx.VisibilitySM },
                            { "WindSpeed", PreviousWx.WindSpeed },
                            { "WindGust", PreviousWx.WindGust },
                            { "WindHeading", PreviousWx.WindHeading },
                            { "Altimeter", PreviousWx.Altimeter },
                            { "Clouds", CloudsToString(PreviousWx) },
                        }
                    }, false);
                }
                else
                {
                    Dictionary<string, dynamic> ToUpdate = new Dictionary<string, dynamic>();

                    #region Check Precipitations
                    if(LatestWeatherData.Precipitation != PreviousWx.Precipitation)
                    {
                        ToUpdate.Add("Precipitation", EnumAttr.GetDescription(LatestWeatherData.Precipitation));
                        ToUpdate.Add("PrecipitationRate", LatestWeatherData.Precipitation_Rate);

                        PreviousWx.Precipitation = LatestWeatherData.Precipitation;
                        PreviousWx.Precipitation_Rate = LatestWeatherData.Precipitation_Rate;
                    }
                    #endregion

                    #region Check Thunderstorm
                    if (LatestWeatherData.Thunderstorm != PreviousWx.Thunderstorm)
                    {
                        ToUpdate.Add("Thunderstorm", Convert.ToString(LatestWeatherData.Thunderstorm));

                        PreviousWx.Thunderstorm = LatestWeatherData.Thunderstorm;
                    }
                    #endregion

                    #region Check Temperature & DewPoint
                    if (_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND < 50)
                    {
                        if (Math.Abs(LatestWeatherData.Temperature - PreviousWx.Temperature) > 5 || Math.Abs(LatestWeatherData.DewPoint - PreviousWx.DewPoint) > 5)
                        {
                            ToUpdate.Add("Temperature", Convert.ToString(LatestWeatherData.Temperature));
                            ToUpdate.Add("DewPoint", Convert.ToString(LatestWeatherData.DewPoint));

                            PreviousWx.Temperature = LatestWeatherData.Temperature;
                            PreviousWx.DewPoint = LatestWeatherData.DewPoint;
                        }
                    }
                    #endregion

                    #region Check Visibility
                    if (Math.Abs(LatestWeatherData.VisibilitySM - PreviousWx.VisibilitySM) > 50)
                    {
                        ToUpdate.Add("Visibility", Convert.ToString(LatestWeatherData.VisibilitySM));

                        PreviousWx.VisibilitySM = LatestWeatherData.VisibilitySM;
                    }
                    #endregion

                    #region Check Wind
                    if (Math.Abs(LatestWeatherData.WindSpeed - PreviousWx.WindSpeed) > 5 || Math.Abs(LatestWeatherData.WindGust - PreviousWx.WindGust) > 5 || Math.Abs(LatestWeatherData.WindHeading - PreviousWx.WindHeading) > 20)
                    {
                        ToUpdate.Add("WindSpeed", Convert.ToString(LatestWeatherData.WindSpeed));
                        ToUpdate.Add("WindGust", Convert.ToString(LatestWeatherData.WindGust));
                        ToUpdate.Add("WindHeading", Convert.ToString(LatestWeatherData.WindHeading));

                        PreviousWx.WindSpeed = LatestWeatherData.WindSpeed;
                        PreviousWx.WindGust = LatestWeatherData.WindGust;
                        PreviousWx.WindHeading = LatestWeatherData.WindHeading;
                    }
                    #endregion

                    #region Check Clouds
                    string Clouds = CloudsToString(SimConnection.LatestWeatherData);
                    if (CloudsToString(PreviousWx) != Clouds)
                    {
                        ToUpdate.Add("Clouds", Clouds);

                        List<WeatherData.CloudLayer> nl = new List<WeatherData.CloudLayer>();
                        foreach(var cl in LatestWeatherData.Clouds)
                        {
                            nl.Add(new WeatherData.CloudLayer()
                            {
                                Base = cl.Base,
                                Coverage = cl.Coverage,
                                Type = cl.Type
                            });
                        }

                        PreviousWx.Clouds = nl;

                        if(nl.Count > 0)
                        {
                            HighestCloud = nl.Max(r => r.Base);
                        }
                        else
                        {
                            HighestCloud = -9999;
                        }
                    }
                    #endregion

                    #region Check Visibility
                    if (Math.Abs(LatestWeatherData.VisibilitySM - PreviousWx.VisibilitySM) > 50)
                    {
                        ToUpdate.Add("Visibility", Convert.ToString(LatestWeatherData.VisibilitySM));

                        PreviousWx.VisibilitySM = LatestWeatherData.VisibilitySM;
                    }
                    #endregion

                    #region Check Altimeter
                    if (Math.Abs(LatestWeatherData.Altimeter - PreviousWx.Altimeter) > 3 && _TemporalNewBuffer.PLANE_ALTITUDE < 18000)
                    {
                        ToUpdate.Add("Altimeter", Convert.ToString(LatestWeatherData.Altimeter));

                        PreviousWx.Altimeter = LatestWeatherData.Altimeter;
                    }
                    #endregion

                    if (ToUpdate.Keys.Count > 0 && _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND - HighestCloud < 5000)
                    {
                        Session.AddEvent(new Event(Session)
                        {
                            Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                            Type = EventType.Weather,
                            TemporalRef = null,
                            Params = ToUpdate
                        }, false);
                    }
                }
                
            }
        }

        internal static string CloudsToString(WeatherData wx)
        {
            string Clouds = "";
            foreach (WeatherData.CloudLayer CloudLayer in wx.Clouds)
            {
                Clouds += EnumAttr.GetDescription(CloudLayer.Type) + "," + CloudLayer.Coverage + "," + CloudLayer.Base + ";";
            }
            return Clouds;
        }
    }
}
