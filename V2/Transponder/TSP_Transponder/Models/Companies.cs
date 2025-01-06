using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models
{
    public class Companies
    {
        public static List<Company> List = new List<Company>()
        {
            new Company()
            {
                Number = 0,
                Code = "clearsky",
                Name = "ClearSky Logistics",
                BankAccount = "clearsky_checking",
                Tag = new string[]
                {
                    "CLEARSKY",
                    "CLEAR SKY",
                },
                Bonification = new Dictionary<float, float>()
                {
                    { 0.25f, 1.5f },
                    { 0.75f, 1.2f },
                    { 1f, 1.1f },
                    { 2f, 1.05f },
                }
            },
            new Company()
            {
                Number = 2,
                Code = "coyote",
                Name = "Transportes Coyote",
                BankAccount = "coyote_checking",
                Tag = new string[]
                {
                    "COYOT",
                    "COYOTE",
                    "COYOTTE",
                    "COYOTT",
                }
            },
            new Company()
            {
                Number = 3,
                Code = "skyparktravel",
                Name = "Skypark Travel",
                BankAccount = "skypark_checking",
                Tag = new string[]
                {
                    "SKYPARK",
                    "SKYPARK TRAVEL",
                    "SKYPARK TRAVELS",
                    "SKY PARK",
                    "SKY PARK TRAVEL",
                    "SKY PARK TRAVELS",
                }
            },
            new Company()
            {
                Number = 4,
                Code = "bobsaeroservice",
                Name = "Bob's Aero Service",
            },
            new Company()
            {
                Number = 5,
                Code = "oceanicair",
                Name = "Oceanic Air",
            },
        };

        public static Company Get(string Code)
        {
            Company Found = List.Find(x => x.Code == Code);
            if(Found != null)
            {
                return Found;
            }
            else
            {
                return new Company()
                {
                    Code = "unknown",
                    Name = "Unknown source",
                };
            }
        }
    }

    public class Company
    {
        public int Number = -1;
        public string Name = "";
        public string Code = "";
        public string BankAccount = null;
        public string[] Tag = new string[0];
        public Dictionary<float, float> Bonification = null;
    }
}
