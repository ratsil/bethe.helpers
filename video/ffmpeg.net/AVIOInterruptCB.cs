///**
// * Callback for checking whether to abort blocking functions.
// * AVERROR_EXIT is returned in this case by the interrupted
// * function. During blocking operations, callback is called with
// * opaque as parameter. If the callback returns 1, the
// * blocking operation will be aborted.
// *
// * No members can be added to this struct without a major bump, if
// * new elements have been added after this struct in AVFormatContext
// * or AVIOContext.
// */
//typedef struct {
//    int (*callback)(void*);
//    void *opaque;
//} AVIOInterruptCB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct AVIOInterruptCB
	{
		public /*int (*callback)(void*)*/IntPtr callback;
		public /*void* */IntPtr opaque;
	}
}
