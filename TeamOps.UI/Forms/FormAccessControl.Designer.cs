namespace TeamOps.UI.Forms
{
    partial class FormAccessControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.Label lblAccessLevel;
        private System.Windows.Forms.NumericUpDown numAccessLevel;
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
            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.lblAccessLevel = new System.Windows.Forms.Label();
            this.numAccessLevel = new System.Windows.Forms.NumericUpDown();
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.btnUpdateAccess = new System.Windows.Forms.Button();
            this.btnResetPassword = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            //this.btnClose.FlatStyle = FlatStyle.Flat;
            //this.btnClose.BackColor = Color.LightGray;
            //this.btnClose.ForeColor = Color.Black;
            //this.btnClose.Image = SystemIcons.Error.ToBitmap();
            //this.btnClose.ImageAlign = ContentAlignment.MiddleLeft;
            //this.btnClose.TextAlign = ContentAlignment.MiddleRight;
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccessLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Location = new System.Drawing.Point(12, 12);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new System.Drawing.Size(560, 200);
            this.dgvUsers.TabIndex = 0;
            // 
            // lblAccessLevel
            // 
            this.lblAccessLevel.AutoSize = true;
            this.lblAccessLevel.Location = new System.Drawing.Point(12, 230);
            this.lblAccessLevel.Name = "lblAccessLevel";
            this.lblAccessLevel.Size = new System.Drawing.Size(92, 15);
            this.lblAccessLevel.TabIndex = 1;
            this.lblAccessLevel.Text = "Nível de Acesso:";
            // 
            // numAccessLevel
            // 
            this.numAccessLevel.Location = new System.Drawing.Point(110, 228);
            this.numAccessLevel.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numAccessLevel.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numAccessLevel.Name = "numAccessLevel";
            this.numAccessLevel.Size = new System.Drawing.Size(60, 23);
            this.numAccessLevel.TabIndex = 2;
            this.numAccessLevel.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblNewPassword
            // 
            this.lblNewPassword.AutoSize = true;
            this.lblNewPassword.Location = new System.Drawing.Point(12, 270);
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.Size = new System.Drawing.Size(72, 15);
            this.lblNewPassword.TabIndex = 3;
            this.lblNewPassword.Text = "Nova Senha:";
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.Location = new System.Drawing.Point(110, 267);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '*';
            this.txtNewPassword.Size = new System.Drawing.Size(200, 23);
            this.txtNewPassword.TabIndex = 4;
            // 
            // btnUpdateAccess
            // 
            this.btnUpdateAccess.Location = new System.Drawing.Point(350, 225);
            this.btnUpdateAccess.Name = "btnUpdateAccess";
            this.btnUpdateAccess.Size = new System.Drawing.Size(100, 30);
            this.btnUpdateAccess.TabIndex = 5;
            this.btnUpdateAccess.Text = "Atualizar Nível";
            this.btnUpdateAccess.UseVisualStyleBackColor = true;
            this.btnUpdateAccess.Click += new System.EventHandler(this.btnUpdateAccess_Click);
            // 
            // btnResetPassword
            // 
            this.btnResetPassword.Location = new System.Drawing.Point(350, 263);
            this.btnResetPassword.Name = "btnResetPassword";
            this.btnResetPassword.Size = new System.Drawing.Size(100, 30);
            this.btnResetPassword.TabIndex = 6;
            this.btnResetPassword.Text = "Resetar Senha";
            this.btnResetPassword.UseVisualStyleBackColor = true;
            this.btnResetPassword.Click += new System.EventHandler(this.btnResetPassword_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(472, 310);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Fechar";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnAddUser
            // 
            this.btnAddUser = new System.Windows.Forms.Button();
            this.btnAddUser.Location = new System.Drawing.Point(12, 310);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Size = new System.Drawing.Size(100, 30);
            this.btnAddUser.TabIndex = 8;
            this.btnAddUser.Text = "Adicionar";
            this.btnAddUser.UseVisualStyleBackColor = true;
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
            // 
            // FormAccessControl
            // 
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnResetPassword);
            this.Controls.Add(this.btnUpdateAccess);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.lblNewPassword);
            this.Controls.Add(this.numAccessLevel);
            this.Controls.Add(this.lblAccessLevel);
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(this.btnAddUser);
            this.Name = "FormAccessControl";
            this.Text = "Controle de Acesso";
            this.Load += new System.EventHandler(this.FormAccessControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccessLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
