using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Foundation.OpenCL.Tests
{
    [Category("Hardware")]
    public class BasicTests
    {
        [Test]
        public void DebugPlatforms()
        {
            var platforms = Platform.GetPlatforms();
            Console.WriteLine($"Found {platforms.Length} OpenCL platform(s):");

            foreach (var platform in platforms)
            {
                var name = platform.GetStringInfo(PlatformInfo.Name);
                var vendor = platform.GetStringInfo(PlatformInfo.Vendor);
                var version = platform.GetStringInfo(PlatformInfo.Version);

                Console.WriteLine($"  Platform: {name}");
                Console.WriteLine($"  Vendor: {vendor}");
                Console.WriteLine($"  Version: {version}");

                // Check what devices each platform offers
                var devices = platform.GetDevices(DeviceType.All);
                Console.WriteLine($"  Devices: {devices.Length}");
                foreach (var device in devices)
                {
                    var deviceName = device.GetStringInfo(DeviceInfo.Name);
                    var deviceType = device.GetInfo<DeviceType>(DeviceInfo.Type);
                    Console.WriteLine($"    - {deviceName} ({deviceType})");
                }
                Console.WriteLine();
            }
        }

        [Test]
        public void TestClBinding()
        {
            var platforms = Platform.GetPlatforms();
            Console.WriteLine($"{platforms.Length} platforms found.");

            for (var i = 0; i < platforms.Length; i++)
            {
                Console.WriteLine($"\n--- Platform {i} ---");

                var platform = platforms[i];

                var platformName = platform.GetStringInfo(PlatformInfo.Name);
                Console.WriteLine($"Platform: {platformName}");

                var devices = platform.GetDevices();
                Console.WriteLine($"Found {devices.Length} device(s)");

                for (var j = 0; j < devices.Length; j++)
                {
                    Console.WriteLine($"\n  Device {j}: {devices[j]:X8}");
                    var device = devices[j];

                    var deviceName = device.GetStringInfo(DeviceInfo.Name);
                    Console.WriteLine($"DeviceName: {deviceName}");


                    var deviceInfoFields = typeof(DeviceInfo)
                        .GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Where(f => f.FieldType == typeof(DeviceInfo)).ToList();

                    foreach (var field in deviceInfoFields)
                    {
                        try
                        {
                            PrintDeviceInfo(device, field);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private unsafe void PrintDeviceInfo(Device device, FieldInfo info)
        {
            var paramName = (DeviceInfo)info.GetValue(null);

            var paramSize = device.GetInfoByteSize(paramName);

            if (paramSize == 4)
            {
                Console.WriteLine($"    {info.Name}: {device.GetInfo<int>(paramName)}");
            }
            else if (paramSize == 8)
            {
                Console.WriteLine($"    {info.Name}: {device.GetInfo<ulong>(paramName)}");
            }
            else if (paramSize == 2)
            {
                Console.WriteLine($"    {info.Name}: {device.GetInfo<ushort>(paramName)}");
            }
            else if (paramSize == 1)
            {
                Console.WriteLine($"    {info.Name}: 0x{device.GetInfo<byte>(paramName):X2}");
            }
            else
            {
                Span<byte> buffer = stackalloc byte[paramSize];
                device.TryGetInfo(paramName, buffer);
                Console.WriteLine($"    {info.Name}: {GetString(buffer)}");
            }
        }

        private static string GetString(ReadOnlySpan<byte> buffer)
        {
            if (IsAsciiCString(buffer))
            {
                return Encoding.ASCII.GetString(buffer[..^1]);
            }
            else
            {
                return string.Join(" ", buffer.ToArray().Select(b => $"{b:X2}"));
            }

        }

        private static bool IsAsciiCString(ReadOnlySpan<byte> buffer)
        {
            if (buffer[^1] != 0) return false;
            for (var i = 0; i < buffer.Length - 1; i++)
            {
                if (buffer[i] == 0) return false;
            }

            return true;
        }
    }
}