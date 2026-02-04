namespace TeamOps.UI.Forms
{
    partial class FormHikitsuguiLeaderRead
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblInicial;
        private System.Windows.Forms.Label lblFinal;
        private System.Windows.Forms.DateTimePicker dtInicial;
        private System.Windows.Forms.DateTimePicker dtFinal;
        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.DataGridView grid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblInicial = new Label();
            lblFinal = new Label();
            dtInicial = new DateTimePicker();
            dtFinal = new DateTimePicker();
            btnFiltrar = new Button();
            grid = new DataGridView();

            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();

            // lblInicial
            lblInicial.AutoSize = true;
            lblInicial.Location = new Point(20, 20);
            lblInicial.Text = "Data inicial / 開始日";

            // dtInicial
            dtInicial.Location = new Point(140, 16);
            dtInicial.Width = 200;

            // lblFinal
            lblFinal.AutoSize = true;
            lblFinal.Location = new Point(360, 20);
            lblFinal.Text = "Data final / 終了日";

            // dtFinal
            dtFinal.Location = new Point(460, 16);
            dtFinal.Width = 200;

            // btnFiltrar
            btnFiltrar.Location = new Point(680, 15);
            btnFiltrar.Size = new Size(100, 30);
            btnFiltrar.Text = "Filtrar / 検索";

            // grid
            grid.Location = new Point(20, 60);
            grid.Size = new Size(760, 420);
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Form
            ClientSize = new Size(800, 500);
            Controls.Add(lblInicial);
            Controls.Add(dtInicial);
            Controls.Add(lblFinal);
            Controls.Add(dtFinal);
            Controls.Add(btnFiltrar);
            Controls.Add(grid);

            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Leitura de Hikitsugui (Líder) / 引継ぎ確認（リーダー）";

            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
