using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using helpers;
using helpers.extensions;
using System.Linq;

// отладка
using System.Drawing;
using System.Drawing.Imaging;           // отладка
// отладка

namespace BTL.Device
{
	public class DecklinkFake : Device
	{
		protected class AudioBuffer
		{
			private IntPtr[] _aPointers;
			private int nIndex;
			private const int nMax = 30;
			public AudioBuffer()
			{
				_aPointers = new IntPtr[nMax];
				for (int ni = 0; nMax > ni; ni++)
				{
					_aPointers[ni] = Marshal.AllocCoTaskMem((int)Preferences.nAudioBytesPerFrame);
				}
				nIndex = 0;
			}
			public void Advance()
			{
				nIndex++;
			}
			internal void Dispose()
			{
				for (int ni = 0; nMax > ni; ni++)
				{
					Marshal.FreeCoTaskMem(_aPointers[ni]);
				}
			}
			public void Write(byte[] aAudioBytes)
			{
				if (_aPointers.Length == nIndex)
					nIndex = 0;
				Marshal.Copy(aAudioBytes, 0, _aPointers[nIndex], aAudioBytes.Length);
			}
			public IntPtr GetCurrent()
			{
				return _aPointers[nIndex];
			}
		}

        public ulong nFramesVideo;
        public ulong nFramesAudio;
        public ulong nFramesDroppedVideo;
        public ulong nFramesDroppedAudio;

        private IntPtr _pFakePointer = IntPtr.Zero;
        private int _eAudioSampleDepth;
        private int _eAudioSampleRate;
        private bool _TurnedOn;
		private Dictionary<Frame.Video, IntPtr> _ahFramesBuffersBinds;
		private long _nVideoStreamTime;
		private long _nAudioStreamTime;
        private int _nAudioQueueLength;
		private uint _nLogCounter2;
        private bool? _bItsOk;
		private string _sIterationsCounter2 = ".";
		protected AudioBuffer _cAudioBuffer;
        private ushort _nTimescale;

        private bool _bStopped;
        override public bool bCardStopped 
        {
            get
            {
                return _bStopped;
            }
        }


        static public int BoardsQtyGet()
        {
            int nRetVal = 1;
            (new Logger("DeckLinkFake", "DeckLinkFake-0_")).WriteDebug3("boards.qty.get:out [" + nRetVal + "]");
            return nRetVal;
        }
        new static public Device BoardGet(uint nIndex)
        {
			(new Logger("DeckLinkFake", "DeckLinkFake-0_")).WriteDebug3("in");
            return new DecklinkFake(nIndex);
		}

