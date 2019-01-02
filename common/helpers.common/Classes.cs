using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

using helpers.extensions;

namespace helpers
{
	public class StringWriter : System.IO.StringWriter
	{
		public Encoding oEncoding = Encoding.UTF8;
		public override Encoding Encoding { get { return oEncoding; } }
	}
	public class Map : Dictionary<string, object>
	{
	}
    public class DownStreamKeyer
    {
        public static DownStreamKeyer cDownStreamKeyer;
        public byte nLevel;
        public bool bInternal;

        public DownStreamKeyer()
        {
            nLevel = 255;
            bInternal = true;
        }
    }
    public class ConsoleTerminaion
    {
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        public delegate bool EventHandler(CtrlType sig);
        public static EventHandler _handler;
        public static bool exitSystem = false;

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
    }
    public enum MergingDevice : int
    {
        DisCom = 0,
        CUDA = 1,
        DisComExternal = 2
    }
    public struct MergingMethod
    {
        private MergingDevice _eDeviceType;
        public MergingDevice eDeviceType
        {
            get
            {
                return _eDeviceType;
            }
        }
        private int _nDeviceNumber;
        public int nDeviceNumber
        {
            get
            {
                return _nDeviceNumber;
            }
        }
        private int _nHash;
        public int nHash
        {
            get
            {
                return _nHash;
            }
        }
        public byte nPMTripleIndexSync;
        public MergingMethod(MergingDevice eDeviceType, int nDeviceNumber)
        {
            _eDeviceType = eDeviceType;
            _nDeviceNumber = nDeviceNumber;
            _nHash = (int)_eDeviceType * 1000 + _nDeviceNumber;
            nPMTripleIndexSync = byte.MaxValue;
        }
        public MergingMethod(XmlNode cXmlNode)
        {
            if (null != cXmlNode.AttributeValueGet("cuda", false))
            {
                bool bCuda = cXmlNode.AttributeGet<bool>("cuda");
                _eDeviceType = bCuda ? MergingDevice.CUDA : MergingDevice.DisCom;
                _nDeviceNumber = 0;
            }
            else
            {
                _eDeviceType = cXmlNode.AttributeOrDefaultGet<MergingDevice>("merging", MergingDevice.DisCom);
                _nDeviceNumber = cXmlNode.AttributeOrDefaultGet<int>("merging_id", 0);
            }

            _nHash = (int)_eDeviceType * 1000 + _nDeviceNumber;
            nPMTripleIndexSync = byte.MaxValue;
        }
        public static bool operator ==(MergingMethod stMM1, MergingMethod stMM2)
        {
            return stMM1.eDeviceType == stMM2.eDeviceType && stMM1.nDeviceNumber == stMM2.nDeviceNumber;
        }
        public static bool operator !=(MergingMethod stMM1, MergingMethod stMM2)
        {
            return !(stMM1 == stMM2);
        }
        public override bool Equals(object obj)
        {
            return this == (MergingMethod)obj;
        }
        public override int GetHashCode()
        {
            return _nHash;
        }
        public override string ToString()
        {
            return "[merdevice=" + eDeviceType + "][merid=" + nDeviceNumber + "]";
        }
    }
    public class Bytes
    {
        private static long _nMaxID = 0;
        public long nID;
        public byte[] aBytes;
        public int Length { get { return aBytes?.Length ?? 0; } }
        public Bytes(int nSize)
        {
            nID = Interlocked.Increment(ref _nMaxID);
            aBytes = new byte[nSize];
        }
        public Bytes()
        {
        }
        public override int GetHashCode()   // don't use. use nID
        {
            return nID.GetHashCode();
        }
    }
    public class BytesInMemory
    {
        private Dictionary<int, Queue<Bytes>> _ahBytesStorage;  //TODO не забыть если удачно, то чистить его периодически!!!! Пока по прошествии 6 месяцев на лдпр - идёт ровненько... // годы прошли уже - норм! Чисти!
        private Dictionary<long, bool> _ahHashPassedOut;   
        private Dictionary<int, ushort> _ahNumTotal;
        private List<long> _aBytesHashes;
        private List<long> _aBytesIgnor;
        private long _nHash;
        private long _nBytesTotal;
        private DateTime _dtNextInfo;
        private string _sLogPrefix;
        public BytesInMemory(string sLogPrefix)
        {
            _ahBytesStorage = new Dictionary<int, Queue<Bytes>>();
            _ahHashPassedOut = new Dictionary<long, bool>();
            _ahNumTotal = new Dictionary<int, ushort>();
            _aBytesIgnor = new List<long>();
            _aBytesHashes = new List<long>();
            _nBytesTotal = 0;
            _dtNextInfo = DateTime.Now.AddMinutes(10);
            _sLogPrefix = sLogPrefix;
        }
        public Bytes BytesGet(int nSize, byte nFrom)
        {
            Bytes cRetVal;
            lock (_ahBytesStorage)
            {
                if (_ahBytesStorage.Keys.Contains(nSize) && 0 < _ahBytesStorage[nSize].Count)
                {
                    _nHash = _ahBytesStorage[nSize].Peek().nID;
                    if (_aBytesHashes.Contains(_nHash))
                    {
                        if (_ahHashPassedOut[_nHash] == false)
                            _ahHashPassedOut[_nHash] = true;
                        else
                            (new Logger()).WriteError(_sLogPrefix + " error - bytes already passed out!!");
                        return _ahBytesStorage[nSize].Dequeue();
                    }
                    (new Logger()).WriteError(_sLogPrefix + " error - bytes not in hashes!");
                }
                else
                {
                    if (!_ahBytesStorage.Keys.Contains(nSize))
                    {
                        (new Logger()).WriteDebug(_sLogPrefix + " adding new size to bytes storage [" + nSize + "] (from=" + nFrom + ")");
                        _ahNumTotal.Add(nSize, 0);
                        _ahBytesStorage.Add(nSize, new Queue<Bytes>());
                    }
                    _nBytesTotal += nSize;
                    _ahNumTotal[nSize]++;
                    cRetVal = new Bytes(nSize);
                    if (DateTime.Now > _dtNextInfo)
                    {
                        (new Logger()).WriteDebug(_sLogPrefix + " info: [sizes=" + _ahNumTotal.Keys.Count + "][total_count=" + _aBytesHashes.Count() + "][bytes=" + _nBytesTotal + "]");
                        _dtNextInfo = DateTime.Now.AddMinutes(10);
                    }
                    (new Logger()).WriteDebug4(_sLogPrefix + " returning new byte array (from=" + nFrom + ")[hc=" + cRetVal.nID + "][" + nSize + "][sizes=" + _ahNumTotal.Keys.Count + "][total=" + _ahNumTotal[nSize] + "(" + _aBytesHashes.Count() + ")][bytes=" + _nBytesTotal + "]");
                    _aBytesHashes.Add(cRetVal.nID);
                    _ahHashPassedOut.Add(cRetVal.nID, true);
                    return cRetVal;
                }
                throw new Exception("bytes getting is impossible");
            }
        }
        public void BytesBack(Bytes cBytes, byte nFrom)
        {
            if (null == cBytes || null == cBytes.aBytes)
            {
                (new Logger()).WriteDebug(_sLogPrefix + "error - received NULL bytes! (from=" + nFrom + ")");
                return;
            }
            lock (_ahBytesStorage)
            {
                if (_aBytesHashes.Contains(cBytes.nID))
                {
                    if (_ahHashPassedOut[cBytes.nID])
                    {
                        _ahHashPassedOut[cBytes.nID] = false;
                        _ahBytesStorage[cBytes.Length].Enqueue(cBytes);
                    }
                    else
                        (new Logger()).WriteError(_sLogPrefix + " error - received twice!!! (from=" + nFrom + ")[size=" + cBytes.Length + "]");
                }
                else if (!_aBytesIgnor.Contains(cBytes.nID))
                    (new Logger()).WriteError(_sLogPrefix + " error - received not our bytes!(from=" + nFrom + ") [hc=" + cBytes.nID + "][size=" + cBytes.Length + "]");
            }
        }
        public void AddToIgnor(Bytes cBytes)
        {
            _aBytesIgnor.Add(cBytes.nID);
        }
        public bool Contains(Bytes cBytes)
        {
            if (null == cBytes || null == cBytes.aBytes)
                return false;
            lock (_ahBytesStorage)
            {
                return _aBytesHashes.Contains(cBytes.nID);
            }
        }


