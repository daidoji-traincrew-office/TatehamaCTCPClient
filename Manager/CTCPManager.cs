using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Media;
using System.Text.RegularExpressions;
using TatehamaCTCPClient.Buttons;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Forms;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

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

        private readonly Dictionary<string, PictureBox> buttonPanels = [];

        private readonly Dictionary<string, TrainWindow> trainWindows;

        private readonly Dictionary<string, List<Route>> routes = [];

        private readonly Dictionary<string, List<Route>> routeGroups = [];

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

        private readonly object syncButtonsImage = new object();


        private CharacterSet middleCharSet;

        private CharacterSet smallCharSet;

        private CharacterSet xsmallCharSet;

        private SoundPlayer? pressButtonSound = null;

        private SoundPlayer? releaseButtonSound = null;

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

        public readonly object syncPictureBox = new object();

        private int pictureBoxWidth;

        public int PictureBoxWidth {
            get {
                return pictureBoxWidth;
            }
            private set {
                pictureBox.Width = value;
                pictureBoxWidth = value;
            }
        }

        private int pictureBoxHeight;

        public int PictureBoxHeight {
            get {
                return pictureBoxHeight;
            }
            private set {
                pictureBox.Height = value;
                pictureBoxHeight = value;
            }
        }

        private Image pictureBoxImage;

        public Image PictureBoxImage {
            get {
                return pictureBox.Image;
            }
            private set {
                var oldPic = pictureBox.Image;
                var oldPic1 = pictureBoxImage;
                pictureBox.Image = value;
                pictureBoxImage = value;
                oldPic?.Dispose();
                oldPic1?.Dispose();
            }
        }

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        public Bitmap OriginalBitmap { get; private set; }

        public int OriginalWidth { get; private set; }

        public int OriginalHeight { get; private set; }

        public ReadOnlyCollection<StationSetting> StationSettings { get; init; }

        public ReadOnlyDictionary<string, List<Route>> Routes { get; init; }

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
            LoadRoutes("routes.tsv");
            buttonTypes = LoadButtonType("buttons_type.tsv");
            LoadRouteButtons("buttons_route.tsv");
            LoadDestinationButtons("buttons_destination.tsv");
            LoadSelectionButtons("buttons_selection.tsv");
            LoadOtherButtons("buttons_others.tsv");
            trainWindows = LoadTrainWindows("trainwindow.tsv");

            StationSettings = stationSettings.AsReadOnly();
            SubWindows = subWindows.AsReadOnly();
            Routes = routes.AsReadOnly();


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




            if (File.Exists(".\\sound\\pressButton.wav")) {
                pressButtonSound = new SoundPlayer(".\\sound\\pressButton.wav");
            }

            if (File.Exists(".\\sound\\releaseButton.wav")) {
                releaseButtonSound = new SoundPlayer(".\\sound\\releaseButton.wav");
            }


            window.Panel1.Size = new Size(window.ClientSize.Width, window.ClientSize.Height - window.Panel1.Location.Y);

            var width = backgroundDefault.Width * window.CTCPScale / 100;
            var height = backgroundDefault.Height * window.CTCPScale / 100;

            if (window.CTCPScale < 0) {
                width = backgroundDefault.Width * 2;
                height = backgroundDefault.Height * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(height, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);


            if (window.CTCPScale < 0) {
                PictureBoxWidth = window.Size.Width - 16;
                PictureBoxHeight = window.Size.Height - 39 - window.Panel1.Location.Y;
                pictureBox.Cursor = Cursors.Default;
            }
            else {
                PictureBoxWidth = width;
                PictureBoxHeight = height;
            }


            pictureBoxImage = new Bitmap(backgroundDefault);
            pictureBox.Image = new Bitmap(backgroundDefault);

            window.Size = new Size(Math.Max(backgroundDefault.Width * window.CTCPScale / 100, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(backgroundDefault.Height * window.CTCPScale / 100, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);

            // 試験表示
            {
                using var gd = Graphics.FromImage(pictureBox.Image);
                using var gi = Graphics.FromImage(backgroundImage);
                foreach (var s in stationSettings) {
                    gd.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), s.LabelLocation.X + 3, s.LabelLocation.Y, 150, 15);
                    gd.DrawString($"{s.Number} {s.Name} 集中", new Font("ＭＳ ゴシック", 16, GraphicsUnit.Pixel), Brushes.White, s.LabelLocation.X, s.LabelLocation.Y);
                }

                var hover = pictureBox.ClientRectangle.Contains(pictureBox.PointToClient(Cursor.Position)) && window.Panel1.ClientRectangle.Contains(window.Panel1.PointToClient(Cursor.Position));



                var iaDest = new ImageAttributes();
                iaDest.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x38, 0x46, 0x72) }]);
                var iaB = new ImageAttributes();
                iaB.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x28, 0x28, 0x28) }]);

                foreach (var b in buttons.Values) {
                    lock (syncButtonsImage) {
                        gd.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y + b.Type.Size.Height + 1, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                        if (b.Label.Length > 0) {
                            DrawSmallText(gd, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, iaB);
                        }
                        gi.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                        if (b.Label.Length > 0) {
                            DrawSmallText(gi, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, new());
                        }
                    }
                    var p = new PictureBox();
                    window.Panel1.Controls.Add(p);
                    p.Location = hover ? b.Location : new Point(-100, -100);
                    p.Name = b.Name;
                    p.Size = b.Type.Size;
                    p.Parent = pictureBox;
                    p.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
                    p.BackColor = Color.White;
                    p.Click += b.NeedsUpdate ? (sender, e) => {
                        if (Started) {
                            b.OnClick();
                            window.ReservedUpdate = true;
                        }
                    }
                    : (sender, e) => {
                        if (Started) {
                            window.ReservedUpdate |= b.OnClick();
                        }
                    };
                    p.MouseDown += (sender, e) => {
                        PlayPressButtonSound();
                    };
                    p.MouseUp += (sender, e) => {
                        PlayReleaseButtonSound();
                    };
                    buttonPanels.Add(b.Name, p);
                }

                foreach (var b in destinationButtons.Values) {
                    lock (syncButtonsImage) {
                        gd.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y + b.Type.Size.Height + 1, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                        middleCharSet.DrawText(gd, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, iaDest);
                        gi.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                        middleCharSet.DrawText(gi, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, iaDest);
                    }
                    var p = new PictureBox();
                    window.Panel1.Controls.Add(p);
                    p.Location = hover ? b.Location : new Point(-100, -100);
                    p.Name = b.Name;
                    p.Size = b.Type.Size;
                    p.Parent = pictureBox;
                    p.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
                    p.BackColor = Color.White;
                    p.Click += (sender, e) => {
                        if (Started) {
                            window.ReservedUpdate |= b.OnClick();
                        }
                    };
                    p.MouseDown += (sender, e) => {
                        PlayPressButtonSound();
                    };
                    p.MouseUp += (sender, e) => {
                        PlayReleaseButtonSound();
                    };
                    buttonPanels.Add(b.Name, p);
                }

                var rand = new Random();

                foreach (var w in trainWindows.Values) {
                    gd.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), w.Location.X, w.Location.Y, 67, 13);
                    var randValue = rand.Next(200);
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

                        DrawTrainNumber(gd, numHeader, numBody, numFooter, w.Location.X, w.Location.Y);

                    }
                    else {
                        var l = middleCharSet.MultiCharacters.Values.Where(s => s.Name.Length > 2).ToArray();
                        randValue = rand.Next(l.Length);
                        var hf = l[randValue].Name;

                        DrawNonTrainNumber(gd, l[randValue].Name, w.Location.X, w.Location.Y);
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

                    if (texts.Length < 10 || texts.Any(t => t.Length <= 0)) {
                        continue;
                    }

                    list.Add(new StationSetting(texts[0], texts[1], texts[2], texts[3], new Point(int.Parse(texts[4]), int.Parse(texts[5])), new Point(int.Parse(texts[6]), int.Parse(texts[7])), new Size(int.Parse(texts[8]), int.Parse(texts[9]))));
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
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
            catch (Exception ex) {
                Debug.WriteLine(ex);
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
                            break;
                        }
                    }
                    if (i < 5) {
                        continue;
                    }
                    StationSetting? station = null;
                    if(texts[0].Length > 0) {
                        station = stationSettings.FirstOrDefault(s => texts[0].Contains(s.Code));
                    }

                    if(station == null) {
                        continue;
                    }

                    var routeName = i == 5 ? "" : texts[5];
                    var route = routeName.Length > 0 ? routes.Values.SelectMany(r => r).FirstOrDefault(r => r.RouteName == routeName) : null;

                    if (routeName.Length <= 0 || route == null) {
                        b = new RouteButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station);
                        buttons.Add(texts[0], b);
                        continue;
                    }


                    if (isButton) {
                        b = new RouteButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station, route);
                        buttons.Add(texts[0], b);
                    }
                    else {
                        b?.AddRoute(route);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        private void LoadSelectionButtons(string fileName) {
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                SelectionButton? b = null;
                DestinationButton? db = null;
                StationSetting? station = null;
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
                            if (isButton && i > 4) {
                                break;
                            }
                            isButton = false;
                        }
                    }
                    if (i < 5) {
                        continue;
                    }

                    if (texts[0].Length > 0) {
                        station = stationSettings.FirstOrDefault(s => texts[0].Contains(s.Code));
                    }

                    if (station == null) {
                        continue;
                    }

                    var routeName = i == 5 ? "" : texts[6];
                    var route = routeName.Length > 0 ? routes.Values.SelectMany(r => r).FirstOrDefault(r => r.RouteName == routeName) : null;

                    if (routeName.Length <= 0 || route == null) {
                        b = new SelectionButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station);
                        buttons.Add(texts[0], b);
                        continue;
                    }

                    if (texts[5].Length > 0) {
                        db = destinationButtons[texts[5]];
                    }

                    if (texts[6].Length <= 0) {
                        continue;
                    }

                    if (db == null) {
                        continue;
                    }

                    var yudoRouteName = texts.Length < 8 ? "" : texts[7];
                    var yudoRoute = routeName.Length > 0 ? routes.Values.SelectMany(r => r).FirstOrDefault(r => r.RouteName == yudoRouteName) : null;

                    if (isButton) {
                        b = new SelectionButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station, db, route, yudoRoute);
                        buttons.Add(texts[0], b);
                    }
                    else {
                        b?.AddRoute(db, route, yudoRoute);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        private void LoadDestinationButtons(string fileName) {
            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                DestinationButton? b = null;
                StationSetting? station = null;
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
                            if (isButton && i > 4) {
                                break;
                            }
                            isButton = false;
                        }
                    }
                    if (i < 5) {
                        continue;
                    }


                    if (texts[0].Length > 0) {
                        station = stationSettings.FirstOrDefault(s => texts[0].Contains(s.Code));
                    }

                    if (station == null) {
                        continue;
                    }

                    var routeName = i == 5 ? "" : texts[5];
                    var route = routeName.Length > 0 ? routes.Values.SelectMany(r => r).FirstOrDefault(r => r.RouteName == routeName) : null;

                    if (routeName.Length <= 0 || route == null) {
                        b = new DestinationButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station);
                        destinationButtons.Add(texts[0], b);
                        continue;
                    }


                    if (isButton) {
                        b = new DestinationButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[texts[3]], texts[4], station, route);
                        destinationButtons.Add(texts[0], b);
                    }
                    else {
                        b?.AddRoute(route);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
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
                            buttons.Add(texts[0], new HikipperButton(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), buttonTypes[buttonType]));
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
            catch (Exception ex) {
                Debug.WriteLine(ex);
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
        private void LoadRoutes(string fileName) {
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
                    if (i < 3) {
                        continue;
                    }
                    StationSetting? station = null;
                    if (texts[0].Length > 0) {
                        station = stationSettings.FirstOrDefault(s => texts[0].Contains(s.Code));
                    }

                    if (station == null) {
                        continue;
                    }
                    var track = texts[2];
                    var group = texts[1];
                    var forcedDrop = i > 3 && texts[3] == "true";
                    var route = new Route(texts[0], track, station, forcedDrop);
                    if (!routes.TryAdd(track, new List<Route>() { route })){
                        routes[track].Add(route);
                    }
                    if (!routeGroups.TryAdd(group, new List<Route>() { route })) {
                        routeGroups[group].Add(route);
                    }


                }
            }
            catch(Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        public void UpdateCTCP(bool updatedBlinkStateFast = false, bool updatedBlinkStateSlow = false) {

            var data = DataToCTCP.Latest;

            var blinkFast = window.BlinkStateFast;

            var blinkSlow = window.BlinkStateSlow;

            Bitmap? newPic = null;
            Image image;
            lock (backgroundImage) {
                newPic = new Bitmap(backgroundImage);
            }

            using var g = Graphics.FromImage(newPic);


            foreach (var s in stationSettings) {
                if(data.CenterControlStates.TryGetValue(s.LeverName, out var state)) {
                    g.DrawString($"{s.Number} {s.Name} {(state == CenterControlState.StationControl ? "駅扱" : "集中")}", new Font("ＭＳ ゴシック", 16, GraphicsUnit.Pixel), s.Active ? (state == CenterControlState.StationControl ? Brushes.LightCoral : Brushes.White) : Brushes.Gray, s.LabelLocation.X, s.LabelLocation.Y);
                }
                else {
                    g.DrawString($"{s.Number} {s.Name} 不明", new Font("ＭＳ ゴシック", 16, GraphicsUnit.Pixel), s.Active ? Brushes.White : Brushes.Gray, s.LabelLocation.X, s.LabelLocation.Y);
                }
            }



            var numSet = middleCharSet.MultiCharacters.Values.Where(s => s.Name.Length > 2).ToArray();


            foreach (var w in trainWindows.Values) {
                var num = "";
                var trainCount = 0;
                foreach(var ttcName in w.TtcNames) {
                    var r = data.Retsubans.FirstOrDefault(r => r.Name == ttcName);
                    if(r == null || r.Retsuban == null || r.Retsuban.Length <= 0) {
                        continue;
                    }
                    num = r.Retsuban;
                    trainCount++;
                }

                if(trainCount > 1) {
                    middleCharSet.DrawText(g, trainCount.ToString(), w.Location.X + 15, w.Location.Y + 1, 11, 11, new ImageAttributes(), ContentAlignment.MiddleLeft);
                    continue;
                }
                else {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), w.Location.X, w.Location.Y, 67, 13);
                }

                if(trainCount <= 0) {
                    continue;
                }


                var numHeader = Regex.Replace(num, @"[0-9a-zA-Z]", "");  // 列番の頭の文字（回、試など）
                var numBodyStr = Regex.Replace(num, @"[^0-9]", "");
                var isTrain = int.TryParse(numBodyStr, out var numBody);  // 列番本体（数字部分）
                var numFooter = Regex.Replace(num, @"[^a-zA-Z]", "");  // 列番の末尾の文字

                var mc = numSet.FirstOrDefault(i => i.Name == num);
                if (mc == null) {
                    DrawTrainNumber(g, numHeader, numBodyStr, numFooter, w.Location.X, w.Location.Y);
                }
                else {
                    DrawNonTrainNumber(g, num, w.Location.X, w.Location.Y);
                }
            }

            var iaDest = new ImageAttributes();
            iaDest.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x38, 0x46, 0x72) }]);
            var iaB = new ImageAttributes();
            iaB.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(0x28, 0x28, 0x28) }]);

            var buttonList = new List<CTCPButton>();

            foreach (var b in buttons.Values) {
                if (b.UpdateLighting()) {
                    buttonList.Add(b);
                }
                else if(updatedBlinkStateFast && b.Lighting == LightingType.BLINKING_FAST || updatedBlinkStateSlow && b.Lighting == LightingType.BLINKING_SLOW) {
                    buttonList.Add(b);
                }
                if(b.Lighting == LightingType.LIGHTING || blinkFast && b.Lighting == LightingType.BLINKING_FAST || blinkSlow && b.Lighting == LightingType.BLINKING_SLOW) {
                    lock (syncButtonsImage) {
                        g.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y + b.Type.Size.Height + 1, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                    }
                    if (b.Label.Length > 0) {
                        DrawSmallText(g, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, iaB);
                    }
                }
            }

            foreach (var b in destinationButtons.Values) {
                if (b.UpdateLighting()) {
                    buttonList.Add(b);
                }
                else if (updatedBlinkStateFast && b.Lighting == LightingType.BLINKING_FAST || updatedBlinkStateSlow && b.Lighting == LightingType.BLINKING_SLOW) {
                    buttonList.Add(b);
                }
                if (b.Lighting == LightingType.LIGHTING || blinkFast && b.Lighting == LightingType.BLINKING_FAST || blinkSlow && b.Lighting == LightingType.BLINKING_SLOW) {
                    lock (syncButtonsImage) {
                        g.DrawImage(buttonsImage, new Rectangle(b.Location.X, b.Location.Y, b.Type.Size.Width, b.Type.Size.Height), b.Type.Location.X, b.Type.Location.Y + b.Type.Size.Height + 1, b.Type.Size.Width, b.Type.Size.Height, GraphicsUnit.Pixel, new());
                    }
                    middleCharSet.DrawText(g, b.Label, b.Location.X + b.Type.LabelPosition.X, b.Location.Y + b.Type.LabelPosition.Y, b.Type.LabelSize.Width, b.Type.LabelSize.Height, iaDest);
                }
            }





            lock (OriginalBitmap)
            lock (syncPictureBox) {
                    var oldOriginal = OriginalBitmap;


                    if (window.CTCPScale < 0) {
                    var aspectRatio = (double)newPic.Width / newPic.Height;
                    if (aspectRatio < (double)PictureBoxWidth / PictureBoxHeight) {
                            image = new Bitmap(newPic, (int)(PictureBoxHeight * aspectRatio), PictureBoxHeight);
                    }
                    else {
                            image = new Bitmap(newPic, PictureBoxWidth, (int)(PictureBoxWidth / aspectRatio));
                    }
                }
                else {
                        image = new Bitmap(newPic, newPic.Width * window.CTCPScale / 100, newPic.Height * window.CTCPScale / 100);
                    }
                    PictureBoxImage = new Bitmap(image);

                    OriginalBitmap = newPic;
                lock (subWindows) {
                    foreach (var sw in subWindows) {
                        sw.UpdateImage(OriginalBitmap, buttonList);
                    }
                    }
                    oldOriginal.Dispose();
                }

            window.SetMagnifyingGlass();

            if (!resizing) {
                lock (syncPictureBox) {
                    try {
                        foreach(var b in buttonList) {
                            if (buttonPanels.TryGetValue(b.Name, out PictureBox? bp)) {
                                var size = ConvertSizeToScreen(b.Type.Size);
                                var loc = ConvertPointToScreen(b.Location);
                                var img = new Bitmap(size.Width, size.Height);
                                using Graphics gp = Graphics.FromImage(img);
                                gp.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), loc.X, loc.Y, size.Width, size.Height, GraphicsUnit.Pixel);
                                var old = bp.Image;
                                bp.Image = img;
                                old?.Dispose();
                            }
                        }
                        /*foreach (var bp in buttonPanels.Values) {
                            if (buttons.TryGetValue(bp.Name, out CTCPButton? b)) {
                                var size = ConvertSizeToScreen(b.Type.Size);
                                var loc = ConvertPointToScreen(b.Location);
                                var img = new Bitmap(size.Width, size.Height);
                                using Graphics gp = Graphics.FromImage(img);
                                gp.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), loc.X, loc.Y, size.Width, size.Height, GraphicsUnit.Pixel);
                                var old = bp.Image;
                                bp.Image = img;
                                old?.Dispose();
                            }
                            else if (destinationButtons.TryGetValue(bp.Name, out DestinationButton? db)) {
                                var size = ConvertSizeToScreen(db.Type.Size);
                                var loc = ConvertPointToScreen(db.Location);
                                var img = new Bitmap(size.Width, size.Height);
                                using Graphics gp = Graphics.FromImage(img);
                                gp.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), loc.X, loc.Y, size.Width, size.Height, GraphicsUnit.Pixel);
                                var old = bp.Image;
                                bp.Image = img;
                                old?.Dispose();
                            }
                        }*/
                    }
                    catch (InvalidOperationException e) {
                        Debug.WriteLine(e.Source);
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                    }
                    finally {
                        image.Dispose();
                    }
                }
            }
            if (!Started) {
                NotificationManager.AddNotification($"サーバからデータの受信を開始しました。", false);
                NavigationWindow.Instance?.UpdateNotification();
                Started = true;
            }

        }

        public void ChangeScale(bool relocateButtons = true) {

            try {
                PrepareChangeScale();

                HideButtons();

                lock (OriginalBitmap)
                lock (syncPictureBox) {

                        if (window.CTCPScale < 0) {
                        var aspectRatio = (double)OriginalWidth / OriginalHeight;
                        if (aspectRatio < (double)PictureBoxWidth / PictureBoxHeight) {
                            var width = (int)(PictureBoxHeight * aspectRatio);
                            PictureBoxImage = new Bitmap(OriginalBitmap, width, PictureBoxHeight);
                            PictureBoxWidth = width;
                        }
                        else {
                            var height = (int)(PictureBoxWidth / aspectRatio);
                            PictureBoxImage = new Bitmap(OriginalBitmap, PictureBoxWidth, height);
                            PictureBoxHeight = height;
                        }
                    }
                    else {
                        PictureBoxImage = new Bitmap(OriginalBitmap, OriginalWidth * window.CTCPScale / 100, OriginalHeight * window.CTCPScale / 100);
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
                    TaskDialog.ShowDialog(window, new TaskDialogPage {
                        Caption = $"描画エラー | {window.SystemNameLong} - ダイヤ運転会",
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
                    bp.Location = new Point(-100 - bp.Width, -100 - bp.Height);
                }
            }
        }

        public void RelocateButtons() {
            Image image;
            lock (syncPictureBox) {
                image = new Bitmap(PictureBoxImage);
            }
            try {
                foreach (var bp in buttonPanels.Values) {
                    if (buttons.TryGetValue(bp.Name, out CTCPButton? b)) {
                        var size = ConvertSizeToScreen(b.Type.Size);
                        var loc = ConvertPointToScreen(b.Location);
                        var img = new Bitmap(size.Width, size.Height);
                        using Graphics g = Graphics.FromImage(img);
                        g.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), loc.X, loc.Y, size.Width, size.Height, GraphicsUnit.Pixel);
                        var old = bp.Image;
                        bp.Image = img;
                        old?.Dispose();
                        bp.Size = size;
                        bp.Location = loc;
                    }
                    else if (destinationButtons.TryGetValue(bp.Name, out DestinationButton? db)) {
                        var size = ConvertSizeToScreen(db.Type.Size);
                        var loc = ConvertPointToScreen(db.Location);
                        var img = new Bitmap(size.Width, size.Height);
                        using Graphics g = Graphics.FromImage(img);
                        g.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), loc.X, loc.Y, size.Width, size.Height, GraphicsUnit.Pixel);
                        var old = bp.Image;
                        bp.Image = img;
                        old?.Dispose();
                        bp.Size = size;
                        bp.Location = loc;
                    }
                }
            }
            catch (InvalidOperationException e) {
                Debug.WriteLine(e.Source);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            finally {
                image.Dispose();
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

            if (window.CTCPScale < 0) {
                PictureBoxWidth = window.ClientSize.Width;
                PictureBoxHeight = window.ClientSize.Height - window.Panel1.Location.Y;
            }
            else {
                PictureBoxWidth = width;
                PictureBoxHeight = height;
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
                    g.DrawString(window.SystemName, new Font("ＭＳ ゴシック", 12, GraphicsUnit.Pixel), Brushes.White, 0, 0);
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
                    g.Clear(Color.FromArgb(45, 15, 15));
                    g.DrawImage(OriginalBitmap, new Rectangle(0, 13, width, height), x, y, width, height, GraphicsUnit.Pixel);
                    g.DrawString(window.SystemName, new Font("ＭＳ ゴシック", 12, GraphicsUnit.Pixel), Brushes.White, 0, 0);
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
            return new Point(x * OriginalWidth / PictureBoxWidth, y * OriginalHeight / PictureBoxHeight);
        }

        public Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        public Point ConvertPointToScreen(int x, int y) {
            return new Point(x * PictureBoxWidth / OriginalWidth, y * PictureBoxHeight / OriginalHeight);
        }

        public Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        public Size ConvertSizeToOriginal(int x, int y) {
            return new Size(x * OriginalWidth / PictureBoxWidth, y * OriginalHeight / PictureBoxHeight);
        }

        public Size ConvertSizeToOriginal(Size s) {
            return ConvertSizeToOriginal(s.Width, s.Height);
        }

        public Size ConvertSizeToScreen(int x, int y) {
            return new Size(x * PictureBoxWidth / OriginalWidth, y * PictureBoxHeight / OriginalHeight);
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

        public CTCPButton? GetButtonInPoint(Point location) {
            foreach(var b in buttons.Values) {
                if(IsInArea(location, b.Location.X, b.Location.Y, b.Type.Size)) {
                    return b;
                }
            }
            foreach (var b in destinationButtons.Values) {
                if (IsInArea(location, b.Location.X, b.Location.Y, b.Type.Size)) {
                    return b;
                }
            }
            return null;
        }

        public void PlayPressButtonSound() {
            pressButtonSound?.Play();
        }

        public void PlayReleaseButtonSound() {
            releaseButtonSound?.Play();
        }

        public bool BlinkingButtons() {
            foreach (var b in buttons.Values) {
                if (b.Lighting == LightingType.BLINKING_SLOW || b.Lighting == LightingType.BLINKING_FAST) {
                    return true;
                }
            }
            foreach (var b in destinationButtons.Values) {
                if (b.Lighting == LightingType.BLINKING_SLOW || b.Lighting == LightingType.BLINKING_FAST) {
                    return true;
                }
            }
            return false;
        }
    }
}
