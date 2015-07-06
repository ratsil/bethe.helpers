using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace helpers.video.qt.atoms
{
	public class Edit : Atom
	{
		public class List : Atom
		{
			public class Entry
			{
				//Track duration 
				//A 32-bit integer that specifies the duration of this edit segment in units of the movie’s time scale.
				private uint _nTrackDuration;
				// Media time 
				//A 32-bit integer containing the starting time within the media of this edit segment (in media timescale units). If this field is set to –1, it is an empty edit. The last edit in a track should never be an empty edit. Any difference between the movie’s duration and the track’s duration is expressed as an implicit empty edit. 
				private uint _nMediaTime;
				//Media rate
				//A 32-bit fixed-point number that specifies the relative rate at which to play the media corresponding to this edit segment. This rate value cannot be 0 or negative. 
				private uint _nMediaRate;

				internal Entry(Stream cStream)
				{
					byte[] aBuffer = new byte[sizeof(uint)];
					int nLength, nBytesReaded;

					#region track duration
					nLength = sizeof(uint);
					nBytesReaded = cStream.Read(aBuffer, 0, nLength);
					if (nBytesReaded != nLength)
						throw new Exception("can't read necessary bytes qty");
					_nTrackDuration = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
					#endregion

					#region media time
					nLength = sizeof(uint);
					nBytesReaded = cStream.Read(aBuffer, 0, nLength);
					if (nBytesReaded != nLength)
						throw new Exception("can't read necessary bytes qty");
					_nMediaTime = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
					#endregion

					#region media rate
					nLength = sizeof(uint);
					nBytesReaded = cStream.Read(aBuffer, 0, nLength);
					if (nBytesReaded != nLength)
						throw new Exception("can't read necessary bytes qty");
					_nMediaRate = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
					#endregion
				}
			}
			//Version 
			//A 1-byte specification of the version of this edit list atom.
			private byte _nVersion;
			// Flags 
			//Three bytes of space for flags. Set this field to 0.
			private byte[] _aFlags = new byte[3];
			// Number of entries 
			//A 32-bit integer that specifies the number of entries in the edit list atom that follows.
			private uint _nNumberOfEntries;
			// Edit list table 
			//An array of 32-bit values, grouped into entries containing 3 values each. Figure 2-15 shows the layout of the entries in this table.
			private Entry[] _aEditListTable;

			internal List(Atom cAtom)
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

				#region number of entries
				nLength = sizeof(uint);
				nBytesReaded = _cStream.Read(aBuffer, 0, nLength);
				if (nBytesReaded != nLength)
					throw new Exception("can't read necessary bytes qty");
				_nNumberOfEntries = BitConverter.ToUInt32((BitConverter.IsLittleEndian ? aBuffer.Take(nLength).Reverse().ToArray() : aBuffer), 0);
				#endregion

				#region edit list table
				List<Entry> aEntries = new List<Entry>();
				for (int nIndx = 0; _nNumberOfEntries > nIndx; nIndx++)
					aEntries.Add(new Entry(_cStream));
				_aEditListTable = aEntries.ToArray();
				#endregion
			}
		}

		internal Edit(Atom cAtom)
			: base(cAtom)
		{
		}
	}
}
