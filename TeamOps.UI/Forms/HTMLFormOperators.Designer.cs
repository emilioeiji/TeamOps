namespace TeamOps.UI.Forms
{
    partial class HTMLFormOperators
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewOperators;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewOperators = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewOperators)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewOperators
            // 
            this.webViewOperators.AllowExternalDrop = true;
            this.webViewOperators.CreationProperties = null;
            this.webViewOperators.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewOperators.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewOperators.Location = new System.Drawing.Point(0, 0);
            this.webViewOperators.Name = "webViewOperators";
            this.webViewOperators.Size = new System.Drawing.Size(1540, 900);
            this.webViewOperators.TabIndex = 0;
            this.webViewOperators.ZoomFactor = 1D;
            // 
            // HTMLFormOperators
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1540, 900);
            this.Controls.Add(this.webViewOperators);
            this.Name = "HTMLFormOperators";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Operadores (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewOperators)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
