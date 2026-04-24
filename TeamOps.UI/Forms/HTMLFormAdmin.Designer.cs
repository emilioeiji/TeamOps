namespace TeamOps.UI.Forms
{
    partial class HTMLFormAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewAdmin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewAdmin = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewAdmin).BeginInit();
            SuspendLayout();
            // 
            // webViewAdmin
            // 
            webViewAdmin.AllowExternalDrop = true;
            webViewAdmin.CreationProperties = null;
            webViewAdmin.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewAdmin.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewAdmin.Location = new System.Drawing.Point(0, 0);
            webViewAdmin.Name = "webViewAdmin";
            webViewAdmin.Size = new System.Drawing.Size(1480, 920);
            webViewAdmin.TabIndex = 0;
            webViewAdmin.ZoomFactor = 1D;
            // 
            // HTMLFormAdmin
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1480, 920);
            Controls.Add(webViewAdmin);
            Name = "HTMLFormAdmin";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Admin (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewAdmin).EndInit();
            ResumeLayout(false);
        }
    }
}
