namespace TeamOps.UI.Forms
{
    partial class HTMLFormTasks
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewTasks;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewTasks = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewTasks)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewTasks
            // 
            this.webViewTasks.AllowExternalDrop = true;
            this.webViewTasks.CreationProperties = null;
            this.webViewTasks.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewTasks.Location = new System.Drawing.Point(0, 0);
            this.webViewTasks.Name = "webViewTasks";
            this.webViewTasks.Size = new System.Drawing.Size(1540, 900);
            this.webViewTasks.TabIndex = 0;
            this.webViewTasks.ZoomFactor = 1D;
            // 
            // HTMLFormTasks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1540, 900);
            this.Controls.Add(this.webViewTasks);
            this.Name = "HTMLFormTasks";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tasks (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewTasks)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
