using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;
using System.Text;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Com : SubRecord, IHasData
    {
        public FrequencyType Type { get; set; }
        public double Frequency { get; set; }
        public string Name { get; set; }

        public Com(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {
            Type = (FrequencyType)reader.ReadUInt16();
            Frequency = reader.ReadUInt32() / 1000000.0;

            var nameBytes = reader.ReadBytes((int)RecordSize - 12);
            Name = Encoding.ASCII.GetString(nameBytes).Trim('\0');
        }

        public override string ToString()
        {
            return $"{Type}: {Name} ({Frequency})";
        }
    }
}
