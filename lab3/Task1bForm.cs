using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3
{
    public partial class Task1bForm : Form
    {
        private PictureBox pictureBox1;
        private Graphics g;
        private Pen pen;
        private Bitmap bmpFill;
        private Color areaToFillColor;

        private Point old;
        private bool drawing = false;
        public Task1bForm()
        {
            InitializeComponent();
            this.Width = 800;
            this.Height = 600;

            
            pictureBox1 = new PictureBox();
            pictureBox1.Left = 10;
            pictureBox1.Top = 10;
            pictureBox1.Width = 760;
            pictureBox1.Height = 480;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.BackColor = Color.White;
            this.Controls.Add(pictureBox1);

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);

            // события мыши
            pictureBox1.MouseClick += pictureBox1_MouseClick;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;

            pen = new Pen(Color.Black, 1);

            
            Button btnLoadTexture = new Button();
            btnLoadTexture.Text = "Загрузить текстуру";
            btnLoadTexture.AutoSize = true;
            btnLoadTexture.Left = 10;
            btnLoadTexture.Top = pictureBox1.Bottom + 10;
            btnLoadTexture.Click += buttonLoadTexture_Click;
            this.Controls.Add(btnLoadTexture);

            
            Button btnClear = new Button();
            btnClear.Text = "Очистить";
            btnClear.AutoSize = true;
            btnClear.Left = btnLoadTexture.Right + 10;
            btnClear.Top = pictureBox1.Bottom + 10;
            btnClear.Click += buttonClear_Click;
            this.Controls.Add(btnClear);
        }

        private void buttonLoadTexture_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bmpFill = new Bitmap(Image.FromFile(ofd.FileName));
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            old = e.Location;
            drawing = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                g.DrawLine(pen, old.X, old.Y, e.X, e.Y);
                old = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            drawing = false;
        }

        
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (bmpFill == null) return;

            Bitmap bitmap = (Bitmap)pictureBox1.Image;
            areaToFillColor = bitmap.GetPixel(e.X, e.Y);

            FillPicHelp(e.X, e.Y, bmpFill.Width / 2, bmpFill.Height / 2);
        }

        enum Direction { left, right }

        private (int x, int px) CopyLine(int x, int y, int px, int py, Direction d)
        {
            Bitmap bitmap = (Bitmap)pictureBox1.Image;

            while (x > 0 && x < bitmap.Width)
            {
                Color current = bitmap.GetPixel(x, y);
                if (current != areaToFillColor || current == pen.Color)
                    break;

                if (px < 0) px += bmpFill.Width;
                else if (px >= bmpFill.Width) px %= bmpFill.Width;

                Color c = bmpFill.GetPixel(px, py);
                bitmap.SetPixel(x, y, c);

                if (d == Direction.right)
                {
                    x++; px++;
                }
                else
                {
                    x--; px--;
                }
            }
            pictureBox1.Invalidate();
            return (x, px);
        }

        private void FillPicHelp(int x, int y, int px, int py)
        {
            Bitmap bitmap = (Bitmap)pictureBox1.Image;

            if (bitmap.GetPixel(x, y) != areaToFillColor || bitmap.GetPixel(x, y) == pen.Color)
                return;

            if (py < 0) py += bmpFill.Height;
            else if (py >= bmpFill.Height) py %= bmpFill.Height;

            (int x_left, int px_left) = CopyLine(x, y, px, py, Direction.left);
            (int x_right, _) = CopyLine(x + 1, y, px + 1, py, Direction.right);

            if (y + 1 < bitmap.Height)
                for (int i = x_left + 1, j = px_left + 1; i < x_right; ++i, ++j)
                    FillPicHelp(i, y + 1, j, py + 1);

            if (y - 1 > 0)
                for (int i = x_left + 1, j = px_left + 1; i < x_right; ++i, ++j)
                    FillPicHelp(i, y - 1, j, py - 1);

            pictureBox1.Invalidate();
        }
        private void Task1bForm_Load(object sender, EventArgs e)
        {

        }
    }
}
