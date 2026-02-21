namespace TeamOps.UI.Forms
{
    partial class FormHikitsuguiReader
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelHeader;
        private Label lblTitle;

        private Label lblDataInicio;
        private Label lblDataFim;

        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFim;

        private RadioButton rbOperadores;
        private RadioButton rbLideres;

        private Button btnBuscar;

        private DataGridView dgvLeituras;

        private ComboBox cmbTurno;
        private Label lblTurno;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            lblDataInicio = new Label();
            lblDataFim = new Label();
            dtpInicio = new DateTimePicker();
            dtpFim = new DateTimePicker();
            rbOperadores = new RadioButton();
            rbLideres = new RadioButton();
            btnBuscar = new Button();
            dgvLeituras = new DataGridView();
            lblTurno = new Label();
            cmbTurno = new ComboBox();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLeituras).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(45, 85, 155);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1070, 70);
            panelHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(100, 23);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Leitura de Hikitsugui – 引継ぎ閲覧";
            // 
            // lblDataInicio
            // 
            lblDataInicio.Location = new Point(20, 87);
            lblDataInicio.Name = "lblDataInicio";
            lblDataInicio.Size = new Size(100, 23);
            lblDataInicio.TabIndex = 3;
            lblDataInicio.Text = "Data Início:";
            // 
            // lblDataFim
            // 
            lblDataFim.Location = new Point(250, 87);
            lblDataFim.Name = "lblDataFim";
            lblDataFim.Size = new Size(100, 23);
            lblDataFim.TabIndex = 4;
            lblDataFim.Text = "Data Fim:";
            // 
            // dtpInicio
            // 
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Location = new Point(20, 110);
            dtpInicio.Name = "dtpInicio";
            dtpInicio.Size = new Size(200, 23);
            dtpInicio.TabIndex = 5;
            // 
            // dtpFim
            // 
            dtpFim.Format = DateTimePickerFormat.Short;
            dtpFim.Location = new Point(250, 110);
            dtpFim.Name = "dtpFim";
            dtpFim.Size = new Size(200, 23);
            dtpFim.TabIndex = 6;
            // 
            // rbOperadores
            // 
            rbOperadores.Location = new Point(474, 110);
            rbOperadores.Name = "rbOperadores";
            rbOperadores.Size = new Size(87, 24);
            rbOperadores.TabIndex = 7;
            rbOperadores.Text = "Operadores";
            // 
            // rbLideres
            // 
            rbLideres.Location = new Point(566, 110);
            rbLideres.Name = "rbLideres";
            rbLideres.Size = new Size(65, 24);
            rbLideres.TabIndex = 8;
            rbLideres.Text = "Líderes";
            // 
            // btnBuscar
            // 
            btnBuscar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBuscar.Location = new Point(920, 108);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(120, 28);
            btnBuscar.TabIndex = 9;
            btnBuscar.Text = "Buscar";
            btnBuscar.Click += btnBuscar_Click;
            // 
            // dgvLeituras
            // 
            dgvLeituras.AllowUserToAddRows = false;
            dgvLeituras.AllowUserToDeleteRows = false;
            dgvLeituras.Location = new Point(20, 160);
            dgvLeituras.Name = "dgvLeituras";
            dgvLeituras.ReadOnly = true;
            dgvLeituras.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLeituras.Size = new Size(1020, 350);
            dgvLeituras.TabIndex = 10;
            dgvLeituras.CellDoubleClick += dgvLeituras_CellDoubleClick;
            // 
            // lblTurno
            // 
            lblTurno.Location = new Point(648, 87);
            lblTurno.Name = "lblTurno";
            lblTurno.Size = new Size(100, 23);
            lblTurno.TabIndex = 0;
            lblTurno.Text = "Turno:";
            // 
            // cmbTurno
            // 
            cmbTurno.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTurno.Location = new Point(648, 110);
            cmbTurno.Name = "cmbTurno";
            cmbTurno.Size = new Size(120, 23);
            // 
            // FormHikitsuguiReader
            // 
            ClientSize = new Size(1070, 540);
            Controls.Add(lblTurno);
            Controls.Add(cmbTurno);
            Controls.Add(panelHeader);
            Controls.Add(lblDataInicio);
            Controls.Add(lblDataFim);
            Controls.Add(dtpInicio);
            Controls.Add(dtpFim);
            Controls.Add(rbOperadores);
            Controls.Add(rbLideres);
            Controls.Add(btnBuscar);
            Controls.Add(dgvLeituras);
            Name = "FormHikitsuguiReader";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Leitura de Hikitsugui";
            panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLeituras).EndInit();
            ResumeLayout(false);
        }
    }
}
