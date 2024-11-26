using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kursach_GSK
{
    public class Arrow3 : Shape
    {
        // Задаем размеры стрелки
        int arrowLength = 100;    // Общая длина стрелки
        int arrowHeight = 40;     // Высота стрелки
        int headLength = 40;      // Длина наконечника
        private Point center;
        private Pen drawPen;
        private SolidBrush fillBrush;
        private Point[] points;

        public Arrow3(Pen pen, Color fillColor)
        {
            drawPen = pen;
            fillBrush = new SolidBrush(fillColor);
            points = new Point[6];
        }

        public void SetPoints(Point start, Point end, Graphics g = null)
        {
            // Вычисляем центр
            center = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);
            CalculatePoints();
        }

        public override void Draw(Graphics g)
        {
            if (points == null) return;

            // Находим границы для сканирования
            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            // Закрашиваем фигуру построчным сканированием
            for (int y = minY; y <= maxY; y++)
            {
                List<int> intersections = new List<int>();

                // Находим все пересечения с ребрами
                for (int i = 0; i < points.Length; i++)
                {
                    int next = (i + 1) % points.Length;
                    Point p1 = points[i];
                    Point p2 = points[next];

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
                    g.DrawLine(new Pen(fillBrush.Color),
                        intersections[i], y,
                        intersections[i + 1], y);
                }
            }

            // Рисуем контур, соединяя точки линиями
            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length;
                g.DrawLine(drawPen, points[i], points[next]);
            }
        }

        public override bool Contains(Point p)
        {
            // Проверка принадлежности точки многоугольнику
            bool result = false;
            int j = points.Length - 1;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Y < p.Y && points[j].Y >= p.Y ||
                    points[j].Y < p.Y && points[i].Y >= p.Y)
                {
                    if (points[i].X + (p.Y - points[i].Y) /
                        (points[j].Y - points[i].Y) *
                        (points[j].X - points[i].X) < p.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        private void CalculatePoints()
        {
            // Задаем размеры стрелки
            int arrowLength = 100;    // Общая длина стрелки
            int arrowHeight = 40;     // Высота стрелки
            int headLength = 40;      // Длина наконечника

            // Вычисляем точки стрелки (6 точек)
            points[0] = new Point(center.X - arrowLength / 2, center.Y - arrowHeight / 2);          // Левая верхняя
            points[1] = new Point(center.X + arrowLength / 2 - headLength, center.Y - arrowHeight / 2); // Правая верхняя
            points[2] = new Point(center.X + arrowLength / 2, center.Y);                          // Острие стрелки
            points[3] = new Point(center.X + arrowLength / 2 - headLength, center.Y + arrowHeight / 2);  // Правая нижняя
            points[4] = new Point(center.X - arrowLength / 2, center.Y + arrowHeight / 2);          // Левая нижняя
            points[5] = new Point(center.X - arrowLength / 2 + headLength / 2, center.Y);           // Внутренний угол
        }
        public override void Transform(Matrix matrix)
        {
            PointF[] pointsF = Array.ConvertAll(points, p => new PointF(p.X, p.Y));
            matrix.TransformPoints(pointsF);
            points = Array.ConvertAll(pointsF, p => new Point((int)p.X, (int)p.Y));
        }

        public override Rectangle GetBounds()
        {
            if (points == null || points.Length == 0)
                return Rectangle.Empty;

            int minX = points[0].X;
            int minY = points[0].Y;
            int maxX = points[0].X;
            int maxY = points[0].Y;

            for (int i = 1; i < points.Length; i++)
            {
                minX = Math.Min(minX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxX = Math.Max(maxX, points[i].X);
                maxY = Math.Max(maxY, points[i].Y);
            }

            return Rectangle.FromLTRB(minX, minY, maxX, maxY);
        }


        public override Point[] GetPoints()
        {
            return points; // возвращаем массив точек стрелки
        }
        public override Shape Clone()
        {
            Arrow3 clone = new Arrow3(drawPen, fillBrush.Color);
            clone.center = new Point(this.center.X, this.center.Y);
            // Копируем массив точек
            clone.points = new Point[this.points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                clone.points[i] = new Point(this.points[i].X, this.points[i].Y);
            }
            clone.IsSelected = this.IsSelected;
            return clone;
        }
    }
}
