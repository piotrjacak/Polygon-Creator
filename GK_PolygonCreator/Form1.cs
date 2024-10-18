using System.Drawing;
using System.Net;
using System.Reflection;
using System.Windows.Forms.VisualStyles;

namespace GK_PolygonCreator
{
    public partial class Form1 : Form
    {
        // Struktury dla punktów
        public Point? startPoint = null;
        public Point? prevPoint = null;

        public Point? movingStartPoint = null;
        public int movingIndex = -1;

        public bool movingPolygon = false;
        public Point lastMousePosition = new Point(0, 0);

        // Struktury dla krawêdzi
        public List<Edge> edges = new List<Edge>();

        public bool drawingMode = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void drawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            // Rysowanie punktów
            if (e.Button == MouseButtons.Left && drawingMode)
            {
                Point currentPoint = new Point(e.X, e.Y);

                // Rozpoczêcie rysowania wielok¹ta
                if (startPoint == null)
                {
                    startPoint = currentPoint;
                }
                // Zamkniêcie wielok¹ta
                else if (IsPointClose(currentPoint, (Point)startPoint))
                {
                    Edge edge = new Edge((Point)prevPoint, (Point)startPoint);
                    edges.Add(edge);

                    // Zaktualizowanie s¹siadów ka¿dej krawêdzi
                    //UpdateNeighborEdges();

                    drawingMode = false;
                }
                // Dodanie krawêdzi
                else
                {
                    Edge edge = new Edge((Point)prevPoint, currentPoint);
                    edges.Add(edge);
                }
                prevPoint = currentPoint;
                drawingPanel.Invalidate();
            }

