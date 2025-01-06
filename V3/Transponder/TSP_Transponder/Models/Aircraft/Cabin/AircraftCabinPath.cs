using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Attributes;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.Aircraft.Cabin
{
    public class AircraftCabinPath
    {
        public ushort X = 0;
        public ushort Y = 0;
        public ushort Z = 0;

        public AircraftCabinPath()
        {
        }

        public override string ToString()
        {
            return X + ", " + Y + ", " + Z;
        }

    }
}
