//Ignore Spelling: Siglent

using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiglentWaveformWizard.Communications.Supporting
{
    internal class ArbitraryWaveform
    {
        public string waveformName = "USER1";

        string Name { get; set; }
        int Frequency { get; set; }
        float Amplitude { get; set; }
        byte[] DataPoints { get; set; }

        public byte[] Header
        {
            get => Encoding.ASCII.GetBytes($"{Name}:WVDT 0,WVNM,{waveformName},FREQ,{Frequency},AMPL,{Amplitude},OFST,0,PHASE,0,WAVEDATA,");
        }

        public byte[] SetMessage
        {
            get => Header.Concat(DataPoints).ToArray();
        }

        public ArbitraryWaveform(string channelName, int frequencyHz, float amplitudeV, byte[] dataPoints) 
        { 
            Name = channelName;
            Frequency = frequencyHz;
            Amplitude = amplitudeV;
            DataPoints = dataPoints;
        }

        public ArbitraryWaveform(string channelName, int frequencyHz, float amplitudeV, short[] dataPoints)
        {
            Name = channelName;
            Frequency = frequencyHz;
            Amplitude = amplitudeV;

            DataPoints = new byte[dataPoints.Count() * 2];
            Buffer.BlockCopy(dataPoints, 0, DataPoints, 0, DataPoints.Count());
        }

    }
}
