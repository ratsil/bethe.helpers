//#define DEBUG_LISTAR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using helpers;

namespace ffmpeg.net
{
	public class Logger : helpers.Logger
	{
		public class Timings
		{
			private Stopwatch _cStopwatch = null;
			private TimeSpan _tsTotal;
			private string _sMessage;

			public Timings()
			{
				if (Logger.Level.notice > Logger.eLevel)
				{
					_sMessage = "";
					_tsTotal = TimeSpan.Zero;
					_cStopwatch = Stopwatch.StartNew();
				}
			}
			public void CheckIn(string sPrefix)
			{
				if (null == _cStopwatch)
					return;
				_sMessage += "[" + sPrefix + ":" + _cStopwatch.Elapsed.TotalMilliseconds + "ms]";
			}
			public void Stop(string sMessage)
			{
				Stop(sMessage, 0);
			}
			public void Stop(string sMessage, ulong nThreshold)
			{
				Stop(sMessage, "total", nThreshold);
			}
			public void Stop(string sMessage, string sPrefix, ulong nThreshold)
			{
				if (null == _cStopwatch)
					return;
				_cStopwatch.Stop();
				_tsTotal.Add(_cStopwatch.Elapsed);
				if (1 > nThreshold || _tsTotal.TotalMilliseconds > nThreshold)
				{
					CheckIn(sPrefix);
					(new Logger()).WriteDebug(sMessage + _sMessage);
				}
			}
			public void Restart(string sPrefix)
			{
				if (null == _cStopwatch)
					return;
				CheckIn(sPrefix);
				_tsTotal.Add(_cStopwatch.Elapsed);
				_cStopwatch.Restart();
			}
		}
		static public Level eLevel = Level.debug1;

		public Logger()
			: base(eLevel, "ffmpeg")
		{ }
	}
	internal class helper
	{
		static private IntPtr NULL = IntPtr.Zero;

		static internal bool _bInitialized = false;
		static internal string ErrorDescriptionGet(int nError)
		{
			StringBuilder sError = new StringBuilder();
			nError = ffmpeg.net.Functions.av_strerror(nError, sError, 1024);
			return sError.ToString();
		}
		static internal object _cSyncRootGlobal = new object();
	}

	abstract public class File
	{
		public class Input : File
		{
			#region members

			private object _cCloseLock;
			private byte[] _aBytesRemainder;
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
			private ulong _nTotalVideoPackets;
			private ulong _nTotalAudioPackets;
			private Format.Video _cFormatVideoTarget;
			private Format.Audio _cFormatAudioTarget;
			private bool _bClose;
			private Thread _cThreadDecodeAndCache;
			private bool _bFileEnd;
			private ushort _nFPS; //FPS
			private Queue<Frame> _aqVideoFrames;
			private Queue<Frame> _aqVideoFramesFree;
			private Queue<Frame> _aqAudioFrames;
			private Queue<Frame> _aqAudioFramesFree;

			private System.Threading.Thread _cThreadWritingFramesWorker;
			private bool _bDoWritingFrames;
			private Queue<byte[]> _aqWritingFrames;

			public int nCacheSize;
			public bool bPrepared;
			public bool bFileEnd
			{
				get
				{
					bool bRetVal = _bFileEnd;
					if(bRetVal && null != _aqVideoFrames)
						lock(_aqVideoFrames)
							bRetVal = (1 > _aqVideoFrames.Count);
					if(bRetVal && null != _aqAudioFrames)
						lock(_aqAudioFrames)
							bRetVal = (1 > _aqAudioFrames.Count);
					return bRetVal;
				}
			}
			public bool bFramesStarvation
			{
				get
				{
					lock(_cSyncRoot) //"голодание", если очередь заполнена меньше, чем на 90%
						return ((nCacheSize * 0.9) > (null == _aqVideoFrames ? _aqAudioFrames.Count : _aqVideoFrames.Count));
				}
			}

			public ulong nFramesQty { get; private set; }
			public ushort nFramesPerSecond { get; private set; }
			public TimeSpan tsTimeout;



