using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using helpers;
using helpers.extensions;

namespace BTL.Device
{
	public class Preferences : helpers.Preferences
	{
        public class XNA
        {
            public int nWidth;
            public int nHeight;
            public bool bFullScreen;
            public bool bPromter;
        }
		static private Preferences _cInstance = new Preferences();
        static public XNA cXNA;
		static public byte nTargetDevice
		{
			get
			{
				return _cInstance._nTargetDevice;
			}
		}
        static public byte nTargetChannel
        {
            get
            {
                return _cInstance._nTargetChannel;
            }
        }
        static public string sDeviceMake
        {
            get
            {
                return _cInstance._sDeviceMake;
            }
        }
        static public bool bDeviceInput
		{
			get
			{
				return _cInstance._bDeviceInput;
			}
		}
		static public ushort nFPS
		{
			get
			{
                if (_cInstance._nFPS == 0)
                    throw new Exception("fps is 0. must be greater.");
                return _cInstance._nFPS;
			}
            set
            {
                _cInstance._nFPS = value; // info from board
            }
		}
		static public int nFrameDuration
		{
			get
			{
                if (_cInstance._nFrameDuration == 0)
                {
                    _cInstance._nFrameDuration = 1000 / nFPS;
                }
                return _cInstance._nFrameDuration;
			}
		}
		static public int nGCFramesInterval
		{
			get
			{
				return _cInstance._nGCFramesInterval;
			}
		}

		static public bool bAudio
		{
			get
			{
				return _cInstance._bAudio;
			}
		}
		static public uint nAudioSamplesRate
		{
			get
			{
				return _cInstance._nAudioSamplesRate;
			}
		}
		static public byte nAudioChannelsQty
		{
			get
			{
				return _cInstance._nAudioChannelsQty;
			}
		}
		static public byte nAudioBitDepth
		{
			get
			{
				return _cInstance._nAudioBitDepth;
			}
		}
		static public byte nAudioByteDepth
		{
			get
			{
				return _cInstance._nAudioByteDepth;
			}
		}
		static public byte nAudioBytesPerSample
		{
			get
			{
				return _cInstance._nAudioBytesPerSample;
			}
		}
		static public uint nAudioSamplesPerFrame
		{
			get
			{
                if (_cInstance._nAudioSamplesPerFrame == 0)
                {
                    _cInstance._nAudioSamplesPerFrame = nAudioSamplesRate / nFPS;
                }
				return _cInstance._nAudioSamplesPerFrame;
			}
		}
		static public uint nAudioBytesPerFrame
		{
			get
			{
                if (_cInstance._nAudioBytesPerFrame == 0)
                {
                    _cInstance._nAudioBytesPerFrame = nAudioBytesPerFramePerChannel * nAudioChannelsQty;
                }
                return _cInstance._nAudioBytesPerFrame;
			}
		}
		static public uint nAudioBytesPerFramePerChannel
		{
			get
			{
                if (_cInstance._nAudioBytesPerFramePerChannel == 0)
                {
                    _cInstance._nAudioBytesPerFramePerChannel = nAudioSamplesPerFrame * nAudioByteDepth;
                }
                return _cInstance._nAudioBytesPerFramePerChannel;
			}
		}
		static public short nAudioVolumeChangeInDB
		{
			get
			{
				return _cInstance._nAudioVolumeChangeInDB;
			}
		}
		static public float nAudioVolumeChange
		{
			get
			{
				return _cInstance._nAudioVolumeChange;
			}
		}

		static public bool bVideo
		{
			get
			{
				return _cInstance._bVideo;
			}
		}
		static public string sVideoFormat
		{
			get
			{
				return _cInstance._sVideoFormat;
			}
		}
		static public bool bAnamorphic   // Maybe it will be useful
        {
			get
			{
				return _cInstance._bAnamorph;
			}
		}   
		static public DownStreamKeyer cDownStreamKeyer
		{
			get
			{
				return _cInstance._cDownStreamKeyer;
			}
			set
			{
				_cInstance._cDownStreamKeyer = value;
			}
		}
        static public string sPixelsFormat
        {
            get
            {
                return _cInstance._sPixelsFormat;
            }
        }
        static public byte nQueueDeviceLength
		{
			get
			{
				return _cInstance._nQueueDeviceLength;
			}
            set
            {
                _cInstance._nQueueDeviceLength = value;
            }
		}
		static public byte nQueuePipeLength
		{
			get
			{
				return _cInstance._nQueuePipeLength;
				//#if SCR || PROMPTER
				//                return 5;
				//#else
				//                return 25;
				//#endif
			}
		}
		static public string sDebugFolder
		{
			get
			{
				return _cInstance._sDebugFolder;
			}
		}

        static public void Reload()
        {
            _cInstance = new Preferences();
        }

