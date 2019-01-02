using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using DeckLinkAPI;
using helpers;
using helpers.extensions;
using System.Linq;

// отладка
using System.Drawing;
using System.Drawing.Imaging;           // отладка
// отладка

namespace BTL.Device
{
	public class Decklink : Device, IDeckLinkAudioOutputCallback, IDeckLinkVideoOutputCallback, IDeckLinkInputCallback
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

		private IDeckLink _iDLDevice;
		private IDeckLinkInput _iDLInput;
		private IDeckLinkOutput _iDLOutput;
		private IDeckLinkDisplayMode _iDLInputDisplayMode;
		private IDeckLinkDisplayMode _iDLOutputDisplayMode;
		private IDeckLinkKeyer _iDeckLinkKeyer;

        private IDeckLinkVideoConversion _iDLVideoConversion;
        private IDeckLinkMutableVideoFrame _iVideoFrameTarget;
        private IDeckLinkVideoFrame _iVideoFrameLast;
        public ulong nFramesVideo;
        public ulong nFramesAudio;
        public ulong nFramesDroppedVideo;
        public ulong nFramesDroppedAudio;


        private bool _TurnedOn;
		private _BMDAudioSampleType _eAudioSampleDepth;
		private _BMDAudioSampleRate _eAudioSampleRate;
		private Dictionary<Frame.Video, IDeckLinkMutableVideoFrame> _ahFramesBuffersBinds;
		private long _nVideoStreamTime;
		private long _nAudioStreamTime;
        private int _nAudioQueueLength;
		private uint _nLogCounter2;
        private bool? _bItsOk;
		private string _sIterationsCounter2 = ".";
		protected AudioBuffer _cAudioBuffer;
        private _BMDPixelFormat? _ePixelFormat;
        private _BMDPixelFormat ePixelFormat
        {
            get
            {
                if (_ePixelFormat == null)
                {
                    bool bFound;
                    _ePixelFormat = Preferences.sPixelsFormat.ToEnumContainingString<_BMDPixelFormat>(_BMDPixelFormat.bmdFormat8BitBGRA, out bFound);
                    if (!bFound)
                        throw new Exception("incorrect pixels format in prefs: [" + Preferences.sPixelsFormat + "]"); //TODO LANG
                }
                return _ePixelFormat.Value;
                //for input
                //return DeckLinkAPI._BMDPixelFormat.bmdFormat8BitYUV;
                //for output
                //return DeckLinkAPI._BMDPixelFormat.bmdFormat8BitBGRA;
            }
        }
        private ushort _nTimescale;

