namespace TeamOps.UI.Forms
{
    partial class HTMLFormMasterCardReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewMasterCardReport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewMasterCardReport = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewMasterCardReport).BeginInit();
            SuspendLayout();
            // 
            // webViewMasterCardReport
            // 
            webViewMasterCardReport.AllowExternalDrop = true;
            webViewMasterCardReport.CreationProperties = null;
            webViewMasterCardReport.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewMasterCardReport.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewMasterCardReport.Location = new System.Drawing.Point(0, 0);
            webViewMasterCardReport.Name = "webViewMasterCardReport";
            webViewMasterCardReport.Size = new System.Drawing.Size(1500, 920);
            webViewMasterCardReport.TabIndex = 0;
            webViewMasterCardReport.ZoomFactor = 1D;
            // 
            // HTMLFormMasterCardReport
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1500, 920);
            Controls.Add(webViewMasterCardReport);
            Name = "HTMLFormMasterCardReport";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Relatorio de MasterCard (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewMasterCardReport).EndInit();
            ResumeLayout(false);
        }
    }
}
