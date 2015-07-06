using System;
using System.Diagnostics;

namespace ffmpeg.net
{
    public class Logger : helpers.Logger
    {
        static public Level eLevel = Level.debug2;
		
        public Logger()
            : base(eLevel, "ffmpeg")
        { }
		public class Timings : helpers.Logger.Timings
		{
			public Timings(string sCategory)
				: base(sCategory, eLevel)
			{ }
		}
    }
}
