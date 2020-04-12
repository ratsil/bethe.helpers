using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using DeckLinkAPI;
using helpers;
using System.Linq;

using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// отладка
using System.Drawing;
using System.Drawing.Imaging;           // отладка
// отладка

namespace BTL.Device
{
    abstract public class Device : IDevice
    {
        public class PointerToBytes
        {
            public string sString;
            public GCHandle cHandle;
            public IntPtr pPtr;
            public PointerToBytes(byte[] aBytes)
            {
                cHandle = GCHandle.Alloc(aBytes, GCHandleType.Pinned);
                pPtr = cHandle.AddrOfPinnedObject();
            }
        }

        static public BytesInMemory _cBinM;

        //private static Dictionary<int, Queue<byte[]>> _ahBytesStorage; //DNF не забыть если удачно, то чистить его периодически!!!!
        //private static Dictionary<int, bool> _ahHashPassedOut;
        //private static Dictionary<int, PointerToBytes> _ahPointers;
        //private static Dictionary<int, ushort> _ahNumTotal;
        //private static List<int> _aBytesHashes;
        //private static List<int> _aBytesEmptyHashes;
        //private static int nHash;
        //private static long nBytesTotal = 0;
        //private static byte[] aRetVal;
        //public static byte[] BytesGet(int nSize, byte nFrom)
        //{
        //	lock (_ahBytesStorage)
        //	{
        //		if (_ahBytesStorage.Keys.Contains(nSize) && 0 < _ahBytesStorage[nSize].Count)
        //		{
        //			nHash = _ahBytesStorage[nSize].Peek().GetHashCode();
        //			if (_aBytesHashes.Contains(nHash))
        //			{
        //				if (_ahHashPassedOut[nHash] == false)
        //					_ahHashPassedOut[nHash] = true;
        //				else
        //					(new Logger("device")).WriteDebug("device.bytes error - already passed out!!");
        //				return _ahBytesStorage[nSize].Dequeue();
        //			}
        //			(new Logger("device")).WriteDebug("device.bytes error - not in hashes!");
        //		}
        //		else
        //		{
        //			if (!_ahBytesStorage.Keys.Contains(nSize))
        //			{
        //				(new Logger("device")).WriteDebug("device.bytes adding new size to bytes storage [" + nSize + "] (from=" + nFrom + ")");
        //				_ahNumTotal.Add(nSize, 0);
        //                      _ahBytesStorage.Add(nSize, new Queue<byte[]>());
        //			}
        //			nBytesTotal += nSize;
        //			_ahNumTotal[nSize]++;
        //			aRetVal = new byte[nSize];
        //			(new Logger("device")).WriteDebug("device.bytes is returning new byte  (from=" + nFrom + ")[hc=" + aRetVal.GetHashCode() + "][" + nSize + "][sizes=" + _ahNumTotal.Keys.Count + "][total=" + _ahNumTotal[nSize] + "(" + _aBytesHashes.Count() + ")][bytes=" + nBytesTotal + "]");
        //			_ahPointers.Add(aRetVal.GetHashCode(), new PointerToBytes(aRetVal));
        //                  _aBytesHashes.Add(aRetVal.GetHashCode());
        //			_ahHashPassedOut.Add(aRetVal.GetHashCode(), true);
        //                  return aRetVal;
        //		}
        //		throw new Exception("bytes get is impossible");
        //	}
        //}
        //public static void BytesBack(byte[] aBytes, byte nFrom)
        //{
        //	if (null == aBytes)
        //	{
        //		(new Logger("device")).WriteDebug("device.bytes error - received NULL bytes! (from=" + nFrom + ")");
        //		return;
        //	}
        //	lock (_ahBytesStorage)
        //	{
        //		if (_aBytesHashes.Contains(aBytes.GetHashCode()))
        //			if (_ahHashPassedOut[aBytes.GetHashCode()])
        //			{
        //				_ahHashPassedOut[aBytes.GetHashCode()] = false;
        //				_ahBytesStorage[aBytes.Length].Enqueue(aBytes);
        //			}
        //			else
        //				(new Logger("device")).WriteDebug("device.bytes error - received twice!!! (from=" + nFrom + ")[size=" + aBytes.Length + "]");
        //		else if (!_aBytesEmptyHashes.Contains(aBytes.GetHashCode()))
        //			(new Logger("device")).WriteDebug("device.bytes error - received not our bytes! [" + aBytes.Length + "] (from=" + nFrom + ")[size=" + aBytes.Length + "]");
        //	}
        //}
        public class Frame : helpers.Frame
        {
            new public class Audio : helpers.Frame.Audio
            {
                override public void Dispose()
                {
                    lock (_oDisposeLock)
                    {
                        if (bDisposed)
                            return;
                        bDisposed = true;
                    }
                    if (null != aFrameBytes)
                        _cBinM.BytesBack(aFrameBytes, 0);
                }
            }
            new public class Video : helpers.Frame.Video
            {
                private object oSyncRoot = new object();
                private int _nReferences = 0;
                public int nReferences  // only for output usage
                {
                    get
                    {
                        lock (oSyncRoot)
                        {
                            if (_bFreeze && 1 > _nRefTotal && DateTime.Now > _dtRefEnd)
                            {
                                _bFreeze = false;
                                _dtRefEnd = DateTime.MaxValue;
                                (new Logger("device", _sDeviceName)).WriteNotice("there is freeze [" + _nFreezeIndx + "] in the air!!!   __END__ (3 seconds ago)");
                                _nFreezeIndx++;
                            }
                            return _nReferences;
                        }
                    }
                    set
                    {
                        lock (oSyncRoot)
                        {
                            _nRefTotal += value - _nReferences;
                            if (0 < _nRefTotal)
                            {
                                if (DateTime.MaxValue > _dtRefEnd)  //0 < Baetylus.nCurrentBufferCount &&    это было против прямого эфира, где остановка - норма жизни ))   подумать надо как быть без btl  // пока дебаг4 включил
                                    (new Logger("device", _sDeviceName)).WriteDebug4("there is freeze [" + _nFreezeIndx + "] in the air!!!   __CONTINUE__  [nRefTotal=" + _nRefTotal + "][_nReferences=" + _nReferences + "][new_value=" + value + "]"); // временно
                                if (DateTime.MaxValue == _dtRefEnd)
                                {
                                    (new Logger("device", _sDeviceName)).WriteNotice("there is freeze [" + _nFreezeIndx + "] in the air!!!   __BEGIN__  [nRefTotal=" + _nRefTotal + "][_nReferences=" + _nReferences + "][new_value=" + value + "]");
                                }
                                _dtRefEnd = DateTime.Now.AddSeconds(3);
                                _bFreeze = true;
                            }
                            _nReferences = value;
                        }
                    }
                }
                private string _sDeviceName;
                public Dictionary<uint, Bytes> ahVancDataLine_Bytes;
                public Video(string sDeviceName)
                    : base()
                {
                    _sDeviceName = sDeviceName;
                }
            }
            new public Audio cAudio;
            new public Video cVideo;
            ~Frame()
            {
                //if (cVideo!= null && cVideo.ahVancDataLine_Bytes != null)
                //{
                //    foreach (Bytes cB in cVideo.ahVancDataLine_Bytes.Values)
                //    {
                //        _cBinM.BytesBack(cB, 100);
                //    }
                //}
            }
        }
        private static int _nRefTotal = 0;
        private static int _nFreezeIndx = 0;
        private static bool _bFreeze = false;
        private static DateTime _dtRefEnd = DateTime.MaxValue;
        public delegate Frame NextFrameNonPipeCallback();
        public event NextFrameNonPipeCallback NextFrameNonPipe;
        public event AVFrameArrivedCallback AVFrameArrived;
		private event NextFrameCallback NextFrame;
		protected bool bAVFrameArrivedAttached
		{
			get
			{
				return (null != AVFrameArrived);
			}
		}
		protected bool NextFrameAttached
		{
			get
			{
				return (null != NextFrame);
			}
		}

