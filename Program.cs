using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tinkerforge;

namespace Plotter
{
    class Program
    {
        public static string NfcDeviceUid { get; } = "H8r";
        public static string SpeedDeviceUid { get; } = "w5b";
        public static string StepperDeviceUid { get; } = "6KuTYm";
        public static string StopSensorUid { get; set; } = "5Jd";
        public static int Port { get; } = 4223;
        public static string Hostname { get; } = "localhost";

        static void Main(string[] args)
        {
            var connection = new IPConnection();
            var speedDevice = new BrickletRotaryEncoder(SpeedDeviceUid, connection);
            var stepperController = new BrickStepper(StepperDeviceUid, connection);
            var ambientLightController = new BrickletAmbientLight(StopSensorUid, connection);
            var nfcController = new BrickletNFC(NfcDeviceUid, connection);
            
            connection.Connect(Hostname, Port);

            speedDevice.CountCallback += (sender, count) => Console.WriteLine($"Ticks {count}");
            speedDevice.SetCountCallbackPeriod(50);

            ambientLightController.IlluminanceReached += (sender, illuminance) => Console.WriteLine("Covered");
            ambientLightController.SetDebouncePeriod(1000);
            ambientLightController.SetIlluminanceCallbackThreshold(BrickletAmbientLight.THRESHOLD_OPTION_SMALLER, 2, 0);
            
            nfcController.SetMode(BrickletNFC.MODE_READER);
            nfcController.ReaderStateChangedCallback += (sender, state, idle) => Console.WriteLine($"state: {state}");
            
            nfcController.ReaderRequestTagID();
            Thread.Sleep(10);
//            WaitWhileInState(nfcController, BrickletNFC.READER_STATE_REQUEST_TAG_ID);
            WaitForState(nfcController, new []{BrickletNFC.READER_STATE_IDLE});
            nfcController.ReaderGetTagID(out var tagType, out var tagId);
           
            Console.WriteLine($"{tagType} {FormatAsHex(tagId)}");

            nfcController.ReaderRequestPage(4, 1);
//            WaitWhileInState(nfcController, 
//                BrickletNFC.READER_STATE_REQUEST_PAGE);

            WaitForState(nfcController, new []{BrickletNFC.READER_STATE_IDLE});

//            nfcController.ReaderGetState(out var pageReadState, out var pageReadIdle);
//            Console.WriteLine(pageReadState);
//            Console.WriteLine($"Page: {FormatAsHex(nfcController.ReaderReadPage())}");
            
            connection.EnumerateCallback +=
                (sender, uid, connectedUid, position, version, firmwareVersion, identifier, type) =>
                {
                    Console.WriteLine($"Device: {type} with {uid} using {connectedUid} as brick");
                };
                
            connection.Enumerate();
            
            Console.ReadLine();
            connection.Disconnect();
        }

        private static string FormatAsHex(byte[] stream)
        {
            return string.Join(" ", stream.Select(b => b.ToString("X2")));
        }

        private static void WaitForState(BrickletNFC nfcController, byte[] expectedStates)
        {
            if (!expectedStates.Any()) return;
            
            byte nfcState = 0;
            var sw = Stopwatch.StartNew();
            do
            {
                Thread.Sleep(1);
                nfcController.ReaderGetState(out nfcState, out bool idle);
            } while (!expectedStates.Any(state => (state & nfcState) > 0));

            sw.Stop();
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}");
        }
        
        private static void WaitWhileInState(BrickletNFC nfcController, byte activeState)
        {
            byte nfcState = 0;
            var sw = Stopwatch.StartNew();
            do
            {
                Thread.Sleep(1);
                nfcController.ReaderGetState(out nfcState, out bool idle);
            } while (activeState == nfcState);

            sw.Stop();
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}");
        }
    }
}