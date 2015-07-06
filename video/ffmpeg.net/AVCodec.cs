﻿ // typedef struct AVCodec {
 //     /**
 //      * Name of the codec implementation.
 //      * The name is globally unique among encoders and among decoders (but an
 //      * encoder and a decoder can share the same name).
 //      * This is the primary way to find a codec from the user perspective.
 //      */
 //     const char *name;
 //     /**
 //      * Descriptive name for the codec, meant to be more human readable than name.
 //      * You should use the NULL_IF_CONFIG_SMALL() macro to define it.
 //      */
 //     const char *long_name;
 //     enum AVMediaType type;
 //     enum AVCodecID id;
 //     /**
 //      * Codec capabilities.
 //      * see CODEC_CAP_*
 //      */
 //     int capabilities;
 //     const AVRational *supported_framerates; ///< array of supported framerates, or NULL if any, array is terminated by {0,0}
 //     const enum AVPixelFormat *pix_fmts;     ///< array of supported pixel formats, or NULL if unknown, array is terminated by -1
 //     const int *supported_samplerates;       ///< array of supported audio samplerates, or NULL if unknown, array is terminated by 0
 //     const enum AVSampleFormat *sample_fmts; ///< array of supported sample formats, or NULL if unknown, array is terminated by -1
 //     const uint64_t *channel_layouts;         ///< array of support channel layouts, or NULL if unknown. array is terminated by 0
 // #if FF_API_LOWRES
 //     uint8_t max_lowres;                     ///< maximum value for lowres supported by the decoder, no direct access, use av_codec_get_max_lowres()
 // #endif
 //     const AVClass *priv_class;              ///< AVClass for the private context
 //     const AVProfile *profiles;              ///< array of recognized profiles, or NULL if unknown, array is terminated by {FF_PROFILE_UNKNOWN}
 // 
 //     /*****************************************************************
 //      * No fields below this line are part of the public API. They
 //      * may not be used outside of libavcodec and can be changed and
 //      * removed at will.
 //      * New public fields should be added right above.
 //      *****************************************************************
 //      */
 //     int priv_data_size;
 //     struct AVCodec *next;
 //     /**
 //      * @name Frame-level threading support functions
 //      * @{
 //      */
 //     /**
 //      * If defined, called on thread contexts when they are created.
 //      * If the codec allocates writable tables in init(), re-allocate them here.
 //      * priv_data will be set to a copy of the original.
 //      */
 //     int (*init_thread_copy)(AVCodecContext *);
 //     /**
 //      * Copy necessary context variables from a previous thread context to the current one.
 //      * If not defined, the next thread will start automatically; otherwise, the codec
 //      * must call ff_thread_finish_setup().
 //      *
 //      * dst and src will (rarely) point to the same context, in which case memcpy should be skipped.
 //      */
 //     int (*update_thread_context)(AVCodecContext *dst, const AVCodecContext *src);
 //     /** @} */
 // 
 //     /**
 //      * Private codec-specific defaults.
 //      */
 //     const AVCodecDefault *defaults;
 // 
 //     /**
 //      * Initialize codec static data, called from avcodec_register().
 //      */
 //     void (*init_static_data)(struct AVCodec *codec);
 // 
 //     int (*init)(AVCodecContext *);
 //     int (*encode_sub)(AVCodecContext *, uint8_t *buf, int buf_size,
 //                       const struct AVSubtitle *sub);
 //     /**
 //      * Encode data to an AVPacket.
 //      *
 //      * @param      avctx          codec context
 //      * @param      avpkt          output AVPacket (may contain a user-provided buffer)
 //      * @param[in]  frame          AVFrame containing the raw data to be encoded
 //      * @param[out] got_packet_ptr encoder sets to 0 or 1 to indicate that a
 //      *                            non-empty packet was returned in avpkt.
 //      * @return 0 on success, negative error code on failure
 //      */
 //     int (*encode2)(AVCodecContext *avctx, AVPacket *avpkt, const AVFrame *frame,
 //                    int *got_packet_ptr);
 //     int (*decode)(AVCodecContext *, void *outdata, int *outdata_size, AVPacket *avpkt);
 //     int (*close)(AVCodecContext *);
 //     /**
 //      * Flush buffers.
 //      * Will be called when seeking
 //      */
 //     void (*flush)(AVCodecContext *);
 // } AVCodec;		
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct AVCodec
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public /*const char* */string name;
		[MarshalAs(UnmanagedType.LPStr)]
		public /*const char* */string long_name;
		public AVMediaType type;
		public AVCodecID id;
		public int capabilities;
		public /*AVRational* */IntPtr supported_framerates; ///< array of supported framerates, or NULL if any, array is terminated by {0,0}
		public /*PixelFormat* */IntPtr pix_fmts;       ///< array of supported pixel formats, or NULL if unknown, array is terminated by -1
		public /*int* */IntPtr supported_samplerates;       ///< array of supported audio samplerates, or NULL if unknown, array is terminated by 0
		public /*SampleFormat* */IntPtr sample_fmts;   ///< array of supported sample formats, or NULL if unknown, array is terminated by -1
		public /*int64_t* */IntPtr channel_layouts;         ///< array of support channel layouts, or NULL if unknown. array is terminated by 0
		public byte max_lowres;                     ///< maximum value for lowres supported by the decoder
		public /*const AVClass* */IntPtr priv_class;              ///< AVClass for the private context
		public /*const AVProfile* */IntPtr profiles;              ///< array of recognized profiles, or NULL if unknown, array is terminated by {FF_PROFILE_UNKNOWN}

		public int priv_data_size;
		public /*AVCodec* */IntPtr next;
		public /*int (*init_thread_copy)(AVCodecContext *)*/IntPtr init_thread_copy;
		public /*int (*update_thread_context)(AVCodecContext *dst, const AVCodecContext *src)*/IntPtr update_thread_context;
		public /*const AVCodecDefault* */IntPtr defaults;
		public /*void (*init_static_data)(struct AVCodec *codec)*/IntPtr init_static_data;
		public /*int (*init)(AVCodecContext *)*/IntPtr init;
        public /*int (*encode_sub)(AVCodecContext *, uint8_t *buf, int buf_size, void *data)*/IntPtr encode_sub;
		public /*int (*encode2)(AVCodecContext *avctx, AVPacket *avpkt, const AVFrame *frame, int *got_packet_ptr)*/IntPtr encode2;
		public /*int (*decode)(AVCodecContext *, void *outdata, int *outdata_size, AVPacket *avpkt)*/IntPtr decode;
		public /*int (*close)(AVCodecContext *)*/IntPtr close;
		public /*void (*flush)(AVCodecContext *)*/IntPtr flush;
	}
}
