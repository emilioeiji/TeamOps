namespace TeamOps.UI.Forms
{
    partial class HTMLFormSobraDePecaReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewSobraReport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewSobraReport = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewSobraReport).BeginInit();
            SuspendLayout();
            // 
            // webViewSobraReport
            // 
            webViewSobraReport.AllowExternalDrop = true;
            webViewSobraReport.CreationProperties = null;
            webViewSobraReport.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewSobraReport.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewSobraReport.Location = new System.Drawing.Point(0, 0);
            webViewSobraReport.Name = "webViewSobraReport";
            webViewSobraReport.Size = new System.Drawing.Size(1500, 920);
            webViewSobraReport.TabIndex = 0;
            webViewSobraReport.ZoomFactor = 1D;
            // 
            // HTMLFormSobraDePecaReport
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1500, 920);
            Controls.Add(webViewSobraReport);
            Name = "HTMLFormSobraDePecaReport";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Relatorio de Sobra de Peca (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewSobraReport).EndInit();
            ResumeLayout(false);
        }
    }
}
