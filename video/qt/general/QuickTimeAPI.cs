using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace helpers.video.qt
{
	public class QuickTimeAPI
	{
#region dll import
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short InitializeQTML(int flag);
		[DllImport("QTMLClient.dll")]
		public extern static void TerminateQTML();
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short EnterMovies();
		[DllImport("QTMLClient.dll")]
		public extern static void ExitMovies();
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short GetMoviesError();
		[DllImport("QTMLClient.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public extern static /*OSErr*/short NewGWorldFromHBITMAP(out IntPtr offscreenGWorld, IntPtr cTable, IntPtr aGDevice, IntPtr flags, IntPtr newHBITMAP, IntPtr newHDC);
		[DllImport("QTMLClient.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
		public extern static /*OSErr*/short NewGWorldFromPtr(out IntPtr offscreenGWorld, uint PixelFormat, IntPtr boundsRect, IntPtr cTable, IntPtr aGDevice, uint flags, IntPtr newBuffer, int rowBytes);
		//public extern static short NewGWorld(out IntPtr offscreenGWorld, short PixelDepth, IntPtr boundsRect, IntPtr cTable, IntPtr aGDevice, uint flags);
		[DllImport("QTMLClient.dll")]
		public extern static void GetMovieGWorld(IntPtr theMovie, out IntPtr port, out IntPtr gdh);
		[DllImport("QTMLClient.dll")]
		public extern static void SetMovieGWorld(IntPtr theMovie, IntPtr port, IntPtr gdh);
		[DllImport("QTMLClient.dll")]
		public extern static void DisposeGWorld(IntPtr offscreenGWorld);
		[DllImport("QTMLClient.dll")]
		public extern static void MacSetRect(IntPtr r, short left, short top, short right, short bottom);
		[DllImport("QTMLClient.dll")]
		public extern static IntPtr c2pstr(StringBuilder aStr);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short FSMakeFSSpec(IntPtr vRefNum, IntPtr dirID, StringBuilder fileName, IntPtr spec);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short OpenMovieFile(IntPtr fileSpec, ref short resRefNum, short permission);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short CloseMovieFile(short resRefNum);
		[DllImport("QTMLClient.dll")]
		public extern static short NewMovieFromFile(out /*Movie*/IntPtr theMovie, short resRefNum, IntPtr resId, IntPtr resName, short newMovieFlags, IntPtr dataRefWasChanged);
		[DllImport("QTMLClient.dll")]
		public extern static void GetMovieNextInterestingTime(IntPtr theMovie, short interestingTimeFlags, short numMediaTypes, uint[] whichMediaTypes, int time, int rate, out int interestingTime, out int interestingDuration);
		[DllImport("QTMLClient.dll")]
		public extern static void SetMovieTimeValue(/*Movie*/IntPtr theMovie, int newtime);
		[DllImport("QTMLClient.dll")]
		public extern static int GetMovieTime(/*Movie*/IntPtr theMovie, IntPtr wtf);
		[DllImport("QTMLClient.dll")]
		public extern static void SetMovieTime(/*Movie*/IntPtr theMovie, ref int newtime);
		[DllImport("QTMLClient.dll")]
		public extern static void MoviesTask(/*Movie*/IntPtr theMovie, int maxMilliSecToUse);
		[DllImport("QTMLClient.dll")]
		public extern static int GetMovieDuration(/*Movie*/IntPtr theMovie);
		[DllImport("QTMLClient.dll")]
		public extern static /*TimeScale*/int GetMovieTimeScale(/*Movie*/IntPtr theMovie);
		[DllImport("QTMLClient.dll")]
		public extern static void GoToBeginningOfMovie(/*Movie*/IntPtr theMovie);
		[DllImport("QTMLClient.dll")]
		public extern static /*Handle*/IntPtr NewHandle(/*Size*/int byteCount);
		[DllImport("QTMLClient.dll")]
		public extern static /*Handle*/IntPtr NewHandleClear(/*Size*/int byteCount);
		[DllImport("QTMLClient.dll")]
		public extern static /*Size*/int GetHandleSize(/*Handle*/IntPtr h);
		[DllImport("QTMLClient.dll")]
		public extern static void DisposeHandle(/*Handle*/IntPtr h);
		[DllImport("QTMLClient.dll")]
		public extern static void HLock(/*Handle*/IntPtr h);
		[DllImport("QTMLClient.dll")]
		public extern static void HUnlock(/*Handle*/IntPtr h);
		[DllImport("QTMLClient.dll")]
		public extern static IntPtr GetMovieIndTrackType(/*Movie*/IntPtr theMovie, int index, /*OSType*/uint trackType, int flags);
		[DllImport("QTMLClient.dll")]
		public extern static /*ComponentInstance*/IntPtr OpenDefaultComponent(/*OSType*/uint componentType, /*OSType*/uint componentSubType);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short CloseComponent(/*ComponentInstance*/IntPtr aComponentInstance);
		[DllImport("QTMLClient.dll")]
		public extern static void GetMediaSampleDescription(/*Media*/IntPtr theMedia, int index, /*SampleDescriptionHandle*/IntPtr descH);
		[DllImport("QTMLClient.dll")]
		public extern static /*Media*/IntPtr GetTrackMedia(/*Track*/IntPtr theTrack);
		[DllImport("QTMLClient.dll")]
		public extern static /*ComponentResult*/IntPtr MovieExportSetSampleDescription(/*MovieExportComponent*/IntPtr ci, /*SampleDescriptionHandle*/IntPtr desc, /*OSType*/uint mediaType);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSErr*/short PutMovieIntoTypedHandle(/*Movie*/IntPtr theMovie, /*Track*/IntPtr targetTrack, /*OSType*/uint handleType, /*Handle*/IntPtr publicMovie, /*TimeValue*/int start, /*TimeValue*/int dur, int flags, /*ComponentInstance*/IntPtr userComp);
        [DllImport("QTMLClient.dll")]
        public extern static /*OSStatus*/int MovieAudioExtractionBegin(/*Movie*/IntPtr m, UInt32 flags, out /*MovieAudioExtractionRef* */IntPtr outSession);
        [DllImport("QTMLClient.dll")]
        public extern static /*OSStatus*/int MovieAudioExtractionEnd(/*MovieAudioExtractionRef* */IntPtr outSession);
        [DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionGetPropertyInfo(/*MovieAudioExtractionRef* */IntPtr session, /*QTPropertyClass*/uint inPropClass, /*QTPropertyID*/uint inPropID, out /*QTPropertyValueType**/uint outPropType, out /*ByteCount*/uint outPropValueSize, out UInt32 outPropertyFlags);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionGetPropertyInfo(/*MovieAudioExtractionRef* */IntPtr session, /*QTPropertyClass*/uint inPropClass, /*QTPropertyID*/uint inPropID, /*QTPropertyValueType**/IntPtr outPropType, out /*ByteCount*/uint outPropValueSize, /*QTPropertyValueType**/IntPtr outPropertyFlags);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionGetProperty(/*MovieAudioExtractionRef* */IntPtr session, /*QTPropertyClass*/uint inPropClass, /*QTPropertyID*/uint inPropID, /*ByteCount*/uint inPropValueSize, /*QTPropertyValuePtr*/IntPtr outPropValueAddress, out /*ByteCount**/uint outPropValueSizeUsed);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionGetProperty(/*MovieAudioExtractionRef* */IntPtr session, /*QTPropertyClass*/uint inPropClass, /*QTPropertyID*/uint inPropID, /*ByteCount*/uint inPropValueSize, /*QTPropertyValuePtr*/IntPtr outPropValueAddress, IntPtr outPropValueSizeUsed);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionSetProperty(/*MovieAudioExtractionRef* */IntPtr session, /*QTPropertyClass*/uint inPropClass, /*QTPropertyID*/uint inPropID, /*ByteCount*/uint inPropValueSize, /*ConstQTPropertyValuePtr*/IntPtr inPropValueAddress);
		[DllImport("QTMLClient.dll")]
		public extern static /*OSStatus*/int MovieAudioExtractionFillBuffer(/*MovieAudioExtractionRef* */IntPtr session, ref UInt32 ioNumFrames, IntPtr ioData, out UInt32 outFlags);

#endregion
#region constants
		public static short noErr = 0;

		public static short nextTimeMediaSample = 1;
		public static short nextTimeEdgeOK = 16384;

		public static short movieTrackMediaType = 1;
		public static short movieTrackEnabledOnly = 4;

		public static uint VIDEO_TYPE = FOUR_CHAR_CODE("vide");//1986618469;
		public static uint SOUND_TYPE = FOUR_CHAR_CODE("soun");//1936684398;
		public static uint k8BitOffsetBinaryFormat = FOUR_CHAR_CODE("raw ");
		public static uint k16BitBigEndianFormat = FOUR_CHAR_CODE("twos");
		public static uint k32BGRAPixelFormat = FOUR_CHAR_CODE("BGRA");

		/*
		* 
		* QTMovieAudioExtractionAudioPropertyID_AudioStreamBasicDescription:
		* Value is AudioStreamBasicDescription (get any time, set before
		* first MovieAudioExtractionFillBuffer call) If you get this
		* property immediately after beginning an audio extraction session,
		* it will tell you the default extraction format for the movie. 
		* This will include the number of channels in the default movie mix.
		* If you set the output AudioStreamBasicDescription, it is
		* recommended that you also set the output channel layout.  If your
		* output ASBD has a different number of channels that the default
		* extraction mix, you _must_ set the output channel layout. You can
		* only set PCM output formats.  Setting a compressed output format
		* will fail.
		*/
		public static uint kQTMovieAudioExtractionAudioPropertyID_AudioStreamBasicDescription = FOUR_CHAR_CODE("asbd");

		/*
		* kQTMovieAudioExtractionAudioPropertyID_AudioChannelLayout: Value
		* is AudioChannelLayout (get any time, set before first
		* MovieAudioExtractionFillBuffer call) If you get this property
		* immediately after beginning an audio extraction session, it will
		* tell you what the channel layout is for the default extraction mix.
		*/
		public static uint kQTMovieAudioExtractionAudioPropertyID_AudioChannelLayout = FOUR_CHAR_CODE("clay");

		public static uint kAudioFormatFlagIsFloat = (1 << 0);
		public static uint kAudioFormatFlagIsBigEndian = (1 << 1);
		public static uint kAudioFormatFlagIsSignedInteger = (1 << 2);
		public static uint kAudioFormatFlagIsPacked = (1 << 3);
		public static uint kAudioFormatFlagIsAlignedHigh = (1 << 4);
		public static uint kAudioFormatFlagIsNonInterleaved = (1 << 5);
		public static uint kAudioFormatFlagIsNonMixable = (1 << 6);

		public static uint kAudioFormatFlagsAreAllClear
		{
			get
			{
				unchecked
				{
					return (uint)(1 << 31);
				}
			}
		}
		

		public static uint kLinearPCMFormatFlagIsFloat = kAudioFormatFlagIsFloat;
		public static uint kLinearPCMFormatFlagIsBigEndian = kAudioFormatFlagIsBigEndian;
		public static uint kLinearPCMFormatFlagIsSignedInteger = kAudioFormatFlagIsSignedInteger;
		public static uint kLinearPCMFormatFlagIsPacked = kAudioFormatFlagIsPacked;
		public static uint kLinearPCMFormatFlagIsAlignedHigh = kAudioFormatFlagIsAlignedHigh;
		public static uint kLinearPCMFormatFlagIsNonInterleaved = kAudioFormatFlagIsNonInterleaved;
		public static uint kLinearPCMFormatFlagIsNonMixable = kAudioFormatFlagIsNonMixable;
		public static uint kLinearPCMFormatFlagsAreAllClear = kAudioFormatFlagsAreAllClear;

		public static uint kAppleLosslessFormatFlag_16BitSourceData = 1;
		public static uint kAppleLosslessFormatFlag_20BitSourceData = 2;
		public static uint kAppleLosslessFormatFlag_24BitSourceData = 3;
		public static uint kAppleLosslessFormatFlag_32BitSourceData = 4;

		public static uint kAudioFormatFlagsNativeEndian = 0;

		public static uint kQTPropertyClass_MovieAudioExtraction_Movie = FOUR_CHAR_CODE("xmov");
		public static uint kQTPropertyClass_MovieAudioExtraction_Audio = FOUR_CHAR_CODE("xaud");

		public static uint kQTMovieAudioExtractionComplete = (1 << 0);

		#region layout tags
		// Some channel abbreviations used below:
		// L - left
		// R - right
		// C - center
		// Ls - left surround
		// Rs - right surround
		// Cs - center surround
		// Rls - rear left surround
		// Rrs - rear right surround
		// Lw - left wide
		// Rw - right wide
		// Lsd - left surround direct
		// Rsd - right surround direct
		// Lc - left center
		// Rc - right center
		// Ts - top surround
		// Vhl - vertical height left
		// Vhc - vertical height center
		// Vhr - vertical height right
		// Lt - left matrix total. for matrix encoded stereo.
		// Rt - right matrix total. for matrix encoded stereo.
		public static uint AudioChannelLayoutTag_UseChannelDescriptions = (0 << 16) | 0; // use the array of AudioChannelDescriptions to define the mapping.
		public static uint kAudioChannelLayoutTag_UseChannelBitmap = (1<<16) | 0; // use the bitmap to define the mapping.
		public static uint kAudioChannelLayoutTag_Mono = (100<<16) | 1; // a standard mono stream
		public static uint kAudioChannelLayoutTag_Stereo = (101<<16) | 2; // a standard stereo stream (L R) - implied playback
		public static uint kAudioChannelLayoutTag_StereoHeadphones = (102<<16) | 2; // a standard stereo stream (L R) - implied headphone playbac
		public static uint kAudioChannelLayoutTag_MatrixStereo = (103<<16) | 2; // a matrix encoded stereo stream (Lt; Rt)
		public static uint kAudioChannelLayoutTag_MidSide = (104<<16) | 2; // mid/side recording
		public static uint kAudioChannelLayoutTag_XY = (105<<16) | 2; // coincident mic pair (often 2 figure 8"s)
		public static uint kAudioChannelLayoutTag_Binaural = (106<<16) | 2; // binaural stereo (left; right)
		public static uint kAudioChannelLayoutTag_Ambisonic_B_Format = (107<<16) | 4; // W; X; Y; Z
		public static uint kAudioChannelLayoutTag_Quadraphonic = (108<<16) | 4; // front left; front right; back left; back right
		public static uint kAudioChannelLayoutTag_Pentagonal = (109<<16) | 5; // left; right; rear left; rear right; center
		public static uint kAudioChannelLayoutTag_Hexagonal = (110<<16) | 6; // left; right; rear left; rear right; center; rear
		public static uint kAudioChannelLayoutTag_Octagonal = (111<<16) | 8; // front left; front right; rear left; rear right; front center; rear center; side left; side right
		public static uint kAudioChannelLayoutTag_Cube = (112<<16) | 8; // left; right; rear left; rear right; top left; top right; top rear left; top rear right
		// MPEG defined layouts
		public static uint kAudioChannelLayoutTag_MPEG_1_0 = kAudioChannelLayoutTag_Mono; // C
		public static uint kAudioChannelLayoutTag_MPEG_2_0 = kAudioChannelLayoutTag_Stereo; // L R
		public static uint kAudioChannelLayoutTag_MPEG_3_0_A = (113<<16) | 3; // L R C
		public static uint kAudioChannelLayoutTag_MPEG_3_0_B = (114<<16) | 3; // C L R
		public static uint kAudioChannelLayoutTag_MPEG_4_0_A = (115<<16) | 4; // L R C Cs
		public static uint kAudioChannelLayoutTag_MPEG_4_0_B = (116<<16) | 4; // C L R Cs
		public static uint kAudioChannelLayoutTag_MPEG_5_0_A = (117<<16) | 5; // L R C Ls Rs
		public static uint kAudioChannelLayoutTag_MPEG_5_0_B = (118<<16) | 5; // L R Ls Rs C
		public static uint kAudioChannelLayoutTag_MPEG_5_0_C = (119<<16) | 5; // L C R Ls Rs
		public static uint kAudioChannelLayoutTag_MPEG_5_0_D = (120<<16) | 5; // C L R Ls Rs
		public static uint kAudioChannelLayoutTag_MPEG_5_1_A = (121<<16) | 6; // L R C LFE Ls Rs
		public static uint kAudioChannelLayoutTag_MPEG_5_1_B = (122<<16) | 6; // L R Ls Rs C LFE
		public static uint kAudioChannelLayoutTag_MPEG_5_1_C = (123<<16) | 6; // L C R Ls Rs LFE
		public static uint kAudioChannelLayoutTag_MPEG_5_1_D = (124<<16) | 6; // C L R Ls Rs LFE
		public static uint kAudioChannelLayoutTag_MPEG_6_1_A = (125<<16) | 7; // L R C LFE Ls Rs Cs
		public static uint kAudioChannelLayoutTag_MPEG_7_1_A = (126<<16) | 8; // L R C LFE Ls Rs Lc Rc
		public static uint kAudioChannelLayoutTag_MPEG_7_1_B = (127<<16) | 8; // C Lc Rc L R Ls Rs LFE (doc: IS-13818-7 MPEG2-AAC Table 3.1)
		public static uint kAudioChannelLayoutTag_MPEG_7_1_C = (128<<16) | 8; // L R C LFE Ls R Rls Rrs
		public static uint kAudioChannelLayoutTag_Emagic_Default_7_1 = (129<<16) | 8; // L R Ls Rs C LFE Lc Rc
		public static uint kAudioChannelLayoutTag_SMPTE_DTV = (130<<16) | 8; // L R C LFE Ls Rs Lt Rt (kAudioChannelLayoutTag_ITU_5_1 plus a matrix encoded stereo mix)
		// ITU defined layouts
		public static uint kAudioChannelLayoutTag_ITU_1_0 = kAudioChannelLayoutTag_Mono; // C
		public static uint kAudioChannelLayoutTag_ITU_2_0 = kAudioChannelLayoutTag_Stereo; // L R
		public static uint kAudioChannelLayoutTag_ITU_2_1 = (131<<16) | 3; // L R Cs
		public static uint kAudioChannelLayoutTag_ITU_2_2 = (132<<16) | 4; // L R Ls Rs
		public static uint kAudioChannelLayoutTag_ITU_3_0 = kAudioChannelLayoutTag_MPEG_3_0_A; // L R C
		public static uint kAudioChannelLayoutTag_ITU_3_1 = kAudioChannelLayoutTag_MPEG_4_0_A; // L R C Cs
		public static uint kAudioChannelLayoutTag_ITU_3_2 = kAudioChannelLayoutTag_MPEG_5_0_A; // L R C Ls Rs
		public static uint kAudioChannelLayoutTag_ITU_3_2_1 = kAudioChannelLayoutTag_MPEG_5_1_A; // L R C LFE Ls Rs
		public static uint kAudioChannelLayoutTag_ITU_3_4_1 = kAudioChannelLayoutTag_MPEG_7_1_C; // L R C LFE Ls Rs Rls Rrs
		// DVD defined layouts
		public static uint kAudioChannelLayoutTag_DVD_0 = kAudioChannelLayoutTag_Mono; // C (mono)
		public static uint kAudioChannelLayoutTag_DVD_1 = kAudioChannelLayoutTag_Stereo; // L R
		public static uint kAudioChannelLayoutTag_DVD_2 = kAudioChannelLayoutTag_ITU_2_1; // L R Cs
		public static uint kAudioChannelLayoutTag_DVD_3 = kAudioChannelLayoutTag_ITU_2_2; // L R Ls Rs
		public static uint kAudioChannelLayoutTag_DVD_4 = (133<<16) | 3; // L R LFE
		public static uint kAudioChannelLayoutTag_DVD_5 = (134<<16) | 4; // L R LFE Cs
		public static uint kAudioChannelLayoutTag_DVD_6 = (135<<16) | 5; // L R LFE Ls Rs
		public static uint kAudioChannelLayoutTag_DVD_7 = kAudioChannelLayoutTag_MPEG_3_0_A; // L R C
		public static uint kAudioChannelLayoutTag_DVD_8 = kAudioChannelLayoutTag_MPEG_4_0_A; // L R C Cs
		public static uint kAudioChannelLayoutTag_DVD_9 = kAudioChannelLayoutTag_MPEG_5_0_A; // L R C Ls Rs
		public static uint kAudioChannelLayoutTag_DVD_10 = (136<<16) | 4; // L R C LFE
		public static uint kAudioChannelLayoutTag_DVD_11 = (137<<16) | 5; // L R C LFE Cs
		public static uint kAudioChannelLayoutTag_DVD_12 = kAudioChannelLayoutTag_MPEG_5_1_A; // L R C LFE Ls Rs
		// 13 through 17 are duplicates of 8 through 12.
		public static uint kAudioChannelLayoutTag_DVD_13 = kAudioChannelLayoutTag_DVD_8; // L R C Cs
		public static uint kAudioChannelLayoutTag_DVD_14 = kAudioChannelLayoutTag_DVD_9; // L R C Ls Rs
		public static uint kAudioChannelLayoutTag_DVD_15 = kAudioChannelLayoutTag_DVD_10; // L R C LFE
		public static uint kAudioChannelLayoutTag_DVD_16 = kAudioChannelLayoutTag_DVD_11; // L R C LFE Cs
		public static uint kAudioChannelLayoutTag_DVD_17 = kAudioChannelLayoutTag_DVD_12; // L R C LFE Ls Rs
		public static uint kAudioChannelLayoutTag_DVD_18 = (138<<16) | 5; // L R Ls Rs LFE
		public static uint kAudioChannelLayoutTag_DVD_19 = kAudioChannelLayoutTag_MPEG_5_0_B; // L R Ls Rs C
		public static uint kAudioChannelLayoutTag_DVD_20 = kAudioChannelLayoutTag_MPEG_5_1_B; // L R Ls Rs C LFE
		// These layouts are recommended for AudioUnit usage
		// These are the symmetrical layouts
		public static uint kAudioChannelLayoutTag_AudioUnit_4 = kAudioChannelLayoutTag_Quadraphonic;
		public static uint kAudioChannelLayoutTag_AudioUnit_5 = kAudioChannelLayoutTag_Pentagonal;
		public static uint kAudioChannelLayoutTag_AudioUnit_6 = kAudioChannelLayoutTag_Hexagonal;
		public static uint kAudioChannelLayoutTag_AudioUnit_8 = kAudioChannelLayoutTag_Octagonal;
		// These are the surround-based layouts
		public static uint kAudioChannelLayoutTag_AudioUnit_5_0 = kAudioChannelLayoutTag_MPEG_5_0_B; // L R Ls Rs C
		public static uint kAudioChannelLayoutTag_AudioUnit_6_0 = (139<<16) | 6; // L R Ls Rs C Cs
		public static uint kAudioChannelLayoutTag_AudioUnit_7_0 = (140<<16) | 7; // L R Ls Rs C Rls Rrs
		public static uint kAudioChannelLayoutTag_AudioUnit_5_1 = kAudioChannelLayoutTag_MPEG_5_1_A; // L R C LFE Ls Rs
		public static uint kAudioChannelLayoutTag_AudioUnit_6_1 = kAudioChannelLayoutTag_MPEG_6_1_A; // L R C LFE Ls Rs Cs
		public static uint kAudioChannelLayoutTag_AudioUnit_7_1 = kAudioChannelLayoutTag_MPEG_7_1_C; // L R C LFE Ls Rs Rls Rrs
		public static uint kAudioChannelLayoutTag_AAC_Quadraphonic = kAudioChannelLayoutTag_Quadraphonic; // L R Ls Rs
		public static uint kAudioChannelLayoutTag_AAC_4_0 = kAudioChannelLayoutTag_MPEG_4_0_B; // C L R Cs
		public static uint kAudioChannelLayoutTag_AAC_5_0 = kAudioChannelLayoutTag_MPEG_5_0_D; // C L R Ls Rs
		public static uint kAudioChannelLayoutTag_AAC_5_1 = kAudioChannelLayoutTag_MPEG_5_1_D; // C L R Ls Rs Lfe
		public static uint kAudioChannelLayoutTag_AAC_6_0 = (141<<16) | 6; // C L R Ls Rs Cs
		public static uint kAudioChannelLayoutTag_AAC_6_1 = (142<<16) | 7; // C L R Ls Rs Cs Lfe
		public static uint kAudioChannelLayoutTag_AAC_7_0 = (143<<16) | 7; // C L R Ls Rs Rls Rrs
		public static uint kAudioChannelLayoutTag_AAC_7_1 = kAudioChannelLayoutTag_MPEG_7_1_B; // C Lc Rc L R Ls Rs Lfe
		public static uint kAudioChannelLayoutTag_AAC_Octagonal = (144<<16) | 8; // C L R Ls Rs Rls Rrs Cs
		public static uint kAudioChannelLayoutTag_TMH_10_2_std = (145<<16) | 16; // L R C Vhc Lsd Rsd Ls Rs Vhl Vhr Lw Rw Csd Cs LFE1 LFE2
		public static uint kAudioChannelLayoutTag_TMH_10_2_full = (146<<16) | 21; // TMH_10_2_std plus: Lc Rc HI VI Haptic
		public static uint kAudioChannelLayoutTag_DiscreteInOrder = (147 << 16) | 0; // needs to be ORed with the actual number of channels 
		#endregion
		#region channel labels
		/*!
			@enum AudioChannelLabel Constants
			@abstract These constants are for use in the mChannelLabel field of an
							AudioChannelDescription structure.
			@discussion These channel labels attempt to list all labels in common use. Due to the
							ambiguities in channel labeling by various groups; there may be some overlap or
							duplication in the labels below. Use the label which most clearly describes what
							you mean.

							WAVE files seem to follow the USB spec for the channel flags. A channel map lets
							you put these channels in any order; however a WAVE file only supports labels
							1-18 and if present, they must be in the order given below. The integer values
							for the labels below match the bit position of the label in the USB bitmap and
							thus also the WAVE file bitmap.
		*/
		public static uint kAudioChannelLabel_Unknown = 0xFFFFFFFF; // unknown or unspecified other use
		public static uint kAudioChannelLabel_Unused = 0; // channel is present; but has no intended use or destination
		public static uint kAudioChannelLabel_UseCoordinates = 100; // channel is described by the mCoordinates fields.

		public static uint kAudioChannelLabel_Left = 1;
		public static uint kAudioChannelLabel_Right = 2;
		public static uint kAudioChannelLabel_Center = 3;
		public static uint kAudioChannelLabel_LFEScreen = 4;
		public static uint kAudioChannelLabel_LeftSurround = 5; // WAVE: "Back Left"
		public static uint kAudioChannelLabel_RightSurround = 6; // WAVE: "Back Right"
		public static uint kAudioChannelLabel_LeftCenter = 7;
		public static uint kAudioChannelLabel_RightCenter = 8;
		public static uint kAudioChannelLabel_CenterSurround = 9; // WAVE: "Back Center" or plain "Rear Surround"
		public static uint kAudioChannelLabel_LeftSurroundDirect = 10; // WAVE: "Side Left"
		public static uint kAudioChannelLabel_RightSurroundDirect = 11; // WAVE: "Side Right"
		public static uint kAudioChannelLabel_TopCenterSurround = 12;
		public static uint kAudioChannelLabel_VerticalHeightLeft = 13; // WAVE: "Top Front Left"
		public static uint kAudioChannelLabel_VerticalHeightCenter = 14; // WAVE: "Top Front Center"
		public static uint kAudioChannelLabel_VerticalHeightRight = 15; // WAVE: "Top Front Right"

		public static uint kAudioChannelLabel_TopBackLeft = 16;
		public static uint kAudioChannelLabel_TopBackCenter = 17;
		public static uint kAudioChannelLabel_TopBackRight = 18;

		public static uint kAudioChannelLabel_RearSurroundLeft = 33;
		public static uint kAudioChannelLabel_RearSurroundRight = 34;
		public static uint kAudioChannelLabel_LeftWide = 35;
		public static uint kAudioChannelLabel_RightWide = 36;
		public static uint kAudioChannelLabel_LFE2 = 37;
		public static uint kAudioChannelLabel_LeftTotal = 38; // matrix encoded 4 channels
		public static uint kAudioChannelLabel_RightTotal = 39; // matrix encoded 4 channels
		public static uint kAudioChannelLabel_HearingImpaired = 40;
		public static uint kAudioChannelLabel_Narration = 41;
		public static uint kAudioChannelLabel_Mono = 42;
		public static uint kAudioChannelLabel_DialogCentricMix = 43;

		public static uint kAudioChannelLabel_CenterSurroundDirect = 44; // back center; non diffuse

		// first order ambisonic channels
		public static uint kAudioChannelLabel_Ambisonic_W = 200;
		public static uint kAudioChannelLabel_Ambisonic_X = 201;
		public static uint kAudioChannelLabel_Ambisonic_Y = 202;
		public static uint kAudioChannelLabel_Ambisonic_Z = 203;

		// Mid/Side Recording
		public static uint kAudioChannelLabel_MS_Mid = 204;
		public static uint kAudioChannelLabel_MS_Side = 205;

		// X-Y Recording
		public static uint kAudioChannelLabel_XY_X = 206;
		public static uint kAudioChannelLabel_XY_Y = 207;

		// other
		public static uint kAudioChannelLabel_HeadphonesLeft = 301;
		public static uint kAudioChannelLabel_HeadphonesRight = 302;
		public static uint kAudioChannelLabel_ClickTrack = 304;
		public static uint kAudioChannelLabel_ForeignLanguage = 305;

		// generic discrete channel
		public static uint kAudioChannelLabel_Discrete = 400;

		// numbered discrete channel
		public static uint kAudioChannelLabel_Discrete_0 = (1<<16) | 0;
		public static uint kAudioChannelLabel_Discrete_1 = (1<<16) | 1;
		public static uint kAudioChannelLabel_Discrete_2 = (1<<16) | 2;
		public static uint kAudioChannelLabel_Discrete_3 = (1<<16) | 3;
		public static uint kAudioChannelLabel_Discrete_4 = (1<<16) | 4;
		public static uint kAudioChannelLabel_Discrete_5 = (1<<16) | 5;
		public static uint kAudioChannelLabel_Discrete_6 = (1<<16) | 6;
		public static uint kAudioChannelLabel_Discrete_7 = (1<<16) | 7;
		public static uint kAudioChannelLabel_Discrete_8 = (1<<16) | 8;
		public static uint kAudioChannelLabel_Discrete_9 = (1<<16) | 9;
		public static uint kAudioChannelLabel_Discrete_10 = (1<<16) | 10;
		public static uint kAudioChannelLabel_Discrete_11 = (1<<16) | 11;
		public static uint kAudioChannelLabel_Discrete_12 = (1<<16) | 12;
		public static uint kAudioChannelLabel_Discrete_13 = (1<<16) | 13;
		public static uint kAudioChannelLabel_Discrete_14 = (1<<16) | 14;
		public static uint kAudioChannelLabel_Discrete_15 = (1<<16) | 15;
		public static uint kAudioChannelLabel_Discrete_65535 = (1 << 16) | 65535;
		#endregion

		/*
		* kQTMovieAudioExtractionMoviePropertyID_CurrentTime: Value is
		* TimeRecord (set & get) When setting, set the timescale to anything
		* you want (output audio sample rate, movie timescale) When getting,
		* the timescale will be output audio sample rate for best accuracy.
		*/
		public static uint kQTMovieAudioExtractionMoviePropertyID_CurrentTime = FOUR_CHAR_CODE("time"); /* value is TimeRecord. Gettable/Settable.*/

		/*
		* kQTMovieAudioExtractionMoviePropertyID_AllChannelsDiscrete: Value
		* is Boolean (set & get) Set to implement export of all audio
		* channels without mixing. When this is set and the extraction asbd
		* or channel layout are read back, you will get information relating
		* to the re-mapped movie.
		*/
		public static uint kQTMovieAudioExtractionMoviePropertyID_AllChannelsDiscrete = FOUR_CHAR_CODE("disc"); /* value is Boolean. Gettable/Settable.*/

		/*
		* kQTMovieAudioExtractionAudioPropertyID_RenderQuality: Value is
		* UInt32 (set & get) Set the render quality to be used for this
		* audio extraction session. UInt32 values are as defined in
		* <AudioUnit/AudioUnitProperties.h> and vary from 0x00
		* (kRenderQuality_Min) to 0x7F (kRenderQuality_Max). We also define
		* a special value (kQTAudioRenderQuality_PlaybackDefault) which
		* resets the quality settings to the same values that were chosen by
		* default for playback.
		*/
		public static uint kQTMovieAudioExtractionAudioPropertyID_RenderQuality = FOUR_CHAR_CODE("qual"); /* value is UInt32. Gettable/Settable.*/



		public static uint VideoMediaType = VIDEO_TYPE;
		public static uint SoundMediaType = SOUND_TYPE;
#endregion
#region structures
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct Point
		{
			public short v;
			public short h;
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct Rect
		{
			public short top;
			public short left;
			public short bottom;
			public short right;
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct ColorSpec
		{
			public short value;                  /*index or other value*/
			public RGBColor rgb;                    /*true color*/
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct RGBColor
		{
			public ushort red;                    /*magnitude of red component*/
			public ushort green;                  /*magnitude of green component*/
			public ushort blue;                   /*magnitude of blue component*/
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct ColorTable
		{
			public long ctSeed;                 /*unique identifier for table*/
			public short ctFlags;                /*high bit: 0 = PixMap; 1 = device*/
			public short ctSize;                 /*number of entries in CTTable*/
			public ColorSpec[] ctTable;                /*array [0..0] of ColorSpec*/
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct PixMap
		{
			public IntPtr baseAddr;               /*pointer to pixels*/
			public short rowBytes;               /*offset to next line*/
			public Rect bounds;                 /*encloses bitmap*/
			public short pmVersion;              /*pixMap version number*/
			public short packType;               /*defines packing format*/
			public int packSize;               /*length of pixel data*/
			public int hRes;                   /*horiz. resolution (ppi)*/
			public int vRes;                   /*vert. resolution (ppi)*/
			public short pixelType;              /*defines pixel type*/
			public short pixelSize;              /*# bits in pixel*/
			public short cmpCount;               /*# components in pixel*/
			public short cmpSize;                /*# bits per component*/
			public uint pixelFormat;                /*fourCharCode representation*/
			public IntPtr pmTable;                    /*color map for this pixMap*/
			public IntPtr pmExt;                      /*Handle to pixMap extension*/
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct CGrafPort
		{
			public short device;                 /* not available in Carbon*/
			public IntPtr portPixMap;             /* in Carbon use GetPortPixMap*/
			public short portVersion;            /* in Carbon use IsPortColor*/
			public IntPtr grafVars;               /* not available in Carbon*/
			public short chExtra;                /* in Carbon use GetPortChExtra*/
			public short pnLocHFrac;             /* in Carbon use Get/SetPortFracHPenLocation*/
			public Rect portRect;               /* in Carbon use Get/SetPortBounds*/
			public IntPtr visRgn;                 /* in Carbon use Get/SetPortVisibleRegion*/
			public IntPtr clipRgn;                /* in Carbon use Get/SetPortClipRegion*/
			public IntPtr bkPixPat;               /* in Carbon use GetPortBackPixPat or BackPixPat*/
			public RGBColor rgbFgColor;             /* in Carbon use GetPortForeColor or RGBForeColor*/
			public RGBColor rgbBkColor;             /* in Carbon use GetPortBackColor or RGBBackColor*/
			public Point pnLoc;                  /* in Carbon use GetPortPenLocation or MoveTo*/
			public Point pnSize;                 /* in Carbon use Get/SetPortPenSize*/
			public short pnMode;                 /* in Carbon use Get/SetPortPenMode*/
			public IntPtr pnPixPat;               /* in Carbon use Get/SetPortPenPixPat*/
			public IntPtr fillPixPat;             /* in Carbon use GetPortFillPixPat*/
			public short pnVis;                  /* in Carbon use GetPortPenVisibility or Show/HidePen*/
			public short txFont;                 /* in Carbon use GetPortTextFont or TextFont*/
			public char txFace;                 /* in Carbon use GetPortTextFace or TextFace*/
			/*StyleField occupies 16-bits, but only first 8-bits are used*/
			public short txMode;                 /* in Carbon use GetPortTextMode or TextMode*/
			public short txSize;                 /* in Carbon use GetPortTextSize or TextSize*/
			public int spExtra;                /* in Carbon use GetPortSpExtra or SpaceExtra*/
			public int fgColor;                /* not available in Carbon*/
			public int bkColor;                /* not available in Carbon*/
			public short colrBit;                /* not available in Carbon*/
			public short patStretch;             /* not available in Carbon*/
			public IntPtr picSave;                /* in Carbon use IsPortPictureBeingDefined*/
			public IntPtr rgnSave;                /* in Carbon use IsPortRegionBeingDefined*/
			public IntPtr polySave;               /* in Carbon use IsPortPolyBeingDefined*/
			public IntPtr grafProcs;              /* in Carbon use Get/SetPortGrafProcs*/
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct FSSpec
		{
			short vRefNum;
			int parID;
			byte[] name;
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct SoundDescription
		{
			public int descSize;               /* total size of SoundDescription including extra data */
			public int dataFormat;             /* sound format */
			public int resvd1;                 /* reserved for apple use. set to zero */
			public short resvd2;                 /* reserved for apple use. set to zero */
			public short dataRefIndex;
			public short version;                /* which version is this data */
			public short revlevel;               /* what version of that codec did this */
			public int vendor;                 /* whose  codec compressed this data */
			public short numChannels;            /* number of channels of sound */
			public short sampleSize;             /* number of bits per sample */
			public short compressionID;          /* unused. set to zero. */
			public short packetSize;             /* unused. set to zero. */
			public /*UnsignedFixed*/uint sampleRate;             /* sample rate sound is captured at */
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct SampleDescription
		{
			public int descSize;
			public int dataFormat;
			public int resvd1;
			public short resvd2;
			public short dataRefIndex;
		};
		public class Float80
		{
			short  exp;
			ushort[]  man; //dimension 4
			public Float80()
			{
				exp = 0;
				man = new ushort[4];
			}
			public Float80(IntPtr pPtr)
				: this()
			{
				if (pPtr == IntPtr.Zero)
					return;
				exp = Marshal.ReadInt16(pPtr);
				pPtr = (IntPtr)(pPtr.ToInt32() + 2);
				for (int nManIndx = 0; man.Length > nManIndx; nManIndx++)
				{
					man[nManIndx] = (ushort)Marshal.ReadInt16(pPtr);
					pPtr = (IntPtr)(pPtr.ToInt32() + 2);
				}
			}
		};
		public class extended80 : Float80
		{
			public extended80(IntPtr pPtr)
				: base(pPtr)
			{
			}
		};
		public struct SndCommand
		{
		  public ushort cmd;
		  public short param1;
		  public int param2;
		};
		public struct ModRef
		{
		  public ushort modNumber;
		  public int modInit;
		};
		public class SndListResource
		{
			public short format;
			public short numModifiers;
			public ModRef[] modifierPart;
			public short numCommands;
			public SndCommand[] commandPart;
			public SoundHeaderType enSoundHeaderType;
			public SoundHeader dataPart;
			public SndListResource()
			{
				format = 0;
				numModifiers = 0;
				modifierPart = null;
				numCommands = 0;
				commandPart = null;
				enSoundHeaderType = SoundHeaderType.Std;
				dataPart = null;
			}
			public SndListResource(IntPtr pSLR)
				: this()
			{
				if (pSLR == IntPtr.Zero)
					return;
				format = Marshal.ReadInt16(pSLR);
				pSLR = (IntPtr)(pSLR.ToInt32() + 2);
				numModifiers = Marshal.ReadInt16(pSLR);
				pSLR = (IntPtr)(pSLR.ToInt32() + 2);
				modifierPart = new QuickTimeAPI.ModRef[numModifiers];
				for (int nModifierIndx = 0; numModifiers > nModifierIndx; nModifierIndx++)
				{
					modifierPart[nModifierIndx].modNumber = (ushort)Marshal.ReadInt16(pSLR);
					pSLR = (IntPtr)(pSLR.ToInt32() + 2);
					modifierPart[nModifierIndx].modInit = Marshal.ReadInt32(pSLR);
					pSLR = (IntPtr)(pSLR.ToInt32() + 4);
				}
				numCommands = Marshal.ReadInt16(pSLR);
				pSLR = (IntPtr)(pSLR.ToInt32() + 2);
				commandPart = new SndCommand[numCommands];
				for (int nCommandIndx = 0; numCommands > nCommandIndx; nCommandIndx++)
				{
					commandPart[nCommandIndx].cmd = (ushort)Marshal.ReadInt16(pSLR);
					pSLR = (IntPtr)(pSLR.ToInt32() + 2);
					commandPart[nCommandIndx].param1 = Marshal.ReadInt16(pSLR);
					pSLR = (IntPtr)(pSLR.ToInt32() + 2);
					commandPart[nCommandIndx].param2 = Marshal.ReadInt32(pSLR);
					pSLR = (IntPtr)(pSLR.ToInt32() + 4);
				}
				enSoundHeaderType = SoundHeader.GetHeaderEncode(pSLR);
				switch(enSoundHeaderType)
				{
					case SoundHeaderType.Std:
						dataPart = new SoundHeader(pSLR);
						break;
					case SoundHeaderType.Cmp:
						dataPart = new CmpSoundHeader(pSLR);
						break;
					case SoundHeaderType.Ext:
						dataPart = new ExtSoundHeader(pSLR);
						break;
				}
			}
		};
		public enum SoundHeaderType
		{
			Std,
			Cmp,
			Ext
		}
		public class SoundHeader
		{
			public IntPtr samplePtr;              /*if NULL then samples are in sampleArea*/
			public uint length;                 /*length of sound in bytes*/
			public uint sampleRate;             /*sample rate for this sound*/
			public uint loopStart;              /*start of looping portion*/
			public uint loopEnd;                /*end of looping portion*/
			public byte encode;                 /*header encoding: stdSH=0x00;extSH=0xFF;cmpSH=0xFE*/
			public byte baseFrequency;          /*baseFrequency value*/
			public byte[] sampleArea;          /*space for when samples follow directly*/
			public SoundHeader()
			{
				samplePtr = IntPtr.Zero;
				length = 0;
				sampleRate = 0;
				loopStart = 0;
				loopEnd = 0;
				encode = 0;
				baseFrequency = 0;
				sampleArea = null;
			}
			public SoundHeader(IntPtr pSH)
				: this()
			{
				if (pSH == IntPtr.Zero)
					return;
				Init(ref pSH);
				if (IntPtr.Zero == samplePtr)
				{
					sampleArea = new byte[length];
					samplePtr = pSH;
					Marshal.Copy(pSH, sampleArea, 0, (int)length);
				}
			}
			protected void Init(ref IntPtr pSH)
			{
				if (pSH == IntPtr.Zero)
					return;
				samplePtr = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				length = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				sampleRate = (uint)Marshal.ReadInt32(pSH) >> 16;
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				loopStart = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				loopEnd = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				encode = Marshal.ReadByte(pSH); //
				pSH = (IntPtr)(pSH.ToInt32() + 1);
				baseFrequency = Marshal.ReadByte(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 1);
			}
			public static SoundHeaderType GetHeaderEncode(IntPtr pSH)
			{
				switch(Marshal.ReadByte(pSH, 20))
				{
					case 0xFF:
						return SoundHeaderType.Ext;
					case 0xFE:
						return SoundHeaderType.Cmp;
				}
				return SoundHeaderType.Std;
			}
		};
		public class CmpSoundHeader : SoundHeader
		{
			new private uint length;
			public uint numChannels            /*number of channels i.e. mono = 1*/
			{
				get
				{
					return base.length;
				}
				set
				{
					base.length = value;
				}
			}
			public uint numFrames;              /*length in frames ( packetFrames or sampleFrames )*/
			public extended80 AIFFSampleRate;         /*IEEE sample rate*/
			public IntPtr markerChunk;            /*sync track*/
			public uint format;                 /*data format type, was futureUse1*/
			public uint futureUse2;             /*reserved by Apple*/
			public IntPtr stateVars;              /*pointer to State Block*/
			public IntPtr leftOverSamples;        /*used to save truncated samples between compression calls*/
			public short compressionID;          /*0 means no compression, non zero means compressionID*/
			public ushort packetSize;             /*number of bits in compressed sample packet*/
			public ushort snthID;                 /*resource ID of Sound Manager snth that contains NRT C/E*/
			public ushort sampleSize;             /*number of bits in non-compressed sample*/
			public CmpSoundHeader() : base()
			{
				numFrames = 0;
				AIFFSampleRate = null;
				markerChunk = IntPtr.Zero;
				format = 0;
				futureUse2 = 0;
				stateVars = IntPtr.Zero;
				leftOverSamples = IntPtr.Zero;
				compressionID = 0;
				packetSize = 0;
				snthID = 0;
				sampleSize = 0;
			}
			public CmpSoundHeader(IntPtr pSH)
				: this()
			{
				if (pSH == IntPtr.Zero)
					return;
				Init(ref pSH);
				numFrames = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				AIFFSampleRate = new extended80(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 10);
				markerChunk = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				format = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				futureUse2 = 0;
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				stateVars = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				leftOverSamples = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				compressionID = Marshal.ReadInt16(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				packetSize = (ushort)Marshal.ReadInt16(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				snthID = (ushort)Marshal.ReadInt16(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				sampleSize = (ushort)Marshal.ReadInt16(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				if (IntPtr.Zero == samplePtr)
				{
					sampleArea = new byte[numFrames * numChannels * (sampleSize / 8)];
					samplePtr = pSH;
					Marshal.Copy(pSH, sampleArea, 0, (int)(numFrames * numChannels * (sampleSize/8)));
				}
			}
		};
		public class ExtSoundHeader : SoundHeader
		{
			new private uint length;                 /*length of sound in bytes*/
			public uint numChannels            /*number of channels i.e. mono = 1*/
			{
				get
				{
					return length;
				}
				set
				{
					length = value;
				}
			}
			public uint numFrames;              /*length in frames ( packetFrames or sampleFrames )*/
			public extended80 AIFFSampleRate;         /*IEEE sample rate*/
			public IntPtr markerChunk;            /*sync track*/
			public IntPtr instrumentChunks;       /*AIFF instrument chunks*/
			public IntPtr AESRecording;
			public ushort sampleSize;             /*number of bits in sample*/
			public ushort futureUse1;             /*reserved by Apple*/
			public uint futureUse2;             /*reserved by Apple*/
			public uint futureUse3;             /*reserved by Apple*/
			public uint futureUse4;             /*reserved by Apple*/
			public ExtSoundHeader()
				: base()
			{
				numFrames = 0;
				AIFFSampleRate = null;
				markerChunk = IntPtr.Zero;
				instrumentChunks = IntPtr.Zero;
				AESRecording = IntPtr.Zero;
				sampleSize = 0;
				futureUse1 = 0;
				futureUse2 = 0;
				futureUse3 = 0;
				futureUse4 = 0;
			}
			public ExtSoundHeader(IntPtr pSH)
				: this()
			{
				if (pSH == IntPtr.Zero)
					return;
				Init(ref pSH);
				numFrames = (uint)Marshal.ReadInt32(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				AIFFSampleRate = new extended80(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 10);
				markerChunk = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				instrumentChunks = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				AESRecording = Marshal.ReadIntPtr(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				sampleSize = (ushort)Marshal.ReadInt16(pSH);
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				futureUse1 = 0;
				pSH = (IntPtr)(pSH.ToInt32() + 2);
				futureUse2 = 0;
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				futureUse3 = 0;
				pSH = (IntPtr)(pSH.ToInt32() + 4);
				futureUse4 = 0;
				pSH = (IntPtr)(pSH.ToInt32() + 4);

				if (IntPtr.Zero == samplePtr)
				{
					sampleArea = new byte[numFrames * numChannels * (sampleSize / 8)];
					samplePtr = pSH;
					Marshal.Copy(pSH, sampleArea, 0, (int)(numFrames * numChannels * (sampleSize / 8)));
				}
			}
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct AudioStreamBasicDescription
		{
			public double mSampleRate;
			public UInt32  mFormatID;
			public UInt32  mFormatFlags;
			public UInt32  mBytesPerPacket;
			public UInt32  mFramesPerPacket;
			public UInt32  mBytesPerFrame;
			public UInt32  mChannelsPerFrame;
			public UInt32  mBitsPerChannel;
			public UInt32  mReserved;
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct wide 
		{
			public UInt32 lo;
			public Int32 hi;
		};
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct TimeRecord 
		{
			public /*CompTimeValue*/wide value; /* units (duration or absolute) */
			public /*TimeScale*/int scale; /* units per second */
			public /*TimeBase*/IntPtr pBase; /* refernce to the time base */
		};
		public class AudioBufferList
		{
			private IntPtr _pABL;
			public UInt32 mNumberBuffers;
			public AudioBuffer[] mBuffers;

			public IntPtr pABL
			{
				get
				{
					if (IntPtr.Zero == _pABL)
					{
						_pABL = Marshal.AllocCoTaskMem((int)(sizeof(UInt32) + (mBuffers[0].nSize * mNumberBuffers)));
						int nOffset = 0;
						Marshal.WriteInt32(_pABL, nOffset, (int)mNumberBuffers);
						nOffset += sizeof(UInt32);
						for (ushort nIndx = 0; mNumberBuffers > nIndx; nIndx++)
						{
							Marshal.WriteInt32(_pABL, nOffset, (int)mBuffers[nIndx].mNumberChannels);
							nOffset += sizeof(UInt32);
							Marshal.WriteInt32(_pABL, nOffset, (int)mBuffers[nIndx].mDataByteSize);
							nOffset += sizeof(UInt32);
							Marshal.WriteInt32(_pABL, nOffset, (int)mBuffers[nIndx].mData);
							nOffset += sizeof(float);
						}
					}
					return _pABL;
				}
			}

			public AudioBufferList()
			{
				_pABL = IntPtr.Zero;
			}
			~AudioBufferList()
			{
				if (IntPtr.Zero != _pABL)
					Marshal.FreeCoTaskMem(_pABL);
			}
		};
		public class AudioBuffer
		{
			private IntPtr _mData;
			private uint _nSize;

			public UInt32 mNumberChannels;
			public UInt32 mDataByteSize;
			public IntPtr mData
			{
				get
				{
					if (IntPtr.Zero == _mData)
						_mData = Marshal.AllocCoTaskMem((int)mDataByteSize);
					return _mData;
				}
			}
			public uint nSize
			{
				get
				{
					if(1 > _nSize)
						_nSize = (uint)(sizeof(UInt32) * 3);
					return _nSize;
				}
			}

			public AudioBuffer()
			{
				_mData = IntPtr.Zero;
			}
			~AudioBuffer()
			{
				if (IntPtr.Zero != _mData)
					Marshal.FreeCoTaskMem(_mData);
			}
		};
		public class AudioChannelDescription
		{
			private uint _nSize;

			public UInt32 mChannelLabel;
			public UInt32 mChannelFlags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
			public float[] mCoordinates;

			public AudioChannelDescription()
			{
				_nSize = 0;
				mCoordinates = new float[3];
			}
			public uint nSize
			{
				get
				{
					if(1 > _nSize)
						_nSize = (uint)((sizeof(UInt32) * 2) + (sizeof(float) * mCoordinates.Length));
					return _nSize;
				}
			}
		};
		public class AudioChannelLayout
		{
			private IntPtr _pLayout;
			private uint _nSize;

			public UInt32 mChannelLayoutTag;
			public UInt32 mChannelBitmap;
			public UInt32 mNumberChannelDescriptions;
			//[MarshalAs(UnmanagedType.Struct)]
			public AudioChannelDescription[] mChannelDescriptions;
			public IntPtr pLayout
			{
				get
				{
					if (IntPtr.Zero == _pLayout)
					{
						_pLayout = Marshal.AllocCoTaskMem((int)nSize);
						int nOffset = 0;
						Marshal.WriteInt32(_pLayout, nOffset, (int)mChannelLayoutTag);
						nOffset += sizeof(UInt32);
						Marshal.WriteInt32(_pLayout, nOffset, (int)mChannelBitmap);
						nOffset += sizeof(UInt32);
						Marshal.WriteInt32(_pLayout, nOffset, (int)mNumberChannelDescriptions);
						nOffset += sizeof(UInt32);
						for (ushort nIndx = 0; mNumberChannelDescriptions > nIndx; nIndx++)
						{
							Marshal.WriteInt32(_pLayout, nOffset, (int)mChannelDescriptions[nIndx].mChannelLabel);
							nOffset += sizeof(UInt32);
							Marshal.WriteInt32(_pLayout, nOffset, (int)mChannelDescriptions[nIndx].mChannelFlags);
							nOffset += sizeof(UInt32);
							for (int nSubIndx = 0; mChannelDescriptions[nIndx].mCoordinates.Length > nSubIndx; nSubIndx++)
							{
								Marshal.WriteInt32(_pLayout, nOffset, (int)mChannelDescriptions[nIndx].mCoordinates[nSubIndx]);
								nOffset += sizeof(float);
							}
						}
					}
					return _pLayout;
				}
			}
			public uint nSize
			{
				get
				{
					if(1 > _nSize)
						_nSize = (uint)((sizeof(UInt32) * 3) + (mChannelDescriptions[0].nSize * mNumberChannelDescriptions));
					return _nSize;
				}
			}

			public AudioChannelLayout()
			{
				_pLayout = IntPtr.Zero;
				_nSize = 0;
			}
			~AudioChannelLayout()
			{
				if (IntPtr.Zero != _pLayout)
				{
					Marshal.FreeCoTaskMem(_pLayout);
				}
			}
		};
#endregion
#region macros
		public static uint FOUR_CHAR_CODE(string sCode)
		{
			return (uint)(((byte)sCode[3]) | ((byte)sCode[2] << 8) | ((byte)sCode[1] << 16) | ((byte)sCode[0] << 24));
		}
		public static uint AudioChannelLayoutTag_GetNumberOfChannels(uint layoutTag)
		{
			return (uint)(layoutTag & 0x0000FFFF);
		}
#endregion
	}
}
