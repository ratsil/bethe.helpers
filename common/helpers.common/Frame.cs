using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helpers
{
    public abstract class Frame
    {
        public abstract class Audio
        {
            public Audio()
            {
                nID = System.Threading.Interlocked.Increment(ref nMaxID);
                _oDisposeLock = new object();
                bDisposed = false;
            }
            static private long nMaxID = 0;
            public long nID;
            protected bool bDisposed;
            protected object _oDisposeLock;
            public Bytes aFrameBytes;
            ~Audio()
            {
                try
                {
                    Dispose();
                }
                catch (Exception ex)
                {
                    (new Logger()).WriteError(ex);
                }
            }
            public abstract void Dispose();
        }
        public abstract class Video
        {
            public Video()
            {
                nID = System.Threading.Interlocked.Increment(ref nMaxID);
            }
            static private long nMaxID = 0;
            public long nID;
            public object oFrameBytes;
            public IntPtr pFrameBytes
            {
                get
                {
                    if (null == oFrameBytes)
                        return IntPtr.Zero;
                    if (oFrameBytes is IntPtr)
                        return (IntPtr)oFrameBytes;
                    throw new Exception("unexpected video frame buffer type");
                }
            }
            public Bytes aFrameBytes
            {
                get
                {
                    if (null == oFrameBytes)
                        return null;
                    if (oFrameBytes is Bytes)
                        return (Bytes)oFrameBytes;
                    throw new Exception("unexpected video frame buffer type");
                }
            }
            ~Video()
            {
            }
        }
        public Audio cAudio;
        public Video cVideo;
        public long nID;
        static private long nMaxID = 0;
        public Frame()
        {
            nID = System.Threading.Interlocked.Increment(ref nMaxID);
        }
    }
}
