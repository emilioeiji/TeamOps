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

            SuspendLayout();

            // FJ
            lblFJ.Text = "FJ / 社員番号";
            lblFJ.Location = new Point(20, 20);
            txtFJ.Location = new Point(150, 17);
            txtFJ.Width = 120;

            // Nome
            lblNome.Text = "Nome:";
            lblNome.Location = new Point(290, 20);
            lblNome.Width = 300;

            // Local
            lblLocal.Text = "Local / 場所";
            lblLocal.Location = new Point(20, 60);
            cboLocal.Location = new Point(150, 57);
            cboLocal.Width = 200;
            cboLocal.DropDownStyle = ComboBoxStyle.DropDownList;

            // Datas
            lblInicial.Text = "Data inicial / 開始日";
            lblInicial.Location = new Point(20, 100);
            dtInicial.Location = new Point(150, 97);

            lblFinal.Text = "Data final / 終了日";
            lblFinal.Location = new Point(380, 100);
            dtFinal.Location = new Point(500, 97);

            // Botão
            btnFiltrar.Text = "Filtrar / 検索";
            btnFiltrar.Location = new Point(750, 95);
            btnFiltrar.Width = 100;

            // Grid
            grid.Location = new Point(20, 150);
            grid.Size = new Size(900, 450);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;

            // Adiciona a coluna de leitura
            var colLer = new DataGridViewButtonColumn();
            colLer.HeaderText = "Ler";
            colLer.Text = "✔";
            colLer.UseColumnTextForButtonValue = true;
            colLer.Width = 60;
            colLer.Name = "colLer";

            grid.Columns.Add(colLer);

            // Form
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

            Text = "Leitura de Hikitsugui (Operador)";
            StartPosition = FormStartPosition.CenterScreen;

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
