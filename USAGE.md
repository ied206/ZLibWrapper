# Usage

## Initialization

ZLibWrapper internally loads zlib dynamically (using `LoadLibrary` and `GetProcAddress`).

Personal recommendation is case 1, since it provides newer version of zlib while convenient.

### Case 1 : Embedded Precompiled zlib

ZLibWrapper contains `zlibwapi.dll`, precompiled binaries of `zlib 1.2.11`.<br>
They will be copied into `$(OutDir)\x86\zlibwpi.all` and `$(OutDir)\x64\zlibwpi.all` at build.

To use `zlibwapi.dll`, call `ZLibNative.AssemblyInit(path_to_zlibwapi_dll)` explicitly at App's init code.

**WARNING**: Architecture of `zlibwapi.dll` must be matched with caller!

Put this snippet in Application's init code:

```cs
if (IntPtr.Size == 8) // This app is running on 64bit .Net Framework
    ZLibNative.AssemblyInit(Path.Combine("x64", "zlibwapi.dll"));
else // This app is running on 32bit .Net Framework
    ZLibNative.AssemblyInit(Path.Combine("x86", "zlibwapi.dll"));
```

#### Known Issue

- x86 version of embedded `zlibwapi.dll` was compiled without assembly optimization, due to [the bug](https://github.com/madler/zlib/issues/274)

### Case 2 : No Explicit Initialzation

Starting from .Net Framework 4.5, .Net has its own copy of zlib, named `clrcompression.dll`.<br>
It is stripped version of zlib, which is used in `System.IO.Compression.DeflateStream`.

If ZLibWrapper is used in .Net Framework 4.5 or later without explicit initialization, `clrcompression.dll` is used by default.

#### Limitation

- Since `clrcompression.dll` does not expose `adler32()` and `crc32()`, checksum calculation feature will be disabled.
- Application built for .Net Framework 4.0 cannot use this method.
- `clrcompression.dll` is based on quite old version of zlib (at least in .Net Framework 4.7).

### Case 3 : Custom zlib

You may want to use custom zlib because of several reasons.

In this case, call `ZLibNative.AssemblyInit` with path to custom `zlibwapi.dll`.

NOTE:
ZLibWrapper can only recognize `zlibwapi.dll`, not `zlib1.dll`.<br>
The difference is the calling convention: `zlibwapi.dll` uses `stdcall`, while `zlib1.dll` uses `cdecl`.

It is advised to prevent copy of package-embedded `zlibwapi.dll` in this case.<br>
To do so, create empty file named `Joveler.ZLibWrapper.Precompiled.Exclude` in project directory.

## Cleanup

To unload zlib dll explicitly, call `ZLibNative.AssemblyCleanup()`.

**NOTE**: Loading and unloading dll too often can impact performance.

## Compression

### DeflateStream

A stream to process a data format conforming [RFC 1951](https://www.ietf.org/rfc/rfc1951.txt).

Its API is similar to `System.IO.Compression.DeflateStream`.

Example of compression:

```cs
using (FileStream fsOrigin = new FileStream("file_origin.bin", FileMode.Open))
using (FileStream fsComp = new FileStream("test.deflate", FileMode.Create))
using (DeflateStream zs = new DeflateStream(fsComp, CompressionMode.Compress, CompressionLevel.Default, true))
{
    fsOrigin.CopyTo(zs);
}
```

`ZLibWrapper.CompressionLevel` has more option compared to `System.IO.Compression.CompressionLevel`:

```cs
public enum CompressionLevel : int
{
    NoCompression = 0,
    Fastest = 1,
    Best = 9,
    Default = -1,
    Level0 = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7,
    Level8 = 8,
    Level9 = 9,
}
```

Example of decompression:

```cs
using (FileStream fsComp = new FileStream("test.deflate", FileMode.Create))
using (FileStream fsDecomp = new FileStream("file_decomp.bin", FileMode.Open))
using (DeflateStream zs = new DeflateStream(fsComp, CompressionMode.Decompress, true))
{
    zs.CopyTo(fsDecomp);
}
```

### ZLibStream

A stream to process a data format conforming [RFC 1950](https://www.ietf.org/rfc/rfc1950.txt).

Same usage with `DeflateStream`.

### GZipStream

A stream to process a data format conforming [RFC 1952](https://www.ietf.org/rfc/rfc1952.txt).

Same usage with `DeflateStream`.

### DeflateCompressor

A helper class for `DeflateStream`.

Example of `DeflateCompressor.Compress(Stream stream)`:

```cs
using (FileStream fsOrigin = new FileStream("file_origin.bin", FileMode.Open))
using (MemoryStream msComp = DeflateCompressor.Compress(fsOrigin))
{
    // write msComp to file, or send through network, etc
}
```

Example of `DeflateCompressor.Decompress(byte[] buffer)`:

```cs
byte[] input = new byte[] { 0x73, 0x74, 0x72, 0x76, 0x71, 0x75, 0x03, 0x00 };
byte[] decompBytes = DeflateCompressor.Decompress(input);
string decompText = Encoding.UTF8.GetString(decompBytes);
Console.WriteLine(decompText); // "ABCDEF"
```

### ZLibCompressor

A helper class for `ZLibStream`.

Same usage with `DeflateCompressor`.

### GZipCompressor

A helper class for `GZipStream`.

Same usage with `DeflateCompressor`.

## Checksum

**NOTE**: To use checksum calculation, you MUST USE `zlibwapi.dll`.

### Adler32Checksum

A class to compute adler32 checksum.

Use `Append()` methods to compute checksum.<br>
Use `Checksum` property to get checksum value.

Example of `Append(Stream stream)`:

```cs
using (FileStream fs = new FileStream("read.txt", FileMode.Open))
{
    Adler32Checksum adler = new Adler32Checksum();
    adler.Append(fs);
    Console.WriteLine("0x" + adler.Checksum.ToString("X8"));
}
```

Example of `Append(byte[] buffer)` and `Append(byte[] buffer, int offset, int count)`:

```cs
Adler32Checksum adler = new Adler32Checksum();
byte[] bin = Encoding.UTF8.GetBytes("ABCDEF");

adler.Append(bin);
Console.WriteLine("0x" + adler.Checksum.ToString("X8")); // 0x057E0196

adler.Append(bin, 2, 3);
Console.WriteLine("0x" + adler.Checksum.ToString("X8")); // 0x0BD60262
```

Static wrapper methods named `Adler32Checksum.Adler32()` behave just like zlib's `adler32()` function.

Example of static wrapper methods:

```cs
byte[] bin = Encoding.UTF8.GetBytes("ABCDEF");

// Call Adler32() without checksum to use initial state.
uint checksum = Adler32Checksum.Adler32(bin);
Console.WriteLine("0x" + checksum.ToString("X8")); // 0x057E0196

// Call Adler32() with checksum to set as current state.
checksum = Adler32Checksum.Adler32(checksum, bin, 2, 3);
Console.WriteLine("0x" + checksum.ToString("X8")); // 0x0BD60262

// Stream can be passed to Adler32() as well as byte
using (MemoryStream ms = new MemoryStream(bin))
{
    checksum = Adler32Checksum.Adler32(ms);
    Console.WriteLine("0x" + checksum.ToString("X8")); // 0x057E0196
}
```

### Crc32Checksum

Same usage with `Adler32Checksum`.

To use static wrapper methods, call `Crc32Checksum.Crc32()` instead of `Adler32Checksum.Adler32()`.

### Adler32Stream

A stream designed to compute adler32 checksum on-the-fly.

Example of reading from `AdlerStream`:

```cs
using (FileStream fs = new FileStream("read.bin", FileMode.Open))
using (Adler32Stream adler = new Adler32Stream(fs))
{
    byte[] buffer = new byte[256];
    adler.Read(buffer, 0, 256);
    Console.WriteLine("0x" + adler.Checksum.ToString("X8"));

    adler.Read(buffer, 0, 128);
    Console.WriteLine("0x" + adler.Checksum.ToString("X8"));
}
```

Example of writing to `AdlerStream`:

```cs
using (FileStream fs = new FileStream("write.bin", FileMode.Create))
using (Adler32Stream adler = new Adler32Stream(fs))
{
    byte[] bin;

    bin = new byte[] { 0x01, 0x02, 0x03 };
    adler.Write(bin, 0, bin.Length);
    Console.WriteLine("0x" + adler.Checksum.ToString("X8")); // 0x000D0007

    bin = new byte[] { 0x04, 0x05, 0x06 };
    adler.Write(bin, 0, bin.Length);
    Console.WriteLine("0x" + adler.Checksum.ToString("X8")); // 0x003E0016
}
```

### Crc32Stream

Same usage with `Adler32Stream`.
