namespace TeamOps.UI.Forms
{
    partial class FormPR
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblSetor;
        private ComboBox cmbSetor;

        private Label lblCategoria;
        private ComboBox cmbCategoria;

        private Label lblPrioridade;
        private ComboBox cmbPrioridade;

        private Label lblTitulo;
        private TextBox txtTitulo;

        private Label lblNomeArquivo;
        private TextBox txtNomeArquivo;

        private Label lblDataEmissao;
        private TextBox txtDataEmissao;

        private Label lblAutor;
        private TextBox txtAutor;

        private Button btnSalvar;
        private Button btnCancelar;
        private Button btnFechar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblSetor = new Label();
            cmbSetor = new ComboBox();
            lblCategoria = new Label();
            cmbCategoria = new ComboBox();
            lblPrioridade = new Label();
            cmbPrioridade = new ComboBox();
            lblTitulo = new Label();
            txtTitulo = new TextBox();
            lblNomeArquivo = new Label();
            txtNomeArquivo = new TextBox();
            lblDataEmissao = new Label();
            txtDataEmissao = new TextBox();
            lblAutor = new Label();
            txtAutor = new TextBox();
            btnSalvar = new Button();
            btnCancelar = new Button();
            btnFechar = new Button();
            SuspendLayout();
            // 
            // lblSetor
            // 
            lblSetor.AutoSize = true;
            lblSetor.Location = new Point(23, 27);
            lblSetor.Name = "lblSetor";
            lblSetor.Size = new Size(47, 20);
            lblSetor.TabIndex = 0;
            lblSetor.Text = "Setor:";
            // 
            // cmbSetor
            // 
            cmbSetor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSetor.Location = new Point(171, 24);
            cmbSetor.Margin = new Padding(3, 4, 3, 4);
            cmbSetor.Name = "cmbSetor";
            cmbSetor.Size = new Size(285, 28);
            cmbSetor.TabIndex = 1;
            // 
            // lblCategoria
            // 
            lblCategoria.AutoSize = true;
            lblCategoria.Location = new Point(23, 80);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.Size = new Size(77, 20);
            lblCategoria.TabIndex = 2;
            lblCategoria.Text = "Categoria:";
            // 
            // cmbCategoria
            // 
            cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoria.Location = new Point(171, 77);
            cmbCategoria.Margin = new Padding(3, 4, 3, 4);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(285, 28);
            cmbCategoria.TabIndex = 3;
            // 
            // lblPrioridade
            // 
            lblPrioridade.AutoSize = true;
            lblPrioridade.Location = new Point(23, 133);
            lblPrioridade.Name = "lblPrioridade";
            lblPrioridade.Size = new Size(81, 20);
            lblPrioridade.TabIndex = 4;
            lblPrioridade.Text = "Prioridade:";
            // 
            // cmbPrioridade
            // 
            cmbPrioridade.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPrioridade.Location = new Point(171, 131);
            cmbPrioridade.Margin = new Padding(3, 4, 3, 4);
            cmbPrioridade.Name = "cmbPrioridade";
            cmbPrioridade.Size = new Size(285, 28);
            cmbPrioridade.TabIndex = 5;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(23, 187);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(50, 20);
            lblTitulo.TabIndex = 6;
            lblTitulo.Text = "Título:";
            // 
            // txtTitulo
            // 
            txtTitulo.Location = new Point(171, 184);
            txtTitulo.Margin = new Padding(3, 4, 3, 4);
            txtTitulo.Name = "txtTitulo";
            txtTitulo.Size = new Size(457, 27);
            txtTitulo.TabIndex = 7;
            // 
            // lblNomeArquivo
            // 
            lblNomeArquivo.AutoSize = true;
            lblNomeArquivo.Location = new Point(23, 240);
            lblNomeArquivo.Name = "lblNomeArquivo";
            lblNomeArquivo.Size = new Size(129, 20);
            lblNomeArquivo.TabIndex = 8;
            lblNomeArquivo.Text = "Nome do arquivo:";
            // 
            // txtNomeArquivo
            // 
            txtNomeArquivo.Location = new Point(171, 237);
            txtNomeArquivo.Margin = new Padding(3, 4, 3, 4);
            txtNomeArquivo.Name = "txtNomeArquivo";
            txtNomeArquivo.ReadOnly = true;
            txtNomeArquivo.Size = new Size(457, 27);
            txtNomeArquivo.TabIndex = 9;
            // 
            // lblDataEmissao
            // 
            lblDataEmissao.AutoSize = true;
            lblDataEmissao.Location = new Point(23, 293);
            lblDataEmissao.Name = "lblDataEmissao";
            lblDataEmissao.Size = new Size(123, 20);
            lblDataEmissao.TabIndex = 10;
            lblDataEmissao.Text = "Data de emissão:";
            // 
            // txtDataEmissao
            // 
            txtDataEmissao.Location = new Point(171, 291);
            txtDataEmissao.Margin = new Padding(3, 4, 3, 4);
            txtDataEmissao.Name = "txtDataEmissao";
            txtDataEmissao.ReadOnly = true;
            txtDataEmissao.Size = new Size(171, 27);
            txtDataEmissao.TabIndex = 11;
            // 
            // lblAutor
            // 
            lblAutor.AutoSize = true;
            lblAutor.Location = new Point(23, 347);
            lblAutor.Name = "lblAutor";
            lblAutor.Size = new Size(49, 20);
            lblAutor.TabIndex = 12;
            lblAutor.Text = "Autor:";
            // 
            // txtAutor
            // 
            txtAutor.Location = new Point(171, 344);
            txtAutor.Margin = new Padding(3, 4, 3, 4);
            txtAutor.Name = "txtAutor";
            txtAutor.ReadOnly = true;
            txtAutor.Size = new Size(171, 27);
            txtAutor.TabIndex = 13;
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(171, 427);
            btnSalvar.Margin = new Padding(3, 4, 3, 4);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(114, 31);
            btnSalvar.TabIndex = 14;
            btnSalvar.Text = "Salvar";
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(297, 427);
            btnCancelar.Margin = new Padding(3, 4, 3, 4);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(114, 31);
            btnCancelar.TabIndex = 15;
            btnCancelar.Text = "Cancelar";
            // 
            // btnFechar
            // 
            btnFechar.Location = new Point(423, 427);
            btnFechar.Margin = new Padding(3, 4, 3, 4);
            btnFechar.Name = "btnFechar";
            btnFechar.Size = new Size(114, 31);
            btnFechar.TabIndex = 16;
            btnFechar.Text = "Fechar";
            // 
            // FormPR
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 507);
            Controls.Add(lblSetor);
            Controls.Add(cmbSetor);
            Controls.Add(lblCategoria);
            Controls.Add(cmbCategoria);
            Controls.Add(lblPrioridade);
            Controls.Add(cmbPrioridade);
            Controls.Add(lblTitulo);
            Controls.Add(txtTitulo);
            Controls.Add(lblNomeArquivo);
            Controls.Add(txtNomeArquivo);
            Controls.Add(lblDataEmissao);
            Controls.Add(txtDataEmissao);
            Controls.Add(lblAutor);
            Controls.Add(txtAutor);
            Controls.Add(btnSalvar);
            Controls.Add(btnCancelar);
            Controls.Add(btnFechar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormPR";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cadastro de PR";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
