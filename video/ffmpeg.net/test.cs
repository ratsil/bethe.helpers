//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.InteropServices;

//namespace ffmpeg.net
//{
//    public class test
//    {
//        static private IntPtr NULL = IntPtr.Zero;

//        static int STREAM_DURATION = 200;
//        static int STREAM_FRAME_RATE = 25; /* 25 images/s */
//        static int STREAM_NB_FRAMES = ((int)(STREAM_DURATION * STREAM_FRAME_RATE));
//        static PixelFormat STREAM_PIX_FMT = PixelFormat.PIX_FMT_YUV420P;

//        static int sws_flags = Constants.SWS_BICUBIC;

//        /**************************************************************/
//        /* audio output */

//        static public float t, tincr, tincr2;
//        static public IntPtr samples;
//        static public IntPtr audio_outbuf;
//        static public int audio_outbuf_size;
//        static public int audio_input_frame_size;
//        static public float M_PI = 3.141592654F;
//        /*
//         * add an audio output stream
//         */
//        static public IntPtr add_audio_stream(IntPtr oc, CodecID codec_id)
//        {
//            IntPtr c;
//            IntPtr st;

//            st = Functions.avformat_new_stream(oc, NULL);
//            if (NULL == st)
//                throw new Exception("avformat_new_stream");
			
//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            stStream.id = 1;

//            c = stStream.codec;

//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));
//            stCC.codec_id = codec_id;
//            stCC.codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;

//            /* put sample parameters */
//            stCC.sample_fmt = AVSampleFormat.SAMPLE_FMT_S16;
//            stCC.bit_rate = 64000;
//            stCC.sample_rate = 44100;
//            stCC.channels = 2;

//            AVFormatContext stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//            AVOutputFormat stOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(stFormatContext.oformat, typeof(AVOutputFormat));
//            // some formats want stream headers to be separate
//            if (0 < (stOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
//                stCC.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
//            Marshal.StructureToPtr(stCC, c, true);
//            Marshal.StructureToPtr(stStream, st, true);

//            return st;
//        }

//        static public void open_audio(IntPtr oc, IntPtr st)
//        {
//            IntPtr c;
//            IntPtr codec;

//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            c = stStream.codec;
//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));

//            /* find the audio encoder */
//            codec = Functions.avcodec_find_encoder(stCC.codec_id);
//            if (NULL == codec)
//                throw new Exception("avcodec_find_encoder");

//            /* open it */
//            if (Functions.avcodec_open2(c, codec, NULL) < 0)
//                throw new Exception("avcodec_open2");

//            /* init signal generator */
//            t = 0;
//            tincr = (float)(2 * M_PI * 110.0 / stCC.sample_rate);
//            /* increment frequency by 110 Hz per second */
//            tincr2 = (float)(2 * M_PI * 110.0 / stCC.sample_rate / stCC.sample_rate);

//            audio_outbuf_size = 10000;
//            audio_outbuf = Functions.av_malloc((uint)audio_outbuf_size);

//            /* ugly hack for PCM codecs (will be removed ASAP with new PCM
//               support to compute the input frame size in samples */
//            if (stCC.frame_size <= 1) {
//                audio_input_frame_size = audio_outbuf_size / stCC.channels;
//                switch(stCC.codec_id)
//                {
//                case CodecID.CODEC_ID_PCM_S16LE:
//                case CodecID.CODEC_ID_PCM_S16BE:
//                case CodecID.CODEC_ID_PCM_U16LE:
//                case CodecID.CODEC_ID_PCM_U16BE:
//                    audio_input_frame_size >>= 1;
//                    break;
//                default:
//                    break;
//                }
//            } else {
//                audio_input_frame_size = stCC.frame_size;
//            }
//            samples = Functions.av_malloc((uint)(audio_input_frame_size * 2 * stCC.channels));
//        }

//        /* prepare a 16 bit dummy audio frame of 'frame_size' samples and
//           'nb_channels' channels */
//        static public void get_audio_frame(IntPtr samples, int frame_size, int nb_channels)
//        {
//            int j, i, v;
//            IntPtr q;

//            q = samples;
//            for (j = 0; j < frame_size; j++) {
//                v = (int)(Math.Sin(t) * 10000);
//                for(i = 0; i < nb_channels; i++)
//                {
//                    Marshal.WriteInt16(q, (short)v);
//                    q+=2;
//                }
//                t += tincr;
//                tincr += tincr2;
//            }
//        }

