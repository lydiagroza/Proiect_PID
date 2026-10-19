using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Sections
{
    public class PointwiseOperations
    {
        #region Contrast and brightness
        public static byte[] ContrastAndBrightnessLUT(double alpha, double beta)
        {
            byte[] lut = new byte[256];

            for (int r = 0; r < 256; ++r)
            {
                double s = alpha * r + beta;
                lut[r] = (byte)Math.Clamp(Math.Round(s), 0, 255);
            }
            return lut;
        }

        public static Image<Gray, byte> ApplyLUT(Image<Gray, byte> inputImage, byte[] lut)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = lut[inputImage.Data[y, x, 0]];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> ApplyLUT(Image<Bgr, byte> inputImage, byte[] lut)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = lut[inputImage.Data[y, x, 0]];
                    result.Data[y, x, 1] = lut[inputImage.Data[y, x, 1]];
                    result.Data[y, x, 2] = lut[inputImage.Data[y, x, 2]];
                }
            }
            return result;
        }
        #endregion

        #region Gamma operator
        public static byte[] GammaLUT(double gamma)
        {
            byte[] lut = new byte[256];

            // c is chosen so that 255 stays 255: c * 255^gamma = 255  =>  c = 255^(1 - gamma)
            double c = Math.Pow(255, 1 - gamma);

            for (int r = 0; r < 256; ++r)
            {
                double s = c * Math.Pow(r, gamma);
                lut[r] = (byte)Math.Clamp(Math.Round(s), 0, 255);
            }
            return lut;
        }
        #endregion
    }
}
