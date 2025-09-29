using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab3
{
    public class Task2Form : Form
    {
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing;
        private bool useWuAlgorithm = false;
        private bool useThickWu = false; 
        private int lineThickness = 7;

        public Task2Form()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Задание 2 - Алгоритмы рисования линий";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            DoubleBuffered = true;

            // Кнопка переключения алгоритмов
            var switchButton = new Button
            {
                Text = "Брезенхем",
                Location = new Point(10, 10),
                Size = new Size(120, 30),
                BackColor = Color.LightBlue
            };
            switchButton.Click += SwitchButton_Click;

            // Кнопка переключения режима Ву
            /* var wuModeButton = new Button
             {
                 Text = "Ву: Толстый",
                 Location = new Point(140, 10),
                 Size = new Size(120, 30),
                 BackColor = Color.LightYellow,
                 Enabled = false // Изначально выключена
             };
             wuModeButton.Click += WuModeButton_Click;
            Controls.Add(wuModeButton);
             */


            // Кнопка уменьшения толщины
            var thicknessDownButton = new Button
            {
                Text = "Тоньше -",
                Location = new Point(140, 10),
                Size = new Size(85, 30),
                BackColor = Color.LightGray
            };
            thicknessDownButton.Click += ThicknessDownButton_Click;

            // Кнопка увеличения толщины
            var thicknessUpButton = new Button
            {
                Text = "Толще +",
                Location = new Point(240, 10),
                Size = new Size(85, 30),
                BackColor = Color.LightGray
            };
            thicknessUpButton.Click += ThicknessUpButton_Click;

            Controls.Add(thicknessDownButton);
            Controls.Add(thicknessUpButton);

            Controls.Add(switchButton);
            

            MouseDown += Task2Form_MouseDown;
            MouseMove += Task2Form_MouseMove;
            MouseUp += Task2Form_MouseUp;
            Paint += Task2Form_Paint;
        }
        private void ThicknessDownButton_Click(object sender, EventArgs e)
        {
            if (lineThickness > 1)
            {
                lineThickness--;
                Invalidate();
            }
        }

        private void ThicknessUpButton_Click(object sender, EventArgs e)
        {
            if (lineThickness < 10)
            {
                lineThickness++;
                Invalidate();
            }
        }
        private void SwitchButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            useWuAlgorithm = !useWuAlgorithm;

           // var wuButton = Controls[1] as Button;

            if (useWuAlgorithm)
            {
                button.Text = "Алгоритм Ву";
                button.BackColor = Color.LightGreen;
                //wuButton.Enabled = true;
            }
            else
            {
                button.Text = "Брезенхем";
                button.BackColor = Color.LightBlue;
               // wuButton.Enabled = false; 
            }

            Invalidate();
        }

        private void WuModeButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            useThickWu = !useThickWu;

            if (useThickWu)
            {
                button.Text = "Ву: Толстый";
                button.BackColor = Color.LightYellow;
            }
            else
            {
                button.Text = "Ву: Тонкий";
                button.BackColor = Color.LightCoral;
            }

            Invalidate();
        }

        private void Task2Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                endPoint = e.Location;
                isDrawing = true;
                Invalidate();
            }
        }

        private void Task2Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                endPoint = e.Location;
                Invalidate();
            }
        }

        private void Task2Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                endPoint = e.Location;
                Invalidate();
            }
        }

        private void Task2Form_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            DrawGrid(g);
            DrawAxes(g);

            if (startPoint != endPoint)
            {
                if (useWuAlgorithm)
                {
                    DrawWuLine(g, startPoint, endPoint);
                }
                else
                {
                    DrawBresenhamLine(g, startPoint, endPoint);
                }
            }

        }

        private void DrawGrid(Graphics g)
        {
            Pen gridPen = new Pen(Color.FromArgb(240, 240, 240), 1);
            int gridSize = 20;

            for (int x = 0; x < ClientSize.Width; x += gridSize)
            {
                g.DrawLine(gridPen, x, 0, x, ClientSize.Height);
            }

            for (int y = 0; y < ClientSize.Height; y += gridSize)
            {
                g.DrawLine(gridPen, 0, y, ClientSize.Width, y);
            }

            gridPen.Dispose();
        }

        private void DrawAxes(Graphics g)
        {
            int centerX = ClientSize.Width / 2;
            int centerY = ClientSize.Height / 2;
            int gridSize = 20;

            Pen axisPen = new Pen(Color.FromArgb(200, 200, 200), 2);
            g.DrawLine(axisPen, 0, centerY, ClientSize.Width, centerY);
            g.DrawLine(axisPen, centerX, 0, centerX, ClientSize.Height);

            Pen tickPen = new Pen(Color.FromArgb(150, 150, 150), 1);
            Font tickFont = new Font("Arial", 8);
            Brush tickBrush = new SolidBrush(Color.Gray);

            for (int x = centerX % gridSize; x < ClientSize.Width; x += gridSize)
            {
                if (x != centerX)
                {
                    g.DrawLine(tickPen, x, centerY - 3, x, centerY + 3);

                    int value = (x - centerX) / gridSize;
                    if (value % 5 == 0 && value != 0)
                    {
                        string label = value.ToString();
                        SizeF textSize = g.MeasureString(label, tickFont);
                        g.DrawString(label, tickFont, tickBrush,
                            x - textSize.Width / 2, centerY + 5);
                    }
                }
            }

            for (int y = centerY % gridSize; y < ClientSize.Height; y += gridSize)
            {
                if (y != centerY)
                {
                    g.DrawLine(tickPen, centerX - 3, y, centerX + 3, y);

                    int value = (centerY - y) / gridSize;
                    if (value % 5 == 0 && value != 0)
                    {
                        string label = value.ToString();
                        SizeF textSize = g.MeasureString(label, tickFont);
                        g.DrawString(label, tickFont, tickBrush,
                            centerX + 5, y - textSize.Height / 2);
                    }
                }
            }

            g.DrawString("0", tickFont, tickBrush, centerX + 2, centerY + 2);
            g.DrawString("X", tickFont, tickBrush, ClientSize.Width - 15, centerY - 15);
            g.DrawString("Y", tickFont, tickBrush, centerX + 5, 5);

            axisPen.Dispose();
            tickPen.Dispose();
            tickFont.Dispose();
            tickBrush.Dispose();
        }

        private void DrawBresenhamLine(Graphics g, Point start, Point end)
        {
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            Brush pixelBrush = new SolidBrush(Color.Red);

            if (dy <= dx)
            {
                if (x0 > x1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }

                int yi = y0 < y1 ? 1 : -1;
                int D = 2 * dy - dx;
                int y = y0;

                for (int x = x0; x <= x1; x++)
                {
                    DrawThickPixel(g, pixelBrush, x, y);
                    if (D > 0)
                    {
                        y += yi;
                        D += 2 * (dy - dx);
                    }
                    else
                    {
                        D += 2 * dy;
                    }
                }
            }
            else
            {
                if (y0 > y1)
                {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }

                int xi = x0 < x1 ? 1 : -1;
                int D = 2 * dx - dy;
                int x = x0;

                for (int y = y0; y <= y1; y++)
                {
                    DrawThickPixel(g, pixelBrush, x, y);
                    if (D > 0)
                    {
                        x += xi;
                        D += 2 * (dx - dy);
                    }
                    else
                    {
                        D += 2 * dx;
                    }
                }
            }

            pixelBrush.Dispose();
        }

        private void DrawWuLine(Graphics g, Point start, Point end)
        {
            if (useThickWu)
            {
                // Режим толстых линий - несколько параллельных
                DrawThickWuLine(g, start, end);
            }
            else
            {
                // Режим тонких линий - одна сглаженная линия
                DrawSingleWuLine(g, start, end);
            }
        }

        private void DrawThickWuLine(Graphics g, Point start, Point end)
        {
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            // Вычисляем нормализованный перпендикулярный вектор для утолщения
            float lineLength = (float)Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2));
            if (lineLength == 0) return;

            float nx = -(y1 - y0) / lineLength; // Нормаль X
            float ny = (x1 - x0) / lineLength;  // Нормаль Y

            // Рисуем несколько параллельных линий Ву для создания толщины
            for (int i = -lineThickness / 2; i <= lineThickness / 2; i++)
            {
                Point offsetStart = new Point(
                    (int)(x0 + nx * i),
                    (int)(y0 + ny * i)
                );
                Point offsetEnd = new Point(
                    (int)(x1 + nx * i),
                    (int)(y1 + ny * i)
                );

                DrawSingleWuLine(g, offsetStart, offsetEnd);
            }
        }

        private void DrawSingleWuLine(Graphics g, Point start, Point end)
        {
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dx == 0 ? 1 : dy / dx;

            float y = y0;
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    PlotWuPixel(g, (int)y, x, 1 - (y - (int)y));
                    PlotWuPixel(g, (int)y + 1, x, y - (int)y);
                }
                else
                {
                    PlotWuPixel(g, x, (int)y, 1 - (y - (int)y));
                    PlotWuPixel(g, x, (int)y + 1, y - (int)y);
                }
                y += gradient;
            }
        }

        private void DrawThickPixel(Graphics g, Brush brush, int x, int y)
        {
            g.FillRectangle(brush, x, y, lineThickness, lineThickness);
        }

        private void PlotWuPixel(Graphics g, int x, int y, float brightness)
        {
            int alpha = (int)(brightness * 200 + 55);
            Color color = Color.FromArgb(alpha, 255, 0, 0);

            using (Brush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, x, y, lineThickness, lineThickness);
            }
        }

        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

 
       
    }
}