        public bool bLogVANC = false;
		public Frame.Video _cVideoFrameEmpty; //bug
		public List<Frame.Video> _aConveyorTotal; //bug
		protected IntPtr _pVideoFrameBuffer;
		protected uint _nAudioBufferCapacity_InSamples;
		protected DateTime _dtLastTimeFrameScheduleCalled = DateTime.MinValue;
		protected int _nFramesLated;
		protected int _nFramesDropped;
		protected int _nFramesFlushed;
        protected bool _bNeedToAddFrame;
		protected int n__PROBA__VideoFramesBuffered, n__PROBA__AudioFramesBuffered;
		protected int n__PROBA__AddToVideoStreamTime;
		protected bool _b__PROBA__OutOfRangeFlag;
		protected Queue<Frame.Audio> _aq__PROBA__AudioFrames = new Queue<Frame.Audio>();
		protected Queue<Frame.Video> _aq__PROBA__VideoFrames = new Queue<Frame.Video>();
		protected Frame.Video _cFrameVideoLast;
		protected Frame.Audio _cFrameAudioEmpty;
        public string sName;
        public bool bLoggingVANCOnInput = false;
        protected uint[] _aTmpUint;

        public int nFramesLated
		{
			get
			{
				return _nFramesLated;
			}
		}
		public int nFramesDropped
		{
			get
			{
				return _nFramesDropped;
			}
		}
		public int nFramesFlushed
		{
			get
			{
				return _nFramesFlushed;
			}
		}

		protected Area _stArea;
		public Area stArea
		{
			get
			{
				return _stArea;
			}
			protected set
			{
				(new Logger("device", sName)).WriteDebug3("device:area:set:in");
				_stArea = value;
				(new Logger("device", sName)).WriteDebug4("device:area:set:return");
			}
		}
		public byte[] aFrameLastBytes { get; private set; }
		private int nBufferOneThird
		{
			get
			{
				return Preferences.nQueueDeviceLength / 3;
			}
		}
		private int nBufferTwoThird
		{
			get
			{
				return 2 * nBufferOneThird;
			}
		}
		private int nBufferCurrent
		{
			get
			{
				if (n__PROBA__VideoFramesBuffered <= n__PROBA__AudioFramesBuffered)
					return n__PROBA__VideoFramesBuffered;
				else
					return n__PROBA__AudioFramesBuffered;
			}
		}
        protected bool bInput = false;
        virtual public bool bCardStopped { get;}

        static Device()
		{
            _cBinM = new BytesInMemory("device bytes");
        }
		static public Device BoardGet(uint nIndex)
		{
			(new Logger("device", null)).WriteDebug3("in");
			Device cRetVal = null;

            try
            {
                int nDeckLinkCount = 0;
                try
                {
                    nDeckLinkCount = Decklink.BoardsQtyGet();
                }
                catch (Exception ex)
                {
                    (new Logger("device", null)).WriteWarning("decklink BoardsQtyGet error", ex);
                }
                int nAjaCount = 0;
                try
                {
                    nAjaCount = Aja.BoardsQtyGet();
                }
                catch (Exception ex)
                {
                    (new Logger("device", null)).WriteWarning("aja BoardsQtyGet error", ex);
                }
                (new Logger("device", null)).WriteNotice("decklink boards count: [qty=" + nDeckLinkCount + "]; aja boards count: [qty=" + nAjaCount + "];");
                (new Logger("device", null)).WriteNotice("will load: [make=" + Preferences.sDeviceMake + "] (see prefs)");
                switch (Preferences.sDeviceMake)
                {
                    case "decklink":
                        cRetVal = Decklink.BoardGet(nIndex);
                        break;
                    case "aja":
                        cRetVal = Aja.BoardGet(nIndex);
                        break;
                    case "decklink_fake":
                        cRetVal = DecklinkFake.BoardGet(nIndex);
                        break;
                    default:
                        throw new Exception("unknown board's make");
                }
            }
            catch (Exception ex)
            {
                (new Logger("device", null)).WriteError(ex);
            }

            if (cRetVal == null)
                throw new Exception($"can't find this board [{Preferences.sDeviceMake}-{nIndex}]");
            (new Logger("device", null)).WriteNotice("board found [name=" + cRetVal.sName + "]");
#if XNA
            try
            {
                aRetVal.AddRange(Display.BoardsGet());
            }
            catch (Exception ex)
            {
                (new Logger("device", null)).WriteError(ex);
            }
            (new Logger("device", null)).WriteNotice("xna boards found [qty=" + (aRetVal.Count - nBCount) + "]");
#endif
            (new Logger("device", null)).WriteDebug4("return");
            return cRetVal;
		}

