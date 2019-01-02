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
        public Logger(string sCategory)
			: base(sCategory)   // "device_main"  , "device[" + System.Diagnostics.Process.GetCurrentProcess().Id + "]"
        { }
	}
}
