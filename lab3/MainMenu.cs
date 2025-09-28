using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab3
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Лабораторная #3";
            Size = new Size(400, 500);
            StartPosition = FormStartPosition.CenterScreen;

            var btnTask1a = new Button { Text = "Задание 1а", Location = new Point(100, 50), Size = new Size(200, 50) };
            var btnTask1b = new Button { Text = "Задание 1б", Location = new Point(100, 120), Size = new Size(200, 50) };
            var btnTask1c = new Button { Text = "Задание 1в", Location = new Point(100, 190), Size = new Size(200, 50) };
            var btnTask2 = new Button { Text = "Задание 2", Location = new Point(100, 260), Size = new Size(200, 50) };
            var btnTask3 = new Button { Text = "Задание 3", Location = new Point(100, 330), Size = new Size(200, 50) };

            btnTask1a.Click += (s, e) => new Task1aForm().Show();
            btnTask1b.Click += (s, e) => new Task1bForm().Show();
            btnTask1c.Click += (s, e) => new Task1cForm().Show();
            btnTask2.Click += (s, e) => new Task2Form().Show();
            btnTask3.Click += (s, e) => new Task3Form().Show();

            Controls.AddRange(new Control[] { btnTask1a, btnTask1b, btnTask1c, btnTask2, btnTask3 });
        }
    }
}