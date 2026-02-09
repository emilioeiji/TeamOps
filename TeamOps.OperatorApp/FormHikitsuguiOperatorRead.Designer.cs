namespace TeamOps.OperatorApp
{
    partial class FormHikitsuguiOperatorRead
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblFJ;
        private TextBox txtFJ;
        private Label lblNome;

        private Label lblLocal;
        private ComboBox cboLocal;

        private Label lblInicial;
        private Label lblFinal;
        private DateTimePicker dtInicial;
        private DateTimePicker dtFinal;

        private Button btnFiltrar;
        private DataGridView grid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblFJ = new Label();
            txtFJ = new TextBox();
            lblNome = new Label();
            lblLocal = new Label();
            cboLocal = new ComboBox();
            lblInicial = new Label();
            lblFinal = new Label();
            dtInicial = new DateTimePicker();
            dtFinal = new DateTimePicker();
            btnFiltrar = new Button();
            grid = new DataGridView();
            colLer = new DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblFJ
            // 
            lblFJ.Location = new Point(20, 20);
            lblFJ.Name = "lblFJ";
            lblFJ.Size = new Size(100, 23);
            lblFJ.TabIndex = 0;
            lblFJ.Text = "FJ / 社員番号";
            // 
            // txtFJ
            // 
            txtFJ.Location = new Point(150, 17);
            txtFJ.Name = "txtFJ";
            txtFJ.Size = new Size(120, 27);
            txtFJ.TabIndex = 1;
            // 
            // lblNome
            // 
            lblNome.Location = new Point(290, 20);
            lblNome.Name = "lblNome";
            lblNome.Size = new Size(300, 23);
            lblNome.TabIndex = 2;
            lblNome.Text = "Nome:";
            // 
            // lblLocal
            // 
            lblLocal.Location = new Point(20, 60);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(100, 23);
            lblLocal.TabIndex = 3;
            lblLocal.Text = "Local / 場所";
            // 
            // cboLocal
            // 
            cboLocal.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocal.Location = new Point(150, 57);
            cboLocal.Name = "cboLocal";
            cboLocal.Size = new Size(200, 28);
            cboLocal.TabIndex = 4;
            // 
            // lblInicial
            // 
            lblInicial.Location = new Point(20, 100);
            lblInicial.Name = "lblInicial";
            lblInicial.Size = new Size(100, 23);
            lblInicial.TabIndex = 5;
            lblInicial.Text = "Data inicial / 開始日";
            // 
            // lblFinal
            // 
            lblFinal.Location = new Point(380, 100);
            lblFinal.Name = "lblFinal";
            lblFinal.Size = new Size(100, 23);
            lblFinal.TabIndex = 7;
            lblFinal.Text = "Data final / 終了日";
            // 
            // dtInicial
            // 
            dtInicial.Location = new Point(150, 97);
            dtInicial.Name = "dtInicial";
            dtInicial.Size = new Size(200, 27);
            dtInicial.TabIndex = 6;
            // 
            // dtFinal
            // 
            dtFinal.Location = new Point(500, 97);
            dtFinal.Name = "dtFinal";
            dtFinal.Size = new Size(200, 27);
            dtFinal.TabIndex = 8;
            // 
            // btnFiltrar
            // 
            btnFiltrar.Location = new Point(750, 95);
            btnFiltrar.Name = "btnFiltrar";
            btnFiltrar.Size = new Size(100, 28);
            btnFiltrar.TabIndex = 9;
            btnFiltrar.Text = "Filtrar / 検索";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ColumnHeadersHeight = 29;
            grid.Columns.AddRange(new DataGridViewColumn[] { colLer });
            grid.Location = new Point(20, 150);
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowHeadersWidth = 51;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new Size(900, 450);
            grid.TabIndex = 10;
            // 
            // colLer
            // 
            colLer.HeaderText = "Ler";
            colLer.MinimumWidth = 6;
            colLer.Name = "colLer";
            colLer.ReadOnly = true;
            colLer.Text = "✔";
            colLer.UseColumnTextForButtonValue = true;
            colLer.Width = 60;
            // 
            // FormHikitsuguiOperatorRead
            // 
            ClientSize = new Size(950, 630);
            Controls.Add(lblFJ);
            Controls.Add(txtFJ);
            Controls.Add(lblNome);
            Controls.Add(lblLocal);
            Controls.Add(cboLocal);
            Controls.Add(lblInicial);
            Controls.Add(dtInicial);
            Controls.Add(lblFinal);
            Controls.Add(dtFinal);
            Controls.Add(btnFiltrar);
            Controls.Add(grid);
            Name = "FormHikitsuguiOperatorRead";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Leitura de Hikitsugui (Operador)";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private DataGridViewButtonColumn colLer;
    }
}
