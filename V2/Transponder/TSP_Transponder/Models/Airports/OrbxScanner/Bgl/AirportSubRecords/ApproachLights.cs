using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class ApproachLights : SubRecord, IHasData
    {
        public ApproachLightType System { get; set; }
        public bool EndLights { get; set; }
        public bool Reil { get; set; }
        public bool Touchdown { get; set; }
        public byte StrobeCount { get; set; }
        public bool IsPrimary { get; set; }

        public ApproachLights(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            IsPrimary = identifier == 0x000F;
        }

        public void GetData(BinaryReader reader)
        {
            var main = reader.ReadByte();

            System = (ApproachLightType)(main & 0b11111);

            EndLights = (main & 0b100000) > 0;
            Reil = (main & 0b1000000) > 0;
            Touchdown = (main & 0b10000000) > 0;

            StrobeCount = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"{System} prim: {IsPrimary}, strobe: {StrobeCount}";
        }
    }
}
