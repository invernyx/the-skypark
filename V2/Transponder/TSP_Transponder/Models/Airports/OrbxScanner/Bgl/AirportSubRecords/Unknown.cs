using Orbx.DataManager.Core.Common;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    class Unknown : SubRecord, IHasData
    {

        public Unknown(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {

        }
    }
}
