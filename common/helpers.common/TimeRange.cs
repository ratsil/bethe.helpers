using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Xml.Serialization;
using helpers.extensions;

namespace helpers
{
	public class TimeRange
	{
		public long nTicksIn;
		public long nTicksOut;
		[XmlIgnore]
		public TimeSpan tsIn
		{
			get
			{
				if (long.MaxValue == nTicksIn)
					return TimeSpan.MaxValue;
				return new TimeSpan(nTicksIn);
			}
			set
			{
				if (TimeSpan.MaxValue == value)
					nTicksIn = long.MaxValue;
				else
					nTicksIn = value.Ticks;
			}
		}
		[XmlIgnore]
		public TimeSpan tsOut
		{
			get
			{
				if (long.MaxValue == nTicksOut)
					return TimeSpan.MaxValue;
				return new TimeSpan(nTicksOut);
			}
			set
			{
				if (TimeSpan.MaxValue == value)
					nTicksOut = long.MaxValue;
				else
					nTicksOut = value.Ticks;
			}
		}
		public DateTime dtIn
		{
			get
			{
				return new DateTime(tsIn.Ticks);
			}
			set
			{
				tsIn = value.TimeOfDay;
			}
		}
		public DateTime dtOut
		{
			get
			{
				return new DateTime(tsOut.Ticks);
			}
			set
			{
				if (DateTime.MaxValue == value)
					tsOut = TimeSpan.MaxValue;
				else
					tsOut = value.TimeOfDay;
			}
		}
		public long nFrameIn
		{
			get
			{
				return (long)tsIn.TotalSeconds * 25; //UNDONE FPS
			}
			set
			{
				tsIn = new TimeSpan(0, 0, 0, 0, (int)(value * 40));
			}
		}
		public long nFrameOut
		{
			get
			{
				return (long)tsOut.TotalSeconds * 25; //UNDONE FPS
			}
			set
			{
				if (int.MaxValue == value)
					tsOut = TimeSpan.MaxValue;
				else
					tsOut = new TimeSpan(0, 0, 0, 0, (int)(value * 40));
			}
		}

		public TimeRange()
		{
			nTicksIn = long.MaxValue;
			nTicksOut = long.MaxValue;
		}
		public TimeRange(long nFrameIn, long nFrameOut)
			: this()
		{
			this.nFrameIn = nFrameIn;
			this.nFrameOut = nFrameOut;
		}
		public TimeRange(Hashtable aValues)
		{
			nTicksIn = long.MaxValue;
			nTicksOut = long.MaxValue;
			if (2 == aValues.Count)
			{
				long[] aFrames = new long[2];
				aValues.Values.CopyTo(aFrames, 0);
				if (aFrames[0] > aFrames[1])
				{
					nFrameIn = aFrames[1];
					nFrameOut = aFrames[0];
				}
				else
				{
					nFrameIn = aFrames[0];
					nFrameOut = aFrames[1];
				}
			}
		}
	}
	public class WeeklyRange
	{
		public int sWeekdayIn; //день недели 0=MON;
		public DateTime dtIn; //время этого дня 
		public int sWeekdayOut;
		public DateTime dtOut;
		public WeeklyRange(string sWDIn, string sTimeIn, string sWDOut, string sTimeOut)
		{
			sWeekdayIn = StringToWeekday(sWDIn);
			dtIn = DateTime.Parse(sTimeIn);
			sWeekdayOut = StringToWeekday(sWDOut);
			dtOut = DateTime.Parse(sTimeOut);
		}
		public int StringToWeekday(string sWD)
		{
			switch (sWD.ToLower())
			{
				case "mon": return 1;
				case "tue": return 2;
				case "wed": return 3;
				case "thu": return 4;
				case "fri": return 5;
				case "sat": return 6;
				case "sun": return 7;
				default: return 0;
			}
		}
		public bool IsDateInRange(DateTime dtDate)
		{
			bool RetVal = false;
			int WeekDayOfDate = dtDate.DayOfWeek == 0 ? 7 : (int)dtDate.DayOfWeek;
			DateTime dtAbsIn = (dtDate.AddDays(sWeekdayIn - WeekDayOfDate)).Date + dtIn.TimeOfDay;
			DateTime dtAbsOut = (dtDate.AddDays(sWeekdayOut - WeekDayOfDate)).Date + dtOut.TimeOfDay;
			if (dtDate >= dtAbsIn && dtDate <= dtAbsOut)
				RetVal = true;
			return RetVal;
		}
	}
}
