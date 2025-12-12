namespace TatehamaCTCPClient.Forms {
    partial class CTCPWindow {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            panel1 = new Panel();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            menuItemCopy = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuItemMarkupType = new ToolStripMenuItem();
            menuItemMarkupType1 = new ToolStripMenuItem();
            menuItemMarkupType2 = new ToolStripMenuItem();
            menuItemMarkupType3 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuItemScale = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            menuItemScaleFit = new ToolStripMenuItem();
            menuItemFixedScale = new ToolStripMenuItem();
            menuItemTopMost = new ToolStripMenuItem();
            menuItemSilent = new ToolStripMenuItem();
            menuItemQuickTimeSetting = new ToolStripMenuItem();
            menuItemServerTime = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripSeparator4 = new ToolStripSeparator();
            menuItemVersion = new ToolStripMenuItem();
            labelStatus = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            labelSilent = new Label();
            labelScale = new Label();
            labelTopMost = new Label();
            labelClock = new Label();
            menuItemNavigationWindow = new ToolStripMenuItem();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox3);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(0, 24);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(0, 0, 3, 0);
            panel1.Size = new Size(984, 561);
            panel1.TabIndex = 2;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Location = new Point(-300, -300);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(100, 100);
            pictureBox3.TabIndex = 7;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Cursor = Cursors.Cross;
            pictureBox2.Location = new Point(-300, -300);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(240, 240);
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            pictureBox2.MouseDown += PictureBox2_MouseDown;
            pictureBox2.MouseMove += PictureBox2_MouseMove;
            // 
            // pictureBox1
            // 
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            pictureBox1.Cursor = Cursors.SizeAll;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(984, 537);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseEnter += picturebox1_Enter;
            pictureBox1.MouseLeave += picturebox1_Leave;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, toolStripSeparator1, menuItemMarkupType, toolStripSeparator2, menuItemScale, menuItemTopMost, menuItemSilent, menuItemQuickTimeSetting, menuItemNavigationWindow, toolStripSeparator4, menuItemVersion });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 220);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(180, 22);
            menuItemCopy.Text = "CTCP画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // menuItemMarkupType
            // 
            menuItemMarkupType.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupType1, menuItemMarkupType2, menuItemMarkupType3 });
            menuItemMarkupType.Name = "menuItemMarkupType";
            menuItemMarkupType.Size = new Size(180, 22);
            menuItemMarkupType.Text = "強調表示タイプ";
            // 
            // menuItemMarkupType1
            // 
            menuItemMarkupType1.Checked = true;
            menuItemMarkupType1.CheckState = CheckState.Indeterminate;
            menuItemMarkupType1.Name = "menuItemMarkupType1";
            menuItemMarkupType1.Size = new Size(191, 22);
            menuItemMarkupType1.Text = "タイプ1（点滅）";
            menuItemMarkupType1.Click += menuItemMarkupType1_Click;
            // 
            // menuItemMarkupType2
            // 
            menuItemMarkupType2.Name = "menuItemMarkupType2";
            menuItemMarkupType2.Size = new Size(191, 22);
            menuItemMarkupType2.Text = "タイプ2（色逆転点滅）";
            menuItemMarkupType2.Click += menuItemMarkupType2_Click;
            // 
            // menuItemMarkupType3
            // 
            menuItemMarkupType3.Name = "menuItemMarkupType3";
            menuItemMarkupType3.Size = new Size(191, 22);
            menuItemMarkupType3.Text = "タイプ3（色逆転固定）";
            menuItemMarkupType3.Click += menuItemMarkupType3_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(177, 6);
            // 
            // menuItemScale
            // 
            menuItemScale.DropDownItems.AddRange(new ToolStripItem[] { toolStripSeparator3, menuItemScaleFit, menuItemFixedScale });
            menuItemScale.Name = "menuItemScale";
            menuItemScale.Size = new Size(180, 22);
            menuItemScale.Text = "拡大率";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(126, 6);
            // 
            // menuItemScaleFit
            // 
            menuItemScaleFit.Name = "menuItemScaleFit";
            menuItemScaleFit.Size = new Size(129, 22);
            menuItemScaleFit.Text = "フィット表示";
            // 
            // menuItemFixedScale
            // 
            menuItemFixedScale.Name = "menuItemFixedScale";
            menuItemFixedScale.Size = new Size(129, 22);
            menuItemFixedScale.Text = "倍率固定";
            menuItemFixedScale.Click += menuItemFixedScale_Click;
            // 
            // menuItemTopMost
            // 
            menuItemTopMost.Name = "menuItemTopMost";
            menuItemTopMost.Size = new Size(180, 22);
            menuItemTopMost.Text = "最前面表示";
            menuItemTopMost.Click += menuItemTopMost_Click;
            // 
            // menuItemSilent
            // 
            menuItemSilent.Name = "menuItemSilent";
            menuItemSilent.Size = new Size(180, 22);
            menuItemSilent.Text = "サイレントモード";
            menuItemSilent.Click += menuItemSilent_Click;
            // 
            // menuItemQuickTimeSetting
            // 
            menuItemQuickTimeSetting.DropDownItems.AddRange(new ToolStripItem[] { menuItemServerTime, toolStripSeparator5 });
            menuItemQuickTimeSetting.Name = "menuItemQuickTimeSetting";
            menuItemQuickTimeSetting.Size = new Size(180, 22);
            menuItemQuickTimeSetting.Text = "クイック時刻設定";
            // 
            // menuItemServerTime
            // 
            menuItemServerTime.Checked = true;
            menuItemServerTime.CheckState = CheckState.Checked;
            menuItemServerTime.Name = "menuItemServerTime";
            menuItemServerTime.Size = new Size(180, 22);
            menuItemServerTime.Text = "サーバ時刻";
            menuItemServerTime.Click += menuItemServerTime_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(177, 6);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(177, 6);
            // 
            // menuItemVersion
            // 
            menuItemVersion.Name = "menuItemVersion";
            menuItemVersion.Size = new Size(180, 22);
            menuItemVersion.Text = "バージョン情報";
            menuItemVersion.Click += menuItemVersion_Click;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.BackColor = Color.Transparent;
            labelStatus.ForeColor = Color.White;
            labelStatus.Location = new Point(3, 3);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(96, 15);
            labelStatus.TabIndex = 3;
            labelStatus.Text = "Status：起動中...";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(labelSilent);
            flowLayoutPanel1.Controls.Add(labelScale);
            flowLayoutPanel1.Controls.Add(labelTopMost);
            flowLayoutPanel1.Controls.Add(labelClock);
            flowLayoutPanel1.Location = new Point(643, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(340, 15);
            flowLayoutPanel1.TabIndex = 4;
            // 
            // labelSilent
            // 
            labelSilent.AutoSize = true;
            labelSilent.BackColor = Color.FromArgb(30, 30, 30);
            labelSilent.Cursor = Cursors.Hand;
            labelSilent.ForeColor = Color.White;
            labelSilent.Location = new Point(3, 0);
            labelSilent.Name = "labelSilent";
            labelSilent.Padding = new Padding(5, 0, 5, 0);
            labelSilent.Size = new Size(95, 15);
            labelSilent.TabIndex = 3;
            labelSilent.Text = "サイレント：OFF";
            labelSilent.Click += labelSilent_Click;
            labelSilent.MouseLeave += labelSilent_Leave;
            labelSilent.MouseHover += labelSilent_Hover;
            // 
            // labelScale
            // 
            labelScale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelScale.AutoSize = true;
            labelScale.BackColor = Color.Transparent;
            labelScale.Cursor = Cursors.Hand;
            labelScale.ForeColor = Color.White;
            labelScale.Location = new Point(104, 0);
            labelScale.Name = "labelScale";
            labelScale.Padding = new Padding(5, 0, 5, 0);
            labelScale.Size = new Size(84, 15);
            labelScale.TabIndex = 2;
            labelScale.Text = "Scale：100%";
            labelScale.TextAlign = ContentAlignment.TopRight;
            labelScale.MouseDown += labelScale_MouseDown;
            labelScale.MouseLeave += labelScale_Leave;
            labelScale.MouseHover += labelScale_Hover;
            // 
            // labelTopMost
            // 
            labelTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTopMost.AutoSize = true;
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
            labelTopMost.Cursor = Cursors.Hand;
            labelTopMost.ForeColor = Color.Gray;
            labelTopMost.Location = new Point(194, 0);
            labelTopMost.Name = "labelTopMost";
            labelTopMost.Padding = new Padding(5, 0, 5, 0);
            labelTopMost.Size = new Size(86, 15);
            labelTopMost.TabIndex = 1;
            labelTopMost.Text = "最前面：OFF";
            labelTopMost.Click += labelTopMost_Click;
            labelTopMost.MouseLeave += labelTopMost_Leave;
            labelTopMost.MouseHover += labelTopMost_Hover;
            // 
            // labelClock
            // 
            labelClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelClock.AutoSize = true;
            labelClock.BackColor = Color.Transparent;
            labelClock.Cursor = Cursors.Hand;
            labelClock.ForeColor = Color.White;
            labelClock.Location = new Point(286, 0);
            labelClock.Name = "labelClock";
            labelClock.Padding = new Padding(1, 0, 1, 0);
            labelClock.Size = new Size(51, 15);
            labelClock.TabIndex = 0;
            labelClock.Text = "00:00:00";
            labelClock.TextAlign = ContentAlignment.TopRight;
            labelClock.MouseDown += labelClock_MouseDown;
            // 
            // menuItemNavigationWindow
            // 
            menuItemNavigationWindow.Name = "menuItemNavigationWindow";
            menuItemNavigationWindow.Size = new Size(180, 22);
            menuItemNavigationWindow.Text = "ナビゲーションウィンドウ";
            menuItemNavigationWindow.Click += menuItemNavigationWindow_Click;
            // 
            // CTCPWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(40, 10, 10);
            ClientSize = new Size(984, 561);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(labelStatus);
            Controls.Add(panel1);
            MaximumSize = new Size(1000, 600);
            MinimumSize = new Size(540, 300);
            Name = "CTCPWindow";
            Text = "全線CTCP | CTCP - ダイヤ運転会";
            FormClosing += CTCPWindow_Closing;
            ResizeBegin += CTCPWindow_ResizeBegin;
            ResizeEnd += CTCPWindow_ResizeEnd;
            SizeChanged += CTCPWindow_SizeChanged;
            KeyDown += CTCPWindow_KeyDown;
            KeyUp += CTCPWindow_KeyUp;
            Resize += CTCPWindow_Resize;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox1;
        private Label labelStatus;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label labelClock;
        private Label labelScale;
        private Label labelTopMost;
        private Label labelSilent;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem menuItemCopy;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemScale;
        private ToolStripMenuItem menuItemTopMost;
        private ToolStripMenuItem menuItemSilent;
        private ToolStripMenuItem menuItemQuickTimeSetting;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItemVersion;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem menuItemScaleFit;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private ToolStripMenuItem menuItemFixedScale;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem menuItemMarkupType;
        private ToolStripMenuItem menuItemMarkupType1;
        private ToolStripMenuItem menuItemMarkupType2;
        private ToolStripMenuItem menuItemMarkupType3;
        private ToolStripMenuItem menuItemServerTime;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem menuItemNavigationWindow;
    }
}
