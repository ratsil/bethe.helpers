using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using helpers.video.qt.atoms;

namespace controls.video.preview.sl
{
	public class MediaStreamSource : System.Windows.Media.MediaStreamSource
    {
		public void log(string sMessage)
		{
			if(sMessage.StartsWith("lock"))
				return;
			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(1);
			sMessage = sf.GetMethod().Name + "::" + sMessage;
			Debug.WriteLine(sMessage);
		}

		class FrameOffset
		{
			public int nIndxAudio;
			public int nIndxVideo;

			public FrameOffset(int nIndxAudio, int nIndxVideo)
			{
				this.nIndxAudio = nIndxAudio;
				this.nIndxVideo = nIndxVideo;
			}
		}
		class NALUnit
		{
			public bool bFrameStart;
			public long nStart;
			public long nBytesQty;
		}
		class MediaStream : MemoryStream
		{
			private object _cSyncRoot;
			private Stream _cStreamSource;
			private long _nBytesBuffered;
			private long _nCache;

			override public long Position
			{
				get
				{
					lock(_cSyncRoot)
						return base.Position;
				}
				set
				{
					lock (_cSyncRoot)
					{
						long nCache = value - _nBytesBuffered;
						if (0 <= nCache)
							Cache(nCache + 1024);
						base.Position = value;
					}
				}
			}
			public long nBytesBuffered
			{
				get
				{
					lock(_cSyncRoot)
						return _nBytesBuffered;
				}
			}

			public MediaStream(Stream cStream)
				: base()
			{
				_cSyncRoot = new object();
				_cStreamSource = cStream;
				_nBytesBuffered = _nCache = 0;
			}

			override public int ReadByte()
			{
				bool bCache = false;
				long nPosition;
				lock (_cSyncRoot)
					bCache = ((nPosition = Position) == _nBytesBuffered);
				if (bCache)
					Cache(1024);
				lock (_cSyncRoot)
				{
					Position = nPosition;
					return base.ReadByte();
				}
			}
			override public int Read(byte[] buffer, int offset, int count)
			{
				long nCache, nPosition;
				lock (_cSyncRoot)
					nCache = (nPosition = Position) + count - _nBytesBuffered;
				if (0 <= nCache)
					Cache(nCache + 1024);
				lock (_cSyncRoot)
				{
					Position = nPosition;
					return base.Read(buffer, offset, count);
				}
			}

			public void Cache(long nQty)
			{
				lock (_cSyncRoot)
				{
					if (0 < _nCache)
					{
						if (_nCache < nQty)
							_nCache = nQty;
						return;
					}
					_nCache = nQty;
				}

				byte[] aBuffer;
				int nOffset;
				long nStreamPosition;
				while(true)
				{
					aBuffer = new byte[nQty];
					nOffset = 0;
					while (nOffset < nQty)
						nOffset += _cStreamSource.Read(aBuffer, nOffset, aBuffer.Length - nOffset);
					//System.Threading.Thread.Sleep(10);
					lock (_cSyncRoot)
					{
						nStreamPosition = Position;
						Position = _nBytesBuffered;
						Write(aBuffer, 0, aBuffer.Length);
						_nBytesBuffered += aBuffer.Length;
						Position = nStreamPosition;
						if (_nCache > aBuffer.Length)
						{
							if (1024 > (_nCache -= aBuffer.Length))
								_nCache = 1024;
						}
						else
						{
							_nCache = 0;
							break;
						}
					}
				}
			}
			public byte[] Read(long nStart, long nQty)
			{
				lock (_cSyncRoot)
				{
					if (nStart + nQty > _nBytesBuffered)
						return null;
					byte[] aRetVal = new byte[nQty];
					long nStreamPosition = Position;
					Position = nStart;
					Read(aRetVal, 0, aRetVal.Length);
					Position = nStreamPosition;
					return aRetVal;
				}
			}
		}
		private ulong _nFramesQty;
		private long _nFramesBuffered;
		private bool _bAudioFrameNeeded, _bVideoFrameNeeded;
		private bool _bCached;

		private double _nFrameDuration, _nSampleDuration;

		private const int _nAudioBitsPerSample = 16;
		private const int _nAudioChannels = 2;
		private int _nAudioSampleRate = 48000;
		private int _nAudioBitRate = 128000;
		private int _nAudioMP3ControlByte = 0x94;

		private const int _nVideoFrameWidth = 288;
		private const int _nVideoFrameHeight = 230;
		private int _nCurrentFrameVideo, _nFrameOffsetVideo = 0;
		private int _nCurrentFrameAudio, _nFrameOffsetAudio = 0;
		private long _nBufferSeconds = 5;

