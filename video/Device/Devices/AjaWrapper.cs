using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;

using helpers;

namespace AjaNTV2
{
    class Logger : helpers.Logger
    {
        public Logger()
            : base("aja_wrapper", "device[" + System.Diagnostics.Process.GetCurrentProcess().Id + "]")
        { }
    }
    public class AjaInterop
    {
        internal static class Win32Interops
        {
            /// <summary>
            /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
            /// </summary>
            /// <param name="hModule">A handle to the DLL module that contains the function or variable.</param>
            /// <param name="lpProcName">he function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
            /// <returns>If the function succeeds, the return value is the address of the exported function or variable. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            /// <summary>
            /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count. When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
            /// </summary>
            /// <param name="hModule">A handle to the loaded library module.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call the GetLastError function.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern bool FreeLibrary(IntPtr hModule);

            /// <summary>
            /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
            /// </summary>
            /// <param name="lpFileName">The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file. If the string specifies a full path, the function searches only that path for the module. If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module; for more information, see the Remarks. If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\), not forward slashes (/). For more information about paths, see Naming a File or Directory. If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string.</param>
            /// <returns>If the function succeeds, the return value is a handle to the module. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            /// <summary>
            /// Closes an open object handle.
            /// </summary>
            /// <param name="handle">A valid handle to an open object.</param>
            /// <returns>If the function succeeds, the return value is nonzero.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            /// <summary>
            /// Changes the parent window of the specified child window.
            /// </summary>
            /// <param name="hWndChild">A handle to the child window.</param>
            /// <param name="hWndNewParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is HWND_MESSAGE, the child window becomes a message-only window.</param>
            /// <returns>If the function succeeds, the return value is a handle to the previous parent window. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        }
        public enum AJAStatus : int
        {
            AJA_STATUS_SUCCESS = 2,     // Function succeeded 
            AJA_STATUS_TRUE = 1,        // Result was true 
            AJA_STATUS_FALSE = 0,       // Result was false 
            AJA_STATUS_UNKNOWN = -1,    // Unknown status 
            AJA_STATUS_FAIL = -2,   // Function failed 
            AJA_STATUS_TIMEOUT = -3,    // A wait timed out 
            AJA_STATUS_RANGE = -4,  // A parameter was out of range 
            AJA_STATUS_INITIALIZE = -5, // The object has not been initialized 
            AJA_STATUS_NULL = -6,   // A pointer parameter was NULL 
            AJA_STATUS_OPEN = -7,   // Something failed to open 
            AJA_STATUS_IO = -8, // An IO operation failed 
            AJA_STATUS_DISABLED = -9,   // Device is disabled 
            AJA_STATUS_BUSY = -10,  // Device is busy 
            AJA_STATUS_BAD_PARAM = -11, // Bad parameter value 
            AJA_STATUS_FEATURE = -12,   // A required device feature does not exist 
            AJA_STATUS_UNSUPPORTED = -13,   // Parameters are valid but not supported 
            AJA_STATUS_READONLY = -14,  // Write is not supported 
            AJA_STATUS_WRITEONLY = -15, // Read is not supported 
            AJA_STATUS_MEMORY = -16,    // Memory allocation failed 
            AJA_STATUS_ALIGN = -17, // Parameter not properly aligned 
            AJA_STATUS_FLUSH = -18, // Something has been flushed 
            AJA_STATUS_NOINPUT = -19,   // No input detected 
            AJA_STATUS_SURPRISE_REMOVAL = -20,  // Hardware communication failed 

            // Sequence errors

            AJA_STATUS_NOBUFFER = -100, // No free buffers, all are scheduled or in use 
            AJA_STATUS_INVALID_TIME = -101, // Invalid schedule time 
            AJA_STATUS_NOSTREAM = -102, // No stream found 
            AJA_STATUS_TIMEEXPIRED = -103,  // Scheduled time is too late 
            AJA_STATUS_BADBUFFERCOUNT = -104,   // Buffer count out of bounds 
            AJA_STATUS_BADBUFFERSIZE = -105,    // Buffer size out of bounds 
            AJA_STATUS_STREAMCONFLICT = -106,   // Another stream is using resources 
            AJA_STATUS_NOTINITIALIZED = -107,   // Streams not initialized 
            AJA_STATUS_STREAMRUNNING = -108 // Streams is running, should be stopped 
        }
        public enum AudioSampleRateEnum : int
        {
            k44p1KHzSampleRate = 44100,
            k48KHzSampleRate = 48000,
            k96KHzSampleRate = 96000
        }
        public enum AudioChannelsPerFrameEnum : int
        {
            kNumAudioChannels2 = 2,
            kNumAudioChannels4 = 4,
            kNumAudioChannels6 = 6,
            kNumAudioChannels8 = 8,
            kNumAudioChannels16 = 16,
            kNumAudioChannelsMax = kNumAudioChannels16  // Used in Linux and Windows too
        }
        public enum AudioBitsPerSampleEnum : int
        {
            k16bitsPerSample = 16,
            k24bitsPerSample = 24,
            k32bitsPerSample = 32
        }
        public enum AudioSourceEnum : int
        {
            kSourceSDI = 0x69736469,
            kSourceAES = 0x69616573,
            kSourceADAT = 0x69616474,
            kSourceAnalog = 0x69616C67,
            kSourceNone = 0x6E6F696E,
            kSourceAll = 0x6F757420
        }
        public enum NTV2AudioSystem : int
        {
            NTV2_AUDIOSYSTEM_1,
            NTV2_AUDIOSYSTEM_2,
            NTV2_AUDIOSYSTEM_3,
            NTV2_AUDIOSYSTEM_4,
            NTV2_AUDIOSYSTEM_5,
            NTV2_AUDIOSYSTEM_6,
            NTV2_AUDIOSYSTEM_7,
            NTV2_AUDIOSYSTEM_8,
            NTV2_MAX_NUM_AudioSystemEnums,
            NTV2_NUM_AUDIOSYSTEMS = NTV2_MAX_NUM_AudioSystemEnums,
            NTV2_AUDIOSYSTEM_INVALID = NTV2_NUM_AUDIOSYSTEMS
        }
        public enum NTV2AudioRate : int
        {
            NTV2_AUDIO_48K,
            NTV2_AUDIO_96K,
            NTV2_MAX_NUM_AudioRates,
            NTV2_AUDIO_RATE_INVALID = NTV2_MAX_NUM_AudioRates
        }
        public enum NTV2AudioBufferSize : int
        {
            NTV2_AUDIO_BUFFER_STANDARD = 0, // 1 MB 00
            NTV2_AUDIO_BUFFER_BIG = 1,  // 4 MB 01
            NTV2_AUDIO_BUFFER_INVALID,
            NTV2_MAX_NUM_AudioBufferSizes = NTV2_AUDIO_BUFFER_INVALID
        }
        public enum NTV2AudioLoopBack : int
        {
            NTV2_AUDIO_LOOPBACK_OFF,
            NTV2_AUDIO_LOOPBACK_ON,
            NTV2_AUDIO_LOOPBACK_INVALID
        }
        public enum NTV2AutoCirculateState : int
        {
            NTV2_AUTOCIRCULATE_DISABLED = 0,
            NTV2_AUTOCIRCULATE_INIT,
            NTV2_AUTOCIRCULATE_STARTING,
            NTV2_AUTOCIRCULATE_PAUSED,
            NTV2_AUTOCIRCULATE_STOPPING,
            NTV2_AUTOCIRCULATE_RUNNING,
            NTV2_AUTOCIRCULATE_STARTING_AT_TIME
        }
        public enum NTV2Channel : int
        {
            NTV2_CHANNEL1,
            NTV2_CHANNEL2,
            NTV2_CHANNEL3,
            NTV2_CHANNEL4,
            NTV2_CHANNEL5,
            NTV2_CHANNEL6,
            NTV2_CHANNEL7,
            NTV2_CHANNEL8,
            NTV2_MAX_NUM_CHANNELS,          //	Always last!
            NTV2_CHANNEL_INVALID = NTV2_MAX_NUM_CHANNELS
        }
        public enum NTV2Crosspoint : int
        {
            NTV2CROSSPOINT_CHANNEL1,
            NTV2CROSSPOINT_CHANNEL2,
            NTV2CROSSPOINT_INPUT1,
            NTV2CROSSPOINT_INPUT2,
            NTV2CROSSPOINT_MATTE,       // @deprecated	This is obsolete
            NTV2CROSSPOINT_FGKEY,       // @deprecated	This is obsolete
            NTV2CROSSPOINT_CHANNEL3,
            NTV2CROSSPOINT_CHANNEL4,
            NTV2CROSSPOINT_INPUT3,
            NTV2CROSSPOINT_INPUT4,
            NTV2CROSSPOINT_CHANNEL5,
            NTV2CROSSPOINT_CHANNEL6,
            NTV2CROSSPOINT_CHANNEL7,
            NTV2CROSSPOINT_CHANNEL8,
            NTV2CROSSPOINT_INPUT5,
            NTV2CROSSPOINT_INPUT6,
            NTV2CROSSPOINT_INPUT7,
            NTV2CROSSPOINT_INPUT8,
            NTV2_NUM_CROSSPOINTS,
            NTV2CROSSPOINT_INVALID = NTV2_NUM_CROSSPOINTS
        }
        public enum NTV2DeviceID : int
        {
            DEVICE_ID_CORVID1 = 0x10244800,
            DEVICE_ID_CORVID22 = 0x10293000,
            DEVICE_ID_CORVID24 = 0x10402100,
            DEVICE_ID_CORVID3G = 0x10294900,
            DEVICE_ID_CORVID44 = 0x10565400,
            DEVICE_ID_CORVID88 = 0x10538200,
            DEVICE_ID_CORVIDHEVC = 0x10634500,
            DEVICE_ID_CORVIDHBR = 0x10668200,
            DEVICE_ID_IO4K = 0x10478300,
            DEVICE_ID_IO4KUFC = 0x10478350,
            DEVICE_ID_IOEXPRESS = 0x10280300,
            DEVICE_ID_IOXT = 0x10378800,
            DEVICE_ID_KONA3G = 0x10294700,
            DEVICE_ID_KONA3GQUAD = 0x10322950,
            DEVICE_ID_KONA4 = 0X10518400,
            DEVICE_ID_KONA4UFC = 0X10518450,
            DEVICE_ID_KONALHEPLUS = 0x10352300,
            DEVICE_ID_KONALHI = 0x10266400,
            DEVICE_ID_KONALHIDVI = 0x10266401,
            DEVICE_ID_TTAP = 0x10416000,
            DEVICE_ID_KONAIP_4CH_1SFP = 0x10646700,
            DEVICE_ID_KONAIP_4CH_2SFP = 0x10646701,
            DEVICE_ID_CORVIDHDBT = DEVICE_ID_CORVIDHBR,     //	Will deprecate in 12.6
            DEVICE_ID_LHE_PLUS = DEVICE_ID_KONALHEPLUS, //	Will deprecate eventually
            DEVICE_ID_LHI = DEVICE_ID_KONALHI,      //	Will deprecate eventually
            DEVICE_ID_LHI_DVI = DEVICE_ID_KONALHIDVI,       //	Will deprecate eventually
            DEVICE_ID_KONAIP22 = DEVICE_ID_KONAIP_4CH_1SFP,//	Will deprecate eventually
            DEVICE_ID_KONAIP4I = DEVICE_ID_KONAIP_4CH_2SFP,//	Will deprecate eventually
            DEVICE_ID_KONAIP_2IN_2OUT = DEVICE_ID_KONAIP_4CH_1SFP,//	Will deprecate eventually
            DEVICE_ID_KONAIP_4I = DEVICE_ID_KONAIP_4CH_2SFP,//	Will deprecate eventually
            DEVICE_ID_NOTFOUND = -1
        }
        public enum NTV2EveryFrameTaskMode : int
        {
            NTV2_DISABLE_TASKS,             //	0	Disabled		--	Board config completely up to controlling app
            NTV2_STANDARD_TASKS,            //	1	Standard/Retail	--	Board config set by AJA ControlPanel + service + driver
            NTV2_OEM_TASKS,                 //	2	OEM				--	Board config set by controlling app, minimal driver involvement
            NTV2_TASK_MODE_INVALID = 0xFF
        }
        public enum NTV2FrameBufferFormat : int
        {
            NTV2_FBF_FIRST = 0,
            NTV2_FBF_10BIT_YCBCR = 0,
            NTV2_FBF_8BIT_YCBCR = 1,
            NTV2_FBF_ARGB = 2,
            NTV2_FBF_RGBA = 3,
            NTV2_FBF_10BIT_RGB = 4,
            NTV2_FBF_8BIT_YCBCR_YUY2 = 5,
            NTV2_FBF_ABGR = 6,
            NTV2_FBF_LAST_SD_FBF = NTV2_FBF_ABGR,
            NTV2_FBF_10BIT_DPX = 7,
            NTV2_FBF_10BIT_YCBCR_DPX = 8,
            NTV2_FBF_8BIT_DVCPRO = 9,
            NTV2_FBF_8BIT_QREZ = 10,
            NTV2_FBF_8BIT_HDV = 11,
            NTV2_FBF_24BIT_RGB = 12,
            NTV2_FBF_24BIT_BGR = 13,
            NTV2_FBF_10BIT_YCBCRA = 14,
            NTV2_FBF_10BIT_DPX_LITTLEENDIAN = 15,
            NTV2_FBF_48BIT_RGB = 16,
            NTV2_FBF_PRORES = 17,
            NTV2_FBF_PRORES_DVCPRO = 18,
            NTV2_FBF_PRORES_HDV = 19,
            NTV2_FBF_10BIT_RGB_PACKED = 20,
            NTV2_FBF_10BIT_ARGB = 21,
            NTV2_FBF_16BIT_ARGB = 22,
            NTV2_FBF_UNUSED_23 = 23,
            NTV2_FBF_10BIT_RAW_RGB = 24,
            NTV2_FBF_10BIT_RAW_YCBCR = 25,
            NTV2_FBF_UNUSED_26 = 26,
            NTV2_FBF_UNUSED_27 = 27,
            NTV2_FBF_10BIT_YCBCR_420PL = 28,
            NTV2_FBF_10BIT_YCBCR_422PL = 29,
            NTV2_FBF_8BIT_YCBCR_420PL = 30,
            NTV2_FBF_8BIT_YCBCR_422PL = 31,
            NTV2_FBF_LAST = 31,

