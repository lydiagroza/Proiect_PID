# Emgu CV

Emgu CV 4.13, the framework's default image library (`ImageBackend` `EmguCV` in `Directory.Build.props`). It comes in
two parts, both here, so the framework needs no Emgu CV NuGet package:

| File | What it is |
|---|---|
| `Emgu.CV.dll`, `Emgu.CV.xml` | the C# API (`Image<TColor, TDepth>`, `Mat`, `CvInvoke`, ...) and its documentation for IntelliSense, unchanged from the Emgu.CV 4.13.0.5924 NuGet package |
| `win-x64/cvextern.dll` | the native OpenCV library for Windows 10 and 11, x64 |
| `linux-x64/libcvextern.so` | for Linux x64 with glibc 2.17 or newer: Ubuntu, Mint, Debian, Fedora, RHEL/Alma/Rocky, openSUSE, Arch, ... (not Alpine, which uses musl) |
| `linux-arm64/libcvextern.so` | for Linux ARM64 with glibc 2.17 or newer (for example a Raspberry Pi with a 64-bit system) |
| `osx/libcvextern.dylib` | for macOS 11 or newer, Apple Silicon and Intel (one universal file) |

`Emgu.CV.dll` calls the native library, which was built from the Emgu CV source instead of using Emgu's own runtime
packages, whose Linux and macOS versions only work on specific systems. The native libraries only need the C library
of the system: OpenCV, the image codecs (zlib, libpng, libjpeg-turbo, libtiff, libwebp) and the C++ runtime are built
in, and there is no GTK, FFmpeg, GStreamer or other system dependency. `Emgu.CV.dll` also needs Microsoft's
System.Text.Json 9 or newer, which is part of .NET 10.

**Contents**: Emgu CV 4.13.0 with OpenCV 4.13.0, modules `core`, `imgproc`, `imgcodecs` and `photo` (the functions of
other modules, for example `features2d` or `highgui`, throw `EntryPointNotFoundException`). Built without Intel IPP
(the official Windows build uses it, which changes the last bits of a few functions, such as `BilateralFilter` and the
HLS conversion), so all the systems run OpenCV's own code; its SIMD code still makes the last bits of some float results
depend on the CPU (AVX2, SSE or ARM's NEON), which ImageCore reproduces.

**Source and license**: the native libraries were built from the Emgu CV 4.13.0 source (github.com/emgucv/emgucv,
tag 4.13.0, with its OpenCV submodule), with two small build-script changes for cross-compiling: on macOS, OpenCV's
NSImage/UIImage conversion helpers (which Emgu CV does not use) are left out, and Emgu's macOS packaging step is
skipped. Emgu CV is licensed under the GPL v3 (see `LICENSE.txt`), OpenCV under the Apache 2.0 license.
