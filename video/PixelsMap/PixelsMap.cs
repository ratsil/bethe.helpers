//#if !PROMPTER
#define CUDA
//#endif
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace helpers
{

    public partial class PixelsMap
	{
        public class Triple
        {
            private PixelsMap[] aPMs;
            private byte _nCurrentPM;
            private byte _nPreviousPM;
            public List<PixelsMap> aAllUsedPMs
            {
                get
                {
                    List<PixelsMap> aRetVal = new List<PixelsMap>();
                    lock (_oLock)
                    {
                        for (int nI = 0; nI < aPMs.Length; nI++)
                            aRetVal.Add(aPMs[nI]);
                    }
                    return aRetVal;
                }
            }
            public PixelsMap cCurrent
            {
                get
                {
                    lock (_oLock)
                        return aPMs[_nCurrentPM];
                }
            }
            public PixelsMap cFirst
            {
                get
                {
                    lock (_oLock)
                        return aPMs[0];
                }
            }
            private object _oLock;
            private MergingMethod _stMergingMethod;
            private bool _bFirstTime;
            static private byte _nMaxTripleIndex = 3;

            static private List<PixelsMap> _aTESTPMDESTRUCTOR = new List<PixelsMap>();



            public Triple(MergingMethod stMergingMethod, Area stArea, Format ePixelFormat, bool bKeepAlive, AddToDisposeBTL dAddToDisposeBTL)
            {
                _dAddToDisposeBTL = dAddToDisposeBTL;
                _bFirstTime = true;
                _stMergingMethod = stMergingMethod;
                _oLock = new object();
                aPMs = new PixelsMap[_nMaxTripleIndex];
                for (int nI = 0; nI < aPMs.Length; nI++)
                {
                    aPMs[nI] = new PixelsMap(stMergingMethod, stArea, ePixelFormat);
                    aPMs[nI].bKeepAlive = bKeepAlive;
                    aPMs[nI]._nIndexTriple = (byte)nI;
                }
                _nCurrentPM = 0;
            }
            ~Triple()
            {
                //try
                //{
                //}
                //catch { };
            }
            public void SetAlphaConstant(byte nAlphaConstant)
            {
                for (int nI = 0; nI < aPMs.Length; nI++)
                {
                    aPMs[nI].nAlphaConstant = nAlphaConstant;
                }
            }
            public void SetAlpha(DisCom.Alpha eAlpha)
            {
                for (int nI = 0; nI < aPMs.Length; nI++)
                {
                    aPMs[nI].eAlpha = eAlpha;
                }
            }
            public delegate void AddToDisposeBTL(PixelsMap cPixelsMap, bool bForce);
            private AddToDisposeBTL _dAddToDisposeBTL;
            public void RenewFirstTime()
            {
                _bFirstTime = true;
            }
            // если не CUDA, то можно просто переключать ПМ в цикле, но с кудой надо быть в синхре, но только в случае, если мы работаем на БТЛ, а не в память набираем (в ролле)
            public PixelsMap Switch(byte nMustUseThisTripleIndex)
            {
                lock (_oLock)
                {
                    if (nMustUseThisTripleIndex < byte.MaxValue)
                        _nCurrentPM = nMustUseThisTripleIndex;
                    else if (cFirst.stMergingMethod.eDeviceType == MergingDevice.CUDA)
                    {
                        (new Logger()).WriteError("merging method is CUDA, but sync index not provided! [id=" + cFirst.nID + "][area=" + cFirst.stArea.ToString() + "]");
                        return null;
                    }
                    else
                        _nCurrentPM = GetNextIndex(_nCurrentPM);

                    if (!_bFirstTime)
                    {
                        // была мысль, что нельзя давать не по порядку, т.к. этот эффект переложили значит из рола в рол и т.п. (на чате такого много)
                        // но это я на воду дул - на самом деле попробуем позволять, т.к. ну переложили - эффект просто потерял очередь, а не обогнал её...
                        // вообщем, если траблы - раскомментить надо только тут - остальное уже прописано как надо
                        //if (GetNextIndex(_nPreviousPM) != _nCurrentPM)
                        //{
                        //    (new Logger()).WriteWarning("next triple is wrong! [id=" + cFirst.nID + "][prev=" + _nPreviousPM + "][cur=" + _nCurrentPM + "][area=" + cFirst.stArea.ToString() + "]");
                        //    return null;
                        //}
                    }
                    else
                        _bFirstTime = false;

                    _nPreviousPM = _nCurrentPM;

                    return aPMs[_nCurrentPM];
                }
            }
            static public byte GetNextIndex(byte nIndx)
            {
                nIndx++;
                if (nIndx >= _nMaxTripleIndex)
                    nIndx = 0;
                return nIndx;
            }
            public void Allocate()
            {
                for (int nI = 0; nI < aPMs.Length; nI++)
                {
                    aPMs[nI].Allocate();
                }
            }
            //public void Dispose(bool bDisposeKeepAlives)
            //{
            //    for (int nI = 0; nI < aPMs.Length; nI++)
            //    {
            //        aPMs[nI].Dispose(bDisposeKeepAlives);
            //    }
            //}
        }
        public enum Format
		{
			RGB24 = 2400,
			BGR24 = -2400,
			ARGB32 = 3200,  //1b
			BGRA32 = -3200,
			YUV420P = 1200,
			BPP8 = 800 //1b
        }
        public class Command
        {
            public enum ID
            {
                Unknown = 0,
                Allocate = 1,
                CopyIn = 2,
                CopyOut = 3,
                Merge = 4,
                Dispose = 5,
            }
            public ID eID;
            public System.Threading.ManualResetEvent cMRE;
            public Dictionary<Type, object> ahParameters;
            public PixelsMap cPM;

            public Command(ID eID, PixelsMap cPM)
            {
                try
                {
                    this.eID = eID;
                    this.cPM = cPM;
                    ahParameters = new Dictionary<Type, object>();
                    cMRE = new System.Threading.ManualResetEvent(false);
                }
                catch (Exception ex)
                {
                    (new Logger("CUDA")).WriteError(ex);
                }
            }
        }
        static private Dictionary<int, ThreadBufferQueue<Command>> _ahMergingHash_CommandsQueue = new Dictionary<int, ThreadBufferQueue<Command>>();
        static private long _nCurrentID = 1;

		private DateTime _dtCreate;
		private bool _bTemp;
		private Bytes __aBytes;
		private Bytes _aBytes
		{
			get { return __aBytes; }
			set
			{
				lock (_cBinM)
                {
                    if (__aBytes?.nID > 0)
                    {
                        if (_cBinM.Contains(__aBytes))
                            _cBinM.BytesBack(__aBytes, 1);
                        else
                            (new Logger()).WriteWarning("pixelmap recovering __abytes: not our member [len=" + ("" + __aBytes?.Length ?? "NULL") + "]");
                    }
                    __aBytes = value;
				}
			}
		}
		private Format _ePixelFormat;
		private Area _stArea;
        private byte _nIndexTriple;
        static private int _nIndexTripleMax;
        static private byte _nIndexTripleCurrent;
        static private BytesInMemory _cBinM;
        public byte nIndexTriple
        {
            get
            {
                return _nIndexTriple;
            }
            set
            {
                _nIndexTriple = value;
            }
        }
        static public byte nIndexTripleCurrent
        {
            get
            {
                return _nIndexTripleCurrent;
            }
        }

        private long _nID;
		private bool _bProcessing;
		private bool _bDisposed;
		private object _cSyncRoot;
		private Exception _cException;
		private uint _nBytesQty;
		private System.Drawing.PointF _stPosition;
		private System.Drawing.PointF _stHalfPosition;
		private float _nShiftTotalX;
		private float _nShiftTotalY;
		private Dock.Offset _cShiftOffset;

  //      private static Dictionary<int, Queue<byte[]>> _ahBytesStorage; //DNF не забыть если удачно, то чистить его периодически!!!!
		//private static List<int> _aBytesHashes;
		//private static int nNumSizes = 0, nNumTotal = 0;
		//private static long nBytesTotal = 0;
  //      private static DateTime _dtNextInfo = DateTime.Now.AddMinutes(5);
  //      private static byte[] aRetVal;
		//private static byte[] BytesGet(int nSize, byte nFrom)
		//{
		//	lock (_ahBytesStorage)
		//	{
		//		if (_ahBytesStorage.Keys.Contains(nSize) && 0 < _ahBytesStorage[nSize].Count)
		//			return _ahBytesStorage[nSize].Dequeue();
		//		else
		//		{
		//			if (!_ahBytesStorage.Keys.Contains(nSize))
		//			{
		//				nNumSizes++;
		//				(new Logger()).WriteDebug("pixelmap: adding new size to bytes storage [" + nSize + "] (from=" + nFrom + ")");
		//				_ahBytesStorage.Add(nSize, new Queue<byte[]>());
		//			}
		//			nBytesTotal += nSize;
		//			nNumTotal++;
		//			aRetVal = new byte[nSize];
		//			while (_aBytesHashes.Contains(aRetVal.GetHashCode()))
		//			{
		//				(new Logger()).WriteDebug("pixelmap.bytes ERROR returning new byte array WITH THE SAME HASH!!! - will try to get another one (from=" + nFrom + ")[hc=" + aRetVal.GetHashCode() + "][" + nSize + "]");
		//				aRetVal = new byte[nSize];
		//			}
  //                  if (DateTime.Now > _dtNextInfo)
  //                  {
  //                      (new Logger()).WriteDebug("pixelmap.bytes info: [sizes=" + nNumSizes + "][total_count=" + nNumTotal + "][bytes=" + nBytesTotal + "]");
  //                      _dtNextInfo = DateTime.Now.AddMinutes(5);
  //                  }
  //                  (new Logger()).WriteDebug4("pixelmap.bytes: returning new byte [from=" + nFrom + "] [hc=" + aRetVal.GetHashCode() + "][" + nSize + "][sizes=" + nNumSizes + "][total=" + nNumTotal + "(" + _aBytesHashes.Count() + ")][bytes=" + nBytesTotal + "]");
		//			_aBytesHashes.Add(aRetVal.GetHashCode());
		//			return aRetVal;
		//		}
		//	}
		//}

		public Area stArea
		{
			get
			{
				return _stArea;
			}
		}
		public long nID
		{
			get
			{
				return _nID;
			}
		}
        public byte nAlphaConstant;
		public bool bKeepAlive;
        public bool bDisposed
        {
            get
            {
                return _bDisposed;
            }
        }
        public DisCom.Alpha eAlpha;
        public Format ePixelFormat
        {
            get
            {
                return _ePixelFormat;
            }
        }
        public int nLength
		{
			get
			{
				return (int)_nBytesQty;
			}
		}
        public MergingMethod stMergingMethod { get; private set; }

        static PixelsMap()
        {
            _cBinM = new BytesInMemory("pixels_map bytes");
        }
        public PixelsMap(MergingMethod stMergingMethod, Area stArea, Format ePixelFormat)
            :this(stMergingMethod, stArea, ePixelFormat, false)
        { }
        public PixelsMap(MergingMethod stMergingMethod, Area stArea, Format ePixelFormat, bool bIsBTLBackground)
        {
            this.stMergingMethod = stMergingMethod;
            if (bIsBTLBackground)
            {
                if (stMergingMethod.eDeviceType == MergingDevice.CUDA)
                {
                    _nIndexTriple = (byte)_nIndexTripleMax;
                    System.Threading.Interlocked.Increment(ref _nIndexTripleMax);
                }
                else
                {
                    _nIndexTriple = byte.MaxValue;
                    _nIndexTripleCurrent = byte.MaxValue;
                }
                (new Logger()).WriteDebug("background pixelmap created [tripindex=" + _nIndexTriple + "]");
            }
            if (!_ahMergingHash_CommandsQueue.ContainsKey(stMergingMethod.nHash))
            {
                _ahMergingHash_CommandsQueue.Add(stMergingMethod.nHash, new ThreadBufferQueue<Command>(0, false));
                System.Threading.Thread cThreadWorker = null;
                if (stMergingMethod.eDeviceType == MergingDevice.CUDA)
                {
                    if (1 > Preferences.nCUDAVersion)
                        throw new Exception("There is no CUDA version in preferences");
                    //cThreadWorker = new System.Threading.Thread(() => CUDAWorker(stMergingMethod.nHash));
                    new CUDAWorkers(stMergingMethod.nHash);
                }
                else if (stMergingMethod.eDeviceType == MergingDevice.DisComExternal)
                {
                    //cThreadWorker = new System.Threading.Thread(() => DisComExternalWorker(stMergingMethod.nHash));
                    new DisComExternalWorkers(stMergingMethod.nHash);
                }
                if (null != cThreadWorker)
                {
                    cThreadWorker.IsBackground = true;
                    cThreadWorker.Priority = System.Threading.ThreadPriority.AboveNormal;
                    cThreadWorker.Start();
                }
            }
            _dtCreate = DateTime.Now;
            _bTemp = false;
            _bProcessing = false;
            _bDisposed = false;
            _nID = 0;
			nAlphaConstant = byte.MaxValue;
			bKeepAlive = false;
			_cException = null;
			_ePixelFormat = ePixelFormat;
			_stArea = stArea;
			_nBytesQty = (uint)(_stArea.nWidth * _stArea.nHeight * Math.Abs((int)((int)ePixelFormat / 100)) / 8);
			_cSyncRoot = new object();
			_cTiming = new helpers.Logger.Timings("pixelsmap");
            _cTiming.TurnOff();
        }
		~PixelsMap()
		{
			try
			{
                _bProcessing = false;
                Dispose(true);
			}
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
        }
		public void Dispose()
		{
			Dispose(false);
		}
		public void Dispose(bool bDisposeKeepAlives)
		{
			if (1 > _nID || (!bDisposeKeepAlives && bKeepAlive))
				return;

			lock (_cSyncRoot)
			{
				if (_bDisposed)
					return;
				_bDisposed = true;
			}

			lock (_cSyncRoot)
			{
				if (_bProcessing)  // i.e. not from destructor
				{
					bKeepAlive = false;
                    _bDisposed = false;
                    return;
				}
            }
            if (stMergingMethod.eDeviceType > 0)
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(new Command(Command.ID.Dispose, this));

            if (null != _aBytes && 0 < _aBytes.Length)
				(new Logger()).WriteDebug4("pixelmap disposed [bytes=" + _aBytes.Length + "]");

			_aBytes = null;
		}
        public void MakeCurrent()
        {
            _nIndexTripleCurrent = _nIndexTriple;
        }
		public void Allocate()
		{
			(new Logger()).WriteDebug3("pixelmap allocate [current_id=" + _nCurrentID + "]");
            if (1 > nLength)
                throw new Exception("attempt to alloc [" + nLength + "] bytes");

#if CUDA
            if (stMergingMethod.eDeviceType > 0)
            {
                Command cCmd = new Command(Command.ID.Allocate, this);
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				return;
			}
#endif
			if (null != _aBytes)
				throw new Exception("Bytes storage already initialized");
			_nID = _nCurrentID++;
			_aBytes =_cBinM.BytesGet((int)_nBytesQty, 1);
		}
		public void CopyIn(IntPtr pSource, int nBytesQty)
		{
			CopyIn(pSource, (uint)nBytesQty);
		}
		public void CopyIn(IntPtr pSource, uint nBytesQty)
		{
			//(new Logger()).WriteDebug2("pixelmap copyin(ptr) cuda=" + bCUDA + "[bytes_qty=" + _nBytesQty + "]");
			if (1 > nBytesQty)
				throw new Exception("bytes qty must be more than zero");
#if CUDA
            if (stMergingMethod.eDeviceType > 0)
            {
				Command cCmd = new Command(Command.ID.CopyIn, this);
				cCmd.ahParameters.Add(typeof(IntPtr), pSource);
				_nBytesQty = nBytesQty;
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				if (null != _cException)
					throw _cException;
				if (_nID == 0)
					(new Logger()).WriteError(new Exception("got 0 in id after copy in"));
				return;
			}
#endif
			Logger.Timings cTimings = new Logger.Timings("btl:pixelsmap:copyin:");
			if (null == _aBytes)
				Allocate();
			cTimings.Restart("allocate");
			Marshal.Copy(pSource, _aBytes.aBytes, 0, (int)nBytesQty);
			cTimings.Stop("copyin > 40ms", "marshal_copy", 40);
		}
		public void CopyIn(byte[] aSource)
		{
			CopyIn(aSource, false, false);  //   CopyIn(aSource, true)   так нельзя теперь, т.к. хэши могут случайно совпасть у разных массивов. Можно только если уверены, что мы даём здешний массив на вход.
		}
		public void CopyIn(byte[] aSource, bool bFreeBytes)
		{
			throw new Exception("before using this copyin do bFreeBytes=true variant");
			return;
			CopyIn(aSource, bFreeBytes, false);
		}
		public void CopyIn(byte[] aSource, bool bFreeBytes, bool bUseTheseBytesIfPossible)
		{
            //(new Logger()).WriteDebug2("pixelmap copyin(byte[]) cuda=" + bCUDA + "[bytes_qty=" + _nBytesQty + "]");
#if CUDA
            if (stMergingMethod.eDeviceType > 0)
            {
				Command cCmd = new Command(Command.ID.CopyIn, this);
				cCmd.ahParameters.Add(typeof(byte[]), aSource);
				_nBytesQty = (uint)aSource.Length;
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				if (null != _cException)
					throw _cException;
				//if (bFreeBytes)
				//	lock (_ahBytesStorage)
				//	{
				//		if (_aBytesHashes.Contains(aSource.GetHashCode()))
				//			_ahBytesStorage[aSource.Length].Enqueue(aSource);
				//	}
				return;
			}
#endif
			if (!bFreeBytes && bUseTheseBytesIfPossible)  // т.е. "если возможно" - это если не куда ну и освобождения массива не должно быть
            {
                _aBytes = new Bytes() { nID = -1, aBytes = aSource };
			}
			else
			{
				if (null == _aBytes)
					Allocate();
				aSource.CopyTo(_aBytes.aBytes, 0);
				//if (bFreeBytes)
				//	lock (_ahBytesStorage)
				//	{
				//		if (_aBytesHashes.Contains(aSource.GetHashCode()))
				//			_ahBytesStorage[aSource.Length].Enqueue(aSource);
				//	}
			}
		}
		public byte[] BytesReferenceGet()   // взятые так байты нельзя освобождать там, куда их взяли, например в copyin-е. 
		{
			return _aBytes.aBytes;
		}
		public void CopyOut(IntPtr pDestination)
		{
            //(new Logger()).WriteDebug2("pixelmap copyout(ptr) cuda=" + bCUDA + "[bytes_qty=" + _nBytesQty + "]");
#if CUDA
            if (stMergingMethod.eDeviceType > 0)
            {
				if (null != _cException)
					throw _cException;
				Command cCmd = new Command(Command.ID.CopyOut, this);
				cCmd.ahParameters.Add(typeof(IntPtr), pDestination);
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				if (null != _cException)
					throw _cException;
				return;
			}
#endif
			if (null == _aBytes)
				throw new Exception("Bytes storage doesn't initialized");
			Marshal.Copy(_aBytes.aBytes, 0, pDestination, _aBytes.Length);
		}
		public void CopyOut(byte[] aBytes)
		{
            //(new Logger()).WriteDebug2("pixelmap copyout(byte[]) cuda=" + bCUDA + "[bytes_qty=" + _nBytesQty + "]");
#if CUDA
            if (stMergingMethod.eDeviceType > 0)
            {
				Command cCmd = new Command(Command.ID.CopyOut, this);
				cCmd.ahParameters.Add(typeof(byte[]), aBytes);
                _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				__aBytes = null;
				return;
			}
#endif
			if (null == _aBytes)
				throw new Exception("bytes storage doesn't initialized");
			if (aBytes.Length != _aBytes.Length)
				(new Logger()).WriteWarning("wrong array size for copyout [got:" + aBytes.Length + "][expected:" + _aBytes.Length + "][qty:" + _nBytesQty + "]");
			_aBytes.aBytes.CopyTo(aBytes, 0);
		}
		public byte[] CopyOut()   // пока юзается только в паре с copyin(byte[])     // не юзается пока и не надо!! т.к. наружу наш массив отдаём местный....
		{
			throw new Exception("function CopyOut with 0 parameters was deprecated");
			return null;
		}

		Logger.Timings _cTiming;
		static public PixelsMap Merge(Area stBase, List<PixelsMap> aPMs)
		{
            if (
					1 == aPMs.Count && 
					0 == aPMs[0]._nShiftTotalX &&     // не надо поля делать
					stBase == aPMs[0].stArea &&
					byte.MaxValue == aPMs[0].nAlphaConstant &&
					aPMs[0]._stPosition.X == (float)Math.Floor(aPMs[0]._stPosition.X) &&     // надеюсь это редко будет вообще проверяться....
					aPMs[0]._stPosition.Y == (float)Math.Floor(aPMs[0]._stPosition.Y)
				)
				return aPMs[0];
#if CUDA
			PixelsMap cRetVal = new PixelsMap(aPMs[0].stMergingMethod, stBase, Format.ARGB32);
			cRetVal._bTemp = true;
			Command cCmd = null;
			if (aPMs[0].stMergingMethod.eDeviceType > 0)
			{
				//cRetVal._nBytesQty = (uint)(stBase.nWidth * stBase.nHeight * 4); //UNDONE нужно определять кол-во байт по ePixelFormat //FIXED
				cCmd = new Command(Command.ID.Allocate, cRetVal);
                _ahMergingHash_CommandsQueue[aPMs[0].stMergingMethod.nHash].Enqueue(cCmd);
			}
			else
				cRetVal.Allocate();
#else
            PixelsMap cRetVal = new PixelsMap(MergingDevice.DisCom, 0, stBase, Format.ARGB32);
            cRetVal._bTemp = true;
			cRetVal.Allocate();
#endif

			cRetVal.Merge(aPMs);
			return cRetVal;
		}
		public void Merge(List<PixelsMap> aPMs)
		{
			Merge(aPMs, false);
		}
		public void Merge(List<PixelsMap> aPMs, bool bHighPriority)
		{
            if (null == aPMs)
                throw new Exception("PixelsMap array is null");
			//ConsistencyCheck();
			if (0 < aPMs.Count)
            {
				_cTiming.TotalRenew();
				List<PixelsMap> aPMsActual = new List<PixelsMap>();
				DisCom.MergeInfo cMergeInfo = new DisCom.MergeInfo();
				List<DisCom.LayerInfo> aLayerInfos = new List<DisCom.LayerInfo>();
				DisCom.LayerInfo cLayerInfo;

				cMergeInfo.nBackgroundSize = _stArea.nWidth * _stArea.nHeight;
				cMergeInfo.nBackgroundWidth_4 = 4 * _stArea.nWidth;
				cMergeInfo.nBackgroundWidth = _stArea.nWidth;
				cMergeInfo.nBackgroundHight = _stArea.nHeight;
				cMergeInfo.nBackgroundAlphaType = (byte)eAlpha;

                if (aPMs[0].stArea== _stArea)
                {
                    cMergeInfo.bLayer1CopyToBackground = true;
                }

                for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                {
					lock (aPMs[nIndx]._cSyncRoot)
					{
						if (aPMs[nIndx]._bDisposed)
							continue;
						aPMs[nIndx]._bProcessing = true;
					}
					cLayerInfo = aPMs[nIndx].Intersect(new Area(0, 0, _stArea.nWidth, _stArea.nHeight)); //не stBase т.к. нужны относительные координаты, а не абсолютные
					if (0 > cLayerInfo.nCropBottomLineInBG)  //если пересечения FG и BG нет.....
					{
						aPMs[nIndx].Dispose();
						continue;
					}

					aPMsActual.Add(aPMs[nIndx]);
					cLayerInfo.nBytesQty = (int)aPMs[nIndx]._nBytesQty;
                    aLayerInfos.Add(cLayerInfo);
                }
				_cTiming.Restart("layer infos");
                if (0 < aPMsActual.Count)
                {
					cMergeInfo.nLayersQty = (ushort)(aLayerInfos.Count + 1);
					cMergeInfo.aLayerInfos = aLayerInfos.ToArray();
#if CUDA
                    if (stMergingMethod.eDeviceType > 0)
                    {
						Command cCmd = new Command(Command.ID.Merge, this);
						cCmd.ahParameters.Add(typeof(List<PixelsMap>), aPMsActual);
						cCmd.ahParameters.Add(typeof(DisCom.MergeInfo), cMergeInfo);
                        _ahMergingHash_CommandsQueue[stMergingMethod.nHash].Enqueue(cCmd);
						cCmd.cMRE.WaitOne();
						_cTiming.Stop("merge", stMergingMethod.ToString(), 40);
						return;
					}
#endif

					List<byte[]> aDPs = new List<byte[]>();

					if (1 > _nID)
						throw new Exception("background PixelsMap have to be allocated for Merge");

					aDPs.Add(_aBytes.aBytes);
					for (int nIndx = 0; nIndx < aPMsActual.Count; nIndx++)
						aDPs.Add(aPMsActual[nIndx]._aBytes.aBytes);
					DisCom cDisCom = new DisCom();

                    //DNF
                    //(new Logger()).WriteDebug2("CPU_MERGE [high=" + bHighPriority + "][aPMs=" + aPMs.Count + "][" + aPMs[0].stArea.nWidth + "x" + aPMs[0].stArea.nHeight + "--" + aPMs[0].stArea.nLeft + ":" + aPMs[0].stArea.nTop + "][bytes=" + aPMs[0]._nBytesQty + "]");

                    _cTiming.Restart("before CPUm");
                    cDisCom.FrameMerge(cMergeInfo, aDPs, bHighPriority);
					_cTiming.Restart("CPU merge");
					cDisCom.Dispose();

					for (int nIndx = 0; nIndx < aPMsActual.Count; nIndx++)
					{
						lock (aPMsActual[nIndx]._cSyncRoot)
							aPMsActual[nIndx]._bProcessing = false;
						aPMsActual[nIndx].Dispose();
					}
				}
				_cTiming.Stop("merge", "disposing", 40); 
			}
        }
        public static void DisComInit()
        {
            DisCom.Init(Preferences.nDisComThreadsQty);
        }

		public void Move(short nLeftNewPos, short nTopNewPos)
		{
			_stArea.nLeft = nLeftNewPos;
			_stArea.nTop = nTopNewPos;
		}
		public void Shift(System.Drawing.PointF stPosition, Dock.Offset cOffsetAbsolute, System.Drawing.PointF stPositionPrev)
		{
			_stPosition = stPosition;
//            _stPosition.Y = (float)Math.Floor(_stPosition.Y);//DNF
            _cShiftOffset = cOffsetAbsolute;  // если null то не делаем поля иначе - делаем
			if (null != _cShiftOffset)
			{
				_nShiftTotalX = stPosition.X - stPositionPrev.X;
				_nShiftTotalY = stPosition.Y - stPositionPrev.Y;
				_stHalfPosition = new System.Drawing.PointF(stPosition.X - _nShiftTotalX / 2, stPosition.Y - _nShiftTotalY / 2);
			}
			else
				_nShiftTotalX = 0;  // признак для куды, что не надо делать поля
		}
		private DisCom.LayerInfo Intersect(Area stBase)  //v: Даёт размерность, ширину, высоту кропа и относительные координаты его лев верх угла от бэка и от фора и ширину кропа.
		{                                         //в целях оптимизации убраны все проверки, т.к. они уже были в FramesMerge...
			DisCom.LayerInfo cRetVal = new DisCom.LayerInfo();
			Area stCrop = _stArea.CropOnBase(stBase);  // если кроп нулевой, то w == h == 0

			if (stCrop.nWidth == 0) //не пересеклись
			{
				cRetVal.nCropTopLineInBG = -1;
				cRetVal.nCropBottomLineInBG = -1;
            }
			else
			{
				cRetVal.nWidth_4 = 4 * stArea.nWidth;
				cRetVal.nCropWidth_4 = 4 * stCrop.nWidth;
				int nCropLeft = stCrop.nLeft - stBase.nLeft; //отступ кропа слева COL
				cRetVal.nCropLeft_4 = 4 * nCropLeft;
				int nBGIndxStart = (stCrop.nTop - stBase.nTop) * stBase.nWidth + nCropLeft; //BGIndxStart=COT*BW+COL;   начальный индекс по бг
																							//формула вычисления FI=BI+M*(FW-BW)-(FOT*FW+FOL); M=(int)(BI/BW);
				cRetVal.nAlphaConstant = nAlphaConstant; //11/ константная альфа
				cRetVal.nAlphaType = (byte)eAlpha;
				cRetVal.nTop = stArea.nTop;
				cRetVal.nLeft_4 = 4 * stArea.nLeft;
				cRetVal.nCropTopLineInBG = nBGIndxStart / stBase.nWidth;
				cRetVal.nCropBottomLineInBG = cRetVal.nCropTopLineInBG + stCrop.nHeight - 1;

				int nFloorX = (int)Math.Floor(_stPosition.X);
				int nFloorY = (int)Math.Floor(_stPosition.Y);
				float nShift = _stPosition.X - nFloorX;   // т.е. всегда теперь 0 <= S < 1  
                if ((cRetVal.nShiftPositionByteX = (byte)Math.Abs(255 * nShift)) == 255)
                    cRetVal.nShiftPositionByteX = 254;
                nShift = _stPosition.Y - nFloorY;
                if ((cRetVal.nShiftPositionByteY = (byte)Math.Abs(255 * nShift)) == 255)
                    cRetVal.nShiftPositionByteY = 254;

                    cRetVal.nShiftTotalX = _nShiftTotalX;
				if (0 < Math.Abs(_nShiftTotalX)) // то делаем поля
				{
					int nFloorHalfX = (int)Math.Floor(_stHalfPosition.X);
					int nFloorHalfY = (int)Math.Floor(_stHalfPosition.Y);
					nShift = _stHalfPosition.X - nFloorHalfX;
					cRetVal.nHalfPathShiftPositionByteX = nShift == 0 ? (byte)0 : (byte)Math.Abs(255 * nShift);
					nShift = _stHalfPosition.Y - nFloorHalfY;
					cRetVal.nHalfPathShiftPositionByteY = nShift == 0 ? (byte)0 : (byte)Math.Abs(255 * nShift);  // (для диагональных смещений с полями)
																												 //   знак узнаем из nDeltaPxX_4
					cRetVal.nHalfDeltaPxX_4 = (nFloorX - nFloorHalfX) * 4;
					cRetVal.nHalfDeltaPxY_4 = (nFloorY - nFloorHalfY) * 4;

					if (null != _cShiftOffset)
					{
						cRetVal.nOffsetTop = _cShiftOffset.nTop & 1;

						if (stBase.nTop > _stArea.nTop && ((stBase.nTop - _stArea.nTop) & 1) == 1)
							cRetVal.nOffsetTop = ~cRetVal.nOffsetTop;
					}
				}
			}
			return cRetVal;
		}
		static private byte GetPreCalcBGPixel(byte A, byte B, byte F)
		{
			int nB = (int)((float)(A * (F - B)) / 255 + B + 0.5);
			if (255 < nB)
			{
				nB = 255;
			}
			return (byte)nB;
		}
		static private byte[] GetPreCalcBGPixelsOnConstAandB(byte A, byte B)
		{
			byte[] aRetVal = new byte[256];
			for (int i = 0; 256 > i; i++)
			{
				aRetVal[i] = GetPreCalcBGPixel(A, B, (byte)i);
			}
			return aRetVal;
		}
	}
}
