using Avalonia;
using Avalonia.Controls;

using Framework.Model;
using Framework.ViewModel;

namespace Framework.View
{
    public partial class HistogramWindow : ChildWindow
    {
        private readonly HistogramVM _histogramVM;

        public HistogramWindow(MainVM mainVM, ImageCanvasType canvas)
            : this(mainVM)
        {
            ShowHistogram(canvas);
        }

        public HistogramWindow(MainVM mainVM, string title, dynamic values)
            : this(mainVM)
        {
            _histogramVM.Title = title;

            if (values != null)
                _histogramVM.CreateHistogram(values);
        }

        private HistogramWindow(MainVM mainVM)
            : this()
        {
            _histogramVM = new HistogramVM();
            _histogramVM.Theme = mainVM.Theme;

            DataContext = _histogramVM;
            SizeChanged += WindowSizeChanged;
        }

        // For the XAML previewer.
        public HistogramWindow()
        {
            InitializeComponent();
        }

        public void ShowHistogram(ImageCanvasType canvas)
        {
            _histogramVM.CreateHistogram(canvas);
        }

        /// <summary>Keeps the window centered when it gets wider or narrower (gray and color histograms).</summary>
        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width == 0 || e.PreviousSize == e.NewSize)
                return;

            double scaling = DesktopScaling;
            Position = new PixelPoint(
                Position.X + (int)((e.PreviousSize.Width - e.NewSize.Width) / 2 * scaling),
                Position.Y + (int)((e.PreviousSize.Height - e.NewSize.Height) / 2 * scaling));
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _histogramVM?.Cleanup();
        }
    }
}