            ///note, if you add more you need to add another  bit in the channel control register.
            ///and an entry in ntv2utils.cpp in frameBufferFormats[].
            NTV2_FBF_NUMFRAMEBUFFERFORMATS,
            NTV2_FBF_INVALID = NTV2_FBF_NUMFRAMEBUFFERFORMATS
        }
        public enum NTV2FrameRate : int
        {
            //	These are tied to the hardware register values
            NTV2_FRAMERATE_UNKNOWN = 0,
            NTV2_FRAMERATE_6000 = 1,
            NTV2_FRAMERATE_5994 = 2,
            NTV2_FRAMERATE_3000 = 3,
            NTV2_FRAMERATE_2997 = 4,
            NTV2_FRAMERATE_2500 = 5,
            NTV2_FRAMERATE_2400 = 6,
            NTV2_FRAMERATE_2398 = 7,
            NTV2_FRAMERATE_5000 = 8,
            NTV2_FRAMERATE_4800 = 9,
            NTV2_FRAMERATE_4795 = 10,
            NTV2_FRAMERATE_12000 = 11,
            NTV2_FRAMERATE_11988 = 12,
            NTV2_FRAMERATE_1500 = 13,
            NTV2_FRAMERATE_1498 = 14,

            // These were never implemented, and are here so old code will still compile
            NTV2_FRAMERATE_1900 = 15,   // Used to be 09 in previous SDKs
            NTV2_FRAMERATE_1898 = 16,   // Used to be 10 in previous SDKs
            NTV2_FRAMERATE_1800 = 17,   // Used to be 11 in previous SDKs
            NTV2_FRAMERATE_1798 = 18,   // Used to be 12 in previous SDKs	

            NTV2_NUM_FRAMERATES,
            NTV2_FRAMERATE_INVALID = NTV2_FRAMERATE_UNKNOWN
        }
        public enum NTV2InputCrosspointID : uint
        {
            NTV2_FIRST_INPUT_CROSSPOINT = 0x01,
            NTV2_XptFrameBuffer1Input = 0x01,
            NTV2_XptFrameBuffer1BInput = 0x02,
            NTV2_XptFrameBuffer2Input = 0x03,
            NTV2_XptFrameBuffer2BInput = 0x04,
            NTV2_XptFrameBuffer3Input = 0x05,
            NTV2_XptFrameBuffer3BInput = 0x06,
            NTV2_XptFrameBuffer4Input = 0x07,
            NTV2_XptFrameBuffer4BInput = 0x08,
            NTV2_XptFrameBuffer5Input = 0x09,
            NTV2_XptFrameBuffer5BInput = 0x0A,
            NTV2_XptFrameBuffer6Input = 0x0B,
            NTV2_XptFrameBuffer6BInput = 0x0C,
            NTV2_XptFrameBuffer7Input = 0x0D,
            NTV2_XptFrameBuffer7BInput = 0x0E,
            NTV2_XptFrameBuffer8Input = 0x0F,
            NTV2_XptFrameBuffer8BInput = 0x10,
            NTV2_XptCSC1VidInput = 0x11,
            NTV2_XptCSC1KeyInput = 0x12,
            NTV2_XptCSC2VidInput = 0x13,
            NTV2_XptCSC2KeyInput = 0x14,
            NTV2_XptCSC3VidInput = 0x15,
            NTV2_XptCSC3KeyInput = 0x16,
            NTV2_XptCSC4VidInput = 0x17,
            NTV2_XptCSC4KeyInput = 0x18,
            NTV2_XptCSC5VidInput = 0x19,
            NTV2_XptCSC5KeyInput = 0x1A,
            NTV2_XptCSC6VidInput = 0x1B,
            NTV2_XptCSC6KeyInput = 0x1C,
            NTV2_XptCSC7VidInput = 0x1D,
            NTV2_XptCSC7KeyInput = 0x1E,
            NTV2_XptCSC8VidInput = 0x1F,
            NTV2_XptCSC8KeyInput = 0x20,
            NTV2_XptLUT1Input = 0x21,
            NTV2_XptLUT2Input = 0x22,
            NTV2_XptLUT3Input = 0x23,
            NTV2_XptLUT4Input = 0x24,
            NTV2_XptLUT5Input = 0x25,
            NTV2_XptLUT6Input = 0x26,
            NTV2_XptLUT7Input = 0x27,
            NTV2_XptLUT8Input = 0x28,
            NTV2_XptSDIOut1Standard = 0x29, //	deprecate?
            NTV2_XptSDIOut2Standard = 0x2A, //	deprecate?
            NTV2_XptSDIOut3Standard = 0x2B, //	deprecate?
            NTV2_XptSDIOut4Standard = 0x2C, //	deprecate?
            NTV2_XptSDIOut1Input = 0x2D,
            NTV2_XptSDIOut1InputDS2 = 0x2E,
            NTV2_XptSDIOut2Input = 0x2F,
            NTV2_XptSDIOut2InputDS2 = 0x30,
            NTV2_XptSDIOut3Input = 0x31,
            NTV2_XptSDIOut3InputDS2 = 0x32,
            NTV2_XptSDIOut4Input = 0x33,
            NTV2_XptSDIOut4InputDS2 = 0x34,
            NTV2_XptSDIOut5Input = 0x35,
            NTV2_XptSDIOut5InputDS2 = 0x36,
            NTV2_XptSDIOut6Input = 0x37,
            NTV2_XptSDIOut6InputDS2 = 0x38,
            NTV2_XptSDIOut7Input = 0x39,
            NTV2_XptSDIOut7InputDS2 = 0x3A,
            NTV2_XptSDIOut8Input = 0x3B,
            NTV2_XptSDIOut8InputDS2 = 0x3C,
            NTV2_XptDualLinkIn1Input = 0x3D,
            NTV2_XptDualLinkIn1DSInput = 0x3E,
            NTV2_XptDualLinkIn2Input = 0x3F,
            NTV2_XptDualLinkIn2DSInput = 0x40,
            NTV2_XptDualLinkIn3Input = 0x41,
            NTV2_XptDualLinkIn3DSInput = 0x42,
            NTV2_XptDualLinkIn4Input = 0x43,
            NTV2_XptDualLinkIn4DSInput = 0x44,
            NTV2_XptDualLinkIn5Input = 0x45,
            NTV2_XptDualLinkIn5DSInput = 0x46,
            NTV2_XptDualLinkIn6Input = 0x47,
            NTV2_XptDualLinkIn6DSInput = 0x48,
            NTV2_XptDualLinkIn7Input = 0x49,
            NTV2_XptDualLinkIn7DSInput = 0x4A,
            NTV2_XptDualLinkIn8Input = 0x4B,
            NTV2_XptDualLinkIn8DSInput = 0x4C,
            NTV2_XptDualLinkOut1Input = 0x4D,
            NTV2_XptDualLinkOut2Input = 0x4E,
            NTV2_XptDualLinkOut3Input = 0x4F,
            NTV2_XptDualLinkOut4Input = 0x50,
            NTV2_XptDualLinkOut5Input = 0x51,
            NTV2_XptDualLinkOut6Input = 0x52,
            NTV2_XptDualLinkOut7Input = 0x53,
            NTV2_XptDualLinkOut8Input = 0x54,
            NTV2_XptMixer1BGKeyInput = 0x55,
            NTV2_XptMixer1BGVidInput = 0x56,
            NTV2_XptMixer1FGKeyInput = 0x57,
            NTV2_XptMixer1FGVidInput = 0x58,
            NTV2_XptMixer2BGKeyInput = 0x59,
            NTV2_XptMixer2BGVidInput = 0x5A,
            NTV2_XptMixer2FGKeyInput = 0x5B,
            NTV2_XptMixer2FGVidInput = 0x5C,
            NTV2_XptMixer3BGKeyInput = 0x5D,
            NTV2_XptMixer3BGVidInput = 0x5E,
            NTV2_XptMixer3FGKeyInput = 0x5F,
            NTV2_XptMixer3FGVidInput = 0x60,
            NTV2_XptMixer4BGKeyInput = 0x61,
            NTV2_XptMixer4BGVidInput = 0x62,
            NTV2_XptMixer4FGKeyInput = 0x63,
            NTV2_XptMixer4FGVidInput = 0x64,
            NTV2_XptHDMIOutInput = 0x65,
            NTV2_XptHDMIOutQ1Input = 0x66,
            NTV2_XptHDMIOutQ2Input = 0x67,
            NTV2_XptHDMIOutQ3Input = 0x68,
            NTV2_XptHDMIOutQ4Input = 0x69,
            NTV2_Xpt4KDCQ1Input = 0x6A,
            NTV2_Xpt4KDCQ2Input = 0x6B,
            NTV2_Xpt4KDCQ3Input = 0x6C,
            NTV2_Xpt4KDCQ4Input = 0x6D,
            NTV2_Xpt425Mux1AInput = 0x6E,
            NTV2_Xpt425Mux1BInput = 0x6F,
            NTV2_Xpt425Mux2AInput = 0x70,
            NTV2_Xpt425Mux2BInput = 0x71,
            NTV2_Xpt425Mux3AInput = 0x72,
            NTV2_Xpt425Mux3BInput = 0x73,
            NTV2_Xpt425Mux4AInput = 0x74,
            NTV2_Xpt425Mux4BInput = 0x75,
            NTV2_XptAnalogOutInput = 0x76,
            NTV2_XptIICT2Input = 0x77,  //	deprecate?
            NTV2_XptAnalogOutCompositeOut = 0x78,   //	deprecate?
            NTV2_XptStereoLeftInput = 0x79, //	deprecate?
            NTV2_XptStereoRightInput = 0x7A,    //	deprecate?
            NTV2_XptProAmpInput = 0x7B, //	deprecate?
            NTV2_XptIICT1Input = 0x7C,  //	deprecate?
            NTV2_XptWaterMarker1Input = 0x7D,   //	deprecate?
            NTV2_XptWaterMarker2Input = 0x7E,   //	deprecate?
            NTV2_XptUpdateRegister = 0x7F,  //	deprecate?
            NTV2_XptConversionMod2Input = 0x80, //	deprecate?
            NTV2_XptCompressionModInput = 0x81, //	deprecate?
            NTV2_XptConversionModInput = 0x82,  //	deprecate?
            NTV2_XptCSC1KeyFromInput2 = 0x83,   //	deprecate?
            NTV2_XptFrameSync2Input = 0x84, //	deprecate?
            NTV2_XptFrameSync1Input = 0x85, //	deprecate?
            NTV2_LAST_INPUT_CROSSPOINT = 0x85,
            NTV2_INPUT_CROSSPOINT_INVALID = 0xFFFFFFFF
        }
        public enum NTV2OutputDestination : int
        {
            NTV2_OUTPUTDESTINATION_ANALOG,
            NTV2_OUTPUTDESTINATION_HDMI,
            NTV2_OUTPUTDESTINATION_SDI1,
            NTV2_OUTPUTDESTINATION_SDI2,
            NTV2_OUTPUTDESTINATION_SDI3,
            NTV2_OUTPUTDESTINATION_SDI4,
            NTV2_OUTPUTDESTINATION_SDI5,
            NTV2_OUTPUTDESTINATION_SDI6,
            NTV2_OUTPUTDESTINATION_SDI7,
            NTV2_OUTPUTDESTINATION_SDI8,

