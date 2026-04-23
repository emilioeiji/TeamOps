namespace TeamOps.UI.Forms
{
    partial class HTMLFormHikitsuguiReader
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewReader;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            webViewReader = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewReader).BeginInit();
            SuspendLayout();
            // 
            // webViewReader
            // 
            webViewReader.AllowExternalDrop = true;
            webViewReader.CreationProperties = null;
            webViewReader.DefaultBackgroundColor = System.Drawing.Color.White;
            webViewReader.Dock = System.Windows.Forms.DockStyle.Fill;
            webViewReader.Location = new System.Drawing.Point(0, 0);
            webViewReader.Name = "webViewReader";
            webViewReader.Size = new System.Drawing.Size(1500, 920);
            webViewReader.TabIndex = 0;
            webViewReader.ZoomFactor = 1D;
            // 
            // HTMLFormHikitsuguiReader
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1500, 920);
            Controls.Add(webViewReader);
            Name = "HTMLFormHikitsuguiReader";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Leitura de Hikitsugui (HTML)";
            ((System.ComponentModel.ISupportInitialize)webViewReader).EndInit();
            ResumeLayout(false);
        }
    }
}
