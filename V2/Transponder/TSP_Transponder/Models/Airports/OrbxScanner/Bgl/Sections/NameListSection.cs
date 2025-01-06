using Orbx.DataManager.Core.Bgl.SubSections;
using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.Sections
{
    public class NameListSection : Section, IHasData
    {
        public List<NameListRecord> NameLists { get; private set; }
        public List<Icao> Icaos { get; private set; }

        public NameListSection()
        {
            NameLists = new List<NameListRecord>();
        }

        public override void GetData(BinaryReader reader)
        {
            reader.BaseStream.Seek(FileOffset, SeekOrigin.Begin);

            for (int i = 0; i < SubSectionCount; i++)
            {
                //Read the start of section header.
                uint qmid = reader.ReadUInt32();
                uint icaoEntriesCount = reader.ReadUInt32();
                uint subSectionOffset = reader.ReadUInt32();
                uint subSectionSize = reader.ReadUInt32();

                // Jump to the start of the record and parse it.
                reader.BaseStream.Seek(subSectionOffset, SeekOrigin.Begin);

                var record = new NameListRecord
                {
                    Id = reader.ReadUInt16(),
                    Size = reader.ReadUInt32(),
                    RegionNamesCount = reader.ReadUInt16(),
                    CountryNamesCount = reader.ReadUInt16(),
                    StateNamesCount = reader.ReadUInt16(),
                    CityNamesCount = reader.ReadUInt16(),
                    AirportNamesCount = reader.ReadUInt16(),
                    IcaoCount = reader.ReadUInt16(),
                    RegionListOffset = reader.ReadUInt32(),
                    CountryListOffSet = reader.ReadUInt32(),
                    StateListOffset = reader.ReadUInt32(),
                    CityListOffset = reader.ReadUInt32(),
                    AirportListOffset = reader.ReadUInt32(),
                    IcaoListOffset = reader.ReadUInt32()
                };


                var regionNames = new NameListObject(record.RegionNamesCount, record.RegionListOffset + subSectionOffset);
                regionNames.GetData(reader);

                var airportNames = new NameListObject(record.AirportNamesCount, record.AirportListOffset + subSectionOffset);
                airportNames.GetData(reader);

                var cityNames = new NameListObject(record.CityNamesCount, record.CityListOffset + subSectionOffset);
                cityNames.GetData(reader);

                var states = new NameListObject(record.StateNamesCount, record.StateListOffset + subSectionOffset);
                states.GetData(reader);

                var countries = new NameListObject(record.CountryNamesCount, record.CountryListOffSet + subSectionOffset);
                countries.GetData(reader);

                Icaos = new List<Icao>();
                reader.BaseStream.Seek(record.IcaoListOffset + subSectionOffset, SeekOrigin.Begin);
                for (i = 0; i < record.IcaoCount; i++)
                {
                    var icaoRecord = new Icao
                    {
                        RegionNameIndex = reader.ReadByte(),
                        CountryNameIndex = reader.ReadByte(),
                        StateNameIndex = (ushort)(reader.ReadUInt16() >> 4),
                        CityNameIndex = reader.ReadUInt16(),
                        AirportNameIndex = reader.ReadUInt16(),
                        IcaoIdent = CodedString.FromDword(reader.ReadUInt32(), true), // Icaos use there own special encoding.
                        Unknown = reader.ReadUInt32(),
                        UpperLeftLongOfQMID = reader.ReadUInt16(),
                        UpperLeftLatOfQMID = reader.ReadUInt16()
                    };
                    icaoRecord.RegionName = regionNames.Names[icaoRecord.RegionNameIndex];
                    icaoRecord.CountryName = countries.Names[icaoRecord.CountryNameIndex];
                    icaoRecord.CityName = cityNames.Names[icaoRecord.CityNameIndex];
                    icaoRecord.StateName = states.Names[icaoRecord.StateNameIndex];
                    icaoRecord.AirportName = airportNames.Names[icaoRecord.AirportNameIndex];

                    Icaos.Add(icaoRecord);
                }
            }
        }

    }
}