//        static public void write_audio_frame(IntPtr oc, IntPtr st)
//        {
//            IntPtr c;
//            AVPacket stPkt = new AVPacket();
//            IntPtr pkt = Functions.av_malloc((uint)Marshal.SizeOf(stPkt));
//            Functions.av_init_packet(pkt);

//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            c = stStream.codec;

//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));
//            get_audio_frame(samples, audio_input_frame_size, stCC.channels);

//            stPkt = (AVPacket)Marshal.PtrToStructure(pkt, typeof(AVPacket));
//            stPkt.size = Functions.avcodec_encode_audio(c, audio_outbuf, audio_outbuf_size, samples);

//            stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));
//            if(NULL != stCC.coded_frame)
//            {
//                AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(stCC.coded_frame, typeof(AVFrame));
//                if (stFrame.pts != Constants.AV_NOPTS_VALUE)
//                    stPkt.pts= Functions.av_rescale_q(stFrame.pts, stCC.time_base, stStream.time_base);
//            }
//            stPkt.flags |= Constants.AV_PKT_FLAG_KEY;
//            stPkt.stream_index = stStream.index;
//            stPkt.data = audio_outbuf;

//            Marshal.StructureToPtr(stPkt, pkt, true);
//            /* write the compressed frame in the media file */
//            if (Functions.av_interleaved_write_frame(oc, pkt) != 0)
//                throw new Exception("av_interleaved_write_frame:audio");
//        }

//        static public void close_audio(IntPtr oc, IntPtr st)
//        {
//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            Functions.avcodec_close(stStream.codec);

//            Functions.av_free(samples);
//            Functions.av_free(audio_outbuf);
//        }

//        /**************************************************************/
//        /* video output */

//        static public IntPtr picture;
//        static public IntPtr tmp_picture;
//        static public IntPtr video_outbuf;
//        static public int frame_count, video_outbuf_size;

//        /* add a video output stream */
//        static public IntPtr add_video_stream(IntPtr oc, CodecID codec_id)
//        {
//            IntPtr c;
//            IntPtr st;
//            IntPtr codec;

//            st = Functions.avformat_new_stream(oc, NULL);
//            if (NULL == st)
//                throw new Exception("avformat_new_stream:video");

//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            c = stStream.codec;

//            /* find the video encoder */
//            codec = Functions.avcodec_find_encoder(codec_id);
//            if (NULL == codec)
//                throw new Exception("avcodec_find_encoder:video");
//            Functions.avcodec_get_context_defaults3(c, codec);

//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));
//            stCC.codec_id = codec_id;

//            /* put sample parameters */
//            stCC.bit_rate = 400000;
//            /* resolution must be a multiple of two */
//            stCC.width = 720;
//            stCC.height = 576;
//            /* time base: this is the fundamental unit of time (in seconds) in terms
//               of which frame timestamps are represented. for fixed-fps content,
//               timebase should be 1/framerate and timestamp increments should be
//               identically 1. */
//            stCC.time_base.den = STREAM_FRAME_RATE;
//            stCC.time_base.num = 1;
//            stCC.gop_size = 250; /* emit one intra frame every twelve frames at most */
//            stCC.pix_fmt = STREAM_PIX_FMT;
//            if (stCC.codec_id == CodecID.CODEC_ID_MPEG2VIDEO) {
//                /* just for testing, we also add B frames */
//                stCC.max_b_frames = 2;
//            }
//            else if (stCC.codec_id == CodecID.CODEC_ID_MPEG1VIDEO)
//            {
//                /* Needed to avoid using macroblocks in which some coeffs overflow.
//                   This does not happen with normal video, it just happens here as
//                   the motion of the chroma plane does not match the luma plane. */
//                stCC.mb_decision = 2;
//            }
//            else if (stCC.codec_id == CodecID.CODEC_ID_H264)
//            {
//                if (0 > Functions.av_opt_set(stCC.priv_data, new StringBuilder("preset"), new StringBuilder("ultrafast"), 0))
//                    throw new Exception();
//                if (0 > Functions.av_opt_set(stCC.priv_data, new StringBuilder("tune"), new StringBuilder("film"), 0))
//                    throw new Exception();
//            }
//            AVFormatContext stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//            AVOutputFormat stOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(stFormatContext.oformat, typeof(AVOutputFormat));
//            // some formats want stream headers to be separate
//            if (0 < (stOutputFormat.flags & Constants.AVFMT_GLOBALHEADER))
//                stCC.flags |= (int)CodecFlags.CODEC_FLAG_GLOBAL_HEADER;
//            Marshal.StructureToPtr(stCC, c, true);
//            Marshal.StructureToPtr(stStream, st, true);

