namespace TeamOps.UI.Forms
{
    partial class FormPaidLeaveTracking
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewPaidLeave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewPaidLeave = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewPaidLeave)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewPaidLeave
            // 
            this.webViewPaidLeave.AllowExternalDrop = true;
            this.webViewPaidLeave.CreationProperties = null;
            this.webViewPaidLeave.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewPaidLeave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewPaidLeave.Location = new System.Drawing.Point(0, 0);
            this.webViewPaidLeave.Name = "webViewPaidLeave";
            this.webViewPaidLeave.Size = new System.Drawing.Size(984, 661);
            this.webViewPaidLeave.TabIndex = 0;
            this.webViewPaidLeave.ZoomFactor = 1D;
            // 
            // FormPaidLeaveTracking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.webViewPaidLeave);
            this.Name = "FormPaidLeaveTracking";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Paid Leave Tracking";
            ((System.ComponentModel.ISupportInitialize)(this.webViewPaidLeave)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
