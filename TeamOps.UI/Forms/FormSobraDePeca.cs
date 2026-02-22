using System;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormSobraDePeca : Form
    {
        private readonly SobraDePecaRepository _sobraRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly OperatorRepository _opRepo;
        private readonly ShainRepository _shainRepo;
        private readonly MachineRepository _equipRepo;

        private Operator? _operadorLogado;

        public FormSobraDePeca()
        {
            InitializeComponent();

            _sobraRepo = new SobraDePecaRepository(Program.ConnectionFactory);
            _shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _opRepo = new OperatorRepository(Program.ConnectionFactory);
            _shainRepo = new ShainRepository(Program.ConnectionFactory);
            _equipRepo = new MachineRepository(Program.ConnectionFactory);

            _operadorLogado = _opRepo.GetByCodigoFJ(Program.CurrentUser.Login);

            LoadLookups();
            LoadGrid();
            ConfigureEvents();
        }

        private void LoadLookups()
        {
            // Turnos
            cmbTurno.DataSource = _shiftRepo.GetAll();
            cmbTurno.DisplayMember = "NamePt";
            cmbTurno.ValueMember = "Id";

            if (_operadorLogado != null)
            {
                cmbTurno.SelectedValue = _operadorLogado.ShiftId;
                txtLider.Text = _operadorLogado.NameRomanji;
            }

            cmbTurno.Enabled = false;
            txtLider.ReadOnly = true;

            // Operadores do turno
            LoadOperadoresByTurno();

            // Equipamentos
            cmbMaquina.DataSource = _equipRepo.GetAll();
            cmbMaquina.DisplayMember = "NamePt";
            cmbMaquina.ValueMember = "Id";
            cmbMaquina.SelectedIndex = -1;

            // Shain
            cmbShain.DataSource = _shainRepo.GetAll();
            cmbShain.DisplayMember = "NameRomanji";
            cmbShain.ValueMember = "Id";
            cmbShain.SelectedIndex = -1;
        }

        private void LoadOperadoresByTurno()
        {
            if (cmbTurno.SelectedValue is int turnoId)
            {
                var list = _opRepo.GetByShift(turnoId);
                cmbOperador.DataSource = list;
                cmbOperador.DisplayMember = "NameRomanji";
                cmbOperador.ValueMember = "CodigoFJ";
                cmbOperador.SelectedIndex = -1;
            }
        }

        private void ConfigureEvents()
        {
            txtPeso.TextChanged += (s, e) => CalculateQuantidade();
            txtTanjuu.TextChanged += (s, e) => CalculateQuantidade();
        }

        private void CalculateQuantidade()
        {
            var pesoText = txtPeso.Text.Replace(".", ",");
            var tanjuuText = txtTanjuu.Text.Replace(".", ",");

            if (decimal.TryParse(pesoText, out var peso) &&
                decimal.TryParse(tanjuuText, out var tanjuu) &&
                tanjuu > 0)
            {
                var qtd = peso / tanjuu;
                txtQuantidade.Text = Math.Round(qtd).ToString();
            }
            else
            {
                txtQuantidade.Text = "";
            }
        }

        private void LoadGrid()
        {
            var list = _sobraRepo.GetAll();

            // Converte IDs para nomes
            var view = list
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
            {
                x.Id,
                x.Data,
                Turno = _shiftRepo.GetById(x.TurnoId)?.NamePt ?? "",
                x.Lote,
                Operador = _opRepo.GetAll()
                    .FirstOrDefault(o => o.CodigoFJ == x.OperadorId)
                    ?.NameRomanji ?? "",
                x.Tanjuu,
                x.PesoGramas,
                x.Quantidade,
                Maquina = _equipRepo.GetById(x.MachineId)?.NamePt ?? "",
                Shain = _shainRepo.GetById(x.ShainId)?.NameRomanji ?? "",
                x.Observacao,
                x.Lider,
                x.CreatedAt
            }).ToList();

            dgvSobra.DataSource = view;

            if (dgvSobra.Columns.Count > 0)
            {
                dgvSobra.Columns["Id"].Width = 50;
                dgvSobra.Columns["Data"].Width = 90;
                dgvSobra.Columns["Turno"].Width = 90;
                dgvSobra.Columns["Lote"].Width = 100;
                dgvSobra.Columns["Operador"].Width = 120;
                dgvSobra.Columns["Tanjuu"].Width = 80;
                dgvSobra.Columns["PesoGramas"].Width = 90;
                dgvSobra.Columns["Quantidade"].Width = 90;
                dgvSobra.Columns["Maquina"].Width = 120;
                dgvSobra.Columns["Shain"].Width = 120;
                dgvSobra.Columns["Observacao"].Width = 200;
                dgvSobra.Columns["Lider"].Width = 120;
                dgvSobra.Columns["CreatedAt"].Width = 120;
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            var sobra = new SobraDePeca
            {
                Data = dtpData.Value,
                TurnoId = (int)cmbTurno.SelectedValue,
                Lote = txtLote.Text.Trim(),
                OperadorId = cmbOperador.SelectedValue.ToString()!,
                Tanjuu = decimal.Parse(txtTanjuu.Text),
                PesoGramas = decimal.Parse(txtPeso.Text),
                Quantidade = decimal.Parse(txtQuantidade.Text),
                MachineId = (int)cmbMaquina.SelectedValue,
                ShainId = (int)cmbShain.SelectedValue,
                Observacao = string.IsNullOrWhiteSpace(txtObservacao.Text)
                                ? null
                                : txtObservacao.Text.Trim(),
                Lider = _operadorLogado?.NameRomanji ?? "N/A",
                CreatedAt = DateTime.Now
            };

            _sobraRepo.Add(sobra);

            ClearForm();
            LoadGrid();
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtLote.Text))
            {
                MessageBox.Show("Informe o lote.");
                return false;
            }

            if (!decimal.TryParse(txtTanjuu.Text, out _))
            {
                MessageBox.Show("Informe um tanjuu válido.");
                return false;
            }

            if (!decimal.TryParse(txtPeso.Text, out _))
            {
                MessageBox.Show("Informe um peso válido.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantidade.Text))
            {
                MessageBox.Show("Quantidade inválida.");
                return false;
            }

            if (cmbOperador.SelectedItem == null)
            {
                MessageBox.Show("Selecione um operador.");
                return false;
            }

            if (cmbMaquina.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma máquina.");
                return false;
            }

            if (cmbShain.SelectedItem == null)
            {
                MessageBox.Show("Selecione um shain.");
                return false;
            }

            return true;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            dtpData.Value = DateTime.Now;
            txtLote.Clear();
            txtTanjuu.Clear();
            txtPeso.Clear();
            txtQuantidade.Clear();
            txtObservacao.Clear();

            if (cmbMaquina.Items.Count > 0)
                cmbMaquina.SelectedIndex = -1;

            if (cmbShain.Items.Count > 0)
                cmbShain.SelectedIndex = -1;

            if (cmbOperador.Items.Count > 0)
                cmbOperador.SelectedIndex = -1;
        }
    }
}
