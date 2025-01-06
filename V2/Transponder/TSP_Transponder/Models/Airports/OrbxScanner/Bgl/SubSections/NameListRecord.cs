using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orbx.DataManager.Core.Bgl.SubSections
{
    public struct NameListRecord
    {
        public UInt16 Id;
        public UInt32 Size;
        public UInt16 RegionNamesCount;
        public UInt16 CountryNamesCount;
        public UInt16 StateNamesCount;
        public UInt16 CityNamesCount;
        public UInt16 AirportNamesCount;
        public UInt16 IcaoCount;
        public UInt32 RegionListOffset;
        public UInt32 CountryListOffSet;
        public UInt32 StateListOffset;
        public UInt32 CityListOffset;
        public UInt32 AirportListOffset;
        public UInt32 IcaoListOffset;
    }

    public struct Icao
    {
        public byte RegionNameIndex;
        public byte CountryNameIndex;
        public UInt16 StateNameIndex;
        public UInt16 CityNameIndex;
        public UInt16 AirportNameIndex;
        public string IcaoIdent;
        public UInt32 Unknown;
        public UInt16 UpperLeftLongOfQMID;
        public UInt16 UpperLeftLatOfQMID;

        public string RegionName;
        public string CountryName;
        public string StateName;
        public string CityName;
        public string AirportName;
    }

    public class NameListObject
    {
        private readonly UInt32 numberOfRecords;
        private readonly UInt32 offset;
        private List<UInt32> indexes = new List<uint>();
        public List<string> Names = new List<string>();
        private long bufferOffset;

        public NameListObject(UInt32 numberOfRecords, UInt32 offset)
        {
            this.numberOfRecords = numberOfRecords;
            this.offset = offset;
        }

        public void GetData(BinaryReader reader)
        {
            indexes = ReadIndexes(reader);
            bufferOffset = reader.BaseStream.Position;
            reader.BaseStream.Seek(bufferOffset, SeekOrigin.Begin);

            foreach (var index in indexes)
            {
                reader.BaseStream.Seek(bufferOffset + index, SeekOrigin.Begin);
                string name = ReadStringTillNullTerminate(reader);
                Names.Add(name);
            }
        }

        private List<UInt32> ReadIndexes(BinaryReader reader)
        {
            var nameIndexes = new List<UInt32>();
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (int i = 0; i < numberOfRecords; i++)
            {
                nameIndexes.Add(reader.ReadUInt32());
            }
            return nameIndexes;
        }

        private string ReadStringTillNullTerminate(BinaryReader reader)
        {
            List<byte> stringInBytes = new List<byte>();
            byte current;
            while ((current = reader.ReadByte()) != (byte)'\0')
            {
                stringInBytes.Add(current);
            }
            return Encoding.UTF8.GetString(stringInBytes.ToArray(), 0, stringInBytes.Count);
        }
    }
}
