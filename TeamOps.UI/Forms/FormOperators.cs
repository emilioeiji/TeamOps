using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormOperators : Form
    {
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly GroupRepository _groupRepo;
        private readonly SectorRepository _sectorRepo;

        public FormOperators()
        {
            InitializeComponent();

            _opRepo = new OperatorRepository(Program.ConnectionFactory);
            _shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _groupRepo = new GroupRepository(Program.ConnectionFactory);
            _sectorRepo = new SectorRepository(Program.ConnectionFactory);

            LoadLookups();
            LoadOperators();
        }

        private void LoadLookups()
        {
            cmbShift.DataSource = _shiftRepo.GetAll();
            cmbShift.DisplayMember = "NamePt";
            cmbShift.ValueMember = "Id";

            cmbGroup.DataSource = _groupRepo.GetAll();
            cmbGroup.DisplayMember = "NamePt";
            cmbGroup.ValueMember = "Id";

            cmbSector.DataSource = _sectorRepo.GetAll();
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
        }

        private void LoadOperators()
        {
            dgvOperators.DataSource = _opRepo.GetAll();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 1) Validação de campos obrigatórios
            if (string.IsNullOrWhiteSpace(txtCodigoFJ.Text) ||
                string.IsNullOrWhiteSpace(txtRomanji.Text) ||
                string.IsNullOrWhiteSpace(txtNihongo.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.");
                return;
            }

            // 2) Verificar duplicação de CodigoFJ
            var existing = _opRepo.GetByCodigoFJ(txtCodigoFJ.Text.Trim());
            if (existing != null)
            {
                MessageBox.Show("Já existe um operador com este CódigoFJ.");
                return;
            }

            // 3) Validar datas
            if (chkHasEnd.Checked && dtpEnd.Value < dtpStart.Value)
            {
                MessageBox.Show("A data de término não pode ser anterior à data de início.");
                return;
            }

            var op = new Operator
            {
                CodigoFJ = txtCodigoFJ.Text.Trim(),
                NameRomanji = txtRomanji.Text.Trim(),
                NameNihongo = txtNihongo.Text.Trim(),
                ShiftId = Convert.ToInt32(cmbShift.SelectedValue),
                GroupId = Convert.ToInt32(cmbGroup.SelectedValue),
                SectorId = Convert.ToInt32(cmbSector.SelectedValue),
                StartDate = dtpStart.Value,
                EndDate = chkHasEnd.Checked ? dtpEnd.Value : null,
                Trainer = chkTrainer.Checked,
                Status = chkStatus.Checked
            };

            _opRepo.Add(op);
            ClearForm();
            LoadOperators();
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvOperators.CurrentRow?.DataBoundItem is Operator op)
            {
                // 1) Validação de campos obrigatórios
                if (string.IsNullOrWhiteSpace(txtRomanji.Text) ||
                    string.IsNullOrWhiteSpace(txtNihongo.Text))
                {
                    MessageBox.Show("Preencha todos os campos obrigatórios.");
                    return;
                }

                // 2) Validar datas
                if (chkHasEnd.Checked && dtpEnd.Value < dtpStart.Value)
                {
                    MessageBox.Show("A data de término não pode ser anterior à data de início.");
                    return;
                }

                // 3) Não permitir alteração do CodigoFJ (PK)
                txtCodigoFJ.ReadOnly = true;

                op.NameRomanji = txtRomanji.Text.Trim();
                op.NameNihongo = txtNihongo.Text.Trim();
                op.ShiftId = Convert.ToInt32(cmbShift.SelectedValue);
                op.GroupId = Convert.ToInt32(cmbGroup.SelectedValue);
                op.SectorId = Convert.ToInt32(cmbSector.SelectedValue);
                op.StartDate = dtpStart.Value;
                op.EndDate = chkHasEnd.Checked ? dtpEnd.Value : null;
                op.Trainer = chkTrainer.Checked;
                op.Status = chkStatus.Checked;

                _opRepo.Update(op);
                ClearForm();
                LoadOperators();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOperators.CurrentRow?.DataBoundItem is Operator op)
            {
                if (MessageBox.Show("Deseja excluir este operador?", "Confirmação",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _opRepo.Delete(op.CodigoFJ);
                    ClearForm();
                    LoadOperators();
                }
            }
        }

        private void dgvOperators_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOperators.CurrentRow?.DataBoundItem is Operator op)
            {
                txtCodigoFJ.Text = op.CodigoFJ;
                txtCodigoFJ.ReadOnly = true; // evita alterar PK
                txtRomanji.Text = op.NameRomanji;
                txtNihongo.Text = op.NameNihongo;
                cmbShift.SelectedValue = op.ShiftId;
                cmbGroup.SelectedValue = op.GroupId;
                cmbSector.SelectedValue = op.SectorId;
                dtpStart.Value = op.StartDate;
                if (op.EndDate.HasValue)
                {
                    chkHasEnd.Checked = true;
                    dtpEnd.Value = op.EndDate.Value;
                }
                else
                {
                    chkHasEnd.Checked = false;
                }
                chkTrainer.Checked = op.Trainer;
                chkStatus.Checked = op.Status;
            }
        }

        private void ClearForm()
        {
            txtCodigoFJ.Clear();
            txtRomanji.Clear();
            txtNihongo.Clear();
            chkTrainer.Checked = false;
            chkStatus.Checked = true;
            chkHasEnd.Checked = false;
        }
    }
}
