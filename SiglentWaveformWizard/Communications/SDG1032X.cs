﻿//Ignore Spelling: Siglent, SDG, IDN

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
        }

        public string? IDN() => device.IDN();
        public string? WaitUntilOperationComplete() => device.WaitUntilOperationComplete();

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

            public string? OutputState
            {
                get
                {
                    return Query("OUTPut?");
                }
            }
        }
    }
}
