using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace lab3
{
    public partial class Task1cForm : Form
    {
        private Bitmap originalImage;
        private Bitmap displayImage;
        private List<Point> boundaryPoints = new List<Point>();
        private List<Point> innerBoundaryPoints = new List<Point>();
        private Color boundaryColor = Color.Red;
        private Color innerBoundaryColor = Color.Blue;

        public Task1cForm()
        {
            InitializeComponent();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    originalImage = new Bitmap(openFileDialog.FileName);
                    displayImage = new Bitmap(originalImage);
                    pictureBox.Image = displayImage;
                    boundaryPoints.Clear();
                    innerBoundaryPoints.Clear();
                    lblStatus.Text = "Изображение загружено. Кликните внутри области.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки изображения: " + ex.Message);
                }
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (originalImage == null) return;

            if (e.Button == MouseButtons.Left)
            {
                // Находим внешнюю границу
                FindExternalBoundary(e.X, e.Y);

                // Сканируем внутреннюю область для поиска отверстий
                ScanForInnerBoundaries();

                // Рисуем все найденные границы
                DrawAllBoundaries();

                pictureBox.Refresh();
                lblStatus.Text = $"Внешних точек: {boundaryPoints.Count}, внутренних: {innerBoundaryPoints.Count}";
            }
        }

        // Алгоритм нахождения внешней границы (трассировка контура)
        private void FindExternalBoundary(int startX, int startY)
        {
            boundaryPoints.Clear();
            innerBoundaryPoints.Clear();

            if (startX < 0 || startX >= originalImage.Width ||
                startY < 0 || startY >= originalImage.Height)
                return;

            // Определяем цвет фона и объекта
            Color backgroundColor = originalImage.GetPixel(0, 0);
            Color objectColor = originalImage.GetPixel(startX, startY);

            // Если кликнули по фону, ищем ближайший объект
            if (ColorsAreSimilar(objectColor, backgroundColor))
            {
                objectColor = FindObjectColor(startX, startY, backgroundColor);
            }

            // Находим стартовую точку на границе
            Point startPoint = FindBoundaryStartPoint(startX, startY, backgroundColor, objectColor);
            if (startPoint.X == -1) return;

            // Алгоритм трассировки границы (Moore-Neighbor)
            Point current = startPoint;
            Point[] directions = {
                new Point(1, 0),   // вправо
                new Point(1, 1),   // вправо-вниз
                new Point(0, 1),   // вниз
                new Point(-1, 1),  // влево-вниз
                new Point(-1, 0),  // влево
                new Point(-1, -1), // влево-вверх
                new Point(0, -1),  // вверх
                new Point(1, -1)   // вправо-вверх
            };

            int startDir = 0;
            int currentDir = startDir;

            do
            {
                boundaryPoints.Add(current);

                // Ищем следующую граничную точку
                bool found = false;
                for (int i = 0; i < 8; i++)
                {
                    int checkDir = (currentDir + 6 + i) % 8; // Начинаем поиск с левого соседа
                    Point dir = directions[checkDir];
                    Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);

                    if (neighbor.X >= 0 && neighbor.X < originalImage.Width &&
                        neighbor.Y >= 0 && neighbor.Y < originalImage.Height)
                    {
                        Color neighborColor = originalImage.GetPixel(neighbor.X, neighbor.Y);
                        if (!ColorsAreSimilar(neighborColor, backgroundColor))
                        {
                            current = neighbor;
                            currentDir = checkDir;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found || boundaryPoints.Count > 10000) break;

            } while (current != startPoint);
        }

        // Сканирование внутренней области для поиска отверстий
        private void ScanForInnerBoundaries()
        {
            if (boundaryPoints.Count == 0) return;

            // Находим bounding box области
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (Point p in boundaryPoints)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            // Сканируем построчно
            for (int y = minY + 1; y < maxY; y++)
            {
                List<int> intersections = new List<int>();

                // Находим пересечения с границей для текущей строки
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsBoundaryPoint(x, y))
                    {
                        intersections.Add(x);
                    }
                }

                // Обрабатываем пары пересечений
                for (int i = 0; i < intersections.Count - 1; i += 2)
                {
                    int left = intersections[i];
                    int right = intersections[i + 1];

                    // Сканируем отрезок между пересечениями
                    ScanSegmentForHoles(left + 1, right - 1, y);
                }
            }
        }

        // Сканирование отрезка для поиска отверстий
        private void ScanSegmentForHoles(int startX, int endX, int y)
        {
            Color backgroundColor = originalImage.GetPixel(0, 0);

            for (int x = startX; x <= endX; x++)
            {
                // Если нашли точку, которая не является фоном и не является уже известной границей
                if (!ColorsAreSimilar(originalImage.GetPixel(x, y), backgroundColor) &&
                    !IsBoundaryPoint(x, y) && !IsInnerBoundaryPoint(x, y))
                {
                    // Нашли потенциальное отверстие - трассируем его границу
                    TraceInnerBoundary(x, y);
                }
            }
        }

        // Трассировка границы отверстия
        private void TraceInnerBoundary(int startX, int startY)
        {
            Color backgroundColor = originalImage.GetPixel(0, 0);
            Color objectColor = originalImage.GetPixel(startX, startY);

            // Находим стартовую точку на границе отверстия
            Point startPoint = FindInnerBoundaryStartPoint(startX, startY, backgroundColor, objectColor);
            if (startPoint.X == -1) return;

            List<Point> holeBoundary = new List<Point>();
            Point current = startPoint;
            Point[] directions = {
                new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(-1, 1),
                new Point(-1, 0), new Point(-1, -1), new Point(0, -1), new Point(1, -1)
            };

            int currentDir = 0;

            do
            {
                holeBoundary.Add(current);

                // Ищем следующую граничную точку
                bool found = false;
                for (int i = 0; i < 8; i++)
                {
                    int checkDir = (currentDir + 6 + i) % 8;
                    Point dir = directions[checkDir];
                    Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);

                    if (neighbor.X >= 0 && neighbor.X < originalImage.Width &&
                        neighbor.Y >= 0 && neighbor.Y < originalImage.Height)
                    {
                        Color neighborColor = originalImage.GetPixel(neighbor.X, neighbor.Y);
                        if (!ColorsAreSimilar(neighborColor, backgroundColor))
                        {
                            current = neighbor;
                            currentDir = checkDir;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found || holeBoundary.Count > 5000) break;

            } while (current != startPoint && holeBoundary.Count < 5000);

            // Добавляем точки отверстия в общий список
            innerBoundaryPoints.AddRange(holeBoundary);
        }

        // Вспомогательные методы
        private Point FindBoundaryStartPoint(int x, int y, Color backgroundColor, Color objectColor)
        {
            // Ищем первый пиксель границы в окрестности
            for (int i = Math.Max(0, x - 5); i <= Math.Min(originalImage.Width - 1, x + 5); i++)
            {
                for (int j = Math.Max(0, y - 5); j <= Math.Min(originalImage.Height - 1, y + 5); j++)
                {
                    Color current = originalImage.GetPixel(i, j);
                    Color backgroundNeighbor = originalImage.GetPixel(Math.Max(0, i - 1), j);

                    if (!ColorsAreSimilar(current, backgroundColor) &&
                        ColorsAreSimilar(backgroundNeighbor, backgroundColor))
                    {
                        return new Point(i, j);
                    }
                }
            }
            return new Point(-1, -1);
        }

        private Point FindInnerBoundaryStartPoint(int x, int y, Color backgroundColor, Color objectColor)
        {
            // Для внутренней границы ищем точку, соседнюю с фоном
            Point[] neighbors = {
                new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)
            };

            foreach (Point dir in neighbors)
            {
                Point neighbor = new Point(x + dir.X, y + dir.Y);
                if (neighbor.X >= 0 && neighbor.X < originalImage.Width &&
                    neighbor.Y >= 0 && neighbor.Y < originalImage.Height)
                {
                    if (ColorsAreSimilar(originalImage.GetPixel(neighbor.X, neighbor.Y), backgroundColor))
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(-1, -1);
        }

        private Color FindObjectColor(int x, int y, Color backgroundColor)
        {
            // Ищем цвет объекта в окрестности
            for (int i = Math.Max(0, x - 10); i <= Math.Min(originalImage.Width - 1, x + 10); i++)
            {
                for (int j = Math.Max(0, y - 10); j <= Math.Min(originalImage.Height - 1, y + 10); j++)
                {
                    Color color = originalImage.GetPixel(i, j);
                    if (!ColorsAreSimilar(color, backgroundColor))
                    {
                        return color;
                    }
                }
            }
            return backgroundColor;
        }

        private bool IsBoundaryPoint(int x, int y)
        {
            return boundaryPoints.Contains(new Point(x, y));
        }

        private bool IsInnerBoundaryPoint(int x, int y)
        {
            return innerBoundaryPoints.Contains(new Point(x, y));
        }

        private bool ColorsAreSimilar(Color c1, Color c2, int tolerance = 50)
        {
            return Math.Abs(c1.R - c2.R) < tolerance &&
                   Math.Abs(c1.G - c2.G) < tolerance &&
                   Math.Abs(c1.B - c2.B) < tolerance;
        }

        // Рисование всех границ
        private void DrawAllBoundaries()
        {
            displayImage = new Bitmap(originalImage);

            using (Graphics g = Graphics.FromImage(displayImage))
            {
                // Рисуем внешнюю границу красным
                if (boundaryPoints.Count > 1)
                {
                    Pen externalPen = new Pen(boundaryColor, 2);
                    g.DrawLines(externalPen, boundaryPoints.ToArray());
                }

                // Рисуем внутренние границы синим
                if (innerBoundaryPoints.Count > 0)
                {
                    Pen internalPen = new Pen(innerBoundaryColor, 2);

                    // Группируем точки внутренних границ по отверстиям
                    List<List<Point>> holes = GroupInnerBoundaries();
                    foreach (var hole in holes)
                    {
                        if (hole.Count > 1)
                        {
                            g.DrawLines(internalPen, hole.ToArray());
                        }
                    }
                }
            }

            pictureBox.Image = displayImage;
        }

        private List<List<Point>> GroupInnerBoundaries()
        {
            List<List<Point>> holes = new List<List<Point>>();
            HashSet<Point> processed = new HashSet<Point>();

            foreach (Point point in innerBoundaryPoints)
            {
                if (!processed.Contains(point))
                {
                    List<Point> hole = new List<Point>();
                    FloodGroup(point, hole, processed);
                    holes.Add(hole);
                }
            }

            return holes;
        }

        private void FloodGroup(Point start, List<Point> hole, HashSet<Point> processed)
        {
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            processed.Add(start);

            Point[] neighbors = {
                new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)
            };

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                hole.Add(current);

                foreach (Point dir in neighbors)
                {
                    Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);
                    if (innerBoundaryPoints.Contains(neighbor) && !processed.Contains(neighbor))
                    {
                        processed.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private void btnClearBoundary_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                displayImage = new Bitmap(originalImage);
                pictureBox.Image = displayImage;
                boundaryPoints.Clear();
                innerBoundaryPoints.Clear();
                lblStatus.Text = "Границы очищены";
                pictureBox.Refresh();
            }
        }

        private void btnSaveResult_Click(object sender, EventArgs e)
        {
            if (displayImage != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    displayImage.Save(saveFileDialog.FileName);
                    MessageBox.Show("Изображение сохранено!");
                }
            }
        }
    }
}