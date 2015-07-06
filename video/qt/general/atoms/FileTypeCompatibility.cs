using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helpers.video.qt.atoms
{
	public class FileTypeCompatibility : Atom
	{
		override protected ushort _nCapacity
		{
			get
			{
				ushort nRetVal = (ushort)(base._nCapacity + sizeof(uint) + sizeof(uint));
				if (null != _aCompatibleBrands && 0 < _aCompatibleBrands.Length)
					nRetVal += (ushort)(sizeof(uint) * _aCompatibleBrands.Length);
				return nRetVal;
			}
		}
		// Major_Brand 
		//A 32-bit unsigned integer that should be set to 'qt  ' (note the two trailing ASCII space characters) for QuickTime movie files. If a file is compatible with multiple brands, all such brands are listed in the Compatible_Brands fields, and the Major_Brand identifies the preferred brand or best use.
		private uint _nMajorBrand;
		// Minor_Version
		//A 32-bit field that indicates the file format specification version. For QuickTime movie files, this takes the form of four binary-coded decimal values, indicating the century, year, and month of the QuickTime File Format Specification, followed by a binary coded decimal zero. For example, for the June 2004 minor version, this field is set to the BCD values 20 04 06 00.
		private uint _nMinorVersion;
		// Compatible_Brands[ ] 
		//A series of unsigned 32-bit integers listing compatible file formats. The major brand must appear in the list of compatible brands. One or more “placeholder” entries with value zero are permitted; such entries should be ignored.
		//If none of the Compatible_Brands fields is set to 'qt  ', then the file is not a QuickTime movie file and is not compatible with this specification. Applications should return an error and close the file, or else invoke a file importer appropriate to one of the specified brands, preferably the major brand. QuickTime currently returns an error when attempting to open a file whose file type, file extension, or MIME type identifies it as a QuickTime movie, but whose file type atom does not include the 'qt  ' brand.
		private uint[] _aCompatibleBrands;
		//Note: A common source of this error is an MPEG-4 file incorrectly named with the .mov file extension or with the MIME type incorrectly set to “video/quicktime”. MPEG-4 files are automatically imported by QuickTime only when they are correctly identified as MPEG-4 files using the Mac OS file type, file extension, or MIME type.
		internal FileTypeCompatibility(Atom cAtom)
			: base(cAtom)
		{
			byte[] aBuffer = new byte[sizeof(uint)];
			int nLength, nBytesReaded;

			#region major brand
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nMajorBrand = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region minor version
			nLength = sizeof(uint);
			nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
			if (nBytesReaded != nLength)
				throw new Exception("can't read necessary bytes qty");
			_nMinorVersion = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
			#endregion

			#region compatible brands
			nLength = sizeof(uint);
			List<uint> aCompatibleBrands = new List<uint>();
			while (_nCapacity < (ulong)_nStreamPositionStart + nSize)
			{
				nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
				if (nBytesReaded != nLength)
					throw new Exception("can't read necessary bytes qty");
				aCompatibleBrands.Add(BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0));
				_aCompatibleBrands = aCompatibleBrands.ToArray();
			}
			#endregion
		}
	}
}
