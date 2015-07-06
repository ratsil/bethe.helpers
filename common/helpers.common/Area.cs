using System;
using System.Collections.Generic;
using System.Text;

namespace helpers
{
	[Serializable]
	public class Dock
	{
		public enum Corner
		{
			upper_left,
			upper_right,
			bottom_left,
			bottom_right,
			center
		}
		[Serializable]
		public class Offset
		{
			public short nLeft { get; set; }
			public short nTop { get; set; }
			public Offset(short nLeft, short nTop)
			{
				this.nLeft = nLeft;
				this.nTop = nTop;
			}
			public Offset()
				: this(0, 0)
			{
			}
		}

		public Corner eCorner { get; set; }
		public Offset cOffset { get; set; }

		public Dock(Corner eCorner, Offset cOffset)
		{
			this.eCorner = eCorner;
			this.cOffset = cOffset;
		}
		public Dock(Offset cOffset)
			: this(Corner.upper_left, cOffset)
		{ }
		public Dock(short nLeft, short nTop)
			: this(Corner.upper_left, new Offset(nLeft, nTop))
		{ }
		public Dock(Corner eCorner)
			: this(eCorner, new Offset())
		{ }
		public Dock(Corner eCorner, short nLeft, short nTop)
			: this(eCorner, new Offset(nLeft, nTop))
		{ }
		public Dock()
			: this(Corner.upper_left, new Offset())
		{ }
	}
	[Serializable]
	public struct Area
	{
		static public Area stEmpty
		{
			get
			{
				return new Area(0, 0, 0, 0);
			}
		}

		public short nTop;
		public short nLeft;
		public ushort nWidth;
		public ushort nHeight;
		public short nRight
		{
			set
			{
				if (nLeft > value)   //Valikoo adds 'nLeft' 
					nWidth = 0;
				else
					nWidth = (ushort)(value - nLeft + 1); //Valikoo adds '+ 1' 
			}
			get
			{
				return (short)(nLeft + nWidth - 1); //Valikoo  adds '- 1' 
			}
		}
		public short nBottom
		{
			set
			{
				if (nTop > value)
					nHeight = 0;
				else
					nHeight = (ushort)(value - nTop + 1); //Valikoo  adds '+ 1' 
			}
			get
			{
				return (short)(nTop + nHeight - 1); //Valikoo  adds '- 1'
			}
		}

		public Area(short nLeft, short nTop, ushort nWidth, ushort nHeight)
		{
			this.nTop = nTop;
			this.nLeft = nLeft;
			this.nWidth = nWidth;
			this.nHeight = nHeight;
		}
		public Area(System.Drawing.Rectangle stRect)
			: this((short)stRect.X, (short)stRect.Y, (ushort)stRect.Width, (ushort)stRect.Height)
		{ }

		#region operators&overrides
		override public int GetHashCode()
		{
			return base.GetHashCode();
		}
		override public bool Equals(object o)
		{
			try
			{
				if (null == o || !(o is Area))
					return false;
				return this == (Area)o;
			}
			catch { }
			return false;
		}
		static public implicit operator System.Drawing.Rectangle(Area stArea)
		{
			return new System.Drawing.Rectangle(stArea.nLeft, stArea.nTop, stArea.nWidth, stArea.nHeight);
		}
		static public bool operator ==(Area stLeft, Area stRight)
		{
			bool bRetVal = false;
			if (stLeft.nLeft == stRight.nLeft && stLeft.nRight == stRight.nRight && stLeft.nTop == stRight.nTop && stLeft.nBottom == stRight.nBottom)
				bRetVal = true;
			return bRetVal;
		}
		static public bool operator !=(Area stLeft, Area stRight)
		{
			return !(stLeft == stRight);
		}
		static public bool operator >(Area stLeft, Area stRight)
		{
			if (stLeft.nLeft < stRight.nLeft && stLeft.nRight > stRight.nRight && stLeft.nTop < stRight.nTop && stLeft.nBottom > stRight.nBottom)
				return true;
			return false;
		}
		static public bool operator >=(Area stLeft, Area stRight)
		{
			return ((stLeft > stRight) || (stRight == stLeft));
		}
		static public bool operator <(Area stLeft, Area stRight)
		{
			return (stRight > stLeft);
		}
		static public bool operator <=(Area stLeft, Area stRight)
		{
			return (stRight >= stLeft);
		}
		#endregion

		public Area CropOnBase(Area stBase)  //Valikoo. Возвращает обрезанную область если она вышла за рамки образца stBase
		{
			Area stRetVal = new Area();
			stRetVal.nLeft = stBase.nLeft > nLeft ? stBase.nLeft : nLeft;
			stRetVal.nTop = stBase.nTop > nTop ? stBase.nTop : nTop;
			stRetVal.nRight = stBase.nRight > nRight ? nRight : stBase.nRight;
			stRetVal.nBottom = stBase.nBottom > nBottom ? nBottom : stBase.nBottom;
			if (0 == stRetVal.nWidth || 0 == stRetVal.nHeight)
			{
				stRetVal.nWidth = 0;
				stRetVal.nHeight = 0;
			}
			return stRetVal;
		}
		public void DockAccept(Dock cDock)
		{
			Area stAreaNew = Dock(this, cDock);
			nLeft = stAreaNew.nLeft;
			nRight = stAreaNew.nRight;
		}
        public Area Dock(Area stBase, Dock cDock)
		{
			Area stRetVal = this;
			if (null != cDock)
			{
				switch (cDock.eCorner)
				{
					case helpers.Dock.Corner.upper_left:
						stRetVal.nLeft = 0;
						stRetVal.nTop = 0;
						break;
					case helpers.Dock.Corner.upper_right:
						stRetVal.nLeft = (short)(stBase.nWidth - nWidth);
						stRetVal.nTop = 0;
						break;
					case helpers.Dock.Corner.bottom_left:
						stRetVal.nLeft = 0;
						stRetVal.nTop = (short)(stBase.nHeight - nHeight);
						break;
					case helpers.Dock.Corner.bottom_right:
						stRetVal.nLeft = (short)(stBase.nWidth - nWidth);
						stRetVal.nTop = (short)(stBase.nHeight - nHeight);
						break;
					case helpers.Dock.Corner.center:
						stRetVal.nLeft = (short)Math.Round((float)stBase.nWidth / 2 - (float)nWidth / 2);
						stRetVal.nTop = (short)Math.Round((float)stBase.nHeight / 2 - (float)nHeight / 2);
						break;
				}
				stRetVal.nLeft += cDock.cOffset.nLeft;
				stRetVal.nTop += cDock.cOffset.nTop;
			}
			return stRetVal;
		}
		public Area Move(short nLeft, short nTop)
		{
			Area stRetVal = this;
			stRetVal.nLeft = nLeft;
			stRetVal.nTop = nTop;
			return stRetVal;
		}
	}
}
