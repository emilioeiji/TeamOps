using System;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormHikitsuguiReader : Form
    {
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly SectorRepository _sectorRepo;
        private readonly OperatorRepository _operatorRepo;

        public FormHikitsuguiReader(
            HikitsuguiReadRepository readRepo,
            SectorRepository sectorRepo,
            OperatorRepository operatorRepo)
        {
            InitializeComponent();

            _readRepo = readRepo;
            _sectorRepo = sectorRepo;
            _operatorRepo = operatorRepo;

            LoadLookups();
        }

        private void LoadLookups()
        {
            cmbSetor.DataSource = _sectorRepo.GetAll();
            cmbSetor.DisplayMember = "NamePt";
            cmbSetor.ValueMember = "Id";

            cmbLider.DataSource = _operatorRepo.GetLeaders();
            cmbLider.DisplayMember = "NameRomanji";
            cmbLider.ValueMember = "CodigoFJ";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            // TODO: Buscar leituras no repositório
            // var lista = _readRepo.GetByFilters(...);
            // dgvLeituras.DataSource = lista;

            MessageBox.Show("Buscar leituras (filtro ainda não implementado).");
        }
    }
}
