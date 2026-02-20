using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormReports : Form
    {
        private readonly Operator _currentOperator;
        private readonly Shift _currentShift;

        public FormReports(Operator currentOperator, Shift currentShift)
        {
            InitializeComponent();
            _currentOperator = currentOperator;
            _currentShift = currentShift;

            // Se quiser exibir algo no header futuramente, já está preparado
            // lblUser.Text = $"{_currentOperator.NameRomanji} - {_currentShift.NamePt}";
        }

        // ---------------------------------------------------------
        // EVENTOS DOS BOTÕES (GENÉRICOS)
        // ---------------------------------------------------------

        private void btnRepOperadores_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de Operadores...");
            // TODO: FormReportOperadores
        }

        private void btnRepPR_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de PR...");
            // TODO: FormReportPR
        }

        private void btnRepCL_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de CL...");
            // TODO: FormReportCL
        }

        private void btnRepHikitsugui_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de Hikitsugui...");
            // TODO: FormReportHikitsugui
        }

        private void btnRepSobra_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de Sobra de Peça...");
            // TODO: FormReportSobra
        }
    }
}
