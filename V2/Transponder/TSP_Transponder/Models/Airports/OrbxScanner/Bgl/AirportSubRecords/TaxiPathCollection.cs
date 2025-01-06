using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    class TaxiPathCollection : SubRecord, IHasData
    {
        public List<TaxiPath> Paths { get; set; }

        public TaxiPathCollection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            Paths = new List<TaxiPath>();
        }

        public void GetData(BinaryReader reader)
        {
            var pathCount = reader.ReadUInt16();

            for (int i = 0; i < pathCount; i++)
            {
                var startIndex = reader.ReadUInt16();
                var endIndexDesignator = reader.ReadUInt16();

                var typeDrawOptions = reader.ReadByte();
                var numberIndex = reader.ReadByte();
                var markingLighting = reader.ReadByte();
                var surface = reader.ReadByte();
                var width = reader.ReadSingle();
                var weightLimit = reader.ReadSingle();
                reader.ReadUInt32(); // ignored

                Paths.Add(new TaxiPath
                {
                    StartIndex = startIndex,
                    EndIndex = endIndexDesignator & 0b111111111111,
                    RunwayDesignator = (endIndexDesignator >> 12) & 0b1111,
                    Type = (TaxiwayPathType)(typeDrawOptions & 0b11111),
                    DrawSurface = (typeDrawOptions & 0b000001) > 0,
                    DrawDetail = (typeDrawOptions & 0b0000001) > 0,
                    RunwayNumberNameIndex = numberIndex,
                    CentreLine = (markingLighting & 0b1) > 0,
                    CentreLineLit = (markingLighting & 0b10) > 0,
                    LeftEdgeType = (TaxiwayEdgeMarking)((markingLighting >> 2) & 0b11),
                    LeftEdgeLit = (markingLighting & 0b10000) > 0,
                    RightEdgeType = (TaxiwayEdgeMarking)((markingLighting >> 5) & 0b11),
                    RightEdgeLit = (markingLighting & 0b10000000) > 0,
                    Surface = (Surface)surface,
                    Width = width,
                    WeightLimit = weightLimit
                });
            }
        }
    }
}