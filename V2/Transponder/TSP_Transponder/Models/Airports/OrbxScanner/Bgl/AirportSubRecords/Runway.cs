using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Runway : SubRecord, IHasData
    {

        public Surface Surface { get; set; }

        public string Primary { get; set; }
        public string Secondary { get; set; }

        public string PrimaryIlsIdent { get; set; }
        public string SecondaryIlsIdent { get; set; }

        public GeoLocation Location { get; set; }
        public double Altitude { get; set; }

        public double Length { get; set; }
        public double Width { get; set; }
        public double Heading { get; set; }

        public double PatternAltitude { get; set; }

        public List<ApproachLights> ApproachLights { get; set; }
        public List<Vasi> Vasi { get; set; }
        public List<OffsetThreshold> OffsetThreshold { get; set; }

        public Runway(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            ApproachLights = new List<ApproachLights>();
            Vasi = new List<Vasi>();
            OffsetThreshold = new List<OffsetThreshold>();
        }

        public void GetData(BinaryReader reader)
        {
            var cardinalPoints = new string[]{
                "N",
                "NE",
                "E",
                "SE",
                "S",
                "SW",
                "W",
                "NW"
            };
            var designators = new string[]
            {
                "",
                "L",
                "R",
                "C",
                "W",
                "A",
                "B"
            };

            Surface = (Surface)reader.ReadUInt16();


            var primaryNumber = reader.ReadByte();
            var primaryDesignator = reader.ReadByte();
            var secondaryNumber = reader.ReadByte();
            var secondaryDesignator = reader.ReadByte();

            Primary = (primaryNumber < 37 ? primaryNumber.ToString() : cardinalPoints[primaryNumber - 37]) + designators[primaryDesignator];

            Secondary = (secondaryNumber < 37 ? secondaryNumber.ToString() : cardinalPoints[secondaryNumber - 37]) + designators[secondaryDesignator];

            PrimaryIlsIdent = CodedString.FromDword(reader.ReadUInt32(), true);
            SecondaryIlsIdent = CodedString.FromDword(reader.ReadUInt32(), true);

            var longitude = reader.ReadUInt32();
            var latitude = reader.ReadUInt32();
            Location = GeoLocation.FromDwords(latitude, longitude);

            Altitude = reader.ReadUInt32() / 1000.0;

            Length = reader.ReadSingle();
            Width = reader.ReadSingle();
            Heading = reader.ReadSingle();

            PatternAltitude = reader.ReadSingle();

            //TODO: parse marking flags
            var markingFlags = reader.ReadUInt16();

            // TODO: parse light flags
            var lightFlags = reader.ReadByte();

            // TODOL parse pattern flags
            var patternFlags = reader.ReadByte();

            var endValue = StartOffset + RecordSize;
            while (reader.BaseStream.Position < endValue)
            {
                var type = reader.ReadUInt16();

                switch (type)
                {
                    case 0x000F:
                    case 0x0010:
                        var approachSubRecord = new ApproachLights(reader, type);
                        approachSubRecord.GetData(reader);
                        ApproachLights.Add(approachSubRecord);
                        break;

                    case 0x000B:
                    case 0x000C:
                    case 0x000D:
                    case 0x000E:
                        var vasiSubRecord = new Vasi(reader, type);
                        vasiSubRecord.GetData(reader);
                        Vasi.Add(vasiSubRecord);
                        break;

                    case 0x0005:
                    case 0x0006:
                        var offsetThreshold = new OffsetThreshold(reader, type);
                        offsetThreshold.GetData(reader);
                        OffsetThreshold.Add(offsetThreshold);
                        break;

                    case 0x0007: // blastpad prim
                    case 0x0008: // blastpad sec
                    case 0x0009: // overrun prim
                    case 0x000A: // overrun sec
                        var skipper = new SkipSection(reader, type);
                        skipper.GetData(reader);
                        break;

                    default:
                        Console.WriteLine("Encountered unknown runway subrecord " + type);
                        reader.BaseStream.Seek(endValue, SeekOrigin.Begin);
                        return;
                }
            }
        }

        public override string ToString()
        {
            return $"RWY {Primary}-{Secondary} {Location} length: {Length}, width: {Width}, surface: {Surface}";
        }
    }
}
