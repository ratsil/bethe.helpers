// struct AVOutputFormat {
//     const char *name;
//     /**
//      * Descriptive name for the format, meant to be more human-readable
//      * than name. You should use the NULL_IF_CONFIG_SMALL() macro
//      * to define it.
//      */
//     const char *long_name;
//     const char *mime_type;
//     const char *extensions; /**< comma-separated filename extensions */
//     /* output support */
//     enum AVCodecID audio_codec;    /**< default audio codec */
//     enum AVCodecID video_codec;    /**< default video codec */
//     enum AVCodecID subtitle_codec; /**< default subtitle codec */
//     /**
//      * can use flags: AVFMT_NOFILE, AVFMT_NEEDNUMBER, AVFMT_RAWPICTURE,
//      * AVFMT_GLOBALHEADER, AVFMT_NOTIMESTAMPS, AVFMT_VARIABLE_FPS,
//      * AVFMT_NODIMENSIONS, AVFMT_NOSTREAMS, AVFMT_ALLOW_FLUSH,
//      * AVFMT_TS_NONSTRICT
//      */
//     int flags;
// 
//     /**
//      * List of supported codec_id-codec_tag pairs, ordered by "better
//      * choice first". The arrays are all terminated by AV_CODEC_ID_NONE.
//      */
//     const struct AVCodecTag * const *codec_tag;
// 
// 
//     const AVClass *priv_class; ///< AVClass for the private context
// 
//     /*****************************************************************
//      * No fields below this line are part of the public API. They
//      * may not be used outside of libavformat and can be changed and
//      * removed at will.
//      * New public fields should be added right above.
//      *****************************************************************
//      */
//     struct AVOutputFormat *next;
//     /**
//      * size of private data so that it can be allocated in the wrapper
//      */
//     int priv_data_size;
// 
//     int (*write_header)(struct AVFormatContext *);
//     /**
//      * Write a packet. If AVFMT_ALLOW_FLUSH is set in flags,
//      * pkt can be NULL in order to flush data buffered in the muxer.
//      * When flushing, return 0 if there still is more data to flush,
//      * or 1 if everything was flushed and there is no more buffered
//      * data.
//      */
//     int (*write_packet)(struct AVFormatContext *, AVPacket *pkt);
//     int (*write_trailer)(struct AVFormatContext *);
//     /**
//      * Currently only used to set pixel format if not YUV420P.
//      */
//     int (*interleave_packet)(struct AVFormatContext *, AVPacket *out,
//                              AVPacket *in, int flush);
//     /**
//      * Test if the given codec can be stored in this container.
//      *
//      * @return 1 if the codec is supported, 0 if it is not.
//      *         A negative number if unknown.
//      *         MKTAG('A', 'P', 'I', 'C') if the codec is only supported as AV_DISPOSITION_ATTACHED_PIC
//      */
//     int (*query_codec)(enum AVCodecID id, int std_compliance);
// 
//     void (*get_output_timestamp)(struct AVFormatContext *s, int stream,
//                                  int64_t *dts, int64_t *wall);
// } AVOutputFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AVOutputFormat
	{
		public /*const char* */IntPtr name;
		public /*const char* */IntPtr long_name;
		public /*const char* */IntPtr mime_type;
		public /*const char* */IntPtr extensions;
		public AVCodecID audio_codec; /*default audio codec*/
		public AVCodecID video_codec; /*default video codec*/
		public AVCodecID subtitle_codec; /*default video codec*/
		public int flags;
		public /*AVCodecTag *const* */IntPtr codec_tag;
		public /*const AVClass* */IntPtr priv_class; ///< AVClass for the private context

		public /*AVOutputFormat* */IntPtr next;
		public int priv_data_size;
		public /*int (*write_header)(struct AVFormatContext *)*/IntPtr write_header;
		public /*int (*write_packet)(struct AVFormatContext *, AVPacket *pkt)*/IntPtr write_packet;
		public /*int (*write_trailer)(struct AVFormatContext *)*/IntPtr write_trailer;
		public /*int (*interleave_packet)(struct AVFormatContext *, AVPacket *out, AVPacket *in, int flush)*/IntPtr interleave_packet;
		public /*int (*query_codec)(enum CodecID id, int std_compliance)*/IntPtr query_codec;
		public /*void (*get_output_timestamp)(struct AVFormatContext *s, int stream, int64_t *dts, int64_t *wall)*/IntPtr get_output_timestamp;
	}
}