//            return st;
//        }

//        static public IntPtr alloc_picture(PixelFormat pix_fmt, int width, int height)
//        {
//            IntPtr picture;
//            IntPtr picture_buf;
//            int size;

//            picture = Functions.avcodec_alloc_frame();
//            if (NULL == picture)
//                return NULL;
//            size = Functions.avpicture_get_size(pix_fmt, width, height);
//            picture_buf = Functions.av_malloc((uint)size);
//            if (NULL == picture_buf) {
//                Functions.av_free(picture);
//                return NULL;
//            }
//            Functions.avpicture_fill(picture, picture_buf, pix_fmt, width, height);
//            return picture;
//        }

//        static public void open_video(IntPtr oc, IntPtr st)
//        {
//            IntPtr codec;
//            IntPtr c;
//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            c = stStream.codec;
//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));

//            /* find the video encoder */
//            codec = Functions.avcodec_find_encoder(stCC.codec_id);
//            if (NULL == codec)
//                throw new Exception("avcodec_find_encoder:video");

//            /* open the codec */
//            if (Functions.avcodec_open2(c, codec, NULL) < 0)
//                throw new Exception("avcodec_open:video");

//            video_outbuf = NULL;
//            AVFormatContext stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//            AVOutputFormat stOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(stFormatContext.oformat, typeof(AVOutputFormat));
//            if (1 > (stOutputFormat.flags & Constants.AVFMT_RAWPICTURE)) {
//                /* allocate output buffer */
//                /* XXX: API change will be done */
//                /* buffers passed into lav* can be allocated any way you prefer,
//                   as long as they're aligned enough for the architecture, and
//                   they're freed appropriately (such as using av_free for buffers
//                   allocated with av_malloc) */
//                video_outbuf_size = 6000000;
//                video_outbuf = Functions.av_malloc((uint)video_outbuf_size);
//            }

//            /* allocate the encoded raw picture */
//            picture = alloc_picture(stCC.pix_fmt, stCC.width, stCC.height);
//            if (NULL == picture)
//                throw new Exception("alloc_picture:picture");

//            /* if the output format is not YUV420P, then a temporary YUV420P
//               picture is needed too. It is then converted to the required
//               output format */
//            //tmp_picture = NULL;
//            //if (stCC.pix_fmt != PixelFormat.PIX_FMT_YUV420P) {
//            //    tmp_picture = alloc_picture(PixelFormat.PIX_FMT_YUV420P, stCC.width, stCC.height);
//            //    if (NULL == tmp_picture)
//            //        throw new Exception("alloc_picture:tmp_picture");
//            //}
//        }

//        /* prepare a dummy image */
//        static public void fill_yuv_image(IntPtr pict, int frame_index, int width, int height)
//        {
//            int x, y, i;

//            i = frame_index;
//            AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(pict, typeof(AVFrame));
//            /* Y */
//            for (y = 0; y < height; y++) {
//                for (x = 0; x < width; x++) {
//                    Marshal.WriteByte((IntPtr)(stFrame.data[0] + y * stFrame.linesize[0] + x), (byte)(x + y + i * 3));
//                }
//            }

//            /* Cb and Cr */
//            for (y = 0; y < height/2; y++) {
//                for (x = 0; x < width/2; x++) {
//                    Marshal.WriteByte((IntPtr)(stFrame.data[1] + y * stFrame.linesize[1] + x), (byte)(128 + y + i * 2));
//                    Marshal.WriteByte((IntPtr)(stFrame.data[2] + y * stFrame.linesize[2] + x), (byte)(64 + x + i * 5));
//                }
//            }
//        }

//        static public IntPtr img_convert_ctx;
//        static public void write_video_frame(IntPtr oc, IntPtr st)
//        {
//            int out_size, ret;
//            IntPtr c;
//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            c = stStream.codec;
//            AVCodecContext stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));

