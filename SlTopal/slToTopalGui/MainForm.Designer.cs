namespace slToTopalGui
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.buttonTest = new System.Windows.Forms.Button();
            this.notifyIconPrint = new System.Windows.Forms.NotifyIcon(this.components);
            this.buttonQuit = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.progressBarDownload = new System.Windows.Forms.ProgressBar();
            this.imageListMain = new System.Windows.Forms.ImageList(this.components);
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(347, 256);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 1;
            this.buttonTest.Text = "TOPAL Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // notifyIconPrint
            // 
            this.notifyIconPrint.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconPrint.Icon")));
            this.notifyIconPrint.Text = "SPORTLOOP Druckclient";
            this.notifyIconPrint.Visible = true;
            this.notifyIconPrint.Click += new System.EventHandler(this.notifyIconPrint_Click);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Location = new System.Drawing.Point(428, 256);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 2;
            this.buttonQuit.Text = "&Beenden";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.buttonQuit_Click);
            // 
            // progressBarDownload
            // 
            this.progressBarDownload.Location = new System.Drawing.Point(95, 255);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(246, 23);
            this.progressBarDownload.TabIndex = 4;
            this.progressBarDownload.Visible = false;
            // 
            // imageListMain
            // 
            this.imageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListMain.ImageStream")));
            this.imageListMain.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListMain.Images.SetKeyName(0, "Report");
            // 
            // listBoxLog
            // 
            this.listBoxLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.Location = new System.Drawing.Point(13, 12);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(490, 234);
            this.listBoxLog.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 286);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.progressBarDownload);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonTest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.NotifyIcon notifyIconPrint;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.ImageList imageListMain;
        private System.Windows.Forms.ListBox listBoxLog;
    }
}