            NTV2_OUTPUTDESTINATION_INVALID,
            NTV2_NUM_OUTPUTDESTINATIONS = NTV2_OUTPUTDESTINATION_INVALID    //	Always last!
        }
        public enum NTV2OutputCrosspointID : int
        {
            NTV2_XptBlack = 0x0,
            NTV2_XptSDIIn1 = 0x1,
            NTV2_XptSDIIn1DS2 = 0x1E,
            NTV2_XptSDIIn2 = 0x2,
            NTV2_XptSDIIn2DS2 = 0x1F,
            NTV2_XptLUT1YUV = 0x4,
            NTV2_XptCSC1VidYUV = 0x5,
            NTV2_XptConversionModule = 0x6,
            NTV2_XptCompressionModule = 0x7,
            NTV2_XptFrameBuffer1YUV = 0x8,
            NTV2_XptFrameSync1YUV = 0x9,
            NTV2_XptFrameSync2YUV = 0xA,
            NTV2_XptDuallinkOut1 = 0xB,
            NTV2_XptDuallinkOut1DS2 = 0x26,
            NTV2_XptDuallinkOut2 = 0x1C,
            NTV2_XptDuallinkOut2DS2 = 0x27,
            NTV2_XptDuallinkOut3 = 0x36,
            NTV2_XptDuallinkOut3DS2 = 0x37,
            NTV2_XptDuallinkOut4 = 0x38,
            NTV2_XptDuallinkOut4DS2 = 0x39,
            NTV2_XptAlphaOut = 0xC,
            NTV2_XptAnalogIn = 0x16,
            NTV2_XptHDMIIn = 0x17,
            NTV2_XptHDMIInQ2 = 0x41,
            NTV2_XptHDMIInQ3 = 0x42,
            NTV2_XptHDMIInQ4 = 0x43,
            NTV2_XptHDMIInRGB = 0x97,
            NTV2_XptHDMIInQ2RGB = 0xC1,
            NTV2_XptHDMIInQ3RGB = 0xC2,
            NTV2_XptHDMIInQ4RGB = 0xC3,
            NTV2_XptDuallinkIn1 = 0x83,
            NTV2_XptDuallinkIn2 = 0xA8,
            NTV2_XptDuallinkIn3 = 0xB4,
            NTV2_XptDuallinkIn4 = 0xB5,
            NTV2_XptLUT1RGB = 0x84,
            NTV2_XptCSC1VidRGB = 0x85,
            NTV2_XptFrameBuffer1RGB = 0x88,
            NTV2_XptFrameSync1RGB = 0x89,
            NTV2_XptFrameSync2RGB = 0x8A,
            NTV2_XptLUT2RGB = 0x8D,
            NTV2_XptCSC1KeyYUV = 0xE,
            NTV2_XptFrameBuffer2YUV = 0xF,
            NTV2_XptFrameBuffer2RGB = 0x8F,
            NTV2_XptCSC2VidYUV = 0x10,
            NTV2_XptCSC2VidRGB = 0x90,
            NTV2_XptCSC2KeyYUV = 0x11,
            NTV2_XptMixer1VidYUV = 0x12,
            NTV2_XptMixer1KeyYUV = 0x13,
            NTV2_XptWaterMarkerRGB = 0x94,
            NTV2_XptWaterMarkerYUV = 0x14,
            NTV2_XptWaterMarker2RGB = 0x9A,
            NTV2_XptWaterMarker2YUV = 0x1A,
            NTV2_XptIICTRGB = 0x95,
            NTV2_XptIICT2RGB = 0x9B,
            NTV2_XptTestPatternYUV = 0x1D,
            NTV2_XptDCIMixerVidYUV = 0x22,
            NTV2_XptDCIMixerVidRGB = 0xA2,
            NTV2_XptMixer2VidYUV = 0x20,
            NTV2_XptMixer2KeyYUV = 0x21,
            NTV2_XptStereoCompressorOut = 0x23,
            NTV2_XptLUT3Out = 0xA9,
            NTV2_XptLUT4Out = 0xAA,
            NTV2_XptFrameBuffer3YUV = 0x24,
            NTV2_XptFrameBuffer3RGB = 0xA4,
            NTV2_XptFrameBuffer4YUV = 0x25,
            NTV2_XptFrameBuffer4RGB = 0xA5,
            NTV2_XptSDIIn3 = 0x30,
            NTV2_XptSDIIn3DS2 = 0x32,
            NTV2_XptSDIIn4 = 0x31,
            NTV2_XptSDIIn4DS2 = 0x33,
            NTV2_XptCSC3VidYUV = 0x3A,
            NTV2_XptCSC3VidRGB = 0xBA,
            NTV2_XptCSC3KeyYUV = 0x3B,
            NTV2_XptCSC4VidYUV = 0x3C,
            NTV2_XptCSC4VidRGB = 0xBC,
            NTV2_XptCSC4KeyYUV = 0x3D,
            NTV2_XptCSC5VidYUV = 0x2C,
            NTV2_XptCSC5VidRGB = 0xAC,
            NTV2_XptCSC5KeyYUV = 0x2D,
            NTV2_XptLUT5Out = 0xAB,
            NTV2_XptDuallinkOut5 = 0x3E,
            NTV2_XptDuallinkOut5DS2 = 0x3F,
            NTV2_Xpt4KDownConverterOut = 0x44,
            NTV2_Xpt4KDownConverterOutRGB = 0xC4,
            NTV2_XptFrameBuffer5YUV = 0x51,
            NTV2_XptFrameBuffer5RGB = 0xD1,
            NTV2_XptFrameBuffer6YUV = 0x52,
            NTV2_XptFrameBuffer6RGB = 0xD2,
            NTV2_XptFrameBuffer7YUV = 0x53,
            NTV2_XptFrameBuffer7RGB = 0xD3,
            NTV2_XptFrameBuffer8YUV = 0x54,
            NTV2_XptFrameBuffer8RGB = 0xD4,
            NTV2_XptSDIIn5 = 0x45,
            NTV2_XptSDIIn5DS2 = 0x47,
            NTV2_XptSDIIn6 = 0x46,
            NTV2_XptSDIIn6DS2 = 0x48,
            NTV2_XptSDIIn7 = 0x49,
            NTV2_XptSDIIn7DS2 = 0x4B,
            NTV2_XptSDIIn8 = 0x4A,
            NTV2_XptSDIIn8DS2 = 0x4C,
            NTV2_XptCSC6VidYUV = 0x59,
            NTV2_XptCSC6VidRGB = 0xD9,
            NTV2_XptCSC6KeyYUV = 0x5A,
            NTV2_XptCSC7VidYUV = 0x5B,
            NTV2_XptCSC7VidRGB = 0xDB,
            NTV2_XptCSC7KeyYUV = 0x5C,
            NTV2_XptCSC8VidYUV = 0x5D,
            NTV2_XptCSC8VidRGB = 0xDD,
            NTV2_XptCSC8KeyYUV = 0x5E,
            NTV2_XptLUT6Out = 0xDF,
            NTV2_XptLUT7Out = 0xE0,
            NTV2_XptLUT8Out = 0xE1,
            NTV2_XptDuallinkOut6 = 0x62,
            NTV2_XptDuallinkOut6DS2 = 0x63,
            NTV2_XptDuallinkOut7 = 0x64,
            NTV2_XptDuallinkOut7DS2 = 0x65,
            NTV2_XptDuallinkOut8 = 0x66,
            NTV2_XptDuallinkOut8DS2 = 0x67,
            NTV2_XptMixer3VidYUV = 0x55,
            NTV2_XptMixer3KeyYUV = 0x56,
            NTV2_XptMixer4VidYUV = 0x57,
            NTV2_XptMixer4KeyYUV = 0x58,
            NTV2_XptDuallinkIn5 = 0xCD,
            NTV2_XptDuallinkIn6 = 0xCE,
            NTV2_XptDuallinkIn7 = 0xCF,
            NTV2_XptDuallinkIn8 = 0xD0,
            NTV2_Xpt425Mux1AYUV = 0x68,
            NTV2_Xpt425Mux1ARGB = 0xE8,
            NTV2_Xpt425Mux1BYUV = 0x69,
            NTV2_Xpt425Mux1BRGB = 0xE9,
            NTV2_Xpt425Mux2AYUV = 0x6A,
            NTV2_Xpt425Mux2ARGB = 0xEA,
            NTV2_Xpt425Mux2BYUV = 0x6B,
            NTV2_Xpt425Mux2BRGB = 0xEB,
            NTV2_Xpt425Mux3AYUV = 0x6C,
            NTV2_Xpt425Mux3ARGB = 0xEC,
            NTV2_Xpt425Mux3BYUV = 0x6D,
            NTV2_Xpt425Mux3BRGB = 0xED,
            NTV2_Xpt425Mux4AYUV = 0x6E,
            NTV2_Xpt425Mux4ARGB = 0xEE,
            NTV2_Xpt425Mux4BYUV = 0x6F,
            NTV2_Xpt425Mux4BRGB = 0xEF,
            NTV2_XptFrameBuffer1_425YUV = 0x70,
            NTV2_XptFrameBuffer1_425RGB = 0xF0,
            NTV2_XptFrameBuffer2_425YUV = 0x71,
            NTV2_XptFrameBuffer2_425RGB = 0xF1,
            NTV2_XptFrameBuffer3_425YUV = 0x72,
            NTV2_XptFrameBuffer3_425RGB = 0xF2,
            NTV2_XptFrameBuffer4_425YUV = 0x73,
            NTV2_XptFrameBuffer4_425RGB = 0xF3,
            NTV2_XptFrameBuffer5_425YUV = 0x74,
            NTV2_XptFrameBuffer5_425RGB = 0xF4,
            NTV2_XptFrameBuffer6_425YUV = 0x75,
            NTV2_XptFrameBuffer6_425RGB = 0xF5,
            NTV2_XptFrameBuffer7_425YUV = 0x76,
            NTV2_XptFrameBuffer7_425RGB = 0xF6,
            NTV2_XptFrameBuffer8_425YUV = 0x77,
            NTV2_XptFrameBuffer8_425RGB = 0xF7,
            NTV2_XptRuntimeCalc = 0xFF,
            NTV2_LAST_OUTPUT_CROSSPOINT = 0xFF,
            NTV2_OUTPUT_CROSSPOINT_INVALID = 0xFF
        }
        public enum NTV2ReferenceSource
        {
            NTV2_REFERENCE_EXTERNAL,
            NTV2_REFERENCE_INPUT1,
            NTV2_REFERENCE_INPUT2,
            NTV2_REFERENCE_FREERUN,
            NTV2_REFERENCE_ANALOG_INPUT,
            NTV2_REFERENCE_HDMI_INPUT,
            NTV2_REFERENCE_INPUT3,
            NTV2_REFERENCE_INPUT4,
            NTV2_REFERENCE_INPUT5,
            NTV2_REFERENCE_INPUT6,
            NTV2_REFERENCE_INPUT7,
            NTV2_REFERENCE_INPUT8,
            NTV2_NUM_REFERENCE_INPUTS,          //	Always last!
            NTV2_REFERENCE_INVALID = NTV2_NUM_REFERENCE_INPUTS
        }
        public enum NTV2Standard : int
        {
            NTV2_STANDARD_1080,         // i/psf			SMPTE
            NTV2_STANDARD_720,          //					SMPTE
            NTV2_STANDARD_525,          // interlaced		SMPTE
            NTV2_STANDARD_625,          // interlaced		SMPTE
            NTV2_STANDARD_1080p,        //					SMPTE
            NTV2_STANDARD_2K,           // 2048x1556psf		SMPTE
            NTV2_STANDARD_2Kx1080p,     //					SMPTE
            NTV2_STANDARD_2Kx1080i,     // psf only			SMPTE
            NTV2_STANDARD_3840x2160p,
            NTV2_STANDARD_4096x2160p,
            NTV2_STANDARD_3840HFR,
            NTV2_STANDARD_4096HFR,
            NTV2_NUM_STANDARDS,
            NTV2_STANDARD_UNDEFINED = NTV2_NUM_STANDARDS,
            NTV2_STANDARD_INVALID = NTV2_NUM_STANDARDS
        }
        public enum NTV2TCIndex : int
        {
            NTV2_TCINDEX_DEFAULT,       // @brief	The "default" timecode
            NTV2_TCINDEX_SDI1,          // @brief	SDI 1 embedded VITC
            NTV2_TCINDEX_SDI2,          // @brief	SDI 2 embedded VITC
            NTV2_TCINDEX_SDI3,          // @brief	SDI 3 embedded VITC
            NTV2_TCINDEX_SDI4,          // @brief	SDI 4 embedded VITC
            NTV2_TCINDEX_SDI1_LTC,      // @brief	SDI 1 embedded ATC LTC
            NTV2_TCINDEX_SDI2_LTC,      // @brief	SDI 2 embedded ATC LTC
            NTV2_TCINDEX_LTC1,          // @brief	Analog LTC 1
            NTV2_TCINDEX_LTC2,          // @brief	Analog LTC 2
            NTV2_TCINDEX_SDI5,          // @brief	SDI 5 embedded VITC
            NTV2_TCINDEX_SDI6,          // @brief	SDI 6 embedded VITC
            NTV2_TCINDEX_SDI7,          // @brief	SDI 7 embedded VITC
            NTV2_TCINDEX_SDI8,          // @brief	SDI 8 embedded VITC
            NTV2_TCINDEX_SDI3_LTC,      // @brief	SDI 3 embedded ATC LTC
            NTV2_TCINDEX_SDI4_LTC,      // @brief	SDI 4 embedded ATC LTC
            NTV2_TCINDEX_SDI5_LTC,      // @brief	SDI 5 embedded ATC LTC
            NTV2_TCINDEX_SDI6_LTC,      // @brief	SDI 6 embedded ATC LTC
            NTV2_TCINDEX_SDI7_LTC,      // @brief	SDI 7 embedded ATC LTC
            NTV2_TCINDEX_SDI8_LTC,      // @brief	SDI 8 embedded ATC LTC
            NTV2_TCINDEX_SDI1_2,        // @brief	SDI 1 embedded VITC 2
            NTV2_TCINDEX_SDI2_2,        // @brief	SDI 2 embedded VITC 2
            NTV2_TCINDEX_SDI3_2,        // @brief	SDI 3 embedded VITC 2
            NTV2_TCINDEX_SDI4_2,        // @brief	SDI 4 embedded VITC 2
            NTV2_TCINDEX_SDI5_2,        // @brief	SDI 5 embedded VITC 2
            NTV2_TCINDEX_SDI6_2,        // @brief	SDI 6 embedded VITC 2
            NTV2_TCINDEX_SDI7_2,        // @brief	SDI 7 embedded VITC 2
            NTV2_TCINDEX_SDI8_2,        // @brief	SDI 8 embedded VITC 2
            NTV2_MAX_NUM_TIMECODE_INDEXES,
            NTV2_TCINDEX_INVALID = NTV2_MAX_NUM_TIMECODE_INDEXES
        }
        public enum NTV2VANCDataShiftMode : int
        {
            NTV2_VANCDATA_NORMAL,
            NTV2_VANCDATA_8BITSHIFT_ENABLE,
            NTV2_MAX_NUM_VANCDataShiftModes
        }
        public enum NTV2VideoFormat : int
        {
            NTV2_FORMAT_UNKNOWN,                                            // 0
            NTV2_FORMAT_FIRST_HIGH_DEF_FORMAT = 1,                              // 1
            NTV2_FORMAT_1080i_5000 = 1,     // 1
            NTV2_FORMAT_1080psf_2500 = 1,              // 1
            NTV2_FORMAT_1080i_5994,                                         // 2
            NTV2_FORMAT_1080psf_2997 = NTV2_FORMAT_1080i_5994,              // 2
            NTV2_FORMAT_1080i_6000,                                         // 3
            NTV2_FORMAT_1080psf_3000 = NTV2_FORMAT_1080i_6000,              // 3
            NTV2_FORMAT_720p_5994,          // 4
            NTV2_FORMAT_720p_6000,          // 5
            NTV2_FORMAT_1080psf_2398,       // 6
            NTV2_FORMAT_1080psf_2400,       // 7
            NTV2_FORMAT_1080p_2997,         // 8
            NTV2_FORMAT_1080p_3000,         // 9
            NTV2_FORMAT_1080p_2500,         // 10
            NTV2_FORMAT_1080p_2398,         // 11
            NTV2_FORMAT_1080p_2400,         // 12
            NTV2_FORMAT_1080p_2K_2398,      // 13
            NTV2_FORMAT_DEPRECATED_525_5994 = NTV2_FORMAT_1080p_2K_2398, // 13 - Backward compatibility for Linux .ntv2 files, do not use
            NTV2_FORMAT_1080p_2K_2400,      // 14
            NTV2_FORMAT_DEPRECATED_625_5000 = NTV2_FORMAT_1080p_2K_2400, // 14 - Backward compatibility for Linux .ntv2 files, do not use
            NTV2_FORMAT_1080psf_2K_2398,    // 15
            NTV2_FORMAT_1080psf_2K_2400,    // 16
            NTV2_FORMAT_720p_5000,          // 17
            NTV2_FORMAT_1080p_5000,         // 18
            NTV2_FORMAT_1080p_5000_B = NTV2_FORMAT_1080p_5000, // 18
            NTV2_FORMAT_1080p_5994,         // 19
            NTV2_FORMAT_1080p_5994_B = NTV2_FORMAT_1080p_5994, // 19
            NTV2_FORMAT_1080p_6000,         // 20
            NTV2_FORMAT_1080p_6000_B = NTV2_FORMAT_1080p_6000, // 20
            NTV2_FORMAT_720p_2398,          // 21
            NTV2_FORMAT_720p_2500,          // 22
            NTV2_FORMAT_1080p_5000_A,       // 23
            NTV2_FORMAT_1080p_5994_A,       // 24
            NTV2_FORMAT_1080p_6000_A,       // 25
            NTV2_FORMAT_1080p_2K_2500,      // 26
            NTV2_FORMAT_1080psf_2K_2500,    // 27
            NTV2_FORMAT_1080psf_2500_2,     // 28 - psf only (non-interlaced), deprecates NTV2_FORMAT_1080psf_2500
            NTV2_FORMAT_1080psf_2997_2,     // 29 - psf only (non-interlaced), deprecates NTV2_FORMAT_1080psf_2997
            NTV2_FORMAT_1080psf_3000_2,     // 30 - psf only (non-interlaced), deprecates NTV2_FORMAT_1080psf_3000
                                            // Add new HD formats here
            NTV2_FORMAT_END_HIGH_DEF_FORMATS,// 31

