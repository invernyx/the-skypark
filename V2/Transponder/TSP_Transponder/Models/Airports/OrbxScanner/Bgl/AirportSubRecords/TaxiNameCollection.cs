using Orbx.DataManager.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiNameCollection : SubRecord, IHasData
    {
        public List<string> Names { get; set; }

        public TaxiNameCollection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            Names = new List<string>();
        }

        public void GetData(BinaryReader reader)
        {
            var nameCount = reader.ReadUInt16();

            for (int i = 0; i < nameCount; i++)
            {
                Names.Add(Encoding.ASCII.GetString(reader.ReadBytes(8)).Trim('\0'));
            }
        }
    }
}
