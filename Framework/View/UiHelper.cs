using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;

using Framework.Model;
using Framework.ViewModel;
using static Framework.Utilities.DrawingHelper;

namespace Framework.View
{
    static class UiHelper
    {
        #region Selection markers
        private static Rectangle _initialSquare;
        private static Rectangle _processedSquare;

        private static Line _initialRowLine;
        private static Line _processedRowLine;

        private static Line _initialColumnLine;
        private static Line _processedColumnLine;

        public static void DrawUiElements(Canvas initialCanvas, Canvas processedCanvas, double scaleValue, MainVM mainVM)
        {
            Size? initialSize = mainVM.GetImageSize(ImageCanvasType.Initial);
            Size? processedSize = mainVM.GetImageSize(ImageCanvasType.Processed);
            Point point = mainVM.SelectedPoint;

            if (mainVM.IsSelectedAreaMarked)
            {
                if (initialSize != null)
                    _initialSquare = GetSquare(initialCanvas, point, scaleValue);
                if (processedSize != null)
                    _processedSquare = GetSquare(processedCanvas, point, scaleValue);
            }

            if (mainVM.IsSelectedRowMarked)
            {
                if (initialSize != null)
                    _initialRowLine = GetRowLine(initialCanvas, initialSize.Value, point, scaleValue);
                if (processedSize != null)
                    _processedRowLine = GetRowLine(processedCanvas, processedSize.Value, point, scaleValue);
            }

            if (mainVM.IsSelectedColumnMarked)
            {
                if (initialSize != null)
                    _initialColumnLine = GetColumnLine(initialCanvas, initialSize.Value, point, scaleValue);
                if (processedSize != null)
                    _processedColumnLine = GetColumnLine(processedCanvas, processedSize.Value, point, scaleValue);
            }
        }

        public static void RemoveUiElements(Canvas initialCanvas, Canvas processedCanvas)
        {
            RemoveUiElement(initialCanvas, _initialSquare);
            RemoveUiElement(processedCanvas, _processedSquare);

            RemoveUiElement(initialCanvas, _initialRowLine);
            RemoveUiElement(processedCanvas, _processedRowLine);

            RemoveUiElement(initialCanvas, _initialColumnLine);
            RemoveUiElement(processedCanvas, _processedColumnLine);
        }
        #endregion

        #region Windows
        private static IClassicDesktopStyleApplicationLifetime Desktop =>
            Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        public static Window MainWindow => Desktop?.MainWindow;

        public static IReadOnlyList<Window> Windows => Desktop?.Windows ?? Array.Empty<Window>();

        /// <summary>The window the user is working in, otherwise the main window.</summary>
        public static Window ActiveWindow => Windows.FirstOrDefault(window => window.IsActive) ?? MainWindow;

        /// <summary>Opens the file dialogs of the operating system.</summary>
        public static IStorageProvider StorageProvider => ActiveWindow?.StorageProvider;

        public static void BringToFront(Window window)
        {
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            window.Activate();
        }

        public static void Shutdown()
        {
            Desktop?.Shutdown();
        }
        #endregion

        #region Helpers
        private static Rectangle GetSquare(Canvas canvas, Point point, double scaleValue)
        {
            var leftTop = new Point(point.X - 5, point.Y - 5);
            var rightBottom = new Point(point.X + 5, point.Y + 5);

            return DrawRectangle(canvas, leftTop, rightBottom, 1, Brushes.Red, scaleValue);
        }

        private static Line GetRowLine(Canvas canvas, Size imageSize, Point point, double scaleValue)
        {
            var start = new Point(0, point.Y);
            var end = new Point(imageSize.Width, point.Y);

            return DrawLine(canvas, start, end, 1, Brushes.Red, scaleValue);
        }

        private static Line GetColumnLine(Canvas canvas, Size imageSize, Point point, double scaleValue)
        {
            var start = new Point(point.X, 0);
            var end = new Point(point.X, imageSize.Height);

            return DrawLine(canvas, start, end, 1, Brushes.Red, scaleValue);
        }
        #endregion
    }
}
