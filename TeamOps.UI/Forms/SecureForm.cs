using System;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.UI;

namespace TeamOps.UI.Forms
{
    public class SecureForm : Form
    {
        /// <summary>
        /// Verifica se o usuário logado tem o nível de acesso exigido.
        /// Fecha o form se não tiver permissão.
        /// </summary>
        protected void EnsureAccess(AccessLevel requiredLevel)
        {
            if (Program.CurrentUser == null || Program.CurrentUser.AccessLevel < requiredLevel)
            {
                MessageBox.Show(
                    "Acesso negado. Seu nível de acesso não permite abrir esta tela.",
                    "Permissão insuficiente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.Close();
            }
        }
    }
}


