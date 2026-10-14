using System;
using System.Globalization;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Controls;
using Avalonia.Threading;
using OxyPlot;

namespace Framework.View.Controls
{
    /// <summary>
    /// Displays an OxyPlot <see cref="PlotModel"/>, like the PlotView of OxyPlot.Wpf.
    /// Left button: show the values under the mouse; right button: pan; wheel: zoom;
    /// middle button or Ctrl + right button: zoom to a rectangle; A or double click with the middle button: reset the axes.
    /// </summary>
    public class PlotView : Control, IPlotView
    {
        public static readonly StyledProperty<PlotModel> ModelProperty =
            AvaloniaProperty.Register<PlotView, PlotModel>(nameof(Model));

        public static readonly StyledProperty<IBrush> BackgroundProperty =
            AvaloniaProperty.Register<PlotView, IBrush>(nameof(Background), Brushes.White);

        private static readonly IBrush TrackerBackground = new ImmutableSolidColorBrush(Color.FromArgb(235, 255, 255, 225));
        private static readonly IPen TrackerBorder = new Pen(Brushes.Black, 1);
        private static readonly IPen TrackerLines = new Pen(new ImmutableSolidColorBrush(Color.FromArgb(160, 0, 0, 0)), 1, DashStyle.Dash);
        private static readonly IBrush ZoomBackground = new ImmutableSolidColorBrush(Color.FromArgb(48, 255, 255, 0));
        private static readonly IPen ZoomBorder = new Pen(Brushes.Black, 1, DashStyle.Dash);

        private readonly IController _controller = new PlotController();
        private PlotModel _attachedModel;
        private TrackerHitResult _tracker;
        private OxyRect? _zoomRectangle;

        static PlotView()
        {
            AffectsRender<PlotView>(ModelProperty, BackgroundProperty);
            FocusableProperty.OverrideDefaultValue<PlotView>(true);
            ClipToBoundsProperty.OverrideDefaultValue<PlotView>(true);
        }

        public PlotModel Model
        {
            get => GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        #region IPlotView
        PlotModel IPlotView.ActualModel => _attachedModel;

        OxyPlot.Model IView.ActualModel => _attachedModel;

        IController IView.ActualController => _controller;

        public OxyRect ClientArea => new OxyRect(0, 0, Bounds.Width, Bounds.Height);

        public void InvalidatePlot(bool updateData = true)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => InvalidatePlot(updateData));
                return;
            }

