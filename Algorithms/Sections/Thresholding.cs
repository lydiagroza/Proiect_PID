using Emgu.CV;
using Emgu.CV.Structure;
using Algorithms.Utilities;

namespace Algorithms.Sections
{
    public class Thresholding
    {
        #region Binarization
        public static Image<Gray, byte> Binarize(Image<Gray, byte> inputImage, int threshold)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = (byte)(inputImage.Data[y, x, 0] > threshold ? 255 : 0);
                }
            }
            return result;
        }
        #endregion

        #region Otsu threshold
        public static int OtsuThreshold(Image<Gray, byte> inputImage)
        {
            double[] p = Utils.ComputeNormalizedHistogram(inputImage);

            int threshold = 0;
            double maxInterVariance = 0;

            for (int t = 0; t < 255; ++t)
            {
                double p1 = 0, sum1 = 0, sum2 = 0;
                for (int k = 0; k <= t; ++k)
                {
                    p1 += p[k];
                    sum1 += k * p[k];
                }
                for (int k = t + 1; k < 256; ++k)
                {
                    sum2 += k * p[k];
                }
                double p2 = 1 - p1;

                // An empty class has no mean, so this t cannot separate anything
                if (p1 <= 0 || p2 <= 0)
                    continue;

                double mu1 = sum1 / p1;
                double mu2 = sum2 / p2;

                double interVariance = p1 * p2 * (mu1 - mu2) * (mu1 - mu2);
                if (interVariance > maxInterVariance)
                {
                    maxInterVariance = interVariance;
                    threshold = t;
                }
            }
            return threshold;
        }
        #endregion
    }
}