		protected Device(string sDeviceName)
		{
            sName = sDeviceName;
            bInput = Preferences.bDeviceInput;
            (new Logger("device", sName)).WriteDebug3("in");
			_bNeedToAddFrame = false;
			_pVideoFrameBuffer = IntPtr.Zero;
			_nFramesLated = 0;
			_nFramesDropped = 0;
			_nFramesFlushed = 0;
			_b__PROBA__OutOfRangeFlag = true;
			_aConveyorTotal = new List<Frame.Video>();
			(new Logger("device", sName)).WriteDebug4("return");

#if DEBUG
            _aqWritingFrames = new Queue<byte[]>();
            _cThreadWritingFramesWorker = new System.Threading.Thread(WritingFramesWorker);
            _cThreadWritingFramesWorker.IsBackground = true;
            _cThreadWritingFramesWorker.Priority = System.Threading.ThreadPriority.Normal;
            _cThreadWritingFramesWorker.Start();
#endif

            _cBugCatcherOnVideoFramePrepare = new BugCatcher(_cVideoFrameEmpty, sName);  // bug
		}
		~Device()
		{
		}
        virtual public void Dispose()
        {
        }

		private System.Threading.Thread _cThread;
		private Thread _PipeClientThread;
		private Thread _PipeClientThread2;
		private string _nBoardNumber;
		private ThreadBufferQueue<Device.Frame> _aqBufferFrame;
		private Queue<Device.Frame> _aqBTLFrames;
        public uint _nChannelBytesQty;
        public uint _nAudioBytesQty;
        public int _nVideoBytesQty;
        public int _nRowBytesQty;
        public ushort nFPS;
        protected ushort _nAudioChannelsQty;
        private long nMem;
        internal delegate void ReverseChannelsDo(byte[] aFrameBytes);
        virtual internal ReverseChannelsDo ReverseChannels { get; }
        public int _nBufferFrameCount;

