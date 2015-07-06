//ffmpeg-20140420-git-f57ac37
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;


namespace ffmpeg.net
{
	#region enums
    public enum AVCodecID {
        AV_CODEC_ID_NONE,

        /* video codecs */
        AV_CODEC_ID_MPEG1VIDEO,
        AV_CODEC_ID_MPEG2VIDEO, ///< preferred ID for MPEG-1/2 video decoding
        AV_CODEC_ID_MPEG2VIDEO_XVMC,
        AV_CODEC_ID_H261,
        AV_CODEC_ID_H263,
        AV_CODEC_ID_RV10,
        AV_CODEC_ID_RV20,
        AV_CODEC_ID_MJPEG,
        AV_CODEC_ID_MJPEGB,
        AV_CODEC_ID_LJPEG,
        AV_CODEC_ID_SP5X,
        AV_CODEC_ID_JPEGLS,
        AV_CODEC_ID_MPEG4,
        AV_CODEC_ID_RAWVIDEO,
        AV_CODEC_ID_MSMPEG4V1,
        AV_CODEC_ID_MSMPEG4V2,
        AV_CODEC_ID_MSMPEG4V3,
        AV_CODEC_ID_WMV1,
        AV_CODEC_ID_WMV2,
        AV_CODEC_ID_H263P,
        AV_CODEC_ID_H263I,
        AV_CODEC_ID_FLV1,
        AV_CODEC_ID_SVQ1,
        AV_CODEC_ID_SVQ3,
        AV_CODEC_ID_DVVIDEO,
        AV_CODEC_ID_HUFFYUV,
        AV_CODEC_ID_CYUV,
        AV_CODEC_ID_H264,
        AV_CODEC_ID_INDEO3,
        AV_CODEC_ID_VP3,
        AV_CODEC_ID_THEORA,
        AV_CODEC_ID_ASV1,
        AV_CODEC_ID_ASV2,
        AV_CODEC_ID_FFV1,
        AV_CODEC_ID_4XM,
        AV_CODEC_ID_VCR1,
        AV_CODEC_ID_CLJR,
        AV_CODEC_ID_MDEC,
        AV_CODEC_ID_ROQ,
        AV_CODEC_ID_INTERPLAY_VIDEO,
        AV_CODEC_ID_XAN_WC3,
        AV_CODEC_ID_XAN_WC4,
        AV_CODEC_ID_RPZA,
        AV_CODEC_ID_CINEPAK,
        AV_CODEC_ID_WS_VQA,
        AV_CODEC_ID_MSRLE,
        AV_CODEC_ID_MSVIDEO1,
        AV_CODEC_ID_IDCIN,
        AV_CODEC_ID_8BPS,
        AV_CODEC_ID_SMC,
        AV_CODEC_ID_FLIC,
        AV_CODEC_ID_TRUEMOTION1,
        AV_CODEC_ID_VMDVIDEO,
        AV_CODEC_ID_MSZH,
        AV_CODEC_ID_ZLIB,
        AV_CODEC_ID_QTRLE,
        AV_CODEC_ID_TSCC,
        AV_CODEC_ID_ULTI,
        AV_CODEC_ID_QDRAW,
        AV_CODEC_ID_VIXL,
        AV_CODEC_ID_QPEG,
        AV_CODEC_ID_PNG,
        AV_CODEC_ID_PPM,
        AV_CODEC_ID_PBM,
        AV_CODEC_ID_PGM,
        AV_CODEC_ID_PGMYUV,
        AV_CODEC_ID_PAM,
        AV_CODEC_ID_FFVHUFF,
        AV_CODEC_ID_RV30,
        AV_CODEC_ID_RV40,
        AV_CODEC_ID_VC1,
        AV_CODEC_ID_WMV3,
        AV_CODEC_ID_LOCO,
        AV_CODEC_ID_WNV1,
        AV_CODEC_ID_AASC,
        AV_CODEC_ID_INDEO2,
        AV_CODEC_ID_FRAPS,
        AV_CODEC_ID_TRUEMOTION2,
        AV_CODEC_ID_BMP,
        AV_CODEC_ID_CSCD,
        AV_CODEC_ID_MMVIDEO,
        AV_CODEC_ID_ZMBV,
        AV_CODEC_ID_AVS,
        AV_CODEC_ID_SMACKVIDEO,
        AV_CODEC_ID_NUV,
        AV_CODEC_ID_KMVC,
        AV_CODEC_ID_FLASHSV,
        AV_CODEC_ID_CAVS,
        AV_CODEC_ID_JPEG2000,
        AV_CODEC_ID_VMNC,
        AV_CODEC_ID_VP5,
        AV_CODEC_ID_VP6,
        AV_CODEC_ID_VP6F,
        AV_CODEC_ID_TARGA,
        AV_CODEC_ID_DSICINVIDEO,
        AV_CODEC_ID_TIERTEXSEQVIDEO,
        AV_CODEC_ID_TIFF,
        AV_CODEC_ID_GIF,
        AV_CODEC_ID_DXA,
        AV_CODEC_ID_DNXHD,
        AV_CODEC_ID_THP,
        AV_CODEC_ID_SGI,
        AV_CODEC_ID_C93,
        AV_CODEC_ID_BETHSOFTVID,
        AV_CODEC_ID_PTX,
        AV_CODEC_ID_TXD,
        AV_CODEC_ID_VP6A,
        AV_CODEC_ID_AMV,
        AV_CODEC_ID_VB,
        AV_CODEC_ID_PCX,
        AV_CODEC_ID_SUNRAST,
        AV_CODEC_ID_INDEO4,
        AV_CODEC_ID_INDEO5,
        AV_CODEC_ID_MIMIC,
        AV_CODEC_ID_RL2,
        AV_CODEC_ID_ESCAPE124,
        AV_CODEC_ID_DIRAC,
        AV_CODEC_ID_BFI,
        AV_CODEC_ID_CMV,
        AV_CODEC_ID_MOTIONPIXELS,
        AV_CODEC_ID_TGV,
        AV_CODEC_ID_TGQ,
        AV_CODEC_ID_TQI,
        AV_CODEC_ID_AURA,
        AV_CODEC_ID_AURA2,
        AV_CODEC_ID_V210X,
        AV_CODEC_ID_TMV,
        AV_CODEC_ID_V210,
        AV_CODEC_ID_DPX,
        AV_CODEC_ID_MAD,
        AV_CODEC_ID_FRWU,
        AV_CODEC_ID_FLASHSV2,
        AV_CODEC_ID_CDGRAPHICS,
        AV_CODEC_ID_R210,
        AV_CODEC_ID_ANM,
        AV_CODEC_ID_BINKVIDEO,
        AV_CODEC_ID_IFF_ILBM,
        AV_CODEC_ID_IFF_BYTERUN1,
        AV_CODEC_ID_KGV1,
        AV_CODEC_ID_YOP,
        AV_CODEC_ID_VP8,
        AV_CODEC_ID_PICTOR,
        AV_CODEC_ID_ANSI,
        AV_CODEC_ID_A64_MULTI,
        AV_CODEC_ID_A64_MULTI5,
        AV_CODEC_ID_R10K,
        AV_CODEC_ID_MXPEG,
        AV_CODEC_ID_LAGARITH,
        AV_CODEC_ID_PRORES,
        AV_CODEC_ID_JV,
        AV_CODEC_ID_DFA,
        AV_CODEC_ID_WMV3IMAGE,
        AV_CODEC_ID_VC1IMAGE,
        AV_CODEC_ID_UTVIDEO,
        AV_CODEC_ID_BMV_VIDEO,
        AV_CODEC_ID_VBLE,
        AV_CODEC_ID_DXTORY,
        AV_CODEC_ID_V410,
        AV_CODEC_ID_XWD,
        AV_CODEC_ID_CDXL,
        AV_CODEC_ID_XBM,
        AV_CODEC_ID_ZEROCODEC,
        AV_CODEC_ID_MSS1,
        AV_CODEC_ID_MSA1,
        AV_CODEC_ID_TSCC2,
        AV_CODEC_ID_MTS2,
        AV_CODEC_ID_CLLC,
        AV_CODEC_ID_MSS2,
        AV_CODEC_ID_VP9,
        AV_CODEC_ID_AIC,
        AV_CODEC_ID_ESCAPE130_DEPRECATED,
        AV_CODEC_ID_G2M_DEPRECATED,

        AV_CODEC_ID_BRENDER_PIX= 0x42504958,//MKBETAG('B','P','I','X'),
        AV_CODEC_ID_Y41P = 0x59343150,//MKBETAG('Y','4','1','P'),
        AV_CODEC_ID_ESCAPE130 = 0x45313330,//MKBETAG('E','1','3','0'),
        AV_CODEC_ID_EXR = 0x30455852,//MKBETAG('0','E','X','R'),
        AV_CODEC_ID_AVRP = 0x41565250,//MKBETAG('A','V','R','P'),

        AV_CODEC_ID_012V = 0x30313256,//MKBETAG('0','1','2','V'),
        AV_CODEC_ID_G2M = 0x3047324D,//MKBETAG( 0 ,'G','2','M'),
        AV_CODEC_ID_AVUI = 0x41565549,//MKBETAG('A','V','U','I'),
        AV_CODEC_ID_AYUV = 0x41595556,//MKBETAG('A','Y','U','V'),
        AV_CODEC_ID_TARGA_Y216 = 0x54323136,//MKBETAG('T','2','1','6'),
        AV_CODEC_ID_V308 = 0x56333038,//MKBETAG('V','3','0','8'),
        AV_CODEC_ID_V408 = 0x56343038,//MKBETAG('V','4','0','8'),
        AV_CODEC_ID_YUV4 = 0x59555634,//MKBETAG('Y','U','V','4'),
        AV_CODEC_ID_SANM = 0x53414E4D,//MKBETAG('S','A','N','M'),
        AV_CODEC_ID_PAF_VIDEO = 0x50414656,//MKBETAG('P','A','F','V'),
        AV_CODEC_ID_AVRN = 0x4156526E,//MKBETAG('A','V','R','n'),
        AV_CODEC_ID_CPIA = 0x43504941,//MKBETAG('C','P','I','A'),
        AV_CODEC_ID_XFACE = 0x58464143,//MKBETAG('X','F','A','C'),
        AV_CODEC_ID_SGIRLE = 0x53474952,//MKBETAG('S','G','I','R'),
        AV_CODEC_ID_MVC1 = 0x4D564331,//MKBETAG('M','V','C','1'),
        AV_CODEC_ID_MVC2 = 0x4D564332,//MKBETAG('M','V','C','2'),
        AV_CODEC_ID_SNOW = 0x534E4F57,//MKBETAG('S','N','O','W'),
        AV_CODEC_ID_WEBP = 0x57454250,//MKBETAG('W','E','B','P'),
        AV_CODEC_ID_SMVJPEG = 0x534D564A,//MKBETAG('S','M','V','J'),

        /* various PCM "codecs" */
        AV_CODEC_ID_PCM_S16LE = 0x10000,
        AV_CODEC_ID_FIRST_AUDIO = 0x10000,     ///< A dummy id pointing at the start of audio codecs
        AV_CODEC_ID_PCM_S16BE,
        AV_CODEC_ID_PCM_U16LE,
        AV_CODEC_ID_PCM_U16BE,
        AV_CODEC_ID_PCM_S8,
        AV_CODEC_ID_PCM_U8,
        AV_CODEC_ID_PCM_MULAW,
        AV_CODEC_ID_PCM_ALAW,
        AV_CODEC_ID_PCM_S32LE,
        AV_CODEC_ID_PCM_S32BE,
        AV_CODEC_ID_PCM_U32LE,
        AV_CODEC_ID_PCM_U32BE,
        AV_CODEC_ID_PCM_S24LE,
        AV_CODEC_ID_PCM_S24BE,
        AV_CODEC_ID_PCM_U24LE,
        AV_CODEC_ID_PCM_U24BE,
        AV_CODEC_ID_PCM_S24DAUD,
        AV_CODEC_ID_PCM_ZORK,
        AV_CODEC_ID_PCM_S16LE_PLANAR,
        AV_CODEC_ID_PCM_DVD,
        AV_CODEC_ID_PCM_F32BE,
        AV_CODEC_ID_PCM_F32LE,
        AV_CODEC_ID_PCM_F64BE,
        AV_CODEC_ID_PCM_F64LE,
        AV_CODEC_ID_PCM_BLURAY,
        AV_CODEC_ID_PCM_LXF,
        AV_CODEC_ID_S302M,
        AV_CODEC_ID_PCM_S8_PLANAR,
        AV_CODEC_ID_PCM_S24LE_PLANAR = 0x18505350,//MKBETAG(24,'P','S','P'),
        AV_CODEC_ID_PCM_S32LE_PLANAR = 0x20272727,//MKBETAG(32,'P','S','P'),
        AV_CODEC_ID_PCM_S16BE_PLANAR = 0x50535010,//MKBETAG('P','S','P',16),

        /* various ADPCM codecs */
        AV_CODEC_ID_ADPCM_IMA_QT = 0x11000,
        AV_CODEC_ID_ADPCM_IMA_WAV,
        AV_CODEC_ID_ADPCM_IMA_DK3,
        AV_CODEC_ID_ADPCM_IMA_DK4,
        AV_CODEC_ID_ADPCM_IMA_WS,
        AV_CODEC_ID_ADPCM_IMA_SMJPEG,
        AV_CODEC_ID_ADPCM_MS,
        AV_CODEC_ID_ADPCM_4XM,
        AV_CODEC_ID_ADPCM_XA,
        AV_CODEC_ID_ADPCM_ADX,
        AV_CODEC_ID_ADPCM_EA,
        AV_CODEC_ID_ADPCM_G726,
        AV_CODEC_ID_ADPCM_CT,
        AV_CODEC_ID_ADPCM_SWF,
        AV_CODEC_ID_ADPCM_YAMAHA,
        AV_CODEC_ID_ADPCM_SBPRO_4,
        AV_CODEC_ID_ADPCM_SBPRO_3,
        AV_CODEC_ID_ADPCM_SBPRO_2,
        AV_CODEC_ID_ADPCM_THP,
        AV_CODEC_ID_ADPCM_IMA_AMV,
        AV_CODEC_ID_ADPCM_EA_R1,
        AV_CODEC_ID_ADPCM_EA_R3,
        AV_CODEC_ID_ADPCM_EA_R2,
        AV_CODEC_ID_ADPCM_IMA_EA_SEAD,
        AV_CODEC_ID_ADPCM_IMA_EA_EACS,
        AV_CODEC_ID_ADPCM_EA_XAS,
        AV_CODEC_ID_ADPCM_EA_MAXIS_XA,
        AV_CODEC_ID_ADPCM_IMA_ISS,
        AV_CODEC_ID_ADPCM_G722,
        AV_CODEC_ID_ADPCM_IMA_APC,
        AV_CODEC_ID_VIMA = 0x56494D41,//MKBETAG('V','I','M','A'),
        AV_CODEC_ID_ADPCM_AFC = 0x41464320,//MKBETAG('A','F','C',' '),
        AV_CODEC_ID_ADPCM_IMA_OKI = 0x4F4B4920,//MKBETAG('O','K','I',' '),
        AV_CODEC_ID_ADPCM_DTK = 0x44544B20,//MKBETAG('D','T','K',' '),
        AV_CODEC_ID_ADPCM_IMA_RAD = 0x52414420,//MKBETAG('R','A','D',' '),

        /* AMR */
        AV_CODEC_ID_AMR_NB = 0x12000,
        AV_CODEC_ID_AMR_WB,

        /* RealAudio codecs*/
        AV_CODEC_ID_RA_144 = 0x13000,
        AV_CODEC_ID_RA_288,

        /* various DPCM codecs */
        AV_CODEC_ID_ROQ_DPCM = 0x14000,
        AV_CODEC_ID_INTERPLAY_DPCM,
        AV_CODEC_ID_XAN_DPCM,
        AV_CODEC_ID_SOL_DPCM,

        /* audio codecs */
        AV_CODEC_ID_MP2 = 0x15000,
        AV_CODEC_ID_MP3, ///< preferred ID for decoding MPEG audio layer 1, 2 or 3
        AV_CODEC_ID_AAC,
        AV_CODEC_ID_AC3,
        AV_CODEC_ID_DTS,
        AV_CODEC_ID_VORBIS,
        AV_CODEC_ID_DVAUDIO,
        AV_CODEC_ID_WMAV1,
        AV_CODEC_ID_WMAV2,
        AV_CODEC_ID_MACE3,
        AV_CODEC_ID_MACE6,
        AV_CODEC_ID_VMDAUDIO,
        AV_CODEC_ID_FLAC,
        AV_CODEC_ID_MP3ADU,
        AV_CODEC_ID_MP3ON4,
        AV_CODEC_ID_SHORTEN,
        AV_CODEC_ID_ALAC,
        AV_CODEC_ID_WESTWOOD_SND1,
        AV_CODEC_ID_GSM, ///< as in Berlin toast format
        AV_CODEC_ID_QDM2,
        AV_CODEC_ID_COOK,
        AV_CODEC_ID_TRUESPEECH,
        AV_CODEC_ID_TTA,
        AV_CODEC_ID_SMACKAUDIO,
        AV_CODEC_ID_QCELP,
        AV_CODEC_ID_WAVPACK,
        AV_CODEC_ID_DSICINAUDIO,
        AV_CODEC_ID_IMC,
        AV_CODEC_ID_MUSEPACK7,
        AV_CODEC_ID_MLP,
        AV_CODEC_ID_GSM_MS, /* as found in WAV */
        AV_CODEC_ID_ATRAC3,
        AV_CODEC_ID_VOXWARE,
        AV_CODEC_ID_APE,
        AV_CODEC_ID_NELLYMOSER,
        AV_CODEC_ID_MUSEPACK8,
        AV_CODEC_ID_SPEEX,
        AV_CODEC_ID_WMAVOICE,
        AV_CODEC_ID_WMAPRO,
        AV_CODEC_ID_WMALOSSLESS,
        AV_CODEC_ID_ATRAC3P,
        AV_CODEC_ID_EAC3,
        AV_CODEC_ID_SIPR,
        AV_CODEC_ID_MP1,
        AV_CODEC_ID_TWINVQ,
        AV_CODEC_ID_TRUEHD,
        AV_CODEC_ID_MP4ALS,
        AV_CODEC_ID_ATRAC1,
        AV_CODEC_ID_BINKAUDIO_RDFT,
        AV_CODEC_ID_BINKAUDIO_DCT,
        AV_CODEC_ID_AAC_LATM,
        AV_CODEC_ID_QDMC,
        AV_CODEC_ID_CELT,
        AV_CODEC_ID_G723_1,
        AV_CODEC_ID_G729,
        AV_CODEC_ID_8SVX_EXP,
        AV_CODEC_ID_8SVX_FIB,
        AV_CODEC_ID_BMV_AUDIO,
        AV_CODEC_ID_RALF,
        AV_CODEC_ID_IAC,
        AV_CODEC_ID_ILBC,
        AV_CODEC_ID_OPUS_DEPRECATED,
        AV_CODEC_ID_COMFORT_NOISE,
        AV_CODEC_ID_TAK_DEPRECATED,
        AV_CODEC_ID_FFWAVESYNTH = 0x46465753,//MKBETAG('F','F','W','S'),
        AV_CODEC_ID_SONIC = 0x534F4E43,//MKBETAG('S','O','N','C'),
        AV_CODEC_ID_SONIC_LS = 0x534F4E4C,//MKBETAG('S','O','N','L'),
        AV_CODEC_ID_PAF_AUDIO = 0x50414641,//MKBETAG('P','A','F','A'),
        AV_CODEC_ID_OPUS = 0x4F505553,//MKBETAG('O','P','U','S'),
        AV_CODEC_ID_TAK = 0x7442614B,//MKBETAG('t','B','a','K'),
        AV_CODEC_ID_EVRC = 0x73657663,//MKBETAG('s','e','v','c'),
        AV_CODEC_ID_SMV = 0x73736D76,//MKBETAG('s','s','m','v'),

