namespace TeamOps.UI.Forms
{
    partial class FormSobraDePeca
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.DateTimePicker dtpData;

        private System.Windows.Forms.Label lblTurno;
        private System.Windows.Forms.ComboBox cmbTurno;

        private System.Windows.Forms.Label lblLote;
        private System.Windows.Forms.TextBox txtLote;

        private System.Windows.Forms.Label lblOperador;
        private System.Windows.Forms.ComboBox cmbOperador;

        private System.Windows.Forms.Label lblTanjuu;
        private System.Windows.Forms.TextBox txtTanjuu;

        private System.Windows.Forms.Label lblPeso;
        private System.Windows.Forms.TextBox txtPeso;

        private System.Windows.Forms.Label lblQuantidade;
        private System.Windows.Forms.TextBox txtQuantidade;

        private System.Windows.Forms.Label lblMaquina;
        private System.Windows.Forms.ComboBox cmbMaquina;

        private System.Windows.Forms.Label lblShain;
        private System.Windows.Forms.ComboBox cmbShain;

        private System.Windows.Forms.Label lblObservacao;
        private System.Windows.Forms.TextBox txtObservacao;

        private System.Windows.Forms.Label lblLider;
        private System.Windows.Forms.TextBox txtLider;

        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;

        private System.Windows.Forms.DataGridView dgvSobra;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblData = new Label();
            dtpData = new DateTimePicker();
            lblTurno = new Label();
            cmbTurno = new ComboBox();
            lblLote = new Label();
            txtLote = new TextBox();
            lblOperador = new Label();
            cmbOperador = new ComboBox();
            lblTanjuu = new Label();
            txtTanjuu = new TextBox();
            lblPeso = new Label();
            txtPeso = new TextBox();
            lblQuantidade = new Label();
            txtQuantidade = new TextBox();
            lblMaquina = new Label();
            cmbMaquina = new ComboBox();
            lblShain = new Label();
            cmbShain = new ComboBox();
            lblObservacao = new Label();
            txtObservacao = new TextBox();
            lblLider = new Label();
            txtLider = new TextBox();
            btnSalvar = new Button();
            btnCancelar = new Button();
            dgvSobra = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvSobra).BeginInit();
            SuspendLayout();
            // 
            // lblData
            // 
            lblData.Location = new Point(20, 260);
            lblData.Name = "lblData";
            lblData.Size = new Size(100, 23);
            lblData.TabIndex = 1;
            lblData.Text = "Data / 日付:";
            // 
            // dtpData
            // 
            dtpData.Format = DateTimePickerFormat.Short;
            dtpData.Location = new Point(140, 257);
            dtpData.Name = "dtpData";
            dtpData.Size = new Size(200, 27);
            dtpData.TabIndex = 2;
            // 
            // lblTurno
            // 
            lblTurno.Location = new Point(358, 260);
            lblTurno.Name = "lblTurno";
            lblTurno.Size = new Size(100, 23);
            lblTurno.TabIndex = 3;
            lblTurno.Text = "Turno / シフト:";
            // 
            // cmbTurno
            // 
            cmbTurno.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTurno.Location = new Point(478, 257);
            cmbTurno.Name = "cmbTurno";
            cmbTurno.Size = new Size(200, 28);
            cmbTurno.TabIndex = 4;
            // 
            // lblLote
            // 
            lblLote.Location = new Point(691, 260);
            lblLote.Name = "lblLote";
            lblLote.Size = new Size(100, 23);
            lblLote.TabIndex = 5;
            lblLote.Text = "Lote / ロット:";
            // 
            // txtLote
            // 
            txtLote.Location = new Point(811, 257);
            txtLote.Name = "txtLote";
            txtLote.Size = new Size(200, 27);
            txtLote.TabIndex = 6;
            // 
            // lblOperador
            // 
            lblOperador.Location = new Point(20, 300);
            lblOperador.Name = "lblOperador";
            lblOperador.Size = new Size(100, 23);
            lblOperador.TabIndex = 7;
            lblOperador.Text = "Operador / 作業者:";
            // 
            // cmbOperador
            // 
            cmbOperador.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOperador.Location = new Point(140, 297);
            cmbOperador.Name = "cmbOperador";
            cmbOperador.Size = new Size(200, 28);
            cmbOperador.TabIndex = 8;
            // 
            // lblTanjuu
            // 
            lblTanjuu.Location = new Point(358, 300);
            lblTanjuu.Name = "lblTanjuu";
            lblTanjuu.Size = new Size(100, 23);
            lblTanjuu.TabIndex = 9;
            lblTanjuu.Text = "Tanjuu / 単重:";
            // 
            // txtTanjuu
            // 
            txtTanjuu.Location = new Point(478, 297);
            txtTanjuu.Name = "txtTanjuu";
            txtTanjuu.Size = new Size(200, 27);
            txtTanjuu.TabIndex = 10;
            // 
            // lblPeso
            // 
            lblPeso.Location = new Point(691, 300);
            lblPeso.Name = "lblPeso";
            lblPeso.Size = new Size(100, 23);
            lblPeso.TabIndex = 11;
            lblPeso.Text = "Peso (g) / 重量:";
            // 
            // txtPeso
            // 
            txtPeso.Location = new Point(811, 297);
            txtPeso.Name = "txtPeso";
            txtPeso.Size = new Size(200, 27);
            txtPeso.TabIndex = 12;
            // 
            // lblQuantidade
            // 
            lblQuantidade.Location = new Point(20, 340);
            lblQuantidade.Name = "lblQuantidade";
            lblQuantidade.Size = new Size(100, 23);
            lblQuantidade.TabIndex = 13;
            lblQuantidade.Text = "Qtd / 数量:";
            // 
            // txtQuantidade
            // 
            txtQuantidade.Location = new Point(140, 337);
            txtQuantidade.Name = "txtQuantidade";
            txtQuantidade.ReadOnly = true;
            txtQuantidade.Size = new Size(200, 27);
            txtQuantidade.TabIndex = 14;
            // 
            // lblMaquina
            // 
            lblMaquina.Location = new Point(358, 340);
            lblMaquina.Name = "lblMaquina";
            lblMaquina.Size = new Size(100, 23);
            lblMaquina.TabIndex = 15;
            lblMaquina.Text = "Máquina / 機械:";
            // 
            // cmbMaquina
            // 
            cmbMaquina.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaquina.Location = new Point(478, 337);
            cmbMaquina.Name = "cmbMaquina";
            cmbMaquina.Size = new Size(200, 28);
            cmbMaquina.TabIndex = 16;
            // 
            // lblShain
            // 
            lblShain.Location = new Point(691, 340);
            lblShain.Name = "lblShain";
            lblShain.Size = new Size(100, 23);
            lblShain.TabIndex = 17;
            lblShain.Text = "Shain / 社員:";
            // 
            // cmbShain
            // 
            cmbShain.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShain.Location = new Point(811, 337);
            cmbShain.Name = "cmbShain";
            cmbShain.Size = new Size(200, 28);
            cmbShain.TabIndex = 18;
            // 
            // lblObservacao
            // 
            lblObservacao.Location = new Point(20, 380);
            lblObservacao.Name = "lblObservacao";
            lblObservacao.Size = new Size(100, 23);
            lblObservacao.TabIndex = 19;
            lblObservacao.Text = "Obs / 備考:";
            // 
            // txtObservacao
            // 
            txtObservacao.Location = new Point(140, 377);
            txtObservacao.Multiline = true;
            txtObservacao.Name = "txtObservacao";
            txtObservacao.Size = new Size(871, 60);
            txtObservacao.TabIndex = 20;
            // 
            // lblLider
            // 
            lblLider.Location = new Point(691, 457);
            lblLider.Name = "lblLider";
            lblLider.Size = new Size(100, 23);
            lblLider.TabIndex = 21;
            lblLider.Text = "Líder / リーダー:";
            // 
            // txtLider
            // 
            txtLider.Location = new Point(811, 454);
            txtLider.Name = "txtLider";
            txtLider.ReadOnly = true;
            txtLider.Size = new Size(200, 27);
            txtLider.TabIndex = 22;
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(801, 487);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(100, 35);
            btnSalvar.TabIndex = 23;
            btnSalvar.Text = "Salvar";
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(911, 487);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(100, 35);
            btnCancelar.TabIndex = 24;
            btnCancelar.Text = "Cancelar";
            // 
            // dgvSobra
            // 
            dgvSobra.AllowUserToAddRows = false;
            dgvSobra.AllowUserToDeleteRows = false;
            dgvSobra.ColumnHeadersHeight = 29;
            dgvSobra.Location = new Point(20, 20);
            dgvSobra.Name = "dgvSobra";
            dgvSobra.ReadOnly = true;
            dgvSobra.RowHeadersWidth = 51;
            dgvSobra.Size = new Size(991, 220);
            dgvSobra.TabIndex = 0;
            // 
            // FormSobraDePeca
            // 
            ClientSize = new Size(1022, 534);
            Controls.Add(dgvSobra);
            Controls.Add(lblData);
            Controls.Add(dtpData);
            Controls.Add(lblTurno);
            Controls.Add(cmbTurno);
            Controls.Add(lblLote);
            Controls.Add(txtLote);
            Controls.Add(lblOperador);
            Controls.Add(cmbOperador);
            Controls.Add(lblTanjuu);
            Controls.Add(txtTanjuu);
            Controls.Add(lblPeso);
            Controls.Add(txtPeso);
            Controls.Add(lblQuantidade);
            Controls.Add(txtQuantidade);
            Controls.Add(lblMaquina);
            Controls.Add(cmbMaquina);
            Controls.Add(lblShain);
            Controls.Add(cmbShain);
            Controls.Add(lblObservacao);
            Controls.Add(txtObservacao);
            Controls.Add(lblLider);
            Controls.Add(txtLider);
            Controls.Add(btnSalvar);
            Controls.Add(btnCancelar);
            Name = "FormSobraDePeca";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sobra de Peça";
            ((System.ComponentModel.ISupportInitialize)dgvSobra).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
