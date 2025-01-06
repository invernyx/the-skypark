using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Messaging
{
    class Contacts
    {
        public static void Startup()
        {
            new Handle()
            {
                FirstName = "You",
                Ident = "%user%"
            }.Store();

            new Handle()
            {
                FirstName = "Brigit",
                Ident = "brigit"
            }.Store();

            new Handle()
            {
                FirstName = "Pablo",
                Ident = "pablo"
            }.Store();

        }
    }
}
