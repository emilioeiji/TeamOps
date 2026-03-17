using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormHikitsuguiReader : Form
    {
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly SectorRepository _sectorRepo;

        public FormHikitsuguiReader(
            HikitsuguiRepository hikRepo,
            HikitsuguiReadRepository readRepo,
            OperatorRepository opRepo,
            ShiftRepository shiftRepo,
            SectorRepository sectorRepo)
        {
            InitializeComponent();

            _hikRepo = hikRepo;
            _readRepo = readRepo;
            _opRepo = opRepo;
            _shiftRepo = shiftRepo;
            _sectorRepo = sectorRepo;

            rbOperadores.Checked = true;

            dtpInicio.Value = DateTime.Now.AddMonths(-1);
            dtpFim.Value = DateTime.Now;

            // Carrega turnos reais do banco
            var shifts = _shiftRepo.GetAll().ToList();

            // Adiciona o turno artificial "Todos"
            shifts.Insert(0, new Shift
            {
                Id = 0,
                NamePt = "Todos",
                NameJp = "すべて"
            });

            // Preenche o ComboBox
            cmbTurno.DataSource = shifts;
            cmbTurno.DisplayMember = "NamePt";
            cmbTurno.ValueMember = "Id";
            cmbTurno.SelectedIndex = 0;

            // Carrega setores reais do banco
            var sectors = _sectorRepo.GetAll().ToList();

            // Adiciona o setor artificial "Todos"
            sectors.Insert(0, new Sector
            {
                Id = 0,
                NamePt = "Todos",
                NameJp = "すべて"
            });

            // Preenche o ComboBox
            cmbSector.DataSource = sectors;
            cmbSector.DisplayMember = "NamePt";
            cmbSector.ValueMember = "Id";
            cmbSector.SelectedIndex = 0;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LoadMatrix();
        }

        private void LoadMatrix()
        {
            dgvLeituras.Columns.Clear();
            dgvLeituras.Rows.Clear();

            DateTime start = dtpInicio.Value.Date;
            DateTime end = dtpFim.Value.Date.AddDays(1);

            bool isLeader = rbLideres.Checked || rbMaSv.Checked;

            int setorSelecionado = (int)cmbSector.SelectedValue;

            // ---------------------------------------------------------
            // 1) Buscar operadores ou líderes (para montar colunas)
            // ---------------------------------------------------------
            var operadores = _opRepo.GetAll()
                .Where(o => o.Status == true && o.IsLeader == isLeader)
                .ToList();

            // ---------------------------------------------------------
            // 1.1) Aplicar filtro de turno AQUI (somente nos operadores)
            // ---------------------------------------------------------
            int turnoSelecionado = (int)cmbTurno.SelectedValue;

            if (turnoSelecionado != 0)
            {
                operadores = operadores
                    .Where(o => o.ShiftId == turnoSelecionado)
                    .ToList();
            }

            if (setorSelecionado != 0)
            {
                if (setorSelecionado == 1)
                {
                    // Setor 1 → pega 1 + 3
                    operadores = operadores
                        .Where(o => o.SectorId == 1 || o.SectorId == 3)
                        .ToList();
                }
                else if (setorSelecionado == 2)
                {
                    // Setor 2 → pega 2 + 3
                    operadores = operadores
                        .Where(o => o.SectorId == 2 || o.SectorId == 3)
                        .ToList();
                }
                else if (setorSelecionado == 3)
                {
                    // Setor 3 → só 3
                    operadores = operadores
                        .Where(o => o.SectorId == 3)
                        .ToList();
                }
            }

            // ---------------------------------------------------------
            // 1.2) Ordenar depois do filtro
            // ---------------------------------------------------------
            operadores = operadores
                .OrderBy(o => isLeader ? o.GroupId : o.SectorId)
                .ThenBy(o => o.GroupId)
                .ThenBy(o => o.NameRomanji)
                .ToList();

            operadores = operadores
                .OrderBy(o => isLeader ? o.GroupId : o.SectorId)
                .ThenBy(o => o.GroupId)
                .ThenBy(o => o.NameRomanji)
                .ToList();

            // ---------------------------------------------------------
            // 2) Buscar Hikitsugui do período (SEM filtro de turno)
            // ---------------------------------------------------------
            var hiks = _hikRepo.GetAllWithSector()
                .Where(h => h.Date >= start && h.Date < end)
                .OrderByDescending(h => h.Date)
                .ToList();

            if (setorSelecionado != 0)
            {
                if (setorSelecionado == 1)
                {
                    // Setor 1 → pega 1 + 3
                    hiks = hiks
                        .Where(h => h.SectorId == 1 || h.SectorId == 3)
                        .ToList();
                }
                else if (setorSelecionado == 2)
                {
                    // Setor 2 → pega 2 + 3
                    hiks = hiks
                        .Where(h => h.SectorId == 2 || h.SectorId == 3)
                        .ToList();
                }
                else if (setorSelecionado == 3)
                {
                    // Setor 3 → só 3
                    hiks = hiks
                        .Where(h => h.SectorId == 3)
                        .ToList();
                }
            }

            // ---------------------------------------------------------
            // 2.2) Filtro por público (Operadores / Líderes / MA-SV)
            // ---------------------------------------------------------
            if (rbOperadores.Checked)
            {
                // Mostrar apenas Hikitsugui destinados a operadores
                hiks = hiks
                    .Where(h => h.ForOperators == true)
                    .ToList();
            }
            else if (rbLideres.Checked)
            {
                // Mostrar apenas Hikitsugui destinados a líderes
                hiks = hiks
                    .Where(h => h.ForLeaders == true)
                    .ToList();
            }
            else if (rbMaSv.Checked)
            {
                // Mostrar apenas Hikitsugui destinados a MA/SV
                hiks = hiks
                    .Where(h => h.ForMaSv == true)
                    .ToList();
            }

            // ---------------------------------------------------------
            // 3) Buscar leituras
            // ---------------------------------------------------------
            var leituras = _readRepo.GetByPeriod(start, end);

            // ---------------------------------------------------------
            // 4) Criar colunas dinâmicas
            // ---------------------------------------------------------
            dgvLeituras.Columns.Add("HikitsuguiInfo", "Hikitsugui");
            dgvLeituras.Columns["HikitsuguiInfo"].Width = 300;

            dgvLeituras.Columns.Add("HikitsuguiId", "Id");
            dgvLeituras.Columns["HikitsuguiId"].Visible = false;

            foreach (var op in operadores)
            {
                dgvLeituras.Columns.Add(op.CodigoFJ, op.NameRomanji);
                dgvLeituras.Columns[op.CodigoFJ].Width = 30;
            }
            
            dgvLeituras.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvLeituras.ColumnHeadersHeight = 120; // ajuste fino se quiser

            // ---------------------------------------------------------
            // 5) Preencher linhas
            // ---------------------------------------------------------
            foreach (var h in hiks)
            {
                int rowIndex = dgvLeituras.Rows.Add();
                var row = dgvLeituras.Rows[rowIndex];

                row.Cells["HikitsuguiId"].Value = h.Id;
                string desc = RtfToPlainText(h.Description);

                row.Cells["HikitsuguiInfo"].Value =
                    $"{h.Date:yyyy/MM/dd} - {h.CategoryName} - {desc}";

                foreach (var op in operadores)
                {
                    bool leu = leituras.Any(r =>
                        r.HikitsuguiId == h.Id &&
                        r.ReaderCodigoFJ == op.CodigoFJ);

                    var cell = row.Cells[op.CodigoFJ];

                    cell.Value = leu ? "○" : "×";
                    cell.Style.ForeColor = leu ? Color.Green : Color.Red;
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void dgvLeituras_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex != 0)
                return;

            int hikId = (int)dgvLeituras.Rows[e.RowIndex].Cells["HikitsuguiId"].Value;

            var hik = _hikRepo.GetById(hikId);
            if (hik == null)
            {
                MessageBox.Show("Hikitsugui não encontrado.");
                return;
            }

            var form = new FormHikitsuguiPreview(hik);
            form.ShowDialog();
        }

        private string RtfToPlainText(string rtf)
        {
            if (string.IsNullOrWhiteSpace(rtf))
                return "";

            using var rtb = new RichTextBox();
            rtb.Rtf = rtf;
            return rtb.Text;
        }
        private void dgvLeituras_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Apenas cabeçalho de coluna
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, false);

                string texto = e.FormattedValue?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(texto))
                {
                    e.Handled = true;
                    return;
                }

                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Rotaciona o texto 90 graus
                    e.Graphics.TranslateTransform(e.CellBounds.Left, e.CellBounds.Bottom);
                    e.Graphics.RotateTransform(-90);

                    e.Graphics.DrawString(
                        texto,
                        e.CellStyle.Font,
                        new SolidBrush(e.CellStyle.ForeColor),
                        new Rectangle(0, 0, e.CellBounds.Height, e.CellBounds.Width),
                        format
                    );

                    e.Graphics.ResetTransform();
                }

                e.Handled = true;
            }
        }
    }
}