        /* subtitle codecs */
        AV_CODEC_ID_FIRST_SUBTITLE = 0x17000,          ///< A dummy ID pointing at the start of subtitle codecs.
        AV_CODEC_ID_DVD_SUBTITLE = 0x17000,
        AV_CODEC_ID_DVB_SUBTITLE,
        AV_CODEC_ID_TEXT,  ///< raw UTF-8 text
        AV_CODEC_ID_XSUB,
        AV_CODEC_ID_SSA,
        AV_CODEC_ID_MOV_TEXT,
        AV_CODEC_ID_HDMV_PGS_SUBTITLE,
        AV_CODEC_ID_DVB_TELETEXT,
        AV_CODEC_ID_SRT,
        AV_CODEC_ID_MICRODVD = 0x6D445644,//MKBETAG('m','D','V','D'),
        AV_CODEC_ID_EIA_608 = 0x63363038,//MKBETAG('c','6','0','8'),
        AV_CODEC_ID_JACOSUB = 0x4A535542,//MKBETAG('J','S','U','B'),
        AV_CODEC_ID_SAMI = 0x53414D49,//MKBETAG('S','A','M','I'),
        AV_CODEC_ID_REALTEXT = 0x52545854,//MKBETAG('R','T','X','T'),
        AV_CODEC_ID_SUBVIEWER1 = 0x53625631,//MKBETAG('S','b','V','1'),
        AV_CODEC_ID_SUBVIEWER = 0x53756256,//MKBETAG('S','u','b','V'),
        AV_CODEC_ID_SUBRIP = 0x53526970,//MKBETAG('S','R','i','p'),
        AV_CODEC_ID_WEBVTT = 0x57565454,//MKBETAG('W','V','T','T'),
        AV_CODEC_ID_MPL2 = 0x4D504C32,//MKBETAG('M','P','L','2'),
        AV_CODEC_ID_VPLAYER = 0x56506C72,//MKBETAG('V','P','l','r'),
        AV_CODEC_ID_PJS = 0x50684A53,//MKBETAG('P','h','J','S'),
        AV_CODEC_ID_ASS = 0x41535320,//MKBETAG('A','S','S',' '),  ///< ASS as defined in Matroska

        /* other specific kind of codecs (generally used for attachments) */
        AV_CODEC_ID_FIRST_UNKNOWN = 0x18000,           ///< A dummy ID pointing at the start of various fake codecs.
        AV_CODEC_ID_TTF = 0x18000,
        AV_CODEC_ID_BINTEXT = 0x42545854,//MKBETAG('B','T','X','T'),
        AV_CODEC_ID_XBIN = 0x5842494E,//MKBETAG('X','B','I','N'),
        AV_CODEC_ID_IDF = 0x494446,//MKBETAG( 0 ,'I','D','F'),
        AV_CODEC_ID_OTF = 0x4F5446,//MKBETAG( 0 ,'O','T','F'),
        AV_CODEC_ID_SMPTE_KLV = 0x4B4C5641,//MKBETAG('K','L','V','A'),
        AV_CODEC_ID_DVD_NAV = 0x444E4156,//MKBETAG('D','N','A','V'),


        AV_CODEC_ID_PROBE = 0x19000, ///< codec_id is not known (like AV_CODEC_ID_NONE) but lavf should attempt to identify it

        AV_CODEC_ID_MPEG2TS = 0x20000, /**< _FAKE_ codec to indicate a raw MPEG-2 TS
                                    * stream (only used by libavformat) */
        AV_CODEC_ID_MPEG4SYSTEMS = 0x20001, /**< _FAKE_ codec to indicate a MPEG-4 Systems
                                    * stream (only used by libavformat) */
        AV_CODEC_ID_FFMETADATA = 0x21000,   ///< Dummy codec for streams containing only metadata information.
                                            

        CODEC_ID_NONE = AV_CODEC_ID_NONE,

        /* video codecs */
        CODEC_ID_MPEG1VIDEO,
        CODEC_ID_MPEG2VIDEO, ///< preferred ID for MPEG-1/2 video decoding
        CODEC_ID_MPEG2VIDEO_XVMC,
        CODEC_ID_H261,
        CODEC_ID_H263,
        CODEC_ID_RV10,
        CODEC_ID_RV20,
        CODEC_ID_MJPEG,
        CODEC_ID_MJPEGB,
        CODEC_ID_LJPEG,
        CODEC_ID_SP5X,
        CODEC_ID_JPEGLS,
        CODEC_ID_MPEG4,
        CODEC_ID_RAWVIDEO,
        CODEC_ID_MSMPEG4V1,
        CODEC_ID_MSMPEG4V2,
        CODEC_ID_MSMPEG4V3,
        CODEC_ID_WMV1,
        CODEC_ID_WMV2,
        CODEC_ID_H263P,
        CODEC_ID_H263I,
        CODEC_ID_FLV1,
        CODEC_ID_SVQ1,
        CODEC_ID_SVQ3,
        CODEC_ID_DVVIDEO,
        CODEC_ID_HUFFYUV,
        CODEC_ID_CYUV,
        CODEC_ID_H264,
        CODEC_ID_INDEO3,
        CODEC_ID_VP3,
        CODEC_ID_THEORA,
        CODEC_ID_ASV1,
        CODEC_ID_ASV2,
        CODEC_ID_FFV1,
        CODEC_ID_4XM,
        CODEC_ID_VCR1,
        CODEC_ID_CLJR,
        CODEC_ID_MDEC,
        CODEC_ID_ROQ,
        CODEC_ID_INTERPLAY_VIDEO,
        CODEC_ID_XAN_WC3,
        CODEC_ID_XAN_WC4,
        CODEC_ID_RPZA,
        CODEC_ID_CINEPAK,
        CODEC_ID_WS_VQA,
        CODEC_ID_MSRLE,
        CODEC_ID_MSVIDEO1,
        CODEC_ID_IDCIN,
        CODEC_ID_8BPS,
        CODEC_ID_SMC,
        CODEC_ID_FLIC,
        CODEC_ID_TRUEMOTION1,
        CODEC_ID_VMDVIDEO,
        CODEC_ID_MSZH,
        CODEC_ID_ZLIB,
        CODEC_ID_QTRLE,
        CODEC_ID_TSCC,
        CODEC_ID_ULTI,
        CODEC_ID_QDRAW,
        CODEC_ID_VIXL,
        CODEC_ID_QPEG,
        CODEC_ID_PNG,
        CODEC_ID_PPM,
        CODEC_ID_PBM,
        CODEC_ID_PGM,
        CODEC_ID_PGMYUV,
        CODEC_ID_PAM,
        CODEC_ID_FFVHUFF,
        CODEC_ID_RV30,
        CODEC_ID_RV40,
        CODEC_ID_VC1,
        CODEC_ID_WMV3,
        CODEC_ID_LOCO,
        CODEC_ID_WNV1,
        CODEC_ID_AASC,
        CODEC_ID_INDEO2,
        CODEC_ID_FRAPS,
        CODEC_ID_TRUEMOTION2,
        CODEC_ID_BMP,
        CODEC_ID_CSCD,
        CODEC_ID_MMVIDEO,
        CODEC_ID_ZMBV,
        CODEC_ID_AVS,
        CODEC_ID_SMACKVIDEO,
        CODEC_ID_NUV,
        CODEC_ID_KMVC,
        CODEC_ID_FLASHSV,
        CODEC_ID_CAVS,
        CODEC_ID_JPEG2000,
        CODEC_ID_VMNC,
        CODEC_ID_VP5,
        CODEC_ID_VP6,
        CODEC_ID_VP6F,
        CODEC_ID_TARGA,
        CODEC_ID_DSICINVIDEO,
        CODEC_ID_TIERTEXSEQVIDEO,
        CODEC_ID_TIFF,
        CODEC_ID_GIF,
        CODEC_ID_DXA,
        CODEC_ID_DNXHD,
        CODEC_ID_THP,
        CODEC_ID_SGI,
        CODEC_ID_C93,
        CODEC_ID_BETHSOFTVID,
        CODEC_ID_PTX,
        CODEC_ID_TXD,
        CODEC_ID_VP6A,
        CODEC_ID_AMV,
        CODEC_ID_VB,
        CODEC_ID_PCX,
        CODEC_ID_SUNRAST,
        CODEC_ID_INDEO4,
        CODEC_ID_INDEO5,
        CODEC_ID_MIMIC,
        CODEC_ID_RL2,
        CODEC_ID_ESCAPE124,
        CODEC_ID_DIRAC,
        CODEC_ID_BFI,
        CODEC_ID_CMV,
        CODEC_ID_MOTIONPIXELS,
        CODEC_ID_TGV,
        CODEC_ID_TGQ,
        CODEC_ID_TQI,
        CODEC_ID_AURA,
        CODEC_ID_AURA2,
        CODEC_ID_V210X,
        CODEC_ID_TMV,
        CODEC_ID_V210,
        CODEC_ID_DPX,
        CODEC_ID_MAD,
        CODEC_ID_FRWU,
        CODEC_ID_FLASHSV2,
        CODEC_ID_CDGRAPHICS,
        CODEC_ID_R210,
        CODEC_ID_ANM,
        CODEC_ID_BINKVIDEO,
        CODEC_ID_IFF_ILBM,
        CODEC_ID_IFF_BYTERUN1,
        CODEC_ID_KGV1,
        CODEC_ID_YOP,
        CODEC_ID_VP8,
        CODEC_ID_PICTOR,
        CODEC_ID_ANSI,
        CODEC_ID_A64_MULTI,
        CODEC_ID_A64_MULTI5,
        CODEC_ID_R10K,
        CODEC_ID_MXPEG,
        CODEC_ID_LAGARITH,
        CODEC_ID_PRORES,
        CODEC_ID_JV,
        CODEC_ID_DFA,
        CODEC_ID_WMV3IMAGE,
        CODEC_ID_VC1IMAGE,
        CODEC_ID_UTVIDEO,
        CODEC_ID_BMV_VIDEO,
        CODEC_ID_VBLE,
        CODEC_ID_DXTORY,
        CODEC_ID_V410,
        CODEC_ID_XWD,
        CODEC_ID_CDXL,
        CODEC_ID_XBM,
        CODEC_ID_ZEROCODEC,
        CODEC_ID_MSS1,
        CODEC_ID_MSA1,
        CODEC_ID_TSCC2,
        CODEC_ID_MTS2,
        CODEC_ID_CLLC,
        CODEC_ID_Y41P = 0x59343150,//MKBETAG('Y', '4', '1', 'P')
        CODEC_ID_ESCAPE130 = 0x45313330,//MKBETAG('E', '1', '3', '0'),
        CODEC_ID_EXR = 0x30455852,//MKBETAG('0', 'E', 'X', 'R'),
        CODEC_ID_AVRP = 0x41565250,//MKBETAG('A', 'V', 'R', 'P'),

        CODEC_ID_G2M = 0x47324D,//MKBETAG(0, 'G', '2', 'M'),
        CODEC_ID_AVUI = 0x41565549,//MKBETAG('A', 'V', 'U', 'I'),
        CODEC_ID_AYUV = 0x41595556,//MKBETAG('A', 'Y', 'U', 'V'),
        CODEC_ID_V308 = 0x56333038,//MKBETAG('V', '3', '0', '8'),
        CODEC_ID_V408 = 0x56343038,//MKBETAG('V', '4', '0', '8'),
        CODEC_ID_YUV4 = 0x59555634,//MKBETAG('Y', 'U', 'V', '4'),
        CODEC_ID_SANM = 0x53414E4D,//MKBETAG('S', 'A', 'N', 'M'),
        CODEC_ID_PAF_VIDEO = 0x50414656,//MKBETAG('P', 'A', 'F', 'V'),
        CODEC_ID_SNOW = AV_CODEC_ID_SNOW,

        /* various PCM "codecs" */
        CODEC_ID_FIRST_AUDIO = 0x10000,     ///< A dummy id pointing at the start of audio codecs
        CODEC_ID_PCM_S16LE = 0x10000,
        CODEC_ID_PCM_S16BE,
        CODEC_ID_PCM_U16LE,
        CODEC_ID_PCM_U16BE,
        CODEC_ID_PCM_S8,
        CODEC_ID_PCM_U8,
        CODEC_ID_PCM_MULAW,
        CODEC_ID_PCM_ALAW,
        CODEC_ID_PCM_S32LE,
        CODEC_ID_PCM_S32BE,
        CODEC_ID_PCM_U32LE,
        CODEC_ID_PCM_U32BE,
        CODEC_ID_PCM_S24LE,
        CODEC_ID_PCM_S24BE,
        CODEC_ID_PCM_U24LE,
        CODEC_ID_PCM_U24BE,
        CODEC_ID_PCM_S24DAUD,
        CODEC_ID_PCM_ZORK,
        CODEC_ID_PCM_S16LE_PLANAR,
        CODEC_ID_PCM_DVD,
        CODEC_ID_PCM_F32BE,
        CODEC_ID_PCM_F32LE,
        CODEC_ID_PCM_F64BE,
        CODEC_ID_PCM_F64LE,
        CODEC_ID_PCM_BLURAY,
        CODEC_ID_PCM_LXF,
        CODEC_ID_S302M,
        CODEC_ID_PCM_S8_PLANAR,

        /* various ADPCM codecs */
        CODEC_ID_ADPCM_IMA_QT = 0x11000,
        CODEC_ID_ADPCM_IMA_WAV,
        CODEC_ID_ADPCM_IMA_DK3,
        CODEC_ID_ADPCM_IMA_DK4,
        CODEC_ID_ADPCM_IMA_WS,
        CODEC_ID_ADPCM_IMA_SMJPEG,
        CODEC_ID_ADPCM_MS,
        CODEC_ID_ADPCM_4XM,
        CODEC_ID_ADPCM_XA,
        CODEC_ID_ADPCM_ADX,
        CODEC_ID_ADPCM_EA,
        CODEC_ID_ADPCM_G726,
        CODEC_ID_ADPCM_CT,
        CODEC_ID_ADPCM_SWF,
        CODEC_ID_ADPCM_YAMAHA,
        CODEC_ID_ADPCM_SBPRO_4,
        CODEC_ID_ADPCM_SBPRO_3,
        CODEC_ID_ADPCM_SBPRO_2,
        CODEC_ID_ADPCM_THP,
        CODEC_ID_ADPCM_IMA_AMV,
        CODEC_ID_ADPCM_EA_R1,
        CODEC_ID_ADPCM_EA_R3,
        CODEC_ID_ADPCM_EA_R2,
        CODEC_ID_ADPCM_IMA_EA_SEAD,
        CODEC_ID_ADPCM_IMA_EA_EACS,
        CODEC_ID_ADPCM_EA_XAS,
        CODEC_ID_ADPCM_EA_MAXIS_XA,
        CODEC_ID_ADPCM_IMA_ISS,
        CODEC_ID_ADPCM_G722,
        CODEC_ID_ADPCM_IMA_APC,
        CODEC_ID_VIMA = 0x56494D41,//MKBETAG('V', 'I', 'M', 'A'),

        /* AMR */
        CODEC_ID_AMR_NB = 0x12000,
        CODEC_ID_AMR_WB,

        /* RealAudio codecs*/
        CODEC_ID_RA_144 = 0x13000,
        CODEC_ID_RA_288,

        /* various DPCM codecs */
        CODEC_ID_ROQ_DPCM = 0x14000,
        CODEC_ID_INTERPLAY_DPCM,
        CODEC_ID_XAN_DPCM,
        CODEC_ID_SOL_DPCM,

        /* audio codecs */
        CODEC_ID_MP2 = 0x15000,
        CODEC_ID_MP3, ///< preferred ID for decoding MPEG audio layer 1, 2 or 3
        CODEC_ID_AAC,
        CODEC_ID_AC3,
        CODEC_ID_DTS,
        CODEC_ID_VORBIS,
        CODEC_ID_DVAUDIO,
        CODEC_ID_WMAV1,
        CODEC_ID_WMAV2,
        CODEC_ID_MACE3,
        CODEC_ID_MACE6,
        CODEC_ID_VMDAUDIO,
        CODEC_ID_FLAC,
        CODEC_ID_MP3ADU,
        CODEC_ID_MP3ON4,
        CODEC_ID_SHORTEN,
        CODEC_ID_ALAC,
        CODEC_ID_WESTWOOD_SND1,
        CODEC_ID_GSM, ///< as in Berlin toast format
        CODEC_ID_QDM2,
        CODEC_ID_COOK,
        CODEC_ID_TRUESPEECH,
        CODEC_ID_TTA,
        CODEC_ID_SMACKAUDIO,
        CODEC_ID_QCELP,
        CODEC_ID_WAVPACK,
        CODEC_ID_DSICINAUDIO,
        CODEC_ID_IMC,
        CODEC_ID_MUSEPACK7,
        CODEC_ID_MLP,
        CODEC_ID_GSM_MS, /* as found in WAV */
        CODEC_ID_ATRAC3,
        CODEC_ID_VOXWARE,
        CODEC_ID_APE,
        CODEC_ID_NELLYMOSER,
        CODEC_ID_MUSEPACK8,
        CODEC_ID_SPEEX,
        CODEC_ID_WMAVOICE,
        CODEC_ID_WMAPRO,
        CODEC_ID_WMALOSSLESS,
        CODEC_ID_ATRAC3P,
        CODEC_ID_EAC3,
        CODEC_ID_SIPR,
        CODEC_ID_MP1,
        CODEC_ID_TWINVQ,
        CODEC_ID_TRUEHD,
        CODEC_ID_MP4ALS,
        CODEC_ID_ATRAC1,
        CODEC_ID_BINKAUDIO_RDFT,
        CODEC_ID_BINKAUDIO_DCT,
        CODEC_ID_AAC_LATM,
        CODEC_ID_QDMC,
        CODEC_ID_CELT,
        CODEC_ID_G723_1,
        CODEC_ID_G729,
        CODEC_ID_8SVX_EXP,
        CODEC_ID_8SVX_FIB,
        CODEC_ID_BMV_AUDIO,
        CODEC_ID_RALF,
        CODEC_ID_IAC,
        CODEC_ID_ILBC,
        CODEC_ID_FFWAVESYNTH = 0x46465753,//MKBETAG('F', 'F', 'W', 'S'),
        CODEC_ID_SONIC = 0x534F4E43,//MKBETAG('S', 'O', 'N', 'C'),
        CODEC_ID_SONIC_LS = 0x534F4E4C,//MKBETAG('S', 'O', 'N', 'L'),
        CODEC_ID_PAF_AUDIO = 0x50414641,//MKBETAG('P', 'A', 'F', 'A'),
        CODEC_ID_OPUS = 0x4F505553,//MKBETAG('O', 'P', 'U', 'S'),

