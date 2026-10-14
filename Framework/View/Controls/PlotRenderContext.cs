using System;
using System.Globalization;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using OxyPlot;

using HorizontalAlignment = OxyPlot.HorizontalAlignment;
using VerticalAlignment = OxyPlot.VerticalAlignment;

namespace Framework.View.Controls
{
    /// <summary>
    /// Draws the primitives of an OxyPlot plot (lines, polygons, rectangles, ellipses, text) with Avalonia.
    /// </summary>
    internal sealed class PlotRenderContext : RenderContextBase
    {
        private readonly DrawingContext _context;
        private readonly Stack<DrawingContext.PushedState> _clips = new Stack<DrawingContext.PushedState>();

        public PlotRenderContext(DrawingContext context)
        {
            _context = context;
            RendersToScreen = true;
        }

        public override void DrawLine(IList<ScreenPoint> points, OxyColor stroke, double thickness,
            EdgeRenderingMode edgeRenderingMode, double[] dashArray, LineJoin lineJoin)
        {
            if (points.Count < 2 || !stroke.IsVisible() || thickness <= 0)
                return;

            _context.DrawGeometry(null, CreatePen(stroke, thickness, dashArray, lineJoin), CreateGeometry(points, false));
        }

        public override void DrawPolygon(IList<ScreenPoint> points, OxyColor fill, OxyColor stroke, double thickness,
            EdgeRenderingMode edgeRenderingMode, double[] dashArray, LineJoin lineJoin)
        {
            if (points.Count < 2)
                return;

            IBrush brush = CreateBrush(fill);
            IPen pen = stroke.IsVisible() && thickness > 0 ? CreatePen(stroke, thickness, dashArray, lineJoin) : null;
            if (brush == null && pen == null)
                return;

            _context.DrawGeometry(brush, pen, CreateGeometry(points, true));
        }

        public override void DrawRectangle(OxyRect rect, OxyColor fill, OxyColor stroke, double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            IBrush brush = CreateBrush(fill);
            IPen pen = stroke.IsVisible() && thickness > 0 ? CreatePen(stroke, thickness, null, LineJoin.Miter) : null;
            if (brush == null && pen == null)
                return;

            _context.DrawRectangle(brush, pen, ToRect(rect));
        }

        public override void DrawEllipse(OxyRect rect, OxyColor fill, OxyColor stroke, double thickness,
            EdgeRenderingMode edgeRenderingMode)
        {
            IBrush brush = CreateBrush(fill);
            IPen pen = stroke.IsVisible() && thickness > 0 ? CreatePen(stroke, thickness, null, LineJoin.Miter) : null;
            if (brush == null && pen == null)
                return;

            _context.DrawEllipse(brush, pen, new Point(rect.Center.X, rect.Center.Y), rect.Width / 2, rect.Height / 2);
        }

        public override void DrawText(ScreenPoint p, string text, OxyColor fill, string fontFamily, double fontSize,
            double fontWeight, double rotate, HorizontalAlignment halign, VerticalAlignment valign, OxySize? maxSize)
        {
            if (string.IsNullOrEmpty(text) || !fill.IsVisible())
                return;

            FormattedText formatted = CreateText(text, fontSize, fontWeight, CreateBrush(fill));
            if (maxSize != null)
            {
                formatted.MaxTextWidth = Math.Max(0, maxSize.Value.Width);
                formatted.MaxTextHeight = Math.Max(0, maxSize.Value.Height);
            }

            double dx = halign == HorizontalAlignment.Center ? -formatted.Width / 2
                      : halign == HorizontalAlignment.Right ? -formatted.Width : 0;
            double dy = valign == VerticalAlignment.Middle ? -formatted.Height / 2
                      : valign == VerticalAlignment.Bottom ? -formatted.Height : 0;

            Matrix transform = Matrix.CreateTranslation(dx, dy)
                             * Matrix.CreateRotation(rotate * Math.PI / 180)
                             * Matrix.CreateTranslation(p.X, p.Y);

            using (_context.PushTransform(transform))
                _context.DrawText(formatted, new Point(0, 0));
        }

        public override OxySize MeasureText(string text, string fontFamily, double fontSize, double fontWeight)
        {
            if (string.IsNullOrEmpty(text))
                return OxySize.Empty;

            FormattedText formatted = CreateText(text, fontSize, fontWeight, Brushes.Black);
            return new OxySize(formatted.Width, formatted.Height);
        }

        public override int ClipCount => _clips.Count;

        public override void PushClip(OxyRect clippingRectangle)
        {
            _clips.Push(_context.PushClip(ToRect(clippingRectangle)));
        }

        public override void PopClip()
        {
            _clips.Pop().Dispose();
        }

        #region Helpers
        private static FormattedText CreateText(string text, double fontSize, double fontWeight, IBrush brush)
        {
            var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, (FontWeight)(int)Math.Clamp(fontWeight, 1, 999));
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, brush);
        }

        private static IBrush CreateBrush(OxyColor color)
        {
            return color.IsVisible() ? new ImmutableSolidColorBrush(ToColor(color)) : null;
        }

        private static IPen CreatePen(OxyColor color, double thickness, double[] dashArray, LineJoin lineJoin)
        {
            var dashStyle = dashArray != null && dashArray.Length > 0 ? new DashStyle(dashArray, 0) : null;
            var join = lineJoin == LineJoin.Round ? PenLineJoin.Round : lineJoin == LineJoin.Bevel ? PenLineJoin.Bevel : PenLineJoin.Miter;
            return new Pen(new ImmutableSolidColorBrush(ToColor(color)), thickness, dashStyle, PenLineCap.Flat, join);
        }

        private static StreamGeometry CreateGeometry(IList<ScreenPoint> points, bool isClosed)
        {
            var geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(ToPoint(points[0]), isClosed);
                for (int i = 1; i < points.Count; ++i)
                    context.LineTo(ToPoint(points[i]));
                context.EndFigure(isClosed);
            }
            return geometry;
        }

        private static Color ToColor(OxyColor color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        private static Point ToPoint(ScreenPoint point) => new Point(point.X, point.Y);

        private static Rect ToRect(OxyRect rect) => new Rect(rect.Left, rect.Top, Math.Max(0, rect.Width), Math.Max(0, rect.Height));
        #endregion
    }
}