        public void PipeStart(string nBoard)
		{
			_nBoardNumber = nBoard;
			(new Logger("device", sName)).WriteNotice("Starting PIPE client" + _nBoardNumber);
			NextFrame += new NextFrameCallback(OnNextFrame);
			_aqBufferFrame = new ThreadBufferQueue<Frame>(Preferences.nQueuePipeLength, true, false);
			_aqBTLFrames = new Queue<Frame>();

            if (NextFrameNonPipe != null)
                _PipeClientThread = new Thread(ThreadNonPipe);
            else
                _PipeClientThread = new Thread(ThreadStartClient);
            _PipeClientThread.IsBackground = true;
			_PipeClientThread.Priority = System.Threading.ThreadPriority.Normal;
			_PipeClientThread.Start();
        }
        public void ThreadNonPipe(object obj)
        {
            int nCount = 0;
            Frame cFrameResult, cFrameGot;
            bool bFirstTime = true;
            Logger.Timings cTimings = new helpers.Logger.Timings("pipe_worker");
            byte[] aTmpVideo = new byte[_nVideoBytesQty];
            while (true)
            {
                try
                {
                    if (bFirstTime && _aqBufferFrame.nCount >= Preferences.nQueuePipeLength - 1)
                    {
                        bFirstTime = false;
                        TurnOn();
                    }

                    cFrameGot = NextFrameNonPipe();
                    cFrameResult = new Device.Frame() { cAudio = null, cVideo = null };
                    cFrameResult.cVideo = FrameBufferGet();
                    if (cFrameResult.cVideo.oFrameBytes is Bytes)  // aja
                    {
                        Buffer.BlockCopy(cFrameGot.cVideo.aFrameBytes.aBytes, 0, cFrameResult.cVideo.aFrameBytes.aBytes, 0, _nVideoBytesQty);
                        if (null != ReverseChannels)
                            ReverseChannels(cFrameResult.cVideo.aFrameBytes.aBytes);
                    }
                    else // decklink - pointer
                    {
                        if (null != ReverseChannels)
                        {
                            //Buffer.BlockCopy(cFrameGot.cVideo.aFrameBytes.aBytes, 0, aTmpVideo, 0, _nVideoBytesQty);
                            //ReverseChannels(aTmpVideo);
                            //Marshal.Copy(aTmpVideo, 0, cFrameResult.cVideo.pFrameBytes, _nVideoBytesQty);
                            ReverseChannels(cFrameGot.cVideo.aFrameBytes.aBytes);
                        }
                        Marshal.Copy(cFrameGot.cVideo.aFrameBytes.aBytes, 0, cFrameResult.cVideo.pFrameBytes, _nVideoBytesQty);
                    }
                    _cBinM.BytesBack(cFrameGot.cVideo.aFrameBytes, 5);

                    if (cFrameResult.cVideo.ahVancDataLine_Bytes != null)
                        foreach (Bytes aB in cFrameResult.cVideo.ahVancDataLine_Bytes.Values)
                            _cBinM.BytesBack(aB, 60);
                    cFrameResult.cVideo.ahVancDataLine_Bytes = cFrameGot.cVideo.ahVancDataLine_Bytes;

                    if (cFrameGot.cAudio != null)
                    {
                        cFrameResult.cAudio = new Frame.Audio();
                        cFrameResult.cAudio.aFrameBytes = _cBinM.BytesGet((int)_nAudioBytesQty, 5);
                        Buffer.BlockCopy(cFrameGot.cAudio.aFrameBytes.aBytes, 0, cFrameResult.cAudio.aFrameBytes.aBytes, 0, (int)_nAudioBytesQty);
                        //_cBinM.BytesBack(cFrameGot.cAudio.aFrameBytes, 7);   // audio frame goes to ~ and dispose
                    }

                    if (nBufferCurrent > nBufferTwoThird)
                        System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
                    else if (nBufferCurrent > nBufferOneThird)
                        System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
                    else
                        System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;

                    if (Preferences.nGCFramesInterval > 0 && nCount++ > Preferences.nGCFramesInterval && nBufferCurrent > nBufferTwoThird)
                    {
                        cTimings.TotalRenew();
                        nMem = GC.GetTotalMemory(false);
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
                        GC.WaitForFullGCComplete(1000);
                        cTimings.Stop("GC_device", "GC-Optimized" + " " + System.Runtime.GCSettings.LatencyMode + "[frames_ago=" + nCount + "]", 10);
                        nCount = 0;
                    }


#if DEBUG
                    if (_bDoWritingFrames)
                    {
                        if (null != cFrameResult?.cVideo?.aFrameBytes)
                        {
                            byte[] aBytes = new byte[_nVideoBytesQty];
                            if (cFrameResult.cVideo.oFrameBytes is IntPtr)
                                Marshal.Copy(cFrameResult.cVideo.pFrameBytes, aBytes, 0, _nVideoBytesQty);
                            else
                                Array.Copy(cFrameResult.cVideo.aFrameBytes.aBytes, aBytes, _nVideoBytesQty);

                            lock (_aqWritingFrames)
                                _aqWritingFrames.Enqueue(aBytes);
                        }
                    }
#endif


                    _aqBufferFrame.Enqueue(cFrameResult); //sleep inside
                }
                catch (Exception ex)
                {
                    (new Logger("device", sName)).WriteError(ex);
                    Thread.Sleep(20);
                }
            }

        }
        public void ThreadStartClient(object obj)
		{
			(new Logger("device", sName)).WriteNotice("Started PIPE client and waiting for the server..." + _nBoardNumber);
			NamedPipeClientStream pipeStream = new NamedPipeClientStream("FramesGettingPipe-"+ _nBoardNumber);
			pipeStream.Connect();
			(new Logger("device", sName)).WriteNotice("PIPE client connected to Server " + _nBoardNumber);

            BinaryFormatter cBinFormatter = new BinaryFormatter();
            StreamWriter cSW = new StreamWriter(pipeStream);
            cSW.AutoFlush = true;
			StreamReader cSR = new StreamReader(pipeStream);
			string sRes;
			System.IO.StringWriter cStringWriter = new System.IO.StringWriter();
			StringBuilder cStringBuilder = cStringWriter.GetStringBuilder();
			bool bFirstTime = true;

			Bytes aTMPVideo = _cBinM.BytesGet((int)_nVideoBytesQty, 3);

            while (null == (sRes = cSR.ReadLine())) { System.Threading.Thread.Sleep(10); }
            if (sRes == "get_area")
            {
                (new XmlSerializer(typeof(Area), new XmlRootAttribute() { ElementName = "stArea", IsNullable = false })).Serialize(cStringWriter, stArea);
                string sSerialized = cStringWriter.ToString();
                cStringBuilder.Remove(0, cStringBuilder.Length);
                sSerialized = sSerialized.Replace("\r\n", "");
                cSW.WriteLine(sSerialized);
            }
            else
                throw new Exception("wrong quest [" + sRes + "][expected='get_area']");

            while (null == (sRes = cSR.ReadLine())) { System.Threading.Thread.Sleep(10); }
            if (sRes == "get_fps")
            {
                cSW.WriteLine("" + Preferences.nFPS);
            }
            else
                throw new Exception("wrong quest [" + sRes + "][expected='get_fps']");

            while (null == (sRes = cSR.ReadLine())) { System.Threading.Thread.Sleep(10); }
            if (sRes == "get_buffer_size")
            {
                cBinFormatter.Serialize(pipeStream, (uint)Preferences.nQueueDeviceLength);
            }
            else
                throw new Exception("wrong quest [" + sRes + "][expected='get_buffer_size']");

            while (null == (sRes = cSR.ReadLine())) { System.Threading.Thread.Sleep(10); }
            if (sRes == "turn_on")
			{
				// см ниже
			}
            else
                throw new Exception("wrong quest [" + sRes + "][expected='turn_on']");
            int nCount = 0;
			Frame cFrameResult;
            Frame.Video cEmptyVideo = new Frame.Video(sName) { oFrameBytes = new Bytes() { aBytes = new byte[0], nID = -1 } };
            _cBinM.AddToIgnor(cEmptyVideo.aFrameBytes);

            int nWatsNext; // 1 - видео нет   2 - видео есть   3 - видео [0]        10 - аудио нет    11 - аудио есть
			Logger.Timings cTimings = new helpers.Logger.Timings("pipe_worker");
            while (true)
			{
				try
				{
					if (bFirstTime && _aqBufferFrame.nCount >= Preferences.nQueuePipeLength - 1)
					{
						bFirstTime = false;
						TurnOn();
					}
					//cSW.WriteLine("next_frame");
					//while (null == (sRes = cSR.ReadLine())) ;
					nWatsNext = pipeStream.ReadByte();
					cFrameResult = new Device.Frame() { cAudio = null, cVideo = null };
					if (nWatsNext == 0)
						throw new Exception("pipestream got 0");
					if (nWatsNext == 2)
					{
						cFrameResult.cVideo = FrameBufferGet();
                        if (cFrameResult.cVideo.oFrameBytes is Bytes)
                        {
                            pipeStream.Read(cFrameResult.cVideo.aFrameBytes.aBytes, 0, aTMPVideo.Length);
                            if (null != ReverseChannels)
                                ReverseChannels(cFrameResult.cVideo.aFrameBytes.aBytes);
                        }
                        else
                        {
                            pipeStream.Read(aTMPVideo.aBytes, 0, aTMPVideo.Length);
                            if (null != ReverseChannels)
                                ReverseChannels(aTMPVideo.aBytes);
                            Marshal.Copy(aTMPVideo.aBytes, 0, cFrameResult.cVideo.pFrameBytes, aTMPVideo.Length);
                        }
					}
					else if (nWatsNext == 3)
						cFrameResult.cVideo = cEmptyVideo;

					nWatsNext = pipeStream.ReadByte();

					if (nWatsNext == 11)
					{
						cFrameResult.cAudio = new Frame.Audio();
						cFrameResult.cAudio.aFrameBytes = _cBinM.BytesGet((int)_nAudioBytesQty, 4);
						pipeStream.Read(cFrameResult.cAudio.aFrameBytes.aBytes, 0, cFrameResult.cAudio.aFrameBytes.Length);
					}

                    cBinFormatter.Serialize(pipeStream, _aqBufferFrame.CountGet() + 1);

                    if (nBufferCurrent > nBufferTwoThird)
						System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
					else if (nBufferCurrent > nBufferOneThird)
						System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
					else
						System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;

					if (Preferences.nGCFramesInterval > 0 && nCount++ > Preferences.nGCFramesInterval && nBufferCurrent > nBufferTwoThird)
					{
						cTimings.TotalRenew();
						nMem = GC.GetTotalMemory(false);
						GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
						GC.WaitForFullGCComplete(1000);
						cTimings.Stop("GC_device", "GC-Optimized" + " " + System.Runtime.GCSettings.LatencyMode + "[frames_ago=" + nCount + "]", 10);
						nCount = 0;
					}


#if DEBUG
                    if (_bDoWritingFrames)
                    {
                        if (null != cFrameResult?.cVideo?.aFrameBytes)
                        {
                            byte[] aBytes = new byte[aTMPVideo.Length];
                            if (cFrameResult.cVideo.oFrameBytes is IntPtr)
                                Marshal.Copy(cFrameResult.cVideo.pFrameBytes, aBytes, 0, aTMPVideo.Length);
                            else
                                Array.Copy(cFrameResult.cVideo.aFrameBytes.aBytes, aBytes, aTMPVideo.Length);

                            lock (_aqWritingFrames)
                                _aqWritingFrames.Enqueue(aBytes);
                        }
                    }
#endif


                    _aqBufferFrame.Enqueue(cFrameResult); //sleep inside
				}
				catch (Exception ex)
				{
					(new Logger("device", sName)).WriteError(ex);
                    Thread.Sleep(20);
				}
			}
		}
        Device.Frame OnNextFrame()
		{
			_nBufferFrameCount = (int)_aqBufferFrame.nCount;
			Device.Frame cRetVal = null;
			if (0 < _aqBufferFrame.nCount)
			{
				cRetVal = _aqBufferFrame.Dequeue();
			}
			else
				Thread.Sleep(0);
			return cRetVal;
		}
        virtual public void TurnOn()
		{
            _bDoWritingFrames = false;

            if (Preferences.bAudio)
			{
				_cFrameAudioEmpty = new Frame.Audio();
                _cFrameAudioEmpty.aFrameBytes = new Bytes() { aBytes = new byte[Preferences.nAudioBytesPerFrame], nID = -1 };
                _cBinM.AddToIgnor(_cFrameAudioEmpty.aFrameBytes);
            }
            if (bInput)
            {
            }
            else
            {
                _cVideoFrameEmpty = FrameBufferPrepare();
                if (_cVideoFrameEmpty.oFrameBytes is IntPtr)
                {
                    uint nBlack = 0x0;
                    for (int nIndx = 0; nIndx < _nRowBytesQty * _stArea.nHeight ; nIndx += 4)
                        Marshal.WriteInt32(_cVideoFrameEmpty.pFrameBytes, nIndx, (Int32)nBlack); //TODO  its only for RGB pixel format (not yuv!!)
                }
                _cFrameVideoLast = _cVideoFrameEmpty;
                (new Logger("device", sName)).WriteNotice("empty frame is " + _cVideoFrameEmpty.nID);
                //while (Preferences.nQueuePipeLength + 2 > _aFrames.Count)
                //    AddNewFrameToConveyor("! from TurnOn !");

                _cThread = new System.Threading.Thread(FrameScheduleWorker);
                _cThread.Priority = System.Threading.ThreadPriority.Normal;
                _cThread.Start();
                _cBugCatcherOnFrameGet = new BugCatcher(_cVideoFrameEmpty, sName);  // bug
                _cBugCatcherOnVideoFrameReturn = new BugCatcher(_cVideoFrameEmpty, sName);  // bug
                //_cBugCatcherOnVideoFramePrepare = new BugCatcher(_cVideoFrameEmpty);  // bug
                _cBugCatcherScheduleFrame = new BugCatcher(_cVideoFrameEmpty, sName);  // bug
                                                                                       //System.Threading.ThreadPool.QueueUserWorkItem(FrameScheduleWorker);
            }
        }
        virtual public void DownStreamKeyer()
		{ }
		abstract protected Frame.Video FrameBufferPrepare();
		private Frame.Video AddNewFrameToConveyor(string sInfo)
		{
			Frame.Video oRetVal;
			oRetVal = FrameBufferPrepare();
			if (_aConveyorTotal.Contains(oRetVal))    // проверить было ли вообще такое	
			{
				do
				{
					oRetVal = FrameBufferPrepare();
					(new Logger("device", sName)).WriteWarning("Trying to ADD NEW videoframe that already exists in conveyor: [_aFrames.Count = " + _aFrames.Count() + "][_aq__PROBA__AVFrames = (" + _aq__PROBA__VideoFrames.Count + ", " + _aq__PROBA__AudioFrames.Count + ")]<br>[info = " + sInfo);
				} while (_aConveyorTotal.Contains(oRetVal));
			}
			_aFrames.AddLast(oRetVal);
			_aConveyorTotal.Add(oRetVal);
			nConvLength++;
			return oRetVal;
		}
		private void ReturnFrameToConveyor(Frame.Video oVF, string sInfo) //bug
		{
			lock (_aFrames)
			{
				if (_aFrames.Contains(oVF))
				{
					(new Logger("device", sName)).WriteWarning("Trying to RETURN videoframe that already returned to conveyor some items ago: [_aFrames.Count = " + _aFrames.Count() + "][_aq__PROBA__AVFrames = (" + _aq__PROBA__VideoFrames.Count + ", " + _aq__PROBA__AudioFrames.Count + ")]<br>[info = " + sInfo);
					return;
				}
				_aFrames.AddLast(oVF);
			}
		}
		protected void FrameBufferReleased(Frame.Video o)  //bug
		{
			lock (_aFrames)
			{
				if (0 < o.nReferences)
				{
					o.nReferences--;
					return;
				}
				if (o == _cVideoFrameEmpty || o == _cFrameVideoLast)
					return;
				ReturnFrameToConveyor(o, "! from FrameBufferReleased !");
			}
		}
        private LinkedList<Frame.Video> _aFrames = new LinkedList<Frame.Video>();
        public int nConveyorCount
        {
            get
            {
                return _aFrames == null ? int.MinValue : _aFrames.Count;
            }
        }
        private int nConvLength = 0;
		long nQ = 0;
		public Frame.Video FrameBufferGet()
		{
			Frame.Video cRetVal = null;
			string sInfo = "video prepare:";//bug
			lock (_aFrames)
			{
				if (3 > _aFrames.Count) // чтобы сохранять зазор в 2 кадра  //bug
				{
					AddNewFrameToConveyor("! from FrameBufferGet !"); 
					(new Logger("device", sName)).WriteNotice("размер конвейера был увеличен до " + nConvLength);
					sInfo += "conveier new:";
				}
				else
					sInfo += "conveier old:";
				cRetVal = _aFrames.First.Value;
				_aFrames.RemoveFirst();
			}
			_cBugCatcherOnVideoFramePrepare.Enqueue(cRetVal, sInfo + "_aq__PROBA__VideoFrames:" + _aq__PROBA__VideoFrames.Count + ":_aq__PROBA__AudioFrames:" + _aq__PROBA__AudioFrames.Count);
			return cRetVal;
		}
		private void FrameScheduleWorker(object cState)
		{
			bool bAdded;
			double nSecondsElapsed;
			int nSleepDuration = Preferences.nFrameDuration / 2;
			Logger.Timings cTimings = new helpers.Logger.Timings("device:FrameScheduleWorker");
			while (true)
			{
				try
				{
					if (_dtLastTimeFrameScheduleCalled > DateTime.MinValue && (nSecondsElapsed = DateTime.Now.Subtract(_dtLastTimeFrameScheduleCalled).TotalSeconds) > 3)
					{
						(new Logger("device", sName)).WriteError(new Exception("frame scheduled was more than 3 seconds ago. device may be dead. [delta t = " + nSecondsElapsed + " seconds]"));
						_dtLastTimeFrameScheduleCalled = DateTime.MinValue;
					}
					if (_bNeedToAddFrame)
					{
						bAdded = FrameSchedule();
						//if (bAdded && Preferences.nQueueDeviceLength <= n__PROBA__AudioFramesBuffered && Preferences.nQueueDeviceLength <= n__PROBA__VideoFramesBuffered)
						if (bAdded && Preferences.nQueueDeviceLength <= n__PROBA__AudioFramesBuffered && Preferences.nQueueDeviceLength <= n__PROBA__VideoFramesBuffered)
							_bNeedToAddFrame = false;
					}
					else
					{
						//if (BTL.Baetylus.nCurrentBufferCount > BTL.Baetylus.nBufferTwoThird && n__PROBA__VideoFramesBuffered > 9 && n__PROBA__AudioFramesBuffered > 9)
						//	System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
						//else if (BTL.Baetylus.nCurrentBufferCount > BTL.Baetylus.nBufferOneThird)
						//	System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
						//else
						//	System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;

						(new Logger("device", sName)).WriteDebug4("FrameScheduleWorker:Sleep");
						System.Threading.Thread.Sleep(nSleepDuration);
					}
				}
				catch (Exception ex)
				{
					(new Logger("device", sName)).WriteError(ex);
				}
			}
		}
		int nQty = 5;

