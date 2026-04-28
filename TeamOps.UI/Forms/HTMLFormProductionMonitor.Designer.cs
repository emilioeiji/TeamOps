namespace TeamOps.UI.Forms
{
    partial class HTMLFormProductionMonitor
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewProductionMonitor;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewProductionMonitor = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewProductionMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewProductionMonitor
            // 
            this.webViewProductionMonitor.AllowExternalDrop = true;
            this.webViewProductionMonitor.CreationProperties = null;
            this.webViewProductionMonitor.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewProductionMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewProductionMonitor.Location = new System.Drawing.Point(0, 0);
            this.webViewProductionMonitor.Name = "webViewProductionMonitor";
            this.webViewProductionMonitor.Size = new System.Drawing.Size(1600, 940);
            this.webViewProductionMonitor.TabIndex = 0;
            this.webViewProductionMonitor.ZoomFactor = 1D;
            // 
            // HTMLFormProductionMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 940);
            this.Controls.Add(this.webViewProductionMonitor);
            this.Name = "HTMLFormProductionMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Producao / Machine Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.webViewProductionMonitor)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
