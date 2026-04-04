// Project: TeamOps.UI
// File: Forms/FormDashboard.cs

using System;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using AppUser = TeamOps.Core.Entities.User;

namespace TeamOps.UI.Forms
{
    public partial class FormDashboard : Form
    {
        private readonly AppUser _user;
        private readonly Operator _currentOperator;
        private readonly Shift _currentShift;

        private readonly HikitsuguiRepository _hikitsuguiRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly EquipmentRepository _equipmentRepository;
        private readonly LocalRepository _localRepository;
        private readonly SectorRepository _sectorRepository;
        private readonly PRRepository _prRepo;
        private readonly PRCategoriaRepository _prCategoriaRepo;
        private readonly PRPrioridadeRepository _prPrioridadeRepo;
        private readonly CLRepository _clRepo;
        private readonly CLCategoriaRepository _clCategoriaRepo;
        private readonly CLPrioridadeRepository _clPrioridadeRepo;
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorRepository _opRepo;
        private readonly SqliteConnectionFactory _factory;

        public FormDashboard(AppUser user)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _user = user;

            // Carrega Operator a partir do CodigoFJ do User
            var opRepo = new OperatorRepository(Program.ConnectionFactory);
            _currentOperator = opRepo.GetByCodigoFJ(_user.CodigoFJ!)
                               ?? throw new InvalidOperationException("Operador não encontrado para o usuário atual.");

            // Carrega Shift a partir do ShiftId do Operator
            var shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _currentShift = shiftRepo.GetById(_currentOperator.ShiftId)
                            ?? throw new InvalidOperationException("Turno não encontrado para o operador atual.");

            // Instancia repositórios usados pelo Hikitsugui
            _hikitsuguiRepository = new HikitsuguiRepository(Program.ConnectionFactory);
            _categoryRepository = new CategoryRepository(Program.ConnectionFactory);
            _equipmentRepository = new EquipmentRepository(Program.ConnectionFactory);
            _localRepository = new LocalRepository(Program.ConnectionFactory);
            _sectorRepository = new SectorRepository(Program.ConnectionFactory);
            _prRepo = new PRRepository(Program.ConnectionFactory);
            _prCategoriaRepo = new PRCategoriaRepository(Program.ConnectionFactory);
            _prPrioridadeRepo = new PRPrioridadeRepository(Program.ConnectionFactory);
            _clRepo = new CLRepository(Program.ConnectionFactory);
            _clCategoriaRepo = new CLCategoriaRepository(Program.ConnectionFactory);
            _clPrioridadeRepo = new CLPrioridadeRepository(Program.ConnectionFactory);
            _hikRepo = new HikitsuguiRepository(Program.ConnectionFactory);
            _readRepo = new HikitsuguiReadRepository(Program.ConnectionFactory);
            _opRepo = new OperatorRepository(Program.ConnectionFactory);

            lblUser.Text = $"Bem-vindo, {_user.Name}";
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        // ---------------------------------------------------------
        // PERMISSÃO
        // ---------------------------------------------------------
        private bool HasAccess(AccessLevel requiredLevel)
        {
            return _user.AccessLevel >= requiredLevel;
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show(
                "Acesso negado. Permissão insuficiente.",
                "Acesso Negado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        // ---------------------------------------------------------
        // BOTÕES DO DASHBOARD
        // ---------------------------------------------------------

        private void btnOperadores_Click(object sender, EventArgs e)
        {
            var form = new FormOperators();
            form.ShowDialog();
        }

        private void btnPresenca_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.GL))
            {
                ShowAccessDenied();
                return;
            }

            // Defina o setor deste botão
            int sectorId = 1; // G-Bareru, por exemplo
            string sectorName = "G-Bareru";

            var form = new FormPresenceLayout(
                sectorId,
                sectorName,
                Program.ConnectionFactory
            );

            form.ShowDialog();
        }

        private void btnRelatorios_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.GL))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormReports(
                _currentOperator,
                _currentShift,
                _hikRepo,
                _readRepo,
                _opRepo,
                _factory
            );

            form.ShowDialog();
        }

        private void btnFollowUp_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormFollowUp();
            form.ShowDialog();
        }
        private void btnPR_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormPR(
                _prRepo,
                _prCategoriaRepo,
                _prPrioridadeRepo,
                _sectorRepository,
                new OperatorRepository(Program.ConnectionFactory),
                _currentOperator
            );

            form.ShowDialog();
        }

        private void btnCL_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormCL(
                _clRepo,
                _clCategoriaRepo,
                _clPrioridadeRepo,
                _sectorRepository,
                new OperatorRepository(Program.ConnectionFactory),
                _currentOperator
            );

            form.ShowDialog();
        }

        private void btnYukyu_Click(object sender, EventArgs e)
        {
            using var form = new FormPaidLeaveTracking(
                _currentOperator,
                _currentShift,
                Program.ConnectionFactory
            );

            form.ShowDialog();
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.Admin))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormAdmin();
            form.ShowDialog();
        }

        private void btnAccessControl_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.Admin))
            {
                ShowAccessDenied();
                return;
            }

            var form = new FormAccessControl();
            form.ShowDialog();
        }
        private void btnSobraDePeca_Click(object sender, EventArgs e)
        {
            // Permissão mínima: KL (líder)
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            using var form = new FormSobraDePeca();
            form.ShowDialog();
        }
        // ---------------------------------------------------------
        // HIKITSUGUI (será ativado quando você adicionar o botão)
        // ---------------------------------------------------------
        private void btnHikitsugui_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            using var form = new FormHikitsugui(
                _currentShift,
                _currentOperator,
                _hikitsuguiRepository,
                _categoryRepository,
                _equipmentRepository,
                _localRepository,
                _sectorRepository
            );

            form.ShowDialog();
        }
        private void btnHikitsuguiLeaderRead_Click(object sender, EventArgs e)
        {
            if (!HasAccess(AccessLevel.KL))
            {
                ShowAccessDenied();
                return;
            }

            var readRepo = new HikitsuguiReadRepository(Program.ConnectionFactory);

            using var form = new FormHikitsuguiLeaderRead(
                _hikitsuguiRepository,
                readRepo,
                _user,
                _currentOperator
            );

            form.ShowDialog();
        }

    }
}
