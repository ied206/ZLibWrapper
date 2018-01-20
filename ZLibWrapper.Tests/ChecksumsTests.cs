using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Joveler.ZLibWrapper.Tests
{
    [TestClass]
    public class ChecksumsTests
    {
        #region Crc32Stream
        [TestMethod]
        [TestCategory("Crc32Stream")]
        public void Crc32Stream_1()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex1.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Crc32Stream crc = new Crc32Stream(ms))
                {
                    fs.CopyTo(crc);
                    Assert.IsTrue(crc.Checksum == 0x1961D0C6);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Stream")]
        public void Crc32Stream_2()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex2.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Crc32Stream crc = new Crc32Stream(ms))
                {
                    fs.CopyTo(crc);
                    Assert.IsTrue(crc.Checksum == 0x7641A243);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Stream")]
        public void Crc32Stream_3()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Crc32Stream crc = new Crc32Stream(ms))
                {
                    fs.CopyTo(crc);
                    Assert.IsTrue(crc.Checksum == 0x63D4D64B);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Stream")]
        public void Crc32Stream_4()
        {
            if (ZLibNative.ZLibProvided)
            {
                using (MemoryStream ms = new MemoryStream())
                using (Crc32Stream crc = new Crc32Stream(ms))
                {
                    byte[] bin = Encoding.UTF8.GetBytes("ABCDEF");
                    crc.Write(bin, 0, bin.Length);
                    Assert.IsTrue(crc.Checksum == 0xBB76FE69);
                }
            }
        }
        #endregion

        #region Crc32Checksum
        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_1()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex1.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Crc32Checksum crc = new Crc32Checksum();
                    crc.Append(fs);
                    Assert.IsTrue(crc.Checksum == 0x1961D0C6);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_2()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex2.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Crc32Checksum crc = new Crc32Checksum();
                    crc.Append(fs);
                    Assert.IsTrue(crc.Checksum == 0x7641A243);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_3()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Crc32Checksum crc = new Crc32Checksum();
                    crc.Append(fs);
                    Assert.IsTrue(crc.Checksum == 0x63D4D64B);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_4()
        {
            if (ZLibNative.ZLibProvided)
            {
                Crc32Checksum crc = new Crc32Checksum();
                crc.Append(Encoding.UTF8.GetBytes("ABC"));
                Assert.IsTrue(crc.Checksum == 0xA3830348); // ABC
                crc.Append(Encoding.UTF8.GetBytes("DEF"));
                Assert.IsTrue(crc.Checksum == 0xBB76FE69); // ABCDEF
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_5()
        {
            if (ZLibNative.ZLibProvided)
            {
                uint checksum = Crc32Checksum.Crc32(Encoding.UTF8.GetBytes("ABC"));
                Assert.IsTrue(checksum == 0xA3830348); // ABC
                checksum = Crc32Checksum.Crc32(checksum, Encoding.UTF8.GetBytes("DEF"));
                Assert.IsTrue(checksum == 0xBB76FE69); // ABCDEF
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_6()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample = Encoding.UTF8.GetBytes("ABCDEF");

                uint checksum = Crc32Checksum.Crc32(sample, 1, 3); 
                Assert.IsTrue(checksum == 0x26BA19F3); // BCD
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_7()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    uint checksum = Crc32Checksum.Crc32(fs);
                    Assert.IsTrue(checksum == 0x63D4D64B);
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_8()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample1 = Encoding.UTF8.GetBytes("ABC");
                byte[] sample2 = Encoding.UTF8.GetBytes("DEF");

                using (MemoryStream ms1 = new MemoryStream(sample1))
                using (MemoryStream ms2 = new MemoryStream(sample2))
                {
                    uint checksum = Crc32Checksum.Crc32(ms1);
                    Assert.IsTrue(checksum == 0xA3830348); // ABC
                    checksum = Crc32Checksum.Crc32(checksum, ms2);
                    Assert.IsTrue(checksum == 0xBB76FE69); // ABCDEF
                }
            }
        }

        [TestMethod]
        [TestCategory("Crc32Checksum")]
        public void Crc32Checksum_9()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample = Encoding.UTF8.GetBytes("ABCDEF");

                using (MemoryStream ms = new MemoryStream(sample))
                {
                    uint checksum = Crc32Checksum.Crc32(ms, 1, 3);
                    Assert.IsTrue(checksum == 0x26BA19F3); // BCD
                }
            }
        }
        #endregion

        #region Adler32Stream
        [TestMethod]
        [TestCategory("Adler32Stream")]
        public void Adler32Stream_1()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex1.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Adler32Stream adler = new Adler32Stream(ms))
                {
                    fs.CopyTo(adler);
                    Assert.IsTrue(adler.Adler32 == 0xD77C7044);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Stream")]
        public void Adler32Stream_2()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex2.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Adler32Stream adler = new Adler32Stream(ms))
                {
                    fs.CopyTo(adler);
                    Assert.IsTrue(adler.Adler32 == 0x9B97EDAD);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Stream")]
        public void Adler32Stream_3()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (MemoryStream ms = new MemoryStream())
                using (Adler32Stream adler = new Adler32Stream(ms))
                {
                    fs.CopyTo(adler);
                    Assert.IsTrue(adler.Adler32 == 0x94B04C6F);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Stream")]
        public void Adler32Stream_4()
        {
            if (ZLibNative.ZLibProvided)
            {
                using (MemoryStream ms = new MemoryStream())
                using (Adler32Stream adler = new Adler32Stream(ms))
                {
                    byte[] bin = Encoding.UTF8.GetBytes("ABCDEF");
                    adler.Write(bin, 0, bin.Length);
                    Assert.IsTrue(adler.Adler32 == 0x057E0196);
                }
            }
        }
        #endregion

        #region Adler32Checksum
        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_1()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex1.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Adler32Checksum adler = new Adler32Checksum();
                    adler.Append(fs);
                    Assert.IsTrue(adler.Checksum == 0xD77C7044);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_2()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex2.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Adler32Checksum adler = new Adler32Checksum();
                    adler.Append(fs);
                    Assert.IsTrue(adler.Checksum == 0x9B97EDAD);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_3()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Adler32Checksum adler = new Adler32Checksum();
                    adler.Append(fs);
                    Assert.IsTrue(adler.Checksum == 0x94B04C6F);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_4()
        {
            if (ZLibNative.ZLibProvided)
            {
                Adler32Checksum adler = new Adler32Checksum();
                adler.Append(Encoding.UTF8.GetBytes("ABC"));
                Assert.IsTrue(adler.Checksum == 0x018D00C7); // ABC
                adler.Append(Encoding.UTF8.GetBytes("DEF"));
                Assert.IsTrue(adler.Checksum == 0x057E0196); // ABCDEF
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_5()
        {
            if (ZLibNative.ZLibProvided)
            {
                uint checksum = Adler32Checksum.Adler32(Encoding.UTF8.GetBytes("ABC"));
                Assert.IsTrue(checksum == 0x018D00C7); // ABC
                checksum = Adler32Checksum.Adler32(checksum, Encoding.UTF8.GetBytes("DEF"));
                Assert.IsTrue(checksum == 0x057E0196); // ABCDEF
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_6()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample = Encoding.UTF8.GetBytes("ABCDEF");

                uint checksum = Adler32Checksum.Adler32(sample, 1, 3);
                Assert.IsTrue(checksum == 0x019300CA); // BCD
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_7()
        {
            if (ZLibNative.ZLibProvided)
            {
                string filePath = Path.Combine(TestHelper.BaseDir, "ex3.jpg");
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    uint checksum = Adler32Checksum.Adler32(fs);
                    Assert.IsTrue(checksum == 0x94B04C6F);
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_8()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample1 = Encoding.UTF8.GetBytes("ABC");
                byte[] sample2 = Encoding.UTF8.GetBytes("DEF");

                using (MemoryStream ms1 = new MemoryStream(sample1))
                using (MemoryStream ms2 = new MemoryStream(sample2))
                {
                    uint checksum = Adler32Checksum.Adler32(ms1);
                    Assert.IsTrue(checksum == 0x018D00C7); // ABC
                    checksum = Adler32Checksum.Adler32(checksum, ms2);
                    Assert.IsTrue(checksum == 0x057E0196); // ABCDEF
                }
            }
        }

        [TestMethod]
        [TestCategory("Adler32Checksum")]
        public void Adler32Checksum_9()
        {
            if (ZLibNative.ZLibProvided)
            {
                byte[] sample = Encoding.UTF8.GetBytes("ABCDEF");

                using (MemoryStream ms = new MemoryStream(sample))
                {
                    uint checksum = Adler32Checksum.Adler32(ms, 1, 3);
                    Assert.IsTrue(checksum == 0x019300CA); // BCD
                }
            }
        }
        #endregion
    }
}
