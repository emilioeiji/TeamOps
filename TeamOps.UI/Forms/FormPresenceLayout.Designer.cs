namespace TeamOps.UI.Forms
{
    partial class FormPresenceLayout
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            this.webViewPresence = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewPresence)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewPresence
            // 
            this.webViewPresence.AllowExternalDrop = true;
            this.webViewPresence.CreationProperties = null;
            this.webViewPresence.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewPresence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewPresence.Location = new System.Drawing.Point(0, 0);
            this.webViewPresence.Name = "webViewPresence";
            this.webViewPresence.Size = new System.Drawing.Size(1000, 700);
            this.webViewPresence.TabIndex = 0;
            this.webViewPresence.ZoomFactor = 1D;
            // 
            // FormPresenceLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.webViewPresence);
            this.Name = "FormPresenceLayout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Presença - TeamOps";
            ((System.ComponentModel.ISupportInitialize)(this.webViewPresence)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewPresence;
    }
}
