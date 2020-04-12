using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;
using helpers;

namespace helpers
{
    //class Logger : helpers.Logger
    //{
    //    public Logger()
    //        : base("wrapper", "device[" + System.Diagnostics.Process.GetCurrentProcess().Id + "]")
    //    { }
    //}
    public class Ebur128CppInterop
    {
        internal static class Win32Interops
        {
            /// <summary>
            /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
            /// </summary>
            /// <param name="hModule">A handle to the DLL module that contains the function or variable.</param>
            /// <param name="lpProcName">he function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
            /// <returns>If the function succeeds, the return value is the address of the exported function or variable. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            /// <summary>
            /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count. When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
            /// </summary>
            /// <param name="hModule">A handle to the loaded library module.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call the GetLastError function.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern bool FreeLibrary(IntPtr hModule);

            /// <summary>
            /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
            /// </summary>
            /// <param name="lpFileName">The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file. If the string specifies a full path, the function searches only that path for the module. If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module; for more information, see the Remarks. If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\), not forward slashes (/). For more information about paths, see Naming a File or Directory. If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string.</param>
            /// <returns>If the function succeeds, the return value is a handle to the module. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            /// <summary>
            /// Closes an open object handle.
            /// </summary>
            /// <param name="handle">A valid handle to an open object.</param>
            /// <returns>If the function succeeds, the return value is nonzero.</returns>
            [DllImport("kernel32", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();

            /// <summary>
            /// Changes the parent window of the specified child window.
            /// </summary>
            /// <param name="hWndChild">A handle to the child window.</param>
            /// <param name="hWndNewParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is HWND_MESSAGE, the child window becomes a message-only window.</param>
            /// <returns>If the function succeeds, the return value is a handle to the previous parent window. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        }
        public enum EbuR128Mode : int
        {
            /** can call ebur128_loudness_momentary */
            EBUR128_MODE_M = (1 << 0),
            /** can call ebur128_loudness_shortterm */
            EBUR128_MODE_S = (1 << 1) | EBUR128_MODE_M,
            /** can call ebur128_loudness_global_* and ebur128_relative_threshold */
            EBUR128_MODE_I = (1 << 2) | EBUR128_MODE_M,
            /** can call ebur128_loudness_range */
            EBUR128_MODE_LRA = (1 << 3) | EBUR128_MODE_S,
            /** can call ebur128_sample_peak */
            EBUR128_MODE_SAMPLE_PEAK = (1 << 4) | EBUR128_MODE_M,
            /** can call ebur128_true_peak */
            EBUR128_MODE_TRUE_PEAK = (1 << 5) | EBUR128_MODE_M | EBUR128_MODE_SAMPLE_PEAK,
            /** uses histogram algorithm to calculate loudness */
            EBUR128_MODE_HISTOGRAM = (1 << 6)
        };
        public enum EbuR128Error : int
        {
            EBUR128_SUCCESS = 0,
            EBUR128_ERROR_NOMEM,
            EBUR128_ERROR_INVALID_MODE,
            EBUR128_ERROR_INVALID_CHANNEL_INDEX,
            EBUR128_ERROR_NO_CHANGE,
            UNKNOWN = 1000,
        };

        // зависимости включил в dll. Зависимости такие:
        // c:\windows\system32\MSVCP140D.DLL
        // c:\windows\system32\VCRUNTIME140D.DLL
        // c:\windows\system32\UCRTBASED.DLL
        // статейка тут: https://stackoverflow.com/questions/20890458/compile-c-in-vs-without-requiring-msvcp120d-dll-at-runtime

        static private Dictionary<string, Delegate> _ahCppFunctionsUsed;
        static private IntPtr _pWrapperDllHandler;
        static Ebur128CppInterop()
        {
            string sAssemblyPath = typeof(Ebur128CppInterop).Assembly.Location;
            string sAssemblyDir = System.IO.Path.GetDirectoryName(sAssemblyPath);
            (new Logger()).WriteNotice("Ebur128CppInterop's assembly path = " + sAssemblyPath); //.GetExecutingAssembly()
            string sDll = System.IO.Path.Combine(sAssemblyDir, "EBUR128_x64_Debug.dll");
            (new Logger()).WriteNotice(sDll + " found [" + System.IO.File.Exists(sDll) + "]");
            _pWrapperDllHandler = Win32Interops.LoadLibrary(sDll); //Win32Project1 // can be a full path
            if (_pWrapperDllHandler == IntPtr.Zero)
            {
                (new Logger()).WriteNotice("kernel32 GetLastError = [" + Win32Interops.GetLastError() + "] [dll = "+ sDll + "]");
                sDll = System.IO.Path.Combine(sAssemblyDir, "EBUR128_x64_Release.dll");
                (new Logger()).WriteNotice(sDll + " found [" + System.IO.File.Exists(sDll) + "]");
                _pWrapperDllHandler = Win32Interops.LoadLibrary(sDll); //Win32Project1 // can be a full path
                if (_pWrapperDllHandler == IntPtr.Zero)
                {
                    (new Logger()).WriteNotice("kernel32 GetLastError = [" + Win32Interops.GetLastError() + "] [dll = " + sDll + "]");
                    throw new System.ComponentModel.Win32Exception($"dll library was not found or cannot be loaded [{sDll}]");
                }
            }
            _ahCppFunctionsUsed = new Dictionary<string, Delegate>();
        }
        static internal T CppFunction<T>()
        {
            string sCppFunctionName = null;
            try
            {
                object[] aAttrs = typeof(T).GetCustomAttributes(typeof(CppFunctionAttribute), false);
                if (aAttrs.Length == 0)
                    throw new Exception("Could not find the AjaFunctionAttribute.");
                CppFunctionAttribute cAjaAttr = (CppFunctionAttribute)aAttrs[0];
                sCppFunctionName = cAjaAttr.sFunctionName;
                if (!_ahCppFunctionsUsed.ContainsKey(sCppFunctionName))
                {
                    IntPtr pFuncAddress = Win32Interops.GetProcAddress(_pWrapperDllHandler, cAjaAttr.sFunctionName);
                    if (pFuncAddress == IntPtr.Zero)
                        throw new System.ComponentModel.Win32Exception();
                    Delegate dFunctionPointer = Marshal.GetDelegateForFunctionPointer(pFuncAddress, typeof(T));
                    _ahCppFunctionsUsed.Add(cAjaAttr.sFunctionName, dFunctionPointer);
                }
                return (T)Convert.ChangeType(_ahCppFunctionsUsed[cAjaAttr.sFunctionName], typeof(T), null);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                throw new MissingMethodException(String.Format("Function " + sCppFunctionName + " was not found in aja dll."), e);
            }
        }

        [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
        internal sealed class CppFunctionAttribute : Attribute
        {
            public string sFunctionName { get; private set; }
            public CppFunctionAttribute(string sFunctionName)
            {
                this.sFunctionName = sFunctionName;
            }
        }

        internal class Functions
        {
            #region EBU_R128
            [CppFunctionAttribute("wrap_ebur128_init")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate EbuR128Error wrap_ebur128_init(uint channels, uint samplerate, EbuR128Mode mode, out IntPtr pState);
            [CppFunctionAttribute("wrap_ebur128_dispose")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate EbuR128Error wrap_ebur128_dispose(IntPtr pState);
            [CppFunctionAttribute("wrap_ebur128_add_frames_short")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate EbuR128Error wrap_ebur128_add_frames_short(IntPtr pState, short[] buffer, UInt64 frames);
            [CppFunctionAttribute("wrap_ebur128_add_frames_int")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate EbuR128Error wrap_ebur128_add_frames_int(IntPtr pState, int[] buffer, UInt64 frames);
            [CppFunctionAttribute("wrap_ebur128_loudness_global")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate EbuR128Error wrap_ebur128_loudness_global(IntPtr pState, out double loudness);
            #endregion
        }
    }
    public abstract class Ebur128CppClass : Ebur128CppInterop, IDisposable
    {
        private object oLock;
        private bool bDisposed;
        internal Ebur128CppClass()
        {
            bDisposed = false;
            oLock = new object();
        }
        ~Ebur128CppClass()
        {
            Dispose();
        }
        internal delegate void DoDispose();
        abstract internal DoDispose dDispose { get; }
        public void Dispose()
        {
            lock (oLock)
            {
                if (bDisposed)
                    return;
                bDisposed = true;
            }
            dDispose();
        }
        void IDisposable.Dispose()
        {
            this.Dispose();
        }
    }
    public class EbuR128 : Ebur128CppClass
    {
        private IntPtr _pState;
        public uint nChannels;
        public uint nSamplerate;
        public EbuR128Mode eAnalizeMode;
        internal override DoDispose dDispose
        {
            get
            {
                return delegate ()   //()=> { };
                {
                    CppFunction<Functions.wrap_ebur128_dispose>().Invoke(_pState);
                };
            }
        }
        public EbuR128(uint nChannels, uint nSamplerate, EbuR128Mode eAnalizeMode)
        {
            EbuR128Error nRes = CppFunction<Functions.wrap_ebur128_init>().Invoke(nChannels, nSamplerate, eAnalizeMode, out _pState);
            if (nRes > 0)
                throw new Exception($"wrap_ebur128_init returns {nRes}");
        }
        public void AddFrames(short[] aBuffer, uint nSize) // IntPtr pBuffer, int nSiz  short[] aBuffer
        {
            EbuR128Error nRes = CppFunction<Functions.wrap_ebur128_add_frames_short>().Invoke(_pState, aBuffer, nSize);
            if (nRes > 0)
                throw new Exception("wrap_ebur128_add_frames_short returns " + nRes + $" [p={_pState}][buf={(aBuffer == null ? "NULL" : "" + aBuffer.Length)}][siz={nSize}]");
        }
        public void AddFrames(int[] aBuffer)
        {
            //GCHandle cHandle = GCHandle.Alloc(aBuffer, GCHandleType.Pinned);   //cHandle.AddrOfPinnedObject()

            EbuR128Error nRes = CppFunction<Functions.wrap_ebur128_add_frames_int>().Invoke(_pState, aBuffer, (uint)aBuffer.Length);
            if (nRes > 0)
                throw new Exception("wrap_ebur128_add_frames_int returns " + nRes);
        }
        public double GetLufs()
        {
            double nRetVal;
            EbuR128Error nRes = CppFunction<Functions.wrap_ebur128_loudness_global>().Invoke(_pState, out nRetVal);
            if (nRes > 0)
                throw new Exception("wrap_ebur128_loudness_global returns " + nRes);
            return nRetVal;
        }
    }
}