		private DecklinkFake(uint nDeviceIndex)
            :base("DeckLinkFake-" + nDeviceIndex + "_")
		{
            (new Logger("DeckLinkFake", sName)).WriteDebug3("in");
			try
			{
				_ahFramesBuffersBinds = new Dictionary<Frame.Video, IntPtr>();

				_nAudioQueueLength = 0;
                if (bInput)
                    ;//_iDLInput = (IDeckLinkInput)_iDLDevice;
                else
                    ;// _iDLOutput = (IDeckLinkOutput)_iDLDevice;

				string sDisplayModeName = "";

                string sMessage = "decklink supported modes:<br>";
                if (bInput)
                {
                    sMessage += "selected:\t" + Preferences.sVideoFormat;
                    (new Logger("DeckLinkFake", sName)).WriteNotice(sMessage);
				}
				else
				{
                    sMessage += "selected:\t" + Preferences.sVideoFormat;
                    (new Logger("DeckLinkFake", sName)).WriteNotice(sMessage);

                    sMessage = "<br>\t\t" + "1920x1080p 16bit audio";
                    (new Logger("DeckLinkFake", sName)).WriteNotice("\tSupported pixel formats:" + sMessage);

					stArea = new Area(0, 0, (ushort)1920, (ushort)1080);
                    _nVideoBytesQty = stArea.nWidth * stArea.nHeight * 4;
                    long nFrameDuration, nFrameTimescale;
                    nFrameDuration = 40;
                    nFrameTimescale = 1000;
					Preferences.nFPS = (ushort)((nFrameTimescale + (nFrameDuration - 1)) / nFrameDuration); //до ближайшего целого - взято из примера деклинка

                    if (Preferences.bAudio)
					{
						switch (Preferences.nAudioBitDepth)
						{
							case 32:
								_eAudioSampleDepth = 32;
								break;
							case 16:
							default:
								_eAudioSampleDepth = 16;
								break;
						}
						switch (Preferences.nAudioSamplesRate)
						{
							case 48000:
								_eAudioSampleRate = 48000;
								break;
							default:
								throw new Exception("unsupported audio sample rate [" + Preferences.nAudioSamplesRate + "]");
						}
						//_pAudioSamplesBuffer = Marshal.AllocCoTaskMem((int)_nAudioFrameSize_InBytes);
						_cAudioBuffer = new AudioBuffer();
						_nAudioBufferCapacity_InSamples = Preferences.nAudioSamplesPerFrame * Preferences.nQueueDeviceLength;
                        _nChannelBytesQty = Preferences.nAudioSamplesPerFrame * Preferences.nAudioByteDepth;
                        _nAudioChannelsQty = Preferences.nAudioChannelsQty;
                        _nAudioBytesQty = _nAudioChannelsQty * _nChannelBytesQty;
                        //for (int nIndx = 0; _nAudioFrameSize_InBytes > nIndx; nIndx++)
                        //    Marshal.WriteByte(_pAudioSamplesBuffer, nIndx, 0);

                        _nTimescale = (ushort)_eAudioSampleRate;
					}
					if (null != Preferences.cDownStreamKeyer)
					{
						if (false)
							;
						else
							(new Logger("DeckLinkFake", sName)).WriteWarning("This device is not Keyer device. Don't use keyer in preferences");
					}
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLinkFake", sName)).WriteError(ex);
				throw;
			}
			(new Logger("DeckLinkFake", sName)).WriteDebug4("return");
		}
		~DecklinkFake()
		{
            Dispose();
		}
        static object oLockDispose = new object();
        static bool bDisposed = false;
        new public void Dispose()
        {
            lock (oLockDispose)
            {
                if (bDisposed)
                    return;
                bDisposed = true;
            }
            try
            {
                if (_TurnedOn)  // иначе, если не TurnedOn, то error при StopScheduledPlayback
                {
                    Marshal.FreeHGlobal(_pFakePointer);
                    _ahFramesBuffersBinds.Clear();
                    _aCurrentFramesIDs.Clear();
                }
            }
            catch (Exception ex)
            {
                (new Logger("DeckLinkFake", sName)).WriteError(ex);
            }
            finally
            {
                try
                {
                    if (null != _cAudioBuffer)
                        _cAudioBuffer.Dispose();
                }
                catch (Exception ex)
                {
                    (new Logger("DeckLinkFake", sName)).WriteError(ex);
                }
                finally
                {
                    _bStopped = true;
                }
            }
        }

        override public void TurnOn()
		{
			_nVideoStreamTime = 0;
			_nAudioStreamTime = 0;
            if (bInput)
            {
                //_iDLInput.SetCallback(this);
			}
			else
			{
				_aCurrentFramesIDs = new Dictionary<long, long>();
				_cStopWatch = System.Diagnostics.Stopwatch.StartNew();
				DownStreamKeyer();
				//_iDLOutput.SetAudioCallback(this);
				//_iDLOutput.SetScheduledFrameCompletionCallback(this);
			}
			base.TurnOn();
            if (bInput)
                ;//_iDLInput.StartStreams();
            else
                ;//_iDLOutput.BeginAudioPreroll();


            System.Threading.ThreadPool.QueueUserWorkItem(Worker);

            _TurnedOn = true;
            (new Logger("DeckLinkFake", sName)).WriteNotice("decklink turned on");
		}
		override public void DownStreamKeyer()
		{// external key =1;  internal key =0
			base.DownStreamKeyer();
			if (null != Preferences.cDownStreamKeyer)
			{
				(new Logger("DeckLinkFake", sName)).WriteNotice("keyer enabled [" + Preferences.cDownStreamKeyer.bInternal + "][" + Preferences.cDownStreamKeyer.nLevel + "]");
			}
			else
			{
				(new Logger("DeckLinkFake", sName)).WriteNotice("keyer disabled");
			}
		}
		override protected Frame.Video FrameBufferPrepare()
		{
			Frame.Video oRetVal = new Frame.Video(sName);
			IntPtr pBuffer;
            (new Logger("DeckLinkFake", sName)).WriteNotice("!!!!!:" + stArea.nWidth + ":" + stArea.nHeight + ":" + stArea.nWidth * 4 + ":" + "BGRA");
            if (_pFakePointer == IntPtr.Zero)
            {
                _pFakePointer = Marshal.AllocHGlobal(_nVideoBytesQty);
            }
            pBuffer = _pFakePointer; 
            if (IntPtr.Zero != pBuffer)
			{
				lock (_ahFramesBuffersBinds)
				{
					_ahFramesBuffersBinds.Add(oRetVal, pBuffer);
				}
				oRetVal.oFrameBytes = pBuffer;
				(new Logger("DeckLinkFake", sName)).WriteNotice("new decklink video frame was created. [count=" + _ahFramesBuffersBinds.Count + "]");
			}
			else
				(new Logger("DeckLinkFake", sName)).WriteError(new Exception("CREATE VIDEOFRAME RETURNED NULL!"));
			return oRetVal;
		}
		private byte[] VolumeChange(byte[] aIn)
		{
			if (!Preferences.bAudio || 0 == Preferences.nAudioVolumeChangeInDB || 16 != Preferences.nAudioBitDepth)
				return aIn;

			short nk;	
			int ntmp, nBytesQty = aIn.Length;
			float nVChange = Preferences.nAudioVolumeChange;

			for (int nii = 0; nii < nBytesQty; nii += 2)
			{
				if (0 != aIn[nii] || 0 != aIn[nii + 1])
				{
					nk = (short)((aIn[nii + 1] << 8) + aIn[nii]);

					ntmp = (int)(nk * nVChange);

					if (Int16.MaxValue < ntmp)
						ntmp = Int16.MaxValue;
					else if (Int16.MinValue > ntmp)
						ntmp = Int16.MinValue;
					nk = (short)ntmp;
					aIn[nii] = (byte)nk;
					aIn[nii + 1] = (byte)(nk >> 8);
				}
			}
			return aIn;
		}
		override protected bool FrameSchedule()
		{
			bool bVideoAdded = false;
			bool bAudioAdded = false;
			uint nVideoFramesBuffered = 0, nAudioSamplesBuffered = 0;
			long nAudioStreamTime = 0, nVideoStreamTime = 0;
			Frame.Audio cFrameAudio;
			Frame.Video cFrameVideo;
			_dtLastTimeFrameScheduleCalled = DateTime.Now;

			#region audio

			if (!NextFrameAttached)
				return false;

			try
			{
                nAudioSamplesBuffered = uint.MinValue;

                if (
					_nAudioBufferCapacity_InSamples >= nAudioSamplesBuffered + Preferences.nAudioSamplesPerFrame
					&& null != (cFrameAudio = AudioFrameGet())
					&& null != cFrameAudio.aFrameBytes
					&& 0 < cFrameAudio.aFrameBytes.Length
				)
				{


					cFrameAudio.Dispose();

					bAudioAdded = true;
					nAudioSamplesBuffered += Preferences.nAudioSamplesPerFrame;
					_cAudioBuffer.Advance();

					_nAudioQueueLength = (int)(nAudioSamplesBuffered / Preferences.nAudioSamplesPerFrame) + 1; //logging
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLinkFake", sName)).WriteError(ex);
			}

			// bug
			//System.Threading.Thread.Sleep(20);


			#endregion
			#region video
			try
			{
                nVideoFramesBuffered = uint.MinValue;
                //BTL.Baetylus.nVideoFramesBuffered = nVideoFramesBuffered; //отладка
                while (Preferences.nQueueDeviceLength > nVideoFramesBuffered) // (_nVideoBufferCapacity > nVideoFramesBuffered)
				{

					if (null == (cFrameVideo = VideoFrameGet()) || IntPtr.Zero == cFrameVideo.pFrameBytes)    // _nFPS + _nVideoBufferExtraCapacity < nVideoFramesBuffered
					{
						(new Logger("DeckLinkFake", sName)).WriteDebug("got null instead of frame IN DECKLINK !!");
						break;
					}
                    bVideoAdded = true;
                    break;




                    lock (_ahFramesBuffersBinds)
					{
						if (!_ahFramesBuffersBinds.ContainsKey(cFrameVideo))
						{
							(new Logger("DeckLinkFake", sName)).WriteError(new Exception("полученный видео буфер не зарегистрирован [" + cFrameVideo.pFrameBytes.ToInt64() + "]"));
							continue;
						}

						_cBugCatcherScheduleFrame.Enqueue(cFrameVideo, "FrameSchedule: [_ahFramesBuffersBinds.Count = " + _ahFramesBuffersBinds.Count + "]");



						if (!Device._aCurrentFramesIDs.ContainsKey(cFrameVideo.nID))
						{
							(new Logger("DeckLinkFake", sName)).WriteDebug4("frame " + cFrameVideo.nID + " was added");
							Device._aCurrentFramesIDs.Add(cFrameVideo.nID, _cStopWatch.ElapsedMilliseconds);
						}
						else if (0 < cFrameVideo.nID && _cVideoFrameEmpty.nID!= cFrameVideo.nID)  // && 0 < Baetylus.nVideoBufferCount
							(new Logger("DeckLinkFake", sName)).WriteDebug("VERY STRANGE - 2   error  [id=" + cFrameVideo.nID + "]");

					}
					bVideoAdded = true;
					n__PROBA__AudioFramesBuffered = _nAudioQueueLength;
					n__PROBA__VideoFramesBuffered = (int)nVideoFramesBuffered + 1;

                    break;
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLinkFake", sName)).WriteError(ex);
			}
			#endregion
			return bVideoAdded || bAudioAdded; //??
		}



        void Worker(object cState)
        {
            while (true)
            {
                RenderAudioSamples(1);
                ScheduledFrameCompleted();
                System.Threading.Thread.Sleep(1);
            }
        }
		#region callbacks
		void RenderAudioSamples(int preroll)
		{
			try
			{
				if (1 > preroll)
                    return;
                FrameSchedule();
                (new Logger("DeckLinkFake", sName)).WriteNotice("preroll " + Preferences.nQueueDeviceLength + " frames. start playback");
			}
			catch (Exception ex)
			{
				(new Logger("DeckLinkFake", sName)).WriteError(ex);
			}
		}
		void ScheduledFrameCompleted()
		{
			try
			{

				Frame.Video oFrame;
				long nNow = _cStopWatch.ElapsedMilliseconds;
				lock (_ahFramesBuffersBinds)
				{
					oFrame = _ahFramesBuffersBinds.Keys.FirstOrDefault();
				}

				if (null == oFrame)
					(new Logger("DeckLinkFake", sName)).WriteWarning("frame is not in _ahFramesBuffersBinds");
				else
				{
					FrameBufferReleased(oFrame);
				}

				_bNeedToAddFrame = true;
			}
			catch (Exception ex)
			{
				(new Logger("DeckLinkFake", sName)).WriteError(ex);
			}
		}



		void ScheduledPlaybackHasStopped()
		{
			(new Logger("DeckLinkFake", sName)).WriteNotice("playback stopped");
		}

		void VideoInputFrameArrived()
		{
            try
            {
                if (bAVFrameArrivedAttached)
                {
                    IntPtr pBytesVideo = _pFakePointer;
                    int nBytesVideoQty = 1920 * 4 * 1080;
                    nFramesVideo++;

                    IntPtr pBytesAudio = _pFakePointer;
                    int nBytesAudioQty = (48000 * sizeof(short)) / 40 * 2;
                    nFramesAudio++;

                    OnAVFrameArrived(nBytesVideoQty, pBytesVideo, nBytesAudioQty, pBytesAudio);
                }
            }
            catch (Exception ex)
            {
                (new Logger("DeckLinkFake", sName)).WriteError(ex);
            }
        }
		#endregion
	}
}
