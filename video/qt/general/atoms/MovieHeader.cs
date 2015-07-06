using System;
using System.Linq;

namespace helpers.video.qt.atoms
{
	public class MovieHeader : Atom
	{
		override protected ushort _nCapacity
		{
			get
			{
				ushort nRetVal = base._nCapacity;
				nRetVal += sizeof(byte);
				nRetVal += (ushort)(_aFlags.Length + _aReserved.Length + _aMatrixStructure.Length);
				nRetVal += sizeof(uint) * 12;
				nRetVal += sizeof(ushort);
				return nRetVal;
			}
		}
		//Version 
		//A 1-byte specification of the version of this movie header atom.
		private byte _nVersion;
		// Flags 
		//Three bytes of space for future movie header flags. 
		private byte[] _aFlags = new byte[3];
		//Creation time 
		//A 32-bit integer that specifies the calendar date and time (in seconds since midnight, January 1, 1904) when the movie atom was created. It is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nCreationTime;
		// Modification time 
		//A 32-bit integer that specifies the calendar date and time (in seconds since midnight, January 1, 1904) when the movie atom was changed. BooleanIt is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nModificationTime;
		// Time scale 
		//A time value that indicates the time scale for this movie—that is, the number of time units that pass per second in its time coordinate system. A time coordinate system that measures time in sixtieths of a second, for example, has a time scale of 60.
		private uint _nTimeScale;
		// Duration 
		//A time value that indicates the duration of the movie in time scale units. Note that this property is derived from the movie’s tracks. The value of this field corresponds to the duration of the longest track in the movie. 
		private uint _nDuration;
		//Preferred rate 
		//A 32-bit fixed-point number that specifies the rate at which to play this movie. A value of 1.0 indicates normal rate.
		private uint _nPreferredRate;
		// Preferred volume 
		//A 16-bit fixed-point number that specifies how loud to play this movie’s sound. A value of 1.0 indicates full volume.
		private ushort _nPreferredVolume;
		// Reserved 
		//Ten bytes reserved for use by Apple. Set to 0.
		private byte[] _aReserved = new byte[10];
		// Matrix structure 
		//The matrix structure associated with this movie. A matrix shows how to map points from one coordinate space into another. See “Matrices” for a discussion of how display matrices are used in QuickTime. 
		private byte[] _aMatrixStructure = new byte[36];
		//Preview time 
		//The time value in the movie at which the preview begins.
		private uint _nPreviewTime;
		// Preview duration 
		//The duration of the movie preview in movie time scale units. 
		private uint _nPreviewDuration;
		//Poster time 
		//The time value of the time of the movie poster.
		private uint _nPosterTime;
		// Selection time 
		//The time value for the start time of the current selection.
		private uint _nSelectionTime;
		// Selection duration 
		//The duration of the current selection in movie time scale units.
		private uint _nSelectionDuration;
		// Current time 
		//The time value for current time position within the movie.
		private uint _nCurrentTime;
		// Next track ID 
		//A 32-bit integer that indicates a value to use for the track ID number of the next track added to this movie. Note that 0 is not a valid track ID value.
		private uint _nNextTrackID;

		public uint nDuration
		{
			get
			{
				return (_nDuration / _nTimeScale);
			}
		}

		internal MovieHeader(Atom cAtom)
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

			#region time scale 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nTimeScale = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region duration 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nDuration = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region preferred rate 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nPreferredRate = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region preferred volume 
			nLength = sizeof(ushort);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nPreferredVolume = BitConverter.ToUInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region reserved 
				_cStream.Position += _aReserved.Length;
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
			_nPreviewTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region preview duration 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nPreviewDuration = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region poster time 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nPosterTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region selection time 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nSelectionTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region selection duration 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nSelectionDuration = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region current time 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nCurrentTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region next track ID 
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nNextTrackID = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion
		}
	}
}