		// bug
		BugCatcher _cBugCatcherOnFrameGet, _cBugCatcherOnVideoFrameReturn, _cBugCatcherOnVideoFramePrepare;
		public BugCatcher _cBugCatcherScheduleFrame;
		public class BugCatcher
		{
			class Info
			{
				public Frame cFrame;
				public string sInfo;
			}
			List<Info> aLastNFrames = new List<Info>();
			Frame.Video _cEmptyVideoFrame;
            string _sDeviceName;
            public BugCatcher(Frame.Video cEmpty, string sDeviceName)
			{
                _sDeviceName = sDeviceName;
                Info cInfo = new Info { cFrame = new Frame() { cVideo = cEmpty }, sInfo = "empty frame" };
				aLastNFrames.Add(cInfo);
				aLastNFrames.Add(cInfo);
				aLastNFrames.Add(cInfo);
				aLastNFrames.Add(cInfo);
				_cEmptyVideoFrame = cEmpty;
			}
			public void Enqueue(Frame cFrame, string sInfo)
			{
				return;
#pragma warning disable CS0162 // Unreachable code detected
				aLastNFrames.Add(new Info() { cFrame = cFrame, sInfo = sInfo });
				aLastNFrames.RemoveAt(0);
				TestForBug();
			}
			public void Enqueue(Frame.Video cFrame, string sInfo)
			{
				return;
#pragma warning disable CS0162 // Unreachable code detected
				Enqueue(new Frame() { cVideo = cFrame }, sInfo);
			}
			void TestForBug()
			{
				try
				{
					if (
							null != aLastNFrames[0].cFrame &&
							null != aLastNFrames[1].cFrame &&
							null != aLastNFrames[2].cFrame &&
							null != aLastNFrames[3].cFrame &&
							aLastNFrames[1].cFrame.cVideo != _cEmptyVideoFrame
							&& aLastNFrames[1].cFrame.cVideo == aLastNFrames[2].cFrame.cVideo
							&& aLastNFrames[0].cFrame.cVideo != aLastNFrames[1].cFrame.cVideo
							&& aLastNFrames[2].cFrame.cVideo != aLastNFrames[3].cFrame.cVideo
						)
						(new Logger("device", _sDeviceName)).WriteWarning("BUG DETECTED: <br>" + aLastNFrames[0].sInfo + "<br>" + aLastNFrames[1].sInfo + "<br>" + aLastNFrames[2].sInfo + "<br>" + aLastNFrames[3].sInfo);
				}
				catch (Exception ex)
				{
					(new Logger("device", _sDeviceName)).WriteError(ex);
				}
			}
		}
		public static Dictionary<long, long> _aCurrentFramesIDs; //DNF   // output only
		public static System.Diagnostics.Stopwatch _cStopWatch;  // output only
        public static long _nLastScTimeComplited = 0;  // output only

