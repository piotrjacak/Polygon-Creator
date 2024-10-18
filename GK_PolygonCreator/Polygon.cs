using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_PolygonCreator
{
    public class Polygon
    {
        // Struktury dla punktów
        public Point? startPoint = null;
        public Point? prevPoint = null;

        public Point? movingStartPoint = null;
        public Point? movingMiddlePoint = null;
        public int movingIndex = -1;
        public bool movingPolygon = false;

        public Point lastMousePosition = new Point(0, 0);
        public bool drawingMode = true;

        public List<Edge> edges = new List<Edge>();

        public Polygon(Point startPoint, List<Edge> edges)
        {
            this.startPoint = startPoint;
            this.edges = edges;
        }

    }
}
