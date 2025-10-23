// Project: TeamOps.UI
// File: Forms/FormAccessControl.cs
using System;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormAccessControl : Form
    {
        private readonly UserRepository _userRepo;

        public FormAccessControl()
        {
            InitializeComponent();
            _userRepo = new UserRepository(Program.ConnectionFactory);
        }

        private void FormAccessControl_Load(object sender, EventArgs e)
        {
            LoadUsers();
            cmbAccessLevel.DataSource = Enum.GetValues(typeof(AccessLevel));
        }

        private void LoadUsers()
        {
            var users = _userRepo.GetAll();
            dgvUsers.DataSource = users;

            // Oculta colunas sensíveis com verificação segura
            var colFJ = dgvUsers.Columns["CodigoFJ"];
            if (colFJ != null)
                colFJ.Visible = false;

            var colHash = dgvUsers.Columns["PasswordHash"];
            if (colHash != null)
                colHash.Visible = false;
        }

        private void btnUpdateAccess_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is User user)
            {
                if (cmbAccessLevel.SelectedItem is AccessLevel selectedLevel)
                {
                    user.AccessLevel = selectedLevel;
                    _userRepo.Update(user);
                    MessageBox.Show("Nível de acesso atualizado com sucesso.");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Selecione um nível de acesso válido.");
                }
                _userRepo.Update(user);
                MessageBox.Show("Nível de acesso atualizado com sucesso.");
                LoadUsers();
            }
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is User user)
            {
                string newPassword = txtNewPassword.Text.Trim();
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    MessageBox.Show("Informe uma nova senha.");
                    return;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _userRepo.Update(user);
                MessageBox.Show("Senha redefinida com sucesso.");
                txtNewPassword.Clear();
            }
        }
        private void btnAddUser_Click(object sender, EventArgs e)
        {
            var form = new FormAddUser();
            form.ShowDialog();
            LoadUsers(); // recarrega após adicionar
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
