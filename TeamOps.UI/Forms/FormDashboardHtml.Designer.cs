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
            this.webViewDashboard = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewDashboard)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewDashboard
            // 
            this.webViewDashboard.AllowExternalDrop = true;
            this.webViewDashboard.CreationProperties = null;
            this.webViewDashboard.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewDashboard.Location = new System.Drawing.Point(0, 0);
            this.webViewDashboard.Name = "webViewDashboard";
            this.webViewDashboard.Size = new System.Drawing.Size(984, 661);
            this.webViewDashboard.TabIndex = 0;
            this.webViewDashboard.ZoomFactor = 1D;
            // 
            // FormDashboardHtml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.webViewDashboard);
            this.Name = "FormDashboardHtml";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TeamOps Dashboard";
            ((System.ComponentModel.ISupportInitialize)(this.webViewDashboard)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
