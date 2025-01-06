using Orbx.DataManager.Core.Common;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class SkipSection : SubRecord, IHasData
    {
        public SkipSection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {
            Skip();
        }
    }
}
