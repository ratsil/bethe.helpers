using System;
using System.Linq;

using System.Runtime.InteropServices;

namespace ffmpeg.net
{
    public class Frame : IEquatable<Frame>
    {
        static private IntPtr NULL = IntPtr.Zero;
        static private int _nIDMax = 1000000000;

        static public object _oSyncRoot = new object();
        static public ulong _nAllocated = 0;
        static public ulong _nDisposed = 0;

        public delegate bool DisposingDelegate(Frame cFrame);
        public event DisposingDelegate Disposing;

        private GCHandle _cGCHandle;
        private IntPtr _pBytes;
        private readonly int _nID;

        private IntPtr _pAVFrame;
        private long _nPTS;
        private byte[] _aBuffer;
        private int _nLength;
        private bool _bDisposed;
        private object _oDisposeLock;

        static private int NextIDGet()
        {
            return System.Threading.Interlocked.Increment(ref _nIDMax);
        }

        public int nID
        {
            get
            {
                return _nID;
            }
        }
        public int nLength
        {
            get
            {
                return _nLength;
            }
            set
            {
                if (value > _aBuffer.Length)
                    throw new Exception("new frame length exceeds a buffer boundaries");
                _nLength = value;
            }
        }
        public int nLengthBuffer
        {
            get
            {
                return aBuffer.Length;
            }
        }
        public IntPtr pBytes
        {
            get
            {
                return _pBytes;
            }
        }
        public byte[] aBuffer
        {
            get
            {
                return _aBuffer;
            }
        }
        public byte[] aBytesCopy
        {
            get
            {
                if(_nLength != nLengthBuffer)
                    return aBuffer.Take(nLength).ToArray();
                return aBuffer.ToArray();
            }
        }
		public void CopyBytesTo(byte[] aBytes)
		{
			Array.Copy(aBuffer, aBytes, _nLength);
        }
        public bool bKeyframe;
        public long nPTS
        {
            get
            {
                if (NULL != _pAVFrame)
                {
                    AVFrame cAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
                    _nPTS = cAVFrame.pts;
                }
                return _nPTS;
            }
            set
            {
                _nPTS = value;
                if (NULL != _pAVFrame)
                {
                    AVFrame cAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
                    cAVFrame.pts = _nPTS;
                    Marshal.StructureToPtr(cAVFrame, _pAVFrame, true);
                }
            }
        }

        public Frame()
        {
            lock(_oSyncRoot)
                _nAllocated++;
            _nID = Frame.NextIDGet();
            _oDisposeLock = new object();
            _aBuffer = null;
            bKeyframe = false;
            _pBytes = NULL;
            _pAVFrame = Functions.av_frame_alloc();
            if (NULL == _pAVFrame)
                throw new Exception("not enough memory for source frame allocate");
        }
        public Frame(Format.Audio cFormat, Frame cFrame)
            : this()
        {
            if (null != cFrame._aBuffer)
                throw new NotImplementedException();
            AVFrame cAVFrame = (AVFrame)Marshal.PtrToStructure(cFrame._pAVFrame, typeof(AVFrame));
            if (0 < cAVFrame.width || 0 < cAVFrame.height || 1 > cAVFrame.nb_samples)
                throw new NotImplementedException();
            int nLineSize = cFormat.nBitsPerSample / 8 * cAVFrame.nb_samples;
			//int nReminder = nLineSize % 64;
			//if(0 < nReminder)
			//	nLineSize += 64 - nReminder;
            _nLength = cFormat.nChannelsQty * nLineSize;
            _aBuffer = new byte[_nLength];
			(new Logger()).WriteDebug4("frame format_frame: - new bytes processed! [size="+ _nLength + "]");
			bool bPlanar = (1 < cAVFrame.data.Count(o => NULL != o));
            if (!bPlanar)
                nLineSize = nLength;
            for (int nIndx = 0; cAVFrame.data.Length > nIndx; nIndx++)
            {
                if (NULL == cAVFrame.data[nIndx])
                    break;
                Marshal.Copy(cAVFrame.data[nIndx], _aBuffer, nIndx * nLineSize, nLineSize);
            }
            Init(cFormat);
        }
        public Frame(Format cFormat)
            : this(cFormat, cFormat.nBufferSize)
        { }
        public Frame(int nLength)
            : this(null, new byte[nLength])
        {
			(new Logger()).WriteDebug4("frame length: - new bytes processed! [size=" + nLength + "]");
		}
        public Frame(byte[] aBytes)
            : this(null, aBytes)
        { }
        public Frame(Format cFormat, int nLength)
            : this(cFormat, new byte[nLength])
        {
			(new Logger()).WriteDebug4("frame format_length: - new bytes processed! [size=" + nLength + "]");
		}
        public Frame(Format cFormat, byte[] aBytes)
            : this()
        {
            _aBuffer = aBytes;
            nLength = aBytes.Length;
            Init(cFormat);
        }