//            if (frame_count >= STREAM_NB_FRAMES) {
//                /* no more frame to compress. The codec has a latency of a few
//                   frames if using B frames, so we get the last frames by
//                   passing the same picture again */
//            } else {
//                if (stCC.pix_fmt != PixelFormat.PIX_FMT_YUV420P) {
//                    /* as we only generate a YUV420P picture, we must convert it
//                       to the codec pixel format if needed */
//                    if (img_convert_ctx == NULL) {
//                        img_convert_ctx = Functions.sws_getContext(stCC.width, stCC.height,
//                                                         PixelFormat.PIX_FMT_YUV420P,
//                                                         stCC.width, stCC.height,
//                                                         stCC.pix_fmt,
//                                                         sws_flags, NULL, NULL, NULL);
//                        if (img_convert_ctx == NULL)
//                            throw new Exception("sws_getContext");
//                    }
//                    fill_yuv_image(tmp_picture, frame_count, stCC.width, stCC.height);
//                    AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(tmp_picture, typeof(AVFrame));
//                    Functions.sws_scale(img_convert_ctx, stFrame.data, stFrame.linesize, 0, stCC.height, stFrame.data, stFrame.linesize);
//                } else {
//                    fill_yuv_image(picture, frame_count, stCC.width, stCC.height);
//                }
//            }


//            //if (stFormatContext.oformat->flags & AVFMT_RAWPICTURE) {
//            //    /* raw video case. The API will change slightly in the near
//            //       future for that. */
//            //    AVPacket pkt;
//            //    Functions.av_init_packet(&pkt);

//            //    pkt.flags |= AV_PKT_FLAG_KEY;
//            //    pkt.stream_index = stStream.index;
//            //    pkt.data = (uint8_t *)picture;
//            //    pkt.size = sizeof(AVPicture);

//            //    ret = Functions.av_interleaved_write_frame(oc, &pkt);
//            //} else {
//                /* encode the image */
//                out_size = Functions.avcodec_encode_video(c, video_outbuf, video_outbuf_size, picture);
//                /* if zero size, it means the image was buffered */
//                if (out_size > 0) {
//                    AVPacket stPkt = new AVPacket();
//                    IntPtr pkt = Functions.av_malloc((uint)Marshal.SizeOf(stPkt));
//                    Functions.av_init_packet(pkt);
//                    stPkt = (AVPacket)Marshal.PtrToStructure(pkt, typeof(AVPacket));

//                    stCC = (AVCodecContext)Marshal.PtrToStructure(c, typeof(AVCodecContext));
//                    if (NULL != stCC.coded_frame)
//                    {
//                        AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(stCC.coded_frame, typeof(AVFrame));
//                        if (stFrame.pts != Constants.AV_NOPTS_VALUE)
//                            stPkt.pts = Functions.av_rescale_q(stFrame.pts, stCC.time_base, stStream.time_base);
//                        if (0 < stFrame.key_frame)
//                            stPkt.flags |= Constants.AV_PKT_FLAG_KEY;
//                    }
//                    stPkt.stream_index = stStream.index;
//                    stPkt.size = out_size;
//                    stPkt.data = video_outbuf;

//                    Marshal.StructureToPtr(stPkt, pkt, true);
//                    /* write the compressed frame in the media file */
//                    ret = Functions.av_interleaved_write_frame(oc, pkt);
//                } else {
//                    ret = 0;
//                }
//            //}
//            if (ret != 0)
//                throw new Exception("write_video_frame");
//            frame_count++;
//        }
//        static public void close_video(IntPtr oc, IntPtr st)
//        {
//            AVStream stStream = (AVStream)Marshal.PtrToStructure(st, typeof(AVStream));
//            Functions.avcodec_close(stStream.codec);
//            AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(picture, typeof(AVFrame));
//            Functions.av_free(stFrame.data[0]);
//            Functions.av_free(picture);
//            if (NULL != tmp_picture) {
//                stFrame = (AVFrame)Marshal.PtrToStructure(tmp_picture, typeof(AVFrame));
//                Functions.av_free(stFrame.data[0]);
//                Functions.av_free(tmp_picture);
//            }
//            Functions.av_free(video_outbuf);
//        }

//        /**************************************************************/
//        /* media file output */

//        public void go(StringBuilder filename)
//        {
//            IntPtr fmt;
//            IntPtr oc;
//            IntPtr audio_st, video_st;
//            double audio_pts, video_pts;
//            int i;

//            /* initialize libavcodec, and register all codecs and formats */
//            Functions.av_register_all();


//            /* allocate the output media context */
//            IntPtr ppoc = Functions.av_malloc((uint)IntPtr.Size);
//            Functions.avformat_alloc_output_context2(ppoc, NULL, null, filename);
//            oc = Marshal.ReadIntPtr(ppoc);
//            Functions.av_free(ppoc);
//            if (NULL == oc)
//                throw new Exception("avformat_alloc_output_context2");
//            AVFormatContext stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//            fmt = stFormatContext.oformat;
//            AVOutputFormat stOutputFormat = (AVOutputFormat)Marshal.PtrToStructure(fmt, typeof(AVOutputFormat));

