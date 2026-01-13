using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using OpenIddict.Client;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using TatehamaCTCPClient.Buttons;
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
        } = DateTime.UtcNow.AddHours(9);

        /// <summary>
        /// 現実との時差
        /// </summary>
        public TimeSpan TimeOffset {
            get;
            private set;
        } = new(14, 0, 0);

        public string SystemName {
            get;
            private set;
        } = "CTCP";

        public string SystemNameLong {
            get;
            private set;
        } = "CTCP";

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
        private Point? mouseLoc = null;

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

        private static SoundPlayer? warningSound = null;

        public static void PlayWarningSound() {
            if (warningSound != null) {
                warningSound.Play();
            }
            else {
                SystemSounds.Hand.Play();
            }
        }

        private bool windowMinimized = false;

        private float blinkInterval = 0.5f;

        private float blinkState = 0f;

        public bool BlinkStateFast => blinkInterval <= 0 || blinkState % blinkInterval > blinkInterval / 2;

        public bool BlinkStateSlow => blinkInterval <= 0 || blinkState > blinkInterval;

        private Point? selectionStarting = null;

        private CTCPButton? pressingButton = null;

        public int MarkupType {
            get;
            private set;
        } = -1;

        public bool ReservedUpdate {
            get;
            set;
        } = false;

        public bool UseServerTime {
            get;
            private set;
        } = true;

        public int ServerTime {
            get;
            private set;
        } = 14;

        public bool DetectResize {
            get; set;
        } = false;

        public bool OpeningDialog {
            get;
            set;
        } = false;

        public bool Silent { get; private set; } = false;

        private Image PictureBoxImage => displayManager.PictureBoxImage;

        private int PictureBoxWidth => displayManager.PictureBoxWidth;

        private int PictureBoxHeight => displayManager.PictureBoxHeight;

        public CTCPWindow(OpenIddictClientService service) {
            this.service = service;
            InitializeComponent();
            LogManager.AddInfoLog($"起動 ver. {ServerAddress.Version}");

            pictureBox2.Parent = pictureBox1;
            pictureBox3.Parent = pictureBox1;

            var loaded = false;

            var docuPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\TRAIN CREW Tool\TatehamaCTCPClient\";
            var lg = LoadSetting($"{docuPath}setting.txt"); ;
            loaded |= lg;
            if (lg) {
                LogManager.AddInfoLog("グローバル設定ファイルを読み込みました");
            }
            var ll = LoadSetting(".\\setting.txt");
            loaded |= ll;
            if (ll) {
                LogManager.AddInfoLog("ローカル設定ファイルを読み込みました");
            }

            if (!loaded) {
                /*if (!Directory.Exists(".\\setting\\")) {
                    Directory.CreateDirectory(".\\setting\\");
                }*/
                using (StreamWriter w = new(".\\setting.txt", false, new UTF8Encoding(false))) {
                    w.Write($"#このファイルは {docuPath} に配置しても動作します。\nsystemName=CTCP\ntopMost=true\nscaleList=50,75,90,100,110,125,150,175,200\ninitialScale=100\ntimeOffset=14\nzoomSize=240\nsilent=false\nflashInterval=0.5\nmarkupType=-1\nhideNumber=false");
                }
                LogManager.AddInfoLog("ローカル設定ファイルを作成しました");

                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "設定ファイル作成 | CTCP - ダイヤ運転会",
                    Heading = "設定ファイルが作成されました",
                    Icon = TaskDialogIcon.Information,
                    Text = $"exeと同じフォルダ内に設定ファイルを作成しました。\n設定ファイルを編集することで起動時の設定などが変更できます。",
                    SizeToContent = true
                });
            }

            if(SystemName != "CTCP") {
                SystemNameLong = $"{SystemName}(CTCP)";
            }
            Text = $"全線CTCP | {SystemNameLong} - ダイヤ運転会";


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
                lock (displayManager.syncPictureBox) {
                    labelScale.Text = $"Scale：{(int)((double)PictureBoxImage.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
                }
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

        private bool LoadSetting(string path) {

            try {
                if (!File.Exists(path)) {
                    return false;
                }
                using var sr = new StreamReader(path);
                var line = sr.ReadLine();
                while (line != null) {
                    var texts = line.Replace(" ", "").Split('=');
                    line = sr.ReadLine();

                    if (texts.Length < 2 || texts.Any(t => t == "")) {
                        continue;
                    }
                    var v = texts[1].Replace(" ", "").ToLower();

                    switch (texts[0]) {
                        case "systemName":
                            SystemName = texts[1].Replace(" ", "");
                            break;
                        case "topMost":
                            topMostSetting = v == "true";
                            break;
                        case "scaleList":
                            var scaleList = new List<int>();
                            foreach (var str in texts[1].Split(',')) {
                                if (!int.TryParse(str, out var scale) || scale <= 0 || scale > 500) {
                                    continue;
                                }
                                scaleList.Add(scale);
                            }
                            if (scaleList.Count > 0) {
                                scaleArray = scaleList.ToArray();
                            }
                            break;
                        case "initialScale":
                        case "scale":
                            foreach (var m in scaleMenuDict.Values) {
                                m.CheckState = CheckState.Unchecked;
                            }
                            menuItemScaleFit.CheckState = CheckState.Unchecked;

                            if (v == "fit") {
                                initialScale = -1;
                                break;
                            }
                            if (int.TryParse(texts[1], out var s)) {
                                initialScale = s;
                            }
                            break;
                        case "timeOffset":
                            if (int.TryParse(texts[1], out var hours)) {
                                TimeOffset = new TimeSpan(((hours % 24) + 24) % 24, 0, 0);
                            }
                            break;
                        case "zoomSize":
                            if (int.TryParse(texts[1], out var size) && size >= 20) {
                                magnifyingGlassSize = size;
                            }
                            break;
                        case "silent":
                            SetSilent(v == "true");
                            break;
                        case "flashInterval":
                        case "blinkInterval":
                            if (float.TryParse(texts[1], out var interval) && interval > 0) {
                                blinkInterval = interval;
                            }
                            break;
                        case "markupType":
                            if (int.TryParse(texts[1], out var mt)) {
                                SetMarkupType(mt);
                            }
                            break;
                        case "hideNumber":
                            /*HideNumber = v == "true" || v == "lock";
                            LockHideNumber = v == "lock";
                            menuItemHideNumber.CheckState = HideNumber ? CheckState.Checked : CheckState.Unchecked;*/
                            break;
                    }
                }
            }
            catch {
            }
            return true;
        }


        private async void TIDWindow_Load(object? sender, EventArgs? e) {
            _ = Task.Run(ClockUpdateLoop);

            TaskDialog.ShowDialog(new TaskDialogPage {
                Caption = $"ご注意 | {SystemNameLong} - ダイヤ運転会",
                Heading = "本ソフト使用上のご注意",
                Icon = TaskDialogIcon.ShieldWarningYellowBar,
                Text = "本ソフトで信号扱いを行わない駅は\n直ちに管轄駅から外してください。\n進行定位の進路が解除されるなど、不具合の原因となります。"
            });


            SetTopMost(topMostSetting);

            //デフォルトのサーバへの接続処理
            serverCommunication = new(this, service);
            serverCommunication.DataUpdated += UpdateServerData;
            LogManager.AddInfoLog($"{(ServerAddress.SignalAddress.Contains("dev") ? "Dev" : "Prod")}サーバに接続します");

            await TryConnectServer();
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
            OpenNavigationWindow();
        }


        /// <summary>
        /// サーバからのデータが更新された際に呼ばれる
        /// </summary>
        /// <param name="tcData"></param>
        private void UpdateServerData(DataToCTCP? data) {
            if (data == null) {
                return;
            }
            DataToCTCP.SetLatest(data);

            ServerTime = data.TimeOffset;
            while (ServerTime < 0) {
                ServerTime += 24;
            }
            var updated = !displayManager.Started/*true*/;

            if (serverCommunication != null) {
                var routes = displayManager.Routes;
                var routeDatas = new List<RouteData>(DataToCTCP.Latest.RouteDatas);
                /*var rrr = routeDatas.FirstOrDefault(r => r.TcName == "TH70_2L");
                var rrrr = routes["TH70_2LT"].FirstOrDefault();
                var tcc = DataToCTCP.Latest.TrackCircuits.FirstOrDefault(t => t.Name == "TH70_2LT");
                if(rrr != null && rrrr != null && tcc != null) {
                    Debug.WriteLine($"TH70_2L: TH70_2LT {tcc.On}  ForcedDrop {rrrr.ForcedDrop}   CTC {rrr.RouteState?.IsCtcRelayRaised}  進路 {rrr.RouteState?.IsRouteLockRaised}  MR {rrr.RouteState?.IsApproachLockMRRaised}");
                }*/
                foreach (var t in DataToCTCP./*Latest.TrackCircuits.Where(t => t.On && routes.ContainsKey(t.Name))*/DifferenceTrack.Where(t => routes.ContainsKey(t.Name))) {
                    var isTrain = int.TryParse(Regex.Replace(t.Last, @"[^0-9]", ""), out var numBody);  // 列番本体（数字部分）
                    if (!isTrain) {
                        continue;
                    }
                    var direction = numBody % 2 == 1 ? "L" : "R";
                    /*var justDrop = previous.TrackCircuits.Any(tp => tp.Name == t.Name && (!tp.On || tp.Name != t.Name));*/
                    foreach (var r in routes[t.Name]) {
                        if (!r.Station.Active || r.IsHikipper || !r.RouteName.Contains(direction)) {
                            continue;
                        }
                        if (routeDatas.Any(rd => {
                            if (rd.TcName != r.RouteName) {
                                return false;
                            }
                            var rs = rd.RouteState;
                            return rs != null /*&& justDrop*/ && rs.IsCtcRelayRaised == RaiseDrop.Raise && (r.ForcedDrop || rs.IsRouteLockRaised == RaiseDrop.Drop && rs.IsApproachLockMRRaised == RaiseDrop.Raise);
                        })) {
                            r.SetHikipper(false);
                            _ = serverCommunication.SetCtcRelay(r.RouteName, RaiseDrop.Drop);
                            Debug.WriteLine($"{DateTime.UtcNow.AddHours(9)} {r.RouteName} を解除しました{(r.IsHikipper ? "(進行定位)" : "")}");
                            NotificationManager.AddNotification($"{r.RouteName} を解除しました{(r.IsHikipper ? "(進行定位)" : "")}", false);
                            NavigationWindow.Instance?.UpdateNotification();
                            updated = true;
                        }
                    }
                }
            }
            updated = updated || DataToCTCP.HasDifference(displayManager);

            /*var tcList = data.TrackCircuitDatas;
            var sList = data.SwitchDatas;
            var dList = data.DirectionDatas;
            var trainList = data.TrainStateDatas;

            var updated = tcList != null && UpdateTrainData(tcList, trainList);
            updated |= tcList != null && trackManager.UpdateTCData(tcList);
            updated |= sList != null && UpdatePointData(sList);
            updated |= dList != null && UpdateDirectionData(dList);
            updated |= trackManager.UpdateNumberWindow();*/

            if (updated) {
                displayManager.UpdateCTCP();
                if (NotificationManager.Updated) {
                    PlayWarningSound();
                }
                NavigationWindow.Instance?.UpdateNotification();
            }
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

            var oldBlinkStateFast = BlinkStateFast;
            var oldBlinkStateSlow = BlinkStateSlow;
            var now = DateTime.UtcNow.AddHours(9);
            var deltaSeconds = (now - RealTime).TotalSeconds;
            RealTime = now;
            if (blinkInterval > 0) {
                blinkState -= (float)deltaSeconds;
                while (blinkState <= 0) {
                    blinkState += blinkInterval * 2;
                }
            }

            if (/*!UpdateDebug() && */displayManager.Started && (ReservedUpdate || (oldBlinkStateFast != BlinkStateFast && displayManager.BlinkingButtons()) /*&& MarkupType < 2 && (trainDataDict.Values.Any(td => td.Markup) || MarkupDuplication || MarkupFillZero || MarkupNotTrain || MarkupDelayed > 0 || displayManager.Markuped)*/)) {
                ReservedUpdate = false;
                displayManager.UpdateCTCP(oldBlinkStateFast != BlinkStateFast, oldBlinkStateSlow != BlinkStateSlow);
            }


            if (usingMagnifyingGlass) {
                var cp1 = pictureBox1.PointToClient(Cursor.Position);
                var cp2 = PointToClient(Cursor.Position);

                if (cp1.X < 0 || cp1.Y < 0 || cp1.X > PictureBoxWidth || cp1.Y > PictureBoxHeight || cp2.X < 0 || cp2.Y < 0 || cp2.X > ClientSize.Width || cp2.Y > ClientSize.Height) {
                    var width = PictureBoxWidth - cp1.X + magnifyingGlassSize / 2;
                    var height = PictureBoxHeight - cp1.Y + magnifyingGlassSize / 2;
                    var mouseX = cp1.X;
                    var mouseY = cp1.Y;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxWidth - cp1.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxHeight - cp1.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(cp1.X, cp1.Y);
                }
            }


            Clock = RealTime;
            if (UseServerTime) {
                TimeOffset = new TimeSpan(ServerTime, 0, 0);
            }
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
                    SetStatusSubWindow("×", Color.Red);
                    Debug.WriteLine($"データ受信不能: {delaySeconds}");
                    if (!Silent) {
                        OpeningDialog = true;
                        TaskDialog.ShowDialog(this, new TaskDialogPage {
                            Caption = $"データ受信不能 | {SystemNameLong} - ダイヤ運転会",
                            Heading = "データ受信不能",
                            Icon = TaskDialogIcon.Error,
                            Text = "サーバ側からのデータ受信が10秒以上ありませんでした。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をおすすめします。"
                        });
                        OpeningDialog = false;
                    }
                    else {
                        PlayWarningSound();
                    }
                }
            }
            else if (delaySeconds > 1) {
                if (!LabelStatusText.Contains("最終受信")) {
                    LogManager.AddWarningLog("サーバからの受信が1秒以上ありません");
                    NotificationManager.AddNotification($"サーバからデータの受信が途切れました。", false);
                    NavigationWindow.Instance?.UpdateNotification();
                }
                LabelStatusText = $"データ正常受信(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                SetStatusSubWindow("▲", Color.Yellow);
                Debug.WriteLine($"データ受信不能: {delaySeconds}");
            }
        }

        private void labelClock_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle) {
                SetUseServerTime(true);
            }
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
            SetUseServerTime(false);
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
            if (NavigationWindow.Instance != null) {
                NavigationWindow.Instance.TopMost = topMost;
            }
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

        private void picturebox1_Enter(object sender, EventArgs e) {
            if (!ModifierKeys.HasFlag(Keys.Control)) {
                displayManager.RelocateButtons();
            }
        }

        private void picturebox1_Leave(object sender, EventArgs e) {
            if (!pictureBox1.ClientRectangle.Contains(pictureBox1.PointToClient(Cursor.Position)) || !panel1.ClientRectangle.Contains(panel1.PointToClient(Cursor.Position))) {
                displayManager.HideButtons();
            }
        }

        private void labelScale_Hover(object sender, EventArgs e) {
            /*displayManager.HideButtons();*/
        }

        private void labelScale_Leave(object sender, EventArgs e) {
            /*displayManager.RelocateButtons();*/
        }


        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (CTCPScale > 0) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    SetScale(-1, false);
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
                        SetScale(scaleArray[i], false);
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
                    SetScale(initialScale, false);
                }
            }
        }


        private void SetScale(int scale, bool relocateButtons = true) {
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

            displayManager.ChangeScale(relocateButtons);
            if (scale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{scale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                lock (displayManager.syncPictureBox) {
                    labelScale.Text = $"Scale：{(int)((double)PictureBoxImage.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
                }
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
                lock (displayManager.syncPictureBox) {
                    displayManager.ChangeScale();
                }

            }
            labelScale.Text = $"Scale：{(int)((double)PictureBoxImage.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
            ChangeDefaultCursor();
        }

        private void SetHourQuick(int hour) {
            SetUseServerTime(false);
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

        private void menuItemMarkupTypeAuto_Click(object sender, EventArgs e) {
            SetMarkupType(-1);
        }

        public void SetMarkupType(int type) {

            MarkupType = type < 3 ? type : 2;
            menuItemMarkupTypeAuto.CheckState = type < 0 ? CheckState.Indeterminate : CheckState.Unchecked;
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

        private void menuItemServerTime_Click(object sender, EventArgs e) {
            SetUseServerTime(!UseServerTime);

        }

        public void SetUseServerTime(bool value) {
            if (serverCommunication != null) {
                UseServerTime = value;
                menuItemServerTime.CheckState = UseServerTime ? CheckState.Checked : CheckState.Unchecked;
                Color color;
                if (UseServerTime) {
                    color = Color.White;
                }
                else {
                    color = Color.Yellow;
                }
                labelClock.ForeColor = color;
                foreach (var w in displayManager.SubWindows) {
                    w.SetClockColor(color);
                }
            }
        }

        private void CTCPWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            /*if ((mod & Keys.Shift) == Keys.Shift) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            else */
            if ((mod & Keys.Control) == Keys.Control) {
                pictureBox1.Cursor = Cursors.Cross;
                displayManager.HideButtons();
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
                    end = new Point(end.X > 10 ? (start.X < PictureBoxWidth && end.X < PictureBoxWidth - 10 ? end.X : PictureBoxWidth) : (start.X > 0 ? 0 : end.X), end.Y > 10 ? (start.Y < PictureBoxHeight && end.Y < PictureBoxHeight - 10 ? end.Y : PictureBoxHeight) : (start.Y > 0 ? 0 : end.Y));
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
            if ((mod & Keys.Control) != Keys.Control && pictureBox1.ClientRectangle.Contains(pictureBox1.PointToClient(Cursor.Position))) {
                displayManager.RelocateButtons();
            }

        }

        private void CTCPWindow_ResizeBegin(object sender, EventArgs e) {
            if (displayManager != null) {
                displayManager.HideButtons();
            }
        }

        private void CTCPWindow_Resize(object sender, EventArgs e) {
            if (displayManager != null && DetectResize) {
                DetectResize = false;
                if (WindowState != FormWindowState.Minimized) {
                    if (CTCPScale == -1 && !FixedScale) {
                        displayManager.ChangeScale(false);
                        lock (displayManager.syncPictureBox) {
                            labelScale.Text = $"Scale：{(int)((double)PictureBoxImage.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
                        }
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
                /*displayManager.RelocateButtons();*/
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
            defaultCursor = CTCPScale == -1 ? Cursors.Default : (PictureBoxWidth < panel1.Width && PictureBoxHeight < panel1.Height ? Cursors.Default : Cursors.SizeAll);
            UpdateMouseCursor();
        }

        private void CTCPWindow_Closing(object sender, EventArgs e) {
            if (LogManager.Output && LogManager.NeededWarning) {
                TaskDialog.ShowDialog(this, new TaskDialogPage {
                    Caption = $"エラーログ出力 | {SystemNameLong} - ダイヤ運転会",
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
                        SetScale(scaleArray[i], false);
                    }
                }
                else {
                    if (FixedScale) {
                        SetFixedScale(false);
                    }
                    var size = Size;
                    var dp = e.Location;
                    var point = ConvertPointToOriginal(dp.X, dp.Y);
                    double rate;
                    lock (displayManager.syncPictureBox) {
                        rate = (PictureBoxImage.Width + e.Delta * 0.2) / displayManager.OriginalWidth;
                    }
                    var width = Size.Width - ClientSize.Width + (int)(displayManager.OriginalWidth * rate);
                    var height = Size.Height - ClientSize.Height + panel1.Location.Y + (int)(displayManager.OriginalHeight * rate);
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
                        height = Size.Height - ClientSize.Height + panel1.Location.Y + displayManager.OriginalHeight * (screenSize.Width - Size.Width + ClientSize.Width) / displayManager.OriginalWidth;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                    else {
                        height = screenSize.Height;
                        width = Size.Width - ClientSize.Width + displayManager.OriginalWidth * (screenSize.Height - Size.Height + ClientSize.Height - panel1.Location.Y) / displayManager.OriginalHeight;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
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
            if (e.Button == MouseButtons.Middle && PictureBoxWidth < displayManager.OriginalWidth) {
                if (usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                }
                else {
                    usingMagnifyingGlass = true;
                    var width = PictureBoxWidth - e.X + magnifyingGlassSize / 2;
                    var height = PictureBoxHeight - e.Y + magnifyingGlassSize / 2;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(e.X - magnifyingGlassSize / 2, e.Y - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxWidth - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxHeight - e.Y + magnifyingGlassSize / 2)));
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
                else */
                if (ModifierKeys.HasFlag(Keys.Control)) {
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
            if (e.Button == MouseButtons.Middle && PictureBoxWidth < displayManager.OriginalWidth) {
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
                if (ModifierKeys.HasFlag(Keys.Control)) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                    selectionStarting = pictureBox1.PointToClient(Cursor.Position);
                }
                else {
                    var b = displayManager.GetButtonInPoint(pictureBox1.PointToClient(Cursor.Position));
                    if (b != null) {
                        pressingButton = b;
                        displayManager.PlayPressButtonSound();
                    }
                }
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e) {
            var b = pressingButton;
            if (b != null && IsInArea(pictureBox1.PointToClient(Cursor.Position), b.Location.X, b.Location.Y, b.Type.Size)) {
                if (displayManager.Started) {
                    b.OnClick();
                    if (b.NeedsUpdate) {
                        ReservedUpdate = true;
                    }
                }
            }
        }

        private void PictureBox2_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = null;
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = pictureBox1.PointToClient(Cursor.Position);
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 16 ? (start.X >= PictureBoxWidth || end.X < PictureBoxWidth - 16 ? end.X : PictureBoxWidth) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= PictureBoxHeight || end.Y < PictureBoxHeight - 16 ? end.Y : PictureBoxHeight) : (start.Y > 0 ? 0 : end.Y));
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
                        sub.SetClockColor(UseServerTime ? Color.White : Color.Yellow);
                        displayManager.AddSubWindow(sub);
                    }
                }
                if (pressingButton != null) {
                    pressingButton = null;
                    displayManager.PlayReleaseButtonSound();
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {
                var width = PictureBoxWidth - e.X + magnifyingGlassSize / 2;
                var height = PictureBoxHeight - e.Y + magnifyingGlassSize / 2;
                var mouseX = e.X;
                var mouseY = e.Y;
                if (width <= 1 || height <= 1) {
                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                }
                else {
                    pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                    pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxWidth - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxHeight - e.Y + magnifyingGlassSize / 2)));
                }

                SetMagnifyingGlass(e.X, e.Y);


            }
            if (mouseLoc.HasValue && !selectionStarting.HasValue && !ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                try {
                    panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Location.X + mouseLoc.Value.X, panel1.VerticalScroll.Value - e.Location.Y + mouseLoc.Value.Y);
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.StackTrace);
                }
            }
            if (selectionStarting.HasValue) {
                var s = selectionStarting.Value;
                selectionStarting = new Point(s.X > 16 ? (s.X < PictureBoxWidth - 16 ? s.X : PictureBoxWidth) : 0, s.Y > 16 ? (s.Y < PictureBoxHeight - 16 ? s.Y : PictureBoxHeight) : 0);
                var start = selectionStarting.Value;
                var end = e.Location;
                var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                end = new Point(end.X > 16 ? (start.X >= PictureBoxWidth || end.X < PictureBoxWidth - 16 ? end.X : PictureBoxWidth) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= PictureBoxHeight || end.Y < PictureBoxHeight - 16 ? end.Y : PictureBoxHeight) : (start.Y > 0 ? 0 : end.Y));
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
                var width = PictureBoxWidth - cp.X + magnifyingGlassSize / 2;
                var height = PictureBoxHeight - cp.Y + magnifyingGlassSize / 2;
                var mouseX = cp.X;
                var mouseY = cp.Y;
                if (width <= 1 || height <= 1) {
                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                }
                else {
                    pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                    pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxWidth - cp.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, PictureBoxHeight - cp.Y + magnifyingGlassSize / 2)));
                }
                UpdateMouseCursor();
                SetMagnifyingGlass(cp.X, cp.Y);
            }
            else if (selectionStarting.HasValue) {
                var s = selectionStarting.Value;
                selectionStarting = new Point(s.X > 16 ? (s.X < PictureBoxWidth - 16 ? s.X : PictureBoxWidth) : 0, s.Y > 16 ? (s.Y < PictureBoxHeight - 16 ? s.Y : PictureBoxHeight) : 0);
                var start = selectionStarting.Value;
                var end = pictureBox1.PointToClient(Cursor.Position);
                var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                end = new Point(end.X > 16 ? (start.X >= PictureBoxWidth || end.X < PictureBoxWidth - 16 ? end.X : PictureBoxWidth) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= PictureBoxHeight || end.Y < PictureBoxHeight - 16 ? end.Y : PictureBoxHeight) : (start.Y > 0 ? 0 : end.Y));
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

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = null;
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = e.Location;
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 16 ? (start.X >= PictureBoxWidth || end.X < PictureBoxWidth - 16 ? end.X : PictureBoxWidth) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= PictureBoxHeight || end.Y < PictureBoxHeight - 16 ? end.Y : PictureBoxHeight) : (start.Y > 0 ? 0 : end.Y));
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
                        sub.SetClockColor(UseServerTime ? Color.White : Color.Yellow);
                        displayManager.AddSubWindow(sub);
                    }
                }
            }
        }

        public void SetMagnifyingGlass(int x, int y) {
            if (usingMagnifyingGlass) {
                lock (pictureBox2) {
                    var posX = magnifyingGlassSize / 2 - x * displayManager.OriginalWidth / PictureBoxWidth;
                    var posY = magnifyingGlassSize / 2 - y * displayManager.OriginalHeight / PictureBoxHeight;
                    posX = posX > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - x : (posX < magnifyingGlassSize / 2 - displayManager.OriginalWidth ? PictureBoxWidth - x + magnifyingGlassSize / 2 - displayManager.OriginalWidth : posX);
                    posY = posY > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - y : (posY < magnifyingGlassSize / 2 - displayManager.OriginalHeight ? PictureBoxHeight - y + magnifyingGlassSize / 2 - displayManager.OriginalHeight : posY);

                    var b = new Bitmap(magnifyingGlassSize, magnifyingGlassSize);
                    var old = pictureBox2.Image;
                    pictureBox2.Image = b;
                    old?.Dispose();
                    using var g = Graphics.FromImage(pictureBox2.Image);
                    GraphicsPath gp = new();
                    gp.AddEllipse(g.VisibleClipBounds);
                    g.Clip = new Region(gp);
                    lock (displayManager.OriginalBitmap) {
                        g.DrawImage(displayManager.OriginalBitmap, posX, posY);
                    }
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
            return new Point(x * displayManager.OriginalWidth / PictureBoxWidth, y * displayManager.OriginalHeight / PictureBoxHeight);
        }

        public Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        public Point ConvertPointToScreen(int x, int y) {
            return new Point(x * PictureBoxWidth / displayManager.OriginalWidth, y * PictureBoxHeight / displayManager.OriginalHeight);
        }

        public Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        public Size ConvertSizeToOriginal(int x, int y) {
            return new Size(x * displayManager.OriginalWidth / PictureBoxWidth, y * displayManager.OriginalHeight / PictureBoxHeight);
        }

        public Size ConvertSizeToOriginal(Size s) {
            return ConvertSizeToOriginal(s.Width, s.Height);
        }

        public Size ConvertSizeToScreen(int x, int y) {
            return new Size(x * PictureBoxWidth / displayManager.OriginalWidth, y * PictureBoxHeight / displayManager.OriginalHeight);
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
            else*/
            if (ModifierKeys.HasFlag(Keys.Control)) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else if (usingMagnifyingGlass) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else if (pictureBox1.Cursor != defaultCursor || pictureBox2.Cursor != Cursors.Cross) {
                pictureBox1.Cursor = defaultCursor;
                pictureBox2.Cursor = Cursors.Cross;
            }
        }


        public void SetStatusSubWindow(string text, Color color) {
            SubWindow.SetStatus(text, color);
            foreach (var w in displayManager.SubWindows) {
                w.UpdateStatus();
            }
        }

        public void OpenNavigationWindow() {
            if (NavigationWindow.Instance != null) {
                NavigationWindow.Instance.Activate();
                return;
            }
            var w = new NavigationWindow(displayManager);
            w.Show();
            w.TopMost = TopMost;
        }

        private void menuItemNavigationWindow_Click(object sender, EventArgs e) {
            OpenNavigationWindow();
        }

        public void MoveScroll(int x, int y) {
            var p = ConvertPointToScreen(x, y);
            var mx = pictureBox1.Size.Width - panel1.Size.Width;
            var my = pictureBox1.Size.Height - panel1.Size.Height;
            panel1.AutoScrollPosition = new Point(mx > p.X ? p.X : (mx + 17), my > p.Y ? p.Y : (my + 17));
        }
    }
}

