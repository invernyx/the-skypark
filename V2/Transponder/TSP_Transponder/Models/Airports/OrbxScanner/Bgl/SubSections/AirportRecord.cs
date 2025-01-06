using Orbx.DataManager.Core.Bgl.AirportSubRecords;
using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.SubSections
{
    public class AirportRecord : IHasData
    {
        public string ICAO { get; set; }
        public string Region { get; set; }

        public string Name { get; set; }

        public GeoLocation Location { get; set; }
        public double Altitude { get; set; }

        public GeoLocation TowerLocation { get; set; }
        public double TowerAltitude { get; set; }

        public List<Runway> Runways { get; set; }
        public List<Start> Starts { get; set; }
        public List<Com> Frequencies { get; set; }
        public List<TaxiPoint> TaxiPoints { get; set; }
        public List<TaxiParking> Parking { get; set; }
        public List<TaxiPath> Paths { get; set; }
        public List<string> TaxiNames { get; set; }
        public List<Apron> Aprons { get; set; }
        public List<Helipad> Helipads { get; set; }

        public ApronEdgeLights ApronEdgeLights { get; set; }

        public AirportRecord()
        {
            Runways = new List<Runway>();
            Starts = new List<Start>();
            Frequencies = new List<Com>();
            TaxiPoints = new List<TaxiPoint>();
            Parking = new List<TaxiParking>();
            Paths = new List<TaxiPath>();
            TaxiNames = new List<string>();
            Aprons = new List<Apron>();
            Helipads = new List<Helipad>();
        }

        public void GetData(BinaryReader reader)
        {
            var startPosition = reader.BaseStream.Position;

            var id = reader.ReadUInt16();
            var recordSize = reader.ReadUInt32();

            var runwayCount = reader.ReadByte();
            var comCount = reader.ReadByte();
            var startCount = reader.ReadByte();
            var approachCount = reader.ReadByte();

            // TODO: read apron masks and if there is a delete
            var apronMask = reader.ReadByte();

            var helipadCount = reader.ReadByte();

            var longitude = reader.ReadUInt32();
            var latitude = reader.ReadUInt32();
            Location = GeoLocation.FromDwords(latitude, longitude);
            Altitude = reader.ReadUInt32() / 1000.0;

            var towerLongitude = reader.ReadUInt32();
            var towerLatitude = reader.ReadUInt32();
            TowerLocation = GeoLocation.FromDwords(towerLatitude, towerLongitude);
            TowerAltitude = reader.ReadUInt32() / 1000.0;

            var magneticVariation = reader.ReadSingle();
            if (magneticVariation > 180)
                magneticVariation -= 360;

            ICAO = CodedString.FromDword(reader.ReadUInt32(), true);
            Region = CodedString.FromDword(reader.ReadUInt32(), true);

            // TODO: read fuel types
            var fuel = reader.ReadUInt32();

            var unknown = reader.ReadByte();
            var trafficScalar = reader.ReadByte() / 255.0;
            var unknown2 = reader.ReadUInt16();

            try
            {
                ReadSubrecords(reader, startPosition + recordSize);
            }
            catch (System.IO.FileFormatException ex)
            {
                Console.WriteLine(ex);
                reader.BaseStream.Seek(startPosition + recordSize, SeekOrigin.Begin);
            }
        }

        private void ReadSubrecords(BinaryReader reader, long endSize)
        {
            while (reader.BaseStream.Position < endSize)
            {
                var type = reader.ReadInt16();

                switch (type)
                {
                    case 0x0019:
                        var nameRecord = new AirportName(reader, type);
                        nameRecord.GetData(reader);
                        Name = nameRecord.Name;
                        break;
                    case 0x0004:
                        var runwayRecord = new Runway(reader, type);
                        runwayRecord.GetData(reader);
                        Runways.Add(runwayRecord);
                        break;
                    case 0x0011:
                        var startRecord = new Start(reader, type);
                        startRecord.GetData(reader);
                        Starts.Add(startRecord);
                        break;
                    case 0x0012:
                        var comRecord = new Com(reader, type);
                        comRecord.GetData(reader);
                        Frequencies.Add(comRecord);
                        break;
                    case 0x003B:
                        var unknownRecord = new Unknown(reader, type);
                        unknownRecord.Skip();
                        break;
                    case 0x0066:
                        var towerObject = new TowerSceneryObject(reader, type);
                        towerObject.Skip(); //TODO: this once we have a MDL reader
                        break;
                    case 0x001A:
                        var taxiPoint = new TaxiPointCollection(reader, type);
                        taxiPoint.GetData(reader);
                        TaxiPoints.AddRange(taxiPoint.Points);
                        break;
                    case 0x003D:
                        var taxiParking = new TaxiParkingCollection(reader, type);
                        taxiParking.GetData(reader);
                        Parking.AddRange(taxiParking.Parking);
                        break;
                    case 0x001C:
                        var taxiPath = new TaxiPathCollection(reader, type);
                        taxiPath.GetData(reader);
                        Paths.AddRange(taxiPath.Paths);
                        break;
                    case 0x001D:
                        var taxiName = new TaxiNameCollection(reader, type);
                        taxiName.GetData(reader);
                        TaxiNames = taxiName.Names;
                        break;
                    case 0x0037:
                        var aprons = new ApronCollection(reader, type);
                        aprons.GetData(reader);
                        Aprons.Add(aprons.Apron);
                        break;
                    case 0x0031:
                        var edgeLights = new ApronEdgeLights(reader, type);
                        edgeLights.GetData(reader);
                        ApronEdgeLights = edgeLights;
                        break;
                    case 0x0026:
                        var helipad = new Helipad(reader, type);
                        helipad.GetData(reader);
                        Helipads.Add(helipad);
                        break;
                    case 0x0024: // approach
                    case 0x0038: // blastfence
                    case 0x0039: // boundarfence
                    case 0x003E:
                    case 0x0040:
                    case 0x0033: // delete airport
                    case 0x003A: //jetway
                        var skipSection = new SkipSection(reader, type);
                        skipSection.GetData(reader);
                        break;
                    default:
                        throw new System.IO.FileFormatException("Unknown airport subrecord type " + type);
                }
            }
        }

        public override string ToString()
        {
            return $"{ICAO} {Name} {Location}";
        }
    }
}
