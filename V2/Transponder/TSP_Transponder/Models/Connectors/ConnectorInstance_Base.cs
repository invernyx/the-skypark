using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.WorldManager;
using static TSP_Transponder.Models.PathFinding.VehMovement;
using static TSP_Transponder.Models.SimLibrary;

namespace TSP_Transponder.Models.Connectors
{
    abstract public class ConnectorInstance_Base
    {
        public MainWindow MW = null;

        public virtual bool IsConnected
        {
            get
            {
                try
                {
                    return false;
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }

        public virtual void Connect()
        {

        }

        public virtual void Disconnect()
        {

        }

        public virtual void Startup(MainWindow _Window)
        {
            MW = _Window;
        }

        public virtual Bitmap CaptureImage(ImageFormat format, string savePath = null)
        {
            return null;
        }

        public virtual void Configure(Simulator sim, bool toInstall = true)
        {
        }

        public virtual void SetPause(bool state)
        {
        }

        public virtual void GetAircraftImage()
        {

        }

        public virtual void MoveSimObject(uint id, MovementState State)
        {
        }

        public virtual void AttachSimObject(uint simIDChild, uint simIDParent, AircraftMountingPoint mount)
        {
        }

        public virtual void ReleaseSimObject(uint simIDChild, uint simIDParent)
        {
        }

        public virtual void CreateSimObject(long uid, string simobject, GeoPosition Loc, SceneObjType Type)
        {
        }

        public virtual void DestroySimObject(uint simID)
        {
        }

        public virtual void SendFlightPlan(string path)
        {

        }

        public virtual void GetCG()
        {

        }

        public virtual void CalibratePayloadStations(AircraftInstance Acf, Action<List<AircraftPayloadStation>> Done)
        {

        }
        
        public virtual void SendPayload(int Station, float WeightPounds)
        {

        }

        public virtual void SendMessage(string msg, float duration)
        {
        }

        public virtual void CreateEffect(long uid, string effect, GeoPosition Loc, int duration)
        {
        }

        public virtual void CreateAttachedEffect(long uid, string effect, Point3D Pos, int duration)
        {
        }

        public virtual void MonitorAI(uint SimID)
        {
        }

        public virtual void DestroyEffect(uint simID)
        {
        }

        public virtual void ReadAircraftConfig(Simulator Simulator, string DirectoryName, string DirectoryFull, Action<string, string, string> Callback)
        {
        }

        public virtual void SimLoaded(bool Status)
        {
        }

        

    }
}
