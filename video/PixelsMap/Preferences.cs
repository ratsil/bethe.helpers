using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using helpers;
using helpers.extensions;

namespace helpers
{
	public partial class PixelsMap
	{
		public class Preferences : helpers.Preferences
		{
			static private Preferences _cInstance = new Preferences();

			static public byte nCUDAVersion
			{
				get
				{
					return _cInstance._nCUDAVersion;
				}
			}

			private byte _nCUDAVersion = 0;

			public Preferences()
				: base("//helpers/pixelsmap")
			{
			}
			override protected void LoadXML(XmlNode cXmlNode)
			{
                if (null == cXmlNode || _bInitialized)
					return;
                _nCUDAVersion = cXmlNode.NodeGet("cuda").AttributeGet<byte>("version");
			}
		}
	}
}
