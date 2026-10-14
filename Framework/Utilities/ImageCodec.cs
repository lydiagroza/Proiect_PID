using System;
using System.IO;
using SkiaSharp;

namespace Framework.Utilities
{
    /// <summary>
    /// Reads and writes image files with SkiaSharp (the graphics library Avalonia uses on every platform),
    /// so that loading and saving behave the same with both image backends.
    /// </summary>
    internal static class ImageCodec
    {
        /// <summary>
        /// Decodes an image file into 8-bit BGR pixels as [row, column, channel], applying the EXIF orientation.
        /// Like OpenCV, the alpha channel (if any) is ignored.
        /// </summary>
        public static byte[,,] DecodeBgr(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var codec = SKCodec.Create(stream))
            {
                if (codec == null)
                    throw new InvalidDataException("The file is not an image in a supported format (JPEG, PNG, BMP, GIF, WebP).");

                var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                using (var bitmap = new SKBitmap(info))
                {
                    SKCodecResult result = codec.GetPixels(info, bitmap.GetPixels());
                    if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
                        throw new InvalidDataException($"The image could not be decoded ({result}).");

                    return ToBgr(bitmap.GetPixelSpan(), info.Width, info.Height, bitmap.RowBytes, codec.EncodedOrigin);
                }
            }
        }

        /// <summary>
        /// Encodes 8-bit pixels ([row, column, channel] with 1 channel for gray or 3 channels for BGR) into a file;
        /// the format is chosen from the extension: .png, .bmp, .webp (lossless), otherwise JPEG (quality 100, no chroma subsampling).
        /// </summary>
        public static void Encode(string path, byte[,,] data, int width, int height)
        {
            int channels = data.GetLength(2);
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension == ".bmp")
            {
                File.WriteAllBytes(path, EncodeBmp(data, width, height));
                return;
            }

            SKImageInfo info = channels == 1
                ? new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque)
                : new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);

            using (var bitmap = new SKBitmap(info))
            {
                Span<byte> pixels = bitmap.GetPixelSpan();
                int rowBytes = bitmap.RowBytes;
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        if (channels == 1)
                        {
                            pixels[y * rowBytes + x] = data[y, x, 0];
                        }
                        else
                        {
                            int i = y * rowBytes + 4 * x;
                            pixels[i] = data[y, x, 0];
                            pixels[i + 1] = data[y, x, 1];
                            pixels[i + 2] = data[y, x, 2];
                            pixels[i + 3] = 255;
                        }
                    }
                }

                using (SKPixmap pixmap = bitmap.PeekPixels())
                using (SKData encoded = Encode(pixmap, extension))
                {
                    if (encoded == null)
                        throw new IOException($"The image could not be encoded as {extension}.");

                    using (var file = File.Create(path))
                        encoded.SaveTo(file);
                }
            }
        }

        private static SKData Encode(SKPixmap pixmap, string extension)
        {
            switch (extension)
            {
                case ".png":
                    return pixmap.Encode(SKEncodedImageFormat.Png, 100);
                case ".webp":
                    return pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 100));
                default:
                    // best quality: no chroma subsampling
                    return pixmap.Encode(new SKJpegEncoderOptions(100, SKJpegEncoderDownsample.Downsample444, SKJpegEncoderAlphaOption.Ignore));
            }
        }

        private static byte[,,] ToBgr(ReadOnlySpan<byte> bgra, int width, int height, int rowBytes, SKEncodedOrigin origin)
        {
            bool swapsSides = origin == SKEncodedOrigin.LeftTop || origin == SKEncodedOrigin.RightTop ||
                              origin == SKEncodedOrigin.RightBottom || origin == SKEncodedOrigin.LeftBottom;
            int outWidth = swapsSides ? height : width;
            int outHeight = swapsSides ? width : height;
            var result = new byte[outHeight, outWidth, 3];

            for (int y = 0; y < outHeight; ++y)
            {
                for (int x = 0; x < outWidth; ++x)
                {
                    // the pixel of the stored image that is displayed at (x, y)
                    int sx, sy;
                    switch (origin)
                    {
                        case SKEncodedOrigin.TopRight: sx = width - 1 - x; sy = y; break;
                        case SKEncodedOrigin.BottomRight: sx = width - 1 - x; sy = height - 1 - y; break;
                        case SKEncodedOrigin.BottomLeft: sx = x; sy = height - 1 - y; break;
                        case SKEncodedOrigin.LeftTop: sx = y; sy = x; break;
                        case SKEncodedOrigin.RightTop: sx = y; sy = height - 1 - x; break;
                        case SKEncodedOrigin.RightBottom: sx = width - 1 - y; sy = height - 1 - x; break;
                        case SKEncodedOrigin.LeftBottom: sx = width - 1 - y; sy = x; break;
                        default: sx = x; sy = y; break;
                    }

                    int i = sy * rowBytes + 4 * sx;
                    result[y, x, 0] = bgra[i];
                    result[y, x, 1] = bgra[i + 1];
                    result[y, x, 2] = bgra[i + 2];
                }
            }
            return result;
        }

        /// <summary>An uncompressed BMP: 8-bit with a gray palette for gray images, 24-bit otherwise (as OpenCV writes them).</summary>
        private static byte[] EncodeBmp(byte[,,] data, int width, int height)
        {
            int channels = data.GetLength(2);
            int bitsPerPixel = channels == 1 ? 8 : 24;
            int rowSize = (width * bitsPerPixel / 8 + 3) & ~3;
            int paletteSize = channels == 1 ? 256 * 4 : 0;
            int pixelOffset = 14 + 40 + paletteSize;
            int fileSize = pixelOffset + rowSize * height;

            var bytes = new byte[fileSize];
            using (var writer = new BinaryWriter(new MemoryStream(bytes)))
            {
                // file header
                writer.Write((byte)'B');
                writer.Write((byte)'M');
                writer.Write(fileSize);
                writer.Write(0);
                writer.Write(pixelOffset);

                // info header
                writer.Write(40);
                writer.Write(width);
                writer.Write(height);
                writer.Write((short)1);
                writer.Write((short)bitsPerPixel);
                writer.Write(0);
                writer.Write(rowSize * height);
                writer.Write(2835);
                writer.Write(2835);
                writer.Write(channels == 1 ? 256 : 0);
                writer.Write(0);

                if (channels == 1)
                {
                    for (int i = 0; i < 256; ++i)
                    {
                        writer.Write((byte)i);
                        writer.Write((byte)i);
                        writer.Write((byte)i);
                        writer.Write((byte)0);
                    }
                }

                // rows from bottom to top
                for (int y = height - 1; y >= 0; --y)
                {
                    long rowStart = writer.BaseStream.Position;
                    for (int x = 0; x < width; ++x)
                        for (int c = 0; c < channels; ++c)
                            writer.Write(data[y, x, c]);
                    writer.BaseStream.Position = rowStart + rowSize;
                }
            }
            return bytes;
        }
    }
}
