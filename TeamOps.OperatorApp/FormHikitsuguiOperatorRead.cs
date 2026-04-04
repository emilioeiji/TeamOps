using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.OperatorApp
{
    public partial class FormHikitsuguiOperatorRead : Form
    {
        private readonly OperatorRepository _operatorRepo;
        private readonly LocalRepository _localRepo;
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorPresenceRepository _presenceRepo;

        private Operator? _currentOperator;

        public FormHikitsuguiOperatorRead()
        {
            InitializeComponent();

            _operatorRepo = new OperatorRepository(Program.ConnectionFactory);
            _localRepo = new LocalRepository(Program.ConnectionFactory);
            _hikRepo = new HikitsuguiRepository(Program.ConnectionFactory);
            _readRepo = new HikitsuguiReadRepository(Program.ConnectionFactory);
            _presenceRepo = new OperatorPresenceRepository(Program.ConnectionFactory);

            Load += Form_Load;
            txtFJ.TextChanged += txtFJ_TextChanged;
            btnFiltrar.Click += btnFiltrar_Click;
            grid.CellClick += grid_CellClick;

            cboLocal.SelectionChangeCommitted += cboLocal_SelectionChangeCommitted;
        }

        private void Form_Load(object? sender, EventArgs e)
        {
            dtFinal.Value = DateTime.Today;
            dtInicial.Value = DateTime.Today.AddDays(-31);

            ConfigurarGrid();

            cboLocal.DataSource = null;
            cboLocal.DisplayMember = "NamePt";
            cboLocal.ValueMember = "Id";
        }

        private void txtFJ_TextChanged(object? sender, EventArgs e)
        {
            if (txtFJ.Text.Length < 4)
                return;

            var codigo = txtFJ.Text.Trim().ToUpper();

            _currentOperator = _operatorRepo.GetByCodigoFJ(codigo);

            if (_currentOperator == null)
            {
                lblNome.Text = "";
                cboLocal.DataSource = null;
                grid.Rows.Clear();
                return;
            }

            lblNome.Text = _currentOperator.NameRomanji;

            int sectorId = _currentOperator.SectorId;

            cboLocal.DataSource = _localRepo.GetBySector(sectorId);
            cboLocal.DisplayMember = "NamePt";
            cboLocal.ValueMember = "Id";

            cboLocal.SelectedIndex = -1;
            grid.Rows.Clear();
        }

        private void cboLocal_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_currentOperator == null)
                return;

            if (cboLocal.SelectedIndex < 0)
                return;

            var local = (Local)cboLocal.SelectedItem;

            _presenceRepo.RegisterPresence(
                _currentOperator.CodigoFJ,
                local.SectorId,
                local.Id,
                _currentOperator.ShiftId,
                DateTime.Now
            );

            CarregarLista();
        }

        private void btnFiltrar_Click(object? sender, EventArgs e)
        {
            if (_currentOperator == null)
            {
                MessageBox.Show("Digite um FJ válido.", "Aviso");
                return;
            }

            CarregarLista();
        }

        private void ConfigurarGrid()
        {
            grid.Columns.Clear();

            grid.Columns.Add("colId", "ID");
            grid.Columns["colId"].Visible = false;

            grid.Columns.Add("colData", "Data");
            grid.Columns.Add("colCategoria", "Categoria");
            grid.Columns.Add("colCriador", "Criador");
            grid.Columns.Add("colDescricao", "Descrição");

            grid.RowTemplate.Height = 60;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersDefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.LightGray;
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            var colBtn = new DataGridViewTextBoxColumn();
            colBtn.Name = "colLeitura";
            colBtn.HeaderText = "Leitura";
            grid.Columns.Add(colBtn);

            grid.Columns["colLeitura"].DefaultCellStyle.SelectionForeColor = Color.Green;
            grid.Columns["colLeitura"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["colLeitura"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 🔥 Ajuste de largura inteligente
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.Columns["colDescricao"].FillWeight = 300;
            grid.Columns["colData"].FillWeight = 80;
            grid.Columns["colCategoria"].FillWeight = 100;
            grid.Columns["colCriador"].FillWeight = 120;
            grid.Columns["colLeitura"].FillWeight = 60;
        }

        private void CarregarLista()
        {
            if (_currentOperator == null)
                return;

            int sectorId = _currentOperator.SectorId;

            var lista = _hikRepo.GetForOperator(
                dtInicial.Value.Date,
                dtFinal.Value.Date.AddDays(1),
                _currentOperator.SectorId,
                (int)cboLocal.SelectedValue
            );


            grid.Rows.Clear();

            foreach (var h in lista)
            {
                bool lido = _readRepo.HasRead(h.Id, _currentOperator.CodigoFJ);

                string preview;

                if (IsRtf(h.Description))
                    preview = StripRtfRobusto(h.Description);
                else
                    preview = h.Description;

                if (preview.Length > 120)
                    preview = preview.Substring(0, 120) + "...";

                int row = grid.Rows.Add(
                    h.Id,
                    h.Date.ToString("yyyy-MM-dd HH:mm"),
                    h.CategoryName,
                    h.CreatorCodigoFJ,
                    preview
                );

                var cell = grid.Rows[row].Cells["colLeitura"];

                if (lido)
                {
                    cell.Value = "〇";
                    cell.Style.ForeColor = Color.Green;
                    cell.Style.SelectionForeColor = Color.Green;
                    cell.Style.Font = new Font("Segoe UI", 20, FontStyle.Bold);
                }
                else
                {
                    cell.Value = "×";
                    cell.Style.ForeColor = Color.Red;
                    cell.Style.SelectionForeColor = Color.Red;
                    cell.Style.Font = new Font("Segoe UI", 20, FontStyle.Bold);
                }
            }
        }

        private void CarregarHikitsugui()
        {
            CarregarLista();
        }

        private void grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = grid.Columns[e.ColumnIndex].Name;

            if (columnName == "colLeitura")
            {
                int id = (int)grid.Rows[e.RowIndex].Cells["colId"].Value;

                if (!_readRepo.HasRead(id, _currentOperator.CodigoFJ))
                {
                    _readRepo.MarkAsRead(id, _currentOperator.CodigoFJ);

                    var cell = grid.Rows[e.RowIndex].Cells["colLeitura"];
                    cell.Value = "〇";
                    cell.Style.ForeColor = Color.Green;
                    cell.Style.SelectionForeColor = Color.Green;
                    cell.Style.Font = new Font("Segoe UI", 20, FontStyle.Bold);
                }
            }
            else if (columnName == "colDescricao")
            {
                int id = (int)grid.Rows[e.RowIndex].Cells["colId"].Value;
                var h = _hikRepo.GetById(id);
                if (h is null) return;

                using var form = new FormHikitsuguiPreview(h);
                form.ShowDialog();
            }
        }
       
        private bool IsRtf(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.TrimStart().StartsWith(@"{\rtf");
        }
        private string StripRtfRobusto(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    return "";

                // Se não for RTF válido, retorna texto puro
                if (!input.TrimStart().StartsWith(@"{\rtf"))
                    return input;

                using var rtb = new RichTextBox();
                rtb.Rtf = input;
                return rtb.Text;
            }
            catch
            {
                // Se der erro, remove tags básicas
                return input
                    .Replace("{", "")
                    .Replace("}", "")
                    .Replace("\\par", " ")
                    .Replace("\\b", "")
                    .Replace("\\i", "")
                    .Replace("\\ul", "")
                    .Replace("\\fs20", "")
                    .Replace("\\f0", "")
                    .Replace("\\f1", "")
                    .Replace("\\f2", "")
                    .Replace("\\f3", "");
            }
        }
    }
}
