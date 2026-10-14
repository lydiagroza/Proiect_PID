using System.ComponentModel;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Framework.Messaging;
using Framework.Model;
using Framework.Themes;
using Framework.ViewModel;

using static Framework.Utilities.DrawingHelper;
using static Framework.View.UiHelper;

namespace Framework.View
{
    public partial class MainWindow : Window
    {
        private readonly MainVM _mainVM;
        private readonly IMessageBus _messageBus;
        private readonly Dictionary<ScrollViewer, Vector> _synchronizedOffsets = new Dictionary<ScrollViewer, Vector>();

        public MainWindow()
        {
            InitializeComponent();

            _messageBus = MessageBus.Instance;
            _mainVM = (MainVM)DataContext;

            _messageBus.Subscribe<RefreshOverlaysMessage>(this, _ => Update());
            Closed += (s, e) => _messageBus.Unsubscribe<RefreshOverlaysMessage>(this);
            _mainVM.PropertyChanged += MainVMPropertyChanged;

            UpdateThemeMenu();
        }

        #region Selection markers
        public void Update()
        {
            double scaleValue = _mainVM.ScaleValue;

            RemoveUiElements(initialImageCanvas, processedImageCanvas);
            DrawUiElements(initialImageCanvas, processedImageCanvas, scaleValue, _mainVM);
        }
        #endregion

        #region Mouse input
        private void ImageMouseMove(object sender, PointerEventArgs e)
        {
            var canvasType = sender == initialImage ? ImageCanvasType.Initial : ImageCanvasType.Processed;
            _messageBus.Publish(new MouseMoveMessage(GetImagePosition(e, (Visual)sender), canvasType));
        }

        private void ImageMouseButtonDown(object sender, PointerPressedEventArgs e)
        {
            PointerPointProperties properties = e.GetCurrentPoint((Visual)sender).Properties;

            if (properties.IsLeftButtonPressed)
                _mainVM.SelectPoint(GetImagePosition(e, (Visual)sender));
            else if (properties.IsRightButtonPressed)
                _mainVM.ClearSelectedPoints();
        }

        /// <summary>The position of the mouse in image coordinates (pixels of the image, before zooming).</summary>
        private Point GetImagePosition(PointerEventArgs e, Visual image)
        {
            Point position = e.GetPosition(image);
            return new Point(position.X / _mainVM.ScaleValue, position.Y / _mainVM.ScaleValue);
        }
        #endregion

        #region Zoom and scrolling
        private void MainVMPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainVM.ScaleValue))
                return;

            double scaleValue = _mainVM.ScaleValue;

            UpdateShapesProperties(initialImageCanvas, scaleValue);
            UpdateShapesProperties(processedImageCanvas, scaleValue);
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var source = (ScrollViewer)sender;

            // Ignore the scrolling done below: when the images have different sizes, the other
            // scroll viewer cannot always reach the same offset and must not scroll this one back.
            if (_synchronizedOffsets.TryGetValue(source, out Vector offset) && offset == source.Offset)
                return;

            var target = source == initialImageCanvasScrollViewer
                ? processedImageCanvasScrollViewer
                : initialImageCanvasScrollViewer;

            target.Offset = source.Offset;
            _synchronizedOffsets[target] = target.Offset;
        }
        #endregion

        #region Theme
        private void UpdateThemeMenu()
        {
            string themeName = ThemeRegistry.GetName(_mainVM.Theme);
            foreach (MenuItem item in themeMenu.Items)
                item.IsChecked = item.Header.ToString() == themeName;
        }

        private void ThemeModeSelector(object sender, RoutedEventArgs e)
        {
            string selectedTheme = ((MenuItem)sender).Header.ToString();
            _mainVM.SetThemeMode(selectedTheme);
            UpdateThemeMenu();

            Properties.Settings.Default.Theme = selectedTheme;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}
