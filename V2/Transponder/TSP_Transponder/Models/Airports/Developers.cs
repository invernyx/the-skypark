using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Airports
{
    public class Developers
    {
        private static readonly Developer NullDev = new Developer()
        {
            Name = "Unknown",
        };

        private static readonly List<Developer> Devs = new List<Developer>()
        {
            new Developer(){
                Name = "Orbx Simulation Systems",
                URL = "https://orbxdirect.com/",
                Matches = new string[]
                {
                    "orbx",
                }
            },
            new Developer(){
                Name = "FlyTampa",
                URL = "https://www.flytampa.org/",
                Matches = new string[]
                {
                    "flytampa",
                    "fly tampa",
                }
            },
            new Developer(){
                Name = "FsDreamTeam",
                URL = "https://www.fsdreamteam.com/",
                Matches = new string[]
                {
                    "fsdt",
                    "fsdreamteam",
                    "fs dreamteam",
                    "fsdream team",
                    "fs dream team",
                }
            },
            new Developer(){
                Name = "Flightbeam Studios",
                URL = "https://www.flightbeam.net/",
                Matches = new string[]
                {
                    "flightbeam",
                    "flight beam",
                }
            },
            new Developer(){
                Name = "Aerosoft",
                URL = "https://www.aerosoft.com/",
                Matches = new string[]
                {
                    "aerosoft",
                }
            },
            new Developer(){
                Name = "PacSim",
                URL = "https://islandsim.com/index.html",
                Matches = new string[]
                {
                    "pacsim",
                    "png-bush-flying",
                }
            },
            new Developer(){
                Name = "LatinVFR",
                URL = "https://www.latinvfr.org/",
                Matches = new string[]
                {
                    "latinvfr",
                    "latin vfr",
                }
            },
            new Developer(){
                Name = "Drzewiecki Design",
                URL = "http://www.drzewiecki-design.net/",
                Matches = new string[]
                {
                    "drzewiecki",
                    "washington x",
                    "seattle x",
                    "ny airports v2 x",
                    "new york city x",
                    "moscow city x"
                }
            },
            new Developer(){
                Name = "Flight Sim Development Group (FSDG)",
                URL = "https://fsdg-online.com/",
                Matches = new string[]
                {
                    "fsdg",
                }
            },
            new Developer(){
                Name = "UK2000",
                URL = "https://www.uk2000scenery.com/",
                Matches = new string[]
                {
                    "uk2000",
                    "uk 2000",
                }
            },
            new Developer(){
                Name = "TropicalSim",
                URL = "http://www.tropicalsim.net/",
                Matches = new string[]
                {
                    "tropicalsim",
                }
            },
            new Developer(){
                Name = "The Airport Guys",
                URL = "https://www.theairportguys.com/",
                Matches = new string[]
                {
                    "the airport guys",
                }
            },
            
            new Developer(){
                Name = "PILOT'S",
                URL = "https://www.pilots.shop/",
                Matches = new string[]
                {
                    "pilot`s",
                    "pilot's",
                }
            },
            new Developer(){
                Name = "29Palms",
                URL = "https://www.29palms.de/",
                Matches = new string[]
                {
                    "29palms",
                }
            },
            new Developer(){
                Name = "JetStream Designs",
                URL = "https://www.jetstream-designs.com/",
                Matches = new string[]
                {
                    "jetstream designs",
                }
            },
            new Developer(){
                Name = "MK Studios",
                URL = "http://mkstudios.pl/page/",
                Matches = new string[]
                {
                    "mkstudios",
                }
            },
            new Developer(){
                Name = "The Airport Guys",
                URL = "https://www.fsimstudios.com/",
                Matches = new string[]
                {
                    "fsimstudios",
                }
            },
            new Developer(){
                Name = "Military AI Works",
                URL = "https://militaryaiworks.com/",
                Matches = new string[]
                {
                    "military ai works",
                }
            },
            new Developer(){
                Name = "Imagine Simulation",
                URL = "https://imaginesim.net/",
                Matches = new string[]
                {
                    "imagine sim",
                    "imaginesim",
                }
            },
            new Developer(){
                Name = "Sim-wings",
                URL = "http://www.sim-wings.de/",
                Matches = new string[]
                {
                    "simwing",
                    "sim-wing",
                    "sim wing",
                }
            },
            new Developer(){
                Name = "iBlueYonder",
                URL = "https://iblueyonder.com/",
                Matches = new string[]
                {
                    "iblueyonder",
                }
            },
            new Developer(){
                Name = "JustSim",
                URL = "https://secure.simmarket.com/justsim.mhtml",
                Matches = new string[]
                {
                    "justsim",
                }
            },
            new Developer(){
                Name = "Paulo Ricardo",
                URL = "https://secure.simmarket.com/paulo-ricardo-fsx.mhtml",
                Matches = new string[]
                {
                    "pauloricardo",
                }
            },
            new Developer(){
                Name = "Cielosim",
                URL = "http://cielosim.com/",
                Matches = new string[]
                {
                    "cielosim",
                }
            },

            
        };

        public static Developer GetDevFromPath(string Query)
        {
            Query = Query.ToLower();
            foreach (Developer Dev in Devs)
            {
                foreach(string DevStr in Dev.Matches)
                {
                    if (Query.Contains(DevStr))
                    {
                        return Dev;
                    }
                }
            }

            return NullDev;
        }

        
        public class Developer
        {
            internal string Name = "";
            internal string[] Matches = { };
            internal string URL = "";
            internal string Img = "";

            internal Dictionary<string, dynamic> ToDictionary()
            {
                Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
                {
                    { "Name", Name },
                    { "Img", Img },
                    { "URL", URL },
                };
                return rt;
            }
        }
    }
}
