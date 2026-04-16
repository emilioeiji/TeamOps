namespace TeamOps.UI.Forms
{
    partial class HTMLHikitsuguiCreate
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCreate;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewCreate = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewCreate)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewCreate
            // 
            this.webViewCreate.AllowExternalDrop = true;
            this.webViewCreate.CreationProperties = null;
            this.webViewCreate.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewCreate.Location = new System.Drawing.Point(0, 0);
            this.webViewCreate.Name = "webViewCreate";
            this.webViewCreate.Size = new System.Drawing.Size(1280, 800);
            this.webViewCreate.TabIndex = 0;
            this.webViewCreate.ZoomFactor = 1D;
            // 
            // HTMLHikitsuguiCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.webViewCreate);
            this.Name = "HTMLHikitsuguiCreate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Novo Hikitsugui (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewCreate)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
