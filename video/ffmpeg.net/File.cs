//#define DEBUG_LISTAR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SIO=System.IO;

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

			static public int nCacheSize;
			public bool bPrepared;
			public bool bFileEnd
			{
				get
				{
					bool bRetVal = _bFileEnd;
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
					if (null == _aqVideoFrames)
						lock (_aqAudioFrames)   //"голодание", если очередь заполнена меньше, чем на 20%
							return (_bFileEnd ? false : (nCacheSize * 0.2) > _aqAudioFrames.Count);

					lock (_aqVideoFrames)   //"голодание", если очередь заполнена меньше, чем на 20%
						return (_bFileEnd ? false : (nCacheSize * 0.2) > _aqVideoFrames.Count);
				}
			}
			public int nCueueLength
			{
				get
				{
					if (null == _aqAudioFrames && null == _aqVideoFrames)
						return -3;
					if (null != _aqVideoFrames)
						lock (_aqVideoFrames)
							return _aqVideoFrames.Count;
					lock (_aqAudioFrames)
						return _aqAudioFrames.Count;
				}
			}

			public ulong nFramesQty { get; private set; }
			public ushort nFramesPerSecond { get; private set; }
			public TimeSpan tsTimeout;



			#endregion

			private Input()
                : base()
			{
				try
				{
					_nPacketIndx = 0; //logging
					_nTotalVideoPackets = 0;
					_nTotalAudioPackets = 0;
					_bClose = false;
					_cCloseLock = new object();
					_bFileEnd = false;
                    _nFPS = nFramesPerSecond = 50; //FPS

					tsTimeout = TimeSpan.FromSeconds(10);
					bPrepared = false;

					_nVideoStreamIndx = -1;
					_nAudioStreamIndx = -1;
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
				if (_bClose)
				{
					(new Logger()).WriteDebug2("in: already disposed");
					return;
				}
				_bClose = true;
				(new Logger()).WriteDebug2("in [hc = " + GetHashCode() + "][v_frames: " + (_aqVideoFrames == null ? "null" : "" + _aqVideoFrames.Count) + " + " + (_aqVideoFramesFree == null ? "null" : "" + _aqVideoFramesFree.Count) + " ][a_frames: " + (_aqAudioFrames == null ? "null" : "" + _aqAudioFrames.Count) + " + " + (_aqAudioFramesFree == null ? "null" : "" + _aqAudioFramesFree.Count) + " ]");
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file:input:dispose:");
				try
				{
                    if (null != _cThreadWritingFramesWorker)
                        _cThreadWritingFramesWorker.Abort();
                    lock (_cCloseLock)
                    {
                        Frame cFrame;
                        if (null != _aqVideoFrames)
                        {
                            while (0 < _aqVideoFrames.Count)
                            {
                                cFrame = _aqVideoFrames.Dequeue();
                                cFrame.Disposing -= cFrameVideo_Disposing;
                                cFrame.Dispose();
                            }
                            while (0 < _aqVideoFramesFree.Count)
                            {
                                cFrame = _aqVideoFramesFree.Dequeue();
                                cFrame.Disposing -= cFrameVideo_Disposing;
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
                                cFrame.Disposing -= cFrameAudio_Disposing;
                                cFrame.Dispose();
                            }
                            while (0 < _aqAudioFramesFree.Count)
                            {
                                cFrame = _aqAudioFramesFree.Dequeue();
                                cFrame.Disposing -= cFrameAudio_Disposing;
                                cFrame.Dispose();
                            }
                            _aqAudioFrames = null;
                            _aqAudioFramesFree = null;
                        }
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
                    if (null != _cFrameVideo)
                        _cFrameVideo.Dispose();
                    if (null != _cFrameAudio)
                        _cFrameAudio.Dispose();
                    base.Dispose();
				}
				catch (Exception ex) 
				{
					(new Logger()).WriteError(ex);
				}
				cTimings.Stop("disposing > 20", 20);
				(new Logger()).WriteDebug3("out [hc: " + GetHashCode() + "]");
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

                            _cFormatVideo = new Format.Video((ushort)stCodecCtx.width, (ushort)stCodecCtx.height, stCodecCtx.codec_id, stCodecCtx.pix_fmt, stStream.codec);
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
                            _aqVideoFramesFree = new Queue<Frame>();
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
						//(new Logger()).WriteWarning("Video and audio frames quantity doesn't match!! [video=" + nVideoFramesQty + "] [audio=" + nAudioFramesQty + "]");
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
			public void Prepare(Format.Video cFormatVideo, Format.Audio cFormatAudio)
			{
				_bDoWritingFrames = false;
				_aqWritingFrames = new Queue<byte[]>();
				_cThreadWritingFramesWorker = new System.Threading.Thread(WritingFramesWorker);
				_cThreadWritingFramesWorker.IsBackground = true;
				_cThreadWritingFramesWorker.Priority = System.Threading.ThreadPriority.Normal;
				_cThreadWritingFramesWorker.Start();

				_cFormatVideoTarget = cFormatVideo;
				_cFormatAudioTarget = cFormatAudio;
				if (5 < nCacheSize)
					_nDecodedFramesInPrepare = 5;
				else
					_nDecodedFramesInPrepare = nCacheSize;
				_nPreparedFramesIndx = _nDecodedFramesInPrepare;
				int nIndx = 0;

				lock (_cCloseLock)
					while (_nDecodedFramesInPrepare > nIndx++ && !_bFileEnd)
					{
						AddFrameToQueue();
						System.Threading.Thread.Sleep(0);
					}
				_cThreadDecodeAndCache = new Thread(DecodeAndCache);
				_cThreadDecodeAndCache.IsBackground = true;
				_cThreadDecodeAndCache.Priority = Thread.CurrentThread.Priority;
				_cThreadDecodeAndCache.Start();
			}
			private void DecodeAndCache()
			{
				try
				{
					//int nLessThan6 = 0, nMoreThan45 = 0;
					int nQueueAverageLength = 0, nQueueLength = 0, nQueueAverageIndx= 0;
					lock (_cCloseLock)
					{
						while (!_bFileEnd && !_bClose)   // 
						{
							if (!AddFrameToQueue() && !bFileEndless)
								break; // конец видео или аудио, если видео нет
							if (nCacheSize == _nPreparedFramesIndx)
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
				if (bVideo && !bAudio) // неполный последний кадр уравновешиваем пустым аудио-кадром, чтобы не плодить рассинхрон дальше....
				{
					(new Logger()).WriteWarning("queue: bad last audio frame. silenced frame added."); //logging
					Frame cFrame = new Frame(_cFormatAudioTarget);
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
				(new Logger()).WriteDebug4("in");
				Frame cRetVal = null;
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
				if (!_bFileEnd)
				{
					DateTime dtTimedOut = DateTime.MaxValue;
					while (!_bFileEnd && 1 > _aqVideoFrames.Count)
					{
                        if (bFileEndless)
                            return null;
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
						System.Threading.Thread.Sleep(5);
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
				cTimings.Stop("frame:next:video: >20ms [queue=" + _aqVideoFrames.Count + "]", 40);
				(new Logger()).WriteDebug4("return");
				return cRetVal;
			}
			public Frame FrameNextAudioGet()
			{
				(new Logger()).WriteDebug4("in");
				if (null == _aqAudioFrames)
					return null;
				Frame cRetVal = null;
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
				if (!_bFileEnd)
				{
					DateTime dtTimedOut = DateTime.MaxValue;
					while (!_bFileEnd && 1 > _aqAudioFrames.Count)
					{
                        if (bFileEndless)
                            return null;
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
						System.Threading.Thread.Sleep(5);
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
				(new Logger()).WriteDebug4("return");
				return cRetVal;
			}

			private void FrameDecodeVideo()
			{
				(new Logger()).WriteDebug4("in");
				if (null == _cFormatVideoTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
                
                int nVideoFrameFinished = 0;
				IntPtr pPacketNext = NULL;

				while (true)
				{
					while (NULL == pPacketNext)
					{
						while (1 > _aqVideoPackets.Count)
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
					cTimings.Restart("packets");
					if (null == _cFrameVideo)
						_cFrameVideo = new Frame(_cFormatVideo);
					try
					{
						int nError = Functions.avcodec_decode_video2(_cFormatVideo.pAVCodecContext, _cFrameVideo, ref nVideoFrameFinished, pPacketNext);
						if (!_bFileEnd)
						{
							Functions.av_free_packet(pPacketNext);
							Functions.av_freep(ref pPacketNext);
							_aqVideoPackets.Dequeue();
						}
					}
					catch (Exception ex)
					{
						(new Logger()).WriteError(ex);
					}
						
					cTimings.Restart("decode");
					if (0 < nVideoFrameFinished)
					{
						Frame cFrame;
                        if (1 > _aqVideoFramesFree.Count) //ротация кадров
                        {
                            cFrame = new Frame(_cFormatVideoTarget);
                            cFrame.Disposing += cFrameVideo_Disposing;
							(new Logger()).WriteDebug3("video frame added. total:" + nFramesQueueVideo++);
                        }
                        else
                            lock (_aqVideoFramesFree)
                                cFrame = _aqVideoFramesFree.Dequeue();
						_cFormatVideo.Transform(_cFormatVideoTarget, _cFrameVideo, cFrame);
                        cFrame.bKeyframe = _cFrameVideo.bKeyframe;
                        cFrame.nPTS = _cFrameVideo.nPTS;
                        lock (_aqVideoFrames)
							_aqVideoFrames.Enqueue(cFrame);


						if (_bDoWritingFrames)
						{
							if (null != cFrame)
							{
								byte[] aBytes = new byte[_cFormatVideoTarget.nBufferSize];
								System.Runtime.InteropServices.Marshal.Copy(cFrame.pBytes, aBytes, 0, (int)_cFormatVideoTarget.nBufferSize);
								lock (_aqWritingFrames)
									_aqWritingFrames.Enqueue(aBytes);
							}
						}

						cTimings.Restart("transform");
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
						throw new Exception("file ended");
					}
				}
				cTimings.Stop("frame:decode:video: >40ms", 40); //FPS
				(new Logger()).WriteDebug4("return");
			}
            int nFramesQueueAudio = 0, nFramesQueueVideo = 0;
			private void FrameDecodeAudio()
			{
				(new Logger()).WriteDebug4("in");
				if (null == _cFormatAudioTarget)
					throw new NotImplementedException("null == cFormatTarget"); //UNDONE нужно доделать возвращение сырых пакетов

				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
                bool bFrameDecoded = false;
				Frame cSamplesTarget = null;
				Frame cFrame;
                if (1 > _aqAudioFramesFree.Count) //ротация кадров
                {
                    cFrame = new Frame(_cFormatAudioTarget);
                    cFrame.Disposing += cFrameAudio_Disposing;
                    (new Logger()).WriteDebug3("audio frame added. total:" + nFramesQueueAudio++);
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
                    if (_aBytesRemainder.Length > cFrame.aBuffer.Length)
                    {
                        //(new Logger()).WriteWarning("_aBytesRemainder.Length > cFrame.aBuffer.Length : " + _aBytesRemainder.Length + ":" + cFrame.aBuffer.Length);
                        Array.Copy(_aBytesRemainder, 0, cFrame.aBuffer, 0, cFrame.aBuffer.Length);
                        _aBytesRemainder = _aBytesRemainder.Skip(cFrame.aBuffer.Length).Take(_aBytesRemainder.Length - cFrame.aBuffer.Length).ToArray();
                        nBytesOffset += cFrame.aBuffer.Length;
                    }
                    else
                    {
                        Array.Copy(_aBytesRemainder, 0, cFrame.aBuffer, 0, _aBytesRemainder.Length);
                        nBytesOffset += _aBytesRemainder.Length;
                        _aBytesRemainder = null;
                    }
				}
				while (cFrame.nLength > nBytesOffset)
				{
					aPacketBytes = null;
					if (NULL == _pPacketAudioDub)
					{
                        _pPacketAudioDub = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
						helpers.WinAPI.memset(_pPacketAudioDub, 0, Marshal.SizeOf(typeof(AVPacket)));
                        Functions.av_init_packet(_pPacketAudioDub);
						_stPacketAudio = (AVPacket)Marshal.PtrToStructure(_pPacketAudioDub, typeof(AVPacket));

					}
					cTimings.Restart("allocation");
					while (true)
					{
						// NOTE: the audio packet can contain several frames 
						while (_stPacketAudio.size > 0)
						{
                            if (null == _cFrameAudio)
                                _cFrameAudio = new Frame();
                            Marshal.StructureToPtr(_stPacketAudio, _pPacketAudioDub, true);
                            nLength = Functions.avcodec_decode_audio4(_cFormatAudio.pAVCodecContext, _cFrameAudio, ref bFrameDecoded, _pPacketAudioDub);
							cTimings.CheckIn("decode");
							if (nLength < 0)
							{
								_stPacketAudio.size = 0;
								break;
							}
							_stPacketAudio.data += nLength;
							_stPacketAudio.size -= nLength;
                            if (!bFrameDecoded)
								continue;
							cTimings.Restart("frame");
                            cSamplesTarget = _cFormatAudio.Transform(_cFormatAudioTarget, new Frame(_cFormatAudio, _cFrameAudio));
                            
                            
                            //cSamplesTarget = new Frame(_cFormatAudio);
                            //aPacketBytes = cSamplesTarget.aBytes;
                            //cSamplesTarget.Dispose();
                            
                            
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
							lock (_oSyncRoot)
                                PacketNext();
						}
						if (_bFileEnd && 1 > _aqAudioPackets.Count)
							break;
						lock (_oSyncRoot)
							_pPacketAudio = _aqAudioPackets.Dequeue();
						stPacket = (AVPacket)Marshal.PtrToStructure(_pPacketAudio, typeof(AVPacket));

						_stPacketAudio.data = stPacket.data;
						_stPacketAudio.size = stPacket.size;
						cTimings.Restart("packets");
					}
					if (null == aPacketBytes)
						throw new Exception("audio packet is null");
					nBytesCapacity = aPacketBytes.Length;
                    if (cFrame.nLength < nBytesOffset + nBytesCapacity)
					{
						nBytesCapacity = cFrame.nLength - nBytesOffset;
						_aBytesRemainder = new byte[aPacketBytes.Length - nBytesCapacity];
						Array.Copy(aPacketBytes, nBytesCapacity, _aBytesRemainder, 0, _aBytesRemainder.Length);
					}
					Array.Copy(aPacketBytes, 0, cFrame.aBuffer, nBytesOffset, nBytesCapacity);
					nBytesOffset += nBytesCapacity;
					cTimings.Restart("accumulation");
				}
				cTimings.Stop("frame:decode:audio: >40ms", 40);//FPS
				lock (_aqAudioFrames)
					_aqAudioFrames.Enqueue(cFrame);
				(new Logger()).WriteDebug4("return");
			}
            private bool cFrameVideo_Disposing(Frame cFrame)
			{
                lock (_aqVideoFramesFree)
                {
                    if (20 < _aqVideoFramesFree.Count)
                    {
						(new Logger()).WriteDebug3("video frame removed. total:" + nFramesQueueVideo--);
                        return true;
                    }
                    else
                        _aqVideoFramesFree.Enqueue(cFrame);
                }
				return false;
			}
            private bool cFrameAudio_Disposing(Frame cFrame)
			{
                lock (_aqAudioFramesFree)
                {
                    if (20 < _aqAudioFramesFree.Count)
                    {
						(new Logger()).WriteDebug3("audio frame removed. total:" + nFramesQueueAudio--);
                        return true;
                    }
                    else
                        _aqAudioFramesFree.Enqueue(cFrame);
                }
				return false;
			}
            private void PacketNext()
			{
				Logger.Timings cTimings = new Logger.Timings("ffmpeg:file");
				_nPacketIndx++; // logging
				IntPtr pPacket;
				AVPacket stPacket;
				if (!_bFileEnd)
				{
                    do
                    {
                        pPacket = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
                        helpers.WinAPI.memset(pPacket, 0, Marshal.SizeOf(typeof(AVPacket)));
                        Functions.av_init_packet(pPacket);
                        //stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                        //stPacket.data = NULL;
                        //stPacket.size = 0;
                        //Marshal.StructureToPtr(stPacket, pPacket, true);
                        cTimings.Restart("allocation");
                        int nResult;
                        if (-1 < (nResult = _cFormatCtx.PacketRead(pPacket)))
                        {
							cTimings.Restart("reeding packet");
                            stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                            if (_nVideoStreamIndx == stPacket.stream_index)
                            {
                                _aqVideoPackets.Enqueue(pPacket);
                                _nTotalVideoPackets++;
                            }
                            else if (_nAudioStreamIndx == stPacket.stream_index)
                            {
                                _aqAudioPackets.Enqueue(pPacket);
                                _nTotalAudioPackets++;
                            }
                            else
                            {
                                Functions.av_free_packet(pPacket);
                                Functions.av_freep(ref pPacket);
                            }
                            break;
                        }
                        else if (!bFileEndless)
                        {
                            if (-541478725 != nResult)
                                (new Logger()).WriteError("File.Input.GetAndSortNextPacket.PacketRead = " + nResult);
                            _bFileEnd = true;
							(new Logger()).WriteDebug("_bFileEnd = true");
                        }
                        else
                            Thread.Sleep(20);
                    } while (bFileEndless);
				}
				cTimings.Stop("packets: > 40ms", 40);
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

                try
                {
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

            //private double _nPTSVideo;
			//private double _nPTSAudio;
			#endregion

            private Output(Format.Video cFormatVideo, Format.Audio cFormatAudio)
                : base()
            {
                //_cFormatVideo = new Format.Video(cFormatVideo);
                //_cFormatAudio = new Format.Audio(cFormatAudio);
                _cFormatVideo = cFormatVideo;
                _cFormatAudio = cFormatAudio;
                _pFormatOutput = NULL;
                _pStreamVideo = NULL;
                _pStreamAudio = NULL;
                _pBitStreamFilterVideo = NULL;
                _sFile = null;
            }
			public Output(string sFile)
                : this(sFile, new Format.Video(0, 0, PixelFormat.AV_PIX_FMT_NONE), new Format.Audio(48000, 2, AVSampleFormat.AV_SAMPLE_FMT_S16))
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
                        if (AVCodecID.CODEC_ID_H264 == cFormatVideo.eCodecID && (new string[] { "f4v", "flv", "f4f", "mpegts" }).Contains(sType))
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
				catch(Exception ex)
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
                catch { }
			}
			override public void Dispose()
			{
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
                        aFrames = new Frame[] { new Frame(null, cFrameSource.aBytes.ToArray()) { nPTS = cFrameSource.nPTS, bKeyframe = cFrameSource.bKeyframe } };
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
                    lock(_aFramesLocked)
                        _aFramesLocked.Add(aFrames[nIndx]);
                    stAVPacket.buf = Functions.av_buffer_create(stAVPacket.data, stAVPacket.size, Marshal.GetFunctionPointerForDelegate(_fFrameUnlock), aFrames[nIndx], 0);
                    //System.IO.File.AppendAllText("packets", stAVPacket.pts + "\t" + stAVPacket.size + Environment.NewLine);
                    if (NULL != pBitStreamFilter && 0 != Functions.av_bitstream_filter_filter(pBitStreamFilter, cFormatTarget.pAVCodecContext, NULL, ref stAVPacket.data, ref stAVPacket.size, stAVPacket.data, stAVPacket.size, aFrames[nIndx].bKeyframe))
                            throw new Exception("error while filter a frame");
                    
                    Marshal.StructureToPtr(stAVPacket, pAVPacket, true);

                    lock(_cFormatCtx)
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
			catch { }
		}
		virtual public void Dispose()
		{
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
                    _aFramesLocked = _aFramesLocked;
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
