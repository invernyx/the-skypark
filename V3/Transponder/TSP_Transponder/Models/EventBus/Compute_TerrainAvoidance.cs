using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.Topography;
using TSP_Transponder.Models.Topography.Utils;
using TSP_Transponder.Models.WorldManager;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_TerrainAvoidance : Compute_Base
    {
        static TileDataSet tds = null;

        internal List<List<PointElevation>> lastElevation = new List<List<PointElevation>>();

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if(tds == null)
                tds = Topo.GetNewDataset();

            if (!Session.Started || _TemporalNewBuffer.IS_SLEW_ACTIVE || _TemporalLast.IS_SLEW_ACTIVE)
                return;

            if(_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND < 50 || _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > 15000)
            {
                lastElevation = new List<List<PointElevation>>();
            }
            else
            {
                List<List<PointElevation>> rays_pts = new List<List<PointElevation>>();
                List<List<GeoLoc>> rays = new List<List<GeoLoc>>();
                int deg = -60;
                while (deg <= 60)
                {
                    List<GeoLoc> ray = new List<GeoLoc>()
                    {
                        _TemporalNewBuffer.PLANE_LOCATION,
                        Utils.MapOffsetPosition(_TemporalNewBuffer.PLANE_LOCATION, 5000, _TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED > 30 ? _TemporalNewBuffer.PLANE_COURSE + deg : _TemporalNewBuffer.PLANE_HEADING_DEGREES + deg)
                    };

                    rays.Add(ray);
                    deg += 10;
                }

                foreach (var ray in rays)
                {
                    List<PointElevation> ray_pts = Topo.GetElevationAlongPath(ray[0], ray[1], 50, tds);
                    rays_pts.Add(ray_pts);
                }

                lastElevation = rays_pts;
            }


        }

        public List<List<List<double>>> GetList()
        {
            return lastElevation.Select(x0 => x0.Select(x1 => new List<double>() { x1.Lon, x1.Lat, x1.Elevation }).ToList()).ToList();
        }
    }
}
