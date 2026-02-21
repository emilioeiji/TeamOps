namespace TeamOps.UI.Forms
{
    partial class FormReports
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRepOperadores;
        private System.Windows.Forms.Button btnRepPR;
        private System.Windows.Forms.Button btnRepCL;
        private System.Windows.Forms.Button btnRepHikitsugui;
        private System.Windows.Forms.Button btnRepSobra;
        private System.Windows.Forms.Button btnRepFollowReport;
        private System.Windows.Forms.Button btnRepFollowChart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            btnRepOperadores = new Button();
            btnRepPR = new Button();
            btnRepCL = new Button();
            btnRepHikitsugui = new Button();
            btnRepSobra = new Button();
            btnRepFollowReport = new Button();
            btnRepFollowChart = new Button();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(45, 85, 155);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1101, 70);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(230, 33);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Relatórios – レポート";
            // 
            // btnRepOperadores
            // 
            btnRepOperadores.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepOperadores.Location = new Point(50, 100);
            btnRepOperadores.Name = "btnRepOperadores";
            btnRepOperadores.Size = new Size(200, 80);
            btnRepOperadores.TabIndex = 1;
            btnRepOperadores.Text = "Operadores\n作業者";
            btnRepOperadores.Click += btnRepOperadores_Click;
            // 
            // btnRepPR
            // 
            btnRepPR.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepPR.Location = new Point(300, 100);
            btnRepPR.Name = "btnRepPR";
            btnRepPR.Size = new Size(200, 80);
            btnRepPR.TabIndex = 2;
            btnRepPR.Text = "PR\nPR文書";
            btnRepPR.Click += btnRepPR_Click;
            // 
            // btnRepCL
            // 
            btnRepCL.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepCL.Location = new Point(550, 100);
            btnRepCL.Name = "btnRepCL";
            btnRepCL.Size = new Size(200, 80);
            btnRepCL.TabIndex = 3;
            btnRepCL.Text = "CL\nCL文書";
            btnRepCL.Click += btnRepCL_Click;
            // 
            // btnRepHikitsugui
            // 
            btnRepHikitsugui.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepHikitsugui.Location = new Point(50, 220);
            btnRepHikitsugui.Name = "btnRepHikitsugui";
            btnRepHikitsugui.Size = new Size(200, 80);
            btnRepHikitsugui.TabIndex = 4;
            btnRepHikitsugui.Text = "Hikitsugui\n引継ぎ";
            btnRepHikitsugui.Click += btnRepHikitsugui_Click;
            // 
            // btnRepSobra
            // 
            btnRepSobra.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepSobra.Location = new Point(300, 220);
            btnRepSobra.Name = "btnRepSobra";
            btnRepSobra.Size = new Size(200, 80);
            btnRepSobra.TabIndex = 5;
            btnRepSobra.Text = "Sobra de Peça\n製品残り";
            btnRepSobra.Click += btnRepSobra_Click;
            // 
            // btnRepFollowReport
            // 
            btnRepFollowReport.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepFollowReport.Location = new Point(800, 100);
            btnRepFollowReport.Name = "btnRepFollowReport";
            btnRepFollowReport.Size = new Size(200, 80);
            btnRepFollowReport.TabIndex = 5;
            btnRepFollowReport.Text = "Relatorio Follow\nフォローレポート";
            btnRepFollowReport.Click += btnRepFollowReport_Click;
            // 
            // btnRepFollowChart
            // 
            btnRepFollowChart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRepFollowChart.Location = new Point(800, 220);
            btnRepFollowChart.Name = "btnRepFollowChart";
            btnRepFollowChart.Size = new Size(200, 80);
            btnRepFollowChart.TabIndex = 5;
            btnRepFollowChart.Text = "Grafico Follow\nフォローグラフ";
            btnRepFollowChart.Click += btnRepFollowChart_Click;
            // 
            // FormReports
            // 
            ClientSize = new Size(1101, 350);
            Controls.Add(panelHeader);
            Controls.Add(btnRepOperadores);
            Controls.Add(btnRepPR);
            Controls.Add(btnRepCL);
            Controls.Add(btnRepHikitsugui);
            Controls.Add(btnRepSobra);
            Controls.Add(btnRepFollowReport);
            Controls.Add(btnRepFollowChart);
            Name = "FormReports";
            Text = "Relatórios";
            panelHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
