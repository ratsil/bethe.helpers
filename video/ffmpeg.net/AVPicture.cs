using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVPicture
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
        public /*uint8_t*[AV_NUM_DATA_POINTERS] */IntPtr[] data;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] //Constants.AV_NUM_DATA_POINTERS
        public /*int[AV_NUM_DATA_POINTERS]*/int[] linesize;       ///< number of bytes per line
	}
}
