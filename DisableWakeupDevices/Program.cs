using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;

namespace DisableWakeupDevices;

internal static class Program
{
    private static List<string> ArmedDevices { get; } = GetArmedDevices().ToList();

    private static List<string> Devices { get; } = GetDevices().ToList();

    private static void Main()
    {
        var response = AskQuestion("Do you wish to (e)nable or (d)isable devices ?", new List<string> { "e", "d" });

        if (string.Equals(response, "e", StringComparison.InvariantCultureIgnoreCase))
            EnableDevices();
        else
            DisableDevices();

        Console.WriteLine("Press any key to close this window.");
        Console.ReadKey();
    }

    private static string AskQuestion(string question, IReadOnlyCollection<string> possibleinputs)
    {
        while (true)
        {
            Console.Write("> " + question + " ");
            var response = Console.ReadLine().Trim();
            if (possibleinputs.Any(p => string.Equals(p, response, StringComparison.InvariantCultureIgnoreCase))) return response;
            Console.WriteLine("Invalid input.");
        }
    }

    private static void EnableDevices()
    {
        Console.WriteLine("Devices that can wake up your computer: ");
        
        foreach (var device in Devices.OrderBy(i => i))
        {
            var index = Devices.IndexOf(device);
            Console.WriteLine($"[{index:00}] {device}");
        }
        
        while (true)
        {
            Console.Write("\n> Type in the devices you want to enable, separated by a comma or type 'a' to enable all of them:: ");
            if (!Console.ReadLine().Split(',').ToDeviceName(out var enabledevices)) continue;
            
            Console.WriteLine("> Are you sure that you want to enable the following devices ?\n");
            foreach (var device in enabledevices) Console.WriteLine(device, Color.Gold);

            YesOrExit();
            EnableDevice(enabledevices);
            break;
        }
    }

    private static void YesOrExit()
    {
        while (true)
        {
            Console.Write("\n> [(y)es/(n)o]: ");
            var response = Console.ReadLine().Trim();
            if (string.Equals(response, "n", StringComparison.InvariantCultureIgnoreCase)) Environment.Exit(0);
            if (string.Equals(response, "y", StringComparison.InvariantCultureIgnoreCase)) break;
            Console.WriteLine("Sorry, please type in only the characters Y for yes or N for no.");
        }
    }

    private static void DisableDevices()
    {
        if (ArmedDevices.Count < 1)
        {
            Console.WriteLine("No devices are currently able to wake up your computer", Color.Gold);
            return;
        }
        
        Console.WriteLine("Devices that can wake up your computer: ");
        
        foreach (var device in ArmedDevices.OrderBy(i => i))
        {
            var index = ArmedDevices.IndexOf(device);
            Console.WriteLine($"[{index:00}] {device}");
        }
        
        while (true)
        {
            Console.Write("\n> Type in the devices you want to disable, separated by a comma or type 'a' to disable all of them: ");
            if (!Console.ReadLine().Split(',').ToArmedDeviceName(out var disabledevices)) continue;
            
            Console.WriteLine("> Are you sure that you want to disable the following devices ?\n");
            foreach (var disabledevice in disabledevices) Console.WriteLine($": {disabledevice}", Color.Crimson);

            YesOrExit();
            DisableDevice(disabledevices);
            break;
        }
    }

    private static bool ToArmedDeviceName(this ICollection<string> list, out List<string> deviceNames) => ParseDeviceRange(list, ArmedDevices, out deviceNames);
    
    private static bool ParseDeviceRange(ICollection<string> list, ICollection<string> indexes, out List<string> deviceNames)
    {
        deviceNames = new List<string>();

        if (list.Count == 1 && string.Equals(list.ElementAt(0), "a", StringComparison.InvariantCultureIgnoreCase))
        {
            deviceNames = indexes.ToList();
            return true;
        }
        
        foreach (var inputInt in list)
        {
            if (int.TryParse(inputInt, out var index))
            {
                if (indexes.ElementAtOrDefault(index) is { } deviceName)
                {
                    deviceNames.Add(deviceName);
                    continue;
                }

                Console.WriteLine("Argument out of range", Color.Red);
                return false;
            }
            
            Console.WriteLine("Argument is not an integer", Color.Red);
            return false;
        }

        return true;
    }

    private static bool ToDeviceName(this ICollection<string> list, out List<string> deviceNames) => ParseDeviceRange(list, Devices, out deviceNames);

    private static string[] GetDevices() => QueryPowerCfg("/devicequery wake_from_any");

    private static string[] QueryPowerCfg(string arguments)
    {
        using var proc = CreatePowerCfgProcess(arguments);
        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();

        return output.Trim().Split('\n');
    }

    private static Process CreatePowerCfgProcess(string arguments)
    {
        return new Process
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "powercfg",
                Arguments = arguments
            }
        };
    }

    private static string[] GetArmedDevices()
    {
        var queryPowerCfg = QueryPowerCfg("/devicequery wake_armed");
        return queryPowerCfg.Length == 1 && queryPowerCfg[0] == "NONE" ? Array.Empty<string>() : queryPowerCfg;
    }

    private static void DisableDevice(IEnumerable<string> devices)
    {
        foreach (var device in devices)
            DisableDevice(device.Trim());
    }

    private static void DisableDevice(string device)
    {
        using var proc = CreatePowerCfgProcess($"/devicedisablewake \"{device}\"");
        proc.Start();
    }

    private static void EnableDevice(IEnumerable<string> devices)
    {
        foreach (var device in devices)
            EnableDevice(device.Trim());
    }

    private static void EnableDevice(string device)
    {
        using var proc = CreatePowerCfgProcess($"/deviceenablewake \"{device}\"");
        proc.Start();
    }
}