using System;
namespace SocketExample
{
    partial class ServerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerWindow));
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
            this.portTextBox.Location = new System.Drawing.Point(116, 38);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(223, 20);
            this.portTextBox.TabIndex = 0;
            this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MakeListeningTcpSocket
            // 
            this.MakeListeningTcpSocket.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MakeListeningTcpSocket.Location = new System.Drawing.Point(60, 162);
            this.MakeListeningTcpSocket.Name = "MakeListeningTcpSocket";
            this.MakeListeningTcpSocket.Size = new System.Drawing.Size(156, 48);
            this.MakeListeningTcpSocket.TabIndex = 2;
            this.MakeListeningTcpSocket.Text = "MakeListeningTcpSocket";
            this.MakeListeningTcpSocket.UseVisualStyleBackColor = true;
            this.MakeListeningTcpSocket.Click += new System.EventHandler(this.MakeListeningTcpSocket_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(199, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "PORT";
            // 
            // StopServerButton
            // 
            this.StopServerButton.Enabled = false;
            this.StopServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopServerButton.Location = new System.Drawing.Point(247, 162);
            this.StopServerButton.Name = "StopServerButton";
            this.StopServerButton.Size = new System.Drawing.Size(156, 48);
            this.StopServerButton.TabIndex = 3;
            this.StopServerButton.Text = "StopServer";
            this.StopServerButton.UseVisualStyleBackColor = true;
            this.StopServerButton.Click += new System.EventHandler(this.StopServerButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(118, 108);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(46, 13);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "Status : ";
            // 
            // statusInfo
            // 
            this.statusInfo.AutoSize = true;
            this.statusInfo.ForeColor = System.Drawing.Color.Red;
            this.statusInfo.Location = new System.Drawing.Point(170, 108);
            this.statusInfo.Name = "statusInfo";
            this.statusInfo.Size = new System.Drawing.Size(92, 13);
            this.statusInfo.TabIndex = 5;
            this.statusInfo.Text = "DISCONNECTED";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(172, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "PASSWORD";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(116, 84);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(223, 20);
            this.passwordTextBox.TabIndex = 1;
            this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // targetInfo
            // 
            this.targetInfo.AutoSize = true;
            this.targetInfo.ForeColor = System.Drawing.Color.Red;
            this.targetInfo.Location = new System.Drawing.Point(170, 133);
            this.targetInfo.Name = "targetInfo";
            this.targetInfo.Size = new System.Drawing.Size(40, 13);
            this.targetInfo.TabIndex = 9;
            this.targetInfo.Text = "FALSE";
            // 
            // TargetLabel
            // 
            this.TargetLabel.AutoSize = true;
            this.TargetLabel.Location = new System.Drawing.Point(118, 133);
            this.TargetLabel.Name = "TargetLabel";
            this.TargetLabel.Size = new System.Drawing.Size(44, 13);
            this.TargetLabel.TabIndex = 8;
            this.TargetLabel.Text = "Target :";
            // 
            // ServerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 229);
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
            this.MaximizeBox = false;
            this.Name = "ServerWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Make Listening TCP Socket";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
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

