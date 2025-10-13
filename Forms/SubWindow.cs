﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TatehamaCTCPClient.Manager;

namespace TatehamaCTCPClient.Forms {
    public partial class SubWindow : Form {

        public Point StartLocation {
            get;
            init;
        }

        public Size DisplaySize {
            get;
            init;
        }

        private bool DetectResize {
            get; set;
        } = false;

        private CTCPManager displayManager;

        private Bitmap original = new Bitmap(1, 1);

        private static int counter = 0;

        private Size windowSize;

        /// <summary>
        /// WASDキーなど使用時の移動量
        /// </summary>
        private int scrollDelta = 15;

        public bool OpeningDialog {
            get;
            private set;
        }

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point mouseLoc = Point.Empty;

        public SubWindow(Point location, Size size, CTCPManager displayManager) {
            StartLocation = location;
            DisplaySize = size;
            this.displayManager = displayManager;
            InitializeComponent();

            Text = $"サブモニタ{++counter} | TID - ダイヤ運転会";

            Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
            MinimumSize = new Size(Size.Width - ClientSize.Width + DisplaySize.Width / 2, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height / 2);

            windowSize = Size;

            pictureBox1.Size = size;


            SetMarkupType(displayManager.Window.MarkupType);

            /*flowLayoutPanel1.Location = new Point(flowLayoutPanel1.Location.X - Size.Width + ClientSize.Width + 16, flowLayoutPanel1.Location.Y);*/

            DetectResize = true;

            UpdateImage(displayManager.OriginalBitmap);
        }

        public void UpdateImage(Image image) {
            lock (pictureBox1) {
                var old = pictureBox1.Image;
                var b = new Bitmap(DisplaySize.Width, DisplaySize.Height);
                using var g = Graphics.FromImage(b);
                g.DrawImage(image, new Rectangle(0, 0, DisplaySize.Width, DisplaySize.Height), StartLocation.X, StartLocation.Y, DisplaySize.Width, DisplaySize.Height, GraphicsUnit.Pixel);
                lock (original) {
                    var origOld = original;
                    original = b;
                    origOld.Dispose();
                }
                if (WindowState != FormWindowState.Minimized) {
                    pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                    old?.Dispose();
                }
            }
        }