		private byte _nTargetDevice;
        private string _sDeviceMake;
        private byte _nTargetChannel;
        private bool _bDeviceInput;
		private ushort _nFPS;
		private int _nFrameDuration;
		private int _nGCFramesInterval;

		private bool _bAudio;
		private uint _nAudioSamplesRate;
		private byte _nAudioChannelsQty;
		private byte _nAudioBitDepth;
		private byte _nAudioByteDepth;
		private byte _nAudioBytesPerSample;
		private uint _nAudioSamplesPerFrame;
		private uint _nAudioBytesPerFrame;
		private uint _nAudioBytesPerFramePerChannel;
		private short _nAudioVolumeChangeInDB;
		private float _nAudioVolumeChange;

		private bool _bVideo;
		private string _sVideoFormat;
        private bool _bAnamorph;
		private DownStreamKeyer _cDownStreamKeyer;
        private string _sPixelsFormat;

        private byte _nQueueDeviceLength;
		private byte _nQueuePipeLength;
		private string _sDebugFolder;

		public Preferences()
			: base("//device")
		{
        }
		override protected void LoadXML(XmlNode cXmlNode)
        {
            if (null == cXmlNode)  // || _bInitialized
                return;
            _sDebugFolder = cXmlNode.AttributeOrDefaultGet<string>("debug_folder", "");
            XmlNode cNodeChild;
            XmlNode cNodeDevice = cXmlNode;
            _nTargetDevice = cNodeDevice.AttributeGet<byte>("target");
            _sDeviceMake = cNodeDevice.AttributeValueGet("make");
            if (_sDeviceMake == "aja")
            {
                _nTargetChannel = cNodeDevice.AttributeGet<byte>("target_channel");
            }
            _bDeviceInput = ("input" == cNodeDevice.AttributeValueGet("type"));

			if (!_bDeviceInput)
			{
                if (_bAudio = (null != (cNodeChild = cNodeDevice.NodeGet("audio", false))))
				{
                    _nAudioSamplesRate = cNodeChild.AttributeGet<uint>("rate");
                    _nAudioChannelsQty = cNodeChild.AttributeGet<byte>("channels");
                    _nAudioBitDepth = cNodeChild.AttributeGet<byte>("bits");
					_nAudioByteDepth = (byte)(_nAudioBitDepth / 8);
					_nAudioBytesPerSample = (byte)(_nAudioByteDepth * _nAudioChannelsQty);
                    _nAudioVolumeChangeInDB = cNodeChild.AttributeOrDefaultGet<short>("volume_change", 0);
                    if (0 != _nAudioVolumeChangeInDB)
                        _nAudioVolumeChange = (float)Math.Pow(10, (float)_nAudioVolumeChangeInDB / 20);
                }
                if (null != (cNodeChild = cNodeDevice.NodeGet("xna", false)))
                {
                    cXNA = new XNA();
                    cXNA.bFullScreen = cNodeChild.AttributeOrDefaultGet<bool>("fullscreen", false);
                    cXNA.bPromter = cNodeChild.AttributeOrDefaultGet<bool>("promter", false);
                    cXNA.nWidth = cNodeChild.AttributeOrDefaultGet<int>("width", 720);
                    cXNA.nHeight = cNodeChild.AttributeOrDefaultGet<int>("height", 576);
                    //_nFPS = cNodeDevice.AttributeGet<ushort>("fps"); //TODO
                }
            }

            if (_bVideo = (null != (cNodeChild = cNodeDevice.NodeGet("video", false))))
			{
                _sVideoFormat = cNodeChild.AttributeValueGet("format").ToLower();
                _sPixelsFormat = cNodeChild.AttributeValueGet("pixels").ToLower();
				_bAnamorph = cNodeChild.AttributeOrDefaultGet<bool>("anamorph", false);
				if (!_bDeviceInput)
				{
					_cDownStreamKeyer = null;
                    if (null != (cNodeChild = cNodeChild.NodeGet("keyer", false)))
					{
						_cDownStreamKeyer = new DownStreamKeyer();
						try
						{
                            _cDownStreamKeyer.nLevel = cNodeChild.AttributeGet<byte>("level");
						}
						catch
						{
							throw new Exception("указан некорректный формат уровня DSK [level][" + cNodeChild.Name + "]"); //TODO LANG
						}
						try
						{
                            _cDownStreamKeyer.bInternal = ("internal" == cNodeChild.AttributeValueGet("type"));
						}
						catch
						{
							throw new Exception("указан некорректный тип DSK [type][" + cNodeChild.Name + "]"); //TODO LANG
						}
					}
				}
			}

            cNodeChild = cNodeDevice.NodeGet("queue");
            _nQueueDeviceLength = cNodeChild.AttributeGet<byte>("device");
            _nQueuePipeLength = cNodeChild.AttributeGet<byte>("pipe");
            _nGCFramesInterval = cNodeChild.AttributeGet<int>("gc_interval");
		}
	}
}
