/*
    Derived from zlib header files (zlib license)
    Copyright (C) 1995-2017 Jean-loup Gailly and Mark Adler

    C# Wrapper based on zlibnet v1.3.3 (https://zlibnet.codeplex.com/)
    Copyright (C) @hardon (https://www.codeplex.com/site/users/view/hardon)
    Copyright (C) 2017-2018 Hajin Jang

    zlib license

    This software is provided 'as-is', without any express or implied
    warranty.  In no event will the authors be held liable for any damages
    arising from the use of this software.

    Permission is granted to anyone to use this software for any purpose,
    including commercial applications, and to alter it and redistribute it
    freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
       claim that you wrote the original software. If you use this software
       in a product, an acknowledgment in the product documentation would be
       appreciated but is not required.
    2. Altered source versions must be plainly marked as such, and must not be
       misrepresented as being the original software.
    3. This notice may not be removed or altered from any source distribution.
*/

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// ReSharper disable InconsistentNaming

namespace Joveler.ZLib
{
    #region SafeLibraryHandle
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region Managed Methods
        public SafeLibraryHandle() : base(true) { }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(handle);
        }
        #endregion        
    }
    #endregion

    #region PinnedObject, PinnedArray
    internal class PinnedObject : IDisposable
    {
        private GCHandle _hObject;
        public IntPtr Ptr => _hObject.AddrOfPinnedObject();

        public PinnedObject(object _object)
        {
            _hObject = GCHandle.Alloc(_object, GCHandleType.Pinned);
        }

        ~PinnedObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hObject.IsAllocated)
                    _hObject.Free();
            }
        }
    }

    internal class PinnedArray : IDisposable
    {
        private GCHandle _hArray;
        public Array Array;
        public IntPtr Ptr => _hArray.AddrOfPinnedObject();

        public IntPtr this[int idx] => Marshal.UnsafeAddrOfPinnedArrayElement(Array, idx);
        public static implicit operator IntPtr(PinnedArray fixedArray) => fixedArray[0];

        public PinnedArray(Array array)
        {
            Array = array;
            _hArray = GCHandle.Alloc(array, GCHandleType.Pinned);
        }

        ~PinnedArray()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hArray.IsAllocated)
                    _hArray.Free();
            }
        }
    }
    #endregion

    #region ZLibInit
    public static class ZLibInit
    {
        #region Propeties
        // Is ZLibNative using .Net Framework's clrcompression.dll, or user provided zlibwapi.dll?
        public static bool ZLibProvided { get; internal set; }
        #endregion

        #region GlobalInit, GlobalCleanup
        public static void GlobalInit(string dllPath = null, int bufferSize = 64 * 1024)
        {
            if (NativeMethods.Loaded)
                throw new InvalidOperationException(NativeMethods.MsgAlreadyInited);

            if (dllPath == null)
            {
                // Use .Net Framework's clrcompression instead
                string fxDir = RuntimeEnvironment.GetRuntimeDirectory();
                dllPath = Path.Combine(fxDir, "clrcompression.dll");
                ZLibProvided = false;
            }
            else if (!File.Exists(dllPath))
            { // Check 
                throw new ArgumentException("Specified dll does not exist");
            }
            else
            {
                ZLibProvided = true;
            }

            NativeMethods.hModule = NativeMethods.LoadLibrary(dllPath);
            if (NativeMethods.hModule.IsInvalid)
                throw new ArgumentException($"Unable to load [{dllPath}]", new Win32Exception());

            // Check if dll is valid zlibwapi.dll/clrcompression.dll
            if (NativeMethods.GetProcAddress(NativeMethods.hModule, "zlibCompileFlags") == IntPtr.Zero)
            {
                GlobalCleanup();
                throw new ArgumentException($"[{dllPath}] is not valid zlibwapi.dll");
            }

            // Check if dll is valid provided zlibwapi.dll
            if (ZLibProvided)
            {
                if (NativeMethods.GetProcAddress(NativeMethods.hModule, "adler32") == IntPtr.Zero)
                {
                    GlobalCleanup();
                    throw new ArgumentException($"[{dllPath}] is not valid zlibwapi.dll");
                }
            }

            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (bufferSize < 4096)
                bufferSize = 4096;
            NativeMethods.BufferSize = bufferSize;

            try
            {
                NativeMethods.LoadFuntions();
            }
            catch (Exception)
            {
                GlobalCleanup();
                throw;
            }
        }

        public static void GlobalCleanup()
        {
            if (NativeMethods.hModule != null)
            {
                NativeMethods.ResetFuntions();

                NativeMethods.hModule.Close();
                NativeMethods.hModule = null;
            }
            else
            {
                throw new InvalidOperationException(NativeMethods.MsgInitFirstError);
            }
        }
        #endregion

        #region ZLibVersion
        /// <summary>
        /// The application can compare zlibVersion and ZLIB_VERSION for consistency.
        /// If the first character differs, the library code actually used is not
        /// compatible with the zlib.h header file used by the application.  This check
        /// is automatically made by deflateInit and inflateInit.
        /// </summary>
        public static string ZLibVersion()
        {
            NativeMethods.CheckZLibUserProvided();

            return NativeMethods.ZLibVersion();
        }
        #endregion
    }
    #endregion

    #region NativeMethods
    internal static class NativeMethods
    {
        #region Const
        public const string MsgInitFirstError = "Please call ZLib.GlobalInit() first!";
        public const string MsgAlreadyInited = "ZLibWrapper is already initialized.";
        public const string MsgRequireUserProvided = "Please init ZLibWrapper with user provided zlibwapi.dll.";

        public const int DEF_MEM_LEVEL = 8;
        public const string ZLIB_VERSION = "1.2.11"; // This code is based on zlib 1.2.11's zlib.h
        #endregion

        #region Fields
        public static SafeLibraryHandle hModule;
        public static bool Loaded => hModule != null;
        public static int BufferSize { get; internal set; } = 4096;
        #endregion

        #region Windows API
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);
        #endregion

        #region LoadFunctions, ResetFunctions
        private static Delegate GetFuncPtr(string exportFunc, Type delegateType)
        {
            IntPtr funcPtr = GetProcAddress(hModule, exportFunc);
            if (funcPtr == null || funcPtr == IntPtr.Zero)
                throw new ArgumentException($"Cannot import [{exportFunc}]", new Win32Exception());
            return Marshal.GetDelegateForFunctionPointer(funcPtr, delegateType);
        }

        public static void LoadFuntions()
        {
            #region (Common) Deflate - DeflateInit2, Deflate, DeflateEnd
            DeflateInit2 = (deflateInit2_)GetFuncPtr("deflateInit2_", typeof(deflateInit2_));
            Deflate = (deflate)GetFuncPtr("deflate", typeof(deflate));
            DeflateEnd = (deflateEnd)GetFuncPtr("deflateEnd", typeof(deflateEnd));
            #endregion

            #region (Common) Inflate - InflateInit2, Inflate, InflateEnd
            InflateInit2 = (inflateInit2_)GetFuncPtr("inflateInit2_", typeof(inflateInit2_));
            Inflate = (inflate)GetFuncPtr("inflate", typeof(inflate));
            InflateEnd = (inflateEnd)GetFuncPtr("inflateEnd", typeof(inflateEnd));
            #endregion

            if (ZLibInit.ZLibProvided)
            {
                #region (zlibwapi) Checksum - Adler32, Crc32
                Adler32 = (adler32)GetFuncPtr("adler32", typeof(adler32));
                Crc32 = (crc32)GetFuncPtr("crc32", typeof(crc32));
                #endregion

                #region (zlibwapi) ZLibVersion
                ZLibVersion = (zlibVersion)GetFuncPtr("zlibVersion", typeof(zlibVersion));
                #endregion
            }
        }

        public static void ResetFuntions()
        {
            #region (Common) Deflate - DeflateInit2, Deflate, DeflateEnd
            DeflateInit2 = null;
            Deflate = null;
            DeflateEnd = null;
            #endregion

            #region (Common) Inflate - InflateInit2, Inflate, InflateEnd
            InflateInit2 = null;
            Inflate = null;
            InflateEnd = null;
            #endregion

            #region (zlibwapi) Checksum - Adler32, Crc32
            Adler32 = null;
            Crc32 = null;
            #endregion

            #region (zlibwapi) ZLibVersion
            ZLibVersion = null;
            #endregion
        }
        #endregion

        #region CheckZLibLoaded, CheckZLibUserProvided
        internal static void CheckZLibLoaded()
        {
            if (!Loaded)
                ZLibInit.GlobalInit();
        }

        internal static void CheckZLibUserProvided()
        {
            if (Loaded && !ZLibInit.ZLibProvided || !Loaded)
                throw new InvalidOperationException(MsgRequireUserProvided);
        }
        #endregion

        #region zlib Function Pointers
        #region (Common) Deflate - DeflateInit2, Deflate, DeflateEnd
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode deflateInit2_(
            ZStream strm,
            ZLibCompLevel level,
            ZLibCompMethod method,
            ZLibWriteType windowBits,
            int memLevel,
            ZLibCompressionStrategy strategy,
            [MarshalAs(UnmanagedType.LPStr)] string version,
            int stream_size);
        private static deflateInit2_ DeflateInit2;

        internal static ZLibReturnCode DeflateInit(ZStream strm, ZLibCompLevel level, ZLibWriteType windowBits)
        {
            return DeflateInit2(strm, level, ZLibCompMethod.DEFLATED, windowBits, DEF_MEM_LEVEL,
                    ZLibCompressionStrategy.DEFAULT_STRATEGY, ZLIB_VERSION, Marshal.SizeOf(typeof(ZStream)));
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode deflate(
            ZStream strm,
            ZLibFlush flush);
        internal static deflate Deflate;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode deflateEnd(
            ZStream strm);
        internal static deflateEnd DeflateEnd;
        #endregion

        #region (Common) Inflate - InflateInit2, Inflate, InflateEnd
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode inflateInit2_(
            ZStream strm,
            ZLibOpenType windowBits,
            [MarshalAs(UnmanagedType.LPStr)] string version,
            int stream_size);
        private static inflateInit2_ InflateInit2;

        internal static ZLibReturnCode InflateInit(ZStream strm, ZLibOpenType windowBits)
        {
            return InflateInit2(strm, windowBits, ZLIB_VERSION, Marshal.SizeOf(typeof(ZStream)));
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode inflate(
            ZStream strm,
            ZLibFlush flush);
        internal static inflate Inflate;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate ZLibReturnCode inflateEnd(
            ZStream strm);
        internal static inflateEnd InflateEnd;
        #endregion

        #region (zlibwapi) Checksum - Adler32, Crc32
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate uint adler32(
            uint crc,
            IntPtr buf,
            uint len);
        internal static adler32 Adler32;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate uint crc32(
            uint crc,
            IntPtr buf,
            uint len);
        internal static crc32 Crc32;
        #endregion

        #region (zlibwapi) ZLibVersion
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal delegate string zlibVersion();
        internal static zlibVersion ZLibVersion;
        #endregion
        #endregion
    }
    #endregion
}
