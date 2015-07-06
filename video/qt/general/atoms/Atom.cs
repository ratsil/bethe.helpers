using System;
using System.IO;
using System.Linq;

namespace helpers.video.qt.atoms
{
	public class Atom
	{
		protected enum Type
		{
			unknown,
			clip,
			cmov,
			crgn,
			ctab,
			dinf,
			dref,
			edts = 0x65647473,
			elst = 0x656c7374,
			free,
			ftyp = 0x66747970,
			hdlr,
			kmat,
			matt,
			mdat = 0x6d646174,
			mdhd = 0x6d646864,
			mdia = 0x6d646961,
			minf,
			moov = 0x6d6f6f76,
			mvhd = 0x6d766864,
			pnot,
			prfl,
			rmra,
			sean,
			skip,
			stbl,
			stco,
			stsc,
			stsd,
			stss,
			stsz,
			stts,
			tkhd = 0x746b6864,
			trak = 0x7472616B,
			udta = 0x75647461,
			vmhd,
			wide = 0x77696465,
		}

		static public Atom Read(Stream cStream)
		{
			Atom cAtom = null;
			try
			{
				cAtom = new Atom(cStream);
				switch (cAtom.eType)
				{
					case Type.ftyp:
						return new FileTypeCompatibility(cAtom);
					case Type.wide:
						return new Wide(cAtom);
					case Type.mdat:
						return new MovieData(cAtom);
					case Type.moov:
						return new Movie(cAtom);
					case Type.mvhd:
						return new MovieHeader(cAtom);
					case Type.trak:
						return new Track(cAtom);
					case Type.tkhd:
						return new TrackHeader(cAtom);
					case Type.edts:
						return new Edit(cAtom);
					case Type.elst:
						return new Edit.List(cAtom);
					case Type.mdia:
						return new MediaHeader(cAtom);
					case Type.udta:
						return new UserData(cAtom);
				}
			}
			catch { }
			return cAtom;
		}
		protected long _nStreamPositionStart;
		protected Stream _cStream;
		protected ushort _nCapacityHeader
		{
			get
			{
				ushort nRetVal = (ushort)(sizeof(uint) + sizeof(uint));
				if (1 == _nSize)
					nRetVal += sizeof(ulong);
				return nRetVal;
			}
		}
		virtual protected ushort _nCapacity
		{
			get
			{
				return _nCapacityHeader;
			}
		}
		//size 
		//A 32-bit integer that indicates the size of the atom, including both the atom header and the atom’s contents, including any contained atoms. Normally, the size field contains the actual size of the atom, in bytes, expressed as a 32-bit unsigned integer. However, the size field can contain special values that indicate an alternate method of determining the atom size. (These special values are normally used only for media data ('mdat') atoms.)
		//If the size field is set to 0, which is allowed only for a top-level atom, this is the last atom in the file and it extends to the end of the file.
		//If the size field is set to 1, then the actual size is given in the extended size field, an optional 64-bit field that follows the type field.
		//This accommodates media data atoms that contain more than 2^32 bytes.
		private uint _nSize;
		//extended size 
		//If the size field of an atom is set to 1, the type field is followed by a 64-bit extended size field, which contains the actual size of the atom as a 64-bit unsigned integer. This is used when the size of a media data atom exceeds 2^32 bytes.
		private ulong _nSizeExtended;
		//type 
		//A 32-bit integer that contains the type of the atom. This can often be usefully treated as a four-character field with a mnemonic value, such as 'moov' (0x6D6F6F76) for a movie atom, or 'trak' (0x7472616B) for a track atom, but non-ASCII values (such as 0x00000001) are also used.
		//Knowing an atom's type allows you to interpret its data. An atom's data can be arranged as any arbitrary collection of fields, tables, or other atoms. The data structure is specific to the atom type. An atom of a given type has a defined data structure.
		//If your application encounters an atom of an unknown type, it should not attempt to interpret the atom's data. Use the atom's size field to skip this atom and all of its contents. This allows a degree of forward compatibility with extensions to the QuickTime file format. 
		//Warning  The internal structure of a given type of atom can change when a new version is introduced. Always check the version field, if one exists. Never attempt to interpret data that falls outside of the atom, as defined by the Size or Extended Size fields.
		private uint _nType;

		public ulong nSize
		{
			get
			{
				if (1 == _nSize)
					return _nSizeExtended;
				return _nSize;
			}
		}
		public string sType
		{
			get
			{
				char[] aChars = new char[sizeof(uint)];
				BitConverter.GetBytes(_nType).CopyTo(aChars, 0);
				return new string(aChars.Reverse().ToArray());
			}
		}
		protected Type eType
		{
			get
			{
				try
				{
					return (Type)_nType;
				}
				catch{}
				return Type.unknown;
			}
		}

		public Atom cParent;
		public Atom cChild
		{
			get
			{
				_cStream.Position = DataOffsetGet();
				return Read(_cStream);
			}
		}
		public Atom cNext
		{
			get
			{
				_cStream.Position = (long)((ulong)_nStreamPositionStart + nSize);
				return Read(_cStream);
			}
		}

		private Atom(Stream cStream)
		{
			_cStream = cStream;
			_nStreamPositionStart = cStream.Position;
			byte[] aBuffer = new byte[sizeof(ulong)];
			int nLength, nBytesReaded;

			#region size
			nLength = sizeof(uint);
			nBytesReaded = cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nSize = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region type
			nLength = sizeof(uint);
			nBytesReaded = cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nType = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region size extended
			if (1 == _nSize)
			{
				nLength = sizeof(ulong);
				nBytesReaded = cStream.Read(aBuffer, 0, nLength);
				if (nBytesReaded != nLength)
					throw new Exception("can't read necessary bytes qty");
				_nSizeExtended = BitConverter.ToUInt64((BitConverter.IsLittleEndian?aBuffer.Take(nLength).Reverse().ToArray():aBuffer), 0);
			}
			#endregion
		}
		internal Atom(Atom cAtom)
		{
			_cStream = cAtom._cStream;
			_nStreamPositionStart = cAtom._nStreamPositionStart;
			_nSize = cAtom._nSize;
			_nType = cAtom._nType;
			_nSizeExtended = cAtom._nSizeExtended;
		}

		public long DataOffsetGet()
		{
			return (long)((ulong)_nStreamPositionStart + _nCapacityHeader);
		}
		public long DataSizeGet()
		{
			return (long)(nSize - _nCapacityHeader);
		}
	}
}
