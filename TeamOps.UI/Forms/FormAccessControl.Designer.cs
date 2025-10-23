namespace TeamOps.UI.Forms
{
    partial class FormAccessControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.Label lblAccessLevel;
        private System.Windows.Forms.ComboBox cmbAccessLevel;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.Button btnUpdateAccess;
        private System.Windows.Forms.Button btnResetPassword;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnAddUser;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvUsers = new System.Windows.Forms.DataGridView();
            lblAccessLevel = new System.Windows.Forms.Label();
            cmbAccessLevel = new System.Windows.Forms.ComboBox();
            lblNewPassword = new System.Windows.Forms.Label();
            txtNewPassword = new System.Windows.Forms.TextBox();
            btnUpdateAccess = new System.Windows.Forms.Button();
            btnResetPassword = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            btnAddUser = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            SuspendLayout();
            // 
            // dgvUsers
            // 
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.AllowUserToDeleteRows = false;
            dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsers.Location = new System.Drawing.Point(12, 12);
            dgvUsers.MultiSelect = false;
            dgvUsers.Name = "dgvUsers";
            dgvUsers.ReadOnly = true;
            dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.Size = new System.Drawing.Size(560, 200);
            dgvUsers.TabIndex = 0;
            // 
            // lblAccessLevel
            // 
            lblAccessLevel.AutoSize = true;
            lblAccessLevel.Location = new System.Drawing.Point(12, 230);
            lblAccessLevel.Name = "lblAccessLevel";
            lblAccessLevel.Size = new System.Drawing.Size(93, 15);
            lblAccessLevel.TabIndex = 1;
            lblAccessLevel.Text = "Nível de Acesso:";
            // 
            // cmbAccessLevel
            // 
            cmbAccessLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbAccessLevel.Location = new System.Drawing.Point(110, 228);
            cmbAccessLevel.Name = "cmbAccessLevel";
            cmbAccessLevel.Size = new System.Drawing.Size(150, 23);
            cmbAccessLevel.TabIndex = 2;
            // 
            // lblNewPassword
            // 
            lblNewPassword.AutoSize = true;
            lblNewPassword.Location = new System.Drawing.Point(12, 270);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new System.Drawing.Size(73, 15);
            lblNewPassword.TabIndex = 3;
            lblNewPassword.Text = "Nova Senha:";
            // 
            // txtNewPassword
            // 
            txtNewPassword.Location = new System.Drawing.Point(110, 267);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.PasswordChar = '*';
            txtNewPassword.Size = new System.Drawing.Size(200, 23);
            txtNewPassword.TabIndex = 4;
            // 
            // btnUpdateAccess
            // 
            btnUpdateAccess.Location = new System.Drawing.Point(350, 225);
            btnUpdateAccess.Name = "btnUpdateAccess";
            btnUpdateAccess.Size = new System.Drawing.Size(100, 30);
            btnUpdateAccess.TabIndex = 5;
            btnUpdateAccess.Text = "Atualizar Nível";
            btnUpdateAccess.UseVisualStyleBackColor = true;
            btnUpdateAccess.Click += btnUpdateAccess_Click;
            // 
            // btnResetPassword
            // 
            btnResetPassword.Location = new System.Drawing.Point(350, 263);
            btnResetPassword.Name = "btnResetPassword";
            btnResetPassword.Size = new System.Drawing.Size(100, 30);
            btnResetPassword.TabIndex = 6;
            btnResetPassword.Text = "Resetar Senha";
            btnResetPassword.UseVisualStyleBackColor = true;
            btnResetPassword.Click += btnResetPassword_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(472, 310);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(100, 30);
            btnClose.TabIndex = 7;
            btnClose.Text = "Fechar";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnAddUser
            // 
            btnAddUser.Location = new System.Drawing.Point(12, 310);
            btnAddUser.Name = "btnAddUser";
            btnAddUser.Size = new System.Drawing.Size(100, 30);
            btnAddUser.TabIndex = 8;
            btnAddUser.Text = "Adicionar";
            btnAddUser.UseVisualStyleBackColor = true;
            btnAddUser.Click += btnAddUser_Click;
            // 
            // FormAccessControl
            // 
            ClientSize = new System.Drawing.Size(584, 361);
            Controls.Add(btnClose);
            Controls.Add(btnResetPassword);
            Controls.Add(btnUpdateAccess);
            Controls.Add(txtNewPassword);
            Controls.Add(lblNewPassword);
            Controls.Add(cmbAccessLevel);
            Controls.Add(lblAccessLevel);
            Controls.Add(dgvUsers);
            Controls.Add(btnAddUser);
            Name = "FormAccessControl";
            Text = "Controle de Acesso";
            Load += FormAccessControl_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
