using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace helpers
{
	public class CopyFileExtended
	{
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

		private delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);
		private enum CopyProgressResult : uint
		{
			PROGRESS_CONTINUE = 0,
			PROGRESS_CANCEL = 1,
			PROGRESS_STOP = 2,
			PROGRESS_QUIET = 3
		}
		private enum CopyProgressCallbackReason : uint
		{
			CALLBACK_CHUNK_FINISHED = 0x00000000,
			CALLBACK_STREAM_SWITCH = 0x00000001
		}
		[Flags]
		private enum CopyFileFlags : uint
		{
			COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
			COPY_FILE_RESTARTABLE = 0x00000002,
			COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
			COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
		}
		private int _nCancel;
		private int _nDelay;
		private int _nChunkIndx;
		private int _nChanksPeriodToDelay;
		private int _nMilisecondPeriodToDelay;
		public int nIndex;
		private DateTime _dtLastDelay;

		//public CopyFileExtended(string sOldFile, string sNewFile, int nDelay, int nChanksPeriodToDelay)
		//{
		//	_nChunkIndx = 0;
		//	_nDelay = nDelay < 0 ? 0 : nDelay;      // 1 ms может увеличить в 6 раз копирование
		//	_nChanksPeriodToDelay = nChanksPeriodToDelay > 0 ? nChanksPeriodToDelay : 1;         // чтобы не каждый чанк делеить
		//	CopyFileEx(sOldFile, sNewFile, new CopyProgressRoutine(this.CopyProgressHandler), IntPtr.Zero, ref _nCancel, CopyFileFlags.COPY_FILE_OPEN_SOURCE_FOR_WRITE);
		//}
		public CopyFileExtended(string sOldFile, string sNewFile, int nDelay, int nMilisecondPeriodToDelay)
		{
			nIndex = 0;
			_nChunkIndx = 0;
			_nDelay = nDelay < 0 ? 0 : nDelay;      // 1 ms может увеличить в 6 раз копирование
			_nMilisecondPeriodToDelay = nMilisecondPeriodToDelay >= 0 ? nMilisecondPeriodToDelay : 0;         // чтобы не каждый чанк делеить
			_dtLastDelay = DateTime.Now;
			CopyFileEx(sOldFile, sNewFile, new CopyProgressRoutine(this.CopyProgressHandler), IntPtr.Zero, ref _nCancel, CopyFileFlags.COPY_FILE_RESTARTABLE);
		}
		public void CopyWithDelay()
		{
		}
		public void CopyWithProgress()  // можно реализовать
		{
		}
		private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long StreamByteTrans, uint dwStreamNumber, CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
		{
			if (_nMilisecondPeriodToDelay < DateTime.Now.Subtract(_dtLastDelay).TotalMilliseconds)
			{
				_dtLastDelay = DateTime.Now;
				nIndex++;
				System.Threading.Thread.Sleep(_nDelay);
			}
			//_nChunkIndx++;
			//if (_nChunkIndx % _nChanksPeriodToDelay == 0)
			//	System.Threading.Thread.Sleep(_nDelay);
			return CopyProgressResult.PROGRESS_CONTINUE;
		}
	}
}
