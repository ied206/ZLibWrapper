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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Joveler.ZLib
{
    #region DeflateStream
    public class DeflateStream : Stream
    {
        #region Fields
        private Stream _baseStream;
        private readonly ZLibMode _mode;
        private readonly bool _leaveOpen;
        private bool _disposed = false;

        private ZStream _zstream;
        private GCHandle _zstreamPtr;

        protected virtual ZLibOpenType OpenType => ZLibOpenType.Deflate;
        protected virtual ZLibWriteType WriteType => ZLibWriteType.Deflate;

        private readonly byte[] _internalBuf;
        private int _internalBufPos = 0;
        #endregion

        #region Properties
        public long TotalIn { get; private set; } = 0;
        public long TotalOut { get; private set; } = 0;
        public Stream BaseStream => _baseStream;
        #endregion

        #region Constructor
        public DeflateStream(Stream stream, ZLibMode mode)
            : this(stream, mode, ZLibCompLevel.Default, false) { }

        public DeflateStream(Stream stream, ZLibMode mode, ZLibCompLevel level) :
            this(stream, mode, level, false)
        { }

        public DeflateStream(Stream stream, ZLibMode mode, bool leaveOpen) :
            this(stream, mode, ZLibCompLevel.Default, leaveOpen)
        { }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public DeflateStream(Stream stream, ZLibMode mode, ZLibCompLevel level, bool leaveOpen)
        {
            NativeMethods.CheckZLibLoaded();

            _zstream = new ZStream();
            _zstreamPtr = GCHandle.Alloc(_zstream, GCHandleType.Pinned);

            _leaveOpen = leaveOpen;
            _baseStream = stream;
            _mode = mode;
            _internalBufPos = 0;

            Debug.Assert(0 < NativeMethods.BufferSize, "Internal Logic Error at DeflateStream");
            _internalBuf = new byte[NativeMethods.BufferSize];

            ZLibReturnCode ret;
            if (_mode == ZLibMode.Compress)
                ret = NativeMethods.DeflateInit(_zstream, level, WriteType);
            else
                ret = NativeMethods.InflateInit(_zstream, OpenType);

            ZLibException.CheckZLibOK(ret, _zstream);
        }
        #endregion

        #region Disposable Pattern
        ~DeflateStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_baseStream != null)
                {
                    if (_mode == ZLibMode.Compress)
                        Flush();
                    if (!_leaveOpen)
                        _baseStream.Close();
                    _baseStream = null;
                }

                if (_zstream != null)
                {
                    if (_mode == ZLibMode.Compress)
                        NativeMethods.DeflateEnd(_zstream);
                    else
                        NativeMethods.InflateEnd(_zstream);
                    _zstreamPtr.Free();
                    _zstream = null;
                }

                _disposed = true;
            }
        }
        #endregion

        #region ValidateReadWriteArgs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ValidateReadWriteArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count));
        }
        #endregion

        #region Stream Methods
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_mode != ZLibMode.Decompress)
                throw new NotSupportedException("Read() not supported on compression");

            ValidateReadWriteArgs(buffer, offset, count);

            int readLen = 0;
            if (_internalBufPos != -1)
            {
                using (PinnedArray pinRead = new PinnedArray(_internalBuf)) // [In] Compressed
                using (PinnedArray pinWrite = new PinnedArray(buffer)) // [Out] Will-be-decompressed
                {
                    _zstream.NextIn = pinRead[_internalBufPos];
                    _zstream.NextOut = pinWrite[offset];
                    _zstream.AvailOut = (uint)count;

                    while (0 < _zstream.AvailOut)
                    {
                        if (_zstream.AvailIn == 0)
                        { // Compressed Data is no longer available in array, so read more from _stream
                            int baseReadSize = _baseStream.Read(_internalBuf, 0, _internalBuf.Length);

                            _internalBufPos = 0;
                            _zstream.NextIn = pinRead;
                            _zstream.AvailIn = (uint)baseReadSize;
                            TotalIn += baseReadSize;
                        }

                        uint inCount = _zstream.AvailIn;
                        uint outCount = _zstream.AvailOut;

                        // flush method for inflate has no effect
                        ZLibReturnCode ret = NativeMethods.Inflate(_zstream, ZLibFlush.NO_FLUSH);

                        _internalBufPos += (int)(inCount - _zstream.AvailIn);
                        readLen += (int)(outCount - _zstream.AvailOut);

                        if (ret == ZLibReturnCode.STREAM_END)
                        {
                            _internalBufPos = -1; // magic for StreamEnd
                            break;
                        }

                        ZLibException.CheckZLibOK(ret, _zstream);
                    }

                    TotalOut += readLen;
                }
            }
            return readLen;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_mode != ZLibMode.Compress)
                throw new NotSupportedException("Write() not supported on decompression");

            TotalIn += count;

            using (PinnedArray pinRead = new PinnedArray(buffer))
            using (PinnedArray pinWrite = new PinnedArray(_internalBuf))
            {
                _zstream.NextIn = pinRead[offset];
                _zstream.AvailIn = (uint)count;
                _zstream.NextOut = pinWrite[_internalBufPos];
                _zstream.AvailOut = (uint)(_internalBuf.Length - _internalBufPos);

                while (_zstream.AvailIn != 0)
                {
                    uint outCount = _zstream.AvailOut;
                    ZLibReturnCode ret = NativeMethods.Deflate(_zstream, ZLibFlush.NO_FLUSH);
                    _internalBufPos += (int)(outCount - _zstream.AvailOut);

                    if (_zstream.AvailOut == 0)
                    {
                        _baseStream.Write(_internalBuf, 0, _internalBuf.Length);
                        TotalOut += _internalBuf.Length;

                        _internalBufPos = 0;
                        _zstream.NextOut = pinWrite;
                        _zstream.AvailOut = (uint)_internalBuf.Length;
                    }

                    ZLibException.CheckZLibOK(ret, _zstream);
                }
            }
        }

        public override void Flush()
        {
            if (_mode == ZLibMode.Decompress)
            {
                _baseStream.Flush();
                return;
            }

            using (PinnedArray pinWrite = new PinnedArray(_internalBuf))
            {
                _zstream.NextIn = IntPtr.Zero;
                _zstream.AvailIn = 0;
                _zstream.NextOut = pinWrite[_internalBufPos];
                _zstream.AvailOut = (uint)(_internalBuf.Length - _internalBufPos);

                ZLibReturnCode ret = ZLibReturnCode.OK;
                while (ret != ZLibReturnCode.STREAM_END)
                {
                    if (_zstream.AvailOut != 0)
                    {
                        uint outCount = _zstream.AvailOut;
                        ret = NativeMethods.Deflate(_zstream, ZLibFlush.FINISH);

                        _internalBufPos += (int)(outCount - _zstream.AvailOut);

                        if (ret != ZLibReturnCode.STREAM_END && ret != ZLibReturnCode.OK)
                            throw new ZLibException(ret, _zstream.LastErrorMsg);
                    }

                    _baseStream.Write(_internalBuf, 0, _internalBufPos);
                    TotalOut += _internalBufPos;

                    _internalBufPos = 0;
                    _zstream.NextOut = pinWrite;
                    _zstream.AvailOut = (uint)_internalBuf.Length;
                }
            }

            _baseStream.Flush();
        }

        public override bool CanRead => _mode == ZLibMode.Decompress && _baseStream.CanRead;
        public override bool CanWrite => _mode == ZLibMode.Compress && _baseStream.CanWrite;
        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek() not supported");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength not supported");
        }

        public override long Length => throw new NotSupportedException("Length not supported");

        public override long Position
        {
            get => throw new NotSupportedException("Position not supported");
            set => throw new NotSupportedException("Position not supported");
        }

        public double CompressionRatio
        {
            get
            {
                if (_mode == ZLibMode.Compress)
                {
                    if (TotalIn == 0)
                        return 0;
                    return 100 - TotalOut * 100.0 / TotalIn;
                }
                else
                {
                    if (TotalOut == 0)
                        return 0;
                    return 100 - TotalIn * 100.0 / TotalOut;
                }
            }
        }
        #endregion
    }
    #endregion

    #region ZLibStream
    /// <inheritdoc />
    /// <summary>
    /// zlib header + adler32 et end.
    /// wraps a deflate stream
    /// </summary>
    public class ZLibStream : DeflateStream
    {
        public ZLibStream(Stream stream, ZLibMode mode)
            : base(stream, mode) { }

        public ZLibStream(Stream stream, ZLibMode mode, bool leaveOpen) :
            base(stream, mode, leaveOpen)
        { }

        public ZLibStream(Stream stream, ZLibMode mode, ZLibCompLevel level) :
            base(stream, mode, level)
        { }

        public ZLibStream(Stream stream, ZLibMode mode, ZLibCompLevel level, bool leaveOpen) :
            base(stream, mode, level, leaveOpen)
        { }

        protected override ZLibOpenType OpenType => ZLibOpenType.ZLib;
        protected override ZLibWriteType WriteType => ZLibWriteType.ZLib;
    }
    #endregion

    #region GZipStream
    /// <inheritdoc />
    /// <summary>
    /// Saved to file (.gz) can be opened with zip utils.
    /// Have hdr + crc32 at end.
    /// Wraps a deflate stream
    /// </summary>
    public class GZipStream : DeflateStream
    {
        public GZipStream(Stream stream, ZLibMode mode)
            : base(stream, mode) { }

        public GZipStream(Stream stream, ZLibMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) { }

        public GZipStream(Stream stream, ZLibMode mode, ZLibCompLevel level)
            : base(stream, mode, level) { }

        public GZipStream(Stream stream, ZLibMode mode, ZLibCompLevel level, bool leaveOpen)
            : base(stream, mode, level, leaveOpen) { }

        protected override ZLibOpenType OpenType => ZLibOpenType.GZip;
        protected override ZLibWriteType WriteType => ZLibWriteType.GZip;
    }
    #endregion
}
