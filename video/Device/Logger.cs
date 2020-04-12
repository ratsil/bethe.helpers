using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTL.Device
{
	class Logger : helpers.Logger
	{
        new public static string sPreferencesFile
        {
            set
            {
                helpers.Logger.sPreferencesFile = value;
            }
        }
        public Logger(string sCategory, string sFile)
            : base(sCategory, sFile == null ? "Device" : sFile)   // "device_main"  , "device[" + System.Diagnostics.Process.GetCurrentProcess().Id + "]"
        {
        }
	}
}
