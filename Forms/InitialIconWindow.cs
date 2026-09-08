using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TatehamaCTCPClient.Forms {
    public partial class InitialIconWindow : Form {

        private readonly Image iconImage;

        public InitialIconWindow(Icon? icon) {
            InitializeComponent();
            Icon = icon;
            var iconImage = Assembly.GetExecutingAssembly().GetManifestResourceStream("TatehamaCTCPClient.TatehamaCTCPIcon.png");
            if (iconImage != null) {
                this.iconImage = new Bitmap(iconImage);
            }
            else {
                this.iconImage = new Bitmap(256, 256);
            }
            var bitmap = new Bitmap(256, 256);
            using var g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.Transparent, new Rectangle(0, 0, 256, 256));
            g.DrawImage(this.iconImage, new Rectangle(0, 0, 256, 256), 0, 0, 512, 512, GraphicsUnit.Pixel);
            pictureBox1.Image = bitmap;
            pictureBox1.Size = new Size(bitmap.Width, bitmap.Height);
        }


        private async void InitialIconWindow_Load(object? sender, EventArgs? e) {
            try {
                while (true) {
                    var timer = Task.Delay(2000);
                    await timer;
                    Close();
                }
            }
            catch (ObjectDisposedException) {
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            Close();
        }

        private void InitialIconWindow_KeyDown(object sender, KeyEventArgs e) {
            Close();
        }
    }
}
