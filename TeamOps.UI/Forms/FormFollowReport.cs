using System;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormFollowReport : Form
    {
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly SectorRepository _sectorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;
        private readonly EquipmentRepository _equipRepo;
        private readonly LocalRepository _localRepo;

        public FormFollowReport(
            FollowUpRepository followRepo,
            OperatorRepository opRepo,
            ShiftRepository shiftRepo,
            SectorRepository sectorRepo,
            FollowUpReasonRepository reasonRepo,
            FollowUpTypeRepository typeRepo,
            EquipmentRepository equipRepo,
            LocalRepository localRepo)
        {
            InitializeComponent();

            _followRepo = followRepo;
            _opRepo = opRepo;
            _shiftRepo = shiftRepo;
            _sectorRepo = sectorRepo;
            _reasonRepo = reasonRepo;
            _typeRepo = typeRepo;
            _equipRepo = equipRepo;
            _localRepo = localRepo;

            ConfigureGrid();
            LoadFilters();
        }

        // ---------------------------------------------------------
        // CONFIGURAÇÃO DO GRID
        // ---------------------------------------------------------
        private void ConfigureGrid()
        {
            dgvFollow.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvFollow.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvFollow.RowHeadersVisible = false;
            dgvFollow.ReadOnly = true;
            dgvFollow.AllowUserToAddRows = false;
            dgvFollow.AllowUserToDeleteRows = false;
            dgvFollow.CellDoubleClick += dgvFollow_CellDoubleClick;
        }

        // ---------------------------------------------------------
        // CARREGAR FILTROS
        // ---------------------------------------------------------
        private void LoadFilters()
        {
            // Período padrão: último mês
            dtpInicio.Value = DateTime.Now.AddMonths(-1);
            dtpFim.Value = DateTime.Now;

            // SHIFT
            var shifts = _shiftRepo.GetAll().ToList();
            shifts.Insert(0, new Shift { Id = 0, NamePt = "Todos" });
            cmbShift.DataSource = shifts;
            cmbShift.DisplayMember = "NamePt";
            cmbShift.ValueMember = "Id";

            // OPERADORES
            var ops = _opRepo.GetAll().Where(o => o.Status).ToList();
            ops.Insert(0, new Operator { CodigoFJ = "0", NameRomanji = "Todos" });
            cmbOperator.DataSource = ops;
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";

            // REASON
            var reasons = _reasonRepo.GetAll().ToList();
            reasons.Insert(0, new FollowUpReason { Id = 0, NamePt = "Todos" });
            cmbReason.DataSource = reasons;
            cmbReason.DisplayMember = "NamePt";
            cmbReason.ValueMember = "Id";

            // TYPE
            var types = _typeRepo.GetAll().ToList();
            types.Insert(0, new FollowUpType { Id = 0, NamePt = "Todos" });
            cmbType.DataSource = types;
            cmbType.DisplayMember = "NamePt";
            cmbType.ValueMember = "Id";

            // EQUIPMENT
            var equips = _equipRepo.GetAll().ToList();
            equips.Insert(0, new Equipment { Id = 0, NamePt = "Todos" });
            cmbEquipment.DataSource = equips;
            cmbEquipment.DisplayMember = "NamePt";
            cmbEquipment.ValueMember = "Id";

            // SECTOR
            var sectors = _sectorRepo.GetAll().ToList();
            sectors.Insert(0, new Sector { Id = 0, NamePt = "Todos" });
            cmbSector.DataSource = sectors;
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
        }

        // ---------------------------------------------------------
        // BOTÃO BUSCAR
        // ---------------------------------------------------------
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LoadGrid();
        }

        // ---------------------------------------------------------
        // CARREGAR GRID
        // ---------------------------------------------------------
        private void LoadGrid()
        {
            dgvFollow.Rows.Clear();
            dgvFollow.Columns.Clear();

            DateTime start = dtpInicio.Value.Date;
            DateTime end = dtpFim.Value.Date.AddDays(1);

            int shiftId = (int)cmbShift.SelectedValue;
            string opCodigo = cmbOperator.SelectedValue.ToString();
            int reasonId = (int)cmbReason.SelectedValue;
            int typeId = (int)cmbType.SelectedValue;
            int equipId = (int)cmbEquipment.SelectedValue;
            int sectorId = (int)cmbSector.SelectedValue;

            // Carrega FollowUps
            var list = _followRepo.GetByPeriod(start, end);

            // FILTROS
            if (shiftId != 0)
                list = list.Where(f => f.ShiftId == shiftId).ToList();

            if (opCodigo != "0")
                list = list.Where(f => f.OperatorCodigoFJ == opCodigo).ToList();

            if (reasonId != 0)
                list = list.Where(f => f.ReasonId == reasonId).ToList();

            if (typeId != 0)
                list = list.Where(f => f.TypeId == typeId).ToList();

            if (equipId != 0)
                list = list.Where(f => f.EquipmentId == equipId).ToList();

            if (sectorId != 0)
                list = list.Where(f => f.SectorId == sectorId).ToList();

            // ---------------------------------------------------------
            // COLUNAS DO GRID
            // ---------------------------------------------------------
            dgvFollow.Columns.Add("Id", "Id");
            dgvFollow.Columns["Id"].Visible = false;

            dgvFollow.Columns.Add("Date", "Data");
            dgvFollow.Columns.Add("Shift", "Turno");
            dgvFollow.Columns.Add("Operator", "Operador");
            dgvFollow.Columns.Add("Executor", "Executor");
            dgvFollow.Columns.Add("Witness", "Testemunha");
            dgvFollow.Columns.Add("Reason", "Motivo");
            dgvFollow.Columns.Add("Type", "Tipo");
            dgvFollow.Columns.Add("Local", "Local");
            dgvFollow.Columns.Add("Equipment", "Equipamento");
            dgvFollow.Columns.Add("Sector", "Setor");
            dgvFollow.Columns.Add("Description", "Descrição");
            dgvFollow.Columns.Add("Guidance", "Orientação");

            dgvFollow.Columns["Description"].Width = 250;
            dgvFollow.Columns["Guidance"].Width = 250;

            // ---------------------------------------------------------
            // LINHAS
            // ---------------------------------------------------------
            foreach (var f in list)
            {
                dgvFollow.Rows.Add(
                    f.Id,
                    f.Date.ToString("yyyy/MM/dd HH:mm"),
                    f.ShiftName,
                    f.OperatorName,
                    f.ExecutorName,
                    f.WitnessName,
                    f.ReasonName,
                    f.TypeName,
                    f.LocalName,
                    f.EquipmentName,
                    f.SectorName,
                    f.Description,
                    f.Guidance
                );
            }
        }
        // ---------------------------------------------------------
        // EXPORTAR
        // ---------------------------------------------------------
        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (dgvFollow.Rows.Count == 0)
            {
                MessageBox.Show("Nenhum dado para exportar.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = "FollowReport.xlsx"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("FollowReport");

            // Cabeçalhos
            for (int c = 0; c < dgvFollow.Columns.Count; c++)
                ws.Cell(1, c + 1).Value = dgvFollow.Columns[c].HeaderText;

            // Linhas
            for (int r = 0; r < dgvFollow.Rows.Count; r++)
            {
                for (int c = 0; c < dgvFollow.Columns.Count; c++)
                {
                    ws.Cell(r + 2, c + 1).Value =
                        dgvFollow.Rows[r].Cells[c].Value?.ToString() ?? "";
                }
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(sfd.FileName);

            MessageBox.Show("Arquivo XLSX exportado com sucesso.");
        }
        private void dgvFollow_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int followId = Convert.ToInt32(dgvFollow.Rows[e.RowIndex].Cells["Id"].Value);

            using var frm = new HTMLFormFollowSingleReport(
                followId,
                _followRepo,
                _opRepo
            );

            frm.ShowDialog();
        }
    }
}
