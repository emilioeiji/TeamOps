namespace TeamOps.UI.Forms
{
    partial class HTMLFormPresenceLayout
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewPresenceLayout;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewPresenceLayout = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewPresenceLayout).BeginInit();
            SuspendLayout();
            // 
            // webViewPresenceLayout
            // 
            webViewPresenceLayout.AllowExternalDrop = true;
            webViewPresenceLayout.CreationProperties = null;
            webViewPresenceLayout.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewPresenceLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewPresenceLayout.Location = new System.Drawing.Point(0, 0);
            webViewPresenceLayout.Name = "webViewPresenceLayout";
            webViewPresenceLayout.Size = new System.Drawing.Size(1580, 960);
            webViewPresenceLayout.TabIndex = 0;
            webViewPresenceLayout.ZoomFactor = 1D;
            // 
            // HTMLFormPresenceLayout
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1580, 960);
            Controls.Add(webViewPresenceLayout);
            Name = "HTMLFormPresenceLayout";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Presence Layout (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewPresenceLayout).EndInit();
            ResumeLayout(false);
        }
    }
}
