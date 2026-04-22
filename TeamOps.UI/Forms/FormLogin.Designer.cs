// Project: TeamOps.UI
// File: Forms/FormLogin.Designer.cs
namespace TeamOps.UI.Forms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label lblSenha;
        private System.Windows.Forms.TextBox txtSenha;
        private System.Windows.Forms.Button btnEntrar;
        private System.Windows.Forms.Button btnAlterarSenha;
        private System.Windows.Forms.Label lblMensagem;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblLogin = new Label();
            txtLogin = new TextBox();
            lblSenha = new Label();
            txtSenha = new TextBox();
            btnEntrar = new Button();
            btnAlterarSenha = new Button();
            lblMensagem = new Label();
            panelHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            pictureBoxLogo = new PictureBox();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            SuspendLayout();
            // 
            // lblLogin
            // 
            lblLogin.AutoSize = true;
            lblLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblLogin.Location = new Point(72, 252);
            lblLogin.Name = "lblLogin";
            lblLogin.Size = new Size(123, 19);
            lblLogin.TabIndex = 1;
            lblLogin.Text = "Login / ログイン";
            // 
            // txtLogin
            // 
            txtLogin.Font = new Font("Segoe UI", 11F);
            txtLogin.Location = new Point(72, 278);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(406, 27);
            txtLogin.TabIndex = 2;
            // 
            // lblSenha
            // 
            lblSenha.AutoSize = true;
            lblSenha.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblSenha.Location = new Point(72, 326);
            lblSenha.Name = "lblSenha";
            lblSenha.Size = new Size(158, 19);
            lblSenha.TabIndex = 3;
            lblSenha.Text = "Senha / パスワード";
            // 
            // txtSenha
            // 
            txtSenha.Font = new Font("Segoe UI", 11F);
            txtSenha.Location = new Point(72, 352);
            txtSenha.Name = "txtSenha";
            txtSenha.Size = new Size(406, 27);
            txtSenha.TabIndex = 4;
            txtSenha.UseSystemPasswordChar = true;
            // 
            // btnEntrar
            // 
            btnEntrar.BackColor = Color.FromArgb(38, 76, 140);
            btnEntrar.FlatStyle = FlatStyle.Flat;
            btnEntrar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnEntrar.ForeColor = Color.White;
            btnEntrar.Location = new Point(72, 406);
            btnEntrar.Name = "btnEntrar";
            btnEntrar.Size = new Size(196, 46);
            btnEntrar.TabIndex = 5;
            btnEntrar.Text = "Entrar / ログイン";
            btnEntrar.UseVisualStyleBackColor = false;
            btnEntrar.Click += btnEntrar_Click;
            // 
            // btnAlterarSenha
            // 
            btnAlterarSenha.Font = new Font("Segoe UI", 10.5F);
            btnAlterarSenha.Location = new Point(282, 406);
            btnAlterarSenha.Name = "btnAlterarSenha";
            btnAlterarSenha.Size = new Size(196, 46);
            btnAlterarSenha.TabIndex = 6;
            btnAlterarSenha.Text = "Alterar Senha / パスワード変更";
            btnAlterarSenha.UseVisualStyleBackColor = true;
            btnAlterarSenha.Click += btnAlterarSenha_Click;
            // 
            // lblMensagem
            // 
            lblMensagem.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
            lblMensagem.ForeColor = Color.Red;
            lblMensagem.Location = new Point(72, 468);
            lblMensagem.Name = "lblMensagem";
            lblMensagem.Size = new Size(406, 46);
            lblMensagem.TabIndex = 7;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(45, 85, 155);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(550, 94);
            panelHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.WhiteSmoke;
            lblSubtitle.Location = new Point(24, 58);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(326, 17);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Acesso ao sistema TeamOps / TeamOpsシステムにログイン";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(24, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(225, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "TeamOps - ログイン";
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackgroundImageLayout = ImageLayout.Center;
            pictureBoxLogo.Location = new Point(0, 110);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(550, 118);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 8;
            pictureBoxLogo.TabStop = false;
            // 
            // FormLogin
            // 
            AcceptButton = btnEntrar;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 540);
            Controls.Add(btnAlterarSenha);
            Controls.Add(pictureBoxLogo);
            Controls.Add(panelHeader);
            Controls.Add(lblLogin);
            Controls.Add(txtLogin);
            Controls.Add(lblSenha);
            Controls.Add(txtSenha);
            Controls.Add(btnEntrar);
            Controls.Add(lblMensagem);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login / ログイン - TeamOps";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private PictureBox pictureBoxLogo;
    }
}
