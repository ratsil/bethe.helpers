//#define DEBUG_LISTAR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Runtime;
using SIO = System.IO;

namespace ffmpeg.net
{
	abstract public class File
	{
		public enum Flags
		{
			None = 0,
			GlobalHeader = 1
		}
		public class Input : File
		{
			#region members

			private Frame _cFrameVideo;
			private Frame _cFrameAudio;
			private byte[] __aBytesRemainder;
			private byte[] _aBytesRemainder
			{
				get { return __aBytesRemainder; }
				set
				{
					lock (_ahBytesStorage)
					{
						if (__aBytesRemainder != null && __aBytesRemainder.Length > 0)
						{
							BytesBack(__aBytesRemainder, 1);
						}
						__aBytesRemainder = value;
					}
				}
			}
			private byte[] __aPacketBytes;
			private byte[] _aPacketBytes
			{
				get { return __aPacketBytes; }
				set
				{
					if (__aPacketBytes != null && __aPacketBytes.Length > 0)
					{
						BytesBack(__aPacketBytes, 2);
					}
					__aPacketBytes = value;
				}
			}
			private int _nRemainderSize;
			private IntPtr _pPacketAudio;
			private IntPtr _pPacketAudioDub;
			private AVPacket _stPacketAudio;

			private int _nVideoStreamIndx;
			private int _nAudioStreamIndx;

			private Queue<IntPtr> _aqAudioPackets;
			private Queue<IntPtr> _aqVideoPackets;

			private int _nDecodedFramesInPrepare;
			private int _nPreparedFramesIndx;
			private int _nPacketIndx; //logging
			private IntPtr _pPacket;
			private ulong _nTotalVideoPackets;
			private ulong _nTotalAudioPackets;
			private ulong _nTotalDataAndOtherPackets;
			private Format.Video _cFormatVideoTarget;
			private Format.Audio _cFormatAudioTarget;
			private bool _bClosed;
			private bool _bFileEnd;
            private bool _bWasClearMem;
            private bool _bFrameGettingStarted;

            public enum PlaybackMode
            {
                RealTime,               // пакеты берутся заблаговременно для всех play and prepared (и не одновременно, что хорошо для HDD, а последовательно), а декод происходит параллельно друг другу и пакетам в несколько потоков
                GivesFrameOnDemand,     // не берутся пакеты заранее, а только по востребованию очередного кадра. Юзать, если нельзя мешать реалтайму (из плагина, напр.) или если не критично.
            }
            public class FramesRotation
			{
				public enum Type
				{
					normal,
					empty,
					process
				}
				private static List<FramesRotation> _aFramesFree;
				private static Dictionary<int, bool> _ahHashPassedOut;
				private static List<int> _aAllFramesHashes;
				private static ulong _nTotalSize;
				private static uint _nTotalCount;
                private Format _cFormat;
				private List<int> _aFramesHashes;
				private Queue<Frame> _aqFrames;
				private List<int> _aFramesEmptyHashes;
				private List<int> _aFramesProcessHashes;
				private Queue<Frame> _aqFramesEmpty; // иногда нужны бывают для чистого использования без изменений, как пустые болванки
				private Queue<Frame> _aqFramesProcess;  // format.cs  358
				static private DateTime _dtNextInfo;
				static FramesRotation()
				{
					_aFramesFree = new List<FramesRotation>();
					_ahHashPassedOut = new Dictionary<int, bool>();
					_nTotalSize = 0;
					_nTotalCount = 0;
                    _dtNextInfo = DateTime.Now.AddMinutes(5);
                }
				private FramesRotation()
				{
					_aqFrames = new Queue<Frame>();
					_aqFramesEmpty = new Queue<Frame>();
					_aqFramesProcess = new Queue<Frame>();
					_aAllFramesHashes = new List<int>();
					_aFramesHashes = new List<int>();
					_aFramesEmptyHashes = new List<int>();
					_aFramesProcessHashes = new List<int>();
				}
				private FramesRotation(Format cFormat)
					: this()
				{
					this._cFormat = cFormat;
				}
				private Frame Dequeue(Type eType)
				{
					switch (eType)
					{
						case Type.normal:
							return (_aqFrames.Count > 0 ? _aqFrames.Dequeue() : null);
						case Type.empty:
							return (_aqFramesEmpty.Count > 0 ? _aqFramesEmpty.Dequeue() : null);
						case Type.process:
							return (_aqFramesProcess.Count > 0 ? _aqFramesProcess.Dequeue() : null);
						default:
							return null;
					}
				}
				public static Frame Dequeue(Format cF, byte nFrom)
				{
					return Dequeue(cF, Type.normal, nFrom);
				}
				public static Frame Dequeue(Format cFormat, Type eType, byte nFrom)
				{
					Frame cRetVal;
					lock (_aFramesFree)
					{
						FramesRotation cFR = _aFramesFree.FirstOrDefault(o => o._cFormat.IsAlikeTo(cFormat));

						if (null != cFR && null != (cRetVal = cFR.Dequeue(eType)))
						{
							if (_ahHashPassedOut[cRetVal.GetHashCode()] == false)
								_ahHashPassedOut[cRetVal.GetHashCode()] = true;
							else
								(new Logger()).WriteDebug("device.bytes error - already passed out!! (from = " + nFrom + ")");
							return cRetVal;
						}
						else
						{
							if (null == cFR)
							{
								cFR = new FramesRotation(cFormat);
								_aFramesFree.Add(cFR);
								(new Logger()).WriteDebug2("new format added (from = " + nFrom + ") [" + cFormat.nBufferSize + "]");
							}
							cRetVal = new Frame(cFormat);
                            int nHashC = cRetVal.GetHashCode();

							if (cFormat is Format.Video)
								cRetVal.Disposing += Input.cFrameVideo_Disposing;
							else
								cRetVal.Disposing += Input.cFrameAudio_Disposing;

							switch (eType)
							{
								case Type.normal:
									cFR._aFramesHashes.Add(nHashC);
									break;
								case Type.empty:
									cFR._aFramesEmptyHashes.Add(nHashC);
									break;
								case Type.process:
									cFR._aFramesProcessHashes.Add(nHashC);
									break;
							}
							_nTotalCount++;
                            _nTotalSize += (uint)cFormat.nBufferSize;
                            _aAllFramesHashes.Add(nHashC);
                            _ahHashPassedOut.Add(nHashC, true);
							(new Logger()).WriteDebug4("frame added (from = " + nFrom + ") [type=" + eType + "][fmt_buf=" + cFormat.nBufferSize + "][hc=" + nHashC + "][fr_len="+ cRetVal.nLengthBuffer + "]");
							if (DateTime.Now > _dtNextInfo)
							{
								(new Logger()).WriteDebug("frames info [" + FramesRotation.InfoGet() + "]");
								_dtNextInfo = DateTime.Now.AddMinutes(5);
							}
							return cRetVal;
						}
					}
				}
				public static void Enqueue(Frame cF, byte nFrom)
				{
					lock (_aFramesFree)
					{
						Type eType = Type.normal;
                        int nHashC = cF.GetHashCode();
                        FramesRotation cFR = _aFramesFree.FirstOrDefault(o => o._aFramesHashes.Contains(nHashC));
						if (null == cFR)
						{
							eType = Type.empty;
							cFR = _aFramesFree.FirstOrDefault(o => o._aFramesEmptyHashes.Contains(nHashC));
							if (null == cFR)
							{
								eType = Type.process;
								cFR = _aFramesFree.FirstOrDefault(o => o._aFramesProcessHashes.Contains(nHashC));
								if (null == cFR)
								{
									(new Logger()).WriteDebug("freeframe. error returning frame is not our frame! [" + cF.nLengthBuffer + "][hc=" + nHashC + "][fr_len=" + cF.nLengthBuffer + "][from=" + nFrom + "]");
									cF.Dispose(true);
									return;
								}
								else
									(new Logger()).WriteDebug2("freeframe.returning process frame[" + cF.nLengthBuffer + "][hc=" + nHashC + "][fr_len=" + cF.nLengthBuffer + "][from=" + nFrom + "]");
							}
							else
								(new Logger()).WriteDebug2("freeframe.returning empty frame[" + cF.nLengthBuffer + "][hc=" + nHashC + "][fr_len=" + cF.nLengthBuffer + "][from=" + nFrom + "]");
						}
						cF.nLength = cF.nLengthBuffer;
						if (_ahHashPassedOut[nHashC])
						{
							_ahHashPassedOut[nHashC] = false;
							if (eType == Type.empty)
								cFR._aqFramesEmpty.Enqueue(cF);
							else if (eType == Type.process)
								cFR._aqFramesProcess.Enqueue(cF);
							else
								cFR._aqFrames.Enqueue(cF);
						}
						else
							(new Logger()).WriteDebug("freeframe error - received twice!!! (size=" + cF.nLengthBuffer + ")[from=" + nFrom + "][type=" + eType + "]");
					}
				}
				private static string InfoGet()
				{
					string sRetVal;
					lock (_aFramesFree)
                    {
                        sRetVal = "frames_free: [sizes=" + _aFramesFree.Count + "][count_total = " + _nTotalCount + "][buff_total=" + _nTotalSize + "]";
                        foreach (FramesRotation cFR in _aFramesFree)
							sRetVal += "[" + cFR._cFormat.nBufferSize + " unused_now = " + cFR._aqFrames.Count + "]";
					}
					return sRetVal;
				}
			}
			private Queue<Frame> _aqVideoFrames;
			private Queue<Frame> _aqVideoFramesFree;
			private Queue<Frame> _aqAudioFrames;
			private Queue<Frame> _aqAudioFramesFree;

            static private Dictionary<int, Queue<byte[]>> _ahBytesStorage; //DNF не забыть если удачно, то чистить его периодически!!!!
			static private Dictionary<int, bool> _ahHashPassedOut;
			static private List<int> _aBytesHashes;
			static private int nNumSizes = 0, nNumTotal = 0, nHash;
			static private long nBytesTotal = 0;
			static private byte[] aBGRetVal;
			static private byte[] BytesGet(int nSize, byte nFrom)
			{
				lock (_ahBytesStorage)
				{
					if (_ahBytesStorage.Keys.Contains(nSize) && 0 < _ahBytesStorage[nSize].Count)
					{
						nHash = _ahBytesStorage[nSize].Peek().GetHashCode();
						if (_aBytesHashes.Contains(nHash))
						{
							if (_ahHashPassedOut[nHash] == false)
								_ahHashPassedOut[nHash] = true;
							else
								(new Logger()).WriteDebug("ffmpeg.bytes error - already passed out!! (from = " + nFrom + ")");
							return _ahBytesStorage[nSize].Dequeue();
						}
						(new Logger()).WriteDebug("ffmpeg.bytes error - not in hashes! (from = " + nFrom + ")");
					}
					else
					{
						if (!_ahBytesStorage.Keys.Contains(nSize))
						{
							(new Logger()).WriteDebug("ffmpeg.bytes adding new size to bytes storage (from = " + nFrom + ") [" + nSize + "]");
							nNumSizes++;
							_ahBytesStorage.Add(nSize, new Queue<byte[]>());
						}
						nBytesTotal += nSize;
						nNumTotal++;
						aBGRetVal = new byte[nSize];
						while (_aBytesHashes.Contains(aBGRetVal.GetHashCode()))
						{
							(new Logger()).WriteDebug("ffmpeg.bytes ERROR returning new byte array WITH THE SAME HASH!!! - will try to get another one (from=" + nFrom + ")[hc=" + aBGRetVal.GetHashCode() + "][" + nSize + "]");
							aBGRetVal = new byte[nSize];
						}
						(new Logger()).WriteDebug("ffmpeg.bytes is returning new byte (from = " + nFrom + ") [hc=" + aBGRetVal.GetHashCode() + "][" + nSize + "][sizes=" + nNumSizes + "][total=" + nNumTotal + "(" + _aBytesHashes.Count() + ")][bytes=" + nBytesTotal + "]");
						_aBytesHashes.Add(aBGRetVal.GetHashCode());
						_ahHashPassedOut.Add(aBGRetVal.GetHashCode(), true);
						return aBGRetVal;
					}
					throw new Exception("bytes get is impossible");
				}
			}
			static private void BytesBack(byte[] aBytes, byte nFrom)  
			{
				if (null == aBytes)
				{
					(new Logger()).WriteDebug("ffmpeg.bytes error - received NULL bytes! (from=" + nFrom + ")");
					return;
				}
				lock (_ahBytesStorage)
				{
					if (_aBytesHashes.Contains(aBytes.GetHashCode()))
						if (_ahHashPassedOut[aBytes.GetHashCode()])
						{
							_ahHashPassedOut[aBytes.GetHashCode()] = false;
							_ahBytesStorage[aBytes.Length].Enqueue(aBytes);
						}
						else
							(new Logger()).WriteDebug("ffmpeg.bytes error - received twice!!!! (from=" + nFrom + ")");
					else
						(new Logger()).WriteDebug("ffmpeg.bytes error - received not our bytes!(from=" + nFrom + ") [hc=" + aBytes.GetHashCode() + "][size=" + aBytes.Length + "]");
				}
			}
            static private int _nFreezeTimeoutTaskSeconds;
            static private int _nFreezeTimeoutPacketsGetSeconds; // 200MB block takes 0.5-1.5 sec normal. Max - 3 sec. If you have bad raid (etc) - you should change _nQueueFfmpegLength too
            static private int nPreparedFramesMinimum = 20;
            static public int nFreezeTimeoutPacketsGetSeconds
            {
                set
                {
                    _nFreezeTimeoutPacketsGetSeconds = value;
                    _nFreezeTimeoutTaskSeconds = value + 10;
                }
            }

