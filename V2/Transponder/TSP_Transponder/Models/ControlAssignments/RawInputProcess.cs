using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.ControlAssignments
{
    class RawInputProcess
    {
        [DllImport("User32.dll", SetLastError = true)]
        extern static uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint uiNumDevices, uint cbSize);

        [DllImport("User32.dll", SetLastError = true)]
        extern static uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, ref DeviceInfo pData, ref uint pcbSize);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterRawInputDevices(RawInputDevice[] pRawInputDevice, uint numberDevices, uint size);

        [DllImport("user32.dll", SetLastError = true)]
        internal extern static uint GetRegisteredRawInputDevices(RawInputDevice[] pRawInputDevice, ref uint puiNumDevices, uint cbSize);

        public static void RegisterControllers(IntPtr hwnd)
        {
            uint deviceCount = 0;
            int dwSize = Marshal.SizeOf(typeof(RawInputDeviceList));

            if (GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
            {
                IntPtr pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                for (int i = 0; i < deviceCount; i++)
                {
                    RawInputDeviceList rid = (RawInputDeviceList)Marshal.PtrToStructure(new IntPtr(pRawInputDeviceList.ToInt32() + (dwSize * i)), typeof(RawInputDeviceList));

                    uint size = (uint)Marshal.SizeOf(typeof(DeviceInfo));
                    var di = new DeviceInfo { Size = Marshal.SizeOf(typeof(DeviceInfo)) };

                    GetRawInputDeviceInfo(rid.hDevice, (uint)RawInputDeviceInfo.RIDI_DEVICEINFO, ref di, ref size);

                    if (di.Type == DeviceType.RimTypeHid && di.HIDInfo.Usage == (ushort)HidUsage.Gamepad && di.HIDInfo.UsagePage == (ushort)HidUsagePage.GENERIC)
                    {
                        var device = new RawInputDevice();
                        Console.WriteLine("Registering Device");

                        device.UsagePage = di.HIDInfo.UsagePage;
                        device.Usage = (ushort)HidUsage.Keyboard;
                        device.Flags = RawInputDeviceFlags.INPUTSINK;
                        device.Target = hwnd;

                        RawInputDevice[] devices = new RawInputDevice[1];
                        devices[0] = device;

                        if (RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf(typeof(RawInputDevice))) == false)
                        {
                            Console.WriteLine("Failure");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Success!");
                        }
                        break;
                    }
                }

                Marshal.FreeHGlobal(pRawInputDeviceList);
            }
            else
            {
                Console.WriteLine(Marshal.GetLastWin32Error());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RawInputDevice
        {
            internal ushort UsagePage;
            internal ushort Usage;
            internal RawInputDeviceFlags Flags;
            internal IntPtr Target;

            public override string ToString()
            {
                return string.Format("{0}/{1}, flags: {2}, target: {3}", UsagePage, Usage, Flags, Target);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RawInputDeviceList
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DeviceInfo
        {
            [FieldOffset(0)]
            public int Size;
            [FieldOffset(4)]
            public int Type;

            [FieldOffset(8)]
            public DeviceInfoMouse MouseInfo;
            [FieldOffset(8)]
            public DeviceInfoKeyboard KeyboardInfo;
            [FieldOffset(8)]
            public DeviceInfoHid HIDInfo;

            public override string ToString()
            {
                return string.Format("DeviceInfo\n Size: {0}\n Type: {1}\n", Size, Type);
            }
        }
    }
}