//            /* add the audio and video streams using the default format codecs
//               and initialize the codecs */
//            video_st = NULL;
//            audio_st = NULL;
//            if (stOutputFormat.video_codec != CodecID.CODEC_ID_NONE)
//            {
//                video_st = add_video_stream(oc, stOutputFormat.video_codec);
//            }
//            if (stOutputFormat.audio_codec != CodecID.CODEC_ID_NONE)
//            {
//                audio_st = add_audio_stream(oc, stOutputFormat.audio_codec);
//            }

//            //Functions.av_dump_format(oc, 0, filename, 1);

//            /* now that all the parameters are set, we can open the audio and
//               video codecs and allocate the necessary encode buffers */
//            if (NULL != video_st)
//                open_video(oc, video_st);
//            if (NULL != audio_st)
//                open_audio(oc, audio_st);

//            if (!(0 < (stOutputFormat.flags & Constants.AVFMT_NOFILE)))
//            {
//                IntPtr ppAVIOContext = Functions.av_malloc((uint)IntPtr.Size);
//                int nResult = Functions.avio_open(ppAVIOContext, filename, Constants.AVIO_FLAG_WRITE);
//                stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//                stFormatContext.pb = Marshal.ReadIntPtr(ppAVIOContext);
//                Functions.av_freep(ref ppAVIOContext);
//                if (nResult < 0)
//                    throw new Exception("could not open " + filename);
//            }
//            Marshal.StructureToPtr(stFormatContext, oc, true);
//            /* write the stream header, if any */
//            Functions.avformat_write_header(oc, NULL);
//            AVStream stStream;
//            AVFrame stFrame = (AVFrame)Marshal.PtrToStructure(picture, typeof(AVFrame));
//            stFrame.pts = 0;
//            Marshal.StructureToPtr(stFrame, picture, true);
//            for (; ; )
//            {
//                /* compute current audio and video time */
//                if (NULL != audio_st)
//                {
//                    stStream = (AVStream)Marshal.PtrToStructure(audio_st, typeof(AVStream));
//                    audio_pts = (double)stStream.pts.val * stStream.time_base.num / stStream.time_base.den;
//                }
//                else
//                    audio_pts = 0.0;

//                if (NULL != video_st)
//                {
//                    stStream = (AVStream)Marshal.PtrToStructure(video_st, typeof(AVStream));
//                    video_pts = (double)stStream.pts.val * stStream.time_base.num / stStream.time_base.den;
//                }
//                else
//                    video_pts = 0.0;

//                if ((NULL == audio_st || audio_pts >= STREAM_DURATION) &&
//                    (NULL == video_st || video_pts >= STREAM_DURATION))
//                    break;

//                /* write interleaved audio and video frames */
//                if (NULL == video_st || (NULL != video_st && NULL != audio_st && audio_pts < video_pts))
//                {
//                    write_audio_frame(oc, audio_st);
//                } else {
//                    write_video_frame(oc, video_st);
//                    stFrame = (AVFrame)Marshal.PtrToStructure(picture, typeof(AVFrame));
//                    stFrame.pts++;
//                    Marshal.StructureToPtr(stFrame, picture, true);
//                }
//            }

//            /* write the trailer, if any.  the trailer must be written
//             * before you close the CodecContexts open when you wrote the
//             * header; otherwise write_trailer may try to use memory that
//             * was freed on av_codec_close() */
//            Functions.av_write_trailer(oc);

//            /* close each codec */
//            if (NULL != video_st)
//                close_video(oc, video_st);
//            if (NULL != audio_st)
//                close_audio(oc, audio_st);

//            stFormatContext = (AVFormatContext)Marshal.PtrToStructure(oc, typeof(AVFormatContext));
//            /* free the streams */
//            //for(i = 0; i < stFormatContext.nb_streams; i++) {
//            //    stStream = (AVStream)Marshal.PtrToStructure(stFormatContext.streams + (i * IntPtr.Size), typeof(AVStream));
//            //    Functions.av_freep(stStream.codec);
//            //    Functions.av_free(stFormatContext.streams + (i * IntPtr.Size));
//            //}

//            Functions.avio_close(stFormatContext.pb);

//            /* free the stream */
//            Functions.av_free(oc);
//        }
//    }
//}
