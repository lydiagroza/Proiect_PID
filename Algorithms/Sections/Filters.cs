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
            Image<Gray, byte> borderedImage = new Image<Gray, byte>(inputImage.Width + w - 1, inputImage.Height + h - 1); 
            
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

        #region Average filter 
        public static long[,] IntegralImage(Image<Gray, byte> image)
        {
            long[,] I = new long[image.Height, image.Width];
            for(int y = 0; y < image.Height; ++y)
            {
                for(int x = 0; x < image.Width; ++x)
                {
                    long f = image.Data[y, x, 0];
                    if(x == 0 && y == 0)
                        I[y,x] = f;
                    else if (x == 0)
                        I[y,x] = I[y - 1, x] + f;
                    else if (y == 0)
                        I[y,x] = I[y, x - 1] + f;
                    else 
                        I[y,x] = I[y - 1, x] + I[y, x - 1] - I[y - 1, x - 1] + f; 
                }
            }
            return I;
        }

        public static long Sum(long[,] I, int x0, int y0, int x1, int y1)
          {
              long A = I[y1, x1];
              if (x0 == 0 && y0 == 0)
                  return A;
              if (y0 == 0)
                  return A - I[y1, x0 - 1];
              if (x0 == 0)
                  return A - I[y0 - 1, x1];

              long B = I[y1, x0 - 1];
              long C = I[y0 - 1, x0 - 1];
              long D = I[y0 - 1, x1];
              return A + C - B - D;
          }

          public static Image<Gray, byte> AverageFilter(Image<Gray, byte> inputImage, int h, int w)
          {
              if (h % 2 == 0) ++h;
              if (w % 2 == 0) ++w;

              Image<Gray, byte> borderedImage = new Image<Gray, byte>(inputImage.Width + w - 1, inputImage.Height + h - 1);
              CvInvoke.CopyMakeBorder(inputImage, borderedImage, h / 2, h / 2, w / 2, w / 2, Emgu.CV.CvEnum.BorderType.Reflect101);

              long[,] I = IntegralImage(borderedImage);
              Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

              for (int y = h / 2; y <= borderedImage.Height - h / 2 - 1; ++y)
              {
                  for (int x = w / 2; x <= borderedImage.Width - w / 2 - 1; ++x)
                  {
                      long sum = Sum(I, x - w / 2, y - h / 2, x + w / 2, y + h / 2);
                      double mean = (double)sum / (h * w);
                      result.Data[y - h / 2, x - w / 2, 0] = (byte)Math.Round(mean);
                  }
              }
              return result;
          }

          public static Image<Bgr, byte> AverageFilter(Image<Bgr, byte> inputImage, int h, int w)
          {
              Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

              for (int c = 0; c < 3; ++c)
              {
                  result[c] = AverageFilter(inputImage[c], h, w);
              }
              return result;
          }
          #endregion
    }
}
