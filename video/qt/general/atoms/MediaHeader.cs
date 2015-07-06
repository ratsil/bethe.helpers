using System;
using System.Linq;

namespace helpers.video.qt.atoms
{
	public class MediaHeader : Atom
	{
		override protected ushort _nCapacity
		{
			get
			{
				ushort nRetVal = base._nCapacity;
				nRetVal += sizeof(byte);
				nRetVal += (ushort)_aFlags.Length;
				nRetVal += sizeof(uint) * 4;
				nRetVal += sizeof(short) * 2;
				return nRetVal;
			}
		}
		//Version 
		//One byte that specifies the version of this header atom.
		private byte _nVersion;
		//Flags
		//Three bytes of space for media header flags. Set this field to 0.
		private byte[] _aFlags = new byte[3];
		//Creation time
		//A 32-bit integer that specifies (in seconds since midnight, January 1, 1904) when the media atom was created. It is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nCreationTime;
		//Modification time
		//A 32-bit integer that specifies (in seconds since midnight, January 1, 1904) when the media atom was changed. It is strongly recommended that this value should be specified using coordinated universal time (UTC).
		private uint _nModificationTime;
		//Time scale
		//A time value that indicates the time scale for this media—that is, the number of time units that pass per second in its time coordinate system.
		private uint _nTimeScale;
		//Duration
		//The duration of this media in units of its time scale.
		private uint _nDuration;
		//Language
		//A 16-bit integer that specifies the language code for this media. See “Language Code Values” for valid language codes.
		private short _nLanguage;
		//Quality
		//A 16-bit integer that specifies the media’s playback quality—that is, its suitability for playback in a given environment. 
		private short _nQuality;

		internal MediaHeader(Atom cAtom)
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

			#region language
			nLength = sizeof(short);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nLanguage = BitConverter.ToInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region quality
			nLength = sizeof(short);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nQuality = BitConverter.ToInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion
		}
	}
}
