using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Forms;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Manager {
    public class CTCPManager {
        /// <summary>
        /// CTCP画面表示用のPictureBox
        /// </summary>
        private PictureBox pictureBox;

        /// <summary>
        /// CTCPWindowオブジェクト
        /// </summary>
        private CTCPWindow window;


        private readonly List<StationSetting> stationSettings;

        /// <summary>
        /// 起動時背景画像
        /// </summary>
        private Image backgroundDefault;

        /// <summary>
        /// 通常時背景画像
        /// </summary>
        private Image backgroundImage;

        /// <summary>
        /// ボタン画像
        /// </summary>
        private Image buttonsImage;

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        private Bitmap originalBitmap;

        private CharacterSet middleCharSet;

        private CharacterSet smallCharSet;

        private CharacterSet xsmallCharSet;









        private readonly List<SubWindow> subWindows = [];


        public bool Started {
            get;
            private set;
        } = false;

        /// <summary>
        /// TID画面表示用のPictureBox
        /// </summary>
        public PictureBox PictureBox => pictureBox;

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        public Bitmap OriginalBitmap => originalBitmap;

        public ReadOnlyCollection<StationSetting> StationSettings { get; init; }

        public ReadOnlyCollection<SubWindow> SubWindows { get; init; }

        public CTCPWindow Window => window;

        public bool IsActiveForm {
            get {
                var v = false;
                lock (subWindows) {
                    v = subWindows.Any(w => Form.ActiveForm == w || w.OpeningDialog);
                }
                return v;
            }
        }

        public CTCPManager(PictureBox pictureBox, CTCPWindow window) {
            this.pictureBox = pictureBox;
            this.window = window;

            stationSettings = LoadStationSetting("station.tsv");

            StationSettings = stationSettings.AsReadOnly();
            SubWindows = subWindows.AsReadOnly();

            backgroundDefault = Image.FromFile(".\\png\\Background-1.png");
            backgroundImage = Image.FromFile(".\\png\\Background.png");
            buttonsImage = Image.FromFile(".\\png\\buttons.png");

            middleCharSet = new CharacterSet(".\\tsv\\char_middle.tsv");
            smallCharSet = new CharacterSet(".\\tsv\\char_small.tsv");
            xsmallCharSet = new CharacterSet(".\\tsv\\char_xsmall.tsv");

            window.Panel1.Size = new Size(window.ClientSize.Width, window.ClientSize.Height - window.Panel1.Location.Y);

            var width = backgroundDefault.Width * window.CTCPScale / 100;
            var height = backgroundDefault.Height * window.CTCPScale / 100;

            if (window.CTCPScale < 0) {
                width = backgroundDefault.Width * 2;
                height = backgroundDefault.Height * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(height, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);


            lock (pictureBox) {
                if (window.CTCPScale < 0) {
                    pictureBox.Width = window.Size.Width - 16;
                    pictureBox.Height = window.Size.Height - 39 - window.Panel1.Location.Y;
                    pictureBox.Cursor = Cursors.Default;
                }
                else {
                    pictureBox.Width = width;
                    pictureBox.Height = height;
                }
            }


            pictureBox.Image = new Bitmap(backgroundDefault);

            window.Size = new Size(Math.Max(backgroundDefault.Width * window.CTCPScale / 100, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(backgroundDefault.Height * window.CTCPScale / 100, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);

            // 試験表示
            {
                using var g = Graphics.FromImage(pictureBox.Image);
                foreach(var s in stationSettings) {
                    g.DrawString($"{s.Number} {s.Name} 集中", new Font("ＭＳ ゴシック", 16, GraphicsUnit.Pixel), Brushes.White, s.Location.X, s.Location.Y);
                }


                DrawSmallText(g, "2R", 104, 102, 19, 5, new());
                DrawSmallText(g, "5LA", 204, 92, 19, 5, new());
                DrawSmallText(g, "15L13", 515, 191, 19, 5, new());

                var ia = new ImageAttributes();
                ia.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x38, 0x46, 0x72) }]);
                middleCharSet.DrawText(g, "TA", 795, 225, 19, 11, ia);
            }

            originalBitmap = new Bitmap(pictureBox.Image);
            ChangeScale();
            window.DetectResize = true;
        }

        private List<StationSetting> LoadStationSetting(string fileName) {
            List<StationSetting> list = [];
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    if (texts.Any(t => t.Length <= 0) || texts.Length < 5) {
                        continue;
                    }

                    list.Add(new StationSetting(texts[0], texts[1], texts[2], new Point(int.Parse(texts[3]), int.Parse(texts[4]))));
                }
            }
            catch {
            }
            return list;
        }

        public void ChangeScale() {

            try {
                PrepareChangeScale();

                lock (originalBitmap)
                    lock (pictureBox) {

                        var oldPic = pictureBox.Image;
                        if (oldPic != null) {
                            if (window.CTCPScale < 0) {
                                var aspectRatio = (double)originalBitmap.Width / originalBitmap.Height;
                                if (aspectRatio < (double)pictureBox.Width / pictureBox.Height) {
                                    var width = (int)(pictureBox.Height * aspectRatio);
                                    pictureBox.Image = new Bitmap(originalBitmap, width, pictureBox.Height);
                                    pictureBox.Width = width;
                                }
                                else {
                                    var height = (int)(pictureBox.Width / aspectRatio);
                                    pictureBox.Image = new Bitmap(originalBitmap, pictureBox.Width, height);
                                    pictureBox.Height = height;
                                }
                            }
                            else {
                                pictureBox.Image = new Bitmap(originalBitmap, originalBitmap.Width * window.CTCPScale / 100, originalBitmap.Height * window.CTCPScale / 100);
                            }
                            oldPic.Dispose();
                        }
                        window.buttonPanel.Location = ConvertPointToScreen(99, 62);
                        window.buttonPanel.Size = ConvertSizeToScreen(29, 19);
                    }
            }
            catch (Exception e) {
                LogManager.AddExceptionLog(e);
                LogManager.OutputLog(true);
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    window.Invoke(new Action(() => { window.LabelStatusText = "データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage {
                        Caption = "描画エラー | TID - ダイヤ運転会",
                        Heading = "描画エラー",
                        Icon = TaskDialogIcon.Error,
                        Text = "TID画面の描画に失敗しました。\nTID製作者に状況を報告願います。"
                    });
                }
            }

        }

        private void PrepareChangeScale() {
            if (window.WindowState == FormWindowState.Minimized) {
                return;
            }
            var dr = window.DetectResize;
            window.DetectResize = false;
            int width, height;
            lock (originalBitmap) {
                width = originalBitmap.Width * window.CTCPScale / 100;
                height = originalBitmap.Height * window.CTCPScale / 100;

                if (window.CTCPScale < 0) {
                    width = originalBitmap.Width * 2;
                    height = originalBitmap.Height * 2;
                }

                window.MaximumSize = new Size(Math.Max(width, originalBitmap.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(height, originalBitmap.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);

                if (-window.Location.X > window.Size.Width - 60) {
                    window.Location = new Point(0, 80);
                }
            }

            lock (pictureBox) {
                if (window.CTCPScale < 0) {
                    pictureBox.Width = window.ClientSize.Width;
                    pictureBox.Height = window.ClientSize.Height - window.Panel1.Location.Y;
                }
                else {
                    pictureBox.Width = width;
                    pictureBox.Height = height;
                }
            }
            window.DetectResize = dr;

        }





        public void CopyImage() {
            lock (originalBitmap) {
                var i = new Bitmap(originalBitmap);
                using (var g = Graphics.FromImage(i)) {
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 12, GraphicsUnit.Pixel), Brushes.White, originalBitmap.Width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();

            }
        }

        public void CopyImage(int x, int y, int width, int height) {
            lock (originalBitmap) {
                var i = new Bitmap(width, height + 13);
                using (var g = Graphics.FromImage(i)) {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.DrawImage(originalBitmap, new Rectangle(0, 13, width, height), x, y, width, height, GraphicsUnit.Pixel);
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 12, GraphicsUnit.Pixel), Brushes.White, width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();
            }
        }

        public void CopyImage(Point location, Size size) {
            CopyImage(location.X, location.Y, size.Width, size.Height);
        }

        public void AddSubWindow(SubWindow subWindow) {
            lock (subWindows) {
                subWindows.Add(subWindow);
            }
        }

        public bool RemoveSubWindow(SubWindow subWindow) {
            lock (subWindows) {
                return subWindows.Remove(subWindow);
            }
        }

        public void SetClockSubWindows(DateTime time) {
            lock (subWindows) {
                foreach (var sw in subWindows) {
                    sw.SetClock(time);
                }
            }
        }

        public Point ConvertPointToOriginal(int x, int y) {
            return new Point(x * originalBitmap.Width / pictureBox.Width, y * originalBitmap.Height / pictureBox.Height);
        }

        public Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        public Point ConvertPointToScreen(int x, int y) {
            return new Point(x * pictureBox.Width / originalBitmap.Width, y * pictureBox.Height / originalBitmap.Height);
        }

        public Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        public Size ConvertSizeToOriginal(int x, int y) {
            return new Size(x * originalBitmap.Width / pictureBox.Width, y * originalBitmap.Height / pictureBox.Height);
        }

        public Size ConvertSizeToOriginal(Size s) {
            return ConvertSizeToOriginal(s.Width, s.Height);
        }

        public Size ConvertSizeToScreen(int x, int y) {
            return new Size(x * pictureBox.Width / originalBitmap.Width, y * pictureBox.Height / originalBitmap.Height);
        }

        public Size ConvertSizeToScreen(Size s) {
            return ConvertSizeToScreen(s.Width, s.Height);
        }

        public bool IsInArea(Point point, int areaX, int areaY, Size areaSize, int padding = 0) {
            var p = ConvertPointToOriginal(point);
            return p.X >= areaX - padding && p.X < (areaX + areaSize.Width + padding) && p.Y >= areaY - padding && p.Y < (areaY + areaSize.Height + padding);
        }

        public bool DrawText(Graphics g, string text, int x, int y, int width, int height, ImageAttributes ia, ContentAlignment align = ContentAlignment.MiddleCenter) {
            return middleCharSet.DrawText(g, text, x, y, width, height, ia, align) || smallCharSet.DrawText(g, text, x, y, width, height, ia, align) || xsmallCharSet.DrawText(g, text, x, y, width, height, ia, align);
        }

        public bool DrawSmallText(Graphics g, string text, int x, int y, int width, int height, ImageAttributes ia, ContentAlignment align = ContentAlignment.MiddleCenter) {
            return smallCharSet.DrawText(g, text, x, y, width, height, ia, align) || xsmallCharSet.DrawText(g, text, x, y, width, height, ia, align);
        }
    }
}
