using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Sections
{
    public class Filters
    {
        #region Apply filter
        private static Image<Gray, byte> CalculateFilter(Image<Gray, byte> inputImage, double[,] mask)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);
            int h = mask.GetLength(0); 
            int w = mask.GetLength(1); 
            if (h % 2 == 0 || w % 2 == 0)
            {
                return inputImage.Clone(); 
            }
            int h_offset = h / 2;
            int w_offset = w / 2;
            for (int y = h_offset; y < inputImage.Height - h_offset; ++y)
            {
                for (int x = w_offset; x < inputImage.Width - w_offset; ++x)
                {
                    double sum = 0; 
                    for (int i = -h_offset; i <= h_offset; ++i)
                    {
                        for (int j = -w_offset; j <= w_offset; ++j)
                        {
                            sum += mask[i + h_offset, j + w_offset] * inputImage.Data[y + i, x + j, 0];
                        }
                    }

                    resultImage.Data[y, x, 0] = (byte)Math.Round(Math.Min(255, Math.Max(sum, 0)), MidpointRounding.AwayFromZero);
                }
            }

            return resultImage;
        }

        public static Image<Gray, byte> ApplyFilter(Image<Gray, byte> inputImage, double[,] mask)
        {
            int h = mask.GetLength(0);
            int w = mask.GetLength(1);
            int h_offset = mask.GetLength(0) / 2;
            int w_offset = mask.GetLength(1) / 2;
            Image<Gray, byte> borderedImage = new Image<Gray, byte>(inputImage.Width + w - 1, inputImage.Height + h - 1); // obiect alocat imaginii bordate
            CvInvoke.CopyMakeBorder(inputImage, borderedImage, h_offset, h_offset, w_offset, w_offset, Emgu.CV.CvEnum.BorderType.Reflect101);
            Image<Gray, byte> resultBorderedImage = CalculateFilter(borderedImage, mask);
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);
            for (int y = 0; y < resultImage.Height; ++y)
            {
                for (int x = 0; x < resultImage.Width; ++x)
                {
                    resultImage.Data[y, x, 0] = resultBorderedImage.Data[y + h_offset, x + w_offset, 0];
                }
            }
            return resultImage;
        }
        #endregion

        #region Gaussian filter
        public static double[] GaussMask(double sigma)
        {
            int l = (int)Math.Ceiling(4 * sigma);
            if (l % 2 == 0)
                ++l;
            int r = l / 2;

            double[] mask = new double[l];
            double sum = 0;
            for (int z = -r; z <= r; ++z)
            {
                mask[z + r] = 1.0 / (Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-(z * z) / (2 * sigma * sigma));
                sum += mask[z + r];
            }

            for (int i = 0; i < l; ++i)
                mask[i] /= sum;

            return mask;
        }

        public static Image<Gray, byte> GaussFiltering(Image<Gray, byte> inputImage, double sigmaX, double sigmaY)
        {
            double[] maskX = GaussMask(sigmaX);
            double[,] horizontalMask = new double[1, maskX.Length];
            for (int j = 0; j < maskX.Length; ++j)
                horizontalMask[0, j] = maskX[j];

            double[] maskY = GaussMask(sigmaY);
            double[,] verticalMask = new double[maskY.Length, 1];
            for (int i = 0; i < maskY.Length; ++i)
                verticalMask[i, 0] = maskY[i];

            Image<Gray, byte> horizontalResult = ApplyFilter(inputImage, horizontalMask);
            return ApplyFilter(horizontalResult, verticalMask);
        }

        public static Image<Bgr, byte> GaussFiltering(Image<Bgr, byte> inputImage, double sigmaX, double sigmaY)
        {
            Image<Bgr, byte> resultImage = new Image<Bgr, byte>(inputImage.Size);

            for (int c = 0; c < 3; ++c)
            {
                Image<Gray, byte> channel = new Image<Gray, byte>(inputImage.Size);
                for (int y = 0; y < inputImage.Height; ++y)
                    for (int x = 0; x < inputImage.Width; ++x)
                        channel.Data[y, x, 0] = inputImage.Data[y, x, c];

                Image<Gray, byte> filtered = GaussFiltering(channel, sigmaX, sigmaY);

                for (int y = 0; y < inputImage.Height; ++y)
                    for (int x = 0; x < inputImage.Width; ++x)
                        resultImage.Data[y, x, c] = filtered.Data[y, x, 0];
            }

            return resultImage;
        }
        #endregion

        #region Median filter
        public static int Median(int[] H, int l)
        {
            int k = 0, s = 0;
            int n = l * l / 2;
            while (k <= 255 && s + H[k] <= n)
            {
                s += H[k];
                ++k;
            }
            return k;
        }

        public static Image<Gray, byte> MedianFiltering(Image<Gray, byte> inputImage, int l)
        {
            Image<Gray, byte> resultImage = new Image<Gray, byte>(inputImage.Size);
            if (l % 2 == 0)
                ++l;

            int[] H = new int[256];
            int r = l / 2;

            // mirrored border without duplicating the first/last row/column
            Image<Gray, byte> B = new Image<Gray, byte>(inputImage.Width + 2 * r, inputImage.Height + 2 * r);
            CvInvoke.CopyMakeBorder(inputImage, B, r, r, r, r, Emgu.CV.CvEnum.BorderType.Reflect101);

            for (int y = r; y <= B.Height - r - 1; ++y)
            {
                Array.Clear(H, 0, H.Length);
                int x = r;
                for (int i = -r; i <= r; ++i)
                    for (int j = -r; j <= r; ++j)
                        ++H[B.Data[y + i, x + j, 0]];

                resultImage.Data[y - r, x - r, 0] = (byte)Median(H, l);

                for (x = r + 1; x <= B.Width - r - 1; ++x)
                {
                    for (int i = -r; i <= r; ++i)
                    {
                        --H[B.Data[y + i, x - r - 1, 0]];
                        ++H[B.Data[y + i, x + r, 0]];
                    }
                    resultImage.Data[y - r, x - r, 0] = (byte)Median(H, l);
                }
            }

            return resultImage;
        }
        #endregion
    }
}
