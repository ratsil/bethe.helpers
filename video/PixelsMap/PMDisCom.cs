using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;


namespace helpers
{
    public partial class PixelsMap
    {
        private class DisComExternalWorkers
        {
            static private Dictionary<int, DisComExternalWorkers> _ahMergingDeviceNumbers_Workers = new Dictionary<int, DisComExternalWorkers>();
            static private int _nCopyChunkSize = 1024 * 200;

            private int _nCopyInSize;
            private int _nCopyOutSize;
            private int _nN;
            private ThreadBufferQueue<byte[]> _aqCopyInStock;
            private ThreadBufferQueue<byte[]> _aqCopyInQueue;
            private ThreadBufferQueue<byte[]> _aqCopyOutStock;
            private ThreadBufferQueue<byte[]> _aqCopyOutQueue;
            private ManualResetEvent _cCopyInWait;
            private ManualResetEvent _cCopyOutWait;
            private NamedPipeClientStream _cPipeCopyIn;
            private IntPtr _pCopyOutCurrent;
            private Dictionary<long, long> _ahPMIDs_DevicePointers;
            private ThreadBufferQueue<Command> _aqCopyIn;
            private ThreadBufferQueue<Command> _aqMerging;
            private ThreadBufferQueue<Command> _aqCopyOut;

