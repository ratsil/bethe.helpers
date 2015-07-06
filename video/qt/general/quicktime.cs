using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;
using System.Drawing;

using System.Security.Permissions;

namespace helpers.video.qt
{
	public class quicktime
	{
		public class File
		{
			private Bitmap _cBitmap;
			private Graphics _cGraphics;
			private IntPtr _pHBMP;
			private IntPtr _pHDC;
			private IntPtr _pFrameBytes;

			private Size _stFrameSize;
            private ulong _nTotalFrames;
			private ushort _nTimeScale;
            private Queue<byte[]> _aqVideoFrames;
            private Queue<byte[]> _aqAudioFrames;

            public ulong nTotalFrames
            {
		        get
		        {
                    if (IntPtr.Zero == pMovie)
                        throw new Exception("File must be opened");
                    if (1 > _nTotalFrames)
                        _nTotalFrames = (ulong)QuickTimeAPI.GetMovieDuration(pMovie);
			        return _nTotalFrames;
		        }
            }
            public ushort nTimeScale
            {
		        get
		        {
                    if (IntPtr.Zero == pMovie)
                        throw new Exception("File must be opened");
					if (1 > _nTimeScale)
						_nTimeScale = (ushort)QuickTimeAPI.GetMovieTimeScale(pMovie);
					return _nTimeScale;
		        }
            }
			public StringBuilder sFile;
			public int nVideoTimeCurrent;
			public int nAudioTimeCurrent;
			public short nFileHandle;
			public int nLastError;
            public QuickTimeAPI.AudioChannelLayout cLayout;
            public QuickTimeAPI.AudioBufferList cABL;

			public IntPtr pFSSpec;
			public IntPtr pMovie;
			public IntPtr pGWorld;
			public IntPtr pMAESession;
            public IntPtr pASBD;

			public IntPtr pHBMP
			{
				get
				{
					if (IntPtr.Zero == _pHBMP)
					{
						_cBitmap = new Bitmap(_stFrameSize.Width, _stFrameSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						_pHBMP = _cBitmap.GetHbitmap();
					}
					return _pHBMP;
				}
			}
			public IntPtr pHDC
			{
				get
				{
					if (IntPtr.Zero == _pHDC)
					{
						_cGraphics = Graphics.FromImage(Image.FromHbitmap(pHBMP));
						_pHDC = _cGraphics.GetHdc();
					}
					return _pHDC;
				}
			}
			public IntPtr pFrameBytes
			{
				get
				{
					if (IntPtr.Zero == _pFrameBytes)
						_pFrameBytes = Marshal.AllocCoTaskMem((int)nBytesPerFrame);
					return _pFrameBytes;
				}
			}
			public uint nBytesPerFrame
			{
				get
				{
					int nRetVal = _stFrameSize.Width * _stFrameSize.Height * 4;
					return (uint)nRetVal;
				}
			}
			public ushort nFrameWidth
			{
				get
				{
					return (ushort)_stFrameSize.Width;
				}
			}
			public ushort nFrameHeight
			{
				get
				{
					return (ushort)_stFrameSize.Height;
				}
			}

			public File(string sFile)
			{
				_pHBMP = IntPtr.Zero;
				_pHDC = IntPtr.Zero;
				_pFrameBytes = IntPtr.Zero;
				_stFrameSize = new Size(720, 576);
				_nTotalFrames = 0;
				_nTimeScale = 0;

				this.sFile = new StringBuilder(sFile);
				QuickTimeAPI.c2pstr(this.sFile); // Convert to Pascal string  
				nFileHandle = 0;

				pFSSpec = IntPtr.Zero;
				pMovie = IntPtr.Zero;
				pGWorld = IntPtr.Zero;
				pMAESession = IntPtr.Zero;
                pASBD = IntPtr.Zero;

                cABL = null;

                _aqVideoFrames = new Queue<byte[]>();
                _aqAudioFrames = new Queue<byte[]>();
			}
			~File()
			{
				try
				{
                    if (IntPtr.Zero != pASBD)
                        Marshal.FreeCoTaskMem(pASBD);
				}
				catch { }
				try
				{
					if (null != _cGraphics && IntPtr.Zero != _pHDC)
						_cGraphics.ReleaseHdc(_pHDC);
				}
				catch { }
				try
				{
					if (0 != nFileHandle)
						QuickTimeAPI.CloseMovieFile(nFileHandle);
				}
				catch { }
				try
				{
					if (IntPtr.Zero != pGWorld)
						QuickTimeAPI.DisposeGWorld(pGWorld);
				}
				catch { }
				try
				{
					if (IntPtr.Zero != pFSSpec)
						Marshal.FreeCoTaskMem(pFSSpec);
				}
				catch { }
				try
				{
					if (IntPtr.Zero != _pFrameBytes)
                        Marshal.FreeCoTaskMem(_pFrameBytes);
				}
				catch { }
			}
		}
		static bool _bInit = false;
        static public object _cSyncRoot;
        public object cSyncRoot
        {
            get
            {
                if(null == _cSyncRoot)
                    _cSyncRoot = new object();
                return new object();// _cSyncRoot;
            }
        }
        public quicktime()
		{
			if (!_bInit)
			{
				if (QuickTimeAPI.noErr != QuickTimeAPI.InitializeQTML(0))
					throw new Exception("InitializeQTML error");//TODO LANG
				if (QuickTimeAPI.noErr != QuickTimeAPI.EnterMovies())
					throw new Exception("EnterMovies error");//TODO LANG
				_bInit = true;
			}
		}
		~quicktime()
		{
			try
			{
				if (_bInit)
				{
					try
					{
						QuickTimeAPI.ExitMovies();
					}
					catch { }
					try
					{
						QuickTimeAPI.TerminateQTML();
					}
					catch { }
				}
			}
			catch { }
		}

