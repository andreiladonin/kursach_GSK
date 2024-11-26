using System.Drawing.Drawing2D;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;

namespace kursach_GSK
{
    public partial class Form1 : Form
    {
        private Tool currentTool = Tool.None;
        private Transform currentTransform = Transform.None;
        private Operation currentOperation = Operation.None;
        private Shape selectedShape = null;
        private List<Shape> shapes = new List<Shape>();
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing = false;
        private Pen DrawPen = new Pen(Color.Black, 1);

        // Добавьте поле для хранения цвета заливки
        private Color fillColor = Color.LightBlue;
        private Arrow3 currentArrow = null;
        private RegularPolygon currentPolygon = null;
        private Point polygonCenter;

        private Point transformCenter; // центр преобразования
        private Point transformLineStart; // начало линии для отражения
        private Point transformLineEnd;   // конец линии для отражения
        private bool isSettingTransformCenter = false; // режим установки центра
        private bool isSettingTransformLine = false;   // режим установки линии

        private Point? rotationCenter = null;  // Центр поворота
        private bool isSettingCenter = false;  // Флаг для установки центра
        private float startAngle = 0;  // Начальный угол для вычисления поворота
        private float currentAngle = 0; // Текущий угол

        private float previousAngle = 0;
        Graphics g;
        private BezierCurve currentBezier = null;
        // Добавляем новые поля для ТМО
        private Shape firstShape = null;
        private Shape secondShape = null;
        private List<Point> XA = new List<Point>();
        private List<Point> XB = new List<Point>();
        private int[] SetQ = new int[2];

        public Form1()
        {
            InitializeComponent();
            SetupMenus();
            SetupStatusBar();

            g = drawingArea.CreateGraphics();
            // Включаем обработку колесика мыши

        }

       
        private void SetupMenus()
        {
            // Draw Menu
            var drawMenu = new ToolStripMenuItem("Рисование");
            drawMenu.DropDownItems.AddRange(new ToolStripItem[] {
            new ToolStripMenuItem("Правильный многоугольник", null, (s,e) => SetCurrentTool(Tool.Polygon)),
            new ToolStripMenuItem("Стрелка", null, (s,e) => SetCurrentTool(Tool.Arrow)),
            new ToolStripMenuItem("Кривая Безье", null, (s,e) => SetCurrentTool(Tool.Bezier))
        });

            drawMenu.DropDownItems.Add(new ToolStripMenuItem("Выбор", null, (s, e) => SetCurrentTool(Tool.None)));

            // Transform Menu
            var transformMenu = new ToolStripMenuItem("Преобразование");
            transformMenu.DropDownItems.AddRange(new ToolStripItem[] {
            new ToolStripMenuItem("Поворот вокруг заданного центра", null, (s,e) => SetCurrentTransform(Transform.Rotate)),
            new ToolStripMenuItem("Отражение по вертикали", null, (s,e) => SetCurrentTransform(Transform.MirrorVertical)),
            new ToolStripMenuItem("Отражение по линии", null, (s,e) => SetCurrentTransform(Transform.MirrorLine))
        });

            // Operations Menu
            var operationsMenu = new ToolStripMenuItem("Операции");
            operationsMenu.DropDownItems.AddRange(new ToolStripItem[] {
            new ToolStripMenuItem("Объединение фигур", null, (s,e) => SetCurrentOperation(Operation.Union)),
            new ToolStripMenuItem("Cимметрическая разность фигур", null, (s,e) => SetCurrentOperation(Operation.SymmetticDiffernt)),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Удалить выбранную фигуру", null, (s,e) => DeleteSelected())
        });

            var colorMenu = new ToolStripMenuItem("Цвет");
            colorMenu.DropDownItems.AddRange(new ToolStripItem[] {
            new ToolStripMenuItem("Черный", null, (s,e) => ChangePenColor(Color.Black)),
            new ToolStripMenuItem("Красный", null, (s,e) => ChangePenColor(Color.Red)),
            new ToolStripMenuItem("Синий", null, (s,e) => ChangePenColor(Color.Blue)),
            new ToolStripMenuItem("Зеленый", null, (s,e) => ChangePenColor(Color.Green)),
            // Цвет заливки
            new ToolStripSeparator(),
            new ToolStripMenuItem("Цвет заливки...", null, (s,e) => ChooseFillColor())
        });
            var clearItem = new ToolStripMenuItem("Очистить холст", null, (s, e) => ClearCanvas());

            menuStrip1.Items.AddRange(new ToolStripItem[] {
            drawMenu,
            transformMenu,
            operationsMenu,
            colorMenu,
            clearItem
        });
        }

