using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace lab3
{
    public partial class Task1cForm : Form
    {
        private Bitmap originalImage;
        private Bitmap displayImage;
        private List<Point> boundaryPoints = new List<Point>();
        private Color fillColor = Color.Red;
        private Color boundaryColor = Color.Blue;
        private int tolerance = 50;
        private bool[,] visitedMap;
        private bool[,] filledMap;

        public Task1cForm()
        {
            InitializeComponent();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    originalImage = new Bitmap(openFileDialog.FileName);
                    displayImage = new Bitmap(originalImage);

                    // Устанавливаем SizeMode в Normal для точного соответствия координат
                    pictureBox.SizeMode = PictureBoxSizeMode.Normal;
                    pictureBox.Image = displayImage;

                    // Масштабируем PictureBox под размер изображения
                    pictureBox.Size = new Size(originalImage.Width, originalImage.Height);

                    // Инициализируем карты
                    visitedMap = new bool[originalImage.Width, originalImage.Height];
                    filledMap = new bool[originalImage.Width, originalImage.Height];

                    boundaryPoints.Clear();
                    lblStatus.Text = "Изображение загружено. Кликните по области для выделения.";
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
                // Очищаем предыдущее выделение
                ClearPreviousSelection();

                // Используем прямые координаты мыши (без масштабирования)
                int x = e.X;
                int y = e.Y;

                if (x >= 0 && x < originalImage.Width &&
                    y >= 0 && y < originalImage.Height)
                {
                    // Упрощенная заливка связанной области
                    SimpleRegionFill(x, y);

                    pictureBox.Refresh();
                    lblStatus.Text = $"Выделение завершено. Точек границы: {boundaryPoints.Count}";
                }
            }
        }

        // Очистка предыдущего выделения
        private void ClearPreviousSelection()
        {
            if (originalImage != null)
            {
                // Восстанавливаем оригинальное изображение
                displayImage = new Bitmap(originalImage);
                pictureBox.Image = displayImage;

                // Очищаем списки
                boundaryPoints.Clear();

                // Сбрасываем карты
                if (visitedMap != null)
                    Array.Clear(visitedMap, 0, visitedMap.Length);
                if (filledMap != null)
                    Array.Clear(filledMap, 0, filledMap.Length);
            }
        }

        // Упрощенная заливка связанной области
        private void SimpleRegionFill(int startX, int startY)
        {
            if (!IsValidPoint(startX, startY)) return;

            // Получаем целевой цвет
            Color targetColor = originalImage.GetPixel(startX, startY);

            // Сбрасываем карты
            Array.Clear(visitedMap, 0, visitedMap.Length);
            Array.Clear(filledMap, 0, filledMap.Length);
            boundaryPoints.Clear();

            // Заливка с использованием стека
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(startX, startY));
            visitedMap[startX, startY] = true;

            // Направления для 4-связности
            Point[] directions = {
                new Point(0, -1),  // вверх
                new Point(1, 0),   // вправо
                new Point(0, 1),   // вниз
                new Point(-1, 0)   // влево
            };

            int filledCount = 0;
            int maxPixels = originalImage.Width * originalImage.Height;

            // Фаза 1: Заливка области
            while (stack.Count > 0 && filledCount < maxPixels)
            {
                Point current = stack.Pop();

                if (filledMap[current.X, current.Y])
                    continue;

                filledMap[current.X, current.Y] = true;
                filledCount++;

                // Закрашиваем пиксель на displayImage
                displayImage.SetPixel(current.X, current.Y, fillColor);

                // Проверяем соседей
                foreach (Point dir in directions)
                {
                    Point neighbor = new Point(current.X + dir.X, current.Y + dir.Y);

                    if (IsValidPoint(neighbor) &&
                        !visitedMap[neighbor.X, neighbor.Y] &&
                        ColorsAreSimilar(originalImage.GetPixel(neighbor.X, neighbor.Y), targetColor, tolerance))
                    {
                        visitedMap[neighbor.X, neighbor.Y] = true;
                        stack.Push(neighbor);
                    }
                }
            }

            // Фаза 2: Нахождение границы
            FindBoundarySimple();
        }

        // Упрощенное нахождение границы
        private void FindBoundarySimple()
        {
            Point[] directions = {
                new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0)
            };

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    if (filledMap[x, y])
                    {
                        // Проверяем, является ли пиксель граничным
                        bool isBoundary = false;

                        foreach (Point dir in directions)
                        {
                            int nx = x + dir.X;
                            int ny = y + dir.Y;

                            if (!IsValidPoint(nx, ny) || !filledMap[nx, ny])
                            {
                                isBoundary = true;
                                break;
                            }
                        }

                        if (isBoundary)
                        {
                            boundaryPoints.Add(new Point(x, y));
                            displayImage.SetPixel(x, y, boundaryColor);
                        }
                    }
                }
            }
        }

        private bool IsValidPoint(Point point)
        {
            return point.X >= 0 && point.X < originalImage.Width &&
                   point.Y >= 0 && point.Y < originalImage.Height;
        }

        private bool IsValidPoint(int x, int y)
        {
            return x >= 0 && x < originalImage.Width && y >= 0 && y < originalImage.Height;
        }

        private bool ColorsAreSimilar(Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance;
        }

        private void ToleranceTrackBar_ValueChanged(object sender, EventArgs e)
        {
            tolerance = toleranceTrackBar.Value;
            lblTolerance.Text = $"Допуск: {tolerance}";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearPreviousSelection();
            lblStatus.Text = "Выделение очищено";
            pictureBox.Refresh();
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