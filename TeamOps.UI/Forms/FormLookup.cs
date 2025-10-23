// Project: TeamOps.UI
// File: Forms/FormLookup.cs
using System;
using System.Windows.Forms;

namespace TeamOps.UI.Forms
{
    public partial class FormLookup<T> : Form where T : class, new()
    {
        private readonly dynamic _repo; // pode ser ShiftRepository, GroupRepository ou SectorRepository
        private readonly string _entityName;

        public FormLookup(dynamic repo, string entityName)
        {
            InitializeComponent();
            _repo = repo;
            _entityName = entityName;
            this.Text = $"Manage {_entityName}";
            LoadData();
        }

        private void LoadData()
        {
            dgvLookup.DataSource = _repo.GetAll();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamePt.Text) || string.IsNullOrWhiteSpace(txtNameJp.Text))
            {
                MessageBox.Show("Preencha os dois campos.");
                return;
            }

            var entity = new T();
            entity.GetType().GetProperty("NamePt")?.SetValue(entity, txtNamePt.Text.Trim());
            entity.GetType().GetProperty("NameJp")?.SetValue(entity, txtNameJp.Text.Trim());

            _repo.Add(entity);
            ClearForm();
            LoadData();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvLookup.CurrentRow?.DataBoundItem is T entity)
            {
                entity.GetType().GetProperty("NamePt")?.SetValue(entity, txtNamePt.Text.Trim());
                entity.GetType().GetProperty("NameJp")?.SetValue(entity, txtNameJp.Text.Trim());

                _repo.Update(entity);
                ClearForm();
                LoadData();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvLookup.CurrentRow?.DataBoundItem is T entity)
            {
                int id = (int)entity.GetType().GetProperty("Id")!.GetValue(entity)!;
                if (MessageBox.Show("Deseja excluir este registro?", "Confirmação",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _repo.Delete(id);
                    ClearForm();
                    LoadData();
                }
            }
        }

        private void dgvLookup_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLookup.CurrentRow?.DataBoundItem is T entity)
            {
                txtNamePt.Text = entity.GetType().GetProperty("NamePt")?.GetValue(entity)?.ToString();
                txtNameJp.Text = entity.GetType().GetProperty("NameJp")?.GetValue(entity)?.ToString();
            }
        }

        private void ClearForm()
        {
            txtNamePt.Clear();
            txtNameJp.Clear();
        }
    }
}
