using TatehamaCTCPClient.Manager;

namespace TatehamaCTCPClient.Forms {
    public partial class NavigationWindow : Form {
        public static NavigationWindow? Instance { get; private set; } = null;

        private CTCPManager displayManager;

        private List<Panel> panelStations = [];

        private List<Label> labelStations = [];

        private List<CheckBox> checkBoxStations = [];

        private int selectedStationIndex = -1;

        public NavigationWindow(CTCPManager displayManager) {
            Instance = this;
            InitializeComponent();
            this.displayManager = displayManager;
            PlaceStations();
        }

        private void PlaceStations() {
            var stations = displayManager.StationSettings;
            for (var i = 0; i < stations.Count; i++) {
                var s = stations[i];
                var p = new Panel();
                var l = new Label();
                var c = new CheckBox();
                panelStations.Add(p);
                labelStations.Add(l);
                checkBoxStations.Add(c);
                tabPage1.Controls.Add(p);
                p.SuspendLayout();
                p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                p.BackColor = s.Active ? Color.LightBlue : SystemColors.ControlLight;
                p.Controls.Add(l);
                p.Controls.Add(c);
                p.Location = new Point(8, label1.Location.Y + label1.Size.Height + 5 + i * 36);
                p.Name = $"panel{s.Code}";
                p.Size = new Size(360, 35);
                p.TabIndex = 0;
                l.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                l.BackColor = Color.Transparent;
                l.Font = new Font("Yu Gothic UI", 11F);
                l.Location = new Point(32, 0);
                l.Name = $"label{s.Code}";
                l.Size = new Size(328, 35);
                l.TabIndex = 1;
                l.Text = s.Name;
                l.TextAlign = ContentAlignment.MiddleLeft;
                l.Click += (sender, e) => {
                    var i = labelStations.IndexOf(l);
                    if (selectedStationIndex < 0) {
                        selectedStationIndex = i;
                        panelStations[i].BackColor = Color.LightSkyBlue;
                    }
                    else {
                        var start = Math.Min(i, selectedStationIndex);
                        var end = Math.Max(i, selectedStationIndex);
                        for(var j = 0; j < checkBoxStations.Count; j++) {
                            panelStations[j].BackColor = j >= start && j <= end ? Color.LightBlue : SystemColors.ControlLight;
                            checkBoxStations[j].Checked = j >= start && j <= end;
                        }
                    }
                };
                l.DoubleClick += (sender, e) => {
                    var i = labelStations.IndexOf(l);
                    for (var j = 0; j < checkBoxStations.Count; j++) {
                        panelStations[j].BackColor = j == i ? Color.LightBlue : SystemColors.ControlLight;
                        checkBoxStations[j].Checked = j == i;
                    }
                };
                c.AutoSize = false;
                c.Location = new Point(9, 0);
                c.Name = $"checkBox{s.Code}";
                c.Size = new Size(23, 35);
                c.Checked = s.Active;
                c.CheckedChanged += (sender, e) => {
                    selectedStationIndex = -1;
                    s.SetActive(c.Checked);
                    p.BackColor = s.Active ? Color.LightBlue : SystemColors.ControlLight;
                };
                c.TabIndex = 0;
                c.UseVisualStyleBackColor = true;
                p.ResumeLayout(false);
                p.PerformLayout();
            }
        }
        private void NavigationWindow_Closing(object sender, FormClosingEventArgs e) {
            Instance = null;
        }
    }
}
