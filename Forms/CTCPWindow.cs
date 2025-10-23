using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using OpenIddict.Client;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Manager;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Forms {
    public partial class CTCPWindow : Form, IWinFormsShell {

        private OpenIddictClientService service;

        /// <summary>
        /// サーバ接続用
        /// </summary>
        private ServerCommunication? serverCommunication;

        /// <summary>
        /// TIDManagerオブジェクト
        /// </summary>
        private readonly CTCPManager displayManager;

        private readonly Dictionary<int, ToolStripMenuItem> scaleMenuDict = [];

        public Panel Panel1 => panel1;

        /// <summary>
        /// 最前面表示であるか
        /// </summary>
        private bool topMostSetting = false;

        /// <summary>
        /// 拡大鏡を使用している状態であるか
        /// </summary>
        private bool usingMagnifyingGlass = false;

        /// <summary>
        /// 拡大鏡の直径
        /// </summary>
        private int magnifyingGlassSize = 240;

        /// <summary>
        /// 表示される時刻の時差を足す前
        /// </summary>
        public DateTime Clock {
            get;
            set;
        }

        /// <summary>
        /// 現実の時刻
        /// </summary>
        public DateTime RealTime {
            get;
            set;
        } = DateTime.Now;

        /// <summary>
        /// 現実との時差
        /// </summary>
        public TimeSpan TimeOffset {
            get;
            private set;
        } = new(14, 0, 0);

        /// <summary>
        /// 時差を表示するか（0は表示せずそれ以外は0までのカウントダウン）
        /// </summary>
        private int showOffset = 0;

        /// <summary>
        /// 拡大率（0未満はフィット表示）
        /// </summary>
        public int CTCPScale {
            get;
            private set;
        } = 100;

        private int initialScale = 100;

        private int[] scaleArray = { 50, 75, 90, 100, 110, 125, 150, 175, 200 };

        public bool FixedScale {
            get;
            private set;
        } = false;

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point mouseLoc = Point.Empty;

        private Cursor defaultCursor = Cursors.SizeAll;

        /// <summary>
        /// WASDキーなど使用時の移動量
        /// </summary>
        private int scrollDelta = 15;

        public string LabelStatusText {
            get => labelStatus.Text;
            set {
                if (serverCommunication != null) {
                    value = $"Status：{(ServerAddress.SignalAddress.Contains("dev") ? "Dev" : "Prod")}サーバ {value}";
                }
                else {
                    value = $"Status：{value}";
                }
                if (InvokeRequired) {
                    Invoke(() => labelStatus.Text = value);
                }
                else {
                    labelStatus.Text = value;
                }
            }
        }

        private SoundPlayer? warningSound = null;

        public void PlayWarningSound() {
            if (warningSound != null) {
                warningSound.Play();
            }
            else {
                SystemSounds.Hand.Play();
            }
        }

        private bool windowMinimized = false;

        private bool resized = false;

        private Point? selectionStarting = null;

        public int MarkupType {
            get;
            private set;
        } = 0;

        public bool ReservedUpdate {
            get;
            set;
        } = false;

        public bool DetectResize {
            get; set;
        } = false;

        public bool OpeningDialog {
            get;
            private set;
        } = false;

        public bool Silent { get; private set; } = false;

        public CTCPWindow(OpenIddictClientService service) {
            this.service = service;
            InitializeComponent();
            LogManager.AddInfoLog($"起動 ver. {ServerAddress.Version}");

            pictureBox2.Parent = pictureBox1;
            pictureBox3.Parent = pictureBox1;


            if (File.Exists(".\\sound\\warning.wav")) {
                warningSound = new SoundPlayer(".\\sound\\warning.wav");
            }


            foreach (var scale in scaleArray) {
                AddScale(scale);
            }


            if (initialScale < 0) {
                initialScale = -1;
                CTCPScale = initialScale;
                menuItemScaleFit.CheckState = CheckState.Indeterminate;
            }
            else {
                if (!scaleMenuDict.ContainsKey(initialScale)) {
                    if (scaleMenuDict.ContainsKey(100)) {
                        initialScale = 100;
                    }
                    else {
                        initialScale = scaleMenuDict.Keys.FirstOrDefault();
                    }
                }
                CTCPScale = initialScale;
                scaleMenuDict[initialScale].CheckState = CheckState.Indeterminate;
            }



            displayManager = new CTCPManager(pictureBox1, this);

            flowLayoutPanel1.Location = new Point(flowLayoutPanel1.Location.X - Size.Width + ClientSize.Width + 16, flowLayoutPanel1.Location.Y);


            if (CTCPScale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{CTCPScale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            }


            Load += TIDWindow_Load;
            menuItemScaleFit.Click += (sender, e) => { SetScale(-1); };


            ChangeDefaultCursor();

            for (var i = 0; i < 24; i++) {
                var time = i;
                var menu = new ToolStripMenuItem();
                menuItemQuickTimeSetting.DropDownItems.Add(menu);
                menu.Name = $"menuItemHour{time}";
                menu.Size = new Size(110, 22);
                menu.Text = $"{time}時台";
                menu.Click += (sender, e) => { SetHourQuick(time); };
            }

        }

        private void AddScale(int scale) {
            var menu = new ToolStripMenuItem();
            scaleMenuDict.Add(scale, menu);
            menuItemScale.DropDownItems.Insert(menuItemScale.DropDownItems.Count - 3, menu);
            menu.Name = $"menuItemScale{scale}";
            menu.Size = new Size(110, 22);
            menu.Text = $"{scale}%";
            menu.Click += (sender, e) => {
                SetScale(scale);
            };
        }


        private async void TIDWindow_Load(object? sender, EventArgs? e) {
            _ = Task.Run(ClockUpdateLoop);



            SetTopMost(topMostSetting);

            //デフォルトのサーバへの接続処理
            /*serverCommunication = new(this, service);
            serverCommunication.DataUpdated += UpdateServerData;
            LogManager.AddInfoLog($"{(ServerAddress.SignalAddress.Contains("dev") ? "Dev" : "Prod")}サーバに接続します");
            await TryConnectServer();*/
        }


        /// <summary>
        /// 運転会サーバと接続する
        /// </summary>
        /// <param name="url">接続先のURL</param>
        /// <returns></returns>
        private async Task TryConnectServer() {
            if (serverCommunication != null) {
                await serverCommunication.Authorize();
            }
        }


        /// <summary>
        /// サーバからのデータが更新された際に呼ばれる
        /// </summary>
        /// <param name="tcData"></param>
        private void UpdateServerData(DataToCTCP? data) {
            if (data == null) {
                return;
            }
            /*var tcList = data.TrackCircuitDatas;
            var sList = data.SwitchDatas;
            var dList = data.DirectionDatas;
            var trainList = data.TrainStateDatas;

            var updated = tcList != null && UpdateTrainData(tcList, trainList);
            updated |= tcList != null && trackManager.UpdateTCData(tcList);
            updated |= sList != null && UpdatePointData(sList);
            updated |= dList != null && UpdateDirectionData(dList);
            updated |= trackManager.UpdateNumberWindow();

            if (updated) {
                displayManager.UpdateTID();
            }*/
        }

        private async void ClockUpdateLoop() {
            try {
                while (true) {
                    var timer = Task.Delay(10);
                    if (InvokeRequired) {
                        Invoke(new Action(UpdateClock));
                    }
                    else {
                        UpdateClock();
                    }
                    await timer;
                }
            }
            catch (ObjectDisposedException) {
            }
        }

        private void UpdateClock() {
            if (!OpeningDialog && ActiveForm != this && !displayManager.IsActiveForm) {
                UpdateMouseCursor();
            }
            if (WindowState == FormWindowState.Maximized) {
                DetectResize = false;
                var width = Width;
                var height = Height;
                var loc = new Point(Location.X, Location.Y + 8);
                WindowState = FormWindowState.Normal;
                Width = width;
                Height = height;
                Location = loc;
                DetectResize = true;
            }
            if (showOffset > 0) {
                showOffset--;
            }

            /*var oldFlashState = FlashState;*/
            var now = DateTime.Now;
            /*var deltaSeconds = (now - RealTime).TotalSeconds;*/
            RealTime = now;
            /*if (flashInterval > 0) {
                flashState -= (float)deltaSeconds;
                while (flashState <= 0) {
                    flashState += flashInterval * 2;
                }
            }*/

            /*if (!UpdateDebug() && displayManager.Started && (ReservedUpdate || (oldFlashState != FlashState) && MarkupType < 2 && (trainDataDict.Values.Any(td => td.Markup) || MarkupDuplication || MarkupFillZero || MarkupNotTrain || MarkupDelayed > 0 || displayManager.Markuped))) {
                ReservedUpdate = false;
                displayManager.UpdateTID();
            }*/


            if (usingMagnifyingGlass) {
                var cp1 = pictureBox1.PointToClient(Cursor.Position);
                var cp2 = PointToClient(Cursor.Position);

                if (cp1.X < 0 || cp1.Y < 0 || cp1.X > pictureBox1.Width || cp1.Y > pictureBox1.Height || cp2.X < 0 || cp2.Y < 0 || cp2.X > ClientSize.Width || cp2.Y > ClientSize.Height) {
                    var width = pictureBox1.Width - cp1.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - cp1.Y + magnifyingGlassSize / 2;
                    var mouseX = cp1.X;
                    var mouseY = cp1.Y;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - cp1.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - cp1.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(cp1.X, cp1.Y);
                }
            }


            Clock = RealTime;
            var time = Clock + TimeOffset;
            displayManager.SetClockSubWindows(time);
            if (showOffset <= 0) {
                labelClock.Text = time.ToString("H:mm:ss");
            }
            if (serverCommunication == null) {
                return;
            }
            var updatedTime = serverCommunication?.UpdatedTime;
            if (updatedTime == null || serverCommunication == null) {
                return;
            }
            var delaySeconds = (Clock - (DateTime)updatedTime).TotalSeconds;
            updatedTime = updatedTime?.Add(TimeOffset);
            if (delaySeconds > 10) {
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    LogManager.AddWarningLog("サーバからの受信が10秒以上ありません");
                    LabelStatusText = $"データ受信不能(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                    Debug.WriteLine($"データ受信不能: {delaySeconds}");
                    if (!Silent) {
                        TaskDialog.ShowDialog(new TaskDialogPage {
                            Caption = "データ受信不能 | TID - ダイヤ運転会",
                            Heading = "データ受信不能",
                            Icon = TaskDialogIcon.Error,
                            Text = "サーバ側からのデータ受信が10秒以上ありませんでした。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をおすすめします。"
                        });
                    }
                    else {
                        PlayWarningSound();
                    }
                }
            }
            else if (delaySeconds > 1) {
                if (!LabelStatusText.Contains("最終受信")) {
                    LogManager.AddWarningLog("サーバからの受信が1秒以上ありません");
                }
                LabelStatusText = $"データ正常受信(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                Debug.WriteLine($"データ受信不能: {delaySeconds}");
            }
        }

        private void labelClock_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) {
                return;
            }
            ChangeTime(e.Button == MouseButtons.Right, !ModifierKeys.HasFlag(Keys.Control), !ModifierKeys.HasFlag(Keys.Shift));
        }

        private void ChangeTime(bool isPlus, bool changeHours, bool changeMinutes) {
            var hour = TimeOffset.Hours;
            var min = TimeOffset.Minutes;
            var sec = TimeOffset.Seconds;
            if (isPlus) {
                if (changeHours) {
                    hour++;
                }
                else if (changeMinutes) {
                    min++;
                    if (min >= 60) {
                        hour++;
                    }
                    showOffset = 40;
                }
                else {
                    sec++;
                    if (sec >= 60) {
                        min++;
                        if (min >= 60) {
                            hour++;
                        }
                    }
                    showOffset = 40;
                }
            }
            else {
                if (changeHours) {
                    hour += 23;
                }
                else if (changeMinutes) {
                    if (min == 0) {
                        hour += 23;
                    }
                    min += 59;
                    showOffset = 40;
                }
                else {
                    if (sec == 0) {
                        if (min == 0) {
                            hour += 23;
                        }
                        min += 59;
                    }
                    sec += 59;
                    showOffset = 40;
                }
            }
            TimeOffset = new TimeSpan(hour % 24, min % 60, sec % 60);
            if (showOffset > 0) {
                labelClock.Text = $"+{TimeOffset.Hours}h{TimeOffset.Minutes}m{TimeOffset.Seconds}s";
            }
        }

        private void labelTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void SetTopMost(bool topMost) {
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

        private void labelSilent_Hover(object sender, EventArgs e) {
            labelSilent.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void labelSilent_Leave(object sender, EventArgs e) {
            labelSilent.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void labelSilent_Click(object sender, EventArgs e) {
            SetSilent(!Silent);
        }


        private void menuItemCopy_Click(object sender, EventArgs e) {
            displayManager.CopyImage();
        }

        private void menuItemTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void menuItemSilent_Click(object sender, EventArgs e) {
            SetSilent(!Silent);
        }

        public void SetSilent(bool silent) {
            Silent = silent;
            menuItemSilent.CheckState = silent ? CheckState.Checked : CheckState.Unchecked;
            labelSilent.Text = $"サイレント：{(silent ? "ON" : "OFF")}";
            labelSilent.ForeColor = silent ? Color.Gray : Color.White;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetSilent(silent);
                }
            }
        }

        private void menuItemFixedScale_Click(object sender, EventArgs e) {
            SetFixedScale(true);
        }


        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (CTCPScale > 0) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    SetScale(-1);
                }
                else if (ModifierKeys.HasFlag(Keys.Control)) {
                    SetFixedScale(true);
                }
                else {
                    var i = -1;
                    if (e.Button == MouseButtons.Right) {
                        i = Math.Min(Array.IndexOf(scaleArray, CTCPScale) + 1, scaleArray.Length - 1);
                    }
                    else if (e.Button == MouseButtons.Left) {
                        i = Math.Max(Array.IndexOf(scaleArray, CTCPScale) - 1, 0);
                    }
                    if (i >= 0) {
                        SetScale(scaleArray[i]);
                    }
                }
            }
            else {
                if (ModifierKeys.HasFlag(Keys.Control)) {
                    SetFixedScale(!FixedScale);
                }
                else if (FixedScale && ModifierKeys.HasFlag(Keys.Shift)) {
                    SetFixedScale(false);
                }
                else {
                    SetScale(initialScale);
                }
            }
        }


        private void SetScale(int scale) {
            var min = scaleMenuDict.Keys.Min();
            var max = scaleMenuDict.Keys.Max();
            if (scale < min && scale != -1) {
                scale = min;
            }
            if (scale > max) {
                scale = max;
            }
            LogManager.AddInfoLog($"拡大率変更：{(scale > 0 ? $"{scale}%" : "fit")}");

            foreach (var k in scaleMenuDict.Keys) {
                scaleMenuDict[k].CheckState = k == scale ? CheckState.Indeterminate : CheckState.Unchecked;
            }

            menuItemScaleFit.CheckState = scale < 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemFixedScale.CheckState = CheckState.Unchecked;



            FixedScale = false;
            CTCPScale = scale;

            displayManager.ChangeScale();
            if (scale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{scale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            }
            ChangeDefaultCursor();
        }

        public void SetFixedScale(bool value) {
            LogManager.AddInfoLog($"拡大率変更：{(value ? "倍率固定" : "fit")}");

            foreach (var k in scaleMenuDict.Keys) {
                scaleMenuDict[k].CheckState = CheckState.Unchecked;
            }

            menuItemScaleFit.CheckState = !value ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemFixedScale.CheckState = value ? CheckState.Indeterminate : CheckState.Unchecked;


            FixedScale = value;
            CTCPScale = -1;

            if (value) {
                labelScale.ForeColor = Color.Red;
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                displayManager.ChangeScale();
            }
            labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            ChangeDefaultCursor();
        }

        private void SetHourQuick(int hour) {
            TimeOffset = new TimeSpan((hour + 24 - Clock.Hour) % 24, TimeOffset.Minutes, TimeOffset.Seconds);
        }




        private void menuItemVersion_Click(object sender, EventArgs e) {
            var form = new VersionWindow();
            form.Icon = Icon;
            var bitmap = Icon != null ? new Icon(Icon, 256, 256).ToBitmap() : new Bitmap(10, 10);
            form.PictureIcon.Image = bitmap;
            form.PictureIcon.Size = new Size(bitmap.Width, bitmap.Height);
            form.LabelVersion.Text = $"TatehamaCTCPClient\nVer. {ServerAddress.Version.Replace("TatehamaCTCPClient_", "")}";
            if (TopMost) {
                form.TopMost = true;
            }
            OpeningDialog = true;
            form.ShowDialog();
            OpeningDialog = false;

        }

        private void menuItemMarkupType1_Click(object sender, EventArgs e) {
            SetMarkupType(0);
        }

        private void menuItemMarkupType2_Click(object sender, EventArgs e) {
            SetMarkupType(1);
        }

        private void menuItemMarkupType3_Click(object sender, EventArgs e) {
            SetMarkupType(2);
        }

        public void SetMarkupType(int type) {

            MarkupType = type < 3 ? (type >= 0 ? type : 0) : 2;
            menuItemMarkupType1.CheckState = type == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType2.CheckState = type == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType3.CheckState = type == 2 ? CheckState.Indeterminate : CheckState.Unchecked;
            if (displayManager == null) {
                return;
            }
            foreach (var w in displayManager.SubWindows) {
                w.SetMarkupType(type);
            }
        }

        private void CTCPWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            /*if ((mod & Keys.Shift) == Keys.Shift) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            else */if ((mod & Keys.Control) == Keys.Control) {
                pictureBox1.Cursor = Cursors.Cross;
            }
            if (e.KeyData == (Keys.C | Keys.Control)) {
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = pictureBox1.PointToClient(Cursor.Position);
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 10 ? (start.X < pictureBox1.Width && end.X < pictureBox1.Width - 10 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 10 ? (start.Y < pictureBox1.Height && end.Y < pictureBox1.Height - 10 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                    start = ConvertPointToOriginal(start);
                    end = ConvertPointToOriginal(end);
                    var p = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                    var s = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                    displayManager.CopyImage(p, s);
                }
                else {
                    displayManager.CopyImage();
                }
            }
            if (e.KeyData == Keys.Tab) {
                SetTopMost(!TopMost);
            }

            if (code == Keys.Right || code == Keys.D) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value + scrollDelta * (mod == Keys.Shift ? 1 : 3), panel1.VerticalScroll.Value);
            }
            if (code == Keys.Left || code == Keys.A) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - scrollDelta * (mod == Keys.Shift ? 1 : 3), panel1.VerticalScroll.Value);
            }
            if (code == Keys.Up || code == Keys.W) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value - scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if (code == Keys.Down || code == Keys.S) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value + scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if (e.KeyData == Keys.D1) {
                panel1.AutoScrollPosition = new Point(0, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D2) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 1 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D3) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 2 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D4) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 3 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D5) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 4 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D6) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 5 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D7) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 6 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D8) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 7 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D9) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 8 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D0) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.NumPad7) {
                panel1.AutoScrollPosition = new Point(0, 0);
            }
            if (e.KeyData == Keys.NumPad8) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, 0);
            }
            if (e.KeyData == Keys.NumPad9) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, 0);
            }
            if (e.KeyData == Keys.NumPad4) {
                panel1.AutoScrollPosition = new Point(0, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad5) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad6) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad1) {
                panel1.AutoScrollPosition = new Point(0, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (e.KeyData == Keys.NumPad2) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (e.KeyData == Keys.NumPad3) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (code == Keys.PageUp || code == Keys.PageDown) {
                ChangeTime(code == Keys.PageUp, (mod & Keys.Control) != Keys.Control, (mod & Keys.Shift) != Keys.Shift);
            }
            if (code == Keys.Oemplus || code == Keys.OemSemicolon) {
                ChangeTime(code == Keys.OemSemicolon, (mod & Keys.Control) != Keys.Control, (mod & Keys.Shift) != Keys.Shift);
            }

        }

        private void CTCPWindow_KeyUp(object sender, KeyEventArgs e) {
            var mod = e.KeyData & Keys.Modifiers;
            UpdateMouseCursor();
            if ((mod & Keys.Shift) != Keys.Shift && (mod & Keys.Control) != Keys.Control) {
                if ((MouseButtons & MouseButtons.Left) == MouseButtons.Left) {
                    mouseLoc = pictureBox1.PointToClient(Cursor.Position);
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                }
            }
            if (resized && (mod & Keys.Control) != Keys.Control) {
                resized = false;
                displayManager.RelocateButtons();
            }

        }

        private void CTCPWindow_Resize(object sender, EventArgs e) {
            if (displayManager != null && DetectResize) {
                DetectResize = false;
                if (WindowState != FormWindowState.Minimized) {
                    if (CTCPScale == -1 && !FixedScale) {
                        displayManager.ChangeScale(false);
                        resized = true;
                        labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
                    }
                    else {
                        ChangeDefaultCursor();
                    }
                }
                DetectResize = true;
            }
        }

        private void CTCPWindow_ResizeEnd(object sender, EventArgs e) {
            if (displayManager != null) {
                resized = false;
                displayManager.RelocateButtons();
            }
        }

        private void CTCPWindow_SizeChanged(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                if (!windowMinimized) {
                    LogManager.AddInfoLog("ウィンドウが最小化されました");
                    windowMinimized = true;
                }
            }
            else if (windowMinimized) {
                LogManager.AddInfoLog("ウィンドウの最小化が解除されました");
                windowMinimized = false;
            }
        }

        private void ChangeDefaultCursor() {
            defaultCursor = CTCPScale == -1 ? Cursors.Default : (pictureBox1.Width < panel1.Width && pictureBox1.Height < panel1.Height ? Cursors.Default : Cursors.SizeAll);
            UpdateMouseCursor();
        }

        private void CTCPWindow_Closing(object sender, EventArgs e) {
            if (LogManager.Output && LogManager.NeededWarning) {
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "エラーログ出力 | TID - ダイヤ運転会",
                    Heading = "エラーログ出力",
                    Icon = TaskDialogIcon.Information,
                    Text =
                        $"エラーログが出力されました。\n本ソフトの製作担当者にお問い合わせのうえ、\n必要な場合はErrorLog.txtをお送りください。\n（ErrorLog.txtは次回起動後に削除される場合があります）"
                });
            }
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                if (CTCPScale > 0) {
                    var i = -1;
                    if (e.Delta > 0) {
                        i = Math.Min(Array.IndexOf(scaleArray, CTCPScale) + 1, scaleArray.Length - 1);
                    }
                    else {
                        i = Math.Max(Array.IndexOf(scaleArray, CTCPScale) - 1, 0);
                    }
                    if (i >= 0) {
                        SetScale(scaleArray[i]);
                    }
                }
                else {
                    if (FixedScale) {
                        SetFixedScale(false);
                    }
                    lock (pictureBox1.Image)
                    lock(displayManager.OriginalBitmap) {
                        var size = Size;
                        var dp = e.Location;
                        var point = ConvertPointToOriginal(dp.X, dp.Y);
                        var rate = (pictureBox1.Image.Width + e.Delta * 0.2) / displayManager.OriginalBitmap.Width;
                        var width = Size.Width - ClientSize.Width + (int)(displayManager.OriginalBitmap.Width * rate);
                        var height = Size.Height - ClientSize.Height + panel1.Location.Y + (int)(displayManager.OriginalBitmap.Height * rate);
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
                            height = Size.Height - ClientSize.Height + panel1.Location.Y + displayManager.OriginalBitmap.Height * (screenSize.Width - Size.Width + ClientSize.Width) / displayManager.OriginalBitmap.Width;
                            Size = new Size(width, height);
                            var np = ConvertPointToScreen(point);
                            if (size != Size) {
                                Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                            }
                        }
                        else {
                            height = screenSize.Height;
                            width = Size.Width - ClientSize.Width + displayManager.OriginalBitmap.Width * (screenSize.Height - Size.Height + ClientSize.Height - panel1.Location.Y) / displayManager.OriginalBitmap.Height;
                            Size = new Size(width, height);
                            var np = ConvertPointToScreen(point);
                            if (size != Size) {
                                Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                            }
                        }
                    }
                }
            }
            else if (ModifierKeys.HasFlag(Keys.Shift)) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Delta, panel1.VerticalScroll.Value);
            }
            else {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value - e.Delta);
            }
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalBitmap.Width) {
                if (usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                }
                else {
                    usingMagnifyingGlass = true;
                    var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(e.X - magnifyingGlassSize / 2, e.Y - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(e.X, e.Y);

                    UpdateMouseCursor();
                }

            }
            else if (usingMagnifyingGlass) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                UpdateMouseCursor();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                /*if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(e.Location, w.PosX, w.PosY, w.GetSize(), 1) && trainDataDict.TryGetValue(t, out var td)) {
                            td.Markup = !td.Markup;
                            trainMenuDict[t].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                            ReservedUpdate = true;
                        }
                    }
                }
                else */if (ModifierKeys.HasFlag(Keys.Control)) {
                    selectionStarting = e.Location;
                }
                else {
                    mouseLoc = e.Location;
                }
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
                pictureBox1.Cursor = defaultCursor;
                selectionStarting = null;
                lock (pictureBox3) {
                    pictureBox3.Location = new Point(-300, -300);
                    pictureBox3.Size = new Size(100, 100);
                }
            }
        }

        private void PictureBox2_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalBitmap.Width) {
                if (usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                }
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                UpdateMouseCursor();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                /*if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(pictureBox1.PointToClient(Cursor.Position), w.PosX, w.PosY, w.GetSize(), 1) && trainDataDict.TryGetValue(t, out var td)) {
                            td.Markup = !td.Markup;
                            trainMenuDict[t].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                            ReservedUpdate = true;
                        }
                    }
                }
                else */if (ModifierKeys.HasFlag(Keys.Control)) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                    selectionStarting = pictureBox1.PointToClient(Cursor.Position);
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {
                var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                var mouseX = e.X;
                var mouseY = e.Y;
                if (width <= 1 || height <= 1) {
                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                }
                else {
                    pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                    pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                }

                SetMagnifyingGlass(e.X, e.Y);


            }
            if (!selectionStarting.HasValue && !ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Location.X + mouseLoc.X, panel1.VerticalScroll.Value - e.Location.Y + mouseLoc.Y);
            }
            if (selectionStarting.HasValue) {
                var s = selectionStarting.Value;
                selectionStarting = new Point(s.X > 16 ? (s.X < pictureBox1.Width - 16 ? s.X : pictureBox1.Width) : 0, s.Y > 16 ? (s.Y < pictureBox1.Height - 16 ? s.Y : pictureBox1.Height) : 0);
                var start = selectionStarting.Value;
                var end = e.Location;
                var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                var startOrig = ConvertPointToOriginal(start);
                var endOrig = ConvertPointToOriginal(end);
                var pos = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                var size = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                var sizeOrig = new Size(Math.Abs(startOrig.X - endOrig.X), Math.Abs(startOrig.Y - endOrig.Y));
                if (size.Width > 1 && size.Height > 1) {
                    lock (pictureBox3) {
                        var screenSize = Screen.FromControl(this).Bounds;
                        screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                        var old = pictureBox3.Image;
                        var b = new Bitmap(size.Width, size.Height);
                        using var g = Graphics.FromImage(b);
                        g.Clear(Color.Transparent);
                        g.DrawRectangle(sizeOrig.Width > 120 && sizeOrig.Width <= screenSize.Width && sizeOrig.Height > 100 && sizeOrig.Height <= screenSize.Height ? Pens.LimeGreen : Pens.DarkRed, 0, 0, size.Width - 1, size.Height - 1);
                        pictureBox3.Image = b;
                        pictureBox3.Location = pos;
                        pictureBox3.Size = size;
                        old?.Dispose();
                    }
                }

            }
        }

        private void PictureBox2_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {

                var cp = pictureBox1.PointToClient(Cursor.Position);
                var width = pictureBox1.Width - cp.X + magnifyingGlassSize / 2;
                var height = pictureBox1.Height - cp.Y + magnifyingGlassSize / 2;
                var mouseX = cp.X;
                var mouseY = cp.Y;
                if (width <= 1 || height <= 1) {
                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                }
                else {
                    pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                    pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - cp.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - cp.Y + magnifyingGlassSize / 2)));
                }
                UpdateMouseCursor();
                SetMagnifyingGlass(cp.X, cp.Y);
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = Point.Empty;
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = e.Location;
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                    start = ConvertPointToOriginal(start);
                    end = ConvertPointToOriginal(end);
                    var p = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                    var s = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                    var screenSize = Screen.FromControl(this).Bounds;
                    screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                    if (s.Width > 120 && s.Width <= screenSize.Width && s.Height > 100 && s.Height <= screenSize.Height - pictureBox1.Location.Y) {
                        var sub = new SubWindow(p, s, displayManager);
                        sub.Icon = Icon;
                        pictureBox1.Cursor = defaultCursor;
                        sub.Show();
                        var border = (Size.Width - ClientSize.Width) / 2;
                        sub.Location = new Point(center.X - s.Width / 2 - border, center.Y - s.Height / 2 - Size.Height + ClientSize.Height - panel1.Location.Y / 2 - border);
                        sub.SetTopMost(TopMost);
                        displayManager.AddSubWindow(sub);
                    }
                }
            }
        }

        public void SetMagnifyingGlass(int x, int y) {
            if (usingMagnifyingGlass) {
                lock (displayManager.OriginalBitmap)
                    lock (pictureBox2) {
                        var posX = magnifyingGlassSize / 2 - x * displayManager.OriginalBitmap.Width / pictureBox1.Width;
                        var posY = magnifyingGlassSize / 2 - y * displayManager.OriginalBitmap.Height / pictureBox1.Height;
                        posX = posX > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - x : (posX < magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Width ? pictureBox1.Width - x + magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Width : posX);
                        posY = posY > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - y : (posY < magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Height ? pictureBox1.Height - y + magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Height : posY);

                        var b = new Bitmap(magnifyingGlassSize, magnifyingGlassSize);
                        var old = pictureBox2.Image;
                        pictureBox2.Image = b;
                        old?.Dispose();
                        using var g = Graphics.FromImage(pictureBox2.Image);
                        GraphicsPath gp = new();
                        gp.AddEllipse(g.VisibleClipBounds);
                        g.Clip = new Region(gp);
                        g.DrawImage(displayManager.OriginalBitmap, posX, posY);
                        g.DrawEllipse(new Pen(Color.DarkGray, 2), 0, 0, magnifyingGlassSize, magnifyingGlassSize);
                    }
            }
        }

        public void SetMagnifyingGlass() {

            if (InvokeRequired) {
                Invoke(() => {
                    var cp = pictureBox1.PointToClient(Cursor.Position);
                    SetMagnifyingGlass(cp.X, cp.Y);
                });
            }
            else {
                var cp = pictureBox1.PointToClient(Cursor.Position);
                SetMagnifyingGlass(cp.X, cp.Y);
            }
        }

        public Point ConvertPointToOriginal(int x, int y) {
            return new Point(x * displayManager.OriginalBitmap.Width / pictureBox1.Width, y * displayManager.OriginalBitmap.Height / pictureBox1.Height);
        }

        public Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        public Point ConvertPointToScreen(int x, int y) {
            return new Point(x * pictureBox1.Width / displayManager.OriginalBitmap.Width, y * pictureBox1.Height / displayManager.OriginalBitmap.Height);
        }

        public Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        public Size ConvertSizeToOriginal(int x, int y) {
            return new Size(x * displayManager.OriginalBitmap.Width / pictureBox1.Width, y * displayManager.OriginalBitmap.Height / pictureBox1.Height);
        }

        public Size ConvertSizeToOriginal(Size s) {
            return ConvertSizeToOriginal(s.Width, s.Height);
        }

        public Size ConvertSizeToScreen(int x, int y) {
            return new Size(x * pictureBox1.Width / displayManager.OriginalBitmap.Width, y * pictureBox1.Height / displayManager.OriginalBitmap.Height);
        }

        public Size ConvertSizeToScreen(Size s) {
            return ConvertSizeToScreen(s.Width, s.Height);
        }

        public bool IsInArea(Point point, int areaX, int areaY, Size areaSize, int padding = 0) {
            var p = ConvertPointToOriginal(point);
            return p.X >= areaX - padding && p.X < (areaX + areaSize.Width + padding) && p.Y >= areaY - padding && p.Y < (areaY + areaSize.Height + padding);
        }

        private void UpdateMouseCursor() {
            /*if (ModifierKeys.HasFlag(Keys.Shift)) {
                pictureBox1.Cursor = Cursors.Hand;
                pictureBox2.Cursor = Cursors.Hand;
            }
            else*/ if (ModifierKeys.HasFlag(Keys.Control)) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else if (usingMagnifyingGlass) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else {
                pictureBox1.Cursor = defaultCursor;
                pictureBox2.Cursor = Cursors.Cross;
            }
        }
    }
}
