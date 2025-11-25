using System.Windows.Forms;

namespace TatehamaCTCPClient.Forms {
    partial class SubWindow {
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
            pictureBox1 = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            menuItemCopy = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuItemMarkupType = new ToolStripMenuItem();
            menuItemMarkupType1 = new ToolStripMenuItem();
            menuItemMarkupType2 = new ToolStripMenuItem();
            menuItemMarkupType3 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuItemTopMost = new ToolStripMenuItem();
            menuItemSilent = new ToolStripMenuItem();
            menuItemRename = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            menuItemVersion = new ToolStripMenuItem();
            flowLayoutPanel1 = new FlowLayoutPanel();
            labelClock = new Label();
            labelTopMost = new Label();
            labelScale = new Label();
            labelStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = Color.Black;
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            pictureBox1.Cursor = Cursors.SizeAll;
            pictureBox1.Location = new Point(0, 24);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(784, 437);
            pictureBox1.TabIndex = 9;
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
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, toolStripSeparator1, menuItemMarkupType, toolStripSeparator2, menuItemTopMost, menuItemSilent, menuItemRename, toolStripSeparator3, menuItemVersion });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(163, 154);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(162, 22);
            menuItemCopy.Text = "CTCP画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(159, 6);
            // 
            // menuItemMarkupType
            // 
            menuItemMarkupType.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupType1, menuItemMarkupType2, menuItemMarkupType3 });
            menuItemMarkupType.Name = "menuItemMarkupType";
            menuItemMarkupType.Size = new Size(162, 22);
            menuItemMarkupType.Text = "強調表示タイプ";
            // 
            // menuItemMarkupType1
            // 
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
            toolStripSeparator2.Size = new Size(159, 6);
            // 
            // menuItemTopMost
            // 
            menuItemTopMost.Name = "menuItemTopMost";
            menuItemTopMost.Size = new Size(162, 22);
            menuItemTopMost.Text = "最前面表示";
            menuItemTopMost.Click += menuItemTopMost_Click;
            // 
            // menuItemSilent
            // 
            menuItemSilent.Name = "menuItemSilent";
            menuItemSilent.Size = new Size(162, 22);
            menuItemSilent.Text = "サイレントモード";
            menuItemSilent.Click += menuItemSilent_Click;
            // 
            // menuItemRename
            // 
            menuItemRename.Name = "menuItemRename";
            menuItemRename.Size = new Size(162, 22);
            menuItemRename.Text = "ウィンドウ名の変更";
            menuItemRename.Click += menuItemRename_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(159, 6);
            // 
            // menuItemVersion
            // 
            menuItemVersion.Name = "menuItemVersion";
            menuItemVersion.Size = new Size(162, 22);
            menuItemVersion.Text = "バージョン情報";
            menuItemVersion.Click += menuItemVersion_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.BackColor = Color.Transparent;
            flowLayoutPanel1.Controls.Add(labelClock);
            flowLayoutPanel1.Controls.Add(labelTopMost);
            flowLayoutPanel1.Controls.Add(labelScale);
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(543, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(239, 15);
            flowLayoutPanel1.TabIndex = 10;
            flowLayoutPanel1.WrapContents = false;
            // 
            // labelClock
            // 
            labelClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelClock.AutoSize = true;
            labelClock.BackColor = Color.Transparent;
            labelClock.ForeColor = Color.White;
            labelClock.Location = new Point(185, 0);
            labelClock.Name = "labelClock";
            labelClock.Padding = new Padding(1, 0, 1, 0);
            labelClock.Size = new Size(51, 15);
            labelClock.TabIndex = 0;
            labelClock.Text = "00:00:00";
            labelClock.TextAlign = ContentAlignment.TopRight;
            // 
            // labelTopMost
            // 
            labelTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTopMost.AutoSize = true;
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
            labelTopMost.Cursor = Cursors.Hand;
            labelTopMost.ForeColor = Color.Gray;
            labelTopMost.Location = new Point(93, 0);
            labelTopMost.Name = "labelTopMost";
            labelTopMost.Padding = new Padding(5, 0, 5, 0);
            labelTopMost.Size = new Size(86, 15);
            labelTopMost.TabIndex = 1;
            labelTopMost.Text = "最前面：OFF";
            labelTopMost.Click += labelTopMost_Click;
            labelTopMost.MouseLeave += labelTopMost_Leave;
            labelTopMost.MouseHover += labelTopMost_Hover;
            // 
            // labelScale
            // 
            labelScale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelScale.AutoSize = true;
            labelScale.BackColor = Color.Transparent;
            labelScale.ForeColor = Color.White;
            labelScale.Location = new Point(3, 0);
            labelScale.Name = "labelScale";
            labelScale.Padding = new Padding(5, 0, 5, 0);
            labelScale.Size = new Size(84, 15);
            labelScale.TabIndex = 2;
            labelScale.Text = "Scale：100%";
            labelScale.TextAlign = ContentAlignment.TopRight;
            labelScale.MouseDown += labelScale_MouseDown;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.BackColor = Color.Transparent;
            labelStatus.ForeColor = Color.White;
            labelStatus.Location = new Point(3, 3);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(10, 15);
            labelStatus.TabIndex = 11;
            labelStatus.Text = " ";
            // 
            // SubWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(50, 20, 20);
            ClientSize = new Size(784, 461);
            Controls.Add(labelStatus);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(pictureBox1);
            Name = "SubWindow";
            Text = "CTCP | CTCP - ダイヤ運転会";
            FormClosing += SubWindow_Closing;
            ResizeBegin += SubWindow_ResizeBegin;
            ResizeEnd += SubWindow_ResizeEnd;
            KeyDown += SubWindow_KeyDown;
            KeyUp += SubWindow_KeyUp;
            Resize += SubWindow_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label labelScale;
        private Label labelTopMost;
        private Label labelClock;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem menuItemCopy;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemMarkupType;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItemTopMost;
        private ToolStripMenuItem menuItemSilent;
        private ToolStripMenuItem menuItemRename;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem menuItemVersion;
        private ToolStripMenuItem menuItemMarkupType1;
        private ToolStripMenuItem menuItemMarkupType2;
        private ToolStripMenuItem menuItemMarkupType3;
        private Label labelStatus;
    }
}