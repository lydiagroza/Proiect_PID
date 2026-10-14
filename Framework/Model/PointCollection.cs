using System.Collections.Generic;
using Avalonia;

namespace Framework.Model
{
    /// <summary>
    /// A list of points, used like WPF's PointCollection (for example for the selected points or a graph).
    /// </summary>
    public class PointCollection : List<Point>
    {
        public PointCollection()
        {
        }

        public PointCollection(IEnumerable<Point> points)
            : base(points)
        {
        }

        public PointCollection Clone()
        {
            return new PointCollection(this);
        }
    }
}
