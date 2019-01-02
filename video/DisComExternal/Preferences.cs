using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using helpers;
using helpers.extensions;

namespace BTL.Merging
{
    public class Preferences : helpers.Preferences
    {
        static private Preferences _cInstance = new Preferences();
        static public MergingMethod stMergingMethod
        {
            get
            {
                return _cInstance._stMergingMethod;
            }
        }
        static public void Reload()
        {
            _cInstance = new Preferences();
        }

        private MergingMethod _stMergingMethod;

        public Preferences()
            : base("//discomext")
        {
        }
        override protected void LoadXML(XmlNode cXmlNode)
        {
            if (null == cXmlNode)  // || _bInitialized
                return;

            int nMergingId = cXmlNode.AttributeOrDefaultGet<int>("merging_id", 0);
            _stMergingMethod = new MergingMethod(MergingDevice.DisComExternal, nMergingId);
        }
    }
}
