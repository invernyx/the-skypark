using Orbx.DataManager.Core.Common;
using System.IO;
using System.Text;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class AirportName : SubRecord, IHasData
    {
        public string Name { get; set; }

        public AirportName(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {
            var nameBytes = reader.ReadBytes((int)RecordSize - 6);
            Name = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        }
    }
}
