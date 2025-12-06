// Project: TeamOps.UI
// File: Forms/FormFollowUp.cs
using System;
using System.Collections.Generic;
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
        // Remova SectorRepository se não for usar
        // private readonly SectorRepository _sectorRepo;

        private bool _isInitializing; // flag para evitar handler durante setup
        private List<Operator> _allOperators = new(); // cache para reutilizar

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
            // _sectorRepo = new SectorRepository(Program.ConnectionFactory);

            this.Load += FormFollowUp_Load;
        }

        private void FormFollowUp_Load(object? sender, EventArgs e)
        {
            // Carrega turnos
            cmbShift.DataSource = _shiftRepo.GetAll();
            cmbShift.DisplayMember = "NamePt";
            cmbShift.ValueMember = "Id";
            cmbShift.SelectedIndex = -1;

            // Carrega todos operadores (para inicialização)
            var operadores = _operatorRepo.GetAll();

            cmbOperator.DataSource = operadores.ToList();
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";
            cmbOperator.SelectedIndex = -1;

            cmbExecutor.DataSource = operadores.ToList();
            cmbExecutor.DisplayMember = "NameRomanji";
            cmbExecutor.ValueMember = "CodigoFJ";
            cmbExecutor.SelectedIndex = -1;

            cmbWitness.DataSource = operadores.ToList();
            cmbWitness.DisplayMember = "NameRomanji";
            cmbWitness.ValueMember = "CodigoFJ";
            cmbWitness.SelectedIndex = -1;

            // Carrega demais combos
            cmbReason.DataSource = _reasonRepo.GetAll();
            cmbReason.DisplayMember = "NamePt";
            cmbReason.ValueMember = "Id";
            cmbReason.SelectedIndex = -1;

            cmbType.DataSource = _typeRepo.GetAll();
            cmbType.DisplayMember = "NamePt";
            cmbType.ValueMember = "Id";
            cmbType.SelectedIndex = -1;

            cmbLocal.DataSource = _localRepo.GetAll();
            cmbLocal.DisplayMember = "NamePt";
            cmbLocal.ValueMember = "Id";
            cmbLocal.SelectedIndex = -1;

            cmbEquipment.DataSource = _equipmentRepo.GetAll();
            cmbEquipment.DisplayMember = "NamePt";
            cmbEquipment.ValueMember = "Id";
            cmbEquipment.SelectedIndex = -1;


            if (Program.CurrentUser != null && !string.IsNullOrEmpty(Program.CurrentUser.Login))
            {
                var codigoFJ = Program.CurrentUser.Login.ToUpper(); // normaliza
                var operador = _operatorRepo.GetByCodigoFJ(codigoFJ);

                if (operador != null)
                {
                    // Seleciona turno
                    cmbShift.SelectedValue = operador.ShiftId;

                    // Filtra operadores pelo turno
                    var operadoresDoTurno = _operatorRepo.GetByShift(operador.ShiftId);

                    cmbExecutor.DataSource = operadoresDoTurno;
                    cmbExecutor.DisplayMember = "NameRomanji";
                    cmbExecutor.ValueMember = "CodigoFJ";

                    // Seleciona executor pelo CodigoFJ
                    cmbExecutor.SelectedValue = codigoFJ;
                }
            }
        }


        private void cmbShift_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isInitializing) return;

            if (cmbShift.SelectedValue is int shiftId)
            {
                ApplyShiftFilter(shiftId);

                // Não zere a seleção aqui; deixe o usuário escolher ou mantenha atual
                // cmbOperator.SelectedIndex = -1;
                // cmbExecutor.SelectedIndex = -1;
                // cmbWitness.SelectedIndex = -1;
            }
        }

        // Centraliza o filtro de operadores por turno
        private void ApplyShiftFilter(int shiftId)
        {
            var operadores = _operatorRepo.GetByShift(shiftId);

            cmbOperator.DataSource = operadores;
            cmbOperator.DisplayMember = "NameRomanji";
            cmbOperator.ValueMember = "CodigoFJ";

            cmbExecutor.DataSource = operadores.ToList();
            cmbExecutor.DisplayMember = "NameRomanji";
            cmbExecutor.ValueMember = "CodigoFJ";

            cmbWitness.DataSource = operadores.ToList();
            cmbWitness.DisplayMember = "NameRomanji";
            cmbWitness.ValueMember = "CodigoFJ";
        }

        private void btnSalvar_Click(object? sender, EventArgs e)
        {
            var followUp = new FollowUp
            {
                Date = dtpDate.Value,
                ShiftId = Convert.ToInt32(cmbShift.SelectedValue ?? 0),
                OperatorCodigoFJ = cmbOperator.SelectedValue?.ToString() ?? string.Empty,
                ExecutorCodigoFJ = cmbExecutor.SelectedValue?.ToString() ?? string.Empty,
                WitnessCodigoFJ = cmbWitness.SelectedValue?.ToString(),
                ReasonId = Convert.ToInt32(cmbReason.SelectedValue ?? 0),
                TypeId = Convert.ToInt32(cmbType.SelectedValue ?? 0),
                LocalId = Convert.ToInt32(cmbLocal.SelectedValue ?? 0),
                EquipmentId = Convert.ToInt32(cmbEquipment.SelectedValue ?? 0),
                // SectorId removido se não usar
                Description = txtDescription.Text.Trim(),
                Guidance = txtGuidance.Text.Trim()
            };

            _followUpRepo.Add(followUp);
            MessageBox.Show("Acompanhamento salvo com sucesso.");
            this.Close();
        }
    }
}
