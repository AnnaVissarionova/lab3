using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab3
{
    public partial class Task1aForm : Form
    {
        private Bitmap canvas;
        private Color fillColor = Color.Red;
        private Color targetColor = Color.White;
        private bool drawMode = false;

        public Task1aForm()
        {
            InitializeComponent();
            InitializeCanvas();
        }

        private void InitializeCanvas()
        {
            canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }
            pictureBox.Image = canvas;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (drawMode)
                {
                    // Режим рисования фигуры
                    DrawRandomShape(e.X, e.Y);
                }
                else
                {
                    // Режим заливки - получаем цвет области, по которой кликнули
                    Color clickedColor = canvas.GetPixel(e.X, e.Y);

                    // Если кликнули по цвету, который уже совпадает с fillColor - ничего не делаем
                    if (clickedColor.ToArgb() != fillColor.ToArgb())
                    {
                        // Устанавливаем целевой цвет как цвет области, по которой кликнули
                        targetColor = clickedColor;
                        FloodFill(e.X, e.Y);
                    }
                }
                pictureBox.Refresh();
            }
        }

        // Рекурсивный алгоритм заливки
        private void FloodFill(int startX, int startY)
        {
            if (startX < 0 || startX >= canvas.Width || startY < 0 || startY >= canvas.Height)
                return;

            // Получаем цвет текущего пикселя
            Color currentColor = canvas.GetPixel(startX, startY);

            // Если цвет не совпадает с целевым ИЛИ уже залит новым цветом - выходим
            if (currentColor.ToArgb() != targetColor.ToArgb() ||
                currentColor.ToArgb() == fillColor.ToArgb())
                return;

            // Находим левую границу
            int left = startX;
            while (left > 0 && canvas.GetPixel(left - 1, startY).ToArgb() == targetColor.ToArgb())
            {
                left--;
            }

            // Находим правую границу
            int right = startX;
            while (right < canvas.Width - 1 && canvas.GetPixel(right + 1, startY).ToArgb() == targetColor.ToArgb())
            {
                right++;
            }

            // Закрашиваем линию от left до right
            for (int x = left; x <= right; x++)
            {
                canvas.SetPixel(x, startY, fillColor);
            }

            // Рекурсивно обрабатываем строки выше и ниже
            for (int x = left; x <= right; x++)
            {
                if (startY > 0)
                {
                    Color topColor = canvas.GetPixel(x, startY - 1);
                    if (topColor.ToArgb() == targetColor.ToArgb() && topColor.ToArgb() != fillColor.ToArgb())
                        FloodFill(x, startY - 1);
                }

                if (startY < canvas.Height - 1)
                {
                    Color bottomColor = canvas.GetPixel(x, startY + 1);
                    if (bottomColor.ToArgb() == targetColor.ToArgb() && bottomColor.ToArgb() != fillColor.ToArgb())
                        FloodFill(x, startY + 1);
                }
            }
        }

        // Рисование случайной фигуры
        private void DrawRandomShape(int x, int y)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                Pen pen = new Pen(Color.Black, 2);
                Random rand = new Random();

                // Случайный выбор фигуры
                int shapeType = rand.Next(1, 4);

                switch (shapeType)
                {
                    case 1: // Прямоугольник
                        g.DrawRectangle(pen, x - 40, y - 30, 80, 60);
                        break;
                    case 2: // Круг
                        g.DrawEllipse(pen, x - 35, y - 35, 70, 70);
                        break;
                    case 3: // Треугольник
                        Point[] trianglePoints = {
                            new Point(x, y - 40),
                            new Point(x - 35, y + 20),
                            new Point(x + 35, y + 20)
                        };
                        g.DrawPolygon(pen, trianglePoints);
                        break;
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            InitializeCanvas();
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                fillColor = colorDialog.Color;
                btnColor.BackColor = fillColor;
            }
        }

        private void btnAddShape_Click(object sender, EventArgs e)
        {
            // Переключаем режим рисования фигур
            drawMode = !drawMode;
            if (drawMode)
            {
                btnAddShape.Text = "Режим: Рисование фигур";
                btnAddShape.BackColor = Color.LightGreen;
            }
            else
            {
                btnAddShape.Text = "Режим: Заливка";
                btnAddShape.BackColor = SystemColors.Control;
            }
        }

        private void Task1aForm_Load(object sender, EventArgs e)
        {
            btnColor.BackColor = fillColor;
        }
    }
}