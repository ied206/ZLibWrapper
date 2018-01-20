/*
 * Forked from zlibnet v1.3.3
 * https://zlibnet.codeplex.com/
 * 
 * Licensed under zlib license.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Joveler.ZLibWrapper
{
    #region Crc32Stream
    public class Crc32Stream : Stream
    {
        private uint crc32 = 0;
        private Stream baseStream;

        public Crc32Stream(Stream stream)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            this.baseStream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readLen = baseStream.Read(buffer, offset, count);
            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                crc32 = ZLibNative.Crc32(crc32, bufferPtr[offset], (uint)readLen);
            }
            return readLen;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                crc32 = ZLibNative.Crc32(crc32, bufferPtr[offset], (uint)count);
            }
        }

        public override void Flush()
        {
            this.baseStream.Flush();
        }

        public uint Crc32 => crc32;

        public uint Checksum => crc32;

        public override bool CanRead => baseStream.CanRead;

        public override bool CanWrite => baseStream.CanWrite;

        public override bool CanSeek => (baseStream.CanSeek);

        public Stream BaseStream => baseStream;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override long Length => baseStream.Length;

        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }
    }
    #endregion

    #region Adler32Stream
    public class Adler32Stream : Stream
    {
        private uint adler32 = 1;
        private Stream baseStream;

        public Adler32Stream(Stream stream)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            this.baseStream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readLen = baseStream.Read(buffer, offset, count);
            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                adler32 = ZLibNative.Adler32(adler32, bufferPtr[offset], (uint)readLen);
            }
            return readLen;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                adler32 = ZLibNative.Adler32(adler32, bufferPtr[offset], (uint)count);
            }
        }

        public override void Flush()
        {
            this.baseStream.Flush();
        }

        public uint Adler32 => adler32;

        public uint Checksum => adler32;

        public override bool CanRead => baseStream.CanRead;

        public override bool CanWrite => baseStream.CanWrite;

        public override bool CanSeek => (baseStream.CanSeek);

        public Stream BaseStream => baseStream;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override long Length => baseStream.Length;

        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }
    }
    #endregion

    #region Crc32Checksum
    public class Crc32Checksum
    {
        #region Field, Property, Constructor
        private const int BUFFER_SIZE = 0x1000;
        private const uint CHECKSUM_INIT = 0;

        private uint _checksum;
        public uint Checksum => _checksum;

        public Crc32Checksum()
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            Reset();
        }
        #endregion

        #region Append, Reset
        public uint Append(byte[] buffer)
        {
            _checksum = Crc32Checksum.Crc32(_checksum, buffer);
            return _checksum;
        }

        public uint Append(byte[] buffer, int offset, int count)
        {
            _checksum = Crc32Checksum.Crc32(_checksum, buffer, offset, count);
            return _checksum;
        }

        public uint Append(Stream stream)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (stream.Position < stream.Length)
            {
                int readByte = stream.Read(buffer, 0, BUFFER_SIZE);
                _checksum = Crc32Checksum.Crc32(_checksum, buffer, 0, readByte);
            }
            return _checksum;
        }

        public void Reset()
        {
            _checksum = CHECKSUM_INIT;
        }
        #endregion

        #region zlib crc32 Wrapper
        public static uint Crc32(byte[] buffer)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Crc32(CHECKSUM_INIT, bufferPtr, (uint)buffer.Length);
            }
        }

        public static uint Crc32(byte[] buffer, int offset, int count)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Crc32(CHECKSUM_INIT, bufferPtr[offset], (uint)count);
            }
        }

        public static uint Crc32(Stream stream)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Crc32(buffer);
        }

        public static uint Crc32(Stream stream, int offset, int count)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Crc32(buffer, offset, count);
        }

        public static uint Crc32(uint checksum, byte[] buffer)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Crc32(checksum, bufferPtr, (uint)buffer.Length);
            }
        }

        public static uint Crc32(uint checksum, byte[] buffer, int offset, int count)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Crc32(checksum, bufferPtr[offset], (uint)count);
            }
        }

        public static uint Crc32(uint checksum, Stream stream)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Crc32(checksum, buffer);
        }

        public static uint Crc32(uint checksum, Stream stream, int offset, int count)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Crc32(checksum, buffer, offset, count);
        }
        #endregion
    }
    #endregion

    #region Adler32Checksum
    public class Adler32Checksum
    {
        #region Field, Property, Constructor
        private const int BUFFER_SIZE = 0x1000;
        private const uint CHECKSUM_INIT = 1;

        private uint _checksum;
        public uint Checksum => _checksum;

        public Adler32Checksum()
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            Reset();
        }
        #endregion

        #region Append, Reset
        public uint Append(byte[] buffer)
        {
            _checksum = Adler32Checksum.Adler32(_checksum, buffer);
            return _checksum;
        }

        public uint Append(byte[] buffer, int offset, int count)
        {
            _checksum = Adler32Checksum.Adler32(_checksum, buffer, offset, count);
            return _checksum;
        }

        public uint Append(Stream stream)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (stream.Position < stream.Length)
            {
                int readByte = stream.Read(buffer, 0, BUFFER_SIZE);
                _checksum = Adler32Checksum.Adler32(_checksum, buffer, 0, readByte);
            }
            return _checksum;
        }

        public void Reset()
        {
            _checksum = CHECKSUM_INIT;
        }
        #endregion

        #region zlib adler32 Wrapper
        public static uint Adler32(byte[] buffer)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Adler32(CHECKSUM_INIT, bufferPtr, (uint)buffer.Length);
            }
        }

        public static uint Adler32(byte[] buffer, int offset, int count)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Adler32(CHECKSUM_INIT, bufferPtr[offset], (uint)count);
            }
        }

        public static uint Adler32(Stream stream)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Adler32(buffer);
        }

        public static uint Adler32(Stream stream, int offset, int count)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Adler32(buffer, offset, count);
        }

        public static uint Adler32(uint checksum, byte[] buffer)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Adler32(checksum, bufferPtr, (uint)buffer.Length);
            }
        }

        public static uint Adler32(uint checksum, byte[] buffer, int offset, int count)
        {
            ZLibNative.CheckLoaded_UserProvidedZLib();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            using (PinnedArray bufferPtr = new PinnedArray(buffer))
            {
                return ZLibNative.Adler32(checksum, bufferPtr[offset], (uint)count);
            }
        }

        public static uint Adler32(uint checksum, Stream stream)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Adler32(checksum, buffer);
        }

        public static uint Adler32(uint checksum, Stream stream, int offset, int count)
        {
            CheckStreamLength.BiggerThanIntMax(stream);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            return Adler32(checksum, buffer, offset, count);
        }
        #endregion
    }
    #endregion

    #region Utility
    internal static class CheckStreamLength
    {
        internal static void BiggerThanIntMax(Stream stream)
        {
            if (int.MaxValue < stream.Length)
                throw new InvalidOperationException("This method cannot be used with a stream longer than int.MaxValue.");
        }
    }
    #endregion
}
