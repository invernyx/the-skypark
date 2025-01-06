using Orbx.DataManager.Core.Bgl.SubSections;
using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.Sections
{
    public class AirportSection : Section, IHasData
    {
        public List<AirportRecord> Airports { get; private set; }

        public AirportSection()
        {
            Airports = new List<AirportRecord>();
        }

        public override void GetData(BinaryReader reader)
        {
            reader.BaseStream.Seek(FileOffset, SeekOrigin.Begin);
            List<AirportSubsection> aptSubsections = new List<AirportSubsection>();

            // read subsections
            for (int i = 0; i < SubSectionCount; i++)
            {
                uint qmidA = reader.ReadUInt32();
                uint qmidB = SubSectionSize == 20 ? reader.ReadUInt32() : 0;

                var numberOfRecords = reader.ReadInt32();

                var fileOffset = reader.ReadUInt32();
                var subsectionSize = reader.ReadUInt32();

                aptSubsections.Add(new AirportSubsection
                {
                    Qmid = Qmid.FromDwords(qmidA, qmidB),
                    NumberOfRecords = numberOfRecords,
                    FileOffset = fileOffset,
                    SubsectionSize = subsectionSize
                });
            }

            foreach (var airport in aptSubsections)
            {
                airport.GetData(reader);

                Airports.AddRange(airport.Airports);
            }
        }
    }
}
