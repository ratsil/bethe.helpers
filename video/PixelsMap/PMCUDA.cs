#define CUDA
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
#if CUDA
using GASS.CUDA;
using GASS.CUDA.Types;
#endif
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace helpers
{
    public partial class PixelsMap
    {
        static private bool _bMemoryStarvation = false;
        static private object _oMemoryStarvationSync = new object();
        static public bool bMemoryStarvation
        {
            get
            {
                lock (_oMemoryStarvationSync)
                    return _bMemoryStarvation;
            }
            private set
            {
                bool bValue;
                lock (_oMemoryStarvationSync)
                {
                    bValue = _bMemoryStarvation;
                    _bMemoryStarvation = value;
                }
                if (value != bValue)
                    (new Logger("CUDA")).WriteWarning("CUDA memory starvation " + (value ? "starts" : "stops"));
            }
        }
        private class CUDAWorkers
        {
            enum ThreadsCount: int  // must be either 3 or 1 (parallel cuda or linear)
            {
                Single = 1,
                Triple = 3
            }
            private class CUDAWorker
            {
                private byte _nIndex;
                private int _nMergingDeviceNumber;
                public ThreadBufferQueue<Command> aqQueue;
                public CUDAWorker(int nMergingDeviceNumber, byte nIndex)
                {
                    _nMergingDeviceNumber = nMergingDeviceNumber;
                    _nIndex = nIndex;
                    aqQueue = new ThreadBufferQueue<Command>(false, true);
                    Thread cCopyInWorker = new Thread(Worker);
                    cCopyInWorker.Priority = ThreadPriority.Normal;
                    cCopyInWorker.Start();
                }
                private void Worker()
                {
#if CUDA
                    int nN = _nMergingDeviceNumber;
                    string sS = "CUDA-" + nN + "-" + _nIndex;
                    try
                    {
                        Command cCmd;
                        CUDA cCUDA;
                        #region CUDA Init
                        try
                        {
                            cCUDA = new CUDA(true);
                            cCUDA.CreateContext(nN % 1000); // nober of cuda in prefs (still alwais 0)
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("CreateContext(" + nN % 1000 + ") error. Try to change CUDA's card number in prefs", ex);
                        }
                        (new Logger(sS)).WriteDebug("CreateContext(" + nN % 1000 + ") is ok!");

                        uint nMemoryReservedForMerge = 2 * 1024 * 1024; //PREFERENCES типа <memory reserved="2097152" />
                        uint nMemoryStarvationThreshold = cCUDA.TotalMemory / 2; //PREFERENCES через проценты... типа <memory starvation="50%" />
                        uint nMemoryFree;
                        string sModule = "CUDAFunctions_" + Preferences.nCUDAVersion + "_x" + (IntPtr.Size * 8);
                        if (Logger.bDebug)
                            (new Logger(sS)).WriteDebug(sModule + "   Current CUDA = [name=" + cCUDA.CurrentDevice.Name + "][compute_capability=" + cCUDA.CurrentDevice.ComputeCapability + "]");
                        cCUDA.LoadModule((byte[])Properties.Resource.ResourceManager.GetObject(sModule)); //   $(ProjectDir)Resources\CUDAFunctions.cubin 
                        CUfunction cCUDAFuncMerge = cCUDA.GetModuleFunction("CUDAFrameMerge");
                        int nThreadsPerBlock = 16;  //32 //256 //пришлось уменьшить с 512 до 256 сридов на блок, потому что при добавлении "движения" и операций с float, ловил ошибку: Too Many Resources Requested for Launch (This error means that the number of registers available on the multiprocessor is being exceeded. Reduce the number of threads per block to solve the problem)
                        cCUDA.SetFunctionBlockShape(cCUDAFuncMerge, nThreadsPerBlock, nThreadsPerBlock, 1);
                        CUDADriver.cuParamSetSize(cCUDAFuncMerge, 8);

                        Dictionary<long, CUdeviceptr> ahPMIDs_DevicePointers = new Dictionary<long, CUdeviceptr>();
                        CUdeviceptr cPMs;
                        CUdeviceptr cInfos;
                        CUdeviceptr cAlphaMap;
                        CUdeviceptr cAlphaMap_info3d;
                        CUdeviceptr cAlphaMap_info2d;
                        if (true)
                        {
                            //IntPtr[] aPointersByAlpha = new IntPtr[254];  //те самые поинтеры-альфы. Ссылаются на массивы поинтеров B, т.е. BackGrounds
                            //IntPtr[] aPointersByBackground = new IntPtr[256];   //  те самые массивы поинтеров B, т.е. BackGrounds
                            byte[] aAlphaMap = new byte[(byte.MaxValue - 1) * (byte.MaxValue + 1) * (byte.MaxValue + 1)];
                            int[] aAlphaMap_info3d = new int[254]; // начала 2d слоёв
                            ushort[] aAlphaMap_info2d = new ushort[256]; // начала строк в одном 2d
                            int nResult, nIndx = 0, nIndxInfo = 0, nIndx2d = 0;
                            for (byte nAlpha = 1; 255 > nAlpha; nAlpha++)
                            {
                                aAlphaMap_info3d[nIndxInfo++] = nIndx;
                                for (ushort nBackground = 0; 256 > nBackground; nBackground++)
                                {
                                    if (nAlpha == 1)
                                        aAlphaMap_info2d[nIndx2d++] = (ushort)nIndx;
                                    for (ushort nForeground = 0; 256 > nForeground; nForeground++)
                                    {
                                        if (255 < (nResult = (int)((float)(nAlpha * (nForeground - nBackground)) / 255 + nBackground + 0.5)))
                                            nResult = 255;
                                        aAlphaMap[nIndx++] = (byte)nResult;
                                    }
                                    //aPointersByBackground[nBackground] = (IntPtr)cCUDA.CopyHostToDevice<byte>(aResults).Pointer;
                                }
                                //aPointersByAlpha[nAlpha - 1] = (IntPtr)cCUDA.CopyHostToDevice<IntPtr>(aPointersByBackground).Pointer;
                            }
                            cAlphaMap_info3d = cCUDA.CopyHostToDevice<int>(aAlphaMap_info3d);
                            cAlphaMap = cCUDA.CopyHostToDevice<byte>(aAlphaMap);
                            cAlphaMap_info2d = cCUDA.CopyHostToDevice<ushort>(aAlphaMap_info2d);
                        }
                        CUdeviceptr cAlphaMap2;
                        CUdeviceptr cAlphaMap2_info2d;
                        {
                            byte[] aAlphaMap2 = new byte[(byte.MaxValue - 1) * (byte.MaxValue - 1)];
                            ushort[] aAlphaMap2_info2d = new ushort[254];
                            int nIndx = 0, nIndx2d = 0;
                            for (byte nFGColorAlpha = 1; 255 > nFGColorAlpha; nFGColorAlpha++)  // можно использовать симметрию умножения, но х с ней пока
                            {
                                aAlphaMap2_info2d[nIndx2d++] = (ushort)nIndx;
                                for (byte nPixelAlpha = 1; 255 > nPixelAlpha; nPixelAlpha++)
                                {
                                    aAlphaMap2[nIndx++] = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
                                }
                            }
                            cAlphaMap2 = cCUDA.CopyHostToDevice<byte>(aAlphaMap2);
                            cAlphaMap2_info2d = cCUDA.CopyHostToDevice<ushort>(aAlphaMap2_info2d);
                        }
                        CUdeviceptr cAlphaMap3;
                        CUdeviceptr cAlphaMap3_info2d;
                        {
                            byte[] aAlphaMap3 = new byte[byte.MaxValue * (byte.MaxValue - 1)];
                            ushort[] aAlphaMap3_info2d = new ushort[255];
                            int nIndx = 0, nIndx2d = 0;
                            for (ushort nFGColorAlpha = 1; 256 > nFGColorAlpha; nFGColorAlpha++)
                            {
                                aAlphaMap3_info2d[nIndx2d++] = (ushort)nIndx;
                                for (byte nMask = 1; 255 > nMask; nMask++)
                                {
                                    aAlphaMap3[nIndx++] = (byte)(nFGColorAlpha * ((255 - nMask) / 255f) + 0.5);
                                }
                            }
                            cAlphaMap3 = cCUDA.CopyHostToDevice<byte>(aAlphaMap3);
                            cAlphaMap3_info2d = cCUDA.CopyHostToDevice<ushort>(aAlphaMap3_info2d);
                        }
                        #endregion CUDA Init
#if DEBUG
                        Dictionary<long, DateTime> ahDebug = new Dictionary<long, DateTime>();
                        Dictionary<long, Area> ahDebugAr = new Dictionary<long, Area>();
#endif
                        DateTime dtNextTime = DateTime.MinValue, dtNow;
                        bool bSet;
                        List<IntPtr> aDPs;
                        List<PixelsMap> aPMs;

                        while (true)
                        {
                            if (1 > aqQueue.CountGet() && (dtNow = DateTime.Now) > dtNextTime)
                            {
                                dtNextTime = dtNow.AddMinutes(20);
#if DEBUG
                                dtNow = dtNow.Subtract(TimeSpan.FromHours(2));
                                string sMessage = "";
                                foreach (long nID in ahDebug.OrderBy(o=>o.Value).Select(o=>o.Key))
                                    if (dtNow > ahDebug[nID])
                                        sMessage += "<br>[" + nID + " - " + ahDebug[nID].ToString("HH:mm:ss") + "]" + ahDebugAr[nID].ToString();
#endif
                                (new Logger(sS)).WriteDebug("CUDA free memory:" + cCUDA.FreeMemory
#if DEBUG
                                                               + "; possibly timeworn allocations:" + (1 > sMessage.Length ? "no" : sMessage)
#endif
                                                             );
                            }
                            cCmd = aqQueue.Dequeue();  //если нечего отдать - заснёт
                            switch (cCmd.eID)
                            {
                                case Command.ID.Allocate:
                                    #region
                                    try
                                    {
                                        cCmd.cPM._cException = null;
                                        if (1 > cCmd.cPM._nID)
                                        {
                                            if (0 < cCmd.cPM._nBytesQty)
                                            {
                                                nMemoryFree = cCUDA.FreeMemory;
                                                if (nMemoryReservedForMerge < nMemoryFree - cCmd.cPM._nBytesQty)
                                                {
                                                    bMemoryStarvation = (nMemoryFree < nMemoryStarvationThreshold);
                                                    (new Logger(sS)).WriteDebug3("pixelmap allocateCUDA [current_id=" + _nCurrentID + "]");
                                                    cCmd.cPM._nID = System.Threading.Interlocked.Increment(ref _nCurrentID);
                                                    ahPMIDs_DevicePointers.Add(cCmd.cPM._nID, cCUDA.Allocate(cCmd.cPM._nBytesQty));
#if DEBUG
                                                    ahDebug.Add(cCmd.cPM._nID, DateTime.Now);
                                                    ahDebugAr.Add(cCmd.cPM._nID, cCmd.cPM.stArea);
#endif
                                                }
                                                else
                                                {
                                                    bMemoryStarvation = true;
                                                    throw new Exception("out of memory in CUDA device during Allocate. Only 2 MBytes reserved for the Merge");
                                                }
                                            }
                                            else
                                                throw new Exception("bytes quantity in PixelsMap have to be greater than zero for Allocate [_bDisposed = " + cCmd.cPM._bDisposed + "][_bProcessing = " + cCmd.cPM._bProcessing + "][_stPosition.X = " + cCmd.cPM._stPosition.X + "][_stPosition.Y = " + cCmd.cPM._stPosition.Y + "][_bTemp = " + cCmd.cPM._bTemp + "][_dt = " + cCmd.cPM._dtCreate + "][_nBytesQty = " + cCmd.cPM._nBytesQty + "][_nID = " + cCmd.cPM._nID + "][_nShiftTotalX = " + cCmd.cPM._nShiftTotalX + "][_stArea.nHeight = " + cCmd.cPM._stArea.nHeight + "][_stArea.nWidth = " + cCmd.cPM._stArea.nWidth + "][bKeepAlive = " + cCmd.cPM.bKeepAlive + "][eAlpha = " + cCmd.cPM.eAlpha + "][bCUDA = " + cCmd.cPM.stMergingMethod + "][nAlphaConstant = " + cCmd.cPM.nAlphaConstant + "][nID = " + cCmd.cPM.nID + "][nLength = " + cCmd.cPM.nLength + "][stArea.nHeight = " + cCmd.cPM.stArea.nHeight + "][stArea.nWidth = " + cCmd.cPM.stArea.nWidth + "]");
                                        }
                                        else
                                            throw new Exception("PixelsMap ID have to be zero for Allocate");
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is CUDAException)
                                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                                        (new Logger(sS)).WriteError(ex);
                                        (new Logger(sS)).WriteDebug("bytes qty:" + cCmd.cPM._nBytesQty);
                                        cCmd.cPM._cException = ex;
                                    }
                                    cCmd.cMRE.Set();
                                    break;
                                #endregion
                                case Command.ID.CopyIn:
                                    #region
                                    try
                                    {
                                        cCmd.cPM._cException = null;
                                        if (1 > cCmd.cPM._nID)
                                        {
                                            if (cCUDA.FreeMemory - cCmd.cPM._nBytesQty > nMemoryReservedForMerge)
                                            {
                                                (new Logger(sS)).WriteDebug3("pixelmap copyinCUDA not allocated [pm_id=" + _nCurrentID + "]");
                                                cCmd.cPM._nID = System.Threading.Interlocked.Increment(ref _nCurrentID);
                                                if (cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
                                                    ahPMIDs_DevicePointers.Add(cCmd.cPM._nID, cCUDA.CopyHostToDevice((IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty));
                                                else if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
                                                    ahPMIDs_DevicePointers.Add(cCmd.cPM._nID, cCUDA.CopyHostToDevice((byte[])cCmd.ahParameters[typeof(byte[])]));
                                                else
                                                    throw new Exception("unknown parameter type");
#if DEBUG
                                                ahDebug.Add(cCmd.cPM._nID, DateTime.Now);
                                                ahDebugAr.Add(cCmd.cPM._nID, cCmd.cPM.stArea);
#endif
                                            }
                                            else
                                                throw new Exception("out of memory in CUDA device during CopyIn. Only 2 MBytes reserved for the Merge.");
                                        }
                                        else
                                        {
                                            (new Logger(sS)).WriteDebug4("pixelmap copyinCUDA allocated [pm_id=" + _nCurrentID + "]");
                                            if (cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
                                                cCUDA.CopyHostToDevice(ahPMIDs_DevicePointers[cCmd.cPM._nID], (IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty);
                                            else if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
                                                cCUDA.CopyHostToDevice(ahPMIDs_DevicePointers[cCmd.cPM._nID], (byte[])cCmd.ahParameters[typeof(byte[])]);
                                            else
                                                throw new Exception("unknown parameter type");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is CUDAException)
                                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                                        (new Logger(sS)).WriteError(ex);
                                        cCmd.cPM._cException = ex;
                                    }
                                    cCmd.cMRE.Set();
                                    #endregion
                                    break;
                                case Command.ID.CopyOut:
                                    #region
                                    try
                                    {
                                        if (0 < cCmd.cPM._nID)
                                        {
                                            if (!cCmd.ahParameters.ContainsKey(typeof(IntPtr)))
                                            {
                                                if (cCmd.ahParameters.ContainsKey(typeof(byte[])))
                                                {
                                                    byte[] aB = (byte[])cCmd.ahParameters[typeof(byte[])];
                                                    cCmd.cPM._aBytes = null;
                                                    if (cCmd.cPM._nBytesQty != aB.Length)
                                                        (new Logger(sS)).WriteWarning("wrong array size for copyout [got:" + aB.Length + "][expected:" + cCmd.cPM._nBytesQty + "]");
                                                    cCUDA.CopyDeviceToHost<byte>(ahPMIDs_DevicePointers[cCmd.cPM._nID], aB);
                                                }
                                                else  // не юзается (см. copyout())
                                                {
                                                    cCmd.cPM._aBytes = _cBinM.BytesGet((int)cCmd.cPM._nBytesQty, 3);
                                                    cCUDA.CopyDeviceToHost<byte>(ahPMIDs_DevicePointers[cCmd.cPM._nID], cCmd.cPM._aBytes.aBytes);
                                                }
                                            }
                                            else
                                                cCUDA.CopyDeviceToHost(ahPMIDs_DevicePointers[cCmd.cPM._nID], (IntPtr)cCmd.ahParameters[typeof(IntPtr)], cCmd.cPM._nBytesQty);
                                            (new Logger(sS)).WriteDebug5("copy out [id:" + cCmd.cPM._nID + "][ptr:" + ahPMIDs_DevicePointers[cCmd.cPM._nID].Pointer + "]");
                                        }
                                        else
                                            throw new Exception("PixelsMap have to be allocated for CopyOut");
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is CUDAException)
                                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                                        (new Logger(sS)).WriteError(ex);
                                        cCmd.cPM._cException = ex;
                                    }
                                    cCmd.cMRE.Set();
                                    #endregion
                                    break;
                                case Command.ID.Merge:
                                    #region
                                    bSet = false;
                                    try
                                    {
                                        aPMs = (List<PixelsMap>)cCmd.ahParameters[typeof(List<PixelsMap>)];
                                        DisCom.MergeInfo cMergeInfo = (DisCom.MergeInfo)cCmd.ahParameters[typeof(DisCom.MergeInfo)];
                                        aDPs = new List<IntPtr>();

                                        if (1 > cCmd.cPM._nID)
                                            throw new Exception("background PixelsMap have to be allocated for Merge");

                                        aDPs.Add((IntPtr)ahPMIDs_DevicePointers[cCmd.cPM._nID].Pointer);
                                        for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                                        {
                                            if (!ahPMIDs_DevicePointers.ContainsKey(aPMs[nIndx]._nID))
                                                throw new Exception("there is a corrupted ID in layers for merge [id:" + aPMs[nIndx]._nID + "]");
                                            if (1 > ahPMIDs_DevicePointers[aPMs[nIndx]._nID].Pointer)
                                                throw new Exception("there is an empty pointer in layers for merge [id:" + aPMs[nIndx]._nID + "]");
                                            aDPs.Add((IntPtr)ahPMIDs_DevicePointers[aPMs[nIndx]._nID].Pointer);
                                        }

                                        cPMs = cCUDA.CopyHostToDevice<IntPtr>(aDPs.ToArray());
                                        cInfos = cCUDA.CopyHostToDevice(cMergeInfo, cMergeInfo.SizeGet());  // operator intptr in DisCom.MergeInfo

                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, 0, (IntPtr)cPMs.Pointer);
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size, (IntPtr)cInfos.Pointer);
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 2, (IntPtr)cAlphaMap.Pointer);   //
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 3, (IntPtr)cAlphaMap_info3d.Pointer); //
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 4, (IntPtr)cAlphaMap_info2d.Pointer); //
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 5, (IntPtr)cAlphaMap2.Pointer);
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 6, (IntPtr)cAlphaMap2_info2d.Pointer); //
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 7, (IntPtr)cAlphaMap3.Pointer);
                                        cCUDA.SetParameter<IntPtr>(cCUDAFuncMerge, IntPtr.Size * 8, (IntPtr)cAlphaMap3_info2d.Pointer); //
                                        cCUDA.SetParameterSize(cCUDAFuncMerge, (uint)(IntPtr.Size * 9));
                                        int nIterationsX = (0 == cMergeInfo.nBackgroundWidth % nThreadsPerBlock ? cMergeInfo.nBackgroundWidth / nThreadsPerBlock : cMergeInfo.nBackgroundWidth / nThreadsPerBlock + 1);
                                        int nIterationsY = (0 == cMergeInfo.nBackgroundHight % nThreadsPerBlock ? cMergeInfo.nBackgroundHight / nThreadsPerBlock : cMergeInfo.nBackgroundHight / nThreadsPerBlock + 1);
                                        //int nIterationsX = (0 == cMergeInfo.nBackgroundHight % nThreadsPerBlock ? cMergeInfo.nBackgroundHight / nThreadsPerBlock : cMergeInfo.nBackgroundHight / nThreadsPerBlock + 1);

                                        cCUDA.Launch(cCUDAFuncMerge, nIterationsX, nIterationsY);




                                        cCUDA.Free(cPMs);
                                        cCUDA.Free(cInfos);

                                        cCmd.cMRE.Set();
                                        bSet = true;

                                        cMergeInfo.Dispose();
                                        for (int nIndx = 0; nIndx < aPMs.Count; nIndx++)
                                        {
                                            lock (aPMs[nIndx]._cSyncRoot)
                                                aPMs[nIndx]._bProcessing = false;
                                            aPMs[nIndx].Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                        {
                                        cCmd.cPM._cException = ex;
                                        if (!bSet)
                                            cCmd.cMRE.Set();
                                        if (ex is CUDAException)
                                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                                        (new Logger(sS)).WriteError(ex);
                                    }
                                    #endregion
                                    break;
                                case Command.ID.Dispose:
                                    #region
                                    (new Logger(sS)).Write(Logger.Level.debug4, "dispose: in");
                                    try
                                    {
                                        if (ahPMIDs_DevicePointers.ContainsKey(cCmd.cPM._nID))
                                        {
                                            if (0 < cCmd.cPM._nID && 0 < ahPMIDs_DevicePointers[cCmd.cPM._nID].Pointer)
                                            {
                                                cCUDA.Free(ahPMIDs_DevicePointers[cCmd.cPM._nID]);
                                                //cCUDA.SynchronizeContext();
                                                bMemoryStarvation = (cCUDA.FreeMemory < nMemoryStarvationThreshold);
                                                (new Logger(sS)).WriteDebug3("dispose [id:" + cCmd.cPM._nID + "][ptr:" + ahPMIDs_DevicePointers[cCmd.cPM._nID].Pointer + "]");
                                            }
                                            ahPMIDs_DevicePointers.Remove(cCmd.cPM._nID);
#if DEBUG
                                            ahDebug.Remove(cCmd.cPM._nID);
                                            ahDebugAr.Remove(cCmd.cPM._nID);
#endif
                                            cCmd.cPM._nID = 0;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is CUDAException)
                                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                                        (new Logger(sS)).WriteError(ex);
                                        cCmd.cPM._cException = ex;
                                    }
                                    (new Logger(sS)).Write(Logger.Level.debug4, "dispose: out");
                                    #endregion
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is CUDAException)
                            ex = new Exception("CUDA Error:" + ((CUDAException)ex).CUDAError.ToString(), ex);
                        (new Logger(sS)).WriteError("CUDA STOPPED!!!! [id = " + _nIndex + "]", ex);
                    }
#endif
                }
            }
            static private Dictionary<int, CUDAWorkers> _ahMergingDeviceNumbers_Workers = new Dictionary<int, CUDAWorkers>();
            private CUDAWorker[] _aCUDAWorkers;
            private int _nN;
            private const ThreadsCount _CUDA_MULTITHREAD = ThreadsCount.Triple;   

            public CUDAWorkers(int nMergingDeviceNumber)
            {
                if (_ahMergingDeviceNumbers_Workers.ContainsKey(nMergingDeviceNumber))
                    throw new Exception("workers with this number [" + nMergingDeviceNumber + "] already exists");

                _nN = nMergingDeviceNumber;
                _ahMergingDeviceNumbers_Workers.Add(_nN, this);
                _aCUDAWorkers = new CUDAWorker[(int)_CUDA_MULTITHREAD];
                if (_CUDA_MULTITHREAD != ThreadsCount.Triple)
                    (new Logger("CUDAWorkers-" + _nN)).WriteError("CUDA workers count is not 3 = " + _CUDA_MULTITHREAD);
                else
                    (new Logger("CUDAWorkers-" + _nN)).WriteNotice("CUDA workers count = 3");
                for (byte nI = 0; nI < _aCUDAWorkers.Length; nI++)
                {
                    _aCUDAWorkers[nI] = new CUDAWorker(_nN, nI);
                }

                Thread cCopyInWorker = new Thread(MainWorker);
                cCopyInWorker.Priority = ThreadPriority.Normal;
                cCopyInWorker.Start();
            }
            private void MainWorker()
            {
                try
                {
                    Command cCmd = null;
                    while (true)
                    {
                        try
                        {
                            cCmd = _ahMergingHash_CommandsQueue[_nN].Dequeue();  //если нечего отдать - заснёт
                            if (_CUDA_MULTITHREAD == ThreadsCount.Triple)
                                _aCUDAWorkers[cCmd.cPM._nIndexTriple].aqQueue.Enqueue(cCmd);
                            else if (_CUDA_MULTITHREAD == ThreadsCount.Single)
                                _aCUDAWorkers[0].aqQueue.Enqueue(cCmd);
                            else
                                throw new Exception("wrong CUDA workers count");
                        }
                        catch (Exception ex)
                        {
                            (new Logger("MainWorker-" + _nN)).WriteError("cmd [" + cCmd.eID + "][tripid=" + cCmd.cPM._nIndexTriple + "]", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    (new Logger("MainWorker-" + _nN)).WriteError(ex);
                }
                finally
                {
                    (new Logger("MainWorker-" + _nN)).WriteNotice("PIPE client STOPPED!");
                }
            }
        }
    }
}