		private string _sUri;
		private HttpWebRequest _cHttpWebRequest;
		private MediaStream _cStream;
		List<NALUnit> _aVideoNALs = new List<NALUnit>();
		List<NALUnit> _aAudioNALs = new List<NALUnit>();
		List<FrameOffset> _aFramesOffsets = new List<FrameOffset>();

		private long nDataStart, nDataSize;
		private Dictionary<MediaSourceAttributesKeys, string> _ahMediaSourceAttributes;
		private List<MediaStreamDescription> _aMediaStreamDescriptions;
		private MpegLayer3WaveFormat _cMP3WaveFormat;
		private System.Threading.Thread _cThread;
		private object _cSyncRoot;

		public ulong nFramesQty
		{
			get
			{
				lock(_cSyncRoot)
					return _nFramesQty;
			}
		}
		public long nFramesBuffered
		{
			get
			{
				lock(_cSyncRoot)
					return _nFramesBuffered;
			}
		}

		public MediaStreamSource(string sUri)
			: this(sUri, 0)
		{ }
		public MediaStreamSource(string sUri, ulong nFramesQty)
        {
			//aLog = new List<string>();
			_cSyncRoot = new object();
			_sUri = sUri;
			_nFramesQty = nFramesQty;
			_bAudioFrameNeeded = false;
			_bCached = false;
		}

