namespace TeamOps.UI.Forms
{
    partial class FormFollowReport
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
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.DataGridView dgvFollow;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            panelFilters = new Panel();
            lblSetor = new Label();
            lblEquipamento = new Label();
            lblTipo = new Label();
            lblMotivo = new Label();
            lblOperador = new Label();
            lblTurno = new Label();
            lblDtFinal = new Label();
            lblDtInicial = new Label();
            dtpInicio = new DateTimePicker();
            dtpFim = new DateTimePicker();
            cmbShift = new ComboBox();
            cmbOperator = new ComboBox();
            cmbReason = new ComboBox();
            cmbType = new ComboBox();
            cmbEquipment = new ComboBox();
            cmbSector = new ComboBox();
            btnBuscar = new Button();
            btnExportar = new Button();
            dgvFollow = new DataGridView();
            panelFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFollow).BeginInit();
            SuspendLayout();
            // 
            // panelFilters
            // 
            panelFilters.BackColor = Color.WhiteSmoke;
            panelFilters.Controls.Add(lblSetor);
            panelFilters.Controls.Add(lblEquipamento);
            panelFilters.Controls.Add(lblTipo);
            panelFilters.Controls.Add(lblMotivo);
            panelFilters.Controls.Add(lblOperador);
            panelFilters.Controls.Add(lblTurno);
            panelFilters.Controls.Add(lblDtFinal);
            panelFilters.Controls.Add(lblDtInicial);
            panelFilters.Controls.Add(dtpInicio);
            panelFilters.Controls.Add(dtpFim);
            panelFilters.Controls.Add(cmbShift);
            panelFilters.Controls.Add(cmbOperator);
            panelFilters.Controls.Add(cmbReason);
            panelFilters.Controls.Add(cmbType);
            panelFilters.Controls.Add(cmbEquipment);
            panelFilters.Controls.Add(cmbSector);
            panelFilters.Controls.Add(btnBuscar);
            panelFilters.Controls.Add(btnExportar);
            panelFilters.Dock = DockStyle.Top;
            panelFilters.Location = new Point(0, 0);
            panelFilters.Name = "panelFilters";
            panelFilters.Padding = new Padding(5);
            panelFilters.Size = new Size(1560, 60);
            panelFilters.TabIndex = 1;
            // 
            // lblSetor
            // 
            lblSetor.AutoSize = true;
            lblSetor.Location = new Point(980, 4);
            lblSetor.Name = "lblSetor";
            lblSetor.Size = new Size(47, 20);
            lblSetor.TabIndex = 16;
            lblSetor.Text = "Setor:";
            // 
            // lblEquipamento
            // 
            lblEquipamento.AutoSize = true;
            lblEquipamento.Location = new Point(825, 4);
            lblEquipamento.Name = "lblEquipamento";
            lblEquipamento.Size = new Size(101, 20);
            lblEquipamento.TabIndex = 15;
            lblEquipamento.Text = "Equipamento:";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(670, 4);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(42, 20);
            lblTipo.TabIndex = 14;
            lblTipo.Text = "Tipo:";
            // 
            // lblMotivo
            // 
            lblMotivo.AutoSize = true;
            lblMotivo.Location = new Point(515, 4);
            lblMotivo.Name = "lblMotivo";
            lblMotivo.Size = new Size(59, 20);
            lblMotivo.TabIndex = 13;
            lblMotivo.Text = "Motivo:";
            // 
            // lblOperador
            // 
            lblOperador.AutoSize = true;
            lblOperador.Location = new Point(360, 4);
            lblOperador.Name = "lblOperador";
            lblOperador.Size = new Size(76, 20);
            lblOperador.TabIndex = 12;
            lblOperador.Text = "Operador:";
            // 
            // lblTurno
            // 
            lblTurno.AutoSize = true;
            lblTurno.Location = new Point(235, 4);
            lblTurno.Name = "lblTurno";
            lblTurno.Size = new Size(48, 20);
            lblTurno.TabIndex = 11;
            lblTurno.Text = "Turno:";
            // 
            // lblDtFinal
            // 
            lblDtFinal.AutoSize = true;
            lblDtFinal.Location = new Point(120, 4);
            lblDtFinal.Name = "lblDtFinal";
            lblDtFinal.Size = new Size(79, 20);
            lblDtFinal.TabIndex = 10;
            lblDtFinal.Text = "Data Final:";
            // 
            // lblDtInicial
            // 
            lblDtInicial.AutoSize = true;
            lblDtInicial.Location = new Point(5, 4);
            lblDtInicial.Name = "lblDtInicial";
            lblDtInicial.Size = new Size(87, 20);
            lblDtInicial.TabIndex = 9;
            lblDtInicial.Text = "Data Inicial:";
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(5, 27);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new Size(110, 27);
            dtpInicio.TabIndex = 0;
            // 
            // dtpFim
            // 
            dtpFim.Format = DateTimePickerFormat.Short;
            dtpFim.Location = new Point(120, 27);
            dtpFim.Name = "dtpFim";
            dtpFim.Size = new Size(110, 27);
            dtpFim.TabIndex = 1;
            // 
            // cmbShift
            // 
            cmbShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShift.Location = new Point(235, 27);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(120, 28);
            cmbShift.TabIndex = 2;
            // 
            // cmbOperator
            // 
            cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperator.Location = new Point(360, 27);
            cmbOperator.Name = "cmbOperator";
            cmbOperator.Size = new Size(150, 28);
            cmbOperator.TabIndex = 3;
            // 
            // cmbReason
            // 
            cmbReason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReason.Location = new Point(515, 27);
            cmbReason.Name = "cmbReason";
            cmbReason.Size = new Size(150, 28);
            cmbReason.TabIndex = 4;
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Location = new Point(670, 27);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(150, 28);
            cmbType.TabIndex = 5;
            // 
            // cmbEquipment
            // 
            cmbEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEquipment.Location = new Point(825, 27);
            cmbEquipment.Name = "cmbEquipment";
            cmbEquipment.Size = new Size(150, 28);
            cmbEquipment.TabIndex = 6;
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.Location = new Point(980, 27);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(150, 28);
            cmbSector.TabIndex = 7;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(1136, 25);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(100, 30);
            btnBuscar.TabIndex = 8;
            btnBuscar.Text = "Buscar";
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Location = new Point(1242, 25);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(100, 30);
            btnExportar.TabIndex = 8;
            btnExportar.Text = "Exportar";
            btnExportar.Click += btnExportar_Click;
            // 
            // dgvFollow
            // 
            dgvFollow.AllowUserToAddRows = false;
            dgvFollow.AllowUserToDeleteRows = false;
            dgvFollow.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFollow.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvFollow.ColumnHeadersHeight = 29;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvFollow.DefaultCellStyle = dataGridViewCellStyle2;
            dgvFollow.Dock = DockStyle.Fill;
            dgvFollow.Location = new Point(0, 60);
            dgvFollow.Name = "dgvFollow";
            dgvFollow.ReadOnly = true;
            dgvFollow.RowHeadersVisible = false;
            dgvFollow.RowHeadersWidth = 51;
            dgvFollow.Size = new Size(1560, 752);
            dgvFollow.TabIndex = 0;
            // 
            // FormFollowReport
            // 
            ClientSize = new Size(1560, 812);
            Controls.Add(dgvFollow);
            Controls.Add(panelFilters);
            Name = "FormFollowReport";
            Text = "Relatório Follow";
            panelFilters.ResumeLayout(false);
            panelFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFollow).EndInit();
            ResumeLayout(false);
        }
        private Label lblMotivo;
        private Label lblOperador;
        private Label lblTurno;
        private Label lblDtFinal;
        private Label lblDtInicial;
        private Label lblSetor;
        private Label lblEquipamento;
        private Label lblTipo;
    }
}
