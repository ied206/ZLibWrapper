# ZLibWrapper
C# wrapper for native zlib.

Based on [zlibnet](https://zlibnet.codeplex.com) by [@hardon](https://www.codeplex.com/site/users/view/hardon).

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
Using lastest version of `zlibwapi.dll` is advised.

#### clrcompression.dll
Starting from .Net Framework 4.5, .Net has its own copy of zlib 1.2.3, named `clrcompression.dll`.  
It is stripped version of zlib, which is used in `System.IO.Compression.DeflateStream`.  

ZLibWrapper uses `clrcompression.dll` by default.   
`clrcompression.dll` does not expose `adler32()` and `crc32()`, so checksum calculation feature is disabled.

#### zlibwapi.dll
ZLibWrapper contains `zlibwapi.dll`, precompiled binary of zlib 1.2.11.  
To use zlibwapi.dll, call `ZLibNative.AssemblyInit(path_to_zlibwapi_dll)` at App's init code.

```cs
string dllPath;
if (IntPtr.Size == 8)
    dllPath = Path.Combine("x64", "zlibwapi.dll");
else
    dllPath = Path.Combine("x86", "zlibwapi.dll");
ZLibNative.AssemblyInit(dllPath);
```
