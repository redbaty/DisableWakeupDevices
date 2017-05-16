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
        public static List<string> ArmedDevices { get; } = GetArmedDevices().ToList();
        public static List<string> Devices { get; } = GetDevices().ToList();

        static void Main(string[] args)
        {
            Console.WriteAscii("Devices", Color.CornflowerBlue);
            Console.WriteLine("Management v1.0\n", Color.YellowGreen);

            var response = AskQuestion("Do you wish to (e)nable or (d)isabled devices ?", new List<string> {"e", "d"});

            if (response == "e")
                EnableDevices();
            else
                DisableDevices();

            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }

        static string AskQuestion(string question, List<string> possibleinputs)
        {
            while (true)
            {
                Console.Write("> " + question + " ");
                var response = Console.ReadLine().Trim().ToLower();
                if (possibleinputs.Any(p => p == response))
                {
                    return response;
                }
                Console.WriteLine("Invalid input.");
            }
        }

        private static void EnableDevices()
        {
            Console.WriteLine("Devices that can wake up your computer: ");
            for (var index = 0; index < Devices.Count; index++)
            {
                var device = Devices[index];
                Console.WriteLine($"[{index}] {device}");
            }

            Console.Write("\n> Type in the devices you want to enable, separated by a comma: ");
            var enabledevices = Console.ReadLine().Split(',').ToDeviceName().ToList();

            Console.WriteLine("> Are you sure that you want to enable the following devices ?\n");
            foreach (var device in enabledevices)
            {
                Console.WriteLine($": {device}", Color.Gold);
            }

            WaitForYes();
            EnableDevice(enabledevices);
        }

        private static void WaitForYes()
        {
            while (true)
            {
                Console.Write("\n> [(y)es/(n)o]: ");
                var response = Console.ReadLine().Trim().ToLower();
                if (response == "n")
                {
                    Environment.Exit(0);
                }
                if (response == "y")
                {
                    break;
                }
                Console.WriteLine("Sorry, please type in only the characters Y for yes or N for no.");
            }
        }

        private static void DisableDevices()
        {
            Console.WriteLine("Devices that can wake up your computer: ");
            for (var index = 0; index < ArmedDevices.Count; index++)
            {
                var device = ArmedDevices[index];
                Console.WriteLine($"[{index}] {device}");
            }

            Console.Write("\n> Type in the devices you want to disable, separated by a comma: ");
            var disabledevices = Console.ReadLine().Split(',').ToArmedDeviceName().ToList();

            Console.WriteLine("> Are you sure that you want to disable the following devices ?\n");
            foreach (var disabledevice in disabledevices)
            {
                Console.WriteLine($": {disabledevice}", Color.Crimson);
            }

            WaitForYes();
            DisableDevice(disabledevices);
        }

        private static IEnumerable<string> ToArmedDeviceName(this IEnumerable<string> list) => list.Select(
            item => ArmedDevices[Convert.ToInt32(item)]);

        private static IEnumerable<string> ToDeviceName(this IEnumerable<string> list) => list.Select(
            item => Devices[Convert.ToInt32(item)]);

        private static IEnumerable<string> GetDevices()
        {
            var proc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "powercfg",
                    Arguments = "/devicequery wake_from_any"
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();

            return output.Trim().Split('\n').ToList();
        }

        private static IEnumerable<string> GetArmedDevices()
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

        private static void DisableDevice(IEnumerable<string> devices)
        {
            foreach (var device in devices)
                DisableDevice(device.Trim());
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

        private static void EnableDevice(IEnumerable<string> devices)
        {
            foreach (var device in devices)
                EnableDevice(device.Trim());
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