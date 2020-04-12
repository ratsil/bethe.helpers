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
		private IDeckLinkDisplayMode _iDLDisplayMode;
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
        private byte _nCurrentFrame_HH;
        private byte _nCurrentFrame_MM;
        private byte _nCurrentFrame_SS;
        private byte _nCurrentFrame_FF;
        private long _nAudioStreamTime;
        private int _nAudioQueueLength;
		private uint _nLogCounter2;
        private bool? _bItsOk;
		private string _sIterationsCounter2 = ".";
		protected AudioBuffer _cAudioBuffer;
        private _BMDPixelFormat? _ePixelFormat;
        public _BMDPixelFormat ePixelFormat
        {
            get
            {
                if (_ePixelFormat == null)
                {
                    bool bFound;
                    _ePixelFormat = Preferences.sPixelsFormat.ToEnumEqualsString<_BMDPixelFormat>(_BMDPixelFormat.bmdFormat8BitBGRA, out bFound);
                    bFound = true;
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
        private _BMDPixelFormat? _ePixelFormatTarget;
        private _BMDPixelFormat ePixelFormatTarget
        {
            get
            {
                if (_ePixelFormatTarget == null)
                {
                    if (Preferences.sPixelsFormatTarget == null)
                        _ePixelFormatTarget = ePixelFormat;
                    else
                    {
                        bool bFound;
                        _ePixelFormatTarget = Preferences.sPixelsFormatTarget.ToEnumEqualsString<_BMDPixelFormat>(ePixelFormat, out bFound);
                        bFound = true;
                        if (!bFound && Preferences.sPixelsFormatTarget != null)
                            throw new Exception("incorrect pixels format target in prefs: [" + Preferences.sPixelsFormatTarget + "]"); //TODO LANG
                    }
                }
                return _ePixelFormatTarget.Value;
            }
        }
        private ushort _nTimescale;

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
            IDeckLinkAPIInformation cInfo = (IDeckLinkAPIInformation)cDeckLinkIterator;
            string sApiInfo;
            cInfo.GetString(_BMDDeckLinkAPIInformationID.BMDDeckLinkAPIVersion, out sApiInfo);
            (new Logger("DeckLink", null)).WriteNotice($"api ver = [BMDDeckLinkAPIVersion = {sApiInfo}]");
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
            (new Logger("DeckLink", null)).WriteDebug3("boards.qty.get:out [" + nRetVal + "]");
            return nRetVal;
        }
        new static public Device BoardGet(uint nIndex)
		{
			(new Logger("DeckLink", null)).WriteDebug3("in");
            Device cRetVal = null;
			IDeckLinkIterator cDeckLinkIterator = new CDeckLinkIterator();
            if (null != cDeckLinkIterator)
			{
                uint nI = 0;
                while (true)
				{
					(new Logger("DeckLink", null)).WriteDebug4("boards:get: next [brdsqty:" + nI + "]");
					IDeckLink cDeckLink;
					(new Logger("DeckLink", null)).WriteDebug4("boards:get:iterator:before");
					cDeckLinkIterator.Next(out cDeckLink);
					(new Logger("DeckLink", null)).WriteDebug4("boards:get:iterator:after");
					if (null == cDeckLink)
						break;
                    if (nIndex == nI++)
                    {
                        cRetVal = new Decklink(cDeckLink, nIndex);
                        break;
                    }
				}
			}
			(new Logger("DeckLink", null)).WriteDebug4("boards:get:device:out");
			return cRetVal;
		}

		private Decklink(IDeckLink cDevice, uint nDeviceIndex)
            :base("DeckLink-" + nDeviceIndex + "_")
		{
            (new Logger("DeckLink", sName)).WriteDebug3("in");
			try
			{
				_ahFramesBuffersBinds = new Dictionary<Frame.Video, IDeckLinkMutableVideoFrame>();

				_nAudioQueueLength = 0;
				_iDLDevice = cDevice;
                IDeckLinkDisplayModeIterator cDisplayModeIterator;
                IDeckLinkDisplayMode cNextDLDisplayMode;
                _iDLDisplayMode = null;
                string sDisplayModeName = "";
                if (bInput)
                {
                    _iDLInput = (IDeckLinkInput)_iDLDevice;
                    _iDLInput.GetDisplayModeIterator(out cDisplayModeIterator);
                }
                else
                {
                    _iDLOutput = (IDeckLinkOutput)_iDLDevice;
                    _iDLOutput.GetDisplayModeIterator(out cDisplayModeIterator);
                }

                string sMessage = "decklink supported modes:<br>";
                while (true)
                {
                    cDisplayModeIterator.Next(out cNextDLDisplayMode);
                    if (cNextDLDisplayMode == null)
                        break;
                    cNextDLDisplayMode.GetName(out sDisplayModeName);
                    if (null == _iDLDisplayMode && sDisplayModeName.ToLower().Contains(Preferences.sVideoFormat))
                    {
                        sMessage += "selected:";
                        _iDLDisplayMode = cNextDLDisplayMode;
                    }
                    else
                        sMessage += "\t";
                    sMessage += sDisplayModeName + "<br>";
                }
                (new Logger("DeckLink", sName)).WriteNotice(sMessage);
                if (null == _iDLDisplayMode)
                    throw new Exception("can't find this mode [" + Preferences.sVideoFormat + "] within specified device");

                sMessage = "";
                foreach (_BMDPixelFormat ePF in Enum.GetValues(typeof(_BMDPixelFormat)))
                    sMessage += "<br>\t\t" + ePF;
                (new Logger("DeckLink", sName)).WriteNotice("\tSupported pixel formats:" + sMessage);

                stArea = new Area(0, 0, (ushort)_iDLDisplayMode.GetWidth(), (ushort)_iDLDisplayMode.GetHeight());

                long nFrameDuration, nFrameTimescale;
                _iDLDisplayMode.GetFrameRate(out nFrameDuration, out nFrameTimescale);
                nFPS = Preferences.nFPS = (ushort)((nFrameTimescale + (nFrameDuration - 1)) / nFrameDuration); //до ближайшего целого - взято из примера деклинка

                _nRowBytesQty = 0;
                switch (ePixelFormat)
                {
                    case _BMDPixelFormat.bmdFormat8BitBGRA:
                    case _BMDPixelFormat.bmdFormat8BitARGB:
                        _nRowBytesQty = stArea.nWidth * 4;
                        break;
                    case _BMDPixelFormat.bmdFormat8BitYUV:
                        _nRowBytesQty = stArea.nWidth * 2;
                        break;
                    case _BMDPixelFormat.bmdFormat10BitYUV:
                        _nRowBytesQty = ((stArea.nWidth + 47) / 48) * 128;
                        break;
                    case _BMDPixelFormat.bmdFormat10BitRGB:
                        _nRowBytesQty = ((stArea.nWidth + 63) / 64) * 256;
                        break;
                }
                _nVideoBytesQty = _nRowBytesQty * stArea.nHeight;
                _aTmpUint = new uint[100];   // new uint[_nRowBytesQty / 4];    //  we use 42 words only (for teletext), but set 100.  _nRowBytesQty / 4 is max value. 

                if (Preferences.bAudio)
                {
                    _nAudioChannelsQty = Preferences.nAudioChannelsQty;
                    if (_nAudioChannelsQty != 2 && _nAudioChannelsQty != 8 && _nAudioChannelsQty != 16)
                        throw new Exception($"Audio Channels must be either 2, 8 or 16. [ch_qty={_nAudioChannelsQty}]");
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
                    _nChannelBytesQty = Preferences.nAudioSamplesPerFrame * Preferences.nAudioByteDepth;
                    _nAudioBytesQty = _nAudioChannelsQty * _nChannelBytesQty;

                    //for (int nIndx = 0; _nAudioFrameSize_InBytes > nIndx; nIndx++)
                    //    Marshal.WriteByte(_pAudioSamplesBuffer, nIndx, 0);
                    _nTimescale = (ushort)_eAudioSampleRate;
                }

                if (_iDLInput != null)
                {
                }
				else
				{
					if (null != Preferences.cDownStreamKeyer)
					{
						if (_iDLDevice is IDeckLinkKeyer)
							_iDeckLinkKeyer = (IDeckLinkKeyer)_iDLDevice;
						else
							(new Logger("DeckLink", sName)).WriteWarning("This device is not Keyer device. Don't use keyer in preferences");
					}
				}

                // moved from turn on
                if (_iDLInput != null)
                {
                    if (ePixelFormatTarget != ePixelFormat)
                    {
                        ((IDeckLinkOutput)_iDLDevice).CreateVideoFrame(stArea.nWidth, stArea.nHeight, _nRowBytesQty, ePixelFormat, _BMDFrameFlags.bmdFrameFlagDefault, out _iVideoFrameTarget);
                        _iDLVideoConversion = new CDeckLinkVideoConversion();
                    }
                    _BMDDisplayMode eDM = _iDLDisplayMode.GetDisplayMode();
                    _iDLInput.SetCallback(this);
                    //_iDLInput.EnableVideoInput(eDM, _BMDPixelFormat.bmdFormat8BitYUV, _BMDVideoInputFlags.bmdVideoInputFlagDefault);
                    _iDLInput.EnableVideoInput(eDM, ePixelFormatTarget, _BMDVideoInputFlags.bmdVideoInputFlagDefault);
                    _iDLInput.EnableAudioInput(_eAudioSampleRate, _eAudioSampleDepth, _nAudioChannelsQty);
                }
                else
                {
                    //_ahFramesBuffersBinds = new Dictionary<Frame.Video, IDeckLinkMutableVideoFrame>();
                    _aCurrentFramesIDs = new Dictionary<long, long>();
                    _cStopWatch = System.Diagnostics.Stopwatch.StartNew();
                    DownStreamKeyer();
                    _iDLOutput.SetAudioCallback(this);
                    _iDLOutput.SetScheduledFrameCompletionCallback(this);
                    _BMDVideoOutputFlags eOutputFlag = _ePixelFormat == _BMDPixelFormat.bmdFormat10BitYUV ? (_BMDVideoOutputFlags.bmdVideoOutputFlagDefault | _BMDVideoOutputFlags.bmdVideoOutputVANC | _BMDVideoOutputFlags.bmdVideoOutputRP188) : _BMDVideoOutputFlags.bmdVideoOutputFlagDefault; // _BMDVideoOutputFlags.bmdVideoOutputFlagDefault | _BMDVideoOutputFlags.bmdVideoOutputVANC | 
                    _iDLOutput.EnableVideoOutput(_iDLDisplayMode.GetDisplayMode(), eOutputFlag);
                    _iDLOutput.EnableAudioOutput(_eAudioSampleRate, _eAudioSampleDepth, _nAudioChannelsQty, _BMDAudioOutputStreamType.bmdAudioOutputStreamContinuous);
                }
            }
            catch (Exception ex)
			{
				(new Logger("DeckLink", sName)).WriteError(ex);
				throw;
			}
			(new Logger("DeckLink", sName)).WriteDebug4("return");
		}
		~Decklink()
		{
            Dispose();
		}
        static object oLockDispose = new object();
        override public void Dispose()
        {
            lock (oLockDispose)
            {
                try
                {
                    if (null != _iDLOutput)
                    {
                        long n;
                        if (_TurnedOn)
                            _iDLOutput.StopScheduledPlayback(0, out n, Preferences.nFPS);
                        _iDLOutput.DisableAudioOutput();
                        _iDLOutput.DisableVideoOutput();
                        _iDLOutput = null;
                        _ahFramesBuffersBinds?.Clear();
                        _aCurrentFramesIDs?.Clear();
                    }
                    if (null != _iDLInput)
                    {
                        if (_TurnedOn)
                            _iDLInput.StopStreams();
                        _iDLInput.DisableAudioInput();
                        _iDLInput.DisableVideoInput();
                        _iDLInput = null;
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("DeckLink", sName)).WriteError(ex);
                }
                finally
                {
                    try
                    {
                        if (null != _cAudioBuffer)
                        {
                            _cAudioBuffer.Dispose();
                            _cAudioBuffer = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DeckLink", sName)).WriteError(ex);
                    }
                    finally
                    {
                        _bStopped = true;
                    }
                }
            }
        }

        override public void TurnOn()
		{
			_nVideoStreamTime = 0;
			_nAudioStreamTime = 0;
            _nCurrentFrame_HH = 0;
            _nCurrentFrame_MM = 0;
            _nCurrentFrame_SS = 0;
            _nCurrentFrame_FF = 0;
            base.TurnOn();
			if (_iDLInput != null)
				_iDLInput.StartStreams();
			else
				_iDLOutput.BeginAudioPreroll();

            _TurnedOn = true;
            (new Logger("DeckLink", sName)).WriteNotice("decklink turned on");
		}
		override public void DownStreamKeyer()
		{// external key =1;  internal key =0
			base.DownStreamKeyer();
			if (null != Preferences.cDownStreamKeyer)
			{
				_iDeckLinkKeyer.Enable(Preferences.cDownStreamKeyer.bInternal ? 0 : 1);
				_iDeckLinkKeyer.SetLevel(Preferences.cDownStreamKeyer.nLevel);
				(new Logger("DeckLink", sName)).WriteNotice("keyer enabled [" + Preferences.cDownStreamKeyer.bInternal + "][" + Preferences.cDownStreamKeyer.nLevel + "]");
			}
			else
			{
				//_iDeckLinkKeyer.Disable();
				(new Logger("DeckLink", sName)).WriteNotice("keyer disabled");
			}
		}
		override protected Frame.Video FrameBufferPrepare()
		{
			IDeckLinkMutableVideoFrame cVideoFrame;
            Frame.Video oRetVal = new Frame.Video(sName);
			IntPtr pBuffer;
            (new Logger("DeckLink", sName)).WriteNotice("!!!!!:" + stArea.nWidth + ":" + stArea.nHeight + ":" + _nRowBytesQty + ":" + ePixelFormat);
			_iDLOutput.CreateVideoFrame(stArea.nWidth, stArea.nHeight, _nRowBytesQty, ePixelFormat, _BMDFrameFlags.bmdFrameFlagDefault, out cVideoFrame);
            if (ePixelFormat == _BMDPixelFormat.bmdFormat10BitYUV)
            {
                IDeckLinkVideoFrameAncillary cVFA; // for adding VANC data
                _iDLOutput.CreateAncillaryData(ePixelFormat, out cVFA);
                if (cVFA != null)
                {
                    if (cVideoFrame != null)
                        cVideoFrame.SetAncillaryData(cVFA);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cVFA);
                }
            }
			cVideoFrame.GetBytes(out pBuffer);
			if (IntPtr.Zero != pBuffer)
			{
				lock (_ahFramesBuffersBinds)
				{
					if (_ahFramesBuffersBinds.Values.Contains(cVideoFrame))   // проверить было ли вообще такое!!!!
						(new Logger("DeckLink", sName)).WriteError(new Exception("TRYING TO INSERT FRAME [type = IDeckLinkMutableVideoFrame] INTO _ahFramesBuffersBinds, THAT ALREADY EXISTS THERE!"));
					_ahFramesBuffersBinds.Add(oRetVal, cVideoFrame);
				}
				oRetVal.oFrameBytes = pBuffer;
				(new Logger("DeckLink", sName)).WriteNotice("new decklink video frame was created. [count=" + _ahFramesBuffersBinds.Count + "]");
			}
			else
				(new Logger("DeckLink", sName)).WriteError(new Exception("CREATE VIDEOFRAME RETURNED NULL!"));
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
				(new Logger("DeckLink", sName)).WriteWarning("Refference status has changed to: [" + eCurrentRef + "]");
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
						(new Logger("DeckLink", sName)).WriteWarning("wrong audio buffer length: " + cFrameAudio.aFrameBytes.Length + " bytes. expecting " + Preferences.nAudioBytesPerFrame + " bytes.");
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
				(new Logger("DeckLink", sName)).WriteError(ex);
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
						(new Logger("DeckLink", sName)).WriteDebug("got null instead of frame IN DECKLINK !!");
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
							(new Logger("DeckLink", sName)).WriteError(new Exception("полученный видео буфер не зарегистрирован [" + cFrameVideo.pFrameBytes.ToInt64() + "]"));
							continue;
						}

						_cBugCatcherScheduleFrame.Enqueue(cFrameVideo, "FrameSchedule: [_ahFramesBuffersBinds.Count = " + _ahFramesBuffersBinds.Count + "]");



						if (!Device._aCurrentFramesIDs.ContainsKey(cFrameVideo.nID))
						{
							(new Logger("DeckLink", sName)).WriteDebug4("frame " + cFrameVideo.nID + " was added");
							Device._aCurrentFramesIDs.Add(cFrameVideo.nID, _cStopWatch.ElapsedMilliseconds);
						}
						else if (0 < cFrameVideo.nID && _cVideoFrameEmpty.nID!= cFrameVideo.nID)  // && 0 < Baetylus.nVideoBufferCount
							(new Logger("DeckLink", sName)).WriteDebug("VERY STRANGE - 2   error  [id=" + cFrameVideo.nID + "]");




                        // new method:
                        ////Guid cG = Guid.Parse("sdfdsfds-sdfds-sdffs-dfdsd-sdfds"); // типа такого - не совсем понял где брать гуид этот
                        //IDeckLinkMutableVideoFrame cMVF = _ahFramesBuffersBinds[cFrameVideo];
                        //IDeckLinkVideoFrameAncillaryPackets cVFAP = (IDeckLinkVideoFrameAncillaryPackets)cMVF;
                        ////Marshal.QueryInterface(cMVF, ref cG, out cVFAP);  // получили пакеты   (довольно замороченно, поэтому, хуячу старым способом, см. ниже)
                        //IntPtr pData;
                        //uint nSize;
                        //IDeckLinkAncillaryPacket cAP;
                        //cVFAP.GetFirstPacketByID(0x61, 0x1, out cAP);
                        //cVFAP.DetachPacket(cAP);
                        //cAP = null; // создаём пакетег (см пример в closed capture в версии 11.4)
                        //////cAP.GetBytes(_BMDAncillaryPacketFormat.bmdAncillaryPacketFormatUInt8, out pData, out nSize);
                        ////// ????  что-то налили в pData  (или новый пакет создали и налили туда...)...
                        //cVFAP.AttachPacket(cAP);
                        // и пихаем cMVF в ScheduleVideoFrame



                        // old method:
                        IDeckLinkMutableVideoFrame cMVF = _ahFramesBuffersBinds[cFrameVideo];

                        // VITC  (_BMDVideoOutputFlags.bmdVideoOutputRP188)    // example is here:  C:\Users\vakhlamovv\Downloads\DeckLink\Blackmagic DeckLink SDK 10.8.6\Examples\RP188VitcOutput.cpp
                        //cMVF.SetTimecodeFromComponents(_BMDTimecodeFormat.bmdTimecodeRP188VITC1, _nCurrentFrame_HH, _nCurrentFrame_MM, _nCurrentFrame_SS, _nCurrentFrame_FF, _BMDTimecodeFlags.bmdTimecodeFlagDefault);
                        //cMVF.SetTimecodeFromComponents(_BMDTimecodeFormat.bmdTimecodeRP188VITC2, _nCurrentFrame_HH, _nCurrentFrame_MM, _nCurrentFrame_SS, _nCurrentFrame_FF, _BMDTimecodeFlags.bmdTimecodeFlagDefault | _BMDTimecodeFlags.bmdTimecodeFieldMark); // if not progressive

                        IDeckLinkVideoFrameAncillary cVFA = null;
                        if (_ePixelFormat == _BMDPixelFormat.bmdFormat10BitYUV)
                        {
                            cMVF.SetTimecodeFromComponents(_BMDTimecodeFormat.bmdTimecodeRP188LTC, _nCurrentFrame_HH, _nCurrentFrame_MM, _nCurrentFrame_SS, _nCurrentFrame_FF, _BMDTimecodeFlags.bmdTimecodeFlagDefault); //  no examples((
                            cMVF.SetTimecodeFromComponents(_BMDTimecodeFormat.bmdTimecodeRP188VITC1, _nCurrentFrame_HH, _nCurrentFrame_MM, _nCurrentFrame_SS, _nCurrentFrame_FF, _BMDTimecodeFlags.bmdTimecodeFlagDefault); //  no examples((
                            cMVF.SetTimecodeFromComponents(_BMDTimecodeFormat.bmdTimecodeRP188VITC2, _nCurrentFrame_HH, _nCurrentFrame_MM, _nCurrentFrame_SS, _nCurrentFrame_FF, _BMDTimecodeFlags.bmdTimecodeFlagDefault | _BMDTimecodeFlags.bmdTimecodeFieldMark); //  no examples((

                            if (!cFrameVideo.ahVancDataLine_Bytes.IsNullOrEmpty())
                            {
                                try
                                {
                                    cMVF.GetAncillaryData(out cVFA);
                                    IntPtr pData;
                                    foreach (uint nLine in cFrameVideo.ahVancDataLine_Bytes.Keys)
                                    {
                                        cVFA.GetBufferForVerticalBlankingLine(nLine, out pData);
                                        VancData.Clear(_aTmpUint);
                                        VancData.Set(_aTmpUint, cFrameVideo.ahVancDataLine_Bytes[nLine].aBytes);
                                        VancData.CopyUintArrayToPointer(_aTmpUint, pData, _aTmpUint.Length);
                                        //VancData.Get(pData, nLine, sName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    (new Logger("DeckLink", sName)).WriteError(ex);
                                }
                            }
                            IncrementTimecode();
                        }
                        _iDLOutput.ScheduleVideoFrame(cMVF, _nVideoStreamTime++, 1, Preferences.nFPS);    //  nVideoStreamTime + _nFramesRecovered   // ScheduleVideoFrame(videoFrame, gTotalFramesScheduled*kFrameDuration, kFrameDuration, kTimeScale)
                        if (cVFA != null)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(cVFA);
                    }
                    bVideoAdded = true;
					n__PROBA__AudioFramesBuffered = _nAudioQueueLength;
					n__PROBA__VideoFramesBuffered = (int)nVideoFramesBuffered + 1;

                    #region logging
                    if (
                            Preferences.nQueueDeviceLength - 2 > _nAudioQueueLength
                            || Preferences.nQueueDeviceLength - 2 > nVideoFramesBuffered + 1
                            || Preferences.nQueuePipeLength * 4 / 5 > base._nBufferFrameCount && 0 < base._nBufferFrameCount
                            || _aq__PROBA__AudioFrames.Count > 2 || _aq__PROBA__VideoFrames.Count > 2
                        )
                    {
                        if (_bItsOk == true)
                        {
                            (new Logger("DeckLink", sName)).WriteError("device queue goes wrong-1:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-0");
                            _bItsOk = false;
                            _nLogCounter2 = 0;
                        }
                        else if (_nLogCounter2++ >= 200)
                        {
                            (new Logger("DeckLink", sName)).WriteError("device queue goes wrong-2:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-" + _nLogCounter2 + _sIterationsCounter2);
                            _sIterationsCounter2 = _sIterationsCounter2 == "." ? ".." : ".";
                            _nLogCounter2 = 0;

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
                        {
                            (new Logger("DeckLink", sName)).WriteError("device queue was wrong:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-" + _nLogCounter2);
                            _bItsOk = true;
                        }

                        if (_nLogCounter2++ >= 2000)
                        {
                            (new Logger("DeckLink", sName)).WriteNotice("device queue:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered + 1) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")        " + _sIterationsCounter2);
                            _sIterationsCounter2 = _sIterationsCounter2 == "." ? ".." : ".";
                            _nLogCounter2 = 0;
                        }
                    }
                    #endregion
                    break;
				}
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink", sName)).WriteError(ex);
			}
			#endregion
			return bVideoAdded || bAudioAdded; //??
		}
        void IncrementTimecode()
        {
            _nCurrentFrame_FF++;
            if (_nCurrentFrame_FF >= 25)
            {
                _nCurrentFrame_FF = 0;
                _nCurrentFrame_SS++;
                if (_nCurrentFrame_SS >= 60)
                {
                    _nCurrentFrame_SS = 0;
                    _nCurrentFrame_MM++;
                    if (_nCurrentFrame_MM >= 60)
                    {
                        _nCurrentFrame_MM = 0;
                        _nCurrentFrame_HH++;
                        if (_nCurrentFrame_HH >= 24)
                        {
                            _nCurrentFrame_HH = 0;
                        }
                    }
                }
            }
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
				(new Logger("DeckLink", sName)).WriteNotice("preroll " + Preferences.nQueueDeviceLength + " frames. start playback");
			}
			catch (Exception ex)
			{
				(new Logger("DeckLink", sName)).WriteError(ex);
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
						(new Logger("DeckLink", sName)).WriteNotice("frame lated. total:" + _nFramesLated);
						break;
					case _BMDOutputFrameCompletionResult.bmdOutputFrameDropped:
						_nFramesDropped++;
						(new Logger("DeckLink", sName)).WriteNotice("frame dropped. total:" + _nFramesDropped);
						break;
					case _BMDOutputFrameCompletionResult.bmdOutputFrameFlushed:
						_nFramesFlushed++;
						(new Logger("DeckLink", sName)).WriteNotice("frame flushed. total:" + _nFramesFlushed);
						break;
					//default:
					//    (new Logger("DeckLink", sName)).WriteDebug4("ScheduledFrameCompleted normal");
					//    break;
				}
				Frame.Video oFrame;
				long nNow = _cStopWatch.ElapsedMilliseconds;
				lock (_ahFramesBuffersBinds)
				{
					oFrame = _ahFramesBuffersBinds.FirstOrDefault(o => o.Value == completedFrame).Key;
					if (null != oFrame && _aCurrentFramesIDs.ContainsKey(oFrame.nID))
					{
						(new Logger("DeckLink", sName)).WriteDebug4("frame " + oFrame.nID + " was passed out " + (nNow - _aCurrentFramesIDs[oFrame.nID]) + " ms");  
						_aCurrentFramesIDs.Remove(oFrame.nID);
                        if (!oFrame.ahVancDataLine_Bytes.IsNullOrEmpty())
                        {
                            IntPtr pData;
                            IDeckLinkVideoFrameAncillary cVFA;
                            completedFrame.GetAncillaryData(out cVFA);
                            if (cVFA != null)
                            {
                                foreach (uint nLine in oFrame.ahVancDataLine_Bytes.Keys)
                                {
                                    cVFA.GetBufferForVerticalBlankingLine(nLine, out pData);
                                    VancData.Clear(_aTmpUint);
                                    VancData.CopyUintArrayToPointer(_aTmpUint, pData, _aTmpUint.Length);

                                    _cBinM.BytesBack(oFrame.ahVancDataLine_Bytes[nLine], 61);
                                }
                                oFrame.ahVancDataLine_Bytes = null;
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(cVFA);
                            }
                        }
                    }
					else if (0 < oFrame.nID && _cVideoFrameEmpty.nID != oFrame.nID)
						(new Logger("DeckLink", sName)).WriteDebug("VERY STRANGE - 1   error  [id=" + oFrame.nID + "]");

					foreach (long nVFid in _aCurrentFramesIDs.Keys.Where(o => (Preferences.nQueueDeviceLength * 40 + 500) < nNow - _aCurrentFramesIDs[o]))
					{
						(new Logger("DeckLink", sName)).WriteDebug2("There are some over timed frames in decklink buffer: \n");
						(new Logger("DeckLink", sName)).WriteDebug2("\t\t[id=" + nVFid + "][delta=" + _aCurrentFramesIDs[nVFid] + "]\n");
					}
				}

				if (null == oFrame)
					(new Logger("DeckLink", sName)).WriteWarning("frame is not in _ahFramesBuffersBinds");
				else
				{
					FrameBufferReleased(oFrame);
				}

				long nDelta = _cStopWatch.ElapsedMilliseconds - _nLastScTimeComplited;
				if (1 > nDelta) // 100 < nDelta || 
					(new Logger("DeckLink", sName)).WriteDebug2("Last ScheduledFrameCompleted was " + nDelta + " ms ago");
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
				(new Logger("DeckLink", sName)).WriteError(ex);
			}
		}



		void IDeckLinkVideoOutputCallback.ScheduledPlaybackHasStopped()
		{
			(new Logger("DeckLink", sName)).WriteNotice("playback stopped");
		}

        string sS = ".";
        ulong nIndex = 0; 
		void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame iVideoInputFrame, IDeckLinkAudioInputPacket iAudioPacket)
		{
            try
            {
                if (iVideoInputFrame == null)
                {
                    (new Logger("DeckLink", sName)).WriteError("a null frame arrived");
                    return;
                }
                
                if (nIndex++ % 1000 == 0)
                {
                    if (sS == ".") sS = ""; else sS = ".";
                    (new Logger("DeckLink", sName)).WriteNotice($"another 1000 frames got.{sS}");
                }

                if (bLoggingVANCOnInput)
                {
                    IDeckLinkTimecode cTC = null;
                    string sTC = null;
                    iVideoInputFrame.GetTimecode(_BMDTimecodeFormat.bmdTimecodeRP188LTC, out cTC);
                    cTC?.GetString(out sTC);
                    (new Logger("DeckLink", sName)).WriteNotice($"VideoInputFrameArrived: [LTC={sTC}]");
                    iVideoInputFrame.GetTimecode(_BMDTimecodeFormat.bmdTimecodeRP188VITC1, out cTC);
                    cTC?.GetString(out sTC);
                    (new Logger("DeckLink", sName)).WriteNotice($"VideoInputFrameArrived: [VITC={sTC}]");
                    iVideoInputFrame.GetTimecode(_BMDTimecodeFormat.bmdTimecodeRP188VITC2, out cTC);
                    cTC?.GetString(out sTC);
                    (new Logger("DeckLink", sName)).WriteNotice($"VideoInputFrameArrived: [VITC2={sTC}]");

                    IDeckLinkVideoFrameAncillary cVFA;
                    iVideoInputFrame.GetAncillaryData(out cVFA);
                    _BMDPixelFormat ePF = cVFA.GetPixelFormat();
                    _BMDDisplayMode eDM = cVFA.GetDisplayMode();

                    if (ePF != _BMDPixelFormat.bmdFormat10BitYUV)
                    {
                        (new Logger("DeckLink", sName)).WriteError($"FORMAT = [{ePF}][{eDM}] can read VANC data only from {_BMDPixelFormat.bmdFormat10BitYUV} pixel format");
                        (new Logger("DeckLink", sName)).WriteNotice($"Turn off VANC logging");
                        bLoggingVANCOnInput = false;
                    }

                    for (uint nN = 1; nN <= 583; nN++)  //for (uint nN = 1; nN <= 20; nN++)   1-20  561-583  for (uint nN = 1; nN <= 583; nN++)   for (uint nN = 1; nN <= 2; nN++)
                    {
                        //if (nN == 1) nN = 17;  //for (uint nN = 1; nN <= 2; nN++)
                        //if (nN == 2) nN = 580; //for (uint nN = 1; nN <= 2; nN++)
                        if (nN == 21)
                            nN = 561;
                        IntPtr pBuf;
                        try
                        {
                            cVFA.GetBufferForVerticalBlankingLine(nN, out pBuf);
                        }
                        catch (Exception ex)
                        {
                            (new Logger("DeckLink", sName)).WriteError("wrong VANC line " + nN + "  ", ex);
                            continue;
                        }

                        VancData.CopyUintArrayFromPointer(pBuf, _aTmpUint, _aTmpUint.Length);
                        VancData.ReadAndLog(_aTmpUint, null, nN, sName);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(cVFA);
                }

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
                        (new Logger("DeckLink", sName)).WriteWarning("video frame dropped");
                    }
                    if (null != iVideoFrame)
                    {
                        iVideoFrame.GetBytes(out pBytesVideo);
                        if (IntPtr.Zero != pBytesVideo)
                        {
                            nBytesVideoQty = iVideoFrame.GetRowBytes() * iVideoFrame.GetHeight();  //  or _nVideoBytesQty
                            nFramesVideo++;
                        }
                        else
                            (new Logger("DeckLink", sName)).WriteWarning("video frame is empty");
                    }
                    if (null != iAudioPacket)
                    {
                        iAudioPacket.GetBytes(out pBytesAudio);
                        if (IntPtr.Zero != pBytesAudio)
                        {
                            nBytesAudioQty = iAudioPacket.GetSampleFrameCount() * ((int)_eAudioSampleDepth / 8) * _nAudioChannelsQty;  //  or _nAudioBytesQty
                            nFramesAudio++;
                        }
                        else
                            (new Logger("DeckLink", sName)).WriteWarning("audio frame is empty");
                    }
                    else
                    {
                        nFramesDroppedAudio++;
                        (new Logger("DeckLink", sName)).WriteWarning("audio frame dropped");
                    }
                    OnAVFrameArrived(nBytesVideoQty, pBytesVideo, nBytesAudioQty, pBytesAudio);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(iVideoInputFrame);
            }
            catch (Exception ex)
            {
                (new Logger("DeckLink", sName)).WriteError(ex);
            }
        }
		void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
		{
		}
		#endregion
	}
}
