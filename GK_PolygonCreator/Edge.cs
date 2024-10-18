using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_PolygonCreator
{
    public class Edge
    {
        public Point startPoint;
        public Point endPoint;
        public Point middlePoint;

        public Edge? nextEdge;
        public Edge? previousEdge;

        public Brush color;

        public bool canBeVertical = true;
        public bool canBeHorizontal = true;
        public bool canBeFixed = true;

        public ContextMenuStrip menuEdge = new ContextMenuStrip();

        public Edge(Point p1, Point p2)
        {
            this.startPoint = p1;
            this.endPoint = p2;
            this.middlePoint = SetMiddlePoint();

            this.color = Brushes.Green;

            UpdateConstraints();
        }


        public virtual Point SetStartPoint(int dx, int dy)
        {
            return this.startPoint;
        }
        public virtual Point SetEndPoint(int dx, int dy)
        {
            return this.endPoint;
        }

        public Point SetMiddlePoint()
        {
            int midX = (startPoint.X + endPoint.X) / 2;
            int midY = (startPoint.Y + endPoint.Y) / 2;

            return new Point(midX, midY);
        }

        public void UpdateConstraints()
        {
            this.menuEdge.Items.Clear();

            ToolStripMenuItem deleteEdge = new ToolStripMenuItem("Delete Edge");
            this.menuEdge.Items.Add(deleteEdge);

            ToolStripMenuItem addPoint = new ToolStripMenuItem("Add Point");
            this.menuEdge.Items.Add(addPoint);

            if (canBeVertical)
            {
                ToolStripMenuItem makeVertical = new ToolStripMenuItem("Make Vertical");
                this.menuEdge.Items.Add(makeVertical);
            }
            if (canBeHorizontal)
            {
                ToolStripMenuItem makeHorizontal = new ToolStripMenuItem("Make Horizontal");
                this.menuEdge.Items.Add(makeHorizontal);
            }
            if (canBeFixed)
            {
                ToolStripMenuItem fixLength = new ToolStripMenuItem("Set Length");
                this.menuEdge.Items.Add(fixLength);
            }
        }
    }
}
