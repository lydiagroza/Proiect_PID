using Emgu.CV;
using Emgu.CV.Structure;
using System;
using Avalonia;

namespace Framework.Model
{
    public interface IImageStore
    {
        Image<Gray, byte> GrayInitialImage { get; set; }
        Image<Gray, byte> GrayProcessedImage { get; set; }
        Image<Bgr, byte> ColorInitialImage { get; set; }
        Image<Bgr, byte> ColorProcessedImage { get; set; }

        bool HasInitialImage { get; }
        bool HasProcessedImage { get; }
        CvArray<byte> GetImage(ImageCanvasType canvas);

        Point LastMouseClick { get; set; }
        PointCollection MouseClickCollection { get; }

        void ClearInitial();
        void ClearProcessed();

        event EventHandler<ImageCanvasType> ImageChanged;
    }
}