            private System.Threading.Thread _cThreadWritingFramesWorker;     //-!-make static?? -!-
			private bool _bDoWritingFrames;
			private Queue<byte[]> _aqWritingFrames;
			private Queue<byte[]> _aqWritingAudioFrames;
			private int nIndxGCForced;
			private int nCacheNow;
			private object oDisposeLock;
			private Logger.Timings _cTimingsFDV;
			private Logger.Timings _cTimingsAFTQ;
			private Logger.Timings _cTimingsFNVG;
			private Logger.Timings _cTimingsFDA;
			private Logger.Timings _cTimingsPAD;

			private int[] _aPacket_streamindex;
			private int[] _aPacket_size;
			private int[] _aPacket_size_Audio;
			private int[] _aPacket_size_AudioDub;
			private IntPtr[] _aPacket_data_Audio;
			private IntPtr[] _aPacket_data_AudioDub;

			private int _nOffset_stream_index;
			private int _nOffset_size;
			private int _nOffset_data;
			private System.Collections.Concurrent.ConcurrentQueue<IntPtr> _aqPackets;
			private bool _bPacketsReadingDone;
            private bool _bPacketsReadingSuccess;
            private int _nTotalPacketsQty;
			private long _nTotalPacketsSize;
            private long _nCurrentBlockSize;
            private PlaybackMode _eFramesGettingMode;
			static private byte[] aBytesSilent;

            public int nCacheSize;
            public long nBlockSize;
            static public int nCacheSizeCommon;
			static public long nBlockSizeCommon;
			static public long nDecodingThreads;
			static public uint nBTLCurrentBuffer;
			static public int nBTLBufferTwoThird;
			static public int nBTLBufferOneThird;
			static public string sDebugFolder;

			public bool bCached;
            public bool bPrepared;
            public bool bFileEnd
			{
				get
				{
					bool bRetVal = _bFileEnd || _bClosed;
					if (bRetVal && null != _aqVideoFrames)
						lock (_aqVideoFrames)
							bRetVal = (1 > _aqVideoFrames.Count);
					if (bRetVal && null != _aqAudioFrames)
						lock (_aqAudioFrames)
							bRetVal = (1 > _aqAudioFrames.Count);
					return bRetVal;
				}
			}
			public bool bFileEndless; //для потокового видео... поток байт может опустошиться, но это не означает конец файла
			public bool bFramesStarvation
			{
				get
                {
                    if (_bFileEnd || (null == _aqAudioFrames && null == _aqVideoFrames))
                        return false;
					if (null == _aqVideoFrames)
						lock (_aqAudioFrames)   //"голодание", если очередь заполнена меньше, чем на 20%
							return ((nCacheSize * 0.2) > _aqAudioFrames.Count);

					lock (_aqVideoFrames)   //"голодание", если очередь заполнена меньше, чем на 20%
						return ((nCacheSize * 0.2) > _aqVideoFrames.Count);
				}
			}
			public int nCueueLength
			{
				get
				{
					if (null == _aqAudioFrames && null == _aqVideoFrames)
						return -3;
					if (null != _aqVideoFrames)
						return _aqVideoFrames.Count;
					return _aqAudioFrames.Count;
				}
			}

			public ulong nFramesQty { get; private set; }
			public ushort nFramesPerSecond { get; private set; }



			#endregion
			static Input()
			{
				_ahBytesStorage = new Dictionary<int, Queue<byte[]>>();
				_ahHashPassedOut = new Dictionary<int, bool>();
				_aBytesHashes = new List<int>();
				aBytesSilent = new byte[10000];
			}
			private Input()
				: base()
			{
				try
				{
                    nBlockSize = nBlockSizeCommon;
                    nCacheSize = nCacheSizeCommon;
                    _nPacketIndx = 0; //logging
					_nTotalVideoPackets = 0;
					_nTotalAudioPackets = 0;
					_bClosed = false;
					_bFileEnd = false;
					nFramesPerSecond = 25; //FPS

					bCached = false;
                    bPrepared = false;

					_nVideoStreamIndx = -1;
					_nAudioStreamIndx = -1;

					oDisposeLock = new object();

					_cTimingsFDV = new Logger.Timings("ffmpeg: file");
					_cTimingsAFTQ = new Logger.Timings("ffmpeg: file", helpers.Logger.Level.debug2);
					_cTimingsFNVG = new Logger.Timings("ffmpeg: file");
					_cTimingsFDA = new Logger.Timings("ffmpeg: file");
					_cTimingsPAD = new Logger.Timings("ffmpeg: file");

					_aPacket_streamindex = new int[1];
					_nOffset_stream_index = Marshal.OffsetOf(typeof(AVPacket), "stream_index").ToInt32();
					_nOffset_size = Marshal.OffsetOf(typeof(AVPacket), "size").ToInt32();
					_nOffset_data = Marshal.OffsetOf(typeof(AVPacket), "data").ToInt32();
					_aPacket_size_Audio = new int[1];
					_aPacket_size_AudioDub = new int[1];
					_aPacket_data_Audio = new IntPtr[1];
					_aPacket_data_AudioDub = new IntPtr[1];

					_aqPackets = new System.Collections.Concurrent.ConcurrentQueue<IntPtr>();
					_bPacketsReadingDone = false;
					_aPacket_size = new int[1];
					_nTotalPacketsQty = 0;
					_nTotalPacketsSize = 0;


				}
				catch
				{
					Dispose();
					throw;
				}
			}

			public Input(string sFile)
				: this(sFile, 0, null)
			{ }
			public Input(string sFile, string sFormat)
				: this(sFile, 0, sFormat)
			{ }
			public Input(string sFile, ulong nFrameStart)
				: this(sFile, nFrameStart, null)
			{ }
			public Input(string sFile, ulong nFrameStart, string sFormat)
				: this()
			{
				_sFile = sFile;
				_cFormatCtx = AVFormatContext.OpenInput(_sFile, sFormat);
				_cFormatCtx.StreamInfoFind();
				Init(nFrameStart);
			}
			public Input(SIO.Stream cStream)
				: this(cStream, 0)
			{
			}
			public Input(SIO.Stream cStream, ulong nFrameStart)
				: this(cStream, nFrameStart, null)
			{ }
			public Input(SIO.Stream cStream, string sFormat)
				: this(cStream, 0, sFormat)
			{ }
			public Input(SIO.Stream cStream, ulong nFrameStart, string sFormat)
				: this()
			{
				_sFile = "buffer:" + cStream.GetHashCode();
				_cFormatCtx = AVFormatContext.OpenInput(cStream, sFormat);
				_cFormatCtx.StreamInfoFind();

				Init(nFrameStart);
			}

			~Input()
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
			override public void Dispose()
			{
				lock (oDisposeLock)
				{
					if (_bClosed)
					{
						(new Logger()).WriteDebug2("in: already disposed");
						return;
					}
					if (null == _aInputsToDispose || !_aInputsToDispose.Contains(this))
						ClearMem();  // т.е. не плеелся вообще, а люди просто узнавали хронометраж. т.е. идёт вне глобал диспоза

                    _bClosed = true;  // инфа для глобал диспоза
				}
			}
            public string LogInfo()
            {
                return "[file=" + _sFile + "][hc = " + GetHashCode() + "][closed = " + _bClosed + "][fend = " + bFileEnd + "][_fend = " + _bFileEnd + "][total_frames: " + _nPreparedFramesIndx + "][average: " + nQueueAverage + "][packets = " + (_aqPackets == null ? "NULL" : "" + _aqPackets.Count) + "]";
            }

