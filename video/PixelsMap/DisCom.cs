using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;
using System.Runtime.InteropServices;

namespace helpers
{
    public class DisCom
    {
        public enum Alpha : byte
        {
            normal = 0,
            mask = 1,
            none = 255
        }
		abstract public class Info
		{
		}
        public class MergeInfo : Info
        {
            public ushort nLayersQty;
            public int nBackgroundSize;
            public int nBackgroundWidth;
			public int nBackgroundHight;
			public byte nBackgroundAlphaType;
			public LayerInfo[] aLayerInfos;

            private IntPtr _p;
            ~MergeInfo()
            {
                try
                {
                    Dispose();
                }
                catch { }
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
                int nRetVal = sizeof(int) * 8;
                if (null != aLayerInfos)
                    nRetVal += aLayerInfos.Length * ((sizeof(int) * 14) + sizeof(float)); //замена bool'ов и byte'ов на int'ы - дань bytealign'у
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
                Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.nBackgroundWidth);
                nOffset += sizeof(int);
                Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.nBackgroundAlphaType);
				nOffset += sizeof(int);

                if (null != cMergeInfo.aLayerInfos)
                {
                    for (int nIndx = 0; cMergeInfo.aLayerInfos.Length > nIndx; nIndx++)
                    {
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nWidthDiff);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nForegroundStart);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nBackgroundStart);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nBackgroundStop);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropLeft);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropRight);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nWidth);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nAlphaConstant);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropWidth);
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nCropHeight);
                        nOffset += sizeof(int);
                        Marshal.Copy(new float[] { cMergeInfo.aLayerInfos[nIndx].nShiftPosition }, 0, cMergeInfo._p + nOffset, 1);
                        nOffset += sizeof(float);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)(cMergeInfo.aLayerInfos[nIndx].bShiftVertical ? 1 : 0));
                        nOffset += sizeof(int);
                        Marshal.WriteInt32(cMergeInfo._p + nOffset, (int)cMergeInfo.aLayerInfos[nIndx].nAlphaType);
                        nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nOffsetLeft);
						nOffset += sizeof(int);
						Marshal.WriteInt32(cMergeInfo._p + nOffset, cMergeInfo.aLayerInfos[nIndx].nOffsetTop);
						nOffset += sizeof(int);
						Marshal.Copy(new float[] { cMergeInfo.aLayerInfos[nIndx].nShiftTotal }, 0, cMergeInfo._p + nOffset, 1);
						nOffset += sizeof(float);
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
                try
                {
                    Dispose();
                }
                catch { }
            }

			void Dispose()
            {
            }
        }
        [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class LayerInfo
        {
            public int nWidthDiff;
            public int nForegroundStart;
            public int nBackgroundStart;
            public int nBackgroundStop;
            public int nCropLeft;
            public int nCropRight;
            public int nWidth;
            public byte nAlphaConstant;
            public int nCropWidth;
            public int nCropHeight;
            public float nShiftPosition;
            public bool bShiftVertical;
            public byte nAlphaType;
			public int nOffsetLeft; 
			public int nOffsetTop;
			public float nShiftTotal;
			public int nLeft;
			public int nTop;
		}
		public class LineInfo
		{
			public class LineLayerInfo
			{
				public int nBGCropStart;
				public int nFGCropStart;
				public int nBGCropEnd;
				public bool bRowUpper;
				public bool bRowUnder;
				public int nBgFgLinesDelta;
				public int nFGLineBeginning;
			}
			public int nBGStart;
			public LineLayerInfo[] aLineLayers;
		}
		private enum Function
        {
            Merge,
            Move
        }
        static private byte[, ,] _aAlphaMap;
        static private ThreadBufferQueue<DisCom> _aqQueue = new ThreadBufferQueue<DisCom>(0, false);
        static private Thread _cThreadMain;
        static private Thread[] _aThreads;
        static private ManualResetEvent[] _aMREStart;
        static private ManualResetEvent[] _aMREDone;
        static private DisCom _cDisComProcessing;
		static private Dictionary<DisCom, int> _aqTasks;


		private Info _cInfo;
        private byte[][] _aLayers;
		private ManualResetEvent _cMREDone;
		private int[] aCropTopLineInBG;
		private int[] aCropBottomLineInBG;
		private int _nMaxTasksIndx;

		public DisCom()
		{
			if (null == _aAlphaMap)
				Init();
			_cMREDone = new ManualResetEvent(false);
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

		static private int GetTask(DisCom cDC)
		{
			lock (cDC)
			{
				if (_aqTasks[cDC] < cDC._nMaxTasksIndx)
					return _aqTasks[cDC]++;
				else
					return int.MaxValue;
			}
		}

		static public void Init()
		{
			lock (_aqQueue)
			{
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
					int nThreadsQty = Environment.ProcessorCount * 2;   // DNF  *2
					_aThreads = new Thread[nThreadsQty];
					_cThreadMain = new Thread(new ThreadStart(WorkerMain));
					_cThreadMain.IsBackground = true;
					_cThreadMain.Priority = ThreadPriority.Highest;
					_cThreadMain.Start();
					Thread cThread = null;
					_aqTasks = new Dictionary<DisCom, int>();
					_aMREStart = new ManualResetEvent[nThreadsQty];
					_aMREDone = new ManualResetEvent[nThreadsQty];
					for (ushort nIndx = 0; nThreadsQty > nIndx; nIndx++)
					{
						_aMREStart[nIndx] = new ManualResetEvent(false);
						_aMREDone[nIndx] = new ManualResetEvent(false);
						cThread = new Thread(new ParameterizedThreadStart(Worker));
						_aThreads[nIndx] = cThread;
						cThread.IsBackground = true;
						cThread.Priority = ThreadPriority.Highest;
						cThread.Start(nIndx);
					}
				}
			}
		}
        public void FrameMerge(MergeInfo cMergeInfo, List<byte[]> aLayers)
        {
			_aqTasks.Add(this, 0);
			_nMaxTasksIndx = cMergeInfo.nBackgroundHight;
            aCropTopLineInBG = new int[cMergeInfo.aLayerInfos.Length];
			aCropBottomLineInBG = new int[cMergeInfo.aLayerInfos.Length];
			for (int nI = 0; nI < aCropTopLineInBG.Length; nI++)
			{
				aCropTopLineInBG[nI] = cMergeInfo.aLayerInfos[nI].nBackgroundStart / cMergeInfo.nBackgroundWidth;
				aCropBottomLineInBG[nI] = aCropTopLineInBG[nI] + cMergeInfo.aLayerInfos[nI].nCropHeight - 1;
            }

			_cInfo = cMergeInfo;
            _aLayers = aLayers.ToArray();
            _aqQueue.Enqueue(this);
			//(new Logger()).WriteDebug3("begin one [" + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss.ms") + "]");
			_cMREDone.WaitOne();

			_aqTasks.Remove(this);
        }
		public void FrameCompare(CompareInfo cCompareInfo, byte[] aFrameBytes)
        {
			_cInfo = cCompareInfo;
            //_aLayers = aLayers;
            //_aqQueue.Enqueue(this);
            //_cMREDone.WaitOne();
        }

        static private void WorkerMain()
        {
            try
            {
                //(new Logger()).WriteNotice("[id:" + GetHashCode() + ":" + nID + "][total:" + nThreadsTotalQty + "][start]");
                while (true)
                {
                    _cDisComProcessing = _aqQueue.Dequeue();
                    _cDisComProcessing._cMREDone.Reset();
                    foreach (ManualResetEvent cMRE in _aMREDone)
                        cMRE.Reset();
                    foreach (ManualResetEvent cMRE in _aMREStart)
                        cMRE.Set();
                    for (int nIndx = 0; nIndx < _aMREDone.Length; nIndx += 64)
                        ManualResetEvent.WaitAll(_aMREDone.Skip(nIndx).Take(64).ToArray());
                    _cDisComProcessing._cMREDone.Set();
                }
                //(new Logger()).WriteNotice("[id:" + GetHashCode() + ":" + nID + "][total:" + nThreadsTotalQty + "][stop]");
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
				int nLineToDo, nMax, nCount = 0, nCropWidth;
				while (true)
				{
					try
					{
						cMREStart.Reset();
						ManualResetEvent.SignalAndWait(cMREDone, cMREStart);
						if (_cDisComProcessing._cInfo is MergeInfo)
						{
							MergeInfo cMergeInfo = (MergeInfo)_cDisComProcessing._cInfo;
							nCount = 0;

							//----Logger.Timings cTiming = new helpers.Logger.Timings("BTL:pixelmap:discom:");

							while ((nLineToDo = GetTask(_cDisComProcessing)) < int.MaxValue)
							{
								LineInfo cLineInfo = new LineInfo();
								cLineInfo.nBGStart = nLineToDo * cMergeInfo.nBackgroundWidth;    // nBGStart - если BG начинается с начала линии всегда - вроде тек и есть
								cLineInfo.aLineLayers = new LineInfo.LineLayerInfo[cMergeInfo.aLayerInfos.Length];
								for (int nI = 0; nI < cLineInfo.aLineLayers.Length; nI++)
								{
									cLineInfo.aLineLayers[nI] = new LineInfo.LineLayerInfo();
                                    if (nLineToDo < _cDisComProcessing.aCropTopLineInBG[nI] || nLineToDo > _cDisComProcessing.aCropBottomLineInBG[nI])
									{
										cLineInfo.aLineLayers[nI].nBGCropStart = int.MaxValue;
										cLineInfo.aLineLayers[nI].nBGCropEnd = int.MinValue;
										cLineInfo.aLineLayers[nI].nFGCropStart = int.MaxValue;
									}
									else
									{
										cLineInfo.aLineLayers[nI].nBGCropStart = cLineInfo.nBGStart + cMergeInfo.aLayerInfos[nI].nCropLeft;
										cLineInfo.aLineLayers[nI].nBGCropEnd = cLineInfo.aLineLayers[nI].nBGCropStart + cMergeInfo.aLayerInfos[nI].nCropWidth - 1;
										//nCropWidth = cLineInfo.aLineLayers[nI].nBGCropEnd + 1 - cLineInfo.aLineLayers[nI].nBGCropStart;


										if (_cDisComProcessing.aCropTopLineInBG[nI] == 0)
											cLineInfo.aLineLayers[nI].nFGCropStart = (nLineToDo - cMergeInfo.aLayerInfos[nI].nTop) * cMergeInfo.aLayerInfos[nI].nWidth;
										else
											cLineInfo.aLineLayers[nI].nFGCropStart = (nLineToDo - _cDisComProcessing.aCropTopLineInBG[nI]) * cMergeInfo.aLayerInfos[nI].nWidth;
										if (cLineInfo.nBGStart < cLineInfo.aLineLayers[nI].nBGCropStart)
										{
											cLineInfo.aLineLayers[nI].nFGLineBeginning = cLineInfo.aLineLayers[nI].nFGCropStart;
										}
										else
										{
											cLineInfo.aLineLayers[nI].nFGCropStart -= cMergeInfo.aLayerInfos[nI].nLeft;
											cLineInfo.aLineLayers[nI].nFGLineBeginning = ((int)((double)cLineInfo.aLineLayers[nI].nFGCropStart / cMergeInfo.aLayerInfos[nI].nWidth)) * cMergeInfo.aLayerInfos[nI].nWidth;
										}
										


										if (nLineToDo - 1 >= _cDisComProcessing.aCropTopLineInBG[nI] && nLineToDo - 1 <= _cDisComProcessing.aCropBottomLineInBG[nI])
											cLineInfo.aLineLayers[nI].bRowUpper = true;
										if (nLineToDo + 1 >= _cDisComProcessing.aCropTopLineInBG[nI] && nLineToDo + 1 <= _cDisComProcessing.aCropBottomLineInBG[nI])
											cLineInfo.aLineLayers[nI].bRowUnder = true;
										cLineInfo.aLineLayers[nI].nBgFgLinesDelta = nLineToDo - _cDisComProcessing.aCropTopLineInBG[nI];
										
                                    }
								}
								for (int nI = cLineInfo.nBGStart; nI < cLineInfo.nBGStart + cMergeInfo.nBackgroundWidth; nI++)
									Merging(nI, cLineInfo);
								nCount++;
							}

							//-----cTiming.Stop("merged " + nID + "-th > 30 [count=" + nCount + "]", 40);
						}
					}
					catch (Exception ex)
					{
						if (ex is ThreadInterruptedException)
							throw;
						(new Logger()).WriteError(ex);
					}
				}
				//(new Logger()).WriteNotice("[id:" + GetHashCode() + ":" + nID + "][total:" + nThreadsTotalQty + "][stop]");
			}
			catch (Exception ex)
			{
                if (!(ex is ThreadInterruptedException))
                    (new Logger()).WriteError(ex);
            }
        }
		static private void Merging(int nBGIndxPixel, LineInfo cLI)
		{
			MergeInfo cMergeInfo = (MergeInfo)_cDisComProcessing._cInfo;
			if (nBGIndxPixel < cMergeInfo.nBackgroundSize)         //2-й - это размер BG, 3-й - ширина BG, 4-й - делать ли задник? 5-й - инфа про FG1; 
			{                                   //Периодичность PRECOMPUTED_INFO_PERIOD - 1-й. 0-й - это количество слоёв
				int M, nIndxIndent, nRow;
				int nBGIndxRed, nBGIndxGreen, nBGIndxBlue, nBGIndxAlpha, nFGIndx;
				byte nFGColorRed = 0, nFGColorGreen = 0, nFGColorBlue = 0, nFGColorAlpha = 0;
				int nNextIndxRed, nNextIndxGreen, nNextIndxBlue, nNextIndxAlpha, nPixelAlphaIndx;
				byte nPixelAlpha;
				int nMaskIndx = -1;
				LayerInfo cLayerInfo;
				LineInfo.LineLayerInfo cLLI;

				nBGIndxRed = nBGIndxPixel * 4;
				nBGIndxGreen = nBGIndxRed + 1;
				nBGIndxBlue = nBGIndxRed + 2;
				nBGIndxAlpha = nBGIndxRed + 3;
				_cDisComProcessing._aLayers[0][nBGIndxRed] = 0;
				_cDisComProcessing._aLayers[0][nBGIndxGreen] = 0;
				_cDisComProcessing._aLayers[0][nBGIndxBlue] = 0;
				if (1 == cMergeInfo.nBackgroundAlphaType)
					nMaskIndx = nBGIndxAlpha;
				else
					_cDisComProcessing._aLayers[0][nBGIndxAlpha] = cMergeInfo.nBackgroundAlphaType;


				for (ushort nLayerIndx = 1; cMergeInfo.nLayersQty > nLayerIndx; nLayerIndx++)
				{
					cLayerInfo = cMergeInfo.aLayerInfos[(int)(nLayerIndx - 1)];
					cLLI = cLI.aLineLayers[(int)(nLayerIndx - 1)];

					if (nBGIndxPixel >= cLLI.nBGCropStart && nBGIndxPixel <= cLLI.nBGCropEnd)
					{
						nFGIndx = (nBGIndxPixel - cLLI.nBGCropStart + cLLI.nFGCropStart) * 4;

						if (1 == cLayerInfo.nAlphaType) //леер является маской
						{
							if (_cDisComProcessing._aLayers[nLayerIndx].Length - 1 < (nMaskIndx = nFGIndx + 3))
								nMaskIndx = -1;
							continue;
						}
						nFGColorAlpha = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3];
						if (-1 < nMaskIndx) //применяем маску
						{
							if (255 == _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx]) //отрезали пиксел по маске
							{
								nMaskIndx = -1;
								continue;
							}
							else if (0 < _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx])
								nFGColorAlpha = (byte)(nFGColorAlpha * (1 - _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx] / 255f) + 0.5);
							nMaskIndx = -1;
						}
						nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx];
						nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1];
						nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2];

						if (0 == cLayerInfo.nAlphaType) // т.е. наш слой не альфирующий, а обычный слой с альфой RGBA
						{
							if (0 != cLayerInfo.nShiftPosition || 0 != cLayerInfo.nShiftTotal)// обработка расположения между пикселями. Берем инфу с того места откуда она приехала в этот пиксель
							{
								if (cLayerInfo.bShiftVertical)   // для вертикальных cLayerInfo.nShiftTotal == 0  
								{
									if (0 < cLayerInfo.nShiftPosition)
									{
										nPixelAlpha = nFGColorAlpha;
										nFGColorAlpha = (byte)((nFGColorAlpha + 1) * (1 - cLayerInfo.nShiftPosition));
										if (cLLI.bRowUnder)
										{
											nNextIndxRed = nFGIndx + (cLayerInfo.nWidth * 4);
											nNextIndxGreen = nNextIndxRed + 1;
											nNextIndxBlue = nNextIndxRed + 2;
											nNextIndxAlpha = nNextIndxRed + 3;
											if (0 < _cDisComProcessing._aLayers[nLayerIndx][nNextIndxAlpha])
											{
												if (0 < (nPixelAlpha = (byte)((_cDisComProcessing._aLayers[nLayerIndx][nNextIndxAlpha] + 1) * cLayerInfo.nShiftPosition)))
												{
													if (0 == nFGColorAlpha || 254 < nPixelAlpha)
													{
														nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed];
														nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxGreen];
														nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxBlue];
													}
													else
													{
														nPixelAlphaIndx = nPixelAlpha - 1;
														nFGColorRed = _aAlphaMap[nPixelAlphaIndx, nFGColorRed, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed]];
														nFGColorGreen = _aAlphaMap[nPixelAlphaIndx, nFGColorGreen, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxGreen]];
														nFGColorBlue = _aAlphaMap[nPixelAlphaIndx, nFGColorBlue, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxBlue]];
													}
												}
												if (255 < nFGColorAlpha + nPixelAlpha)
													nFGColorAlpha = 255;
												else
													nFGColorAlpha += nPixelAlpha;
											}
										}
									}
									else // тестовый элз - это если в роле не 1 - (....)   а просто - (.....)  в шифт идёт. тут другой способ.
									{
										if (cLLI.bRowUpper)
										{
											int nUpPxIndx = nFGIndx - (cLayerInfo.nWidth * 4);
											nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx] * (-cLayerInfo.nShiftPosition));
											nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 1] * (-cLayerInfo.nShiftPosition));
											nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 2] * (-cLayerInfo.nShiftPosition));
											nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 3] * (-cLayerInfo.nShiftPosition));
										}
										else
										{
											nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition));
											nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition));
											nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition));
											nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition));
										}
									}
								}
								else  // движение горизонтальное
								{
									if (0 > cLayerInfo.nShiftPosition || 0 > cLayerInfo.nShiftTotal) // значит движение было влево (определяем по двум, т.к. могло попасть ровно пиксель в пиксель)
									{
										int nRowBeginingIndx = cLLI.nFGLineBeginning * 4;                
										if (0 == ((cLLI.nBgFgLinesDelta + cLayerInfo.nOffsetTop) & 1))   // -----в dvPal это та по чётности строка, которая первой должна показывааться! Т.е. половина движения
										{
											double nHalf = (cLayerInfo.nShiftTotal / 2) + cLayerInfo.nShiftPosition;
											int nDeltaPx = (int)nHalf;
											double nNewShift = nHalf - nDeltaPx;

											nFGIndx = nFGIndx + 4 * nDeltaPx;
											int nLeftPxIndx = nFGIndx - 4;

											if (nLeftPxIndx >= nRowBeginingIndx)
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx] * (-nNewShift));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1] * (-nNewShift));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2] * (-nNewShift));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] * (-nNewShift));
											}
											else if (nFGIndx >= nRowBeginingIndx)
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + nNewShift));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + nNewShift));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + nNewShift));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + nNewShift));
											}
										}
										else   // -----в dvPal это та по чётности строка, которая второй должна показывааться! Т.е. целое движение
										{   // берем инфу с двух соседних пикселей в пропорции шифта - с этого и с пикселя слева от этого. Т.к. был перелет из-за отбрасывания дробной части от X
											//  this_pixel = this_pixel * (1+shift)  + left_pixel * (-shift)    тут   shift<0    |shift|<1
											int nLeftPxIndx = nFGIndx - 4;
											if (nLeftPxIndx >= nRowBeginingIndx) // левый пиксель ещё в нашей строке
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx] * (-cLayerInfo.nShiftPosition));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1] * (-cLayerInfo.nShiftPosition));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2] * (-cLayerInfo.nShiftPosition));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] * (-cLayerInfo.nShiftPosition));
											}
											else // если наш пиксель первый в строке - он просто "ослабнет"
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition));
											}
										}
										//nColumn = nIndxIndent - cLayerInfo.nCropLeft;
									}
									else   // значит движение было вправо
									{
									}
								}
							}
							nPixelAlpha = cLayerInfo.nAlphaConstant;

							if (255 == nPixelAlpha)
								nPixelAlpha = nFGColorAlpha;
							else if (0 == nFGColorAlpha)
								nPixelAlpha = 0;
							else if (0 < nPixelAlpha && 255 > nFGColorAlpha) // объединение альфы слоя с константной альфой !!!!
								nPixelAlpha = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
						}
						else
							nPixelAlpha = 255;
						if (0 < nPixelAlpha)
						{
							if (255 == nPixelAlpha || 0 == _cDisComProcessing._aLayers[0][nBGIndxAlpha])
							{
								_cDisComProcessing._aLayers[0][nBGIndxRed] = nFGColorRed;
								_cDisComProcessing._aLayers[0][nBGIndxGreen] = nFGColorGreen;
								_cDisComProcessing._aLayers[0][nBGIndxBlue] = nFGColorBlue;
							}
							else
							{                           //индекс меньше, т.к. 0-е значение альфы мы не считаем и все индексы сдвинулись...
								nPixelAlphaIndx = nPixelAlpha - 1;
								_cDisComProcessing._aLayers[0][nBGIndxRed] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxRed], nFGColorRed];
								_cDisComProcessing._aLayers[0][nBGIndxGreen] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxGreen], nFGColorGreen];
								_cDisComProcessing._aLayers[0][nBGIndxBlue] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxBlue], nFGColorBlue];
							}
							if (_cDisComProcessing._aLayers[0][nBGIndxAlpha] < nPixelAlpha)   // очередная попытка примирить альфу с действительностью ))
								_cDisComProcessing._aLayers[0][nBGIndxAlpha] = nPixelAlpha;
						}
					}
					else
						nMaskIndx = -1;
				}
			}
		}










        static private void Merging(int nBGIndxPixel)
		{
			MergeInfo cMergeInfo = (MergeInfo)_cDisComProcessing._cInfo;
			if (nBGIndxPixel < cMergeInfo.nBackgroundSize) //2-й - это размер BG, 3-й - ширина BG, 4-й - делать ли задник? 5-й - инфа про FG1; 
			{									//Периодичность PRECOMPUTED_INFO_PERIOD - 1-й. 0-й - это количество слоёв
				int M, nIndxIndent, nRow;
				int nBGIndxRed, nBGIndxGreen, nBGIndxBlue, nBGIndxAlpha, nFGIndx;
				byte nFGColorRed = 0, nFGColorGreen = 0, nFGColorBlue = 0, nFGColorAlpha = 0;
				int nNextIndxRed, nNextIndxGreen, nNextIndxBlue, nNextIndxAlpha, nPixelAlphaIndx;
                byte nPixelAlpha;
                int nMaskIndx = -1;
				LayerInfo cLayerInfo;

				M = nBGIndxPixel / cMergeInfo.nBackgroundWidth; //M=(int)(BI/BW) т.е. с отбрасыванием дробной части. это полных строк над нами
				nIndxIndent = nBGIndxPixel - M * cMergeInfo.nBackgroundWidth;
				nBGIndxRed = nBGIndxPixel * 4;
				nBGIndxGreen = nBGIndxRed + 1;
				nBGIndxBlue = nBGIndxRed + 2;
				nBGIndxAlpha = nBGIndxRed + 3;
				_cDisComProcessing._aLayers[0][nBGIndxRed] = 0;
				_cDisComProcessing._aLayers[0][nBGIndxGreen] = 0;
				_cDisComProcessing._aLayers[0][nBGIndxBlue] = 0;
                if(1 == cMergeInfo.nBackgroundAlphaType)
                    nMaskIndx = nBGIndxAlpha;
                else
                    _cDisComProcessing._aLayers[0][nBGIndxAlpha] = cMergeInfo.nBackgroundAlphaType;

                for (ushort nLayerIndx = 1; cMergeInfo.nLayersQty > nLayerIndx; nLayerIndx++)
				{
					cLayerInfo = cMergeInfo.aLayerInfos[(int)(nLayerIndx - 1)];
					if ((nBGIndxPixel >= cLayerInfo.nBackgroundStart) && (nBGIndxPixel <= cLayerInfo.nBackgroundStop) && (nIndxIndent >= cLayerInfo.nCropLeft) && (nIndxIndent <= cLayerInfo.nCropRight))
					{
						nFGIndx = (nBGIndxPixel + M * cLayerInfo.nWidthDiff - cLayerInfo.nForegroundStart) * 4;
						//формулу см. в методе Intersect.
						if (1 == cLayerInfo.nAlphaType) //леер является маской
						{
							if (_cDisComProcessing._aLayers[nLayerIndx].Length - 1 < (nMaskIndx = nFGIndx + 3))
								nMaskIndx = -1;
							continue;
						}
						nFGColorAlpha = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3];
						if (-1 < nMaskIndx) //применяем маску
						{
							if (255 == _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx]) //отрезали пиксел по маске
							{
								nMaskIndx = -1;
								continue;
							}
							else if(0 < _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx])
								nFGColorAlpha = (byte)(nFGColorAlpha * (1 - _cDisComProcessing._aLayers[nLayerIndx - 1][nMaskIndx] / 255f) + 0.5);
							nMaskIndx = -1;
						}
						nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx];
						nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1];
						nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2];

						if (0 == cLayerInfo.nAlphaType) // т.е. наш слой не альфирующий, а обычный слой с альфой RGBA
						{
							if (0 != cLayerInfo.nShiftPosition || 0 != cLayerInfo.nShiftTotal)// обработка расположения между пикселями. Берем инфу с того места откуда она приехала в этот пиксель
							{
								if (cLayerInfo.bShiftVertical)   // для вертикальных cLayerInfo.nShiftTotal == 0  
								{
									if (0 < cLayerInfo.nShiftPosition)
									{
										nPixelAlpha = nFGColorAlpha;
										nFGColorAlpha = (byte)((nFGColorAlpha + 1) * (1 - cLayerInfo.nShiftPosition));
										nRow = M - (cLayerInfo.nBackgroundStart / cMergeInfo.nBackgroundWidth);
										if (nRow < (cLayerInfo.nCropHeight - 1))
										{
											nNextIndxRed = nFGIndx + (cLayerInfo.nWidth * 4);
											nNextIndxGreen = nNextIndxRed + 1;
											nNextIndxBlue = nNextIndxRed + 2;
											nNextIndxAlpha = nNextIndxRed + 3;
											if (0 < _cDisComProcessing._aLayers[nLayerIndx][nNextIndxAlpha])
											{
												if (0 < (nPixelAlpha = (byte)((_cDisComProcessing._aLayers[nLayerIndx][nNextIndxAlpha] + 1) * cLayerInfo.nShiftPosition)))
												{
													if (0 == nFGColorAlpha || 254 < nPixelAlpha)
													{
														nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed];
														nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxGreen];
														nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxBlue];
													}
													else
													{
														nPixelAlphaIndx = nPixelAlpha - 1;
														nFGColorRed = _aAlphaMap[nPixelAlphaIndx, nFGColorRed, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed]];
														nFGColorGreen = _aAlphaMap[nPixelAlphaIndx, nFGColorGreen, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxGreen]];
														nFGColorBlue = _aAlphaMap[nPixelAlphaIndx, nFGColorBlue, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxBlue]];
													}
												}
												if (255 < nFGColorAlpha + nPixelAlpha)
													nFGColorAlpha = 255;
												else
													nFGColorAlpha += nPixelAlpha;
											}
										}
									}
									else // тестовый элз - это если в роле не 1 - (....)   а просто - (.....)  в шифт идёт. тут другой способ.
									{
										nRow = M - (cLayerInfo.nBackgroundStart / cMergeInfo.nBackgroundWidth);
										if (nRow > 0)
										{
											int nUpPxIndx = nFGIndx - (cLayerInfo.nWidth * 4);
											nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx] * (-cLayerInfo.nShiftPosition));
											nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 1] * (-cLayerInfo.nShiftPosition));
											nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 2] * (-cLayerInfo.nShiftPosition));
											nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 3] * (-cLayerInfo.nShiftPosition));
										}
										else
										{
											nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition));
											nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition));
											nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition));
											nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition));
										}
									}
								}
								else  // движение горизонтальное
								{
									if (0 > cLayerInfo.nShiftPosition || 0 > cLayerInfo.nShiftTotal) // значит движение было влево (определяем по двум, т.к. могло попасть ровно пиксель в пиксель)
									{
										nRow = M - (cLayerInfo.nBackgroundStart / cMergeInfo.nBackgroundWidth);  //полных строк над нами - полных строк над кропом
										int nRowBeginingIndx = ((int)((double)nFGIndx / 4 / cLayerInfo.nWidth)) * cLayerInfo.nWidth * 4;
										if (0 == ((nRow + cLayerInfo.nOffsetTop) & 1))   // -----в dvPal это та по чётности строка, которая первой должна показывааться! Т.е. половина движения
										{
											double nHalf = (cLayerInfo.nShiftTotal / 2) + cLayerInfo.nShiftPosition;
											int nDeltaPx = (int)nHalf;
											double nNewShift = nHalf - nDeltaPx;

											nFGIndx = nFGIndx + 4 * nDeltaPx;
											int nLeftPxIndx = nFGIndx - 4;

											if (nLeftPxIndx >= nRowBeginingIndx)
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx] * (-nNewShift));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1] * (-nNewShift));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2] * (-nNewShift));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + nNewShift) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] * (-nNewShift));
											}
											else if (nFGIndx >= nRowBeginingIndx)
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + nNewShift));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + nNewShift));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + nNewShift));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + nNewShift));
											}
										}
										else   // -----в dvPal это та по чётности строка, которая второй должна показывааться! Т.е. целое движение
										{	// берем инфу с двух соседних пикселей в пропорции шифта - с этого и с пикселя слева от этого. Т.к. был перелет из-за отбрасывания дробной части от X
											//  this_pixel = this_pixel * (1+shift)  + left_pixel * (-shift)    тут   shift<0    |shift|<1
											int nLeftPxIndx = nFGIndx - 4;
											if (nLeftPxIndx >= nRowBeginingIndx) // левый пиксель ещё в нашей строке
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx] * (-cLayerInfo.nShiftPosition));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1] * (-cLayerInfo.nShiftPosition));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2] * (-cLayerInfo.nShiftPosition));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition) + _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] * (-cLayerInfo.nShiftPosition));
											}
											else // если наш пиксель первый в строке - он просто "ослабнет"
											{
												nFGColorRed = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx] * (1 + cLayerInfo.nShiftPosition));
												nFGColorGreen = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 1] * (1 + cLayerInfo.nShiftPosition));
												nFGColorBlue = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 2] * (1 + cLayerInfo.nShiftPosition));
												nFGColorAlpha = (byte)(_cDisComProcessing._aLayers[nLayerIndx][nFGIndx + 3] * (1 + cLayerInfo.nShiftPosition));
											}
										}
										//nColumn = nIndxIndent - cLayerInfo.nCropLeft;
									}
									else   // значит движение было вправо
									{
									}
								}
							}
							nPixelAlpha = cLayerInfo.nAlphaConstant;

							if (255 == nPixelAlpha)
								nPixelAlpha = nFGColorAlpha;
							else if (0 == nFGColorAlpha)
								nPixelAlpha = 0;
							else if (0 < nPixelAlpha && 255 > nFGColorAlpha) // объединение альфы слоя с константной альфой !!!!
								nPixelAlpha = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
						}
						else
							nPixelAlpha = 255;
						if (0 < nPixelAlpha)
						{
							if (255 == nPixelAlpha || 0 == _cDisComProcessing._aLayers[0][nBGIndxAlpha])
							{
								_cDisComProcessing._aLayers[0][nBGIndxRed] = nFGColorRed;
								_cDisComProcessing._aLayers[0][nBGIndxGreen] = nFGColorGreen;
								_cDisComProcessing._aLayers[0][nBGIndxBlue] = nFGColorBlue;
							}
							else
							{							//индекс меньше, т.к. 0-е значение альфы мы не считаем и все индексы сдвинулись...
								nPixelAlphaIndx = nPixelAlpha - 1;
								_cDisComProcessing._aLayers[0][nBGIndxRed] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxRed], nFGColorRed];
								_cDisComProcessing._aLayers[0][nBGIndxGreen] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxGreen], nFGColorGreen];
								_cDisComProcessing._aLayers[0][nBGIndxBlue] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxBlue], nFGColorBlue];
							}
							if (_cDisComProcessing._aLayers[0][nBGIndxAlpha] < nPixelAlpha)   // очередная попытка примирить альфу с действительностью ))
								_cDisComProcessing._aLayers[0][nBGIndxAlpha] = nPixelAlpha;
						}
					}
					else
						nMaskIndx = -1;

				}
			}
		}
		//static private void Merging(int nBGIndxPixel)
		//{
		//    MergeInfo cMergeInfo = (MergeInfo)_cDisComProcessing._cInfo;
		//    if (nBGIndxPixel < cMergeInfo.nBackgroundSize) //2-й - это размер BG, 3-й - ширина BG, 4-й - делать ли задник? 5-й - инфа про FG1; 
		//    {									//Периодичность PRECOMPUTED_INFO_PERIOD - 1-й. 0-й - это количество слоёв
		//        int M, nIndxIndent, nRow;
		//        int nBGIndxRed, nFGIndx;
		//        byte nFGColorRed = 0, nFGColorAlpha = 0;
		//        int nNextIndxRed;
		//        byte nPixelAlpha;
		//        LayerInfo cLayerInfo;

		//        M = nBGIndxPixel / cMergeInfo.nBackgroundWidth; //M=(int)(BI/BW) т.е. с отбрасыванием дробной части.
		//        nIndxIndent = nBGIndxPixel - M * cMergeInfo.nBackgroundWidth;
		//        nBGIndxRed = nBGIndxPixel;
		//        _cDisComProcessing._aLayers[0][nBGIndxRed] = 0;

		//        for (ushort nLayerIndx = 1; nLayerIndx < cMergeInfo.nLayersQty; nLayerIndx++)
		//        {
		//            cLayerInfo = cMergeInfo.aLayerInfos[(int)(nLayerIndx - 1)];

		//            if ((nBGIndxPixel >= cLayerInfo.nBackgroundStart) && (nBGIndxPixel <= cLayerInfo.nBackgroundStop) && (nIndxIndent >= cLayerInfo.nCropLeft) && (nIndxIndent <= cLayerInfo.nCropRight))
		//            {
		//                nFGIndx = (nBGIndxPixel + M * cLayerInfo.nWidthDiff - cLayerInfo.nForegroundStart);
		//                //формулу см. в методе Intersect.
		//                nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndx];
		//                nFGColorAlpha = 255;

		//                if (0 != cLayerInfo.nShiftPosition && 1 > cLayerInfo.nShiftPosition && -1 < cLayerInfo.nShiftPosition)
		//                {
		//                    if (cLayerInfo.bShiftVertical)
		//                    {
		//                        if (0 < cLayerInfo.nShiftPosition)
		//                        {
		//                            nPixelAlpha = nFGColorAlpha;
		//                            nFGColorAlpha = (byte)((nFGColorAlpha) * (1 - cLayerInfo.nShiftPosition));  //nFGColorAlpha+1
		//                            nRow = M - (cLayerInfo.nBackgroundStart / cMergeInfo.nBackgroundWidth);
		//                            if (nRow < (cLayerInfo.nCropHeight - 1))
		//                            {
		//                                nNextIndxRed = nFGIndx + (cLayerInfo.nWidth);
		//                                if (0 < 255)
		//                                {
		//                                    if (0 < (nPixelAlpha = (byte)((255) * cLayerInfo.nShiftPosition)))   //255+1
		//                                    {
		//                                        if (0 == nFGColorAlpha || 254 < nPixelAlpha)
		//                                        {
		//                                            nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed];
		//                                        }
		//                                        else
		//                                        {
		//                                            nFGColorRed = _aAlphaMap[nPixelAlpha - 1, nFGColorRed, _cDisComProcessing._aLayers[nLayerIndx][nNextIndxRed]];
		//                                        }
		//                                    }
		//                                    if (255 < nFGColorAlpha + nPixelAlpha)
		//                                        nFGColorAlpha = 255;
		//                                    else
		//                                        nFGColorAlpha += nPixelAlpha;
		//                                }
		//                            }
		//                        }
		//                        else
		//                        {
		//                        }
		//                    }
		//                    else
		//                    {
		//                        if (0 < cLayerInfo.nShiftPosition)
		//                        {
		//                            //nColumn = nIndxIndent - cLayerInfo.nCropLeft;
		//                        }
		//                        else
		//                        {
		//                        }
		//                    }
		//                }

		//                nPixelAlpha = cLayerInfo.nAlphaConstant;

		//                if (255 == nPixelAlpha)
		//                    nPixelAlpha = nFGColorAlpha;
		//                else if (0 == nFGColorAlpha)
		//                    nPixelAlpha = 0;
		//                else if (0 < nPixelAlpha && 255 > nFGColorAlpha)                        // объединение альфы слоя с константной альфой !!!!
		//                    nPixelAlpha = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);

		//                if (0 < nPixelAlpha)
		//                {
		//                    if (255 == nPixelAlpha || 0 == 255)
		//                    {
		//                        _cDisComProcessing._aLayers[0][nBGIndxRed] = nFGColorRed;
		//                    }
		//                    else
		//                    {							//индекс меньше, т.к. 0-е значение альфы мы не считаем и все индексы сдвинулись...
		//                        _cDisComProcessing._aLayers[0][nBGIndxRed] = _aAlphaMap[nPixelAlpha - 1, _cDisComProcessing._aLayers[0][nBGIndxRed], nFGColorRed];
		//                    }
		//                }
		//            }

		//        }
		//    }
		//}
		//1b
    }
}
