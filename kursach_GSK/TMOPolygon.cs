using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kursach_GSK
{
    public class TMOPolygon : Shape
    {
        private List<Point> points;
        private Color fillColor;
        private Pen pen;

        public TMOPolygon(Pen pen, Color fillColor) 
        {
            this.fillColor = fillColor;
            points = new List<Point>();
            this.pen = pen;
        }

        public void AddPoint(Point point)
        {
            points.Add(point);
        }
        public override void Draw(Graphics g)
        {
            if (points.Count >= 2)
            {
                // Находим границы для сканирования
                int minY = points.Min(p => p.Y);
                int maxY = points.Max(p => p.Y);

                // Закрашиваем фигуру построчным сканированием
                for (int y = minY; y <= maxY; y++)
                {
                    List<int> intersections = new List<int>();

                    // Находим все пересечения с ребрами
                    for (int i = 0; i < points.Count - 1; i += 2)
                    {
                        Point p1 = points[i];
                        Point p2 = points[i + 1];

                        // Проверяем, пересекает ли строка сканирования текущее ребро
                        if ((p1.Y > y && p2.Y <= y) || (p2.Y > y && p1.Y <= y))
                        {
                            if (p1.Y != p2.Y) // Избегаем деления на ноль
                            {
                                // Вычисляем x-координату пересечения
                                int x = (int)(p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y));
                                intersections.Add(x);
                            }
                        }
                    }

                    // Сортируем точки пересечения по X
                    intersections.Sort();

                    // Рисуем линии между парами точек пересечения
                    for (int i = 0; i < intersections.Count - 1; i += 2)
                    {
                        g.DrawLine(new Pen(fillColor),
                            intersections[i], y,
                            intersections[i + 1], y);
                    }
                }

                // Рисуем контур, соединяя точки линиями
                for (int i = 0; i < points.Count - 1; i += 2)
                {
                    g.DrawLine(pen, points[i], points[i + 1]);
                }
            }
        }

        public override Point[] GetPoints() => points.ToArray();

        public override bool Contains(Point p)
        {
            for (int i = 0; i < points.Count - 1; i += 2)
            {
                if (p.Y >= points[i].Y - 2 && p.Y <= points[i].Y + 2 &&
                    p.X >= points[i].X && p.X <= points[i + 1].X)
                    return true;
            }
            return false;
        }

        public override void Transform(Matrix matrix)
        {
            PointF[] pointsF = points.Select(p => new PointF(p.X, p.Y)).ToArray();
            matrix.TransformPoints(pointsF);
            points = pointsF.Select(p => new Point((int)p.X, (int)p.Y)).ToList();
        }

        public override Shape Clone()
        {
            var clone = new TMOPolygon(new Pen(pen.Color), fillColor);
            clone.points = new List<Point>(points);
            return clone;
        }

        public override Rectangle GetBounds()
        {
            if (points.Count == 0) return Rectangle.Empty;

            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxX = points.Max(p => p.X);
            int maxY = points.Max(p => p.Y);

            return Rectangle.FromLTRB(minX, minY, maxX, maxY);
        }
    }
}