        // ==================================

        private IntPtr pRetVal;
        private GCHandle cGCHandle;
        private byte[] aPinnedBytes;
        private int nHashPinned;
        private long nBytesTotalPinned = 0;
        private Dictionary<IntPtr, byte[]> _ahPointersBytes;
        private Dictionary<IntPtr, GCHandle> _ahPointersHandles;
        private Dictionary<int, Queue<IntPtr>> _ahPinnedBytesStorage;
        private Dictionary<int, ushort> _ahPinnedNumTotal;

        public IntPtr PinnedBytesGet(int nSize, byte nFrom)
        {
            throw new Exception("EDIT CODE BEFORE USING (add public class PinnedBytes)"); // just moved from BTL
            lock (_ahBytesStorage)
            {
                if (_ahPinnedBytesStorage.Keys.Contains(nSize) && 0 < _ahPinnedBytesStorage[nSize].Count)
                {
                    nHashPinned = _ahPinnedBytesStorage[nSize].Peek().GetHashCode();
                    if (_aBytesHashes.Contains(nHashPinned))
                    {
                        if (_ahHashPassedOut[nHashPinned] == false)
                            _ahHashPassedOut[nHashPinned] = true;
                        else
                            (new Logger()).WriteDebug("btl.pinnedbytes error - already passed out!!");
                        return _ahPinnedBytesStorage[nSize].Dequeue();
                    }
                    (new Logger()).WriteDebug("btl.pinnedbytes error - not in hashes!");
                }
                else
                {
                    if (!_ahPinnedBytesStorage.Keys.Contains(nSize))
                    {
                        (new Logger()).WriteDebug("btl.pinnedbytes adding new size to bytes storage [" + nSize + "] (from=" + nFrom + ")");
                        _ahPinnedNumTotal.Add(nSize, 0);
                        _ahPinnedBytesStorage.Add(nSize, new Queue<IntPtr>());
                    }
                    nBytesTotalPinned += nSize;
                    _ahPinnedNumTotal[nSize]++;

                    aPinnedBytes = new byte[nSize];
                    cGCHandle = GCHandle.Alloc(aPinnedBytes, GCHandleType.Pinned);
                    pRetVal = cGCHandle.AddrOfPinnedObject();
                    while (_aBytesHashes.Contains(pRetVal.GetHashCode()))
                    {
                        (new Logger()).WriteDebug("btl.pinnedbytes ERROR returning new byte array WITH THE SAME HASH!!! - will try to get another one (from=" + nFrom + ")[hc=" + pRetVal.GetHashCode() + "][" + nSize + "]");
                        GCHandle cTMP = cGCHandle;
                        cGCHandle = GCHandle.Alloc(aPinnedBytes, GCHandleType.Pinned);
                        pRetVal = cGCHandle.AddrOfPinnedObject();
                        cTMP.Free();
                    }
                    (new Logger()).WriteDebug("btl.pinnedbytes returning new byte array (from=" + nFrom + ")[hc=" + pRetVal.GetHashCode() + "][" + nSize + "][sizes=" + _ahPinnedNumTotal.Keys.Count + "][total=" + _ahPinnedNumTotal[nSize] + "(" + _aBytesHashes.Count() + ")][bytes=" + nBytesTotalPinned + "]");
                    _aBytesHashes.Add(pRetVal.GetHashCode());
                    _ahHashPassedOut.Add(pRetVal.GetHashCode(), true);
                    _ahPointersBytes.Add(pRetVal, aPinnedBytes);
                    _ahPointersHandles.Add(pRetVal, cGCHandle);
                    return pRetVal;
                }
                throw new Exception("pinnedbytes getting is impossible");
            }
        }
        public void PinnedBytesBack(IntPtr pBytes, byte nFrom)
        {
            int nSize;
            if (null == pBytes)
            {
                (new Logger()).WriteDebug("btl.pinnedbytes error - received NULL pointer! (from=" + nFrom + ")");
                return;
            }
            lock (_ahBytesStorage)
            {
                if (_aBytesHashes.Contains(pBytes.GetHashCode()))
                {
                    nSize = _ahPointersBytes[pBytes].Length;
                    if (_ahHashPassedOut[pBytes.GetHashCode()])
                    {
                        _ahHashPassedOut[pBytes.GetHashCode()] = false;
                        _ahPinnedBytesStorage[nSize].Enqueue(pBytes);
                    }
                    else
                        (new Logger()).WriteDebug("btl.pinnedbytes error - received twice!!! (from=" + nFrom + ")[size=" + nSize + "]");
                }
                else
                    (new Logger()).WriteDebug("btl.pinnedbytes error - received not our bytes!(from=" + nFrom + ") [hc=" + pBytes.GetHashCode() + "]");
            }
        }
        public byte[] ByteByPointerGet(IntPtr pBytes)
        {
            lock (_ahBytesStorage)
            {
                if (_ahPointersBytes.Keys.Contains(pBytes))
                    return _ahPointersBytes[pBytes];
                else
                {
                    (new Logger()).WriteDebug("btl.pinnedbytes error - getting not our bytes by pointer! [hc=" + pBytes.GetHashCode() + "]");
                    return null;
                }
            }
        }
    }
    public class SyncConstants
    {
        static public string sFilePauseCopying = "PAUSE_COPYING";
    }
    public class FailoverConstants {
        static public string sFilesDoNotRemoveFromCache = "DO_NOT_REMOVE_NOR_ADD_FROM_CACHE";
        static public string sFileBesiegedFortress = "BESIEGED_FORTRESS";
        static public string[] aPossibleExtensions = new string[5] { ".mxf", ".mov", ".mp4", ".mpg", ".avi" };
        static private object oLockRename = new object();
        static public bool IsFilesDoNotRemoveMode(string sCacheFolder, out string sLogInfo)
        {
            sLogInfo = "";
            string sDoNotRemoveFile = System.IO.Path.Combine(sCacheFolder, sFilesDoNotRemoveFromCache);
            string sBesiegedFortressFile = System.IO.Path.Combine(sCacheFolder, sFileBesiegedFortress);
            bool bDoNotRemoveFile = System.IO.File.Exists(sDoNotRemoveFile);
            if (bDoNotRemoveFile)
                sLogInfo += "[file exists = " + sDoNotRemoveFile + "]";
            bool bBesiegedFortressFile = System.IO.File.Exists(sBesiegedFortressFile);
            if (bBesiegedFortressFile)
                sLogInfo += "[file exists = " + sBesiegedFortressFile + "]";
            return bDoNotRemoveFile || bBesiegedFortressFile;
        }
        static public bool IsBesiegedFortressMode(string sCacheFolder)
        {
            string sBesiegedFortressFile = System.IO.Path.Combine(sCacheFolder, sFileBesiegedFortress);
            return System.IO.File.Exists(sBesiegedFortressFile);
        }
        static public bool IsBesiegedFortressMode(string sCacheFolder, out string sLogInfo)
        {
            sLogInfo = "";
            string sBesiegedFortressFile = System.IO.Path.Combine(sCacheFolder, sFileBesiegedFortress);
            bool bBesiegedFortressFile = System.IO.File.Exists(sBesiegedFortressFile);
            if (bBesiegedFortressFile)
                sLogInfo += "[file exists = " + sBesiegedFortressFile + "]";
            return bBesiegedFortressFile;
        }
        static private bool CreateOrRenameFile(string sFile, string sText)
        {
            string sRenamedFile = sFile + "!";
            lock (oLockRename)
            {
                if (System.IO.File.Exists(sFile))
                    return true;

                if (System.IO.File.Exists(sRenamedFile))
                {
                    System.IO.File.AppendAllText(sRenamedFile, "\n" + sText);
                    System.IO.File.Move(sRenamedFile, sFile);
                }
                else
                    System.IO.File.WriteAllText(sFile, sText);

                System.Threading.Thread.Sleep(100);
                if (System.IO.File.Exists(sFile))
                {
                    System.IO.File.SetLastWriteTime(sFile, DateTime.Now);
                    return true;
                }
                return false;
            }
        }
        static public bool EnterFilesDoNotRemoveMode(string sCacheFolder, string sText)
        {
            string sFile = System.IO.Path.Combine(sCacheFolder, sFilesDoNotRemoveFromCache);
            return CreateOrRenameFile(sFile, sText);
        }
        static public bool EnterBesiegedFortressMode(string sCacheFolder, string sText)
        {
            string sFile = System.IO.Path.Combine(sCacheFolder, sFileBesiegedFortress);
            return CreateOrRenameFile(sFile, sText);
        }
        static public bool ExitFilesDoNotRemoveMode(string sCacheFolder)
        {
            string sFile = System.IO.Path.Combine(sCacheFolder, sFilesDoNotRemoveFromCache);
            if (System.IO.File.Exists(sFile))
                System.IO.File.Move(sFile, sFile + "!");
            System.Threading.Thread.Sleep(100);
            if (System.IO.File.Exists(sFile))
                return false;
            return true;
        }
        static public bool SyncerWasProbablyStopped(string sCacheFolder, int nMinutesThreshold)
        {
            System.IO.DirectoryInfo cDir = new System.IO.DirectoryInfo(sCacheFolder);
            DateTime dtThreshold = DateTime.Now.AddMinutes(-nMinutesThreshold);

            foreach (System.IO.FileInfo cFile in cDir.GetFiles())
            {
                if (aPossibleExtensions.Contains(cFile.Extension.ToLower()) && cFile.CreationTime > dtThreshold)
                    return false;
            }
            return true;
        }
    }
    public class ClassSaveLoadFromFile
    {
        static public bool Save(object oClass, string sInfoFile)
        {
            BinaryFormatter cBF = new BinaryFormatter();
            bool bRetVal = false;
            try
            {
                (new Logger()).WriteNotice("The class saving to file [" + sInfoFile + "]");
                FileStream cFS = new FileStream(sInfoFile + "!", FileMode.Create, FileAccess.Write);
                cBF.Serialize(cFS, oClass);
                cFS.Close();

                if (File.Exists(sInfoFile))
                {
                    if (File.Exists(sInfoFile + ".bkp"))
                        File.Delete(sInfoFile + ".bkp");
                    Thread.Sleep(50);
                    File.Move(sInfoFile, sInfoFile + ".bkp");
                }
                Thread.Sleep(50);
                File.Move(sInfoFile + "!", sInfoFile);
                bRetVal = true;
                (new Logger()).WriteNotice("The class saved to file [" + sInfoFile + "]");
            }
            catch (Exception ex)
            {
                (new Logger()).WriteError("Unable to save the class to file [" + sInfoFile + "]", ex);
            }
            return bRetVal;
        }
        static public object Load(string sInfoFile)
        {
            BinaryFormatter cBF = new BinaryFormatter();
            object cRetVal = null;
            (new Logger()).WriteNotice("The class loading from file [" + sInfoFile + "]");
            if (File.Exists(sInfoFile))
            {
                try
                {
                    FileStream readerFileStream = new FileStream(sInfoFile, FileMode.Open, FileAccess.Read);
                    cRetVal = cBF.Deserialize(readerFileStream);
                    readerFileStream.Close();
                    (new Logger()).WriteNotice("The class loaded from file [" + sInfoFile + "]");
                }
                catch (Exception ex)
                {
                    (new Logger()).WriteError("The class not loaded from file [" + sInfoFile + "]", ex);
                }
            }
            else
                (new Logger()).WriteError("No file. The class not loaded from file [" + sInfoFile + "]");
            return cRetVal;
        }
    }
}
