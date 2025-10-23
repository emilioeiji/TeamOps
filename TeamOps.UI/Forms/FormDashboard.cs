// Project: TeamOps.UI
// File: Forms/FormDashboard.cs
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using AppUser = TeamOps.Core.Entities.User;


namespace TeamOps.UI.Forms
{
    public partial class FormDashboard : Form
    {
        private readonly AppUser _user;

        public FormDashboard(AppUser user)
        {
            InitializeComponent();
            _user = user;

            lblUser.Text = $"Bem-vindo, {_user.Name}";
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private bool HasAccess(AccessLevel requiredLevel)
        {
            return _user.AccessLevel >= requiredLevel;
        }

        private void btnOperadores_Click(object sender, EventArgs e)
        {
            // Operadores: qualquer usuário logado pode abrir
            var form = new FormOperators();
            form.ShowDialog();
        }

        private void btnAtribuir_Click(object sender, EventArgs e)
        {
            if (HasAccess(AccessLevel.Admin))
            {
                var adminForm = new FormAssignments();
                adminForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Acesso negado. Apenas administradores.");
            }
        }

        private void btnRelatorios_Click(object sender, EventArgs e)
        {
            if (HasAccess(AccessLevel.GL))
            {
                MessageBox.Show("Abrir tela de Relatórios...");
                // Aqui depois chamaremos o FormReports
            }
            else
            {
                MessageBox.Show("Acesso negado. Apenas líderes ou administradores podem acessar relatórios.");
            }
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            if (HasAccess(AccessLevel.Admin))
            {
                var adminForm = new FormAdmin();
                adminForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Acesso negado. Apenas administradores podem acessar o painel administrativo.");
            }
        }

        private void btnAccessControl_Click(object sender, EventArgs e)
        {
            if (HasAccess(AccessLevel.Admin))
            {
                var adminForm = new FormAccessControl();
                adminForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Acesso negado. Apenas administradores podem acessar o painel administrativo.");
            }
        }
    }
}
