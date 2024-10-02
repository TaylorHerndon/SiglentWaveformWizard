//Ignore Spelling: Siglent, SDG, IDN OPC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Windows.Interop;
using System.Printing.IndexedProperties;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using SiglentWaveformWizard.Resources;
using SiglentWaveformWizard.Communications.Supporting;

namespace SiglentWaveformWizard.Communications
{
    class SDG1032X : IDisposable
    {
        private ScpiDevice device;
        public string Manufacturer { get; private set; }
        public string ModelNumber { get; private set; }
        public string SerialNumber { get; private set; }
        public List<OutputChannel> Channels { get; private set; }

        public SDG1032X(string ip, int port)
        {
            device = new ScpiDevice(ip, port);
            
            string? idn = device.IDN(); //Siglent Technologies,SDG1032X,SDG1XCAD3R4770,1.01.01.33R1B5\n
            if (idn == null) { throw new Exception("No response from '*IDN?'."); }

            string[] idParts = idn.Split(',');
            if (idParts.Length < 3) { throw new Exception("Insufficient ID information."); }
            
            Manufacturer = idParts[0];
            ModelNumber = idParts[1];
            SerialNumber = idParts[2];
            if (ModelNumber != "SDG1032X") { throw new Exception("Invalid device model number."); }

            Channels = new List<OutputChannel>() { 
                new OutputChannel("C1", device),
                new OutputChannel("C2", device)
            };

            device.Reset();
        }

        public string? IDN() => device.IDN();
        public string? OPC() => device.OPC();
        public void Reset()
        {
            device.Reset();
            device.OPC();
            Channels[1].IsEnabled = false;
            Channels[0].IsEnabled = false;
        }

        public void Close() => device?.Dispose();
        public void Dispose() => device?.Dispose();

        public class OutputChannel
        {
            public string Name { get; private set; }
            private ScpiDevice Device;
            public OutputChannel(string channelName, ScpiDevice device) 
            { 
                this.Name = channelName; 
                this.Device = device;
            }

            /// <summary>
            /// Wrapper function for ScpiDevice Query.
            /// Will add channel as root of query message if it is not there already.
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            private string? Query(string message)
            {
                if (message.StartsWith($"{Name}:")) { message = $"{Name}:{message}"; }
                return Device.Query(message);
            }

            public OutputState OutputState
            {
                get
                {
                    string? resp = Device.Query("OUTPut?");
                    return resp == null ? throw new Exception("Output status could not be queried.") : new OutputState(resp);
                }
                set
                {
                    Load = value.Load;
                    Inverted = value.Inverted;
                    IsEnabled = value.IsEnabled;
                }
            }

            public bool IsEnabled
            {
                get => OutputState.IsEnabled;
                set 
                { 
                    Device.Write($"{Name}:OUTP {(value ? "ON" : "OFF")}");
                    Device.OPC();
                }
            }

            public bool Inverted
            { 
                get => OutputState.Inverted;
                set 
                {
                    Device.Write($"{Name}:OUTP PLRT,{(value ? "INVT" : "NOR")}");
                    Device.OPC();
                }
            }

            public OutputImpedances Load
            {
                get => OutputState.Load;
                set 
                { 
                    Device.Write($"{Name}:OUTP LOAD,{(value == OutputImpedances.HighZ ? "HZ" : "50")}");
                    Device.OPC();
                }
            }
        }
    }
}
