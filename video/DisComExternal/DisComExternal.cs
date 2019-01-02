using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using helpers;
using helpers.extensions;

namespace BTL.Merging
{
    partial class DisComExternal
    {
        static BytesInMemory _cBinM;
        static private int _nCopyInChunkSize;
        static private int _nN;
        static private Dictionary<long, Bytes> _ahBytesID_Bytes = new Dictionary<long, Bytes>();

        static void Main(string[] args)
        {
            int nPID = System.Diagnostics.Process.GetCurrentProcess().Id;
            if (0 < args.Length)
                Preferences.sFile = AppDomain.CurrentDomain.BaseDirectory + args[0];
            if (!System.IO.File.Exists(Preferences.sFile))
                throw new System.IO.FileNotFoundException("файл конфигурации не найден [pid:" + nPID + "][" + Preferences.sFile + "]");

            Preferences.Reload();

            _nN = Preferences.stMergingMethod.nHash;
            _cBinM = new BytesInMemory("discom_ext bytes");

            Thread cCopyInWorker = new Thread(CopyInWorker);
            cCopyInWorker.Priority = ThreadPriority.Normal;
            cCopyInWorker.Start();
            Thread cMergingWorker = new Thread(MergingWorker);
            cMergingWorker.Priority = ThreadPriority.Highest;
            cMergingWorker.Start();
            Thread cCopyOutWorker = new Thread(CopyOutWorker);
            cCopyOutWorker.Priority = ThreadPriority.Normal;
            cCopyOutWorker.Start();
        }

