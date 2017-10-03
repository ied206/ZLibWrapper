using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;
using System.Linq;

namespace ZLibWrapper.Tests
{
    [TestClass]
    public class ZLibCompressorsTests
    {
        #region DeflateCompressor - Compress
        public void DeflateCompressor_Compress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream compMs = DeflateCompressor.Compress(fs))
                {
                    using (MemoryStream decompMs = DeflateCompressor.Decompress(compMs))
                    {
                        // Compare SHA256 Digest
                        fs.Position = 0;
                        byte[] fileDigest = TestHelper.SHA256Digest(fs);
                        byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                        Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Compressor_1()
        {
            DeflateCompressor_Compress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Compressor_2()
        {
            DeflateCompressor_Compress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Compressor_3()
        {
            DeflateCompressor_Compress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Compressor_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");

            // Compress first,
            // 73-74-72-76-71-75-03-00
            byte[] compBytes = DeflateCompressor.Compress(input);

            // then Decompress.
            byte[] decompBytes = DeflateCompressor.Decompress(compBytes);

            // Comprare SHA256 Digest
            byte[] inputDigest = TestHelper.SHA256Digest(input);
            byte[] decompDigest = TestHelper.SHA256Digest(decompBytes);
            Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
        }
        #endregion

        #region DeflateCompressor - Decompress
        public void DeflateCompressor_Decompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".deflate");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream decompMs = DeflateCompressor.Decompress(compFs))
                {
                    // Compare SHA256 Digest
                    byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                    byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                    Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                }
            }
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Decompressor_1()
        {
            DeflateCompressor_Decompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Decompressor_2()
        {
            DeflateCompressor_Decompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Decompressor_3()
        {
            DeflateCompressor_Decompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateCompressor")]
        public void DeflateStream_Decompressor_4()
        {
            byte[] input = new byte[] { 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decompBytes = DeflateCompressor.Decompress(input);
                Assert.IsTrue(decompBytes.SequenceEqual(plaintext));
            }
        }
        #endregion

        #region ZLibCompressor - Compress
        public void ZLibCompressor_Compress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream compMs = ZLibCompressor.Compress(fs))
                {
                    using (MemoryStream decompMs = ZLibCompressor.Decompress(compMs))
                    {
                        // Compare SHA256 Digest
                        fs.Position = 0;
                        byte[] fileDigest = TestHelper.SHA256Digest(fs);
                        byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                        Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Compressor_1()
        {
            ZLibCompressor_Compress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Compressor_2()
        {
            ZLibCompressor_Compress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Compressor_3()
        {
            ZLibCompressor_Compress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Compressor_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");
            
            // Compress first,
            // 78-9C-73-74-72-76-71-75-03-00-05-7E-01-96
            byte[] compBytes = ZLibCompressor.Compress(input);

            // then Decompress.
            byte[] decompBytes = ZLibCompressor.Decompress(compBytes);

            // Comprare SHA256 Digest
            byte[] inputDigest = TestHelper.SHA256Digest(input);
            byte[] decompDigest = TestHelper.SHA256Digest(decompBytes);
            Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
        }
        #endregion

        #region ZLibCompressor - Decompress
        public void ZLibCompressor_Decompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".zz");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream decompMs = ZLibCompressor.Decompress(compFs))
                {
                    // Compare SHA256 Digest
                    byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                    byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                    Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                }
            }
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Decompressor_1()
        {
            ZLibCompressor_Decompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Decompressor_2()
        {
            ZLibCompressor_Decompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Decompressor_3()
        {
            ZLibCompressor_Decompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibCompressor")]
        public void ZLibStream_Decompressor_4()
        {
            byte[] input = new byte[] { 0x78, 0x9C, 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00, 0x05, 0x7E, 0x01, 0x96 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decompBytes = ZLibCompressor.Decompress(input);
                Assert.IsTrue(decompBytes.SequenceEqual(plaintext));
            }
        }
        #endregion

        #region GZipCompressor - Compress
        public void GZipCompressor_Compress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream compMs = GZipCompressor.Compress(fs))
                {
                    using (MemoryStream decompMs = GZipCompressor.Decompress(compMs))
                    {
                        // Compare SHA256 Digest
                        fs.Position = 0;
                        byte[] fileDigest = TestHelper.SHA256Digest(fs);
                        byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                        Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Compressor_1()
        {
            GZipCompressor_Compress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Compressor_2()
        {
            GZipCompressor_Compress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Compressor_3()
        {
            GZipCompressor_Compress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Compressor_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");

            // Compress first,
            // 1F-8B-08-00-00-00-00-00-00-0A-73-74-72-76-71-75-03-00-69-FE-76-BB-06-00-00-00
            byte[] compBytes = GZipCompressor.Compress(input);

            // then Decompress.
            byte[] decompBytes = GZipCompressor.Decompress(compBytes);

            // Comprare SHA256 Digest
            byte[] inputDigest = TestHelper.SHA256Digest(input);
            byte[] decompDigest = TestHelper.SHA256Digest(decompBytes);
            Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
        }
        #endregion

        #region GZipCompressor - Decompress
        public void GZipCompressor_Decompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".gz");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream decompMs = GZipCompressor.Decompress(compFs))
                {
                    // Compare SHA256 Digest
                    byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                    byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                    Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
                }
            }
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Decompressor_1()
        {
            GZipCompressor_Decompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Decompressor_2()
        {
            GZipCompressor_Decompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Decompressor_3()
        {
            GZipCompressor_Decompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("GZipCompressor")]
        public void GZipStream_Decompressor_4()
        {
            byte[] input = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00, 0x69, 0xFE, 0x76, 0xBB, 0x06, 0x00, 0x00, 0x00 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decompBytes = GZipCompressor.Decompress(input);
                Assert.IsTrue(decompBytes.SequenceEqual(plaintext));
            }
        }
        #endregion
    }
}
