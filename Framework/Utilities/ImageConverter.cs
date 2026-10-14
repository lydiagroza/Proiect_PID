using Emgu.CV;

using System;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using System.Runtime.InteropServices;

namespace Framework.Utilities
{
    static class ImageConverter
    {
        /// <summary>
        /// Converts a gray (1 channel), color (3 channels, BGR) or color with alpha (4 channels, BGRA) 8-bit image
        /// into a bitmap that can be displayed. Returns null for an empty image.
        /// </summary>
        public static WriteableBitmap Convert<TColor>(Image<TColor, byte> image)
            where TColor : struct, IColor
        {
            int width = image.Width, height = image.Height;
            if (width == 0 || height == 0)
                return null;

            int channels = image.NumberOfChannels;
            byte[,,] data = image.Data;
            // Emgu CV pads the rows of some images, so a row of Data can be longer than Width.
            int rowLength = data.GetLength(1) * channels;
            Span<byte> pixels = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference(data), data.Length);

            var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            using (ILockedFramebuffer buffer = bitmap.Lock())
            {
                var row = new byte[width * 4];
                for (int y = 0; y < height; ++y)
                {
                    Span<byte> source = pixels.Slice(y * rowLength, width * channels);
                    for (int x = 0, s = 0, d = 0; x < width; ++x, s += channels, d += 4)
                    {
                        if (channels == 1)
                        {
                            row[d] = row[d + 1] = row[d + 2] = source[s];
                        }
                        else
                        {
                            row[d] = source[s];
                            row[d + 1] = source[s + 1];
                            row[d + 2] = source[s + 2];
                        }
                        row[d + 3] = 255;
                    }
                    Marshal.Copy(row, 0, buffer.Address + y * buffer.RowBytes, row.Length);
                }
            }
            return bitmap;
        }
    }
}
