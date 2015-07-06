using System;
using System.Linq;

namespace helpers.video.qt.atoms
{
	public class TrackHeader : Atom
	{
		override protected ushort _nCapacity
		{
			get
			{
				ushort nRetVal = base._nCapacity;
				nRetVal += sizeof(byte) * 2;
				nRetVal += (ushort)(_aFlags.Length + _aMatrixStructure.Length);
				nRetVal += sizeof(uint) * 7;
				nRetVal += sizeof(short) * 3;
				nRetVal += sizeof(ushort);
				return nRetVal;
			}
		}
		//Version 
		//A 1-byte specification of the version of this track header.
		private byte _nVersion;
		// Flags 
		//Three bytes that are reserved for the track header flags. These flags indicate how the track is used in the movie. The following flags are valid (all flags are enabled when set to 1).
		//Track enabled : Indicates that the track is enabled. Flag value is 0x0001.
		//Track in movie : Indicates that the track is used in the movie. Flag value is 0x0002.
		//Track in preview : Indicates that the track is used in the movie’s preview. Flag value is 0x0004.
		//Track in poster : Indicates that the track is used in the movie’s poster. Flag value is 0x0008.
		private byte[] _aFlags = new byte[3];
		// Creation time
		//A 32-bit integer that indicates the calendar date and time (expressed in seconds since midnight, January 1, 1904) when the track header was created. It is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nCreationTime;
		// Modification time 
		//A 32-bit integer that indicates the calendar date and time (expressed in seconds since midnight, January 1, 1904) when the track header was changed. It is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nModificationTime;
		// Track ID 
		//A 32-bit integer that uniquely identifies the track. The value 0 cannot be used.
		private uint _nTrackID;
		// Reserved 
		//A 32-bit integer that is reserved for use by Apple. Set this field to 0.
		private uint _nReserved1;
		// Duration 
		//A time value that indicates the duration of this track (in the movie’s time coordinate system). Note that this property is derived from the track’s edits. The value of this field is equal to the sum of the durations of all of the track’s edits. If there is no edit list, then the duration is the sum of the sample durations, converted into the movie timescale. 
		private uint _nDuration;
		//Reserved 
		//An 8-byte value that is reserved for use by Apple. Set this field to 0.
		private byte _nReserved2;
		// Layer 
		//A 16-bit integer that indicates this track’s spatial priority in its movie. The QuickTime Movie Toolbox uses this value to determine how tracks overlay one another. Tracks with lower layer values are displayed in front of tracks with higher layer values.
		private short _nLayer;
		// Alternate group 
		//A 16-bit integer that specifies a collection of movie tracks that contain alternate data for one another. QuickTime chooses one track from the group to be used when the movie is played. The choice may be based on such considerations as playback quality, language, or the capabilities of the computer.
		private short _nAlternateGroup;
		// Volume 
		//A 16-bit fixed-point value that indicates how loudly this track’s sound is to be played. A value of 1.0 indicates normal volume.
		private ushort _nVolume;
		// Reserved 
		//A 16-bit integer that is reserved for use by Apple. Set this field to 0.
		private short _nReserved3;
		// Matrix structure 
		//The matrix structure associated with this track. See Figure 2-3 for an illustration of a matrix structure.
		private byte[] _aMatrixStructure = new byte[36];
		// Track width 
		//A 32-bit fixed-point number that specifies the width of this track in pixels. 
		private uint _nTrackWidth;
		//Track height 
		//A 32-bit fixed-point number that indicates the height of this track in pixels.
		private uint _nTrackHeight;

		internal TrackHeader(Atom cAtom)
			: base(cAtom)
		{
			byte[] aBuffer = new byte[sizeof(uint)];
			int nLength, nBytesReaded;

			#region version
			nLength = sizeof(byte);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nVersion = aBuffer[0];
			#endregion

			#region flags
			_cStream.Position += _aFlags.Length;
			#endregion

			#region creation time
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nCreationTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region modification time
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nModificationTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region track id
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nTrackID = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region reserved
			_cStream.Position += sizeof(uint);
			#endregion

			#region duration
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nDuration = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region reserved
			_cStream.Position += sizeof(byte);
			#endregion

			#region layer
			nLength = sizeof(short);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nLayer = BitConverter.ToInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region alternate group
			nLength = sizeof(short);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nAlternateGroup = BitConverter.ToInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region volume
			nLength = sizeof(ushort);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nVolume = BitConverter.ToUInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region reserved
			_cStream.Position += sizeof(short);
			#endregion

			#region matrix structure
			nLength = _aMatrixStructure.Length;
			nBytesReaded = _cStream.Read(_aMatrixStructure, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			#endregion

			#region preview time
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nTrackWidth = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region preview duration
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nTrackHeight = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion
		}
	}
}
