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

                    resultImage.Data[y, x, 0] = (byte)Math.Round(Math.Min(255, Math.Max(sum, 0)));
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
    }
}
