using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class OffsetThreshold : SubRecord, IHasData
    {
        public bool IsPrimary { get; set; }
        public Surface Surface { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }

        public OffsetThreshold(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            IsPrimary = identifier == 0x0005;
        }

        public void GetData(BinaryReader reader)
        {
            Surface = (Surface)reader.ReadUInt16();
            Length = reader.ReadSingle();
            Width = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"Offset prim: {IsPrimary}, surf {Surface}";
        }
    }
}
