using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;

namespace helpers
{
	public class CopyFileExtended
	{
        [StructLayout(LayoutKind.Explicit)]
        private struct COPYFILE2_EXTENDED_PARAMETERS
        {
            [FieldOffset(0)]
            public uint dwSize; //dword
            [FieldOffset(4)]
            public Copy2FileFlags dwCopyFlags; //dword
            [FieldOffset(8)]
            public IntPtr pfCancel; //bool*
            [FieldOffset(16)]
            public Copy2ProgressRoutine pProgressRoutine; //pointer
            [FieldOffset(24)]
            public IntPtr pvCallbackContext; //pointer
        }
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CopyFile2(string lpExistingFileName, string lpNewFileName, ref COPYFILE2_EXTENDED_PARAMETERS pExtendedParameters);

        private delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);
        private delegate CopyProgressResult Copy2ProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);
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
			COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
            COPY_FILE_NO_BUFFERING = 0x00001000
		}
        [Flags]
        private enum Copy2FileFlags : uint
        {
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_RESTARTABLE = 0x00000002,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
            COPY_FILE_NO_BUFFERING = 0x00001000,
            COPY_FILE_COPY_SYMLINK = 0x00000800,
            COPY_FILE_NO_OFFLOAD = 0x00040000,
            COPY_FILE_REQUEST_SECURITY_PRIVILEGES = 0x00002000,
            COPY_FILE_RESUME_FROM_PAUSE = 0x00004000
        }
        private int _nCancel;
		private int _nDelay;
		private int _nMilisecondPeriodToDelay;
        private long _nChunkIndx;
        private int _nChanksPeriod;
        private int _nBytesPeriod;
        private long _nBytesNextPoint;
        private DateTime _dtNextDelay;
        public delegate void dProgressChanged(float nProgress);
        private dProgressChanged _OnProgressChanged;
        private string _sSrcFile;
        private string _sDestFile;
        private float _nProgressPercent;
        private bool _bPause;
        private bool _bCancelCopying;
        private long _nFramesDur;
        private TimeSpan _tsReal;
        public TimeSpan tsReal
        {
            get
            {
                return _tsReal;
            }
        }
        private TimeSpan _tsDiff;
        public TimeSpan tsDiff
        {
            get
            {
                return _tsDiff;
            }
        }
        private long _nDelayTotal;
        public long nDelayTotal
        {
            get
            {
                return _nDelayTotal;
            }
        }
        private long _nTotalSize;
        public long nTotalSize
        {
            get
            {
                return _nTotalSize;
            }
        }

        static private string _sFilePauseCopying;
        static public string sFilePauseCopying
        {
            get
            {
                return _sFilePauseCopying;
            }
            set
            {
                lock (oLock)
                    if (null == _sFilePauseCopying && null != value)
                    {
                        _sFilePauseCopying = value;
                        System.Threading.ThreadPool.QueueUserWorkItem(PausingWorker);
                    }
            }
        }
        static private bool _bPauseFile;
        static private object oLock = new object();

        public float nProgressPercent
        {
            get
            {
                return _nProgressPercent;
            }
        }
        public bool bCompleted;

        public CopyFileExtended(string sSrcFile, string sDestFile, int nDelayMiliseconds, int nMilisecondPeriodToDelay, long nFramesDur)
            : this(sSrcFile, sDestFile, nDelayMiliseconds, nMilisecondPeriodToDelay, nFramesDur, null)
        {}
        public CopyFileExtended(string sSrcFile, string sDestFile, int nDelayMiliseconds, int nPeriodToDelayMiliseconds, long nFramesDur, dProgressChanged OnProgressChanged)
        {
            _sSrcFile = sSrcFile;
            _sDestFile = sDestFile;
            _OnProgressChanged = OnProgressChanged;
            _bPause = false;
            _nChunkIndx = 0;
            _nChanksPeriod = 10;
            _nBytesPeriod = 120 * 1000 * 1000;
            _nBytesNextPoint = _nBytesPeriod;
            _nDelay = nDelayMiliseconds < 0 ? 0 : nDelayMiliseconds;      // 1 ms может увеличить в 6 раз копирование
			_nMilisecondPeriodToDelay = nPeriodToDelayMiliseconds > 0 ? nPeriodToDelayMiliseconds : 0;         // чтобы не каждый чанк делеить
			_dtNextDelay = DateTime.Now.AddMilliseconds(_nMilisecondPeriodToDelay);
            _aqReadBytes = new ThreadBufferQueue<ReadTask>(2, true, true);
            _aAvailableBytes = new ThreadBufferQueue<byte[]>(false, false);
            _nFramesDur = nFramesDur;
            _nDelayTotal = 0;
            _nTotalSize = 0;
        }
        static private void PausingWorker(object oState)
        {
            try
            {
                while (true)
                {
                    if (System.IO.File.Exists(_sFilePauseCopying))
                        _bPauseFile = true;
                    else
                        _bPauseFile = false;
                    System.Threading.Thread.Sleep(3000);
                }
            }
            catch(Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
            finally
            {
                _bPauseFile = false;
            }
        }
        public bool DoCopy(bool bResetLastWriteTime)  // with kernel32.dll  CopyFileEx   -- еле-еле обгоняет хронометраж на 10.100
        {
            DateTime dtStart = DateTime.Now;
            bool bRetVal = false;
            while (!_bCancelCopying && (_bPause || _bPauseFile))
                System.Threading.Thread.Sleep(300);

            if (!_bCancelCopying)
            {
                bRetVal = CopyFileEx(_sSrcFile, _sDestFile, new CopyProgressRoutine(this.CopyProgressHandler), IntPtr.Zero, ref _nCancel, CopyFileFlags.COPY_FILE_NO_BUFFERING);
                //return CopyFileEx(_sSrcFile, _sDestFile, null, IntPtr.Zero, ref _nCancel, CopyFileFlags.COPY_FILE_NO_BUFFERING);  // COPY_FILE_RESTARTABLE  медленнее
            }
            DiffResultSet(dtStart);
            if (bResetLastWriteTime && System.IO.File.Exists(_sDestFile))
                System.IO.File.SetLastWriteTime(_sDestFile, DateTime.Now);
            (new Logger()).WriteNotice("copy extended done [real=" + _tsReal.ToString("hh\\:mm\\:ss") + "][diff=" + _tsDiff.ToString("hh\\:mm\\:ss") + "][delay=" + (_nDelayTotal / 1000) + " sec][exit " + (_bCancelCopying ? "on cancel" : "normal") + "][src=" + _sSrcFile + "][trg=" + _sDestFile + "][progr=" + _nProgressPercent.ToString("0.0") + "][canceled=" + _bCancelCopying + "][size=" + _nTotalSize + "]");
            return bRetVal;
        }
        public bool DoCopy3()  // with kernel32.dll  CopyFile  -- быстрее всех на 10.100 и даже File.Copy медленнее
        {
            while (!_bCancelCopying && (_bPause || _bPauseFile))
                System.Threading.Thread.Sleep(300);

            if (!_bCancelCopying)
                return CopyFile(_sSrcFile, _sDestFile, true);
            return false;
        }
        public bool DoCopy4()  // with kernel32.dll  CopyFile2  -- ооооочень медленно!!
        {
            while (!_bCancelCopying && (_bPause || _bPauseFile))
                System.Threading.Thread.Sleep(300);

            bool bRetVal = false;
            if (!_bCancelCopying)
            {
                COPYFILE2_EXTENDED_PARAMETERS stParams = new COPYFILE2_EXTENDED_PARAMETERS();
                int bCancel = 0;
                int size = Marshal.SizeOf(bCancel);
                IntPtr pBool = Marshal.AllocHGlobal(size);
                Marshal.WriteInt32(pBool, 0, bCancel);  // last parameter 0 (FALSE), 1 (TRUE)

                stParams.dwSize = (uint)Marshal.SizeOf(stParams);
                stParams.dwCopyFlags = Copy2FileFlags.COPY_FILE_NO_BUFFERING | Copy2FileFlags.COPY_FILE_RESTARTABLE;
                stParams.pfCancel = pBool;
                stParams.pProgressRoutine = null;
                stParams.pvCallbackContext = IntPtr.Zero;
                bRetVal = CopyFile2(_sSrcFile, _sDestFile, ref stParams);
                Marshal.FreeHGlobal(pBool);
            }
            return bRetVal;
        }
        



        private ThreadBufferQueue<byte[]> _aAvailableBytes;
        private ThreadBufferQueue<ReadTask> _aqReadBytes;
        private FileStream _cStreamSource;
        private const int _nBytesMaxRead = 100 * 1024 * 512;
        private class ReadTask
        {
            public byte[] aBytes;
            public int nLength;
        }
        public bool DoCopy2()  // with system.io.Stream  -- быстрее, чем DoCopy(), но на 10.100 такая же убитая, даже хуже. (на 10.100 сдох буфер на рейде)
        {
            bool bRetVal = false;
            _cStreamSource = null;
            FileStream cStreamTarget = null;
            _nTotalSize = 0;
            DateTime dtStart = DateTime.Now;
            string sExit = "normal";

            if (!_bCancelCopying)
            {
                try
                {
                    _cStreamSource = new FileStream(_sSrcFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096 * 3, FileOptions.SequentialScan | FileOptions.Asynchronous);
                    cStreamTarget = new FileStream(_sDestFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096 * 3, FileOptions.SequentialScan | FileOptions.Asynchronous | FileOptions.WriteThrough);
                    cStreamTarget.SetLength(_nTotalSize = _cStreamSource.Length);
                    int nBytesRead = _nBytesMaxRead;
                    long nBytesTotal = 0;
                    byte[] aBytes;
                    ReadTask cRT;

                    System.Threading.ThreadPool.QueueUserWorkItem((object o) => { ReadWorker(); });

                    while (0 < nBytesRead)
                    {
                        while (!_bCancelCopying && (_bPause || _bPauseFile))
                            System.Threading.Thread.Sleep(300);

                        if (_bCancelCopying)
                        {
                            sExit = "on cancel";
                            return false;
                        }

                        cRT = _aqReadBytes.Dequeue();
                        aBytes = cRT.aBytes;
                        nBytesRead = cRT.nLength;
                        if (nBytesRead == 0)
                        {
                            sExit = "on break-1";
                            break;
                        }

                        nBytesTotal += nBytesRead;
                        cStreamTarget.Write(aBytes, 0, nBytesRead);
                        cStreamTarget.Flush(true);



                        _aAvailableBytes.Enqueue(aBytes);
                        
                        if (nBytesTotal > _nBytesNextPoint)    // а лучше раз во сколько-то байт
                        {
                            _nBytesNextPoint += _nBytesPeriod;
                            _nProgressPercent = (float)(((float)nBytesTotal / _nTotalSize) * 100.0);
                            if (null != _OnProgressChanged)
                                _OnProgressChanged(_nProgressPercent);
                        }

                        if (_nMilisecondPeriodToDelay > 0 && _dtNextDelay < DateTime.Now) // а можно раз во сколько-то миллисекунд что-то делать
                        {
                            _nDelayTotal += _nDelay;
                            System.Threading.Thread.Sleep(_nDelay);
                            _dtNextDelay = DateTime.Now.AddMilliseconds(_nMilisecondPeriodToDelay);
                        }
                    }
                    bRetVal = true;
                    _nProgressPercent = 100F;
                    bCompleted = true;
                    if (null != _OnProgressChanged)
                        _OnProgressChanged(_nProgressPercent);
                }
                catch (Exception ex)
                {
                    sExit = "on error";
                    (new Logger()).WriteError(ex);
                }
                finally
                {
                    if (null != _cStreamSource)
                        _cStreamSource.Close();
                    if (null != cStreamTarget)
                    {
                        cStreamTarget.Flush(true);
                        cStreamTarget.Close();
                    }
                    //System.Threading.Thread.Sleep(50); // реально если этого не делать, то снаружи ловим иногда "файл занят" (раз в 2-3 дня где-то)  // Flush(true) пофиксил
                    DiffResultSet(dtStart);
                    (new Logger()).WriteNotice("copy extended done [real=" + _tsReal.ToString("hh\\:mm\\:ss") + "][diff=" + _tsDiff.ToString("hh\\:mm\\:ss") + "][delay=" + (_nDelayTotal / 1000) + " sec][exit " + sExit + "][src=" + _sSrcFile + "][trg=" + _sDestFile + "][progr=" + _nProgressPercent.ToString("0.0") + "][canceled=" + _bCancelCopying + "][size=" + _nTotalSize + "]");
                }
            }
            return bRetVal;
        }
        private void DiffResultSet(DateTime dtStart)
        {
            _tsReal = DateTime.Now.Subtract(dtStart);
            TimeSpan tsExpected = TimeSpan.FromMilliseconds(_nFramesDur * 40);
            _tsDiff = _tsReal.Subtract(tsExpected);
        }
        private void ReadWorker()
        {
            try
            {
                byte[] aBytes;
                int nBytesRead;
                while (true)
                {
                    if (_aAvailableBytes.CountGet() <= 0)
                        aBytes = new byte[_nBytesMaxRead];
                    else
                        aBytes = _aAvailableBytes.Dequeue();

                    nBytesRead = _cStreamSource.Read(aBytes, 0, _nBytesMaxRead);
                    _aqReadBytes.Enqueue(new ReadTask() { aBytes = aBytes, nLength = nBytesRead });
                    if (nBytesRead == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError(ex);
            }
        }
        public void Pause()
        {
            _bPause = true;
        }
        public void UnPause()
        {
            _bPause = false;
        }
        public void Cancel()
        {
            _bCancelCopying = true;
            _bPause = false;
        }
        public void CopyWithDelay()
		{
		}
		public void CopyWithProgress()  // можно реализовать
		{
		}
		private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long StreamByteTrans, uint dwStreamNumber, CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
		{
            while (!_bCancelCopying && (_bPause || _bPauseFile))
                System.Threading.Thread.Sleep(300);

            if (_bCancelCopying)
                return CopyProgressResult.PROGRESS_CANCEL;

            _nTotalSize = total;
            if (transferred >= total)  //transfer completed
            {
                //if file is read only, remove read-only attribute(case to handle CD drive import)
                //FileAttributes attr = File.GetAttributes(destinationFilePath);
                //if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                //{
                //    attr = attr & ~FileAttributes.ReadOnly;
                //    File.SetAttributes(destinationFilePath, attr);
                //}
                _nProgressPercent = 100F;
                bCompleted = true;
                if (null != _OnProgressChanged)
                    _OnProgressChanged(_nProgressPercent);
            }
            else
            {
                if (reason == CopyProgressCallbackReason.CALLBACK_CHUNK_FINISHED)
                {
                    //_nChunkIndx++;  
                    //if (_nChunkIndx % _nChunksPeriod == 0)    // можно раз во сколько-то чанков что-то делать
                    //   {}
                    if (transferred > _nBytesNextPoint)    // а лучше раз во сколько-то байт
                    {
                        _nBytesNextPoint += _nBytesPeriod;
                        _nProgressPercent = (float)((transferred / (double)total) * 100.0);
                        if (null != _OnProgressChanged)
                            _OnProgressChanged(_nProgressPercent);
                    }

                    if (_nMilisecondPeriodToDelay > 0 && _dtNextDelay < DateTime.Now) // а можно раз во сколько-то миллисекунд что-то делать
                    {
                        _nDelayTotal += _nDelay;
                        System.Threading.Thread.Sleep(_nDelay);
                        _dtNextDelay = DateTime.Now.AddMilliseconds(_nMilisecondPeriodToDelay);
                    }
                }
            }
			return CopyProgressResult.PROGRESS_CONTINUE;
		}
	}

	public class PinvokeWindowsNetworking  // Use this function to connect to a UNC path with authenticaiton, NOT to map a drive   Example: \\computername\c$\program files\Folder\file.txt
	{
		#region Consts
		const int RESOURCE_CONNECTED = 0x00000001;
		const int RESOURCE_GLOBALNET = 0x00000002;
		const int RESOURCE_REMEMBERED = 0x00000003;

		const int RESOURCETYPE_ANY = 0x00000000;
		const int RESOURCETYPE_DISK = 0x00000001;
		const int RESOURCETYPE_PRINT = 0x00000002;

		const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
		const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
		const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
		const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
		const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
		const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;

		const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
		const int RESOURCEUSAGE_CONTAINER = 0x00000002;


		const int CONNECT_INTERACTIVE = 0x00000008;
		const int CONNECT_PROMPT = 0x00000010;
		const int CONNECT_REDIRECT = 0x00000080;
		const int CONNECT_UPDATE_PROFILE = 0x00000001;
		const int CONNECT_COMMANDLINE = 0x00000800;
		const int CONNECT_CMD_SAVECRED = 0x00001000;

		const int CONNECT_LOCALDRIVE = 0x00000100;
		#endregion

		#region Errors
		const int NO_ERROR = 0;

		const int ERROR_ACCESS_DENIED = 5;
		const int ERROR_ALREADY_ASSIGNED = 85;
		const int ERROR_BAD_DEVICE = 1200;
		const int ERROR_BAD_NET_NAME = 67;
		const int ERROR_BAD_PROVIDER = 1204;
		const int ERROR_CANCELLED = 1223;
		const int ERROR_EXTENDED_ERROR = 1208;
		const int ERROR_INVALID_ADDRESS = 487;
		const int ERROR_INVALID_PARAMETER = 87;
		const int ERROR_INVALID_PASSWORD = 1216;
		const int ERROR_MORE_DATA = 234;
		const int ERROR_NO_MORE_ITEMS = 259;
		const int ERROR_NO_NET_OR_BAD_PATH = 1203;
		const int ERROR_NO_NETWORK = 1222;

		const int ERROR_BAD_PROFILE = 1206;
		const int ERROR_CANNOT_OPEN_PROFILE = 1205;
		const int ERROR_DEVICE_IN_USE = 2404;
		const int ERROR_NOT_CONNECTED = 2250;
		const int ERROR_OPEN_FILES = 2401;

		private struct ErrorClass
		{
			public int num;
			public string message;
			public ErrorClass(int num, string message)
			{
				this.num = num;
				this.message = message;
			}
		}


		// Created with excel formula:
		// ="new ErrorClass("&A1&", """&PROPER(SUBSTITUTE(MID(A1,7,LEN(A1)-6), "_", " "))&"""), "
		private static ErrorClass[] ERROR_LIST = new ErrorClass[] {
			new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
			new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
			new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
			new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
			new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
			new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
			new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
			new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
			new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
			new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
			new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
			new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
			new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
			new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
			new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
			new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
			new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
			new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
			new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
			new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
		};

		private static string getErrorForNumber(int errNum)
		{
			foreach (ErrorClass er in ERROR_LIST)
			{
				if (er.num == errNum) return er.message;
			}
			return "Error: Unknown, " + errNum;
		}
		#endregion

		[DllImport("Mpr.dll")]
		private static extern int WNetUseConnection(
			IntPtr hwndOwner,
			NETRESOURCE lpNetResource,
			string lpPassword,
			string lpUserID,
			int dwFlags,
			string lpAccessName,
			string lpBufferSize,
			string lpResult
		);

		[DllImport("Mpr.dll")]
		private static extern int WNetCancelConnection2(
			string lpName,
			int dwFlags,
			bool fForce
		);

		[StructLayout(LayoutKind.Sequential)]
		private class NETRESOURCE
		{
			public int dwScope = 0;
			public int dwType = 0;
			public int dwDisplayType = 0;
			public int dwUsage = 0;
			public string lpLocalName = "";
			public string lpRemoteName = "";
			public string lpComment = "";
			public string lpProvider = "";
		}


		public static string connectToRemote(string remoteUNC, string username, string password)
		{
			return connectToRemote(remoteUNC, username, password, false);
		}

		public static string connectToRemote(string remoteUNC, string username, string password, bool promptUser)
		{
			NETRESOURCE nr = new NETRESOURCE();
			nr.dwType = RESOURCETYPE_DISK;
			nr.lpRemoteName = remoteUNC;
			//			nr.lpLocalName = "F:";

			int ret;
			if (promptUser)
				ret = WNetUseConnection(IntPtr.Zero, nr, "", "", CONNECT_INTERACTIVE | CONNECT_PROMPT, null, null, null);
			else
				ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

			if (ret == NO_ERROR) return null;
			return getErrorForNumber(ret);
		}

		public static string disconnectRemote(string remoteUNC)
		{
			int ret = WNetCancelConnection2(remoteUNC, CONNECT_UPDATE_PROFILE, false);
			if (ret == NO_ERROR) return null;
			return getErrorForNumber(ret);
		}
	}

	public class Zip
	{
		static public void ZipFiles(string[] aFiles, string sZipName, bool bDeleteExistingZipArchive, bool bMoveFilesToZip, CompressionLevel eCompLevel)
		{
			if (bDeleteExistingZipArchive && File.Exists(sZipName))
				File.Delete(sZipName);
			System.IO.FileStream sStream = File.OpenWrite(sZipName);
			using (ZipArchive cZip = new ZipArchive(sStream, ZipArchiveMode.Create, false))
			{
				foreach (string sFile in aFiles)
				{
					if (File.Exists(sFile))
					{
						cZip.CreateEntryFromFile(sFile, Path.GetFileName(sFile), eCompLevel);
						if (bMoveFilesToZip)
							File.Delete(sFile);
					}
				}
				cZip.Dispose();
			}
			sStream.Close();
		}
	}
}
