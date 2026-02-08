namespace TeamOps.UI.Forms
{
    partial class FormHikitsugui
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lstAnexos;
        private System.Windows.Forms.Button btnBold;
        private System.Windows.Forms.Button btnItalic;
        private System.Windows.Forms.Button btnUnderline;
        private System.Windows.Forms.Button btnBullet;
        private System.Windows.Forms.Button btnNumbered;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblShift = new Label();
            txtShift = new TextBox();
            lblCreator = new Label();
            txtCreator = new TextBox();
            lblDate = new Label();
            txtDate = new TextBox();
            lblCategoria = new Label();
            cboCategoria = new ComboBox();
            lblEquipamento = new Label();
            cboEquipamento = new ComboBox();
            lblLocal = new Label();
            cboLocal = new ComboBox();
            chkLider = new CheckBox();
            chkOperador = new CheckBox();
            lblDescricao = new Label();
            txtDescricao = new RichTextBox();
            lblAnexoTitulo = new Label();
            lblAnexo = new Label();
            btnSelecionarAnexo = new Button();
            btnSalvar = new Button();
            btnCancelar = new Button();
            lstAnexos = new ListBox();
            btnBold = new Button();
            btnItalic = new Button();
            btnUnderline = new Button();
            btnBullet = new Button();
            btnNumbered = new Button();
            lblSector = new Label();
            cboSector = new ComboBox();
            SuspendLayout();
            // 
            // lblShift
            // 
            lblShift.AutoSize = true;
            lblShift.Location = new Point(20, 20);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(76, 15);
            lblShift.TabIndex = 1;
            lblShift.Text = "Turno / シフト";
            // 
            // txtShift
            // 
            txtShift.Location = new Point(169, 17);
            txtShift.Name = "txtShift";
            txtShift.ReadOnly = true;
            txtShift.Size = new Size(250, 23);
            txtShift.TabIndex = 2;
            // 
            // lblCreator
            // 
            lblCreator.AutoSize = true;
            lblCreator.Location = new Point(20, 55);
            lblCreator.Name = "lblCreator";
            lblCreator.Size = new Size(93, 15);
            lblCreator.TabIndex = 3;
            lblCreator.Text = "Criador / 作成者";
            // 
            // txtCreator
            // 
            txtCreator.Location = new Point(169, 52);
            txtCreator.Name = "txtCreator";
            txtCreator.ReadOnly = true;
            txtCreator.Size = new Size(250, 23);
            txtCreator.TabIndex = 4;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Location = new Point(20, 90);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(66, 15);
            lblDate.TabIndex = 5;
            lblDate.Text = "Data / 日付";
            // 
            // txtDate
            // 
            txtDate.Location = new Point(169, 87);
            txtDate.Name = "txtDate";
            txtDate.ReadOnly = true;
            txtDate.Size = new Size(250, 23);
            txtDate.TabIndex = 6;
            // 
            // lblCategoria
            // 
            lblCategoria.AutoSize = true;
            lblCategoria.Location = new Point(20, 135);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.Size = new Size(103, 15);
            lblCategoria.TabIndex = 7;
            lblCategoria.Text = "Categoria / カテゴリ";
            // 
            // cboCategoria
            // 
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Location = new Point(169, 132);
            cboCategoria.Name = "cboCategoria";
            cboCategoria.Size = new Size(250, 23);
            cboCategoria.TabIndex = 8;
            // 
            // lblEquipamento
            // 
            lblEquipamento.AutoSize = true;
            lblEquipamento.Location = new Point(20, 170);
            lblEquipamento.Name = "lblEquipamento";
            lblEquipamento.Size = new Size(113, 15);
            lblEquipamento.TabIndex = 9;
            lblEquipamento.Text = "Equipamento / 設備";
            // 
            // cboEquipamento
            // 
            cboEquipamento.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEquipamento.Location = new Point(169, 167);
            cboEquipamento.Name = "cboEquipamento";
            cboEquipamento.Size = new Size(250, 23);
            cboEquipamento.TabIndex = 10;
            // 
            // lblLocal
            // 
            lblLocal.AutoSize = true;
            lblLocal.Location = new Point(20, 205);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(70, 15);
            lblLocal.TabIndex = 11;
            lblLocal.Text = "Local / 場所";
            // 
            // cboLocal
            // 
            cboLocal.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocal.Location = new Point(169, 202);
            cboLocal.Name = "cboLocal";
            cboLocal.Size = new Size(250, 23);
            cboLocal.TabIndex = 12;
            // 
            // chkLider
            // 
            chkLider.AutoSize = true;
            chkLider.Location = new Point(169, 280);
            chkLider.Name = "chkLider";
            chkLider.Size = new Size(133, 19);
            chkLider.TabIndex = 13;
            chkLider.Text = "Para líderes / リーダー";
            // 
            // chkOperador
            // 
            chkOperador.AutoSize = true;
            chkOperador.Location = new Point(169, 305);
            chkOperador.Name = "chkOperador";
            chkOperador.Size = new Size(158, 19);
            chkOperador.TabIndex = 14;
            chkOperador.Text = "Para operadores / 作業者";
            // 
            // lblDescricao
            // 
            lblDescricao.AutoSize = true;
            lblDescricao.Location = new Point(20, 370);
            lblDescricao.Name = "lblDescricao";
            lblDescricao.Size = new Size(93, 15);
            lblDescricao.TabIndex = 15;
            lblDescricao.Text = "Descrição / 内容";
            // 
            // txtDescricao
            // 
            txtDescricao.Font = new Font("Segoe UI", 10F);
            txtDescricao.Location = new Point(169, 367);
            txtDescricao.Name = "txtDescricao";
            txtDescricao.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtDescricao.Size = new Size(350, 120);
            txtDescricao.TabIndex = 16;
            txtDescricao.Text = "";
            // 
            // lblAnexoTitulo
            // 
            lblAnexoTitulo.AutoSize = true;
            lblAnexoTitulo.Location = new Point(20, 505);
            lblAnexoTitulo.Name = "lblAnexoTitulo";
            lblAnexoTitulo.Size = new Size(109, 15);
            lblAnexoTitulo.TabIndex = 17;
            lblAnexoTitulo.Text = "Anexo / 添付ファイル";
            // 
            // lblAnexo
            // 
            lblAnexo.AutoSize = true;
            lblAnexo.Location = new Point(169, 505);
            lblAnexo.Name = "lblAnexo";
            lblAnexo.Size = new Size(103, 15);
            lblAnexo.TabIndex = 18;
            lblAnexo.Text = "(nenhum arquivo)";
            // 
            // btnSelecionarAnexo
            // 
            btnSelecionarAnexo.Location = new Point(169, 530);
            btnSelecionarAnexo.Name = "btnSelecionarAnexo";
            btnSelecionarAnexo.Size = new Size(150, 30);
            btnSelecionarAnexo.TabIndex = 19;
            btnSelecionarAnexo.Text = "Selecionar / 選択";
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(269, 691);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(120, 35);
            btnSalvar.TabIndex = 20;
            btnSalvar.Text = "Salvar / 保存";
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(399, 691);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(120, 35);
            btnCancelar.TabIndex = 21;
            btnCancelar.Text = "Cancelar / キャンセル";
            // 
            // lstAnexos
            // 
            lstAnexos.Font = new Font("Segoe UI", 10F);
            lstAnexos.HorizontalScrollbar = true;
            lstAnexos.Location = new Point(169, 566);
            lstAnexos.Name = "lstAnexos";
            lstAnexos.Size = new Size(350, 106);
            lstAnexos.TabIndex = 0;
            // 
            // btnBold
            // 
            btnBold.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBold.Location = new Point(169, 333);
            btnBold.Name = "btnBold";
            btnBold.Size = new Size(30, 28);
            btnBold.TabIndex = 0;
            btnBold.Text = "B";
            // 
            // btnItalic
            // 
            btnItalic.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            btnItalic.Location = new Point(205, 333);
            btnItalic.Name = "btnItalic";
            btnItalic.Size = new Size(30, 28);
            btnItalic.TabIndex = 1;
            btnItalic.Text = "I";
            // 
            // btnUnderline
            // 
            btnUnderline.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            btnUnderline.Location = new Point(241, 333);
            btnUnderline.Name = "btnUnderline";
            btnUnderline.Size = new Size(30, 28);
            btnUnderline.TabIndex = 2;
            btnUnderline.Text = "U";
            // 
            // btnBullet
            // 
            btnBullet.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBullet.Location = new Point(277, 333);
            btnBullet.Name = "btnBullet";
            btnBullet.Size = new Size(30, 28);
            btnBullet.TabIndex = 3;
            btnBullet.Text = "•";
            // 
            // btnNumbered
            // 
            btnNumbered.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNumbered.Location = new Point(313, 333);
            btnNumbered.Name = "btnNumbered";
            btnNumbered.Size = new Size(40, 28);
            btnNumbered.TabIndex = 4;
            btnNumbered.Text = "1.";
            // 
            // lblSector
            // 
            lblSector.AutoSize = true;
            lblSector.Location = new Point(20, 240);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(81, 15);
            lblSector.TabIndex = 13;
            lblSector.Text = "Setor / セクター";
            // 
            // cboSector
            // 
            cboSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSector.Location = new Point(169, 237);
            cboSector.Name = "cboSector";
            cboSector.Size = new Size(250, 23);
            cboSector.TabIndex = 14;
            // 
            // FormHikitsugui
            // 
            ClientSize = new Size(540, 743);
            Controls.Add(btnBold);
            Controls.Add(btnItalic);
            Controls.Add(btnUnderline);
            Controls.Add(btnBullet);
            Controls.Add(btnNumbered);
            Controls.Add(lstAnexos);
            Controls.Add(lblShift);
            Controls.Add(txtShift);
            Controls.Add(lblCreator);
            Controls.Add(txtCreator);
            Controls.Add(lblDate);
            Controls.Add(txtDate);
            Controls.Add(lblCategoria);
            Controls.Add(cboCategoria);
            Controls.Add(lblEquipamento);
            Controls.Add(cboEquipamento);
            Controls.Add(lblLocal);
            Controls.Add(cboLocal);
            Controls.Add(lblSector);
            Controls.Add(cboSector);
            Controls.Add(chkLider);
            Controls.Add(chkOperador);
            Controls.Add(lblDescricao);
            Controls.Add(txtDescricao);
            Controls.Add(lblAnexoTitulo);
            Controls.Add(lblAnexo);
            Controls.Add(btnSelecionarAnexo);
            Controls.Add(btnSalvar);
            Controls.Add(btnCancelar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormHikitsugui";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Registro de Hikitsugui / 引継ぎ登録";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblShift;
        private System.Windows.Forms.TextBox txtShift;
        private System.Windows.Forms.Label lblCreator;
        private System.Windows.Forms.TextBox txtCreator;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.TextBox txtDate;

        private System.Windows.Forms.Label lblCategoria;
        private System.Windows.Forms.ComboBox cboCategoria;

        private System.Windows.Forms.Label lblEquipamento;
        private System.Windows.Forms.ComboBox cboEquipamento;

        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.ComboBox cboLocal;

        private System.Windows.Forms.Label lblSector;
        private System.Windows.Forms.ComboBox cboSector;

        private System.Windows.Forms.CheckBox chkLider;
        private System.Windows.Forms.CheckBox chkOperador;

        private System.Windows.Forms.Label lblDescricao;
        private System.Windows.Forms.RichTextBox txtDescricao;

        private System.Windows.Forms.Label lblAnexoTitulo;
        private System.Windows.Forms.Label lblAnexo;
        private System.Windows.Forms.Button btnSelecionarAnexo;

        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
