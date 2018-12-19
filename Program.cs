using System;
using System.Collections.Generic;
using Tinkerforge;

namespace Plotter
{
    class Program
    {
        public static string SpeedDeviceUid { get; } = "w5b";
        public static string StepperDeviceUid { get; } = "6KuTYm";
        public static int Port { get; } = 4223;
        public static string Hostname { get; } = "localhost";

        static void Main(string[] args)
        {
            var connection = new IPConnection();
            var speedDevice = new BrickletRotaryEncoder(SpeedDeviceUid, connection);
            speedDevice.CountCallback += (sender, count) => Console.WriteLine($"Ticks {count}");

            var stepperController = new BrickStepper(StepperDeviceUid, connection);
            
            connection.Connect(Hostname, Port);

            speedDevice.SetCountCallbackPeriod(50);

            connection.EnumerateCallback +=
                (sender, uid, connectedUid, position, version, firmwareVersion, identifier, type) =>
                {
                    Console.WriteLine($"Device: {type} with {uid} using {connectedUid} as brick");
                };
                
            connection.Enumerate();
            
            Console.ReadLine();
            Console.WriteLine($"Ticks {speedDevice.GetCount(false)}");
            connection.Disconnect();
        }
    }
}