        /* subtitle codecs */
        CODEC_ID_FIRST_SUBTITLE = 0x17000,          ///< A dummy ID pointing at the start of subtitle codecs.
        CODEC_ID_DVD_SUBTITLE = 0x17000,
        CODEC_ID_DVB_SUBTITLE,
        CODEC_ID_TEXT,  ///< raw UTF-8 text
        CODEC_ID_XSUB,
        CODEC_ID_SSA,
        CODEC_ID_MOV_TEXT,
        CODEC_ID_HDMV_PGS_SUBTITLE,
        CODEC_ID_DVB_TELETEXT,
        CODEC_ID_SRT,
        CODEC_ID_MICRODVD = 0x6D445644,//MKBETAG('m', 'D', 'V', 'D'),
        CODEC_ID_EIA_608 = 0x63363038,//MKBETAG('c', '6', '0', '8'),
        CODEC_ID_JACOSUB = 0x4A535542,//MKBETAG('J', 'S', 'U', 'B'),
        CODEC_ID_SAMI = 0x53414D49,//MKBETAG('S', 'A', 'M', 'I'),
        CODEC_ID_REALTEXT = 0x52545854,//MKBETAG('R', 'T', 'X', 'T'),
        CODEC_ID_SUBVIEWER = 0x53756256,//MKBETAG('S', 'u', 'b', 'V'),

        /* other specific kind of codecs (generally used for attachments) */
        CODEC_ID_FIRST_UNKNOWN = 0x18000,           ///< A dummy ID pointing at the start of various fake codecs.
        CODEC_ID_TTF = 0x18000,
        CODEC_ID_BINTEXT = 0x42545854,//MKBETAG('B', 'T', 'X', 'T'),
        CODEC_ID_XBIN = 0x5842494E,//MKBETAG('X', 'B', 'I', 'N'),
        CODEC_ID_IDF = 0x494446,//MKBETAG(0, 'I', 'D', 'F'),
        CODEC_ID_OTF = 0x4F5446,//MKBETAG(0, 'O', 'T', 'F'),

        CODEC_ID_PROBE = 0x19000, ///< codec_id is not known (like CODEC_ID_NONE) but lavf should attempt to identify it

        CODEC_ID_MPEG2TS = 0x20000, /**< _FAKE_ codec to indicate a raw MPEG-2 TS
                                * stream (only used by libavformat) */
        CODEC_ID_MPEG4SYSTEMS = 0x20001, /**< _FAKE_ codec to indicate a MPEG-4 Systems
                                * stream (only used by libavformat) */
        CODEC_ID_FFMETADATA = 0x21000,   ///< Dummy codec for streams containing only metadata information.



        //CODEC_ID_H264_MOBILE
    };
	public enum PixelFormat
	{
        AV_PIX_FMT_NONE = -1,
        AV_PIX_FMT_YUV420P,   ///< planar YUV 4:2:0, 12bpp, (1 Cr & Cb sample per 2x2 Y samples)
        AV_PIX_FMT_YUYV422,   ///< packed YUV 4:2:2, 16bpp, Y0 Cb Y1 Cr
        AV_PIX_FMT_RGB24,     ///< packed RGB 8:8:8, 24bpp, RGBRGB...
        AV_PIX_FMT_BGR24,     ///< packed RGB 8:8:8, 24bpp, BGRBGR...
        AV_PIX_FMT_YUV422P,   ///< planar YUV 4:2:2, 16bpp, (1 Cr & Cb sample per 2x1 Y samples)
        AV_PIX_FMT_YUV444P,   ///< planar YUV 4:4:4, 24bpp, (1 Cr & Cb sample per 1x1 Y samples)
        AV_PIX_FMT_YUV410P,   ///< planar YUV 4:1:0,  9bpp, (1 Cr & Cb sample per 4x4 Y samples)
        AV_PIX_FMT_YUV411P,   ///< planar YUV 4:1:1, 12bpp, (1 Cr & Cb sample per 4x1 Y samples)
        AV_PIX_FMT_GRAY8,     ///<        Y        ,  8bpp
        AV_PIX_FMT_MONOWHITE, ///<        Y        ,  1bpp, 0 is white, 1 is black, in each byte pixels are ordered from the msb to the lsb
        AV_PIX_FMT_MONOBLACK, ///<        Y        ,  1bpp, 0 is black, 1 is white, in each byte pixels are ordered from the msb to the lsb
        AV_PIX_FMT_PAL8,      ///< 8 bit with PIX_FMT_RGB32 palette
        AV_PIX_FMT_YUVJ420P,  ///< planar YUV 4:2:0, 12bpp, full scale (JPEG), deprecated in favor of PIX_FMT_YUV420P and setting color_range
        AV_PIX_FMT_YUVJ422P,  ///< planar YUV 4:2:2, 16bpp, full scale (JPEG), deprecated in favor of PIX_FMT_YUV422P and setting color_range
        AV_PIX_FMT_YUVJ444P,  ///< planar YUV 4:4:4, 24bpp, full scale (JPEG), deprecated in favor of PIX_FMT_YUV444P and setting color_range
        AV_PIX_FMT_XVMC_MPEG2_MC,///< XVideo Motion Acceleration via common packet passing
        AV_PIX_FMT_XVMC_MPEG2_IDCT,
        AV_PIX_FMT_UYVY422,   ///< packed YUV 4:2:2, 16bpp, Cb Y0 Cr Y1
        AV_PIX_FMT_UYYVYY411, ///< packed YUV 4:1:1, 12bpp, Cb Y0 Y1 Cr Y2 Y3
        AV_PIX_FMT_BGR8,      ///< packed RGB 3:3:2,  8bpp, (msb)2B 3G 3R(lsb)
        AV_PIX_FMT_BGR4,      ///< packed RGB 1:2:1 bitstream,  4bpp, (msb)1B 2G 1R(lsb), a byte contains two pixels, the first pixel in the byte is the one composed by the 4 msb bits
        AV_PIX_FMT_BGR4_BYTE, ///< packed RGB 1:2:1,  8bpp, (msb)1B 2G 1R(lsb)
        AV_PIX_FMT_RGB8,      ///< packed RGB 3:3:2,  8bpp, (msb)2R 3G 3B(lsb)
        AV_PIX_FMT_RGB4,      ///< packed RGB 1:2:1 bitstream,  4bpp, (msb)1R 2G 1B(lsb), a byte contains two pixels, the first pixel in the byte is the one composed by the 4 msb bits
        AV_PIX_FMT_RGB4_BYTE, ///< packed RGB 1:2:1,  8bpp, (msb)1R 2G 1B(lsb)
        AV_PIX_FMT_NV12,      ///< planar YUV 4:2:0, 12bpp, 1 plane for Y and 1 plane for the UV components, which are interleaved (first byte U and the following byte V)
        AV_PIX_FMT_NV21,      ///< as above, but U and V bytes are swapped

        AV_PIX_FMT_ARGB,      ///< packed ARGB 8:8:8:8, 32bpp, ARGBARGB...
        AV_PIX_FMT_RGBA,      ///< packed RGBA 8:8:8:8, 32bpp, RGBARGBA...
        AV_PIX_FMT_ABGR,      ///< packed ABGR 8:8:8:8, 32bpp, ABGRABGR...
        AV_PIX_FMT_BGRA,      ///< packed BGRA 8:8:8:8, 32bpp, BGRABGRA...

        AV_PIX_FMT_GRAY16BE,  ///<        Y        , 16bpp, big-endian
        AV_PIX_FMT_GRAY16LE,  ///<        Y        , 16bpp, little-endian
        AV_PIX_FMT_YUV440P,   ///< planar YUV 4:4:0 (1 Cr & Cb sample per 1x2 Y samples)
        AV_PIX_FMT_YUVJ440P,  ///< planar YUV 4:4:0 full scale (JPEG), deprecated in favor of PIX_FMT_YUV440P and setting color_range
        AV_PIX_FMT_YUVA420P,  ///< planar YUV 4:2:0, 20bpp, (1 Cr & Cb sample per 2x2 Y & A samples)
        AV_PIX_FMT_VDPAU_H264,///< H.264 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_VDPAU_MPEG1,///< MPEG-1 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_VDPAU_MPEG2,///< MPEG-2 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_VDPAU_WMV3,///< WMV3 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_VDPAU_VC1, ///< VC-1 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_RGB48BE,   ///< packed RGB 16:16:16, 48bpp, 16R, 16G, 16B, the 2-byte value for each R/G/B component is stored as big-endian
        AV_PIX_FMT_RGB48LE,   ///< packed RGB 16:16:16, 48bpp, 16R, 16G, 16B, the 2-byte value for each R/G/B component is stored as little-endian

        AV_PIX_FMT_RGB565BE,  ///< packed RGB 5:6:5, 16bpp, (msb)   5R 6G 5B(lsb), big-endian
        AV_PIX_FMT_RGB565LE,  ///< packed RGB 5:6:5, 16bpp, (msb)   5R 6G 5B(lsb), little-endian
        AV_PIX_FMT_RGB555BE,  ///< packed RGB 5:5:5, 16bpp, (msb)1A 5R 5G 5B(lsb), big-endian, most significant bit to 0
        AV_PIX_FMT_RGB555LE,  ///< packed RGB 5:5:5, 16bpp, (msb)1A 5R 5G 5B(lsb), little-endian, most significant bit to 0

        AV_PIX_FMT_BGR565BE,  ///< packed BGR 5:6:5, 16bpp, (msb)   5B 6G 5R(lsb), big-endian
        AV_PIX_FMT_BGR565LE,  ///< packed BGR 5:6:5, 16bpp, (msb)   5B 6G 5R(lsb), little-endian
        AV_PIX_FMT_BGR555BE,  ///< packed BGR 5:5:5, 16bpp, (msb)1A 5B 5G 5R(lsb), big-endian, most significant bit to 1
        AV_PIX_FMT_BGR555LE,  ///< packed BGR 5:5:5, 16bpp, (msb)1A 5B 5G 5R(lsb), little-endian, most significant bit to 1

        AV_PIX_FMT_VAAPI_MOCO, ///< HW acceleration through VA API at motion compensation entry-point, Picture.data[3] contains a vaapi_render_state struct which contains macroblocks as well as various fields extracted from headers
        AV_PIX_FMT_VAAPI_IDCT, ///< HW acceleration through VA API at IDCT entry-point, Picture.data[3] contains a vaapi_render_state struct which contains fields extracted from headers
        AV_PIX_FMT_VAAPI_VLD,  ///< HW decoding through VA API, Picture.data[3] contains a vaapi_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers

        AV_PIX_FMT_YUV420P16LE,  ///< planar YUV 4:2:0, 24bpp, (1 Cr & Cb sample per 2x2 Y samples), little-endian
        AV_PIX_FMT_YUV420P16BE,  ///< planar YUV 4:2:0, 24bpp, (1 Cr & Cb sample per 2x2 Y samples), big-endian
        AV_PIX_FMT_YUV422P16LE,  ///< planar YUV 4:2:2, 32bpp, (1 Cr & Cb sample per 2x1 Y samples), little-endian
        AV_PIX_FMT_YUV422P16BE,  ///< planar YUV 4:2:2, 32bpp, (1 Cr & Cb sample per 2x1 Y samples), big-endian
        AV_PIX_FMT_YUV444P16LE,  ///< planar YUV 4:4:4, 48bpp, (1 Cr & Cb sample per 1x1 Y samples), little-endian
        AV_PIX_FMT_YUV444P16BE,  ///< planar YUV 4:4:4, 48bpp, (1 Cr & Cb sample per 1x1 Y samples), big-endian
        AV_PIX_FMT_VDPAU_MPEG4,  ///< MPEG4 HW decoding with VDPAU, data[0] contains a vdpau_render_state struct which contains the bitstream of the slices as well as various fields extracted from headers
        AV_PIX_FMT_DXVA2_VLD,    ///< HW decoding through DXVA2, Picture.data[3] contains a LPDIRECT3DSURFACE9 pointer

        AV_PIX_FMT_RGB444LE,  ///< packed RGB 4:4:4, 16bpp, (msb)4A 4R 4G 4B(lsb), little-endian, most significant bits to 0
        AV_PIX_FMT_RGB444BE,  ///< packed RGB 4:4:4, 16bpp, (msb)4A 4R 4G 4B(lsb), big-endian, most significant bits to 0
        AV_PIX_FMT_BGR444LE,  ///< packed BGR 4:4:4, 16bpp, (msb)4A 4B 4G 4R(lsb), little-endian, most significant bits to 1
        AV_PIX_FMT_BGR444BE,  ///< packed BGR 4:4:4, 16bpp, (msb)4A 4B 4G 4R(lsb), big-endian, most significant bits to 1
        AV_PIX_FMT_GRAY8A,    ///< 8bit gray, 8bit alpha
        AV_PIX_FMT_BGR48BE,   ///< packed RGB 16:16:16, 48bpp, 16B, 16G, 16R, the 2-byte value for each R/G/B component is stored as big-endian
        AV_PIX_FMT_BGR48LE,   ///< packed RGB 16:16:16, 48bpp, 16B, 16G, 16R, the 2-byte value for each R/G/B component is stored as little-endian

        //the following 10 formats have the disadvantage of needing 1 format for each bit depth, thus
        //If you want to support multiple bit depths, then using AV_PIX_FMT_YUV420P16* with the bpp stored separately
        //is better
        AV_PIX_FMT_YUV420P9BE, ///< planar YUV 4:2:0, 13.5bpp, (1 Cr & Cb sample per 2x2 Y samples), big-endian
        AV_PIX_FMT_YUV420P9LE, ///< planar YUV 4:2:0, 13.5bpp, (1 Cr & Cb sample per 2x2 Y samples), little-endian
        AV_PIX_FMT_YUV420P10BE,///< planar YUV 4:2:0, 15bpp, (1 Cr & Cb sample per 2x2 Y samples), big-endian
        AV_PIX_FMT_YUV420P10LE,///< planar YUV 4:2:0, 15bpp, (1 Cr & Cb sample per 2x2 Y samples), little-endian
        AV_PIX_FMT_YUV422P10BE,///< planar YUV 4:2:2, 20bpp, (1 Cr & Cb sample per 2x1 Y samples), big-endian
        AV_PIX_FMT_YUV422P10LE,///< planar YUV 4:2:2, 20bpp, (1 Cr & Cb sample per 2x1 Y samples), little-endian
        AV_PIX_FMT_YUV444P9BE, ///< planar YUV 4:4:4, 27bpp, (1 Cr & Cb sample per 1x1 Y samples), big-endian
        AV_PIX_FMT_YUV444P9LE, ///< planar YUV 4:4:4, 27bpp, (1 Cr & Cb sample per 1x1 Y samples), little-endian
        AV_PIX_FMT_YUV444P10BE,///< planar YUV 4:4:4, 30bpp, (1 Cr & Cb sample per 1x1 Y samples), big-endian
        AV_PIX_FMT_YUV444P10LE,///< planar YUV 4:4:4, 30bpp, (1 Cr & Cb sample per 1x1 Y samples), little-endian
        AV_PIX_FMT_YUV422P9BE, ///< planar YUV 4:2:2, 18bpp, (1 Cr & Cb sample per 2x1 Y samples), big-endian
        AV_PIX_FMT_YUV422P9LE, ///< planar YUV 4:2:2, 18bpp, (1 Cr & Cb sample per 2x1 Y samples), little-endian
        AV_PIX_FMT_VDA_VLD,    ///< hardware decoding through VDA

        AV_PIX_FMT_RGBA64BE,  ///< packed RGBA 16:16:16:16, 64bpp, 16R, 16G, 16B, 16A, the 2-byte value for each R/G/B/A component is stored as big-endian
        AV_PIX_FMT_RGBA64LE,  ///< packed RGBA 16:16:16:16, 64bpp, 16R, 16G, 16B, 16A, the 2-byte value for each R/G/B/A component is stored as little-endian
        AV_PIX_FMT_BGRA64BE,  ///< packed RGBA 16:16:16:16, 64bpp, 16B, 16G, 16R, 16A, the 2-byte value for each R/G/B/A component is stored as big-endian
        AV_PIX_FMT_BGRA64LE,  ///< packed RGBA 16:16:16:16, 64bpp, 16B, 16G, 16R, 16A, the 2-byte value for each R/G/B/A component is stored as little-endian

        AV_PIX_FMT_GBRP,      ///< planar GBR 4:4:4 24bpp
        AV_PIX_FMT_GBRP9BE,   ///< planar GBR 4:4:4 27bpp, big-endian
        AV_PIX_FMT_GBRP9LE,   ///< planar GBR 4:4:4 27bpp, little-endian
        AV_PIX_FMT_GBRP10BE,  ///< planar GBR 4:4:4 30bpp, big-endian
        AV_PIX_FMT_GBRP10LE,  ///< planar GBR 4:4:4 30bpp, little-endian
        AV_PIX_FMT_GBRP16BE,  ///< planar GBR 4:4:4 48bpp, big-endian
        AV_PIX_FMT_GBRP16LE,  ///< planar GBR 4:4:4 48bpp, little-endian

        /**
         * duplicated pixel formats for compatibility with libav.
         * FFmpeg supports these formats since May 8 2012 and Jan 28 2012 (commits f9ca1ac7 and 143a5c55)
         * Libav added them Oct 12 2012 with incompatible values (commit 6d5600e85)
         */
        AV_PIX_FMT_YUVA422P_LIBAV,  ///< planar YUV 4:2:2 24bpp, (1 Cr & Cb sample per 2x1 Y & A samples)
        AV_PIX_FMT_YUVA444P_LIBAV,  ///< planar YUV 4:4:4 32bpp, (1 Cr & Cb sample per 1x1 Y & A samples)

        AV_PIX_FMT_YUVA420P9BE,  ///< planar YUV 4:2:0 22.5bpp, (1 Cr & Cb sample per 2x2 Y & A samples), big-endian
        AV_PIX_FMT_YUVA420P9LE,  ///< planar YUV 4:2:0 22.5bpp, (1 Cr & Cb sample per 2x2 Y & A samples), little-endian
        AV_PIX_FMT_YUVA422P9BE,  ///< planar YUV 4:2:2 27bpp, (1 Cr & Cb sample per 2x1 Y & A samples), big-endian
        AV_PIX_FMT_YUVA422P9LE,  ///< planar YUV 4:2:2 27bpp, (1 Cr & Cb sample per 2x1 Y & A samples), little-endian
        AV_PIX_FMT_YUVA444P9BE,  ///< planar YUV 4:4:4 36bpp, (1 Cr & Cb sample per 1x1 Y & A samples), big-endian
        AV_PIX_FMT_YUVA444P9LE,  ///< planar YUV 4:4:4 36bpp, (1 Cr & Cb sample per 1x1 Y & A samples), little-endian
        AV_PIX_FMT_YUVA420P10BE, ///< planar YUV 4:2:0 25bpp, (1 Cr & Cb sample per 2x2 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA420P10LE, ///< planar YUV 4:2:0 25bpp, (1 Cr & Cb sample per 2x2 Y & A samples, little-endian)
        AV_PIX_FMT_YUVA422P10BE, ///< planar YUV 4:2:2 30bpp, (1 Cr & Cb sample per 2x1 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA422P10LE, ///< planar YUV 4:2:2 30bpp, (1 Cr & Cb sample per 2x1 Y & A samples, little-endian)
        AV_PIX_FMT_YUVA444P10BE, ///< planar YUV 4:4:4 40bpp, (1 Cr & Cb sample per 1x1 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA444P10LE, ///< planar YUV 4:4:4 40bpp, (1 Cr & Cb sample per 1x1 Y & A samples, little-endian)
        AV_PIX_FMT_YUVA420P16BE, ///< planar YUV 4:2:0 40bpp, (1 Cr & Cb sample per 2x2 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA420P16LE, ///< planar YUV 4:2:0 40bpp, (1 Cr & Cb sample per 2x2 Y & A samples, little-endian)
        AV_PIX_FMT_YUVA422P16BE, ///< planar YUV 4:2:2 48bpp, (1 Cr & Cb sample per 2x1 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA422P16LE, ///< planar YUV 4:2:2 48bpp, (1 Cr & Cb sample per 2x1 Y & A samples, little-endian)
        AV_PIX_FMT_YUVA444P16BE, ///< planar YUV 4:4:4 64bpp, (1 Cr & Cb sample per 1x1 Y & A samples, big-endian)
        AV_PIX_FMT_YUVA444P16LE, ///< planar YUV 4:4:4 64bpp, (1 Cr & Cb sample per 1x1 Y & A samples, little-endian)

