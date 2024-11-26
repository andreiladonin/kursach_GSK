using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kursach_GSK
{
    public abstract class Shape
    {
        protected Point location;
        protected Color lineColor;
        protected Color fillColor;
        protected bool isSelected;

        public Shape()
        {
            lineColor = Color.Black;
            fillColor = Color.White;
            isSelected = false;
        }

        public abstract void Draw(Graphics g);
        public virtual void DrawPreview(Graphics g)
        {
            Draw(g); // По умолчанию просто рисуем как обычно
        }
        public abstract bool Contains(Point p);
        public abstract void Transform(Matrix matrix);
        public abstract Shape Clone();
        public abstract Rectangle GetBounds();

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public virtual void DrawSelection(Graphics g)
        {
            // Можно переопределить в наследниках для специфической отрисовки выделения
            if (IsSelected)
            {
                // Например, рисуем прямоугольник вокруг фигуры
                using (Pen selectionPen = new Pen(Color.Blue, 1))
                {
                    selectionPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    // Получаем прямоугольник, охватывающий фигуру
                    Rectangle bounds = GetBounds();
                    g.DrawRectangle(selectionPen, bounds);
                }
            }
        }
        public void Select()
        {
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public abstract Point[] GetPoints();
    }
}