			#endregion
			//IntPtr pLogCallback;
			public Input(string sFile)
				: this(sFile, 0)
			{
			}
			public Input(string sFile, ulong nFrameStart)
			{
				try
				{
					lock (helper._cSyncRootGlobal)
					{
						if (!helper._bInitialized)
						{
							Functions.av_register_all();
							helper._bInitialized = true;
						}
					}

					//Functions.av_log_set_level(Constants.AV_LOG_DEBUG * 10);
					//System.IO.File.WriteAllText("c:/ffmpeg.log", "");
					//System.IO.File.WriteAllText("c:/ffmpeg1.log", "");

					//Functions.av_log_set_callback(new Functions.av_log_callback(av_log));
					//Functions.av_log_set_callback(Functions.av_log_callback);
					//pLogCallback = Functions.av_log_get_callback();
					//Functions.av_log_set_callback(pLogCallback);


					_cSyncRoot = new object();
					_nPacketIndx = 0; //logging
					_nTotalVideoPackets = 0;
					_nTotalAudioPackets = 0;
					_bClose = false;
					_cCloseLock = new object();
					_bFileEnd = false;
					_nFPS = 25; //FPS
					_sFile = sFile;


					nCacheSize = 100;
					tsTimeout = TimeSpan.FromSeconds(10);
					bPrepared = false;

					AVStream stStream;
					AVCodecContext stCodecCtx;
					_cFormatCtx = AVFormatContext.OpenInput(_sFile);// Functions.avformat_open_input(_sFile);
					_cFormatCtx.StreamInfoFind();
					_nVideoStreamIndx = -1;
					_nAudioStreamIndx = -1;


					nFramesPerSecond = _nFPS;
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
							_nVideoStreamIndx = nIndx;
							long nFrameTarget;
							nFrameTarget = Functions.av_rescale((long)(nFrameStart * 40), stStream.time_base.den, stStream.time_base.num) / 1000;
							if (0 < nFrameStart)
								_cFormatCtx.Seek(_nVideoStreamIndx, nFrameTarget);

							_cFormatVideo = new Format.Video((ushort)stCodecCtx.width, (ushort)stCodecCtx.height, stCodecCtx.codec_id, stCodecCtx.pix_fmt, stStream.codec);
							nFramesPerSecond = (ushort)stStream.r_frame_rate.num;
							//nFramesPerSecond = (ushort)stStream.time_base.den;
							nVideoDuration = stStream.duration / (ushort)(stStream.time_base.den / stStream.time_base.num);  // (float)nFramesPerSecond;

							_aqVideoPackets = new Queue<IntPtr>();
							_aqVideoFrames = new Queue<Frame>();
							_aqVideoFramesFree = new Queue<Frame>();
							#endregion
						}
						else if (AVMediaType.AVMEDIA_TYPE_AUDIO == eAVMediaType && 0 > _nAudioStreamIndx)
						{
							#region AUDIO
							_nAudioStreamIndx = nIndx;
							nAudioDuration = stStream.duration / (float)stStream.time_base.den;
							_cFormatAudio = new Format.Audio(stStream.time_base.den, stCodecCtx.channels, stCodecCtx.codec_id, (AVSampleFormat)stCodecCtx.sample_fmt, stStream.codec);

							_pPacketAudio = NULL;
							_aqAudioPackets = new Queue<IntPtr>();
							_aqAudioFrames = new Queue<Frame>();
							_aqAudioFramesFree = new Queue<Frame>();
							#endregion
						}
					}
					if (0 > _nVideoStreamIndx && 0 > _nAudioStreamIndx)
						throw new Exception("can't find suitable streams");
					if (nVideoDuration < float.MaxValue || nAudioDuration < float.MaxValue)
					{
						ulong nVideoFramesQty = nVideoDuration < float.MaxValue ? (ulong)(nVideoDuration * nFramesPerSecond) : ulong.MaxValue;
						ulong nAudioFramesQty = nAudioDuration < float.MaxValue ? (ulong)(nAudioDuration * nFramesPerSecond) : ulong.MaxValue;
						if (1 == nVideoFramesQty - nAudioFramesQty || 2 == nVideoFramesQty - nAudioFramesQty)
							nFramesQty = nVideoFramesQty - nFrameStart;
						else
							nFramesQty = (nVideoFramesQty < nAudioFramesQty ? nVideoFramesQty : nAudioFramesQty) - nFrameStart;
					}
				}
				catch
				{
					Dispose();
					throw;
				}
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
				try
				{
					Close();
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
					base.Dispose();
				}
				catch (Exception ex) 
				{
					(new Logger()).WriteError(ex);
				}
			}
#if DEBUG_LISTAR
            int _frame = 0;
			System.Drawing.Bitmap cFrame = null;
			System.Drawing.Imaging.BitmapData  cFrameBD = null;
#endif
			public void Close()
			{
				if (_bClose)
					return;
				_bClose = true;
				lock (_cCloseLock)
				{
					Frame cFrame;
					if (null != _aqVideoFrames)
					{
						while (0 < _aqVideoFrames.Count)
						{
							cFrame = _aqVideoFrames.Dequeue();
							cFrame.Disposing -= new Frame.DisposingDelegate(cFrameVideo_Disposing);
							cFrame.Dispose();
						}
						while (0 < _aqVideoFramesFree.Count)
						{
							cFrame = _aqVideoFramesFree.Dequeue();
							cFrame.Disposing -= new Frame.DisposingDelegate(cFrameVideo_Disposing);
							cFrame.Dispose();
						}
						_aqVideoFrames = null;
						_aqVideoFramesFree = null;
					}
					if (null != _aqAudioFrames)
					{
						while (0 < _aqAudioFrames.Count)
						{
							cFrame = _aqAudioFrames.Dequeue();
							cFrame.Disposing -= new Frame.DisposingDelegate(cFrameAudio_Disposing);
							cFrame.Dispose();
						}
						while (0 < _aqAudioFramesFree.Count)
						{
							cFrame = _aqAudioFramesFree.Dequeue();
							cFrame.Disposing -= new Frame.DisposingDelegate(cFrameAudio_Disposing);
							cFrame.Dispose();
						}
						_aqAudioFrames = null;
						_aqAudioFramesFree = null;
					}
				}
			}
			public void Prepare(Format.Video cFormatVideo, Format.Audio cFormatAudio)
			{
				_cFormatVideoTarget = cFormatVideo;
				_cFormatAudioTarget = cFormatAudio;
				if (5 < nCacheSize)
					_nDecodedFramesInPrepare = 5;
				else
					_nDecodedFramesInPrepare = nCacheSize;
				_nPreparedFramesIndx = _nDecodedFramesInPrepare + 1;
				int nIndx = 0;

				lock (_cCloseLock)
					while (_nDecodedFramesInPrepare > nIndx++ && !_bFileEnd)
						AddFrameToQueue();
				_cThreadDecodeAndCache = new Thread(DecodeAndCache);
				_cThreadDecodeAndCache.IsBackground = true;
				_cThreadDecodeAndCache.Priority = Thread.CurrentThread.Priority;
				_cThreadDecodeAndCache.Start();

				_bDoWritingFrames = false;
				_aqWritingFrames = new Queue<byte[]>();
				_cThreadWritingFramesWorker = new System.Threading.Thread(WritingFramesWorker);
				_cThreadWritingFramesWorker.IsBackground = true;
				_cThreadWritingFramesWorker.Priority = System.Threading.ThreadPriority.Normal;
				_cThreadWritingFramesWorker.Start();
			}
			private void DecodeAndCache()
			{
				try
				{
					//int nLessThan6 = 0, nMoreThan45 = 0;
					int nQueueAverageLength = 0, nQueueLength = 0;
					lock (_cCloseLock)
					{
						while (!_bFileEnd && !_bClose)   // 
						{
							if (!AddFrameToQueue())
								break; // конец видео или аудио, если видео нет
							if (nCacheSize == _nPreparedFramesIndx++)
							{
								bPrepared = true;
								(new Logger()).WriteDebug("cache: prepared " + nCacheSize + " frames [hc:" + this.GetHashCode()); //logging
							}

							nQueueLength = (null == _aqVideoFrames ? _aqAudioFrames.Count : _aqVideoFrames.Count);

							//if (6 > nQueueLength)   //logging
							//    nLessThan6++;
							//else if (45 < nQueueLength)  //logging
							//    nMoreThan45++;
							nQueueAverageLength += nQueueLength;
						}
						(new Logger()).WriteDebug("cache: stopped [hc:" + this.GetHashCode() + "][frames:" + _nPreparedFramesIndx + "][average:" + nQueueAverageLength / _nPreparedFramesIndx + "]"); //logging
					}
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
			}
			private bool AddFrameToQueue()
			{
				bool bVideo = true, bAudio = true;
				if (null != _aqVideoFrames)
				{
					while (!_bFileEnd)
					{
						try
						{
							FrameDecodeVideo();
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
				if (bVideo && null != _aqAudioFrames)
				{
					while (!_bFileEnd)
					{
						try
						{
							FrameDecodeAudio();
							break;
						}
						catch (Exception ex)
						{
							bAudio = false;
							if (!_bFileEnd)
								(new Logger()).WriteWarning(ex); //logging
						}
					}
				}
				if (bVideo && !bAudio) // неполный последний кадр уравновешиваем пустым аудио-кадром, чтобы не плодить рассинхрон дальше....
				{
					(new Logger()).WriteWarning("queue: bad last audio frame. silenced frame added."); //logging
					Frame cFrame = new Frame(_cFormatAudioTarget, _cFormatAudioTarget.nBufferSize / _nFPS);
					lock (_aqAudioFrames)
						_aqAudioFrames.Enqueue(cFrame);
				}

				if (_bFileEnd || !bVideo)
					return false;
				while (nCacheSize < (null == _aqVideoFrames ? _aqAudioFrames.Count : _aqVideoFrames.Count))
				{
					if (_bClose)
						return false;
					System.Threading.Thread.Sleep(40);//FPS
				}
				return true;
			}
			public Frame FrameNextVideoGet()
			{
				//(new Logger()).WriteDebug("frame:next:video: begin");
				Frame cRetVal = null;
				Logger.Timings cTimings = new Logger.Timings();
				if (!_bFileEnd)
				{
					DateTime dtTimedOut = DateTime.MaxValue;
					while (!_bFileEnd && 1 > _aqVideoFrames.Count)
					{
						if (DateTime.MaxValue == dtTimedOut)
						{
							try
							{
								dtTimedOut = DateTime.Now.Add(tsTimeout);
							}
							catch
							{
								dtTimedOut = DateTime.MaxValue.AddTicks(-1);
							}
							(new Logger()).WriteDebug("frame:next:video: queue is empty");
						}
						System.Threading.Thread.Sleep(10);
						if (DateTime.Now > dtTimedOut)
							throw new TimeoutException("video queue is empty");
					}
				}
				cTimings.Restart("frame waiting");
				if (0 < _aqVideoFrames.Count)
				{
					lock (_aqVideoFrames)
						cRetVal = _aqVideoFrames.Dequeue();
					cTimings.CheckIn("dequeue"); // logging
				}
				cTimings.Stop("frame:next:video: >20ms", 20);
				//(new Logger()).WriteDebug("frame:next:video: end");
				return cRetVal;
			}
			public Frame FrameNextAudioGet()
			{
				if (null == _aqAudioFrames)
					return null;
				Frame cRetVal = null;
				Logger.Timings cTimings = new Logger.Timings();
				if (!_bFileEnd)
				{
					DateTime dtTimedOut = DateTime.MaxValue;
					while (!_bFileEnd && 1 > _aqAudioFrames.Count)
					{
						if (DateTime.MaxValue == dtTimedOut)
						{
							try
							{
								dtTimedOut = DateTime.Now.Add(tsTimeout);
							}
							catch
							{
								dtTimedOut = DateTime.MaxValue.AddTicks(-1);
							}
							(new Logger()).WriteDebug("frame:next:audio: queue is empty"); //logging
						}
						System.Threading.Thread.Sleep(10);
						if (DateTime.Now > dtTimedOut)
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
				return cRetVal;
			}

			private void FrameDecodeVideo()
			{
				if (null == _cFormatVideoTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов
				Logger.Timings cTimings = new Logger.Timings();

				int nVideoFrameFinished = 0;
				IntPtr pPacketNext = NULL;

				while (true)
				{
					while (NULL == pPacketNext)
					{
						while (1 > _aqVideoPackets.Count)
						{
							if (_bFileEnd)
								throw new Exception("file ended");
							lock (_cSyncRoot)
								GetAndSortNextPacket();
						}
						pPacketNext = _aqVideoPackets.Peek();
					}
					cTimings.Restart("packets");
					if (null == _cFrameVideo)
						_cFrameVideo = new Frame(_cFormatVideo, _cFormatVideo.nBufferSize);
					try
					{
						int nError = Functions.avcodec_decode_video2(_cFormatVideo.pAVCodecContext, _cFrameVideo.AVFrameGet(), ref nVideoFrameFinished, pPacketNext);
						Functions.av_free_packet(pPacketNext);
						Functions.av_freep(ref pPacketNext);
					}
					catch (Exception ex)
					{
						(new Logger()).WriteError(ex);
					}
					_aqVideoPackets.Dequeue();
					cTimings.Restart("decode");
					if (0 < nVideoFrameFinished)
					{
						Frame cFrame;
						if (1 > _aqVideoFramesFree.Count) //ротация кадров
						{
							cFrame = new Frame(_cFormatVideoTarget, _cFormatVideoTarget.nBufferSize);
							cFrame.Disposing += new Frame.DisposingDelegate(cFrameVideo_Disposing);
						}
						else
							lock (_aqVideoFramesFree)
								cFrame = _aqVideoFramesFree.Dequeue();
						//Functions.avpicture_fill(_pAVFrameTarget, cFrame.aBytes, _cFormatVideoTarget.ePixelFormat, _cFormatVideoTarget.nWidth, _cFormatVideoTarget.nHeight);
						_cFormatVideo.Transform(_cFormatVideoTarget, _cFrameVideo, cFrame);   // lock inside!
						lock (_aqVideoFrames)
							_aqVideoFrames.Enqueue(cFrame);


						if (_bDoWritingFrames)
						{
							if (null != cFrame)
							{
								byte[] aBytes = new byte[_cFormatVideoTarget.nBufferSize];
								System.Runtime.InteropServices.Marshal.Copy(cFrame.p, aBytes, 0, (int)_cFormatVideoTarget.nBufferSize);
								lock (_aqWritingFrames)
									_aqWritingFrames.Enqueue(aBytes);
							}
						}

						cTimings.Restart("transform");
						break;
					}
				}
				cTimings.Stop("frame:decode:video: >40ms", 40);
			}
			private void FrameDecodeAudio()
			{
				if (null == _cFormatAudioTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов
				Logger.Timings cTimings = new Logger.Timings();
				int nBufferSizeSource;
				if(null == _cFrameAudio)
					_cFrameAudio = new Frame(_cFormatAudio, _cFormatAudio.nBufferSize);
				Frame cSamplesTarget = null;
				Frame cFrame;
				if (1 > _aqAudioFramesFree.Count) //ротация кадров
				{
					cFrame = new Frame(_cFormatAudioTarget, _cFormatAudioTarget.nBufferSize / _nFPS);
					cFrame.Disposing += new Frame.DisposingDelegate(cFrameAudio_Disposing);
				}
				else
					lock (_aqAudioFramesFree)
						cFrame = _aqAudioFramesFree.Dequeue();

				int nBytesCapacity = 0;
				int nBytesOffset = 0;
				byte[] aPacketBytes;
				int nLength = 0;
				AVPacket stPacket;
				if (null != _aBytesRemainder)
				{
					Array.Copy(_aBytesRemainder, 0, cFrame.aBuffer, 0, _aBytesRemainder.Length);
					nBytesOffset += _aBytesRemainder.Length;
					_aBytesRemainder = null;
				}
				while (cFrame.nLength > nBytesOffset)
				{
					aPacketBytes = null;
					if (NULL == _pPacketAudioDub)
					{
						_pPacketAudioDub = Functions.avcodec_alloc_frame();
						_stPacketAudio = (AVPacket)Marshal.PtrToStructure(_pPacketAudioDub, typeof(AVPacket));
					}
					cTimings.Restart("allocation");
					while (true)
					{
						// NOTE: the audio packet can contain several frames 
						while (_stPacketAudio.size > 0)
						{
							nBufferSizeSource = _cFrameAudio.nLengthBuffer;
							Marshal.StructureToPtr(_stPacketAudio, _pPacketAudioDub, true);
							nLength = Functions.avcodec_decode_audio3(_cFormatAudio.pAVCodecContext, _cFrameAudio.aBuffer, ref nBufferSizeSource, _pPacketAudioDub);
							cTimings.CheckIn("decode");
							if (nLength < 0)
							{
								_stPacketAudio.size = 0;
								break;
							}
							_stPacketAudio.data += nLength;
							_stPacketAudio.size -= nLength;
							nLength = nBufferSizeSource;
							if (nLength <= 0)
								continue;
							cTimings.Restart("frame");
							_cFrameAudio.nLength = nLength;
							cSamplesTarget = _cFormatAudio.Transform(_cFormatAudioTarget, _cFrameAudio);
							aPacketBytes = cSamplesTarget.aBytes;
							cTimings.Restart("transform");
							break;
						}
						if (null != aPacketBytes)
							break;
						if (NULL != _pPacketAudio)
						{
							Functions.av_free_packet(_pPacketAudio);
							Functions.av_freep(ref _pPacketAudio);
							cTimings.Restart("packet free");
						}
						while (!_bFileEnd && 1 > _aqAudioPackets.Count)
						{
							lock (_cSyncRoot)
								GetAndSortNextPacket();
						}
						if (_bFileEnd && 1 > _aqAudioPackets.Count)
							break;
						lock (_cSyncRoot)
							_pPacketAudio = _aqAudioPackets.Dequeue();
						stPacket = (AVPacket)Marshal.PtrToStructure(_pPacketAudio, typeof(AVPacket));

						_stPacketAudio.data = stPacket.data;
						_stPacketAudio.size = stPacket.size;
						cTimings.Restart("packets");
					}
					if (null == aPacketBytes)
						throw new Exception("audio packet is null");
					nBytesCapacity = aPacketBytes.Length;
					if (cFrame.nLength < nBytesOffset + aPacketBytes.Length)
					{
						nBytesCapacity = cFrame.nLength - nBytesOffset;
						_aBytesRemainder = new byte[aPacketBytes.Length - nBytesCapacity];
						Array.Copy(aPacketBytes, nBytesCapacity, _aBytesRemainder, 0, _aBytesRemainder.Length);
					}
					Array.Copy(aPacketBytes, 0, cFrame.aBuffer, nBytesOffset, nBytesCapacity);
					nBytesOffset += nBytesCapacity;
					cTimings.Restart("accumulation");
				}
				cTimings.Stop("frame:decode:audio: >40ms", 40);
				lock (_aqAudioFrames)
					_aqAudioFrames.Enqueue(cFrame);
			}
			bool cFrameVideo_Disposing(Frame cFrame)
			{
				lock (_aqVideoFramesFree)
					_aqVideoFramesFree.Enqueue(cFrame);
				return false;
			}
			bool cFrameAudio_Disposing(Frame cFrame)
			{
				lock (_aqAudioFramesFree)
					_aqAudioFramesFree.Enqueue(cFrame);
				return false;
			}
			private void GetAndSortNextPacket()
			{
				Logger.Timings cTimings = new Logger.Timings();
				_nPacketIndx++; // logging
				IntPtr pPacket;
				AVPacket stPacket;
				if (!_bFileEnd)
				{
					pPacket = Functions.av_malloc((uint)Marshal.SizeOf(typeof(AVPacket)));
					cTimings.Restart("allocation");

					if (_cFormatCtx.PacketRead(pPacket) >= 0 && Functions.av_dup_packet(pPacket) >= 0)
					{
						stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
						if (stPacket.stream_index == _nVideoStreamIndx)
						{
							_aqVideoPackets.Enqueue(pPacket);
							_nTotalVideoPackets++;
						}
						else if (stPacket.stream_index == _nAudioStreamIndx)
						{
							_aqAudioPackets.Enqueue(pPacket);
							_nTotalAudioPackets++;
						}
						else
						{
							Functions.av_free_packet(pPacket);
							Functions.av_freep(ref pPacket);
						}
					}
					else
						_bFileEnd = true;
				}
				cTimings.Stop("packets: >20ms", 20);
			}
			private void WritingFramesWorker(object cState)
			{
				string _sWritingFramesFile = "d:/FramesDebugWriting/WritingDebugFrames.txt";
				string _sWritingFramesDir = "d:/FramesDebugWriting/FFMPEG/";
				int _nFramesCount = 0;
				System.Drawing.Bitmap cBFrame;
				System.Drawing.Imaging.BitmapData cFrameBD;
				string[] aLines;
				bool bQueueIsNotEmpty = false;
				byte[] aBytes;

				while (!_bFileEnd && !_bClose)
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
								_bDoWritingFrames = false;
						}
						else
							_bDoWritingFrames = false;

						if (_bDoWritingFrames || 0 < _aqWritingFrames.Count)
						{
							while (bQueueIsNotEmpty)
							{
								cBFrame = new System.Drawing.Bitmap(_cFormatVideo.nWidth, _cFormatVideo.nHeight);
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
								cBFrame.Save(_sWritingFramesDir + "frame_" + _nFramesCount.ToString("0000") + ".png");
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
							System.Threading.Thread.Sleep(2000);
						}
					}
					catch (Exception ex)
					{
						(new Logger()).WriteError(ex);
					}
				}
			}
		}
		public class Output : File
		{
			#region members
			private IntPtr _pFormatOutput; //AVOutputFormat*
			private IntPtr _pStreamVideo; //AVStream*
			private IntPtr _pStreamAudio; //AVStream*
			//private double _nPTSVideo;
			//private double _nPTSAudio;
			#endregion

			public Output(string sFile, string sFourCC)
				: this(sFile, sFourCC, new Format.Video(0, 0, PixelFormat.PIX_FMT_NONE), new Format.Audio(48000, 2, AVSampleFormat.SAMPLE_FMT_S16))
			{
			}
			public Output(string sFile, string sFourCC, Format.Video cFormatVideo, Format.Audio cFormatAudio)
			{
				try
				{
					lock (helper._cSyncRootGlobal)
					{
						if (!helper._bInitialized)
						{
							Functions.av_register_all();
							helper._bInitialized = true;
						}
					}
					AVOutputFormat stAVOutputFormat;

					_pFormatOutput = NULL;
					_pStreamVideo = NULL;
					_pStreamAudio = NULL;

					_cSyncRoot = new object();

					_sFile = sFile.Substring(0, (sFile.Length > 1024 ? 1024 : sFile.Length));

					_cFormatCtx = AVFormatContext.CreateOutput(_sFile);
					_pFormatOutput = _cFormatCtx.oformat;
					AVOutputFormat stOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));

					if (null != cFormatVideo)
						VideoStreamCreate(cFormatVideo);
					if (null != cFormatAudio)
					{
						stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
						if (stAVOutputFormat.audio_codec != cFormatAudio.eCodecID)
						{
							stAVOutputFormat.audio_codec = cFormatAudio.eCodecID;
							Marshal.StructureToPtr(stAVOutputFormat, _pFormatOutput, true);
						}
						AudioStreamCreate(cFormatAudio);
					}

					stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
					if (!(0 < (stAVOutputFormat.flags & Constants.AVFMT_NOFILE)))
						_cFormatCtx.SaveOutput(_sFile);

					/* write the stream header, if any */
					_cFormatCtx.WriteHeader();
					//_nPTSVideo = 0.0;
					//_nPTSAudio = 0.0;
				}
				catch
				{
					Dispose();
				}
			}

			~Output()
			{
				Dispose();
			}
			override public void Dispose()
			{
				try
				{
					Close();
				}
				catch { }
				try
				{
					base.Dispose();
				}
				catch { }
			}
			public void Close()
			{
				if (null != _cFormatCtx)
				{
					/* write the trailer, if any.  the trailer must be written
						* before you close the CodecContexts open when you wrote the
						* header; otherwise write_trailer may try to use memory that
						* was freed on av_codec_close() */
					_cFormatCtx.WriteTrailer();
					if (null != _cFormatVideo)
						_cFormatVideo.Dispose();
					if (null != _cFormatAudio)
						_cFormatAudio.Dispose();
					if (NULL == _pFormatOutput)
						throw new Exception("invalid output format");
					//AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
					//if (!(0 < (stAVOutputFormat.flags & Constants.AVFMT_NOFILE)))
					//    Functions.avio_close(_cFormatCtx.pb);
					_cFormatCtx.Dispose();
					_cFormatCtx = null;
				}
			}
			private void VideoStreamCreate(Format.Video cFormat)
			{
				AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
				if (stAVOutputFormat.video_codec == CodecID.CODEC_ID_NONE)
					return;

				_pStreamVideo = _cFormatCtx.StreamAdd();
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamVideo, typeof(AVStream));
				AVCodecContext stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(cFormat.pAVCodecContext, typeof(AVCodecContext));
				cFormat.stAVCodecContext.codec_id = stAVOutputFormat.video_codec;
				Marshal.StructureToPtr(cFormat.stAVCodecContext, cFormat.pAVCodecContext, true);
				_cFormatVideo = new Format.Video(cFormat, stAVStream.codec);
				_cFormatVideo.stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(_cFormatVideo.pAVCodecContext, typeof(AVCodecContext));

				// some formats want stream headers to be separate
				if (0 < (stAVOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
					_cFormatVideo.stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
				Marshal.StructureToPtr(_cFormatVideo.stAVCodecContext, stAVStream.codec, true);
				Marshal.StructureToPtr(stAVStream, _pStreamVideo, true);
			}
			private void AudioStreamCreate(Format.Audio cFormat)
			{
				AVOutputFormat stAVOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(_pFormatOutput, typeof(AVOutputFormat));
				if (stAVOutputFormat.audio_codec == CodecID.CODEC_ID_NONE)
					return;
				_pStreamAudio = _cFormatCtx.StreamAdd();
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamAudio, typeof(AVStream));
				_cFormatAudio = new Format.Audio(cFormat, stAVStream.codec);
				AVCodecContext stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(_cFormatAudio.pAVCodecContext, typeof(AVCodecContext));
				if (0 < (stAVOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
					stAVCodecContext.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
				Marshal.StructureToPtr(stAVCodecContext, stAVStream.codec, true);
				Marshal.StructureToPtr(stAVStream, _pStreamAudio, true);
			}
			public void VideoFrameNext(Format.Video cFormatSource, Frame cFrameSource)
			{
				if (NULL == _pStreamVideo)
					throw new Exception("there is no video stream in file");
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamVideo, typeof(AVStream));
				//_nPTSVideo = (double)stAVStream.pts.val * stAVStream.time_base.num / stAVStream.time_base.den;
				//(1 / FPS) * sample rate * frame number
				if (null == _cFrameVideo)
					_cFrameVideo = new Frame(_cFormatVideo, _cFormatVideo.nBufferSize);
				Frame[] aFrames = cFormatSource.Convert(_cFormatVideo, cFrameSource, _cFrameVideo);
				AVPacket stAVPacket = new AVPacket();
				IntPtr pAVPacket;
				for (int nIndx = 0; aFrames.Length > nIndx; nIndx++)
				{
					cFrameSource = aFrames[nIndx];
					pAVPacket = Functions.av_malloc((uint)(Marshal.SizeOf(stAVPacket)));
					Functions.av_init_packet(pAVPacket);
					stAVPacket = (AVPacket)Marshal.PtrToStructure(pAVPacket, typeof(AVPacket));
					if (cFrameSource.nPTS != Constants.AV_NOPTS_VALUE)
						stAVPacket.pts = Functions.av_rescale_q(cFrameSource.nPTS, _cFormatVideo.stAVCodecContext.time_base, stAVStream.time_base);
					if (cFrameSource.bKeyframe)
						stAVPacket.flags |= Constants.AV_PKT_FLAG_KEY;
					stAVPacket.stream_index = stAVStream.index;
					stAVPacket.size = cFrameSource.nLength;
					stAVPacket.data = cFrameSource.p;
					Marshal.StructureToPtr(stAVPacket, pAVPacket, true);
					//System.IO.File.AppendAllText("packets", stAVPacket.pts + "\t" + stAVPacket.size + "\r\n");
					_cFormatCtx.PacketWrite(pAVPacket);
					Functions.av_free_packet(pAVPacket);
					Functions.av_freep(ref pAVPacket);
					//cFrameSource.Dispose();
				}
			}
			public void AudioFrameNext(Format.Audio cFormatSource, Frame cFrameSource)
			{
				if (NULL == _pStreamAudio)
					throw new Exception("there is no audio stream in file");
				if (null == cFrameSource)
					return;
				AVStream stAVStream = (AVStream)Marshal.PtrToStructure(_pStreamAudio, typeof(AVStream));
				//_nPTSAudio = (double)stAVStream.pts.val * stAVStream.time_base.num / stAVStream.time_base.den;
				Frame cFrame;
				AVPacket stAVPacket = new AVPacket();
				IntPtr pAVPacket;
				if (null == _cFrameAudio)
					_cFrameAudio = new Frame(_cFormatAudio, _cFormatAudio.nBufferSize);
				Frame[] aFrames = cFormatSource.Convert(_cFormatAudio, cFrameSource, _cFrameAudio);
				for (int nIndx = 0; aFrames.Length > nIndx; nIndx++)
				{
					cFrame = aFrames[nIndx];
					pAVPacket = Functions.av_malloc((uint)(Marshal.SizeOf(stAVPacket)));
					Functions.av_init_packet(pAVPacket);
					stAVPacket = (AVPacket)Marshal.PtrToStructure(pAVPacket, typeof(AVPacket));
					if (cFrame.nPTS != Constants.AV_NOPTS_VALUE)
						stAVPacket.pts = Functions.av_rescale_q(cFrame.nPTS, _cFormatAudio.stAVCodecContext.time_base, stAVStream.time_base);
					//stAVPacket.pts = stAVPacket.dts = Functions.av_rescale_q((long)_nPTSAudio, _cFormatVideo.stAVCodecContext.time_base, stAVStream.time_base);
					stAVPacket.flags |= Constants.AV_PKT_FLAG_KEY;
					stAVPacket.stream_index = stAVStream.index;
					stAVPacket.size = cFrame.nLength;
					stAVPacket.data = cFrame.p;
					Marshal.StructureToPtr(stAVPacket, pAVPacket, true);

					_cFormatCtx.PacketWrite(pAVPacket);
					Functions.av_free_packet(pAVPacket);
					Functions.av_freep(ref pAVPacket);
					cFrame.Dispose();
					//_nPTSAudio++;
				}
			}
		}

		#region members
		private IntPtr NULL = IntPtr.Zero;
		private object _cSyncRoot;
		private string _sFile;
		private Format.Video _cFormatVideo;
		private Format.Audio _cFormatAudio;
		private AVFormatContext _cFormatCtx;
		private Frame _cFrameVideo;
		private Frame _cFrameAudio;

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

		private File()
		{
		}
		~File()
		{
			try
			{
				Dispose();
			}
			catch { }
		}
		virtual public void Dispose()
		{
			if (null != _cFrameVideo)
				_cFrameVideo.Dispose();
			if (null != _cFrameAudio)
				_cFrameAudio.Dispose();
		}
		#endregion
	}
	public class Frame
	{
		static private IntPtr NULL = IntPtr.Zero;
		public delegate bool DisposingDelegate(Frame cFrame);
		public event DisposingDelegate Disposing;

		private GCHandle _cGCHandle;
		private IntPtr _pBytes;

		private IntPtr _pAVFrame;
		private long _nPTS;

		public int nLength;
		public int nLengthBuffer
		{
			get
			{
				return aBuffer.Length;
			}
		}
		public IntPtr p
		{
			get
			{
				return _pBytes;
			}
		}
		public byte[] aBuffer;
		public byte[] aBytes
		{
			get
			{
				//if(nLength != nLengthBuffer)
					return aBuffer.Take(nLength).ToArray();
				return aBuffer;
			}
		}
		public bool bKeyframe;
		public long nPTS
		{
			get
			{
				if (NULL != _pAVFrame)
				{
					AVFrame stAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
					_nPTS = stAVFrame.pts;
				}
				return _nPTS;
			}
			set
			{
				_nPTS = value;
				if (NULL != _pAVFrame)
				{
					AVFrame stAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
					stAVFrame.pts = _nPTS;
					Marshal.StructureToPtr(stAVFrame, _pAVFrame, true);
				}
			}
		}

		public Frame()
		{
			aBuffer = null;
			bKeyframe = false;
			_pBytes = NULL;
			_pAVFrame = NULL;
		}
		public Frame(Format cFormat, byte[] aBytes)
			: this()
		{
			aBuffer = aBytes;
			nLength = aBytes.Length;
			_cGCHandle = GCHandle.Alloc(aBuffer, GCHandleType.Pinned);
			_pBytes = _cGCHandle.AddrOfPinnedObject();
			if (null != cFormat && cFormat is Format.Video)
				AVFrameInit(cFormat);
		}
		public Frame(Format cFormat, int nLength)
			: this(cFormat, new byte[nLength])
		{
		}

		~Frame()
		{
			try
			{
				Dispose();
			}
			catch { }
		}
		public void Dispose()
		{
			if (null != Disposing && !Disposing(this))
				return;
			if (NULL != _pBytes)
			{
				_cGCHandle.Free();
				_pBytes = NULL;
			}	
			if (NULL != _pAVFrame)
				Functions.av_freep(ref _pAVFrame);
		}

		private void AVFrameInit(Format cFormat)
		{
			if (NULL == _pAVFrame)
			{
				_pAVFrame = Functions.avcodec_alloc_frame();
				if (NULL == _pAVFrame)
					throw new Exception("not enough memory for source frame allocate");
				if (null != aBuffer)
				{
					Format.Video cFormatVideo = (Format.Video)cFormat;
					Functions.avpicture_fill(_pAVFrame, aBuffer, cFormatVideo.ePixelFormat, cFormatVideo.nWidth, cFormatVideo.nHeight);
				}
				AVFrame stAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
				stAVFrame.quality = 1;
				stAVFrame.pts = 0;
				////stAVFrame.interlaced_frame = 1;// || 0 != stAVPicture.top_field_first)
				Marshal.StructureToPtr(stAVFrame, _pAVFrame, true);
			}
		}
		public IntPtr AVFrameGet()
		{
			return _pAVFrame;
		}
	}
}
