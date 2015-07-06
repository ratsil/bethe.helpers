using System;
using System.IO;
using System.Linq;

namespace helpers.video.qt.atoms
{
	class QT : Atom
	{
		// Atom ID 
		//A 32-bit integer that contains the atom’s ID value. This value must be unique among its siblings. The root atom always has an atom ID value of 1.
		private uint _nID;
		// Reserved 
		//A 16-bit integer that must be set to 0.
		private ushort _nReserved1;
		// Child count 
		//A 16-bit integer that specifies the number of child atoms that an atom contains. This count only includes immediate children. If this field is set to 0, the atom is a leaf atom and only contains data.
		private ushort _nChildCount;
		// Reserved 
		//A 32-bit integer that must be set to 0.
		private uint _nReserved2;

		internal QT(Atom cAtom)
			: base(cAtom)
		{
			byte[] aBuffer = new byte[sizeof(ulong)];
			int nLength, nBytesReaded;

			#region id
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nID = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			_cStream.Position += sizeof(ushort); //_nReserved1

			#region child count
			nLength = sizeof(ushort);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nChildCount = BitConverter.ToUInt16((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			_cStream.Position += sizeof(uint); //_nReserved2
		}
	}
}
