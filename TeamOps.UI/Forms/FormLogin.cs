// Project: TeamOps.UI
// File: Forms/FormLogin.cs
using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;
using TeamOps.Core.Common;

namespace TeamOps.UI.Forms
{
    public partial class FormLogin : Form
    {
        private readonly UserRepository _userRepo;

        public FormLogin()
        {
            InitializeComponent();
            _userRepo = new UserRepository(Program.ConnectionFactory);
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            var login = txtLogin.Text.Trim();
            var senha = txtSenha.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                lblMensagem.Text = "Informe login e senha.";
                return;
            }

            // 🔹 Normaliza login se começar com "FJ"
            if (login.StartsWith("FJ", StringComparison.OrdinalIgnoreCase))
            {
                login = login.ToUpper();
            }

            var user = _userRepo.GetByLogin(login);

            if (user != null && BCrypt.Net.BCrypt.Verify(senha, user.PasswordHash))
            {
                Program.CurrentUser = user;
                lblMensagem.Text = string.Empty;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                lblMensagem.Text = "Login ou senha inválidos.";
            }
        }
    }
}

