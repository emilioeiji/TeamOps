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
            // 
            // lblInicial
            // 
            lblInicial.AutoSize = true;
            lblInicial.Location = new Point(20, 20);
            lblInicial.Name = "lblInicial";
            lblInicial.Size = new Size(143, 20);
            lblInicial.TabIndex = 0;
            lblInicial.Text = "Data inicial / 開始日";
            // 
            // lblFinal
            // 
            lblFinal.AutoSize = true;
            lblFinal.Location = new Point(385, 20);
            lblFinal.Name = "lblFinal";
            lblFinal.Size = new Size(132, 20);
            lblFinal.TabIndex = 2;
            lblFinal.Text = "Data final / 終了日";
            // 
            // dtInicial
            // 
            dtInicial.Location = new Point(169, 15);
            dtInicial.Name = "dtInicial";
            dtInicial.Size = new Size(200, 27);
            dtInicial.TabIndex = 1;
            // 
            // dtFinal
            // 
            dtFinal.Location = new Point(523, 15);
            dtFinal.Name = "dtFinal";
            dtFinal.Size = new Size(200, 27);
            dtFinal.TabIndex = 3;
            // 
            // btnFiltrar
            // 
            btnFiltrar.Location = new Point(743, 14);
            btnFiltrar.Name = "btnFiltrar";
            btnFiltrar.Size = new Size(100, 30);
            btnFiltrar.TabIndex = 4;
            btnFiltrar.Text = "Filtrar / 検索";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 29;
            grid.Location = new Point(20, 60);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowHeadersWidth = 51;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new Size(900, 500);
            grid.TabIndex = 5;
            // 
            // FormHikitsuguiLeaderRead
            // 
            ClientSize = new Size(938, 580);
            Controls.Add(lblInicial);
            Controls.Add(dtInicial);
            Controls.Add(lblFinal);
            Controls.Add(dtFinal);
            Controls.Add(btnFiltrar);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormHikitsuguiLeaderRead";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Leitura de Hikitsugui (Líder) / 引継ぎ確認（リーダー）";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
