using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;

namespace DisableWakeupDevices
{
    static class Program
    {
        public static List<string> Devices { get; } = GetDevices().ToList();

        static void Main(string[] args)
        {
            Console.WriteAscii("Devices", Color.CornflowerBlue);
            Console.WriteLine("Management v1.0\n", Color.YellowGreen);

            for (var index = 0; index < Devices.Count; index++)
            {
                var device = Devices[index];
                Console.WriteLine($"[{index}] {device}");
            }

            Console.Write("\n> Type in the devices you want to disable, separated by a comma: ");
            var disabledevices = Console.ReadLine().Split(',').ToDeviceName().ToList();

            Console.WriteLine("> Are you sure that you want to disable the following devices ?\n");
            foreach (var disabledevice in disabledevices)
            {
                Console.WriteLine($": {disabledevice}", Color.Crimson);
            }

            while (true)
            {
                Console.Write("\n> [(y)es/(n)o]: ");
                var response = Console.ReadLine().Trim().ToLower();
                if (response == "n")
                {
                    return;
                }
                if (response == "y")
                {
                    break;
                }
                Console.WriteLine("Sorry, please type in only the characters Y for yes or N for no.");
            }
            DisableDevices(disabledevices);
            Console.WriteLine("Device(s) disabled sucessfully. Press any key to close this window.");
            Console.ReadKey();
        }

        private static IEnumerable<string> ToDeviceName(this IEnumerable<string> list) => list.Select(item => Devices[Convert.ToInt32(item)]);

        private static IEnumerable<string> GetDevices()
        {
            var proc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "powercfg",
                    Arguments = "/devicequery wake_armed"
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();

            return output.Trim().Split('\n').ToList();
        }

        private static void DisableDevices(IEnumerable<string> devices)
        {
            foreach (var device in devices)
                DisableDevice(device);
        }

        private static void DisableDevice(string device)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    FileName = "powercfg",
                    Arguments = $"/devicedisablewake \"{device}\""
                }
            };
            proc.Start();
        }

        private static void EnableDevice(string device)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "powercfg",
                    Arguments = $"/deviceenablewake \"{device}\""
                }
            };
            proc.Start();
        }
    }
}