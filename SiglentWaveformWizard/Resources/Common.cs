//Ignore Spelling: Siglent

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SiglentWaveformWizard.Resources
{
    public static class Common
    {
        public static void InfoPopup(string message) => HandyControl.Controls.Growl.InfoGlobal(message);

        private static List<string> exponents = new List<string>() { "p", "n", "u", "m", "", "k", "M", "G", "T" };
        public static string ToEngineering(this double value)
        {
            int exp = (value == 0) ? 0 : (int)(Math.Floor(Math.Log10(value) / 3.0) * 3.0);
            double newValue = value * Math.Pow(10.0, -exp);
            if (newValue >= 1000.0)
            {
                newValue = newValue / 1000.0;
                exp = exp + 3;
            }

            return string.Format("{0:##0}{1}", newValue, exponents[(exp + 12)/3]);
        }

        public static List<double> standardDivisionTimes = new List<double>()
        { 
            5, 2, 1,
            0.5, 0.2, 0.1,
            0.05, 0.02, 0.01,
            0.005, 0.002, 0.001,
            0.0005, 0.0002, 0.0001,
            0.00005, 0.00002, 0.00001,
        };

        public static List<double> standardDivisionVoltages = new List<double>()
        {
            5, 2, 1,
            0.5, 0.2, 0.1
        };
    }
}
