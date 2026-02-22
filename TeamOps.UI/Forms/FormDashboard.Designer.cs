namespace TeamOps.UI.Forms
{
    partial class FormDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnOperadores;
        private System.Windows.Forms.Button btnAtribuir;
        private System.Windows.Forms.Button btnRelatorios;
        private System.Windows.Forms.Button btnAdmin;
        private System.Windows.Forms.Button btnAccessControl;
        private System.Windows.Forms.Button btnFollowUp;
        private System.Windows.Forms.Button btnHikitsugui;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Button btnHikitsuguiLeaderRead;
        private System.Windows.Forms.Button btnSobraDePeca;
        private System.Windows.Forms.Button btnPR;
        private System.Windows.Forms.Button btnCL;

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
            lblUser = new Label();
            btnOperadores = new Button();
            btnAtribuir = new Button();
            btnRelatorios = new Button();
            btnAdmin = new Button();
            btnAccessControl = new Button();
            btnFollowUp = new Button();
            btnHikitsugui = new Button();
            panelFooter = new Panel();
            lblDate = new Label();
            btnHikitsuguiLeaderRead = new Button();
            btnSobraDePeca = new Button();
            btnPR = new Button();
            btnCL = new Button();
            panel1 = new Panel();
            panel2 = new Panel();
            panel3 = new Panel();
            panelHeader.SuspendLayout();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(45, 85, 155);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblUser);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1077, 80);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(448, 37);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "TeamOps – Gestão de Operadores";
            // 
            // lblUser
            // 
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 10F);
            lblUser.ForeColor = Color.White;
            lblUser.Location = new Point(877, 30);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(0, 23);
            lblUser.TabIndex = 1;
            // 
            // btnOperadores
            // 
            btnOperadores.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnOperadores.Location = new Point(320, 320);
            btnOperadores.Name = "btnOperadores";
            btnOperadores.Size = new Size(200, 80);
            btnOperadores.TabIndex = 2;
            btnOperadores.Text = "Operadores\r\n作業者";
            btnOperadores.Click += btnOperadores_Click;
            // 
            // btnAtribuir
            // 
            btnAtribuir.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAtribuir.Location = new Point(811, 320);
            btnAtribuir.Name = "btnAtribuir";
            btnAtribuir.Size = new Size(200, 80);
            btnAtribuir.TabIndex = 7;
            btnAtribuir.Text = "Atribuir Operadores";
            btnAtribuir.Click += btnAtribuir_Click;
            // 
            // btnRelatorios
            // 
            btnRelatorios.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRelatorios.Location = new Point(80, 320);
            btnRelatorios.Name = "btnRelatorios";
            btnRelatorios.Size = new Size(200, 80);
            btnRelatorios.TabIndex = 3;
            btnRelatorios.Text = "Relatórios\r\nレポート";
            btnRelatorios.Click += btnRelatorios_Click;
            // 
            // btnAdmin
            // 
            btnAdmin.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAdmin.Location = new Point(811, 120);
            btnAdmin.Name = "btnAdmin";
            btnAdmin.Size = new Size(200, 80);
            btnAdmin.TabIndex = 5;
            btnAdmin.Text = "Admin";
            btnAdmin.Click += btnAdmin_Click;
            // 
            // btnAccessControl
            // 
            btnAccessControl.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAccessControl.Location = new Point(811, 220);
            btnAccessControl.Name = "btnAccessControl";
            btnAccessControl.Size = new Size(200, 80);
            btnAccessControl.TabIndex = 6;
            btnAccessControl.Text = "Accesso\r\nアクセス権";
            btnAccessControl.Click += btnAccessControl_Click;
            // 
            // btnFollowUp
            // 
            btnFollowUp.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnFollowUp.Location = new Point(80, 120);
            btnFollowUp.Name = "btnFollowUp";
            btnFollowUp.Size = new Size(200, 80);
            btnFollowUp.TabIndex = 4;
            btnFollowUp.Text = "Acompanhamento\r\nフォロー";
            btnFollowUp.Click += btnFollowUp_Click;
            // 
            // btnHikitsugui
            // 
            btnHikitsugui.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnHikitsugui.Location = new Point(320, 120);
            btnHikitsugui.Name = "btnHikitsugui";
            btnHikitsugui.Size = new Size(200, 80);
            btnHikitsugui.TabIndex = 8;
            btnHikitsugui.Text = "Hikitsugui\r\n引継ぎ";
            btnHikitsugui.Click += btnHikitsugui_Click;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.LightGray;
            panelFooter.Controls.Add(lblDate);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 454);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(1077, 40);
            panelFooter.TabIndex = 9;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Segoe UI", 9F);
            lblDate.ForeColor = Color.Black;
            lblDate.Location = new Point(20, 10);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(0, 20);
            lblDate.TabIndex = 0;
            // 
            // btnHikitsuguiLeaderRead
            // 
            btnHikitsuguiLeaderRead.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnHikitsuguiLeaderRead.Location = new Point(320, 220);
            btnHikitsuguiLeaderRead.Name = "btnHikitsuguiLeaderRead";
            btnHikitsuguiLeaderRead.Size = new Size(200, 80);
            btnHikitsuguiLeaderRead.TabIndex = 0;
            btnHikitsuguiLeaderRead.Text = "Leitura Hikitsugui\r\n引継ぎ閲覧";
            btnHikitsuguiLeaderRead.Click += btnHikitsuguiLeaderRead_Click;
            // 
            // btnSobraDePeca
            // 
            btnSobraDePeca.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSobraDePeca.Location = new Point(80, 220);
            btnSobraDePeca.Name = "btnSobraDePeca";
            btnSobraDePeca.Size = new Size(200, 80);
            btnSobraDePeca.TabIndex = 0;
            btnSobraDePeca.Text = "Sobra de Peça\r\n製品残り";
            btnSobraDePeca.Click += btnSobraDePeca_Click;
            // 
            // btnPR
            // 
            btnPR.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPR.Location = new Point(568, 120);
            btnPR.Name = "btnPR";
            btnPR.Size = new Size(200, 80);
            btnPR.TabIndex = 0;
            btnPR.Text = "PR\r\nPR文書";
            btnPR.Click += btnPR_Click;
            // 
            // btnCL
            // 
            btnCL.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCL.Location = new Point(568, 220);
            btnCL.Name = "btnCL";
            btnCL.Size = new Size(200, 80);
            btnCL.TabIndex = 0;
            btnCL.Text = "CL\r\nCL文書";
            btnCL.Click += btnCL_Click;
            // 
            // panel1
            // 
            panel1.AccessibleName = "teste";
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(800, 99);
            panel1.Name = "panel1";
            panel1.Size = new Size(223, 310);
            panel1.TabIndex = 10;
            // 
            // panel2
            // 
            panel2.AccessibleName = "teste";
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Location = new Point(68, 99);
            panel2.Name = "panel2";
            panel2.Size = new Size(464, 310);
            panel2.TabIndex = 11;
            // 
            // panel3
            // 
            panel3.AccessibleName = "teste";
            panel3.BorderStyle = BorderStyle.Fixed3D;
            panel3.Location = new Point(556, 99);
            panel3.Name = "panel3";
            panel3.Size = new Size(223, 310);
            panel3.TabIndex = 11;
            // 
            // FormDashboard
            // 
            ClientSize = new Size(1077, 494);
            Controls.Add(btnSobraDePeca);
            Controls.Add(btnHikitsuguiLeaderRead);
            Controls.Add(btnHikitsugui);
            Controls.Add(btnFollowUp);
            Controls.Add(btnAccessControl);
            Controls.Add(btnOperadores);
            Controls.Add(btnAtribuir);
            Controls.Add(btnRelatorios);
            Controls.Add(panelHeader);
            Controls.Add(panelFooter);
            Controls.Add(btnAdmin);
            Controls.Add(btnPR);
            Controls.Add(btnCL);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Controls.Add(panel3);
            Name = "FormDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TeamOps - Dashboard";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
    }
}
