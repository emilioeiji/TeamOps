namespace TeamOps.UI.Forms
{
    partial class HTMLFormSobraDePeca
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewSobraDePeca;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewSobraDePeca = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewSobraDePeca)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewSobraDePeca
            // 
            this.webViewSobraDePeca.AllowExternalDrop = true;
            this.webViewSobraDePeca.CreationProperties = null;
            this.webViewSobraDePeca.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewSobraDePeca.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewSobraDePeca.Location = new System.Drawing.Point(0, 0);
            this.webViewSobraDePeca.Name = "webViewSobraDePeca";
            this.webViewSobraDePeca.Size = new System.Drawing.Size(1420, 860);
            this.webViewSobraDePeca.TabIndex = 0;
            this.webViewSobraDePeca.ZoomFactor = 1D;
            // 
            // HTMLFormSobraDePeca
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 860);
            this.Controls.Add(this.webViewSobraDePeca);
            this.Name = "HTMLFormSobraDePeca";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sobra de Peça (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewSobraDePeca)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
