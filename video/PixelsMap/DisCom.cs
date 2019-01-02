//#define CUDATEST  //DNF merge like native cuda function
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime;
using System.Threading;
using System.Runtime.InteropServices;

namespace helpers
{
    public partial class DisCom
    {
        public enum Alpha : byte
        { // внимание! берется из префов! не переименовывать просто так
            normal = 0, // не маска
            mask = 1,  // альфирует (маскирует) только где у слоя нет альфы.
			mask_invert = 2,   // альфирует везде, где у слоя альфа или где вообще нету слоя (Area). круто бывает использовать саму плашку для альфирования текста на плашке.
            mask_all_upper = 3,  // то же, только альфирует все вышестоящие слои
            mask_all_upper_invert = 4,   // то же, только альфирует все вышестоящие слои
            none = 255,  // маска отключена (не работает)
        }
        [Serializable]
        abstract public class Info
		{
		}
        [Serializable]
        public class MergeInfo : Info
        {
            public ushort nLayersQty;
            public int nBackgroundSize;
			public int nBackgroundWidth_4;
			public int nBackgroundWidth;
			public int nBackgroundHight;
			public byte nBackgroundAlphaType;
			public LayerInfo[] aLayerInfos;
            public bool bLayer1CopyToBackground;

            private IntPtr _p;
            ~MergeInfo()
            {
				try
				{
                    Dispose();
                }
                catch (Exception ex)
                {
                    (new Logger()).WriteError(ex);
                }
            }