		public File FileOpen(string sFile)
		{
            File cRetVal = null;
            try
            {
                cRetVal = new File(sFile);
                cRetVal.pFSSpec = Marshal.AllocCoTaskMem(266);
                if (QuickTimeAPI.noErr != QuickTimeAPI.FSMakeFSSpec(IntPtr.Zero, IntPtr.Zero, cRetVal.sFile, cRetVal.pFSSpec))
                    throw new Exception("FSMakeFSSpec error");//TODO LANG
                if (QuickTimeAPI.noErr != QuickTimeAPI.OpenMovieFile(cRetVal.pFSSpec, ref cRetVal.nFileHandle, 1))
                    throw new Exception("OpenMovieFile error");//TODO LANG
                if (QuickTimeAPI.noErr != QuickTimeAPI.NewMovieFromFile(out cRetVal.pMovie, cRetVal.nFileHandle, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero))
                    throw new Exception("NewMovieFromFile error");//TODO LANG
            }
            catch
            {
                cRetVal = null;
            }
			return cRetVal;
		}
		public Queue<byte[]> VideoFramesGet(File cFile, int nFrameStart, int nFramesQty)
		{
            Queue<byte[]> aqRetVal = null;
            try
            {
                cFile.nLastError = 0;
                if (IntPtr.Zero == cFile.pGWorld)
                {
                    //if (QuickTimeAPI.noErr != QuickTimeAPI.NewGWorldFromHBITMAP(out cFile.pGWorld, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, cFile.pHBMP, cFile.pHDC))
                    IntPtr pRect = Marshal.AllocCoTaskMem(8);
                    QuickTimeAPI.MacSetRect(pRect, 0, 0, (short)cFile.nFrameWidth, (short)cFile.nFrameHeight);
                    if (QuickTimeAPI.noErr != QuickTimeAPI.GetMoviesError())
                        throw new Exception("MacSetRect error");//TODO LANG
                    if (QuickTimeAPI.noErr != QuickTimeAPI.NewGWorldFromPtr(out cFile.pGWorld, QuickTimeAPI.k32BGRAPixelFormat, pRect, IntPtr.Zero, IntPtr.Zero, 0, cFile.pFrameBytes, (int)(cFile.nBytesPerFrame / cFile.nFrameHeight)))
                        throw new Exception("NewGWorldFromPtr error");//TODO LANG
                    QuickTimeAPI.SetMovieGWorld(cFile.pMovie, cFile.pGWorld, IntPtr.Zero);
                    if (QuickTimeAPI.noErr != QuickTimeAPI.GetMoviesError())
                        throw new Exception("SetMovieGWorld error");//TODO LANG
                    Marshal.FreeCoTaskMem(pRect);
                }
                short nFlags = (short)(QuickTimeAPI.nextTimeMediaSample | QuickTimeAPI.nextTimeEdgeOK);
                uint[] aTypes = new uint[1];
                aTypes[0] = QuickTimeAPI.VideoMediaType;
                int nMovieNextTime = 0, nDuration = 0, nFramesGrabbed = 0;

                if (0 > nFrameStart)
                {
                    if (0 < cFile.nVideoTimeCurrent)
                    {
                        nFrameStart = cFile.nVideoTimeCurrent - 1;
                        nFlags = (short)QuickTimeAPI.nextTimeMediaSample;
                    }
                    else
                        nFrameStart = 0;

                }
                else if (0 < nFrameStart)
                {
                    nFrameStart--;
                    nFlags = (short)QuickTimeAPI.nextTimeMediaSample;
                }

                QuickTimeAPI.SetMovieTimeValue(cFile.pMovie, nFrameStart);
                byte[] aFrameBytes;
                aqRetVal = new Queue<byte[]>();
                while (nFramesQty > nFramesGrabbed)
                {
                    QuickTimeAPI.GetMovieNextInterestingTime(cFile.pMovie, nFlags, (short)aTypes.Length, aTypes, nFrameStart, 1, out nMovieNextTime, out nDuration);
                    cFile.nLastError = QuickTimeAPI.GetMoviesError();
                    if (QuickTimeAPI.noErr != cFile.nLastError)
                        break;
                    QuickTimeAPI.SetMovieTimeValue(cFile.pMovie, nMovieNextTime);
                    cFile.nLastError = QuickTimeAPI.GetMoviesError();
                    if (QuickTimeAPI.noErr != cFile.nLastError)
                        break;
                    nFrameStart = nMovieNextTime;
                    QuickTimeAPI.MoviesTask(cFile.pMovie, 0);
                    aFrameBytes = new byte[cFile.nBytesPerFrame];
                    Marshal.Copy(cFile.pFrameBytes, aFrameBytes, 0, aFrameBytes.Length);
                    aqRetVal.Enqueue(aFrameBytes);
                    nFramesGrabbed++;
                    nFlags = QuickTimeAPI.nextTimeMediaSample;
                }
                if (-1 < nFrameStart)
                    cFile.nVideoTimeCurrent = nFrameStart + 1;
            }
            catch
            {
                cFile.nLastError = 1;
            }
			return aqRetVal;
		}
        public Queue<byte[]> VideoFramesGet(File cFile, int nFramesQty)
        {
            return VideoFramesGet(cFile, -1, nFramesQty);
        }
        public Queue<byte[]> AudioSamplesGet(File cFile, int nFrameStart, int nFramesQty)
        {
			Queue<byte[]> aqRetVal = null;
			ulong nSamplingRate = 48000;
			ushort nChannelsQty = 2;
			ushort nBitsPerChannel = 16;
			ulong nSamplesActual;

            try
            {
                if (0 > nFrameStart)
                    nFrameStart = cFile.nAudioTimeCurrent;
                cFile.nLastError = QuickTimeAPI.noErr;
				

				//char tmp = IntPtr.Zero;

				// Note: the "+1" makes it, of course, an (not necessarily strict) upper bound.
				nSamplesActual = (cFile.nTotalFrames * nSamplingRate) / cFile.nTimeScale + 1;
                UInt32 nFlags, nSamplesToFill = 1920, nSamplesFilled = 0;

                if (IntPtr.Zero == cFile.pMAESession)
                {
                    cFile.nLastError = QuickTimeAPI.MovieAudioExtractionBegin(cFile.pMovie, 0, out cFile.pMAESession);
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception("MovieAudioExtractionBegin error");
                    IntPtr pAllChannelsDiscrete = IntPtr.Zero;
                    try
                    {
                        pAllChannelsDiscrete = Marshal.AllocCoTaskMem(1);
                        Marshal.WriteByte(pAllChannelsDiscrete, 1);
                        // disable mixing of audio channels
                        cFile.nLastError = QuickTimeAPI.MovieAudioExtractionSetProperty(cFile.pMAESession, QuickTimeAPI.kQTPropertyClass_MovieAudioExtraction_Movie, QuickTimeAPI.kQTMovieAudioExtractionMoviePropertyID_AllChannelsDiscrete, 1, pAllChannelsDiscrete);
                    }
                    finally
                    {
                        if (IntPtr.Zero != pAllChannelsDiscrete)
                            Marshal.FreeCoTaskMem(pAllChannelsDiscrete);
                    }
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception("MovieAudioExtractionSetProperty for disable channels mixing error");

                    uint nASBDSize = (uint)Marshal.SizeOf(typeof(QuickTimeAPI.AudioStreamBasicDescription));
                    cFile.pASBD = Marshal.AllocCoTaskMem((int)nASBDSize);

                    cFile.nLastError = QuickTimeAPI.MovieAudioExtractionGetProperty(cFile.pMAESession, QuickTimeAPI.kQTPropertyClass_MovieAudioExtraction_Audio, QuickTimeAPI.kQTMovieAudioExtractionAudioPropertyID_AudioStreamBasicDescription, nASBDSize, cFile.pASBD, IntPtr.Zero);
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception();

                    QuickTimeAPI.AudioStreamBasicDescription stASBD = (QuickTimeAPI.AudioStreamBasicDescription)Marshal.PtrToStructure(cFile.pASBD, typeof(QuickTimeAPI.AudioStreamBasicDescription));
                    // convert the ASBD to return noninterleaved PCM instead of non-interleaved Float32:
                    stASBD.mFormatFlags = QuickTimeAPI.kAudioFormatFlagIsSignedInteger | QuickTimeAPI.kAudioFormatFlagIsPacked | QuickTimeAPI.kAudioFormatFlagsNativeEndian;
                    stASBD.mBitsPerChannel = (uint)nBitsPerChannel;
                    stASBD.mChannelsPerFrame = (uint)nChannelsQty;
                    stASBD.mBytesPerFrame = (uint)(nBitsPerChannel / 8) * stASBD.mChannelsPerFrame;
                    stASBD.mBytesPerPacket = stASBD.mBytesPerFrame;
                    stASBD.mSampleRate = nSamplingRate;
                    //stASBD.mFramesPerPacket = 1;
                    Marshal.StructureToPtr(stASBD, cFile.pASBD, true);

                    // set the new audio extraction ASBD:
                    cFile.nLastError = QuickTimeAPI.MovieAudioExtractionSetProperty(cFile.pMAESession, QuickTimeAPI.kQTPropertyClass_MovieAudioExtraction_Audio, QuickTimeAPI.kQTMovieAudioExtractionAudioPropertyID_AudioStreamBasicDescription, nASBDSize, cFile.pASBD);
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception("AudioStreamBasicDescription processing error");

                    cFile.cLayout = new QuickTimeAPI.AudioChannelLayout();
                    cFile.cLayout.mChannelLayoutTag = QuickTimeAPI.kAudioChannelLayoutTag_Stereo;
                    cFile.cLayout.mNumberChannelDescriptions = 2;
                    cFile.cLayout.mChannelDescriptions = new QuickTimeAPI.AudioChannelDescription[cFile.cLayout.mNumberChannelDescriptions];
                    cFile.cLayout.mChannelDescriptions[0] = new QuickTimeAPI.AudioChannelDescription();
                    cFile.cLayout.mChannelDescriptions[0].mChannelLabel = QuickTimeAPI.kAudioChannelLabel_Left;
                    cFile.cLayout.mChannelDescriptions[1] = new QuickTimeAPI.AudioChannelDescription();
                    cFile.cLayout.mChannelDescriptions[1].mChannelLabel = QuickTimeAPI.kAudioChannelLabel_Right;
                    cFile.nLastError = QuickTimeAPI.MovieAudioExtractionSetProperty(cFile.pMAESession, QuickTimeAPI.kQTPropertyClass_MovieAudioExtraction_Audio, QuickTimeAPI.kQTMovieAudioExtractionAudioPropertyID_AudioChannelLayout, cFile.cLayout.nSize, cFile.cLayout.pLayout);
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception("MovieAudioExtractionSetProperty error");
                    IntPtr pTimeRec = IntPtr.Zero;
                    try
                    {
                        QuickTimeAPI.TimeRecord stTimeRec;
                        uint nTimeRecordSize = (uint)Marshal.SizeOf(typeof(QuickTimeAPI.TimeRecord));
                        stTimeRec = new QuickTimeAPI.TimeRecord();
                        stTimeRec.scale = (int)cFile.nTimeScale;
                        stTimeRec.pBase = IntPtr.Zero;
                        stTimeRec.value.hi = 0;
                        stTimeRec.value.lo = (uint)(nFrameStart * stTimeRec.scale); // for instance, to start at time 1:00.00
                        pTimeRec = Marshal.AllocCoTaskMem((int)nTimeRecordSize);
                        Marshal.StructureToPtr(stTimeRec, pTimeRec, true);
                        cFile.nLastError = QuickTimeAPI.MovieAudioExtractionSetProperty(cFile.pMAESession, QuickTimeAPI.kQTPropertyClass_MovieAudioExtraction_Movie, QuickTimeAPI.kQTMovieAudioExtractionMoviePropertyID_CurrentTime, nTimeRecordSize, pTimeRec);
                    }
                    finally
                    {
                        if (IntPtr.Zero != pTimeRec)
                            Marshal.FreeCoTaskMem(pTimeRec);
                    }
                    if (cFile.nLastError != QuickTimeAPI.noErr)
                        throw new Exception("MovieAudioExtractionFillBuffer error");
                    cFile.cABL = new QuickTimeAPI.AudioBufferList();
                    cFile.cABL.mNumberBuffers = 1;
                    cFile.cABL.mBuffers = new QuickTimeAPI.AudioBuffer[cFile.cABL.mNumberBuffers];
                    cFile.cABL.mBuffers[0] = new QuickTimeAPI.AudioBuffer();
                    cFile.cABL.mBuffers[0].mNumberChannels = 2;
                    cFile.cABL.mBuffers[0].mDataByteSize = (uint)(nSamplesToFill * nChannelsQty * (nBitsPerChannel / 8));//((((int)nSamplingRate * (nBitsPerChannel / 8) * nChannelsQty) / cFile.nTimeScale) * nFramesQty);
                }

				nSamplesActual = 0;


				//AudioBufferList bflst = { 1, { { 2, 1920 * 2 * 2, malloc(1920 * 2 * 2) } } };
				aqRetVal = new Queue<byte[]>();
				byte[] aSamples = null;
                for (int nIndx = 0; nFramesQty > nIndx; nIndx++)
				{
					nSamplesFilled = nSamplesToFill;
					cFile.nLastError = QuickTimeAPI.MovieAudioExtractionFillBuffer(cFile.pMAESession, ref nSamplesFilled, cFile.cABL.pABL, out nFlags);
					if (cFile.nLastError != QuickTimeAPI.noErr)
						throw new Exception("MovieAudioExtractionFillBuffer error");
                    if (0 < nSamplesFilled)
                    {
                        nSamplesActual += nSamplesFilled;
                        aSamples = new byte[nSamplesFilled * nChannelsQty * (nBitsPerChannel / 8)];
                        Marshal.Copy(cFile.cABL.mBuffers[0].mData, aSamples, 0, aSamples.Length);
                        aqRetVal.Enqueue(aSamples);
                    }
                    if (0 < (nFlags & QuickTimeAPI.kQTMovieAudioExtractionComplete))
                    {
                        QuickTimeAPI.MovieAudioExtractionEnd(cFile.pMAESession);
                        break;
                    }
				}
                if (cFile.nLastError != QuickTimeAPI.noErr)
                    throw new Exception("MovieAudioExtractionFillBuffer error");
                cFile.nAudioTimeCurrent += (int)(nSamplesActual / nSamplingRate * cFile.nTimeScale);
			}
            catch
            {
                if(QuickTimeAPI.noErr == cFile.nLastError)
                    cFile.nLastError = 1;
            }
			return aqRetVal;
		}
        public Queue<byte[]> AudioSamplesGet(File cFile, int nFramesQty)
        {
            return AudioSamplesGet(cFile, -1, nFramesQty);
        }
		public void FileClose(ref File cFile)
		{
			cFile = null;
			GC.Collect();
		}
		/*
        public Queue<byte[]> AudioSamplesGet(File cFile, int nFrameStart, int nFramesQty)
        {
			Queue<byte[]> aqRetVal = null;
            /* Handle *//*
            IntPtr soundData = IntPtr.Zero;
            /* ComponentInstance *//*IntPtr soundComp = IntPtr.Zero;
            /*SoundDescriptionHandle*//*IntPtr inDesc = IntPtr.Zero;
            /*SoundDescriptionHandle*//*IntPtr outDesc = IntPtr.Zero;
            try
            {
                cFile.nLastError = 0;
                if (0 > nFrameStart)
                    nFrameStart = cFile.nAudioTimeCurrent;

                IntPtr srcMovie = cFile.pMovie;
                cFile.nLastError = QuickTimeAPI.noErr;
                soundData = QuickTimeAPI.NewHandle(0);
                soundComp = IntPtr.Zero;
                /*Track*//*IntPtr soundTrack = QuickTimeAPI.GetMovieIndTrackType(srcMovie, 1, QuickTimeAPI.SoundMediaType, QuickTimeAPI.movieTrackMediaType | QuickTimeAPI.movieTrackEnabledOnly);
                if (IntPtr.Zero == soundTrack)
                    throw new Exception("Can't find soundtrack");

                soundComp = QuickTimeAPI.OpenDefaultComponent(QuickTimeAPI.FOUR_CHAR_CODE("spit"), QuickTimeAPI.FOUR_CHAR_CODE("snd "));
                QuickTimeAPI.SoundDescription stInDesc = new QuickTimeAPI.SoundDescription();
                QuickTimeAPI.SoundDescription stOutDesc = new QuickTimeAPI.SoundDescription();
                inDesc = (IntPtr)QuickTimeAPI.NewHandle(0);
                outDesc = QuickTimeAPI.NewHandleClear(Marshal.SizeOf(typeof(QuickTimeAPI.SoundDescription)));

                QuickTimeAPI.GetMediaSampleDescription(QuickTimeAPI.GetTrackMedia(soundTrack), 1, inDesc);

                stInDesc = (QuickTimeAPI.SoundDescription)Marshal.PtrToStructure(Marshal.ReadIntPtr(inDesc), typeof(QuickTimeAPI.SoundDescription));

                stOutDesc.descSize = Marshal.SizeOf(typeof(QuickTimeAPI.SoundDescription));
                stOutDesc.dataFormat = (int)QuickTimeAPI.k8BitOffsetBinaryFormat;
                stOutDesc.numChannels = 2;
                stOutDesc.sampleSize = 16;
                stOutDesc.sampleRate = stInDesc.sampleRate;

                Marshal.StructureToPtr(stOutDesc, Marshal.ReadIntPtr(outDesc), true);
                QuickTimeAPI.MovieExportSetSampleDescription(soundComp, outDesc, QuickTimeAPI.FOUR_CHAR_CODE("soun"));
                //cFile.nLastError = QuickTimeAPI.GetMoviesError();
                while (true)
                {
                    cFile.nLastError = QuickTimeAPI.PutMovieIntoTypedHandle(srcMovie, IntPtr.Zero, QuickTimeAPI.FOUR_CHAR_CODE("snd "), soundData, nFrameStart, nFramesQty, 0, soundComp);
                    if (-2014 == cFile.nLastError && 0 < nFramesQty)
                        nFramesQty--;
                    else
                        break;
                }
                if (QuickTimeAPI.noErr != cFile.nLastError)
                    throw new Exception();
                QuickTimeAPI.HLock(soundData);
                QuickTimeAPI.SndListResource stSlr = new QuickTimeAPI.SndListResource(Marshal.ReadIntPtr(soundData));

                int nActualSamplesAreaSize = QuickTimeAPI.GetHandleSize(soundData) - (stSlr.dataPart.samplePtr.ToInt32() - Marshal.ReadIntPtr(soundData).ToInt32());

                QuickTimeAPI.HUnlock(soundData);

                uint nFormalSamplesAreaSize = 0;
                switch (stSlr.enSoundHeaderType)
                {
                    case QuickTimeAPI.SoundHeaderType.Std:
                        nFormalSamplesAreaSize = stSlr.dataPart.length;
                        break;
                    case QuickTimeAPI.SoundHeaderType.Ext:
                        QuickTimeAPI.ExtSoundHeader cESH = (QuickTimeAPI.ExtSoundHeader)stSlr.dataPart;
                        nFormalSamplesAreaSize = (uint)(cESH.numFrames * cESH.numChannels * (cESH.sampleSize / 8));
                        break;
                    case QuickTimeAPI.SoundHeaderType.Cmp:
                        QuickTimeAPI.CmpSoundHeader cCSH = (QuickTimeAPI.CmpSoundHeader)stSlr.dataPart;
                        nFormalSamplesAreaSize = (uint)(cCSH.numFrames * cCSH.numChannels * (cCSH.sampleSize / 8));
                        break;
                }

                if (nActualSamplesAreaSize != nFormalSamplesAreaSize)
                    throw new Exception("parsed SoundHandle Wrong");
                cFile.nAudioTimeCurrent = (int)(nFrameStart + nFramesQty);

                if (null != stSlr.dataPart.sampleArea)
                {
                    uint nSampleSize = (uint)(stSlr.dataPart.sampleArea.Length / nFramesQty);
                    aqRetVal = new Queue<byte[]>();
                    byte[] aSample = null;
                    for (uint nSampleOffset = 0; stSlr.dataPart.sampleArea.Length > nSampleOffset; nSampleOffset += nSampleSize)
                    {
                        aSample = new byte[nSampleSize];
                        Array.Copy(stSlr.dataPart.sampleArea, nSampleOffset, aSample, 0, nSampleSize);
                        aqRetVal.Enqueue(aSample);
                    }
                }
            }
            catch
            {
                if(QuickTimeAPI.noErr == cFile.nLastError)
                    cFile.nLastError = 1;
            }
            if (IntPtr.Zero != soundData)
                QuickTimeAPI.DisposeHandle(soundData);
            if (IntPtr.Zero != outDesc)
                QuickTimeAPI.DisposeHandle(outDesc);
            if (IntPtr.Zero != inDesc)
                QuickTimeAPI.DisposeHandle(inDesc);
            if (IntPtr.Zero != soundComp)
                QuickTimeAPI.CloseComponent(soundComp);
            return aqRetVal;
		}
		*/
	}
}