        private void SelectShape(Point mousePoint)
        {
            if (currentOperation != Operation.None)
            {
                foreach (var shape in shapes)
                {
                    if (shape.Contains(mousePoint))
                    {
                        if (firstShape == null)
                        {
                            firstShape = shape;
                            XA.Clear();
                            GetBoundaryPoints(firstShape, XA);
                            MessageBox.Show("Выберите вторую фигуру");
                            return;
                        }
                        else if (secondShape == null && shape != firstShape)
                        {
                            secondShape = shape;
                            XB.Clear();
                            GetBoundaryPoints(secondShape, XB);
                            DrawResult(); // Теперь эта функция создаст новую фигуру

                            // Сбрасываем состояние
                            firstShape = null;
                            secondShape = null;
                            currentOperation = Operation.None;
                            drawingArea.Invalidate();
                            return;
                        }
                    }
                }
            }

            else
            {
                foreach (var shape in shapes)
                {
                    shape.IsSelected = false;
                }

                selectedShape = null;
                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    if (shapes[i].Contains(mousePoint))
                    {
                        selectedShape = shapes[i];
                        selectedShape.IsSelected = true;
                        break;
                    }
                }
                drawingArea.Invalidate();
            }

        }

        private void GetBoundaryPoints(Shape shape, List<Point> boundaryPoints)
        {
            Point[] points = shape.GetPoints();
            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    int next = (i + 1) % points.Length;
                    Point p1 = points[i];
                    Point p2 = points[next];

                    if ((p1.Y < y && p2.Y >= y) || (p2.Y < y && p1.Y >= y))
                    {
                        if (p1.Y != p2.Y)
                        {
                            int x = (int)(p1.X + (y - p1.Y) * (p2.X - p1.X) / (float)(p2.Y - p1.Y));
                            boundaryPoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            BubbleSort(boundaryPoints);
        }
        private void ClearCanvas()
        {
            g.Clear(Color.White);
            shapes.Clear();
            currentBezier = null;
            drawingArea.Invalidate();
        }

        private void ChangePenColor(Color color)
        {
            DrawPen.Color = color;
        }

        private void ChooseFillColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                fillColor = colorDialog.Color;
            }
        }

        private void DrawingArea_Paint(object sender, PaintEventArgs e)
        {
            foreach (var shape in shapes)
            {
                shape.Draw(e.Graphics);
                shape.DrawSelection(e.Graphics);
            }

            // Временное отображение центра многоугольника
            if (currentTool == Tool.Polygon && currentPolygon != null)
            {
                e.Graphics.FillEllipse(Brushes.Red, polygonCenter.X - 3, polygonCenter.Y - 3, 6, 6);
            }
        }

