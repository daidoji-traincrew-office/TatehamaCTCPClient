namespace TatehamaCTCPClient.Forms {
    partial class NavigationWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            tabControl1 = new TabControl();
            tabHavingStation = new TabPage();
            label1 = new Label();
            tabTrain = new TabPage();
            tabNotification = new TabPage();
            labelNotifications = new Label();
            tabVolume = new TabPage();
            labelVolumeAprPi = new Label();
            buttonMuteAprPi = new Button();
            trackBarVolumeAprPi = new TrackBar();
            labelVolumeAprUp = new Label();
            buttonMuteAprUp = new Button();
            trackBarVolumeAprUp = new TrackBar();
            labelVolumeAprDown = new Label();
            buttonMuteAprDown = new Button();
            trackBarVolumeAprDown = new TrackBar();
            labelVolumeSpawn = new Label();
            buttonMuteSpawn = new Button();
            trackBarVolumeSpawn = new TrackBar();
            labelVolumeWarning = new Label();
            buttonMuteWarning = new Button();
            trackBarVolumeWarning = new TrackBar();
            labelVolumeMaster = new Label();
            buttonMuteMaster = new Button();
            trackBarVolumeMaster = new TrackBar();
            toolTip1 = new ToolTip(components);
            checkBoxTopMost = new CheckBox();
            tabControl1.SuspendLayout();
            tabHavingStation.SuspendLayout();
            tabNotification.SuspendLayout();
            tabVolume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprPi).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprUp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeSpawn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeWarning).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeMaster).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabHavingStation);
            tabControl1.Controls.Add(tabTrain);
            tabControl1.Controls.Add(tabNotification);
            tabControl1.Controls.Add(tabVolume);
            tabControl1.Location = new Point(0, 24);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(384, 537);
            tabControl1.TabIndex = 0;
            // 
            // tabHavingStation
            // 
            tabHavingStation.AutoScroll = true;
            tabHavingStation.BackColor = SystemColors.Control;
            tabHavingStation.Controls.Add(label1);
            tabHavingStation.Location = new Point(4, 24);
            tabHavingStation.Name = "tabHavingStation";
            tabHavingStation.Padding = new Padding(3);
            tabHavingStation.Size = new Size(376, 509);
            tabHavingStation.TabIndex = 0;
            tabHavingStation.Text = "管轄駅選択";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 6);
            label1.Name = "label1";
            label1.Size = new Size(345, 45);
            label1.TabIndex = 0;
            label1.Text = "本ソフトで信号扱いを行う管轄駅を選択してください。\r\n駅名部分クリックで範囲選択できます（ダブルクリックで単駅選択）\r\n！本ソフトで信号扱いを行わない駅は直ちに管轄駅から外してください！";
            // 
            // tabTrain
            // 
            tabTrain.AutoScroll = true;
            tabTrain.Location = new Point(4, 24);
            tabTrain.Name = "tabTrain";
            tabTrain.Padding = new Padding(3);
            tabTrain.Size = new Size(376, 509);
            tabTrain.TabIndex = 2;
            tabTrain.Text = "列車運行情報";
            tabTrain.UseVisualStyleBackColor = true;
            // 
            // tabNotification
            // 
            tabNotification.AutoScroll = true;
            tabNotification.Controls.Add(labelNotifications);
            tabNotification.Location = new Point(4, 24);
            tabNotification.Name = "tabNotification";
            tabNotification.Padding = new Padding(3);
            tabNotification.Size = new Size(376, 509);
            tabNotification.TabIndex = 1;
            tabNotification.Text = "通知";
            tabNotification.UseVisualStyleBackColor = true;
            // 
            // labelNotifications
            // 
            labelNotifications.AutoSize = true;
            labelNotifications.Location = new Point(4, 4);
            labelNotifications.Name = "labelNotifications";
            labelNotifications.Size = new Size(88, 15);
            labelNotifications.TabIndex = 0;
            labelNotifications.Text = "通知がありません";
            // 
            // tabVolume
            // 
            tabVolume.AutoScroll = true;
            tabVolume.BackColor = SystemColors.Control;
            tabVolume.Controls.Add(labelVolumeAprPi);
            tabVolume.Controls.Add(buttonMuteAprPi);
            tabVolume.Controls.Add(trackBarVolumeAprPi);
            tabVolume.Controls.Add(labelVolumeAprUp);
            tabVolume.Controls.Add(buttonMuteAprUp);
            tabVolume.Controls.Add(trackBarVolumeAprUp);
            tabVolume.Controls.Add(labelVolumeAprDown);
            tabVolume.Controls.Add(buttonMuteAprDown);
            tabVolume.Controls.Add(trackBarVolumeAprDown);
            tabVolume.Controls.Add(labelVolumeSpawn);
            tabVolume.Controls.Add(buttonMuteSpawn);
            tabVolume.Controls.Add(trackBarVolumeSpawn);
            tabVolume.Controls.Add(labelVolumeWarning);
            tabVolume.Controls.Add(buttonMuteWarning);
            tabVolume.Controls.Add(trackBarVolumeWarning);
            tabVolume.Controls.Add(labelVolumeMaster);
            tabVolume.Controls.Add(buttonMuteMaster);
            tabVolume.Controls.Add(trackBarVolumeMaster);
            tabVolume.Location = new Point(4, 24);
            tabVolume.Name = "tabVolume";
            tabVolume.Padding = new Padding(3);
            tabVolume.Size = new Size(376, 509);
            tabVolume.TabIndex = 3;
            tabVolume.Text = "音量設定";
            // 
            // labelVolumeAprPi
            // 
            labelVolumeAprPi.AutoSize = true;
            labelVolumeAprPi.ForeColor = Color.Black;
            labelVolumeAprPi.Location = new Point(30, 425);
            labelVolumeAprPi.Name = "labelVolumeAprPi";
            labelVolumeAprPi.Size = new Size(67, 15);
            labelVolumeAprPi.TabIndex = 17;
            labelVolumeAprPi.Text = "接近警告音";
            // 
            // buttonMuteAprPi
            // 
            buttonMuteAprPi.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteAprPi.Location = new Point(275, 418);
            buttonMuteAprPi.Name = "buttonMuteAprPi";
            buttonMuteAprPi.Size = new Size(70, 28);
            buttonMuteAprPi.TabIndex = 16;
            buttonMuteAprPi.Text = "ミュート";
            buttonMuteAprPi.UseVisualStyleBackColor = true;
            buttonMuteAprPi.Click += buttonMuteAprPi_Click;
            // 
            // trackBarVolumeAprPi
            // 
            trackBarVolumeAprPi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeAprPi.LargeChange = 2;
            trackBarVolumeAprPi.Location = new Point(25, 450);
            trackBarVolumeAprPi.Maximum = 100;
            trackBarVolumeAprPi.Name = "trackBarVolumeAprPi";
            trackBarVolumeAprPi.Size = new Size(327, 45);
            trackBarVolumeAprPi.TabIndex = 15;
            trackBarVolumeAprPi.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeAprPi, "100");
            trackBarVolumeAprPi.Value = 100;
            trackBarVolumeAprPi.ValueChanged += trackBarVolumeAprPi_ValueChanged;
            trackBarVolumeAprPi.MouseUp += trackBarVolumeAprPi_MouseUp;
            // 
            // labelVolumeAprUp
            // 
            labelVolumeAprUp.AutoSize = true;
            labelVolumeAprUp.ForeColor = Color.Black;
            labelVolumeAprUp.Location = new Point(30, 345);
            labelVolumeAprUp.Name = "labelVolumeAprUp";
            labelVolumeAprUp.Size = new Size(63, 15);
            labelVolumeAprUp.TabIndex = 14;
            labelVolumeAprUp.Text = "上り接近音";
            // 
            // buttonMuteAprUp
            // 
            buttonMuteAprUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteAprUp.Location = new Point(275, 338);
            buttonMuteAprUp.Name = "buttonMuteAprUp";
            buttonMuteAprUp.Size = new Size(70, 28);
            buttonMuteAprUp.TabIndex = 13;
            buttonMuteAprUp.Text = "ミュート";
            buttonMuteAprUp.UseVisualStyleBackColor = true;
            buttonMuteAprUp.Click += buttonMuteAprUp_Click;
            // 
            // trackBarVolumeAprUp
            // 
            trackBarVolumeAprUp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeAprUp.LargeChange = 2;
            trackBarVolumeAprUp.Location = new Point(25, 370);
            trackBarVolumeAprUp.Maximum = 100;
            trackBarVolumeAprUp.Name = "trackBarVolumeAprUp";
            trackBarVolumeAprUp.Size = new Size(327, 45);
            trackBarVolumeAprUp.TabIndex = 12;
            trackBarVolumeAprUp.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeAprUp, "100");
            trackBarVolumeAprUp.Value = 100;
            trackBarVolumeAprUp.ValueChanged += trackBarVolumeAprUp_ValueChanged;
            trackBarVolumeAprUp.MouseUp += trackBarVolumeAprUp_MouseUp;
            // 
            // labelVolumeAprDown
            // 
            labelVolumeAprDown.AutoSize = true;
            labelVolumeAprDown.ForeColor = Color.Black;
            labelVolumeAprDown.Location = new Point(30, 265);
            labelVolumeAprDown.Name = "labelVolumeAprDown";
            labelVolumeAprDown.Size = new Size(63, 15);
            labelVolumeAprDown.TabIndex = 11;
            labelVolumeAprDown.Text = "下り接近音";
            // 
            // buttonMuteAprDown
            // 
            buttonMuteAprDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteAprDown.Location = new Point(275, 258);
            buttonMuteAprDown.Name = "buttonMuteAprDown";
            buttonMuteAprDown.Size = new Size(70, 28);
            buttonMuteAprDown.TabIndex = 10;
            buttonMuteAprDown.Text = "ミュート";
            buttonMuteAprDown.UseVisualStyleBackColor = true;
            buttonMuteAprDown.Click += buttonMuteAprDown_Click;
            // 
            // trackBarVolumeAprDown
            // 
            trackBarVolumeAprDown.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeAprDown.LargeChange = 2;
            trackBarVolumeAprDown.Location = new Point(25, 290);
            trackBarVolumeAprDown.Maximum = 100;
            trackBarVolumeAprDown.Name = "trackBarVolumeAprDown";
            trackBarVolumeAprDown.Size = new Size(327, 45);
            trackBarVolumeAprDown.TabIndex = 9;
            trackBarVolumeAprDown.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeAprDown, "100");
            trackBarVolumeAprDown.Value = 100;
            trackBarVolumeAprDown.ValueChanged += trackBarVolumeAprDown_ValueChanged;
            trackBarVolumeAprDown.MouseUp += trackBarVolumeAprDown_MouseUp;
            // 
            // labelVolumeSpawn
            // 
            labelVolumeSpawn.AutoSize = true;
            labelVolumeSpawn.ForeColor = Color.Black;
            labelVolumeSpawn.Location = new Point(30, 185);
            labelVolumeSpawn.Name = "labelVolumeSpawn";
            labelVolumeSpawn.Size = new Size(43, 15);
            labelVolumeSpawn.TabIndex = 8;
            labelVolumeSpawn.Text = "入線音";
            // 
            // buttonMuteSpawn
            // 
            buttonMuteSpawn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteSpawn.Location = new Point(275, 178);
            buttonMuteSpawn.Name = "buttonMuteSpawn";
            buttonMuteSpawn.Size = new Size(70, 28);
            buttonMuteSpawn.TabIndex = 7;
            buttonMuteSpawn.Text = "ミュート";
            buttonMuteSpawn.UseVisualStyleBackColor = true;
            buttonMuteSpawn.Click += buttonMuteSpawn_Click;
            // 
            // trackBarVolumeSpawn
            // 
            trackBarVolumeSpawn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeSpawn.LargeChange = 2;
            trackBarVolumeSpawn.Location = new Point(25, 210);
            trackBarVolumeSpawn.Maximum = 100;
            trackBarVolumeSpawn.Name = "trackBarVolumeSpawn";
            trackBarVolumeSpawn.Size = new Size(327, 45);
            trackBarVolumeSpawn.TabIndex = 6;
            trackBarVolumeSpawn.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeSpawn, "100");
            trackBarVolumeSpawn.Value = 100;
            trackBarVolumeSpawn.ValueChanged += trackBarVolumeSpawn_ValueChanged;
            trackBarVolumeSpawn.MouseUp += trackBarVolumeSpawn_MouseUp;
            // 
            // labelVolumeWarning
            // 
            labelVolumeWarning.AutoSize = true;
            labelVolumeWarning.ForeColor = Color.Black;
            labelVolumeWarning.Location = new Point(30, 105);
            labelVolumeWarning.Name = "labelVolumeWarning";
            labelVolumeWarning.Size = new Size(74, 15);
            labelVolumeWarning.TabIndex = 5;
            labelVolumeWarning.Text = "警告・エラー音";
            // 
            // buttonMuteWarning
            // 
            buttonMuteWarning.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteWarning.Location = new Point(275, 98);
            buttonMuteWarning.Name = "buttonMuteWarning";
            buttonMuteWarning.Size = new Size(70, 28);
            buttonMuteWarning.TabIndex = 4;
            buttonMuteWarning.Text = "ミュート";
            buttonMuteWarning.UseVisualStyleBackColor = true;
            buttonMuteWarning.Click += buttonMuteWarning_Click;
            // 
            // trackBarVolumeWarning
            // 
            trackBarVolumeWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeWarning.LargeChange = 2;
            trackBarVolumeWarning.Location = new Point(25, 130);
            trackBarVolumeWarning.Maximum = 100;
            trackBarVolumeWarning.Name = "trackBarVolumeWarning";
            trackBarVolumeWarning.Size = new Size(327, 45);
            trackBarVolumeWarning.TabIndex = 3;
            trackBarVolumeWarning.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeWarning, "100");
            trackBarVolumeWarning.Value = 100;
            trackBarVolumeWarning.ValueChanged += trackBarVolumeWarning_ValueChanged;
            trackBarVolumeWarning.MouseUp += trackBarVolumeWarning_MouseUp;
            // 
            // labelVolumeMaster
            // 
            labelVolumeMaster.AutoSize = true;
            labelVolumeMaster.ForeColor = Color.Black;
            labelVolumeMaster.Location = new Point(30, 25);
            labelVolumeMaster.Name = "labelVolumeMaster";
            labelVolumeMaster.Size = new Size(66, 15);
            labelVolumeMaster.TabIndex = 2;
            labelVolumeMaster.Text = "マスター音量";
            // 
            // buttonMuteMaster
            // 
            buttonMuteMaster.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteMaster.Location = new Point(275, 18);
            buttonMuteMaster.Name = "buttonMuteMaster";
            buttonMuteMaster.Size = new Size(70, 28);
            buttonMuteMaster.TabIndex = 1;
            buttonMuteMaster.Text = "ミュート";
            buttonMuteMaster.UseVisualStyleBackColor = true;
            buttonMuteMaster.Click += buttonMuteMaster_Click;
            // 
            // trackBarVolumeMaster
            // 
            trackBarVolumeMaster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeMaster.LargeChange = 2;
            trackBarVolumeMaster.Location = new Point(25, 50);
            trackBarVolumeMaster.Maximum = 100;
            trackBarVolumeMaster.Name = "trackBarVolumeMaster";
            trackBarVolumeMaster.Size = new Size(327, 45);
            trackBarVolumeMaster.TabIndex = 0;
            trackBarVolumeMaster.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeMaster, "100");
            trackBarVolumeMaster.Value = 100;
            trackBarVolumeMaster.ValueChanged += trackBarVolumeMaster_ValueChanged;
            trackBarVolumeMaster.MouseUp += trackBarVolumeMaster_MouseUp;
            // 
            // toolTip1
            // 
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 100;
            // 
            // checkBoxTopMost
            // 
            checkBoxTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkBoxTopMost.AutoSize = true;
            checkBoxTopMost.Location = new Point(320, 1);
            checkBoxTopMost.Name = "checkBoxTopMost";
            checkBoxTopMost.Size = new Size(62, 19);
            checkBoxTopMost.TabIndex = 1;
            checkBoxTopMost.Text = "最前面";
            checkBoxTopMost.UseVisualStyleBackColor = true;
            checkBoxTopMost.CheckedChanged += checkBoxTopMost_CheckedChanged;
            // 
            // NavigationWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 561);
            Controls.Add(checkBoxTopMost);
            Controls.Add(tabControl1);
            MaximizeBox = false;
            MaximumSize = new Size(1000, 800);
            MinimumSize = new Size(200, 200);
            Name = "NavigationWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ナビゲーション | CTCP - ダイヤ運転会";
            FormClosing += NavigationWindow_Closing;
            tabControl1.ResumeLayout(false);
            tabHavingStation.ResumeLayout(false);
            tabHavingStation.PerformLayout();
            tabNotification.ResumeLayout(false);
            tabNotification.PerformLayout();
            tabVolume.ResumeLayout(false);
            tabVolume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprPi).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprUp).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeAprDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeSpawn).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeWarning).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeMaster).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabHavingStation;
        private Label label1;
        private TabPage tabNotification;
        private Label labelNotifications;
        private TabPage tabTrain;
        private TabPage tabVolume;
        private TrackBar trackBarVolumeMaster;
        private ToolTip toolTip1;
        private Button buttonMuteMaster;
        private Label labelVolumeMaster;
        private Label labelVolumeWarning;
        private Button buttonMuteWarning;
        private TrackBar trackBarVolumeWarning;
        private Label labelVolumeSpawn;
        private Button buttonMuteSpawn;
        private TrackBar trackBarVolumeSpawn;
        private Label labelVolumeAprDown;
        private Button buttonMuteAprDown;
        private TrackBar trackBarVolumeAprDown;
        private Label labelVolumeAprUp;
        private Button buttonMuteAprUp;
        private TrackBar trackBarVolumeAprUp;
        private Label labelVolumeAprPi;
        private Button buttonMuteAprPi;
        private TrackBar trackBarVolumeAprPi;
        private CheckBox checkBoxTopMost;
    }
}