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

namespace Joveler.ZLibWrapper
{
	/// <summary>
	/// Classes that simplify a common use of compression streams
	/// </summary>

	delegate DeflateStream CreateStreamDelegate(Stream s, CompressionMode mode, CompressionLevel level, bool leaveOpen);

    #region DeflateCompressor
    public static class DeflateCompressor
	{
		public static MemoryStream Compress(Stream source, CompressionLevel level = CompressionLevel.Default)
		{
			return CommonCompressor.Compress(CreateStream, source, level);
		}
        public static MemoryStream Decompress(Stream source)
		{
			return CommonCompressor.Decompress(CreateStream, source);
		}
		public static byte[] Compress(byte[] source, CompressionLevel level = CompressionLevel.Default)
		{
			return CommonCompressor.Compress(CreateStream, source, level);
		}
		public static byte[] Decompress(byte[] source)
		{
			return CommonCompressor.Decompress(CreateStream, source);
		}
		private static DeflateStream CreateStream(Stream s, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			return new DeflateStream(s, mode, level, leaveOpen);
		}
	}
    #endregion

    #region ZLibCompressor
    public static class ZLibCompressor
    {
        public static MemoryStream Compress(Stream source, CompressionLevel level = CompressionLevel.Default)
        {
            return CommonCompressor.Compress(CreateStream, source, level);
        }
        public static MemoryStream Decompress(Stream source)
        {
            return CommonCompressor.Decompress(CreateStream, source);
        }
        public static byte[] Compress(byte[] source, CompressionLevel level = CompressionLevel.Default)
        {
            return CommonCompressor.Compress(CreateStream, source, level);
        }
        public static byte[] Decompress(byte[] source)
        {
            return CommonCompressor.Decompress(CreateStream, source);
        }
        private static DeflateStream CreateStream(Stream s, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            return new ZLibStream(s, mode, level, leaveOpen);
        }
    }
    #endregion

    #region GZipCompressor
    public static class GZipCompressor
	{
		public static MemoryStream Compress(Stream source, CompressionLevel level = CompressionLevel.Default)
		{
			return CommonCompressor.Compress(CreateStream, source, level);
		}
		public static MemoryStream Decompress(Stream source)
		{
			return CommonCompressor.Decompress(CreateStream, source);
		}
		public static byte[] Compress(byte[] source, CompressionLevel level = CompressionLevel.Default)
		{
			return CommonCompressor.Compress(CreateStream, source, level);
		}
		public static byte[] Decompress(byte[] source)
		{
			return CommonCompressor.Decompress(CreateStream, source);
		}
		private static DeflateStream CreateStream(Stream s, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			return new GZipStream(s, mode, level, leaveOpen);
		}
	}
    #endregion

    #region CommonCompressor
    internal class CommonCompressor
	{
		private static void Compress(CreateStreamDelegate sc, Stream source, Stream dest, CompressionLevel level)
		{
            using (DeflateStream zsDest = sc(dest, CompressionMode.Compress, level, true))
			{
				source.CopyTo(zsDest);
			}
		}

		private static void Decompress(CreateStreamDelegate sc, Stream source, Stream dest)
		{
            // CompressionLevel.Default in CompressionMode.Decompress does not affect performance or efficiency
            using (DeflateStream zsSource = sc(source, CompressionMode.Decompress, CompressionLevel.Default, true))
			{
				zsSource.CopyTo(dest);
			}
		}

		public static MemoryStream Compress(CreateStreamDelegate sc, Stream source, CompressionLevel level = CompressionLevel.Default)
		{
            MemoryStream result = new MemoryStream();
			Compress(sc, source, result, level);
			result.Position = 0;
			return result;
		}

		public static MemoryStream Decompress(CreateStreamDelegate sc, Stream source)
		{
            MemoryStream result = new MemoryStream();
			Decompress(sc, source, result);
			result.Position = 0;
			return result;
		}

		public static byte[] Compress(CreateStreamDelegate sc, byte[] source, CompressionLevel level = CompressionLevel.Default)
		{
            using (MemoryStream srcStream = new MemoryStream(source))
            using (MemoryStream dstStream = Compress(sc, srcStream, level))
            {
                return dstStream.ToArray();
            }
		}

		public static byte[] Decompress(CreateStreamDelegate sc, byte[] source)
		{
            using (MemoryStream srcStream = new MemoryStream(source))
            using (MemoryStream dstStream = Decompress(sc, srcStream))
            {
                return dstStream.ToArray();
            }
		}
	}
    #endregion
}