        private void DrawingArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTransform == Transform.Rotate)
            {
                if (selectedShape != null)
                {
                    if (!isSettingTransformCenter)
                    {
                        // Первый клик - задаем центр поворота
                        transformCenter = e.Location;
                        isSettingTransformCenter = true;
                        g.FillEllipse(Brushes.Red, transformCenter.X - 3, transformCenter.Y - 3, 6, 6);
                        MessageBox.Show("Укажите вторую точку для определения угла поворота");
                    }
                    else
                    {
                        // Второй клик - выполняем поворот
                        Point secondPoint = e.Location;

                        // Вычисляем угол
                        float angle = CalculateRotationAngle(transformCenter, secondPoint);

                        // Выполняем поворот
                        Matrix rotationMatrix = new Matrix();
                        rotationMatrix.RotateAt(angle, transformCenter);
                        selectedShape.Transform(rotationMatrix);

                        // Сбрасываем состояние
                        isSettingTransformCenter = false;
                        currentTransform = Transform.None;
                        drawingArea.Invalidate();
                    }
                }
            }

            else if (currentTransform == Transform.MirrorVertical && selectedShape != null)
            {
                // Применяем отражение при клике
                MirrorShapeVertical(selectedShape, e.X);
                currentTransform = Transform.None;
                drawingArea.Invalidate();
            }

            // Обрабатываем отражение по линии
            else if (currentTransform == Transform.MirrorLine && selectedShape != null)
            {
                if (!isSettingTransformLine)
                {
                    // Первый клик - начало линии
                    transformLineStart = e.Location;
                    isSettingTransformLine = true;
                }
                else
                {
                    // Второй клик - конец линии и отражение
                    transformLineEnd = e.Location;
                    MirrorShapeOverLine(selectedShape, transformLineStart, transformLineEnd);
                    isSettingTransformLine = false;
                    currentTransform = Transform.None;
                    drawingArea.Invalidate();
                }
            }

            // Обработка других инструментов
            if (currentTool == Tool.None)
            {
                SelectShape(e.Location);
                return;
            }
            if (currentTool == Tool.Arrow)
            {
                startPoint = e.Location;
                currentArrow = new Arrow3(DrawPen, fillColor);
                isDrawing = true;
            }
            if (currentTool == Tool.Polygon)
            {
                if (currentPolygon == null)
                {
                    polygonCenter = e.Location;
                    currentPolygon = new RegularPolygon(DrawPen, fillColor);
                }
                else
                {
                    currentPolygon.SetParameters(polygonCenter, e.Location, (int)numericUpDown1.Value, g);
                    shapes.Add(currentPolygon);
                    currentPolygon = null;
                }
                drawingArea.Invalidate();
            }
            if (currentTool == Tool.Bezier)
            {
                if (currentBezier == null)
                {
                    currentBezier = new BezierCurve(DrawPen);
                }

                currentBezier.AddPoint(e.Location);
                g.FillEllipse(Brushes.Blue, e.X - 3, e.Y - 3, 6, 6);

                if (e.Button == MouseButtons.Right)
                {
                    if (currentBezier.Points.Count >= 4)
                    {
                        shapes.Add(currentBezier);
                        currentBezier = null;
                        drawingArea.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Необходимо как минимум 4 точки для создания кривой Безье!");
                    }
                }
            }
        }

        private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentTransform == Transform.Rotate && isSettingTransformCenter && selectedShape != null)
            {
                drawingArea.Invalidate();
                using (Graphics g = drawingArea.CreateGraphics())
                {
                    g.FillEllipse(Brushes.Red, transformCenter.X - 3, transformCenter.Y - 3, 6, 6);
                    g.DrawLine(new Pen(Color.Black), transformCenter, e.Location);

                    // Отображаем предварительный результат поворота
                    Shape previewShape = selectedShape.Clone();
                    float previewAngle = CalculateRotationAngle(transformCenter, e.Location);
                    Matrix previewMatrix = new Matrix();
                    previewMatrix.RotateAt(previewAngle, transformCenter);
                    previewShape.Transform(previewMatrix);
                    using (Pen previewPen = new Pen(Color.Gray))
                    {
                        previewPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        previewShape.Draw(g);
                    }
                }
            }
            if (currentTransform == Transform.MirrorVertical && selectedShape != null)
            {
                // Рисуем вертикальную линию при движении мыши
                drawingArea.Invalidate();
                using (Graphics g = drawingArea.CreateGraphics())
                {
                    // Рисуем пунктирную линию
                    using (Pen dashedPen = new Pen(Color.Black))
                    {
                        dashedPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(dashedPen, e.X, 0, e.X, drawingArea.Height);
                    }

                    // Показываем предварительный результат отражения
                    Shape preview = selectedShape.Clone();
                    Matrix matrix = new Matrix(-1, 0, 0, 1, 2 * e.X, 0);
                    preview.Transform(matrix);
                    preview.Draw(g);
                }
            }

            if (currentTransform == Transform.MirrorLine && isSettingTransformLine && selectedShape != null)
            {
                drawingArea.Invalidate();
                using (Graphics g = drawingArea.CreateGraphics())
                {
                    // Рисуем линию отражения пунктиром
                    using (Pen dashedPen = new Pen(Color.Black))
                    {
                        dashedPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(dashedPen, transformLineStart, e.Location);
                    }

                    // Показываем предварительный результат отражения
                    Shape preview = selectedShape.Clone();
                    MirrorShapeOverLine(preview, transformLineStart, e.Location);
                    preview.Draw(g);
                }
            }

            if (currentTool == Tool.Arrow && isDrawing)
            {
                currentArrow.SetPoints(startPoint, e.Location);
                drawingArea.Invalidate();
            }

            drawingArea.Invalidate();
        }

        private void DrawingArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentTransform == Transform.Rotate)
            {
                isSettingTransformCenter = false;
                currentTransform = Transform.None;
            }
            if (isSettingCenter)
            {
                FinishRotation();
                MessageBox.Show("Поворот завершён.");
            }

            if (currentTool == Tool.Arrow && isDrawing)
            {
                currentArrow.SetPoints(startPoint, e.Location);
                shapes.Add(currentArrow);
                currentArrow = null;
                isDrawing = false;
                drawingArea.Invalidate();
            }
        }

        private float CalculateRotationAngle(Point center, Point second)
        {
            // Вычисляем вектор от центра к точке
            float dx = second.X - center.X;
            float dy = second.Y - center.Y;

            // Вычисляем угол в градусах
            float angle = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);

            // Возвращаем угол (можно добавить коэффициент для изменения скорости поворота)
            return angle;
        }

        private void RotateShapeAroundPoint(Shape shape, Point center, float angle)
        {
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, center);
            shape.Transform(matrix);
            drawingArea.Invalidate();
        }
        private void FinishRotation()
        {
            isSettingCenter = false;
            startAngle = currentAngle;
        }

        private void SetCurrentTool(Tool tool)
        {
            currentTool = tool;
            currentTransform = Transform.None;
            UpdateStatusBar();
        }

        private void SetCurrentTransform(Transform transform)
        {
            currentTool = Tool.None;
            if (transform == Transform.Rotate)
            {
                isSettingCenter = true;
                MessageBox.Show("Кликните на точку для установки центра поворота.");
            }
            else
            {
                isSettingCenter = false;
            }
            currentTransform = transform;
            UpdateStatusBar();
        }
        private void SetCurrentOperation(Operation operation)
        {
            currentOperation = operation;
            currentTool = Tool.None;

            if (selectedShape != null)
            {
                firstShape = selectedShape;
                XA.Clear();
                GetBoundaryPoints(firstShape, XA);
                MessageBox.Show("Выберите вторую фигуру");
            }
            else
            {
                MessageBox.Show("Сначала выберите первую фигуру");
            }
        }
        private void DeleteSelected()
        {
            currentTool = Tool.None;
            if (selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                drawingArea.Invalidate();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteSelected();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateStatusBar()
        {
            var toolLabel = (ToolStripStatusLabel)statusStrip1.Items[2];
            toolLabel.Text = $"Инструмент: {currentTool}";
        }

        private void SetupStatusBar()
        {
            var positionLabel = new ToolStripStatusLabel("Позиция: 0, 0");
            var toolLabel = new ToolStripStatusLabel("Инструмент: нет");

            statusStrip1.Items.AddRange(new ToolStripItem[] {
            positionLabel,
            new ToolStripStatusLabel(" | "),
            toolLabel
        });
        }

        private double CalculateAngle(Point center, Point point)
        {
            return Math.Atan2(point.Y - center.Y, point.X - center.X) * 180 / Math.PI;
        }

        private void RotateShape(Shape shape, Point center, float angle)
        {
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, center);
            shape.Transform(matrix);
            drawingArea.Invalidate();
        }
        private void MirrorShapeOverLine(Shape shape, Point p1, Point p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float lengthSquared = dx * dx + dy * dy;

            Matrix matrix = new Matrix(
                (dx * dx - dy * dy) / lengthSquared, 2 * dx * dy / lengthSquared,
                2 * dx * dy / lengthSquared, (dy * dy - dx * dx) / lengthSquared,
                0, 0
            );

            matrix.Translate(-p1.X, -p1.Y, MatrixOrder.Prepend);
            shape.Transform(matrix);
            matrix.Reset();
            matrix.Translate(p1.X, p1.Y, MatrixOrder.Append);
            shape.Transform(matrix);

            drawingArea.Invalidate();
        }

        private void MirrorShapeVertical(Shape shape, float x)
        {
            Matrix matrix = new Matrix(-1, 0, 0, 1, 2 * x, 0);
            shape.Transform(matrix);
            drawingArea.Invalidate();
        }


        public class workArray
        {
            public int X;
            public int dQ;

            public workArray(int X, int dQ)
            {
                this.X = X;
                this.dQ = dQ;
            }
        }

        private void DrawResult()
        {
            List<int> Xa = new List<int>();
            List<int> Xb = new List<int>();
            int na = 0, nb = 0, y = 0;

            // Сортируем точки
            BubbleSort(XA);
            BubbleSort(XB);

            while (na < XA.Count || nb < XB.Count)
            {
                Xa.Clear();
                Xb.Clear();

                if (na < XA.Count && nb < XB.Count && XA[na].Y < XB[nb].Y)
                {
                    y = XA[na].Y;
                    while (na < XA.Count && XA[na].Y == y)
                    {
                        Xa.Add(XA[na].X);
                        na++;
                    }
                }
                else if (na < XA.Count && nb < XB.Count && XA[na].Y > XB[nb].Y)
                {
                    y = XB[nb].Y;
                    while (nb < XB.Count && XB[nb].Y == y)
                    {
                        Xb.Add(XB[nb].X);
                        nb++;
                    }
                }
                else
                {
                    if (na < XA.Count)
                    {
                        y = XA[na].Y;
                        while (na < XA.Count && XA[na].Y == y)
                        {
                            Xa.Add(XA[na].X);
                            na++;
                        }
                    }
                    if (nb < XB.Count)
                    {
                        y = XB[nb].Y;
                        while (nb < XB.Count && XB[nb].Y == y)
                        {
                            Xb.Add(XB[nb].X);
                            nb++;
                        }
                    }
                }

                if (Xa.Count > 0 || Xb.Count > 0)
                {
                    Xa.Sort();
                    Xb.Sort();
                    TMO(Xa, Xb, y);
                }
            }
        }

        private void TMO(List<int> Xa, List<int> Xb, int y)
        {
            List<workArray> M = new List<workArray>();
            var resultFigure = new TMOPolygon(new Pen(currentOperation == Operation.Union ? Color.Green : Color.Blue),
                                             currentOperation == Operation.Union ? Color.LightGreen : Color.LightBlue);

            for (int i = 0; i < Xa.Count - 1; i += 2)
            {
                M.Add(new workArray(Xa[i], 2));
                M.Add(new workArray(Xa[i + 1], -2));
            }

            for (int i = 0; i < Xb.Count - 1; i += 2)
            {
                M.Add(new workArray(Xb[i], 1));
                M.Add(new workArray(Xb[i + 1], -1));
            }

            BubbleSort(M);
            int Q = 0;

            for (int i = 0; i < M.Count - 1; i++)
            {
                Q += M[i].dQ;
                if ((currentOperation == Operation.Union && Q >= 1) ||
                    (currentOperation == Operation.SymmetticDiffernt && (Q == 1 || Q == 2)))
                {
                    resultFigure.AddPoint(new Point(M[i].X, y));
                    resultFigure.AddPoint(new Point(M[i + 1].X, y));
                }
            }

            if (resultFigure.GetPoints().Length >= 2)
            {
                shapes.Add(resultFigure);
                drawingArea.Invalidate();
            }
        }

        private static void BubbleSort(List<Point> M)
        {
            for (int i = 0; i < M.Count; i++)
            {
                for (int j = 0; j < M.Count - 1; j++)
                {
                    if (M[j].Y > M[j + 1].Y)
                    {
                        Point t = M[j + 1];
                        M[j + 1] = M[j];
                        M[j] = t;
                    }
                    else if (M[j].Y == M[j + 1].Y)
                    {
                        if (M[j].X > M[j + 1].X)
                        {
                            Point t = M[j + 1];
                            M[j + 1] = M[j];
                            M[j] = t;
                        }
                    }
                }
            }
        }

        private static void BubbleSort(List<workArray> M)
        {
            for (int i = 0; i < M.Count; i++)
            {
                for (int j = 0; j < M.Count - 1; j++)
                {
                    if (M[j].X > M[j + 1].X)
                    {
                        workArray t = M[j + 1];
                        M[j + 1] = M[j];
                        M[j] = t;
                    }
                }
            }
        }
    }
}