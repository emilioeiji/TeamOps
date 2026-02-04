// Project: TeamOps.UI
// File: Forms/FormDashboard.cs

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

        // ---------------------------------------------------------
        // PERMISSÃO
        // ---------------------------------------------------------
        private bool HasAccess(AccessLevel requiredLevel)
        {
            return _user.AccessLevel >= requiredLevel;
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show(
                "Acesso negado. Permissão insuficiente.",
                "Acesso Negado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        // ---------------------------------------------------------
        // BOTÕES DO DASHBOARD
        // ---------------------------------------------------------

        private void btnOperadores_Click(object sender, EventArgs e)
        {
            var form = new FormOperators();
            form.ShowDialog();
        }

        private void btnAtribuir_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.Admin))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormAssignments();
            form.ShowDialog();
        }

        private void btnRelatorios_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.GL))
            {
                ShowAccessDenied();
                return;
            }

            MessageBox.Show("Abrir tela de Relatórios...");
            // TODO: FormReports
        }

        private void btnFollowUp_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormFollowUp();
            form.ShowDialog();
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.Admin))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormAdmin();
            form.ShowDialog();
        }

        private void btnAccessControl_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.Admin))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormAccessControl();
            form.ShowDialog();
        }

        // ---------------------------------------------------------
        // HIKITSUGUI (será ativado quando você adicionar o botão)
        // ---------------------------------------------------------
        private void btnHikitsugui_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            // Aqui você vai injetar os repositórios reais
            // quando integrar com o restante do sistema.
            var form = new FormHikitsugui(
                currentShift: null,          // ajustar depois
                currentOperator: null,       // ajustar depois
                hikitsuguiRepository: null,  // ajustar depois
                categoryRepository: null,    // ajustar depois
                equipmentRepository: null,   // ajustar depois
                localRepository: null        // ajustar depois
            );

            form.ShowDialog();
        }
    }
}
