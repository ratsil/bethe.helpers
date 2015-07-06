// /**
//  * Stream structure.
//  * New fields can be added to the end with minor version bumps.
//  * Removal, reordering and changes to existing fields require a major
//  * version bump.
//  * sizeof(AVStream) must not be used outside libav*.
//  */
// typedef struct AVStream {
//     int index;    /**< stream index in AVFormatContext */
//     /**
//      * Format-specific stream ID.
//      * decoding: set by libavformat
//      * encoding: set by the user, replaced by libavformat if left unset
//      */
//     int id;
//     /**
//      * Codec context associated with this stream. Allocated and freed by
//      * libavformat.
//      *
//      * - decoding: The demuxer exports codec information stored in the headers
//      *             here.
//      * - encoding: The user sets codec information, the muxer writes it to the
//      *             output. Mandatory fields as specified in AVCodecContext
//      *             documentation must be set even if this AVCodecContext is
//      *             not actually used for encoding.
//      */
//     AVCodecContext *codec;
//     void *priv_data;
// 
//     /**
//      * encoding: pts generation when outputting stream
//      */
//     struct AVFrac pts;
// 
//     /**
//      * This is the fundamental unit of time (in seconds) in terms
//      * of which frame timestamps are represented.
//      *
//      * decoding: set by libavformat
//      * encoding: set by libavformat in avformat_write_header. The muxer may use the
//      * user-provided value of @ref AVCodecContext.time_base "codec->time_base"
//      * as a hint.
//      */
//     AVRational time_base;
// 
//     /**
//      * Decoding: pts of the first frame of the stream in presentation order, in stream time base.
//      * Only set this if you are absolutely 100% sure that the value you set
//      * it to really is the pts of the first frame.
//      * This may be undefined (AV_NOPTS_VALUE).
//      * @note The ASF header does NOT contain a correct start_time the ASF
//      * demuxer must NOT set this.
//      */
//     int64_t start_time;
// 
//     /**
//      * Decoding: duration of the stream, in stream time base.
//      * If a source file does not specify a duration, but does specify
//      * a bitrate, this value will be estimated from bitrate and file size.
//      */
//     int64_t duration;
// 
//     int64_t nb_frames;                 ///< number of frames in this stream if known or 0
// 
//     int disposition; /**< AV_DISPOSITION_* bit field */
// 
//     enum AVDiscard discard; ///< Selects which packets can be discarded at will and do not need to be demuxed.
// 
//     /**
//      * sample aspect ratio (0 if unknown)
//      * - encoding: Set by user.
//      * - decoding: Set by libavformat.
//      */
//     AVRational sample_aspect_ratio;
// 
//     AVDictionary *metadata;
// 
//     /**
//      * Average framerate
//      */
//     AVRational avg_frame_rate;
// 
//     /**
//      * For streams with AV_DISPOSITION_ATTACHED_PIC disposition, this packet
//      * will contain the attached picture.
//      *
//      * decoding: set by libavformat, must not be modified by the caller.
//      * encoding: unused
//      */
//     AVPacket attached_pic;
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
//      * Stream information used internally by av_find_stream_info()
//      */
// #define MAX_STD_TIMEBASES (60*12+6)
//     struct {
//         int64_t last_dts;
//         int64_t duration_gcd;
//         int duration_count;
//         double (*duration_error)[2][MAX_STD_TIMEBASES];
//         int64_t codec_info_duration;
//         int64_t codec_info_duration_fields;
//         int found_decoder;
// 
//         int64_t last_duration;
// 
//         /**
//          * Those are used for average framerate estimation.
//          */
//         int64_t fps_first_dts;
//         int     fps_first_dts_idx;
//         int64_t fps_last_dts;
//         int     fps_last_dts_idx;
// 
//     } *info;
// 
//     int pts_wrap_bits; /**< number of bits in pts (used for wrapping control) */
// 
// #if FF_API_REFERENCE_DTS
//     /* a hack to keep ABI compatibility for ffmpeg and other applications, which accesses parser even
//      * though it should not */
//     int64_t do_not_use;
// #endif
//     // Timestamp generation support:
//     /**
//      * Timestamp corresponding to the last dts sync point.
//      *
//      * Initialized when AVCodecParserContext.dts_sync_point >= 0 and
//      * a DTS is received from the underlying container. Otherwise set to
//      * AV_NOPTS_VALUE by default.
//      */
//     int64_t first_dts;
//     int64_t cur_dts;
//     int64_t last_IP_pts;
//     int last_IP_duration;
// 
//     /**
//      * Number of packets to buffer for codec probing
//      */
// #define MAX_PROBE_PACKETS 2500
//     int probe_packets;
// 
//     /**
//      * Number of frames that have been demuxed during av_find_stream_info()
//      */
//     int codec_info_nb_frames;
// 
//     /* av_read_frame() support */
//     enum AVStreamParseType need_parsing;
//     struct AVCodecParserContext *parser;
// 
//     /**
//      * last packet in packet_buffer for this stream when muxing.
//      */
//     struct AVPacketList *last_in_packet_buffer;
//     AVProbeData probe_data;
// #define MAX_REORDER_DELAY 16
//     int64_t pts_buffer[MAX_REORDER_DELAY+1];
// 
//     AVIndexEntry *index_entries; /**< Only used if the format does not
//                                     support seeking natively. */
//     int nb_index_entries;
//     unsigned int index_entries_allocated_size;
// 
//     /**
//      * Real base framerate of the stream.
//      * This is the lowest framerate with which all timestamps can be
//      * represented accurately (it is the least common multiple of all
//      * framerates in the stream). Note, this value is just a guess!
//      * For example, if the time base is 1/90000 and all frames have either
//      * approximately 3600 or 1800 timer ticks, then r_frame_rate will be 50/1.
//      *
//      * Code outside avformat should access this field using:
//      * av_stream_get/set_r_frame_rate(stream)
//      */
//     AVRational r_frame_rate;
// 
//     /**
//      * Stream Identifier
//      * This is the MPEG-TS stream identifier +1
//      * 0 means unknown
//      */
//     int stream_identifier;
// 
//     int64_t interleaver_chunk_size;
//     int64_t interleaver_chunk_duration;
// 
//     /**
//      * stream probing state
//      * -1   -> probing finished
//      *  0   -> no probing requested
//      * rest -> perform probing with request_probe being the minimum score to accept.
//      * NOT PART OF PUBLIC API
//      */
//     int request_probe;
//     /**
//      * Indicates that everything up to the next keyframe
//      * should be discarded.
//      */
//     int skip_to_keyframe;
// 
//     /**
//      * Number of samples to skip at the start of the frame decoded from the next packet.
//      */
//     int skip_samples;
// 
//     /**
//      * Number of internally decoded frames, used internally in libavformat, do not access
//      * its lifetime differs from info which is why it is not in that structure.
//      */
//     int nb_decoded_frames;
// 
//     /**
//      * Timestamp offset added to timestamps before muxing
//      * NOT PART OF PUBLIC API
//      */
//     int64_t mux_ts_offset;
// 
//     /**
//      * Internal data to check for wrapping of the time stamp
//      */
//     int64_t pts_wrap_reference;
// 
//     /**
//      * Options for behavior, when a wrap is detected.
//      *
//      * Defined by AV_PTS_WRAP_ values.
//      *
//      * If correction is enabled, there are two possibilities:
//      * If the first time stamp is near the wrap point, the wrap offset
//      * will be subtracted, which will create negative time stamps.
//      * Otherwise the offset will be added.
//      */
//     int pts_wrap_behavior;
// 
// } AVStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct AVStream
	{
		public int index;    /**< stream index in AVFormatContext */
		public int id;       /**< format-specific stream ID */
		public /*AVCodecContext* */IntPtr codec; /**< codec context */
		public /*void* */IntPtr priv_data;
		public AVFrac pts;
		public AVRational time_base;
		public long start_time;
		public long duration;
		public long nb_frames;                 ///< number of frames in this stream if known or 0
		public int disposition; /**< AV_DISPOSITION_* bit field */
        public AVDiscard discard; ///< Selects which packets can be discarded at will and do not need to be demuxed.
        public AVRational sample_aspect_ratio;
		public /*AVMetadata* */IntPtr metadata;
		public AVRational avg_frame_rate;
        public AVPacket attached_pic;

		public /*struct {
		    int64_t last_dts;
		    int64_t duration_gcd;
		    int duration_count;
		    double duration_error[2][2][MAX_STD_TIMEBASES];
		    int64_t codec_info_duration;
		    int nb_decoded_frames;
		}* */IntPtr info;
        public int pts_wrap_bits; /**< number of bits in pts (used for wrapping control) */
        public long do_not_use;
        public long first_dts;
        public long cur_dts;
        public long last_IP_pts;
        public int last_IP_duration;
        public int probe_packets;
        public int codec_info_nb_frames;
        public AVStreamParseType need_parsing;
        public /*AVCodecParserContext* */IntPtr parser;
        public /*struct AVPacketList* */IntPtr last_in_packet_buffer;
        public AVProbeData probe_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)] //MAX_REORDER_DELAY=16
        public /*int64_t pts_buffer[MAX_REORDER_DELAY+1];*/long[] pts_buffer;
        public /*AVIndexEntry* */IntPtr index_entries; /**< Only used if the format does not
		                                support seeking natively. */
        public int nb_index_entries;
        public uint index_entries_allocated_size;
        public AVRational r_frame_rate;
        public int stream_identifier;
        public long interleaver_chunk_size;
        public long interleaver_chunk_duration;
        public int request_probe;
        public int skip_to_keyframe;
        public int skip_samples;
        public int nb_decoded_frames;
        public long mux_ts_offset;
        public long pts_wrap_reference;
        public int pts_wrap_behavior;
	}
}
