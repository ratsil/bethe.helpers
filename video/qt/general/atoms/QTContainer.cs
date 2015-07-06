using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpers.video.qt.atoms
{
	class QTContainer
	{
		//Reserved 
		//A 10-byte element that must be set to 0.
		private byte[] _aReserved1;
		// Lock count 
		//A 16-bit integer that must be set to 0.
		private ushort _nLockCount;
	}
}
