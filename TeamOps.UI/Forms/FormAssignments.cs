// Project: TeamOps.UI
// File: Forms/FormAssignments.cs
using System;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormAssignments : Form
    {
        private readonly GroupLeaderRepository _glRepo;
        private readonly OperatorRepository _opRepo;
        private readonly AssignmentRepository _assignRepo;

        public FormAssignments()
        {
            InitializeComponent();

            _glRepo = new GroupLeaderRepository(Program.ConnectionFactory);
            _opRepo = new OperatorRepository(Program.ConnectionFactory);
            _assignRepo = new AssignmentRepository(Program.ConnectionFactory);

            LoadGroupLeaders();
        }

        private void LoadGroupLeaders()
        {
            cmbGroupLeaders.SelectedIndexChanged -= cmbGroupLeaders_SelectedIndexChanged;

            var leaders = _glRepo.GetAll();
            cmbGroupLeaders.DataSource = leaders;
            cmbGroupLeaders.DisplayMember = "Name";
            cmbGroupLeaders.ValueMember = "Id";

            if (leaders.Count > 0)
            {
                cmbGroupLeaders.SelectedIndex = 0;
                LoadOperators();
            }

            cmbGroupLeaders.SelectedIndexChanged += cmbGroupLeaders_SelectedIndexChanged;
        }

        private void LoadOperators()
        {
            if (cmbGroupLeaders.SelectedValue is int glId)
            {
                var allOps = _opRepo.GetAll();
                var assignedOps = _assignRepo.GetByGroupLeader(glId);

                // Operadores disponíveis
                lstAvailable.DataSource = allOps
                    .Where(o => !assignedOps.Any(a => a.CodigoFJ == o.CodigoFJ))
                    .Select(o => new
                    {
                        CodigoFJ = o.CodigoFJ,
                        Display = $"{o.CodigoFJ} - {o.NameRomanji}"
                    })
                    .ToList();
                lstAvailable.DisplayMember = "Display";
                lstAvailable.ValueMember = "CodigoFJ";

                // Operadores atribuídos
                lstAssigned.DataSource = assignedOps
                    .Select(o => new
                    {
                        CodigoFJ = o.CodigoFJ,
                        Display = $"{o.CodigoFJ} - {o.NameRomanji}"
                    })
                    .ToList();
                lstAssigned.DisplayMember = "Display";
                lstAssigned.ValueMember = "CodigoFJ";
            }
        }

        private void cmbGroupLeaders_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadOperators();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbGroupLeaders.SelectedValue is int glId && lstAvailable.SelectedValue is string codigoFJ)
            {
                _assignRepo.Add(glId, codigoFJ);
                LoadOperators();
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (cmbGroupLeaders.SelectedValue is int glId && lstAssigned.SelectedValue is string codigoFJ)
            {
                _assignRepo.Remove(glId, codigoFJ);
                LoadOperators();
            }
        }
    }
}
