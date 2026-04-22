namespace TeamOps.UI.Forms
{
    partial class HTMLFormFollowChart
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewFollowChart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webViewFollowChart = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowChart)).BeginInit();
            this.SuspendLayout();
            //
            // webViewFollowChart
            //
            this.webViewFollowChart.AllowExternalDrop = true;
            this.webViewFollowChart.CreationProperties = null;
            this.webViewFollowChart.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewFollowChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewFollowChart.Location = new System.Drawing.Point(0, 0);
            this.webViewFollowChart.Name = "webViewFollowChart";
            this.webViewFollowChart.Size = new System.Drawing.Size(1460, 900);
            this.webViewFollowChart.TabIndex = 0;
            this.webViewFollowChart.ZoomFactor = 1D;
            //
            // HTMLFormFollowChart
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1460, 900);
            this.Controls.Add(this.webViewFollowChart);
            this.Name = "HTMLFormFollowChart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Grafico Follow (HTML)";
            ((System.ComponentModel.ISupportInitialize)(this.webViewFollowChart)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
