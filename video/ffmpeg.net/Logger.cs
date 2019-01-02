using System;
using System.Diagnostics;

namespace ffmpeg.net
{
    public class Logger : helpers.Logger
    {
        static public Level eLevel = Level.debug3;
		
        public Logger()
            : base(eLevel, "ffmpeg")
        { }
		public class Timings : helpers.Logger.Timings
		{
			public Timings(string sCategory)
				: base(sCategory, eLevel)
			{ }
            public Timings(string sCategory, Level eLevelTimings)
                : base(sCategory, eLevelTimings)
            { }
        }
    }
}
