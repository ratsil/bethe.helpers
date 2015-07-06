//#if !PROMPTER
#define CUDA
//#endif
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
#if CUDA
using GASS.CUDA;
using GASS.CUDA.Types;
#endif
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace helpers
{
	public partial class PixelsMap
	{
		public enum Format
		{
			RGB24 = 2400,
			BGR24 = -2400,
			ARGB32 = 3200,  //1b
			BGRA32 = -3200,
			YUV420P = 1200,
			BPP8 = 800 //1b
		}

		const byte PRECOMPUTED_INFO_PERIOD = 7;

		static private ThreadBufferQueue<Command> _aqCommands = null;
		static private ulong _nCurrentID = 1;

		static private bool _bMemoryStarvation = false;
		static private object _oMemoryStarvationSync = new object();
		static public bool bMemoryStarvation
		{
			get
			{
				lock (_oMemoryStarvationSync)
					return _bMemoryStarvation;
			}
			private set
			{
				bool bValue;
				lock (_oMemoryStarvationSync)
				{
					bValue = _bMemoryStarvation;
					_bMemoryStarvation = value;
				}
				if (value != bValue)
					(new Logger()).WriteWarning("CUDA memory starvation " + (value ? "starts" : "stops"));
			}
		}

		private DateTime _dt;
		private bool _bTemp;
		private byte[] _aBytes;
		private Format _ePixelFormat;
		private Area _stArea;
		//отладка
		private ulong _nID;
		private bool _bProcessing;
		private bool _bDisposed;
		private object _cSyncRoot;
		private Exception _cException;
		private uint _nBytesQty;
		private bool _bShiftVertical;
		private float _nShiftPosition;
		private float _nShiftTotal;
		private Dock.Offset _cShiftOffset;

		public Area stArea
		{
			get
			{
				return _stArea;
			}
		}
		public ulong nID
		{
			get
			{
				return _nID;
			}
		}
        public byte nAlphaConstant; //EMERGENCY:l в принципе, можно это как-то отражать в enum Alpha... тогда здесь освободится значение 255... если оно нам нужно, конечно
        public bool bBackgroundClear; //UNDONE не реализовано //EMERGENCY:l мне кажется это тот же bOpacity... а он уже реализован... так что может просто удалить, чтобы глаза не мозолил
		public bool bKeepAlive;
		public DisCom.Alpha eAlpha;
		public int nLength
		{
			get
			{
				return (int)_nBytesQty;
			}
		}
		public bool bCUDA { get; private set; }
#if CUDA
		static PixelsMap()
		{
			if (0 < Preferences.nCUDAVersion)
			{
				_aqCommands = new ThreadBufferQueue<Command>(0, false);
				System.Threading.ThreadPool.QueueUserWorkItem(Worker);
			}
		}
		public PixelsMap()
			: this(true, new Area(0, 0, 1, 1), Format.BGRA32)
		{
		}
		public PixelsMap(Area stArea, Format ePixelFormat)
			: this(true, stArea, ePixelFormat)
		{
		}
		public PixelsMap(bool bIsCUDA, Area stArea, Format ePixelFormat)
		{
			_dt = DateTime.Now;
			_bTemp = false;
			_bProcessing = false;
			_bDisposed = false;
			_cSyncRoot = new object();
			if (bCUDA = bIsCUDA && 1 > Preferences.nCUDAVersion)
				throw new Exception("There is no CUDA version in preferences");
#else
		public PixelsMap()
			:this(false, new Area(0, 0, 1, 1), Format.BGRA32)
		{
		}
		public PixelsMap(bool bCUDA, Area stArea, Format ePixelFormat)
		{
#endif
			_nID = 0;
			nAlphaConstant = byte.MaxValue;
			bBackgroundClear = false;
			bKeepAlive = false;
			_cException = null;
			_nBytesQty = 0;
			_ePixelFormat = ePixelFormat;
			_stArea = stArea;
			_nBytesQty = (uint)(_stArea.nWidth * _stArea.nHeight * Math.Abs((int)((int)ePixelFormat / 100)) / 8);
			_nShiftPosition = 0;
			_cSyncRoot = new object();
		}
		~PixelsMap()
		{
			try
			{
				Dispose(true);
			}
			catch { }
		}
		public void Dispose()
		{
			Dispose(false);
		}
		public void Dispose(bool bDisposeKeepAlives)
		{
			if (1 > _nID || (!bDisposeKeepAlives && bKeepAlive))
				return;
#if CUDA
			lock (_cSyncRoot)
			{
				_bDisposed = true;
				if (_bProcessing)
				{
					bKeepAlive = false;
					return;
				}
			}
			if (bCUDA)
				_aqCommands.Enqueue(new Command(Command.ID.Dispose, this));
#endif
			_aBytes = null;
		}

		public void Allocate()
		{
#if CUDA
			if (bCUDA)
			{
				Command cCmd = new Command(Command.ID.Allocate, this);
				if (1 > nLength)
					(new Logger()).WriteNotice("1 > nLength. pixelsmap.allocate");
				_aqCommands.Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				return;
			}
#endif
			if (null != _aBytes)
				throw new Exception("Bytes storage already initialized");
			_nID = _nCurrentID++;
			_aBytes = new byte[_nBytesQty];
		}
		public void CopyIn(IntPtr pSource, int nBytesQty)
		{
			CopyIn(pSource, (uint)nBytesQty);
		}
		public void CopyIn(IntPtr pSource, uint nBytesQty)
		{
			if (1 > nBytesQty)
				throw new Exception("bytes qty must be more than zero");
#if CUDA
			if (bCUDA)
			{
				Command cCmd = new Command(Command.ID.CopyIn, this);
				cCmd.ahParameters.Add(typeof(IntPtr), pSource);
				_nBytesQty = nBytesQty;
				_aqCommands.Enqueue(cCmd);
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
			Marshal.Copy(pSource, _aBytes, 0, (int)nBytesQty);
			cTimings.Stop("copyin > 40ms", "marshal_copy", 40);
		}
		public void CopyIn(byte[] aSource)
		{
#if CUDA
			if (bCUDA)
			{
				Command cCmd = new Command(Command.ID.CopyIn, this);
				cCmd.ahParameters.Add(typeof(byte[]), aSource);
				_nBytesQty = (uint)aSource.Length;
				_aqCommands.Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				if (null != _cException)
					throw _cException;
				return;
			}
#endif
			if (null == _aBytes)
				Allocate();
			aSource.CopyTo(_aBytes, 0);
		}
		public void CopyOut(IntPtr pDestination)
		{
#if CUDA
			if (bCUDA)
			{
				if (null != _cException)
					throw _cException;
				Command cCmd = new Command(Command.ID.CopyOut, this);
				cCmd.ahParameters.Add(typeof(IntPtr), pDestination);
				_aqCommands.Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				if (null != _cException)
					throw _cException;
				return;
			}
#endif
			if (null == _aBytes)
				throw new Exception("Bytes storage doesn't initialized");
			Marshal.Copy(_aBytes, 0, pDestination, _aBytes.Length);
		}
		public void CopyOut(byte[] aBytes)
		{
#if CUDA
			if (bCUDA)
			{
				Command cCmd = new Command(Command.ID.CopyOut, this);
				cCmd.ahParameters.Add(typeof(byte[]), aBytes);
				_aqCommands.Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				return;
			}
#endif
			if (null == _aBytes)
				throw new Exception("bytes storage doesn't initialized");
			if(aBytes.Length != _aBytes.Length)
				(new Logger()).WriteWarning("wrong array size for copyout [got:" + aBytes.Length + "][expected:" + _aBytes.Length + "]");
			_aBytes.CopyTo(aBytes, 0);
		}
		public byte[] CopyOut()
		{
#if CUDA
			if (bCUDA)
			{
				Command cCmd = new Command(Command.ID.CopyOut, this);
				_aqCommands.Enqueue(cCmd);
				cCmd.cMRE.WaitOne();
				return _aBytes;
			}
#endif
			if (null == _aBytes)
				throw new Exception("bytes storage doesn't initialized");
			byte[] aRetVal = new byte[_aBytes.Length];
			_aBytes.CopyTo(aRetVal, 0);
			return aRetVal;
		}

		static public PixelsMap Merge(Area stBase, List<PixelsMap> aPMs)
		{
            if (1 == aPMs.Count && 0 == aPMs[0]._nShiftPosition && stBase == aPMs[0].stArea && byte.MaxValue == aPMs[0].nAlphaConstant)
				return aPMs[0];
#if CUDA
			PixelsMap cRetVal = new PixelsMap(aPMs[0].bCUDA, stBase, Format.ARGB32);
			cRetVal._bTemp = true;
			Command cCmd = null;
			if (aPMs[0].bCUDA)
			{
				//cRetVal._nBytesQty = (uint)(stBase.nWidth * stBase.nHeight * 4); //UNDONE нужно определять кол-во байт по ePixelFormat //FIXED
				cCmd = new Command(Command.ID.Allocate, cRetVal);
				_aqCommands.Enqueue(cCmd);
			}
			else
				cRetVal.Allocate();
#else
			PixelsMap cRetVal = new PixelsMap(false, stBase, Format.ARGB32);
			cRetVal._bTemp = true;
			cRetVal.Allocate();
#endif

			cRetVal.Merge(aPMs);
			return cRetVal;
		}
		public void Merge(List<PixelsMap> aPMs)
		{
            if (null == aPMs)
                throw new Exception("PixelsMap array is null");
            
            //ConsistencyCheck();
            if (0 < aPMs.Count)
            {
                List<PixelsMap> aPMsActual = new List<PixelsMap>();
				DisCom.MergeInfo cMergeInfo = new DisCom.MergeInfo();
				List<DisCom.LayerInfo> aLayerInfos = new List<DisCom.LayerInfo>();
				DisCom.LayerInfo cLayerInfo;

				cMergeInfo.nBackgroundSize = _stArea.nWidth * _stArea.nHeight;
				cMergeInfo.nBackgroundWidth = _stArea.nWidth;
				cMergeInfo.nBackgroundHight = _stArea.nHeight;
				cMergeInfo.nBackgroundAlphaType = (byte)eAlpha;

                for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                {
					lock (aPMs[nIndx]._cSyncRoot)
					{
						if (aPMs[nIndx]._bDisposed)
							continue;
						aPMs[nIndx]._bProcessing = true;
					}
					cLayerInfo = aPMs[nIndx].Intersect(new Area(0, 0, _stArea.nWidth, _stArea.nHeight)); //не stBase т.к. нужны относительные координаты, а не абсолютные
					if (0 > (cLayerInfo.nBackgroundStop - cLayerInfo.nBackgroundStart))  //если пересечения FG и BG нет.....
					{
						aPMs[nIndx].Dispose();
						continue;
					}
                    aPMsActual.Add(aPMs[nIndx]);
					aLayerInfos.Add(cLayerInfo);
                }

                if (0 < aPMsActual.Count)
                {
					cMergeInfo.nLayersQty = (ushort)(aLayerInfos.Count + 1);
					cMergeInfo.aLayerInfos = aLayerInfos.ToArray();
#if CUDA
					if (bCUDA)
					{
						Command cCmd = new Command(Command.ID.Merge, this);
						cCmd.ahParameters.Add(typeof(List<PixelsMap>), aPMsActual);
						cCmd.ahParameters.Add(typeof(DisCom.MergeInfo), cMergeInfo);
						_aqCommands.Enqueue(cCmd);
						cCmd.cMRE.WaitOne();
						return;
					}
#endif

					List<byte[]> aDPs = new List<byte[]>();

					if (1 > _nID)
						throw new Exception("background PixelsMap have to be allocated for Merge");

					aDPs.Add(_aBytes);
					for (int nIndx = 0; nIndx < aPMsActual.Count; nIndx++)
						aDPs.Add(aPMsActual[nIndx]._aBytes);
					DisCom cDisCom = new DisCom();
					Logger.Timings cTiming = new helpers.Logger.Timings("BTL:pixelmap:");
					cDisCom.FrameMerge(cMergeInfo, aDPs);
					cTiming.Stop("merge > 35", 35);
					cDisCom.Dispose();

					for (int nIndx = 0; nIndx < aPMsActual.Count; nIndx++)
					{
						lock (aPMsActual[nIndx]._cSyncRoot)
							aPMsActual[nIndx]._bProcessing = false;
						aPMsActual[nIndx].Dispose();
					}
				}
            }
        }

		public void Move(short nLeftNewPos, short nTopNewPos)
		{
			_stArea.nLeft = nLeftNewPos;
			_stArea.nTop = nTopNewPos;
		}
		public void Shift(bool bVertical, float nPosition, Dock.Offset cOffsetAbsolute, float nShiftTotal)
		{
			_bShiftVertical = bVertical;
			_nShiftPosition = nPosition;
			_cShiftOffset = cOffsetAbsolute;
			_nShiftTotal = nShiftTotal;
		}
		private DisCom.LayerInfo Intersect(Area stBase)  //v: Даёт размерность, ширину, высоту кропа и относительные координаты его лев верх угла от бэка и от фора и ширину кропа.
		{                                         //в целях оптимизации убраны все проверки, т.к. они уже были в FramesMerge...
			DisCom.LayerInfo cRetVal = new DisCom.LayerInfo();
			cRetVal.nShiftPosition = _nShiftPosition;
			cRetVal.bShiftVertical = _bShiftVertical;
			Area stCrop = _stArea.CropOnBase(stBase);
			cRetVal.nWidth = stArea.nWidth;
			cRetVal.nCropWidth = stCrop.nWidth;
			cRetVal.nCropHeight = stCrop.nHeight;
			cRetVal.nCropLeft = stCrop.nLeft - stBase.nLeft; //отступ кропа слева COL
			int nBGIndxStart = (stCrop.nTop - stBase.nTop) * stBase.nWidth + cRetVal.nCropLeft; //BGIndxStart=COT*BW+COL;
			//формула вычисления FI=BI+M*(FW-BW)-(FOT*FW+FOL); M=(int)(BI/BW);
			cRetVal.nWidthDiff = _stArea.nWidth - stBase.nWidth; //5/ константа 1 - (FW-BW) 
			cRetVal.nForegroundStart = (_stArea.nTop - stBase.nTop) * _stArea.nWidth + _stArea.nLeft - stBase.nLeft; //6/ константа 2 - (FOT*FW+FOL)
			cRetVal.nBackgroundStart = nBGIndxStart; //7/  начальный индекс по BG
			cRetVal.nBackgroundStop = nBGIndxStart + (stCrop.nHeight - 1) * stBase.nWidth + stCrop.nWidth - 1;//8/ конечный индекс по BG
			cRetVal.nCropRight = cRetVal.nCropLeft + stCrop.nWidth - 1; //10/ константа COL+CW-1
			cRetVal.nAlphaConstant = nAlphaConstant; //11/ константная альфа
            cRetVal.nAlphaType = (byte)eAlpha;
			cRetVal.nShiftTotal = _nShiftTotal;
			cRetVal.nTop = stArea.nTop;
			cRetVal.nLeft = stArea.nLeft;

			if (null != _cShiftOffset)
			{
				cRetVal.nOffsetLeft = _cShiftOffset.nLeft;
				cRetVal.nOffsetTop = _cShiftOffset.nTop & 1;


				if (stBase.nTop > _stArea.nTop && ((stBase.nTop - _stArea.nTop) & 1) == 1)
					cRetVal.nOffsetTop = ~cRetVal.nOffsetTop;
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

#if CUDA
		static public uint _CommandsCount;   //отладка
		static private long _CudaPerformance;   //отладка   //in tiks
		static public long _CudaPerformanceCumulative;  //отладка   //raz v 100 ciklov
		static public int _CudaPerformanceCircles = 200;
		static private byte _CudaCirclesCount;   //отладка

		internal class Command
		{
			internal enum ID
			{
                Allocate,
                CopyIn,
				CopyOut,
				Merge,
				Dispose
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
					(new Logger()).WriteError(ex);
				}
			}
		}

		static private void Worker(object cState)
		{
			try
			{
				Command cCmd;
				CUDA cCUDA = new CUDA(true);
				for (int i = 0; i < 10; i++)
				{
					try
					{
						cCUDA.CreateContext(i);
						(new Logger()).WriteDebug2(i + ": success");
						break;
					}
					catch (Exception ex)
					{
                        (new Logger()).WriteDebug2(i + ": failed");
                        if (Logger.bDebug && Logger.Level.debug3 > Logger.eLevelMinimum)
                            (new Logger()).WriteError(ex);
                    }
				}
				uint nMemoryReservedForMerge = 2 * 1024 * 1024; //PREFERENCES типа <memory reserved="2097152" />
				uint nMemoryStarvationThreshold = cCUDA.TotalMemory / 2; //PREFERENCES через проценты... типа <memory starvation="50%" />
				uint nMemoryFree;
                string sModule = "CUDAFunctions_" + Preferences.nCUDAVersion + "_x" + (IntPtr.Size * 8);
                if (Logger.bDebug)
                    (new Logger()).WriteDebug3(sModule);
                cCUDA.LoadModule((byte[])Properties.Resource.ResourceManager.GetObject(sModule)); //   $(ProjectDir)Resources\CUDAFunctions.cubin
				//cCUDA.LoadModule(@"c:\projects\!helpers\video\PixelsMap\Resources\CUDAFunctions.cubin");
				CUfunction cCUDAFuncMerge = cCUDA.GetModuleFunction("CUDAFrameMerge");
				int nThreadsPerBlock = 256; //пришлось уменьшить с 512 до 256 сридов на блок, потому что при добавлении "движения" и операций с float, ловил ошибку: Too Many Resources Requested for Launch (This error means that the number of registers available on the multiprocessor is being exceeded. Reduce the number of threads per block to solve the problem)
				cCUDA.SetFunctionBlockShape(cCUDAFuncMerge, nThreadsPerBlock, 1, 1);
				CUDADriver.cuParamSetSize(cCUDAFuncMerge, 8);

				Dictionary<ulong, CUdeviceptr> ahDevicePointers = new Dictionary<ulong, CUdeviceptr>();
				CUdeviceptr cPMs;
				CUdeviceptr cInfos;
				CUdeviceptr cAlphaMap;
				{
					//IntPtr[] aPointersByAlpha = new IntPtr[254];  //те самые поинтеры-альфы. Ссылаются на массивы поинтеров B, т.е. BackGrounds
					//IntPtr[] aPointersByBackground = new IntPtr[256];   //  те самые массивы поинтеров B, т.е. BackGrounds
					byte[] aAlphaMap = new byte[16646144];
					int nResult, nIndx = 0;
					for (byte nAlpha = 1; 255 > nAlpha; nAlpha++)
					{
						for (ushort nBackground = 0; 256 > nBackground; nBackground++)
						{
							for (ushort nForeground = 0; 256 > nForeground; nForeground++)
							{
								if (255 < (nResult = (int)((float)(nAlpha * (nForeground - nBackground)) / 255 + nBackground + 0.5)))
									nResult = 255;
								aAlphaMap[nIndx++] = (byte)nResult;
							}
							//aPointersByBackground[nBackground] = (IntPtr)cCUDA.CopyHostToDevice<byte>(aResults).Pointer;
						}
						//aPointersByAlpha[nAlpha - 1] = (IntPtr)cCUDA.CopyHostToDevice<IntPtr>(aPointersByBackground).Pointer;
					}
					cAlphaMap = cCUDA.CopyHostToDevice<byte>(aAlphaMap);
				}
				//{
				//    IntPtr[] aPointersByAlpha = new IntPtr[254];  //те самые поинтеры-альфы. Ссылаются на массивы поинтеров B, т.е. BackGrounds
				//    IntPtr[] aPointersByBackground = new IntPtr[256];   //  те самые массивы поинтеров B, т.е. BackGrounds
				//    byte[] aResults = new byte[256];
				//    int nResult;
				//    for (byte nAlpha = 1; 255 > nAlpha; nAlpha++)
				//    {
				//        for (ushort nBackground = 0; 256 > nBackground; nBackground++)
				//        {
				//            for (ushort nForeground = 0; 256 > nForeground; nForeground++)
				//            {
				//                if (255 < (nResult = (int)((float)(nAlpha * (nForeground - nBackground)) / 255 + nBackground + 0.5)))
				//                    nResult = 255;
				//                aResults[nForeground] = (byte)nResult;
				//            }
				//            aPointersByBackground[nBackground] = (IntPtr)cCUDA.CopyHostToDevice<byte>(aResults).Pointer;
				//        }
				//        aPointersByAlpha[nAlpha - 1] = (IntPtr)cCUDA.CopyHostToDevice<IntPtr>(aPointersByBackground).Pointer;
				//    }
				//    cAlphaMap = cCUDA.CopyHostToDevice<IntPtr>(aPointersByAlpha);
				//}

#if DEBUG
				Dictionary<ulong, DateTime> ahDebug = new Dictionary<ulong,DateTime>();
#endif
				DateTime dtNextTime = DateTime.MinValue, dtNow;
				long nStartTick; // logging
				while (true)
				{
					if (1 > _aqCommands.CountGet() && (dtNow = DateTime.Now) > dtNextTime)
					{
						dtNextTime = dtNow.AddSeconds(60);
#if DEBUG
						dtNow = dtNow.Subtract(TimeSpan.FromHours(2));
						string sMessage = "";
						foreach (ulong nID in ahDebug.Keys)
							if (dtNow > ahDebug[nID])
								sMessage += "<br>[" + nID + ":" + ahDebug[nID].ToString("HH:mm:ss") + "]";
#endif
						(new Logger()).WriteDebug("CUDA free memory:" + cCUDA.FreeMemory
#if DEBUG
							+ "; possibly timeworn allocations:" + (1 > sMessage.Length ? "no" : sMessage)
#endif
						);
					}
					while (true)
					{
						try
						{
							cCmd = _aqCommands.Dequeue();  //если нечего отдать - заснёт
							break;
						}
						catch (Exception ex)
						{
							(new Logger()).WriteError(ex);
						}
					}
					_CommandsCount = _aqCommands.nCount;
					switch (cCmd.eID)
					{
						case Command.ID.Allocate:
							#region
							try
							{
								cCmd.cPM._cException = null;
								if (1 > cCmd.cPM._nID)
								{
									if (0 < cCmd.cPM._nBytesQty)
									{
										nMemoryFree = cCUDA.FreeMemory;
										if (nMemoryReservedForMerge < nMemoryFree - cCmd.cPM._nBytesQty)
										{
											bMemoryStarvation = (nMemoryFree < nMemoryStarvationThreshold);
											cCmd.cPM._nID = _nCurrentID++;
											ahDevicePointers.Add(cCmd.cPM._nID, cCUDA.Allocate(cCmd.cPM._nBytesQty));
#if DEBUG
											ahDebug.Add(cCmd.cPM._nID, DateTime.Now);
#endif
										}
										else
										{
											bMemoryStarvation = true;
											throw new Exception("out of memory in CUDA device during Allocate. Only 2 MBytes reserved for the Merge");
										}
									}
									else
										throw new Exception("bytes quantity in PixelsMap have to be greater than zero for Allocate [_bDisposed = " + cCmd.cPM._bDisposed + "][_bProcessing = " + cCmd.cPM._bProcessing + "][_bShiftVertical = " + cCmd.cPM._bShiftVertical + "][_bTemp = " + cCmd.cPM._bTemp + "][_dt = " + cCmd.cPM._dt + "][_nBytesQty = " + cCmd.cPM._nBytesQty + "][_nID = " + cCmd.cPM._nID + "][_nShiftPosition = " + cCmd.cPM._nShiftPosition + "][_stArea.nHeight = " + cCmd.cPM._stArea.nHeight + "][_stArea.nWidth = " + cCmd.cPM._stArea.nWidth + "][bKeepAlive = " + cCmd.cPM.bKeepAlive + "][bBackgroundClear = " + cCmd.cPM.bBackgroundClear + "][eAlpha = " + cCmd.cPM.eAlpha + "][bCUDA = " + cCmd.cPM.bCUDA + "][nAlphaConstant = " + cCmd.cPM.nAlphaConstant + "][nID = " + cCmd.cPM.nID + "][nLength = " + cCmd.cPM.nLength + "][stArea.nHeight = " + cCmd.cPM.stArea.nHeight + "][stArea.nWidth = " + cCmd.cPM.stArea.nWidth + "]");
								}
								else
									throw new Exception("PixelsMap ID have to be zero for Allocate");
							}
							catch (Exception ex)
							{
								if (ex is CUDAException)
									ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
								(new Logger()).WriteError(ex);
								(new Logger()).WriteDebug("bytes qty:" + cCmd.cPM._nBytesQty);
								cCmd.cPM._cException = ex;
							}
							cCmd.cMRE.Set();
							break;
							#endregion
						case Command.ID.CopyIn:
							#region
							nStartTick = DateTime.Now.Ticks; // logging
							try
							{
								cCmd.cPM._cException = null;
								if (1 > cCmd.cPM._nID)
								{
									if (cCUDA.FreeMemory - cCmd.cPM._nBytesQty > nMemoryReservedForMerge)
									{
										cCmd.cPM._nID = _nCurrentID++;
										if (cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
											ahDevicePointers.Add(cCmd.cPM._nID, cCUDA.CopyHostToDevice((IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty));
										else if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
											ahDevicePointers.Add(cCmd.cPM._nID, cCUDA.CopyHostToDevice((byte[])cCmd.ahParameters[typeof(byte[])]));
										else
											throw new Exception("unknown parameter type");
#if DEBUG
											ahDebug.Add(cCmd.cPM._nID, DateTime.Now);
#endif
									}
									else
										throw new Exception("out of memory in CUDA device during CopyIn. Only 2 MBytes reserved for the Merge.");
								}
								else
								{
									if (cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
										cCUDA.CopyHostToDevice(ahDevicePointers[cCmd.cPM._nID], (IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty);
									else if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
										cCUDA.CopyHostToDevice(ahDevicePointers[cCmd.cPM._nID], (byte[])cCmd.ahParameters[typeof(byte[])]);
									else
										throw new Exception("unknown parameter type");
								}
								if (ahDevicePointers.ContainsKey(cCmd.cPM._nID))
									(new Logger()).WriteDebug5("copy in [id:" + cCmd.cPM._nID + "][ptr:" + ahDevicePointers[cCmd.cPM._nID].Pointer + "]");
								else
									(new Logger()).WriteDebug5("copy in [id:" + cCmd.cPM._nID + "][ptr: not in dictionary]");
							}
							catch (Exception ex)
							{
								if (ex is CUDAException)
									ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
								(new Logger()).WriteError(ex);
								cCmd.cPM._cException = ex;
							}
							if (new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds >= 20)    // logging
								(new Logger()).WriteNotice("PixelMap: Command.ID.CopyIn: execution time > 20ms: " + new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds +"ms");    // logging
							cCmd.cMRE.Set();
							break;
							#endregion
						case Command.ID.CopyOut:
							#region
							nStartTick = DateTime.Now.Ticks; // logging
							try
							{
								if (0 < cCmd.cPM._nID)
								{
									if(!cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
									{
										if(cCmd.ahParameters.ContainsKey(typeof(byte[])))
										{
											cCmd.cPM._aBytes = (byte[])cCmd.ahParameters[typeof(byte[])];
											if(cCmd.cPM._nBytesQty != cCmd.cPM._aBytes.Length)
												(new Logger()).WriteWarning("wrong array size for copyout [got:" + cCmd.cPM._aBytes.Length + "][expected:" + cCmd.cPM._nBytesQty + "]");
										}
										else
											cCmd.cPM._aBytes = new byte[cCmd.cPM._nBytesQty];
										cCUDA.CopyDeviceToHost<byte>(ahDevicePointers[cCmd.cPM._nID], cCmd.cPM._aBytes);
									}
									else 
										cCUDA.CopyDeviceToHost(ahDevicePointers[cCmd.cPM._nID], (IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty);
									(new Logger()).WriteDebug5("copy out [id:" + cCmd.cPM._nID + "][ptr:" + ahDevicePointers[cCmd.cPM._nID].Pointer + "]");
								}
								else
									throw new Exception("PixelsMap have to be allocated for CopyOut");
							}
							catch (Exception ex)
							{
								if (ex is CUDAException)
									ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
								(new Logger()).WriteError(ex);
								cCmd.cPM._cException = ex;
							}
							if (new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds >= 20)    // logging
								(new Logger()).WriteNotice("PixelMap: Command.ID.CopyOut: execution time > 20ms: " + new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds +"ms");    // logging
							cCmd.cMRE.Set();
							break;
							#endregion
						case Command.ID.Merge:
							#region
							try
							{
								List<PixelsMap> aPMs = (List<PixelsMap>)cCmd.ahParameters[typeof(List<PixelsMap>)];
								DisCom.MergeInfo cMergeInfo = (DisCom.MergeInfo)cCmd.ahParameters[typeof(DisCom.MergeInfo)];
								List<IntPtr> aDPs = new List<IntPtr>();

								if (1 > cCmd.cPM._nID)
									throw new Exception("background PixelsMap have to be allocated for Merge");

								aDPs.Add((IntPtr)ahDevicePointers[cCmd.cPM._nID].Pointer);
								for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
								{
									if (!ahDevicePointers.ContainsKey(aPMs[nIndx]._nID))
										throw new Exception("there is a corrupted ID in layers for merge [id:" + aPMs[nIndx]._nID + "]");
									if (1 > ahDevicePointers[aPMs[nIndx]._nID].Pointer)
										throw new Exception("there is an empty pointer in layers for merge [id:" + aPMs[nIndx]._nID + "]");
									aDPs.Add((IntPtr)ahDevicePointers[aPMs[nIndx]._nID].Pointer);
								}

								cPMs = cCUDA.CopyHostToDevice<IntPtr>(aDPs.ToArray());
								cInfos = cCUDA.CopyHostToDevice(cMergeInfo, cMergeInfo.SizeGet());

								cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, 0, (IntPtr)cPMs.Pointer);
								cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size, (IntPtr)cInfos.Pointer);
								cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 2, (IntPtr)cAlphaMap.Pointer);
								cCUDA.SetParameterSize(cCUDAFuncMerge, (uint)(IntPtr.Size * 3));
								int nIterations = (0 == cMergeInfo.nBackgroundSize % nThreadsPerBlock ? cMergeInfo.nBackgroundSize / nThreadsPerBlock : cMergeInfo.nBackgroundSize / nThreadsPerBlock + 1);
								cCUDA.Launch(cCUDAFuncMerge, nIterations, 1);
								cCmd.cMRE.Set();

                                cMergeInfo.Dispose();

								cCUDA.Free(cPMs);
								cCUDA.Free(cInfos);
								for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
								{
									lock (aPMs[nIndx]._cSyncRoot)
										aPMs[nIndx]._bProcessing = false;
									aPMs[nIndx].Dispose();
								}
							}
							catch (Exception ex)
							{
								cCmd.cMRE.Set();
								if (ex is CUDAException)
									ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
								(new Logger()).WriteError(ex);
								cCmd.cPM._cException = ex;
							}
							break;
							#endregion
						case Command.ID.Dispose:
							#region
							nStartTick = DateTime.Now.Ticks; // logging
							(new Logger()).Write(Logger.Level.debug2, "dispose: in");
							try
							{
								if (ahDevicePointers.ContainsKey(cCmd.cPM._nID))
								{
									if (0 < cCmd.cPM._nID && 0 < ahDevicePointers[cCmd.cPM._nID].Pointer)
									{
										cCUDA.Free(ahDevicePointers[cCmd.cPM._nID]);
										//cCUDA.SynchronizeContext();
										bMemoryStarvation = (cCUDA.FreeMemory < nMemoryStarvationThreshold);
										(new Logger()).WriteDebug3("dispose [id:" + cCmd.cPM._nID + "][ptr:" + ahDevicePointers[cCmd.cPM._nID].Pointer + "]");
									}
									ahDevicePointers.Remove(cCmd.cPM._nID);
#if DEBUG
									ahDebug.Remove(cCmd.cPM._nID);
#endif
									cCmd.cPM._nID = 0;
								}
							}
							catch (Exception ex)
							{
								if (ex is CUDAException)
									ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
								(new Logger()).WriteError(ex);
								cCmd.cPM._cException = ex;
							}
							(new Logger()).Write(Logger.Level.debug2, "dispose: out");
							if (new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds >= 20)    // logging
								(new Logger()).WriteNotice("PixelMap: Command.ID.Dispose: execution time > 20ms: " + new TimeSpan(DateTime.Now.Ticks - nStartTick).TotalMilliseconds +"ms");    // logging
							break;
							#endregion
					}
				}
			}
			catch (Exception ex)
			{
				(new Logger()).WriteError(ex);
			}
		}
#endif
	}
}
