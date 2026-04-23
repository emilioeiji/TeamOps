namespace TeamOps.UI.Forms
{
    partial class HTMLFormFollowUp
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFollowUp;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewFollowUp = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowUp)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewFollowUp
            // 
            this.webViewFollowUp.AllowExternalDrop = true;
            this.webViewFollowUp.CreationProperties = null;
            this.webViewFollowUp.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewFollowUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewFollowUp.Location = new System.Drawing.Point(0, 0);
            this.webViewFollowUp.Name = "webViewFollowUp";
            this.webViewFollowUp.Size = new System.Drawing.Size(1240, 860);
            this.webViewFollowUp.TabIndex = 0;
            this.webViewFollowUp.ZoomFactor = 1D;
            // 
            // HTMLFormFollowUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1240, 860);
            this.Controls.Add(this.webViewFollowUp);
            this.Name = "HTMLFormFollowUp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cadastro de Acompanhamento";
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowUp)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
