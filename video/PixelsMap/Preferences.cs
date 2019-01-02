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
            static public int nDisComThreadsQty
            {
                get
                {
                    return _cInstance._nDisComThreadsQty;
                }
            }

            private byte _nCUDAVersion;
            private int _nDisComThreadsQty;

            public Preferences()
				: base("//helpers/pixelsmap")
			{
			}
			override protected void LoadXML(XmlNode cXmlNode)
			{
                if (null == cXmlNode || _bInitialized)
					return;
                _nCUDAVersion = 0;
                if (null != cXmlNode.NodeGet("cuda", false))
                    _nCUDAVersion = cXmlNode.NodeGet("cuda").AttributeGet<byte>("version");

                _nDisComThreadsQty = -1;
                if (null != cXmlNode.NodeGet("discom", false))
                {
                    string sQty = cXmlNode.NodeGet("discom").AttributeOrDefaultGet<string>("threads", "auto");
                    _nDisComThreadsQty = sQty == "auto" ? -1 : int.Parse(sQty);
                }
            }
		}
	}
}