            PlotModel model = _attachedModel;
            if (model != null)
            {
                lock (model.SyncRoot)
                    ((IPlotModel)model).Update(updateData);
            }
            InvalidateVisual();
        }

        public void ShowTracker(TrackerHitResult trackerHitResult)
        {
            _tracker = trackerHitResult;
            InvalidateVisual();
        }

        public void HideTracker()
        {
            if (_tracker == null)
                return;

            _tracker = null;
            InvalidateVisual();
        }

        public void ShowZoomRectangle(OxyRect rectangle)
        {
            _zoomRectangle = rectangle;
            InvalidateVisual();
        }

        public void HideZoomRectangle()
        {
            _zoomRectangle = null;
            InvalidateVisual();
        }

        public void SetCursorType(CursorType cursorType)
        {
            StandardCursorType cursor = cursorType switch
            {
                CursorType.Pan => StandardCursorType.SizeAll,
                CursorType.ZoomRectangle => StandardCursorType.Cross,
                CursorType.ZoomHorizontal => StandardCursorType.SizeWestEast,
                CursorType.ZoomVertical => StandardCursorType.SizeNorthSouth,
                _ => StandardCursorType.Arrow
            };
            Cursor = new Cursor(cursor);
        }

        public void SetClipboardText(string text)
        {
            // Copying the plot as text (Ctrl+C) is not needed by the framework.
        }
        #endregion

        #region Model
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ModelProperty)
                AttachModel();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AttachModel();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            DetachModel();
        }

        private void AttachModel()
        {
            DetachModel();

            PlotModel model = Model;
            if (model == null || TopLevel.GetTopLevel(this) == null)
                return;

            // A plot model can be shown by one view at a time.
            ((IPlotModel)model).AttachPlotView(this);
            _attachedModel = model;
            InvalidatePlot(true);
        }

        private void DetachModel()
        {
            if (_attachedModel != null)
            {
                ((IPlotModel)_attachedModel).AttachPlotView(null);
                _attachedModel = null;
            }

            _tracker = null;
            _zoomRectangle = null;
        }
        #endregion

        #region Rendering
        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);
            if (Background != null)
                context.FillRectangle(Background, bounds);

            PlotModel model = _attachedModel;
            if (model == null || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            lock (model.SyncRoot)
                ((IPlotModel)model).Render(new PlotRenderContext(context), new OxyRect(0, 0, bounds.Width, bounds.Height));

            if (_zoomRectangle is OxyRect zoom)
                context.DrawRectangle(ZoomBackground, ZoomBorder, new Rect(zoom.Left, zoom.Top, zoom.Width, zoom.Height));

            if (_tracker != null)
                RenderTracker(context, _tracker, bounds);
        }

        private static void RenderTracker(DrawingContext context, TrackerHitResult tracker, Rect bounds)
        {
            var position = new Point(tracker.Position.X, tracker.Position.Y);
            OxyRect lines = tracker.LineExtents;
            if (lines.Width > 0 && lines.Height > 0)
            {
                context.DrawLine(TrackerLines, new Point(lines.Left, position.Y), new Point(lines.Right, position.Y));
                context.DrawLine(TrackerLines, new Point(position.X, lines.Top), new Point(position.X, lines.Bottom));
            }

            if (string.IsNullOrEmpty(tracker.Text))
                return;

            var text = new FormattedText(tracker.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface.Default, 12, Brushes.Black);
            double width = text.Width + 12, height = text.Height + 8;

            // above and to the right of the point, but inside the view
            double x = position.X + 10, y = position.Y - height - 10;
            if (x + width > bounds.Width)
                x = position.X - width - 10;
            if (y < 0)
                y = position.Y + 10;
            x = Math.Clamp(x, 0, Math.Max(0, bounds.Width - width));
            y = Math.Clamp(y, 0, Math.Max(0, bounds.Height - height));

            context.DrawRectangle(TrackerBackground, TrackerBorder, new Rect(x, y, width, height), 3, 3);
            context.DrawText(text, new Point(x + 6, y + 4));
        }
        #endregion

        #region Mouse and keyboard
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled || _attachedModel == null)
                return;

            Focus();
            e.Pointer.Capture(this);

            PointerPoint point = e.GetCurrentPoint(this);
            e.Handled = _controller.HandleMouseDown(this, new OxyMouseDownEventArgs
            {
                ChangedButton = ToMouseButton(point.Properties.PointerUpdateKind),
                ClickCount = e.ClickCount,
                Position = ToScreenPoint(point.Position),
                ModifierKeys = ToModifierKeys(e.KeyModifiers)
            });
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled || _attachedModel == null)
                return;

            e.Handled = _controller.HandleMouseMove(this, CreateMouseEventArgs(e));
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.Handled || _attachedModel == null)
                return;

            e.Handled = _controller.HandleMouseUp(this, CreateMouseEventArgs(e));
            e.Pointer.Capture(null);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e.Handled || _attachedModel == null)
                return;

            e.Handled = _controller.HandleMouseWheel(this, new OxyMouseWheelEventArgs
            {
                Delta = (int)(e.Delta.Y * 120),
                Position = ToScreenPoint(e.GetPosition(this)),
                ModifierKeys = ToModifierKeys(e.KeyModifiers)
            });
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            if (_attachedModel != null)
                _controller.HandleMouseEnter(this, CreateMouseEventArgs(e));
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_attachedModel != null)
                _controller.HandleMouseLeave(this, CreateMouseEventArgs(e));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled || _attachedModel == null)
                return;

            e.Handled = _controller.HandleKeyDown(this, new OxyKeyEventArgs
            {
                Key = ToKey(e.Key),
                ModifierKeys = ToModifierKeys(e.KeyModifiers)
            });
        }

        private OxyMouseEventArgs CreateMouseEventArgs(PointerEventArgs e)
        {
            return new OxyMouseEventArgs
            {
                Position = ToScreenPoint(e.GetPosition(this)),
                ModifierKeys = ToModifierKeys(e.KeyModifiers)
            };
        }

        private static ScreenPoint ToScreenPoint(Point point) => new ScreenPoint(point.X, point.Y);

        private static OxyMouseButton ToMouseButton(PointerUpdateKind kind)
        {
            switch (kind)
            {
                case PointerUpdateKind.LeftButtonPressed: return OxyMouseButton.Left;
                case PointerUpdateKind.MiddleButtonPressed: return OxyMouseButton.Middle;
                case PointerUpdateKind.RightButtonPressed: return OxyMouseButton.Right;
                case PointerUpdateKind.XButton1Pressed: return OxyMouseButton.XButton1;
                case PointerUpdateKind.XButton2Pressed: return OxyMouseButton.XButton2;
                default: return OxyMouseButton.None;
            }
        }

        private static OxyModifierKeys ToModifierKeys(KeyModifiers modifiers)
        {
            var keys = OxyModifierKeys.None;
            // Cmd on macOS works like Ctrl
            if (modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Meta))
                keys |= OxyModifierKeys.Control;
            if (modifiers.HasFlag(KeyModifiers.Alt))
                keys |= OxyModifierKeys.Alt;
            if (modifiers.HasFlag(KeyModifiers.Shift))
                keys |= OxyModifierKeys.Shift;
            return keys;
        }

        private static OxyKey ToKey(Key key)
        {
            switch (key)
            {
                case Key.Back: return OxyKey.Backspace;
                case Key.Enter: return OxyKey.Enter;
                case Key.PageUp: return OxyKey.PageUp;
                case Key.PageDown: return OxyKey.PageDown;
                case Key.OemPlus: return OxyKey.Add;
                case Key.OemMinus: return OxyKey.Subtract;
                default: return Enum.TryParse(key.ToString(), out OxyKey oxyKey) ? oxyKey : OxyKey.Unknown;
            }
        }
        #endregion
    }
}
