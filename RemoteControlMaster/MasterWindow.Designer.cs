namespace RemoteControlMaster
{
    partial class MasterWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerList = new System.Windows.Forms.ListBox();
            this.X = new System.Windows.Forms.Button();
            this.MouseArea = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.disconnectServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MinimizeButton = new System.Windows.Forms.Button();
            this.remotePasteProgressBar = new System.Windows.Forms.ProgressBar();
            this.RemotePaste = new System.Windows.Forms.Label();
            this.RemoteCpy = new System.Windows.Forms.Label();
            this.remoteCopyProgressBar = new System.Windows.Forms.ProgressBar();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(1352, 44);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addServerToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(150, 36);
            this.fileToolStripMenuItem.Text = "Connection";
            // 
            // addServerToolStripMenuItem
            // 
            this.addServerToolStripMenuItem.Name = "addServerToolStripMenuItem";
            this.addServerToolStripMenuItem.Size = new System.Drawing.Size(286, 38);
            this.addServerToolStripMenuItem.Text = "Add New Server";
            this.addServerToolStripMenuItem.Click += new System.EventHandler(this.addServerToolStripMenuItem_Click);
            // 
            // ServerList
            // 
            this.ServerList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ServerList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerList.ForeColor = System.Drawing.Color.Tomato;
            this.ServerList.FormattingEnabled = true;
            this.ServerList.ItemHeight = 37;
            this.ServerList.Location = new System.Drawing.Point(24, 75);
            this.ServerList.Margin = new System.Windows.Forms.Padding(6);
            this.ServerList.Name = "ServerList";
            this.ServerList.Size = new System.Drawing.Size(388, 483);
            this.ServerList.TabIndex = 9;
            this.ServerList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ServerList_MouseDoubleClick);
            this.ServerList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ServerList_MouseDown);
            // 
            // X
            // 
            this.X.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.X.BackColor = System.Drawing.Color.Red;
            this.X.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.X.Location = new System.Drawing.Point(1304, 0);
            this.X.Margin = new System.Windows.Forms.Padding(6);
            this.X.Name = "X";
            this.X.Size = new System.Drawing.Size(48, 44);
            this.X.TabIndex = 11;
            this.X.Text = "X";
            this.X.UseVisualStyleBackColor = false;
            this.X.Click += new System.EventHandler(this.X_Click);
            // 
            // MouseArea
            // 
            this.MouseArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MouseArea.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.MouseArea.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MouseArea.ForeColor = System.Drawing.SystemColors.WindowText;
            this.MouseArea.FormattingEnabled = true;
            this.MouseArea.ItemHeight = 25;
            this.MouseArea.Location = new System.Drawing.Point(428, 75);
            this.MouseArea.Margin = new System.Windows.Forms.Padding(6);
            this.MouseArea.Name = "MouseArea";
            this.MouseArea.Size = new System.Drawing.Size(900, 800);
            this.MouseArea.TabIndex = 12;
            this.MouseArea.SelectedIndexChanged += new System.EventHandler(this.MouseArea_SelectedIndexChanged);
            this.MouseArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseArea_MouseDown);
            this.MouseArea.MouseEnter += new System.EventHandler(this.MouseArea_MouseEnter);
            this.MouseArea.MouseLeave += new System.EventHandler(this.MouseArea_MouseLeave);
            this.MouseArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseArea_MouseMove);
            this.MouseArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseArea_MouseUp);
            this.MouseArea.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MouseArea_MouseWheel);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectServerToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(283, 40);
            // 
            // disconnectServerToolStripMenuItem
            // 
            this.disconnectServerToolStripMenuItem.Name = "disconnectServerToolStripMenuItem";
            this.disconnectServerToolStripMenuItem.Size = new System.Drawing.Size(282, 36);
            this.disconnectServerToolStripMenuItem.Text = "Disconnect Server";
            this.disconnectServerToolStripMenuItem.Click += new System.EventHandler(this.disconnectServerToolStripMenuItem_Click);
            // 
            // MinimizeButton
            // 
            this.MinimizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MinimizeButton.BackColor = System.Drawing.Color.Red;
            this.MinimizeButton.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.MinimizeButton.Location = new System.Drawing.Point(1244, 0);
            this.MinimizeButton.Margin = new System.Windows.Forms.Padding(6);
            this.MinimizeButton.Name = "MinimizeButton";
            this.MinimizeButton.Size = new System.Drawing.Size(48, 44);
            this.MinimizeButton.TabIndex = 13;
            this.MinimizeButton.Text = "_";
            this.MinimizeButton.UseVisualStyleBackColor = false;
            this.MinimizeButton.Click += new System.EventHandler(this.MinimizeButton_Click);
            // 
            // remotePasteProgressBar
            // 
            this.remotePasteProgressBar.Location = new System.Drawing.Point(26, 673);
            this.remotePasteProgressBar.Margin = new System.Windows.Forms.Padding(6);
            this.remotePasteProgressBar.Name = "remotePasteProgressBar";
            this.remotePasteProgressBar.Size = new System.Drawing.Size(388, 44);
            this.remotePasteProgressBar.TabIndex = 14;
            this.remotePasteProgressBar.Visible = false;
            // 
            // RemotePaste
            // 
            this.RemotePaste.AutoSize = true;
            this.RemotePaste.BackColor = System.Drawing.Color.Transparent;
            this.RemotePaste.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.RemotePaste.Location = new System.Drawing.Point(134, 637);
            this.RemotePaste.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.RemotePaste.Name = "RemotePaste";
            this.RemotePaste.Size = new System.Drawing.Size(147, 25);
            this.RemotePaste.TabIndex = 15;
            this.RemotePaste.Text = "Remote Paste";
            this.RemotePaste.Visible = false;
            // 
            // RemoteCpy
            // 
            this.RemoteCpy.AutoSize = true;
            this.RemoteCpy.BackColor = System.Drawing.Color.Transparent;
            this.RemoteCpy.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.RemoteCpy.Location = new System.Drawing.Point(140, 752);
            this.RemoteCpy.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.RemoteCpy.Name = "RemoteCpy";
            this.RemoteCpy.Size = new System.Drawing.Size(142, 25);
            this.RemoteCpy.TabIndex = 17;
            this.RemoteCpy.Text = "Remote Copy";
            this.RemoteCpy.Visible = false;
            // 
            // remoteCopyProgressBar
            // 
            this.remoteCopyProgressBar.Location = new System.Drawing.Point(28, 788);
            this.remoteCopyProgressBar.Margin = new System.Windows.Forms.Padding(6);
            this.remoteCopyProgressBar.Name = "remoteCopyProgressBar";
            this.remoteCopyProgressBar.Size = new System.Drawing.Size(388, 44);
            this.remoteCopyProgressBar.TabIndex = 16;
            this.remoteCopyProgressBar.Visible = false;
            // 
            // RemoteControlMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MediumSlateBlue;
            this.ClientSize = new System.Drawing.Size(1352, 892);
            this.Controls.Add(this.RemoteCpy);
            this.Controls.Add(this.remoteCopyProgressBar);
            this.Controls.Add(this.RemotePaste);
            this.Controls.Add(this.remotePasteProgressBar);
            this.Controls.Add(this.MinimizeButton);
            this.Controls.Add(this.MouseArea);
            this.Controls.Add(this.X);
            this.Controls.Add(this.ServerList);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "RemoteControlMaster";
            this.Text = "Remote Control Master";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addServerToolStripMenuItem;
        private System.Windows.Forms.ListBox ServerList;
        private System.Windows.Forms.Button X;
        private System.Windows.Forms.ListBox MouseArea;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem disconnectServerToolStripMenuItem;
        private System.Windows.Forms.Button MinimizeButton;
        private System.Windows.Forms.ProgressBar remotePasteProgressBar;
        private System.Windows.Forms.Label RemotePaste;
        private System.Windows.Forms.Label RemoteCpy;
        private System.Windows.Forms.ProgressBar remoteCopyProgressBar;

    }
}