        static private void CopyInWorker()
        {
            try
            {
                NamedPipeServerStream cServerPipeStream = new NamedPipeServerStream("DisComPipe-CopyIn-" + _nN);
                (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteNotice("PIPE Server ["+ "DisComPipe-CopyIn-" + _nN + "] started and waiting for the discom client...");
                cServerPipeStream.WaitForConnection();
                (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteNotice("PIPE connection accepted");
                BinaryFormatter cBinFormatter = new BinaryFormatter();
                _nCopyInChunkSize = (int)cBinFormatter.Deserialize(cServerPipeStream);
                (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteNotice("chunk size got [" + _nCopyInChunkSize + "]");

                PixelsMap.Command.ID eComand = PixelsMap.Command.ID.Unknown;
                Bytes cBytes;
                long nID;
                byte nB;
                int nLastChunkSize, nChunksQty, nAllChunksSize;

                while (true)
                {
                    try
                    {
                        eComand = (PixelsMap.Command.ID)cServerPipeStream.ReadByte();
                        switch (eComand)
                        {
                            case PixelsMap.Command.ID.Allocate:
                                int nSize = (int)cBinFormatter.Deserialize(cServerPipeStream);
                                cBytes = _cBinM.BytesGet(nSize, 1);
                                lock (_ahBytesID_Bytes)
                                    _ahBytesID_Bytes.Add(cBytes.nID, cBytes);
                                cBinFormatter.Serialize(cServerPipeStream, cBytes.nID);
                                break;
                            case PixelsMap.Command.ID.CopyIn:
                                nID = (long)cBinFormatter.Deserialize(cServerPipeStream);
                                lock (_ahBytesID_Bytes)
                                    cBytes = _ahBytesID_Bytes[nID];
                                nB = (byte)cServerPipeStream.ReadByte();
                                if (nB == 0) // from byte[]
                                    cServerPipeStream.Read(cBytes.aBytes, 0, cBytes.aBytes.Length);
                                else // from IntPtr
                                {
                                    nChunksQty = cBytes.aBytes.Length / _nCopyInChunkSize;
                                    nLastChunkSize = cBytes.aBytes.Length % _nCopyInChunkSize;
                                    nAllChunksSize = _nCopyInChunkSize * nChunksQty;
                                    for (int nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyInChunkSize)
                                    {
                                        if (nOffset == nAllChunksSize)
                                        {
                                            if (nLastChunkSize > 0)
                                                cServerPipeStream.Read(cBytes.aBytes, nOffset, nLastChunkSize);
                                        }
                                        else
                                            cServerPipeStream.Read(cBytes.aBytes, nOffset, _nCopyInChunkSize);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteError("in CopyInWorker", ex);
            }
            finally
            {
                (new Logger("DisComExt-CopyInWorker-" + _nN)).WriteNotice("copy in worker STOPPED!");
            }
        }
        static private void MergingWorker()
        {
            try
            {
                NamedPipeServerStream cServerPipeStream = new NamedPipeServerStream("DisComPipe-Merging-" + _nN);
                (new Logger("DisComExt-MergingWorker-" + _nN)).WriteNotice("PIPE Server [" + "DisComPipe-Merging - " + _nN + "] started and waiting for the discom client...");
                cServerPipeStream.WaitForConnection();
                (new Logger("DisComExt-MergingWorker-" + _nN)).WriteNotice("PIPE connection accepted");
                BinaryFormatter cBinFormatter = new BinaryFormatter();
                _nCopyInChunkSize = (int)cBinFormatter.Deserialize(cServerPipeStream);
                (new Logger("DisComExt-MergingWorker-" + _nN)).WriteNotice("chunk size got [" + _nCopyInChunkSize + "]");

                PixelsMap.Command.ID eComand = PixelsMap.Command.ID.Unknown;
                DisCom cDisCom = new DisCom();
                List<long> aDPs;
                List<byte[]> aByteArs = new List<byte[]>();
                DisCom.MergeInfo cMergeInfo;

                while (true)
                {
                    try
                    {
                        eComand = (PixelsMap.Command.ID)cServerPipeStream.ReadByte();
                        switch (eComand)
                        {
                            case PixelsMap.Command.ID.Merge:
                                aDPs = (List<long>)cBinFormatter.Deserialize(cServerPipeStream);
                                aByteArs.Clear();
                                foreach (long nID2 in aDPs)
                                    lock (_ahBytesID_Bytes)
                                        aByteArs.Add(_ahBytesID_Bytes[nID2].aBytes);
                                cMergeInfo = (DisCom.MergeInfo)cBinFormatter.Deserialize(cServerPipeStream);
                                cDisCom.FrameMerge(cMergeInfo, aByteArs, false);
                                cMergeInfo.Dispose();
                                cServerPipeStream.WriteByte(1);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DisComExt-MergingWorker-" + _nN)).WriteError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                (new Logger("DisComExt-MergingWorker-" + _nN)).WriteError("in MergingWorker", ex);
            }
            finally
            {
                (new Logger("DisComExt-MergingWorker-" + _nN)).WriteNotice("merging worker STOPPED!");
            }
        }
        static private void CopyOutWorker()
        {
            try
            {
                NamedPipeServerStream cServerPipeStream = new NamedPipeServerStream("DisComPipe-CopyOut-" + _nN);
                (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteNotice("PIPE Server [" + "DisComPipe-CopyOut-" + _nN + "] started and waiting for the discom client...");
                cServerPipeStream.WaitForConnection();
                (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteNotice("PIPE connection accepted");
                BinaryFormatter cBinFormatter = new BinaryFormatter();
                _nCopyInChunkSize = (int)cBinFormatter.Deserialize(cServerPipeStream);
                (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteNotice("chunk size got [" + _nCopyInChunkSize + "]");

                PixelsMap.Command.ID eComand = PixelsMap.Command.ID.Unknown;
                Bytes cBytes;
                long nID;
                byte nB;
                int nLastChunkSize, nChunksQty, nAllChunksSize;

                while (true)
                {
                    try
                    {
                        eComand = (PixelsMap.Command.ID)cServerPipeStream.ReadByte();
                        switch (eComand)
                        {
                            case PixelsMap.Command.ID.CopyOut:
                                nID = (long)cBinFormatter.Deserialize(cServerPipeStream);
                                lock (_ahBytesID_Bytes)
                                    cBytes = _ahBytesID_Bytes[nID];
                                nB = (byte)cServerPipeStream.ReadByte();
                                if (nB == 0) // to byte[]
                                    cServerPipeStream.Write(cBytes.aBytes, 0, cBytes.aBytes.Length);
                                else // to IntPtr
                                {
                                    nChunksQty = cBytes.aBytes.Length / _nCopyInChunkSize;
                                    nLastChunkSize = cBytes.aBytes.Length % _nCopyInChunkSize;
                                    nAllChunksSize = _nCopyInChunkSize * nChunksQty;
                                    for (int nOffset = 0; nOffset <= nAllChunksSize; nOffset += _nCopyInChunkSize)
                                    {
                                        if (nOffset == nAllChunksSize)
                                        {
                                            if (nLastChunkSize > 0)
                                                cServerPipeStream.Write(cBytes.aBytes, nOffset, nLastChunkSize);
                                        }
                                        else
                                            cServerPipeStream.Write(cBytes.aBytes, nOffset, _nCopyInChunkSize);
                                    }
                                }
                                break;
                            case PixelsMap.Command.ID.Dispose:
                                nID = (long)cBinFormatter.Deserialize(cServerPipeStream);
                                lock (_ahBytesID_Bytes)
                                    cBytes = _ahBytesID_Bytes[nID];
                                _cBinM.BytesBack(cBytes, 2);
                                lock (_ahBytesID_Bytes)
                                    _ahBytesID_Bytes.Remove(nID);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteError("in CopyOutWorker", ex);
            }
            finally
            {
                (new Logger("DisComExt-CopyOutWorker-" + _nN)).WriteNotice("copy out worker STOPPED!");
            }
        }

    }
}
