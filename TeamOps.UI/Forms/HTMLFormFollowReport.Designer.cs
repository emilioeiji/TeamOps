namespace TeamOps.UI.Forms
{
    partial class HTMLFormFollowReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFollowReport;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewFollowReport = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowReport)).BeginInit();
            this.SuspendLayout();
            //
            // webViewFollowReport
            //
            this.webViewFollowReport.AllowExternalDrop = true;
            this.webViewFollowReport.CreationProperties = null;
            this.webViewFollowReport.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewFollowReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewFollowReport.Location = new System.Drawing.Point(0, 0);
            this.webViewFollowReport.Name = "webViewFollowReport";
            this.webViewFollowReport.Size = new System.Drawing.Size(1500, 920);
            this.webViewFollowReport.TabIndex = 0;
            this.webViewFollowReport.ZoomFactor = 1D;
            //
            // HTMLFormFollowReport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1500, 920);
            this.Controls.Add(this.webViewFollowReport);
            this.Name = "HTMLFormFollowReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Relatorio Follow (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowReport)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
