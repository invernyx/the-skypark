using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Vasi : SubRecord, IHasData
    {
        public bool IsPrimary { get; set; }
        public bool IsLeft { get; set; }

        public VasiType Type { get; set; }

        public double BiasX { get; set; }
        public double BiasZ { get; set; }
        public double Spacing { get; set; }
        public double Pitch { get; set; }

        public Vasi(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            IsPrimary = identifier == 0x000B || identifier == 0x000C;
            IsLeft = identifier == 0x000B || identifier == 0x000D;
        }

        public void GetData(BinaryReader reader)
        {
            Type = (VasiType)reader.ReadUInt16();

            BiasX = reader.ReadSingle();
            BiasZ = reader.ReadSingle();
            Spacing = reader.ReadSingle();
            Pitch = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"{Type} {BiasX} {BiasZ}";
        }
    }
}
