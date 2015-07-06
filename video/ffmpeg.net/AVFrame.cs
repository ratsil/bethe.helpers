﻿// /**
//  * This structure describes decoded (raw) audio or video data.
//  *
//  * AVFrame must be allocated using av_frame_alloc(). Note that this only
//  * allocates the AVFrame itself, the buffers for the data must be managed
//  * through other means (see below).
//  * AVFrame must be freed with av_frame_free().
//  *
//  * AVFrame is typically allocated once and then reused multiple times to hold
//  * different data (e.g. a single AVFrame to hold frames received from a
//  * decoder). In such a case, av_frame_unref() will free any references held by
//  * the frame and reset it to its original clean state before it
//  * is reused again.
//  *
//  * The data described by an AVFrame is usually reference counted through the
//  * AVBuffer API. The underlying buffer references are stored in AVFrame.buf /
//  * AVFrame.extended_buf. An AVFrame is considered to be reference counted if at
//  * least one reference is set, i.e. if AVFrame.buf[0] != NULL. In such a case,
//  * every single data plane must be contained in one of the buffers in
//  * AVFrame.buf or AVFrame.extended_buf.
//  * There may be a single buffer for all the data, or one separate buffer for
//  * each plane, or anything in between.
//  *
//  * sizeof(AVFrame) is not a part of the public ABI, so new fields may be added
//  * to the end with a minor bump.
//  * Similarly fields that are marked as to be only accessed by
//  * av_opt_ptr() can be reordered. This allows 2 forks to add fields
//  * without breaking compatibility with each other.
//  */
// typedef struct AVFrame {
// #define AV_NUM_DATA_POINTERS 8
//     /**
//      * pointer to the picture/channel planes.
//      * This might be different from the first allocated byte
//      *
//      * Some decoders access areas outside 0,0 - width,height, please
//      * see avcodec_align_dimensions2(). Some filters and swscale can read
//      * up to 16 bytes beyond the planes, if these filters are to be used,
//      * then 16 extra bytes must be allocated.
//      */
//     uint8_t *data[AV_NUM_DATA_POINTERS];
// 
//     /**
//      * For video, size in bytes of each picture line.
//      * For audio, size in bytes of each plane.
//      *
//      * For audio, only linesize[0] may be set. For planar audio, each channel
//      * plane must be the same size.
//      *
//      * For video the linesizes should be multiplies of the CPUs alignment
//      * preference, this is 16 or 32 for modern desktop CPUs.
//      * Some code requires such alignment other code can be slower without
//      * correct alignment, for yet other it makes no difference.
//      *
//      * @note The linesize may be larger than the size of usable data -- there
//      * may be extra padding present for performance reasons.
//      */
//     int linesize[AV_NUM_DATA_POINTERS];
// 
//     /**
//      * pointers to the data planes/channels.
//      *
//      * For video, this should simply point to data[].
//      *
//      * For planar audio, each channel has a separate data pointer, and
//      * linesize[0] contains the size of each channel buffer.
//      * For packed audio, there is just one data pointer, and linesize[0]
//      * contains the total size of the buffer for all channels.
//      *
//      * Note: Both data and extended_data should always be set in a valid frame,
//      * but for planar audio with more channels that can fit in data,
//      * extended_data must be used in order to access all channels.
//      */
//     uint8_t **extended_data;
// 
//     /**
//      * width and height of the video frame
//      */
//     int width, height;
// 
//     /**
//      * number of audio samples (per channel) described by this frame
//      */
//     int nb_samples;
// 
//     /**
//      * format of the frame, -1 if unknown or unset
//      * Values correspond to enum AVPixelFormat for video frames,
//      * enum AVSampleFormat for audio)
//      */
//     int format;
// 
//     /**
//      * 1 -> keyframe, 0-> not
//      */
//     int key_frame;
// 
//     /**
//      * Picture type of the frame.
//      */
//     enum AVPictureType pict_type;
// 
// #if FF_API_AVFRAME_LAVC
//     attribute_deprecated
//     uint8_t *base[AV_NUM_DATA_POINTERS];
// #endif
// 
//     /**
//      * Sample aspect ratio for the video frame, 0/1 if unknown/unspecified.
//      */
//     AVRational sample_aspect_ratio;
// 
//     /**
//      * Presentation timestamp in time_base units (time when frame should be shown to user).
//      */
//     int64_t pts;
// 
//     /**
//      * PTS copied from the AVPacket that was decoded to produce this frame.
//      */
//     int64_t pkt_pts;
// 
//     /**
//      * DTS copied from the AVPacket that triggered returning this frame. (if frame threading isnt used)
//      * This is also the Presentation time of this AVFrame calculated from
//      * only AVPacket.dts values without pts values.
//      */
//     int64_t pkt_dts;
// 
//     /**
//      * picture number in bitstream order
//      */
//     int coded_picture_number;
//     /**
//      * picture number in display order
//      */
//     int display_picture_number;
// 
//     /**
//      * quality (between 1 (good) and FF_LAMBDA_MAX (bad))
//      */
//     int quality;
// 
// #if FF_API_AVFRAME_LAVC
//     attribute_deprecated
//     int reference;
// 
//     /**
//      * QP table
//      */
//     attribute_deprecated
//     int8_t *qscale_table;
//     /**
//      * QP store stride
//      */
//     attribute_deprecated
//     int qstride;
// 
//     attribute_deprecated
//     int qscale_type;
// 
//     /**
//      * mbskip_table[mb]>=1 if MB didn't change
//      * stride= mb_width = (width+15)>>4
//      */
//     attribute_deprecated
//     uint8_t *mbskip_table;
// 
//     /**
//      * motion vector table
//      * @code
//      * example:
//      * int mv_sample_log2= 4 - motion_subsample_log2;
//      * int mb_width= (width+15)>>4;
//      * int mv_stride= (mb_width << mv_sample_log2) + 1;
//      * motion_val[direction][x + y*mv_stride][0->mv_x, 1->mv_y];
//      * @endcode
//      */
//     attribute_deprecated
//     int16_t (*motion_val[2])[2];
// 
//     /**
//      * macroblock type table
//      * mb_type_base + mb_width + 2
//      */
//     attribute_deprecated
//     uint32_t *mb_type;
// 
//     /**
//      * DCT coefficients
//      */
//     attribute_deprecated
//     short *dct_coeff;
// 
//     /**
//      * motion reference frame index
//      * the order in which these are stored can depend on the codec.
//      */
//     attribute_deprecated
//     int8_t *ref_index[2];
// #endif
// 
//     /**
//      * for some private data of the user
//      */
//     void *opaque;
// 
//     /**
//      * error
//      */
//     uint64_t error[AV_NUM_DATA_POINTERS];
// 
// #if FF_API_AVFRAME_LAVC
//     attribute_deprecated
//     int type;
// #endif
// 
//     /**
//      * When decoding, this signals how much the picture must be delayed.
//      * extra_delay = repeat_pict / (2*fps)
//      */
//     int repeat_pict;
// 
//     /**
//      * The content of the picture is interlaced.
//      */
//     int interlaced_frame;
// 
//     /**
//      * If the content is interlaced, is top field displayed first.
//      */
//     int top_field_first;
// 
//     /**
//      * Tell user application that palette has changed from previous frame.
//      */
//     int palette_has_changed;
// 
// #if FF_API_AVFRAME_LAVC
//     attribute_deprecated
//     int buffer_hints;
// 
//     /**
//      * Pan scan.
//      */
//     attribute_deprecated
//     struct AVPanScan *pan_scan;
// #endif
// 
//     /**
//      * reordered opaque 64bit (generally an integer or a double precision float
//      * PTS but can be anything).
//      * The user sets AVCodecContext.reordered_opaque to represent the input at
//      * that time,
//      * the decoder reorders values as needed and sets AVFrame.reordered_opaque
//      * to exactly one of the values provided by the user through AVCodecContext.reordered_opaque
//      * @deprecated in favor of pkt_pts
//      */
//     int64_t reordered_opaque;
// 
// #if FF_API_AVFRAME_LAVC
//     /**
//      * @deprecated this field is unused
//      */
//     attribute_deprecated void *hwaccel_picture_private;
// 
//     attribute_deprecated
//     struct AVCodecContext *owner;
//     attribute_deprecated
//     void *thread_opaque;
// 
//     /**
//      * log2 of the size of the block which a single vector in motion_val represents:
//      * (4->16x16, 3->8x8, 2-> 4x4, 1-> 2x2)
//      */
//     attribute_deprecated
//     uint8_t motion_subsample_log2;
// #endif
// 
//     /**
//      * Sample rate of the audio data.
//      */
//     int sample_rate;
// 
//     /**
//      * Channel layout of the audio data.
//      */
//     uint64_t channel_layout;
// 
//     /**
//      * AVBuffer references backing the data for this frame. If all elements of
//      * this array are NULL, then this frame is not reference counted.
//      *
//      * There may be at most one AVBuffer per data plane, so for video this array
//      * always contains all the references. For planar audio with more than
//      * AV_NUM_DATA_POINTERS channels, there may be more buffers than can fit in
//      * this array. Then the extra AVBufferRef pointers are stored in the
//      * extended_buf array.
//      */
//     AVBufferRef *buf[AV_NUM_DATA_POINTERS];
// 
//     /**
//      * For planar audio which requires more than AV_NUM_DATA_POINTERS
//      * AVBufferRef pointers, this array will hold all the references which
//      * cannot fit into AVFrame.buf.
//      *
//      * Note that this is different from AVFrame.extended_data, which always
//      * contains all the pointers. This array only contains the extra pointers,
//      * which cannot fit into AVFrame.buf.
//      *
//      * This array is always allocated using av_malloc() by whoever constructs
//      * the frame. It is freed in av_frame_unref().
//      */
//     AVBufferRef **extended_buf;
//     /**
//      * Number of elements in extended_buf.
//      */
//     int        nb_extended_buf;
// 
//     AVFrameSideData **side_data;
//     int            nb_side_data;
// 
// /**
//  * The frame data may be corrupted, e.g. due to decoding errors.
//  */
// #define AV_FRAME_FLAG_CORRUPT       (1 << 0)
// 
//     /**
//      * Frame flags, a combination of AV_FRAME_FLAG_*
//      */
//     int flags;
// 
//     /**
//      * frame timestamp estimated using various heuristics, in stream time base
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_best_effort_timestamp(frame)
//      * - encoding: unused
//      * - decoding: set by libavcodec, read by user.
//      */
//     int64_t best_effort_timestamp;
// 
//     /**
//      * reordered pos from the last AVPacket that has been input into the decoder
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_pkt_pos(frame)
//      * - encoding: unused
//      * - decoding: Read by user.
//      */
//     int64_t pkt_pos;
// 
//     /**
//      * duration of the corresponding packet, expressed in
//      * AVStream->time_base units, 0 if unknown.
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_pkt_duration(frame)
//      * - encoding: unused
//      * - decoding: Read by user.
//      */
//     int64_t pkt_duration;
// 
//     /**
//      * metadata.
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_metadata(frame)
//      * - encoding: Set by user.
//      * - decoding: Set by libavcodec.
//      */
//     AVDictionary *metadata;
// 
//     /**
//      * decode error flags of the frame, set to a combination of
//      * FF_DECODE_ERROR_xxx flags if the decoder produced a frame, but there
//      * were errors during the decoding.
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_decode_error_flags(frame)
//      * - encoding: unused
//      * - decoding: set by libavcodec, read by user.
//      */
//     int decode_error_flags;
// #define FF_DECODE_ERROR_INVALID_BITSTREAM   1
// #define FF_DECODE_ERROR_MISSING_REFERENCE   2
// 
//     /**
//      * number of audio channels, only used for audio.
//      * Code outside libavcodec should access this field using:
//      * av_frame_get_channels(frame)
//      * - encoding: unused
//      * - decoding: Read by user.
//      */
//     int channels;
// 
//     /**
//      * size of the corresponding packet containing the compressed
//      * frame. It must be accessed using av_frame_get_pkt_size() and
//      * av_frame_set_pkt_size().
//      * It is set to a negative value if unknown.
//      * - encoding: unused
//      * - decoding: set by libavcodec, read by user.
//      */
//     int pkt_size;
// 
//     /**
//      * YUV colorspace type.
//      * It must be accessed using av_frame_get_colorspace() and
//      * av_frame_set_colorspace().
//      * - encoding: Set by user
//      * - decoding: Set by libavcodec
//      */
//     enum AVColorSpace colorspace;
// 
//     /**
//      * MPEG vs JPEG YUV range.
//      * It must be accessed using av_frame_get_color_range() and
//      * av_frame_set_color_range().
//      * - encoding: Set by user
//      * - decoding: Set by libavcodec
//      */
//     enum AVColorRange color_range;
// 
// 
//     /**
//      * Not to be accessed directly from outside libavutil
//      */
//     AVBufferRef *qp_table_buf;
// } AVFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct AVFrame
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
		public /*uint8_t *data[AV_NUM_DATA_POINTERS]*/IntPtr[] data;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
		public /*int linesize[AV_NUM_DATA_POINTERS]*/int[] linesize;
		public /*uint8_t** */IntPtr extended_data;
		public int width, height;
		public int nb_samples;
		public int format;
		public int key_frame;
		public AVPictureType pict_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
		public /*uint8_t*[AV_NUM_DATA_POINTERS] */IntPtr[] base_;
		public AVRational sample_aspect_ratio;
        public /*int64_t*/long pts;
		public /*int64_t*/long pkt_pts;
		public /*int64_t*/long pkt_dts;
		public int coded_picture_number;
		public int display_picture_number;
		public int quality;
		public int reference;
		public /*int8_t* */IntPtr qscale_table;
		public int qstride;
		public int qscale_type;
		public /*uint8_t* */IntPtr mbskip_table;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public /*int16_t (*motion_val[2])[2]*/short[] motion_val;
		public /*uint32_t* */IntPtr mb_type;
		public /*short* */IntPtr dct_coeff;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public /*int8_t*[2] */IntPtr[] ref_index;
		public /*void* */IntPtr opaque;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
		public /*uint64_t error[AV_NUM_DATA_POINTERS]*/ulong[] error;
		public int type;
		public int repeat_pict;
		public int interlaced_frame;
		public int top_field_first;
		public int palette_has_changed;
		public int buffer_hints;
		public /*AVPanScan* */IntPtr pan_scan;
		public /*int64_t*/long reordered_opaque;
		public /*void* */IntPtr hwaccel_picture_private;
		public /*struct AVCodecContext* */IntPtr owner;
		public /*void* */IntPtr thread_opaque;
        public /*uint8_t*/byte motion_subsample_log2;
		public int sample_rate;
        public ulong channel_layout;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public /*AVBufferRef *buf[AV_NUM_DATA_POINTERS]*/IntPtr[] buf;
        public /*AVBufferRef***/IntPtr extended_buf;
        public int nb_extended_buf;
        public /*AVFrameSideData***/IntPtr side_data;
        public int nb_side_data;
        public int flags;
        public long best_effort_timestamp;
        public long pkt_pos;
        public long pkt_duration;
        public /*AVDictionary**/IntPtr metadata;
        public int decode_error_flags;
        public int channels;
        public int pkt_size;
        public AVColorSpace colorspace;
        public AVColorRange color_range;
        public /*AVBufferRef**/IntPtr qp_table_buf;
	}
}