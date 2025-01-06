using Orbx.DataManager.Core.Common;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TowerSceneryObject : SubRecord, IHasData
    {

        public TowerSceneryObject(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {

        }
    }
}