            public DisComExternalWorkers(int nMergingDeviceNumber)
            {
                if (_ahMergingDeviceNumbers_Workers.ContainsKey(nMergingDeviceNumber))
                    throw new Exception("workers with this number [" + nMergingDeviceNumber + "] already exists");

                _nN = nMergingDeviceNumber;
                _ahMergingDeviceNumbers_Workers.Add(_nN, this);
                _ahPMIDs_DevicePointers = new Dictionary<long, long>();
                _aqCopyIn = new ThreadBufferQueue<Command>(false, true);
                _aqMerging = new ThreadBufferQueue<Command>(false, true);
                _aqCopyOut = new ThreadBufferQueue<Command>(false, true);

                Thread cCopyInWorker = new Thread(MainWorker);
                cCopyInWorker.Priority = ThreadPriority.Normal;
                cCopyInWorker.Start();
            }
            private void MainWorker()
            {
                try
                {
                    Thread cCopyInWorker = new Thread(CopyInWorker);
                    cCopyInWorker.Priority = ThreadPriority.Normal;
                    cCopyInWorker.Start();
                    Thread cMergingWorker = new Thread(MergingWorker);
                    cMergingWorker.Priority = ThreadPriority.Normal;
                    cMergingWorker.Start();
                    Thread cCopyOutWorker = new Thread(CopyOutWorker);
                    cCopyOutWorker.Priority = ThreadPriority.Normal;
                    cCopyOutWorker.Start();

                    Command cCmd;
                    while (true)
                    {
                        try
                        {
                            cCmd = _ahMergingHash_CommandsQueue[_nN].Dequeue();  //если нечего отдать - заснёт
                            switch (cCmd.eID)
                            {
                                case Command.ID.Allocate:
                                    _aqCopyIn.Enqueue(cCmd);
                                    break;
                                case Command.ID.CopyIn:
                                    _aqCopyIn.Enqueue(cCmd);
                                    break;
                                case Command.ID.CopyOut:
                                    _aqCopyOut.Enqueue(cCmd);
                                    break;
                                case Command.ID.Merge:
                                    _aqMerging.Enqueue(cCmd);
                                    break;
                                case Command.ID.Dispose:
                                    _aqCopyOut.Enqueue(cCmd);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Logger("DisCom-" + _nN)).WriteError(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("DisCom-" + _nN)).WriteError(ex);
                }
                finally
                {
                    (new Logger("DisCom-" + _nN)).WriteNotice("PIPE client STOPPED!");
                }
            }

            private void CopyInWorker()
            {
                try
                {
                    _cCopyInWait = new ManualResetEvent(false);
                    Thread cCopyHelperWorker = new Thread(CopyInHelperWorker);
                    cCopyHelperWorker.Priority = ThreadPriority.Normal;
                    cCopyHelperWorker.Start();

                    (new Logger("DisCom-CopyInWorker-" + _nN)).WriteNotice("Starting PIPE client [" + "DisComPipe-CopyIn-" + _nN + "] and waiting for the server...");
                    _cPipeCopyIn = new NamedPipeClientStream("DisComPipe-CopyIn-" + _nN);
                    _cPipeCopyIn.Connect();
                    (new Logger("DisCom-CopyInWorker-" + _nN)).WriteNotice("PIPE client connected to Server");

                    //StreamWriter cSW = new StreamWriter(_cPipeStream);
                    //cSW.AutoFlush = true;
                    //StreamReader cSR = new StreamReader(_cPipeStream);
                    //System.IO.StringWriter cStringWriter = new System.IO.StringWriter();
                    //StringBuilder cStringBuilder = cStringWriter.GetStringBuilder();
                    //BinaryWriter cBB = new BinaryWriter(_cPipeStream);
                    BinaryFormatter cBinFormatter = new BinaryFormatter();

                    int nLastChunkSize, nChunksQty, nAllChunksSize, nCurrentSize;
                    long nDevPointer;
                    byte[] aChunk;
                    Command cCmd;
                    IntPtr pCopyInCurrent;

                    cBinFormatter.Serialize(_cPipeCopyIn, _nCopyChunkSize);

                    while (true)
                    {
                        cCmd = null;
                        try
                        {
                            cCmd = _aqCopyIn.Dequeue();  //если нечего отдать - заснёт
                            cCmd.cPM._cException = null;
                            switch (cCmd.eID)
                            {
                                case Command.ID.Allocate:
                                    #region
                                    if (1 > cCmd.cPM._nID)
                                    {
                                        if (0 < cCmd.cPM._nBytesQty)
                                        {
                                            (new Logger("DisCom-CopyInWorker-" + _nN)).WriteDebug3("pixelmap allocate [current_id=" + _nCurrentID + "]");
                                            cCmd.cPM._nID = Interlocked.Increment(ref _nCurrentID);
                                            _cPipeCopyIn.WriteByte((byte)Command.ID.Allocate);
                                            cBinFormatter.Serialize(_cPipeCopyIn, (int)cCmd.cPM._nBytesQty);
                                            lock (_ahPMIDs_DevicePointers)
                                                _ahPMIDs_DevicePointers.Add(cCmd.cPM._nID, (long)cBinFormatter.Deserialize(_cPipeCopyIn));
                                        }
                                        else
                                            throw new Exception("bytes quantity in PixelsMap have to be greater than zero for Allocate [_bDisposed = " + cCmd.cPM._bDisposed + "][_bProcessing = " + cCmd.cPM._bProcessing + "][_stPosition.X = " + cCmd.cPM._stPosition.X + "][_stPosition.Y = " + cCmd.cPM._stPosition.Y + "][_bTemp = " + cCmd.cPM._bTemp + "][_dt = " + cCmd.cPM._dtCreate + "][_nBytesQty = " + cCmd.cPM._nBytesQty + "][_nID = " + cCmd.cPM._nID + "][_nShiftTotalX = " + cCmd.cPM._nShiftTotalX + "][_stArea.nHeight = " + cCmd.cPM._stArea.nHeight + "][_stArea.nWidth = " + cCmd.cPM._stArea.nWidth + "][bKeepAlive = " + cCmd.cPM.bKeepAlive + "][eAlpha = " + cCmd.cPM.eAlpha + "][bCUDA = " + cCmd.cPM.stMergingMethod + "][nAlphaConstant = " + cCmd.cPM.nAlphaConstant + "][nID = " + cCmd.cPM.nID + "][nLength = " + cCmd.cPM.nLength + "][stArea.nHeight = " + cCmd.cPM.stArea.nHeight + "][stArea.nWidth = " + cCmd.cPM.stArea.nWidth + "]");
                                    }
                                    else
                                        throw new Exception("PixelsMap ID have to be zero for Allocate");
                                    #endregion
                                    break;
                                case Command.ID.CopyIn:
                                    #region
                                    if (1 > cCmd.cPM._nID)  // not allocated
                                    {
                                        (new Logger("DisCom-CopyInWorker-" + _nN)).WriteDebug3("pixelmap copyin not allocated [pm_id=" + _nCurrentID + "]");
                                        cCmd.cPM._nID = Interlocked.Increment(ref _nCurrentID);
                                        _cPipeCopyIn.WriteByte((byte)Command.ID.Allocate);
                                        cBinFormatter.Serialize(_cPipeCopyIn, (int)cCmd.cPM._nBytesQty);
                                        lock (_ahPMIDs_DevicePointers)
                                            _ahPMIDs_DevicePointers.Add(cCmd.cPM._nID, (long)cBinFormatter.Deserialize(_cPipeCopyIn));
                                    }

                                    if (!cCmd.ahParameters.ContainsKey(typeof(IntPtr)) && !cCmd.ahParameters.ContainsKey(typeof(byte[])))
                                        throw new Exception("unknown parameter type");

                                    (new Logger("DisCom-CopyInWorker-" + _nN)).WriteDebug4("pixelmap copyin [pm_id=" + cCmd.cPM._nID + "]");
                                    _cPipeCopyIn.WriteByte((byte)Command.ID.CopyIn);
                                    lock (_ahPMIDs_DevicePointers)
                                        nDevPointer = _ahPMIDs_DevicePointers[cCmd.cPM._nID];
                                    cBinFormatter.Serialize(_cPipeCopyIn, nDevPointer);
                                    if (cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
                                    {
                                        _cPipeCopyIn.WriteByte(1);
                                        pCopyInCurrent = (IntPtr)cCmd.ahParameters[typeof(IntPtr)];
                                        _nCopyInSize = (int)cCmd.cPM._nBytesQty;
                                        nChunksQty = _nCopyInSize / _nCopyChunkSize;
                                        nLastChunkSize = _nCopyInSize % _nCopyChunkSize;
                                        nAllChunksSize = _nCopyChunkSize * nChunksQty;
                                        nCurrentSize = _nCopyChunkSize;
                                        int nOffset;
                                        for (nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyChunkSize)
                                        {
                                            if (nOffset == nAllChunksSize)
                                            {
                                                if (nLastChunkSize > 0)
                                                    nCurrentSize = nLastChunkSize;
                                                else
                                                    break;
                                            }
                                            aChunk = _aqCopyInStock.Dequeue();
                                            Marshal.Copy(IntPtr.Add(pCopyInCurrent, nOffset), aChunk, 0, nCurrentSize);
                                            _aqCopyInQueue.Enqueue(aChunk);
                                        }
                                        _cCopyInWait.WaitOne();
                                        _cCopyInWait.Reset();
                                    }
                                    else if (cCmd.ahParameters.ContainsKey(typeof(byte[]))) // else if === else
                                    {
                                        _cPipeCopyIn.WriteByte(0);
                                        _cPipeCopyIn.Write((byte[])cCmd.ahParameters[typeof(byte[])], 0, (int)cCmd.cPM._nBytesQty);
                                    }
                                    #endregion
                                    break;
                                default:
                                    cCmd = null;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Logger("DisCom-CopyInWorker-" + _nN)).WriteError("in switch command [cmd:" + cCmd?.eID + "][bytes_qty:" + cCmd?.cPM._nBytesQty + "]  ", ex);
                            if (null != cCmd)
                                cCmd.cPM._cException = ex;
                        }
                        cCmd?.cMRE.Set();
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("DisCom-CopyInWorker-" + _nN)).WriteError(ex);
                }
                finally
                {
                    (new Logger("DisCom-CopyInWorker-" + _nN)).WriteNotice("worker STOPPED!");
                }
            }
            private void MergingWorker()
            {
                try
                {
                    (new Logger("DisCom-MergingWorker-" + _nN)).WriteNotice("Starting PIPE client [" + "DisComPipe-Merging-" + _nN + "] and waiting for the server...");
                    NamedPipeClientStream cPipeMerging;
                    cPipeMerging = new NamedPipeClientStream("DisComPipe-Merging-" + _nN);
                    cPipeMerging.Connect();
                    (new Logger("DisCom-MergingWorker-" + _nN)).WriteNotice("MergingWorker PIPE client connected to Server");
                    BinaryFormatter cBinFormatter = new BinaryFormatter();

                    Command cCmd;
                    bool bSet;

                    cBinFormatter.Serialize(cPipeMerging, _nCopyChunkSize);

                    while (true)
                    {
                        cCmd = null;
                        bSet = false;
                        try
                        {
                            cCmd = _aqMerging.Dequeue();  //если нечего отдать - заснёт
                            cCmd.cPM._cException = null;
                            switch (cCmd.eID)
                            {
                                case Command.ID.Merge:
                                    #region
                                    List<PixelsMap> aPMs = (List<PixelsMap>)cCmd.ahParameters[typeof(List<PixelsMap>)];
                                    DisCom.MergeInfo cMergeInfo = (DisCom.MergeInfo)cCmd.ahParameters[typeof(DisCom.MergeInfo)];
                                    List<long> aDPs = new List<long>();

                                    if (1 > cCmd.cPM._nID)
                                        throw new Exception("background PixelsMap have to be allocated for Merge");

                                    lock (_ahPMIDs_DevicePointers)
                                    {
                                        aDPs.Add(_ahPMIDs_DevicePointers[cCmd.cPM._nID]);
                                        for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                                        {
                                            if (!_ahPMIDs_DevicePointers.ContainsKey(aPMs[nIndx]._nID))
                                                throw new Exception("there is a corrupted ID in layers for merge [pm_id:" + aPMs[nIndx]._nID + "]");
                                            if (1 > _ahPMIDs_DevicePointers[aPMs[nIndx]._nID])
                                                throw new Exception("there is an empty pointer in layers for merge [pm_id:" + aPMs[nIndx]._nID + "]");
                                            aDPs.Add(_ahPMIDs_DevicePointers[aPMs[nIndx]._nID]);
                                        }
                                    }
                                    cPipeMerging.WriteByte((byte)Command.ID.Merge);
                                    cBinFormatter.Serialize(cPipeMerging, aDPs);
                                    cBinFormatter.Serialize(cPipeMerging, cMergeInfo);
                                    cPipeMerging.ReadByte();

                                    cCmd.cMRE.Set();
                                    bSet = true;

                                    cMergeInfo.Dispose();
                                    for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                                    {
                                        lock (aPMs[nIndx]._cSyncRoot)
                                            aPMs[nIndx]._bProcessing = false;
                                        aPMs[nIndx].Dispose();
                                    }
                                    #endregion
                                    break;
                                default:
                                    cCmd = null;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Logger("DisCom-MergingWorker-" + _nN)).WriteError("in switch command [cmd:" + cCmd?.eID + "][bytes_qty:" + cCmd?.cPM._nBytesQty + "]  ", ex);
                            if (null != cCmd)
                            {
                                cCmd.cPM._cException = ex;
                                if (!bSet)
                                    cCmd.cMRE.Set();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("DisCom-MergingWorker-" + _nN)).WriteError("in MergingWorker", ex);
                }
                finally
                {
                    (new Logger("DisCom-MergingWorker-" + _nN)).WriteNotice("merging worker STOPPED!");
                }
            }
            private void CopyOutWorker()
            {
                try
                {
                    _cCopyOutWait = new ManualResetEvent(false);
                    Thread cCopyHelperWorker = new Thread(CopyOutHelperWorker);
                    cCopyHelperWorker.Priority = ThreadPriority.Normal;
                    cCopyHelperWorker.Start();

                    (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteNotice("CopyOutWorker Starting PIPE client and waiting for the server...");
                    NamedPipeClientStream cPipeCopyOut;
                    cPipeCopyOut = new NamedPipeClientStream("DisComPipe-CopyOut-" + _nN);
                    cPipeCopyOut.Connect();
                    (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteNotice("CopyOutWorker PIPE client connected to Server");
                    BinaryFormatter cBinFormatter = new BinaryFormatter();

                    int nLastChunkSize, nChunksQty, nAllChunksSize, nCurrentSize;
                    long nDevicePointer;
                    byte[] aChunk;
                    Command cCmd;

                    cBinFormatter.Serialize(cPipeCopyOut, _nCopyChunkSize);

                    while (true)
                    {
                        cCmd = null;
                        try
                        {
                            cCmd = _aqCopyOut.Dequeue();  //если нечего отдать - заснёт
                            cCmd.cPM._cException = null;
                            switch (cCmd.eID)
                            {
                                case Command.ID.CopyOut:
                                    #region
                                    if (0 < cCmd.cPM._nID)
                                    {
                                        cPipeCopyOut.WriteByte((byte)Command.ID.CopyOut);
                                        lock (_ahPMIDs_DevicePointers)
                                            cBinFormatter.Serialize(cPipeCopyOut, _ahPMIDs_DevicePointers[cCmd.cPM._nID]);
                                        if (!cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
                                        {
                                            cPipeCopyOut.WriteByte(0);
                                            if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
                                            {
                                                byte[] aB = (byte[])cCmd.ahParameters[typeof(byte[])];
                                                cCmd.cPM._aBytes = null;
                                                if (cCmd.cPM._nBytesQty != aB.Length)
                                                    (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteError("wrong array size for copyout [id:" + cCmd.cPM._nID + "][got:" + aB.Length + "][expected:" + cCmd.cPM._nBytesQty + "]");

                                                cPipeCopyOut.Read(aB, 0, aB.Length);
                                            }
                                            else  // не юзается (см. copyout())
                                            {
                                                cCmd.cPM._aBytes = _cBinM.BytesGet((int)cCmd.cPM._nBytesQty, 33);
                                                cPipeCopyOut.Read(cCmd.cPM._aBytes.aBytes, 0, cCmd.cPM._aBytes.Length);
                                            }
                                        }
                                        else
                                        {
                                            cPipeCopyOut.WriteByte(1);
                                            _pCopyOutCurrent = (IntPtr)cCmd.ahParameters[typeof(IntPtr)];
                                            _nCopyOutSize = (int)cCmd.cPM._nBytesQty;
                                            nChunksQty = _nCopyOutSize / _nCopyChunkSize;
                                            nLastChunkSize = _nCopyOutSize % _nCopyChunkSize;
                                            nAllChunksSize = _nCopyChunkSize * nChunksQty;
                                            nCurrentSize = _nCopyChunkSize;
                                            int nOffset;
                                            for (nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyChunkSize)
                                            {
                                                if (nOffset == nAllChunksSize)
                                                {
                                                    if (nLastChunkSize > 0)
                                                        nCurrentSize = nLastChunkSize;
                                                    else
                                                        break;
                                                }
                                                aChunk = _aqCopyOutStock.Dequeue();
                                                cPipeCopyOut.Read(aChunk, 0, nCurrentSize);
                                                _aqCopyOutQueue.Enqueue(aChunk);
                                            }
                                            _cCopyOutWait.WaitOne();
                                            _cCopyOutWait.Reset();
                                        }
                                    }
                                    else
                                        throw new Exception("PixelsMap have to be allocated for CopyOut");
                                    #endregion
                                    break;
                                case Command.ID.Dispose:
                                    #region
                                    (new Logger("DisCom-CopyOutWorker-" + _nN)).Write(Logger.Level.debug2, "dispose: in");
                                    nDevicePointer = -1;
                                    lock (_ahPMIDs_DevicePointers)
                                    {
                                        if (_ahPMIDs_DevicePointers.ContainsKey(cCmd.cPM._nID))
                                            nDevicePointer = _ahPMIDs_DevicePointers[cCmd.cPM._nID];
                                    }
                                    if (nDevicePointer > 0)
                                    {
                                        if (0 < cCmd.cPM._nID && 0 < nDevicePointer)
                                        {
                                            cPipeCopyOut.WriteByte((byte)Command.ID.Dispose);
                                            cBinFormatter.Serialize(cPipeCopyOut, nDevicePointer);
                                            (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteDebug3("dispose [pm_id:" + cCmd.cPM._nID + "][ptr:" + nDevicePointer + "]");
                                        }
                                        lock (_ahPMIDs_DevicePointers)
                                            _ahPMIDs_DevicePointers.Remove(cCmd.cPM._nID);
                                        cCmd.cPM._nID = 0;
                                    }
                                    (new Logger("DisCom-CopyOutWorker-" + _nN)).Write(Logger.Level.debug3, "dispose: out [pm_id=" + cCmd.cPM._nID + "]");
                                    #endregion
                                    break;
                                default:
                                    cCmd = null;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteError("in switch command [cmd:" + cCmd?.eID + "][bytes_qty:" + cCmd?.cPM._nBytesQty + "]  ", ex);
                            if (null != cCmd)
                                cCmd.cPM._cException = ex;
                        }
                        if (null != cCmd && cCmd.eID == Command.ID.CopyOut)
                            cCmd.cMRE.Set();
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteError("in CopyOutWorker", ex);
                }
                finally
                {
                    (new Logger("DisCom-CopyOutWorker-" + _nN)).WriteNotice("copy out worker STOPPED!");
                }
            }

            private void CopyInHelperWorker()
            {
                int nLastChunkSize, nChunksQty, nAllChunksSize, nCurrentSize;
                byte[] aChunk;
                _aqCopyInQueue = new ThreadBufferQueue<byte[]>(20, false, true);
                _aqCopyInStock = new ThreadBufferQueue<byte[]>(20, false, true);
                for (int nI = 0; nI < 20; nI++)
                    _aqCopyInStock.Enqueue(new byte[_nCopyChunkSize]);


                while (true)
                {
                    try
                    {
                        aChunk = _aqCopyInQueue.Dequeue();

                        nChunksQty = _nCopyInSize / _nCopyChunkSize;
                        nLastChunkSize = _nCopyInSize % _nCopyChunkSize;
                        nAllChunksSize = _nCopyChunkSize * nChunksQty;
                        nCurrentSize = _nCopyChunkSize;
                        for (int nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyChunkSize)
                        {
                            if (nOffset == nAllChunksSize)
                            {
                                if (nLastChunkSize > 0)
                                    nCurrentSize = nLastChunkSize;
                                else
                                    break;
                            }
                            if (nOffset > 0)
                                aChunk = _aqCopyInQueue.Dequeue();

                            _cPipeCopyIn.Write(aChunk, 0, nCurrentSize);
                            _aqCopyInStock.Enqueue(aChunk);
                        }
                        if (_aqCopyInQueue.nCount > 0)
                        {
                            (new Logger("DisCom-CopyInHelperWorker-" + _nN)).WriteError("in CopyInHelperWorker. queue contains elements after work [" + _aqCopyInQueue.nCount + "][_nCopyInSize=" + _nCopyInSize + "][nChunksQty=" + nChunksQty + "][nLastChunkSize=" + nLastChunkSize + "]");
                        }
                        _cCopyInWait.Set();
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DisCom-CopyInHelperWorker-" + _nN)).WriteError("in CopyInHelperWorker", ex);
                    }
                }
            }
            private void CopyOutHelperWorker()
            {
                int nLastChunkSize, nChunksQty, nAllChunksSize, nCurrentSize;
                byte[] aChunk;
                _aqCopyOutQueue = new ThreadBufferQueue<byte[]>(20, false, true);
                _aqCopyOutStock = new ThreadBufferQueue<byte[]>(20, false, true);
                for (int nI = 0; nI < 20; nI++)
                    _aqCopyOutStock.Enqueue(new byte[_nCopyChunkSize]);


                while (true)
                {
                    try
                    {
                        aChunk = _aqCopyOutQueue.Dequeue();

                        nChunksQty = _nCopyOutSize / _nCopyChunkSize;
                        nLastChunkSize = _nCopyOutSize % _nCopyChunkSize;
                        nAllChunksSize = _nCopyChunkSize * nChunksQty;
                        nCurrentSize = _nCopyChunkSize;
                        for (int nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyChunkSize)
                        {
                            if (nOffset == nAllChunksSize)
                            {
                                if (nLastChunkSize > 0)
                                    nCurrentSize = nLastChunkSize;
                                else
                                    break;
                            }
                            if (nOffset > 0)
                                aChunk = _aqCopyOutQueue.Dequeue();

                            Marshal.Copy(aChunk, 0, IntPtr.Add(_pCopyOutCurrent, nOffset), nCurrentSize);
                            _aqCopyOutStock.Enqueue(aChunk);
                        }
                        if (_aqCopyOutQueue.nCount > 0)
                        {
                            (new Logger("DisCom-CopyInHelperWorker-" + _nN)).WriteError("in CopyOutHelperWorker. queue contains elements after work [" + _aqCopyOutQueue.nCount + "][_nCopyOutSize=" + _nCopyOutSize + "][nChunksQty=" + nChunksQty + "][nLastChunkSize=" + nLastChunkSize + "]");
                        }
                        _cCopyOutWait.Set();
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DisCom-CopyInHelperWorker-" + _nN)).WriteError("in CopyOutHelperWorker", ex);
                    }
                }
            }
        }
    }
}