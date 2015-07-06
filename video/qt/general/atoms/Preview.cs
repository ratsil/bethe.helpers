using System;
using System.IO;

namespace helpers.video.qt.atoms
{
	class Preview : Atom
	{
//Modification date 
//A 32-bit unsigned integer containing a date that indicates when the preview was last updated. The data is in standard Macintosh format. 
//Version number 
//A 16-bit integer that must be set to 0.
// Atom type 
//A 32-bit integer that indicates the type of atom that contains the preview data. Typically, this is set to 'PICT' to indicate a QuickDraw picture.
// Atom index 
//A 16-bit integer that identifies which atom of the specified type is to be used as the preview. Typically, this field is set to 1 to indicate that you should use the first atom of the type specified in the atom type field.
		public Preview(Atom cAtom)
			: base(cAtom)
		{
		}
	}
}
