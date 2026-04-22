namespace TeamOps.UI.Forms
{
    partial class HTMLFormAccessControl
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewAccessControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewAccessControl = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewAccessControl)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewAccessControl
            // 
            this.webViewAccessControl.AllowExternalDrop = true;
            this.webViewAccessControl.CreationProperties = null;
            this.webViewAccessControl.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewAccessControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewAccessControl.Location = new System.Drawing.Point(0, 0);
            this.webViewAccessControl.Name = "webViewAccessControl";
            this.webViewAccessControl.Size = new System.Drawing.Size(1480, 860);
            this.webViewAccessControl.TabIndex = 0;
            this.webViewAccessControl.ZoomFactor = 1D;
            // 
            // HTMLFormAccessControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1480, 860);
            this.Controls.Add(this.webViewAccessControl);
            this.Name = "HTMLFormAccessControl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Controle de Acesso (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewAccessControl)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
