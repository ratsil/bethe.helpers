using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using helpers;

namespace ffmpeg.net
{
	public abstract class Format
	{
		public class Video : Format
		{
			private class TransformContext
			{
				private IntPtr pContext;
				private Frame cFrame;
				private Format.Video _cFormatSource;
				private Format.Video _cFormatTarget;
				public TransformContext(Format.Video cFormatSource, Format.Video cFormatTarget)
				{
					////lock (helper._oSyncRootGlobal)
						pContext = Functions.sws_getContext(cFormatSource.nWidth, cFormatSource.nHeight, cFormatSource.ePixelFormat, cFormatTarget.nWidth, cFormatTarget.nHeight, cFormatTarget.ePixelFormat, Constants.SWS_BICUBIC, NULL, NULL, NULL);
					if (NULL == pContext)
						throw new Exception("can't init trasform context");
					_cFormatSource = cFormatSource;
					_cFormatTarget = cFormatTarget;
				}
				~TransformContext()
				{
					try
					{
						Dispose();
					}
					catch { }
				}
				public void Dispose()
				{
					if (NULL != pContext)
					{
						////lock (helper._oSyncRootGlobal)
						{
							Functions.sws_freeContext(pContext);
							pContext = NULL;
						}
					}
					if (null != cFrame)
						cFrame.Dispose();
				}

				public Frame Process(Frame cFrameSource)
				{
					if (null == cFrame)
						cFrame = new Frame(_cFormatTarget);
					Process(cFrameSource, cFrame);
                    return cFrame;
				}
				public void Process(Frame cFrameSource, Frame cFrameTarget)
				{
					int nResult = 0;
                    AVFrame cAVFrameSource = (AVFrame)Marshal.PtrToStructure(cFrameSource, typeof(AVFrame));
					AVFrame cAVFrameTarget = (AVFrame)Marshal.PtrToStructure(cFrameTarget, typeof(AVFrame));
                    nResult = Functions.sws_scale(pContext, cAVFrameSource.data, cAVFrameSource.linesize, 0, _cFormatSource.nHeight, cAVFrameTarget.data, cAVFrameTarget.linesize);
				}
			}
			private Dictionary<Video, TransformContext> _ahTransformContexts;
			private ushort _nBitsPerPixel;

			//public IntPtr pAVFrame;

			public ushort nWidth
			{
				get
				{
					return (ushort)(0 > stAVCodecContext.width ? 0 : stAVCodecContext.width);
				}
			}
			public ushort nHeight
			{
				get
				{
					return (ushort)(0 > stAVCodecContext.height ? 0 : stAVCodecContext.height);
				}
			}
			public PixelFormat ePixelFormat
			{
				get
				{
					return stAVCodecContext.pix_fmt;
				}
			}
			public ushort nBitsPerPixel
			{
				get
				{
					return _nBitsPerPixel;
				}
			}