        AV_PIX_FMT_VDPAU,     ///< HW acceleration through VDPAU, Picture.data[3] contains a VdpVideoSurface

        AV_PIX_FMT_XYZ12LE,      ///< packed XYZ 4:4:4, 36 bpp, (msb) 12X, 12Y, 12Z (lsb), the 2-byte value for each X/Y/Z is stored as little-endian, the 4 lower bits are set to 0
        AV_PIX_FMT_XYZ12BE,      ///< packed XYZ 4:4:4, 36 bpp, (msb) 12X, 12Y, 12Z (lsb), the 2-byte value for each X/Y/Z is stored as big-endian, the 4 lower bits are set to 0

        AV_PIX_FMT_RGBA64BE1 = 0x123,  ///< packed RGBA 16:16:16:16, 64bpp, 16R, 16G, 16B, 16A, the 2-byte value for each R/G/B/A component is stored as big-endian
        AV_PIX_FMT_RGBA64LE1,  ///< packed RGBA 16:16:16:16, 64bpp, 16R, 16G, 16B, 16A, the 2-byte value for each R/G/B/A component is stored as little-endian
        AV_PIX_FMT_BGRA64BE1,  ///< packed RGBA 16:16:16:16, 64bpp, 16B, 16G, 16R, 16A, the 2-byte value for each R/G/B/A component is stored as big-endian
        AV_PIX_FMT_BGRA64LE1,  ///< packed RGBA 16:16:16:16, 64bpp, 16B, 16G, 16R, 16A, the 2-byte value for each R/G/B/A component is stored as little-endian
        AV_PIX_FMT_0RGB = 0x123 + 4,      ///< packed RGB 8:8:8, 32bpp, 0RGB0RGB...
        AV_PIX_FMT_RGB0,      ///< packed RGB 8:8:8, 32bpp, RGB0RGB0...
        AV_PIX_FMT_0BGR,      ///< packed BGR 8:8:8, 32bpp, 0BGR0BGR...
        AV_PIX_FMT_BGR0,      ///< packed BGR 8:8:8, 32bpp, BGR0BGR0...
        AV_PIX_FMT_YUVA444P,  ///< planar YUV 4:4:4 32bpp, (1 Cr & Cb sample per 1x1 Y & A samples)
        AV_PIX_FMT_YUVA422P,  ///< planar YUV 4:2:2 24bpp, (1 Cr & Cb sample per 2x1 Y & A samples)

        AV_PIX_FMT_YUV420P12BE, ///< planar YUV 4:2:0,18bpp, (1 Cr & Cb sample per 2x2 Y samples), big-endian
        AV_PIX_FMT_YUV420P12LE, ///< planar YUV 4:2:0,18bpp, (1 Cr & Cb sample per 2x2 Y samples), little-endian
        AV_PIX_FMT_YUV420P14BE, ///< planar YUV 4:2:0,21bpp, (1 Cr & Cb sample per 2x2 Y samples), big-endian
        AV_PIX_FMT_YUV420P14LE, ///< planar YUV 4:2:0,21bpp, (1 Cr & Cb sample per 2x2 Y samples), little-endian
        AV_PIX_FMT_YUV422P12BE, ///< planar YUV 4:2:2,24bpp, (1 Cr & Cb sample per 2x1 Y samples), big-endian
        AV_PIX_FMT_YUV422P12LE, ///< planar YUV 4:2:2,24bpp, (1 Cr & Cb sample per 2x1 Y samples), little-endian
        AV_PIX_FMT_YUV422P14BE, ///< planar YUV 4:2:2,28bpp, (1 Cr & Cb sample per 2x1 Y samples), big-endian
        AV_PIX_FMT_YUV422P14LE, ///< planar YUV 4:2:2,28bpp, (1 Cr & Cb sample per 2x1 Y samples), little-endian
        AV_PIX_FMT_YUV444P12BE, ///< planar YUV 4:4:4,36bpp, (1 Cr & Cb sample per 1x1 Y samples), big-endian
        AV_PIX_FMT_YUV444P12LE, ///< planar YUV 4:4:4,36bpp, (1 Cr & Cb sample per 1x1 Y samples), little-endian
        AV_PIX_FMT_YUV444P14BE, ///< planar YUV 4:4:4,42bpp, (1 Cr & Cb sample per 1x1 Y samples), big-endian
        AV_PIX_FMT_YUV444P14LE, ///< planar YUV 4:4:4,42bpp, (1 Cr & Cb sample per 1x1 Y samples), little-endian
        AV_PIX_FMT_GBRP12BE,    ///< planar GBR 4:4:4 36bpp, big-endian
        AV_PIX_FMT_GBRP12LE,    ///< planar GBR 4:4:4 36bpp, little-endian
        AV_PIX_FMT_GBRP14BE,    ///< planar GBR 4:4:4 42bpp, big-endian
        AV_PIX_FMT_GBRP14LE,    ///< planar GBR 4:4:4 42bpp, little-endian
        AV_PIX_FMT_GBRAP,       ///< planar GBRA 4:4:4:4 32bpp
        AV_PIX_FMT_GBRAP16BE,   ///< planar GBRA 4:4:4:4 64bpp, big-endian
        AV_PIX_FMT_GBRAP16LE,   ///< planar GBRA 4:4:4:4 64bpp, little-endian
        AV_PIX_FMT_YUVJ411P,    ///< planar YUV 4:1:1, 12bpp, (1 Cr & Cb sample per 4x1 Y samples) full scale (JPEG), deprecated in favor of PIX_FMT_YUV411P and setting color_range
        AV_PIX_FMT_NB,        ///< number of pixel formats, DO NOT USE THIS if you want to link with shared libav* because the number of formats might differ between versions
	}
	public enum AVSampleFormat
	{
        AV_SAMPLE_FMT_NONE = -1,
        AV_SAMPLE_FMT_U8,          ///< unsigned 8 bits
        AV_SAMPLE_FMT_S16,         ///< signed 16 bits
        AV_SAMPLE_FMT_S32,         ///< signed 32 bits
        AV_SAMPLE_FMT_FLT,         ///< float
        AV_SAMPLE_FMT_DBL,         ///< double

        AV_SAMPLE_FMT_U8P,         ///< unsigned 8 bits, planar
        AV_SAMPLE_FMT_S16P,        ///< signed 16 bits, planar
        AV_SAMPLE_FMT_S32P,        ///< signed 32 bits, planar
        AV_SAMPLE_FMT_FLTP,        ///< float, planar
        AV_SAMPLE_FMT_DBLP,        ///< double, planar

        AV_SAMPLE_FMT_NB           ///< Number of sample formats. DO NOT USE if linking dynamically
    }
	enum CodecFlags : ulong
	{
		CODEC_FLAG_QSCALE = 0x0002, ///< Use fixed qscale.
		CODEC_FLAG_4MV = 0x0004, ///< 4 MV per MB allowed / advanced prediction for H.263.
		CODEC_FLAG_QPEL = 0x0010, ///< Use qpel MC.
		CODEC_FLAG_GMC = 0x0020, ///< Use GMC.
		CODEC_FLAG_MV0 = 0x0040, ///< Always try a MB with MV=<0,0>.
		CODEC_FLAG_PART = 0x0080, ///< Use data partitioning.
		/**
		 * The parent program guarantees that the input for B-frames containing
		 * streams is not written to for at least s->max_b_frames+1 frames, if
		 * this is not set the input will be copied.
		 */
		CODEC_FLAG_INPUT_PRESERVED = 0x0100,
		CODEC_FLAG_PASS1 = 0x0200, ///< Use internal 2pass ratecontrol in first pass mode.
		CODEC_FLAG_PASS2 = 0x0400, ///< Use internal 2pass ratecontrol in second pass mode.
		CODEC_FLAG_EXTERN_HUFF = 0x1000, ///< Use external Huffman table (for MJPEG).
		CODEC_FLAG_GRAY = 0x2000, ///< Only decode/encode grayscale.
		CODEC_FLAG_EMU_EDGE = 0x4000, ///< Don't draw edges.
		CODEC_FLAG_PSNR = 0x8000, ///< error[?] variables will be set during encoding.
		CODEC_FLAG_TRUNCATED = 0x00010000, /** Input bitstream might be truncated at a random
														  location instead of only at frame boundaries. */
		CODEC_FLAG_NORMALIZE_AQP = 0x00020000, ///< Normalize adaptive quantization.
		CODEC_FLAG_INTERLACED_DCT = 0x00040000, ///< Use interlaced DCT.
		CODEC_FLAG_LOW_DELAY = 0x00080000, ///< Force low delay.
		CODEC_FLAG_ALT_SCAN = 0x00100000, ///< Use alternate scan.
		CODEC_FLAG_GLOBAL_HEADER = 0x00400000, ///< Place global headers in extradata instead of every keyframe.
		CODEC_FLAG_BITEXACT = 0x00800000, ///< Use only bitexact stuff (except (I)DCT).
		/* Fx : Flag for h263+ extra options */
		CODEC_FLAG_AC_PRED = 0x01000000, ///< H.263 advanced intra coding / MPEG-4 AC prediction
		CODEC_FLAG_H263P_UMV = 0x02000000, ///< unlimited motion vector
		CODEC_FLAG_CBP_RD = 0x04000000, ///< Use rate distortion optimization for cbp.
		CODEC_FLAG_QP_RD = 0x08000000, ///< Use rate distortion optimization for qp selectioon.
		CODEC_FLAG_H263P_AIV = 0x00000008, ///< H.263 alternative inter VLC
		CODEC_FLAG_OBMC = 0x00000001, ///< OBMC
		CODEC_FLAG_LOOP_FILTER = 0x00000800, ///< loop filter
		CODEC_FLAG_H263P_SLICE_STRUCT = 0x10000000,
		CODEC_FLAG_INTERLACED_ME = 0x20000000, ///< interlaced motion estimation
		CODEC_FLAG_SVCD_SCAN_OFFSET = 0x40000000, ///< Will reserve space for SVCD scan offset user data.
		CODEC_FLAG_CLOSED_GOP = 0x80000000,
		CODEC_FLAG2_FAST = 0x00000001, ///< Allow non spec compliant speedup tricks.
		CODEC_FLAG2_STRICT_GOP = 0x00000002, ///< Strictly enforce GOP size.
		CODEC_FLAG2_NO_OUTPUT = 0x00000004, ///< Skip bitstream encoding.
		CODEC_FLAG2_LOCAL_HEADER = 0x00000008, ///< Place global headers at every keyframe instead of in extradata.
		CODEC_FLAG2_BPYRAMID = 0x00000010, ///< H.264 allow B-frames to be used as references.
		CODEC_FLAG2_WPRED = 0x00000020, ///< H.264 weighted biprediction for B-frames
		CODEC_FLAG2_MIXED_REFS = 0x00000040, ///< H.264 one reference per partition, as opposed to one reference per macroblock
		CODEC_FLAG2_8X8DCT = 0x00000080, ///< H.264 high profile 8x8 transform
		CODEC_FLAG2_FASTPSKIP = 0x00000100, ///< H.264 fast pskip
		CODEC_FLAG2_AUD = 0x00000200, ///< H.264 access unit delimiters
		CODEC_FLAG2_BRDO = 0x00000400, ///< B-frame rate-distortion optimization
		CODEC_FLAG2_INTRA_VLC = 0x00000800, ///< Use MPEG-2 intra VLC table.
		CODEC_FLAG2_MEMC_ONLY = 0x00001000, ///< Only do ME/MC (I frames -> ref, P frame -> ME+MC).
		CODEC_FLAG2_DROP_FRAME_TIMECODE = 0x00002000, ///< timecode is in drop frame format.
		CODEC_FLAG2_SKIP_RD = 0x00004000, ///< RD optimal MB level residual skipping
		CODEC_FLAG2_CHUNKS = 0x00008000, ///< Input bitstream might be truncated at a packet boundaries instead of only at frame boundaries.
		CODEC_FLAG2_NON_LINEAR_QUANT = 0x00010000, ///< Use MPEG-2 nonlinear quantizer.
		CODEC_FLAG2_BIT_RESERVOIR = 0x00020000, ///< Use a bit reservoir when encoding if possible
		CODEC_FLAG2_MBTREE = 0x00040000, ///< Use macroblock tree ratecontrol (x264 only)
		CODEC_FLAG2_PSY = 0x00080000, ///< Use psycho visual optimizations.
		CODEC_FLAG2_SSIM = 0x00100000, ///< Compute SSIM during encoding, error[] values are undefined.
		CODEC_FLAG2_INTRA_REFRESH = 0x00200000 ///< Use periodic insertion of intra blocks instead of keyframes.
	}
	enum CodecCapabilities
	{
		CODEC_CAP_DRAW_HORIZ_BAND = 0x0001, ///< Decoder can use draw_horiz_band callback.
		/**
		 * Codec uses get_buffer() for allocating buffers and supports custom allocators.
		 * If not set, it might not use get_buffer() at all or use operations that
		 * assume the buffer was allocated by avcodec_default_get_buffer.
		 */
		CODEC_CAP_DR1 = 0x0002,
		/* If 'parse_only' field is true, then avcodec_parse_frame() can be used. */
		CODEC_CAP_PARSE_ONLY = 0x0004,
		CODEC_CAP_TRUNCATED = 0x0008,
		/* Codec can export data for HW decoding (XvMC). */
		CODEC_CAP_HWACCEL = 0x0010,
		/**
		 * Codec has a nonzero delay and needs to be fed with NULL at the end to get the delayed data.
		 * If this is not set, the codec is guaranteed to never be fed with NULL data.
		 */
		CODEC_CAP_DELAY = 0x0020,
		/**
		 * Codec can be fed a final frame with a smaller size.
		 * This can be used to prevent truncation of the last audio samples.
		 */
		CODEC_CAP_SMALL_LAST_FRAME = 0x0040,
		/**
		 * Codec can export data for HW decoding (VDPAU).
		 */
		CODEC_CAP_HWACCEL_VDPAU = 0x0080,
		/**
		 * Codec can output multiple frames per AVPacket
		 * Normally demuxers return one frame at a time, demuxers which do not do
		 * are connected to a parser to split what they return into proper frames.
		 * This flag is reserved to the very rare category of codecs which have a
		 * bitstream that cannot be split into frames without timeconsuming
		 * operations like full decoding. Demuxers carring such bitstreams thus
		 * may return multiple frames in a packet. This has many disadvantages like
		 * prohibiting stream copy in many cases thus it should only be considered
		 * as a last resort.
		 */
		CODEC_CAP_SUBFRAMES = 0x0100,
		/**
		 * Codec is experimental and is thus avoided in favor of non experimental
		 * encoders
		 */
		CODEC_CAP_EXPERIMENTAL = 0x0200
	}
	enum AVStreamParseType
	{
        AVSTREAM_PARSE_NONE,
        AVSTREAM_PARSE_FULL,       /**< full parsing and repack */
        AVSTREAM_PARSE_HEADERS,    /**< Only parse headers, do not repack. */
        AVSTREAM_PARSE_TIMESTAMPS, /**< full parsing and interpolation of timestamps for frames not starting on a packet boundary */
        AVSTREAM_PARSE_FULL_ONCE,  /**< full parsing and repack of the first frame only, only implemented for H.264 currently */
        AVSTREAM_PARSE_FULL_RAW = 0x292C2C00,       /**MKTAG(0,'R','A','W')< full parsing and repack with timestamp and position generation by parser for raw
                                                         this assumes that each packet in the file contains no demuxer level headers and
                                                         just codec level data, otherwise position generation would fail */
    }
	enum AVMediaType
	{
		AVMEDIA_TYPE_UNKNOWN = -1,
		AVMEDIA_TYPE_VIDEO,
		AVMEDIA_TYPE_AUDIO,
		AVMEDIA_TYPE_DATA,
		AVMEDIA_TYPE_SUBTITLE,
		AVMEDIA_TYPE_ATTACHMENT,
		AVMEDIA_TYPE_NB
	}
	enum AVDiscard
	{
        /* We leave some space between them for extensions (drop some
        * keyframes for intra-only or drop just some bidir frames). */
        AVDISCARD_NONE = -16, ///< discard nothing
        AVDISCARD_DEFAULT = 0, ///< discard useless packets like 0 size packets in avi
        AVDISCARD_NONREF = 8, ///< discard all non reference
        AVDISCARD_BIDIR = 16, ///< discard all bidirectional frames
        AVDISCARD_NONKEY = 32, ///< discard all frames except keyframes
        AVDISCARD_ALL = 48, ///< discard all
    }
	enum AVColorPrimaries
	{
        AVCOL_PRI_BT709 = 1, ///< also ITU-R BT1361 / IEC 61966-2-4 / SMPTE RP177 Annex B
        AVCOL_PRI_UNSPECIFIED = 2,
        AVCOL_PRI_BT470M = 4,
        AVCOL_PRI_BT470BG = 5, ///< also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL & SECAM
        AVCOL_PRI_SMPTE170M = 6, ///< also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC
        AVCOL_PRI_SMPTE240M = 7, ///< functionally identical to above
        AVCOL_PRI_FILM = 8,
        AVCOL_PRI_NB, ///< Not part of ABI
    }
	enum AVColorTransferCharacteristic
	{
        AVCOL_TRC_BT709 = 1, ///< also ITU-R BT1361
        AVCOL_TRC_UNSPECIFIED = 2,
        AVCOL_TRC_GAMMA22 = 4, ///< also ITU-R BT470M / ITU-R BT1700 625 PAL & SECAM
        AVCOL_TRC_GAMMA28 = 5, ///< also ITU-R BT470BG
        AVCOL_TRC_SMPTE240M = 7,
        AVCOL_TRC_NB, ///< Not part of ABI
    }
	enum AVColorSpace
	{
        AVCOL_SPC_RGB = 0,
        AVCOL_SPC_BT709 = 1, ///< also ITU-R BT1361 / IEC 61966-2-4 xvYCC709 / SMPTE RP177 Annex B
        AVCOL_SPC_UNSPECIFIED = 2,
        AVCOL_SPC_FCC = 4,
        AVCOL_SPC_BT470BG = 5, ///< also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL & SECAM / IEC 61966-2-4 xvYCC601
        AVCOL_SPC_SMPTE170M = 6, ///< also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC / functionally identical to above
        AVCOL_SPC_SMPTE240M = 7,
        AVCOL_SPC_YCOCG = 8, ///< Used by Dirac / VC-2 and H.264 FRext, see ITU-T SG16
        AVCOL_SPC_NB, ///< Not part of ABI
    }
	enum AVColorRange
	{
		AVCOL_RANGE_UNSPECIFIED = 0,
		AVCOL_RANGE_MPEG = 1, ///< the normal 219*2^(n-8) "MPEG" YUV ranges
		AVCOL_RANGE_JPEG = 2, ///< the normal     2^n-1   "JPEG" YUV ranges
		AVCOL_RANGE_NB ///< Not part of ABI
	}
	enum AVChromaLocation
	{
		AVCHROMA_LOC_UNSPECIFIED = 0,
		AVCHROMA_LOC_LEFT = 1, ///< mpeg2/4, h264 default
		AVCHROMA_LOC_CENTER = 2, ///< mpeg1, jpeg, h263
		AVCHROMA_LOC_TOPLEFT = 3, ///< DV
		AVCHROMA_LOC_TOP = 4,
		AVCHROMA_LOC_BOTTOMLEFT = 5,
		AVCHROMA_LOC_BOTTOM = 6,
		AVCHROMA_LOC_NB ///< Not part of ABI
	}
	enum AVLPCType
	{
		AV_LPC_TYPE_DEFAULT = -1, ///< use the codec default LPC type
		AV_LPC_TYPE_NONE = 0, ///< do not use LPC prediction or use all zero coefficients
		AV_LPC_TYPE_FIXED = 1, ///< fixed LPC coefficients
		AV_LPC_TYPE_LEVINSON = 2, ///< Levinson-Durbin recursion
		AV_LPC_TYPE_CHOLESKY = 3, ///< Cholesky factorization
		AV_LPC_TYPE_NB ///< Not part of ABI
	}
	enum Motion_Est_ID
	{
        ME_ZERO = 1,    ///< no search, that is use 0,0 vector whenever one is needed
        ME_FULL,
        ME_LOG,
        ME_PHODS,
        ME_EPZS,        ///< enhanced predictive zonal search
        ME_X1,          ///< reserved for experiments
        ME_HEX,         ///< hexagon based search
        ME_UMH,         ///< uneven multi-hexagon search
        ME_TESA,        ///< transformed exhaustive search algorithm
        ME_ITER = 50     ///< iterative search
    };
	enum X264Partition : int
	{
		X264_PART_I4X4 = 0x001,  /* Analyze i4x4 */
		X264_PART_I8X8 = 0x002,  /* Analyze i8x8 (requires 8x8 transform) */
		X264_PART_P8X8 = 0x010,  /* Analyze p16x8, p8x16 and p8x8 */
		X264_PART_P4X4 = 0x020,  /* Analyze p8x4, p4x8, p4x4 */
		X264_PART_B8X8 = 0x100  /* Analyze b16x8, b8x16 and b8x8 */
	}
	enum MotionCompare : int
	{
		FF_CMP_SAD = 0,
		FF_CMP_SSE = 1,
		FF_CMP_SATD = 2,
		FF_CMP_DCT = 3,
		FF_CMP_PSNR = 4,
		FF_CMP_BIT = 5,
		FF_CMP_RD = 6,
		FF_CMP_ZERO = 7,
		FF_CMP_VSAD = 8,
		FF_CMP_VSSE = 9,
		FF_CMP_NSSE = 10,
		FF_CMP_W53 = 11,
		FF_CMP_W97 = 12,
		FF_CMP_DCTMAX = 13,
		FF_CMP_DCT264 = 14,
		FF_CMP_CHROMA = 256
	}
	enum CoderType : int
	{
		FF_CODER_TYPE_VLC = 0,
		FF_CODER_TYPE_AC = 1,
		FF_CODER_TYPE_RAW = 2,
		FF_CODER_TYPE_RLE = 3,
		FF_CODER_TYPE_DEFLATE = 4
	}
	enum AVAudioServiceType : int
	{
		AV_AUDIO_SERVICE_TYPE_MAIN = 0,
		AV_AUDIO_SERVICE_TYPE_EFFECTS = 1,
		AV_AUDIO_SERVICE_TYPE_VISUALLY_IMPAIRED = 2,
		AV_AUDIO_SERVICE_TYPE_HEARING_IMPAIRED = 3,
		AV_AUDIO_SERVICE_TYPE_DIALOGUE = 4,
		AV_AUDIO_SERVICE_TYPE_COMMENTARY = 5,
		AV_AUDIO_SERVICE_TYPE_EMERGENCY = 6,
		AV_AUDIO_SERVICE_TYPE_VOICE_OVER = 7,
		AV_AUDIO_SERVICE_TYPE_KARAOKE = 8,
		AV_AUDIO_SERVICE_TYPE_NB ///< Not part of ABI
	}
	enum AVFieldOrder
	{
		AV_FIELD_UNKNOWN,
		AV_FIELD_PROGRESSIVE,
		AV_FIELD_TT,          //< Top coded_first, top displayed first
		AV_FIELD_BB,          //< Bottom coded first, bottom displayed first
		AV_FIELD_TB,          //< Top coded first, bottom displayed first
		AV_FIELD_BT          //< Bottom coded first, top displayed first
	}
	enum AVPictureType
	{
		AV_PICTURE_TYPE_NONE = 0, ///< Undefined
		AV_PICTURE_TYPE_I,     ///< Intra
		AV_PICTURE_TYPE_P,     ///< Predicted
		AV_PICTURE_TYPE_B,     ///< Bi-dir predicted
		AV_PICTURE_TYPE_S,     ///< S(GMC)-VOP MPEG4
		AV_PICTURE_TYPE_SI,    ///< Switching Intra
		AV_PICTURE_TYPE_SP,    ///< Switching Predicted
		AV_PICTURE_TYPE_BI    ///< BI type
	}
    enum AVDurationEstimationMethod
    {
        AVFMT_DURATION_FROM_PTS,    ///< Duration accurately estimated from PTSes
        AVFMT_DURATION_FROM_STREAM, ///< Duration estimated from a stream with a known duration
        AVFMT_DURATION_FROM_BITRATE ///< Duration estimated from bitrate (less accurate)
    }
    enum AVMatrixEncoding
    {
        AV_MATRIX_ENCODING_NONE,
        AV_MATRIX_ENCODING_DOLBY,
        AV_MATRIX_ENCODING_DPLII,
        AV_MATRIX_ENCODING_NB
    }
    enum SwrEngine
    {
        SWR_ENGINE_SWR,             /**< SW Resampler */
        SWR_ENGINE_SOXR,            /**< SoX Resampler */
        SWR_ENGINE_NB,              ///< not part of API/ABI
    }
    enum AVResampleDitherMethod
    {
        AV_RESAMPLE_DITHER_NONE,            /**< Do not use dithering */
        AV_RESAMPLE_DITHER_RECTANGULAR,     /**< Rectangular Dither */
        AV_RESAMPLE_DITHER_TRIANGULAR,      /**< Triangular Dither*/
        AV_RESAMPLE_DITHER_TRIANGULAR_HP,   /**< Triangular Dither with High Pass */
        AV_RESAMPLE_DITHER_TRIANGULAR_NS,   /**< Triangular Dither with Noise Shaping */
        AV_RESAMPLE_DITHER_NB,              /**< Number of dither types. Not part of ABI. */
    }
    enum SwrFilterType
    {
        SWR_FILTER_TYPE_CUBIC,              /**< Cubic */
        SWR_FILTER_TYPE_BLACKMAN_NUTTALL,   /**< Blackman Nuttall Windowed Sinc */
        SWR_FILTER_TYPE_KAISER,             /**< Kaiser Windowed Sinc */
    }
    enum ChannelsLayout : ulong
    {
        AV_CH_FRONT_LEFT = 0x00000001,
        AV_CH_FRONT_RIGHT = 0x00000002,
        AV_CH_FRONT_CENTER = 0x00000004,
        AV_CH_LOW_FREQUENCY = 0x00000008,
        AV_CH_BACK_LEFT = 0x00000010,
        AV_CH_BACK_RIGHT = 0x00000020,
        AV_CH_FRONT_LEFT_OF_CENTER = 0x00000040,
        AV_CH_FRONT_RIGHT_OF_CENTER = 0x00000080,
        AV_CH_BACK_CENTER = 0x00000100,
        AV_CH_SIDE_LEFT = 0x00000200,
        AV_CH_SIDE_RIGHT = 0x00000400,
        AV_CH_TOP_CENTER = 0x00000800,
        AV_CH_TOP_FRONT_LEFT = 0x00001000,
        AV_CH_TOP_FRONT_CENTER = 0x00002000,
        AV_CH_TOP_FRONT_RIGHT = 0x00004000,
        AV_CH_TOP_BACK_LEFT = 0x00008000,
        AV_CH_TOP_BACK_CENTER = 0x00010000,
        AV_CH_TOP_BACK_RIGHT = 0x00020000,
        AV_CH_STEREO_LEFT = 0x20000000,  ///< Stereo downmix.
        AV_CH_STEREO_RIGHT = 0x40000000,  ///< See AV_CH_STEREO_LEFT.
        AV_CH_WIDE_LEFT = 0x0000000080000000UL,
        AV_CH_WIDE_RIGHT = 0x0000000100000000UL,
        AV_CH_SURROUND_DIRECT_LEFT = 0x0000000200000000UL,
        AV_CH_SURROUND_DIRECT_RIGHT = 0x0000000400000000UL,
        AV_CH_LOW_FREQUENCY_2 = 0x0000000800000000UL,
        AV_CH_LAYOUT_NATIVE = 0x8000000000000000UL,

