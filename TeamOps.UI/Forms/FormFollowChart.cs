using System;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using System.Windows.Forms.DataVisualization.Charting;

namespace TeamOps.UI.Forms
{
    public partial class FormFollowChart : Form
    {
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly SectorRepository _sectorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;
        private readonly EquipmentRepository _equipRepo;
        private readonly LocalRepository _localRepo;

        public FormFollowChart(
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

            LoadFilters();
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
            shifts.Insert(0, new Core.Entities.Shift { Id = 0, NamePt = "Todos" });
            cmbShift.DataSource = shifts;
            cmbShift.DisplayMember = "NamePt";
            cmbShift.ValueMember = "Id";

            // OPERADORES
            var ops = _opRepo.GetAll().Where(o => o.Status).ToList();
            ops.Insert(0, new Core.Entities.Operator { CodigoFJ = "0", NameRomanji = "Todos" });
            cmbOperator.DataSource = ops;
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";

            // REASON
            var reasons = _reasonRepo.GetAll().ToList();
            reasons.Insert(0, new Core.Entities.FollowUpReason { Id = 0, NamePt = "Todos" });
            cmbReason.DataSource = reasons;
            cmbReason.DisplayMember = "NamePt";
            cmbReason.ValueMember = "Id";

            // TYPE
            var types = _typeRepo.GetAll().ToList();
            types.Insert(0, new Core.Entities.FollowUpType { Id = 0, NamePt = "Todos" });
            cmbType.DataSource = types;
            cmbType.DisplayMember = "NamePt";
            cmbType.ValueMember = "Id";

            // EQUIPMENT
            var equips = _equipRepo.GetAll().ToList();
            equips.Insert(0, new Core.Entities.Equipment { Id = 0, NamePt = "Todos" });
            cmbEquipment.DataSource = equips;
            cmbEquipment.DisplayMember = "NamePt";
            cmbEquipment.ValueMember = "Id";

            // SECTOR
            var sectors = _sectorRepo.GetAll().ToList();
            sectors.Insert(0, new Core.Entities.Sector { Id = 0, NamePt = "Todos" });
            cmbSector.DataSource = sectors;
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
        }

        // ---------------------------------------------------------
        // BOTÃO BUSCAR
        // ---------------------------------------------------------
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LoadCharts();
        }

        // ---------------------------------------------------------
        // CARREGAR GRÁFICOS
        // ---------------------------------------------------------
        private void LoadCharts()
        {
            DateTime start = dtpInicio.Value.Date;
            DateTime end = dtpFim.Value.Date.AddDays(1);

            int shiftId = (int)cmbShift.SelectedValue;
            string opCodigo = cmbOperator.SelectedValue.ToString();
            int reasonId = (int)cmbReason.SelectedValue;
            int typeId = (int)cmbType.SelectedValue;
            int equipId = (int)cmbEquipment.SelectedValue;
            int sectorId = (int)cmbSector.SelectedValue;

            var list = _followRepo.GetByPeriod(start, end);

            // FILTROS (exceto turno para o gráfico de turno)
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
            // GRÁFICO POR TURNO (NÃO FILTRA TURNO)
            // ---------------------------------------------------------
            var listTurno = _followRepo.GetByPeriod(start, end); // sem filtro de turno

            var turnoGroup = listTurno
                .GroupBy(f => f.ShiftName)
                .Select(g => new { Turno = g.Key, Count = g.Count() })
                .ToList();

            chartTurno.Series[0].Points.Clear();

            foreach (var item in turnoGroup)
            {
                var point = new DataPoint();
                point.AxisLabel = item.Turno;                 // Nome da categoria
                point.YValues = new double[] { item.Count };  // Valor
                point.Label = $"{item.Turno}: {item.Count}";  // Texto dentro da fatia

                chartTurno.Series[0].Points.Add(point);
            }

            chartTurno.Series[0].IsValueShownAsLabel = true;
            chartTurno.Series[0].LegendText = "#VALX";

            chartTurno.Titles[0].Text = $"Distribuição por Turno ({turnoGroup.Sum(x => x.Count)})";

            // ---------------------------------------------------------
            // GRÁFICO POR TIPO
            // ---------------------------------------------------------
            var tipoGroup = list
                .GroupBy(f => f.TypeName)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToList();

            chartTipo.Series[0].Points.Clear();

            foreach (var item in tipoGroup)
            {
                var point = new DataPoint();
                point.AxisLabel = item.Tipo;
                point.YValues = new double[] { item.Count };
                point.Label = $"{item.Tipo}: {item.Count}";

                chartTipo.Series[0].Points.Add(point);
            }

            chartTipo.Series[0].IsValueShownAsLabel = true;
            chartTipo.Series[0].LegendText = "#VALX";

            chartTipo.Titles[0].Text = $"Distribuição por Tipo ({tipoGroup.Sum(x => x.Count)})";

            // ---------------------------------------------------------
            // GRÁFICO POR MOTIVO
            // ---------------------------------------------------------
            var motivoGroup = list
                .GroupBy(f => f.ReasonName)
                .Select(g => new { Motivo = g.Key, Count = g.Count() })
                .ToList();

            chartMotivo.Series[0].Points.Clear();

            foreach (var item in motivoGroup)
            {
                var point = new DataPoint();
                point.AxisLabel = item.Motivo;
                point.YValues = new double[] { item.Count };
                point.Label = $"{item.Motivo}: {item.Count}";

                chartMotivo.Series[0].Points.Add(point);
            }

            chartMotivo.Series[0].IsValueShownAsLabel = true;
            chartMotivo.Series[0].LegendText = "#VALX";

            chartMotivo.Titles[0].Text = $"Distribuição por Motivo ({motivoGroup.Sum(x => x.Count)})";

        }
    }
}
