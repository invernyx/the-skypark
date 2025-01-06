using Orbx.DataManager.Core.Common;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.SubSections
{
    public class AirportSubsection : SubSection, IHasData
    {
        public List<AirportRecord> Airports { get; set; }

        public AirportSubsection()
        {
            Airports = new List<AirportRecord>();
        }

        public void GetData(BinaryReader reader)
        {
            reader.BaseStream.Seek(FileOffset, SeekOrigin.Begin);

            for (int i = 0; i < NumberOfRecords; i++)
            {
                AirportRecord airport = new AirportRecord();
                airport.GetData(reader);

                Airports.Add(airport);
            }
        }
    }
}
