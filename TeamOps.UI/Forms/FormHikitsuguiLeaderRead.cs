using System;
using System.Drawing;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormHikitsuguiLeaderRead : Form
    {
        private readonly HikitsuguiRepository _hikitsuguiRepository;
        private readonly HikitsuguiReadRepository _readRepository;
        private readonly Operator _currentLeader;

        public FormHikitsuguiLeaderRead(
            HikitsuguiRepository hikitsuguiRepository,
            HikitsuguiReadRepository readRepository,
            Operator currentLeader)
        {
            InitializeComponent();

            _hikitsuguiRepository = hikitsuguiRepository;
            _readRepository = readRepository;
            _currentLeader = currentLeader;

            Load += FormHikitsuguiLeaderRead_Load;
            btnFiltrar.Click += btnFiltrar_Click;
            grid.CellClick += grid_CellClick;
        }

        private void FormHikitsuguiLeaderRead_Load(object? sender, EventArgs e)
        {
            dtFinal.Value = DateTime.Today;
            dtInicial.Value = DateTime.Today.AddDays(-31);

            ConfigurarGrid();
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

            grid.RowTemplate.Height = 60; // ou 80 se quiser mais alto
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            var colBtn = new DataGridViewButtonColumn();
            colBtn.Name = "colLeitura";
            colBtn.HeaderText = "Leitura";
            colBtn.Width = 60;
            grid.Columns.Add(colBtn);
        }

        private void btnFiltrar_Click(object? sender, EventArgs e)
        {
            CarregarLista();
        }

        private void CarregarLista()
        {
            var lista = _hikitsuguiRepository.GetForLeader(
                dtInicial.Value.Date,
                dtFinal.Value.Date.AddDays(1) // fim aberto
            );

            grid.Rows.Clear();

            foreach (var h in lista)
            {
                bool lido = _readRepository.HasRead(h.Id, _currentLeader.CodigoFJ);

                string preview;

                if (IsRtf(h.Description))
                {
                    preview = StripRtfRobusto(h.Description);
                }
                else
                {
                    preview = h.Description;
                }

                if (preview.Length > 120)
                    preview = preview.Substring(0, 120) + "...";

                int row = grid.Rows.Add(
                    h.Id,
                    h.Date.ToString("yyyy-MM-dd HH:mm"),
                    h.CategoryName,
                    h.CreatorCodigoFJ,
                    preview
                );

                var cell = (DataGridViewButtonCell)grid.Rows[row].Cells["colLeitura"];

                if (lido)
                {
                    cell.Value = "〇";
                    cell.Style.ForeColor = Color.Green;
                    cell.ReadOnly = true;
                }
                else
                {
                    cell.Value = "×";
                    cell.Style.ForeColor = Color.Red;
                    cell.ReadOnly = false;
                }
            }
        }
        private string StripRtf(string rtf)
        {
            try
            {
                using var rtb = new RichTextBox();
                rtb.Rtf = rtf;
                return rtb.Text;
            }
            catch
            {
                return rtf; // fallback
            }
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

        private void grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = grid.Columns[e.ColumnIndex].Name;

            if (columnName == "colLeitura")
            {
                int id = (int)grid.Rows[e.RowIndex].Cells["colId"].Value;

                if (!_readRepository.HasRead(id, _currentLeader.CodigoFJ))
                {
                    _readRepository.MarkAsRead(id, _currentLeader.CodigoFJ);

                    var cell = (DataGridViewButtonCell)grid.Rows[e.RowIndex].Cells["colLeitura"];
                    cell.Value = "〇";
                    cell.Style.ForeColor = Color.Green;
                    cell.ReadOnly = true;
                }
            }
            else if (columnName == "colDescricao")
            {
                int id = (int)grid.Rows[e.RowIndex].Cells["colId"].Value;
                var h = _hikitsuguiRepository.GetById(id);
                if (h is null) return;

                using var form = new FormHikitsuguiPreview(h.Description);
                form.ShowDialog();
            }
        }
        private bool IsRtf(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.TrimStart().StartsWith(@"{\rtf");
        }

    }
}