			public void Dispose()
            {
                Free();
                aLayerInfos = null;
            }
            private void Free()
            {
                if (IntPtr.Zero != _p)
                {
                    Marshal.FreeHGlobal(_p);
                    _p = IntPtr.Zero;
                }
            }
            public uint SizeGet()
            {
                int nRetVal = sizeof(int) * 6;  // *8
                if (null != aLayerInfos)
                    nRetVal += aLayerInfos.Length * ((sizeof(int) * 17) + sizeof(float)); //замена bool'ов и byte'ов на int'ы - дань bytealign'у
                return (uint)nRetVal;
            }
            static public implicit operator IntPtr(MergeInfo cMergeInfo)
            {
                cMergeInfo.Free();
                cMergeInfo._p = Marshal.AllocHGlobal((int)cMergeInfo.SizeGet());
                Marshal.WriteInt32(cMergeInfo._p, cMergeInfo.nLayersQty);
                int nOffset = sizeof(int);
                Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.nBackgroundSize);
                nOffset += sizeof(int);
                Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.nBackgroundWidth_4);
                nOffset += sizeof(int);
                Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.nBackgroundAlphaType);
				nOffset += sizeof(int);
				Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.nBackgroundHight);
				nOffset += sizeof(int);
				Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.nBackgroundWidth);
				nOffset += sizeof(int);

				if (null != cMergeInfo.aLayerInfos)
                {
                    for (int nIndx = 0; cMergeInfo.aLayerInfos.Length > nIndx; nIndx++)
                    {
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropTopLineInBG);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropBottomLineInBG);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropLeft_4);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nWidth_4);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropWidth_4);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nLeft_4);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nTop);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nAlphaConstant);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nHalfDeltaPxX_4);
                        nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nHalfDeltaPxY_4);
						nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nHalfPathShiftPositionByteX);
						nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nHalfPathShiftPositionByteY);
						nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nShiftPositionByteX);
						nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nShiftPositionByteY);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nAlphaType);
                        nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nOffsetTop);
						nOffset += sizeof(int);
						Marshal.Copy(new float[] { cMergeInfo.aLayerInfos[nIndx].nShiftTotalX }, 0, cMergeInfo._p + nOffset, 1);
						nOffset += sizeof(float);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nBytesQty);
						nOffset += sizeof(int);
                    }
                }
                return cMergeInfo._p;
            }
        }
        public class CompareInfo : Info
        {
			public class Point
			{
				private int _nIndx;
				private byte _nSimilarity;
				private byte[] _aRGBA;
				public Point(int nIndx)
				{
					_nIndx = nIndx;
					_nSimilarity = 0;
					_aRGBA = new byte[4];
					Array.ForEach(_aRGBA, row=>row=0);
				}

				public void Compare(Point cPoint)
				{
					int nResult = 0;
					for(int nIndx = 0; _aRGBA.Length > nIndx; nIndx++)
						nResult += (_aRGBA[nIndx] + cPoint._aRGBA[nIndx]) / 2;
					_nSimilarity = (byte)(nResult / 4);
				}
			}
			~CompareInfo()
            {
				//try
				//{
    //                Dispose();
    //            }
    //            catch (Exception ex)
    //            {
    //                (new Logger()).WriteError(ex);
    //            }
            }

			void Dispose()
            {
            }
        }
        [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        [Serializable]
        public class LayerInfo
        {
			public int nCropTopLineInBG;
			public int nCropBottomLineInBG;
			public int nCropLeft_4;
			public int nWidth_4;
			public byte nAlphaConstant;
			public int nCropWidth_4;
            public byte nShiftPositionByteX;
			public byte nShiftPositionByteY;
			//public bool bShiftVertical;
			public byte nAlphaType;
			//public int nOffsetLeft; 
			public int nOffsetTop;
			public float nShiftTotalX;
			public int nLeft_4;
			public int nTop;
			public int nHalfDeltaPxX_4;
			public int nHalfDeltaPxY_4; 
			public byte nHalfPathShiftPositionByteX;
			public byte nHalfPathShiftPositionByteY;
			public int nBytesQty;
        }
		public class LineLayerInfo
		{
			public int nBGCropStartRed;
			public int nFGCropStartRed;
			public int nBGCropEndRed;
			public bool bRowUpper;
			public bool bRowUnder;
			public int nBgFgLinesDelta;
			public int nFGLineBeginningRed;
		}
		private enum Function
        {
            Merge,
            Move
        }
        static private byte[, ,] _aAlphaMap;
		static private byte[,] _aAlphaMap2;
		static private byte[,] _aAlphaMap3;
		static private ThreadBufferQueue<DisCom> _aqQueue = new ThreadBufferQueue<DisCom>(0, false);
        static private Thread _cThreadMain;
        static private Thread[] _aThreads;
        static private ManualResetEvent[] _aMREStart;
        static private ManualResetEvent[] _aMREDone;
        static private DisCom _cDisComProcessing;
		static private ulong _nThreadsQty;
        static MergeInfo _cMergeInfo;
        static private ulong _nJobIndx; // for profiling
        private long _nJobsDone = 0; // for profiling
        static public bool _bInited = false;

        private Info _cInfo;
        private byte[][] _aLayers;
		private ManualResetEvent _cMREDone;
		private int _nMaxTasksIndx;
		private int _nTaskNumber;
		private object oLock;

		public DisCom()
		{
            PixelsMap.DisComInit();
			_cMREDone = new ManualResetEvent(false);
			oLock = new object();
		}
		~DisCom()
		{
			try
			{
				Dispose();
			}
			catch (Exception ex)
			{
				(new Logger()).WriteError(ex);
			}
		}

		public void Dispose()
		{
			_aLayers = null;
			_cInfo = null;
		}
        //static public void Init()
        //{
        //    Init(Environment.ProcessorCount / 2);
        //}
        
        static public void Init(int nThreadsQty)
		{
			lock (_aqQueue)
			{
                if (_bInited)
                    return;
#if CUDATEST
                (new Logger()).WriteError("MERGING CUDA TEST IS ON!!!");                
#endif
                if (null == _aAlphaMap)
				{
					_cDisComProcessing = null;
					_aAlphaMap = new byte[byte.MaxValue - 1, byte.MaxValue + 1, byte.MaxValue + 1];
					int nResult;
					for (byte nAlpha = 1; 255 > nAlpha; nAlpha++)
					{
						for (ushort nBackground = 0; 256 > nBackground; nBackground++)
						{
							for (ushort nForeground = 0; 256 > nForeground; nForeground++)
							{
								if (255 < (nResult = (int)((float)(nAlpha * (nForeground - nBackground)) / 255 + nBackground + 0.5)))
									nResult = 255;
								_aAlphaMap[nAlpha - 1, nBackground, nForeground] = (byte)nResult;
							}
						}
					}
					//nPixelAlpha = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
					_aAlphaMap2 = new byte[byte.MaxValue - 1, byte.MaxValue - 1];
					for (byte nFGColorAlpha = 1; 255 > nFGColorAlpha; nFGColorAlpha++)  // мможно использовать симметрию умножения, но х с ней пока
					{
						for (byte nPixelAlpha = 1; 255 > nPixelAlpha; nPixelAlpha++)
						{
							_aAlphaMap2[nFGColorAlpha - 1, nPixelAlpha - 1] = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
						}
					}
					//nFGColorAlpha = (byte)(nFGColorAlpha * (1 - _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx] / 255f) + 0.5);
					_aAlphaMap3 = new byte[byte.MaxValue, byte.MaxValue - 1];
					for (ushort nFGColorAlpha = 1; 256 > nFGColorAlpha; nFGColorAlpha++)
					{
						for (byte nMask = 1; 255 > nMask; nMask++)
						{
							_aAlphaMap3[nFGColorAlpha - 1, nMask - 1] = (byte)(nFGColorAlpha * ((255 - nMask) / 255f) + 0.5);
						}
					}

                    _nThreadsQty = (ulong)nThreadsQty;
                    (new Logger()).WriteNotice("[threads=" + _nThreadsQty + "][ProcessorCount=" + Environment.ProcessorCount + "][pref_cout=" + PixelsMap.Preferences.nDisComThreadsQty + "]");

                    _aThreads = new Thread[_nThreadsQty];
					_cThreadMain = new Thread(new ThreadStart(WorkerMain));
					_cThreadMain.IsBackground = true;
					_cThreadMain.Priority = ThreadPriority.AboveNormal;
					_cThreadMain.Start();
					Thread cThread = null;
					_aMREStart = new ManualResetEvent[_nThreadsQty];
					_aMREDone = new ManualResetEvent[_nThreadsQty];
					for (ushort nIndx = 0; _nThreadsQty > nIndx; nIndx++)
					{
						_aMREStart[nIndx] = new ManualResetEvent(false);
						_aMREDone[nIndx] = new ManualResetEvent(false);
						cThread = new Thread(new ParameterizedThreadStart(Worker)); //(new ParameterizedThreadStart(Worker));
						_aThreads[nIndx] = cThread;
						cThread.IsBackground = true;
						cThread.Priority = ThreadPriority.AboveNormal;
						cThread.Start(nIndx);
					}
				}
                _bInited = true;
            }
		}
		public void FrameMerge(MergeInfo cMergeInfo, List<byte[]> aLayers, bool bEnqueueFirst)
		{
            _cMREDone.Reset();  // для многоразового юзания
            _nMaxTasksIndx = cMergeInfo.nBackgroundHight;
			_nTaskNumber = 0;
			_cInfo = cMergeInfo;
			_aLayers = aLayers.ToArray();
			lock (_aqQueue.oSyncRoot)  // что б вклинить таск из байтилуса - только он не конкурирует с пре-рендерами роллов и т.п.
			{
				if (bEnqueueFirst)
					_aqQueue.EnqueueFirst(this);
				else
					_aqQueue.Enqueue(this);
			}
            //(new Logger()).WriteDebug3("begin one [" + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss.ms") + "]");

            _cMREDone.WaitOne();
        }
        public void FrameCompare(CompareInfo cCompareInfo, byte[] aFrameBytes)
        {
			_cInfo = cCompareInfo;
            //_aLayers = aLayers;
            //_aqQueue.Enqueue(this);
            //_cMREDone.WaitOne();
        }
		private int GetTask()
		{
			lock (oLock)
			{
				if (_nTaskNumber < _nMaxTasksIndx)
					return _nTaskNumber++;
				else
					return int.MaxValue;
			}
		}
		static private void WorkerMain()
        {
            try
			{
				_nJobIndx = 0;
				int nCounter = 0;
                (new Logger()).WriteNotice("main worker started");
                //Logger.Timings cTimings = new helpers.Logger.Timings("discom:WorkerMain:profiling");
                while (true)
                {
                    _cDisComProcessing = _aqQueue.Dequeue(); //DNF
                    if (_cDisComProcessing._cInfo is MergeInfo)
                    {
                        _cMergeInfo = (MergeInfo)_cDisComProcessing._cInfo;
#if !CUDATEST
                        if (_cMergeInfo.bLayer1CopyToBackground)
                        {
                            Array.Copy(_cDisComProcessing._aLayers[1], _cDisComProcessing._aLayers[0], _cDisComProcessing._aLayers[0].Length);
                        }
                        else
                        {
                            Array.Clear(_cDisComProcessing._aLayers[0], 0, _cDisComProcessing._aLayers[0].Length);
                        }
#endif
                    }
                    else
                        _cMergeInfo = null;

                    if (100 < nCounter++)
                    {
                        //cTimings.TotalRenew(); //profiling
                        nCounter = 0;
                        _nJobIndx = 1;
                        //(new Logger()).WriteNotice("profiling begin [id:" + _cDisComProcessing.GetHashCode() + "][max_indx:" + _cDisComProcessing._nMaxTasksIndx + "][total_threads:" + _aMREDone.Length + "]");
                    }
                    else
                        _nJobIndx = 0;




                    //_cDisComProcessing._cMREDone.Reset();
                    foreach (ManualResetEvent cMRE in _aMREDone)
                        cMRE.Reset();

                    //if (_nJobIndx == 1) //profiling
                    //    cTimings.Restart("done reset");

                    foreach (ManualResetEvent cMRE in _aMREStart)
                        cMRE.Set();

                    //if (_nJobIndx == 1) //profiling
                    //    cTimings.Restart("start set");

                    for (int nIndx = 0; nIndx < _aMREDone.Length; nIndx += 64)
                        ManualResetEvent.WaitAll(_aMREDone.Skip(nIndx).Take(64).ToArray());


                    _cDisComProcessing._cMREDone.Set();


                    //if (_nJobIndx == 1) //profiling
                    //{
                    //    (new Logger()).WriteNotice("profiling end [id:" + _cDisComProcessing.GetHashCode() + "][max_indx:" + _cDisComProcessing._nMaxTasksIndx + "][total_jobs_done = " + _cDisComProcessing._nJobsDone + "]");
                    //    cTimings.Stop("merge done");
                    //}
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
        }
        static private void Worker(object cState)
        {
            try
            {
                ushort nID = (ushort)cState;
				ManualResetEvent cMREStart = _aMREStart[nID];
				ManualResetEvent cMREDone = _aMREDone[nID];
				int nLineToDo = 0, nCount = 0;
                //Logger.Timings cTimings = new helpers.Logger.Timings("discom:Worker:profiling");

                while (true)
				{
					try
					{
						cMREStart.Reset();
						ManualResetEvent.SignalAndWait(cMREDone, cMREStart);
                        if (null != _cMergeInfo)
                        {
							nCount = 0;
                            //if (_nJobIndx == 1) //profiling
                            //    cTimings.TotalRenew();

                            while ((nLineToDo = _cDisComProcessing.GetTask()) < int.MaxValue)
                            {
#if CUDATEST
                                MergingCUDATest(nLineToDo);
#else
                                Merging(nLineToDo);
#endif

                                nCount++;

                                //if (_nJobIndx == 1) //profiling
                                //{
                                //    cTimings.Restart("job=" + nLineToDo);
                                //    System.Threading.Interlocked.Increment(ref _cDisComProcessing._nJobsDone);
                                //}

                            }


                            //if (_nJobIndx == 1) //profiling
                            //{
                            //    cTimings.Stop("thread stopped [n=" + nID + "][id_parent:" + _cDisComProcessing.GetHashCode() + "][jobs_by_this_thread=" + nCount + "]");
                            //}
                        }
                    }
					catch (Exception ex)
					{
						if (ex is ThreadInterruptedException)
							throw;
						(new Logger()).WriteError("_cDisComProcessing = " + (_cDisComProcessing == null ? "null!!" : "not null") + "[count=" + nCount + "][line=" + nLineToDo + "][cInfo is null=" + (bool)(_cDisComProcessing._cInfo == null) + "][aLayers is null=" + (bool)(_cDisComProcessing._aLayers == null) + "]", ex);
					}
				}
			}
			catch (Exception ex)
			{
                if (!(ex is ThreadInterruptedException))
                    (new Logger()).WriteError(ex);
            }
        }
	}
}
