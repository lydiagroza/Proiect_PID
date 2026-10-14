using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Framework.Utilities
{
    public static class DrawingHelper
    {
        #region Drawn shapes
        public static Collection<Line> VectorOfLines { get; } = new Collection<Line>();
        public static Collection<Rectangle> VectorOfRectangles { get; } = new Collection<Rectangle>();
        public static Collection<Ellipse> VectorOfEllipses { get; } = new Collection<Ellipse>();
        public static Collection<Polygon> VectorOfPolygons { get; } = new Collection<Polygon>();

        private static Dictionary<Shape, (double, double)> ShapesProperties { get; } =
            new Dictionary<Shape, (double, double)>();
        #endregion

        #region Drawing
        public static Line DrawLine(
            Canvas canvas, Point start, Point end, int thickness, IBrush color, double scaleValue)
        {
            Line line = new Line
            {
                StartPoint = start,
                EndPoint = end,
                StrokeThickness = thickness,
                Stroke = color
            };

            VectorOfLines.Add(line);
            AddShapeProperties(canvas, line, 0, 0, scaleValue);

            return line;
        }

        public static Rectangle DrawRectangle(
            Canvas canvas, Point leftTop, Point rightBottom, int thickness, IBrush color, double scaleValue)
        {
            Rectangle rectangle = new Rectangle
            {
                Stroke = color,
                StrokeThickness = thickness,
                Width = Math.Abs(rightBottom.X - leftTop.X),
                Height = Math.Abs(rightBottom.Y - leftTop.Y)
            };

            VectorOfRectangles.Add(rectangle);
            AddShapeProperties(canvas, rectangle, leftTop.X, leftTop.Y, scaleValue);

            return rectangle;
        }

        public static Ellipse DrawEllipse(
            Canvas canvas, Point center, double width, double height, int thickness, IBrush color, double scaleValue)
        {
            Ellipse ellipse = new Ellipse
            {
                Stroke = color,
                StrokeThickness = thickness,
                Width = width,
                Height = height
            };

            VectorOfEllipses.Add(ellipse);
            AddShapeProperties(canvas, ellipse, center.X - width / 2.0, center.Y - height / 2.0, scaleValue);

            return ellipse;
        }

        public static Polygon DrawPolygon(
            Canvas canvas, IEnumerable<Point> vectorOfPoints, int thickness, IBrush color, double scaleValue)
        {
            Polygon polygon = new Polygon
            {
                Stroke = color,
                StrokeThickness = thickness,
                Points = vectorOfPoints.ToList()
            };

            VectorOfPolygons.Add(polygon);
            AddShapeProperties(canvas, polygon, 0, 0, scaleValue);

            return polygon;
        }
        #endregion

        #region Zoom
        public static void UpdateShapesProperties(
            Canvas canvas, double scaleValue)
        {
            if (canvas != null)
            {
                foreach (Shape shape in ShapesProperties.Keys)
                {
                    if (shape.Parent != canvas)
                        continue;

                    Canvas.SetLeft(shape, ShapesProperties[shape].Item1 * scaleValue);
                    Canvas.SetTop(shape, ShapesProperties[shape].Item2 * scaleValue);

                    shape.RenderTransform = new ScaleTransform(scaleValue, scaleValue);
                }
            }
        }
        #endregion

        #region Removing
        public static bool RemoveUiElement(Canvas canvas, Control element)
        {
            if (element != null)
            {
                canvas.Children.Remove(element);
                if (element is Shape shape)
                    ShapesProperties.Remove(shape);

                if (element is Line line)
                    return VectorOfLines.Remove(line);
                else if (element is Rectangle rectangle)
                    return VectorOfRectangles.Remove(rectangle);
                else if (element is Ellipse ellipse)
                    return VectorOfEllipses.Remove(ellipse);
                else if (element is Polygon polygon)
                    return VectorOfPolygons.Remove(polygon);
            }

            return false;
        }

        public static void RemoveUiElements<TElement>(Canvas canvas)
            where TElement : Control
        {
            foreach (var item in canvas.Children.OfType<TElement>().ToList())
                RemoveUiElement(canvas, item);
        }

        public static void RemoveUiElements(Canvas canvas)
        {
            RemoveUiElements<Line>(canvas);
            RemoveUiElements<Rectangle>(canvas);
            RemoveUiElements<Ellipse>(canvas);
            RemoveUiElements<Polygon>(canvas);
        }
        #endregion

        #region Helpers
        private static void AddShapeProperties(
            Canvas canvas, Shape shape, double leftProperty, double topProperty, double scaleValue)
        {
            canvas.Children.Add(shape);
            ShapesProperties.Add(shape, (leftProperty, topProperty));

            Canvas.SetLeft(shape, leftProperty * scaleValue);
            Canvas.SetTop(shape, topProperty * scaleValue);

            // The shapes are drawn in image coordinates and scaled with the zoom, like the image.
            // They let the mouse events through to the image below them.
            shape.IsHitTestVisible = false;
            shape.RenderTransformOrigin = RelativePoint.TopLeft;
            shape.RenderTransform = new ScaleTransform(scaleValue, scaleValue);
        }
        #endregion
    }
}
