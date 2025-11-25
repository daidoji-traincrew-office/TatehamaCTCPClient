using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TatehamaCTCPClient.Buttons;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Forms;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace TatehamaCTCPClient.Manager
{
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

        private readonly Dictionary<string, ButtonType> buttonTypes;

        private readonly Dictionary<string, CTCPButton> buttons = [];

        private readonly Dictionary<string, DestinationButton> destinationButtons = [];

        private readonly Dictionary<string, Panel> buttonPanels = [];

        private readonly Dictionary<string, TrainWindow> trainWindows;

        /// <summary>
        /// 列車番号の色
        /// </summary>
        private readonly Dictionary<string, Color> numColor = [];

        /// <summary>
        /// 列車番号以外の色
        /// </summary>
        private readonly Dictionary<string, Color> colorDict = [];

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
        private CharacterSet middleCharSet;

        private CharacterSet smallCharSet;

        private CharacterSet xsmallCharSet;

        private bool resizing = false;







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
        public Bitmap OriginalBitmap { get; private set; }

        public int OriginalWidth { get; private set; }

        public int OriginalHeight { get; private set; }

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
            buttonTypes = LoadButtonType("buttons_type.tsv");
            LoadRouteButtons("buttons_route.tsv");
            LoadDestinationButtons("buttons_destination.tsv");
            LoadSelectionButtons("buttons_selection.tsv");
            LoadOtherButtons("buttons_others.tsv");
            trainWindows = LoadTrainWindows("trainwindow.tsv");

            StationSettings = stationSettings.AsReadOnly();
            SubWindows = subWindows.AsReadOnly();

            backgroundDefault = Image.FromFile(".\\png\\Background-1.png");
            backgroundImage = Image.FromFile(".\\png\\Background.png");
            buttonsImage = Image.FromFile(".\\png\\buttons.png");

            middleCharSet = new CharacterSet("char_middle.tsv");
            smallCharSet = new CharacterSet("char_small.tsv");
            xsmallCharSet = new CharacterSet("char_xsmall.tsv");



            try {
                using var sr = new StreamReader(".\\tsv\\color_setting.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    var i = 0;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 4) {
                        continue;
                    }

                    var s = texts[0];

                    if (s.Length < 6) {
                        var color = Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]));
                        numColor.Add(s, color);
                    }
                    else {
                        colorDict.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                }
            }
            catch {
            }

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
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), s.Location.X + 3, s.Location.Y, 150, 15);
                    g.DrawString($"{s.Number} {s.Name} 集中", new Font("ＭＳ ゴシック", 16, GraphicsUnit.Pixel), Brushes.White, s.Location.X, s.Location.Y);
                }

                var hover = pictureBox.ClientRectangle.Contains(pictureBox.PointToClient(Cursor.Position)) && window.Panel1.ClientRectangle.Contains(window.Panel1.PointToClient(Cursor.Position));



                var ia = new ImageAttributes();
                ia.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x38, 0x46, 0x72) }]);

                foreach(var b in buttons.Values) {
                    g.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                    if (b.Label.Length > 0) {
                        DrawSmallText(g, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, new());
                    }
                    var p = new Panel();
                    window.Panel1.Controls.Add(p);
                    p.Location = hover ? b.Location : new Point(-100, -100);
                    p.Name = b.Name;
                    p.Size = b.Type.Size;
                    p.Parent = pictureBox;
                    p.Cursor = Cursors.Hand;
                    p.BackColor = Color.Transparent;
                    p.Click += (sender, e) => { b.OnClick(); };
                    buttonPanels.Add(b.Name, p);
                }

                foreach (var b in destinationButtons.Values) {
                    g.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                    middleCharSet.DrawText(g, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, ia);
                    var p = new Panel();
                    window.Panel1.Controls.Add(p);
                    p.Location = hover ? b.Location : new Point(-100, -100);
                    p.Name = b.Name;
                    p.Size = b.Type.Size;
                    p.Parent = pictureBox;
                    p.Cursor = Cursors.Hand;
                    p.BackColor = Color.Transparent;
                    p.Click += (sender, e) => { b.OnClick(); };
                    buttonPanels.Add(b.Name, p);
                }

                var rand = new Random();

                foreach (var w in trainWindows.Values) {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), w.Location.X, w.Location.Y, 67, 13);
                    var randValue = rand.Next(20);
                    if (randValue > 0) {
                        randValue = rand.Next(43);
                        var numHeader = "";
                        var numFooter = "";
                        var numBody = "";
                        if (randValue == 42) {
                            numHeader = "試";
                        }
                        else if (randValue >= 40) {
                            numHeader = "回";
                            if (randValue == 41) {
                                numFooter = "A";
                            }
                        }
                        else if (randValue % 4 == 0) {
                            numHeader = "臨";
                        }
                        switch (randValue / 4) {
                            case 1:
                                numFooter = "C";
                                break;
                            case 2:
                                numFooter = "B";
                                break;
                            case 3:
                                numFooter = "K";
                                break;
                            case 4:
                                numFooter = "A";
                                break;
                        }
                        randValue = rand.Next(12);
                        switch (randValue - 8) {
                            case 1:
                                numFooter += "X";
                                break;
                            case 2:
                                numFooter += "Y";
                                break;
                            case 3:
                                numFooter += "Z";
                                break;
                        }
                        randValue = rand.Next(4, 25) * 100;
                        randValue += rand.Next(3) * 3000;
                        randValue += rand.Next(2, 100);
                        numBody = randValue.ToString();

                        DrawTrainNumber(g, numHeader, numBody, numFooter, w.Location.X, w.Location.Y);

                    }
                    else {
                        var l = middleCharSet.MultiCharacters.Values.Where(s => s.Name.Length > 2).ToArray();
                        randValue = rand.Next(l.Length);
                        var hf = l[randValue].Name;

                        DrawNonTrainNumber(g, l[randValue].Name, w.Location.X, w.Location.Y);
                    }
                }
            }

            OriginalBitmap = new Bitmap(pictureBox.Image);
            OriginalWidth = OriginalBitmap.Width;
            OriginalHeight = OriginalBitmap.Height;
            ChangeScale(false);
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

                    if (texts.Length < 5 || texts.Any(t => t.Length <= 0)) {
                        continue;
                    }

                    list.Add(new StationSetting(texts[0], texts[1], texts[2], new Point(int.Parse(texts[3]), int.Parse(texts[4]))));
                }
            }
            catch {
            }
            return list;
        }

        private Dictionary<string, ButtonType> LoadButtonType(string fileName) {
            Dictionary<string, ButtonType> list = [];
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


                    if (texts.Length < 10 || texts.Any(t => t == "")) {
                        continue;
                    }

                    list.Add(texts[0], new ButtonType(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]), int.Parse(texts[4]), int.Parse(texts[6]), int.Parse(texts[7]), int.Parse(texts[8]), int.Parse(texts[9])));
                }
            }
            catch {
            }
            return list;
        }

        private void LoadRouteButtons(string fileName) {
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                RouteButton? b = null;
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    var i = 0;
                    var isButton = true;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            if(i > 4) {
                                break;
                            }
                            isButton = false;
                        }
                    }
                    if (i < 7) {
                        continue;
                    }
                    if (isButton) {
                        b = new RouteButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], texts[5], texts[6] == "R" ? LCR.Right : LCR.Left);
                        buttons.Add(texts[0], b);
                    }
                    else {
                        b?.AddRoute(texts[5], texts[6] == "R" ? LCR.Right : LCR.Left);
                    }
                }
            }
            catch {
            }
        }

        private void LoadSelectionButtons(string fileName) {
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                SelectionButton? b = null;
                DestinationButton? db = null;
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();



                    var i = 0;
                    var isButton = true;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            if (i > 5) {
                                break;
                            }
                            if(isButton && i > 4) {
                                break;
                            }
                            isButton = false;
                        }
                    }
                    if (i < 8) {
                        continue;
                    }

                    if (texts[5].Length > 0) {
                        db = destinationButtons[texts[5]];
                    }

                    if(db == null) {
                        continue;
                    }

                    if (isButton) {
                        b = new SelectionButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], db, texts[6], texts[7] == "R" ? LCR.Right : LCR.Left);
                        buttons.Add(texts[0], b);
                    }
                    else {
                        b?.AddRoute(db, texts[6], texts[7] == "R" ? LCR.Right : LCR.Left);
                    }
                }
            }
            catch {
            }
        }

        private void LoadDestinationButtons(string fileName) {
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


                    var i = 0;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 5) {
                        continue;
                    }

                    destinationButtons.Add(texts[0], new DestinationButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4]));
                }
            }
            catch {
            }
        }

        private void LoadOtherButtons(string fileName) {
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

                    {
                        var i = 0;
                        for (; i < texts.Length; i++) {
                            if (texts[i] == "") {
                                break;
                            }
                        }
                        if (i < 4) {
                            continue;
                        }
                    }

                    var buttonType = texts[3];

                    switch (buttonType) {
                        case "cancel":
                            buttons.Add(texts[0], new CancelButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[buttonType]));
                            break;
                        case "snk_t":
                            buttons.Add(texts[0], new MaintainingButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[buttonType]));
                            break;
                        case "pnk_yd":
                        case "ylw_yd":
                        case "wht_yd":
                            var l = new List<SelectionButton>();
                            for(var i = 4; i < texts.Length; i++) {
                                var t = texts[i];
                                if (t.Length > 0 && buttons.TryGetValue(t, out var b) && b is SelectionButton) {
                                    l.Add((SelectionButton)b);
                                }
                            }
                            buttons.Add(texts[0], new YudoButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[buttonType], l));
                            break;
                    }
                }
            }
            catch {
            }
        }

        private Dictionary<string, TrainWindow> LoadTrainWindows(string fileName) {
            Dictionary<string, TrainWindow> list = [];
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                TrainWindow? t = null;
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    if (texts.Length < 4 || texts[3].Length <= 0) {
                        continue;
                    }
                    if (texts[0].Length <= 0) {
                        if (t == null) {
                            continue;
                        }
                        t.AddTtcName(texts[3]);
                    }
                    else {
                        t = new TrainWindow(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), texts[3]);
                        list.Add(texts[0], t);
                    }

                }
            }
            catch {
            }
            return list;
        }

        public void ChangeScale(bool relocateButtons = true) {

            try {
                PrepareChangeScale();

                HideButtons();

                lock (OriginalBitmap)
                    lock (pictureBox) {

                        var oldPic = pictureBox.Image;
                        if (oldPic != null) {
                            if (window.CTCPScale < 0) {
                                var aspectRatio = (double)OriginalWidth / OriginalHeight;
                                if (aspectRatio < (double)pictureBox.Width / pictureBox.Height) {
                                    var width = (int)(pictureBox.Height * aspectRatio);
                                    pictureBox.Image = new Bitmap(OriginalBitmap, width, pictureBox.Height);
                                    pictureBox.Width = width;
                                }
                                else {
                                    var height = (int)(pictureBox.Width / aspectRatio);
                                    pictureBox.Image = new Bitmap(OriginalBitmap, pictureBox.Width, height);
                                    pictureBox.Height = height;
                                }
                            }
                            else {
                                pictureBox.Image = new Bitmap(OriginalBitmap, OriginalWidth * window.CTCPScale / 100, OriginalHeight * window.CTCPScale / 100);
                            }
                            oldPic.Dispose();
                        }
                        if (relocateButtons) {
                            RelocateButtons();
                        }
                    }
            }
            catch (Exception e) {
                LogManager.AddExceptionLog(e);
                LogManager.OutputLog(true);
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    window.LabelStatusText = "描画エラー";
                    window.SetStatusSubWindow("×", Color.Red);
                    TaskDialog.ShowDialog(new TaskDialogPage {
                        Caption = "描画エラー | TID - ダイヤ運転会",
                        Heading = "描画エラー",
                        Icon = TaskDialogIcon.Error,
                        Text = "TID画面の描画に失敗しました。\nTID製作者に状況を報告願います。"
                    });
                }
            }

        }

        public void HideButtons() {
            if (!resizing) {
                resizing = true;
                foreach (var bp in buttonPanels.Values) {
                    bp.Location = new Point(-100, -100);
                    bp.Size = new Size(1, 1);
                }
            }
        }

        public void RelocateButtons() {
            foreach (var bp in buttonPanels.Values) {
                if (buttons.TryGetValue(bp.Name, out CTCPButton? b)) {
                    bp.Size = ConvertSizeToScreen(b.Type.Size);
                    bp.Location = ConvertPointToScreen(b.Location);
                }
                else if(destinationButtons.TryGetValue(bp.Name, out DestinationButton? db)) {
                    bp.Size = ConvertSizeToScreen(db.Type.Size);
                    bp.Location = ConvertPointToScreen(db.Location);
                }

            }
            resizing = false;
        }

        private void PrepareChangeScale() {
            if (window.WindowState == FormWindowState.Minimized) {
                return;
            }
            var dr = window.DetectResize;
            window.DetectResize = false;
            int width, height;
            width = OriginalWidth * window.CTCPScale / 100;
            height = OriginalHeight * window.CTCPScale / 100;

            if (window.CTCPScale < 0) {
                width = OriginalWidth * 2;
                height = OriginalHeight * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, OriginalWidth) + window.Size.Width - window.ClientSize.Width, Math.Max(height, OriginalHeight) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);

            if (-window.Location.X > window.Size.Width - 60) {
                window.Location = new Point(0, 80);
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

        public void DrawTrainNumber(Graphics g, string numHeader, string numBody, string numFooter, int x, int y, bool fill = false) {
            if (fill) {
                g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), x, y, 67, 13);
            }


            // 種別色
            Color? classColor = null;
            var hf = $"{numHeader}{numFooter}";
            foreach (var k in numColor.Keys) {
                if (hf.Contains(k)) {
                    classColor = numColor[k];
                    break;
                }
            }
            if (classColor == null) {
                classColor = Color.White;
            }
            var iaType = new ImageAttributes();
            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = classColor.Value }]);

            middleCharSet.DrawText(g, numHeader, x + 1, y + 1, 11, 11, iaType, ContentAlignment.MiddleLeft);
            middleCharSet.DrawText(g, numBody, x + 13, y + 1, 39, 11, iaType, ContentAlignment.MiddleRight);
            middleCharSet.DrawText(g, numFooter, x + 53, y + 1, 13, 11, iaType, ContentAlignment.MiddleLeft);
        }

        public void DrawNonTrainNumber(Graphics g, string number, int x, int y, bool fill = false) {
            if (fill) {
                g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), x, y, 67, 13);
            }

            // 種別色
            Color? classColor = null;
            foreach (var k in numColor.Keys) {
                if (number.Contains(k)) {
                    classColor = numColor[k];
                    break;
                }
            }
            if (classColor == null) {
                if (colorDict.ContainsKey("UNKNOWN")) {
                    classColor = colorDict["UNKNOWN"];
                }
                else {
                    classColor = Color.FromArgb(129, 129, 129);
                }
            }
            var iaType = new ImageAttributes();
            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = classColor.Value }]);

            middleCharSet.DrawText(g, number, x + 1, y + 1, 65, 11, iaType, ContentAlignment.MiddleLeft);
        }

        public void DrawTrainNumber(Graphics g, string number, int x, int y, bool fill) {
            if (fill) {
                g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), x, y, 67, 13);
            }

            var numHeader = Regex.Replace(number, @"[0-9a-zA-Z]", "");  // 列番の頭の文字（回、試など）
            var numBodyStr = Regex.Replace(number, @"[^0-9]", "");
            var isTrain = int.TryParse(numBodyStr, out var numBody);  // 列番本体（数字部分）
            var numFooter = Regex.Replace(number, @"[^a-zA-Z]", "");  // 列番の末尾の文字

            if (isTrain) {
                DrawTrainNumber(g, numHeader, numBodyStr, numFooter, x, y);
            }
            else {
                DrawNonTrainNumber(g, number, x, y);
            }
        }



        public void CopyImage() {
            lock (OriginalBitmap) {
                var i = new Bitmap(OriginalBitmap);
                using (var g = Graphics.FromImage(i)) {
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 12, GraphicsUnit.Pixel), Brushes.White, OriginalWidth - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();

            }
        }

        public void CopyImage(int x, int y, int width, int height) {
            lock (OriginalBitmap) {
                var i = new Bitmap(width, height + 13);
                using (var g = Graphics.FromImage(i)) {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.DrawImage(OriginalBitmap, new Rectangle(0, 13, width, height), x, y, width, height, GraphicsUnit.Pixel);
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
            return new Point(x * OriginalWidth / pictureBox.Width, y * OriginalHeight / pictureBox.Height);
        }

        public Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        public Point ConvertPointToScreen(int x, int y) {
            return new Point(x * pictureBox.Width / OriginalWidth, y * pictureBox.Height / OriginalHeight);
        }

        public Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        public Size ConvertSizeToOriginal(int x, int y) {
            return new Size(x * OriginalWidth / pictureBox.Width, y * OriginalHeight / pictureBox.Height);
        }

        public Size ConvertSizeToOriginal(Size s) {
            return ConvertSizeToOriginal(s.Width, s.Height);
        }

        public Size ConvertSizeToScreen(int x, int y) {
            return new Size(x * pictureBox.Width / OriginalWidth, y * pictureBox.Height / OriginalHeight);
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

        public List<CTCPButton> GetButtonInArea(Point location, Size size) {
            var l = new List<CTCPButton>();
            l.AddRange(buttons.Values.Where(b => b.Location.X > location.X - b.Type.Size.Width && b.Location.Y > location.Y - b.Type.Size.Height && b.Location.X < location.X + size.Width && b.Location.Y < location.Y + size.Height));
            l.AddRange(destinationButtons.Values.Where(b => b.Location.X > location.X - b.Type.Size.Width && b.Location.Y > location.Y - b.Type.Size.Height && b.Location.X < location.X + size.Width && b.Location.Y < location.Y + size.Height));
            return l;
        }
    }
}
