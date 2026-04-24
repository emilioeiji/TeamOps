namespace TeamOps.UI.Forms
{
    partial class HTMLFormTasksReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewTasksReport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewTasksReport = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewTasksReport).BeginInit();
            SuspendLayout();
            // 
            // webViewTasksReport
            // 
            webViewTasksReport.AllowExternalDrop = true;
            webViewTasksReport.CreationProperties = null;
            webViewTasksReport.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewTasksReport.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewTasksReport.Location = new System.Drawing.Point(0, 0);
            webViewTasksReport.Name = "webViewTasksReport";
            webViewTasksReport.Size = new System.Drawing.Size(1500, 920);
            webViewTasksReport.TabIndex = 0;
            webViewTasksReport.ZoomFactor = 1D;
            // 
            // HTMLFormTasksReport
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1500, 920);
            Controls.Add(webViewTasksReport);
            Name = "HTMLFormTasksReport";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Relatorio de Tasks (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewTasksReport).EndInit();
            ResumeLayout(false);
        }
    }
}