            // Menu kontekstowe dla punktów i krawêdzi
            else if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < edges.Count; i++)
                {
                    if (IsPointClose(edges[i].startPoint, e.Location))
                    {
                        ContextMenuStrip menuPoint = new ContextMenuStrip();
                        ToolStripMenuItem deletePoint = new ToolStripMenuItem("Delete Vertex");

                        // Akcja na usuniêcie punktu
                        deletePoint.Click += (sender, args) => DeletePoint(i);

                        menuPoint.Items.Add(deletePoint);
                        menuPoint.Show(this, e.Location);
                        break;
                    }
                    else if (IsPointClose(edges[i].middlePoint, e.Location))
                    {
                        edges[i].menuEdge.Items[0].Click += (sender, args) => DeleteEdge(i);
                        edges[i].menuEdge.Items[1].Click += (sender, args) => AddPoint(i);
                        
                        // Opcje dla krawêdzi pionowej
                        if (edges[i].GetType() == typeof(EdgeVertical))
                        {
                            edges[i].menuEdge.Items[2].Click += (sender, args) => RemoveVerticalConstraint(i);
                        }
                        // Opcje dla krawêdzi zwyk³ej
                        else
                        {
                            edges[i].menuEdge.Items[2].Click += (sender, args) => MakeVertical(i);
                            edges[i].menuEdge.Items[3].Click += (sender, args) => MakeHorizontal(i);
                        }

                        edges[i].menuEdge.Show(this, e.Location);
                        break;
                    }
                }
            }
        }


        // Rysowanie wielok¹ta
        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            if (startPoint != null)
                e.Graphics.FillEllipse(Brushes.Blue, ((Point)startPoint).X - 5, ((Point)startPoint).Y - 5, 10, 10);

            foreach (var edge in edges)
            {
                e.Graphics.FillEllipse(Brushes.Blue, edge.startPoint.X - 5, edge.startPoint.Y - 5, 10, 10);
                e.Graphics.FillEllipse(Brushes.Blue, edge.endPoint.X - 5, edge.endPoint.Y - 5, 10, 10);
                e.Graphics.FillEllipse(edge.color, edge.middlePoint.X - 4, edge.middlePoint.Y - 4, 8, 8);

                e.Graphics.DrawLine(Pens.Black, edge.startPoint, edge.endPoint);
            }
        }


        private void drawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Szukamy punktu, który znajduje siê blisko klikniêcia
                for (int i = 0; i < edges.Count; i++)
                {
                    if (IsPointClose(edges[i].startPoint, e.Location))
                    {
                        movingStartPoint = edges[i].startPoint;
                        movingIndex = i;
                        lastMousePosition = e.Location;
                        return;
                    }
                }

                // Przesuwanie ca³ego wielok¹ta
                if (!drawingMode)
                {
                    movingPolygon = true;
                    lastMousePosition = e.Location;
                }
            }
        }

        private void drawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // Przesuwanie punktu
            if (movingStartPoint.HasValue && movingIndex >= 0)
            {
                Point currPoint = new Point(e.X, e.Y);
                int dx = currPoint.X - lastMousePosition.X;
                int dy = currPoint.Y - lastMousePosition.Y;
                
                edges[movingIndex].startPoint = currPoint;
                edges[movingIndex].endPoint = edges[movingIndex].SetEndPoint(dx, dy);
                edges[movingIndex].middlePoint = edges[movingIndex].SetMiddlePoint();

                edges[(movingIndex - 1 + edges.Count) % edges.Count].startPoint = edges[(movingIndex - 1 + edges.Count) % edges.Count].SetStartPoint(dx, dy);
                edges[(movingIndex - 1 + edges.Count) % edges.Count].endPoint = edges[movingIndex].startPoint;
                edges[(movingIndex - 1 + edges.Count) % edges.Count].middlePoint = edges[(movingIndex - 1 + edges.Count) % edges.Count].SetMiddlePoint();

                RefreshEdges(movingIndex, dx, dy);
                //edges[(movingIndex + 1) % edges.Count].startPoint = edges[movingIndex].endPoint;
                //edges[(movingIndex + 1) % edges.Count].endPoint = edges[(movingIndex + 1) % edges.Count].SetEndPoint(dx, dy);
                //edges[(movingIndex + 1) % edges.Count].middlePoint = edges[(movingIndex + 1) % edges.Count].SetMiddlePoint();

                if (movingIndex == 0)
                    startPoint = currPoint;
                lastMousePosition = e.Location;
                drawingPanel.Invalidate();
            }

            // Przesuwanie wielok¹ta
            else if (movingPolygon && startPoint != null)
            {
                int dx = e.X - lastMousePosition.X;
                int dy = e.Y - lastMousePosition.Y;

                for (int i = 0; i < edges.Count; i++)
                {
                    edges[i].startPoint = new Point(edges[i].startPoint.X + dx, edges[i].startPoint.Y + dy);
                    edges[i].endPoint = new Point(edges[i].endPoint.X + dx, edges[i].endPoint.Y + dy);
                    edges[i].middlePoint = edges[i].SetMiddlePoint();
                }

                startPoint = new Point(((Point)startPoint).X + dx, ((Point)startPoint).Y + dy);
                lastMousePosition = e.Location;
                drawingPanel.Invalidate();
            }
        }

        private void drawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            // Zakoñczenie przesuwania punktu
            if (movingStartPoint.HasValue && movingIndex >= 0)
            {
                movingStartPoint = null;
                movingIndex = -1;
                drawingPanel.Invalidate();
            }

            // Zakoñczenie przesuwania wielok¹ta
            else if (movingPolygon)
            {
                movingPolygon = false;
                drawingPanel.Invalidate();
            }
        }

        private void DeletePoint(int edgeIndex)
        {
            RemoveAllConstraints(edgeIndex);
            RemoveAllConstraints((edgeIndex - 1) % edges.Count);

            // Usuniêcie punktu
            if (edges[edgeIndex].startPoint == startPoint)
                startPoint = edges[(edgeIndex + 1) % edges.Count].startPoint;

            edges[(edgeIndex - 1 + edges.Count) % edges.Count] = new Edge(edges[(edgeIndex - 1 + edges.Count) % edges.Count].startPoint, edges[edgeIndex].endPoint);
            edges.RemoveAt(edgeIndex);

            //UpdateNeighborEdges();
            drawingPanel.Invalidate();
        }

        private void DeleteEdge(int edgeIndex)
        {
            RemoveAllConstraints(edgeIndex);

            // Usuniêcie krawêdzi
            if (edges[edgeIndex].endPoint == startPoint)
                startPoint = edges[(edgeIndex + 1) % edges.Count].endPoint;

            edges[(edgeIndex + 1) % edges.Count].startPoint = edges[edgeIndex].startPoint;
            edges[(edgeIndex + 1) % edges.Count].middlePoint = edges[(edgeIndex + 1) % edges.Count].SetMiddlePoint();

            edges.RemoveAt(edgeIndex);

            //UpdateNeighborEdges();
            drawingPanel.Invalidate();
        }

        private void AddPoint(int edgeIndex)
        {
            RemoveAllConstraints(edgeIndex);

            // Dodanie punktu na œrodku krawêdzi
            Edge newEdge = new Edge(edges[edgeIndex].middlePoint, edges[edgeIndex].endPoint);
            edges[edgeIndex] = new Edge(edges[edgeIndex].startPoint, edges[edgeIndex].middlePoint);

            edges.Insert(edgeIndex + 1, newEdge);

            //UpdateNeighborEdges();
            drawingPanel.Invalidate();
        }

        private void MakeVertical(int edgeIndex)
        {
            Point sp = edges[edgeIndex].startPoint;
            Point ep = new Point(edges[edgeIndex].startPoint.X, edges[edgeIndex].endPoint.Y);

            edges[(edgeIndex + 1) % edges.Count].startPoint = ep;
            edges[(edgeIndex + 1) % edges.Count].middlePoint = edges[(edgeIndex + 1) % edges.Count].SetMiddlePoint();
            edges[(edgeIndex + 1) % edges.Count].canBeVertical = false;
            edges[(edgeIndex + 1) % edges.Count].UpdateConstraints();

            edges[(edgeIndex - 1 + edges.Count) % edges.Count].canBeVertical = false;
            edges[(edgeIndex - 1 + edges.Count) % edges.Count].UpdateConstraints();

            edges[edgeIndex] = new EdgeVertical(sp, ep);
            edges[edgeIndex].menuEdge.Items[2].Click += (sender, args) => RemoveVerticalConstraint(edgeIndex);

            drawingPanel.Invalidate();
        }

        private void RemoveVerticalConstraint(int edgeIndex)
        {
            edges[(edgeIndex + 1) % edges.Count].canBeVertical = true;
            edges[(edgeIndex + 1) % edges.Count].UpdateConstraints();

            edges[(edgeIndex - 1 + edges.Count) % edges.Count].canBeVertical = true;
            edges[(edgeIndex - 1 + edges.Count) % edges.Count].UpdateConstraints();

            edges[edgeIndex] = new Edge(edges[edgeIndex].startPoint, edges[edgeIndex].endPoint);

            drawingPanel.Invalidate();
        }

        private void MakeHorizontal(int edgeIndex)
        {

        }

        private void RemoveAllConstraints(int edgeIndex)
        {
            edges[(edgeIndex + 1) % edges.Count].canBeVertical = true;
            edges[(edgeIndex + 1) % edges.Count].canBeHorizontal = true;
            edges[(edgeIndex + 1) % edges.Count].canBeFixed = true;
            edges[(edgeIndex + 1) % edges.Count].UpdateConstraints();

            edges[(edgeIndex - 1 + edges.Count) % edges.Count].canBeVertical = true;
            edges[(edgeIndex - 1 + edges.Count) % edges.Count].canBeHorizontal = true;
            edges[(edgeIndex - 1 + edges.Count) % edges.Count].canBeFixed = true;
            edges[(edgeIndex - 1 + edges.Count) % edges.Count].UpdateConstraints();

            edges[edgeIndex] = new Edge(edges[edgeIndex].startPoint, edges[edgeIndex].endPoint);

            drawingPanel.Invalidate();
        }

        // Czyszczenie pola do rysowania
        private void cleanAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            edges = new List<Edge>();
            startPoint = null;
            prevPoint = null;
            drawingMode = true;
            drawingPanel.Invalidate();
        }


        // Funkcja pomocnicza do sprawdzenia czy miejsce klikniêcia jest blisko punktu
        private bool IsPointClose(Point p1, Point p2)
        {
            const int tolerance = 8;
            return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance;
        }

        private void RefreshEdges(int movingIndex, int dx, int dy)
        {
            for (int i = movingIndex + 1; i < edges.Count; i++)
            {
                edges[i % edges.Count].startPoint = edges[i - 1].endPoint;
                edges[i % edges.Count].endPoint = edges[i % edges.Count].SetEndPoint(dx, dy);
                edges[i % edges.Count].middlePoint = edges[i % edges.Count].SetMiddlePoint();
            }
            edges[(movingIndex - 1 + edges.Count) % edges.Count].middlePoint = edges[(movingIndex - 1 + edges.Count) % edges.Count].SetMiddlePoint();
            edges[(movingIndex - 1 + edges.Count) % edges.Count].endPoint = edges[movingIndex].startPoint;
        }

        //private void UpdateNeighborEdges()
        //{
        //    for (int i = 0; i < edges.Count; i++)
        //    {
        //        edges[i].previousEdge = edges[(i - 1 + edges.Count) % edges.Count];
        //        edges[i].nextEdge = edges[(i + 1) % edges.Count];
        //    }
        //}
    }
}