        private long BalanceBeemTimeCounter_a;
        private long BalanceBeemTimeCounter_v;

        private bool bTEST;
        private DateTime dtTEST = DateTime.MaxValue;
        private int nTEST;
		protected Frame.Audio AudioFrameGet()
		{
            #region beem test  
            //   тест рассинхрона
            //if (!bTEST)
            //{
            //    bTEST = true;
            //    dtTEST = DateTime.Now.AddSeconds(15);
            //}
            //if (dtTEST < DateTime.Now)
            //{
            //    dtTEST = DateTime.MaxValue;
            //    nTEST = 7;
            //}
            //if (nTEST-- > 0)
            //{
            //    Frame cFrameT = NextFrame();
            //    _aq__PROBA__VideoFrames.Enqueue(cFrameT.cVideo);
            //    (new Logger("device", sName)).WriteDebug("TEST BEEM. AUDIO FRAME DROPPED. [remain=" + nTEST + "]");
            //}
            #endregion







            // пока не разобрались с набегающим рассинхроном - теряем видео-кадрик!  // набегает на некоторых карточках (BMD), если подана синхра!  // без синхры оочень редко - раз в несколько месяцев и не на всех карточках
            if (2 < _aq__PROBA__VideoFrames.Count)
            {
                if (100 < BalanceBeemTimeCounter_a)
                {
                    BalanceBeemTimeCounter_a = 0;
                    Frame.Video cVF = _aq__PROBA__VideoFrames.Dequeue();
                    FrameBufferReleased(cVF); //  возврат в конвейер
                    (new Logger("device", sName)).WriteWarning("BALANCE-BEEM. SYNC CORRECTED BY DROPPING __VIDEO__ FRAME! [video_frame_id=" + cVF.nID + "][av=(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")][ticks=" + DateTime.Now.Ticks + "]");
                }
                else if (0 == BalanceBeemTimeCounter_a)
                    (new Logger("device", sName)).WriteDebug3("balance-beem-1 started [ticks=" + DateTime.Now.Ticks + "]");
                BalanceBeemTimeCounter_a++;
            }
            else if (0 < BalanceBeemTimeCounter_a)
            {
                BalanceBeemTimeCounter_a = 0;
                (new Logger("device", sName)).WriteDebug3("balance-beem-1 ended [ticks=" + DateTime.Now.Ticks + "]");
            }









