using Emgu.CV;
using Emgu.CV.Structure;
using Framework.Messaging;
using Framework.Model;
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Framework.ViewModel
{
    public class MagnifierVM : ThemedVM
    {
        private static readonly IBrush OutsideBrush = Brushes.White;
        private static readonly IBrush GridBrush = new ImmutableSolidColorBrush(Color.FromArgb(70, 128, 128, 128));

        private readonly IImageStore _imageStore;
        private Point _lastPoint;

        public MagnifierVM()
            : this(MessageBus.Instance, ImageStore.Instance)
        {
        }

        public MagnifierVM(IMessageBus messageBus, IImageStore imageStore)
            : base(messageBus)
        {
            _imageStore = imageStore;
        }

        #region Rendering
        public void RefreshMagnifierImages()
        {
            _lastPoint = _imageStore.LastMouseClick;

            if (_imageStore.GrayInitialImage != null)
                InitialMagnifierCells = RenderGrid(_imageStore.GrayInitialImage);
            else if (_imageStore.ColorInitialImage != null)
                InitialMagnifierCells = RenderGrid(_imageStore.ColorInitialImage);
            else
                InitialMagnifierCells = null;

            if (_imageStore.GrayProcessedImage != null)
                ProcessedMagnifierCells = RenderGrid(_imageStore.GrayProcessedImage);
            else if (_imageStore.ColorProcessedImage != null)
                ProcessedMagnifierCells = RenderGrid(_imageStore.ColorProcessedImage);
            else
                ProcessedMagnifierCells = null;
        }

        private IReadOnlyList<MagnifierCell> RenderGrid(Image<Bgr, byte> image)
        {
            return RenderGrid(image.Width, image.Height, (x, y) =>
            {
                int b = image.Data[y, x, 0];
                int g = image.Data[y, x, 1];
                int r = image.Data[y, x, 2];
                return (Color.FromRgb((byte)r, (byte)g, (byte)b), r + "\n" + g + "\n" + b, b <= 128 && g <= 128 && r <= 128);
            });
        }

        private IReadOnlyList<MagnifierCell> RenderGrid(Image<Gray, byte> image)
        {
            return RenderGrid(image.Width, image.Height, (x, y) =>
            {
                byte gray = image.Data[y, x, 0];
                return (Color.FromRgb(gray, gray, gray), gray.ToString(), gray <= 128);
            });
        }

        /// <summary>The 9 x 9 pixels around the selected point, row by row.</summary>
        private IReadOnlyList<MagnifierCell> RenderGrid(int imageWidth, int imageHeight,
            Func<int, int, (Color Fill, string Text, bool IsDark)> describePixel)
        {
            var cells = new List<MagnifierCell>(81);

            for (int dy = -4; dy <= 4; ++dy)
            {
                for (int dx = -4; dx <= 4; ++dx)
                {
                    int x = (int)_lastPoint.X + dx;
                    int y = (int)_lastPoint.Y + dy;
                    bool isSelected = dx == 0 && dy == 0;

                    if (x >= 0 && y >= 0 && x < imageWidth && y < imageHeight)
                    {
                        var pixel = describePixel(x, y);
                        cells.Add(new MagnifierCell(
                            new ImmutableSolidColorBrush(pixel.Fill),
                            pixel.Text,
                            pixel.IsDark ? Brushes.White : Brushes.Black,
                            isSelected));
                    }
                    else
                    {
                        cells.Add(new MagnifierCell(OutsideBrush, string.Empty, Brushes.Black, isSelected));
                    }
                }
            }

            return cells;
        }
        #endregion

        #region Properties
        private IReadOnlyList<MagnifierCell> _initialMagnifierCells;
        public IReadOnlyList<MagnifierCell> InitialMagnifierCells
        {
            get => _initialMagnifierCells;
            set { _initialMagnifierCells = value; NotifyPropertyChanged(nameof(InitialMagnifierCells)); }
        }

        private IReadOnlyList<MagnifierCell> _processedMagnifierCells;
        public IReadOnlyList<MagnifierCell> ProcessedMagnifierCells
        {
            get => _processedMagnifierCells;
            set { _processedMagnifierCells = value; NotifyPropertyChanged(nameof(ProcessedMagnifierCells)); }
        }
        #endregion

        /// <summary>One magnified pixel: its color and its value(s).</summary>
        public class MagnifierCell
        {
            public MagnifierCell(IBrush fill, string text, IBrush textBrush, bool isSelected)
            {
                Fill = fill;
                Text = text;
                TextBrush = textBrush;
                BorderBrush = isSelected ? Brushes.Red : GridBrush;
                BorderThickness = new Thickness(isSelected ? 2 : 0.5);
            }

            public IBrush Fill { get; }
            public string Text { get; }
            public IBrush TextBrush { get; }
            public IBrush BorderBrush { get; }
            public Thickness BorderThickness { get; }
        }
    }
}