        ~Frame()
        {
            try
            {
                Dispose(); // возврат фреймов - только через диспоз тут!
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
        }
        public void Dispose()  // осторожно!! - много раз вызывается диспоз, т.к. это на самом деле не диспоз, а завуалированный frame_back, т.к. фрейм многоразовый
        {
            lock (_oDisposeLock)
                if (null != Disposing && !Disposing(this))
                    return;

            lock (_oDisposeLock)  // выше не двигать!
            {
                if (_bDisposed)
                    return;
                _bDisposed = true;
            }

            Disposing = null;
            if (NULL != _pAVFrame)
            {
                Functions.av_frame_unref(_pAVFrame);
                Functions.av_frame_free(ref _pAVFrame);
            }
            if (NULL != _pBytes)
            {
                _cGCHandle.Free();
                _pBytes = NULL;
            }
            lock (_oSyncRoot)
                _nDisposed++;
        }
		public void Dispose(bool bForce)
		{
            if (bForce)
			    Disposing = null;
			Dispose();
		}

        public bool Equals(Frame cFrame)
        {
            return null != cFrame && _nID == cFrame.nID;
        }
        public override bool Equals(object oFrame)
        {
            return Equals((Frame)oFrame);
        }
        public override int GetHashCode()
        {
            return nID;
        }


        public void PassSamples(Format.Audio cFormat)
		{
			if (1 > _nLength)
				throw new NotImplementedException();

			AVFrame cAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
			if (0 < cAVFrame.width || 0 < cAVFrame.height || 1 > cAVFrame.nb_samples)
				throw new NotImplementedException();
			int nLineSize = cFormat.nBitsPerSample / 8 * cAVFrame.nb_samples;

			_nLength = cFormat.nChannelsQty * nLineSize;
			if (null == _aBuffer)
			{
				_aBuffer = new byte[_nLength];
				(new Logger()).WriteDebug("passSamples: - new bytes processed! [size=" + _nLength + "]");
			}
			bool bPlanar = (1 < cAVFrame.data.Count(o => NULL != o));
			if (!bPlanar)
				nLineSize = nLength;
			for (int nIndx = 0; cAVFrame.data.Length > nIndx; nIndx++)
			{
				if (NULL == cAVFrame.data[nIndx])
					break;
				Marshal.Copy(cAVFrame.data[nIndx], _aBuffer, nIndx * nLineSize, nLineSize);
			}
		}
		private void Init(Format cFormat)
        {
            _cGCHandle = GCHandle.Alloc(aBuffer, GCHandleType.Pinned);
            _pBytes = _cGCHandle.AddrOfPinnedObject();
            if (null == cFormat)
                return;
            AVFrame cAVFrame;
            if (null != aBuffer)
            {
                int nResult;
                if (cFormat is Format.Video)
                {
                    Format.Video cFormatVideo = (Format.Video)cFormat;
                    //lock (helper._oSyncRootGlobal)
                        if (0 > (nResult = Functions.avpicture_fill(_pAVFrame, aBuffer, cFormatVideo.ePixelFormat, cFormatVideo.nWidth, cFormatVideo.nHeight)))
                            throw new Exception("Frame.AVFrameInit.avpicture_fill = " + nResult);
                    cAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
                    cAVFrame.quality = 1;
                    cAVFrame.pts = 0;
                    Marshal.StructureToPtr(cAVFrame, _pAVFrame, true);
                }
                else
                {
                    Format.Audio cFormatAudio = (Format.Audio)cFormat;
                    cAVFrame = (AVFrame)Marshal.PtrToStructure(_pAVFrame, typeof(AVFrame));
                    if (1 > (cAVFrame.nb_samples = cFormatAudio.stAVCodecContext.frame_size))
                        cAVFrame.nb_samples = aBuffer.Length / ((cFormatAudio.nBitsPerSample / 8) * cFormatAudio.nChannelsQty);
                    cAVFrame.channel_layout = cFormatAudio.stAVCodecContext.channel_layout;
                    cAVFrame.format = (int)cFormatAudio.stAVCodecContext.sample_fmt;
                    Marshal.StructureToPtr(cAVFrame, _pAVFrame, true);
                    //lock (helper._oSyncRootGlobal)
                        if (0 > (nResult = Functions.avcodec_fill_audio_frame(_pAVFrame, cFormatAudio.nChannelsQty, cFormatAudio.eSampleFormat, aBuffer, nLengthBuffer, 1)))
                            throw new Exception("Frame.AVFrameInit.avcodec_fill_audio_frame = " + nResult);
                }
            }
        }

        static public implicit operator IntPtr(Frame cFrame)
        {
            if (null == cFrame)
                return NULL;
            return cFrame._pAVFrame;
        }
    }
}
