namespace TeamOps.UI.Forms
{
    partial class FormHikitsuguiReader
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.Label lblDataInicio;
        private System.Windows.Forms.Label lblDataFim;
        private System.Windows.Forms.Label lblSetor;
        private System.Windows.Forms.Label lblLider;

        private System.Windows.Forms.DateTimePicker dtpInicio;
        private System.Windows.Forms.DateTimePicker dtpFim;
        private System.Windows.Forms.ComboBox cmbSetor;
        private System.Windows.Forms.ComboBox cmbLider;

        private System.Windows.Forms.Button btnBuscar;

        private System.Windows.Forms.DataGridView dgvLeituras;

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
            lblSetor = new Label();
            lblLider = new Label();
            dtpInicio = new DateTimePicker();
            dtpFim = new DateTimePicker();
            cmbSetor = new ComboBox();
            cmbLider = new ComboBox();
            btnBuscar = new Button();
            dgvLeituras = new DataGridView();
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
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(100, 23);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Leituras de Hikitsugui – 引継ぎ閲覧";
            // 
            // lblDataInicio
            // 
            lblDataInicio.Location = new Point(20, 85);
            lblDataInicio.Name = "lblDataInicio";
            lblDataInicio.Size = new Size(100, 23);
            lblDataInicio.TabIndex = 1;
            lblDataInicio.Text = "Data Início:";
            // 
            // lblDataFim
            // 
            lblDataFim.Location = new Point(250, 85);
            lblDataFim.Name = "lblDataFim";
            lblDataFim.Size = new Size(100, 23);
            lblDataFim.TabIndex = 2;
            lblDataFim.Text = "Data Fim:";
            // 
            // lblSetor
            // 
            lblSetor.Location = new Point(480, 85);
            lblSetor.Name = "lblSetor";
            lblSetor.Size = new Size(100, 23);
            lblSetor.TabIndex = 3;
            lblSetor.Text = "Setor:";
            // 
            // lblLider
            // 
            lblLider.Location = new Point(700, 85);
            lblLider.Name = "lblLider";
            lblLider.Size = new Size(100, 23);
            lblLider.TabIndex = 4;
            lblLider.Text = "Líder:";
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
            // cmbSetor
            // 
            cmbSetor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSetor.Location = new Point(480, 110);
            cmbSetor.Name = "cmbSetor";
            cmbSetor.Size = new Size(200, 23);
            cmbSetor.TabIndex = 7;
            // 
            // cmbLider
            // 
            cmbLider.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLider.Location = new Point(700, 110);
            cmbLider.Name = "cmbLider";
            cmbLider.Size = new Size(200, 23);
            cmbLider.TabIndex = 8;
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
            // 
            // FormHikitsuguiReader
            // 
            ClientSize = new Size(1070, 540);
            Controls.Add(panelHeader);
            Controls.Add(lblDataInicio);
            Controls.Add(lblDataFim);
            Controls.Add(lblSetor);
            Controls.Add(lblLider);
            Controls.Add(dtpInicio);
            Controls.Add(dtpFim);
            Controls.Add(cmbSetor);
            Controls.Add(cmbLider);
            Controls.Add(btnBuscar);
            Controls.Add(dgvLeituras);
            Name = "FormHikitsuguiReader";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Leituras de Hikitsugui";
            panelHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLeituras).EndInit();
            ResumeLayout(false);
        }
    }
}
