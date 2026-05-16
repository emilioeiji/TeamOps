namespace TeamOps.UI.Forms
{
    partial class HTMLFormMasterCard
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewMasterCard;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewMasterCard = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewMasterCard)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewMasterCard
            // 
            this.webViewMasterCard.AllowExternalDrop = true;
            this.webViewMasterCard.CreationProperties = null;
            this.webViewMasterCard.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewMasterCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewMasterCard.Location = new System.Drawing.Point(0, 0);
            this.webViewMasterCard.Name = "webViewMasterCard";
            this.webViewMasterCard.Size = new System.Drawing.Size(1540, 900);
            this.webViewMasterCard.TabIndex = 0;
            this.webViewMasterCard.ZoomFactor = 1D;
            // 
            // HTMLFormMasterCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1540, 900);
            this.Controls.Add(this.webViewMasterCard);
            this.Name = "HTMLFormMasterCard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MasterCard (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewMasterCard)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
