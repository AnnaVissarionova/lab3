using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3
{
    public partial class Task3Form : Form
    {
        private bool isDrawing = false;
        private int colorSchemeIndex = 0;
        private readonly Color[][] colorSchemes;
        private Point[] triangleVertices = new Point[3];
        private Color[] vertexColors = new Color[3];
        private int vertexCount = 0;
    

        public Task3Form()
        {
            colorSchemes = new Color[][]
            {
                new Color[] { Color.Red, Color.Green, Color.Blue },
                new Color[] { Color.FromArgb(163, 22, 33), Color.FromArgb(191, 219, 247), Color.FromArgb(5, 60, 94) },
                new Color[] { Color.FromArgb(0, 38, 66), Color.FromArgb(132, 0, 50), Color.FromArgb(229, 149, 0) },
                new Color[] { Color.FromArgb(114, 24, 23), Color.FromArgb(250, 159, 66), Color.FromArgb(43, 65, 98) }
            };
            vertexColors = colorSchemes[0]; 
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Задание 3 - Градиентное окрашивание треугольника";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            DoubleBuffered = true;

            var clearButton = new Button()
            {
                Text = "Очистить",
                Location = new Point(15, 15),
                Size = new Size(120, 40),
                BackColor = Color.PowderBlue, 
                
            };
            clearButton.Click += ClearButton_Click;

            var paletteButton = new Button()
            {
                Text = "Смена палитры",
                Location = new Point(150, 15),
                Size = new Size(140, 40),
                BackColor = Color.LightSteelBlue, 
            };
            paletteButton.Click += PaletteButton_Click;

            Controls.Add(clearButton);
            Controls.Add(paletteButton);

            MouseDown += Task3Form_MouseDown;
            Paint += Task3Form_Paint;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            vertexCount = 0;
            isDrawing = false;
            Invalidate();
        }

        private void PaletteButton_Click(object sender, EventArgs e)
        {
            colorSchemeIndex = (colorSchemeIndex + 1) % colorSchemes.Length;
            vertexColors = colorSchemes[colorSchemeIndex];

            if (isDrawing)
            {
                Invalidate();
            }
        }

        private void Task3Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (vertexCount < 3)
            {
                triangleVertices[vertexCount] = e.Location;
                vertexCount++;

                if (vertexCount == 3)
                {
                    isDrawing = true;
                }

                Invalidate();
            }
        }

        private void Task3Form_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawGrid(g);

            if (vertexCount > 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    DrawDot(g, triangleVertices[i], vertexColors[i]);
                }

                if (isDrawing)
                {
                    DrawGradientTriangle(g, triangleVertices[0], triangleVertices[1], triangleVertices[2],
                                        vertexColors[0], vertexColors[1], vertexColors[2]);

                    // DrawTriangleBorder(g, triangleVertices[0], triangleVertices[1], triangleVertices[2]);
                }
            }

        }

        private void DrawGrid(Graphics g)
        {
            Pen gridPen = new Pen(Color.FromArgb(240, 240, 240), 1);
            int gridSize = 20;

            for (int x = 0; x < ClientSize.Width; x += gridSize) 
                g.DrawLine(gridPen, x, 0, x, ClientSize.Height);

            for (int y = 0; y < ClientSize.Height; y += gridSize)
                g.DrawLine(gridPen, 0, y, ClientSize.Width, y);

            gridPen.Dispose();
        }

        private void DrawDot(Graphics g, Point point, Color color)
        {
            int size = 8;
            using (Brush brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, point.X - size / 2, point.Y - size / 2, size, size);
            }
        }

        private void DrawGradientTriangle(Graphics g, Point A, Point B, Point C, Color colorA, Color colorB, Color colorC)
        {
            int minX = Math.Min(A.X, Math.Min(B.X, C.X));
            int maxX = Math.Max(A.X, Math.Max(B.X, C.X));
            int minY = Math.Min(A.Y, Math.Min(B.Y, C.Y));
            int maxY = Math.Max(A.Y, Math.Max(B.Y, C.Y));
            float PABC = Math.Abs(A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) +C.X * (A.Y - B.Y)) / 2.0f;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Point P = new Point(x, y);

                    float PPBC = Math.Abs(P.X * (B.Y - C.Y) +B.X * (C.Y - P.Y) +C.X * (P.Y - B.Y)) / 2.0f;

                    float PAPC = Math.Abs(A.X * (P.Y - C.Y) +P.X * (C.Y - A.Y) +C.X * (A.Y - P.Y)) / 2.0f;

                    float PABP = Math.Abs(A.X * (B.Y - P.Y) +B.X * (P.Y - A.Y) +P.X * (A.Y - B.Y)) / 2.0f;

                    float a = PPBC / PABC; 
                    float b = PAPC / PABC;   
                    float c = PABP / PABC;  
                    float sum = a + b + c;

                    if (a >= 0 && b >= 0 && c >= 0 && Math.Abs(sum - 1.0f) < 0.01f)
                    {
                        a /= sum;
                        b /= sum;
                        c /= sum;

                        Color pixelColor = GetDotColor(colorA, colorB, colorC, a, b, c);
                        using (Brush brush = new SolidBrush(pixelColor))
                        {
                            g.FillRectangle(brush, x, y, 1, 1);
                        }
                    }
                }
            }
        }
      
        private Color GetDotColor(Color colorA, Color colorB, Color colorC, float a, float b, float c)
        {
            int R = (int)(a * colorA.R + b * colorB.R + c * colorC.R);
            int G = (int)(a * colorA.G + b * colorB.G + c * colorC.G);
            int B = (int)(a * colorA.B + b * colorB.B + c * colorC.B);

            R = Math.Max(0, Math.Min(255, R));
            G = Math.Max(0, Math.Min(255, G));
            B = Math.Max(0, Math.Min(255, B));

            return Color.FromArgb(R,G,B);
        }


        private void DrawTriangleBorder(Graphics g, Point A, Point B, Point C)
        {
            using (Pen borderPen = new Pen(Color.LightSlateGray, 2f))
            {
                g.SmoothingMode = SmoothingMode.Default;
                borderPen.EndCap = LineCap.Round;
                borderPen.StartCap = LineCap.Round;

                g.DrawLine(borderPen, A, B);
                g.DrawLine(borderPen, B, C);
                g.DrawLine(borderPen, C, A);
            }
        }

    }
}
