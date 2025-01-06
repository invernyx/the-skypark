using Orbx.DataManager.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class ApronEdgeLights : SubRecord, IHasData
    {
        public List<GeoLocation> Vertices { get; set; }
        public List<Tuple<int, int>> Edges { get; set; }

        public ApronEdgeLights(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            Vertices = new List<GeoLocation>();
            Edges = new List<Tuple<int, int>>();
        }

        public void GetData(BinaryReader reader)
        {
            reader.ReadUInt16(); //skip

            var vertexCount = reader.ReadUInt16();
            var edgeCount = reader.ReadUInt16();

            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();

            for (int i = 0; i < vertexCount; i++)
            {
                var lon = reader.ReadUInt32();
                var lat = reader.ReadUInt32();

                Vertices.Add(GeoLocation.FromDwords(lat, lon));
            }

            for (int i = 0; i < edgeCount; i++)
            {
                reader.ReadUInt32();

                Edges.Add(new Tuple<int, int>(reader.ReadInt16(), reader.ReadInt16()));
            }
        }
    }
}
