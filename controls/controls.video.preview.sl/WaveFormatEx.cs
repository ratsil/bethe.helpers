﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Globalization;

namespace controls.video.preview.sl
{
    public class WaveFormatEx
    {
        #region Data
        public short FormatTag { get; set; }
        public short Channels { get; set; }
        public int SamplesPerSec { get; set; }
        public int AvgBytesPerSec { get; set; }
        public short BlockAlign { get; set; }
        public short BitsPerSample { get; set; }
        public short Size { get; set; }
        public const uint SizeOf = 18;
        public byte[] ext { get; set; }
        #endregion Data

        /// <summary>
        /// Convert the data to a hex string
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            string s = "";

            s += string.Format("{0:X4}", FormatTag).ToLittleEndian();
            s += string.Format("{0:X4}", Channels).ToLittleEndian();
            s += string.Format("{0:X8}", SamplesPerSec).ToLittleEndian();
            s += string.Format("{0:X8}", AvgBytesPerSec).ToLittleEndian();
			s += string.Format("{0:X4}", BlockAlign).ToLittleEndian();
            s += string.Format("{0:X4}", BitsPerSample).ToLittleEndian();
            s += string.Format("{0:X4}", Size).ToLittleEndian();

            return s;
        }

        /// <summary>
        /// Set the data from a byte array (usually read from a file)
        /// </summary>
        /// <param name="byteArray"></param>
        public void SetFromByteArray(byte[] byteArray)
        {
            if ((byteArray.Length + 2) < SizeOf)
            {
                throw new ArgumentException("Byte array is too small");
            }

            FormatTag = BitConverter.ToInt16(byteArray, 0);
            Channels = BitConverter.ToInt16(byteArray, 2);
            SamplesPerSec = BitConverter.ToInt32(byteArray, 4);
            AvgBytesPerSec = BitConverter.ToInt32(byteArray, 8);
            BlockAlign = BitConverter.ToInt16(byteArray, 12);
            BitsPerSample = BitConverter.ToInt16(byteArray, 14);
            if (byteArray.Length >= SizeOf)
            {
                Size = BitConverter.ToInt16(byteArray, 16);
            }
            else
            {
                Size = 0;
            }

            if (byteArray.Length > WaveFormatEx.SizeOf)
            {
                ext = new byte[byteArray.Length - WaveFormatEx.SizeOf];
                Array.Copy(byteArray, (int)WaveFormatEx.SizeOf, ext, 0, ext.Length);
            }
            else
            {
                ext = null;
            }
        }

