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

                int row = grid.Rows.Add(
                    h.Id,
                    h.Date.ToString("yyyy-MM-dd HH:mm"),
                    h.CategoryName,
                    h.CreatorCodigoFJ,
                    h.Description.Length > 80 ? h.Description.Substring(0, 80) + "..." : h.Description
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

        private void grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (grid.Columns[e.ColumnIndex].Name == "colLeitura")
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
        }
    }
}
