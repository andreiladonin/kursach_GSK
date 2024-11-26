using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kursach_GSK
{
    internal class RegularPolygon : Shape
    {

        public List<PointF> vertices;
        private Pen drawPen;
        private SolidBrush fillBrush;
        private Point centerPoint;
        private bool isCreating; // Флаг режима создания

        public RegularPolygon(Pen pen, Color fillColor)
        {
            drawPen = pen;
            fillBrush = new SolidBrush(fillColor);
            vertices = new List<PointF>();
            isCreating = true;
        }

        public override bool Contains(Point p)
        {
            if (vertices.Count < 3) return false;

            bool result = false;
            int j = vertices.Count - 1;

            // Алгоритм трассировки лучом
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Y < p.Y && vertices[j].Y >= p.Y ||
                    vertices[j].Y < p.Y && vertices[i].Y >= p.Y)
                {
                    if (vertices[i].X + (p.Y - vertices[i].Y) /
                        (vertices[j].Y - vertices[i].Y) *
                        (vertices[j].X - vertices[i].X) < p.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            return result;

        }

        public override void Draw(Graphics g)
        {
            if (vertices.Count < 3) return;

            // Находим границы по Y
            float ymin = vertices[0].Y;
            float ymax = vertices[0].Y;

            for (int i = 1; i < vertices.Count; i++)
            {
                if (vertices[i].Y < ymin) ymin = vertices[i].Y;
                if (vertices[i].Y > ymax) ymax = vertices[i].Y;
            }

            // Для каждой строки Y
            for (int y = (int)ymin; y <= ymax; y++)
            {
                List<float> xb = new List<float>();

                // Находим пересечения со строкой
                for (int i = 0; i < vertices.Count; i++)
                {
                    int k = (i < vertices.Count - 1) ? i + 1 : 0;

                    if ((vertices[i].Y < y && vertices[k].Y >= y) ||
                        (vertices[i].Y >= y && vertices[k].Y < y))
                    {
                        // Вычисляем x-координату пересечения
                        float x = vertices[i].X +
                                 (y - vertices[i].Y) *
                                 (vertices[k].X - vertices[i].X) /
                                 (vertices[k].Y - vertices[i].Y);

                        xb.Add(x);
                    }
                }

                // Сортируем список пересечений
                xb.Sort();

                // Закрашиваем сегменты строки между парами точек пересечения
                for (int i = 0; i < xb.Count - 1; i += 2)
                {
                    g.DrawLine(new Pen(fillBrush.Color),
                        xb[i], y, xb[i + 1], y);
                }
            }

            // Рисуем центральную точку только в режиме создания
            if (isCreating)
            {
                g.FillEllipse(Brushes.Red, centerPoint.X - 3, centerPoint.Y - 3, 6, 6);
            }
            // Рисуем контур
            g.DrawPolygon(drawPen, vertices.ToArray());
        }

        public void SetParameters(Point center, Point radiusPoint, int sides, Graphics g)
        {
            this.centerPoint = center;
            vertices.Clear();

            // Вычисляем радиус
            double radius = Math.Sqrt(Math.Pow(radiusPoint.X - center.X, 2) +
                                    Math.Pow(radiusPoint.Y - center.Y, 2));

            // Угол между вершинами                    
            double dFi = 2 * Math.PI / sides;

            // Вычисляем координаты вершин
            for (int i = 0; i < sides; i++)
            {
                float x = (float)(-radius * Math.Sin(i * dFi) + center.X);
                float y = (float)(radius * Math.Cos(i * dFi) + center.Y);
                vertices.Add(new PointF(x, y));
            }

            if (g != null && isCreating) // Рисуем точку только в режиме создания
            {
                g.FillEllipse(Brushes.Red, center.X - 3, center.Y - 3, 6, 6);
            }

            if (isCreating)
            {
                isCreating = false; // После установки параметров выключаем режим создания
            }
        }

        public override void Transform(Matrix matrix)
        {
            // Сохраняем радиус и пропорции
            float originalRadius = CalculateRadius();

            PointF[] points = vertices.ToArray();
            matrix.TransformPoints(points);
            vertices = new List<PointF>(points);

            // Пересчитываем центр
            Point[] centerPoint = new Point[] { this.centerPoint };
            matrix.TransformPoints(centerPoint);
            this.centerPoint = centerPoint[0];

            // Корректируем вершины для сохранения формы
            NormalizeVertices(originalRadius);
        }

        private float CalculateRadius()
        {
            if (vertices.Count == 0) return 0;
            return (float)Math.Sqrt(
                Math.Pow(vertices[0].X - centerPoint.X, 2) +
                Math.Pow(vertices[0].Y - centerPoint.Y, 2));
        }

        private void NormalizeVertices(float radius)
        {
            float currentRadius = CalculateRadius();
            float scale = radius / currentRadius;

            for (int i = 0; i < vertices.Count; i++)
            {
                float dx = vertices[i].X - centerPoint.X;
                float dy = vertices[i].Y - centerPoint.Y;
                vertices[i] = new PointF(
                    centerPoint.X + dx * scale,
                    centerPoint.Y + dy * scale);
            }
        }
        public override Rectangle GetBounds()
        {
            if (vertices == null || vertices.Count == 0)
                return Rectangle.Empty;

            float minX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxX = vertices[0].X;
            float maxY = vertices[0].Y;

            for (int i = 1; i < vertices.Count; i++)
            {
                minX = Math.Min(minX, vertices[i].X);
                minY = Math.Min(minY, vertices[i].Y);
                maxX = Math.Max(maxX, vertices[i].X);
                maxY = Math.Max(maxY, vertices[i].Y);
            }

            return Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY);
        }

        public override Point[] GetPoints()
        {
            // Конвертируем PointF в Point
            return vertices.Select(v => new Point((int)v.X, (int)v.Y)).ToArray();
        }

        public override Shape Clone()
        {
            RegularPolygon clone = new RegularPolygon(drawPen, fillBrush.Color);
            clone.vertices = new List<PointF>(this.vertices.Select(v => new PointF(v.X, v.Y)));
            clone.centerPoint = this.centerPoint;
            clone.IsSelected = this.IsSelected;
            clone.isCreating = this.isCreating;
            return clone;
        }
    }
}
