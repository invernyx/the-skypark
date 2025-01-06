using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSP_Installer
{
    public class LicensingManager
    {
        public enum LicenseApplication
        {
            None,
            Skypark,
        }

        public class ApplicationMetadata
        {
            public Guid ID { get; set; }
            public byte[] Key { get; set; }
            public LicenseApplication App { get; set; }
        }

        private static List<ApplicationMetadata> GetApps()
        {
            return new List<ApplicationMetadata>
            {
                // The Skypark
                new ApplicationMetadata {
                    App = LicenseApplication.Skypark,
                    ID = Guid.Parse("cdebbb64-e9bd-4e75-80ff-ff1b0cea3a92"),
                    Key = new byte[] {
                        0x2D, 0x2F, 0x31, 0x33, 0x35, 0x4C, 0x51, 0x55,
                        0x59, 0x60, 0x34, 0x66, 0x6D, 0x5C, 0x68, 0x67,
                        0x63, 0x42, 0x6F, 0x6B, 0x81, 0x57, 0x59, 0x5B,
                        0x5D, 0x5F, 0x3E, 0x83, 0x7E, 0x93, 0xB3, 0x83,
                        0x81, 0x9B, 0x8C, 0x91, 0xB7, 0xA4, 0x95, 0xC8,
                        0xBA, 0x82, 0x97, 0x97, 0xA9, 0xB3, 0xA2, 0xA9,
                        0x94, 0xA7, 0xA9, 0xA7, 0xA9, 0xD9, 0xB0, 0xBF,
                        0xD7, 0xB3, 0xB9, 0xDC, 0xE3, 0xA5, 0xED, 0xCE,
                        0xF4, 0xF1, 0xCD, 0xB7, 0xFB, 0xD3, 0xF2, 0xE0,
                        0xF9, 0xE3, 0xEA, 0xE4, 0xE0, 0x11, 0x10, 0xD1,
                        0x7, 0xF6, 0x8, 0xEF, 0xB, 0xF2, 0x12, 0xE3,
                        0x24, 0x5, 0x1B, 0xC0, 0x2C, 0xF, 0xF, 0x21,
                        0x19, 0x2E, 0x2D, 0x14, 0x39, 0x13, 0x36, 0x1F,
                        0x39, 0x49, 0x8, 0x3D, 0x20, 0xC, 0x53, 0x20,
                        0x2B, 0x55, 0x3E, 0x5E, 0x56, 0x54, 0x44, 0x31,
                        0x58, 0x28, 0x48, 0x59, 0x46, 0x3D, 0x51, 0x4A,
                        0x67, 0x7C, 0x7D, 0x71, 0x6B, 0x55, 0x76, 0x5F,
                        0x53, 0x86, 0x7C, 0x6A, 0x92, 0x93, 0x4C, 0x4F,
                        0x98, 0x89, 0x61, 0x63, 0x32, 0x57, 0x59, 0x5B,
                        0x5D, 0x5F, 0x79, 0x84, 0x7C, 0x5A, 0x8C, 0x93,
                        0x82, 0x8E, 0x8D, 0x89, 0x68, 0x95, 0x91, 0xA7,
                        0x7D, 0x7F, 0x81, 0x83, 0x85, 0x64
                    }
                },
            };

        }

        /// <summary>
        /// Gets a list of products with valid license keys.
        /// </summary>
        /// <returns></returns>
        public static List<LicenseApplication> GetValidApps()
        {
            return GetApps().Where(q => {
                try
                {
                    if(q.Key != null)
                    {
                        return new LicensingInstance(q.ID, q.Key).IsValid();
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }).Select(q => q.App).ToList();
        }

        /// <summary>
        /// Refreshes all the license keys.
        /// </summary>
        public static void Refresh()
        {
            foreach (var app in GetApps())
            {
                Task.Run(async () => await new LicensingInstance(app.ID, app.Key).Refresh());
            }
        }
    }
}
