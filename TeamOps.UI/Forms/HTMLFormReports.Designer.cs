namespace TeamOps.UI.Forms
{
    partial class HTMLFormReports
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewReports;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewReports = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewReports)).BeginInit();
            this.SuspendLayout();
            //
            // webViewReports
            //
            this.webViewReports.AllowExternalDrop = true;
            this.webViewReports.CreationProperties = null;
            this.webViewReports.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewReports.Location = new System.Drawing.Point(0, 0);
            this.webViewReports.Name = "webViewReports";
            this.webViewReports.Size = new System.Drawing.Size(1360, 860);
            this.webViewReports.TabIndex = 0;
            this.webViewReports.ZoomFactor = 1D;
            //
            // HTMLFormReports
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1360, 860);
            this.Controls.Add(this.webViewReports);
            this.Name = "HTMLFormReports";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Relatórios (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewReports)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