            private void ClearMem()
			{
                lock (oDisposeLock)
                {
                    if (_bWasClearMem)
                        return;
                    _bWasClearMem = true;
                }
                (new Logger()).WriteDebug2("clearmem. in [hc = " + GetHashCode() + "][a_packets:" + (_aqAudioPackets == null ? "null" : "" + _aqAudioPackets.Count) + "][v_packets:" + (_aqVideoPackets == null ? "null" : "" + _aqVideoPackets.Count) + "][packets:" + (_aqPackets == null ? "null" : "" + _aqPackets.Count) + "][v_frames: " + (_aqVideoFrames == null ? "null" : "" + _aqVideoFrames.Count) + " + " + (_aqVideoFramesFree == null ? "null" : "" + _aqVideoFramesFree.Count) + " ][a_frames: " + (_aqAudioFrames == null ? "null" : "" + _aqAudioFrames.Count) + " + " + (_aqAudioFramesFree == null ? "null" : "" + _aqAudioFramesFree.Count) + " ]");
                Logger.Timings cTimings = new Logger.Timings("ffmpeg:file:input:dispose:");
                try
                {
                    base.Dispose();
					_aBytesRemainder = null;
					_aPacketBytes = null;

					Frame cFrame;
					if (null != _aqVideoFrames)
					{
						while (0 < _aqVideoFrames.Count)
						{
							cFrame = _aqVideoFrames.Dequeue();
							//cFrame.Disposing -= cFrameVideo_Disposing;
							cFrame.Dispose();
						}
						//while (0 < _aqVideoFramesFree.Count)
						//{
						//    cFrame = _aqVideoFramesFree.Dequeue();
						//    cFrame.Disposing -= cFrameVideo_Disposing;
						//    cFrame.Dispose();
						//}
						_aqVideoFrames = null;
						_aqVideoFramesFree = null;
					}
					if (null != _aqAudioFrames)
					{
						while (0 < _aqAudioFrames.Count)
						{
							cFrame = _aqAudioFrames.Dequeue();
							//cFrame.Disposing -= cFrameAudio_Disposing;
							cFrame.Dispose();
						}
						//while (0 < _aqAudioFramesFree.Count)
						//{
						//    cFrame = _aqAudioFramesFree.Dequeue();
						//    cFrame.Disposing -= cFrameAudio_Disposing;
						//    cFrame.Dispose();
						//}
						_aqAudioFrames = null;
						_aqAudioFramesFree = null;
					}
					IntPtr pPacket;
					if (null != _aqAudioPackets)
					{
						while (0 < _aqAudioPackets.Count)
						{
							pPacket = _aqAudioPackets.Dequeue();
							if (NULL != pPacket)
							{
								Functions.av_free_packet(pPacket);
								Functions.av_freep(ref pPacket);
							}
						}
						_aqAudioPackets = null;
					}
					if (null != _aqVideoPackets)
					{
						while (0 < _aqVideoPackets.Count)
						{
							pPacket = _aqVideoPackets.Dequeue();
							if (NULL != pPacket)
							{
								Functions.av_free_packet(pPacket);
								Functions.av_freep(ref pPacket);
							}
						}
						_aqVideoPackets = null;
					}
					if (NULL != _pPacketAudio)
					{
						Functions.av_free_packet(_pPacketAudio);
						Functions.av_freep(ref _pPacketAudio);
					}
					if (NULL != _pPacketAudioDub)
					{
						Functions.av_free_packet(_pPacketAudioDub);
						Functions.av_freep(ref _pPacketAudioDub);
					}
					if (null != _cFrameVideo)
						_cFrameVideo.Dispose();
					_cFrameVideo = null;
					if (null != _cFrameAudio)
						_cFrameAudio.Dispose();
					_cFrameAudio = null;

					if (null != _aqPackets)
					{
						int nSleepIndx = 0;
						while (0 < _aqPackets.Count)
						{
							nSleepIndx++;
							while (!_aqPackets.TryDequeue(out pPacket)) ;
							if (NULL != pPacket)
							{
								Functions.av_free_packet(pPacket);
								Functions.av_freep(ref pPacket);
							}
							if (nSleepIndx % 100 == 0)  // а то на 7600 элементах факапит девайс!! хотя и не сильно, но неприятно
								Thread.Sleep(1);
						}
						_aqPackets = null;
					}


				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				(new Logger()).WriteDebug3("out [hc: " + GetHashCode() + "]");

				cTimings.Stop("disposing > 20", "disposing", 5);
			}
#if DEBUG_LISTAR
            int _frame = 0;
			System.Drawing.Bitmap cFrame = null;
			System.Drawing.Imaging.BitmapData  cFrameBD = null;
#endif
			private void Init(ulong nFrameStart)
			{
				try
				{
					AVStream stStream;
					AVCodecContext stCodecCtx;

					float nVideoDuration, nAudioDuration;
					nVideoDuration = nAudioDuration = float.MaxValue;
					AVMediaType eAVMediaType;
                    for (int nIndx = 0; nIndx < _cFormatCtx.nStreamsQty; nIndx++)
					{
						stStream = _cFormatCtx.aStreams[nIndx];
						stCodecCtx = (AVCodecContext)Marshal.PtrToStructure(stStream.codec, typeof(AVCodecContext));
						eAVMediaType = (AVMediaType)stCodecCtx.codec_type;
                        if (AVMediaType.AVMEDIA_TYPE_VIDEO == eAVMediaType)
						{
							#region VIDEO
							_nVideoStreamIndx = stStream.index;
							long nFrameTarget;
							nFrameTarget = Functions.av_rescale((long)(nFrameStart * 40), stStream.time_base.den, stStream.time_base.num) / 1000; //FPS
							if (0 < nFrameStart)
								_cFormatCtx.Seek(_nVideoStreamIndx, nFrameTarget);
							(new Logger()).WriteDebug("init: seek [file_start_fr:" + nFrameStart + "] [frame_target:" + nFrameTarget + "]"); //logging
							_cFormatVideo = new Format.Video((ushort)stCodecCtx.width, (ushort)stCodecCtx.height, stCodecCtx.codec_id, stCodecCtx.pix_fmt, stStream.codec, stCodecCtx.field_order);
							//nFramesPerSecond = (ushort)stStream.r_frame_rate.num;
							//nFramesPerSecond = (ushort)stStream.time_base.den;
							nFramesPerSecond = (ushort)(stStream.avg_frame_rate.num);

							if (0 < stStream.time_base.num && 0 < stStream.time_base.den && 0 < stStream.duration)
								nVideoDuration = stStream.duration * stStream.time_base.num / (float)stStream.time_base.den;
							else
							{
								(new Logger()).WriteWarning("init: wrong duration numbers");
								if (0 < stStream.nb_frames)
									nVideoDuration = stStream.nb_frames / (float)nFramesPerSecond;  // для mov DvPal, hdv 
								else
									nVideoDuration = stStream.duration / (float)nFramesPerSecond;   // для HD MXF работает только так
							}
							_aqVideoPackets = new Queue<IntPtr>();
							_aqVideoFrames = new Queue<Frame>();
							_aqVideoFramesFree = null; // new Queue<Frame>();
							#endregion
						}
						else if (AVMediaType.AVMEDIA_TYPE_AUDIO == eAVMediaType && 0 > _nAudioStreamIndx)
						{
							#region AUDIO
							_nAudioStreamIndx = stStream.index;
							nAudioDuration = stStream.duration / (float)stStream.time_base.den;
							_cFormatAudio = new Format.Audio(stStream.time_base.den, stCodecCtx.channels, stCodecCtx.codec_id, (AVSampleFormat)stCodecCtx.sample_fmt, stStream.codec);
							//_cFormatAudio = new Format.Audio(stCodecCtx.sample_rate, stCodecCtx.channels, stCodecCtx.codec_id, (AVSampleFormat)stCodecCtx.sample_fmt, stStream.codec);

							_pPacketAudio = NULL;
							_aqAudioPackets = new Queue<IntPtr>();
							_aqAudioFrames = new Queue<Frame>();
							_aqAudioFramesFree = null; // new Queue<Frame>();
							#endregion
						}
					}
					if (0 > _nVideoStreamIndx && 0 > _nAudioStreamIndx)
						throw new Exception("can't find suitable streams");
					if (nVideoDuration < float.MaxValue || nAudioDuration < float.MaxValue)
					{
						ulong nVideoFramesQty = nVideoDuration < float.MaxValue ? (ulong)(nVideoDuration * nFramesPerSecond) : ulong.MaxValue;
						ulong nAudioFramesQty = nAudioDuration < float.MaxValue ? (ulong)(nAudioDuration * nFramesPerSecond) : ulong.MaxValue;
						//(new Logger()).WriteWarning("Video and audio frames quantity doesn't match!! [video=" + nVideoFramesQty + "] [audio=" + nAudioFramesQty + "]");
						ulong nTotalFrames;
						if (1 == nVideoFramesQty - nAudioFramesQty || 2 == nVideoFramesQty - nAudioFramesQty)
							nTotalFrames = nVideoFramesQty;
						else
							nTotalFrames = (nVideoFramesQty < nAudioFramesQty ? nVideoFramesQty : nAudioFramesQty);

						if (nTotalFrames > nFrameStart)
							nFramesQty = nTotalFrames - nFrameStart;
						else
							throw new Exception("Start frame cannot be greater than total frames count!!! [name=" + this._sFile + "][nTotalFrames="+ nTotalFrames + "][nFrameStart=" + nFrameStart + "]");
					}
				}
				catch (Exception ex)
				{
					Dispose();
					throw;
				}
			}
            public void Prepare(Format.Video cFormatVideo, Format.Audio cFormatAudio, PlaybackMode eFramesGettingMode)
			{
				_bDoWritingFrames = false;
#if DEBUG
                if (_cFormatVideoTarget != null && sDebugFolder != null)
                {
                    _aqWritingFrames = new Queue<byte[]>();
                    _aqWritingAudioFrames = new Queue<byte[]>();
                    _cThreadWritingFramesWorker = new System.Threading.Thread(WritingFramesWorker);
                    _cThreadWritingFramesWorker.IsBackground = true;
                    _cThreadWritingFramesWorker.Priority = System.Threading.ThreadPriority.Normal;
                    _cThreadWritingFramesWorker.Start();
                }
#endif
                _eFramesGettingMode = eFramesGettingMode;
                _cFormatVideoTarget = cFormatVideo;
				_cFormatAudioTarget = cFormatAudio;
                if (_cFormatVideoTarget == null)
                {
                    _aqVideoFrames = null;
                    _aqVideoPackets = null;
                }
                if (cFormatAudio == null)
                {
                    _aqAudioFrames = null;
                    _aqAudioPackets = null;
                }

                if (nPreparedFramesMinimum < nCacheSize)
					_nDecodedFramesInPrepare = nPreparedFramesMinimum;
				else
					_nDecodedFramesInPrepare = nCacheSize;
				//_nPreparedFramesIndx = _nDecodedFramesInPrepare;
				int nIndx = 0;

                //System.Threading.ThreadPool.QueueUserWorkItem(DecodeAndCache);


                if (_eFramesGettingMode == PlaybackMode.GivesFrameOnDemand)
                {
                    nCacheSize = 5;
                    while (nCueueLength < nCacheSize && !_bClosed)
                        if (!GetAndAddFrame())
                        {
                            _bFileEnd = true;
                            return;
                        }

                    bPrepared = true;
                    return;
                }


				lock (_oThreadDisposeLock)
				{
					if (null == _cThreadDispose)
					{
						_cThreadDispose = new Thread(DisposeGlobal);
						_cThreadDispose.IsBackground = true;
						_cThreadDispose.Priority = Thread.CurrentThread.Priority;
						_cThreadDispose.Priority = System.Threading.ThreadPriority.Normal;
						_cThreadDispose.Start();

						_aInputsToDispose = new List<Input>();
					}
					if (!_aInputsToDispose.Contains(this))
						_aInputsToDispose.Add(this);
				}

				lock (_oThreadPacketsGetLock)
				{
					if (null == _cThreadPacketsGet)
					{
						_cThreadPacketsGet = new Thread(PacketsGetGlobal);
						_cThreadPacketsGet.IsBackground = true;
						_cThreadPacketsGet.Priority = Thread.CurrentThread.Priority;
						_cThreadPacketsGet.Priority = System.Threading.ThreadPriority.Normal;
						_cThreadPacketsGet.Start();

						_aInputsToPacketsRead = new List<Input>();
					}
					if (!_aInputsToPacketsRead.Contains(this))
						_aInputsToPacketsRead.Add(this);
				}
				while (6 > _aqPackets.Count)
				{
                    if (_bClosed || _bFileEnd)
                    {
                        return;
                    }
					Thread.Sleep(1);
				}

				lock (_oThreadDecodeAndCacheLock)
				{
					if (null == _cThreadDecodeAndCache)
					{
						_cThreadDecodeAndCache = new Thread(DecodeAndCacheGlobal);
						_cThreadDecodeAndCache.IsBackground = true;
						_cThreadDecodeAndCache.Priority = Thread.CurrentThread.Priority;
						_cThreadDecodeAndCache.Priority = System.Threading.ThreadPriority.Normal;
						_cThreadDecodeAndCache.Start();

						_aInputsToDecode = new List<Input>();
					}
					if (!_aInputsToDecode.Contains(this))
						_aInputsToDecode.Add(this);
				}
				while (!_bClosed && !_bFileEnd && _nDecodedFramesInPrepare > nCueueLength)
				{
					System.Threading.Thread.Sleep(1);
				}
                bPrepared = true;
                (new Logger()).WriteDebug("input prepared: [" + _sFile + "][hc=" + GetHashCode() + "]");
			}
            private bool GetAndAddFrame()     
            {
                bool bRetVal = true;

                if (nCueueLength < nCacheSize && !_bClosed)
                {
                    if (!AddFrameToQueue())
                    {
                        if (!_bFileEnd)
                        {
                            _bFileEnd = true;
                            (new Logger()).WriteError("GetAndAddFrame error: No video before the end of file [hc=" + GetHashCode() + "][" + _sFile + "][packets=" + _aqPackets.Count + "]");
                        }
                        bRetVal = false;
                    }
                }
                return bRetVal;
            }

            private static void DisposeGlobal(object oState)
			{
				try
				{
					(new Logger()).WriteNotice("DisposeGlobal: opened!! ");
					Logger.Timings cTimings = new Logger.Timings("DisposeGlobal");
					List<Input> aToWaitingDispose = new List<Input>();
					int nIndx, nI;
					string sLog;
					while (true)
					{
						nIndx = 0;
						lock (_oThreadDisposeLock)  // что б не меняли  _aInputsToDispose  в prepare()
						{
							cTimings.TotalRenew();
							//sLog = "";
							foreach (Input cI in _aInputsToDispose)
							{
								//sLog += "[dispose_wait-" + nIndx + "(" + _aInputsToDispose.Count() + ") [hc=" + cI.GetHashCode() + "]__";
								try
								{
									if (cI._bClosed)   // закрыли в другом месте где-то
									{
										aToWaitingDispose.Add(cI);
									}
								}
								catch (Exception ex)
								{
									(new Logger()).WriteError(ex);
								}
								nIndx++;
							}
						}
						cTimings.Restart("foreach");
						for (nI = 0; nI < aToWaitingDispose.Count(); nI++)
						{
							if (_aInputsToDispose.Contains(aToWaitingDispose[nI]))
							{
								_aInputsToDispose.Remove(aToWaitingDispose[nI]);
								lock (_oThreadDisposeLock)
								{
									sLog = "";
									nIndx = 0;
									foreach (Input cI in _aInputsToDispose)
									{
										sLog += "[inputs_in_dispose-" + nIndx + "(" + _aInputsToDispose.Count() + ") [hc=" + cI.GetHashCode() + "]__";
										nIndx++;
									}
								}
								(new Logger()).WriteDebug("input removed from dispose_inputs: [" + aToWaitingDispose[nI]._sFile + "][hc=" + aToWaitingDispose[nI].GetHashCode() + "][waiting_count=" + aToWaitingDispose.Count() + "]___" + sLog);
							}
							if (!_aInputsToDecode.Contains(aToWaitingDispose[nI]) && !_aInputsToPacketsRead.Contains(aToWaitingDispose[nI]) && aToWaitingDispose[nI].bGotPacketsAssync)
							{
								cTimings.Restart("before clearmem");
                                aToWaitingDispose[nI].ClearMem(); //Dispose();  потому что только тут ждут остальные потоки!
                                cTimings.Restart("ClearMem ");
								aToWaitingDispose.RemoveAt(nI);
								nI--;
							}
						}
						cTimings.Stop("dispose circle", "end", 30);

						Thread.Sleep(40);
					}
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				(new Logger()).WriteNotice("DisposeGlobal: closed!! ");
			}

            private bool bGotPacketsAssync;
            static bool _bAbortPacketsGetGlobalAssync;  // not used yet
            static bool _bAbortedPacketsGetGlobalAssync;
            static DateTime _dtFreeze_bAbortPacketsGetGlobal;
            static private Input _cCurrentInputWithPacketGeting;
            static private Thread _cThreadPacketsGetGlobalAssync;

            private static void PacketsGetGlobalAssync(object oState)
            {
                Input cCurrent = null;
                Input cLast = null;
                try
                {
                    (new Logger()).WriteNotice("PacketsGetGlobalAssync: started"); //logging
                    _bAbortedPacketsGetGlobalAssync = false;
                    while (!_bAbortPacketsGetGlobalAssync)
                    {
                        lock (_oLockPacketsBlock)
                        {
                            if (null != _cCurrentInputWithPacketGeting)
                                cLast = cCurrent = _cCurrentInputWithPacketGeting;
                        }
                        if (cCurrent == null)
                            Thread.Sleep(1);
                        else
                        {
                            (new Logger()).WriteDebug2("PacketsGetGlobalAssync: will get packets [got_for_hc=" + cCurrent.GetHashCode() + "][f=" + cCurrent._sFile + "][_aqPackets=" + cCurrent._aqPackets.Count() + "]"); //logging
                            cCurrent.PacketsBlockGet(false);
                            (new Logger()).WriteDebug2("PacketsGetGlobalAssync: got packets [got_for_hc=" + cCurrent.GetHashCode() + "][f=" + cCurrent._sFile + "][_aqPackets=" + cCurrent._aqPackets.Count() + "]"); //logging
                            lock (_oLockPacketsBlock)
                            {
                                _cCurrentInputWithPacketGeting = null;
                                cCurrent = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    (new Logger()).WriteError(ex);
                }
                finally
                {
                    if (cLast != null)
                        cLast.bGotPacketsAssync = true;
                    _bAbortedPacketsGetGlobalAssync = true;
                    (new Logger()).WriteNotice("PacketsGetGlobalAssync: closed!! "); //logging
                }
            }
            private static void StartThreadPacketsGetGlobalAssync()
            {
                if (null == _cThreadPacketsGetGlobalAssync)
                {
                    _cThreadPacketsGetGlobalAssync = new Thread(PacketsGetGlobalAssync);
                    _cThreadPacketsGetGlobalAssync.IsBackground = true;
                    _cThreadPacketsGetGlobalAssync.Priority = Thread.CurrentThread.Priority;
                    _cThreadPacketsGetGlobalAssync.Priority = System.Threading.ThreadPriority.Normal;
                    _cThreadPacketsGetGlobalAssync.Start();
                }
            }
            private static void PacketsGetGlobal(object oState)
			{
				try
				{
					(new Logger()).WriteNotice("PacketsGetGlobal: opened!! ");
					Logger.Timings cTimings = new Logger.Timings("PacketsGetGlobal", helpers.Logger.Level.debug2);
					List<Input> aToDispose = new List<Input>();
                    int nIndx;
					bool bGotPackets;
                    string sLog = "", sLog2 = "";
                    bool bTested = false;
                    Input cI;
                    int nCount = 0;
                    StartThreadPacketsGetGlobalAssync();

                    while (true)
                    {
                        try
                        {
                            nIndx = 0;
                            bGotPackets = false;
                            aToDispose.Clear();
                            cI = null;
                            lock (_oThreadPacketsGetLock)  // что б не меняли  _aInputsToPacketsRead  в prepare()
                            {
                                nCount = _aInputsToPacketsRead.Count();
                                cTimings.TotalRenew();
                                sLog = "";
                                sLog2 = "";
                                foreach (Input cInput in _aInputsToPacketsRead)
                                {
                                    try
                                    {
                                        if (cInput._bClosed)   // закрыли в другом месте где-то
                                        {
                                            aToDispose.Add(cInput);
                                        }
                                        else if (!cInput._bPacketsReadingDone && cInput._aqPackets.Count <= cInput._nMinQtyPackets)
                                        {
                                            cI = cInput;
                                            sLog += "[packets-" + nIndx + "(" + nCount + ") = " + cI._aqPackets.Count + "(" + cI._nTotalPacketsQty + ")][tot_s=" + cI._nTotalPacketsSize + "][cur_bl_s=" + cI._nCurrentBlockSize + "][data_p=" + cI._nTotalDataAndOtherPackets + "][r_done=" + cI._bPacketsReadingDone + "][hc=" + cI.GetHashCode() + "]--\n\t\t";
                                            for (int nI = 0; nI < nCount; nI++)
                                                sLog2 += " [i=" + nI + "][hc=" + _aInputsToPacketsRead[nI].GetHashCode() + "][p_now=" + _aInputsToPacketsRead[nI]._aqPackets.Count + "]";
                                            (new Logger()).WriteDebug2("PacketsGetGlobal: need to get packets for [i=" + nIndx + "][hc=" + cI.GetHashCode() + "][p_min=" + cInput._nMinQtyPackets + "]" + sLog2);
                                            break;
                                        }
                                        else if (cInput._bPacketsReadingDone && !cInput._bPacketsReadingSuccess && cInput._aqPackets.Count == 0)
                                        {
                                            (new Logger()).WriteError("_bPacketsReadingDone = true, but _aqPackets = 0 [f=" + cInput._sFile + "][hc=" + cInput.GetHashCode() + "]");
                                        }
                                        nIndx++;
                                    }
                                    catch (Exception ex)
                                    {
                                        (new Logger()).WriteError(ex);
                                        (new Logger()).WriteNotice("PacketsGetGlobal:foreach1: error. [input=" + (cInput == null ? "NULL" : "" + cInput.GetHashCode()) + "][_aqPackets=" + (cInput._aqPackets == null ? "NULL" : "" + cInput._aqPackets.Count()) + "] " + sLog + "<br>" + sLog2);
                                        cInput._bFileEnd = true;
                                        aToDispose.Add(cInput);
                                        break;
                                    }
                                }
                            }
                            if (cI != null)
                            {
                                try
                                {
                                    //#if DEBUG
                                    //if (!bTested && SIO.Path.GetFileName(cI._sFile) == "002_582020.mxf" && cI._nPreparedFramesIndx > 100) //DNF !!!!!!!  TEST!!!!
                                    //{
                                    //    bTested = true;
                                    //    (new Logger()).WriteDebug("I paused getting frames )) [file=" + cI._sFile + "]");
                                    //    throw new Exception("SOMETHING WAS HAPPENED IN PACKETS GLOBAL!!!!!");
                                    //}
                                    //#endif


                                    _dtFreeze_bAbortPacketsGetGlobal = DateTime.Now.AddSeconds(_nFreezeTimeoutPacketsGetSeconds);
                                    lock (_oLockPacketsBlock)
                                        _cCurrentInputWithPacketGeting = cI;
                                    cI.bGotPacketsAssync = false;
                                    do
                                    {
                                        lock (_oLockPacketsBlock)
                                        {
                                            if (null == _cCurrentInputWithPacketGeting)
                                                cI.bGotPacketsAssync = true;
                                        }
                                        Thread.Sleep(1);
                                    }
                                    while (!cI.bGotPacketsAssync && DateTime.Now < _dtFreeze_bAbortPacketsGetGlobal);

                                    if (!cI.bGotPacketsAssync) // т.е. повисли
                                    {
                                        (new Logger()).WriteError("PacketsGetGlobal: packets_get_assync thread is freezing - will abort. [got_for_hc=" + cI.GetHashCode() + "][f=" + cI._sFile + "][_aqPackets=" + cI._aqPackets.Count() + "][r_done=" + cI._bPacketsReadingDone + "] " + sLog);
                                        cI._bFileEnd = true;
                                        aToDispose.Add(cI);
                                        lock (_oLockPacketsBlock)
                                            _cCurrentInputWithPacketGeting = null;
                                        _cThreadPacketsGetGlobalAssync.Abort();
                                        _cThreadPacketsGetGlobalAssync = null;
                                        StartThreadPacketsGetGlobalAssync();
                                    }
                                    else
                                        bGotPackets = true;

                                    cTimings.Restart("[got_for_hc=" + cI.GetHashCode() + "][total_count_" + nIndx + " (" + nCount + ")_____");
                                }
                                catch (Exception ex)
                                {
                                    (new Logger()).WriteError(ex);
                                    (new Logger()).WriteNotice("PacketsGetGlobal: error. [_aqPackets=" + (cI._aqPackets == null ? "NULL" : "" + cI._aqPackets.Count()) + "][total_s=" + cI._nTotalPacketsSize + "] " + sLog);
                                    cI._bFileEnd = true;
                                    aToDispose.Add(cI);
                                }
                            }

                            foreach (Input cInput in aToDispose)
                            {
                                lock (_oThreadPacketsGetLock)  // что б не меняли  _aInputsToPacketsRead  в prepare()
                                {
                                    if (_aInputsToPacketsRead.Contains(cInput))
                                    {
                                        _aInputsToPacketsRead.Remove(cInput);
                                        (new Logger()).WriteDebug("input removed from packet_inputs: [" + cInput._sFile + "]");
                                    }
                                    else
                                        (new Logger()).WriteDebug("input NOT CONTAINED in packet_inputs: [" + cInput._sFile + "]");
                                }
                            }

                            if (bGotPackets)
                            {
                                cTimings.Stop("got next " + nBlockSizeCommon / 1048576 + " MBt.   " + sLog);
                            }
                            else
                            {
                                cTimings.Stop("didn't get packets ", " " + sLog, 20);
                                Thread.Sleep(50);
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Logger()).WriteError(ex);
                            (new Logger()).WriteNotice("PacketsGetGlobal:while: strange error. [count=" + (_aInputsToPacketsRead == null ? "NULL" : "" + _aInputsToPacketsRead.Count()) + "] " + sLog + sLog2);
                            Thread.Sleep(20);
                        }
                    }
                }
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				(new Logger()).WriteNotice("PacketsGetGlobal: closed!!");
			}

			private static Thread _cThreadPacketsGet;
			private static object _oThreadPacketsGetLock = new object();
			private static List<Input> _aInputsToPacketsRead;

			private static Thread _cThreadDispose;
			private static object _oThreadDisposeLock = new object();
			private static List<Input> _aInputsToDispose;

            private int nQueueAverageLength = 0;
            private int nQueueAverage
            {
                get
                {
                    if (_nPreparedFramesIndx > nCacheSize)
                        return nQueueAverageLength / (_nPreparedFramesIndx - 1 - nCacheSize / 2);
                    else
                        return _nPreparedFramesIndx;
                }
            }

            private static Thread _cThreadDecodeAndCache;
			private static List<Input> _aInputsToDecode;
			private static object _oThreadDecodeAndCacheLock = new object();
			private static bool _bCachesAreFull;
			public class Task
			{
				public enum Status
				{
					Waiting,
					Done,
					Busy
				}
				public enum Result
				{
					Stopped,
					NoVideo,
					FullQueue,
					Unknown, 
					Error
				}
				private Input _cCurrentJob;
				private long _nLastJobHC;
				private Thread _cThread;
				private Input _cRetVal;
				private bool _bTemporaryFull;
				private bool _bAbort;
				private int _nID;

				public Result eResult;
				public Status eStatus;
				public DateTime dtFreezeInAddFrame;
				public Task()
				{
					Init();
				}
				public Task(int nID)
					:this()
				{
					_nID = nID;
				}
				public static bool ContainsInput(Task[] aT, Input cI)
				{
					foreach (Task cT in aT)
					{
						if (cI == cT._cCurrentJob)
							return true;
					}
					return false;
				}
				public static bool AreAllStandIdlyBy(Task[] aT)
				{
					foreach (Task cT in aT)
					{
						if (cT.eStatus == Status.Busy && !cT._bTemporaryFull)
							return false;
					}
					return true;
				}
				public static void StopAll(Task[] aT)
				{
					foreach (Task cT in aT)
					{
						cT._bAbort = true;
					}
				}
				public static void StartAll(Task[] aT)
				{
					for (int ni = 0; ni < aT.Length; ni++)
					{
						aT[ni] = new Task();
						aT[ni]._nID = ni;
					}
				}
				public void Init()
				{
					_bAbort = false;
					_bTemporaryFull = false;
					eStatus = Status.Waiting;
					_cCurrentJob = null;
					eResult = Result.Unknown;
					dtFreezeInAddFrame = DateTime.MaxValue;

					_cThread = new Thread(DecodeAndCacheWorker);
					_cThread.IsBackground = true;
					_cThread.Priority = Thread.CurrentThread.Priority;
					_cThread.Priority = System.Threading.ThreadPriority.Highest;
					_cThread.Start();
				}
				public Input ResetOnFreeze()
				{
					Input cRetVal = _cCurrentJob;
					_cCurrentJob = null; // важно до аборта занулить из-за кэтча в воркере
					try
					{
						if (_cThread.IsAlive)
						{
							_cThread.Abort();
						}
					}
					catch (Exception ex)
					{
						(new Logger()).WriteError("ResetOnFreeze: ", ex);
					}

					//cRetVal._bClosed = true;
					Init();
					return cRetVal;
				}
				public Input GetJob()
				{
					if (null == _cCurrentJob || eStatus != Status.Done)
						return null;
					_cRetVal = _cCurrentJob;
					_cCurrentJob = null;
					eStatus = Status.Waiting;
					return _cRetVal;
				}
				public void AddJob(Input cI)
				{
					if (_cCurrentJob == null && (eStatus == Status.Done || eStatus == Status.Waiting))
					{
						_cCurrentJob = cI;
						_nLastJobHC = _cCurrentJob.GetHashCode();
						eStatus = Status.Busy;
						eResult = Result.Unknown;
					}
				}

                public string LogInfo()
                {
                    return "[task=" + _nID + "][result=" + eResult + "][status=" + eStatus + "]-->" + (_cCurrentJob == null ? "NULL" : _cCurrentJob.LogInfo());
                }

                static bool bTested = false;

                //[System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
                private void DecodeAndCacheWorker(object oState)
				{
					(new Logger()).WriteNotice("DecodeAndCacheWorker-" + _nID + ": started!");
					int nQLength;
					DateTime dtCheck;
                    bool bCacheWasFull = false;
					while (!_bAbort)
					{
						try
						{
							if (eStatus == Status.Done || eStatus == Status.Waiting)
							{
								//GC.Collect
								while (!_bAbort && (eStatus == Status.Done || eStatus == Status.Waiting))   // свободная касса
									Thread.Sleep(1);
							}
							if (_bAbort)
								break;
							if ((nQLength = _cCurrentJob.nCueueLength) < _cCurrentJob.nCacheSize && !_cCurrentJob._bClosed)
							{
								_bTemporaryFull = false;
								dtFreezeInAddFrame = DateTime.Now.AddSeconds(_nFreezeTimeoutTaskSeconds);


//#if DEBUG
                                //bool bTEST = false;
                                //if (!bTested && SIO.Path.GetFileName(_cCurrentJob._sFile) == "002_582020.mxf" && _cCurrentJob._nPreparedFramesIndx == 105) //DNF !!!!!!!  TEST!!!!
                                //{
                                //    bTested = true;
                                //    (new Logger()).WriteDebug("I paused getting frames )) [file=" + _cCurrentJob._sFile + "]");
                                //    bTEST = true;
                                //}
                                //if (bTEST || !_cCurrentJob.AddFrameToQueue())
//#endif

                                if (!_cCurrentJob.AddFrameToQueue())
								{
									eResult = Result.NoVideo;
									eStatus = Status.Done;
									dtFreezeInAddFrame = DateTime.MaxValue;

                                    if (!_cCurrentJob._bFileEnd)
                                    {
                                        _cCurrentJob._bFileEnd = true;
                                        (new Logger()).WriteError("DecodeAndCacheWorker_" + _nID + " error: No video before the end of file [_cCurrentJob=" + (_cCurrentJob == null ? "null" : _cCurrentJob.GetHashCode() + "") + "][status=" + eStatus + "][result=" + eResult + "][packets=" + _cCurrentJob._aqPackets.Count + "]");
                                    }
                                    continue;
								}
//#if DEBUG
                                //if (SIO.File.Exists(@"c:\freeze_task")) //DNF !!!!!!!  TEST!!!!    (TEST WAS OK!!!!  Even if video has not started yet)
                                //{
                                //    SIO.File.Delete(@"c:\freeze_task");
                                //    (new Logger()).WriteDebug("I'm FREEZE )) [id=" + _nID + "] [now:" + nQLength + "][hc=" + _cCurrentJob.GetHashCode() + "][packets=" + _cCurrentJob._aqPackets.Count + "]");
                                //    Thread.Sleep(40000);  //DNF !!!!!!!  TEST!!!!
                                //}
//#endif

                                dtFreezeInAddFrame = DateTime.MaxValue;

                                if(!bCacheWasFull && nQLength== _cCurrentJob.nCacheSize - 1)
                                {
                                    bCacheWasFull = true;
                                    (new Logger()).WriteDebug("cache-" + _nID + " is full: [" + nQLength + "][hc=" + _cCurrentJob.GetHashCode() + "][packets=" + _cCurrentJob._aqPackets.Count + "]");
                                }

								if (_cCurrentJob._bFrameGettingStarted && _cCurrentJob.nCacheSize - 5 > nQLength)  //DNF
								{
									(new Logger()).WriteDebug3("cache-" + _nID + " now: [" + nQLength + "][hc=" + _cCurrentJob.GetHashCode() + "][packets=" + _cCurrentJob._aqPackets.Count + "]");
                                }

                                if (_cCurrentJob._bFrameGettingStarted && _cCurrentJob.nCacheSize / 3 > nQLength)
                                    (new Logger()).WriteDebug2("cache-" + _nID + " now: [" + nQLength + "][hc=" + _cCurrentJob.GetHashCode() + "][packets=" + _cCurrentJob._aqPackets.Count + "]");

                                _cCurrentJob.nQueueAverageLength += nQLength;
                            }
                            else
                            {
								_bTemporaryFull = true;
								if (!_cCurrentJob.bCached)
									_cCurrentJob.bCached = true;
								if (_cCurrentJob._bClosed)
								{
									eResult = Result.Stopped;
									eStatus = Status.Done;
									continue;
								}
								else
								{
									dtCheck = DateTime.Now.AddMilliseconds(1000);
									while (!_cCurrentJob._bClosed && !_bAbort && (_cCurrentJob.nCueueLength >= _cCurrentJob.nCacheSize && dtCheck > DateTime.Now))
										Thread.Sleep(1);
									if (_cCurrentJob.nCueueLength >= _cCurrentJob.nCacheSize)
									{
										eResult = Result.FullQueue;
										eStatus = Status.Done;
										continue;
									}
								}
							}
						}
						catch (Exception ex)  // System.AccessViolationException  doesn't catch by managed code!!!!!!!!!!!!!!!!   it can be in "AddFrameToQueue()" -> .... -> "ffmpeg.net.Functions.av_read_frame(IntPtr, IntPtr)"
                        {
							if (_cCurrentJob != null)
							{
								eStatus = Status.Done;
								eResult = Result.Error;
								dtFreezeInAddFrame = DateTime.MaxValue;
							}
							(new Logger()).WriteError("DecodeAndCacheWorker_" + _nID + ": [_cCurrentJob=" + (_cCurrentJob == null ? "null" : _cCurrentJob.GetHashCode() + "") + "][last_job="+ _nLastJobHC + "][status=" + eStatus + "][result=" + eResult + "]", ex);
						}
					}
					(new Logger()).WriteNotice("DecodeAndCacheWorker_" + _nID + ": finished!! ");
				}
			}
			private static Task[] _aTasks;
			private static void DecodeAndCacheGlobal(object oState)
			{
				try
				{
					(new Logger()).WriteNotice("DecodeAndCacheGlobal: opened!! ");

					_aTasks = new Task[Input.nDecodingThreads];
					Task.StartAll(_aTasks);

					List<Input> aToDispose = new List<Input>();
					Logger.Timings cTimings = new Logger.Timings("DecodeAndCacheGlobal");
					Task cT;
					DateTime dtLog = DateTime.MinValue;
					int nI = 0;
					Input cCurrentJob;
					bool bNoJob;
					while (true)
					{
						cTimings.TotalRenew();
						aToDispose.Clear();
						bNoJob = false;

						lock (_oThreadDecodeAndCacheLock)  // что б не меняли  _aInputsToDecode  в prepare()
                        {
                            for (nI = 0; nI < _aTasks.Length; nI++)
                            {
                                try
                                {
                                    cT = _aTasks[nI];
                                    if (cT.eStatus == Task.Status.Busy)
                                    {
                                        if (cT.dtFreezeInAddFrame < DateTime.Now)
                                        {
                                            cCurrentJob = cT.ResetOnFreeze();
                                            _aInputsToDecode.Remove(cCurrentJob);
                                            (new Logger()).WriteDebug("Input Freezes! Input removed from inputs_to_decode: " + cT.LogInfo()); //logging
                                        }
                                        continue;
                                    }

                                    if (cT.eStatus == Task.Status.Done)
                                    {
                                        cCurrentJob = cT.GetJob();
                                        if (cT.eResult == Task.Result.Unknown || cT.eResult == Task.Result.Error)
                                        {
                                            if (cCurrentJob != null)
                                                cCurrentJob._bFileEnd = true;
                                            _aInputsToDecode.Remove(cCurrentJob);
                                            (new Logger()).WriteDebug("error - got. Input removed from inputs_to_decode: " + cT.LogInfo()); //logging
                                        }
                                        if (cT.eResult == Task.Result.NoVideo || cT.eResult == Task.Result.Stopped)
                                        {
                                            if (cCurrentJob != null)
                                                cCurrentJob._bFileEnd = true;
                                            _aInputsToDecode.Remove(cCurrentJob);
                                            (new Logger()).WriteDebug("input removed from inputs_to_decode: " + cT.LogInfo());
                                        }
                                    }

                                    if (!bNoJob && cT.eStatus == Task.Status.Waiting)
                                    {
                                        cCurrentJob = _aInputsToDecode.FirstOrDefault(o => !Task.ContainsInput(_aTasks, o) && o.nCueueLength < o.nCacheSize);
                                        if (null != cCurrentJob && cCurrentJob._bClosed)
                                        {
                                            _aInputsToDecode.Remove(cCurrentJob);
                                            (new Logger()).WriteDebug("input removed from inputs_to_decode in strange point-1!! error!!: " + cT.LogInfo());
                                        }
                                        if (null == cCurrentJob)
                                        {
                                            bNoJob = true;
                                            continue;
                                        }
                                        cT.AddJob(cCurrentJob);
                                    }
                                    cCurrentJob = null;
                                }
                                catch (Exception ex)
                                {
                                    (new Logger()).WriteError(ex);
                                    (new Logger()).WriteNotice("DecodeAndCacheGlobal: error. " + (_aTasks[nI] == null ? "NULL" : _aTasks[nI].LogInfo()));
                                }
                            }
                            try
                            {
                                foreach (Input cIn in _aInputsToDecode.Where(o => !Task.ContainsInput(_aTasks, o) && o._bClosed).ToArray())
                                {
                                    _aInputsToDecode.Remove(cIn);
                                    (new Logger()).WriteDebug("input removed from inputs_to_decode in strange point-2!! error!!: " + cIn.LogInfo());
                                }
                            }
                            catch (Exception ex)
                            {
                                (new Logger()).WriteError("DecodeAndCacheGlobal.InputsRemoving", ex);
                            }
                        }
                        cTimings.Restart("foreach");

						_bCachesAreFull = Task.AreAllStandIdlyBy(_aTasks);

						cTimings.Stop("decode_n_cache2 " + "bCacheIsFull=" + _bCachesAreFull, "btl_queue:" + nBTLCurrentBuffer, 20);
						System.Threading.Thread.Sleep(5);
					}
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				finally
				{
					Task.StopAll(_aTasks);
					(new Logger()).WriteNotice("DecodeAndCacheGlobal: closed!! "); //logging
				}
			}

			//private void DecodeAndCache(object oState)
			//{
			//	try
			//	{
			//		int nQueueAverageLength = 0;
			//		lock (_cCloseLock)
			//		{
			//			while (!_bFileEnd && !_bClosed)   
			//			{
			//				if (!AddFrameToQueue() && !bFileEndless)
			//					break; // конец видео или аудио, если видео нет
			//				if (nCacheSize == _nPreparedFramesIndx)
			//				{
			//					bPrepared = true;
			//					(new Logger()).WriteDebug("cache: prepared " + nCacheSize + " frames [hc:" + this.GetHashCode()); //logging
			//				}
			//				nQueueAverageLength += nCueueLength;
			//			}
			//			(new Logger()).WriteDebug("cache: stopped [hc:" + this.GetHashCode() + "][frames:" + _nPreparedFramesIndx + "][average:" + nQueueAverageLength / _nPreparedFramesIndx + "]"); //logging
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		(new Logger()).WriteError(ex);
			//	}
			//}

			private bool AddFrameToQueue()
			{
				_cTimingsAFTQ.TotalRenew();

				bool bVideo = true, bAudio = true;
				if (null != _aqVideoFrames)
				{
					while (!_bFileEnd)
					{
						try
						{
                            FrameDecodeVideo();
							_nPreparedFramesIndx++;
							break;
						}
						catch (Exception ex)
						{
							bVideo = false;
							if (!_bFileEnd)
								(new Logger()).WriteWarning(ex); //logging
						}
					}
				}
				_cTimingsAFTQ.Restart("decode video");
				if ((bVideo || _bFileEnd) && null != _aqAudioFrames)
				{
					while (!_bFileEnd && bVideo || !bVideo)
					{
						try
						{
							if (_bFileEnd)
								while (true)
									FrameDecodeAudio(); // дообрабатываем аудиопакеты, взятые ранее и валимся в catch
							else
								FrameDecodeAudio();
							break;
						}
						catch (Exception ex)
						{
							bAudio = false;
							if (!_bFileEnd)
								(new Logger()).WriteWarning(ex); //logging
							else
								break;
						}
					}
				}
				_cTimingsAFTQ.Restart("decode audio");
				if (bVideo && null != _aqVideoFrames && !bAudio) // неполный последний кадр уравновешиваем пустым аудио-кадром, чтобы не плодить рассинхрон дальше....
				{
					(new Logger()).WriteWarning("queue: bad last audio frame. silenced frame added."); //logging
					Frame cFrame = FramesRotation.Dequeue(_cFormatAudioTarget, FramesRotation.Type.empty, 0);  //new Frame(_cFormatAudioTarget);
					lock (_aqAudioFrames)
						_aqAudioFrames.Enqueue(cFrame);
				}

                _cTimingsAFTQ.Stop("AddFrameToQueue", "enqueue", 60);

                if (_bFileEnd || !bVideo || (!bAudio && null == _aqVideoFrames))
					return false;

				

				//while (nCueueLength > nCacheSize)  //(null == _aqVideoFrames ? _aqAudioFrames.Count : _aqVideoFrames.Count)
				//{
				//	if (_bClosed)
				//		return false;
				//	System.Threading.Thread.Sleep(40);//FPS
				//}
				return true;
			}
			public Frame FrameNextVideoGet()
			{
                (new Logger()).WriteDebug4("in");
                if (!_bFrameGettingStarted) _bFrameGettingStarted = true;
                Frame cRetVal = null;
                _cTimingsFNVG.TotalRenew();
                if (!_bFileEnd && 1 > _aqVideoFrames.Count)
                {
                    if (_eFramesGettingMode == PlaybackMode.GivesFrameOnDemand)
                    {
                        if (!GetAndAddFrame() && _aqVideoFrames.Count <= 0)
                        {
                            if (bFileEndless)
                                return null;
                            if (_nPreparedFramesIndx < (int)nFramesQty)
                                throw new TimeoutException("can't get next video frame 'on demand'");
                            return null;
                        }
                    }
                    else
                    {
                        if (bFileEndless)
                            return null;
                        throw new TimeoutException("video queue is empty");
                    }
                }
                _cTimingsFNVG.Restart("frame waiting");
				if (0 < _aqVideoFrames.Count)
				{
					lock (_aqVideoFrames)
						cRetVal = _aqVideoFrames.Dequeue();
					_cTimingsFNVG.CheckIn("dequeue"); // logging
				}
				_cTimingsFNVG.Stop("FrameNextVideoGet: [queue=" + _aqVideoFrames.Count + "]", 40);
				(new Logger()).WriteDebug4("return");
				return cRetVal;
			}
			public Frame FrameNextAudioGet()
			{
				(new Logger()).WriteDebug4("in");
                if (!_bFrameGettingStarted) _bFrameGettingStarted = true;
                if (null == _aqAudioFrames)
					return null;
				Frame cRetVal = null;
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
                if (!_bFileEnd && 1 > _aqAudioFrames.Count)
                {
                    if (_eFramesGettingMode == PlaybackMode.GivesFrameOnDemand)
                    {
                        if (!GetAndAddFrame() && _aqAudioFrames.Count <= 0)
                        {
                            if (bFileEndless)
                                return null;
                            throw new TimeoutException("can't get next audio frame 'on demand'");
                        }
                    }
                    else
                    {
                        if (bFileEndless)
                            return null;
                        throw new TimeoutException("audio queue is empty");
                    }
                }
				cTimings.Restart("frame waiting");
				lock (_aqAudioFrames)
				{
					if (0 < _aqAudioFrames.Count)
						cRetVal = _aqAudioFrames.Dequeue();
					cTimings.CheckIn("dequeue"); // logging
				}
				cTimings.Stop("frame:next:audio: >20ms", 20);
				(new Logger()).WriteDebug4("return");
				return cRetVal;
			}

			private void FrameDecodeVideo()
			{
				(new Logger()).WriteDebug4("in");
				if (null == _cFormatVideoTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов

				int nVideoFrameFinished = 0;
				IntPtr pPacketNext = NULL;

				_cTimingsFDV.TotalRenew();
				int nIndxTimes = 0;

				while (true)
				{
					nIndxTimes++;
					_cTimingsPAD.TotalRenew();
					while (NULL == pPacketNext)
					{
						while (1 > _aqVideoPackets.Count)  // пакеты не пришли, а надо. Если не придут, то через таймаут_1 (8 сек) проскочим это видео при фризе, а через таймаут_2 (30 сек) будет закрыт этот таск декодинга
                        {
							if (_bFileEnd)
							{
								pPacketNext = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
								helpers.WinAPI.memset(pPacketNext, 0, Marshal.SizeOf(typeof(AVPacket)));
								Functions.av_init_packet(pPacketNext);
								break;
							}
							lock (_oSyncRoot)
								PacketNext();
						}
						if (!_bFileEnd)
							pPacketNext = _aqVideoPackets.Peek();
					}
					_cTimingsPAD.Restart("packet number -" + nIndxTimes + "- (" + _aqPackets.Count + " packets in q)");
					if (null == _cFrameVideo)
					{
						_cFrameVideo = FramesRotation.Dequeue(_cFormatVideo, 1);
						//_cFrameVideo = new Frame(_cFormatVideo);
						_cTimingsPAD.Restart("new frame");
					}
					try
					{
						int nError = Functions.avcodec_decode_video2(_cFormatVideo.pAVCodecContext, _cFrameVideo, ref nVideoFrameFinished, pPacketNext);
						_cTimingsPAD.Restart("decode packet");
						if (!_bFileEnd)
						{
							Functions.av_free_packet(pPacketNext);
							Functions.av_freep(ref pPacketNext);
							_aqVideoPackets?.Dequeue();
						}
						_cTimingsPAD.Stop("1 packet", "free mem for packet", 40);
					}
					catch (Exception ex)
					{
						(new Logger()).WriteError(ex);
					}

                    if (0 < nVideoFrameFinished)
                    {
                        _cTimingsFDV.Restart("packets_and_decode [times:" + nIndxTimes + "]");
                        Frame cFrame;
                        cFrame = FramesRotation.Dequeue(_cFormatVideoTarget, 2);
                        if (null == cFrame) //ротация кадров
                        {
                            //cFrame = new Frame(_cFormatVideoTarget);
                            //cFrame.Disposing += cFrameVideo_Disposing;
                            (new Logger()).WriteError("video frame is null!");  // + nFramesQueueVideo++
                        }

                        _cTimingsFDV.Restart("frame_get");
                        _cFormatVideo.Transform(_cFormatVideoTarget, _cFrameVideo, cFrame);
                        _cTimingsFDV.Restart("transform");
                        cFrame.bKeyframe = _cFrameVideo.bKeyframe;
                        cFrame.nPTS = _cFrameVideo.nPTS;
                        _cTimingsFDV.Restart("pts");
                        lock (_aqVideoFrames)
                            _aqVideoFrames.Enqueue(cFrame);
                        _cTimingsFDV.Restart("lock2");

                        if (_bDoWritingFrames)
                        {
                            if (null != cFrame)
                            {
                                byte[] aBytes = new byte[_cFormatVideoTarget.nBufferSize];
                                System.Runtime.InteropServices.Marshal.Copy(cFrame.pBytes, aBytes, 0, (int)_cFormatVideoTarget.nBufferSize);
                                lock (_aqWritingFrames)
                                    _aqWritingFrames.Enqueue(aBytes);
                            }
                            _cTimingsFDV.Restart("writing");
                        }

                        if (!_bFileEnd)
                            break;
                    }
                    else if (_bFileEnd)
                    {
                        if (NULL != pPacketNext)
                        {
                            Functions.av_free_packet(pPacketNext);
                            Functions.av_freep(ref pPacketNext);
                        }
                        int nDiff = (int)nFramesQty - _nPreparedFramesIndx - Math.Abs(_aqVideoFrames.Count - (_aqAudioFrames == null ? 0 : _aqAudioFrames.Count));
                        nDiff = Math.Abs(nDiff);

                        if (nDiff > 1 && nDiff <= 20) // несоответствие маленькое. При diff = -1 || 1 || 0  на практике кадры берутся все. Больше пока не встречалось - интересно посмотреть...
                            (new Logger()).WriteWarning("file ended in wrong way [diff=" + nDiff + "][fq="+nFramesQty+ "][prep=" + _nPreparedFramesIndx + "][vidfr=" + _aqVideoFrames.Count + "][audfr=" + _aqAudioFrames?.Count + "]"); //logging
                        else if (nDiff > 20) // большое
                            (new Logger()).WriteError("file ended in wrong way [diff=" + nDiff + "][fq=" + nFramesQty + "][prep=" + _nPreparedFramesIndx + "][vidfr=" + _aqVideoFrames.Count + "][audfr=" + _aqAudioFrames?.Count + "]"); //logging
                        throw new Exception("file ended in wrong way"); // плохого ничего не даёт
                    }
                    else
                        ;  // это случай неполного кадра из пакета и надо брать еще пакет и т.п. - ничего не надо делать
                }
				_cTimingsFDV.Stop("FrameDecodeVideo", 50); //FPS
				(new Logger()).WriteDebug4("return");
			}

			private void FrameDecodeAudio()
			{
				(new Logger()).WriteDebug4("in");
				if (null == _cFormatAudioTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов

				_cTimingsFDA.TotalRenew();
				bool bFrameDecoded = false;
				//Frame cSamplesTarget = null;
				Frame cFrame;
				cFrame = FramesRotation.Dequeue(_cFormatAudioTarget, 3);
				if (null == cFrame) //ротация кадров
				{
					//cFrame = new Frame(_cFormatAudioTarget);
					//cFrame.Disposing += cFrameAudio_Disposing;
					(new Logger()).WriteError("audio frame is null!");  // + nFramesQueueAudio++
				}

				int nBytesCapacity = 0;
				int nBytesOffset = 0;
				int nLength = 0;

				//AVPacket stPacket;
				if (null != _aBytesRemainder)
				{
					if (_nRemainderSize > cFrame.aBuffer.Length)
					{
						(new Logger()).WriteDebug("audio_decode: error - toArray processed!");
						Array.Copy(_aBytesRemainder, 0, cFrame.aBuffer, 0, cFrame.aBuffer.Length);
						_aBytesRemainder = _aBytesRemainder.Skip(cFrame.aBuffer.Length).Take(_nRemainderSize - cFrame.aBuffer.Length).ToArray();   // много копий массива
						nBytesOffset += cFrame.aBuffer.Length;
					}
					else
					{
						Array.Copy(_aBytesRemainder, 0, cFrame.aBuffer, 0, _nRemainderSize);
						nBytesOffset += _nRemainderSize;
						_aBytesRemainder = null;
					}
				}
				while (cFrame.nLength > nBytesOffset)
				{
					_aPacketBytes = null;
					if (NULL == _pPacketAudioDub)
					{
						_pPacketAudioDub = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
						helpers.WinAPI.memset(_pPacketAudioDub, 0, Marshal.SizeOf(typeof(AVPacket)));
						Functions.av_init_packet(_pPacketAudioDub);
						//_stPacketAudio = (AVPacket)Marshal.PtrToStructure(_pPacketAudioDub, typeof(AVPacket));
						Marshal.Copy(IntPtr.Add(_pPacketAudioDub, _nOffset_size), _aPacket_size_AudioDub, 0, 1);
						Marshal.Copy(IntPtr.Add(_pPacketAudioDub, _nOffset_data), _aPacket_data_AudioDub, 0, 1);
					}
					_cTimingsFDA.Restart("allocation");
					while (true)
					{
						// NOTE: the audio packet can contain several frames 
						while (_aPacket_size_AudioDub[0] > 0)
						{
							if (null == _cFrameAudio)
							{
								_cFrameAudio = FramesRotation.Dequeue(_cFormatAudio, 4);
								//_cFrameAudio = new Frame(_cFormatAudio);
							}
							//Marshal.StructureToPtr(_stPacketAudio, _pPacketAudioDub, true);
							Marshal.Copy(_aPacket_size_AudioDub, 0, IntPtr.Add(_pPacketAudioDub, _nOffset_size), 1);
							Marshal.Copy(_aPacket_data_AudioDub, 0, IntPtr.Add(_pPacketAudioDub, _nOffset_data), 1);
							nLength = Functions.avcodec_decode_audio4(_cFormatAudio.pAVCodecContext, _cFrameAudio, ref bFrameDecoded, _pPacketAudioDub);
							_cTimingsFDA.CheckIn("decode");
							if (nLength < 0)
							{
								//_stPacketAudio.size = 0;
								_aPacket_size_AudioDub[0] = 0;
								break;
							}
							//_stPacketAudio.data += nLength;
							//_stPacketAudio.size -= nLength;
							_aPacket_data_AudioDub[0] += nLength;
							_aPacket_size_AudioDub[0] -= nLength;
							if (!bFrameDecoded)
								continue;
							_cTimingsFDA.Restart("frame");

							//cSamplesTarget = _cFormatAudio.Transform(_cFormatAudioTarget, new Frame(_cFormatAudio, _cFrameAudio));

							////cSamplesTarget = new Frame(_cFormatAudio);
							////aPacketBytes = cSamplesTarget.aBytes;
							////cSamplesTarget.Dispose();

							_cFrameAudio.PassSamples(_cFormatAudio);
							Frame cSamplesTarget = _cFormatAudio.Transform(_cFormatAudioTarget, _cFrameAudio);
							_aPacketBytes = BytesGet(cSamplesTarget.nLength, 1);
							cSamplesTarget.CopyBytesTo(_aPacketBytes);

							//_cFormatAudio.Transform(_cFormatAudioTarget, _cFrameAudio).CopyBytesTo(_aPacketBytes);   //new Frame(_cFormatAudio, _cFrameAudio)  //---------------TRANSFORM  AUDIO
							_cTimingsFDA.Restart("transform");
							break;
						}
						if (null != _aPacketBytes)
							break;
						if (NULL != _pPacketAudio)
						{
							Functions.av_free_packet(_pPacketAudio);
							Functions.av_freep(ref _pPacketAudio);
							_cTimingsFDA.Restart("packet free");
						}
						while (!_bFileEnd && 1 > _aqAudioPackets.Count)
						{
							lock (_oSyncRoot)
								PacketNext();
						}
						if (_bFileEnd && 1 > _aqAudioPackets.Count)
							break;
						lock (_oSyncRoot)
							_pPacketAudio = _aqAudioPackets.Dequeue();
						//stPacket = (AVPacket)Marshal.PtrToStructure(_pPacketAudio, typeof(AVPacket));
						Marshal.Copy(IntPtr.Add(_pPacketAudio, _nOffset_size), _aPacket_size_Audio, 0, 1);
						Marshal.Copy(IntPtr.Add(_pPacketAudio, _nOffset_data), _aPacket_data_Audio, 0, 1);


						//_stPacketAudio.data = stPacket.data;
						//_stPacketAudio.size = stPacket.size;
						_aPacket_data_AudioDub[0] = _aPacket_data_Audio[0];
						_aPacket_size_AudioDub[0] = _aPacket_size_Audio[0];
						_cTimingsFDA.Restart("packets");
					}
					if (null == _aPacketBytes)
					{
						if (0 < nBytesOffset)  // подналить надо тишины до полного кадра, раз уж часть кадра всё-равно есть и отдать его (бывает не хватает байт нескольких)
						{
							nBytesCapacity = cFrame.nLength - nBytesOffset;
							while (nBytesCapacity > 0)
							{
								if (aBytesSilent.Length < nBytesCapacity)
									nBytesCapacity = aBytesSilent.Length;
								Array.Copy(aBytesSilent, 0, cFrame.aBuffer, nBytesOffset, nBytesCapacity);
								nBytesOffset += nBytesCapacity;
								nBytesCapacity = cFrame.nLength - nBytesOffset;
							}
							break;
						}
						throw new Exception("audio packet is null");
					}
					nBytesCapacity = _aPacketBytes.Length;
					if (cFrame.nLength < nBytesOffset + nBytesCapacity)
					{
						nBytesCapacity = cFrame.nLength - nBytesOffset;
						_nRemainderSize = _aPacketBytes.Length - nBytesCapacity;
						if (null == _aBytesRemainder)
							_aBytesRemainder = BytesGet(_aPacketBytes.Length, 2);  //new byte[aPacketBytes.Length - nBytesCapacity];
						Array.Copy(_aPacketBytes, nBytesCapacity, _aBytesRemainder, 0, _nRemainderSize);
					}
					Array.Copy(_aPacketBytes, 0, cFrame.aBuffer, nBytesOffset, nBytesCapacity);
					nBytesOffset += nBytesCapacity;
					_cTimingsFDA.Restart("accumulation");
				}
				//_aPacketBytes = null;
				_cTimingsFDA.Stop("frame:decode:audio: ", 30);//FPS


//#if DEBUG
                //DNF   имитация зависания
                //DateTime dtSleep = new DateTime(2017, 4, 28, 19, 34, 40);
                //if (DateTime.Now > dtSleep && DateTime.Now < dtSleep.AddSeconds(1))
                //    System.Threading.Thread.Sleep(1500000);
//#endif



                lock (_aqAudioFrames)
					_aqAudioFrames.Enqueue(cFrame);

				if (_bDoWritingFrames)
				{
					if (null != cFrame)
					{
						byte[] aBytes = new byte[cFrame.nLength];
						(new Logger()).WriteDebug("frame decode audio: - new bytes processed for writingframes! [size=" + cFrame.nLength + "]");
						aBytes = cFrame.aBytesCopy;
						lock (_aqWritingAudioFrames)
							_aqWritingAudioFrames.Enqueue(aBytes);
					}
					_cTimingsFDV.Restart("writing");
				}

				(new Logger()).WriteDebug4("return");
			}
			private static bool cFrameVideo_Disposing(Frame cFrame)
			{
				FramesRotation.Enqueue(cFrame, 0);
				return false;


				//            lock (_aqVideoFramesFree)
				//            {
				//                if (20 < _aqVideoFramesFree.Count)
				//                {
				//		(new Logger()).WriteDebug3("video frame removed. total:" + nFramesQueueVideo--);
				//                    return true;
				//                }
				//                else
				//                    _aqVideoFramesFree.Enqueue(cFrame);
				//            }
				//return false;
			}
			private static bool cFrameAudio_Disposing(Frame cFrame)
			{
				FramesRotation.Enqueue(cFrame, 1);
				return false;
				//lock (_aqAudioFramesFree)
				//            {
				//                if (20 < _aqAudioFramesFree.Count)
				//                {
				//		(new Logger()).WriteDebug3("audio frame removed. total:" + nFramesQueueAudio--);
				//                    return true;
				//                }
				//                else
				//                    _aqAudioFramesFree.Enqueue(cFrame);
				//            }
				//return false;
			}

			private int _nMinQtyPackets = 0;
            private long _nMinBlockSize = 10 * 1024 * 1024;
            static private object _oLockPacketsBlock = new object();
            private bool _bWasErrorGettingPackets;
            private bool bTested;
            private bool _bLoggedEmergencyCall;
            private bool _bDoingPacketsBlockGetNow;

            [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
            private void PacketsBlockGet(bool bEmergencyCall)
            {
                try
                {
                    long nBlockSizeBkp = nBlockSize;
                    lock (_oLockPacketsBlock)
                    {
                        if (_bDoingPacketsBlockGetNow)  //_cCurrentInputWithPacketGeting == this
                        {
                            Thread.Sleep(1);
                            return;
                        }
                        _bDoingPacketsBlockGetNow = true;
                        if (bEmergencyCall)
                        {
                            nBlockSize = _nMinBlockSize;
                            if (!_bLoggedEmergencyCall)
                            {
                                _bLoggedEmergencyCall = true;
                                (new Logger()).WriteNotice("PacketsBlockGet: Was Emergency Call [file=" + _sFile + "]");
                            }
                        }
                    }
                    IntPtr pPacket;
                    int nResult;
                    _nCurrentBlockSize = 0;
                    long nSleepSize = 0;


                    //#if DEBUG
                    //                if (!bTested && SIO.Path.GetFileName(_sFile) == "006.mxf") //DNF !!!!!!!  TEST!!!!    && cI._nPreparedFramesIndx > 100
                    //                {
                    //                    bTested = true;
                    //                    Thread.Sleep(40000);
                    //                    (new Logger()).WriteDebug("I paused getting packets )) [file=" + _sFile + "]");
                    //                    return;
                    //                }
                    //#endif


                    while (true)
                    {
                        _aPacket_size[0] = 0;
                        pPacket = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
                        helpers.WinAPI.memset(pPacket, 0, Marshal.SizeOf(typeof(AVPacket)));
                        if (pPacket != null && pPacket != NULL)
                        {
                            Functions.av_init_packet(pPacket);

                            if (-1 < (nResult = _cFormatCtx.PacketRead(pPacket)))
                            {
                                Marshal.Copy(IntPtr.Add(pPacket, _nOffset_stream_index), _aPacket_size, 0, 1);
                                if (_nVideoStreamIndx != _aPacket_size[0] && _nAudioStreamIndx != _aPacket_size[0])
                                {
                                    _nTotalDataAndOtherPackets++;
                                    Functions.av_free_packet(pPacket);
                                    Functions.av_freep(ref pPacket);
                                    continue;
                                }
                                Marshal.Copy(IntPtr.Add(pPacket, _nOffset_size), _aPacket_size, 0, 1);
                                if (_aPacket_size[0] > 0)
                                {
                                    _nTotalPacketsSize += _aPacket_size[0];
                                    _nCurrentBlockSize += _aPacket_size[0];
                                    nSleepSize += _aPacket_size[0];
                                    _nTotalPacketsQty++;
                                    _aqPackets.Enqueue(pPacket);
#if DEBUG
                                    //if (nSleepSize > 10000000)
                                    //{
                                    //	Thread.Sleep(1);
                                    //	nSleepSize = 0;
                                    //}
#endif
                                }
                                _bPacketsReadingSuccess = true;
                            }
                            else if (!bFileEndless)
                            {
                                if (-541478725 != nResult)
                                    (new Logger()).WriteError("File.Input.GetAndSortNextPacket.PacketRead = " + nResult);
                                _bPacketsReadingDone = true;
                                (new Logger()).WriteDebug("_PacketsReadingDone = true [nResult = " + nResult + "]");
                                break;
                            }
                            else
                                Thread.Sleep(20);

                            if (_nTotalPacketsSize > nBlockSize && _nMinQtyPackets == 0)
                            {
                                _nMinQtyPackets = _nTotalPacketsQty / 2;
                            }

                            if (_nCurrentBlockSize > nBlockSize)
                            {
                                break;
                            }
                        }
                        else
                            (new Logger()).WriteDebug("bad packet - here it is! [packet=" + (pPacket == null ? "null" : pPacket.ToString()) + "][input=" + GetHashCode() + "]");
                    }
                    lock (_oLockPacketsBlock)
                    {
                        _bDoingPacketsBlockGetNow = false;
                        if (bEmergencyCall)
                            nBlockSize = nBlockSizeBkp;
                    }
                }
                catch(Exception ex)  // т.о. лочим эту функцию по _bDoingPacketsBlockGetNow
                {
                    _bPacketsReadingDone = true;
                    (new Logger()).WriteError("File.Input.GetAndSortNextPacket.PacketRead", ex);
                }
            }
            private void PacketNext()
			{
				_nPacketIndx++; // logging
                if (!_bFileEnd)
                {
                    if (0 < _aqPackets.Count)
                    {
                        while (!_aqPackets.TryDequeue(out _pPacket))
                        {
                            if (!_bWasErrorGettingPackets)
                            {
                                _bWasErrorGettingPackets = true;
                                (new Logger()).WriteError("getting packets!! very strange! [packets=" + _aqPackets.Count + "]"); // было ли такое??
                            }
                            Thread.Sleep(0);
                        }
                        _bWasErrorGettingPackets = false;
                        //AVPacket stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));

                        if (_pPacket != null && _pPacket != NULL)
                            Marshal.Copy(IntPtr.Add(_pPacket, _nOffset_stream_index), _aPacket_streamindex, 0, 1);
                        else
                            _aPacket_streamindex[0] = -1;
                        if (_nVideoStreamIndx == _aPacket_streamindex[0])   //stPacket.stream_index
                        {
                            _aqVideoPackets?.Enqueue(_pPacket);
                            _nTotalVideoPackets++;
                        }
                        else if (_nAudioStreamIndx == _aPacket_streamindex[0])
                        {
                            _aqAudioPackets?.Enqueue(_pPacket);
                            _nTotalAudioPackets++;
                        }
                        else
                        {
                            if (2 != _aPacket_streamindex[0])
                                (new Logger()).WriteDebug("bad packet - here it is! [packet=" + (_pPacket == null ? "null" : _pPacket.ToString()) + "][stream=" + _aPacket_streamindex[0] + "]");
                            else
                                _nTotalDataAndOtherPackets++;
                            Functions.av_free_packet(_pPacket);
                            Functions.av_freep(ref _pPacket);
                        }
                    }
                    else if (!bFileEndless && (_bPacketsReadingDone || _bFileEnd || _bClosed))
                    {
                        _bFileEnd = true;
                        (new Logger()).WriteDebug("_bFileEnd = true");
                    }
                    else
                    {
                        PacketsBlockGet(true); // берем так - скорее всего повис thread взятия пакетов. сработает только если повисли не мы!
                        //if (_bPacketsReadingDone)  // т.к. _aqPackets еще не пуст скорее всего
                        //    _bFileEnd = true;
                    }
                }
            }
			private void WritingFramesWorker(object cState)
			{
				if (_cFormatVideoTarget == null || sDebugFolder == null)
					return;
				string _sWritingFramesFile = SIO.Path.Combine(sDebugFolder, "WritingDebugFrames.txt");
				string _sWritingFramesDir = SIO.Path.Combine(sDebugFolder, "FFMPEG/");
				int _nFramesCount = 0;
				System.Drawing.Bitmap cBFrame;
				System.Drawing.Imaging.BitmapData cFrameBD;
				string[] aLines;
				bool bQueueIsNotEmpty = false;
				byte[] aBytes;
				string sInputName = SIO.Path.GetFileNameWithoutExtension(this._sFile);
				int nWidth = _cFormatVideoTarget.nWidth;
				int nHeight = _cFormatVideoTarget.nHeight;
				try
				{
					while (!_bFileEnd && !_bClosed || _aqWritingAudioFrames.Count > 0)
					{
						try
						{
							if (System.IO.File.Exists(_sWritingFramesFile))
							{
								aLines = System.IO.File.ReadAllLines(_sWritingFramesFile);
								if ("ffmpeg" == aLines.FirstOrDefault(o => o.ToLower() == "ffmpeg"))
								{
									_bDoWritingFrames = true;
									if (!System.IO.Directory.Exists(_sWritingFramesDir))
										System.IO.Directory.CreateDirectory(_sWritingFramesDir);
								}
								else
								{
									_aqWritingFrames.Clear();
									_bDoWritingFrames = false;
									if (_aqWritingAudioFrames.Count > 0)
									{   // на 3 минутки хедер на 48  16 бит литл ендиан стерео - синтезировать его лень было
										byte[] aHeaderWav = new byte[46] { 0x52, 0x49, 0x46, 0x46, 0x02, 0x92, 0x58, 0x02, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20, 0x12, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x80, 0xBB, 0x00, 0x00, 0x00, 0xEE, 0x02, 0x00, 0x04, 0x00, 0x10, 0x00, 0x00, 0x00, 0x64, 0x61, 0x74, 0x61, 0x00, 0x78, 0x58, 0x02 };
										System.IO.FileStream stream = new System.IO.FileStream(_sWritingFramesDir + sInputName + "samples.wav", System.IO.FileMode.Append);
										stream.Write(aHeaderWav, 0, aHeaderWav.Length);
										byte[] aBytesTMP;
										while (0 < _aqWritingAudioFrames.Count)
										{
											aBytesTMP = _aqWritingAudioFrames.Dequeue();
											stream.Write(aBytesTMP, 0, aBytesTMP.Length);
										}
										stream.Close();
									}
								}
							}
							else
								_bDoWritingFrames = false;

							if (_bDoWritingFrames || 0 < _aqWritingFrames.Count)
							{
								while (bQueueIsNotEmpty)
								{
									cBFrame = new System.Drawing.Bitmap(nWidth, nHeight);
									cFrameBD = cBFrame.LockBits(new System.Drawing.Rectangle(0, 0, cBFrame.Width, cBFrame.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
									lock (_aqWritingFrames)
									{
										aBytes = _aqWritingFrames.Dequeue();
										if (0 < _aqWritingFrames.Count)
											bQueueIsNotEmpty = true;
										else
											bQueueIsNotEmpty = false;
									}
									System.Runtime.InteropServices.Marshal.Copy(aBytes, 0, cFrameBD.Scan0, (int)_cFormatVideoTarget.nBufferSize);
									cBFrame.UnlockBits(cFrameBD);
									cBFrame.Save(_sWritingFramesDir + sInputName + "frame_" + _nFramesCount.ToString("0000") + ".png");
									_nFramesCount++;

									aLines = System.IO.File.ReadAllLines(_sWritingFramesFile);
									if (null == aLines.FirstOrDefault(o => o.ToLower() == "ffmpeg"))
										_bDoWritingFrames = false;
									if (3000 < _aqWritingFrames.Count)
									{
										_bDoWritingFrames = false;
										System.IO.File.Delete(_sWritingFramesFile);
									} 
								}
								System.Threading.Thread.Sleep(40);//FPS
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
								System.Threading.Thread.Sleep(2000);
							}
						}
						catch (System.Threading.ThreadAbortException)
						{ }
						catch (Exception ex)
						{
							(new Logger()).WriteError(ex);
						}
					}
				}
				catch (System.Threading.ThreadAbortException)
				{ }
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
			}
		}
		public class Output : File
		{
			#region members
			private IntPtr _pFormatOutput; //AVOutputFormat*
			private IntPtr _pStreamVideo; //AVStream*
			private IntPtr _pStreamAudio; //AVStream*
			private IntPtr _pBitStreamFilterVideo;
			private IntPtr _pBitStreamFilterAudio;

            new private object _oDisposeLock;
            new private bool _bDisposed;

            //private double _nPTSVideo;
            //private double _nPTSAudio;
            #endregion

            private Output(Format.Video cFormatVideo, Format.Audio cFormatAudio)
				: base()
			{
                //_cFormatVideo = new Format.Video(cFormatVideo);
                //_cFormatAudio = new Format.Audio(cFormatAudio);
                _oDisposeLock = new object();
                _cFormatVideo = cFormatVideo;
				_cFormatAudio = cFormatAudio;
				_pFormatOutput = NULL;
				_pStreamVideo = NULL;
				_pStreamAudio = NULL;
				_pBitStreamFilterVideo = NULL;
				_sFile = null;
			}
			public Output(string sFile)
				: this(sFile, new Format.Video(0, 0, PixelFormat.AV_PIX_FMT_NONE, AVFieldOrder.AV_FIELD_UNKNOWN), new Format.Audio(48000, 2, AVSampleFormat.AV_SAMPLE_FMT_S16))
			{ }
			public Output(string sFile, Format.Video cFormatVideo, Format.Audio cFormatAudio)
				: this(sFile, null, null, cFormatVideo, cFormatAudio, null)
			{ }
			public Output(string sFile, string sType, Format.Video cFormatVideo, Format.Audio cFormatAudio)
				: this(sFile, sType, null, cFormatVideo, cFormatAudio, null)
			{ }
			public Output(string sFile, SIO.Stream cStream, Format.Video cFormatVideo, Format.Audio cFormatAudio)
				: this(sFile, null, cStream, cFormatVideo, cFormatAudio, null)
			{ }
			public Output(string sFile, SIO.Stream cStream, Format.Video cFormatVideo, Format.Audio cFormatAudio, Flags? eFlags)
				: this(sFile, null, cStream, cFormatVideo, cFormatAudio, eFlags)
			{ }
			public Output(string sFile, string sType, SIO.Stream cStream, Format.Video cFormatVideo, Format.Audio cFormatAudio, Flags? eFlags)
				: this(cFormatVideo, cFormatAudio)
			{
				try
				{
					if (null == sType)
					{
						if (null == cStream)
							sType = System.IO.Path.GetExtension(sFile).ToLower().Substring(1);
						else
							sType = sFile;
					}
					if (null == cStream)
						_sFile = sFile.Substring(0, (sFile.Length > 1024 ? 1024 : sFile.Length));

					_cFormatCtx = AVFormatContext.CreateOutput(sType, cStream);
					_pFormatOutput = _cFormatCtx.oformat;
					AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));

					if (null != _cFormatVideo)
					{
						if (AVCodecID.CODEC_ID_H264 == cFormatVideo.eCodecID && (new string[] { "f4v", "flv", "f4f", "mpegts", "mp4" }).Contains(sType))
							_pBitStreamFilterVideo = Functions.av_bitstream_filter_init(new StringBuilder("h264_mp4toannexb"));
						if (stAVOutputFormat.video_codec != _cFormatVideo.eCodecID)
							stAVOutputFormat.video_codec = _cFormatVideo.eCodecID;
						VideoStreamCreate(eFlags);
					}
					if (null != _cFormatAudio)
					{
						if (AVCodecID.CODEC_ID_AAC == cFormatAudio.eCodecID && (new string[] { "mov", "f4v", "flv", "f4f", "mp4" }).Contains(sType))
							_pBitStreamFilterAudio = Functions.av_bitstream_filter_init(new StringBuilder("aac_adtstoasc"));
						if (stAVOutputFormat.audio_codec != _cFormatAudio.eCodecID)
							stAVOutputFormat.audio_codec = _cFormatAudio.eCodecID;
						AudioStreamCreate(eFlags);
					}

					if (null != _sFile && !(0 < (stAVOutputFormat.flags & Constants.AVFMT_NOFILE)))
						_cFormatCtx.SaveOutput(_sFile);

					Marshal.StructureToPtr(stAVOutputFormat, _pFormatOutput, true);
					_cFormatCtx.WriteHeader();
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
					Dispose();
					throw;
				}
			}

			~Output()
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
			override public void Dispose()
			{
                lock (_oDisposeLock)
                {
                    if (_bDisposed)
                        return;
                    _bDisposed = true;
                }
                try
				{
					if (null != _cFormatCtx)
					{
						//Flush();
						_cFormatCtx.WriteTrailer();
						if (NULL != _pBitStreamFilterVideo)
							Functions.av_bitstream_filter_close(_pBitStreamFilterVideo);
						if (NULL != _pBitStreamFilterAudio)
							Functions.av_bitstream_filter_close(_pBitStreamFilterAudio);
						_pStreamVideo = _pStreamAudio = _pBitStreamFilterAudio = _pBitStreamFilterVideo = NULL;
						if (NULL == _pFormatOutput)
							throw new Exception("invalid output format");
					}
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				try
				{
					base.Dispose();
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
			}

			private void VideoStreamCreate(Flags? eFlags)
			{
				AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
				if (stAVOutputFormat.video_codec == AVCodecID.CODEC_ID_NONE)
					return;
				_cFormatVideo.stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(_cFormatVideo.pAVCodecContext, typeof(AVCodecContext));

				_pStreamVideo = _cFormatCtx.StreamAdd();
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamVideo, typeof(AVStream));
				//stAVStream.time_base.den = stAVStream.r_frame_rate.den = _cFormatVideo.stAVCodecContext.time_base.den;
				//stAVStream.time_base.num = stAVStream.r_frame_rate.num = _cFormatVideo.stAVCodecContext.time_base.num;
				//stAVOutputFormat.video_codec = _cFormatVideo.stAVCodecContext.codec_id;
				if (null != eFlags)
				{
					if (eFlags.Value.HasFlag(Flags.GlobalHeader))
					{
						stAVOutputFormat.flags |= Constants.AVFMT_GLOBALHEADER;
						_cFormatVideo.stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
					}
					else
					{
						stAVOutputFormat.flags ^= Constants.AVFMT_GLOBALHEADER;
						_cFormatVideo.stAVCodecContext.flags ^= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
					}
				}
				else if (0 < (stAVOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
					_cFormatVideo.stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
				Marshal.StructureToPtr(stAVOutputFormat, _pFormatOutput, true);
				Marshal.StructureToPtr(_cFormatVideo.stAVCodecContext, _cFormatVideo.pAVCodecContext, true);
				stAVStream.codec = _cFormatVideo.pAVCodecContext;
				Marshal.StructureToPtr(stAVStream, _pStreamVideo, true);
			}
			private void AudioStreamCreate(Flags? eFlags)
			{
				AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
				if (stAVOutputFormat.audio_codec == AVCodecID.CODEC_ID_NONE)
					return;
				_cFormatAudio.stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(_cFormatAudio.pAVCodecContext, typeof(AVCodecContext));

				_pStreamAudio = _cFormatCtx.StreamAdd();
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamAudio, typeof(AVStream));
				//stAVStream.time_base.den = stAVStream.r_frame_rate.den = _cFormatAudio.stAVCodecContext.time_base.den;
				//stAVStream.time_base.num = stAVStream.r_frame_rate.num = _cFormatAudio.stAVCodecContext.time_base.num;
				//stAVOutputFormat.audio_codec = _cFormatAudio.stAVCodecContext.codec_id;
				if (null != eFlags)
				{
					if (eFlags.Value.HasFlag(Flags.GlobalHeader))
					{
						stAVOutputFormat.flags |= Constants.AVFMT_GLOBALHEADER;
						_cFormatAudio.stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
					}
					else
					{
						stAVOutputFormat.flags ^= Constants.AVFMT_GLOBALHEADER;
						_cFormatAudio.stAVCodecContext.flags ^= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
					}
				}
				else if (0 < (stAVOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
					_cFormatAudio.stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;

				Marshal.StructureToPtr(stAVOutputFormat, _pFormatOutput, true);
				Marshal.StructureToPtr(_cFormatAudio.stAVCodecContext, _cFormatAudio.pAVCodecContext, true);
				stAVStream.codec = _cFormatAudio.pAVCodecContext;
				Marshal.StructureToPtr(stAVStream, _pStreamAudio, true);
			}
			public void FrameNextVideo(Frame cFrameSource)
			{
				FrameNextVideo(null, cFrameSource);
			}
			public void FrameNextVideo(Format.Video cFormatSource, Frame cFrameSource)
			{
				if (null == cFrameSource)
					return;
				if (NULL == _pStreamVideo)
					throw new Exception("there is no video stream in file");
				FrameNext(_pStreamVideo, cFormatSource, cFrameSource, _cFormatVideo, _pBitStreamFilterVideo);
			}
			public void FrameNextAudio(Frame cFrameSource)
			{
				FrameNextAudio(null, cFrameSource);
			}
			public void FrameNextAudio(Format.Audio cFormatSource, Frame cFrameSource)
			{
				if (null == cFrameSource)
					return;
				if (NULL == _pStreamAudio)
					throw new Exception("there is no audio stream in file");
				FrameNext(_pStreamAudio, cFormatSource, cFrameSource, _cFormatAudio, _pBitStreamFilterAudio);
			}
			public void Flush()
			{
				if (NULL == _pStreamAudio)
					throw new Exception("there is no audio stream in file");
				FrameNext(_pStreamAudio, null, null, _cFormatAudio, _pBitStreamFilterAudio);
			}
			private void FrameNext(IntPtr pStream, Format cFormatSource, Frame cFrameSource, Format cFormatTarget, IntPtr pBitStreamFilter)
			{
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(pStream, typeof(AVStream));
				Frame[] aFrames;
				if (null == cFormatSource)
				{
					if (null == cFrameSource && cFormatTarget is Format.Audio)
						aFrames = ((Format.Audio)cFormatTarget).Flush();
					else
						aFrames = new Frame[] { new Frame(null, cFrameSource.aBytesCopy) { nPTS = cFrameSource.nPTS, bKeyframe = cFrameSource.bKeyframe } };
				}
				else
					aFrames = cFormatSource.Convert(cFormatTarget, cFrameSource);
				IntPtr pAVPacket;
				AVPacket stAVPacket;
				for (int nIndx = 0; aFrames.Length > nIndx; nIndx++)
				{
					pAVPacket = Functions.av_malloc((uint)(Marshal.SizeOf(typeof(AVPacket))));
					Functions.av_init_packet(pAVPacket);
					stAVPacket = (AVPacket)Marshal.PtrToStructure(pAVPacket, typeof(AVPacket));
					if (aFrames[nIndx].nPTS != Constants.AV_NOPTS_VALUE)
						stAVPacket.pts = Functions.av_rescale_q(aFrames[nIndx].nPTS, cFormatTarget.stAVCodecContext.time_base, stAVStream.time_base);
					if (aFrames[nIndx].bKeyframe)
						stAVPacket.flags |= Constants.AV_PKT_FLAG_KEY;
					stAVPacket.stream_index = stAVStream.index;
					stAVPacket.size = aFrames[nIndx].nLength;
					stAVPacket.data = aFrames[nIndx].pBytes;
					lock (_aFramesLocked)
						_aFramesLocked.Add(aFrames[nIndx]);
					stAVPacket.buf = Functions.av_buffer_create(stAVPacket.data, stAVPacket.size, Marshal.GetFunctionPointerForDelegate(_fFrameUnlock), aFrames[nIndx], 0);
					//System.IO.File.AppendAllText("packets", stAVPacket.pts + "\t" + stAVPacket.size + Environment.NewLine);
					if (NULL != pBitStreamFilter && 0 != Functions.av_bitstream_filter_filter(pBitStreamFilter, cFormatTarget.pAVCodecContext, NULL, ref stAVPacket.data, ref stAVPacket.size, stAVPacket.data, stAVPacket.size, aFrames[nIndx].bKeyframe))
						throw new Exception("error while filter a frame");

					Marshal.StructureToPtr(stAVPacket, pAVPacket, true);

					lock (_cFormatCtx)
						_cFormatCtx.PacketWrite(pAVPacket);
					//Functions.av_free_packet(pAVPacket);
					//Functions.av_freep(ref pAVPacket);
				}
			}
		}
		protected delegate void FrameUnlockDelegate(IntPtr opaque, IntPtr data);
		protected FrameUnlockDelegate _fFrameUnlock;

		#region members
		static private IntPtr NULL = IntPtr.Zero;
		private object _oSyncRoot;
		private string _sFile;
		private Format.Video _cFormatVideo;
		private Format.Audio _cFormatAudio;
		private AVFormatContext _cFormatCtx;
		protected List<Frame> _aFramesLocked;
        private object _oDisposeLock;
        private bool _bDisposed;
        public Dictionary<string, string> ahMetadata
        {
            get
            {
                return _cFormatCtx.ahMetadata;
            }
        }

        public Format.Video cFormatVideo
		{
			get
			{
				return _cFormatVideo;
			}
		}
		public Format.Audio cFormatAudio
		{
			get
			{
				return _cFormatAudio;
			}
		}

		static public string sLoggerPath
		{
			get
			{
				return helper._sLoggerPath;
			}
			set
			{
				helper._sLoggerPath = value;
			}
		}

		private File()
		{
            _oDisposeLock = new object();
            helper.Initialize();
			_oSyncRoot = new object();
			_aFramesLocked = new List<Frame>();
			_fFrameUnlock = FrameUnlock;
        }
		~File()
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
		virtual public void Dispose()
		{
            lock (_oDisposeLock)
            {
                if (_bDisposed)
                    return;
                _bDisposed = true;
            }

            if (null != _cFormatAudio)
			{
				_cFormatAudio.Dispose();
				_cFormatAudio = null;
			}
			if (null != _cFormatVideo)
			{
				_cFormatVideo.Dispose();
				_cFormatVideo = null;
			}
			if (null != _cFormatCtx)
			{
				_cFormatCtx.Dispose();
				_cFormatCtx = null;
			}
			lock (_aFramesLocked)
			{
				if (0 < _aFramesLocked.Count)
					_aFramesLocked = null;
			}
		}
		public void Close()
		{
			Dispose();
		}
		protected void FrameUnlock(IntPtr opaque, IntPtr data)
		{
			Frame cFrame;
			lock (_aFramesLocked)
			{
				cFrame = _aFramesLocked.FirstOrDefault(o => opaque == (IntPtr)o);
				if (null == cFrame)
					return;
				_aFramesLocked.Remove(cFrame);
			}
			cFrame.Dispose();
		}
		#endregion
	}
}
