namespace TeamOps.UI.Forms
{
    partial class HTMLFormFollowOperatorReport
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFollowOperator;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewFollowOperator = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowOperator)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewFollowOperator
            // 
            this.webViewFollowOperator.AllowExternalDrop = true;
            this.webViewFollowOperator.CreationProperties = null;
            this.webViewFollowOperator.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewFollowOperator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewFollowOperator.Location = new System.Drawing.Point(0, 0);
            this.webViewFollowOperator.Name = "webViewFollowOperator";
            this.webViewFollowOperator.Size = new System.Drawing.Size(1260, 940);
            this.webViewFollowOperator.TabIndex = 0;
            this.webViewFollowOperator.ZoomFactor = 1D;
            // 
            // HTMLFormFollowOperatorReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 940);
            this.Controls.Add(this.webViewFollowOperator);
            this.Name = "HTMLFormFollowOperatorReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Follow History by Operator (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowOperator)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