            NTV2_FORMAT_FIRST_STANDARD_DEF_FORMAT = 32,
            NTV2_FORMAT_525_5994 = NTV2_FORMAT_FIRST_STANDARD_DEF_FORMAT, // 32
            NTV2_FORMAT_625_5000,           // 33
            NTV2_FORMAT_525_2398,           // 34
            NTV2_FORMAT_525_2400,           // 35
            NTV2_FORMAT_525psf_2997,        // 36
            NTV2_FORMAT_625psf_2500,        // 37
                                            // Add new SD formats here
            NTV2_FORMAT_END_STANDARD_DEF_FORMATS, // 38

            // 2K Starts Here.
            NTV2_FORMAT_FIRST_2K_DEF_FORMAT = 64,
            NTV2_FORMAT_2K_1498 = NTV2_FORMAT_FIRST_2K_DEF_FORMAT,  // 64
            NTV2_FORMAT_2K_1500,                // 65
            NTV2_FORMAT_2K_2398,                // 66
            NTV2_FORMAT_2K_2400,                // 67
            NTV2_FORMAT_2K_2500,                // 68
                                                // Add new 2K formats here
            NTV2_FORMAT_END_2K_DEF_FORMATS, // 69

            // 4K Starts Here.
            NTV2_FORMAT_FIRST_4K_DEF_FORMAT = 80,
            NTV2_FORMAT_4x1920x1080psf_2398 = NTV2_FORMAT_FIRST_4K_DEF_FORMAT, // 80
            NTV2_FORMAT_4x1920x1080psf_2400,    // 81
            NTV2_FORMAT_4x1920x1080psf_2500,    // 82
            NTV2_FORMAT_4x1920x1080p_2398,      // 83
            NTV2_FORMAT_4x1920x1080p_2400,      // 84
            NTV2_FORMAT_4x1920x1080p_2500,      // 85
            NTV2_FORMAT_4x2048x1080psf_2398,    // 86
            NTV2_FORMAT_4x2048x1080psf_2400,    // 87
            NTV2_FORMAT_4x2048x1080psf_2500,    // 88
            NTV2_FORMAT_4x2048x1080p_2398,      // 89
            NTV2_FORMAT_4x2048x1080p_2400,      // 90
            NTV2_FORMAT_4x2048x1080p_2500,      // 91
            NTV2_FORMAT_4x1920x1080p_2997,      // 92
            NTV2_FORMAT_4x1920x1080p_3000,      // 93
            NTV2_FORMAT_4x1920x1080psf_2997,    // 94 NOT SUPPORTED
            NTV2_FORMAT_4x1920x1080psf_3000,    // 95 NOT SUPPORTED
            NTV2_FORMAT_4x2048x1080p_2997,      // 96
            NTV2_FORMAT_4x2048x1080p_3000,      // 97
            NTV2_FORMAT_4x2048x1080psf_2997,    // 98 NOT SUPPORTED
            NTV2_FORMAT_4x2048x1080psf_3000,    // 99 NOT SUPPORTED
            NTV2_FORMAT_4x1920x1080p_5000,      // 100
            NTV2_FORMAT_4x1920x1080p_5994,      // 101
            NTV2_FORMAT_4x1920x1080p_6000,      // 102
            NTV2_FORMAT_4x2048x1080p_5000,      // 103
            NTV2_FORMAT_4x2048x1080p_5994,      // 104
            NTV2_FORMAT_4x2048x1080p_6000,      // 105
            NTV2_FORMAT_4x2048x1080p_4795,      // 106
            NTV2_FORMAT_4x2048x1080p_4800,      // 107
            NTV2_FORMAT_4x2048x1080p_11988,     // 108
            NTV2_FORMAT_4x2048x1080p_12000,     // 109
                                                // Add new 4K formats here
            NTV2_FORMAT_END_4K_DEF_FORMATS,     // 110

            NTV2_FORMAT_FIRST_HIGH_DEF_FORMAT2 = 110,
            NTV2_FORMAT_1080p_2K_6000 = NTV2_FORMAT_FIRST_HIGH_DEF_FORMAT2, // 110
            NTV2_FORMAT_1080p_2K_6000_A = NTV2_FORMAT_1080p_2K_6000,    // 110
            NTV2_FORMAT_1080p_2K_5994,          // 111
            NTV2_FORMAT_1080p_2K_5994_A = NTV2_FORMAT_1080p_2K_5994,    // 111
            NTV2_FORMAT_1080p_2K_2997,          // 112
            NTV2_FORMAT_1080p_2K_3000,          // 113
            NTV2_FORMAT_1080p_2K_5000,          // 114
            NTV2_FORMAT_1080p_2K_5000_A = NTV2_FORMAT_1080p_2K_5000,    // 114
            NTV2_FORMAT_1080p_2K_4795,          // 115
            NTV2_FORMAT_1080p_2K_4795_A = NTV2_FORMAT_1080p_2K_4795,    // 115
            NTV2_FORMAT_1080p_2K_4800,          // 116
            NTV2_FORMAT_1080p_2K_4800_A = NTV2_FORMAT_1080p_2K_4800,    // 116
            NTV2_FORMAT_1080p_2K_4795_B,        // 117
            NTV2_FORMAT_1080p_2K_4800_B,        // 118
            NTV2_FORMAT_1080p_2K_5000_B,        // 119
            NTV2_FORMAT_1080p_2K_5994_B,        // 120
            NTV2_FORMAT_1080p_2K_6000_B,        // 121
            NTV2_FORMAT_END_HIGH_DEF_FORMATS2,  // 122
            NTV2_MAX_NUM_VIDEO_FORMATS = NTV2_FORMAT_END_HIGH_DEF_FORMATS2
        }
        public enum NTV2WidgetID : int
        {
            NTV2_WgtFrameBuffer1,
            NTV2_WgtFrameBuffer2,
            NTV2_WgtFrameBuffer3,
            NTV2_WgtFrameBuffer4,
            NTV2_WgtCSC1,
            NTV2_WgtCSC2,
            NTV2_WgtLUT1,
            NTV2_WgtLUT2,
            NTV2_WgtFrameSync1,
            NTV2_WgtFrameSync2,
            NTV2_WgtSDIIn1,
            NTV2_WgtSDIIn2,
            NTV2_Wgt3GSDIIn1,
            NTV2_Wgt3GSDIIn2,
            NTV2_Wgt3GSDIIn3,
            NTV2_Wgt3GSDIIn4,
            NTV2_WgtSDIOut1,
            NTV2_WgtSDIOut2,
            NTV2_WgtSDIOut3,
            NTV2_WgtSDIOut4,
            NTV2_Wgt3GSDIOut1,
            NTV2_Wgt3GSDIOut2,
            NTV2_Wgt3GSDIOut3,
            NTV2_Wgt3GSDIOut4,
            NTV2_WgtDualLinkIn1,
            NTV2_WgtDualLinkV2In1,
            NTV2_WgtDualLinkV2In2,
            NTV2_WgtDualLinkOut1,
            NTV2_WgtDualLinkOut2,
            NTV2_WgtDualLinkV2Out1,
            NTV2_WgtDualLinkV2Out2,
            NTV2_WgtAnalogIn1,
            NTV2_WgtAnalogOut1,
            NTV2_WgtAnalogCompositeOut1,
            NTV2_WgtHDMIIn1,
            NTV2_WgtHDMIOut1,
            NTV2_WgtUpDownConverter1,
            NTV2_WgtUpDownConverter2,
            NTV2_WgtMixer1,
            NTV2_WgtCompression1,
            NTV2_WgtProcAmp1,
            NTV2_WgtWaterMarker1,
            NTV2_WgtWaterMarker2,
            NTV2_WgtIICT1,
            NTV2_WgtIICT2,
            NTV2_WgtTestPattern1,
            NTV2_WgtGenLock,
            NTV2_WgtDCIMixer1,
            NTV2_WgtMixer2,
            NTV2_WgtStereoCompressor,
            NTV2_WgtLUT3,
            NTV2_WgtLUT4,
            NTV2_WgtDualLinkV2In3,
            NTV2_WgtDualLinkV2In4,
            NTV2_WgtDualLinkV2Out3,
            NTV2_WgtDualLinkV2Out4,
            NTV2_WgtCSC3,
            NTV2_WgtCSC4,
            NTV2_WgtHDMIIn1v2,
            NTV2_WgtHDMIOut1v2,
            NTV2_WgtSDIMonOut1,
            NTV2_WgtCSC5,
            NTV2_WgtLUT5,
            NTV2_WgtDualLinkV2Out5,
            NTV2_Wgt4KDownConverter,
            NTV2_Wgt3GSDIIn5,
            NTV2_Wgt3GSDIIn6,
            NTV2_Wgt3GSDIIn7,
            NTV2_Wgt3GSDIIn8,
            NTV2_Wgt3GSDIOut5,
            NTV2_Wgt3GSDIOut6,
            NTV2_Wgt3GSDIOut7,
            NTV2_Wgt3GSDIOut8,
            NTV2_WgtDualLinkV2In5,
            NTV2_WgtDualLinkV2In6,
            NTV2_WgtDualLinkV2In7,
            NTV2_WgtDualLinkV2In8,
            NTV2_WgtDualLinkV2Out6,
            NTV2_WgtDualLinkV2Out7,
            NTV2_WgtDualLinkV2Out8,
            NTV2_WgtCSC6,
            NTV2_WgtCSC7,
            NTV2_WgtCSC8,
            NTV2_WgtLUT6,
            NTV2_WgtLUT7,
            NTV2_WgtLUT8,
            NTV2_WgtMixer3,
            NTV2_WgtMixer4,
            NTV2_WgtFrameBuffer5,
            NTV2_WgtFrameBuffer6,
            NTV2_WgtFrameBuffer7,
            NTV2_WgtFrameBuffer8,
            NTV2_WgtHDMIIn1v3,
            NTV2_WgtHDMIOut1v3,
            NTV2_Wgt425Mux1,
            NTV2_Wgt425Mux2,
            NTV2_Wgt425Mux3,
            NTV2_Wgt425Mux4,
            NTV2_WgtModuleTypeCount,// always last
            NTV2_WgtUndefined = NTV2_WgtModuleTypeCount,
            NTV2_WIDGET_INVALID = NTV2_WgtModuleTypeCount
        }
        static private Dictionary<string, Delegate> _ahAjaFunctionsUsed;
        static private IntPtr _pAjaWrapperDllHandler;
        static AjaInterop()
        {
            _pAjaWrapperDllHandler = Win32Interops.LoadLibrary("AjaWrapper_64d.dll"); // can be a full path
            _ahAjaFunctionsUsed = new Dictionary<string, Delegate>();
        }   //AjaWrapper_64d.dll
        static internal T AjaFunction<T>()
        {
            string sAjaFunctionName = null;
            try
            {
                object[] aAttrs = typeof(T).GetCustomAttributes(typeof(AjaFunctionAttribute), false);
                if (aAttrs.Length == 0)
                    throw new Exception("Could not find the AjaFunctionAttribute.");
                AjaFunctionAttribute cAjaAttr = (AjaFunctionAttribute)aAttrs[0];
                sAjaFunctionName = cAjaAttr.sFunctionName;
                if (!_ahAjaFunctionsUsed.ContainsKey(sAjaFunctionName))
                {
                    IntPtr pFuncAddress = Win32Interops.GetProcAddress(_pAjaWrapperDllHandler, cAjaAttr.sFunctionName);
                    if (pFuncAddress == IntPtr.Zero)
                        throw new System.ComponentModel.Win32Exception();
                    Delegate dFunctionPointer = Marshal.GetDelegateForFunctionPointer(pFuncAddress, typeof(T));
                    _ahAjaFunctionsUsed.Add(cAjaAttr.sFunctionName, dFunctionPointer);
                }
                return (T)Convert.ChangeType(_ahAjaFunctionsUsed[cAjaAttr.sFunctionName], typeof(T), null);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                throw new MissingMethodException(String.Format("Function " + sAjaFunctionName + " was not found in aja dll."), e);
            }
        }