		private MediaStreamDescription _cMediaStreamVideoDescription, _cMediaStreamAudioDescription;
		protected override void OpenMediaAsync()
        {
			try
			{
				_cHttpWebRequest = HttpWebRequest.CreateHttp(_sUri);
				_cHttpWebRequest.BeginGetResponse(ResponseCallback, null);
            }
			catch { }
        }
		private void ResponseCallback(IAsyncResult iAsynchronousResult)
		{
			try
			{
				_ahMediaSourceAttributes = null;
				_aMediaStreamDescriptions = null;
				_cMP3WaveFormat = null;
				_cStream = new MediaStream(((HttpWebResponse)_cHttpWebRequest.EndGetResponse(iAsynchronousResult)).GetResponseStream());

				_cStream.Position = 0;
				// Initialize data structures to pass to the Media pipeline via the MediaStreamSource
				_ahMediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
				_aMediaStreamDescriptions = new List<MediaStreamDescription>();

				Dictionary<MediaStreamAttributeKeys, string> ahMediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

				ahMediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "H264";
				ahMediaStreamAttributes[MediaStreamAttributeKeys.Width] = _nVideoFrameWidth.ToString();
				ahMediaStreamAttributes[MediaStreamAttributeKeys.Height] = _nVideoFrameHeight.ToString();
				_nCurrentFrameVideo = 0;
				_nCurrentFrameAudio = 0;

				_cMediaStreamVideoDescription = new MediaStreamDescription(MediaStreamType.Video, ahMediaStreamAttributes);
				_aMediaStreamDescriptions.Add(_cMediaStreamVideoDescription);

				FileTypeCompatibility cFtyp = (FileTypeCompatibility)Atom.Read(_cStream);
				Wide cWide = (Wide)cFtyp.cNext;
				MovieData cMdat = (MovieData)cWide.cNext;

				nDataStart = cMdat.DataOffsetGet();
				nDataSize = cMdat.DataSizeGet();

				//1. When the next four bytes in the bitstream form the four-byte sequence 0x00000001, the next byte in the byte
				//stream (which is a zero_byte syntax element) is extracted and discarded and the current position in the byte
				//stream is set equal to the position of the byte following this discarded byte.
				while (true)
				{
					if (0 == _cStream.ReadByte() && 0 == _cStream.ReadByte() && 0 == _cStream.ReadByte() && 1 == _cStream.ReadByte())
					{
						break;
					}
				}

				//2. The next three-byte sequence in the byte stream (which is a start_code_prefix_one_3bytes) is extracted and
				//discarded and the current position in the byte stream is set equal to the position of the byte following this
				//three-byte sequence.


				_cMP3WaveFormat = new MpegLayer3WaveFormat();
				_cMP3WaveFormat.WaveFormatEx = new WaveFormatEx();

				_cMP3WaveFormat.WaveFormatEx.FormatTag = WaveFormatEx.FormatMP3;
				//_cMP3WaveFormat.WaveFormatEx.Channels = (short)((mpegLayer3Frame.Channels == MediaParsers.Channel.SingleChannel) ? 1 : 2);
				_cMP3WaveFormat.WaveFormatEx.Channels = (short)2;
				//_cMP3WaveFormat.WaveFormatEx.SamplesPerSec = mpegLayer3Frame.SamplingRate;
				_cMP3WaveFormat.WaveFormatEx.SamplesPerSec = _nAudioSampleRate;
				//_cMP3WaveFormat.WaveFormatEx.AvgBytesPerSec = mpegLayer3Frame.Bitrate / 8;
				_cMP3WaveFormat.WaveFormatEx.AvgBytesPerSec = _nAudioBitRate / 8;
				_cMP3WaveFormat.WaveFormatEx.BlockAlign = 1;
				_cMP3WaveFormat.WaveFormatEx.BitsPerSample = 0;
				_cMP3WaveFormat.WaveFormatEx.Size = 12;

				_cMP3WaveFormat.Id = 1;
				_cMP3WaveFormat.BitratePaddingMode = 0;
				_cMP3WaveFormat.FramesPerBlock = 1;
				//_cMP3WaveFormat.BlockSize = (short)mpegLayer3Frame.FrameSize; //(short)(144 * nBitRate / _nAudioSampleRate + _cMP3WaveFormat.BitratePaddingMode);
				_cMP3WaveFormat.BlockSize = (short)(144 * _nAudioBitRate / _nAudioSampleRate + _cMP3WaveFormat.BitratePaddingMode);
				_cMP3WaveFormat.CodecDelay = 0;

				ahMediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
				ahMediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = _cMP3WaveFormat.ToHexString();
				_cMediaStreamAudioDescription = new MediaStreamDescription(MediaStreamType.Audio, ahMediaStreamAttributes);
				_aMediaStreamDescriptions.Add(_cMediaStreamAudioDescription);

				switch (_nAudioBitRate)
				{
					case 64000:
						_nAudioMP3ControlByte = 0x54;
						break;
					case 128000:
						_nAudioMP3ControlByte = 0x94;
						break;
					default:
						throw new Exception("unsupported audio bit rate:" + _nAudioBitRate);
				}
				_aFramesOffsets.Add(new FrameOffset(0, 0));
				_nFrameDuration = TimeSpan.FromSeconds((double)1 / 25).Ticks; //FPS
				_nSampleDuration = TimeSpan.FromSeconds(((double)1 / _nAudioSampleRate) * 1152).Ticks;

				try
				{
					long nBufferTicks = TimeSpan.FromSeconds(_nBufferSeconds).Ticks;
					while ((ulong)_nBufferSeconds > _nFramesQty || nBufferTicks > (_aAudioNALs.Count * _nSampleDuration)) //если длительность меньше буфера, то крутим до исключения, по которому и выйдем из цикла
						NALUnitParse();
					_cThread = new System.Threading.Thread(NALUnitsReceive);
					_cThread.Start();
				}
				catch
				{
					_bCached = true;
				}

				TimeSpan tsDuration;
				if (1 > _nFramesQty)
				{
					lock (_cSyncRoot)
					{
						long nDurationAudio = (long)(_aAudioNALs.Count * _nSampleDuration);
						long nDurationVideo = (long)(_aVideoNALs.Count(row => row.bFrameStart) * _nFrameDuration);
						tsDuration = TimeSpan.FromTicks(nDurationAudio > nDurationVideo ? nDurationAudio : nDurationVideo);
					}
				}
				else
					tsDuration = TimeSpan.FromMilliseconds(_nFramesQty * 40); //FPS

				_ahMediaSourceAttributes[MediaSourceAttributesKeys.Duration] = tsDuration.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);
				_ahMediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = true.ToString();

				ReportOpenMediaCompleted(_ahMediaSourceAttributes, _aMediaStreamDescriptions);
			}
			catch (WebException e)
			{
				// Need to handle the exception
			}
		}
		private void NALUnitVideoAdd(NALUnit cNALUnit)
		{
			if (cNALUnit.nStart + cNALUnit.nBytesQty > _cStream.nBytesBuffered)
				_cStream.Cache(cNALUnit.nStart + cNALUnit.nBytesQty - _cStream.nBytesBuffered);
			double nProgress = 0;
			lock (_cSyncRoot)
			{
				_aVideoNALs.Add(cNALUnit);
				if (cNALUnit.bFrameStart)
				{
					_nFramesBuffered++;
					if (_bAudioFrameNeeded)
					{
						if (1 < (nProgress = (double)(_nFramesBuffered - _nCurrentFrameVideo) / (_nBufferSeconds * 25)))
							nProgress = 1;
						ReportGetSampleProgress(nProgress);
					}
				}
			}
			if (0.9 < nProgress)
			{
				if (_bAudioFrameNeeded)
					GetSampleAsync(MediaStreamType.Audio);
				if (_bVideoFrameNeeded)
					GetSampleAsync(MediaStreamType.Video);
			}
		}
		private void NALUnitAudioAdd(NALUnit cNALUnit)
		{
			if (cNALUnit.nStart + cNALUnit.nBytesQty > _cStream.nBytesBuffered)
				_cStream.Cache(cNALUnit.nStart + cNALUnit.nBytesQty - _cStream.nBytesBuffered);
			lock (_cSyncRoot)
				_aAudioNALs.Add(cNALUnit);
		}
		private void NALUnitParse()
        {
			//3. NumBytesInNALunit is set equal to the number of bytes starting with the byte at the current position in the byte
			//stream up to and including the last byte that precedes the location of any of the following:
			//– A subsequent byte-aligned three-byte sequence equal to 0x000000,
			//– A subsequent byte-aligned three-byte sequence equal to 0x000001,
			//– The end of the byte stream, as determined by unspecified means.
			//4. NumBytesInNALunit bytes are removed from the bitstream and the current position in the byte stream is
			//advanced by NumBytesInNALunit bytes. This sequence of bytes is nal_unit( NumBytesInNALunit ) and is
			//decoded using the NAL unit decoding process.

			//5. When the current position in the byte stream is not at the end of the byte stream (as determined by unspecified
			//means) and the next bytes in the byte stream do not start with a three-byte sequence equal to 0x000001 and the
			//next bytes in the byte stream do not start with a four byte sequence equal to 0x00000001, the decoder extracts
			//and discards each trailing_zero_8bits syntax element, moving the current position in the byte stream forward
			//one byte at a time, until the current position in the byte stream is such that the next bytes in the byte stream form
			//the four-byte sequence 0x00000001 or the end of the byte stream has been encountered (as determined by
			//unspecified means).
			NALUnit cNALUnitAudio = null;
			NALUnit cNALUnitVideo = new NALUnit();
			cNALUnitVideo.nStart = _cStream.Position - 3;
			
			bool bNALStart = false;
			byte nStepVideo = 0, nStepAudio = 0;
			int nByte = 0;

			//_cStream.ReadByte();
			//int nByteType = _cStream.ReadByte();
			//if (0 > nByteType)
			//    throw new Exception();
			while (true)
			{
				if (_cStream.Position >= nDataSize + nDataStart || 0 > (nByte = _cStream.ReadByte()))
					throw new Exception();

				if (0x00 == nByte)
				{
					nStepAudio = 0;
					nStepVideo++;
					if (2 < nStepVideo)
						bNALStart = true;
				}
				else if (0x01 == nByte)
				{
					nStepAudio = 0;
					if (1 < nStepVideo)
						bNALStart = true;
					else
						nStepVideo = 0;
				}
				else
				{
					nStepVideo = 0;
					if (0xFF == nByte)
						nStepAudio = 1;
					else if (0xFB == nByte && 1 == nStepAudio)
						nStepAudio++;
					else if (_nAudioMP3ControlByte == nByte && 2 == nStepAudio)
						nStepAudio++;
					else if ((0x64 == nByte || 0x44 == nByte) && 3 == nStepAudio)
					{
						long nPosition = _cStream.Position;
						_cStream.Position += _cMP3WaveFormat.BlockSize - 4;
						nByte = _cStream.ReadByte();
						if ((0 == nByte && 0 == _cStream.ReadByte() && 0 == _cStream.ReadByte()) || (0xFF == nByte && 0xFB == _cStream.ReadByte() && _nAudioMP3ControlByte == _cStream.ReadByte()))
						{
							if (null != cNALUnitVideo)
							{
								cNALUnitVideo.nBytesQty = nPosition - cNALUnitVideo.nStart - 4;
								cNALUnitVideo.bFrameStart = true;
								NALUnitVideoAdd(cNALUnitVideo);
								lock (_cSyncRoot)
									_aFramesOffsets.Add(new FrameOffset(_aAudioNALs.Count + 1, _aVideoNALs.Count));
								cNALUnitVideo = null;
							}
							cNALUnitAudio = new NALUnit();
							cNALUnitAudio.nStart = nPosition - 4;
							cNALUnitAudio.nBytesQty = (long)_cMP3WaveFormat.BlockSize;
							NALUnitAudioAdd(cNALUnitAudio);
							_cStream.Position -= 3;
						}
						else
							_cStream.Position = nPosition;
						nStepAudio = 0;
					}
					else
						nStepAudio = 0;
				}
				if (bNALStart)
				{
					if (null != cNALUnitVideo)
					{
						cNALUnitVideo.nBytesQty = _cStream.Position - cNALUnitVideo.nStart - 2;
						NALUnitVideoAdd(cNALUnitVideo);
					}
					return;
				}
			}
		}
		private void NALUnitsReceive()
		{
			try
			{
				while (true)
				{
					NALUnitParse();
					//System.Threading.Thread.Sleep(10);
				}
			}
			catch { }
			bool bProgress = false;
			lock (_cSyncRoot)
			{
				bProgress = (_bAudioFrameNeeded || _bVideoFrameNeeded);
				_bCached = true;
			}
			if (bProgress)
			{
				ReportGetSampleProgress(1);
				if (_bAudioFrameNeeded)
					GetSampleAsync(MediaStreamType.Audio);
				if (_bVideoFrameNeeded)
					GetSampleAsync(MediaStreamType.Video);
			}
		}

