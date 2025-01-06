using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace TSP_Transponder.Models.Aircraft
{
    
    public class AircraftMountingPoint
    {
        internal bool Occupied = false;
        internal Point3D Location = new Point3D();
        internal float Pitch = 0;
        internal float Bank = 0;
        internal float Heading = 0;
        internal float Size = 0;

        public AircraftMountingPoint(string Name, Dictionary<string, dynamic> Struct)
        {
            try
            {
                Location.X = Convert.ToSingle(Struct["Location"][0]);
                Location.Y = Convert.ToSingle(Struct["Location"][1]);
                Location.Z = Convert.ToSingle(Struct["Location"][2]);

                Pitch = Convert.ToSingle(Struct["Pitch"]);
                Bank = Convert.ToSingle(Struct["Bank"]);
                Heading = Convert.ToSingle(Struct["Heading"]);
                Size = Convert.ToSingle(Struct["Size"]);

            }
            catch
            {
                Console.WriteLine("Mounting Point is not valid for " + Name);
            }
        }

    }
}