        [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
        internal sealed class AjaFunctionAttribute : Attribute
        {
            public string sFunctionName { get; private set; }
            public AjaFunctionAttribute(string sFunctionName)
            {
                this.sFunctionName = sFunctionName;
            }
        }

        internal class Functions
        {
            #region CNTV2DeviceScanner
            [AjaFunctionAttribute("CNTV2DeviceScanner_Create")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr CNTV2DeviceScanner_Create();
            [AjaFunctionAttribute("CNTV2DeviceScanner_Dispose")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr CNTV2DeviceScanner_Dispose(IntPtr pDeviceScannerObject);
            [AjaFunctionAttribute("CNTV2DeviceScanner_GetNumDevices")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate uint CNTV2DeviceScanner_GetNumDevices(IntPtr pDeviceScannerObject);
            [AjaFunctionAttribute("CNTV2DeviceScanner_GetDeviceInfo")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2DeviceScanner_GetDeviceInfo(IntPtr pDeviceScannerObject, uint inDeviceIndex, out DeviceScanner.NTV2DeviceInfo.Wrapper boardInfo);
            [AjaFunctionAttribute("CNTV2DeviceScanner_GetDeviceAtIndex")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2DeviceScanner_GetDeviceAtIndex(IntPtr pDeviceScannerObject, uint inDeviceIndexNumber, out IntPtr outDevice);
            #endregion

            #region CNTV2Card
            [AjaFunctionAttribute("CNTV2Card_Dispose")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CNTV2Card_Dispose(IntPtr pDevice);
            [AjaFunctionAttribute("CNTV2Card_IsDeviceReady")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_IsDeviceReady(IntPtr pDevice);
            [AjaFunctionAttribute("CNTV2Card_AcquireStreamForApplication")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_AcquireStreamForApplication(IntPtr pDevice, uint appCode, int pid);
            [AjaFunctionAttribute("CNTV2Card_ReleaseStreamForApplication")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_ReleaseStreamForApplication(IntPtr pDevice, uint appCode, int pid);
            [AjaFunctionAttribute("CNTV2Card_GetEveryFrameServices")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetEveryFrameServices(IntPtr pDevice, out NTV2EveryFrameTaskMode pOutMode);
            [AjaFunctionAttribute("CNTV2Card_SetEveryFrameServices")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetEveryFrameServices(IntPtr pDevice, NTV2EveryFrameTaskMode mode);
            [AjaFunctionAttribute("CNTV2Card_GetDeviceID")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2DeviceID CNTV2Card_GetDeviceID(IntPtr pDevice);
            [AjaFunctionAttribute("CNTV2Card_SetMultiFormatMode")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetMultiFormatMode(IntPtr pDevice, [MarshalAs(UnmanagedType.U1)] bool inEnable);
            [AjaFunctionAttribute("CNTV2Card_GetVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetVideoFormat(IntPtr pDevice, out NTV2VideoFormat pOutValue, NTV2Channel inChannel);
            [AjaFunctionAttribute("CNTV2Card_SetVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetVideoFormat(IntPtr pDevice, NTV2VideoFormat value, [MarshalAs(UnmanagedType.U1)] bool ajaRetail, [MarshalAs(UnmanagedType.U1)] bool keepVancSettings, NTV2Channel channel);
            [AjaFunctionAttribute("CNTV2Card_SetSDIOutLevelAtoLevelBConversion")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetSDIOutLevelAtoLevelBConversion(IntPtr pDevice, NTV2Channel inOutputChannel, [MarshalAs(UnmanagedType.U1)] bool inEnable);
            [AjaFunctionAttribute("CNTV2Card_SetFrameBufferFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetFrameBufferFormat(IntPtr pDevice, NTV2Channel channel, NTV2FrameBufferFormat newFormat, [MarshalAs(UnmanagedType.U1)] bool inIsRetailMode); // def inIsRetailMode = false
            [AjaFunctionAttribute("CNTV2Card_SetReference")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetReference(IntPtr pDevice, NTV2ReferenceSource value);
            [AjaFunctionAttribute("CNTV2Card_EnableChannel")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_EnableChannel(IntPtr pDevice, NTV2Channel inChannel);
            [AjaFunctionAttribute("CNTV2Card_SetEnableVANCData")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetEnableVANCData(IntPtr pDevice, [MarshalAs(UnmanagedType.U1)] bool inVANCenable, [MarshalAs(UnmanagedType.U1)] bool inTallerVANC, NTV2Channel inChannel);
            [AjaFunctionAttribute("CNTV2Card_SubscribeOutputVerticalEvent")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SubscribeOutputVerticalEvent(IntPtr pDevice, NTV2Channel channel);
            [AjaFunctionAttribute("CNTV2Card_OutputDestHasRP188BypassEnabled")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_OutputDestHasRP188BypassEnabled(IntPtr pDevice, NTV2OutputDestination inOutputDest);
            [AjaFunctionAttribute("CNTV2Card_DisableRP188Bypass")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_DisableRP188Bypass(IntPtr pDevice, NTV2OutputDestination inOutputDest);
            [AjaFunctionAttribute("GetRP188RegisterForOutput")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate uint GetRP188RegisterForOutput(NTV2OutputDestination inOutputDest);
            [AjaFunctionAttribute("CNTV2Card_SetNumberAudioChannels")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetNumberAudioChannels(IntPtr pDevice, uint inNumChannels, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_SetAudioRate")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetAudioRate(IntPtr pDevice, NTV2AudioRate inRate, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_SetAudioBufferSize")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetAudioBufferSize(IntPtr pDevice, NTV2AudioBufferSize inValue, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_SetSDIOutputAudioSystem")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetSDIOutputAudioSystem(IntPtr pDevice, NTV2Channel inChannel, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_SetSDIOutputDS2AudioSystem")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetSDIOutputDS2AudioSystem(IntPtr pDevice, NTV2Channel inChannel, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_SetAudioLoopBack")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetAudioLoopBack(IntPtr pDevice, NTV2AudioLoopBack inValue, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_Connect")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_Connect(IntPtr pDevice, NTV2InputCrosspointID inInputXpt, NTV2OutputCrosspointID inOutputXpt);
            [AjaFunctionAttribute("CNTV2Card_SetSDITransmitEnable")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetSDITransmitEnable(IntPtr pDevice, NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool enable);
            [AjaFunctionAttribute("CNTV2Card_SetSDIOutputStandard")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetSDIOutputStandard(IntPtr pDevice, NTV2Channel inOutputSpigot, NTV2Standard inValue);
            [AjaFunctionAttribute("CNTV2Card_ClearRouting")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_ClearRouting(IntPtr pDevice);
            [AjaFunctionAttribute("CNTV2Card_SetUpOutputAutoCirculate")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetUpOutputAutoCirculate(IntPtr pDevice, NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool withAudio, NTV2AudioSystem inAudioSystem);
            [AjaFunctionAttribute("CNTV2Card_AutoCirculateGetStatus")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_AutoCirculateGetStatus(IntPtr pDevice, NTV2Channel inChannel, out NTV2Card.AutocirculateStatus.Wrapper outStatus);
            [AjaFunctionAttribute("CNTV2Card_AutoCirculateStart")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_AutoCirculateStart(IntPtr pDevice, NTV2Channel inChannel, ulong inStartTime = 0);
            //@param[in] inStartTime     Optionally specifies a future start time as an unsigned 64-bit "tick count" value that
            //is host-OS-dependent.If set to zero, the default, AutoCirculate will switch to the
            //"running" state at the next VBI received by the given channel.If non-zero, AutoCirculate
            //will remain in the "starting" state until the system tick clock exceeds this value, at
            //which point it will switch to the "running" state.This value is denominated in the same
            //time units as the finest-grained time counter available on the host's operating system.
            [AjaFunctionAttribute("CNTV2Card_AutoCirculateTransfer")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_AutoCirculateTransfer(IntPtr pDevice, NTV2Channel inChannel, IntPtr pACTransfer);
            [AjaFunctionAttribute("CNTV2Card_WaitForOutputVerticalInterrupt")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_WaitForOutputVerticalInterrupt(IntPtr pDevice, NTV2Channel inChannel, ushort inRepeatCount = 1);
            //inRepeatCount = 1 default .	Specifies the number of output VBIs to wait for until returning. Defaults to 1.
            [AjaFunctionAttribute("CNTV2Card_AutoCirculateStop")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_AutoCirculateStop(IntPtr pDevice, NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool inAbort = false);
            //inAbort  Specifies if AutoCirculate is to be immediately stopped, not gracefully. Defaults to false (graceful stop).
            [AjaFunctionAttribute("CNTV2Card_UnsubscribeOutputVerticalEvent")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_UnsubscribeOutputVerticalEvent(IntPtr pDevice, NTV2Channel channel);
            [AjaFunctionAttribute("CNTV2Card_SetVANCShiftMode")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_SetVANCShiftMode(IntPtr pDevice, NTV2Channel inChannel, NTV2VANCDataShiftMode inValue);
            [AjaFunctionAttribute("CNTV2Card_GetVideoBufferSize")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetVideoBufferSize(IntPtr pDevice, NTV2VideoFormat inVideoFormat, NTV2FrameBufferFormat inPixelFormat, out uint outVideoBufferSize);
            [AjaFunctionAttribute("CNTV2Card_GetAudioBufferSize")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetAudioBufferSize(IntPtr pDevice, NTV2AudioSystem inAudioSystem, out uint outAudioBufferSize);
            [AjaFunctionAttribute("CNTV2Card_GetStreamingApplication")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetStreamingApplication(IntPtr pDevice, out uint appCode, out int pid);
            [AjaFunctionAttribute("CNTV2Card_GetFrameRate")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetFrameRate(IntPtr pDevice, out NTV2FrameRate outValue, NTV2Channel inChannel);
            [AjaFunctionAttribute("CNTV2Card_GetActiveFrameDimensions")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool CNTV2Card_GetActiveFrameDimensions(IntPtr pDevice, out uint outWidth, out uint outHeight, NTV2Channel inChannel);
            [AjaFunctionAttribute("CNTV2Card_GetReferenceVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2VideoFormat CNTV2Card_GetReferenceVideoFormat(IntPtr pDevice);
            #endregion

            #region AUTOCIRCULATE_TRANSFER
            [AjaFunctionAttribute("AUTOCIRCULATE_TRANSFER_Create")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr AUTOCIRCULATE_TRANSFER_Create();
            [AjaFunctionAttribute("AUTOCIRCULATE_TRANSFER_Dispose")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void AUTOCIRCULATE_TRANSFER_Dispose(IntPtr pACTransfer);
            [AjaFunctionAttribute("AUTOCIRCULATE_TRANSFER_SetVideoBuffer")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AUTOCIRCULATE_TRANSFER_SetVideoBuffer(IntPtr pACTransfer, IntPtr pInVideoBuffer, uint inVideoByteCount); // uint*  pInVideoBuffer
            [AjaFunctionAttribute("AUTOCIRCULATE_TRANSFER_SetAudioBuffer")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AUTOCIRCULATE_TRANSFER_SetAudioBuffer(IntPtr pACTransfer, IntPtr pInAudioBuffer, uint inAudioByteCount); // uint*  pInAudioBuffer
            #endregion

            #region AjaTools
            [AjaFunctionAttribute("AjaTools_IsVideoFormatA")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_IsVideoFormatA(NTV2VideoFormat format);
            [AjaFunctionAttribute("AjaTools_IsVideoFormatB")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_IsVideoFormatB(NTV2VideoFormat format);
            [AjaFunctionAttribute("AjaTools_IsRGBFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_IsRGBFormat(NTV2FrameBufferFormat format);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDoMultiFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDoMultiFormat(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDoFrameStore1Display")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDoFrameStore1Display(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDoVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDoVideoFormat(NTV2DeviceID inDeviceID, NTV2VideoFormat inVideoFormat);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceGetNumFrameStores")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_NTV2DeviceGetNumFrameStores(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDo3GLevelConversion")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDo3GLevelConversion(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDoFrameBufferFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDoFrameBufferFormat(NTV2DeviceID inDeviceID, NTV2FrameBufferFormat inFBFormat);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceGetMaxAudioChannels")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_NTV2DeviceGetMaxAudioChannels(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceGetNumAudioSystems")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_NTV2DeviceGetNumAudioSystems(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceGetNumVideoOutputs")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_NTV2DeviceGetNumVideoOutputs(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceGetNumCSCs")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_NTV2DeviceGetNumCSCs(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceHasBiDirectionalSDI")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceHasBiDirectionalSDI(NTV2DeviceID inDeviceID);
            [AjaFunctionAttribute("AjaTools_NTV2DeviceCanDoWidget")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2DeviceCanDoWidget(NTV2DeviceID inDeviceID, NTV2WidgetID inWidgetID);
            [AjaFunctionAttribute("AjaTools_GetVideoWriteSize")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate ushort AjaTools_GetVideoWriteSize(NTV2VideoFormat inVideoFormat, NTV2FrameBufferFormat inFBFormat, [MarshalAs(UnmanagedType.U1)] bool inTallVANC, [MarshalAs(UnmanagedType.U1)] bool inTallerVANC);
            [AjaFunctionAttribute("AjaTools_GetNTV2ChannelForIndex")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2Channel AjaTools_GetNTV2ChannelForIndex(uint index);
            [AjaFunctionAttribute("AjaTools_NTV2ChannelToOutputDestination")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2OutputDestination AjaTools_NTV2ChannelToOutputDestination(NTV2Channel inChannel);
            [AjaFunctionAttribute("AjaTools_NTV2ChannelToAudioSystem")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2AudioSystem AjaTools_NTV2ChannelToAudioSystem(NTV2Channel inChannel);
            [AjaFunctionAttribute("AjaTools_GetNTV2StandardFromVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2Standard AjaTools_GetNTV2StandardFromVideoFormat(NTV2VideoFormat inVideoFormat);
            [AjaFunctionAttribute("AjaTools_GetCSCOutputXptFromChannel")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2OutputCrosspointID AjaTools_GetCSCOutputXptFromChannel(NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool inIsKey, [MarshalAs(UnmanagedType.U1)] bool inIsRGB);
            [AjaFunctionAttribute("AjaTools_GetFrameBufferOutputXptFromChannel")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2OutputCrosspointID AjaTools_GetFrameBufferOutputXptFromChannel(NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool inIsRGB, [MarshalAs(UnmanagedType.U1)] bool inIs425);
            [AjaFunctionAttribute("AjaTools_GetCSCInputXptFromChannel")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2InputCrosspointID AjaTools_GetCSCInputXptFromChannel(NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool inIsKeyInput);
            [AjaFunctionAttribute("AjaTools_GetSDIOutputInputXpt")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2InputCrosspointID AjaTools_GetSDIOutputInputXpt(NTV2Channel inChannel, [MarshalAs(UnmanagedType.U1)] bool inIsDS2);
            [AjaFunctionAttribute("AjaTools_GetOutputDestInputXpt")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate NTV2InputCrosspointID AjaTools_GetOutputDestInputXpt(NTV2OutputDestination inOutputDest, [MarshalAs(UnmanagedType.U1)] bool inIsSDI_DS2, ushort inHDMI_Quadrant);
            [AjaFunctionAttribute("AjaTools_NTV2VideoFormatToString")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void AjaTools_NTV2VideoFormatToString(NTV2VideoFormat inFormat, bool inUseFrameRate, byte[] outFormatString, out int outSize);
            [AjaFunctionAttribute("AjaTools_NTV2IsHdVideoFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_NTV2IsHdVideoFormat(NTV2VideoFormat inFormat);
            [AjaFunctionAttribute("AjaTools_Is8BitFrameBufferFormat")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool AjaTools_Is8BitFrameBufferFormat(NTV2FrameBufferFormat inPixelFormat);
            #endregion
        }
    }
    public abstract class AjaClass : AjaInterop, IDisposable
    {
        private object oLock;
        private bool bDisposed;
        internal AjaClass()
        {
            bDisposed = false;
            oLock = new object();
        }
        ~AjaClass()
        {
            Dispose();
        }
        internal delegate void DoDispose();
        abstract internal DoDispose dDispose { get; }
        public void Dispose()
        {
            lock (oLock)
            {
                if (bDisposed)
                    return;
                bDisposed = true;
            }
            dDispose();
        }
        void IDisposable.Dispose()
        {
            this.Dispose();
        }
    }
    public class DeviceScanner : AjaClass
    {
        internal override DoDispose dDispose
        {
            get
            {
                return delegate ()   //()=> { };
                {
                    if (_pDeviceScanner != IntPtr.Zero)
                        AjaFunction<Functions.CNTV2DeviceScanner_Dispose>().Invoke(_pDeviceScanner);
                };
            }
        }
        public class NTV2DeviceInfo  // representation of c++ struct NTV2DeviceInfo
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Wrapper
            {
                public NTV2DeviceID deviceID;                          /// @brief	Device ID/species	(e.g., DEVICE_ID_KONA3G, DEVICE_ID_IOXT, etc.)
                public uint deviceIndex;                     /// @brief		Device index number -- this will be phased out someday
                public uint pciSlot;                         /// @brief	PCI slot (if applicable and/or known)
                public ulong deviceSerialNumber;                    /// @brief	Unique device serial number
                public IntPtr deviceIdentifier_p;    //string               /// @brief	Device name as seen in Control Panel, Watcher, Cables, etc.
                public ulong deviceIdentifier_n;
                public ushort numVidInputs;                     /// @brief	Total number of video inputs -- analog, digital, whatever
                public ushort numVidOutputs;                        /// @brief	Total number of video outputs -- analog, digital, whatever
                public ushort numAnlgVidInputs;                 /// @brief	Total number of analog video inputs
                public ushort numAnlgVidOutputs;                    /// @brief	Total number of analog video outputs
                public ushort numHDMIVidInputs;                 /// @brief	Total number of HDMI inputs
                public ushort numHDMIVidOutputs;                    /// @brief	Total number of HDMI outputs
                public ushort numInputConverters;                   /// @brief	Total number of input converters
                public ushort numOutputConverters;              /// @brief	Total number of output converters
                public ushort numUpConverters;                  /// @brief	Total number of up-converters
                public ushort numDownConverters;                    /// @brief	Total number of down-converters
                public ushort downConverterDelay;
                [MarshalAs(UnmanagedType.U1)]
                public bool isoConvertSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool rateConvertSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool dvcproHDSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool qrezSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool hdvSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool quarterExpandSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool vidProcSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool dualLinkSupport;                   /// @brief	Supports dual-link?
                [MarshalAs(UnmanagedType.U1)]
                public bool colorCorrectionSupport;                /// @brief	Supports color correction?
                [MarshalAs(UnmanagedType.U1)]
                public bool programmableCSCSupport;                /// @brief	Programmable color space converter?
                [MarshalAs(UnmanagedType.U1)]
                public bool rgbAlphaOutputSupport;             /// @brief	Supports RGB alpha channel?
                [MarshalAs(UnmanagedType.U1)]
                public bool breakoutBoxSupport;                    /// @brief	Can support a breakout box?
                [MarshalAs(UnmanagedType.U1)]
                public bool procAmpSupport;
                [MarshalAs(UnmanagedType.U1)]
                public bool has2KSupport;                      /// @brief	Supports 2K formats?
                [MarshalAs(UnmanagedType.U1)]
                public bool has4KSupport;                      /// @brief	Supports 4K formats?
                [MarshalAs(UnmanagedType.U1)]
                public bool has3GLevelConversion;               /// @brief	Supports 3G Level Conversion?
                [MarshalAs(UnmanagedType.U1)]
                public bool proResSupport;                     /// @brief	Supports ProRes?
                [MarshalAs(UnmanagedType.U1)]
                public bool sdi3GSupport;                      /// @brief	Supports 3G?
                [MarshalAs(UnmanagedType.U1)]
                public bool ltcInSupport;                      /// @brief	Accepts LTC input?
                [MarshalAs(UnmanagedType.U1)]
                public bool ltcOutSupport;                     /// @brief	Supports LTC output?
                [MarshalAs(UnmanagedType.U1)]
                public bool ltcInOnRefPort;                        /// @brief	Supports LTC on reference input?
                [MarshalAs(UnmanagedType.U1)]
                public bool stereoOutSupport;                  /// @brief	Supports stereo output?
                [MarshalAs(UnmanagedType.U1)]
                public bool stereoInSupport;                   /// @brief	Supports stereo input?
                [MarshalAs(UnmanagedType.U1)]
                public bool multiFormat;                       /// @brief	Supports multiple video formats?
                public IntPtr videoFormatsList_p; //NTV2VideoFormat Enum
                public ulong videoFormatsList_n;
                public IntPtr audioSampleRateList_p; //AudioSampleRateEnum    /// @brief	My supported audio sample rates
                public ulong audioSampleRateList_n;
                public IntPtr audioNumChannelsList_p;  //AudioChannelsPerFrameEnum   /// @brief	My supported number of audio channels per frame
                public ulong audioNumChannelsList_n;
                public IntPtr audioBitsPerSampleList_p;  //AudioBitsPerSampleEnum      /// @brief	My supported audio bits-per-sample
                public ulong audioBitsPerSampleList_n;
                public IntPtr audioInSourceList_p;   //AudioSourceEnum      /// @brief	My supported audio input sources (AES, ADAT, etc.)
                public ulong audioInSourceList_n;
                public IntPtr audioOutSourceList_p;      //AudioSourceEnum             /// @brief	My supported audio output destinations (AES, etc.)
                public ulong audioOutSourceList_n;
                public ushort numAudioStreams;                  /// @brief	Maximum number of independent audio streams
                public ushort numAnalogAudioInputChannels;      /// @brief	Total number of analog audio input channels
                public ushort numAESAudioInputChannels;         /// @brief	Total number of AES audio input channels
                public ushort numEmbeddedAudioInputChannels;        /// @brief	Total number of embedded (SDI) audio input channels
                public ushort numHDMIAudioInputChannels;            /// @brief	Total number of HDMI audio input channels
                public ushort numAnalogAudioOutputChannels;     /// @brief	Total number of analog audio output channels
                public ushort numAESAudioOutputChannels;            /// @brief	Total number of AES audio output channels
                public ushort numEmbeddedAudioOutputChannels;       /// @brief	Total number of embedded (SDI) audio output channels
                public ushort numHDMIAudioOutputChannels;           /// @brief	Total number of HDMI audio output channels
                public ushort numDMAEngines;                        /// @brief	Total number of DMA engines
                public ushort numSerialPorts;                       /// @brief	Total number of serial ports
                public uint pingLED;
            }
            public Wrapper stWrapper;
            private string _deviceIdentifier;    //string    
            public string deviceIdentifier
            {
                get
                {
                    if (null == _deviceIdentifier && stWrapper.deviceIdentifier_p != IntPtr.Zero)
                    {
                        byte[] aTmp = new byte[stWrapper.deviceIdentifier_n];
                        Marshal.Copy(stWrapper.deviceIdentifier_p, aTmp, 0, (int)stWrapper.deviceIdentifier_n);
                        _deviceIdentifier = System.Text.Encoding.UTF8.GetString(aTmp);
                    }
                    return _deviceIdentifier;
                }
            }
            private NTV2VideoFormat[] _videoFormatsList;
            public NTV2VideoFormat[] videoFormatsList
            {
                get
                {
                    if (null == _videoFormatsList && stWrapper.videoFormatsList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.videoFormatsList_n];
                        Marshal.Copy(stWrapper.videoFormatsList_p, aTmp, 0, (int)stWrapper.videoFormatsList_n);
                        _videoFormatsList = aTmp.Select(o => (NTV2VideoFormat)o).ToArray();
                        //_videoFormatsList = (NTV2VideoFormat[])(object)aTmp;
                    }
                    return _videoFormatsList;
                }
            }
            private AudioSampleRateEnum[] _audioSampleRateList;
            public AudioSampleRateEnum[] audioSampleRateList
            {
                get
                {
                    if (null == _audioSampleRateList && stWrapper.audioSampleRateList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.audioSampleRateList_n];
                        Marshal.Copy(stWrapper.audioSampleRateList_p, aTmp, 0, (int)stWrapper.audioSampleRateList_n);
                        _audioSampleRateList = aTmp.Select(o => (AudioSampleRateEnum)o).ToArray();
                        //_audioSampleRateList = (AudioSampleRateEnum[])(object)aTmp;
                    }
                    return _audioSampleRateList;
                }
            }
            private AudioChannelsPerFrameEnum[] _audioNumChannelsList;
            public AudioChannelsPerFrameEnum[] audioNumChannelsList
            {
                get
                {
                    if (null == _audioNumChannelsList && stWrapper.audioNumChannelsList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.audioNumChannelsList_n];
                        Marshal.Copy(stWrapper.audioNumChannelsList_p, aTmp, 0, (int)stWrapper.audioNumChannelsList_n);
                        _audioNumChannelsList = aTmp.Select(o => (AudioChannelsPerFrameEnum)o).ToArray();
                    }
                    return _audioNumChannelsList;
                }
            }
            private AudioBitsPerSampleEnum[] _audioBitsPerSampleList;
            public AudioBitsPerSampleEnum[] audioBitsPerSampleList
            {
                get
                {
                    if (null == _audioBitsPerSampleList && stWrapper.audioBitsPerSampleList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.audioBitsPerSampleList_n];
                        Marshal.Copy(stWrapper.audioBitsPerSampleList_p, aTmp, 0, (int)stWrapper.audioBitsPerSampleList_n);
                        _audioBitsPerSampleList = aTmp.Select(o => (AudioBitsPerSampleEnum)o).ToArray();
                    }
                    return _audioBitsPerSampleList;
                }
            }
            private AudioSourceEnum[] _audioInSourceList;
            public AudioSourceEnum[] audioInSourceList
            {
                get
                {
                    if (null == _audioInSourceList && stWrapper.audioInSourceList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.audioInSourceList_n];
                        Marshal.Copy(stWrapper.audioInSourceList_p, aTmp, 0, (int)stWrapper.audioInSourceList_n);
                        _audioInSourceList = aTmp.Select(o => (AudioSourceEnum)o).ToArray();
                    }
                    return _audioInSourceList;
                }
            }
            private AudioSourceEnum[] _audioOutSourceList;
            public AudioSourceEnum[] audioOutSourceList
            {
                get
                {
                    if (null == _audioOutSourceList && stWrapper.audioOutSourceList_p != IntPtr.Zero)
                    {
                        int[] aTmp = new int[stWrapper.audioOutSourceList_n];
                        Marshal.Copy(stWrapper.audioOutSourceList_p, aTmp, 0, (int)stWrapper.audioOutSourceList_n);
                        _audioOutSourceList = aTmp.Select(o => (AudioSourceEnum)o).ToArray();
                    }
                    return _audioOutSourceList;
                }
            }
        }

        private IntPtr _pDeviceScanner;

        public DeviceScanner()
            : base()
        {
            _pDeviceScanner = AjaFunction<Functions.CNTV2DeviceScanner_Create>().Invoke();
        }

        public uint GetNumDevices()
        {
            return AjaFunction<Functions.CNTV2DeviceScanner_GetNumDevices>().Invoke(_pDeviceScanner);
        }
        public NTV2DeviceInfo GetDeviceInfo(uint nDeviceIndex)
        {
            NTV2DeviceInfo cRetVal = new NTV2DeviceInfo();
            AjaFunction<Functions.CNTV2DeviceScanner_GetDeviceInfo>().Invoke(_pDeviceScanner, nDeviceIndex, out cRetVal.stWrapper);
            return cRetVal;
        }
        public IntPtr GetDeviceAtIndex(uint nDeviceIndex)
        {
            IntPtr pRetVal;
            AjaFunction<Functions.CNTV2DeviceScanner_GetDeviceAtIndex>().Invoke(_pDeviceScanner, nDeviceIndex, out pRetVal);
            return pRetVal;
        }
    }
    public class NTV2Card : AjaClass
    {
        internal override DoDispose dDispose
        {
            get
            {
                return delegate ()   //()=> { };
                {
                    if (bChannelInitiated == true)
                    {
                        AjaFunction<Functions.CNTV2Card_UnsubscribeOutputVerticalEvent>().Invoke(_pNTV2Card, _eOutputChannel);

                        if (!_bDoMultiChannel)
                        {
                            AjaFunction<Functions.CNTV2Card_SetEveryFrameServices>().Invoke(_pNTV2Card, _eSavedTaskMode);          //	Restore the previously saved service level
                            AjaFunction<Functions.CNTV2Card_ReleaseStreamForApplication>().Invoke(_pNTV2Card, _nKAppSignature, _nProcessID);    //	Release the device
                        }
                    }

                    Stop();

                    AjaFunction<Functions.CNTV2Card_Dispose>().Invoke(_pNTV2Card);
                    (new Logger()).WriteNotice("card disposed [index=" + _nDeviceIndex + "][channel=" + _eOutputChannel + "]");
                };
            }
        }
        public class AutocirculateStatus
        {
            public struct Wrapper
            {
                public NTV2Crosspoint acCrosspoint;                // @brief	The crosspoint (channel number with direction)
                public NTV2AutoCirculateState acState;                 // @brief	Current AutoCirculate state
                public int acStartFrame;             // @brief	First frame to circulate		FIXFIXFIX	Why is this signed?		CHANGE TO uint??
                public int acEndFrame;                   // @brief	Last frame to circulate			FIXFIXFIX	Why is this signed?		CHANGE TO uint??
                public int acActiveFrame;                // @brief	Current frame actually being captured/played when AutoCirculateGetStatus called	FIXFIXFIX	CHANGE TO uint??
                public ulong acRDTSCStartTime;          // @brief	Performance counter when AutoCirculateStart called
                public ulong acAudioClockStartTime;     // @brief	Register 28 when AutoCirculateStart called
                public ulong acRDTSCCurrentTime;            // @brief	Performance counter at moment AutoCirculateGetStatus called
                public ulong acAudioClockCurrentTime;   // @brief	Register 28 with wrap logic
                public uint acFramesProcessed;           // @brief	Total number of frames successfully processed since AutoCirculateStart called
                public uint acFramesDropped;         // @brief	Total number of frames dropped since AutoCirculateStart called
                public uint acBufferLevel;               // @brief	Number of buffered frames in driver ready to capture or play
                public uint acOptionFlags;               // @brief	AutoCirculate options (e.g., AUTOCIRCULATE_WITH_RP188, etc.)
                public NTV2AudioSystem acAudioSystem;              // @brief	The audio system being used for this channel (NTV2_AUDIOSYSTEM_INVALID if none)
            }
            public Wrapper stWrapper;
            public uint nFrameCount   // The number of frames being auto-circulated.
            {
                get
                {
                    return stWrapper.acEndFrame >= stWrapper.acStartFrame ? (uint)(stWrapper.acEndFrame - stWrapper.acStartFrame + 1) : 0;
                }
            }
            public uint nNumAvailableOutputFrames // The number of "unoccupied" output (playout) frames the device's AutoCirculate channel can currently accommodate.
            {
                get
                {
                    return nFrameCount > stWrapper.acBufferLevel ? nFrameCount - stWrapper.acBufferLevel : 0;
                }
            }

            public void LoadAutocirculateStatus(IntPtr pCard, NTV2Channel eOutputChannel)
            {
                AjaFunction<Functions.CNTV2Card_AutoCirculateGetStatus>().Invoke(pCard, eOutputChannel, out stWrapper);
            }
            public void LoadAutocirculateStatus(NTV2Card cCard)
            {
                LoadAutocirculateStatus(cCard._pNTV2Card, cCard._eOutputChannel);
            }
        }

        private IntPtr _pNTV2Card;
        private int _nProcessID;
        private uint _nKAppSignature;
        private DeviceScanner _cDScanner;
        private uint _nDeviceIndex;
        private bool _bWithAudio;
        private NTV2Channel _eOutputChannel;
        private NTV2FrameBufferFormat _ePixelFormat;
        private NTV2OutputDestination _eOutputDestination;
        private NTV2VideoFormat _eVideoFormat;
        private bool _bDoMultiChannel;
        private bool _bEnableVanc;
        private bool _bLevelConversion;
        private NTV2DeviceID _eDeviceID;
        private NTV2EveryFrameTaskMode _eSavedTaskMode;
        private NTV2AudioSystem _eAudioSystem;
        private NTV2AudioRate _AudioSaplesRate;
        private System.Threading.Thread _cThreadPlayingFramesWorker;
        private bool _bGlobalQuit;
        private ushort _nAudioChannelsQty;
        public bool? bChannelStopped;
        public bool? bChannelInitiated;
        public uint nVideoBufferSize;
        public uint nAudioBufferSize;
        public delegate byte[] GetVideoFrame();
        public GetVideoFrame DoGetVideoFrame;
        public delegate byte[] GetAudioFrame();
        public GetAudioFrame DoGetAudioFrame;
        public uint nBufferLength;
        public uint nBufferMaxLength;

        public NTV2Card(uint nDeviceIndex, bool bWithAudio, NTV2Channel eOutputChannel, NTV2FrameBufferFormat ePixelFormat, NTV2OutputDestination eOutputDestination, NTV2VideoFormat eVideoFormat, NTV2AudioRate AudioSaplesRate, bool bDoMultiChannel, ushort nAudioChannelsQty, bool bEnableVanc, bool bLevelConversion)
        {
            _nDeviceIndex = nDeviceIndex;
            _bWithAudio = bWithAudio;
            _eOutputChannel = eOutputChannel;
            _ePixelFormat = ePixelFormat;
            _eOutputDestination = eOutputDestination;
            _eVideoFormat = eVideoFormat;
            _bDoMultiChannel = bDoMultiChannel;
            _bEnableVanc = bEnableVanc;
            _bLevelConversion = bLevelConversion;
            _nAudioChannelsQty = nAudioChannelsQty;
            _AudioSaplesRate = AudioSaplesRate;
            _nProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
            _nKAppSignature = AjaTools.AjaFourCC('R', 'P', 'L', 'C');
            _cDScanner = new DeviceScanner();
            _pNTV2Card = _cDScanner.GetDeviceAtIndex(nDeviceIndex);
        }

        public NTV2VideoFormat eRefStatus
        {
            get
            {
                return AjaFunction<Functions.CNTV2Card_GetReferenceVideoFormat>().Invoke(_pNTV2Card);  // NTV2VideoFormat.NTV2_FORMAT_UNKNOWN  if no ref
            }
        }
        public void Init()
        {
            (new Logger()).WriteNotice("will init");
            if (!AjaFunction<Functions.CNTV2Card_IsDeviceReady>().Invoke(_pNTV2Card))
                throw new Exception("The Aja device (" + _nDeviceIndex + ") not ready");

            if (!_bDoMultiChannel)
            {
                if (!AjaFunction<Functions.CNTV2Card_AcquireStreamForApplication>().Invoke(_pNTV2Card, _nKAppSignature, _nProcessID))
                {
                    bool bT = false;
                    if (false)
                    {
                        // GetStreamingApplication   наверно надо в начале, а потом зарелизить (ниже) - это если CNTV2Card_AcquireStreamForApplication не проходит. Однажды было при отладке. Прошло само при запуске 'DEMO'.
                        uint nAppCode;
                        int nPid;
                        AjaFunction<Functions.CNTV2Card_GetStreamingApplication>().Invoke(_pNTV2Card, out nAppCode, out nPid);
                        bT = AjaFunction<Functions.CNTV2Card_ReleaseStreamForApplication>().Invoke(_pNTV2Card, nAppCode, nPid);    //	Release the device
                        bT = AjaFunction<Functions.CNTV2Card_AcquireStreamForApplication>().Invoke(_pNTV2Card, _nKAppSignature, _nProcessID);
                    }
                    if (!bT)
                        throw new Exception("Device (" + _nDeviceIndex + ") is in use by another app");
                }
                AjaFunction<Functions.CNTV2Card_GetEveryFrameServices>().Invoke(_pNTV2Card, out _eSavedTaskMode);  //	Save the current service level
            }

            AjaFunction<Functions.CNTV2Card_SetEveryFrameServices>().Invoke(_pNTV2Card, NTV2EveryFrameTaskMode.NTV2_OEM_TASKS);  //	Set OEM service level

            _eDeviceID = AjaFunction<Functions.CNTV2Card_GetDeviceID>().Invoke(_pNTV2Card);  //	Keep this ID handy -- it's used frequently

            if (AjaFunction<Functions.AjaTools_NTV2DeviceCanDoMultiFormat>().Invoke(_eDeviceID))
            {
                if (_bDoMultiChannel)
                    AjaFunction<Functions.CNTV2Card_SetMultiFormatMode>().Invoke(_pNTV2Card, true);
                else
                    AjaFunction<Functions.CNTV2Card_SetMultiFormatMode>().Invoke(_pNTV2Card, false);
            }

            //	Beware -- some devices (e.g. Corvid1) can only output from FrameStore 2...
            if ((_eOutputChannel == NTV2Channel.NTV2_CHANNEL1) && !AjaFunction<Functions.AjaTools_NTV2DeviceCanDoFrameStore1Display>().Invoke(_eDeviceID))
                _eOutputChannel = NTV2Channel.NTV2_CHANNEL2;
            ushort nFrameStoresQty = AjaFunction<Functions.AjaTools_NTV2DeviceGetNumFrameStores>().Invoke(_eDeviceID);
            if ((int)_eOutputChannel >= nFrameStoresQty)
            {
                string sErr = "Device (" + _nDeviceIndex + ") Cannot use channel [" + _eOutputChannel + "]  -- device only supports channel 1";
                sErr += nFrameStoresQty > 1 ? " thru " + nFrameStoresQty : "";
                throw new Exception(sErr);
            }


            //=========================== Set up the video...
            if (!AjaFunction<Functions.AjaTools_NTV2DeviceCanDoVideoFormat>().Invoke(_eDeviceID, _eVideoFormat))
                throw new Exception("This device (" + _nDeviceIndex + ") cannot handle video format [" + _eVideoFormat + " = '" + AjaTools.NTV2VideoFormatToString(_eVideoFormat) + "']");

            //	Configure the device to handle the requested video format...
            AjaFunction<Functions.CNTV2Card_SetVideoFormat>().Invoke(_pNTV2Card, _eVideoFormat, false, false, _eOutputChannel);

            if (!AjaFunction<Functions.AjaTools_NTV2DeviceCanDo3GLevelConversion>().Invoke(_eDeviceID) && _bLevelConversion && AjaFunction<Functions.AjaTools_IsVideoFormatA>().Invoke(_eVideoFormat))
                _bLevelConversion = false;
            if (_bLevelConversion)
                AjaFunction<Functions.CNTV2Card_SetSDIOutLevelAtoLevelBConversion>().Invoke(_pNTV2Card, _eOutputChannel, _bLevelConversion);

            //	Set the frame buffer pixel format for all the channels on the device.
            //	If the device doesn't support it, fall back to 8-bit YCbCr...

            if (!AjaFunction<Functions.AjaTools_NTV2DeviceCanDoFrameBufferFormat>().Invoke(_eDeviceID, _ePixelFormat))
                throw new Exception("Device cannot handle pixel format [" + _ePixelFormat + "] -- use for example [NTV2_FBF_8BIT_YCBCR] instead");

            AjaFunction<Functions.CNTV2Card_SetFrameBufferFormat>().Invoke(_pNTV2Card, _eOutputChannel, _ePixelFormat, false);
            AjaFunction<Functions.CNTV2Card_SetReference>().Invoke(_pNTV2Card, NTV2ReferenceSource.NTV2_REFERENCE_FREERUN);
            AjaFunction<Functions.CNTV2Card_EnableChannel>().Invoke(_pNTV2Card, _eOutputChannel);

            // VANC
            if (_bEnableVanc && !AjaFunction<Functions.AjaTools_IsRGBFormat>().Invoke(_ePixelFormat) && AjaFunction<Functions.AjaTools_NTV2IsHdVideoFormat>().Invoke(_eVideoFormat))
            {
                //	Try enabling VANC...
                AjaFunction<Functions.CNTV2Card_SetEnableVANCData>().Invoke(_pNTV2Card, true, false, _eOutputChannel); // def NTV2_CHANNEL1    // Enable VANC for non-SD formats, to pass thru captions, etc.
                if (AjaFunction<Functions.AjaTools_Is8BitFrameBufferFormat>().Invoke(_ePixelFormat))
                {
                    //	8-bit FBFs require VANC bit shift...
                    AjaFunction<Functions.CNTV2Card_SetVANCShiftMode>().Invoke(_pNTV2Card, _eOutputChannel, NTV2VANCDataShiftMode.NTV2_VANCDATA_8BITSHIFT_ENABLE);
                }
            }   //	if not RGB and is HD video format
            else
                AjaFunction<Functions.CNTV2Card_SetEnableVANCData>().Invoke(_pNTV2Card, false, false, _eOutputChannel);  // def NTV2_CHANNEL1     // No VANC with RGB pixel formats (for now)

            //	Subscribe the output interrupt -- it's enabled by default...
            AjaFunction<Functions.CNTV2Card_SubscribeOutputVerticalEvent>().Invoke(_pNTV2Card, _eOutputChannel);

            if (AjaFunction<Functions.CNTV2Card_OutputDestHasRP188BypassEnabled>().Invoke(_pNTV2Card, _eOutputDestination))
                AjaFunction<Functions.CNTV2Card_DisableRP188Bypass>().Invoke(_pNTV2Card, _eOutputDestination);

            // get buffer size
            AjaFunction<Functions.CNTV2Card_GetVideoBufferSize>().Invoke(_pNTV2Card, _eVideoFormat, _ePixelFormat, out nVideoBufferSize);


            //=========================== Set up the audio...
            ushort nAudioChannelsQty = AjaFunction<Functions.AjaTools_NTV2DeviceGetMaxAudioChannels>().Invoke(_eDeviceID);
            if (_nAudioChannelsQty == 6 || _nAudioChannelsQty == 8) // only 6, 8 and 16(max) accepted
                nAudioChannelsQty = _nAudioChannelsQty;

            //	Use NTV2_AUDIOSYSTEM_1, unless the device has more than one audio system...
            if (AjaFunction<Functions.AjaTools_NTV2DeviceGetNumAudioSystems>().Invoke(_eDeviceID) > 1)
                _eAudioSystem = AjaFunction<Functions.AjaTools_NTV2ChannelToAudioSystem>().Invoke(_eOutputChannel);  //	...and base it on the channel
                                                                                                                     //	However, there are a few older devices that have only 1 audio system, yet 2 frame stores (or must use channel 2 for playout)...
            if (!AjaFunction<Functions.AjaTools_NTV2DeviceCanDoFrameStore1Display>().Invoke(_eDeviceID))
                throw new Exception("this device (" + _nDeviceIndex + ") cannot use [" + _eAudioSystem + "] and have only [NTV2_AUDIOSYSTEM_1]");

            AjaFunction<Functions.CNTV2Card_SetNumberAudioChannels>().Invoke(_pNTV2Card, nAudioChannelsQty, _eAudioSystem);
            AjaFunction<Functions.CNTV2Card_SetAudioRate>().Invoke(_pNTV2Card, _AudioSaplesRate, _eAudioSystem);

            //	How big should the on-device audio buffer be?   1MB? 2MB? 4MB? 8MB?
            //	For this demo, 4MB will work best across all platforms (Windows, Mac & Linux)...
            AjaFunction<Functions.CNTV2Card_SetAudioBufferSize>().Invoke(_pNTV2Card, NTV2AudioBufferSize.NTV2_AUDIO_BUFFER_BIG, _eAudioSystem);

            //	Set the SDI output audio embedders to embed audio samples from the output of _eAudioSystem...
            AjaFunction<Functions.CNTV2Card_SetSDIOutputAudioSystem>().Invoke(_pNTV2Card, _eOutputChannel, _eAudioSystem);
            AjaFunction<Functions.CNTV2Card_SetSDIOutputDS2AudioSystem>().Invoke(_pNTV2Card, _eOutputChannel, _eAudioSystem);

            //	If the last app using the device left it in end-to-end mode (input passthru),
            //	then loopback must be disabled, or else the output will contain whatever audio
            //	is present in whatever signal is feeding the device's SDI input...
            AjaFunction<Functions.CNTV2Card_SetAudioLoopBack>().Invoke(_pNTV2Card, NTV2AudioLoopBack.NTV2_AUDIO_LOOPBACK_OFF, _eAudioSystem);

            // get buffer size
            AjaFunction<Functions.CNTV2Card_GetAudioBufferSize>().Invoke(_pNTV2Card, _eAudioSystem, out nAudioBufferSize);


            //=========================== Set up Route Output...
            //	Set up the device signal routing, and playout AutoCirculate...
            NTV2Standard eOutputStandard = AjaFunction<Functions.AjaTools_GetNTV2StandardFromVideoFormat>().Invoke(_eVideoFormat);
            ushort nVideoOutputsQty = AjaFunction<Functions.AjaTools_NTV2DeviceGetNumVideoOutputs>().Invoke(_eDeviceID);
            bool bIsRGB = AjaFunction<Functions.AjaTools_IsRGBFormat>().Invoke(_ePixelFormat);

            //	If device has no RGB conversion capability for the desired channel, use YUV instead
            if ((ushort)_eOutputChannel > AjaFunction<Functions.AjaTools_NTV2DeviceGetNumCSCs>().Invoke(_eDeviceID))
                bIsRGB = false;

            NTV2OutputCrosspointID eCSCVidOutXpt = AjaFunction<Functions.AjaTools_GetCSCOutputXptFromChannel>().Invoke(_eOutputChannel, false, bIsRGB);
            NTV2OutputCrosspointID eFSVidOutXpt = AjaFunction<Functions.AjaTools_GetFrameBufferOutputXptFromChannel>().Invoke(_eOutputChannel, bIsRGB, false);
            if (bIsRGB)
                AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetCSCInputXptFromChannel>().Invoke(_eOutputChannel, false), eFSVidOutXpt);

            if (_bDoMultiChannel)
            {
                //	Multiformat --- route the one SDI output to the CSC video output (RGB) or FrameStore output (YUV)...
                if (AjaFunction<Functions.AjaTools_NTV2DeviceHasBiDirectionalSDI>().Invoke(_eDeviceID))
                    AjaFunction<Functions.CNTV2Card_SetSDITransmitEnable>().Invoke(_pNTV2Card, _eOutputChannel, true);

                AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetSDIOutputInputXpt>().Invoke(_eOutputChannel, false), bIsRGB ? eCSCVidOutXpt : eFSVidOutXpt);
                AjaFunction<Functions.CNTV2Card_SetSDIOutputStandard>().Invoke(_pNTV2Card, _eOutputChannel, eOutputStandard);
            }
            else
            {
                //	Not multiformat:  Route all possible SDI outputs to CSC video output (RGB) or FrameStore output (YUV)...
                AjaFunction<Functions.CNTV2Card_ClearRouting>().Invoke(_pNTV2Card);

                if (bIsRGB)
                    AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetCSCInputXptFromChannel>().Invoke(_eOutputChannel, false), eFSVidOutXpt);

                for (NTV2Channel eChan = NTV2Channel.NTV2_CHANNEL1; (ushort)eChan < nVideoOutputsQty; eChan = (NTV2Channel)(eChan + 1))
                {
                    if (AjaFunction<Functions.AjaTools_NTV2DeviceHasBiDirectionalSDI>().Invoke(_eDeviceID))
                        AjaFunction<Functions.CNTV2Card_SetSDITransmitEnable>().Invoke(_pNTV2Card, eChan, true);       //	Make it an output

                    AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetSDIOutputInputXpt>().Invoke(eChan, false), bIsRGB ? eCSCVidOutXpt : eFSVidOutXpt);
                    AjaFunction<Functions.CNTV2Card_SetSDIOutputStandard>().Invoke(_pNTV2Card, eChan, eOutputStandard);
                }   //	for each output spigot

                if (AjaFunction<Functions.AjaTools_NTV2DeviceCanDoWidget>().Invoke(_eDeviceID, NTV2WidgetID.NTV2_WgtAnalogOut1))
                    AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetOutputDestInputXpt>().Invoke(NTV2OutputDestination.NTV2_OUTPUTDESTINATION_ANALOG, false, 99), bIsRGB ? eCSCVidOutXpt : eFSVidOutXpt);

                if (AjaFunction<Functions.AjaTools_NTV2DeviceCanDoWidget>().Invoke(_eDeviceID, NTV2WidgetID.NTV2_WgtHDMIOut1)
                    || AjaFunction<Functions.AjaTools_NTV2DeviceCanDoWidget>().Invoke(_eDeviceID, NTV2WidgetID.NTV2_WgtHDMIOut1v2)
                    || AjaFunction<Functions.AjaTools_NTV2DeviceCanDoWidget>().Invoke(_eDeviceID, NTV2WidgetID.NTV2_WgtHDMIOut1v3))
                    AjaFunction<Functions.CNTV2Card_Connect>().Invoke(_pNTV2Card, AjaFunction<Functions.AjaTools_GetOutputDestInputXpt>().Invoke(NTV2OutputDestination.NTV2_OUTPUTDESTINATION_HDMI, false, 99), bIsRGB ? eCSCVidOutXpt : eFSVidOutXpt);
            }


            //=========================== Other set up...
            if (IsDeviceInUse())
                throw new Exception("attempt to init busy channel [indx="+_nDeviceIndex+"][channel="+ _eOutputChannel + "]");
            if (AjaFunction<Functions.CNTV2Card_SetUpOutputAutoCirculate>().Invoke(_pNTV2Card, _eOutputChannel, _bWithAudio, _eAudioSystem))
                bChannelInitiated = true;

            // не нужно пока:
            //	This is for the timecode that we will burn onto the image...
            //NTV2FormatDescriptor fd  (::GetFormatDescriptor(mVideoFormat, _ePixelFormat, mVancEnabled, mWideVanc));   
            //	Lastly, prepare my AJATimeCodeBurn instance...
            //mTCBurner.RenderTimeCodeFont(CNTV2DemoCommon::GetAJAPixelFormat(_ePixelFormat), fd.numPixels, fd.numLines);
        }
        public void Run()
        {
            (new Logger()).WriteNotice("will run");
            if (null == _cThreadPlayingFramesWorker)
            {
                _cThreadPlayingFramesWorker = new System.Threading.Thread(PlayingFramesWorker);
                _cThreadPlayingFramesWorker.IsBackground = true;
                _cThreadPlayingFramesWorker.Priority = System.Threading.ThreadPriority.AboveNormal;
                _cThreadPlayingFramesWorker.Start();
            }
        }
        public void Stop()
        {
            (new Logger()).WriteNotice("will stop");
            _bGlobalQuit = true;
            while (null != _cThreadPlayingFramesWorker && _cThreadPlayingFramesWorker.IsAlive)
                System.Threading.Thread.Sleep(10);
            if (bChannelStopped == null && bChannelInitiated == true)
            {
                AjaFunction<Functions.CNTV2Card_AutoCirculateStop>().Invoke(_pNTV2Card, _eOutputChannel, false);
                (new Logger()).WriteNotice("AutoCirculate after init stopped");
                bChannelInitiated = false;
            }
        }
        private void PlayingFramesWorker(object cState)
        {
            try
            {
                (new Logger()).WriteNotice("autocirculate start");
                AutocirculateTransfer cOutputXferInfo = new AutocirculateTransfer();

                bChannelStopped = false;
                bChannelInitiated = false;
                ulong nStartTime = 0;
                
                AutocirculateStatus cOutputStatus = new AutocirculateStatus();

                while (true)  // preroll
                {
                    if (!AddFrameToDevice(cOutputXferInfo, cOutputStatus))
                        break;
                }

                AjaFunction<Functions.CNTV2Card_AutoCirculateStart>().Invoke(_pNTV2Card, _eOutputChannel, nStartTime); //	Start it running
                while (!_bGlobalQuit)
                {
                    if (!AddFrameToDevice(cOutputXferInfo, cOutputStatus))
                        AjaFunction<Functions.CNTV2Card_WaitForOutputVerticalInterrupt>().Invoke(_pNTV2Card, _eOutputChannel, 1);
                }   //	loop til quit signaled
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
            finally
            {
                //	Stop AutoCirculate...
                AjaFunction<Functions.CNTV2Card_AutoCirculateStop>().Invoke(_pNTV2Card, _eOutputChannel, false);
                (new Logger()).WriteNotice("AutoCirculate thread stopped");
                bChannelStopped = true;
            }
        }
        private bool AddFrameToDevice(AutocirculateTransfer cOutputXferInfo, AutocirculateStatus cOutputStatus)
        {
            cOutputStatus.LoadAutocirculateStatus(_pNTV2Card, _eOutputChannel);

            if (0 == nBufferMaxLength)
                nBufferMaxLength = cOutputStatus.nFrameCount;
            nBufferLength = cOutputStatus.stWrapper.acBufferLevel;
            //	Check if there's room for another frame on the card...
            if (cOutputStatus.nNumAvailableOutputFrames > 1)
            {
                //	Wait for the next frame to become ready to "consume"...
                byte[] aAudioFrame = _bWithAudio ? DoGetAudioFrame() : null;
                byte[] aVideoFrame = DoGetVideoFrame();
                if (aAudioFrame != null && aVideoFrame != null)
                {
                    //	Include timecode in output signal...
                    //mOutputXferInfo.SetOutputTimeCode(new AutocirculateTransfer.RP188_STRUCT(), _eOutputChannel);

                    //	Transfer the timecode-burned frame to the device for playout...
                    cOutputXferInfo.SetVideoBuffer(aVideoFrame);
                    cOutputXferInfo.SetAudioBuffer(aAudioFrame);
                    AjaFunction<Functions.CNTV2Card_AutoCirculateTransfer>().Invoke(_pNTV2Card, _eOutputChannel, cOutputXferInfo._pAutocirculateTransfer);
                    cOutputXferInfo.FreeGCHandles();
                }
                return true;
            }
            return false;
        }
        public NTV2VideoFormat GetCurrentVideoFormat()
        {
            NTV2VideoFormat eRetVal;
            AjaFunction<Functions.CNTV2Card_GetVideoFormat>().Invoke(_pNTV2Card, out eRetVal, _eOutputChannel);
            return eRetVal;
        }
        public NTV2FrameRate GetCurrentFramrRate()
        {
            NTV2FrameRate eRetVal;
            AjaFunction<Functions.CNTV2Card_GetFrameRate>().Invoke(_pNTV2Card, out eRetVal, _eOutputChannel);
            return eRetVal;
        }
        public void GetActiveFrameDimensions(out ushort nWidth, out ushort nHeight)
        {
            uint nW, nH;
            AjaFunction<Functions.CNTV2Card_GetActiveFrameDimensions>().Invoke(_pNTV2Card, out nW, out nH, _eOutputChannel);
            nWidth = (ushort)nW;
            nHeight = (ushort)nH;
        }
        public void SetVideoFormat()
        {
            _eDeviceID = AjaFunction<Functions.CNTV2Card_GetDeviceID>().Invoke(_pNTV2Card);
            if (!AjaFunction<Functions.AjaTools_NTV2DeviceCanDoVideoFormat>().Invoke(_eDeviceID, _eVideoFormat))
                throw new Exception("This device (" + _nDeviceIndex + ") is used by another app or cannot handle video format [" + _eVideoFormat + " = '" + AjaTools.NTV2VideoFormatToString(_eVideoFormat) + "']");

            if (!AjaFunction<Functions.CNTV2Card_SetVideoFormat>().Invoke(_pNTV2Card, _eVideoFormat, false, false, _eOutputChannel))
                throw new Exception("This device (" + _nDeviceIndex + ") cannot set the video format [" + _eVideoFormat + " = '" + AjaTools.NTV2VideoFormatToString(_eVideoFormat) + "']");
        }
        public void ResetChannel()
        {
            AjaFunction<Functions.CNTV2Card_AutoCirculateStop>().Invoke(_pNTV2Card, _eOutputChannel, false);
        }
        public bool IsDeviceInUse()
        {
            AutocirculateStatus cOutputStatus = new AutocirculateStatus();
            cOutputStatus.LoadAutocirculateStatus(_pNTV2Card, _eOutputChannel);
            if (cOutputStatus.stWrapper.acState != NTV2AutoCirculateState.NTV2_AUTOCIRCULATE_DISABLED)
                return true;
            return false;
        }
    }
    public class AutocirculateTransfer : AjaClass
    {
        internal override DoDispose dDispose
        {
            get
            {
                return delegate ()   //()=> { };
                {
                    AjaFunction<Functions.AUTOCIRCULATE_TRANSFER_Dispose>().Invoke(_pAutocirculateTransfer);
                };
            }
        }
        public struct RP188_STRUCT  // wrapper.  НЕ заведён из aja c++ dll, т.к. пока не надо
        {
            uint DBB;
            uint Low;     //  |  BG 4  | Secs10 |  BG 3  | Secs 1 |  BG 2  | Frms10 |  BG 1  | Frms 1 |
            uint High;        //  |  BG 8  | Hrs 10 |  BG 7  | Hrs  1 |  BG 6  | Mins10 |  BG 5  | Mins 1 |
        }
        internal IntPtr _pAutocirculateTransfer;
        public AutocirculateTransfer()
        {
            _pAutocirculateTransfer = AjaFunction<Functions.AUTOCIRCULATE_TRANSFER_Create>().Invoke();
        }

        private GCHandle _cGCHandleVideo;
        private GCHandle _cGCHandleAudio;

        public void SetOutputTimeCode(RP188_STRUCT stRP188, NTV2Channel eOutputChannel)
        {
            // не заведено пока
            //mOutputXferInfo.SetOutputTimeCode(stRP188, ::NTV2ChannelToTimecodeIndex(eOutputChannel));
        }
        public void SetVideoBuffer(byte[] aBytes)
        {
            //Marshal.Copy(aTMPVideo, 0, cFrameResult.cVideo.pFrameBytes, aTMPVideo.Length);
            _cGCHandleVideo = GCHandle.Alloc(aBytes, GCHandleType.Pinned);
            AjaFunction<Functions.AUTOCIRCULATE_TRANSFER_SetVideoBuffer>().Invoke(_pAutocirculateTransfer, _cGCHandleVideo.AddrOfPinnedObject(), (uint)aBytes.Length);
        }
        public void SetAudioBuffer(byte[] aBytes)
        {
            if (aBytes == null)
                AjaFunction<Functions.AUTOCIRCULATE_TRANSFER_SetAudioBuffer>().Invoke(_pAutocirculateTransfer, IntPtr.Zero, 0);
            else
            {
                _cGCHandleAudio = GCHandle.Alloc(aBytes, GCHandleType.Pinned);
                AjaFunction<Functions.AUTOCIRCULATE_TRANSFER_SetAudioBuffer>().Invoke(_pAutocirculateTransfer, _cGCHandleAudio.AddrOfPinnedObject(), (uint)aBytes.Length);
            }
        }
        public void FreeGCHandles()
        {
            _cGCHandleVideo.Free();
            _cGCHandleAudio.Free();
        }
    }
    public class AjaTools : AjaClass
    {
        internal override DoDispose dDispose
        {
            get
            {
                return delegate ()   //()=> { };
                {

                };
            }
        }
        public static uint AjaFourCC(char a, char b, char c, char d)
        {
            return ((((uint)(a)) << 24) + (((uint)(b)) << 16) + (((uint)(c)) << 8) + (((uint)(d)) << 0));
        }
        public static string NTV2VideoFormatToString(AjaClass.NTV2VideoFormat inFormat, bool inUseFrameRate = false)
        {
            byte[] aFormatString = new byte[100];
            int nSize;
            AjaFunction<Functions.AjaTools_NTV2VideoFormatToString>().Invoke(inFormat, inUseFrameRate, aFormatString, out nSize);
            byte[] aFormatString2 = new byte[nSize];
            for (int nI = 0; nI < nSize; nI++)
                aFormatString2[nI] = aFormatString[nI];
            return System.Text.Encoding.ASCII.GetString(aFormatString2);
        }
        public static NTV2OutputDestination NTV2ChannelToOutputDestination(NTV2Channel eChannel)
        {
            return AjaFunction<Functions.AjaTools_NTV2ChannelToOutputDestination>().Invoke(eChannel);
        }
    }
}
