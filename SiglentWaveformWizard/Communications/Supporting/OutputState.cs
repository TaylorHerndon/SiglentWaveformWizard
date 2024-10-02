// Ignore Spelling: Siglent

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiglentWaveformWizard.Communications.Supporting
{
    public enum OutputImpedances
    { 
        HighZ,
        FiftyOhm
    }

    class OutputState
    {
        public string Channel { get; set; }
        public bool IsEnabled { get; set; }
        public OutputImpedances Load { get; set; }
        public bool Inverted { get; set; }

        public int ChannelIndex => (Channel == "C1") ? 0 : 1;

        public OutputState() 
        {
            Channel = "";
            IsEnabled = false;
            Load = OutputImpedances.HighZ;
            Inverted = false;
        }

        public OutputState(int channelIndex)
        {
            Channel = channelIndex == 0 ? "C1" : "C2";
            IsEnabled = false;
            Load = OutputImpedances.HighZ;
            Inverted = false;
        }

        public OutputState(string statusQuery)
        {
            List<string> fields = statusQuery.Split(',').ToList();
            Channel = fields[0].Split(':')[0];
            IsEnabled = fields[0].Split(' ')[1] == "ON";
            Load = (fields[2] == "HZ") ? OutputImpedances.HighZ : OutputImpedances.FiftyOhm;
            Inverted = fields[4] == "INVT";
        }
    }
}
