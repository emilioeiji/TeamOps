namespace TeamOps.UI.Forms
{
    partial class FormDashboardHtml
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewDashboard;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewDashboard = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewDashboard).BeginInit();
            SuspendLayout();
            // 
            // webViewDashboard
            // 
            webViewDashboard.AllowExternalDrop = true;
            webViewDashboard.CreationProperties = null;
            webViewDashboard.DefaultBackgroundColor = Color.White;
            webViewDashboard.Dock = DockStyle.Fill;
            webViewDashboard.Location = new Point(0, 0);
            webViewDashboard.Name = "webViewDashboard";
            webViewDashboard.Size = new Size(1134, 820);
            webViewDashboard.TabIndex = 0;
            webViewDashboard.ZoomFactor = 1D;
            // 
            // FormDashboardHtml
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1134, 820);
            Controls.Add(webViewDashboard);
            Name = "FormDashboardHtml";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TeamOps Dashboard";
            ((System.ComponentModel.ISupportInitialize)webViewDashboard).EndInit();
            ResumeLayout(false);
        }
    }
}
