// Project: TeamOps.UI
// File: Forms/FormLogin.cs
using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormLogin : Form
    {
        private readonly UserRepository _userRepo;

        public FormLogin()
        {
            InitializeComponent();
            _userRepo = new UserRepository(Program.ConnectionFactory);
            pictureBoxLogo.Image = Image.FromFile("Logo.png");
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            var login = NormalizeLogin(txtLogin.Text);
            var senha = txtSenha.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                lblMensagem.ForeColor = Color.Firebrick;
                lblMensagem.Text = "Informe login e senha. / ログインとパスワードを入力してください。";
                return;
            }

            var user = _userRepo.GetByLogin(login);

            if (user != null && BCrypt.Net.BCrypt.Verify(senha, user.PasswordHash))
            {
                Program.CurrentUser = user;
                lblMensagem.Text = string.Empty;
                DialogResult = DialogResult.OK;
            }
            else
            {
                lblMensagem.ForeColor = Color.Firebrick;
                lblMensagem.Text = "Login ou senha invalidos. / ログインまたはパスワードが正しくありません。";
            }
        }

        private void btnAlterarSenha_Click(object sender, EventArgs e)
        {
            using var form = new FormChangePassword(txtLogin.Text);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                txtSenha.Clear();
                lblMensagem.ForeColor = Color.FromArgb(30, 108, 67);
                lblMensagem.Text = "Senha alterada. Entre com a nova senha. / 新しいパスワードでログインしてください。";
            }
        }

        private static string NormalizeLogin(string? login)
        {
            var normalized = (login ?? string.Empty).Trim();

            if (normalized.StartsWith("FJ", StringComparison.OrdinalIgnoreCase))
                return normalized.ToUpperInvariant();

            return normalized;
        }
    }
}

