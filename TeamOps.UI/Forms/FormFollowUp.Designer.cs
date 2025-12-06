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
            lblId.Location = new Point(23, 40);
            lblId.Name = "lblId";
            lblId.Size = new Size(61, 20);
            lblId.TabIndex = 0;
            lblId.Text = "Código:";
            // 
            // txtId
            // 
            txtId.Location = new Point(137, 37);
            txtId.Margin = new Padding(3, 4, 3, 4);
            txtId.Name = "txtId";
            txtId.ReadOnly = true;
            txtId.Size = new Size(114, 27);
            txtId.TabIndex = 1;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Location = new Point(23, 93);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(44, 20);
            lblDate.TabIndex = 2;
            lblDate.Text = "Data:";
            // 
            // dtpDate
            // 
            dtpDate.Location = new Point(137, 91);
            dtpDate.Margin = new Padding(3, 4, 3, 4);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new Size(228, 27);
            dtpDate.TabIndex = 3;
            // 
            // lblShift
            // 
            lblShift.AutoSize = true;
            lblShift.Location = new Point(23, 147);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(48, 20);
            lblShift.TabIndex = 4;
            lblShift.Text = "Turno:";
            // 
            // cmbShift
            // 
            cmbShift.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShift.Location = new Point(137, 144);
            cmbShift.Margin = new Padding(3, 4, 3, 4);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(228, 28);
            cmbShift.TabIndex = 5;
            cmbShift.SelectedIndexChanged += cmbShift_SelectedIndexChanged;
            // 
            // lblOperator
            // 
            lblOperator.AutoSize = true;
            lblOperator.Location = new Point(23, 200);
            lblOperator.Name = "lblOperator";
            lblOperator.Size = new Size(76, 20);
            lblOperator.TabIndex = 6;
            lblOperator.Text = "Operador:";
            // 
            // cmbOperator
            // 
            cmbOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperator.Location = new Point(137, 197);
            cmbOperator.Margin = new Padding(3, 4, 3, 4);
            cmbOperator.Name = "cmbOperator";
            cmbOperator.Size = new Size(228, 28);
            cmbOperator.TabIndex = 7;
            cmbOperator.SelectedIndexChanged += cmbOperator_SelectedIndexChanged;
            // 
            // lblExecutor
            // 
            lblExecutor.AutoSize = true;
            lblExecutor.Location = new Point(23, 253);
            lblExecutor.Name = "lblExecutor";
            lblExecutor.Size = new Size(69, 20);
            lblExecutor.TabIndex = 8;
            lblExecutor.Text = "Executor:";
            // 
            // cmbExecutor
            // 
            cmbExecutor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbExecutor.Location = new Point(137, 251);
            cmbExecutor.Margin = new Padding(3, 4, 3, 4);
            cmbExecutor.Name = "cmbExecutor";
            cmbExecutor.Size = new Size(228, 28);
            cmbExecutor.TabIndex = 9;
            // 
            // lblWitness
            // 
            lblWitness.AutoSize = true;
            lblWitness.Location = new Point(23, 307);
            lblWitness.Name = "lblWitness";
            lblWitness.Size = new Size(90, 20);
            lblWitness.TabIndex = 10;
            lblWitness.Text = "Testemunha:";
            // 
            // cmbWitness
            // 
            cmbWitness.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWitness.Location = new Point(137, 304);
            cmbWitness.Margin = new Padding(3, 4, 3, 4);
            cmbWitness.Name = "cmbWitness";
            cmbWitness.Size = new Size(228, 28);
            cmbWitness.TabIndex = 11;
            // 
            // lblReason
            // 
            lblReason.AutoSize = true;
            lblReason.Location = new Point(23, 360);
            lblReason.Name = "lblReason";
            lblReason.Size = new Size(59, 20);
            lblReason.TabIndex = 12;
            lblReason.Text = "Motivo:";
            // 
            // cmbReason
            // 
            cmbReason.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReason.Location = new Point(137, 357);
            cmbReason.Margin = new Padding(3, 4, 3, 4);
            cmbReason.Name = "cmbReason";
            cmbReason.Size = new Size(228, 28);
            cmbReason.TabIndex = 13;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(23, 413);
            lblType.Name = "lblType";
            lblType.Size = new Size(42, 20);
            lblType.TabIndex = 14;
            lblType.Text = "Tipo:";
            // 
            // cmbType
            // 
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Location = new Point(137, 411);
            cmbType.Margin = new Padding(3, 4, 3, 4);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(228, 28);
            cmbType.TabIndex = 15;
            // 
            // lblLocal
            // 
            lblLocal.AutoSize = true;
            lblLocal.Location = new Point(23, 467);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(47, 20);
            lblLocal.TabIndex = 16;
            lblLocal.Text = "Local:";
            // 
            // cmbLocal
            // 
            cmbLocal.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLocal.Location = new Point(137, 464);
            cmbLocal.Margin = new Padding(3, 4, 3, 4);
            cmbLocal.Name = "cmbLocal";
            cmbLocal.Size = new Size(228, 28);
            cmbLocal.TabIndex = 17;
            // 
            // lblEquipment
            // 
            lblEquipment.AutoSize = true;
            lblEquipment.Location = new Point(23, 520);
            lblEquipment.Name = "lblEquipment";
            lblEquipment.Size = new Size(101, 20);
            lblEquipment.TabIndex = 18;
            lblEquipment.Text = "Equipamento:";
            // 
            // cmbEquipment
            // 
            cmbEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEquipment.Location = new Point(137, 517);
            cmbEquipment.Margin = new Padding(3, 4, 3, 4);
            cmbEquipment.Name = "cmbEquipment";
            cmbEquipment.Size = new Size(228, 28);
            cmbEquipment.TabIndex = 19;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(23, 622);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(77, 20);
            lblDescription.TabIndex = 20;
            lblDescription.Text = "Descrição:";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(137, 622);
            txtDescription.Margin = new Padding(3, 4, 3, 4);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(457, 105);
            txtDescription.TabIndex = 21;
            // 
            // lblGuidance
            // 
            lblGuidance.AutoSize = true;
            lblGuidance.Location = new Point(23, 735);
            lblGuidance.Name = "lblGuidance";
            lblGuidance.Size = new Size(85, 20);
            lblGuidance.TabIndex = 22;
            lblGuidance.Text = "Orientação:";
            // 
            // txtGuidance
            // 
            txtGuidance.Location = new Point(137, 735);
            txtGuidance.Margin = new Padding(3, 4, 3, 4);
            txtGuidance.Multiline = true;
            txtGuidance.Name = "txtGuidance";
            txtGuidance.Size = new Size(457, 105);
            txtGuidance.TabIndex = 23;
            // 
            // btnSalvar
            // 
            btnSalvar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnSalvar.Location = new Point(137, 848);
            btnSalvar.Margin = new Padding(3, 4, 3, 4);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(171, 53);
            btnSalvar.TabIndex = 99;
            btnSalvar.Text = "Salvar";
            btnSalvar.Click += btnSalvar_Click;
            // 
            // lblSector
            // 
            lblSector.AutoSize = true;
            lblSector.Location = new Point(23, 573);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(47, 20);
            lblSector.TabIndex = 20;
            lblSector.Text = "Setor:";
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.Location = new Point(137, 570);
            cmbSector.Margin = new Padding(3, 4, 3, 4);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(228, 28);
            cmbSector.TabIndex = 21;
            // 
            // FormFollowUp
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 933);
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
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormFollowUp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cadastro de Acompanhamento";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