        /// <summary>
        /// Ouput the data into a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            char[] rawData = new char[18];
            BitConverter.GetBytes(FormatTag).CopyTo(rawData, 0);
            BitConverter.GetBytes(Channels).CopyTo(rawData, 2);
            BitConverter.GetBytes(SamplesPerSec).CopyTo(rawData, 4);
            BitConverter.GetBytes(AvgBytesPerSec).CopyTo(rawData, 8);
            BitConverter.GetBytes(BlockAlign).CopyTo(rawData, 12);
            BitConverter.GetBytes(BitsPerSample).CopyTo(rawData, 14);
            BitConverter.GetBytes(Size).CopyTo(rawData, 16);
            return new string(rawData);
        }

        /// <summary>
        /// Calculate the duration of audio based on the size of the buffer
        /// </summary>
        /// <param name="cbAudioDataSize"></param>
        /// <returns></returns>
        public Int64 AudioDurationFromBufferSize(UInt32 cbAudioDataSize)
        {
            if (AvgBytesPerSec == 0)
            {
                return 0;
            }

            return (Int64)(cbAudioDataSize * 10000000 / AvgBytesPerSec);
        }

        /// <summary>
        /// Calculate the buffer size necessary for a duration of audio
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Int64 BufferSizeFromAudioDuration(Int64 duration)
        {
            Int64 size = duration * AvgBytesPerSec / 10000000;
            UInt32 remainder = (UInt32)(size % BlockAlign);
            if (remainder != 0)
            {
                size += BlockAlign - remainder;
            }

            return size;
        }

        /// <summary>
        /// Validate that the Wave format is consistent.
        /// </summary>
        public void ValidateWaveFormat()
        {
            if (FormatTag != FormatPCM)
            {
                throw new ArgumentException("Only PCM format is supported");
            }

            if (Channels != 1 && Channels != 2)
            {
                throw new ArgumentException("Only 1 or 2 channels are supported");
            }

            if (BitsPerSample != 8 && BitsPerSample != 16)
            {
                throw new ArgumentException("Only 8 or 16 bit samples are supported");
            }

            if (Size != 0)
            {
                throw new ArgumentException("Size must be 0");
            }

            if (BlockAlign != Channels * (BitsPerSample / 8))
            {
                throw new ArgumentException("Block Alignment is incorrect");
            }

            if (SamplesPerSec > (UInt32.MaxValue / BlockAlign))
            {
                throw new ArgumentException("SamplesPerSec overflows");
            }

            if (AvgBytesPerSec != SamplesPerSec * BlockAlign)
            {
                throw new ArgumentException("AvgBytesPerSec is wrong");
            }
        }

        public const Int16 FormatPCM = 1;
		public const Int16 FormatIEEE = 3;
		public const Int16 FormatMP3 = 85;
	}
	/// <summary>
	/// A managed representation of the multimedia MPEGLAYER3WAVEFORMATEX 
	/// structure declared in mmreg.h.
	/// </summary>
	/// <remarks>
	/// This was designed for usage in an environment where PInvokes are not
	/// allowed.
	/// </remarks>
	public class MpegLayer3WaveFormat
	{
		/// <summary>
		/// Gets or sets the core WaveFormatEx strucutre representing the Mp3 audio data's
		/// core attributes. 
		/// </summary>
		/// <remarks>
		/// wfx.FormatTag must be WAVE_FORMAT_MPEGLAYER3 = 0x0055 = (85)
		/// wfx.Size must be >= 12
		/// </remarks>
		public WaveFormatEx WaveFormatEx { get; set; }

		/// <summary>
		/// Gets or sets the FormatTag that defines what type of waveform audio this is.
		/// </summary>
		/// <remarks>
		/// Set this to 
		/// MPEGLAYER3_ID_MPEG = 1
		/// </remarks>
		public short Id { get; set; }

		/// <summary>
		/// Gets or sets the bitrate padding mode. 
		/// This value is set in an Mp3 file to determine if padding is needed to adjust the average bitrate
		/// to meet the sampling rate.
		/// 0 = adjust as needed
		/// 1 = always pad
		/// 2 = never pad
		/// </summary>
		/// <remarks>
		/// This is different than the unmanaged version of MpegLayer3WaveFormat
		/// which has the field Flags instead of this name.
		/// </remarks>
		public int BitratePaddingMode { get; set; }

		/// <summary>
		/// Gets or sets the Block Size in bytes. For MP3 audio this is
		/// 144 * bitrate / samplingRate + padding
		/// </summary>
		public short BlockSize { get; set; }

		/// <summary>
		/// Gets or sets the number of frames per block.
		/// </summary>
		public short FramesPerBlock { get; set; }

		/// <summary>
		/// Gets or sets the encoder delay in samples.
		/// </summary>
		public short CodecDelay { get; set; }

		/// <summary>
		/// Returns a string representing the structure in little-endian 
		/// hexadecimal format.
		/// </summary>
		/// <remarks>
		/// The string generated here is intended to be passed as 
		/// CodecPrivateData for Silverlight 2's MediaStreamSource
		/// </remarks>
		/// <returns>
		/// A string representing the structure in little-endia hexadecimal
		/// format.
		/// </returns>
		public string ToHexString()
		{
			string s = WaveFormatEx.ToHexString();
			s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.Id).ToLittleEndian();
			s += string.Format(CultureInfo.InvariantCulture, "{0:X8}", this.BitratePaddingMode).ToLittleEndian();
			s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.BlockSize).ToLittleEndian();
			s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.FramesPerBlock).ToLittleEndian();
			s += string.Format(CultureInfo.InvariantCulture, "{0:X4}", this.CodecDelay).ToLittleEndian();
			return s;
		}

		/// <summary>
		/// Returns a string representing all of the fields in the object.
		/// </summary>
		/// <returns>
		/// A string representing all of the fields in the object.
		/// </returns>
		public override string ToString()
		{
			return "MPEGLAYER3 "
				+ WaveFormatEx.ToString()
				+ string.Format(
					CultureInfo.InvariantCulture,
					"ID: {0}, Flags: {1}, BlockSize: {2}, FramesPerBlock {3}, CodecDelay {4}",
					this.Id,
					this.BitratePaddingMode,
					this.BlockSize,
					this.FramesPerBlock,
					this.CodecDelay);
		}
	}

}
