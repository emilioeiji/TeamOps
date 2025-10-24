using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormLocals : Form
    {
        private readonly LocalRepository _localRepo;
        private readonly SectorRepository _sectorRepo;

        public FormLocals()
        {
            InitializeComponent();
            _localRepo = new LocalRepository(Program.ConnectionFactory);
            _sectorRepo = new SectorRepository(Program.ConnectionFactory);

            LoadLookups();
            LoadLocals();
        }

        private void LoadLookups()
        {
            cmbSector.DataSource = _sectorRepo.GetAll();
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
        }

        private void LoadLocals()
        {
            dgvLocals.DataSource = _localRepo.GetAll();
        }

        private void dgvLocals_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLocals.CurrentRow?.DataBoundItem is Local local)
            {
                txtNamePt.Text = local.NamePt;
                txtNameJp.Text = local.NameJp;
                cmbSector.SelectedValue = local.SectorId;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbSector.SelectedValue == null || Convert.ToInt32(cmbSector.SelectedValue) == 0)
            {
                MessageBox.Show("Selecione um setor válido.");
                return;
            }

            var local = new Local
            {
                NamePt = txtNamePt.Text.Trim(),
                NameJp = txtNameJp.Text.Trim(),
                SectorId = Convert.ToInt32(cmbSector.SelectedValue)
            };

            _localRepo.Add(local);
            LoadLocals();
            ClearForm();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvLocals.CurrentRow?.DataBoundItem is Local local)
            {
                // Validação: precisa ter setor selecionado
                if (cmbSector.SelectedValue == null || Convert.ToInt32(cmbSector.SelectedValue) == 0)
                {
                    MessageBox.Show("Selecione um setor válido antes de atualizar.",
                                    "Validação",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                local.NamePt = txtNamePt.Text.Trim();
                local.NameJp = txtNameJp.Text.Trim();
                local.SectorId = Convert.ToInt32(cmbSector.SelectedValue);

                _localRepo.Update(local);
                LoadLocals();
                ClearForm();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvLocals.CurrentRow?.DataBoundItem is Local local)
            {
                if (MessageBox.Show("Deseja excluir este Local?", "Confirmação",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _localRepo.Delete(local.Id);
                    LoadLocals();
                    ClearForm();
                }
            }
        }

        private void ClearForm()
        {
            txtNamePt.Clear();
            txtNameJp.Clear();
            cmbSector.SelectedIndex = -1;
        }
    }
}