			public Video(ushort nWidth, ushort nHeight, PixelFormat ePixelFormat)
				: this(nWidth, nHeight, ePixelFormat, 0)
			{ }
            public Video(ushort nWidth, ushort nHeight, PixelFormat ePixelFormat, byte nThreads)
				: this(nWidth, nHeight, AVCodecID.CODEC_ID_RAWVIDEO, ePixelFormat, nThreads)
			{ }
            public Video(ushort nWidth, ushort nHeight, PixelFormat ePixelFormat, byte nThreads, uint nBitRate)
				: this(nWidth, nHeight, AVCodecID.CODEC_ID_RAWVIDEO, ePixelFormat, nThreads, nBitRate)
			{ }
			public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat)
				: this(nWidth, nHeight, eCodecID, ePixelFormat, 0)
			{ }
			public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat, byte nThreads)
				: this(nWidth, nHeight, eCodecID, ePixelFormat, NULL, nThreads)
			{ }
            public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat, byte nThreads, uint nBitRate)
				: this(nWidth, nHeight, eCodecID, ePixelFormat, NULL, nThreads, nBitRate)
			{ }
			public Video(Video cFormat)
                : this(cFormat, NULL)
			{ }
			public Video(Video cFormat, IntPtr pAVCC)
				: this(cFormat.nWidth, cFormat.nHeight, cFormat.eCodecID, cFormat.ePixelFormat, pAVCC)
			{ }
			public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat, IntPtr pAVCC)
				: this(nWidth, nHeight, eCodecID, ePixelFormat, pAVCC, 0)
			{ }
            public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat, IntPtr pAVCC, byte nThreads)
                : this(nWidth, nHeight, eCodecID, ePixelFormat, pAVCC, nThreads, 800000)
            { }
			public Video(ushort nWidth, ushort nHeight, AVCodecID eCodecID, PixelFormat ePixelFormat, IntPtr pAVCC, byte nThreads, uint nBitRate)
				: base(eCodecID, pAVCC, nThreads)
			{
				int nResult = 0;
				_ahTransformContexts = new Dictionary<Video, TransformContext>();
                nBufferSize = Functions.avpicture_get_size(ePixelFormat, nWidth, nHeight);
                _nBitsPerPixel = (ushort)(nBufferSize * 8 / (nWidth * nHeight));
				if (_bEncode)
				{
					stAVCodecContext.codec_type = AVMediaType.AVMEDIA_TYPE_VIDEO;
					stAVCodecContext.width = nWidth;
					stAVCodecContext.height = nHeight;
					stAVCodecContext.pix_fmt = ePixelFormat;
					stAVCodecContext.time_base.num = 1;
					stAVCodecContext.time_base.den = 25; //FPS
                    stAVCodecContext.bit_rate = (int)(int.MaxValue < nBitRate ? int.MaxValue : nBitRate); //чем больше, тем лучше качество
					if (NULL != _pCodec)
					{
						switch (eCodecID)
						{
							case AVCodecID.CODEC_ID_H264:
								stAVCodecContext.gop_size = 250; //кол-во неключевых кадров между ключевыми. чем больше, тем легче для CPU
								stAVCodecContext.max_b_frames = 2;
								stAVCodecContext.keyint_min = 25; //минимальное кол-во неключевых кадров между ключевыми

                                //stAVCodecContext.flags |= (int)(CodecFlags.CODEC_FLAG_LOOP_FILTER | CodecFlags.CODEC_FLAG_4MV | CodecFlags.CODEC_FLAG_GLOBAL_HEADER);
                                //stAVCodecContext.flags2 |= (int)CodecFlags.CODEC_FLAG2_MIXED_REFS;
                                //stAVCodecContext.me_cmp |= (int)MotionCompare.FF_CMP_CHROMA;
                                //stAVCodecContext.max_qdiff = 4;
                                //stAVCodecContext.i_quant_factor = 0.71F;
                                //stAVCodecContext.qcompress = 0.6F; 
                                //stAVCodecContext.gop_size = 250; //кол-во неключевых кадров между ключевыми. чем больше, тем легче для CPU
                                //stAVCodecContext.max_b_frames = 2;
                                //stAVCodecContext.keyint_min = 25; //минимальное кол-во неключевых кадров между ключевыми
								if (0 > (nResult = Functions.av_opt_set(stAVCodecContext.priv_data, new StringBuilder("profile"), new StringBuilder("baseline"), 1)))
									throw new Exception(helper.ErrorDescriptionGet(nResult));
								if (0 > (nResult = Functions.av_opt_set(stAVCodecContext.priv_data, new StringBuilder("preset"), new StringBuilder("slow"), 0)))
									throw new Exception(helper.ErrorDescriptionGet(nResult));
								if (0 > (nResult = Functions.av_opt_set(stAVCodecContext.priv_data, new StringBuilder("tune"), new StringBuilder("zerolatency"), 0))) //film,zerolatency,etc.
									throw new Exception(helper.ErrorDescriptionGet(nResult));

								break;
							case AVCodecID.CODEC_ID_MPEG2VIDEO:
								stAVCodecContext.max_b_frames = 2;
								break;
							case AVCodecID.CODEC_ID_MPEG1VIDEO:
								stAVCodecContext.mb_decision = 2;
								break;
                            default:
                                break;
						}
						Marshal.StructureToPtr(stAVCodecContext, pAVCodecContext, true);
						////lock (helper._oSyncRootGlobal)
						{
							if (0 > (nResult = Functions.avcodec_open2(pAVCodecContext, _pCodec, NULL)))
							{
								base.Dispose();
								throw new Exception("can't open video codec:" + eCodecID.ToString() + "[" + helper.ErrorDescriptionGet(nResult) + "]");
							}
						}
					}
					else
						Marshal.StructureToPtr(stAVCodecContext, pAVCodecContext, true);
				}
				else
				{
					////lock (helper._oSyncRootGlobal)
					{
						if (CodecIDRawGet() != eCodecID && 0 > Functions.avcodec_open2(pAVCodecContext, Functions.avcodec_find_decoder(eCodecID), NULL))
							if (CodecIDRawGet() != eCodecID && 0 > Functions.avcodec_open2(pAVCodecContext, Functions.avcodec_find_decoder(eCodecID), NULL))
							{
								base.Dispose();
								throw new Exception("can't open video codec:" + eCodecID.ToString());
							}
					}
				}
			}

			~Video()
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
				////lock (helper._oSyncRootGlobal)
					if (NULL != pAVCodecContext)
						Functions.avcodec_close(pAVCodecContext);
				foreach (TransformContext cTransform in _ahTransformContexts.Values)
					cTransform.Dispose();
				_ahTransformContexts.Clear();
				base.Dispose();
			}

			public Frame Transform(Video cFormatVideoTarget, Frame cFrameSource)
			{
                return Transform(cFormatVideoTarget, cFrameSource, null);
			}
            public Frame Transform(Video cFormatVideoTarget, Frame cFrameSource, Frame cFrameTarget)
			{
                if (!_ahTransformContexts.ContainsKey(cFormatVideoTarget) || null == _ahTransformContexts[cFormatVideoTarget])
                {
                    foreach (Video cFormatVideo in _ahTransformContexts.Keys.Where(o => NULL == o.pAVCodecContext).ToArray())
                    {
                        _ahTransformContexts[cFormatVideo].Dispose();
                        _ahTransformContexts.Remove(cFormatVideo);
                    }
                    _ahTransformContexts.Add(cFormatVideoTarget, new TransformContext(this, cFormatVideoTarget));
                }
                if (null == cFrameTarget)
				    return _ahTransformContexts[cFormatVideoTarget].Process(cFrameSource);
				_ahTransformContexts[cFormatVideoTarget].Process(cFrameSource, cFrameTarget);
                return cFrameTarget;
			}

			override protected AVCodecID CodecIDRawGet()
			{
				return AVCodecID.CODEC_ID_RAWVIDEO;
			}
			override public Frame[] Convert(Format cFormatTarget, Frame cFrameSource) //в pAVFrameSource лежат байты в формате this!!!
			{
				List<Frame> aRetVal = new List<Frame>();
				if (null == cFormatTarget || !(cFormatTarget is Format.Video))
					throw new Exception("target format is null or has a wrong type");
				Format.Video cFormatVideoTarget = (Format.Video)cFormatTarget;
				try
				{
                    if (ePixelFormat == cFormatVideoTarget.ePixelFormat && nHeight == cFormatVideoTarget.nHeight && nWidth == cFormatVideoTarget.nWidth)
                        return new Frame[] { new Frame(cFrameSource.aBytes) { nPTS = cFrameSource.nPTS, bKeyframe = cFrameSource.bKeyframe } };
                    if (eCodecID == cFormatTarget.eCodecID || NULL != _pCodec)
                        throw new NotImplementedException(); //TODO доделать конверт из encoded в raw
					
					cFrameSource = Transform(cFormatVideoTarget, cFrameSource);

                    int nSize;
                    if (NULL == cFrameSource)
                        (new Logger()).WriteWarning("Format.Video.Convert: IntPtr.Zero == cFrameSource.AVFrameGet()");
                    if (NULL == cFormatVideoTarget.pAVCodecContext)
                        (new Logger()).WriteWarning("Format.Video.Convert: IntPtr.Zero == cFormatVideoTarget.pAVCodecContext");
                    if (null == _cFrame)
                    {
                        _cFrame = new Frame(cFormatVideoTarget.nBufferSize);
                        _cFrame.nPTS = 0;
                    }
                    cFrameSource.nPTS = _cFrame.nPTS;
                    nSize = Functions.avcodec_encode_video(cFormatVideoTarget.pAVCodecContext, _cFrame.aBuffer, _cFrame.nLengthBuffer, cFrameSource);
					if (0 > nSize)
						throw new Exception("video encoding failed:" + nSize);
					if (0 < nSize)
					{
                        aRetVal.Add(new Frame(null, _cFrame.aBuffer.Take(nSize).ToArray()));

						AVCodecContext stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(cFormatVideoTarget.pAVCodecContext, typeof(AVCodecContext));
						if (NULL != stAVCodecContext.coded_frame)
						{
							AVFrame cAVFrame = (AVFrame)Marshal.PtrToStructure(stAVCodecContext.coded_frame, typeof(AVFrame));
							aRetVal[0].nPTS = cAVFrame.pts;
							aRetVal[0].bKeyframe = 0 < cAVFrame.key_frame;
						}
                    }
                    _cFrame.nPTS++;
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
				return aRetVal.ToArray();
			}
		}
		public class Audio : Format
		{
			private class TransformContext
			{
				private IntPtr pContext;
                private Frame cSamples;
				private Format.Audio _cFormatSource;
				private Format.Audio _cFormatTarget;

                public TransformContext(Format.Audio cFormatSource, Format.Audio cFormatTarget)
				{
                    pContext = NULL;
                    ////lock (helper._oSyncRootGlobal)
                    {
                        pContext = Functions.swr_alloc_set_opts(pContext, (long)cFormatTarget.stAVCodecContext.channel_layout, cFormatTarget.eSampleFormat, cFormatTarget.nSamplesRate, (long)cFormatSource.stAVCodecContext.channel_layout, cFormatSource.eSampleFormat, cFormatSource.nSamplesRate, 0, NULL);
                        if (NULL == pContext || 0 > Functions.swr_init(pContext))
                            throw new Exception("can't init audio transform context");
                    }
					_cFormatSource = cFormatSource;
					_cFormatTarget = cFormatTarget;
				}
				~TransformContext()
				{
					try
					{
						Dispose();
					}
					catch { }
				}
				public void Dispose()
				{
					if (NULL != pContext)
					{
                        Functions.swr_free(ref pContext);
                        pContext = NULL;
					}
                    if (null != cSamples)
						cSamples.Dispose();
				}
				public Frame Process(Frame cSamplesSource)
				{
                    if (null == cSamples)
						cSamples = new Frame(_cFormatTarget);
					Process(cSamplesSource, cSamples);
					return cSamples;
				}
				public void Process(Frame cSamplesSource, Frame cSamplesTarget)
				{
                    int nSamplesSourceQty = 0;
                    if(null != cSamplesSource)
                        nSamplesSourceQty = cSamplesSource.nLength / (_cFormatSource.nChannelsQty * _cFormatSource.nBitsPerSample / 8);
                    int nSamplesTargetQty = cSamplesTarget.nLengthBuffer / (_cFormatTarget.nChannelsQty * _cFormatTarget.nBitsPerSample / 8);
                    nSamplesTargetQty = Functions.swr_convert(pContext, cSamplesTarget, nSamplesTargetQty, cSamplesSource, nSamplesSourceQty);
                    cSamplesTarget.nLength = nSamplesTargetQty * _cFormatTarget.nChannelsQty * _cFormatTarget.nBitsPerSample / 8;
				}
			}
			private Dictionary<Audio, TransformContext> _ahTransformContexts;
			private List<List<byte>> aByteStream;

			public int nSamplesRate
			{
				get
				{
					return stAVCodecContext.sample_rate;
				}
			}
			public int nChannelsQty
			{
				get
				{
					return stAVCodecContext.channels;
				}
			}
			public AVSampleFormat eSampleFormat
			{
				get
				{
					return stAVCodecContext.sample_fmt;
				}
			}
            public int nBitsPerSample;

			public Audio(int nSamplesRate, int nChannelsQty, AVSampleFormat eSampleFormat)
				: this(nSamplesRate, nChannelsQty, eSampleFormat, 0)
			{ }
			public Audio(int nSamplesRate, int nChannelsQty, AVSampleFormat eSampleFormat, byte nThreads)
				: this(nSamplesRate, nChannelsQty, AVCodecID.CODEC_ID_NONE, eSampleFormat, NULL, nThreads)
			{ }
            public Audio(int nSamplesRate, int nChannelsQty, AVSampleFormat eSampleFormat, byte nThreads, uint nBitRate)
				: this(nSamplesRate, nChannelsQty, AVCodecID.CODEC_ID_NONE, eSampleFormat, NULL, nThreads, nBitRate)
			{ }
			public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat)
				: this(nSamplesRate, nChannelsQty, eCodecID, eSampleFormat, 0)
			{ }
			public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat, byte nThreads)
				: this(nSamplesRate, nChannelsQty, eCodecID, eSampleFormat, NULL, nThreads)
			{ }
            public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat, byte nThreads, uint nBitRate)
				: this(nSamplesRate, nChannelsQty, eCodecID, eSampleFormat, NULL, nThreads, nBitRate)
			{ }
            public Audio(Audio cFormat)
                : this(cFormat, NULL)
            { }
            public Audio(Audio cFormat, IntPtr pAVCC)
                : this(cFormat.nSamplesRate, cFormat.nChannelsQty, cFormat.eCodecID, cFormat.eSampleFormat, pAVCC)
            { }
            public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat, IntPtr pAVCC)
				: this(nSamplesRate, nChannelsQty, eCodecID, eSampleFormat, pAVCC, 0)
			{
			}

			public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat, IntPtr pAVCC, byte nThreads)
				: this(nSamplesRate, nChannelsQty, eCodecID, eSampleFormat, pAVCC, 0, 128000)
			{ }
            public Audio(int nSamplesRate, int nChannelsQty, AVCodecID eCodecID, AVSampleFormat eSampleFormat, IntPtr pAVCC, byte nThreads, uint nBitRate)
				: base(eCodecID, pAVCC, nThreads)
			{
				//_pBytesStream = NULL;
				_ahTransformContexts = new Dictionary<Audio, TransformContext>();
                nBitsPerSample = Functions.av_get_bits_per_sample_fmt(eSampleFormat);
                if(1 > nBitsPerSample)
				    throw new Exception("unknown sample format");
                if (_bEncode)
				{
					stAVCodecContext.codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
                    stAVCodecContext.sample_fmt = eSampleFormat;
                    stAVCodecContext.sample_rate = nSamplesRate;
                    stAVCodecContext.time_base.num = 1;
                    stAVCodecContext.time_base.den = nSamplesRate;
                    stAVCodecContext.channels = nChannelsQty;

					if (NULL == _pCodec)
						stAVCodecContext.bit_rate = nSamplesRate * nBitsPerSample * nChannelsQty;
                    else
                        stAVCodecContext.bit_rate = (int)(int.MaxValue < nBitRate ? int.MaxValue : nBitRate); //чем больше, тем лучше качество
                    Marshal.StructureToPtr(stAVCodecContext, pAVCodecContext, true);
					if (NULL != _pCodec)
					{
						if (0 > Functions.avcodec_open2(pAVCodecContext, _pCodec, NULL))
						{
							base.Dispose();
							throw new Exception("can't open audio codec 1:" + eCodecID.ToString());
						}
					}
				}
				else
				{
					//audio test
					//lock (helper._oSyncRootGlobal)
					{
						if (0 > Functions.avcodec_open2(pAVCodecContext, Functions.avcodec_find_decoder(eCodecID), NULL))
							if (0 > Functions.avcodec_open2(pAVCodecContext, Functions.avcodec_find_decoder(eCodecID), NULL))
							{
								base.Dispose();
								throw new Exception("can't open audio codec 2:" + eCodecID.ToString());
							}
					}
					//nBufferSize = 192000 * 2;
				}
                aByteStream = null;
				stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(pAVCodecContext, typeof(AVCodecContext));
                nBufferSize = Functions.av_samples_get_buffer_size(NULL, nChannelsQty, 0 < stAVCodecContext.frame_size? stAVCodecContext.frame_size : nSamplesRate / 25, eSampleFormat, 0);// stAVCodecContext.bit_rate / 8;
                if(1 > stAVCodecContext.channel_layout)
                    stAVCodecContext.channel_layout = (ulong)Functions.av_get_default_channel_layout(nChannelsQty);
				Marshal.StructureToPtr(stAVCodecContext, pAVCodecContext, true);
			}

			~Audio()
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
				//lock (helper._oSyncRootGlobal)
				{
					if (NULL != pAVCodecContext)
						Functions.avcodec_close(pAVCodecContext);
				}
				base.Dispose();
			}

			public Frame Transform(Audio cFormatAudioTarget, Frame cSamplesSource)
			{
				return Transform(cFormatAudioTarget, cSamplesSource, null);
			}
			public Frame Transform(Audio cFormatAudioTarget, Frame cSamplesSource, Frame cSamplesTarget)
			{
				if (nSamplesRate != cFormatAudioTarget.nSamplesRate || eSampleFormat != cFormatAudioTarget.eSampleFormat || nChannelsQty != cFormatAudioTarget.nChannelsQty)
				{

					if (!_ahTransformContexts.ContainsKey(cFormatAudioTarget) || null == _ahTransformContexts[cFormatAudioTarget])
                    {
                        foreach (Audio cFormatAudio in _ahTransformContexts.Keys.Where(o => NULL == o.pAVCodecContext).ToArray())
                        {
                            _ahTransformContexts[cFormatAudio].Dispose();
                            _ahTransformContexts.Remove(cFormatAudio);
                        }
                        _ahTransformContexts.Add(cFormatAudioTarget, new TransformContext(this, cFormatAudioTarget));
                    }

					if (null == cSamplesTarget)
						return _ahTransformContexts[cFormatAudioTarget].Process(cSamplesSource);
					_ahTransformContexts[cFormatAudioTarget].Process(cSamplesSource, cSamplesTarget);
					return cSamplesTarget;
				}
				return cSamplesSource;
			}

			override protected AVCodecID CodecIDRawGet()
			{
				return AVCodecID.CODEC_ID_NONE;
			}
			override public Frame[] Convert(Format cFormatTarget, Frame cFrameSource) //в cFrameSource лежат байты в формате this!!!
			{
				List<Frame> aRetVal = new List<Frame>();
				if (null == cFormatTarget || !(cFormatTarget is Format.Audio))
					throw new Exception("target format is null or has a wrong type");
				Format.Audio cFormatAudioTarget = (Format.Audio)cFormatTarget;
                IntPtr pPacket = NULL;
                Frame cFrameConverted;
                AVFrame cAVFrame;
                int nIndx = 0, nFrameSize, nSize, nPacketGot = 0, nOffset = 0;
                try
                {
                    if (eCodecID == cFormatTarget.eCodecID)
                    {
                        if (nSamplesRate == cFormatAudioTarget.nSamplesRate && eSampleFormat == cFormatAudioTarget.eSampleFormat && nChannelsQty == cFormatAudioTarget.nChannelsQty)
                            return new Frame[] { new Frame(null, cFrameSource.aBytes) { nPTS = cFrameSource.nPTS, bKeyframe = cFrameSource.bKeyframe } };
                        if (NULL != _pCodec)
                            throw new NotImplementedException(); //TODO доделать конверт из encoded в raw
                    }
                    if (nBufferSize < cFrameSource.nLength)
                        throw new Exception("wrong bytes qty for specified audio format. Should be less than " + nBufferSize + " but got " + cFrameSource.nLength);

                    while(true)
                    {
                        cFrameConverted = Transform(cFormatAudioTarget, cFrameSource);
                        if (null == cFrameConverted || 1 > cFrameConverted.nLength)
                            break;
                        cFrameSource = null;
                        cAVFrame = (AVFrame)Marshal.PtrToStructure(cFrameConverted, typeof(AVFrame));
                        if (null == aByteStream)
                        {
                            aByteStream = new List<List<byte>>();
                            for (nIndx = 0; cAVFrame.data.Length > nIndx; nIndx++)
                            {
                                if (NULL == cAVFrame.data[nIndx])
                                    break;
                                aByteStream.Add(new List<byte>());
                            }
                            if(1 > aByteStream.Count)
                                aByteStream.Add(new List<byte>());
                        }
                        int nLineSize = cFrameConverted.nLength / aByteStream.Count;
                        for (nIndx = 0; aByteStream.Count > nIndx; nIndx++)
                            aByteStream[nIndx].AddRange(cFrameConverted.aBuffer.Skip((int)((long)cAVFrame.data[nIndx] - (long)cAVFrame.data[0])).Take(nLineSize));
                    }
                    pPacket = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
                    Functions.av_init_packet(pPacket);
                    AVPacket stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                    stPacket.size = 0;
                    if (null == _cFrame)
                        _cFrame = new Frame(cFormatAudioTarget.nBufferSize);
                    if (1 > (nFrameSize = cFormatAudioTarget.stAVCodecContext.frame_size))
                        nFrameSize = cFrameConverted.nLength / ((cFormatAudioTarget.nBitsPerSample / 8) * cFormatAudioTarget.nChannelsQty);
                    nFrameSize *= (cFormatAudioTarget.nBitsPerSample / 8);
                    if (null == cFormatAudioTarget._cFrame)
                        cFormatTarget._cFrame = new Frame(this, nFrameSize * cFormatAudioTarget.nChannelsQty);
                    if (2 > aByteStream.Count)
                        nFrameSize *= cFormatAudioTarget.nChannelsQty;
                    while (nFrameSize <= aByteStream[0].Count && _cFrame.nLengthBuffer > (nOffset + stPacket.size))
                    {
                        for (nIndx = 0; aByteStream.Count > nIndx; nIndx++)
                        {
                            aByteStream[nIndx].CopyTo(0, cFormatTarget._cFrame.aBuffer, nIndx * nFrameSize, nFrameSize);
                            aByteStream[nIndx].RemoveRange(0, nFrameSize);
                        }
                        stPacket.data = _cFrame.pBytes + nOffset;
                        stPacket.size = _cFrame.nLengthBuffer - nOffset;
                        Marshal.StructureToPtr(stPacket, pPacket, true);

                        //lock (helper._oSyncRootGlobal)
                            nSize = Functions.avcodec_encode_audio2(cFormatAudioTarget.pAVCodecContext, pPacket, cFormatTarget._cFrame, ref nPacketGot);
                        if (0 > nSize)
                            throw new Exception("audio encoding failed\n");
                        if (0 < nPacketGot)
                        {
                            stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                            if (0 < stPacket.size)
                            {
                                aRetVal.Add(new Frame(_cFrame.aBuffer.Skip(nOffset).Take(stPacket.size).ToArray()) { nPTS = stPacket.pts }); //TODO нужно сделать "наследование" одного фрейма от другого (один aBytes на оба Frame'а)
                                nOffset += stPacket.size;
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
                    if (NULL != pPacket)
                    {
                        Functions.av_free_packet(pPacket);
                        Functions.av_freep(ref pPacket);
                    }
                    //if (NULL != pAVFrame)
                    //    Functions.avcodec_free_frame(ref pAVFrame);
                }
				return aRetVal.ToArray();
			}
			public Frame[] Flush()
			{
                if (null == _cFrame)
                    _cFrame = new Frame(nBufferSize);
                List<Frame> aRetVal = new List<Frame>();
                IntPtr pPacket = NULL;
                try
                {
                    int nPacketGot = 0, nOffset = 0;
                    pPacket = Functions.av_malloc(Marshal.SizeOf(typeof(AVPacket)));
                    Functions.av_init_packet(pPacket);
                    AVPacket stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                    stPacket.size = 0;
                    while (true)
                    {
                        stPacket.data = _cFrame.pBytes + nOffset;
                        stPacket.size = _cFrame.nLengthBuffer - nOffset;
                        Marshal.StructureToPtr(stPacket, pPacket, true);
                        //lock (helper._oSyncRootGlobal)
                            Functions.avcodec_encode_audio2(pAVCodecContext, pPacket, NULL, ref nPacketGot);
                        stPacket = (AVPacket)Marshal.PtrToStructure(pPacket, typeof(AVPacket));
                        if (0 < nPacketGot && 0 < stPacket.size)
                        {
                            aRetVal.Add(new Frame(_cFrame.aBuffer.Skip(nOffset).Take(stPacket.size).ToArray()) { nPTS = stPacket.pts }); //TODO нужно сделать "наследование" одного фрейма от другого (один aBytes на оба Frame'а)
                            nOffset += stPacket.size;
                        }
//                        else
                        break;
                    }
                }
                catch (Exception ex)
                {
                    (new Logger()).WriteError(ex);
                }
                finally
                {
                    if (NULL != pPacket)
                    {
                        Functions.av_free_packet(pPacket);
                        Functions.av_freep(ref pPacket);
                    }
                }
				return aRetVal.ToArray();
			}
		}

		static private IntPtr NULL = IntPtr.Zero;
		private bool _bAVCodecContextAllocationInternal;
		protected IntPtr _pCodec;
		protected bool _bEncode;
		public int nBufferSize;
        private Frame _cFrame;

		public IntPtr pAVCodecContext;
		internal AVCodecContext stAVCodecContext;
		public AVCodecID eCodecID
		{
			get
			{
				return stAVCodecContext.codec_id;
			}
		}

		private Format()
		{
		}
		protected Format(AVCodecID eCodecID, IntPtr pAVCC, byte nThreads)
		{
            helper.Initialize();
			_pCodec = NULL;
			nBufferSize = 0;
			int nResult = 0;
			_bEncode = false;
			pAVCodecContext = pAVCC;
			_bAVCodecContextAllocationInternal = false;
			AVMediaType eAVMediaType = AVMediaType.AVMEDIA_TYPE_UNKNOWN;
			if (NULL != pAVCodecContext)
			{
				stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(pAVCodecContext, typeof(AVCodecContext));
				eAVMediaType = stAVCodecContext.codec_type;
			}

			if (AVMediaType.AVMEDIA_TYPE_UNKNOWN == eAVMediaType)
			{
				if (CodecIDRawGet() != eCodecID)
				{
					//if (AVCodecID.CODEC_ID_H264_MOBILE == eCodecID)
					//	eCodecID = AVCodecID.CODEC_ID_H264;
					if (NULL == (_pCodec = Functions.avcodec_find_encoder(eCodecID)))
						throw new Exception("can't find codec " + eCodecID.ToString());
				}
				if (NULL == pAVCodecContext)
				{
					//lock (helper._oSyncRootGlobal)
					{
						pAVCodecContext = Functions.avcodec_alloc_context3(_pCodec);
						_bAVCodecContextAllocationInternal = true;
					}
				}
				else
				{
					//lock (helper._oSyncRootGlobal)
						nResult = Functions.avcodec_get_context_defaults3(pAVCodecContext, _pCodec);
				}
				stAVCodecContext = (AVCodecContext)Marshal.PtrToStructure(pAVCodecContext, typeof(AVCodecContext));
				stAVCodecContext.codec_id = eCodecID;
				_bEncode = true;
			}
			if(1 > nThreads)
				nThreads = (byte)Environment.ProcessorCount;
			stAVCodecContext.thread_count = nThreads;
			Marshal.StructureToPtr(stAVCodecContext, pAVCodecContext, true);
		}
		~Format()
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
            if (_bAVCodecContextAllocationInternal && NULL != pAVCodecContext)
                Functions.av_freep(ref pAVCodecContext);
            else
                pAVCodecContext = NULL;
            if (null != _cFrame)
                _cFrame.Dispose();
		}

		abstract protected AVCodecID CodecIDRawGet();
		abstract public Frame[] Convert(Format cFormatTarget, Frame cFrameSource);
        //abstract public Frame[] Flush(Format cFormatTarget);
    }
}
