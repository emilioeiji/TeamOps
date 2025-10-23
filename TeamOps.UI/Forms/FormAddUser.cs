// Project: TeamOps.UI
// File: Forms/FormAddUser.cs
using System;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormAddUser : Form
    {
        private readonly UserRepository _userRepo;

        public FormAddUser()
        {
            InitializeComponent();
            _userRepo = new UserRepository(Program.ConnectionFactory);
            cmbAccessLevel.DataSource = Enum.GetValues(typeof(AccessLevel));
            cmbAccessLevel.SelectedItem = AccessLevel.Basic; // valor padrão
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            var login = txtLogin.Text.Trim();
            var name = txtName.Text.Trim();
            var password = txtPassword.Text.Trim();

            // 🔒 Verificação segura do nível de acesso
            if (cmbAccessLevel.SelectedItem is not AccessLevel level)
            {
                MessageBox.Show("Selecione um nível de acesso válido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Login e senha são obrigatórios.");
                return;
            }

            var user = new User
            {
                Login = login,
                Name = name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                AccessLevel = level,
                CreatedAt = DateTime.Now
            };

            _userRepo.Add(user);
            MessageBox.Show("Usuário adicionado com sucesso.");
            this.Close();
        }
    }
}
