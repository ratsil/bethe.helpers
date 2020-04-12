using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;
using AjaNTV2;

using helpers;
using helpers.extensions;

namespace BTL.Device
{
    public class Aja : Device
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
        public enum AudioSampleBitDepth : int
        {
            LittleEndian_16 = 16,
            LittleEndian_20 = 20,
            LittleEndian_24 = 24,
        }

        private AjaInterop.NTV2FrameBufferFormat? _ePixelFormat;
        private AjaInterop.NTV2FrameBufferFormat ePixelFormat
        {
            get
            {
                if (_ePixelFormat == null)
                {
                    bool bFound;
                    _ePixelFormat = Preferences.sPixelsFormat.ToEnumContainingString<AjaInterop.NTV2FrameBufferFormat>(AjaInterop.NTV2FrameBufferFormat.NTV2_FBF_INVALID, out bFound);
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
        private AjaInterop.NTV2VideoFormat? _eVideoFormat;
        private AjaInterop.NTV2VideoFormat eVideoFormat
        {
            get
            {
                if (null == _eVideoFormat)
                {
                    bool bFound;
                    _eVideoFormat = Preferences.sVideoFormat.ToEnumContainingString<AjaInterop.NTV2VideoFormat>(AjaInterop.NTV2VideoFormat.NTV2_FORMAT_UNKNOWN, out bFound);
                    if (!bFound)
                        throw new Exception("incorrect VideoFormat in prefs: [" + Preferences.sVideoFormat + "]"); //TODO LANG
                }
                return _eVideoFormat.Value;
            }
        }
        private AudioSampleBitDepth? _eAudioBitDepth; // conversion is in Baetylus in ig.server !!! here is only 32bits in aja!
        private AudioSampleBitDepth eAudioBitDepth
        {
            get
            {
                if (null == _eAudioBitDepth)
                {
                    bool bFound;
                    _eAudioBitDepth = Preferences.nAudioBitDepth.ToString().ToEnumContainingString<AudioSampleBitDepth>(AudioSampleBitDepth.LittleEndian_16, out bFound);
                    if (!bFound)
                        throw new Exception("incorrect audio sample bit depth in prefs: [" + Preferences.nAudioBitDepth + "]"); //TODO LANG
                }
                return _eAudioBitDepth.Value;
            }
        }
        private AjaInterop.NTV2AudioRate _eAudioSampleRate;
        private AjaInterop.NTV2Channel _eChannel;
        private NTV2Card _cCard;
        private DeviceScanner.NTV2DeviceInfo cInfo;
        private int _nAudioQueueLength;
        private uint _nDeviceIndex;
        private bool _bDoMultichannel;
        private bool _bDoWritingFrames;
        private AjaInterop.NTV2VideoFormat? _eReferenceStatus = null;
        private bool? _bItsOk;
        private uint _nLogCounter2;
        private string _sIterationsCounter2 = ".";
        private Queue<byte[]> _aqWritingFrames;
        private System.Threading.Thread _cThreadWritingFramesWorker;
        protected AudioBuffer _cAudioBuffer;
        private uint _AjaFramesBufferMaxCount;
        private ThreadBufferQueue<Frame.Audio> _AjaFramesAudioBuffer; // 2 frames max
        private ThreadBufferQueue<Frame.Video> _AjaFramesVideoBuffer; // 2 frames max
        private ThreadBufferQueue<Frame.Audio> _AjaFramesAudioToDispose; // 2 frames max
        private ThreadBufferQueue<Frame.Video> _AjaFramesVideoToDispose; // 2 frames max
        override public bool bCardStopped // maybe not stopped, if was started elsewhere...
        {
            get
            {
                (new Logger("Aja", sName)).WriteDebug2("bCardStopped [bChannelStopped=" + _cCard.bChannelStopped + "][bChannelInitiated=" + _cCard.bChannelInitiated + "]");
                return (_cCard.bChannelStopped == null || _cCard.bChannelStopped == true) && (_cCard.bChannelInitiated == null || _cCard.bChannelInitiated == false);
            }
        }

        static public int BoardsQtyGet()
        {
            DeviceScanner cDScanner = new DeviceScanner();
            return (int)cDScanner.GetNumDevices();
        }
        new static public Aja BoardGet(uint nIndex)
        {
            (new Logger("Aja", null)).WriteDebug2("in");
            DeviceScanner cDScanner = new DeviceScanner();
            Aja cRetVal = null;
            uint nDevicesCount = cDScanner.GetNumDevices();
            if (nIndex < nDevicesCount)
                cRetVal = new Aja(cDScanner.GetDeviceInfo(nIndex), nIndex);

            (new Logger("Aja", null)).WriteDebug2("out");
            return cRetVal;
        }

        public Aja(DeviceScanner.NTV2DeviceInfo cDeviceInfo, uint nDeviceIndex)
            :base("Aja-" + nDeviceIndex + "_")
        {
            try
            {
                _nDeviceIndex = nDeviceIndex;
                _nAudioQueueLength = 0;
                _bDoMultichannel = true; // separate channels (if false, all channels provides one signal)
                string sMessage = "aja card [devid=" + cDeviceInfo.stWrapper.deviceID + "][device_id=" + cDeviceInfo.deviceIdentifier + "][multiformat=" + cDeviceInfo.stWrapper.multiFormat + "][ins=" + cDeviceInfo.stWrapper.numVidInputs + "][outs=" + cDeviceInfo.stWrapper.numVidOutputs + "]";
                if (bInput)
                {
                    throw new Exception("aja input mode has not relized yet");
                }
                else
                {

                    _eChannel = (AjaInterop.NTV2Channel)Preferences.nTargetChannel;

                    sMessage += "<br>\tsupported video formats:<br>\t\t" + cDeviceInfo.videoFormatsList.ToEnumerationString("<br>\t\t", "", null, null, true);
                    sMessage += "<br>\tsupported audio Bits Per Sample:<br>\t\t" + cDeviceInfo.audioBitsPerSampleList.ToEnumerationString("<br>\t\t", "", null, null, true);
                    sMessage += "<br>\tsupported audio in sources:<br>\t\t" + cDeviceInfo.audioInSourceList.ToEnumerationString("<br>\t\t", "", null, null, true);
                    sMessage += "<br>\tsupported audio out sources:<br>\t\t" + cDeviceInfo.audioOutSourceList.ToEnumerationString("<br>\t\t", "", null, null, true);
                    sMessage += "<br>\tsupported audio sample rates:<br>\t\t" + cDeviceInfo.audioSampleRateList.ToEnumerationString("<br>\t\t", "", null, null, true);
                    (new Logger("Aja", sName)).WriteNotice(sMessage);

                    if (Preferences.bAudio)
                    {
                        _nAudioChannelsQty = Preferences.nAudioChannelsQty;  // can be only 6, 8, 16
                        if (_nAudioChannelsQty != 6 && _nAudioChannelsQty != 8 && _nAudioChannelsQty != 16)
                            throw new Exception("unsupported audio channels qty (only 6, 8, 16) [" + _nAudioChannelsQty + "]");

                        switch (Preferences.nAudioSamplesRate)
                        {
                            case 48000:
                                _eAudioSampleRate = AjaInterop.NTV2AudioRate.NTV2_AUDIO_48K;
                                break;
                            case 96000:
                                _eAudioSampleRate = AjaInterop.NTV2AudioRate.NTV2_AUDIO_96K;
                                throw new Exception("96000 audio sample rate has not realised yet");
                            default:
                                throw new Exception("unsupported audio sample rate [" + Preferences.nAudioSamplesRate + "]");
                        }
                        //for (int nIndx = 0; _nAudioFrameSize_InBytes > nIndx; nIndx++)
                        //    Marshal.WriteByte(_pAudioSamplesBuffer, nIndx, 0);
                    }

                    _cCard = new NTV2Card(_nDeviceIndex, Preferences.bAudio, _eChannel, ePixelFormat, AjaTools.NTV2ChannelToOutputDestination(_eChannel), eVideoFormat, _eAudioSampleRate, _bDoMultichannel, _nAudioChannelsQty, false, false);
                    _cCard.SetVideoFormat();
                    ushort nW, nH;
                    _cCard.GetActiveFrameDimensions(out nW, out nH);
                    stArea = new Area(0, 0, nW, nH);
                    _nVideoBytesQty = stArea.nWidth * stArea.nHeight * 4;

                    AjaInterop.NTV2FrameRate eFR;
                    switch (eFR = _cCard.GetCurrentFramrRate())
                    {
                        case AjaInterop.NTV2FrameRate.NTV2_FRAMERATE_2500:
                            Preferences.nFPS = 25;
                            break;
                        default:
                            throw new Exception("unsupported frame rate [" + eFR + "][video_format=" + _eVideoFormat + "]");
                    }
                    if (Preferences.bAudio)
                    {
                        _nAudioBufferCapacity_InSamples = Preferences.nAudioSamplesPerFrame * Preferences.nQueueDeviceLength;
                        _nChannelBytesQty = Preferences.nAudioSamplesPerFrame * Preferences.nAudioByteDepth;
                        _nAudioBytesQty = _nAudioChannelsQty * _nChannelBytesQty;
                    }
                    _AjaFramesBufferMaxCount = 3;
                    _AjaFramesVideoBuffer = new ThreadBufferQueue<Frame.Video>(_AjaFramesBufferMaxCount, true, true);
                    _AjaFramesVideoToDispose = new ThreadBufferQueue<Frame.Video>(false, false);
                    if (Preferences.bAudio)
                    {
                        _AjaFramesAudioBuffer = new ThreadBufferQueue<Frame.Audio>(_AjaFramesBufferMaxCount, true, true);
                        _AjaFramesAudioToDispose = new ThreadBufferQueue<Frame.Audio>(false, false);
                    }
                    _bDoWritingFrames = false;
#if DEBUG
                    _aqWritingFrames = new Queue<byte[]>();
                    _cThreadWritingFramesWorker = new System.Threading.Thread(WritingFramesWorker);
                    _cThreadWritingFramesWorker.IsBackground = true;
                    _cThreadWritingFramesWorker.Priority = System.Threading.ThreadPriority.Normal;
                    _cThreadWritingFramesWorker.Start();
#endif

                    if (null != Preferences.cDownStreamKeyer)
                    {
                        throw new Exception("aja keyer has not realised yet");
                    }
                }
            }
            catch (Exception ex)
            {
                (new Logger("Aja", sName)).WriteError(ex);
                throw;
            }
        }
        ~Aja()
        {
            Dispose();
        }
        static object oLockDispose = new object();
        static bool bDisposed = false;
        override public void Dispose()
        {
            lock (oLockDispose)
            {
                if (bDisposed)
                    return;
                bDisposed = true;
            }
            try
            {
                if (null != _cCard)
                    _cCard.Dispose();
                if (null != _aCurrentFramesIDs)
                    _aCurrentFramesIDs.Clear();
            }
            catch (Exception ex)
            {
                (new Logger("Aja", sName)).WriteError(ex);
            }
            try
            {
                if (null != _cAudioBuffer)
                    _cAudioBuffer.Dispose();

                if (_AjaFramesAudioBuffer.nCount > 0)
                    _AjaFramesAudioBuffer.Dequeue().Dispose();
                if (_AjaFramesAudioToDispose.nCount > 0)
                    _AjaFramesAudioToDispose.Dequeue().Dispose();
                if (_AjaFramesVideoBuffer.nCount > 0)
                    FrameBufferReleased(_AjaFramesVideoBuffer.Dequeue()); //TODO - not disposed in device...
                if (_AjaFramesVideoToDispose.nCount > 0)
                    FrameBufferReleased(_AjaFramesVideoToDispose.Dequeue());

                base.Dispose();
            }
            catch (Exception ex)
            {
                (new Logger("Aja", sName)).WriteError(ex);
            }
        }
        override public void TurnOn()
        {
            if (bInput)
            {
                throw new Exception("input mode has not realised yet");
            }
            else
            {
                _cStopWatch = System.Diagnostics.Stopwatch.StartNew();
                DownStreamKeyer();

#if DEBUG
                _cCard.ResetChannel();  
#endif

                if (_cCard.IsDeviceInUse())
                {
                    string sFile = AppDomain.CurrentDomain.BaseDirectory + "force_start_" + _nDeviceIndex + "_" + Preferences.nTargetChannel;

                    if (System.IO.File.Exists(sFile))
                    {
                        System.IO.File.Move(sFile, sFile + "!");
                        _cCard.ResetChannel();
                    }
                    else
                        throw new Exception("the aja card [" + _nDeviceIndex + "] or channel [" + _eChannel + "] is in use. If you are absolutely shure about using this card and this channel - place force_start_" + _nDeviceIndex + "_" + Preferences.nTargetChannel + " file to the binary dir");
                }
                _cCard.DoGetVideoFrame = GetVideoFrame;
                _cCard.DoGetAudioFrame = GetAudioFrame;
                _cCard.Init();
            }
            base.TurnOn();
            if (bInput)
            {
                throw new Exception("output mode has not realised yet");
            }
            else
            {
                _cCard.Run();
            }

            (new Logger("Aja", sName)).WriteNotice("aja turned on");
        }
        override public void DownStreamKeyer()
        {
            // not realized
        }
        override protected Frame.Video FrameBufferPrepare()
        {
            if (_ePixelFormat!= AjaInterop.NTV2FrameBufferFormat.NTV2_FBF_ARGB && _ePixelFormat != AjaInterop.NTV2FrameBufferFormat.NTV2_FBF_ABGR)
            {
                throw new Exception("not supported pixel format [" + _ePixelFormat + "][expected=" + AjaInterop.NTV2FrameBufferFormat.NTV2_FBF_ARGB + "]");
            }
            Frame.Video cRetVal = new Frame.Video(sName);
            cRetVal.oFrameBytes = new Bytes() { aBytes = new byte[_nVideoBytesQty], nID = -1 };  // if problems - look at ntv2player.cc: mVideoBufferSize = GetVideoWriteSize (mVideoFormat, mPixelFormat, mVancEnabled, mWideVanc);
            return cRetVal;
        }

        override protected bool FrameSchedule()
        {
            if (!NextFrameAttached)
                return false;

            int nVideoFramesBuffered = 0;
            Frame.Audio cFrameAudio;
            Frame.Video cFrameVideo;
            _dtLastTimeFrameScheduleCalled = DateTime.Now;

            AjaInterop.NTV2VideoFormat eCurrentRef;

            while (true) // puts frames up to max (_AjaFramesBufferMaxCount)
            {
                eCurrentRef = _cCard.eRefStatus;
                if (null == _eReferenceStatus || _eReferenceStatus != eCurrentRef)
                {
                    _eReferenceStatus = eCurrentRef;
                    (new Logger("Aja", sName)).WriteWarning("Refference status has changed to: [" + eCurrentRef + "] (unknown status means 'no ref')");
                }

                cFrameAudio = AudioFrameGet();
                cFrameVideo = VideoFrameGet();
                #region audio
                if (
                    null == (cFrameAudio)
                    || cFrameAudio.aFrameBytes.IsNullOrEmpty()
                    )
                {
                    (new Logger("Aja", sName)).WriteError("audio frame is empty! [cFrameAudio=" + (cFrameAudio == null ? "NULL" : (null == cFrameAudio.aFrameBytes ? "bytes is NULL" : "" + cFrameAudio.aFrameBytes.Length)) + "]");
                    break;
                }
                #endregion
                #region video
                if (null == (cFrameVideo) || cFrameVideo.aFrameBytes.IsNullOrEmpty())    // _nFPS + _nVideoBufferExtraCapacity < nVideoFramesBuffered
                {
                    (new Logger("Aja", sName)).WriteDebug("got null instead of frame IN DECKLINK !!");
                    break;
                }

                //_cBugCatcherScheduleFrame.Enqueue(cFrameVideo, "FrameSchedule: [_ahFramesBuffersBinds.Count = " + _ahFramesBuffersBinds.Count + "]");

                if (_bDoWritingFrames)
                {
                    if (null != cFrameVideo)
                    {
                        byte[] aBytes = new byte[_nVideoBytesQty];
                        System.Runtime.InteropServices.Marshal.Copy(cFrameVideo.pFrameBytes, aBytes, 0, (int)_nVideoBytesQty);
                        lock (_aqWritingFrames)
                            _aqWritingFrames.Enqueue(aBytes);
                    }
                }
                #endregion

                _AjaFramesAudioBuffer.Enqueue(cFrameAudio);
                _AjaFramesVideoBuffer.Enqueue(cFrameVideo);

                _nAudioQueueLength = (int)_cCard.nBufferLength;
                nVideoFramesBuffered = (int)_cCard.nBufferLength;
                n__PROBA__AudioFramesBuffered = (int)_AjaFramesAudioBuffer.nCount;
                n__PROBA__VideoFramesBuffered = (int)_AjaFramesVideoBuffer.nCount;

                while (_AjaFramesAudioToDispose.nCount > 1) // last frame possible can be in work
                {
                    _AjaFramesAudioToDispose.Dequeue().Dispose();
                }
                while (_AjaFramesVideoToDispose.nCount > 1) // last frame possible can be in work
                {
                    FrameBufferReleased(_AjaFramesVideoToDispose.Dequeue());
                }

                #region logging
                if (
                        Preferences.nQueueDeviceLength - 2 > _nAudioQueueLength
                        || Preferences.nQueueDeviceLength - 2 > nVideoFramesBuffered
                        || Preferences.nQueuePipeLength * 4 / 5 > base._nBufferFrameCount && 0 < base._nBufferFrameCount
                        || _aq__PROBA__AudioFrames.Count > 2 || _aq__PROBA__VideoFrames.Count > 2
                    )
                {
                    if (_bItsOk == true)
                    {
                        (new Logger("Aja", sName)).WriteError("device queue goes wrong-1:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-0");
                        _bItsOk = false;
                        _nLogCounter2 = 0;
                    }
                    else if (_nLogCounter2++ >= 200)
                    {
                        (new Logger("Aja", sName)).WriteError("device queue goes wrong-2:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-" + _nLogCounter2 + _sIterationsCounter2);
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
                        (new Logger("Aja", sName)).WriteError("device queue was wrong:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ") -- logc-" + _nLogCounter2);
                        _bItsOk = true;
                    }

                    if (_nLogCounter2++ >= 2000)
                    {
                        (new Logger("Aja", sName)).WriteNotice("device queue:(" + _nAudioQueueLength + ", " + (nVideoFramesBuffered) + ")(" + n__PROBA__AudioFramesBuffered + ", " + n__PROBA__VideoFramesBuffered + ") dev buffer:" + base._nBufferFrameCount + " internal buffer_av:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")        " + _sIterationsCounter2);
                        _sIterationsCounter2 = _sIterationsCounter2 == "." ? ".." : ".";
                        _nLogCounter2 = 0;
                    }
                }
                #endregion

                if (_AjaFramesAudioBuffer.CountGet() >= _AjaFramesBufferMaxCount)
                    return true;
            }
            return false;
        }
        #region callbacks
        bool bFirsTime = true;
        byte[] GetAudioFrame()
        {
            if (bFirsTime)
                Preferences.nQueueDeviceLength = (byte)(_cCard.nBufferMaxLength - 2);
            _bNeedToAddFrame = true;
            Frame.Audio cA = _AjaFramesAudioBuffer.Dequeue();
            _AjaFramesAudioToDispose.Enqueue(cA);
            return cA.aFrameBytes.aBytes;
        }
        byte[] GetVideoFrame()
        {
            // см внимательно ScheduledFrameCompleted в деклинке!!
            Frame.Video cV = _AjaFramesVideoBuffer.Dequeue();
            _AjaFramesVideoToDispose.Enqueue(cV);
            return cV.aFrameBytes.aBytes;
        }
        #endregion

        // если нужен будет прерол preroll, look here ->  bool CNTV2Card::PrerollAutoCirculate (NTV2Crosspoint channelSpec, ULWord lPrerollFrames)
        private void WritingFramesWorker(object cState)
        {
            // запись кадров перенести в хелпер ото всюду!  
        }
    }
}

