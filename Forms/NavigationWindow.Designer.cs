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
            tabControl1 = new TabControl();
            tabHavingStation = new TabPage();
            label1 = new Label();
            tabTrain = new TabPage();
            tabNotification = new TabPage();
            labelNotifications = new Label();
            tabControl1.SuspendLayout();
            tabHavingStation.SuspendLayout();
            tabNotification.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabHavingStation);
            tabControl1.Controls.Add(tabTrain);
            tabControl1.Controls.Add(tabNotification);
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
            label1.Size = new Size(329, 30);
            label1.TabIndex = 0;
            label1.Text = "信号扱いを行う管轄駅を選択してください。\r\n駅名部分クリックで範囲選択できます（ダブルクリックで単駅選択）";
            // 
            // tabTrain
            // 
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
            // NavigationWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 561);
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
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabHavingStation;
        private Label label1;
        private TabPage tabNotification;
        private Label labelNotifications;
        private TabPage tabTrain;
    }
}