            //(new Logger("device", sName)).WriteDebug2("AudioFrameGet  [count=" + _aq__PROBA__AudioFrames.Count + "]");

            if (0 < _aq__PROBA__AudioFrames.Count)
			{
				//(new Logger("device", sName)).WriteDebug2("AudioFrameGet return from q  [id=" + _aq__PROBA__AudioFrames.Peek().nID + "]");
				return _aq__PROBA__AudioFrames.Dequeue();
			}
			Frame.Audio cRetVal = _cFrameAudioEmpty;
			Frame cFrame = NextFrame();
			_cBugCatcherOnFrameGet.Enqueue(cFrame, "audio recieve:_aq__PROBA__VideoFrames:" + _aq__PROBA__VideoFrames.Count + ":_aq__PROBA__AudioFrames:" + _aq__PROBA__AudioFrames.Count);
            if (null != cFrame && null != cFrame.cVideo)
            {
				if (null == cFrame.cVideo.oFrameBytes)
				{
					_cFrameVideoLast.nReferences++;
					(new Logger("device", sName)).WriteDebug2("BYTES FROM BTL IS NULL 1 - repeat the last [id=" + _cFrameVideoLast.nID + "][ref=" + _cFrameVideoLast.nReferences + "]");
				}
				else if (cFrame.cVideo.oFrameBytes is Bytes && 1 > cFrame.cVideo.aFrameBytes.Length)
					_cFrameVideoLast = _cVideoFrameEmpty; //получили признак необходимости очистить экран
				else
					_cFrameVideoLast = cFrame.cVideo;



				_aq__PROBA__VideoFrames.Enqueue(_cFrameVideoLast);



				if (null != cFrame.cAudio && null != cFrame.cAudio.aFrameBytes)
					cRetVal = cFrame.cAudio;
				else
					(new Logger("device", sName)).WriteDebug4("Got null audio frame from BTL! [audio_frame_is_null = " + (cFrame.cAudio == null ? "true]" : "false][bytes_is_null = " + (cFrame.cAudio.aFrameBytes == null ? "true" : "false") + "]"));
			}
			else
			{
				_aq__PROBA__VideoFrames.Enqueue(_cFrameVideoLast);
				_cFrameVideoLast.nReferences++;
				// пока не разобрались с набегающим рассинхроном - теряем кадрик!
				(new Logger("device", sName)).WriteDebug3("FRAME FROM BTL IS NULL 1 - repeat the last [id=" + _cFrameVideoLast.nID + "][ref=" + _cFrameVideoLast.nReferences + "]");
			}
			//(new Logger("device", sName)).WriteDebug2("AudioFrameGet return end  [id=" + cRetVal.nID + "]");
			return cRetVal;
		}
		protected Frame.Video VideoFrameGet()
		{
            #region beem test  
            //   тест рассинхрона
            //if (!bTEST)
            //{
            //    bTEST = true;
            //    dtTEST = DateTime.Now.AddSeconds(15);
            //}
            //if (dtTEST < DateTime.Now)
            //{
            //    dtTEST = DateTime.MaxValue;
            //    nTEST = 7;
            //}
            //if (nTEST-- > 0)
            //{
            //    Frame cFrameT = NextFrame();
            //    _aq__PROBA__AudioFrames.Enqueue(cFrameT.cAudio);
            //    FrameBufferReleased(cFrameT.cVideo); //  возврат в конвейер
            //    (new Logger("device", sName)).WriteDebug("TEST BEEM. VIDEO FRAME DROPPED. [remain=" + nTEST + "]");
            //}
            #endregion






            // пока не разобрались с набегающим рассинхроном - повторяем видео-кадрик!  // набегает на некоторых карточках (BMD), если подана синхра!  // без синхры оочень редко - раз в несколько месяцев и не на всех карточках
            if (1 < _aq__PROBA__AudioFrames.Count)  // 1  т.к. аудио берется первое и тут уже на 1 меньше
            {
                if (100 < BalanceBeemTimeCounter_v)
                {
                    BalanceBeemTimeCounter_v = 1;
                    _cFrameVideoLast.nReferences++;
                    (new Logger("device", sName)).WriteWarning("BALANCE-BEEM. SYNC CORRECTED BY REPEATING __VIDEO__ FRAME! [video_frame_id=" + _cFrameVideoLast.nID + "][av=(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")][ticks=" + DateTime.Now.Ticks + "]");
                    return _cFrameVideoLast;
                }
                else if (0 == BalanceBeemTimeCounter_v)
                    (new Logger("device", sName)).WriteDebug3("balance-beem-2 started [ticks=" + DateTime.Now.Ticks + "]");
                BalanceBeemTimeCounter_v++;
            }
            else if (0 < BalanceBeemTimeCounter_v)
            {
                BalanceBeemTimeCounter_v = 0;
                (new Logger("device", sName)).WriteDebug3("balance-beem-2 ended [ticks=" + DateTime.Now.Ticks + "]");
            }












