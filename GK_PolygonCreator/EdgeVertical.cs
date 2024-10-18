using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_PolygonCreator
{
    public class EdgeVertical: Edge
    {
        public EdgeVertical(Point p1, Point p2): base(p1, p2)
        {
            ToolStripMenuItem deleteEdge = new ToolStripMenuItem("Delete Edge");
            ToolStripMenuItem addPoint = new ToolStripMenuItem("Add Point");
            ToolStripMenuItem removeVertical = new ToolStripMenuItem("Remove Vertical Constraint");

            base.color = Brushes.Red;
            base.canBeVertical = false;
            base.canBeHorizontal = false;
            base.canBeFixed = false;

            UpdateConstraints();
            base.menuEdge.Items.Add(removeVertical);
        }

        public override Point SetEndPoint(int dx, int dy)
        {
            return new Point(endPoint.X + dx, endPoint.Y + dy);
        }

        public override Point SetStartPoint(int dx, int dy)
        {
            return new Point(startPoint.X + dx, startPoint.Y + dy);
        }
    }
}