		protected override void GetSampleAsync(MediaStreamType eMediaStreamType)
        {
			Dictionary<MediaSampleAttributeKeys, string> emptyDict = new Dictionary<MediaSampleAttributeKeys, string>();
			MediaStreamSample cMediaStreamSample = null;
			NALUnit cNALUnit = null;
			lock (_cSyncRoot)
			{
				if (!_bCached && _nCurrentFrameVideo >= _nFramesBuffered)
				{
					this.ReportGetSampleProgress(0);
					if(MediaStreamType.Audio == eMediaStreamType)
						_bAudioFrameNeeded = true;
					else
						_bVideoFrameNeeded = true;
					return;
				}
				if (MediaStreamType.Audio == eMediaStreamType)
					_bAudioFrameNeeded = false;
				else
					_bVideoFrameNeeded = false;
			}
			if (eMediaStreamType == MediaStreamType.Audio)
			{
				try
				{
					lock (_cSyncRoot)
					{
						if (0 < _aAudioNALs.Count && _aAudioNALs.Count > _nFrameOffsetAudio)
							cNALUnit = _aAudioNALs[_nFrameOffsetAudio++];
					}
					if (null != cNALUnit)
					{
						cMediaStreamSample = new MediaStreamSample(_cMediaStreamAudioDescription, new MemoryStream(_cStream.Read(cNALUnit.nStart, cNALUnit.nBytesQty)), 0, cNALUnit.nBytesQty, (long)(_nCurrentFrameAudio * _nSampleDuration), emptyDict); //(long)(_nCurrentFrameAudio * _nSampleDuration)
						_nCurrentFrameAudio++;
					}
				}
				catch { }
				if(null == cMediaStreamSample)
					cMediaStreamSample = new MediaStreamSample(_cMediaStreamAudioDescription, null, 0, 0, 0, emptyDict);
			}
			else
			{
				try
				{
					lock (_cSyncRoot)
						if (0 < _aVideoNALs.Count && _aVideoNALs.Count > _nFrameOffsetVideo)
							cNALUnit = _aVideoNALs[_nFrameOffsetVideo++];
					if (null != cNALUnit)
						cMediaStreamSample = new MediaStreamSample(_cMediaStreamVideoDescription, new MemoryStream(_cStream.Read(cNALUnit.nStart, cNALUnit.nBytesQty)), 0, cNALUnit.nBytesQty, (long)(_nCurrentFrameVideo * _nFrameDuration), emptyDict); //(long)(_nCurrentFrameVideo * _nFrameDuration)
					if (null == cNALUnit || cNALUnit.bFrameStart)
						_nCurrentFrameVideo++;
				}
				catch { }
				if (null == cMediaStreamSample)
					cMediaStreamSample = new MediaStreamSample(_cMediaStreamVideoDescription, null, 0, 0, 0, emptyDict);
			}
			this.ReportGetSampleCompleted(cMediaStreamSample);
		}
        protected override void CloseMedia()
        {
        }
        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }
        protected override void SeekAsync(long seekToTime)
        {
			lock (_cSyncRoot)
			{
				_nCurrentFrameVideo = (int)(seekToTime / _nFrameDuration);
				if (_nCurrentFrameVideo >= _aFramesOffsets.Count)
					_nCurrentFrameVideo = _aFramesOffsets.Count - 1;
				_nFrameOffsetVideo = _aFramesOffsets[_nCurrentFrameVideo].nIndxVideo;
				_nFrameOffsetAudio = _aFramesOffsets[_nCurrentFrameVideo].nIndxAudio;
				_nCurrentFrameAudio = _nFrameOffsetAudio;
			}
            this.ReportSeekCompleted(seekToTime);
        }
        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }
    }
}