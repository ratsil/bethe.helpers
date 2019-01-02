using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using helpers.extensions;

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
			center,
			center_horizontal,
			center_vertical,
			unknown,
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
            public Offset(XmlNode cXmlNode)
                :this()
            {
                LoadXML(cXmlNode);
            }
            public void LoadXML(XmlNode cXmlNode)
			{
				string sTMP;
				sTMP = cXmlNode.AttributeValueGet("x", false);
				if (null == sTMP)
					sTMP = cXmlNode.AttributeValueGet("left");
				nLeft = sTMP.ToShort();
				sTMP = cXmlNode.AttributeValueGet("y", false);
				if (null == sTMP)
					sTMP = cXmlNode.AttributeValueGet("top");
				nTop = sTMP.ToShort();
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
		public void LoadXML(XmlNode cXmlNode)
		{
			XmlNode cNodeChild, cNodeOffset=null;
			if (null != (cNodeChild = cXmlNode.NodeGet("dock", false)))
			{
				eCorner = (Dock.Corner)Enum.Parse(typeof(Dock.Corner), System.Text.RegularExpressions.Regex.Replace(cNodeChild.AttributeValueGet("corner").Trim(), "\\W", "_"), true);
				cNodeOffset = cNodeChild.NodeGet("offset", false);
			}
			if (cNodeOffset != null || null!=(cNodeOffset = cXmlNode.NodeGet("offset", false)))
				cOffset.LoadXML(cNodeOffset);
		}
	}
    [Serializable]
	public struct Area
	{
        [Serializable]
        public class Size
        {
            public ushort nWidth;
            public ushort nHeight;
            public Size(XmlNode cXmlNode)
            {
                LoadXML(cXmlNode);
            }
            public void LoadXML(XmlNode cXmlNode)
            {
                string sTMP;
                sTMP = cXmlNode.AttributeValueGet("w", false);
                if (null == sTMP)
                    sTMP = cXmlNode.AttributeValueGet("width");
                nWidth = sTMP.ToUShort();
                sTMP = cXmlNode.AttributeValueGet("h", false);
                if (null == sTMP)
                    sTMP = cXmlNode.AttributeValueGet("height");
                nHeight = sTMP.ToUShort();
            }
        }
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
		public Area LoadXML(XmlNode cXmlNode)
		{
			XmlNode cNodeChild;
			Area stRetVal = new Area(this.nLeft, this.nTop, 0, 0);
			if (null != (cNodeChild = cXmlNode.NodeGet("size", false)))
			{
				stRetVal.nWidth = cNodeChild.AttributeGet<ushort>("width");
				stRetVal.nHeight = cNodeChild.AttributeGet<ushort>("height");
			}
			return stRetVal;
		}
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
            if (stLeft.nLeft == stRight.nLeft && stLeft.nWidth == stRight.nWidth && stLeft.nTop == stRight.nTop && stLeft.nHeight == stRight.nHeight)
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
        public Area Dock(Area stBase, Dock cDock)
		{
			if (stBase.nWidth == 0 || stBase.nHeight == 0)
				return this;
			Area stRetVal = this;
			if (cDock == null && (stRetVal.nLeft != 0 || stRetVal.nTop != 0))
				cDock = new helpers.Dock(stRetVal.nLeft, stRetVal.nTop);
			stRetVal.nLeft = 0;
			stRetVal.nTop = 0;
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
						stRetVal.nLeft = (short)Math.Round((float)(stBase.nWidth - nWidth) / 2);
						stRetVal.nTop = (short)Math.Round((float)(stBase.nHeight - nHeight) / 2);
						break;
					case helpers.Dock.Corner.center_horizontal:
						stRetVal.nLeft = (short)Math.Round((float)(stBase.nWidth - nWidth) / 2);
						break;
					case helpers.Dock.Corner.center_vertical:
						stRetVal.nTop = (short)Math.Round((float)(stBase.nHeight - nHeight) / 2);
						break;
					case helpers.Dock.Corner.unknown:
						return this;
				}
				stRetVal.nLeft += cDock.cOffset.nLeft; 
				stRetVal.nTop += cDock.cOffset.nTop; 
			}
			return stRetVal;
		}
		public Area Move(short nLeft, short nTop)
		{
            Area stRetVal = this;  // this при отдаче уже будет новым объектом, т.к. это не ссылочный тип данных
            stRetVal.nLeft += nLeft;
            stRetVal.nTop += nTop;
            return stRetVal;
		}
        public Area Move(Dock.Offset cOffset)  
        {
            return Move(cOffset.nLeft, cOffset.nTop);
        }
        public override string ToString()
        {
            return "[("+nLeft+ ", " + nTop + ") " + nWidth + " x " + nHeight + "]";  // [(23, 0) 1288 x 312]
        }
    }
}