        private void SubWindow_Closing(object sender, FormClosingEventArgs e) {
            displayManager.RemoveSubWindow(this);
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                lock (pictureBox1.Image) {
                    var size = Size;
                    var dp = e.Location;
                    var point = ConvertPointToOriginal(dp.X, dp.Y);
                    var rate = (pictureBox1.Image.Width + e.Delta * 0.2) / DisplaySize.Width;
                    var width = Size.Width - ClientSize.Width + (int)(DisplaySize.Width * rate);
                    var height = Size.Height - ClientSize.Height + pictureBox1.Location.Y + (int)(DisplaySize.Height * rate);
                    var screenSize = Screen.FromControl(this).Bounds;
                    screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                    if (width <= screenSize.Width && height <= screenSize.Height) {
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                    else if (width > screenSize.Width) {
                        width = screenSize.Width;
                        height = Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height * (screenSize.Width - Size.Width + ClientSize.Width) / DisplaySize.Width;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                    else {
                        height = screenSize.Height;
                        width = Size.Width - ClientSize.Width + DisplaySize.Width * (screenSize.Height - Size.Height + ClientSize.Height - pictureBox1.Location.Y) / DisplaySize.Height;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                }
            }
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                /*if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(e.Location, w.PosX, w.PosY, w.GetSize(), 1) && displayManager.Window.TrainDataDict.TryGetValue(t, out var td)) {
                            td.Markup = !td.Markup;
                            displayManager.Window.UpdateTrainCheck(td);
                            displayManager.Window.ReservedUpdate = true;
                        }
                    }
                }
                else {*/
                    mouseLoc = Cursor.Position;
                /*}*/
            }
        }

        private void labelTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        public void SetTopMost(bool topMost) {
            TopMost = topMost;
            menuItemTopMost.CheckState = topMost ? CheckState.Checked : CheckState.Unchecked;
            labelTopMost.Text = $"最前面：{(topMost ? "ON" : "OFF")}";
            labelTopMost.ForeColor = topMost ? Color.Yellow : Color.Gray;
        }

        private void labelTopMost_Hover(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void labelTopMost_Leave(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (pictureBox1.Width / (double)DisplaySize.Width != 1) {
                DetectResize = false;
                Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
                if (Location.X < 0) {
                    Location = new Point(0, Location.Y);
                }
                lock (pictureBox1) {
                    var old = pictureBox1.Image;
                    pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                    old?.Dispose();
                }


                labelScale.Text = $"Scale：100%";
                labelScale.ForeColor = Color.White;
                labelScale.Cursor = Cursors.Default;
                DetectResize = true;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (!ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                var pos = Cursor.Position;
                Location = new Point(Location.X + pos.X - mouseLoc.X, Location.Y + pos.Y - mouseLoc.Y);
                mouseLoc = pos;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = Point.Empty;
            }
        }

        private void SubWindow_ResizeBegin(object sender, EventArgs e) {
            var screenSize = Screen.FromControl(this).Bounds;
            screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
            var mw = Size.Width - ClientSize.Width + DisplaySize.Width * (screenSize.Height - Size.Height + ClientSize.Height - pictureBox1.Location.Y) / DisplaySize.Height;
            var mh = Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height * (screenSize.Width - Size.Width + ClientSize.Width) / DisplaySize.Width;
            if (screenSize.Height < mh) {
                MaximumSize = new Size(mw, screenSize.Height + Size.Height - ClientSize.Height);
            }
            else {
                MaximumSize = new Size(screenSize.Width + Size.Width - ClientSize.Width, mh);
            }

        }

        private void SubWindow_Resize(object sender, EventArgs e) {
            if (!DetectResize || WindowState == FormWindowState.Minimized) {
                return;
            }
            DetectResize = false;
            if (Size.Width == windowSize.Width && Size.Height != windowSize.Height) {
                Size = new Size(Size.Width - ClientSize.Width + (DisplaySize.Width * pictureBox1.Height / DisplaySize.Height), Size.Height);
            }
            else {
                Size = new Size(Size.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + (DisplaySize.Height * pictureBox1.Width / DisplaySize.Width));
            }
            lock (pictureBox1) {
                var old = pictureBox1.Image;
                pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                old?.Dispose();
                var ratio = pictureBox1.Width * 100 / (double)DisplaySize.Width;
                labelScale.Text = $"Scale：{(int)ratio}%";
                if (ratio == 100) {
                    labelScale.Cursor = Cursors.Default;
                    labelScale.ForeColor = Color.White;
                }
                else {
                    labelScale.Cursor = Cursors.Hand;
                    labelScale.ForeColor = Color.LightGreen;
                }
            }
            DetectResize = true;
        }

        private void SubWindow_ResizeEnd(object sender, EventArgs e) {
            windowSize = Size;
        }

        private void SubWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            if ((mod & Keys.Shift) == Keys.Shift) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            if (e.KeyData == (Keys.C | Keys.Control)) {
                CopyImage();
            }
            if (e.KeyData == Keys.Tab) {
                SetTopMost(!TopMost);
            }

            if (code == Keys.Right || code == Keys.D) {
                Location = new Point(Location.X + scrollDelta * (mod == Keys.Shift ? 1 : 3), Location.Y);
            }
            if (code == Keys.Left || code == Keys.A) {
                Location = new Point(Location.X - scrollDelta * (mod == Keys.Shift ? 1 : 3), Location.Y);
            }
            if (code == Keys.Up || code == Keys.W) {
                Location = new Point(Location.X, Location.Y - scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if (code == Keys.Down || code == Keys.S) {
                Location = new Point(Location.X, Location.Y + scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
        }

        private void SubWindow_KeyUp(object sender, KeyEventArgs e) {
            UpdateMouseCursor();

        }

        public void SetClock(DateTime time) {
            labelClock.Text = time.ToString("H:mm:ss");
        }

        private Point ConvertPointToOriginal(int x, int y) {
            return new Point(StartLocation.X + x * DisplaySize.Width / pictureBox1.Width, StartLocation.Y + y * DisplaySize.Height / pictureBox1.Height);
        }

        private Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        private Point ConvertPointToScreen(int x, int y) {
            return new Point((x - StartLocation.X) * pictureBox1.Width / DisplaySize.Width, (y - StartLocation.Y) * pictureBox1.Height / DisplaySize.Height);
        }

        private Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        private bool IsInArea(Point point, int areaX, int areaY, Size areaSize, int padding = 0) {
            var p = ConvertPointToOriginal(point);
            return p.X >= areaX - padding && p.X < (areaX + areaSize.Width + padding) && p.Y >= areaY - padding && p.Y < (areaY + areaSize.Height + padding);
        }

        private void UpdateMouseCursor() {
            if (ModifierKeys.HasFlag(Keys.Shift)) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            else {
                pictureBox1.Cursor = Cursors.SizeAll;
            }
        }

        public void CopyImage() {
            lock (original) {
                var i = new Bitmap(original.Width, original.Height + 13);
                using (var g = Graphics.FromImage(i)) {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.DrawImage(original, 0, 13);
                    g.DrawString(labelClock.Text, new Font("ＭＳ ゴシック", 9), Brushes.White, original.Width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();
            }
        }

        private void menuItemCopy_Click(object sender, EventArgs e) {
            CopyImage();
        }

        private void menuItemTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void menuItemSilent_Click(object sender, EventArgs e) {
            displayManager.Window.SetSilent(!displayManager.Window.Silent);
        }

        public void SetSilent(bool silent) {
            menuItemSilent.CheckState = silent ? CheckState.Checked : CheckState.Unchecked;
        }

        public void SetWindowName(string name) {
            Text = $"{name} | TID - ダイヤ運転会";
        }

        private void menuItemRename_Click(object sender, EventArgs e) {
            var d = new SubWindowName(this);
            d.TopMost = TopMost;
            OpeningDialog = true;
            d.ShowDialog();
            OpeningDialog = false;
        }

        private void menuItemVersion_Click(object sender, EventArgs e) {
            var form = new VersionWindow();
            form.Icon = Icon;
            var bitmap = Icon != null ? new Icon(Icon, 256, 256).ToBitmap() : new Bitmap(10, 10);
            form.PictureIcon.Image = bitmap;
            form.PictureIcon.Size = new Size(bitmap.Width, bitmap.Height);
            form.LabelVersion.Text = $"TrainCrewTIDWindow\nVer. {ServerAddress.Version.Replace("TrainCrewTIDWindow_", "")}";
            if (TopMost) {
                form.TopMost = true;
            }
            OpeningDialog = true;
            form.ShowDialog();
            OpeningDialog = false;
        }

        private void menuItemMarkupType1_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(0);
        }

        private void menuItemMarkupType2_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(1);
        }

        private void menuItemMarkupType3_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(2);
        }

        public void SetMarkupType(int type) {
            menuItemMarkupType1.CheckState = type == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType2.CheckState = type == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType3.CheckState = type == 2 ? CheckState.Indeterminate : CheckState.Unchecked;
        }
    }
}
