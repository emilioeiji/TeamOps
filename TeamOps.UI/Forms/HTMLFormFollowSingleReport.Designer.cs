namespace TeamOps.UI.Forms
{
    partial class HTMLFormFollowSingleReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFollowSingle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewFollowSingle = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowSingle)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewFollowSingle
            // 
            this.webViewFollowSingle.AllowExternalDrop = true;
            this.webViewFollowSingle.CreationProperties = null;
            this.webViewFollowSingle.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewFollowSingle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewFollowSingle.Location = new System.Drawing.Point(0, 0);
            this.webViewFollowSingle.Name = "webViewFollowSingle";
            this.webViewFollowSingle.Size = new System.Drawing.Size(1180, 920);
            this.webViewFollowSingle.TabIndex = 0;
            this.webViewFollowSingle.ZoomFactor = 1D;
            // 
            // HTMLFormFollowSingleReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 920);
            this.Controls.Add(this.webViewFollowSingle);
            this.Name = "HTMLFormFollowSingleReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FollowUp Report (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowSingle)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
