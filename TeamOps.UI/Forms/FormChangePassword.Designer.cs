namespace TeamOps.UI.Forms
{
    partial class FormChangePassword
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label lblSenhaAtual;
        private System.Windows.Forms.TextBox txtSenhaAtual;
        private System.Windows.Forms.Label lblNovaSenha;
        private System.Windows.Forms.TextBox txtNovaSenha;
        private System.Windows.Forms.Label lblConfirmarSenha;
        private System.Windows.Forms.TextBox txtConfirmarSenha;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label lblMensagem;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            lblLogin = new Label();
            txtLogin = new TextBox();
            lblSenhaAtual = new Label();
            txtSenhaAtual = new TextBox();
            lblNovaSenha = new Label();
            txtNovaSenha = new TextBox();
            lblConfirmarSenha = new Label();
            txtConfirmarSenha = new TextBox();
            btnSalvar = new Button();
            btnCancelar = new Button();
            lblMensagem = new Label();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(38, 76, 140);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(560, 96);
            panelHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.WhiteSmoke;
            lblSubtitle.Location = new Point(24, 55);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(251, 17);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Atualize sua senha com seguranca / 安全に更新";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(24, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(282, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Alterar Senha / パスワード変更";
            // 
            // lblLogin
            // 
            lblLogin.AutoSize = true;
            lblLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblLogin.Location = new Point(52, 128);
            lblLogin.Name = "lblLogin";
            lblLogin.Size = new Size(123, 19);
            lblLogin.TabIndex = 1;
            lblLogin.Text = "Login / ログイン";
            // 
            // txtLogin
            // 
            txtLogin.Font = new Font("Segoe UI", 11F);
            txtLogin.Location = new Point(52, 151);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(456, 27);
            txtLogin.TabIndex = 2;
            // 
            // lblSenhaAtual
            // 
            lblSenhaAtual.AutoSize = true;
            lblSenhaAtual.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblSenhaAtual.Location = new Point(52, 193);
            lblSenhaAtual.Name = "lblSenhaAtual";
            lblSenhaAtual.Size = new Size(207, 19);
            lblSenhaAtual.TabIndex = 3;
            lblSenhaAtual.Text = "Senha Atual / 現在のパスワード";
            // 
            // txtSenhaAtual
            // 
            txtSenhaAtual.Font = new Font("Segoe UI", 11F);
            txtSenhaAtual.Location = new Point(52, 216);
            txtSenhaAtual.Name = "txtSenhaAtual";
            txtSenhaAtual.Size = new Size(456, 27);
            txtSenhaAtual.TabIndex = 4;
            txtSenhaAtual.UseSystemPasswordChar = true;
            // 
            // lblNovaSenha
            // 
            lblNovaSenha.AutoSize = true;
            lblNovaSenha.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblNovaSenha.Location = new Point(52, 258);
            lblNovaSenha.Name = "lblNovaSenha";
            lblNovaSenha.Size = new Size(205, 19);
            lblNovaSenha.TabIndex = 5;
            lblNovaSenha.Text = "Nova Senha / 新しいパスワード";
            // 
            // txtNovaSenha
            // 
            txtNovaSenha.Font = new Font("Segoe UI", 11F);
            txtNovaSenha.Location = new Point(52, 281);
            txtNovaSenha.Name = "txtNovaSenha";
            txtNovaSenha.Size = new Size(456, 27);
            txtNovaSenha.TabIndex = 6;
            txtNovaSenha.UseSystemPasswordChar = true;
            // 
            // lblConfirmarSenha
            // 
            lblConfirmarSenha.AutoSize = true;
            lblConfirmarSenha.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblConfirmarSenha.Location = new Point(52, 323);
            lblConfirmarSenha.Name = "lblConfirmarSenha";
            lblConfirmarSenha.Size = new Size(254, 19);
            lblConfirmarSenha.TabIndex = 7;
            lblConfirmarSenha.Text = "Confirmar Senha / パスワード確認";
            // 
            // txtConfirmarSenha
            // 
            txtConfirmarSenha.Font = new Font("Segoe UI", 11F);
            txtConfirmarSenha.Location = new Point(52, 346);
            txtConfirmarSenha.Name = "txtConfirmarSenha";
            txtConfirmarSenha.Size = new Size(456, 27);
            txtConfirmarSenha.TabIndex = 8;
            txtConfirmarSenha.UseSystemPasswordChar = true;
            // 
            // btnSalvar
            // 
            btnSalvar.BackColor = Color.FromArgb(38, 76, 140);
            btnSalvar.FlatStyle = FlatStyle.Flat;
            btnSalvar.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnSalvar.ForeColor = Color.White;
            btnSalvar.Location = new Point(244, 410);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(264, 42);
            btnSalvar.TabIndex = 10;
            btnSalvar.Text = "Salvar Nova Senha / 保存";
            btnSalvar.UseVisualStyleBackColor = false;
            btnSalvar.Click += btnSalvar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Font = new Font("Segoe UI", 10.5F);
            btnCancelar.Location = new Point(52, 410);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(174, 42);
            btnCancelar.TabIndex = 9;
            btnCancelar.Text = "Cancelar / キャンセル";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // lblMensagem
            // 
            lblMensagem.ForeColor = Color.Firebrick;
            lblMensagem.Location = new Point(52, 468);
            lblMensagem.Name = "lblMensagem";
            lblMensagem.Size = new Size(456, 45);
            lblMensagem.TabIndex = 11;
            // 
            // FormChangePassword
            // 
            AcceptButton = btnSalvar;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 528);
            Controls.Add(lblMensagem);
            Controls.Add(btnCancelar);
            Controls.Add(btnSalvar);
            Controls.Add(txtConfirmarSenha);
            Controls.Add(lblConfirmarSenha);
            Controls.Add(txtNovaSenha);
            Controls.Add(lblNovaSenha);
            Controls.Add(txtSenhaAtual);
            Controls.Add(lblSenhaAtual);
            Controls.Add(txtLogin);
            Controls.Add(lblLogin);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormChangePassword";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Alterar Senha / パスワード変更";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
