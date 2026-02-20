namespace TeamOps.UI.Forms
{
    partial class FormOperators
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvOperators;
        private System.Windows.Forms.Label lblCodigoFJ;
        private System.Windows.Forms.TextBox txtCodigoFJ;
        private System.Windows.Forms.Label lblRomanji;
        private System.Windows.Forms.TextBox txtRomanji;
        private System.Windows.Forms.Label lblNihongo;
        private System.Windows.Forms.TextBox txtNihongo;
        private System.Windows.Forms.Label lblShift;
        private System.Windows.Forms.ComboBox cmbShift;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.ComboBox cmbGroup;
        private System.Windows.Forms.Label lblSector;
        private System.Windows.Forms.ComboBox cmbSector;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Label lblEnd;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.CheckBox chkHasEnd;
        private System.Windows.Forms.CheckBox chkTrainer;
        private System.Windows.Forms.CheckBox chkStatus;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.CheckBox chkIsLeader;
        private System.Windows.Forms.Label lblIsLeader;
        private System.Windows.Forms.Label lblTelefone;
        private System.Windows.Forms.TextBox txtTelefone;
        private System.Windows.Forms.Label lblEndereco;
        private System.Windows.Forms.TextBox txtEndereco;
        private System.Windows.Forms.Label lblNascimento;
        private System.Windows.Forms.DateTimePicker dtpNascimento;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvOperators = new DataGridView();
            lblCodigoFJ = new Label();
            txtCodigoFJ = new TextBox();
            lblRomanji = new Label();
            txtRomanji = new TextBox();
            lblNihongo = new Label();
            txtNihongo = new TextBox();
            lblShift = new Label();
            cmbShift = new ComboBox();
            lblGroup = new Label();
            cmbGroup = new ComboBox();
            lblSector = new Label();
            cmbSector = new ComboBox();
            lblStart = new Label();
            dtpStart = new DateTimePicker();
            lblEnd = new Label();
            dtpEnd = new DateTimePicker();
            chkHasEnd = new CheckBox();
            chkTrainer = new CheckBox();
            chkStatus = new CheckBox();
            chkIsLeader = new CheckBox();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnNew = new Button();
            lblTelefone = new Label();
            txtTelefone = new TextBox();
            lblEndereco = new Label();
            txtEndereco = new TextBox();
            lblNascimento = new Label();
            dtpNascimento = new DateTimePicker();
            dtpNascimento.Format = DateTimePickerFormat.Short;
            ((System.ComponentModel.ISupportInitialize)dgvOperators).BeginInit();
            SuspendLayout();
            // 
            // dgvOperators
            // 
            dgvOperators.AllowUserToAddRows = false;
            dgvOperators.AllowUserToDeleteRows = false;
            dgvOperators.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvOperators.ColumnHeadersHeight = 29;
            dgvOperators.Location = new Point(20, 20);
            dgvOperators.MultiSelect = false;
            dgvOperators.Name = "dgvOperators";
            dgvOperators.ReadOnly = true;
            dgvOperators.RowHeadersWidth = 51;
            dgvOperators.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOperators.Size = new Size(760, 200);
            dgvOperators.TabIndex = 0;
            dgvOperators.SelectionChanged += dgvOperators_SelectionChanged;
            // 
            // lblCodigoFJ
            // 
            lblCodigoFJ.Location = new Point(20, 240);
            lblCodigoFJ.Name = "lblCodigoFJ";
            lblCodigoFJ.Size = new Size(74, 23);
            lblCodigoFJ.TabIndex = 1;
            lblCodigoFJ.Text = "CódigoFJ:";
            // 
            // txtCodigoFJ
            // 
            txtCodigoFJ.Location = new Point(100, 237);
            txtCodigoFJ.Name = "txtCodigoFJ";
            txtCodigoFJ.Size = new Size(150, 23);
            txtCodigoFJ.TabIndex = 2;
            // 
            // lblRomanji
            // 
            lblRomanji.Location = new Point(20, 280);
            lblRomanji.Name = "lblRomanji";
            lblRomanji.Size = new Size(74, 23);
            lblRomanji.TabIndex = 3;
            lblRomanji.Text = "Romanji:";
            // 
            // txtRomanji
            // 
            txtRomanji.Location = new Point(100, 277);
            txtRomanji.Name = "txtRomanji";
            txtRomanji.Size = new Size(200, 23);
            txtRomanji.TabIndex = 4;
            // 
            // lblNihongo
            // 
            lblNihongo.Location = new Point(20, 320);
            lblNihongo.Name = "lblNihongo";
            lblNihongo.Size = new Size(74, 23);
            lblNihongo.TabIndex = 5;
            lblNihongo.Text = "Nihongo:";
            // 
            // txtNihongo
            // 
            txtNihongo.Location = new Point(100, 317);
            txtNihongo.Name = "txtNihongo";
            txtNihongo.Size = new Size(200, 23);
            txtNihongo.TabIndex = 6;
            // 
            // lblShift
            // 
            lblShift.AutoSize = true;
            lblShift.Location = new Point(20, 360);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(34, 15);
            lblShift.TabIndex = 7;
            lblShift.Text = "Shift:";
            // 
            // cmbShift
            // 
            cmbShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShift.Location = new Point(100, 357);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(200, 23);
            cmbShift.TabIndex = 8;
            // 
            // lblGroup
            // 
            lblGroup.AutoSize = true;
            lblGroup.Location = new Point(20, 400);
            lblGroup.Name = "lblGroup";
            lblGroup.Size = new Size(43, 15);
            lblGroup.TabIndex = 9;
            lblGroup.Text = "Group:";
            // 
            // cmbGroup
            // 
            cmbGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroup.Location = new Point(100, 397);
            cmbGroup.Name = "cmbGroup";
            cmbGroup.Size = new Size(200, 23);
            cmbGroup.TabIndex = 10;
            // 
            // lblSector
            // 
            lblSector.AutoSize = true;
            lblSector.Location = new Point(20, 440);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(43, 15);
            lblSector.TabIndex = 11;
            lblSector.Text = "Sector:";
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.Location = new Point(100, 437);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(200, 23);
            cmbSector.TabIndex = 12;
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Location = new Point(350, 240);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(61, 15);
            lblStart.TabIndex = 13;
            lblStart.Text = "Start Date:";
            // 
            // dtpStart
            // 
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Location = new Point(430, 237);
            dtpStart.Name = "dtpStart";
            dtpStart.Size = new Size(120, 23);
            dtpStart.TabIndex = 14;
            // 
            // lblEnd
            // 
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(350, 280);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(57, 15);
            lblEnd.TabIndex = 15;
            lblEnd.Text = "End Date:";
            // 
            // dtpEnd
            // 
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Location = new Point(430, 277);
            dtpEnd.Name = "dtpEnd";
            dtpEnd.Size = new Size(120, 23);
            dtpEnd.TabIndex = 16;
            // 
            // chkHasEnd
            // 
            chkHasEnd.AutoSize = true;
            chkHasEnd.Location = new Point(560, 277);
            chkHasEnd.Name = "chkHasEnd";
            chkHasEnd.Size = new Size(96, 19);
            chkHasEnd.TabIndex = 17;
            chkHasEnd.Text = "Has End Date";
            // 
            // chkTrainer
            // 
            chkTrainer.AutoSize = true;
            chkTrainer.Location = new Point(76, 479);
            chkTrainer.Name = "chkTrainer";
            chkTrainer.Size = new Size(62, 19);
            chkTrainer.TabIndex = 18;
            chkTrainer.Text = "Trainer";
            // 
            // chkStatus
            // 
            chkStatus.AutoSize = true;
            chkStatus.Location = new Point(156, 479);
            chkStatus.Name = "chkStatus";
            chkStatus.Size = new Size(59, 19);
            chkStatus.TabIndex = 19;
            chkStatus.Text = "Active";
            // 
            // chkIsLeader
            // 
            chkIsLeader.AutoSize = true;
            chkIsLeader.Location = new Point(236, 479);
            chkIsLeader.Name = "chkIsLeader";
            chkIsLeader.Size = new Size(52, 19);
            chkIsLeader.TabIndex = 20;
            chkIsLeader.Text = "Líder";
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(520, 475);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(80, 30);
            btnAdd.TabIndex = 20;
            btnAdd.Text = "Add";
            btnAdd.Click += btnAdd_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(610, 475);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(80, 30);
            btnUpdate.TabIndex = 21;
            btnUpdate.Text = "Update";
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(700, 475);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(80, 30);
            btnDelete.TabIndex = 22;
            btnDelete.Text = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(430, 475);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(80, 30);
            btnNew.TabIndex = 23;
            btnNew.Text = "New";
            btnNew.Click += btnNew_Click;
            // 
            // lblTelefone
            // 
            lblTelefone.AutoSize = true;
            lblTelefone.Location = new Point(350, 320);
            lblTelefone.Name = "lblTelefone";
            lblTelefone.Size = new Size(55, 15);
            lblTelefone.TabIndex = 24;
            lblTelefone.Text = "Telefone:";
            // 
            // txtTelefone
            // 
            txtTelefone.Location = new Point(430, 317);
            txtTelefone.Name = "txtTelefone";
            txtTelefone.Size = new Size(200, 23);
            txtTelefone.TabIndex = 25;
            // 
            // lblEndereco
            // 
            lblEndereco.AutoSize = true;
            lblEndereco.Location = new Point(350, 360);
            lblEndereco.Name = "lblEndereco";
            lblEndereco.Size = new Size(59, 15);
            lblEndereco.TabIndex = 26;
            lblEndereco.Text = "Endereço:";
            // 
            // txtEndereco
            // 
            txtEndereco.Location = new Point(430, 360);
            txtEndereco.Multiline = true;
            txtEndereco.Name = "txtEndereco";
            txtEndereco.Size = new Size(350, 60);
            txtEndereco.TabIndex = 27;
            // 
            // lblNascimento
            // 
            lblNascimento.AutoSize = true;
            lblNascimento.Location = new Point(350, 440);
            lblNascimento.Name = "lblNascimento";
            lblNascimento.Size = new Size(74, 15);
            lblNascimento.TabIndex = 26;
            lblNascimento.Text = "Nascimento:";
            // 
            // dtpNascimento
            // 
            dtpNascimento.Location = new Point(430, 437);
            dtpNascimento.Name = "dtpNascimento";
            dtpNascimento.Size = new Size(350, 23);
            dtpNascimento.TabIndex = 0;
            // 
            // FormOperators
            // 
            ClientSize = new Size(800, 519);
            Controls.Add(btnNew);
            Controls.Add(dgvOperators);
            Controls.Add(lblCodigoFJ);
            Controls.Add(txtCodigoFJ);
            Controls.Add(lblRomanji);
            Controls.Add(txtRomanji);
            Controls.Add(lblNihongo);
            Controls.Add(txtNihongo);
            Controls.Add(lblShift);
            Controls.Add(cmbShift);
            Controls.Add(lblGroup);
            Controls.Add(cmbGroup);
            Controls.Add(lblSector);
            Controls.Add(cmbSector);
            Controls.Add(lblStart);
            Controls.Add(dtpStart);
            Controls.Add(lblEnd);
            Controls.Add(dtpEnd);
            Controls.Add(chkHasEnd);
            Controls.Add(chkTrainer);
            Controls.Add(chkStatus);
            Controls.Add(chkIsLeader);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(lblEndereco);
            Controls.Add(lblTelefone);
            Controls.Add(txtTelefone);
            Controls.Add(txtEndereco);
            Controls.Add(lblNascimento);
            Controls.Add(dtpNascimento);
            Name = "FormOperators";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Manage Operators";
            ((System.ComponentModel.ISupportInitialize)dgvOperators).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
