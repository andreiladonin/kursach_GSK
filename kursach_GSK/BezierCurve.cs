using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kursach_GSK
{
    public class BezierCurve:Shape
    {

        private List<PointF> controlPoints = new List<PointF>();
        private float dt = 0.01f;
        private Pen drawPen;

        public BezierCurve(Pen pen)
        {
            drawPen = pen;
        }

        public void AddPoint(PointF point)
        {
            controlPoints.Add(point);
            // Рисуем точку сразу при добавлении

            // Рисуем линию между точками

        }

        public List<PointF> Points { get { return controlPoints; } }

        private int Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }

        public override void Draw(Graphics g)
        {
            // Рисуем контрольные точки
            foreach (var p in controlPoints)
            {
                g.FillEllipse(Brushes.Blue, p.X - 3, p.Y - 3, 6, 6);
            }

            if (controlPoints.Count < 2) return;

            float t = 0;
            float xt, yt;
            int n = controlPoints.Count - 1;
            int nFact = Factorial(n);

            // Начальная точка
            float xPred = controlPoints[0].X;
            float yPred = controlPoints[0].Y;

            // Построение кривой по алгоритму из методички раздел 1.1
            while (t < 1 + dt / 2)
            {
                xt = 0; yt = 0;

                for (int i = 0; i <= n; i++)
                {
                    // Вычисляем полином Бернштейна
                    float J = (float)(Math.Pow(t, i) * Math.Pow(1 - t, n - i) *
                              nFact / (Factorial(i) * Factorial(n - i)));

                    // Накапливаем координаты точки кривой          
                    xt += controlPoints[i].X * J;
                    yt += controlPoints[i].Y * J;
                }

                g.DrawLine(drawPen, xPred, yPred, xt, yt);

                t += dt;
                xPred = xt;
                yPred = yt;
            }
        }

        public override bool Contains(Point p)
        {
            if (controlPoints.Count < 2) return false;

            // Проверяем, находится ли точка рядом с контрольными точками
            foreach (var point in controlPoints)
            {
                if (Math.Abs(point.X - p.X) < 5 && Math.Abs(point.Y - p.Y) < 5)
                    return true;
            }

            // Проверяем, находится ли точка рядом с кривой
            float t = 0;
            float dt = 0.01f;
            PointF prevPoint = controlPoints[0];

            while (t <= 1)
            {
                float xt = 0, yt = 0;
                int n = controlPoints.Count - 1;
                float nFact = Factorial(n);

                for (int i = 0; i <= n; i++)
                {
                    float J = (float)(Math.Pow(t, i) * Math.Pow(1 - t, n - i) *
                              nFact / (Factorial(i) * Factorial(n - i)));
                    xt += controlPoints[i].X * J;
                    yt += controlPoints[i].Y * J;
                }

                PointF currentPoint = new PointF(xt, yt);

                // Проверяем расстояние от точки до отрезка кривой
                if (DistanceToLine(p, prevPoint, currentPoint) < 5)
                    return true;

                prevPoint = currentPoint;
                t += dt;
            }

            return false;
        }

        private float DistanceToLine(Point p, PointF lineStart, PointF lineEnd)
        {
            float numerator = Math.Abs((lineEnd.Y - lineStart.Y) * p.X -
                                      (lineEnd.X - lineStart.X) * p.Y +
                                       lineEnd.X * lineStart.Y -
                                       lineEnd.Y * lineStart.X);

            float denominator = (float)Math.Sqrt(Math.Pow(lineEnd.Y - lineStart.Y, 2) +
                                                Math.Pow(lineEnd.X - lineStart.X, 2));

            return numerator / denominator;
        }

        public override void Transform(Matrix matrix)
        {
            PointF[] points = controlPoints.ToArray();
            matrix.TransformPoints(points);
            controlPoints = new List<PointF>(points);
        }

        public override Rectangle GetBounds()
        {
            if (controlPoints == null || controlPoints.Count == 0)
                return Rectangle.Empty;

            float minX = controlPoints[0].X;
            float minY = controlPoints[0].Y;
            float maxX = controlPoints[0].X;
            float maxY = controlPoints[0].Y;

            for (int i = 1; i < controlPoints.Count; i++)
            {
                minX = Math.Min(minX, controlPoints[i].X);
                minY = Math.Min(minY, controlPoints[i].Y);
                maxX = Math.Max(maxX, controlPoints[i].X);
                maxY = Math.Max(maxY, controlPoints[i].Y);
            }

            // Добавляем небольшой отступ для кривой
            return Rectangle.FromLTRB(
                (int)(minX - 5),
                (int)(minY - 5),
                (int)(maxX + 5),
                (int)(maxY + 5)
            );
        }
        public override Point[] GetPoints()
        {
            // Конвертируем контрольные точки в Point
            return controlPoints.Select(p => new Point((int)p.X, (int)p.Y)).ToArray();
        }
        public override Shape Clone()
        {
            BezierCurve clone = new BezierCurve(drawPen);
            clone.controlPoints = new List<PointF>(this.controlPoints.Select(p => new PointF(p.X, p.Y)));
            clone.IsSelected = this.IsSelected;
            return clone;
        }
    }
}
