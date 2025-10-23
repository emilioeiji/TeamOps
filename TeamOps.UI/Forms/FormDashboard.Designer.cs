// Project: TeamOps.UI
// File: Forms/FormDashboard.Designer.cs
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
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Label lblDate;

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
            panelFooter = new Panel();
            lblDate = new Label();
            btnAdmin = new Button();
            btnAccessControl = new Button();
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
            panelHeader.Size = new Size(800, 80);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(364, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "TeamOps – Gestão de Operadores";
            // 
            // lblUser
            // 
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 10F);
            lblUser.ForeColor = Color.White;
            lblUser.Location = new Point(1200, 30);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(0, 19);
            lblUser.TabIndex = 1;
            // 
            // btnOperadores
            // 
            btnOperadores.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnOperadores.Location = new Point(80, 120);
            btnOperadores.Name = "btnOperadores";
            btnOperadores.Size = new Size(200, 80);
            btnOperadores.TabIndex = 2;
            btnOperadores.Text = "Gerenciar Operadores";
            btnOperadores.Click += btnOperadores_Click;
            // 
            // btnAtribuir
            // 
            btnAtribuir.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAtribuir.Location = new Point(320, 120);
            btnAtribuir.Name = "btnAtribuir";
            btnAtribuir.Size = new Size(200, 80);
            btnAtribuir.TabIndex = 3;
            btnAtribuir.Text = "Atribuir Operadores";
            btnAtribuir.Click += btnAtribuir_Click;
            // 
            // btnRelatorios
            // 
            btnRelatorios.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRelatorios.Location = new Point(560, 120);
            btnRelatorios.Name = "btnRelatorios";
            btnRelatorios.Size = new Size(200, 80);
            btnRelatorios.TabIndex = 4;
            btnRelatorios.Text = "Relatórios";
            btnRelatorios.Click += btnRelatorios_Click;
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.LightGray;
            panelFooter.Controls.Add(lblDate);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 410);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(800, 40);
            panelFooter.TabIndex = 1;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Segoe UI", 9F);
            lblDate.ForeColor = Color.Black;
            lblDate.Location = new Point(20, 10);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(0, 15);
            lblDate.TabIndex = 0;
            // 
            // btnAdmin
            // 
            btnAdmin.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAdmin.Location = new Point(80, 220);
            btnAdmin.Name = "btnAdmin";
            btnAdmin.Size = new Size(200, 80);
            btnAdmin.TabIndex = 0;
            btnAdmin.Text = "Admin";
            btnAdmin.Click += btnAdmin_Click;
            // 
            // btnAccessControl
            // 
            btnAccessControl.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAccessControl.Location = new Point(320, 220);
            btnAccessControl.Name = "btnAccessControl";
            btnAccessControl.Size = new Size(200, 80);
            btnAccessControl.TabIndex = 1;
            btnAccessControl.Text = "Access";
            btnAccessControl.Click += btnAccessControl_Click;
            // 
            // FormDashboard
            // 
            ClientSize = new Size(800, 450);
            Controls.Add(btnAccessControl);
            Controls.Add(btnOperadores);
            Controls.Add(btnAtribuir);
            Controls.Add(btnRelatorios);
            Controls.Add(panelHeader);
            Controls.Add(panelFooter);
            Controls.Add(btnAdmin);
            Name = "FormDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TeamOps - Dashboard";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }
    }
}
