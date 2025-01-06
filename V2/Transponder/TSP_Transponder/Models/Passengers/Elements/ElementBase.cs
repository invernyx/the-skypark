using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Passengers.Elements
{
    internal class ElementBase
    {
        internal int Width = 1;
        internal int Height = 1;

        internal virtual void Emit()
        {

        }
    }
}
