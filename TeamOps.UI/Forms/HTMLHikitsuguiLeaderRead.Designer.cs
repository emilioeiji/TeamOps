namespace TeamOps.UI.Forms
{
    partial class HTMLHikitsuguiLeaderRead
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewHikitsugui;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewHikitsugui = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewHikitsugui)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewHikitsugui
            // 
            this.webViewHikitsugui.AllowExternalDrop = true;
            this.webViewHikitsugui.CreationProperties = null;
            this.webViewHikitsugui.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewHikitsugui.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewHikitsugui.Location = new System.Drawing.Point(0, 0);
            this.webViewHikitsugui.Name = "webViewHikitsugui";
            this.webViewHikitsugui.Size = new System.Drawing.Size(1280, 800);
            this.webViewHikitsugui.TabIndex = 0;
            this.webViewHikitsugui.ZoomFactor = 1D;
            // 
            // HTMLHikitsuguiLeaderRead
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.webViewHikitsugui);
            this.Name = "HTMLHikitsuguiLeaderRead";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hikitsugui (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewHikitsugui)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
