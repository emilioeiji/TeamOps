// Project: TeamOps.UI
// File: Forms/FormFollowUp.Designer.cs
namespace TeamOps.UI.Forms
{
    partial class FormFollowUp
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.DateTimePicker dtpDate;
        private System.Windows.Forms.Label lblShift;
        private System.Windows.Forms.ComboBox cmbShift;
        private System.Windows.Forms.Label lblOperator;
        private System.Windows.Forms.ComboBox cmbOperator;
        private System.Windows.Forms.Label lblExecutor;
        private System.Windows.Forms.ComboBox cmbExecutor;
        private System.Windows.Forms.Label lblWitness;
        private System.Windows.Forms.ComboBox cmbWitness;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.ComboBox cmbReason;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.ComboBox cmbLocal;
        private System.Windows.Forms.Label lblEquipment;
        private System.Windows.Forms.ComboBox cmbEquipment;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblGuidance;
        private System.Windows.Forms.TextBox txtGuidance;
        private System.Windows.Forms.Label lblSector;
        private System.Windows.Forms.ComboBox cmbSector;
        private System.Windows.Forms.Button btnSalvar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblId = new Label();
            txtId = new TextBox();
            lblDate = new Label();
            dtpDate = new DateTimePicker();
            lblShift = new Label();
            cmbShift = new ComboBox();
            lblOperator = new Label();
            cmbOperator = new ComboBox();
            lblExecutor = new Label();
            cmbExecutor = new ComboBox();
            lblWitness = new Label();
            cmbWitness = new ComboBox();
            lblReason = new Label();
            cmbReason = new ComboBox();
            lblType = new Label();
            cmbType = new ComboBox();
            lblLocal = new Label();
            cmbLocal = new ComboBox();
            lblEquipment = new Label();
            cmbEquipment = new ComboBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            lblGuidance = new Label();
            txtGuidance = new TextBox();
            btnSalvar = new Button();
            lblSector = new Label();
            cmbSector = new ComboBox();
            SuspendLayout();
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(20, 30);
            lblId.Name = "lblId";
            lblId.Size = new Size(49, 15);
            lblId.TabIndex = 0;
            lblId.Text = "Código:";
            // 
            // txtId
            // 
            txtId.Location = new Point(120, 28);
            txtId.Name = "txtId";
            txtId.ReadOnly = true;
            txtId.Size = new Size(100, 23);
            txtId.TabIndex = 1;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Location = new Point(20, 70);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(34, 15);
            lblDate.TabIndex = 2;
            lblDate.Text = "Data:";
            // 
            // dtpDate
            // 
            dtpDate.Location = new Point(120, 68);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new Size(200, 23);
            dtpDate.TabIndex = 3;
            // 
            // lblShift
            // 
            lblShift.AutoSize = true;
            lblShift.Location = new Point(20, 150);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(42, 15);
            lblShift.TabIndex = 4;
            lblShift.Text = "Turno:";
            cmbShift.SelectedIndexChanged += cmbShift_SelectedIndexChanged;
            // 
            // cmbShift
            // 
            cmbShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShift.Location = new Point(120, 148);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(200, 23);
            cmbShift.TabIndex = 5;
            cmbShift.SelectedIndexChanged += cmbShift_SelectedIndexChanged;
            // 
            // lblOperator
            // 
            lblOperator.AutoSize = true;
            lblOperator.Location = new Point(20, 190);
            lblOperator.Name = "lblOperator";
            lblOperator.Size = new Size(60, 15);
            lblOperator.TabIndex = 6;
            lblOperator.Text = "Operador:";
            // 
            // cmbOperator
            // 
            cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperator.Location = new Point(120, 188);
            cmbOperator.Name = "cmbOperator";
            cmbOperator.Size = new Size(200, 23);
            cmbOperator.TabIndex = 7;
            // 
            // lblExecutor
            // 
            lblExecutor.AutoSize = true;
            lblExecutor.Location = new Point(20, 230);
            lblExecutor.Name = "lblExecutor";
            lblExecutor.Size = new Size(55, 15);
            lblExecutor.TabIndex = 8;
            lblExecutor.Text = "Executor:";
            // 
            // cmbExecutor
            // 
            cmbExecutor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbExecutor.Location = new Point(120, 228);
            cmbExecutor.Name = "cmbExecutor";
            cmbExecutor.Size = new Size(200, 23);
            cmbExecutor.TabIndex = 9;
            // 
            // lblWitness
            // 
            lblWitness.AutoSize = true;
            lblWitness.Location = new Point(20, 270);
            lblWitness.Name = "lblWitness";
            lblWitness.Size = new Size(75, 15);
            lblWitness.TabIndex = 10;
            lblWitness.Text = "Testemunha:";
            // 
            // cmbWitness
            // 
            cmbWitness.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWitness.Location = new Point(120, 268);
            cmbWitness.Name = "cmbWitness";
            cmbWitness.Size = new Size(200, 23);
            cmbWitness.TabIndex = 11;
            // 
            // lblReason
            // 
            lblReason.AutoSize = true;
            lblReason.Location = new Point(20, 310);
            lblReason.Name = "lblReason";
            lblReason.Size = new Size(48, 15);
            lblReason.TabIndex = 12;
            lblReason.Text = "Motivo:";
            // 
            // cmbReason
            // 
            cmbReason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReason.Location = new Point(120, 308);
            cmbReason.Name = "cmbReason";
            cmbReason.Size = new Size(200, 23);
            cmbReason.TabIndex = 13;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(20, 350);
            lblType.Name = "lblType";
            lblType.Size = new Size(34, 15);
            lblType.TabIndex = 14;
            lblType.Text = "Tipo:";
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Location = new Point(120, 348);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(200, 23);
            cmbType.TabIndex = 15;
            // 
            // lblLocal
            // 
            lblLocal.AutoSize = true;
            lblLocal.Location = new Point(20, 390);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(38, 15);
            lblLocal.TabIndex = 16;
            lblLocal.Text = "Local:";
            // 
            // cmbLocal
            // 
            cmbLocal.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocal.Location = new Point(120, 388);
            cmbLocal.Name = "cmbLocal";
            cmbLocal.Size = new Size(200, 23);
            cmbLocal.TabIndex = 17;
            // 
            // lblEquipment
            // 
            lblEquipment.AutoSize = true;
            lblEquipment.Location = new Point(20, 430);
            lblEquipment.Name = "lblEquipment";
            lblEquipment.Size = new Size(81, 15);
            lblEquipment.TabIndex = 18;
            lblEquipment.Text = "Equipamento:";
            // 
            // cmbEquipment
            // 
            cmbEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEquipment.Location = new Point(120, 428);
            cmbEquipment.Name = "cmbEquipment";
            cmbEquipment.Size = new Size(200, 23);
            cmbEquipment.TabIndex = 19;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(20, 466);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(61, 15);
            lblDescription.TabIndex = 20;
            lblDescription.Text = "Descrição:";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(120, 466);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(400, 80);
            txtDescription.TabIndex = 21;
            // 
            // lblGuidance
            // 
            lblGuidance.AutoSize = true;
            lblGuidance.Location = new Point(20, 551);
            lblGuidance.Name = "lblGuidance";
            lblGuidance.Size = new Size(68, 15);
            lblGuidance.TabIndex = 22;
            lblGuidance.Text = "Orientação:";
            // 
            // txtGuidance
            // 
            txtGuidance.Location = new Point(120, 551);
            txtGuidance.Multiline = true;
            txtGuidance.Name = "txtGuidance";
            txtGuidance.Size = new Size(400, 80);
            txtGuidance.TabIndex = 23;
            // 
            // btnSalvar
            // 
            btnSalvar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSalvar.Location = new Point(120, 636);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(150, 40);
            btnSalvar.TabIndex = 99;
            btnSalvar.Text = "Salvar";
            btnSalvar.Click += btnSalvar_Click;
            // 
            // lblSector
            // 
            lblSector.AutoSize = true;
            lblSector.Location = new Point(20, 110);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(37, 15);
            lblSector.TabIndex = 20;
            lblSector.Text = "Setor:";
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.Location = new Point(120, 108);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(200, 23);
            cmbSector.TabIndex = 21;
            cmbSector.SelectedIndexChanged += cmbSector_SelectedIndexChanged;
            // 
            // FormFollowUp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 700);
            Controls.Add(lblId);
            Controls.Add(txtId);
            Controls.Add(lblDate);
            Controls.Add(dtpDate);
            Controls.Add(lblShift);
            Controls.Add(cmbShift);
            Controls.Add(lblOperator);
            Controls.Add(cmbOperator);
            Controls.Add(lblExecutor);
            Controls.Add(cmbExecutor);
            Controls.Add(lblWitness);
            Controls.Add(cmbWitness);
            Controls.Add(lblReason);
            Controls.Add(cmbReason);
            Controls.Add(lblType);
            Controls.Add(cmbType);
            Controls.Add(lblLocal);
            Controls.Add(cmbLocal);
            Controls.Add(lblEquipment);
            Controls.Add(cmbEquipment);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(lblGuidance);
            Controls.Add(txtGuidance);
            Controls.Add(lblSector);
            Controls.Add(cmbSector);
            Controls.Add(btnSalvar);
            Name = "FormFollowUp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cadastro de Acompanhamento";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
