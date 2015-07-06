using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace helpers
{
	static public class WinAPI
	{
#region dll import
		[DllImport("gdi32.dll")]
		static public extern bool DeleteObject(IntPtr hObject);
		[DllImport("user32.dll")]
		static public extern IntPtr GetDC(IntPtr hwnd);
		[DllImport("gdi32.dll")]
		static public extern IntPtr CreateCompatibleDC(IntPtr hdc);
		[DllImport("user32.dll")]
		static public extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
		[DllImport("gdi32.dll")]
		static public extern int DeleteDC(IntPtr hdc);
		[DllImport("gdi32.dll")]
		static public extern short StretchBlt(IntPtr hdcDst, int xDst, int yDst, int wDst, int hDst, IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);
		[DllImport("gdi32.dll")]
		static public extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);
		[DllImport("gdi32.dll")]
		static public extern int GetBitmapBits(IntPtr hbmp, int cbBuffer, [Out] byte[] lpvBits);

		[DllImport("gdi32.dll")]
		//void * memcpy( void * destination, const void * source, size_t num );
		static public extern IntPtr memcpy(IntPtr destination, IntPtr source, int num);
		[DllImport("gdi32.dll")]
		//void * memmove ( void * destination, const void * source, size_t num );
		static public extern IntPtr memmove(IntPtr destination, IntPtr source, int num);
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(IntPtr dest, int c, int count);
        [DllImport("user32.dll")]
		static public extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags); 
 

		//Sets window attributes
		[DllImport("USER32.DLL")]
		static public extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		//Gets window attributes
		[DllImport("USER32.DLL")]
		static public extern int GetWindowLong(IntPtr hWnd, int nIndex);
#endregion
#region constants
		static public int SRCCOPY = 0x00CC0020;
		static public uint BI_RGB = 0;
		static public uint DIB_RGB_COLORS = 0;

		static public int GWL_STYLE = -16;
		static public int WS_CHILD = 0x40000000; //child window
		static public int WS_BORDER = 0x00800000; //window with border
		static public int WS_DLGFRAME = 0x00400000; //window with double border but no title
		static public int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar 
		static public int SWP_NOSIZE = 0x0001; 
		static public int SWP_NOMOVE = 0x0002; 
		static public int SWP_SHOWWINDOW = 0x0040; 
 
#endregion
#region structures
		[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public uint biSize;
			public int biWidth, biHeight;
			public short biPlanes, biBitCount;
			public uint biCompression, biSizeImage;
			public int biXPelsPerMeter, biYPelsPerMeter;
			public uint biClrUsed, biClrImportant;
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 256)]
			public uint[] cols;
		}
#endregion
#region macros
		static uint MAKERGB(int r, int g, int b)
		{
			return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
		}
#endregion
	}
}
