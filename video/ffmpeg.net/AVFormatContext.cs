// /**
//  * Format I/O context.
//  * New fields can be added to the end with minor version bumps.
//  * Removal, reordering and changes to existing fields require a major
//  * version bump.
//  * sizeof(AVFormatContext) must not be used outside libav*, use
//  * avformat_alloc_context() to create an AVFormatContext.
//  */
// typedef struct AVFormatContext {
//     /**
//      * A class for logging and AVOptions. Set by avformat_alloc_context().
//      * Exports (de)muxer private options if they exist.
//      */
//     const AVClass *av_class;
// 
//     /**
//      * Can only be iformat or oformat, not both at the same time.
//      *
//      * decoding: set by avformat_open_input().
//      * encoding: set by the user.
//      */
//     struct AVInputFormat *iformat;
//     struct AVOutputFormat *oformat;
// 
//     /**
//      * Format private data. This is an AVOptions-enabled struct
//      * if and only if iformat/oformat.priv_class is not NULL.
//      */
//     void *priv_data;
// 
//     /**
//      * I/O context.
//      *
//      * decoding: either set by the user before avformat_open_input() (then
//      * the user must close it manually) or set by avformat_open_input().
//      * encoding: set by the user.
//      *
//      * Do NOT set this field if AVFMT_NOFILE flag is set in
//      * iformat/oformat.flags. In such a case, the (de)muxer will handle
//      * I/O in some other way and this field will be NULL.
//      */
//     AVIOContext *pb;
// 
//     /* stream info */
//     int ctx_flags; /**< Format-specific flags, see AVFMTCTX_xx */
// 
//     /**
//      * A list of all streams in the file. New streams are created with
//      * avformat_new_stream().
//      *
//      * decoding: streams are created by libavformat in avformat_open_input().
//      * If AVFMTCTX_NOHEADER is set in ctx_flags, then new streams may also
//      * appear in av_read_frame().
//      * encoding: streams are created by the user before avformat_write_header().
//      */
//     unsigned int nb_streams;
//     AVStream **streams;
// 
//     char filename[1024]; /**< input or output filename */
// 
//     /**
//      * Decoding: position of the first frame of the component, in
//      * AV_TIME_BASE fractional seconds. NEVER set this value directly:
//      * It is deduced from the AVStream values.
//      */
//     int64_t start_time;
// 
//     /**
//      * Decoding: duration of the stream, in AV_TIME_BASE fractional
//      * seconds. Only set this value if you know none of the individual stream
//      * durations and also do not set any of them. This is deduced from the
//      * AVStream values if not set.
//      */
//     int64_t duration;
// 
//     /**
//      * Decoding: total stream bitrate in bit/s, 0 if not
//      * available. Never set it directly if the file_size and the
//      * duration are known as FFmpeg can compute it automatically.
//      */
//     int bit_rate;
// 
//     unsigned int packet_size;
//     int max_delay;
// 
//     int flags;
// #define AVFMT_FLAG_GENPTS       0x0001 ///< Generate missing pts even if it requires parsing future frames.
// #define AVFMT_FLAG_IGNIDX       0x0002 ///< Ignore index.
// #define AVFMT_FLAG_NONBLOCK     0x0004 ///< Do not block when reading packets from input.
// #define AVFMT_FLAG_IGNDTS       0x0008 ///< Ignore DTS on frames that contain both DTS & PTS
// #define AVFMT_FLAG_NOFILLIN     0x0010 ///< Do not infer any values from other values, just return what is stored in the container
// #define AVFMT_FLAG_NOPARSE      0x0020 ///< Do not use AVParsers, you also must set AVFMT_FLAG_NOFILLIN as the fillin code works on frames and no parsing -> no frames. Also seeking to frames can not work if parsing to find frame boundaries has been disabled
// #define AVFMT_FLAG_NOBUFFER     0x0040 ///< Do not buffer frames when possible
// #define AVFMT_FLAG_CUSTOM_IO    0x0080 ///< The caller has supplied a custom AVIOContext, don't avio_close() it.
// #define AVFMT_FLAG_DISCARD_CORRUPT  0x0100 ///< Discard frames marked corrupted
// #define AVFMT_FLAG_FLUSH_PACKETS    0x0200 ///< Flush the AVIOContext every packet.
// #define AVFMT_FLAG_MP4A_LATM    0x8000 ///< Enable RTP MP4A-LATM payload
// #define AVFMT_FLAG_SORT_DTS    0x10000 ///< try to interleave outputted packets by dts (using this flag can slow demuxing down)
// #define AVFMT_FLAG_PRIV_OPT    0x20000 ///< Enable use of private options by delaying codec open (this could be made default once all code is converted)
// #define AVFMT_FLAG_KEEP_SIDE_DATA 0x40000 ///< Don't merge side data but keep it separate.
// 
//     /**
//      * decoding: size of data to probe; encoding: unused.
//      */
//     unsigned int probesize;
// 
//     /**
//      * decoding: maximum time (in AV_TIME_BASE units) during which the input should
//      * be analyzed in avformat_find_stream_info().
//      */
//     int max_analyze_duration;
// 
//     const uint8_t *key;
//     int keylen;
// 
//     unsigned int nb_programs;
//     AVProgram **programs;
// 
//     /**
//      * Forced video codec_id.
//      * Demuxing: Set by user.
//      */
//     enum AVCodecID video_codec_id;
// 
//     /**
//      * Forced audio codec_id.
//      * Demuxing: Set by user.
//      */
//     enum AVCodecID audio_codec_id;
// 
//     /**
//      * Forced subtitle codec_id.
//      * Demuxing: Set by user.
//      */
//     enum AVCodecID subtitle_codec_id;
// 
//     /**
//      * Maximum amount of memory in bytes to use for the index of each stream.
//      * If the index exceeds this size, entries will be discarded as
//      * needed to maintain a smaller size. This can lead to slower or less
//      * accurate seeking (depends on demuxer).
//      * Demuxers for which a full in-memory index is mandatory will ignore
//      * this.
//      * muxing  : unused
//      * demuxing: set by user
//      */
//     unsigned int max_index_size;
// 
//     /**
//      * Maximum amount of memory in bytes to use for buffering frames
//      * obtained from realtime capture devices.
//      */
//     unsigned int max_picture_buffer;
// 
//     /**
//      * Number of chapters in AVChapter array.
//      * When muxing, chapters are normally written in the file header,
//      * so nb_chapters should normally be initialized before write_header
//      * is called. Some muxers (e.g. mov and mkv) can also write chapters
//      * in the trailer.  To write chapters in the trailer, nb_chapters
//      * must be zero when write_header is called and non-zero when
//      * write_trailer is called.
//      * muxing  : set by user
//      * demuxing: set by libavformat
//      */
//     unsigned int nb_chapters;
//     AVChapter **chapters;
// 
//     AVDictionary *metadata;
// 
//     /**
//      * Start time of the stream in real world time, in microseconds
//      * since the unix epoch (00:00 1st January 1970). That is, pts=0
//      * in the stream was captured at this real world time.
//      * - encoding: Set by user.
//      * - decoding: Unused.
//      */
//     int64_t start_time_realtime;
// 
//     /**
//      * decoding: number of frames used to probe fps
//      */
//     int fps_probe_size;
// 
//     /**
//      * Error recognition; higher values will detect more errors but may
//      * misdetect some more or less valid parts as errors.
//      * - encoding: unused
//      * - decoding: Set by user.
//      */
//     int error_recognition;
// 
//     /**
//      * Custom interrupt callbacks for the I/O layer.
//      *
//      * decoding: set by the user before avformat_open_input().
//      * encoding: set by the user before avformat_write_header()
//      * (mainly useful for AVFMT_NOFILE formats). The callback
//      * should also be passed to avio_open2() if it's used to
//      * open the file.
//      */
//     AVIOInterruptCB interrupt_callback;
// 
//     /**
//      * Flags to enable debugging.
//      */
//     int debug;
// #define FF_FDEBUG_TS        0x0001
// 
//     /**
//      * Transport stream id.
//      * This will be moved into demuxer private options. Thus no API/ABI compatibility
//      */
//     int ts_id;
// 
//     /**
//      * Audio preload in microseconds.
//      * Note, not all formats support this and unpredictable things may happen if it is used when not supported.
//      * - encoding: Set by user via AVOptions (NO direct access)
//      * - decoding: unused
//      */
//     int audio_preload;
// 
//     /**
//      * Max chunk time in microseconds.
//      * Note, not all formats support this and unpredictable things may happen if it is used when not supported.
//      * - encoding: Set by user via AVOptions (NO direct access)
//      * - decoding: unused
//      */
//     int max_chunk_duration;
// 
//     /**
//      * Max chunk size in bytes
//      * Note, not all formats support this and unpredictable things may happen if it is used when not supported.
//      * - encoding: Set by user via AVOptions (NO direct access)
//      * - decoding: unused
//      */
//     int max_chunk_size;
// 
//     /**
//      * forces the use of wallclock timestamps as pts/dts of packets
//      * This has undefined results in the presence of B frames.
//      * - encoding: unused
//      * - decoding: Set by user via AVOptions (NO direct access)
//      */
//     int use_wallclock_as_timestamps;
// 
//     /**
//      * Avoid negative timestamps during muxing.
//      *  0 -> allow negative timestamps
//      *  1 -> avoid negative timestamps
//      * -1 -> choose automatically (default)
//      * Note, this only works when interleave_packet_per_dts is in use.
//      * - encoding: Set by user via AVOptions (NO direct access)
//      * - decoding: unused
//      */
//     int avoid_negative_ts;
// 
//     /**
//      * avio flags, used to force AVIO_FLAG_DIRECT.
//      * - encoding: unused
//      * - decoding: Set by user via AVOptions (NO direct access)
//      */
//     int avio_flags;
// 
//     /**
//      * The duration field can be estimated through various ways, and this field can be used
//      * to know how the duration was estimated.
//      * - encoding: unused
//      * - decoding: Read by user via AVOptions (NO direct access)
//      */
//     enum AVDurationEstimationMethod duration_estimation_method;
// 
//     /**
//      * Skip initial bytes when opening stream
//      * - encoding: unused
//      * - decoding: Set by user via AVOptions (NO direct access)
//      */
//     unsigned int skip_initial_bytes;
// 
//     /**
//      * Correct single timestamp overflows
//      * - encoding: unused
//      * - decoding: Set by user via AVOPtions (NO direct access)
//      */
//     unsigned int correct_ts_overflow;
// 
//     /**
//      * Force seeking to any (also non key) frames.
//      * - encoding: unused
//      * - decoding: Set by user via AVOPtions (NO direct access)
//      */
//     int seek2any;
// 
//     /**
//      * Flush the I/O context after each packet.
//      * - encoding: Set by user via AVOptions (NO direct access)
//      * - decoding: unused
//      */
//     int flush_packets;
// 
//     /**
//      * format probing score.
//      * The maximal score is AVPROBE_SCORE_MAX, its set when the demuxer probes
//      * the format.
//      * - encoding: unused
//      * - decoding: set by avformat, read by user via av_format_get_probe_score() (NO direct access)
//      */
//     int probe_score;
// 
//     /*****************************************************************
//      * All fields below this line are not part of the public API. They
//      * may not be used outside of libavformat and can be changed and
//      * removed at will.
//      * New public fields should be added right above.
//      *****************************************************************
//      */
// 
//     /**
//      * This buffer is only needed when packets were already buffered but
//      * not decoded, for example to get the codec parameters in MPEG
//      * streams.
//      */
//     struct AVPacketList *packet_buffer;
//     struct AVPacketList *packet_buffer_end;
// 
//     /* av_seek_frame() support */
//     int64_t data_offset; /**< offset of the first packet */
// 
//     /**
//      * Raw packets from the demuxer, prior to parsing and decoding.
//      * This buffer is used for buffering packets until the codec can
//      * be identified, as parsing cannot be done without knowing the
//      * codec.
//      */
//     struct AVPacketList *raw_packet_buffer;
//     struct AVPacketList *raw_packet_buffer_end;
//     /**
//      * Packets split by the parser get queued here.
//      */
//     struct AVPacketList *parse_queue;
//     struct AVPacketList *parse_queue_end;
//     /**
//      * Remaining size available for raw_packet_buffer, in bytes.
//      */
// #define RAW_PACKET_BUFFER_SIZE 2500000
//     int raw_packet_buffer_remaining_size;
// 
//     /**
//      * Offset to remap timestamps to be non-negative.
//      * Expressed in timebase units.
//      * @see AVStream.mux_ts_offset
//      */
//     int64_t offset;
// 
//     /**
//      * Timebase for the timestamp offset.
//      */
//     AVRational offset_timebase;
// 
//     /**
//      * IO repositioned flag.
//      * This is set by avformat when the underlaying IO context read pointer
//      * is repositioned, for example when doing byte based seeking.
//      * Demuxers can use the flag to detect such changes.
//      */
//     int io_repositioned;
// 
//     /**
//      * Forced video codec.
//      * This allows forcing a specific decoder, even when there are multiple with
//      * the same codec_id.
//      * Demuxing: Set by user via av_format_set_video_codec (NO direct access).
//      */
//     AVCodec *video_codec;
// 
//     /**
//      * Forced audio codec.
//      * This allows forcing a specific decoder, even when there are multiple with
//      * the same codec_id.
//      * Demuxing: Set by user via av_format_set_audio_codec (NO direct access).
//      */
//     AVCodec *audio_codec;
// 
//     /**
//      * Forced subtitle codec.
//      * This allows forcing a specific decoder, even when there are multiple with
//      * the same codec_id.
//      * Demuxing: Set by user via av_format_set_subtitle_codec (NO direct access).
//      */
//     AVCodec *subtitle_codec;
// } AVFormatContext; 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	class AVFormatContext : Functions
	{
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		private struct AVFormatContextInternal
		{
			public /*AVClass* */IntPtr av_class;
			public /*AVInputFormat* */IntPtr iformat;
			public /*AVOutputFormat* */IntPtr oformat;
			public /*void* */IntPtr priv_data;
			public /*AVIOContext* */IntPtr pb;
            public int ctx_flags; /**< Format-specific flags, see AVFMTCTX_xx */
            public uint nb_streams;
			public /*AVStream** */IntPtr streams;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
			public /*char[1024]*/ string filename; /**< input or output filename */
			public long start_time;
			public long duration;
			public int bit_rate;
			public uint packet_size;
			public int max_delay;
			public int flags;
			public uint probesize;
			public int max_analyze_duration;
			public /*uint8_t* */IntPtr key;
			public int keylen;
			public uint nb_programs;
			public /*AVProgram** */IntPtr programs;
			public AVCodecID video_codec_id;
			public AVCodecID audio_codec_id;
			public AVCodecID subtitle_codec_id;
			public uint max_index_size;
			public uint max_picture_buffer;
			public uint nb_chapters;
			public /*AVChapter** */IntPtr chapters;
			public /*AVMetadata* */IntPtr metadata;
			public long start_time_realtime;
			public int fps_probe_size;
			public int error_recognition;
			public AVIOInterruptCB interrupt_callback;
            public int debug;
            public int ts_id;
			public int audio_preload;
			public int max_chunk_duration;
			public int max_chunk_size;
            public int use_wallclock_as_timestamps;
            public int avoid_negative_ts;
            public int avio_flags;
            public AVDurationEstimationMethod duration_estimation_method;
            public uint skip_initial_bytes;
            public uint correct_ts_overflow;
            public int seek2any;
            public int flush_packets;
            public int probe_score;

            public /*AVPacketList* */IntPtr packet_buffer;
            public /*AVPacketList* */IntPtr packet_buffer_end;
            public long data_offset; /**< offset of the first packet */

            public /*AVPacketList* */IntPtr raw_packet_buffer;
            public /*AVPacketList* */IntPtr raw_packet_buffer_end;
            public /*AVPacketList* */IntPtr parse_queue;
            public /*AVPacketList* */IntPtr parse_queue_end;
            public int raw_packet_buffer_remaining_size;
            public long offset;
            public AVRational offset_timebase;
            public int io_repositioned;
            public /*AVCodec* */IntPtr video_codec;
            public /*AVCodec* */IntPtr audio_codec;
            public /*AVCodec* */IntPtr subtitle_codec;
        }

		private AVFormatContextInternal _st;
        private System.IO.Stream _cStream;
        private IntPtr _pAVIOBuffer;
        private IntPtr _pAVIOContext;
		private bool _bMarshalOutNeeds;
		private bool _bMarshalInNeeds;

		public uint nStreamsQty
		{
			get
			{
				MarshalOut();
				return _st.nb_streams;
			}
		}
		public AVStream[] aStreams
		{
			get
			{
				AVStream[] aRetVal = new AVStream[nStreamsQty];
				for (int nIndx = 0; nIndx < nStreamsQty; nIndx++)
				{
					aRetVal[nIndx] = new AVStream();
					aRetVal[nIndx] = (AVStream)Marshal.PtrToStructure(Marshal.ReadIntPtr(_st.streams + (int)(nIndx * IntPtr.Size)), typeof(AVStream));
				}
				return aRetVal;
			}
		}
        public IntPtr iformat
        {
            get
            {
                MarshalOut();
                return _st.iformat;
            }
        }
        public IntPtr oformat
        {
            get
            {
                MarshalOut();
                return _st.oformat;
            }
        }
        public IntPtr pb
		{
			get
			{
				MarshalOut();
				return _st.pb;
			}
			set
			{
				MarshalOut();
				_st.pb = value;
				_bMarshalInNeeds = true;
			}
		}

        static public ffmpeg.net.AVFormatContext OpenInput(string sFile)
        {
            return OpenInput(sFile, null);
        }
		static public ffmpeg.net.AVFormatContext OpenInput(string sFile, string sFormat)
		{
			net.AVFormatContext cRetVal = new net.AVFormatContext();
			int nError = 0;
            IntPtr pFormat = NULL;
            if(null != sFormat)
                pFormat = av_find_input_format(sFormat);
			//lock (helper._oSyncRootGlobal)
			{
                if (0 != (nError = avformat_open_input(ref cRetVal._p, Encoding.UTF8.GetBytes(sFile), pFormat, NULL)))
					throw new Exception(helper.ErrorDescriptionGet(nError)); // Couldn't open file
			}
			cRetVal._bMarshalOutNeeds = true;
			return cRetVal;
		}
        AVIOBufferReadWriteDelegate fAVIOBufferRead;
        AVIOBufferReadWriteDelegate fAVIOBufferWrite;
        AVIOBufferSeekDelegate fAVIOBufferSeek;
        static public ffmpeg.net.AVFormatContext OpenInput(System.IO.Stream cStream)
        {
            return OpenInput(cStream, null);
        }
        static public ffmpeg.net.AVFormatContext OpenInput(System.IO.Stream cStream, string sFormat)
        {
            net.AVFormatContext cRetVal = new net.AVFormatContext();
			int nError = 0;
            cRetVal._cStream = cStream;
			//lock (helper._oSyncRootGlobal)
			{
                cRetVal._p = avformat_alloc_context();
                cRetVal._st = (AVFormatContextInternal)Marshal.PtrToStructure(cRetVal._p, typeof(AVFormatContextInternal));
                int nBufferSize = 32768;
                cRetVal._pAVIOBuffer = av_malloc(nBufferSize + Constants.FF_INPUT_BUFFER_PADDING_SIZE);
                cRetVal.fAVIOBufferRead = cRetVal.StreamRead;
                cRetVal.fAVIOBufferSeek = cRetVal.StreamSeek;
                cRetVal._st.pb = cRetVal._pAVIOContext = avio_alloc_context(cRetVal._pAVIOBuffer, nBufferSize, 0, NULL, Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferRead), Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferSeek), Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferSeek));
                Marshal.StructureToPtr(cRetVal._st, cRetVal._p, true);

                IntPtr pFormat = NULL;
                if (null != sFormat)
                    pFormat = av_find_input_format(sFormat);
                if (0 != (nError = avformat_open_input(ref cRetVal._p, "buffer" + cStream.GetHashCode() + ".ts", pFormat, NULL)))
					throw new Exception(helper.ErrorDescriptionGet(nError)); // Couldn't open file
			}
			cRetVal._bMarshalOutNeeds = true;
			return cRetVal;
		}
		static public ffmpeg.net.AVFormatContext CreateOutput(string sFile)
		{
			net.AVFormatContext cRetVal = new net.AVFormatContext();
			//lock (helper._oSyncRootGlobal)
			{
                avformat_alloc_output_context2(ref cRetVal._p, NULL, null, new StringBuilder(sFile));
				if (NULL == cRetVal._p)
					throw new Exception("avformat_alloc_output_context2");
			}
			cRetVal._bMarshalOutNeeds = true;
			return cRetVal;
		}
		static public ffmpeg.net.AVFormatContext CreateOutput(string sType, System.IO.Stream cStream)
		{
            net.AVFormatContext cRetVal = new net.AVFormatContext();
            int nError = 0;
            cRetVal._cStream = cStream;
            //lock (helper._oSyncRootGlobal)
            {
                if (0 != (nError = avformat_alloc_output_context2(ref cRetVal._p, NULL, new StringBuilder(sType), null)))
                    throw new Exception("avformat_alloc_output_context2:" + helper.ErrorDescriptionGet(nError)); // Couldn't open file
                //cRetVal._p = avformat_alloc_context();
                cRetVal._st = (AVFormatContextInternal)Marshal.PtrToStructure(cRetVal._p, typeof(AVFormatContextInternal));
                int nBufferSize = 32768;
                cRetVal._pAVIOBuffer = av_malloc(nBufferSize + Constants.FF_INPUT_BUFFER_PADDING_SIZE);
                cRetVal.fAVIOBufferRead = cRetVal.StreamRead;
                cRetVal.fAVIOBufferSeek = cRetVal.StreamSeek;
                cRetVal.fAVIOBufferWrite = cRetVal.StreamWrite;
                cRetVal._st.pb = cRetVal._pAVIOContext = avio_alloc_context(cRetVal._pAVIOBuffer, nBufferSize, 1, NULL, Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferRead), Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferWrite), Marshal.GetFunctionPointerForDelegate(cRetVal.fAVIOBufferSeek));
                if ("mpegts" == sType)
                    av_opt_set(cRetVal._st.priv_data, new StringBuilder("mpegts_flags"), new StringBuilder("resend_headers"), 0);
                Marshal.StructureToPtr(cRetVal._st, cRetVal._p, true);
            }
            cRetVal._bMarshalOutNeeds = true;
            return cRetVal;
		}

		private AVFormatContext()
		{
			_st = new AVFormatContextInternal();
			_bMarshalOutNeeds = true;
		}

		public void Dispose()
		{
			Close();
		}

		private void MarshalOut()
		{
			if (_bMarshalOutNeeds)
			{
				_st = (AVFormatContextInternal)Marshal.PtrToStructure(_p, typeof(AVFormatContextInternal));
				_bMarshalOutNeeds = false;
			}
		}
		private void MarshalIn()
		{
			if (_bMarshalInNeeds)
			{
				Marshal.StructureToPtr(_st, _p, true);
				_bMarshalInNeeds = false;
			}
		}
		public void SaveOutput(string sFile)
		{
			int nError = 0;
			//lock (helper._oSyncRootGlobal)
			{
                IntPtr pAVIOContext = av_malloc(IntPtr.Size);
                if (0 > (nError = Functions.avio_open(ref pAVIOContext, new StringBuilder(sFile), Constants.AVIO_FLAG_WRITE)))
					throw new Exception(helper.ErrorDescriptionGet(nError)); // Couldn't open file
                pb = pAVIOContext;
			}
		}
		public void Close()
		{
			//IntPtr pStream;
			//AVStream stAVStream;
			//for (int nIndx = 0; nIndx < nStreamsQty; nIndx++)
			//{
			//    pStream = Marshal.ReadIntPtr(_st.streams + (int)(nIndx * IntPtr.Size));
			//    stAVStream = (AVStream)Marshal.PtrToStructure(pStream, typeof(AVStream));
			//    Functions.av_free(stAVStream.codec);
			//    Functions.av_free(pStream);
			//}
			if (NULL != _p)
			{
                if (NULL != oformat)
                {
                    if (NULL != pb)// && NULL == _pAVIOContext)
                        avio_close(pb);
					av_freep(ref _p);
				}
                else if (NULL != iformat)
					Functions.avformat_close_input(ref _p);
				_p = NULL;
				_bMarshalOutNeeds = true;
			}
            if (NULL != _pAVIOBuffer)
            {
                av_freep(ref _pAVIOBuffer);
				av_freep(ref _pAVIOContext);
            }
        }
		public void StreamInfoFind()
		{
			MarshalIn();
			int nError = 0;
			//lock (helper._oSyncRootGlobal)
				if (0 > (nError = avformat_find_stream_info(_p, NULL)))
					throw new Exception(helper.ErrorDescriptionGet(nError)); // Couldn't open file
			_bMarshalOutNeeds = true;
		}
		public void Seek(int _nVideoStreamIndx, long nFrameTarget)
		{
			MarshalIn();
			int nError = 0;
			if (0 > (nError = avformat_seek_file(_p, _nVideoStreamIndx, nFrameTarget, nFrameTarget, nFrameTarget, 0)))
				throw new Exception(helper.ErrorDescriptionGet(nError));
			_bMarshalOutNeeds = true;
		}
		public int PacketRead(IntPtr pPacket)
		{
			MarshalIn();
			_bMarshalOutNeeds = true;
            int nRetVal = av_read_frame(_p, pPacket);
            if(-1 < nRetVal)
                nRetVal = av_dup_packet(pPacket);
            return nRetVal;
		}
		public void PacketWrite(IntPtr pPacket)
		{
			MarshalIn();
            int nRetVal = av_interleaved_write_frame(_p, pPacket);
            //int nRetVal = av_write_frame(_p, pPacket);
            if (0 != nRetVal)
				throw new Exception("error while writing frame");
			_bMarshalOutNeeds = true;
		}
		public void WriteHeader()
		{
			MarshalIn();
			//lock (helper._oSyncRootGlobal)
				Functions.avformat_write_header(_p, NULL);
			_bMarshalOutNeeds = true;
		}
		public void WriteTrailer()
		{
			MarshalIn();
			//lock (helper._oSyncRootGlobal)
				av_write_trailer(_p); 
			_bMarshalOutNeeds = true;
		}
		public IntPtr StreamAdd()
		{
			MarshalIn();
			IntPtr pStream; // 2012_08_02
			//lock (helper._oSyncRootGlobal)
				pStream = Functions.avformat_new_stream(_p, NULL);
			if (NULL == pStream)
				throw new Exception("could not allocate stream");
			_bMarshalOutNeeds = true;
			return pStream;
		}

        private int StreamRead(IntPtr opaque, IntPtr buf, int buf_size)
        {
            int nRetVal = 0;
            if (NULL != buf)
            {
                byte[] aBytes = new byte[buf_size];
                lock(_cStream)
                    nRetVal = _cStream.Read(aBytes, 0, aBytes.Length);
                if(0 < nRetVal)
                    Marshal.Copy(aBytes, 0, buf, aBytes.Length);
            }
            else
            {
                while (true)
                {
                    lock (_cStream)
                    {
                        if (_cStream.Position + buf_size <= _cStream.Length)
                        {
                            _cStream.Position += buf_size;
                            nRetVal = buf_size;
                            break;
                        }
                    }
                    System.Threading.Thread.Sleep(0);
                }
            }
            return nRetVal;
        }
        private int StreamWrite(IntPtr opaque, IntPtr buf, int buf_size)
        {
            int nRetVal = 0;
            try
            {
                if (NULL != buf && null != _cStream)
                {
                    byte[] aBytes = new byte[buf_size];
                    Marshal.Copy(buf, aBytes, 0, aBytes.Length);
                    lock (_cStream)
                        _cStream.Write(aBytes, 0, aBytes.Length);
                    nRetVal = buf_size;
                }
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
            if (1 > nRetVal)
                (new Logger()).WriteWarning("AVFormatContext.StreamWrite is returning 0");
            return nRetVal;
        }
        private long StreamSeek(IntPtr opaque, long offset, int whence)
        {
            if (null == _cStream || !_cStream.CanSeek)
            {
                (new Logger()).WriteWarning("null == _cStream || !_cStream.CanSeek");
                return -1;
            }
            System.IO.SeekOrigin eSeekOrigin = System.IO.SeekOrigin.Begin;
            switch((AVSeek)whence)
            {
                case AVSeek.SIZE:
                    return -1;
                    lock (_cStream)
                        return _cStream.Length;
                case AVSeek.FLAG_BEGINNING:
                    eSeekOrigin = System.IO.SeekOrigin.Begin;
                    break;
                case AVSeek.FLAG_BACKWARD:
                    eSeekOrigin = System.IO.SeekOrigin.End;
                    break;
                case AVSeek.FLAG_BYTE:
                    eSeekOrigin = System.IO.SeekOrigin.Current;
                    lock (_cStream)
                        if (0 > offset && 0 > _cStream.Position + offset)
                            offset = 0;
                    break;
                case AVSeek.FLAG_FRAME:
                    (new Logger()).WriteWarning("StreamSeek:FLAG_FRAME");
                    return -1;
                case AVSeek.FLAG_ANY:
                    (new Logger()).WriteWarning("StreamSeek:FLAG_ANY");
                    return -1;
                case AVSeek.FLAG_FORCE:
                    (new Logger()).WriteWarning("StreamSeek:FLAG_FORCE");
                    return -1;
                default:
                    (new Logger()).WriteWarning("StreamSeek:UNKNOWN:" + whence);
                    return -1;
            }
            try
            {
                lock (_cStream)
                {
                    _cStream.Seek(offset, eSeekOrigin);
                    return _cStream.Position;
                }
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
                (new Logger()).WriteNotice(eSeekOrigin + ":" + offset + ":" + whence);
            }
            return -1;
        }
    }
}