            Frame.Video cRetVal;
            if (0 < _aq__PROBA__VideoFrames.Count)
			{
				cRetVal = _aq__PROBA__VideoFrames.Dequeue();	
				_cBugCatcherOnVideoFrameReturn.Enqueue(cRetVal, "first return:_aq__PROBA__VideoFrames:" + _aq__PROBA__VideoFrames.Count + ":_aq__PROBA__AudioFrames:" + _aq__PROBA__AudioFrames.Count);  // bug
				//(new Logger("device", sName)).WriteDebug2("VideoFrameGet return from q  [id=" + cRetVal.nID + "]");
				return cRetVal;
			}
			cRetVal = _cFrameVideoLast;
			Frame cFrame = NextFrame();
			_cBugCatcherOnFrameGet.Enqueue(cFrame, "video recieve:_aq__PROBA__VideoFrames:" + _aq__PROBA__VideoFrames.Count + ":_aq__PROBA__AudioFrames:" + _aq__PROBA__AudioFrames.Count);
            if (null != cFrame && null != cFrame.cVideo)
            {
				if (null == cFrame.cVideo.oFrameBytes)
				{
					_cFrameVideoLast.nReferences++;
					(new Logger("device", sName)).WriteDebug2("BYTES FROM BTL IS NULL 2 - repeat the last [id=" + _cFrameVideoLast.nID + "][ref=" + _cFrameVideoLast.nReferences + "]");
				}
				else if (cFrame.cVideo.oFrameBytes is Bytes && 1 > cFrame.cVideo.aFrameBytes.Length)
					cRetVal = _cFrameVideoLast = _cVideoFrameEmpty; //получили признак необходимости очистить экран
				else
					cRetVal = _cFrameVideoLast = cFrame.cVideo;

				if (null != cFrame.cAudio && null != cFrame.cAudio.aFrameBytes)
					_aq__PROBA__AudioFrames.Enqueue(cFrame.cAudio);
				else
					_aq__PROBA__AudioFrames.Enqueue(_cFrameAudioEmpty);
			}
			else
			{
				_cFrameVideoLast.nReferences++;
				(new Logger("device", sName)).WriteDebug2("FRAME FROM BTL IS NULL 2 - repeat the last [id=" + _cFrameVideoLast.nID + "][ref=" + _cFrameVideoLast.nReferences + "]");
				_aq__PROBA__AudioFrames.Enqueue(_cFrameAudioEmpty);
			}
			_cBugCatcherOnVideoFrameReturn.Enqueue(cRetVal, "last return:_aq__PROBA__VideoFrames:" + _aq__PROBA__VideoFrames.Count + ":_aq__PROBA__AudioFrames:" + _aq__PROBA__AudioFrames.Count);  // bug

            //(new Logger("device", sName)).WriteDebug2("VideoFrameGet return end  [id=" + cRetVal.nID + "]");
            return cRetVal;
		}
		abstract protected bool FrameSchedule();

		protected void OnAVFrameArrived(int nBytesVideoQty, IntPtr pBytesVideo, int nBytesAudioQty, IntPtr pBytesAudio)
		{
			if(null != AVFrameArrived)
				AVFrameArrived(nBytesVideoQty, pBytesVideo, nBytesAudioQty, pBytesAudio);
		}

		event AVFrameArrivedCallback IDevice.AVFrameArrived
		{
			add
			{
                lock (AVFrameArrived)
                {
                    AVFrameArrived += value;
                }
			}
			remove
			{
				lock (AVFrameArrived)
					AVFrameArrived -= value;
			}
		}
		event NextFrameCallback IDevice.NextVideoFrame
		{
			add
			{
				lock (NextFrame)
					NextFrame += value;
			}
			remove
			{
				lock (NextFrame)
					NextFrame -= value;
			}
		}
		void IDevice.TurnOn()
		{
			this.TurnOn();
		}
		Frame.Video IDevice.FrameBufferGet()
		{
			return this.FrameBufferGet();
		}

        private System.Threading.Thread _cThreadWritingFramesWorker;
        private bool _bDoWritingFrames;
        private Queue<byte[]> _aqWritingFrames;
        private void WritingFramesWorker(object cState)
        {
            (new Logger("DeckLink", sName)).WriteNotice("DECKLINK.WritingFramesWorker: started");

            string _sWritingFramesFile = System.IO.Path.Combine(Preferences.sDebugFolder, "WritingDebugFrames.txt");
            string _sWritingFramesDir = System.IO.Path.Combine(Preferences.sDebugFolder, "DECKLINK/");
            int _nFramesCount = 0;
            System.Drawing.Bitmap cBFrame;
            System.Drawing.Imaging.BitmapData cFrameBD;
            string[] aLines;
            bool bQueueIsNotEmpty = false;
            byte[] aBytes;

            while (true)
            {
                try
                {
                    if (System.IO.File.Exists(_sWritingFramesFile))
                    {
                        aLines = System.IO.File.ReadAllLines(_sWritingFramesFile);
                        if ("decklink" == aLines.FirstOrDefault(o => o.ToLower() == "decklink"))
                        {
                            _bDoWritingFrames = true;
                            if (!System.IO.Directory.Exists(_sWritingFramesDir))
                                System.IO.Directory.CreateDirectory(_sWritingFramesDir);
                        }
                        else
                            _bDoWritingFrames = false;
                    }
                    else
                        _bDoWritingFrames = false;

                    if (_bDoWritingFrames || 0 < _aqWritingFrames.Count)
                    {
                        while (bQueueIsNotEmpty)
                        {
                            cBFrame = new System.Drawing.Bitmap(stArea.nWidth, stArea.nHeight);
                            cFrameBD = cBFrame.LockBits(new System.Drawing.Rectangle(0, 0, cBFrame.Width, cBFrame.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            lock (_aqWritingFrames)
                            {
                                aBytes = _aqWritingFrames.Dequeue();
                                if (0 < _aqWritingFrames.Count)
                                    bQueueIsNotEmpty = true;
                                else
                                    bQueueIsNotEmpty = false;
                            }
                            System.Runtime.InteropServices.Marshal.Copy(aBytes, 0, cFrameBD.Scan0, aBytes.Length);
                            cBFrame.UnlockBits(cFrameBD);
                            cBFrame.Save(_sWritingFramesDir + "frame_" + _nFramesCount.ToString("0000") + ".png");
                            _nFramesCount++;

                            aLines = System.IO.File.ReadAllLines(_sWritingFramesFile);
                            if (null == aLines.FirstOrDefault(o => o.ToLower() == "decklink"))
                                _bDoWritingFrames = false;
                            if (3000 < _aqWritingFrames.Count)
                            {
                                _bDoWritingFrames = false;
                                System.IO.File.Delete(_sWritingFramesFile);
                            }
                        }
                        System.Threading.Thread.Sleep(40);
                        if (0 < _aqWritingFrames.Count)
                            bQueueIsNotEmpty = true;
                        else
                            bQueueIsNotEmpty = false;
                    }
                    else
                    {
                        lock (_aqWritingFrames)
                            if (0 == _aqWritingFrames.Count)
                                _nFramesCount = 0;
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                catch (System.Threading.ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    (new Logger("DeckLink", sName)).WriteError(ex);
                }
            }
        }

    }
}
