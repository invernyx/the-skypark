using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Payload
{
    public class CargoUnit
    {
        public CargoManifest Manifest = null;
        public Guid? GUID = null;
        public ushort Health = 100;

        public CargoUnit(CargoManifest Manifest, Dictionary<string, dynamic> config)
        {
            this.Manifest = Manifest;
            GUID = new Guid(config["GUID"]);
            Health = Convert.ToUInt16(config["Health"]);
        }

        public CargoUnit(CargoManifest Manifest)
        {
            this.Manifest = Manifest;
            GUID = Guid.NewGuid();
        }

        public Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "GUID", GUID.ToString() },
                { "Health", Convert.ToInt32(Health) },
            };
        }

        public void ImportState(Dictionary<string, dynamic> state)
        {
            Health = Convert.ToUInt16(state["Health"]);
        }
    }
}