        AV_CH_LAYOUT_MONO = (AV_CH_FRONT_CENTER),
        AV_CH_LAYOUT_STEREO = (AV_CH_FRONT_LEFT | AV_CH_FRONT_RIGHT),
        AV_CH_LAYOUT_2POINT1 = (AV_CH_LAYOUT_STEREO | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_2_1 = (AV_CH_LAYOUT_STEREO | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_SURROUND = (AV_CH_LAYOUT_STEREO | AV_CH_FRONT_CENTER),
        AV_CH_LAYOUT_3POINT1 = (AV_CH_LAYOUT_SURROUND | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_4POINT0 = (AV_CH_LAYOUT_SURROUND | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_4POINT1 = (AV_CH_LAYOUT_4POINT0 | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_2_2 = (AV_CH_LAYOUT_STEREO | AV_CH_SIDE_LEFT | AV_CH_SIDE_RIGHT),
        AV_CH_LAYOUT_QUAD = (AV_CH_LAYOUT_STEREO | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT),
        AV_CH_LAYOUT_5POINT0 = (AV_CH_LAYOUT_SURROUND | AV_CH_SIDE_LEFT | AV_CH_SIDE_RIGHT),
        AV_CH_LAYOUT_5POINT1 = (AV_CH_LAYOUT_5POINT0 | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_5POINT0_BACK = (AV_CH_LAYOUT_SURROUND | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT),
        AV_CH_LAYOUT_5POINT1_BACK = (AV_CH_LAYOUT_5POINT0_BACK | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_6POINT0 = (AV_CH_LAYOUT_5POINT0 | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_6POINT0_FRONT = (AV_CH_LAYOUT_2_2 | AV_CH_FRONT_LEFT_OF_CENTER | AV_CH_FRONT_RIGHT_OF_CENTER),
        AV_CH_LAYOUT_HEXAGONAL = (AV_CH_LAYOUT_5POINT0_BACK | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_6POINT1 = (AV_CH_LAYOUT_5POINT1 | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_6POINT1_BACK = (AV_CH_LAYOUT_5POINT1_BACK | AV_CH_BACK_CENTER),
        AV_CH_LAYOUT_6POINT1_FRONT = (AV_CH_LAYOUT_6POINT0_FRONT | AV_CH_LOW_FREQUENCY),
        AV_CH_LAYOUT_7POINT0 = (AV_CH_LAYOUT_5POINT0 | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT),
        AV_CH_LAYOUT_7POINT0_FRONT = (AV_CH_LAYOUT_5POINT0 | AV_CH_FRONT_LEFT_OF_CENTER | AV_CH_FRONT_RIGHT_OF_CENTER),
        AV_CH_LAYOUT_7POINT1 = (AV_CH_LAYOUT_5POINT1 | AV_CH_BACK_LEFT | AV_CH_BACK_RIGHT),
        AV_CH_LAYOUT_7POINT1_WIDE = (AV_CH_LAYOUT_5POINT1 | AV_CH_FRONT_LEFT_OF_CENTER | AV_CH_FRONT_RIGHT_OF_CENTER),
        AV_CH_LAYOUT_7POINT1_WIDE_BACK = (AV_CH_LAYOUT_5POINT1_BACK | AV_CH_FRONT_LEFT_OF_CENTER | AV_CH_FRONT_RIGHT_OF_CENTER),
        AV_CH_LAYOUT_OCTAGONAL = (AV_CH_LAYOUT_5POINT0 | AV_CH_BACK_LEFT | AV_CH_BACK_CENTER | AV_CH_BACK_RIGHT),
        AV_CH_LAYOUT_STEREO_DOWNMIX = (AV_CH_STEREO_LEFT | AV_CH_STEREO_RIGHT)
    }
    enum AVSeek
    {
        SIZE = 0x10000,  ///< doesn't seek. just return total size
        FLAG_BEGINNING = 0, ///< seek backward
        FLAG_BACKWARD = 1, ///< seek backward
        FLAG_BYTE = 2, ///< seeking based on position in bytes
		FLAG_ANY = 4, ///< seek to any frame, even non-keyframes
        FLAG_FRAME = 8, ///< seeking based on frame number 
        FLAG_FORCE = 0x20000
    }
    #endregion

	#region structures
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVRational
	{
		public int num; ///< numerator
		public int den; ///< denominator
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVFrac
	{
		public long val;
		public long num;
		public long den;
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVProbeData
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public /*const char* */string filename;
		public /*unsigned char* */IntPtr buf; /**< Buffer must have AVPROBE_PADDING_SIZE of extra allocated bytes filled with zero. */
		public int buf_size;       /**< Size of buf except extra allocated bytes */
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_sei_t
	{
		int num_payloads;
		IntPtr payloads;
		/* In: optional callback to free each payload AND x264_sei_payload_t when used. */
		IntPtr sei_free;
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	/* Cropping Rectangle parameters: added to those implicitly defined by
	   non-mod16 video resolutions. */
	public struct STCROP_RECT
	{
		uint i_left;
		uint i_top;
		uint i_right;
		uint i_bottom;
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_hrd_t
	{
		double cpb_initial_arrival_time;
		double cpb_final_arrival_time;
		double cpb_removal_time;

		double dpb_output_time;
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_image_t
	{
		int i_csp;       /* Colorspace */
		int i_plane;     /* Number of image planes */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		int[] i_stride; /* Strides for each plane */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		IntPtr[] plane;   /* Pointers to each plane */
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_image_properties_t
	{
		/* In: an array of quantizer offsets to be applied to this image during encoding.
		 *     These are added on top of the decisions made by x264.
		 *     Offsets can be fractional; they are added before QPs are rounded to integer.
		 *     Adaptive quantization must be enabled to use this feature.  Behavior if quant
		 *     offsets differ between encoding passes is undefined.
		 *
		 *     Array contains one offset per macroblock, in raster scan order.  In interlaced
		 *     mode, top-field MBs and bottom-field MBs are interleaved at the row level. */
		IntPtr quant_offsets;
		/* In: optional callback to free quant_offsets when used.
		 *     Useful if one wants to use a different quant_offset array for each frame. */
		IntPtr quant_offsets_free;
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_picture_t
	{
		/* In: force picture type (if not auto)
		 *     If x264 encoding parameters are violated in the forcing of picture types,
		 *     x264 will correct the input picture type and log a warning.
		 *     The quality of frametype decisions may suffer if a great deal of fine-grained
		 *     mixing of auto and forced frametypes is done.
		 * Out: type of the picture encoded */
		int i_type;
		/* In: force quantizer for != X264_QP_AUTO */
		int i_qpplus1;
		/* In: pic_struct, for pulldown/doubling/etc...used only if b_pic_struct=1.
		 *     use pic_struct_e for pic_struct inputs
		 * Out: pic_struct element associated with frame */
		int i_pic_struct;
		/* Out: whether this frame is a keyframe.  Important when using modes that result in
		 * SEI recovery points being used instead of IDR frames. */
		int b_keyframe;
		/* In: user pts, Out: pts of encoded picture (user)*/
		long i_pts;
		/* Out: frame dts. When the pts of the first frame is close to zero,
		 *      initial frames may have a negative dts which must be dealt with by any muxer */
		long i_dts;
		/* In: custom encoding parameters to be set from this frame forwards
			   (in coded order, not display order). If NULL, continue using
			   parameters from the previous frame.  Some parameters, such as
			   aspect ratio, can only be changed per-GOP due to the limitations
			   of H.264 itself; in this case, the caller must force an IDR frame
			   if it needs the changed parameter to apply immediately. */
		IntPtr param;
		/* In: raw data */
		x264_image_t img;
		/* In: optional information to modify encoder decisions for this frame */
		x264_image_properties_t prop;
		/* Out: HRD timing information. Output only when i_nal_hrd is set. */
		x264_hrd_t hrd_timing;
		/* In: arbitrary user SEI (e.g subtitles, AFDs) */
		x264_sei_t extra_sei;
		/* private user data. copied from input to output frames. */
		IntPtr opaque;
	};
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	/* Rate control parameters */
	public struct STRC
	{
		int i_rc_method;    /* X264_RC_* */

		int i_qp_constant;  /* 0 to (51 + 6*(x264_bit_depth-8)). 0=lossless */
		int i_qp_min;       /* min allowed QP value */
		int i_qp_max;       /* max allowed QP value */
		int i_qp_step;      /* max QP step between frames */

		int i_bitrate;
		float f_rf_constant;  /* 1pass VBR, nominal QP */
		float f_rf_constant_max;  /* In CRF mode, maximum CRF as caused by VBV */
		float f_rate_tolerance;
		int i_vbv_max_bitrate;
		int i_vbv_buffer_size;
		float f_vbv_buffer_init; /* <=1: fraction of buffer_size. >1: kbit */
		float f_ip_factor;
		float f_pb_factor;

		int i_aq_mode;      /* psy adaptive QP. (X264_AQ_*) */
		float f_aq_strength;
		int b_mb_tree;      /* Macroblock-tree ratecontrol. */
		int i_lookahead;

		/* 2pass */
		int b_stat_write;   /* Enable stat writing in psz_stat_out */
		[MarshalAs(UnmanagedType.LPStr)]
		string psz_stat_out;
		int b_stat_read;    /* Read stat from psz_stat_in and use it */
		[MarshalAs(UnmanagedType.LPStr)]
		string psz_stat_in;

		/* 2pass params (same as ffmpeg ones) */
		float f_qcompress;    /* 0.0 => cbr, 1.0 => constant qp */
		float f_qblur;        /* temporally blur quants */
		float f_complexity_blur; /* temporally blur complexity */
		IntPtr zones;         /* ratecontrol overrides */ //x264_zone_t *
		int i_zones;        /* number of zone_t's */
		[MarshalAs(UnmanagedType.LPStr)]
		string psz_zones;     /* alternate method of specifying zones */
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	/* Encoder analyser parameters */
	public struct STANALYSE
	{
		uint intra;     /* intra partitions */
		uint inter;     /* inter partitions */

		int b_transform_8x8;
		int i_weighted_pred; /* weighting for P-frames */
		int b_weighted_bipred; /* implicit weighting for B-frames */
		int i_direct_mv_pred; /* spatial vs temporal mv prediction */
		int i_chroma_qp_offset;

		int i_me_method; /* motion estimation algorithm to use (X264_ME_*) */
		int i_me_range; /* integer pixel motion estimation search range (from predicted mv) */
		int i_mv_range; /* maximum length of a mv (in pixels). -1 = auto, based on level */
		int i_mv_range_thread; /* minimum space between threads. -1 = auto, based on number of threads. */
		int i_subpel_refine; /* subpixel motion estimation quality */
		int b_chroma_me; /* chroma ME for subpel and mode decision in P-frames */
		int b_mixed_references; /* allow each mb partition to have its own reference number */
		int i_trellis;  /* trellis RD quantization */
		int b_fast_pskip; /* early SKIP detection on P-frames */
		int b_dct_decimate; /* transform coefficient thresholding on P-frames */
		int i_noise_reduction; /* adaptive pseudo-deadzone */
		float f_psy_rd; /* Psy RD strength */
		float f_psy_trellis; /* Psy trellis strength */
		int b_psy; /* Toggle all psy optimizations */

		/* the deadzone size that will be used in luma quantization */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		int[] i_luma_deadzone; /* {inter, intra} */

		int b_psnr;    /* compute and print PSNR stats */
		int b_ssim;    /* compute and print SSIM stats */
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct STVUI
	{
		/* they will be reduced to be 0 < x <= 65535 and prime */
		int i_sar_height;
		int i_sar_width;

		int i_overscan;    /* 0=undef, 1=no overscan, 2=overscan */

		/* see h264 annex E for the values of the following */
		int i_vidformat;
		int b_fullrange;
		int i_colorprim;
		int i_transfer;
		int i_colmatrix;
		int i_chroma_loc;    /* both top & bottom */
	}
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct x264_param_t
	{
		/* CPU flags */
		uint cpu;
		int i_threads;       /* encode multiple frames in parallel */
		int b_sliced_threads;  /* Whether to use slice-based threading. */
		int b_deterministic; /* whether to allow non-deterministic optimizations when threaded */
		int b_cpu_independent; /* force canonical behavior rather than cpu-dependent optimal algorithms */
		int i_sync_lookahead; /* threaded lookahead buffer */

		/* Video Properties */
		int i_width;
		int i_height;
		int i_csp;         /* CSP of encoded bitstream */
		int i_level_idc;
		int i_frame_total; /* number of frames to encode if known, else 0 */

		/* NAL HRD
		 * Uses Buffering and Picture Timing SEIs to signal HRD
		 * The HRD in H.264 was not designed with VFR in mind.
		 * It is therefore not recommendeded to use NAL HRD with VFR.
		 * Furthermore, reconfiguring the VBV (via x264_encoder_reconfig)
		 * will currently generate invalid HRD. */
		int i_nal_hrd;

		STVUI vui;
		/* Bitstream parameters */
		int i_frame_reference;  /* Maximum number of reference frames */
		int i_dpb_size;         /* Force a DPB size larger than that implied by B-frames and reference frames.
                                     * Useful in combination with interactive error resilience. */
		int i_keyint_max;       /* Force an IDR keyframe at this interval */
		int i_keyint_min;       /* Scenecuts closer together than this are coded as I, not IDR. */
		int i_scenecut_threshold; /* how aggressively to insert extra I frames */
		int b_intra_refresh;    /* Whether or not to use periodic intra refresh instead of IDR frames. */

		int i_bframe;   /* how many b-frame between 2 references pictures */
		int i_bframe_adaptive;
		int i_bframe_bias;
		int i_bframe_pyramid;   /* Keep some B-frames as references: 0=off, 1=strict hierarchical, 2=normal */
		int b_open_gop;
		int b_bluray_compat;

		int b_deblocking_filter;
		int i_deblocking_filter_alphac0;    /* [-6, 6] -6 light filter, 6 strong */
		int i_deblocking_filter_beta;       /* [-6, 6]  idem */

		int b_cabac;
		int i_cabac_init_idc;

		int b_interlaced;
		int b_constrained_intra;

		int i_cqm_preset;
		[MarshalAs(UnmanagedType.LPStr)]
		string psz_cqm_file;      /* JM format */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		byte cqm_4iy;        /* used only if i_cqm_preset == X264_CQM_CUSTOM */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		byte cqm_4py;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		byte cqm_4ic;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		byte cqm_4pc;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		byte cqm_8iy;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		byte cqm_8py;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		byte cqm_8ic;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		byte cqm_8pc;

		/* Log */
		IntPtr pf_log;//(*pf_log)( void *, int i_level, const char *psz, va_list );
		IntPtr p_log_private;
		int i_log_level;
		int b_visualize;
		[MarshalAs(UnmanagedType.LPStr)]
		string psz_dump_yuv;  /* filename for reconstructed frames */

		STANALYSE analyse;

		STRC rc;

		STCROP_RECT crop_rect;

		/* frame packing arrangement flag */
		int i_frame_packing;

		/* Muxing parameters */
		int b_aud;                  /* generate access unit delimiters */
		int b_repeat_headers;       /* put SPS/PPS before each keyframe */
		int b_annexb;               /* if set, place start codes (4 bytes) before NAL units,
                                 * otherwise place size (4 bytes) before NAL units. */
		int i_sps_id;               /* SPS and PPS id number */
		int b_vfr_input;            /* VFR input.  If 1, use timebase and timestamps for ratecontrol purposes.
                                 * If 0, use fps only. */
		int b_pulldown;             /* use explicity set timebase for CFR */
		uint i_fps_num;
		uint i_fps_den;
		uint i_timebase_num;    /* Timebase numerator */
		uint i_timebase_den;    /* Timebase denominator */

		int b_tff;

		/* Pulldown:
		 * The correct pic_struct must be passed with each input frame.
		 * The input timebase should be the timebase corresponding to the output framerate. This should be constant.
		 * e.g. for 3:2 pulldown timebase should be 1001/30000
		 * The PTS passed with each frame must be the PTS of the frame after pulldown is applied.
		 * Frame doubling and tripling require b_vfr_input set to zero (see H.264 Table D-1)
		 *
		 * Pulldown changes are not clearly defined in H.264. Therefore, it is the calling app's responsibility to manage this.
		 */

		int b_pic_struct;

		/* Fake Interlaced.
		 *
		 * Used only when b_interlaced=0. Setting this flag makes it possible to flag the stream as PAFF interlaced yet
		 * encode all frames progessively. It is useful for encoding 25p and 30p Blu-Ray streams.
		 */

		int b_fake_interlaced;

		/* Slicing parameters */
		int i_slice_max_size;    /* Max size per slice in bytes; includes estimated NAL overhead. */
		int i_slice_max_mbs;     /* Max number of MBs per slice; overrides i_slice_count. */
		int i_slice_count;       /* Number of slices per frame: forces rectangular slices. */

		/* Optional callback for freeing this x264_param_t when it is done being used.
		 * Only used when the x264_param_t sits in memory for an indefinite period of time,
		 * i.e. when an x264_param_t is passed to x264_t in an x264_picture_t or in zones.
		 * Not used when x264_encoder_reconfig is called directly. */
		IntPtr param_free;//void (*param_free)( void* );

		/* Optional low-level callback for low-latency encoding.  Called for each output NAL unit
		 * immediately after the NAL unit is finished encoding.  This allows the calling application
		 * to begin processing video data (e.g. by sending packets over a network) before the frame
		 * is done encoding.
		 *
		 * This callback MUST do the following in order to work correctly:
		 * 1) Have available an output buffer of at least size nal->i_payload*3/2 + 5 + 16.
		 * 2) Call x264_nal_encode( h, dst, nal ), where dst is the output buffer.
		 * After these steps, the content of nal is valid and can be used in the same way as if
		 * the NAL unit were output by x264_encoder_encode.
		 *
		 * This does not need to be synchronous with the encoding process: the data pointed to
		 * by nal (both before and after x264_nal_encode) will remain valid until the next
		 * x264_encoder_encode call.  The callback must be re-entrant.
		 *
		 * This callback does not work with frame-based threads; threads must be disabled
		 * or sliced-threads enabled.  This callback also does not work as one would expect
		 * with HRD -- since the buffering period SEI cannot be calculated until the frame
		 * is finished encoding, it will not be sent via this callback.
		 *
		 * Note also that the NALs are not necessarily returned in order when sliced threads is
		 * enabled.  Accordingly, the variable i_first_mb and i_last_mb are available in
		 * x264_nal_t to help the calling application reorder the slices if necessary.
		 *
		 * When this callback is enabled, x264_encoder_encode does not return valid NALs;
		 * the calling application is expected to acquire all output NALs through the callback.
		 *
		 * It is generally sensible to combine this callback with a use of slice-max-mbs or
		 * slice-max-size. */
		IntPtr nalu_process;//void (*nalu_process) ( x264_t *h, x264_nal_t *nal );
	}

	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct X264Context
	{
		public IntPtr class_;
		public x264_param_t params_;
		public IntPtr enc;
		public x264_picture_t pic;
		public IntPtr sei;
		public int sei_size;
		public AVFrame out_pic;
		[MarshalAs(UnmanagedType.LPStr)]
		public string preset;
		[MarshalAs(UnmanagedType.LPStr)]
		public string tune;
		[MarshalAs(UnmanagedType.LPStr)]
		public string profile;
		[MarshalAs(UnmanagedType.LPStr)]
		public string level;
		public int fastfirstpass;
		[MarshalAs(UnmanagedType.LPStr)]
		public string stats;
		[MarshalAs(UnmanagedType.LPStr)]
		public string wpredp;
		[MarshalAs(UnmanagedType.LPStr)]
		public string x264opts;
		public float crf;
		public float crf_max;
		public int cqp;
		public int aq_mode;
		public float aq_strength;
		[MarshalAs(UnmanagedType.LPStr)]
		public string psy_rd;
		public int psy;
		public int rc_lookahead;
		public int weightp;
		public int weightb;
		public int ssim;
		public int intra_refresh;
		public int b_bias;
		public int b_pyramid;
		public int mixed_refs;
		public int dct8x8;
		public int fast_pskip;
		public int aud;
		public int mbtree;
		[MarshalAs(UnmanagedType.LPStr)]
		public string deblock;
		public float cplxblur;
		[MarshalAs(UnmanagedType.LPStr)]
		public string partitions;
		public int direct_pred;
		public int slice_max_size;
	}
	#endregion

    static internal class helper
    {
        static internal object _oSyncRootGlobal = new object();
        static internal string _sLoggerPath = null;

        static private bool _bInitialized = false;
        static private Functions.LogCallbackDelegate _fLogCallbackDelegate;
        //static private IntPtr _pLogCallbackDelegate;
        //static private GCHandle _cLogCallbackGCH;

        static internal void Initialize()
        {
            lock (_oSyncRootGlobal)
            {
                if (!_bInitialized)
                {
                    Functions.av_register_all();
                    Functions.avformat_network_init();
                    Functions.avdevice_register_all();
                    _bInitialized = true;
#if DEBUG
                    if (Logger.bDebug)
                    {
                        Functions.av_log_set_level(Constants.AV_LOG_DEBUG);
                        _fLogCallbackDelegate = LogCallback;
                        //_pLogCallbackDelegate = Marshal.GetFunctionPointerForDelegate((Functions.LogCallbackDelegate)LogCallback);//_fLogCallbackDelegate
                        //_cLogCallbackGCH = GCHandle.Alloc(_fLogCallbackDelegate);

                        Functions.av_log_set_callback(Marshal.GetFunctionPointerForDelegate(_fLogCallbackDelegate));//_pLogCallbackDelegate);
                    }
#else
                    Functions.av_log_set_level(Constants.AV_LOG_FATAL);
#endif
                }
            }
        }
        static internal string ErrorDescriptionGet(int nError)
        {
            StringBuilder sError = new StringBuilder();
            nError = ffmpeg.net.Functions.av_strerror(nError, sError, 1024);
            return sError.ToString();
        }
        static private void LogCallback(IntPtr ptr, int level, string fmt, IntPtr vl)
        {
            //return;
            //if(level <= Constants.AV_LOG_WARNING)

            if ("%s" != fmt && "%s: fd=%d" != fmt && "\n" != fmt && !fmt.StartsWith("frame=%4d") && !fmt.StartsWith("scene cut at"))
                (new helpers.Logger(level.ToString(), "ffmpeg", true)).WriteDebug(fmt);
        }
    }

    public class Constants
	{
		#region SWS_*
		/* values for the flags, the stuff on the command line is different */
		static public int SWS_FAST_BILINEAR = 1;
		static public int SWS_BILINEAR = 2;
		static public int SWS_BICUBIC = 4;
		static public int SWS_X = 8;
		static public int SWS_POINT = 0x10;
		static public int SWS_AREA = 0x20;
		static public int SWS_BICUBLIN = 0x40;
		static public int SWS_GAUSS = 0x80;
		static public int SWS_SINC = 0x100;
		static public int SWS_LANCZOS = 0x200;
		static public int SWS_SPLINE = 0x400;

		static public int SWS_SRC_V_CHR_DROP_MASK = 0x30000;
		static public int SWS_SRC_V_CHR_DROP_SHIFT = 16;

		static public int SWS_PARAM_DEFAULT = 123456;

		static public int SWS_PRINT_INFO = 0x1000;

		//the following 3 flags are not completely implemented
		//internal chrominace subsampling info
		static public int SWS_FULL_CHR_H_INT = 0x2000;
		//input subsampling info
		static public int SWS_FULL_CHR_H_INP = 0x4000;
		static public int SWS_DIRECT_BGR = 0x8000;
		static public int SWS_ACCURATE_RND = 0x40000;
		static public int SWS_BITEXACT = 0x80000;

		//#if FF_API_SWS_CPU_CAPS
		/**
		* CPU caps are autodetected now, those flags
		* are only provided for API compatibility.
		*/
		static public uint SWS_CPU_CAPS_MMX = 0x80000000;
		static public int SWS_CPU_CAPS_MMX2 = 0x20000000;
		static public int SWS_CPU_CAPS_3DNOW = 0x40000000;
		static public int SWS_CPU_CAPS_ALTIVEC = 0x10000000;
		static public int SWS_CPU_CAPS_BFIN = 0x01000000;
		static public int SWS_CPU_CAPS_SSE2 = 0x02000000;
		//#endif

		static public float SWS_MAX_REDUCE_CUTOFF = 0.002F;

		static public int SWS_CS_ITU709 = 1;
		static public int SWS_CS_FCC = 4;
		static public int SWS_CS_ITU601 = 5;
		static public int SWS_CS_ITU624 = 5;
		static public int SWS_CS_SMPTE170M = 5;
		static public int SWS_CS_SMPTE240M = 7;
		static public int SWS_CS_DEFAULT = 5;
		#endregion

		static public int FF_INPUT_BUFFER_PADDING_SIZE = 16;
		static private ulong AV_NOPTS_VALUE_UL = 0x8000000000000000;
		static public long AV_NOPTS_VALUE = (long)AV_NOPTS_VALUE_UL;
		static public int AV_PKT_FLAG_KEY = 0x0001;
		//! Demuxer will use url_fopen, no opened file should be provided by the caller.
		static public int AVFMT_NOFILE = 0x0001;
		static public int AVFMT_NEEDNUMBER = 0x0002; /**< Needs '%d' in filename. */
		static public int AVFMT_SHOW_IDS = 0x0008; /**< Show format stream IDs numbers. */
		static public int AVFMT_RAWPICTURE = 0x0020; /**< Format wants AVPicture structure for raw picture data. */
		static public int AVFMT_GLOBALHEADER = 0x0040; /**< Format wants global header. */
		static public int AVFMT_NOTIMESTAMPS = 0x0080; /**< Format does not need / have any timestamps. */
		static public int AVFMT_GENERIC_INDEX = 0x0100; /**< Use generic index building code. */
		static public int AVFMT_TS_DISCONT = 0x0200; /**< Format allows timestamp discontinuities. */
		static public int AVFMT_VARIABLE_FPS = 0x0400; /**< Format allows variable fps. */
		static public int AVFMT_NODIMENSIONS = 0x0800; /**< Format does not need width/height */

		static public int AVIO_FLAG_READ = 1; /**< read-only */
		static public int AVIO_FLAG_WRITE = 2; /**< write-only */
		static public int AVIO_FLAG_READ_WRITE = (AVIO_FLAG_READ | AVIO_FLAG_WRITE); /**< read-write pseudo flag */

		static public int AV_LOG_QUIET = -8;

		/**
		* Something went really wrong and we will crash now.
		*/
		static public int AV_LOG_PANIC = 0;

		/**
		* Something went wrong and recovery is not possible.
		* For example, no header was found for a format which depends
		* on headers or an illegal combination of parameters is used.
		*/
		static public int AV_LOG_FATAL = 8;

		/**
		* Something went wrong and cannot losslessly be recovered.
		* However, not all future data is affected.
		*/
		static public int AV_LOG_ERROR = 16;

		/**
		* Something somehow does not look correct. This may or may not
		* lead to problems. An example would be the use of '-vstrict -2'.
		*/
		static public int AV_LOG_WARNING = 24;

		static public int AV_LOG_INFO = 32;
		static public int AV_LOG_VERBOSE = 40;

		/**
		* Stuff which is only useful for libav* developers.
		*/
		static public int AV_LOG_DEBUG = 48;
		static public int AVSEEK_FLAG_BACKWARD = 1; ///< seek backward
		static public int AVSEEK_FLAG_BYTE = 2; ///< seeking based on position in bytes
		static public int AVSEEK_FLAG_ANY = 4; ///< seek to any frame, even non-keyframes
		static public int AVSEEK_FLAG_FRAME = 8; ///< seeking based on frame number
		///
		static public int AV_NUM_DATA_POINTERS = 8;

        static public int AUDIO_BLOCK_SIZE = 4096;
        static public int SWR_CH_MAX = 32;
	}

	public class Functions
	{
		static protected IntPtr NULL = IntPtr.Zero;
		protected IntPtr _p;
		~Functions()
		{
			try
			{
				if (NULL != _p)
					Functions.av_freep(ref _p);
			}
			catch { }
		}
		static public implicit operator IntPtr(Functions cAVBase)
		{
			return cAVBase._p;
		}

        private const string _sPathAVFormatDLL = "avformat-55.dll";//"avformat-54.dll";
        private const string _sPathAVCodecDLL = "avcodec-55.dll";//"avcodec-54.dll";
        private const string _sPathAVUtilDLL = "avutil-52.dll";//"avutil-51.dll";
        private const string _sPathSWScaleDLL = "swscale-2.dll";
        private const string _sPathSWResampleDLL = "swresample-0.dll";
        private const string _sPathAVDeviceDLL = "avdevice-55.dll";


        #region avformat
        [DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void av_register_all();
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void avformat_network_init();
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocate an AVFormatContext.
		 * Can be freed with av_free() but do not forget to free everything you
		 * explicitly allocated as well!
		 */
		//AVFormatContext* avformat_alloc_context();
		static public extern IntPtr avformat_alloc_context();
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Open an input stream and read the header. The codecs are not opened.
		 * The stream must be closed with av_close_input_file().
		 *
		 * @param ps Pointer to user-supplied AVFormatContext (allocated by avformat_alloc_context).
		 *           May be a pointer to NULL, in which case an AVFormatContext is allocated by this
		 *           function and written into ps.
		 *           Note that a user-supplied AVFormatContext will be freed on failure.
		 * @param filename Name of the stream to open.
		 * @param fmt If non-NULL, this parameter forces a specific input format.
		 *            Otherwise the format is autodetected.
		 * @param options  A dictionary filled with AVFormatContext and demuxer-private options.
		 *                 On return this parameter will be destroyed and replaced with a dict containing
		 *                 options that were not found. May be NULL.
		 *
		 * @return 0 on success, a negative AVERROR on failure.
		 *
		 * @note If you want to use custom IO, preallocate the format context and set its pb field.
		 */
		//int avformat_open_input(AVFormatContext** ps, const char* filename, AVInputFormat* fmt, AVDictionary* options);
		protected static extern int avformat_open_input(ref IntPtr ps, string filename, IntPtr fmt, IntPtr options);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		protected static extern int avformat_open_input(ref IntPtr ps, byte[] filename, IntPtr fmt, IntPtr options);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Read packets of a media file to get stream information. This
		 * is useful for file formats with no headers such as MPEG. This
		 * function also computes the real framerate in case of MPEG-2 repeat
		 * frame mode.
		 * The logical file position is not changed by this function;
		 * examined packets may be buffered for later processing.
		 *
		 * @param ic media file handle
		 * @param options  If non-NULL, an ic.nb_streams long array of pointers to
		 *                 dictionaries, where i-th member contains options for
		 *                 codec corresponding to i-th stream.
		 *                 On return each dictionary will be filled with options that were not found.
		 * @return >=0 if OK, AVERROR_xxx on error
		 *
		 * @note this function isn't guaranteed to open all the codecs, so
		 *       options being non-empty at return is a perfectly normal behavior.
		 *
		 * @todo Let the user decide somehow what information is needed so that
		 *       we do not waste time getting stuff the user does not need.
		 */
		//int avformat_find_stream_info(AVFormatContext* ic, AVDictionary** options);
		protected static extern int avformat_find_stream_info(IntPtr ic, IntPtr options);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void dump_format(/*AVFormatContext* */IntPtr ic, int index, /*const char* */StringBuilder url, int is_output);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_read_frame(/*AVCodecContext* */IntPtr s, /*AVPacket* */IntPtr pkt);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void avformat_close_input(/*AVFormatContext** */ref IntPtr s);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Add a new stream to a media file.
		 *
		 * When demuxing, it is called by the demuxer in read_header(). If the
		 * flag AVFMTCTX_NOHEADER is set in s.ctx_flags, then it may also
		 * be called in read_packet().
		 *
		 * When muxing, should be called by the user before avformat_write_header().
		 *
		 * @param c If non-NULL, the AVCodecContext corresponding to the new stream
		 * will be initialized to use this codec. This is needed for e.g. codec-specific
		 * defaults to be set, so codec should be provided if it is known.
		 *
		 * @return newly created stream or NULL on error.
		 */
		static public extern /*AVStream* */IntPtr avformat_new_stream(/*AVFormatContext* */IntPtr s, /*AVCodec* */IntPtr c);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Write a packet to an output media file ensuring correct interleaving.
		 *
		 * The packet must contain one audio or video frame.
		 * If the packets are already correctly interleaved, the application should
		 * call av_write_frame() instead as it is slightly faster. It is also important
		 * to keep in mind that completely non-interleaved input will need huge amounts
		 * of memory to interleave with this, so it is preferable to interleave at the
		 * demuxer level.
		 *
		 * @param s media file handle
		 * @param pkt The packet, which contains the stream_index, buf/buf_size,
					  dts/pts, ...
		 * @return < 0 on error, = 0 if OK, 1 if end of stream wanted
		 */
		static public extern int av_interleaved_write_frame(/*AVFormatContext* */IntPtr s, /*AVPacket* */IntPtr pkt);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Write a packet to an output media file.
		 *
		 * The packet shall contain one audio or video frame.
		 * The packet must be correctly interleaved according to the container
		 * specification, if not then av_interleaved_write_frame must be used.
		 *
		 * @param s media file handle
		 * @param pkt The packet, which contains the stream_index, buf/buf_size,
		 *            dts/pts, ...
		 *            This can be NULL (at any time, not just at the end), in
		 *            order to immediately flush data buffered within the muxer,
		 *            for muxers that buffer up data internally before writing it
		 *            to the output.
		 * @return < 0 on error, = 0 if OK, 1 if flushed and there is no more data to flush
		 */
		static public extern int av_write_frame(/*AVFormatContext* */IntPtr s, /*AVPacket* */IntPtr pkt);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Return the output format in the list of registered output formats
		 * which best matches the provided parameters, or return NULL if
		 * there is no match.
		 *
		 * @param short_name if non-NULL checks if short_name matches with the
		 * names of the registered formats
		 * @param filename if non-NULL checks if filename terminates with the
		 * extensions of the registered formats
		 * @param mime_type if non-NULL checks if mime_type matches with the
		 * MIME type of the registered formats
		 */
		static public extern /*AVOutputFormat* */IntPtr av_guess_format(/*const char* */StringBuilder short_name, /*const char* */StringBuilder filename, /*const char* */StringBuilder mime_type);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Guess the codec ID based upon muxer and filename.
		 */
		static internal extern AVCodecID av_guess_codec(/*AVOutputFormat* */IntPtr fmt, /*const char* */StringBuilder short_name, /*const char* */StringBuilder filename, /*const char* */StringBuilder mime_type, AVMediaType type);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocate an AVFormatContext for an output format.
		 * avformat_free_context() can be used to free the context and
		 * everything allocated by the framework within it.
		 *
		 * @param *ctx is set to the created format context, or to NULL in
		 * case of failure
		 * @param oformat format to use for allocating the context, if NULL
		 * format_name and filename are used instead
		 * @param format_name the name of output format to use for allocating the
		 * context, if NULL filename is used instead
		 * @param filename the name of the filename to use for allocating the
		 * context, may be NULL
		 * @return >= 0 in case of success, a negative AVERROR code in case of
		 * failure
		 */
		static public extern int avformat_alloc_output_context2(/*AVFormatContext** */ref IntPtr ctx, /*AVOutputFormat* */IntPtr oformat, /*const char* */StringBuilder format_name, /*const char* */StringBuilder filename);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void avformat_free_context(/*AVFormatContext** */IntPtr ctx);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * media file output
		 */
		static public extern int av_set_parameters(/*AVFormatContext* */IntPtr s, /*AVFormatParameters* */IntPtr ap);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        /**
         * Allocate and initialize an AVIOContext for buffered I/O. It must be later
         * freed with av_free().
         *
         * @param buffer Memory block for input/output operations via AVIOContext.
         *        The buffer must be allocated with av_malloc() and friends.
         * @param buffer_size The buffer size is very important for performance.
         *        For protocols with fixed blocksize it should be set to this blocksize.
         *        For others a typical size is a cache page, e.g. 4kb.
         * @param write_flag Set to 1 if the buffer should be writable, 0 otherwise.
         * @param opaque An opaque pointer to user-specific data.
         * @param read_packet  A function for refilling the buffer, may be NULL.
         * @param write_packet A function for writing the buffer contents, may be NULL.
         *        The function may not change the input buffers content.
         * @param seek A function for seeking to specified byte position, may be NULL.
         *
         * @return Allocated AVIOContext or NULL on failure.
         */
        //AVIOContext *avio_alloc_context(unsigned char *buffer, int buffer_size, int write_flag, void *opaque, int (*read_packet)(void *opaque, uint8_t *buf, int buf_size), int (*write_packet)(void *opaque, uint8_t *buf, int buf_size), int64_t (*seek)(void *opaque, int64_t offset, int whence));
        static public extern IntPtr avio_alloc_context(IntPtr buffer, int buffer_size, int write_flag, IntPtr opaque, IntPtr read_packet, IntPtr write_packet, IntPtr seek);
        public delegate int AVIOBufferReadWriteDelegate(IntPtr opaque, IntPtr buf, int buf_size);
        public delegate long AVIOBufferSeekDelegate(IntPtr opaque, long offset, int whence);
        [DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Create and initialize a AVIOContext for accessing the
		 * resource indicated by url.
		 * @note When the resource indicated by url has been opened in
		 * read+write mode, the AVIOContext can be used only for writing.
		 *
		 * @param s Used to return the pointer to the created AVIOContext.
		 * In case of failure the pointed to value is set to NULL.
		 * @param flags flags which control how the resource indicated by url
		 * is to be opened
		 * @return 0 in case of success, a negative value corresponding to an
		 * AVERROR code in case of failure
		 */
		static public extern int avio_open(/*AVIOContext** */ref IntPtr s, /*const char* */StringBuilder url, int flags);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//void avio_wb32(AVIOContext *s, uint64_t val); 
		static public extern void avio_wb32(IntPtr s, uint val);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//void avio_wb64(AVIOContext *s, uint64_t val);
		static public extern void avio_wb64(IntPtr s, ulong val);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Write a NULL-terminated string.
		 * @return number of bytes written.
		 */
		//int avio_put_str(AVIOContext *s, const char *str);
		static public extern int avio_put_str(IntPtr s, StringBuilder str);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Close the resource accessed by the AVIOContext s and free it.
		 * This function can only be used if s was opened by avio_open().
		 *
		 * @return 0 on success, an AVERROR < 0 on error.
		 */
		static public extern int avio_close(/*AVIOContext* */IntPtr s);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocate the stream private data and write the stream header to
		 * an output media file.
		 *
		 * @param s Media file handle, must be allocated with avformat_alloc_context().
		 *          Its oformat field must be set to the desired output format;
		 *          Its pb field must be set to an already openened AVIOContext.
		 * @param options  An AVDictionary filled with AVFormatContext and muxer-private options.
		 *                 On return this parameter will be destroyed and replaced with a dict containing
		 *                 options that were not found. May be NULL.
		 *
		 * @return 0 on success, negative AVERROR on failure.
		 *
		 * @see av_opt_find, av_dict_set, avio_open, av_oformat_next.
		 */
		static public extern int avformat_write_header(/*AVFormatContext* */IntPtr s, /*AVDictionary** */IntPtr options);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Write the stream trailer to an output media file and free the
		 * file private data.
		 *
		 * May only be called after a successful call to av_write_header.
		 *
		 * @param s media file handle
		 * @return 0 if OK, AVERROR_xxx on error
		 */
		static public extern int av_write_trailer(/*AVFormatContext* */IntPtr s);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Seeks to timestamp ts.
		 * Seeking will be done so that the point from which all active streams
		 * can be presented successfully will be closest to ts and within min/max_ts.
		 * Active streams are all streams that have AVStream.discard < AVDISCARD_ALL.
		 *
		 * If flags contain AVSEEK_FLAG_BYTE, then all timestamps are in bytes and
		 * are the file position (this may not be supported by all demuxers).
		 * If flags contain AVSEEK_FLAG_FRAME, then all timestamps are in frames
		 * in the stream with stream_index (this may not be supported by all demuxers).
		 * Otherwise all timestamps are in units of the stream selected by stream_index
		 * or if stream_index is -1, in AV_TIME_BASE units.
		 * If flags contain AVSEEK_FLAG_ANY, then non-keyframes are treated as
		 * keyframes (this may not be supported by all demuxers).
		 *
		 * @param stream_index index of the stream which is used as time base reference
		 * @param min_ts smallest acceptable timestamp
		 * @param ts target timestamp
		 * @param max_ts largest acceptable timestamp
		 * @param flags flags
		 * @return >=0 on success, error code otherwise
		 *
		 * @NOTE This is part of the new seek API which is still under construction.
		 *       Thus do not use this yet. It may change at any time, do not expect
		 *       ABI compatibility yet!
		 */
		protected static extern int avformat_seek_file(/*AVFormatContext* */IntPtr s, int stream_index, /*int64_t*/long min_ts, /*int64_t*/long ts, /*int64_t*/long max_ts, int flags);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Seeks to the keyframe at timestamp.
		 * 'timestamp' in 'stream_index'.
		 * @param stream_index If stream_index is (-1), a default
		 * stream is selected, and timestamp is automatically converted
		 * from AV_TIME_BASE units to the stream specific time_base.
		 * @param timestamp Timestamp in AVStream.time_base units
		 *        or, if no stream is specified, in AV_TIME_BASE units.
		 * @param flags flags which select direction and seeking mode
		 * @return >= 0 on success
		 */
		static public extern int av_seek_frame(/*AVFormatContext* */IntPtr s, int stream_index, /*int64_t*/long timestamp, int flags);
		[DllImport(_sPathAVFormatDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Seeks to the keyframe at timestamp.
		 * 'timestamp' in 'stream_index'.
		 * @param stream_index If stream_index is (-1), a default
		 * stream is selected, and timestamp is automatically converted
		 * from AV_TIME_BASE units to the stream specific time_base.
		 * @param timestamp Timestamp in AVStream.time_base units
		 *        or, if no stream is specified, in AV_TIME_BASE units.
		 * @param flags flags which select direction and seeking mode
		 * @return >= 0 on success
		 */
        //AVInputFormat *av_find_input_format(const char *short_name)
        static public extern IntPtr av_find_input_format(string short_name);

		#endregion

		#region avcodec
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void avcodec_register_all();
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern /*AVCodec* */IntPtr avcodec_find_decoder(AVCodecID id);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/*
		 * Initialize the AVCodecContext to use the given AVCodec. Prior to using this
		 * function the context has to be allocated with avcodec_alloc_context3().
		 *
		 * The functions avcodec_find_decoder_by_name(), avcodec_find_encoder_by_name(),
		 * avcodec_find_decoder() and avcodec_find_encoder() provide an easy way for
		 * retrieving a codec.
		 *
		 * @warning This function is not thread safe!
		 *
		 * @code
		 * avcodec_register_all();
		 * av_dict_set(&opts, "b", "2.5M", 0);
		 * codec = avcodec_find_decoder(CODEC_ID_H264);
		 * if (!codec)
		 *     exit(1);
		 *
		 * context = avcodec_alloc_context3(codec);
		 *
		 * if (avcodec_open2(context, codec, opts) < 0)
		 *     exit(1);
		 * @endcode
		 *
		 * @param avctx The context to initialize.
		 * @param codec The codec to open this context for. If a non-NULL codec has been
		 *              previously passed to avcodec_alloc_context3() or
		 *              avcodec_get_context_defaults3() for this context, then this
		 *              parameter MUST be either NULL or equal to the previously passed
		 *              codec.
		 * @param options A dictionary filled with AVCodecContext and codec-private options.
		 *                On return this object will be filled with options that were not found.
		 *
		 * @return zero on success, a negative value on error
		 * @see avcodec_alloc_context3(), avcodec_find_decoder(), avcodec_find_encoder(),
		 *      av_dict_set(), av_opt_find().
		 */
		static public extern int avcodec_open2(/*AVCodecContext* */IntPtr avctx, /*AVCodec* */IntPtr codec, /*AVDictionary** */IntPtr options);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern /*AVFrame* */IntPtr avcodec_alloc_frame_todel();
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void avcodec_free_frame_todel(/*AVFrame***/ref IntPtr frame);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Set the fields of the given AVFrame to default values.
		 *
		 * @param pic The AVFrame of which the fields should be set to default values.
		 */
        static public extern void avcodec_get_frame_defaults_todel(/*AVFrame* */IntPtr pic);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int avcodec_decode_video2(AVCodecContext* avctx, AVFrame* picture, int* got_picture_ptr, AVPacket* avpkt);
		static public extern int avcodec_decode_video2(IntPtr avctx, IntPtr picture, ref int got_picture_ptr, IntPtr avpkt);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        /**
         * Decode the audio frame of size avpkt->size from avpkt->data into frame.
         *
         * Some decoders may support multiple frames in a single AVPacket. Such
         * decoders would then just decode the first frame. In this case,
         * avcodec_decode_audio4 has to be called again with an AVPacket containing
         * the remaining data in order to decode the second frame, etc...
         * Even if no frames are returned, the packet needs to be fed to the decoder
         * with remaining data until it is completely consumed or an error occurs.
         *
         * @warning The input buffer, avpkt->data must be FF_INPUT_BUFFER_PADDING_SIZE
         *          larger than the actual read bytes because some optimized bitstream
         *          readers read 32 or 64 bits at once and could read over the end.
         *
         * @note You might have to align the input buffer. The alignment requirements
         *       depend on the CPU and the decoder.
         *
         * @param      avctx the codec context
         * @param[out] frame The AVFrame in which to store decoded audio samples.
         *                   The decoder will allocate a buffer for the decoded frame by
         *                   calling the AVCodecContext.get_buffer2() callback.
         *                   When AVCodecContext.refcounted_frames is set to 1, the frame is
         *                   reference counted and the returned reference belongs to the
         *                   caller. The caller must release the frame using av_frame_unref()
         *                   when the frame is no longer needed. The caller may safely write
         *                   to the frame if av_frame_is_writable() returns 1.
         *                   When AVCodecContext.refcounted_frames is set to 0, the returned
         *                   reference belongs to the decoder and is valid only until the
         *                   next call to this function or until closing the decoder.
         *                   The caller may not write to it.
         * @param[out] got_frame_ptr Zero if no frame could be decoded, otherwise it is
         *                           non-zero.
         * @param[in]  avpkt The input AVPacket containing the input buffer.
         *                   At least avpkt->data and avpkt->size should be set. Some
         *                   decoders might also require additional fields to be set.
         * @return A negative error code is returned if an error occurred during
         *         decoding, otherwise the number of bytes consumed from the input
         *         AVPacket is returned.
         */
        //int avcodec_decode_audio4(AVCodecContext *avctx, AVFrame *frame, int *got_frame_ptr, const AVPacket *avpkt);
        static public extern int avcodec_decode_audio4(IntPtr avctx, IntPtr frame, ref bool got_frame_ptr, IntPtr avpkt);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int avcodec_decode_audio3(AVCodecContext* avctx, int16_t* samples, int* frame_size_ptr, AVPacket* avpkt);
		static public extern int avcodec_decode_audio3(IntPtr avctx, short[] samples, ref int frame_size_ptr, IntPtr avpkt);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocate the payload of a packet and initialize its fields with
		 * default values.
		 *
		 * @param pkt packet
		 * @param size wanted payload size
		 * @return 0 if OK, AVERROR_xxx otherwise
		 */
		static public extern int av_new_packet(/*AVPacket* */IntPtr pkt, int size);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void av_free_packet(/*AVPacket* */IntPtr pkt);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int avcodec_close(/*AVCodecContext* */IntPtr avctx);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Calculate the size in bytes that a picture of the given width and height
		 * would occupy if stored in the given picture format.
		 * Note that this returns the size of a compact representation as generated
		 * by avpicture_layout, which can be smaller than the size required for e.g.
		 * avpicture_fill.
		 *
		 * @param pix_fmt the given picture format
		 * @param width the width of the image
		 * @param height the height of the image
		 * @return Image data size in bytes or -1 on error (e.g. too large dimensions).
		 */
        static public extern int avpicture_get_size(PixelFormat pix_fmt, int width, int height);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        /**
         * Fill in the AVPicture fields.
         * The fields of the given AVPicture are filled in by using the 'ptr' address
         * which points to the image data buffer. Depending on the specified picture
         * format, one or multiple image data pointers and line sizes will be set.
         * If a planar format is specified, several pointers will be set pointing to
         * the different picture planes and the line sizes of the different planes
         * will be stored in the lines_sizes array.
         * Call with ptr == NULL to get the required size for the ptr buffer.
         *
         * @param picture AVPicture whose fields are to be filled in
         * @param ptr Buffer which will contain or contains the actual image data
         * @param pix_fmt The format in which the picture data is stored.
         * @param width the width of the image in pixels
         * @param height the height of the image in pixels
         * @return size of the image data in bytes
         */
        //int avpicture_fill(AVPicture* picture, uint8_t* ptr, PixelFormat pix_fmt, int width, int height);
        static public extern int avpicture_fill(IntPtr picture, byte[] ptr, PixelFormat pix_fmt, int width, int height);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern int avpicture_fill(IntPtr picture, IntPtr ptr, PixelFormat pix_fmt, int width, int height);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern /*AVCodec* */IntPtr avcodec_find_encoder(AVCodecID id);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Initialize optional fields of a packet with default values.
		 *
		 * @param pkt packet
		 */
		static public extern void av_init_packet(/*AVPacket* */IntPtr pkt);

		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Encode an audio frame from samples into buf.
		 *
		 * @deprecated Use avcodec_encode_audio2 instead.
		 *
		 * @note The output buffer should be at least FF_MIN_BUFFER_SIZE bytes large.
		 * However, for codecs with avctx->frame_size equal to 0 (e.g. PCM) the user
		 * will know how much space is needed because it depends on the value passed
		 * in buf_size as described below. In that case a lower value can be used.
		 *
		 * @param avctx the codec context
		 * @param[out] buf the output buffer
		 * @param[in] buf_size the output buffer size
		 * @param[in] samples the input buffer containing the samples
		 * The number of samples read from this buffer is frame_size*channels,
		 * both of which are defined in avctx.
		 * For codecs which have avctx->frame_size equal to 0 (e.g. PCM) the number of
		 * samples read from samples is equal to:
		 * buf_size * 8 / (avctx->channels * av_get_bits_per_sample(avctx->codec_id))
		 * This also implies that av_get_bits_per_sample() must not return 0 for these
		 * codecs.
		 * @return On error a negative value is returned, on success zero or the number
		 * of bytes used to encode the data read from the input buffer.
		 */
		//int avcodec_encode_audio(AVCodecContext* avctx, uint8_t* buf, int buf_size, const short* samples);
		static public extern int avcodec_encode_audio(/*AVCodecContext* */IntPtr avctx, /*uint8_t* */IntPtr buf, int buf_size, /*const short* */IntPtr samples);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int avcodec_encode_audio(AVCodecContext* avctx, uint8_t* buf, int buf_size, const short* samples);
		static public extern int avcodec_encode_audio(/*AVCodecContext* */IntPtr avctx, byte[] buf, int buf_size, byte[] samples);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int avcodec_encode_audio(AVCodecContext* avctx, uint8_t* buf, int buf_size, const short* samples);
		static public extern int avcodec_encode_audio(/*AVCodecContext* */IntPtr avctx, byte[] buf, int buf_size, short[] samples);

		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Encode a frame of audio.
		 *
		 * Takes input samples from frame and writes the next output packet, if
		 * available, to avpkt. The output packet does not necessarily contain data for
		 * the most recent frame, as encoders can delay, split, and combine input frames
		 * internally as needed.
		 *
		 * @param avctx     codec context
		 * @param avpkt     output AVPacket.
		 *                  The user can supply an output buffer by setting
		 *                  avpkt->data and avpkt->size prior to calling the
		 *                  function, but if the size of the user-provided data is not
		 *                  large enough, encoding will fail. All other AVPacket fields
		 *                  will be reset by the encoder using av_init_packet(). If
		 *                  avpkt->data is NULL, the encoder will allocate it.
		 *                  The encoder will set avpkt->size to the size of the
		 *                  output packet.
		 * @param[in] frame AVFrame containing the raw audio data to be encoded.
		 *                  May be NULL when flushing an encoder that has the
		 *                  CODEC_CAP_DELAY capability set.
		 *                  There are 2 codec capabilities that affect the allowed
		 *                  values of frame->nb_samples.
		 *                  If CODEC_CAP_SMALL_LAST_FRAME is set, then only the final
		 *                  frame may be smaller than avctx->frame_size, and all other
		 *                  frames must be equal to avctx->frame_size.
		 *                  If CODEC_CAP_VARIABLE_FRAME_SIZE is set, then each frame
		 *                  can have any number of samples.
		 *                  If neither is set, frame->nb_samples must be equal to
		 *                  avctx->frame_size for all frames.
		 * @param[out] got_packet_ptr This field is set to 1 by libavcodec if the
		 *                            output packet is non-empty, and to 0 if it is
		 *                            empty. If the function returns an error, the
		 *                            packet can be assumed to be invalid, and the
		 *                            value of got_packet_ptr is undefined and should
		 *                            not be used.
		 * @return          0 on success, negative error code on failure
		 */
		//int avcodec_encode_audio2(AVCodecContext* avctx, AVPacket* avpkt, const AVFrame* frame, int* got_packet_ptr);
		static public extern int avcodec_encode_audio2(IntPtr avctx, IntPtr avpkt, IntPtr frame, ref int got_packet_ptr);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Encode a video frame from pict into buf.
		 * The input picture should be
		 * stored using a specific format, namely avctx.pix_fmt.
		 *
		 * @param avctx the codec context
		 * @param[out] buf the output buffer for the bitstream of encoded frame
		 * @param[in] buf_size the size of the output buffer in bytes
		 * @param[in] pict the input picture to encode
		 * @return On error a negative value is returned, on success zero or the number
		 * of bytes used from the output buffer.
		 */
		//int avcodec_encode_video(AVCodecContext* avctx, uint8_t* buf, int buf_size, const AVFrame* pict);
		//static public extern int avcodec_encode_video(IntPtr avctx, IntPtr buf, int buf_size, IntPtr pict);
		static public extern int avcodec_encode_video(IntPtr avctx, byte[] buf, int buf_size, IntPtr pict);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocate an AVCodecContext and set its fields to default values.  The
		 * resulting struct can be deallocated by calling avcodec_close() on it followed
		 * by av_free().
		 *
		 * @param codec if non-NULL, allocate private data and initialize defaults
		 *              for the given codec. It is illegal to then call avcodec_open2()
		 *              with a different codec.
		 *              If NULL, then the codec-specific defaults won't be initialized,
		 *              which may result in suboptimal default settings (this is
		 *              important mainly for encoders, e.g. libx264).
		 *
		 * @return An AVCodecContext filled with default values or NULL on failure.
		 * @see avcodec_get_context_defaults
		 */
		static public extern /*AVCodecContext* */IntPtr avcodec_alloc_context3(/*AVCodec* */IntPtr codec);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//static public extern /*AVCodecContext* */IntPtr avcodec_alloc_context();
		/**
		 * Set the fields of the given AVCodecContext to default values corresponding
		 * to the given codec (defaults may be codec-dependent).
		 *
		 * Do not call this function if a non-NULL codec has been passed
		 * to avcodec_alloc_context3() that allocated this AVCodecContext.
		 * If codec is non-NULL, it is illegal to call avcodec_open2() with a
		 * different codec on this AVCodecContext.
		 */
		static public extern int avcodec_get_context_defaults3(/*AVCodecContext* */IntPtr s, /*AVCodec* */IntPtr codec);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * @warning This is a hack - the packet memory allocation stuff is broken. The
		 * packet is allocated if it was not really allocated.
		 */
		static public extern int av_dup_packet(/*AVPacket* */IntPtr pkt);

		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 *  Initializes audio resampling context
		 *
		 * @param output_channels  number of output channels
		 * @param input_channels   number of input channels
		 * @param output_rate      output sample rate
		 * @param input_rate       input sample rate
		 * @param sample_fmt_out   requested output sample format
		 * @param sample_fmt_in    input sample format
		 * @param filter_length    length of each FIR filter in the filterbank relative to the cutoff freq
		 * @param log2_phase_count log2 of the number of entries in the polyphase filterbank
		 * @param linear           If 1 then the used FIR filter will be linearly interpolated
								   between the 2 closest, if 0 the closest will be used
		 * @param cutoff           cutoff frequency, 1.0 corresponds to half the output sampling rate
		 * @return allocated ReSampleContext, NULL if error occured
		 */
		static public extern /*ReSampleContext* */IntPtr av_audio_resample_init(int output_channels, int input_channels, int output_rate, int input_rate, AVSampleFormat sample_fmt_out, AVSampleFormat sample_fmt_in, int filter_length, int log2_phase_count, int linear, double cutoff);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int audio_resample(ReSampleContext* s, short* output, short* input, int nb_samples);
		static public extern int audio_resample(IntPtr s, byte[] output, byte[] input, int nb_samples);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//int audio_resample(ReSampleContext* s, short* output, short* input, int nb_samples);
		static public extern int audio_resample(IntPtr s, short[] output, short[] input, int nb_samples);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void audio_resample_close(/*ReSampleContext* */IntPtr s);
		[DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Flush buffers, should be called when seeking or when switching to a different stream.
		 */
		static public extern void avcodec_flush_buffers(/*AVCodecContext* */IntPtr avctx);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        // int avcodec_fill_audio_frame(AVFrame *frame, int nb_channels, enum AVSampleFormat sample_fmt, const uint8_t *buf, int buf_size, int align)
        static public extern int avcodec_fill_audio_frame(IntPtr frame, int nb_channels, AVSampleFormat sample_fmt, byte[] buf, int buf_size, int align);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        // int avcodec_fill_audio_frame(AVFrame *frame, int nb_channels, enum AVSampleFormat sample_fmt, const uint8_t *buf, int buf_size, int align)
        static public extern int avcodec_fill_audio_frame(IntPtr frame, int nb_channels, AVSampleFormat sample_fmt, IntPtr buf, int buf_size, int align);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //static public extern void AVBitStreamFilterContext *av_bitstream_filter_init(const char *name);
        static public extern IntPtr av_bitstream_filter_init(StringBuilder name);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int av_bitstream_filter_filter(AVBitStreamFilterContext *bsfc, AVCodecContext *avctx, const char *args, uint8_t **poutbuf, int *poutbuf_size, const uint8_t *buf, int buf_size, int keyframe)
        static public extern int av_bitstream_filter_filter(IntPtr bsfc, IntPtr avctx, IntPtr args, ref IntPtr poutbuf, ref int poutbuf_size, IntPtr buf, int buf_size, bool keyframe);
        [DllImport(_sPathAVCodecDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //void av_bitstream_filter_close(AVBitStreamFilterContext *bsf)
        static public extern void av_bitstream_filter_close(IntPtr bsfc);
        #endregion

		#region avutil
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Memory allocation of size byte with alignment suitable for all memory accesses
		 * (including vectors if available on the CPU). av_malloc(0) must return a non NULL pointer.
		*/
		static public extern /*void* */IntPtr av_malloc(uint size);
		static public IntPtr av_malloc(int nSize)
		{
			return av_malloc((uint)nSize);
		}
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Reallocate the given block if it is not large enough, otherwise do nothing.
		 *
		 * @see av_realloc
		 */
		//void *av_fast_realloc(void *ptr, unsigned int *size, size_t min_size); 
		static public extern IntPtr av_fast_realloc(IntPtr ptr, IntPtr size, int min_size);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void av_free(/*void* */IntPtr ptr);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_strerror(int errnum, /*char* */StringBuilder errbuf, /*size_t*/ uint errbuf_size);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Rescale a 64-bit integer by 2 rational numbers.
		 */
		static public extern /*int64_t*/long av_rescale_q(/*int64_t*/long a, AVRational bq, AVRational cq);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Rescales a 64-bit integer with rounding to nearest.
		 * A simple a*b/c isn't possible as it can overflow.
		 */
		static public extern /*int64_t*/long av_rescale(/*int64_t*/long a, /*int64_t*/long b, /*int64_t*/long c);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Free a memory block which has been allocated with av_malloc(z)() or
		 * av_realloc() and set the pointer pointing to it to NULL.
		 * @param ptr Pointer to the pointer to the memory block which should
		 * be freed.
		 * @see av_free()
		 */
		static public extern void av_freep(ref /*void* */IntPtr ptr);

        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void av_log_set_level(int level);
        //public delegate void LogCallbackDelegate(IntPtr ptr, int level, IntPtr fmt, IntPtr vl);
        public delegate void LogCallbackDelegate(IntPtr ptr, int level, string fmt, IntPtr vl);
        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern void av_log_set_callback(/*void (*)(void*, int, const char*, va_list)*/IntPtr callback);
		
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        /**
		 * Initializes an AVFifoBuffer.
		 * @param size of FIFO
		 * @return AVFifoBuffer or NULL in case of memory allocation failure
		 */
		static public extern /*AVFifoBuffer* */IntPtr av_fifo_alloc(uint size);

		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Frees an AVFifoBuffer.
		 * @param *f AVFifoBuffer to free
		 */
		static public extern void av_fifo_free(/*AVFifoBuffer* */IntPtr f);

		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Feeds data from an AVFifoBuffer to a user-supplied callback.
		 * @param *f AVFifoBuffer to read from
		 * @param buf_size number of bytes to read
		 * @param *func generic read function
		 * @param *dest data destination
		 */
		static public extern int av_fifo_generic_read(/*AVFifoBuffer* */IntPtr f, /*void* */IntPtr dest, int buf_size, /*void (*func)(void*, void*, int)*/IntPtr func);

		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Feeds data from a user-supplied callback to an AVFifoBuffer.
		 * @param *f AVFifoBuffer to write to
		 * @param *src data source; non-const since it may be used as a
		 * modifiable context by the function defined in func
		 * @param size number of bytes to write
		 * @param *func generic write function; the first parameter is src,
		 * the second is dest_buf, the third is dest_buf_size.
		 * func must return the number of bytes written to dest_buf, or <= 0 to
		 * indicate no more data available to write.
		 * If func is NULL, src is interpreted as a simple byte array for source data.
		 * @return the number of bytes written to the FIFO
		 */
		static public extern int av_fifo_generic_write(/*AVFifoBuffer* */IntPtr f, /*void* */IntPtr src, int size, /*void (*func)(void*, void*, int)*/IntPtr func);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Return number of bytes per sample.
		 *
		 * @param sample_fmt the sample format
		 * @return number of bytes per sample or zero if unknown for the given
		 * sample format
		 */
		static public extern int av_get_bytes_per_sample(AVSampleFormat sample_fmt);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
			* Get the required buffer size for the given audio parameters.
			*
			* @param[out] linesize calculated linesize, may be NULL
			* @param nb_channels   the number of channels
			* @param nb_samples    the number of samples in a single channel
			* @param sample_fmt    the sample format
			* @return              required buffer size, or negative error code on failure
			*/
		static public extern int av_samples_get_buffer_size(/*int* */IntPtr linesize, int nb_channels, int nb_samples, AVSampleFormat sample_fmt, int align);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int av_sample_fmt_is_planar(enum AVSampleFormat sample_fmt)
        static public extern int av_sample_fmt_is_planar(AVSampleFormat sample_fmt);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int64_t av_get_default_channel_layout (int nb_channels) 	
        static public extern long av_get_default_channel_layout(int nb_channels);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int av_get_bits_per_sample_fmt (enum AVSampleFormat sample_fmt)
        static public extern int av_get_bits_per_sample_fmt (AVSampleFormat sample_fmt);
        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //AVBufferRef* av_buffer_create(uint8_t *data, int size, void(*)(void *opaque, uint8_t *data) free, void *opaque, int flags)	
        static public extern IntPtr av_buffer_create(IntPtr data, int size, IntPtr free, IntPtr opaque, int flags);

        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern IntPtr av_frame_alloc();
        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void get_frame_defaults(IntPtr frame);
        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void av_frame_unref(IntPtr frame);
        [DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static public extern void av_frame_free(ref IntPtr frame);

		#region av_opt_set
		/**
		 * Those functions set the field of obj with the given name to value.
		 *
		 * @param[in] obj A struct whose first element is a pointer to an AVClass.
		 * @param[in] name the name of the field to set
		 * @param[in] val The value to set. In case of av_opt_set() if the field is not
		 * of a string type, then the given string is parsed.
		 * SI postfixes and some named scalars are supported.
		 * If the field is of a numeric type, it has to be a numeric or named
		 * scalar. Behavior with more than one scalar and +- infix operators
		 * is undefined.
		 * If the field is of a flags type, it has to be a sequence of numeric
		 * scalars or named flags separated by '+' or '-'. Prefixing a flag
		 * with '+' causes it to be set without affecting the other flags;
		 * similarly, '-' unsets a flag.
		 * @param search_flags flags passed to av_opt_find2. I.e. if AV_OPT_SEARCH_CHILDREN
		 * is passed here, then the option may be set on a child of obj.
		 *
		 * @return 0 if the value has been set, or an AVERROR code in case of
		 * error:
		 * AVERROR_OPTION_NOT_FOUND if no matching option exists
		 * AVERROR(ERANGE) if the value is out of range
		 * AVERROR(EINVAL) if the value is not valid
		 */
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_opt_set(/*void* */IntPtr obj, /*const char* */StringBuilder name, /*const char* */StringBuilder val, int search_flags);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_opt_set_int(/*void* */IntPtr obj, /*const char* */StringBuilder name, long val, int search_flags);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_opt_set_double(/*void* */IntPtr obj, /*const char* */StringBuilder name, double val, int search_flags);
		[DllImport(_sPathAVUtilDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern int av_opt_set_q(/*void* */IntPtr obj, /*const char* */StringBuilder name, AVRational val, int search_flags);
		#endregion
		#endregion

		#region swscale
		[DllImport(_sPathSWScaleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Allocates and returns a SwsContext. You need it to perform
		 * scaling/conversion operations using sws_scale().
		 *
		 * @param srcW the width of the source image
		 * @param srcH the height of the source image
		 * @param srcFormat the source image format
		 * @param dstW the width of the destination image
		 * @param dstH the height of the destination image
		 * @param dstFormat the destination image format
		 * @param flags specify which algorithm and options to use for rescaling
		 * @return a pointer to an allocated context, or NULL in case of error
		 */
		static public extern /*struct SwsContext* */IntPtr sws_getContext(int srcW, int srcH, PixelFormat srcFormat, int dstW, int dstH, PixelFormat dstFormat, int flags, /*SwsFilter* */IntPtr srcFilter, /*SwsFilter* */IntPtr dstFilter, /*const double* */IntPtr param);
		[DllImport(_sPathSWScaleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static public extern /*struct SwsContext* */IntPtr sws_freeContext(/*struct SwsContext* */IntPtr swsContext);
		[DllImport(_sPathSWScaleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		/**
		 * Scales the image slice in srcSlice and puts the resulting scaled
		 * slice in the image in dst. A slice is a sequence of consecutive
		 * rows in an image.
		 *
		 * Slices have to be provided in sequential order, either in
		 * top-bottom or bottom-top order. If slices are provided in
		 * non-sequential order the behavior of the function is undefined.
		 *
		 * @param context   the scaling context previously created with
		 *                  sws_getContext()
		 * @param srcSlice  the array containing the pointers to the planes of
		 *                  the source slice
		 * @param srcStride the array containing the strides for each plane of
		 *                  the source image
		 * @param srcSliceY the position in the source image of the slice to
		 *                  process, that is the number (counted starting from
		 *                  zero) in the image of the first row of the slice
		 * @param srcSliceH the height of the source slice, that is the number
		 *                  of rows in the slice
		 * @param dst       the array containing the pointers to the planes of
		 *                  the destination image
		 * @param dstStride the array containing the strides for each plane of
		 *                  the destination image
		 * @return          the height of the output slice
		 */
		//int sws_scale(struct SwsContext* context, const uint8_t* const[] srcSlice, const int[] srcStride, int srcSliceY, int srcSliceH, uint8_t* const[] dst, const int[] dstStride);
		static public extern int sws_scale(IntPtr context, IntPtr[] srcSlice, int[] srcStride, int srcSliceY, int srcSliceH, IntPtr[] dst, int[] dstStride);
		#endregion

		#region swresample
        [DllImport(_sPathSWResampleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //struct SwrContext *swr_alloc_set_opts(struct SwrContext *s, int64_t out_ch_layout, enum AVSampleFormat out_sample_fmt, int out_sample_rate, int64_t  in_ch_layout, enum AVSampleFormat  in_sample_fmt, int  in_sample_rate, int log_offset, void *log_ctx)
        static public extern IntPtr swr_alloc_set_opts(IntPtr s, long out_ch_layout, AVSampleFormat out_sample_fmt, int out_sample_rate, long in_ch_layout, AVSampleFormat in_sample_fmt, int in_sample_rate, int log_offset, IntPtr log_ctx);
        [DllImport(_sPathSWResampleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int swr_init(struct SwrContext *s)	
        static public extern int swr_init(IntPtr s);
        [DllImport(_sPathSWResampleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //int swr_convert (struct SwrContext *s, uint8_t **out, int out_count, const uint8_t **in, int in_count)	
        static public extern int swr_convert(IntPtr s, IntPtr out_, int out_count, IntPtr in_, int in_count);
        [DllImport(_sPathSWResampleDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //void swr_free(SwrContext **ss)
        static public extern void swr_free(ref IntPtr ss);
        #endregion

        #region avdevice
        [DllImport(_sPathAVDeviceDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //void avdevice_register_all()
        static public extern void avdevice_register_all();
        #endregion
    }
}
