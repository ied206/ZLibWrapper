using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Joveler.ZLibWrapper.Tests
{
    [TestClass]
    public class ZLibStreamsTests
    {
        #region DeflateStream - Compress
        public void DeflateStream_FileCompress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (DeflateStream zs = new DeflateStream(compMs, CompressionMode.Compress, level, true))
                {
                    fs.CopyTo(zs);
                }

                fs.Position = 0;
                compMs.Position = 0;

                // Decompress compMs again
                using (DeflateStream zs = new DeflateStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(fs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Compress_1()
        {
            DeflateStream_FileCompress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Compress_2()
        {
            DeflateStream_FileCompress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Compress_3()
        {
            DeflateStream_FileCompress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Compress_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (DeflateStream zs = new DeflateStream(compMs, CompressionMode.Compress, CompressionLevel.Default, true))
                {
                    zs.Write(input, 0, input.Length);
                }

                compMs.Position = 0;
                // 73-74-72-76-71-75-03-00

                // Decompress compMs again
                using (DeflateStream zs = new DeflateStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] inputDigest = TestHelper.SHA256Digest(input);

                Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
            }
        }
        #endregion

        #region DeflateStream - Decompress
        public void DeflateStream_FileDecompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".deflate");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (MemoryStream decompMs = new MemoryStream())
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (DeflateStream zs = new DeflateStream(compFs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Decompress_1()
        {
            DeflateStream_FileDecompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Decompress_2()
        {
            DeflateStream_FileDecompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Decompress_3()
        {
            DeflateStream_FileDecompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("DeflateStream")]
        public void DeflateStream_Decompress_4()
        {
            byte[] input = new byte[] { 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (MemoryStream inputMs = new MemoryStream(input))
                using (DeflateStream zs = new DeflateStream(inputMs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decomp = decompMs.ToArray();

                Assert.IsTrue(decomp.SequenceEqual(plaintext));
            }
        }
        #endregion

        #region ZLibStream - Compress
        public void ZLibStream_FileCompress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (ZLibStream zs = new ZLibStream(compMs, CompressionMode.Compress, level, true))
                {
                    fs.CopyTo(zs);
                }

                fs.Position = 0;
                compMs.Position = 0;

                // Decompress compMs again
                using (ZLibStream zs = new ZLibStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(fs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Compress_1()
        {
            ZLibStream_FileCompress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Compress_2()
        {
            ZLibStream_FileCompress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Compress_3()
        {
            ZLibStream_FileCompress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Compress_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (ZLibStream zs = new ZLibStream(compMs, CompressionMode.Compress, CompressionLevel.Default, true))
                {
                    zs.Write(input, 0, input.Length);
                }

                compMs.Position = 0;
                // 78-9C-73-74-72-76-71-75-03-00-05-7E-01-96

                // Decompress compMs again
                using (ZLibStream zs = new ZLibStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] inputDigest = TestHelper.SHA256Digest(input);

                Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
            }
        }
        #endregion

        #region ZLibStream - Decompress
        public void ZLibStream_FileDecompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".zz");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (MemoryStream decompMs = new MemoryStream())
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (ZLibStream zs = new ZLibStream(compFs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Decompress_1()
        {
            ZLibStream_FileDecompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Decompress_2()
        {
            ZLibStream_FileDecompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Decompress_3()
        {
            ZLibStream_FileDecompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("ZLibStream")]
        public void ZLibStream_Decompress_4()
        {
            byte[] input = new byte[] { 0x78, 0x9C, 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00, 0x05, 0x7E, 0x01, 0x96 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (MemoryStream inputMs = new MemoryStream(input))
                using (ZLibStream zs = new ZLibStream(inputMs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decomp = decompMs.ToArray();

                Assert.IsTrue(decomp.SequenceEqual(plaintext));
            }
        }
        #endregion

        #region GZipStream - Compress
        public void GZipStream_FileCompress_Template(string fileName, CompressionLevel level)
        {
            string filePath = Path.Combine(TestHelper.BaseDir, fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(compMs, CompressionMode.Compress, level, true))
                {
                    fs.CopyTo(zs);
                }

                fs.Position = 0;
                compMs.Position = 0;

                // Decompress compMs again
                using (GZipStream zs = new GZipStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(fs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Compress_1()
        {
            GZipStream_FileCompress_Template("ex1.jpg", CompressionLevel.Default);
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Compress_2()
        {
            GZipStream_FileCompress_Template("ex2.jpg", CompressionLevel.Best);
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Compress_3()
        {
            GZipStream_FileCompress_Template("ex3.jpg", CompressionLevel.Fastest);
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Compress_4()
        {
            byte[] input = Encoding.UTF8.GetBytes("ABCDEF");
            using (MemoryStream compMs = new MemoryStream())
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(compMs, CompressionMode.Compress, CompressionLevel.Default, true))
                {
                    zs.Write(input, 0, input.Length);
                }

                compMs.Position = 0;
                // 1F-8B-08-00-00-00-00-00-00-0A-73-74-72-76-71-75-03-00-69-FE-76-BB-06-00-00-00

                // Decompress compMs again
                using (GZipStream zs = new GZipStream(compMs, CompressionMode.Decompress, true))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] inputDigest = TestHelper.SHA256Digest(input);

                Assert.IsTrue(decompDigest.SequenceEqual(inputDigest));
            }
        }
        #endregion

        #region GZipStream - Decompress
        public void GZipStream_FileDecompress_Template(string fileName)
        {
            string compPath = Path.Combine(TestHelper.BaseDir, fileName + ".gz");
            string decompPath = Path.Combine(TestHelper.BaseDir, fileName);
            using (MemoryStream decompMs = new MemoryStream())
            using (FileStream decompFs = new FileStream(decompPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream compFs = new FileStream(compPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (GZipStream zs = new GZipStream(compFs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                // Compare SHA256 Digest
                byte[] decompDigest = TestHelper.SHA256Digest(decompMs);
                byte[] fileDigest = TestHelper.SHA256Digest(decompFs);
                Assert.IsTrue(decompDigest.SequenceEqual(fileDigest));
            }
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Decompress_1()
        {
            GZipStream_FileDecompress_Template("ex1.jpg");
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Decompress_2()
        {
            GZipStream_FileDecompress_Template("ex2.jpg");
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Decompress_3()
        {
            GZipStream_FileDecompress_Template("ex3.jpg");
        }

        [TestMethod]
        [TestCategory("GZipStream")]
        public void GZipStream_Decompress_4()
        {
            byte[] input = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00, 0x69, 0xFE, 0x76, 0xBB, 0x06, 0x00, 0x00, 0x00 };
            using (MemoryStream decompMs = new MemoryStream())
            {
                using (MemoryStream inputMs = new MemoryStream(input))
                using (GZipStream zs = new GZipStream(inputMs, CompressionMode.Decompress))
                {
                    zs.CopyTo(decompMs);
                }

                decompMs.Position = 0;

                byte[] plaintext = Encoding.UTF8.GetBytes("ABCDEF");
                byte[] decomp = decompMs.ToArray();

                Assert.IsTrue(decomp.SequenceEqual(plaintext));
            }
        }
        #endregion
    }
}
