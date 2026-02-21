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
        private System.Windows.Forms.DataGridView dgvFollow;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
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
            dgvFollow = new DataGridView();
            lblDtInicial = new Label();
            lblDtFinal = new Label();
            lblTurno = new Label();
            lblOperador = new Label();
            lblMotivo = new Label();
            lblTipo = new Label();
            lblEquipamento = new Label();
            lblSetor = new Label();
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
            panelFilters.Dock = DockStyle.Top;
            panelFilters.Location = new Point(0, 0);
            panelFilters.Name = "panelFilters";
            panelFilters.Padding = new Padding(5);
            panelFilters.Size = new Size(1280, 60);
            panelFilters.TabIndex = 1;
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(5, 27);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new Size(110, 23);
            dtpInicio.TabIndex = 0;
            // 
            // dtpFim
            // 
            dtpFim.Format = DateTimePickerFormat.Short;
            dtpFim.Location = new Point(120, 27);
            dtpFim.Name = "dtpFim";
            dtpFim.Size = new Size(110, 23);
            dtpFim.TabIndex = 1;
            // 
            // cmbShift
            // 
            cmbShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShift.Location = new Point(235, 27);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(120, 23);
            cmbShift.TabIndex = 2;
            // 
            // cmbOperator
            // 
            cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperator.Location = new Point(360, 27);
            cmbOperator.Name = "cmbOperator";
            cmbOperator.Size = new Size(150, 23);
            cmbOperator.TabIndex = 3;
            // 
            // cmbReason
            // 
            cmbReason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReason.Location = new Point(515, 27);
            cmbReason.Name = "cmbReason";
            cmbReason.Size = new Size(150, 23);
            cmbReason.TabIndex = 4;
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Location = new Point(670, 27);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(150, 23);
            cmbType.TabIndex = 5;
            // 
            // cmbEquipment
            // 
            cmbEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEquipment.Location = new Point(825, 27);
            cmbEquipment.Name = "cmbEquipment";
            cmbEquipment.Size = new Size(150, 23);
            cmbEquipment.TabIndex = 6;
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.Location = new Point(980, 27);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(150, 23);
            cmbSector.TabIndex = 7;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(1136, 22);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(100, 30);
            btnBuscar.TabIndex = 8;
            btnBuscar.Text = "Buscar";
            btnBuscar.Click += btnBuscar_Click;
            // 
            // dgvFollow
            // 
            dgvFollow.AllowUserToAddRows = false;
            dgvFollow.AllowUserToDeleteRows = false;
            dgvFollow.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFollow.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvFollow.DefaultCellStyle = dataGridViewCellStyle4;
            dgvFollow.Dock = DockStyle.Fill;
            dgvFollow.Location = new Point(0, 60);
            dgvFollow.Name = "dgvFollow";
            dgvFollow.ReadOnly = true;
            dgvFollow.RowHeadersVisible = false;
            dgvFollow.Size = new Size(1280, 660);
            dgvFollow.TabIndex = 0;
            // 
            // lblDtInicial
            // 
            lblDtInicial.AutoSize = true;
            lblDtInicial.Location = new Point(5, 9);
            lblDtInicial.Name = "lblDtInicial";
            lblDtInicial.Size = new Size(68, 15);
            lblDtInicial.TabIndex = 9;
            lblDtInicial.Text = "Data Inicial:";
            // 
            // lblDtFinal
            // 
            lblDtFinal.AutoSize = true;
            lblDtFinal.Location = new Point(120, 9);
            lblDtFinal.Name = "lblDtFinal";
            lblDtFinal.Size = new Size(62, 15);
            lblDtFinal.TabIndex = 10;
            lblDtFinal.Text = "Data Final:";
            // 
            // lblTurno
            // 
            lblTurno.AutoSize = true;
            lblTurno.Location = new Point(235, 9);
            lblTurno.Name = "lblTurno";
            lblTurno.Size = new Size(42, 15);
            lblTurno.TabIndex = 11;
            lblTurno.Text = "Turno:";
            // 
            // lblOperador
            // 
            lblOperador.AutoSize = true;
            lblOperador.Location = new Point(360, 9);
            lblOperador.Name = "lblOperador";
            lblOperador.Size = new Size(60, 15);
            lblOperador.TabIndex = 12;
            lblOperador.Text = "Operador:";
            // 
            // lblMotivo
            // 
            lblMotivo.AutoSize = true;
            lblMotivo.Location = new Point(515, 9);
            lblMotivo.Name = "lblMotivo";
            lblMotivo.Size = new Size(48, 15);
            lblMotivo.TabIndex = 13;
            lblMotivo.Text = "Motivo:";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(670, 9);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(34, 15);
            lblTipo.TabIndex = 14;
            lblTipo.Text = "Tipo:";
            // 
            // lblEquipamento
            // 
            lblEquipamento.AutoSize = true;
            lblEquipamento.Location = new Point(825, 9);
            lblEquipamento.Name = "lblEquipamento";
            lblEquipamento.Size = new Size(81, 15);
            lblEquipamento.TabIndex = 15;
            lblEquipamento.Text = "Equipamento:";
            // 
            // lblSetor
            // 
            lblSetor.AutoSize = true;
            lblSetor.Location = new Point(980, 9);
            lblSetor.Name = "lblSetor";
            lblSetor.Size = new Size(37, 15);
            lblSetor.TabIndex = 16;
            lblSetor.Text = "Setor:";
            // 
            // FormFollowReport
            // 
            ClientSize = new Size(1280, 720);
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
