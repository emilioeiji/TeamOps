using System;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormReports : Form
    {
        private readonly Operator _currentOperator;
        private readonly Shift _currentShift;
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorRepository _opRepo;
        private readonly SqliteConnectionFactory _factory;

        public FormReports(
            Operator currentOperator,
            Shift currentShift,
            HikitsuguiRepository hikRepo,
            HikitsuguiReadRepository readRepo,
            OperatorRepository opRepo,
            SqliteConnectionFactory factory)
        {
            InitializeComponent();

            _currentOperator = currentOperator;
            _currentShift = currentShift;
            _hikRepo = hikRepo;
            _readRepo = readRepo;
            _opRepo = opRepo;
            _factory = factory;
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
            var shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            var sectorRepo = new SectorRepository(Program.ConnectionFactory);

            using var form = new FormHikitsuguiReader(
                _hikRepo,
                _readRepo,
                _opRepo,
                shiftRepo,
                sectorRepo
            );

            form.ShowDialog();
        }

        private void btnRepSobra_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Abrir relatório de Sobra de Peça...");
            // TODO: FormReportSobra
        }

        private void btnRepFollowReport_Click(object sender, EventArgs e)
        {
            var followRepo = new FollowUpRepository(Program.ConnectionFactory);
            var opRepo = new OperatorRepository(Program.ConnectionFactory);
            var shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            var sectorRepo = new SectorRepository(Program.ConnectionFactory);
            var reasonRepo = new FollowUpReasonRepository(Program.ConnectionFactory);
            var typeRepo = new FollowUpTypeRepository(Program.ConnectionFactory);
            var equipRepo = new EquipmentRepository(Program.ConnectionFactory);
            var localRepo = new LocalRepository(Program.ConnectionFactory);

            using var form = new FormFollowReport(
                followRepo,
                opRepo,
                shiftRepo,
                sectorRepo,
                reasonRepo,
                typeRepo,
                equipRepo,
                localRepo
            );

            form.ShowDialog();
        }

        private void btnRepFollowChart_Click(object sender, EventArgs e)
        {
            using var form = new FormFollowChart();
            form.ShowDialog();
        }
    }
}
