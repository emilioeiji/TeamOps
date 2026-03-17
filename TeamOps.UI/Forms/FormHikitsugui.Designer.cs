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
            chkForMaSv = new CheckBox();
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
            lblShift.Size = new Size(91, 20);
            lblShift.TabIndex = 1;
            lblShift.Text = "Turno / シフト";
            // 
            // txtShift
            // 
            txtShift.Location = new Point(169, 17);
            txtShift.Name = "txtShift";
            txtShift.ReadOnly = true;
            txtShift.Size = new Size(250, 27);
            txtShift.TabIndex = 2;
            // 
            // lblCreator
            // 
            lblCreator.AutoSize = true;
            lblCreator.Location = new Point(20, 55);
            lblCreator.Name = "lblCreator";
            lblCreator.Size = new Size(117, 20);
            lblCreator.TabIndex = 3;
            lblCreator.Text = "Criador / 作成者";
            // 
            // txtCreator
            // 
            txtCreator.Location = new Point(169, 52);
            txtCreator.Name = "txtCreator";
            txtCreator.ReadOnly = true;
            txtCreator.Size = new Size(250, 27);
            txtCreator.TabIndex = 4;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Location = new Point(20, 90);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(85, 20);
            lblDate.TabIndex = 5;
            lblDate.Text = "Data / 日付";
            // 
            // txtDate
            // 
            txtDate.Location = new Point(169, 87);
            txtDate.Name = "txtDate";
            txtDate.ReadOnly = true;
            txtDate.Size = new Size(250, 27);
            txtDate.TabIndex = 6;
            // 
            // lblCategoria
            // 
            lblCategoria.AutoSize = true;
            lblCategoria.Location = new Point(20, 135);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.Size = new Size(131, 20);
            lblCategoria.TabIndex = 7;
            lblCategoria.Text = "Categoria / カテゴリ";
            // 
            // cboCategoria
            // 
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Location = new Point(169, 132);
            cboCategoria.Name = "cboCategoria";
            cboCategoria.Size = new Size(250, 28);
            cboCategoria.TabIndex = 8;
            // 
            // lblEquipamento
            // 
            lblEquipamento.AutoSize = true;
            lblEquipamento.Location = new Point(20, 170);
            lblEquipamento.Name = "lblEquipamento";
            lblEquipamento.Size = new Size(142, 20);
            lblEquipamento.TabIndex = 9;
            lblEquipamento.Text = "Equipamento / 設備";
            // 
            // cboEquipamento
            // 
            cboEquipamento.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEquipamento.Location = new Point(169, 167);
            cboEquipamento.Name = "cboEquipamento";
            cboEquipamento.Size = new Size(250, 28);
            cboEquipamento.TabIndex = 10;
            // 
            // lblLocal
            // 
            lblLocal.AutoSize = true;
            lblLocal.Location = new Point(20, 205);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(88, 20);
            lblLocal.TabIndex = 11;
            lblLocal.Text = "Local / 場所";
            // 
            // cboLocal
            // 
            cboLocal.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocal.Location = new Point(169, 202);
            cboLocal.Name = "cboLocal";
            cboLocal.Size = new Size(250, 28);
            cboLocal.TabIndex = 12;
            // 
            // chkForMaSv
            // 
            chkForMaSv.AutoSize = true;
            chkForMaSv.Location = new Point(169, 277);
            chkForMaSv.Name = "chkForMaSv";
            chkForMaSv.Size = new Size(110, 24);
            chkForMaSv.TabIndex = 13;
            chkForMaSv.Text = "Para MA/SV";
            // 
            // chkLider
            // 
            chkLider.AutoSize = true;
            chkLider.Location = new Point(169, 302);
            chkLider.Name = "chkLider";
            chkLider.Size = new Size(165, 24);
            chkLider.TabIndex = 13;
            chkLider.Text = "Para líderes / リーダー";
            // 
            // chkOperador
            // 
            chkOperador.AutoSize = true;
            chkOperador.Location = new Point(169, 327);
            chkOperador.Name = "chkOperador";
            chkOperador.Size = new Size(199, 24);
            chkOperador.TabIndex = 14;
            chkOperador.Text = "Para operadores / 作業者";
            // 
            // lblDescricao
            // 
            lblDescricao.AutoSize = true;
            lblDescricao.Location = new Point(20, 395);
            lblDescricao.Name = "lblDescricao";
            lblDescricao.Size = new Size(118, 20);
            lblDescricao.TabIndex = 15;
            lblDescricao.Text = "Descrição / 内容";
            // 
            // txtDescricao
            // 
            txtDescricao.Font = new Font("Segoe UI", 10F);
            txtDescricao.Location = new Point(169, 392);
            txtDescricao.Name = "txtDescricao";
            txtDescricao.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtDescricao.Size = new Size(922, 486);
            txtDescricao.TabIndex = 16;
            txtDescricao.Text = "";
            // 
            // lblAnexoTitulo
            // 
            lblAnexoTitulo.AutoSize = true;
            lblAnexoTitulo.Location = new Point(447, 20);
            lblAnexoTitulo.Name = "lblAnexoTitulo";
            lblAnexoTitulo.Size = new Size(137, 20);
            lblAnexoTitulo.TabIndex = 17;
            lblAnexoTitulo.Text = "Anexo / 添付ファイル";
            // 
            // lblAnexo
            // 
            lblAnexo.AutoSize = true;
            lblAnexo.Location = new Point(447, 55);
            lblAnexo.Name = "lblAnexo";
            lblAnexo.Size = new Size(126, 20);
            lblAnexo.TabIndex = 18;
            lblAnexo.Text = "(nenhum arquivo)";
            // 
            // btnSelecionarAnexo
            // 
            btnSelecionarAnexo.Location = new Point(447, 87);
            btnSelecionarAnexo.Name = "btnSelecionarAnexo";
            btnSelecionarAnexo.Size = new Size(150, 30);
            btnSelecionarAnexo.TabIndex = 19;
            btnSelecionarAnexo.Text = "Selecionar / 選択";
            // 
            // btnSalvar
            // 
            btnSalvar.Location = new Point(169, 893);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new Size(120, 35);
            btnSalvar.TabIndex = 20;
            btnSalvar.Text = "Salvar / 保存";
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(299, 893);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(120, 35);
            btnCancelar.TabIndex = 21;
            btnCancelar.Text = "Cancelar / キャンセル";
            // 
            // lstAnexos
            // 
            lstAnexos.Font = new Font("Segoe UI", 10F);
            lstAnexos.HorizontalScrollbar = true;
            lstAnexos.Location = new Point(447, 132);
            lstAnexos.Name = "lstAnexos";
            lstAnexos.Size = new Size(350, 119);
            lstAnexos.TabIndex = 0;
            // 
            // btnBold
            // 
            btnBold.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBold.Location = new Point(169, 358);
            btnBold.Name = "btnBold";
            btnBold.Size = new Size(30, 28);
            btnBold.TabIndex = 0;
            btnBold.Text = "B";
            // 
            // btnItalic
            // 
            btnItalic.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            btnItalic.Location = new Point(205, 358);
            btnItalic.Name = "btnItalic";
            btnItalic.Size = new Size(30, 28);
            btnItalic.TabIndex = 1;
            btnItalic.Text = "I";
            // 
            // btnUnderline
            // 
            btnUnderline.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            btnUnderline.Location = new Point(241, 358);
            btnUnderline.Name = "btnUnderline";
            btnUnderline.Size = new Size(30, 28);
            btnUnderline.TabIndex = 2;
            btnUnderline.Text = "U";
            // 
            // btnBullet
            // 
            btnBullet.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnBullet.Location = new Point(277, 358);
            btnBullet.Name = "btnBullet";
            btnBullet.Size = new Size(30, 28);
            btnBullet.TabIndex = 3;
            btnBullet.Text = "•";
            // 
            // btnNumbered
            // 
            btnNumbered.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNumbered.Location = new Point(313, 358);
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
            lblSector.Size = new Size(102, 20);
            lblSector.TabIndex = 13;
            lblSector.Text = "Setor / セクター";
            // 
            // cboSector
            // 
            cboSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSector.Location = new Point(169, 237);
            cboSector.Name = "cboSector";
            cboSector.Size = new Size(250, 28);
            cboSector.TabIndex = 14;
            // 
            // FormHikitsugui
            // 
            ClientSize = new Size(1103, 943);
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
            Controls.Add(chkForMaSv);
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
        private System.Windows.Forms.CheckBox chkForMaSv;

        private System.Windows.Forms.Label lblDescricao;
        private System.Windows.Forms.RichTextBox txtDescricao;

        private System.Windows.Forms.Label lblAnexoTitulo;
        private System.Windows.Forms.Label lblAnexo;
        private System.Windows.Forms.Button btnSelecionarAnexo;

        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
