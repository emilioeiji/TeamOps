using System.Windows.Forms.DataVisualization.Charting;

namespace TeamOps.UI.Forms
{
    partial class FormFollowChart
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelFilters;
        private System.Windows.Forms.DateTimePicker dtpInicio;
        private System.Windows.Forms.DateTimePicker dtpFim;
        private System.Windows.Forms.ComboBox cmbShift;
        private System.Windows.Forms.ComboBox cmbOperator;
        private System.Windows.Forms.ComboBox cmbReason;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.ComboBox cmbEquipment;
        private System.Windows.Forms.ComboBox cmbSector;
        private System.Windows.Forms.Button btnBuscar;

        private System.Windows.Forms.Label lblDtInicial;
        private System.Windows.Forms.Label lblDtFinal;
        private System.Windows.Forms.Label lblTurno;
        private System.Windows.Forms.Label lblOperador;
        private System.Windows.Forms.Label lblMotivo;
        private System.Windows.Forms.Label lblTipo;
        private System.Windows.Forms.Label lblEquipamento;
        private System.Windows.Forms.Label lblSetor;

        private Chart chartTurno;
        private Chart chartTipo;
        private Chart chartMotivo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            panelFilters = new Panel();
            dtpInicio = new DateTimePicker();
            dtpFim = new DateTimePicker();
            cmbShift = new ComboBox();
            cmbOperator = new ComboBox();
            cmbReason = new ComboBox();
            cmbType = new ComboBox();
            cmbEquipment = new ComboBox();
            cmbSector = new ComboBox();
            btnBuscar = new Button();

            lblDtInicial = new Label();
            lblDtFinal = new Label();
            lblTurno = new Label();
            lblOperador = new Label();
            lblMotivo = new Label();
            lblTipo = new Label();
            lblEquipamento = new Label();
            lblSetor = new Label();

            chartTurno = new Chart();
            chartTipo = new Chart();
            chartMotivo = new Chart();

            panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(chartTurno)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(chartTipo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(chartMotivo)).BeginInit();
            SuspendLayout();

            // ---------------------------------------------------------
            // PANEL FILTERS
            // ---------------------------------------------------------
            panelFilters.BackColor = Color.WhiteSmoke;
            panelFilters.Dock = DockStyle.Top;
            panelFilters.Height = 90;
            panelFilters.Padding = new Padding(5);

            int x = 5;

            // LABEL + CONTROL helper
            void AddLabel(Label lbl, string text, int posX)
            {
                lbl.Text = text;
                lbl.Location = new Point(posX, 5);
                lbl.AutoSize = true;
                panelFilters.Controls.Add(lbl);
            }

            void AddCombo(ComboBox cb, int posX)
            {
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.Location = new Point(posX, 25);
                cb.Width = 120;
                panelFilters.Controls.Add(cb);
            }

            void AddDate(DateTimePicker dtp, int posX)
            {
                dtp.Format = DateTimePickerFormat.Short;
                dtp.Location = new Point(posX, 25);
                dtp.Width = 110;
                panelFilters.Controls.Add(dtp);
            }

            // Data Inicial
            AddLabel(lblDtInicial, "Data Inicial:", x);
            AddDate(dtpInicio, x);
            x += 120;

            // Data Final
            AddLabel(lblDtFinal, "Data Final:", x);
            AddDate(dtpFim, x);
            x += 120;

            // Turno
            AddLabel(lblTurno, "Turno:", x);
            AddCombo(cmbShift, x);
            x += 130;

            // Operador
            AddLabel(lblOperador, "Operador:", x);
            AddCombo(cmbOperator, x);
            x += 130;

            // Motivo
            AddLabel(lblMotivo, "Motivo:", x);
            AddCombo(cmbReason, x);
            x += 130;

            // Tipo
            AddLabel(lblTipo, "Tipo:", x);
            AddCombo(cmbType, x);
            x += 130;

            // Equipamento
            AddLabel(lblEquipamento, "Equipamento:", x);
            AddCombo(cmbEquipment, x);
            x += 130;

            // Setor
            AddLabel(lblSetor, "Setor:", x);
            AddCombo(cmbSector, x);
            x += 130;

            // Botão Buscar
            btnBuscar.Text = "Buscar";
            btnBuscar.Location = new Point(x, 25);
            btnBuscar.Size = new Size(100, 30);
            btnBuscar.Click += btnBuscar_Click;
            panelFilters.Controls.Add(btnBuscar);

            // ---------------------------------------------------------
            // CHARTS
            // ---------------------------------------------------------
            void SetupChart(System.Windows.Forms.DataVisualization.Charting.Chart chart, string title, int posX, int posY)
            {
                chart.Location = new Point(posX, posY);
                chart.Size = new Size(400, 300);

                var area = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                chart.ChartAreas.Add(area);

                var series = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie
                };
                chart.Series.Add(series);

                var t = new System.Windows.Forms.DataVisualization.Charting.Title(title);
                chart.Titles.Add(t);

                Controls.Add(chart);
            }

            SetupChart(chartTurno, "Distribuição por Turno", 20, 110);
            SetupChart(chartTipo, "Distribuição por Tipo", 440, 110);
            SetupChart(chartMotivo, "Distribuição por Motivo", 860, 110);

            // ---------------------------------------------------------
            // FORM
            // ---------------------------------------------------------
            ClientSize = new Size(1280, 720);
            Controls.Add(panelFilters);
            Name = "FormFollowChart";
            Text = "Gráficos Follow";

            panelFilters.ResumeLayout(false);
            panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(chartTurno)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(chartTipo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(chartMotivo)).EndInit();
            ResumeLayout(false);
        }
    }
}
