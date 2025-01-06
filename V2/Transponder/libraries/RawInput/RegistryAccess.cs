using Microsoft.Win32;
using System;

namespace RawInput_Module
{
    static internal class RegistryAccess
    {
        static internal RegistryKey GetDeviceKey(string device)
        {
            try
            {
                var split = device.Substring(4).Split('#');
                if (split.Length == 3)
                {
                    var classCode = split[0];       // ACPI (Class code)
                    var subClassCode = split[1];    // PNP0303 (SubClass code)
                    var protocolCode = split[2];    // 3&13c0b0c5&0 (Protocol code)
                    return Registry.LocalMachine.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", classCode, subClassCode, protocolCode));

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Get DeviceKey because " + ex.Message + " - " + ex.StackTrace);
                return null;
            }

        }

        static internal string GetClassType(string classGuid)
        {
            var classGuidKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\" + classGuid);

            return classGuidKey != null ? (string)classGuidKey.GetValue("Class") : string.Empty;
        }
    }
}
