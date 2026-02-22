using Microsoft.VisualBasic.Logging;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormFollowSingleReport : Form
    {
        private readonly int _followId;
        private readonly FollowUpRepository _followRepo;
        private readonly OperatorRepository _opRepo;

        private PrintDocument printDoc = new PrintDocument();
        private FollowUp _follow;
        private string _operatorRomanji;
        private string _operatorNihongo;
        private Operator _operator;
        private Image _logo;

        public FormFollowSingleReport(
            int followId,
            FollowUpRepository followRepo,
            OperatorRepository opRepo)
        {
            InitializeComponent();

            _followId = followId;
            _followRepo = followRepo;
            _opRepo = opRepo;

            _logo = Image.FromFile("Assets/logo_rodape.png");

            // Evento de impressão
            printDoc.PrintPage += PrintDoc_PrintPage;

            // Organiza os labels do painel superior
            ArrangeInfoLabels();

            LoadReport();
        }

        private void LoadReport()
        {
            var f = _followRepo.GetByIdWithJoins(_followId);

            if (f == null)
            {
                MessageBox.Show("FollowUp não encontrado.");
                Close();
                return;
            }

            var op = _opRepo.GetByCodigoFJ(f.OperatorCodigoFJ);
            var ex = _opRepo.GetByCodigoFJ(f.ExecutorCodigoFJ);
            var wi = f.WitnessCodigoFJ != null
                ? _opRepo.GetByCodigoFJ(f.WitnessCodigoFJ)
                : null;
            
            _operator = op;

            lblTitle.Text = $"{op.NameRomanji} / {op.NameNihongo} ({op.CodigoFJ})";

            lblDate.Text = $"Data: {f.Date:yyyy/MM/dd HH:mm}";
            lblShift.Text = $"Turno: {f.ShiftName}";
            lblExecutor.Text = $"Executor: {ex.NameRomanji} / {ex.NameNihongo}";
            lblWitness.Text = wi != null
                ? $"Testemunha: {wi.NameRomanji} / {wi.NameNihongo}"
                : "Testemunha: -";

            lblReason.Text = $"Motivo: {f.ReasonName}";
            lblType.Text = $"Tipo: {f.TypeName}";
            lblLocal.Text = $"Local: {f.LocalName}";
            lblEquipment.Text = $"Equipamento: {f.EquipmentName}";
            lblSector.Text = $"Setor: {f.SectorName}";

            rtbDescription.Text = f.Description;
            rtbGuidance.Text = f.Guidance;

            _follow = f;
            _operatorRomanji = op.NameRomanji;
            _operatorNihongo = op.NameNihongo;
        }

        // ---------------------------------------------------------
        // IMPRESSÃO A4
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // MÉTODO PARA DESENHAR CAIXAS ARREDONDADAS
        // ---------------------------------------------------------
        private GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            float d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        // ---------------------------------------------------------
        // IMPRESSÃO A4 PREMIUM
        // ---------------------------------------------------------
        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            float left = 50;
            float right = e.MarginBounds.Right;
            float y = 40;

            Font titleFont = new Font("Yu Gothic UI", 20, FontStyle.Bold);
            Font sectionFont = new Font("Yu Gothic UI", 13, FontStyle.Bold);
            Font labelFont = new Font("Yu Gothic UI", 11, FontStyle.Bold);
            Font textFont = new Font("Yu Gothic UI", 11);
            Pen linePen = new Pen(Color.Black, 1);

            // ---------------------------------------------------------
            // CABEÇALHO
            // ---------------------------------------------------------
            e.Graphics.DrawString("FOLLOW-UP REPORT / フォローアップ報告書", titleFont, Brushes.Black, left, y);
            y += 40;

            e.Graphics.DrawLine(linePen, left, y, right, y);
            y += 20;

            e.Graphics.DrawString(
                $"{_operatorRomanji} / {_operatorNihongo} ({_follow.OperatorCodigoFJ})",
                new Font("Yu Gothic UI", 14, FontStyle.Bold),
                Brushes.Black,
                left,
                y
            );
            y += 35;

            e.Graphics.DrawLine(linePen, left, y, right, y);
            y += 25;

            // ---------------------------------------------------------
            // INFORMAÇÕES GERAIS
            // ---------------------------------------------------------
            e.Graphics.DrawString("Informações Gerais / 基本情報", sectionFont, Brushes.Black, left, y);
            y += 30;

            void Draw(string label, string value)
            {
                e.Graphics.DrawString(label, labelFont, Brushes.Black, left, y);
                e.Graphics.DrawString(value, textFont, Brushes.Black, left + 180, y);
                y += 22;
            }

            Draw("Data / 日付:", _follow.Date.ToString("yyyy/MM/dd HH:mm"));
            Draw("Turno / シフト:", _follow.ShiftName);
            Draw("Admissão / 入社日:",
                _operator.StartDate.ToString("yyyy/MM/dd"));
            Draw("Executor / 実行者:", lblExecutor.Text.Replace("Executor: ", ""));
            Draw("Testemunha / 目撃者:", lblWitness.Text.Replace("Testemunha: ", ""));
            Draw("Motivo / 理由:", _follow.ReasonName);
            Draw("Tipo / 種類:", _follow.TypeName);
            Draw("Local / 場所:", _follow.LocalName);
            Draw("Equipamento / 設備:", _follow.EquipmentName);
            Draw("Setor / 工程:", _follow.SectorName);

            y += 15;
            e.Graphics.DrawLine(linePen, left, y, right, y);
            y += 25;

            // ---------------------------------------------------------
            // CÁLCULO DO ESPAÇO RESTANTE
            // ---------------------------------------------------------
            float availableHeight = e.MarginBounds.Bottom - y - 120;
            float boxHeight = availableHeight / 2;

            // ---------------------------------------------------------
            // DESCRIÇÃO
            // ---------------------------------------------------------
            e.Graphics.DrawString("Descrição / 説明", sectionFont, Brushes.Black, left, y);
            y += 30;

            RectangleF descBox = new RectangleF(left, y, right - left, boxHeight);

            using (GraphicsPath gp = RoundedRect(descBox, 12))
                e.Graphics.DrawPath(Pens.Black, gp);

            e.Graphics.DrawString(_follow.Description, textFont, Brushes.Black, descBox);
            y += boxHeight + 25;

            // ---------------------------------------------------------
            // ORIENTAÇÃO
            // ---------------------------------------------------------
            e.Graphics.DrawString("Orientação / 指示", sectionFont, Brushes.Black, left, y);
            y += 30;

            RectangleF guideBox = new RectangleF(left, y, right - left, boxHeight);

            using (GraphicsPath gp = RoundedRect(guideBox, 12))
                e.Graphics.DrawPath(Pens.Black, gp);

            e.Graphics.DrawString(_follow.Guidance, textFont, Brushes.Black, guideBox);

            // ---------------------------------------------------------
            // RODAPÉ + LOGO
            // ---------------------------------------------------------
            string footer = $"Gerado em {DateTime.Now:yyyy/MM/dd HH:mm}";
            e.Graphics.DrawString(footer, new Font("Yu Gothic UI", 9), Brushes.Gray, left, e.MarginBounds.Bottom + 20);

            if (_logo != null)
            {
                float logoWidth = 120;
                float logoHeight = 40;
                float logoX = e.MarginBounds.Right - logoWidth;
                float logoY = e.MarginBounds.Bottom + 5;

                e.Graphics.DrawImage(_logo, logoX, logoY, logoWidth, logoHeight);
            }
        }

        // ---------------------------------------------------------
        // BOTÃO IMPRIMIR
        // ---------------------------------------------------------
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDoc;
            preview.Width = 1200;
            preview.Height = 800;
            preview.ShowDialog();
        }

        // ---------------------------------------------------------
        // BOTÃO PDF
        // ---------------------------------------------------------
        private void btnPdf_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "FollowUp.pdf";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            printDoc.PrinterSettings.PrintToFile = true;
            printDoc.PrinterSettings.PrintFileName = sfd.FileName;
            printDoc.Print();
        }

        // ---------------------------------------------------------
        // ORGANIZAÇÃO DOS LABELS DO PAINEL SUPERIOR
        // ---------------------------------------------------------
        private void ArrangeInfoLabels()
        {
            int y = 10;
            int step = 25;

            void Place(Label lbl)
            {
                lbl.Location = new Point(10, y);
                y += step;
            }

            Place(lblDate);
            Place(lblShift);
            Place(lblExecutor);
            Place(lblWitness);
            Place(lblReason);
            Place(lblType);
            Place(lblLocal);
            Place(lblEquipment);
            Place(lblSector);
        }
    }
}
