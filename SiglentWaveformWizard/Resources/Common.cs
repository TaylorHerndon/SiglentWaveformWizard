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
    }
}
