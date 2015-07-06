// /**
//  * This structure stores compressed data. It is typically exported by demuxers
//  * and then passed as input to decoders, or received as output from encoders and
//  * then passed to muxers.
//  *
//  * For video, it should typically contain one compressed frame. For audio it may
//  * contain several compressed frames.
//  *
//  * AVPacket is one of the few structs in FFmpeg, whose size is a part of public
//  * ABI. Thus it may be allocated on stack and no new fields can be added to it
//  * without libavcodec and libavformat major bump.
//  *
//  * The semantics of data ownership depends on the buf or destruct (deprecated)
//  * fields. If either is set, the packet data is dynamically allocated and is
//  * valid indefinitely until av_free_packet() is called (which in turn calls
//  * av_buffer_unref()/the destruct callback to free the data). If neither is set,
//  * the packet data is typically backed by some static buffer somewhere and is
//  * only valid for a limited time (e.g. until the next read call when demuxing).
//  *
//  * The side data is always allocated with av_malloc() and is freed in
//  * av_free_packet().
//  */
// typedef struct AVPacket {
//     /**
//      * A reference to the reference-counted buffer where the packet data is
//      * stored.
//      * May be NULL, then the packet data is not reference-counted.
//      */
//     AVBufferRef *buf;
//     /**
//      * Presentation timestamp in AVStream->time_base units; the time at which
//      * the decompressed packet will be presented to the user.
//      * Can be AV_NOPTS_VALUE if it is not stored in the file.
//      * pts MUST be larger or equal to dts as presentation cannot happen before
//      * decompression, unless one wants to view hex dumps. Some formats misuse
//      * the terms dts and pts/cts to mean something different. Such timestamps
//      * must be converted to true pts/dts before they are stored in AVPacket.
//      */
//     int64_t pts;
//     /**
//      * Decompression timestamp in AVStream->time_base units; the time at which
//      * the packet is decompressed.
//      * Can be AV_NOPTS_VALUE if it is not stored in the file.
//      */
//     int64_t dts;
//     uint8_t *data;
//     int   size;
//     int   stream_index;
//     /**
//      * A combination of AV_PKT_FLAG values
//      */
//     int   flags;
//     /**
//      * Additional packet data that can be provided by the container.
//      * Packet can contain several types of side information.
//      */
//     struct {
//         uint8_t *data;
//         int      size;
//         enum AVPacketSideDataType type;
//     } *side_data;
//     int side_data_elems;
// 
//     /**
//      * Duration of this packet in AVStream->time_base units, 0 if unknown.
//      * Equals next_pts - this_pts in presentation order.
//      */
//     int   duration;
// #if FF_API_DESTRUCT_PACKET
//     attribute_deprecated
//     void  (*destruct)(struct AVPacket *);
//     attribute_deprecated
//     void  *priv;
// #endif
//     int64_t pos;                            ///< byte position in stream, -1 if unknown
// 
//     /**
//      * Time difference in AVStream->time_base units from the pts of this
//      * packet to the point at which the output from the decoder has converged
//      * independent from the availability of previous frames. That is, the
//      * frames are virtually identical no matter if decoding started from
//      * the very first frame or from this keyframe.
//      * Is AV_NOPTS_VALUE if unknown.
//      * This field is not the display duration of the current packet.
//      * This field has no meaning if the packet does not have AV_PKT_FLAG_KEY
//      * set.
//      *
//      * The purpose of this field is to allow seeking in streams that have no
//      * keyframes in the conventional sense. It corresponds to the
//      * recovery point SEI in H.264 and match_time_delta in NUT. It is also
//      * essential for some types of subtitle streams to ensure that all
//      * subtitles are correctly displayed after seeking.
//      */
//     int64_t convergence_duration;
// } AVPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVPacket
	{
        public /*AVBufferRef**/IntPtr buf;
		public /*int64_t*/long pts;
		public /*int64_t*/long dts;
		public /*uint8_t* */IntPtr data;
		public int size;
		public int stream_index;
		public int flags;
		public /*struct {
			uint8_t *data;
			int      size;
			enum AVPacketSideDataType type;
		}* */IntPtr side_data;
		public int side_data_elems;
		public int duration;
		public /*void  (*destruct)(struct AVPacket *)*/IntPtr destruct;
		public /*void*  */IntPtr priv;
		public /*int64_t*/long pos;                            ///< byte position in stream, -1 if unknown
		public /*int64_t*/long convergence_duration;
	}
}
