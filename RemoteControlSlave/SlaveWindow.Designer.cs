using System;
namespace RemoteControlSlave
{
    partial class SlaveWindow
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
            if(this.hWndNextWindow != IntPtr.Zero)
                ChangeClipboardChain(this.Handle, this.hWndNextWindow);

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SlaveWindow));
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.MakeListeningTcpSocket = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.StopServerButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.statusInfo = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.targetInfo = new System.Windows.Forms.Label();
            this.TargetLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(232, 73);
            this.portTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(442, 31);
            this.portTextBox.TabIndex = 0;
            this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MakeListeningTcpSocket
            // 
            this.MakeListeningTcpSocket.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MakeListeningTcpSocket.Location = new System.Drawing.Point(120, 312);
            this.MakeListeningTcpSocket.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MakeListeningTcpSocket.Name = "MakeListeningTcpSocket";
            this.MakeListeningTcpSocket.Size = new System.Drawing.Size(312, 92);
            this.MakeListeningTcpSocket.TabIndex = 2;
            this.MakeListeningTcpSocket.Text = "MakeListeningTcpSocket";
            this.MakeListeningTcpSocket.UseVisualStyleBackColor = true;
            this.MakeListeningTcpSocket.Click += new System.EventHandler(this.MakeListeningTcpSocket_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(398, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 37);
            this.label1.TabIndex = 2;
            this.label1.Text = "PORT";
            // 
            // StopServerButton
            // 
            this.StopServerButton.Enabled = false;
            this.StopServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopServerButton.Location = new System.Drawing.Point(494, 312);
            this.StopServerButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.StopServerButton.Name = "StopServerButton";
            this.StopServerButton.Size = new System.Drawing.Size(312, 92);
            this.StopServerButton.TabIndex = 3;
            this.StopServerButton.Text = "StopServer";
            this.StopServerButton.UseVisualStyleBackColor = true;
            this.StopServerButton.Click += new System.EventHandler(this.StopServerButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(236, 208);
            this.statusLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(91, 25);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "Status : ";
            // 
            // statusInfo
            // 
            this.statusInfo.AutoSize = true;
            this.statusInfo.ForeColor = System.Drawing.Color.Red;
            this.statusInfo.Location = new System.Drawing.Point(340, 208);
            this.statusInfo.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.statusInfo.Name = "statusInfo";
            this.statusInfo.Size = new System.Drawing.Size(178, 25);
            this.statusInfo.TabIndex = 5;
            this.statusInfo.Text = "DISCONNECTED";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(344, 117);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(211, 37);
            this.label4.TabIndex = 7;
            this.label4.Text = "PASSWORD";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(232, 162);
            this.passwordTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(442, 31);
            this.passwordTextBox.TabIndex = 1;
            this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // targetInfo
            // 
            this.targetInfo.AutoSize = true;
            this.targetInfo.ForeColor = System.Drawing.Color.Red;
            this.targetInfo.Location = new System.Drawing.Point(340, 256);
            this.targetInfo.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.targetInfo.Name = "targetInfo";
            this.targetInfo.Size = new System.Drawing.Size(79, 25);
            this.targetInfo.TabIndex = 9;
            this.targetInfo.Text = "FALSE";
            // 
            // TargetLabel
            // 
            this.TargetLabel.AutoSize = true;
            this.TargetLabel.Location = new System.Drawing.Point(236, 256);
            this.TargetLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.TargetLabel.Name = "TargetLabel";
            this.TargetLabel.Size = new System.Drawing.Size(86, 25);
            this.TargetLabel.TabIndex = 8;
            this.TargetLabel.Text = "Target :";
            // 
            // SlaveWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(910, 440);
            this.Controls.Add(this.targetInfo);
            this.Controls.Add(this.TargetLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.statusInfo);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.StopServerButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MakeListeningTcpSocket);
            this.Controls.Add(this.portTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.Name = "SlaveWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote Control Slave";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.SlaveWindow_Load);
            this.Resize += new System.EventHandler(this.ServerWindow_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Button MakeListeningTcpSocket;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button StopServerButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label statusInfo;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label targetInfo;
        private System.Windows.Forms.Label TargetLabel;
    }
}

