using System;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormFollowUp : Form
    {
        private readonly FollowUpRepository _followUpRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly OperatorRepository _operatorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;
        private readonly LocalRepository _localRepo;
        private readonly EquipmentRepository _equipmentRepo;
        private readonly SectorRepository _sectorRepo;

        private bool _isInitializing;

        public FormFollowUp()
        {
            InitializeComponent();

            _followUpRepo = new FollowUpRepository(Program.ConnectionFactory);
            _shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _operatorRepo = new OperatorRepository(Program.ConnectionFactory);
            _reasonRepo = new FollowUpReasonRepository(Program.ConnectionFactory);
            _typeRepo = new FollowUpTypeRepository(Program.ConnectionFactory);
            _localRepo = new LocalRepository(Program.ConnectionFactory);
            _equipmentRepo = new EquipmentRepository(Program.ConnectionFactory);
            _sectorRepo = new SectorRepository(Program.ConnectionFactory);

            Load += FormFollowUp_Load;
        }

        // ---------------------------------------------------------
        // LOAD
        // ---------------------------------------------------------
        private void FormFollowUp_Load(object? sender, EventArgs e)
        {
            _isInitializing = true;

            LoadCombos();
            ApplyUserContext();

            _isInitializing = false;

            // Aplica o filtro inicial (setor + turno)
            ApplyOperatorFilters();
        }

        // ---------------------------------------------------------
        // CARREGAR COMBOS
        // ---------------------------------------------------------
        private void LoadCombos()
        {
            // Turnos
            cmbShift.DataSource = _shiftRepo.GetAll();
            cmbShift.DisplayMember = "NamePt";
            cmbShift.ValueMember = "Id";
            cmbShift.SelectedIndex = -1;

            // Setores
            cmbSector.DataSource = _sectorRepo.GetAll();
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
            cmbSector.SelectedIndex = -1;

            // Operadores (carrega todos inicialmente)
            var ops = _operatorRepo.GetAll().Where(o => o.Status).ToList();

            cmbOperator.DataSource = ops.ToList();
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";
            cmbOperator.SelectedIndex = -1;

            // Executor NÃO é filtrado
            cmbExecutor.DataSource = ops.ToList();
            cmbExecutor.DisplayMember = "NameRomanji";
            cmbExecutor.ValueMember = "CodigoFJ";
            cmbExecutor.SelectedIndex = -1;

            // Testemunha (filtrada depois)
            cmbWitness.DataSource = ops.ToList();
            cmbWitness.DisplayMember = "NameRomanji";
            cmbWitness.ValueMember = "CodigoFJ";
            cmbWitness.SelectedIndex = -1;

            // Reason
            cmbReason.DataSource = _reasonRepo.GetAll();
            cmbReason.DisplayMember = "NamePt";
            cmbReason.ValueMember = "Id";
            cmbReason.SelectedIndex = -1;

            // Type
            cmbType.DataSource = _typeRepo.GetAll();
            cmbType.DisplayMember = "NamePt";
            cmbType.ValueMember = "Id";
            cmbType.SelectedIndex = -1;

            // Local
            cmbLocal.DataSource = _localRepo.GetAll();
            cmbLocal.DisplayMember = "NamePt";
            cmbLocal.ValueMember = "Id";
            cmbLocal.SelectedIndex = -1;

            // Equipment
            cmbEquipment.DataSource = _equipmentRepo.GetAll();
            cmbEquipment.DisplayMember = "NamePt";
            cmbEquipment.ValueMember = "Id";
            cmbEquipment.SelectedIndex = -1;
        }

        // ---------------------------------------------------------
        // CONTEXTO DO USUÁRIO LOGADO
        // ---------------------------------------------------------
        private void ApplyUserContext()
        {
            if (Program.CurrentUser == null || string.IsNullOrEmpty(Program.CurrentUser.Login))
                return;

            var codigoFJ = Program.CurrentUser.Login.ToUpper();
            var operador = _operatorRepo.GetByCodigoFJ(codigoFJ);

            if (operador == null)
                return;

            // Turno automático
            cmbShift.SelectedValue = operador.ShiftId;

            // Executor = usuário logado
            cmbExecutor.SelectedValue = codigoFJ;
        }

        // ---------------------------------------------------------
        // EVENTOS DE FILTRO
        // ---------------------------------------------------------
        private void cmbSector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            ApplyOperatorFilters();
        }

        private void cmbShift_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            ApplyOperatorFilters();
        }

        // ---------------------------------------------------------
        // FILTRAR OPERADORES POR SETOR + TURNO
        // ---------------------------------------------------------
        private void ApplyOperatorFilters()
        {
            if (cmbSector.SelectedValue is not int sectorId) return;
            if (cmbShift.SelectedValue is not int shiftId) return;

            var ops = _operatorRepo
                .GetAll()
                .Where(o => o.Status)
                .Where(o => o.SectorId == sectorId)
                .Where(o => o.ShiftId == shiftId)
                .ToList();

            // Operador
            cmbOperator.DataSource = ops.ToList();
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";
            cmbOperator.SelectedIndex = -1;

            // Testemunha
            cmbWitness.DataSource = ops.ToList();
            cmbWitness.DisplayMember = "NameRomanji";
            cmbWitness.ValueMember = "CodigoFJ";
            cmbWitness.SelectedIndex = -1;

            // Executor NÃO é filtrado
        }

        // ---------------------------------------------------------
        // SALVAR
        // ---------------------------------------------------------
        private void btnSalvar_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            var followUp = new FollowUp
            {
                Date = dtpDate.Value,
                ShiftId = (int)cmbShift.SelectedValue,
                OperatorCodigoFJ = cmbOperator.SelectedValue.ToString(),
                ExecutorCodigoFJ = cmbExecutor.SelectedValue.ToString(),
                WitnessCodigoFJ = cmbWitness.SelectedValue?.ToString(),
                ReasonId = (int)cmbReason.SelectedValue,
                TypeId = (int)cmbType.SelectedValue,
                LocalId = (int)cmbLocal.SelectedValue,
                EquipmentId = (int)cmbEquipment.SelectedValue,
                SectorId = (int)cmbSector.SelectedValue,
                Description = txtDescription.Text.Trim(),
                Guidance = txtGuidance.Text.Trim()
            };

            _followUpRepo.Add(followUp);

            MessageBox.Show("Acompanhamento salvo com sucesso.");
            Close();
        }

        // ---------------------------------------------------------
        // VALIDAÇÃO
        // ---------------------------------------------------------
        private bool ValidateForm()
        {
            if (cmbShift.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o turno.");
                return false;
            }

            if (cmbSector.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o setor.");
                return false;
            }

            if (cmbOperator.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o operador.");
                return false;
            }

            if (cmbExecutor.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o executor.");
                return false;
            }

            if (cmbReason.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o motivo.");
                return false;
            }

            if (cmbType.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o tipo.");
                return false;
            }

            if (cmbLocal.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o local.");
                return false;
            }

            if (cmbEquipment.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione o equipamento.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Digite a descrição.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtGuidance.Text))
            {
                MessageBox.Show("Digite a orientação.");
                return false;
            }

            return true;
        }
    }
}