        int _nFrameBufSize;
		private _BMDReferenceStatus? _eReferenceStatus = null;
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
            int nRetVal = 0;
            IDeckLinkIterator cDeckLinkIterator = new CDeckLinkIterator();
            if (null != cDeckLinkIterator)
            {
                while (true)
                {
                    IDeckLink cDeckLink;
                    cDeckLinkIterator.Next(out cDeckLink);
                    if (null == cDeckLink)
                        break;
                    nRetVal++;
                }
            }
            (new Logger("DeckLink")).WriteDebug3("boards.qty.get:out [" + nRetVal + "]");
            return nRetVal;
        }
        new static public Device[] BoardsGet()
		{
			(new Logger("DeckLink")).WriteDebug3("in");
			List<Device> aRetVal = new List<Device>();
			IDeckLinkIterator cDeckLinkIterator = new CDeckLinkIterator();
			if (null != cDeckLinkIterator)
			{
				while (true)
				{
					(new Logger("DeckLink")).WriteDebug4("boards:get: next [brdsqty:" + aRetVal.Count + "]");
					IDeckLink cDeckLink;
					(new Logger("DeckLink")).WriteDebug4("boards:get:iterator:before");
					cDeckLinkIterator.Next(out cDeckLink);
					(new Logger("DeckLink")).WriteDebug4("boards:get:iterator:after");
					if (null == cDeckLink)
						break;
					aRetVal.Add(new Decklink(cDeckLink));
				}
			}
			(new Logger("DeckLink")).WriteDebug4("boards:get:device:out");
			return aRetVal.ToArray();
		}

		private Decklink(IDeckLink cDevice)
		{
			(new Logger("DeckLink")).WriteDebug3("in");
			try
			{
				_ahFramesBuffersBinds = new Dictionary<Frame.Video, IDeckLinkMutableVideoFrame>();

				_nAudioQueueLength = 0;
				_iDLDevice = cDevice;
				if(Preferences.bDeviceInput)
					_iDLInput = (IDeckLinkInput)_iDLDevice;
				else
					_iDLOutput = (IDeckLinkOutput)_iDLDevice;

				IDeckLinkDisplayModeIterator cDisplayModeIterator;
				IDeckLinkDisplayMode cNextDLDisplayMode;
				string sDisplayModeName = "";

                string sMessage = "decklink supported modes:<br>";
				if (Preferences.bDeviceInput)
				{
					_iDLInputDisplayMode = null;
					_iDLInput.GetDisplayModeIterator(out cDisplayModeIterator);
					while (true)
					{
						cDisplayModeIterator.Next(out cNextDLDisplayMode);
						if (cNextDLDisplayMode == null)
							break;
						cNextDLDisplayMode.GetName(out sDisplayModeName);
						if (null == _iDLInputDisplayMode && sDisplayModeName.ToLower().Contains(Preferences.sVideoFormat))
						{
							sMessage += "selected:";
							_iDLInputDisplayMode = cNextDLDisplayMode;
						}
						else
							sMessage += "\t";
						sMessage += sDisplayModeName + "<br>";
					}
					(new Logger("DeckLink")).WriteNotice(sMessage);
					if (null == _iDLInputDisplayMode)
						throw new Exception("can't find " + Preferences.sVideoFormat + " mode within specified device for input");
				}
				else
				{
					_iDLOutputDisplayMode = null;
					_iDLOutput.GetDisplayModeIterator(out cDisplayModeIterator);
					while (true)
					{

						cDisplayModeIterator.Next(out cNextDLDisplayMode);
						if (cNextDLDisplayMode == null)
							break;
						cNextDLDisplayMode.GetName(out sDisplayModeName);
						if (null == _iDLOutputDisplayMode && sDisplayModeName.ToLower().Contains(Preferences.sVideoFormat))
						{
							sMessage += "selected:";
							_iDLOutputDisplayMode = cNextDLDisplayMode;
						}
						else
							sMessage += "\t";
						sMessage += sDisplayModeName + "<br>";
                    }
                    (new Logger("DeckLink")).WriteNotice(sMessage);

                    sMessage = "";
                    foreach (_BMDPixelFormat ePF in Enum.GetValues(typeof(_BMDPixelFormat)))
                        sMessage += "<br>\t\t" + ePF;
                    (new Logger("DeckLink")).WriteNotice("\tSupported pixel formats:" + sMessage);

                    if (null == _iDLOutputDisplayMode)
						throw new Exception("can't find " + Preferences.sVideoFormat + " mode within specified device for output");
					stArea = new Area(0, 0, (ushort)_iDLOutputDisplayMode.GetWidth(), (ushort)_iDLOutputDisplayMode.GetHeight());

					long nFrameDuration, nFrameTimescale;
					_iDLOutputDisplayMode.GetFrameRate(out nFrameDuration, out nFrameTimescale);
					Preferences.nFPS = (ushort)((nFrameTimescale + (nFrameDuration - 1)) / nFrameDuration); //до ближайшего целого - взято из примера деклинка

                    if (Preferences.bAudio)
					{
						switch (Preferences.nAudioBitDepth)
						{
							case 32:
								_eAudioSampleDepth = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
								break;
							case 16:
							default:
								_eAudioSampleDepth = _BMDAudioSampleType.bmdAudioSampleType16bitInteger;
								break;
						}
						switch (Preferences.nAudioSamplesRate)
						{
							case 48000:
								_eAudioSampleRate = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
								break;
							default:
								throw new Exception("unsupported audio sample rate [" + Preferences.nAudioSamplesRate + "]");
						}
						//_pAudioSamplesBuffer = Marshal.AllocCoTaskMem((int)_nAudioFrameSize_InBytes);
						_cAudioBuffer = new AudioBuffer();
						_nAudioBufferCapacity_InSamples = Preferences.nAudioSamplesPerFrame * Preferences.nQueueDeviceLength;
						//for (int nIndx = 0; _nAudioFrameSize_InBytes > nIndx; nIndx++)
						//    Marshal.WriteByte(_pAudioSamplesBuffer, nIndx, 0);

						_nTimescale = (ushort)_eAudioSampleRate;
					}
					if (null != Preferences.cDownStreamKeyer)
					{
						if (_iDLDevice is IDeckLinkKeyer)
							_iDeckLinkKeyer = (IDeckLinkKeyer)_iDLDevice;
						else
							(new Logger("DeckLink")).WriteWarning("This device is not Keyer device. Don't use keyer in preferences");
					}
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink")).WriteError(ex);
				throw;
			}
			(new Logger("DeckLink")).WriteDebug4("return");
		}
		~Decklink()
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
                if (null != _iDLOutput && _TurnedOn)  // иначе, если не TurnedOn, то error при StopScheduledPlayback
                {
                    long n;
                    _iDLOutput.StopScheduledPlayback(0, out n, Preferences.nFPS);
                    _iDLOutput.DisableAudioOutput();
                    _iDLOutput.DisableVideoOutput();
                    _ahFramesBuffersBinds.Clear();
                    _aCurrentFramesIDs.Clear();
                }
            }
            catch (Exception ex)
            {
                (new Logger("DeckLink")).WriteError(ex);
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
                    (new Logger("DeckLink")).WriteError(ex);
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
			if (Preferences.bDeviceInput)
			{
                if (_BMDPixelFormat.bmdFormat8BitYUV != ePixelFormat)
                {
                    int nWidth = _iDLInputDisplayMode.GetWidth(), nRowBytesQty = 0;
                    switch (ePixelFormat)
                    {
                        case _BMDPixelFormat.bmdFormat8BitBGRA:
                        case _BMDPixelFormat.bmdFormat8BitARGB:
                        case _BMDPixelFormat.bmdFormat10BitRGB:
                            nRowBytesQty = nWidth * 4;
                            break;
                        case _BMDPixelFormat.bmdFormat8BitYUV:
                            nRowBytesQty = nWidth * 2;
                            break;
                        case _BMDPixelFormat.bmdFormat10BitYUV:
                            nRowBytesQty = ((nWidth + 47) / 48) * 128;
                            break;
                    }
                    ((IDeckLinkOutput)_iDLDevice).CreateVideoFrame(nWidth, _iDLInputDisplayMode.GetHeight(), nRowBytesQty, ePixelFormat, _BMDFrameFlags.bmdFrameFlagDefault, out _iVideoFrameTarget);
                    _iDLVideoConversion = new CDeckLinkVideoConversion();
                }
                _iDLInput.EnableVideoInput(_iDLInputDisplayMode.GetDisplayMode(), _BMDPixelFormat.bmdFormat8BitYUV, _BMDVideoInputFlags.bmdVideoInputFlagDefault);
                _iDLInput.EnableAudioInput(_BMDAudioSampleRate.bmdAudioSampleRate48kHz, _BMDAudioSampleType.bmdAudioSampleType16bitInteger, 2);
                _iDLInput.SetCallback(this);

			}
			else
			{
				//_ahFramesBuffersBinds = new Dictionary<Frame.Video, IDeckLinkMutableVideoFrame>();
				_aCurrentFramesIDs = new Dictionary<long, long>();
				_cStopWatch = System.Diagnostics.Stopwatch.StartNew();
				DownStreamKeyer();
				_iDLOutput.SetAudioCallback(this);
				_iDLOutput.SetScheduledFrameCompletionCallback(this);
				_iDLOutput.EnableVideoOutput(_iDLOutputDisplayMode.GetDisplayMode(), _BMDVideoOutputFlags.bmdVideoOutputFlagDefault);
				_iDLOutput.EnableAudioOutput(_eAudioSampleRate, _eAudioSampleDepth, Preferences.nAudioChannelsQty, _BMDAudioOutputStreamType.bmdAudioOutputStreamContinuous);
			}
			base.TurnOn();
			if (Preferences.bDeviceInput)
				_iDLInput.StartStreams();
			else
				_iDLOutput.BeginAudioPreroll();

            _TurnedOn = true;
            (new Logger("DeckLink")).WriteNotice("decklink turned on");
		}
		override public void DownStreamKeyer()
		{// external key =1;  internal key =0
			base.DownStreamKeyer();
			if (null != Preferences.cDownStreamKeyer)
			{
				_iDeckLinkKeyer.Enable(Preferences.cDownStreamKeyer.bInternal ? 0 : 1);
				_iDeckLinkKeyer.SetLevel(Preferences.cDownStreamKeyer.nLevel);
				(new Logger("DeckLink")).WriteNotice("keyer enabled [" + Preferences.cDownStreamKeyer.bInternal + "][" + Preferences.cDownStreamKeyer.nLevel + "]");
			}
			else
			{
				//_iDeckLinkKeyer.Disable();
				(new Logger("DeckLink")).WriteNotice("keyer disabled");
			}
		}
		override protected Frame.Video FrameBufferPrepare()
		{
			IDeckLinkMutableVideoFrame cVideoFrame;
			Frame.Video oRetVal = new Frame.Video();
			IntPtr pBuffer;
            (new Logger("DeckLink")).WriteNotice("!!!!!:" + stArea.nWidth + ":" + stArea.nHeight + ":" + stArea.nWidth * 4 + ":" + ePixelFormat);
			_iDLOutput.CreateVideoFrame(stArea.nWidth, stArea.nHeight, stArea.nWidth * 4, ePixelFormat, _BMDFrameFlags.bmdFrameFlagDefault, out cVideoFrame);
			_nFrameBufSize = stArea.nWidth * stArea.nHeight * 4;
			cVideoFrame.GetBytes(out pBuffer);
			if (IntPtr.Zero != pBuffer)
			{
				lock (_ahFramesBuffersBinds)
				{
					if (_ahFramesBuffersBinds.Values.Contains(cVideoFrame))   // проверить было ли вообще такое!!!!
						(new Logger("DeckLink")).WriteError(new Exception("TRYING TO INSERT FRAME [type = IDeckLinkMutableVideoFrame] INTO _ahFramesBuffersBinds, THAT ALREADY EXISTS THERE!"));
					_ahFramesBuffersBinds.Add(oRetVal, cVideoFrame);
				}
				oRetVal.oFrameBytes = pBuffer;
				(new Logger("DeckLink")).WriteNotice("new decklink video frame was created. [count=" + _ahFramesBuffersBinds.Count + "]");
			}
			else
				(new Logger("DeckLink")).WriteError(new Exception("CREATE VIDEOFRAME RETURNED NULL!"));
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
			_BMDReferenceStatus eCurrentRef;
			_iDLOutput.GetReferenceStatus(out eCurrentRef);
			if (null == _eReferenceStatus || _eReferenceStatus != eCurrentRef)
			{
				_eReferenceStatus = eCurrentRef;
				(new Logger("DeckLink")).WriteWarning("Refference status has changed to: [" + eCurrentRef + "]");
			}

			#region audio

			if (!NextFrameAttached)
				return false;

			try
			{
				_iDLOutput.GetBufferedAudioSampleFrameCount(out nAudioSamplesBuffered);
				#region отладка
				/*
                    BMD.DeckLink._nAudioSamplesBuffered = nAudioSamplesBuffered; 
                    if (DeckLink._bMustGetImage)
                    {
                        ffmpeg.net.File.Input._bMustGetImage = true;
                        DeckLink._bMustGetImage = false;
                    }
                    if (ffmpeg.net.File.Input._bImageIsReady)
                    {
                        DeckLink.BM = ffmpeg.net.File.Input.BM;
                        DeckLink._bImageIsReady = true;
                        ffmpeg.net.File.Input._bImageIsReady = false;
                    }
					 * */
				#endregion

				if (
					_nAudioBufferCapacity_InSamples >= nAudioSamplesBuffered + Preferences.nAudioSamplesPerFrame
					&& null != (cFrameAudio = AudioFrameGet())
					&& null != cFrameAudio.aFrameBytes
					&& 0 < cFrameAudio.aFrameBytes.Length
				)
				{
					//if (1 > ((_nFramesDropped - _nFramesLated) - _nFramesAudioDropped))
					//{

					uint nSamplesWritten = 0;
					//Marshal.Copy(aAudioBytes, 0, _pAudioSamplesBuffer, aAudioBytes.Length);



					_cAudioBuffer.Write(VolumeChange(cFrameAudio.aFrameBytes.aBytes));
					if (Preferences.nAudioBytesPerFrame != cFrameAudio.aFrameBytes.Length)
						(new Logger("DeckLink")).WriteWarning("wrong audio buffer length: " + cFrameAudio.aFrameBytes.Length + " bytes. expecting " + Preferences.nAudioBytesPerFrame + " bytes.");
					cFrameAudio.Dispose();

					//BTL.Baetylus.nAudioStreamTime = cFrameAudio.nID;  //отладка
					nAudioStreamTime = (long)(_nAudioStreamTime * Preferences.nAudioSamplesPerFrame);
					_nAudioStreamTime++;

					nSamplesWritten = 0;
					uint nSamplesToWrite = Preferences.nAudioSamplesPerFrame;
					while (nSamplesToWrite > nSamplesWritten)
					{
						nSamplesToWrite -= nSamplesWritten;
						nAudioStreamTime += nSamplesWritten;
						_iDLOutput.ScheduleAudioSamples((IntPtr)(_cAudioBuffer.GetCurrent().ToInt64() + ((Preferences.nAudioSamplesPerFrame - nSamplesToWrite) * Preferences.nAudioBytesPerSample)), nSamplesToWrite, nAudioStreamTime, _nTimescale, out nSamplesWritten);
					}
					bAudioAdded = true;
					nAudioSamplesBuffered += Preferences.nAudioSamplesPerFrame;
					_cAudioBuffer.Advance();

					_nAudioQueueLength = (int)(nAudioSamplesBuffered / Preferences.nAudioSamplesPerFrame) + 1; //logging
					//}
					//else
					//    _nFramesAudioDropped++;
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink")).WriteError(ex);
			}

			// bug
			//System.Threading.Thread.Sleep(20);


			#endregion
			#region video
			try
			{
				_iDLOutput.GetBufferedVideoFrameCount(out nVideoFramesBuffered);
				//BTL.Baetylus.nVideoFramesBuffered = nVideoFramesBuffered; //отладка
				while (Preferences.nQueueDeviceLength > nVideoFramesBuffered) // (_nVideoBufferCapacity > nVideoFramesBuffered)
				{

					if (null == (cFrameVideo = VideoFrameGet()) || IntPtr.Zero == cFrameVideo.pFrameBytes)    // _nFPS + _nVideoBufferExtraCapacity < nVideoFramesBuffered
					{
						(new Logger("DeckLink")).WriteDebug("got null instead of frame IN DECKLINK !!");
						break;
					}

					#region отладка
					//BTL.Baetylus._nVideoStreamTime = _nVideoStreamTime;
					//if (Baetylus._bMustGetImageInDevice)
					//{
					//    BitmapData BD;
					//    Baetylus.BM = new Bitmap(720,576);
					//    Rectangle stRect = new Rectangle(0, 0, 720, 576);
					//    BD = Baetylus.BM.LockBits(stRect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					//    Marshal.Copy(_aFrameBytes, 0, BD.Scan0, _aFrameBytes.Length);
					//    Baetylus.BM.UnlockBits(BD);
					//    Baetylus.BM.Save(@"\IMG.png");
					//    Baetylus._bImageIsReadyInDevice = true;
					//    Baetylus._bMustGetImageInDevice = false;
					//}
					#endregion

					lock (_ahFramesBuffersBinds)
					{
						if (!_ahFramesBuffersBinds.ContainsKey(cFrameVideo))
						{
							(new Logger("DeckLink")).WriteError(new Exception("полученный видео буфер не зарегистрирован [" + cFrameVideo.pFrameBytes.ToInt64() + "]"));
							continue;
						}

						_cBugCatcherScheduleFrame.Enqueue(cFrameVideo, "FrameSchedule: [_ahFramesBuffersBinds.Count = " + _ahFramesBuffersBinds.Count + "]");



						if (!Device._aCurrentFramesIDs.ContainsKey(cFrameVideo.nID))
						{
							(new Logger("DeckLink")).WriteDebug4("frame " + cFrameVideo.nID + " was added");
							Device._aCurrentFramesIDs.Add(cFrameVideo.nID, _cStopWatch.ElapsedMilliseconds);
						}
						else if (0 < cFrameVideo.nID && _cVideoFrameEmpty.nID!= cFrameVideo.nID)  // && 0 < Baetylus.nVideoBufferCount
							(new Logger("DeckLink")).WriteDebug("VERY STRANGE - 2   error  [id=" + cFrameVideo.nID + "]");

						_iDLOutput.ScheduleVideoFrame(_ahFramesBuffersBinds[cFrameVideo], _nVideoStreamTime++, 1, Preferences.nFPS);  //  nVideoStreamTime + _nFramesRecovered
					}
					bVideoAdded = true;
					n__PROBA__AudioFramesBuffered = _nAudioQueueLength;
					n__PROBA__VideoFramesBuffered = (int)nVideoFramesBuffered + 1;

					#region logging
					if (
							Preferences.nQueueDeviceLength - 2 > _nAudioQueueLength
							|| Preferences.nQueueDeviceLength - 2 > nVideoFramesBuffered + 1
							|| Preferences.nQueuePipeLength - 2 > base._nBufferFrameCount && 0 < base._nBufferFrameCount
							|| _aq__PROBA__AudioFrames.Count > 2 || _aq__PROBA__VideoFrames.Count > 2
						)
					{
                        if (_bItsOk == true)
                        {
                            (new Logger("DeckLink")).WriteError("device queue goes wrong-1:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc" + _nLogCounter2);
                            _bItsOk = false;
                            _nLogCounter2 = 0;
                        }
                        else if (_nLogCounter2++ >= 200)
                        {
                            _nLogCounter2 = 0;
                            (new Logger("DeckLink")).WriteError("device queue goes wrong-2:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc" + _nLogCounter2);
                        }
                    }
					else
					{
                        if (_bItsOk == null)
                        {
                            if (_nLogCounter2 >= 150)
                                _bItsOk = true;
                        }
                        else if (_bItsOk == false)
                            _bItsOk = true;

                        if (_nLogCounter2++ >= 2000)  
						{
							(new Logger("DeckLink")).WriteNotice("device queue:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")        " + _sIterationsCounter2);
							_sIterationsCounter2 = _sIterationsCounter2 == "." ? ".." : ".";
							_nLogCounter2 = 0;
                        }
					}

					//if (Preferences.nQueueBaetylusLength - 4 >= Baetylus.nVideoBufferCount && 0 < Baetylus.nVideoBufferCount)
					//	nLogCounter = 3000;
					//else
					//{
					//	if (nLogCounter++ >= 3000)
					//	{
					//		(new Logger("DeckLink")).WriteNotice("device queue:(" + nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ") baetylus buffer:" + Baetylus.nVideoBufferCount + " internal buffer:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")        " + nIterationsCounter++);
					//		nIterationsCounter = nIterationsCounter > 1 ? 0 : nIterationsCounter;
					//		nLogCounter = 0;
					//	}
					//}
					#endregion
					break;
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink")).WriteError(ex);
			}
			#endregion
			return bVideoAdded || bAudioAdded; //??
		}
		#region callbacks
		void IDeckLinkAudioOutputCallback.RenderAudioSamples(int preroll)
		{
			try
			{
				if (1 > preroll)
					return;
				uint nVideoFramesBuffered = 0, nAudioSamplesBuffered = 0;
				_iDLOutput.GetBufferedAudioSampleFrameCount(out nAudioSamplesBuffered);
				_iDLOutput.GetBufferedVideoFrameCount(out nVideoFramesBuffered);
				if (_nAudioBufferCapacity_InSamples > (nAudioSamplesBuffered + Preferences.nAudioSamplesPerFrame) || Preferences.nQueueDeviceLength > nVideoFramesBuffered)
				{
					FrameSchedule();
					return;
				}
				_iDLOutput.EndAudioPreroll();
				_iDLOutput.StartScheduledPlayback(0, Preferences.nFPS, 1.0);
				(new Logger("DeckLink")).WriteNotice("preroll " + Preferences.nQueueDeviceLength + " frames. start playback");
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink")).WriteError(ex);
			}
		}

		void IDeckLinkVideoOutputCallback.ScheduledFrameCompleted(IDeckLinkVideoFrame completedFrame, _BMDOutputFrameCompletionResult result)
		{
			try
			{
				switch (result)
				{
					case _BMDOutputFrameCompletionResult.bmdOutputFrameDisplayedLate:
						_nFramesLated++;
						(new Logger("DeckLink")).WriteNotice("frame lated. total:" + _nFramesLated);
						break;
					case _BMDOutputFrameCompletionResult.bmdOutputFrameDropped:
						_nFramesDropped++;
						(new Logger("DeckLink")).WriteNotice("frame dropped. total:" + _nFramesDropped);
						break;
					case _BMDOutputFrameCompletionResult.bmdOutputFrameFlushed:
						_nFramesFlushed++;
						(new Logger("DeckLink")).WriteNotice("frame flushed. total:" + _nFramesFlushed);
						break;
					//default:
					//    (new Logger("DeckLink")).WriteDebug4("ScheduledFrameCompleted normal");
					//    break;
				}
				Frame.Video oFrame;
				long nNow = _cStopWatch.ElapsedMilliseconds;
				lock (_ahFramesBuffersBinds)
				{
					oFrame = _ahFramesBuffersBinds.FirstOrDefault(o => o.Value == completedFrame).Key;
					if (null != oFrame && _aCurrentFramesIDs.ContainsKey(oFrame.nID))
					{
						(new Logger("DeckLink")).WriteDebug4("frame " + oFrame.nID + " was passed out " + (nNow - _aCurrentFramesIDs[oFrame.nID]) + " ms");  
						_aCurrentFramesIDs.Remove(oFrame.nID);
					}
					else if (0 < oFrame.nID && _cVideoFrameEmpty.nID != oFrame.nID)
						(new Logger("DeckLink")).WriteDebug("VERY STRANGE - 1   error  [id=" + oFrame.nID + "]");

					foreach (long nVFid in _aCurrentFramesIDs.Keys.Where(o => (Preferences.nQueueDeviceLength * 40 + 500) < nNow - _aCurrentFramesIDs[o]))
					{
						(new Logger("DeckLink")).WriteDebug2("There are some over timed frames in decklink buffer: \n");
						(new Logger("DeckLink")).WriteDebug2("\t\t[id=" + nVFid + "][delta=" + _aCurrentFramesIDs[nVFid] + "]\n");
					}
				}

				if (null == oFrame)
					(new Logger("DeckLink")).WriteWarning("frame is not in _ahFramesBuffersBinds");
				else
				{
					FrameBufferReleased(oFrame);
				}

				long nDelta = _cStopWatch.ElapsedMilliseconds - _nLastScTimeComplited;
				if (1 > nDelta) // 100 < nDelta || 
					(new Logger("DeckLink")).WriteDebug2("Last ScheduledFrameCompleted was " + nDelta + " ms ago");
				_nLastScTimeComplited = _cStopWatch.ElapsedMilliseconds;

				_bNeedToAddFrame = true;
				#region закаменчено /*   */ 
				//bool bFound = false;
				//IntPtr pFrameLast = IntPtr.Zero;
				//if (completedFrame != _cFrameEmpty)
				//{
				//    IDeckLinkMutableVideoFrame iFrame = (IDeckLinkMutableVideoFrame)completedFrame;
				//    if (0 > _aFramesRecovered.IndexOf(iFrame))
				//    {
				//        lock (_ahFramesBuffersBinds)
				//        {
				//            foreach (IntPtr pFB in _ahFramesBuffersBinds.Keys.ToArray())
				//            {
				//                if (iFrame == _ahFramesBuffersBinds[pFB])
				//                {
				//                    _ahFramesBuffersBinds.Remove(pFB);
				//                    pFrameLast = pFB;
				//                    bFound = true;
				//                    break;
				//                }
				//            }
				//        }
				//    }
				//    else
				//    {
				//        bFound = true;
				//        _aFramesRecovered.Remove(iFrame);
				//    }
				//}
				//if (!bFound)
				//    throw new Exception("не найден показанный кадр");
				//if (Preferences.bFrameLastSave && IntPtr.Zero != pFrameLast)
				//{
				//    if (null == aFrameLastBytes)
				//        aFrameLastBytes = new byte[completedFrame.GetHeight() * completedFrame.GetRowBytes()];
				//    lock (aFrameLastBytes)
				//        Marshal.Copy(pFrameLast, aFrameLastBytes, 0, aFrameLastBytes.Length);
				//}
				#endregion
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink")).WriteError(ex);
			}
		}



		void IDeckLinkVideoOutputCallback.ScheduledPlaybackHasStopped()
		{
			(new Logger("DeckLink")).WriteNotice("playback stopped");
		}

		void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame iVideoInputFrame, IDeckLinkAudioInputPacket iAudioPacket)
		{
            try
            {
                if (bAVFrameArrivedAttached)
                {
                    IDeckLinkVideoFrame iVideoFrame = iVideoInputFrame;
                    IntPtr pBytesVideo = IntPtr.Zero, pBytesAudio = IntPtr.Zero;
                    int nBytesVideoQty = 0, nBytesAudioQty = 0;
                    if (null != iVideoFrame)
                    {
                        if (null != _iDLVideoConversion)
                        {
                            _iDLVideoConversion.ConvertFrame(iVideoFrame, _iVideoFrameTarget);
                            iVideoFrame = _iVideoFrameTarget;
                        }
                        _iVideoFrameLast = iVideoFrame;
                    }
                    else
                    {
                        nFramesDroppedVideo++;
                        iVideoFrame = _iVideoFrameLast;
                        (new Logger("DeckLink")).WriteWarning("video frame dropped");
                    }
                    if (null != iVideoFrame)
                    {
                        iVideoFrame.GetBytes(out pBytesVideo);
                        if (IntPtr.Zero != pBytesVideo)
                        {
                            nBytesVideoQty = iVideoFrame.GetRowBytes() * iVideoFrame.GetHeight();
                            nFramesVideo++;
                        }
                        else
                            (new Logger("DeckLink")).WriteWarning("video frame is empty");
                    }
                    if (null != iAudioPacket)
                    {
                        iAudioPacket.GetBytes(out pBytesAudio);
                        if (IntPtr.Zero != pBytesAudio)
                        {
                            nBytesAudioQty = iAudioPacket.GetSampleFrameCount() * ((int)_BMDAudioSampleType.bmdAudioSampleType16bitInteger / 8) * 2;
                            nFramesAudio++;
                        }
                        else
                            (new Logger("DeckLink")).WriteWarning("audio frame is empty");
                    }
                    else
                    {
                        nFramesDroppedAudio++;
                        (new Logger("DeckLink")).WriteWarning("audio frame dropped");
                    }
                    OnAVFrameArrived(nBytesVideoQty, pBytesVideo, nBytesAudioQty, pBytesAudio);
                }
            }
            catch (Exception ex)
            {
                (new Logger("DeckLink")).WriteError(ex);
            }
        }
		void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
		{
		}
		#endregion
	}
}
