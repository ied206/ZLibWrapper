# ZLibWrapper
C# wrapper for native zlib.

Based on [@hardon](https://www.codeplex.com/site/users/view/hardon)'s [zlibnet](https://zlibnet.codeplex.com).

Master Build Status  
[![CI Master Branch Build Status](https://ci.appveyor.com/api/projects/status/9t1fg4vyavqowb3p/branch/master?svg=true)](https://ci.appveyor.com/project/ied206/zlibwrapper/branch/master)

Develop Build Status  
[![CI Develop Branch Build Status](https://ci.appveyor.com/api/projects/status/9t1fg4vyavqowb3p/branch/develop?svg=true)](https://ci.appveyor.com/project/ied206/zlibwrapper/branch/develop)

## Feature
### Main Feature
- ZLibStream, a stream implementation conforming to [RFC 1950](https://www.ietf.org/rfc/rfc1950.txt)
- DeflateStream, GZipStream with diverse compression level
- Adler32 and CRC32 checksum calculation

### Which zlib to use?
#### clrcompression.dll
Starting from .Net Framework 4.5, .Net has its own copy of zlib 1.2.3, named `clrcompression.dll`.  
It is stripped version of zlib, which is used in `System.IO.Compression.DeflateStream`.  

ZLibWrapper uses `clrcompression.dll` by default.   
`clrcompression.dll` does not expose `adler32()` and `crc32()`, so checksum calculation feature is disabled.

#### zlibwapi.dll
ZLibWrapper contains `zlibwapi.dll`, precompiled binary of zlib 1.2.11.  
To use zlibwapi.dll, call `ZLibNative.AssemblyInit()` first with correct path to `zlibwapi.dll`.

```cs
string dllPath;
if (IntPtr.Size == 8)
    dllPath = Path.Combine("x64", "zlibwapi.dll");
else
    dllPath = Path.Combine("x86", "zlibwapi.dll");
ZLibNative.AssemblyInit(dllPath);
```

## Known Issue
- Even though `_zstream` was pinned by `GCHandle` in `DeflateStream`, unit tests rarely fails due to change of `_zstream`'s address.

