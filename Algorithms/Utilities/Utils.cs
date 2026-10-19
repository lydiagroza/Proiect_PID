using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Utilities
{
    public class Utils
    {
        #region Compute histogram
        public static int[] ComputeHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    ++histogram[inputImage.Data[y, x, 0]];
                }
            }

            return histogram;
        }
        #endregion

        #region Compute normalized histogram
        public static double[] ComputeNormalizedHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = ComputeHistogram(inputImage);
            double numberOfPixels = inputImage.Width * inputImage.Height;

            double[] normalizedHistogram = new double[256];
            for (int i = 0; i < 256; ++i)
            {
                normalizedHistogram[i] = histogram[i] / numberOfPixels;
            }

            return normalizedHistogram;
        }
        #endregion
    }
}