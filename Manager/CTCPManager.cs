using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Forms;

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

        /// <summary>
        /// 起動時背景画像
        /// </summary>
        private Image backgroundDefault;

        /// <summary>
        /// 通常時背景画像
        /// </summary>
        private Image backgroundImage;

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        private Bitmap originalBitmap;

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




            SubWindows = subWindows.AsReadOnly();

            backgroundDefault = Image.FromFile(".\\png\\Background-1.png");
            backgroundImage = Image.FromFile(".\\png\\Background.png");

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


            originalBitmap = new Bitmap(pictureBox.Image);
            ChangeScale();
            window.DetectResize = true;
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
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 9), Brushes.White, originalBitmap.Width - 51, 0);
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
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 9), Brushes.White, width - 51, 0);
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
    }
}
