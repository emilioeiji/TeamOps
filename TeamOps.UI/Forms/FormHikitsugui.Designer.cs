namespace TeamOps.UI.Forms
{
    partial class FormHikitsugui
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblShift = new System.Windows.Forms.Label();
            this.txtShift = new System.Windows.Forms.TextBox();
            this.lblCreator = new System.Windows.Forms.Label();
            this.txtCreator = new System.Windows.Forms.TextBox();
            this.lblDate = new System.Windows.Forms.Label();
            this.txtDate = new System.Windows.Forms.TextBox();

            this.lblCategoria = new System.Windows.Forms.Label();
            this.cboCategoria = new System.Windows.Forms.ComboBox();

            this.lblEquipamento = new System.Windows.Forms.Label();
            this.cboEquipamento = new System.Windows.Forms.ComboBox();

            this.lblLocal = new System.Windows.Forms.Label();
            this.cboLocal = new System.Windows.Forms.ComboBox();

            this.chkLider = new System.Windows.Forms.CheckBox();
            this.chkOperador = new System.Windows.Forms.CheckBox();

            this.lblDescricao = new System.Windows.Forms.Label();
            this.txtDescricao = new System.Windows.Forms.TextBox();

            this.lblAnexoTitulo = new System.Windows.Forms.Label();
            this.lblAnexo = new System.Windows.Forms.Label();
            this.btnSelecionarAnexo = new System.Windows.Forms.Button();

            this.btnSalvar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // 
            // lblShift
            // 
            this.lblShift.AutoSize = true;
            this.lblShift.Location = new System.Drawing.Point(20, 20);
            this.lblShift.Name = "lblShift";
            this.lblShift.Size = new System.Drawing.Size(110, 15);
            this.lblShift.Text = "Turno / シフト";

            // 
            // txtShift
            // 
            this.txtShift.Location = new System.Drawing.Point(150, 17);
            this.txtShift.Name = "txtShift";
            this.txtShift.ReadOnly = true;
            this.txtShift.Size = new System.Drawing.Size(250, 23);

            // 
            // lblCreator
            // 
            this.lblCreator.AutoSize = true;
            this.lblCreator.Location = new System.Drawing.Point(20, 55);
            this.lblCreator.Name = "lblCreator";
            this.lblCreator.Size = new System.Drawing.Size(130, 15);
            this.lblCreator.Text = "Criador / 作成者";

            // 
            // txtCreator
            // 
            this.txtCreator.Location = new System.Drawing.Point(150, 52);
            this.txtCreator.Name = "txtCreator";
            this.txtCreator.ReadOnly = true;
            this.txtCreator.Size = new System.Drawing.Size(250, 23);

            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(20, 90);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(120, 15);
            this.lblDate.Text = "Data / 日付";

            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(150, 87);
            this.txtDate.Name = "txtDate";
            this.txtDate.ReadOnly = true;
            this.txtDate.Size = new System.Drawing.Size(250, 23);

            // 
            // lblCategoria
            // 
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(20, 135);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Size = new System.Drawing.Size(120, 15);
            this.lblCategoria.Text = "Categoria / カテゴリ";

            // 
            // cboCategoria
            // 
            this.cboCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategoria.Location = new System.Drawing.Point(150, 132);
            this.cboCategoria.Name = "cboCategoria";
            this.cboCategoria.Size = new System.Drawing.Size(250, 23);

            // 
            // lblEquipamento
            // 
            this.lblEquipamento.AutoSize = true;
            this.lblEquipamento.Location = new System.Drawing.Point(20, 170);
            this.lblEquipamento.Name = "lblEquipamento";
            this.lblEquipamento.Size = new System.Drawing.Size(140, 15);
            this.lblEquipamento.Text = "Equipamento / 設備";

            // 
            // cboEquipamento
            // 
            this.cboEquipamento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEquipamento.Location = new System.Drawing.Point(150, 167);
            this.cboEquipamento.Name = "cboEquipamento";
            this.cboEquipamento.Size = new System.Drawing.Size(250, 23);

            // 
            // lblLocal
            // 
            this.lblLocal.AutoSize = true;
            this.lblLocal.Location = new System.Drawing.Point(20, 205);
            this.lblLocal.Name = "lblLocal";
            this.lblLocal.Size = new System.Drawing.Size(90, 15);
            this.lblLocal.Text = "Local / 場所";

            // 
            // cboLocal
            // 
            this.cboLocal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocal.Location = new System.Drawing.Point(150, 202);
            this.cboLocal.Name = "cboLocal";
            this.cboLocal.Size = new System.Drawing.Size(250, 23);

            // 
            // chkLider
            // 
            this.chkLider.AutoSize = true;
            this.chkLider.Location = new System.Drawing.Point(150, 240);
            this.chkLider.Name = "chkLider";
            this.chkLider.Size = new System.Drawing.Size(150, 19);
            this.chkLider.Text = "Para líderes / リーダー";

            // 
            // chkOperador
            // 
            this.chkOperador.AutoSize = true;
            this.chkOperador.Location = new System.Drawing.Point(150, 265);
            this.chkOperador.Name = "chkOperador";
            this.chkOperador.Size = new System.Drawing.Size(180, 19);
            this.chkOperador.Text = "Para operadores / 作業者";

            // 
            // lblDescricao
            // 
            this.lblDescricao.AutoSize = true;
            this.lblDescricao.Location = new System.Drawing.Point(20, 305);
            this.lblDescricao.Name = "lblDescricao";
            this.lblDescricao.Size = new System.Drawing.Size(130, 15);
            this.lblDescricao.Text = "Descrição / 内容";

            // 
            // txtDescricao
            // 
            this.txtDescricao.Location = new System.Drawing.Point(150, 302);
            this.txtDescricao.Multiline = true;
            this.txtDescricao.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescricao.Name = "txtDescricao";
            this.txtDescricao.Size = new System.Drawing.Size(350, 120);

            // 
            // lblAnexoTitulo
            // 
            this.lblAnexoTitulo.AutoSize = true;
            this.lblAnexoTitulo.Location = new System.Drawing.Point(20, 440);
            this.lblAnexoTitulo.Name = "lblAnexoTitulo";
            this.lblAnexoTitulo.Size = new System.Drawing.Size(120, 15);
            this.lblAnexoTitulo.Text = "Anexo / 添付ファイル";

            // 
            // lblAnexo
            // 
            this.lblAnexo.AutoSize = true;
            this.lblAnexo.Location = new System.Drawing.Point(150, 440);
            this.lblAnexo.Name = "lblAnexo";
            this.lblAnexo.Size = new System.Drawing.Size(120, 15);
            this.lblAnexo.Text = "(nenhum arquivo)";

            // 
            // btnSelecionarAnexo
            // 
            this.btnSelecionarAnexo.Location = new System.Drawing.Point(150, 465);
            this.btnSelecionarAnexo.Name = "btnSelecionarAnexo";
            this.btnSelecionarAnexo.Size = new System.Drawing.Size(150, 30);
            this.btnSelecionarAnexo.Text = "Selecionar / 選択";

            // 
            // btnSalvar
            // 
            this.btnSalvar.Location = new System.Drawing.Point(250, 515);
            this.btnSalvar.Name = "btnSalvar";
            this.btnSalvar.Size = new System.Drawing.Size(120, 35);
            this.btnSalvar.Text = "Salvar / 保存";

            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(380, 515);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(120, 35);
            this.btnCancelar.Text = "Cancelar / キャンセル";

            // 
            // FormHikitsugui
            // 
            this.ClientSize = new System.Drawing.Size(540, 580);
            this.Controls.Add(this.lblShift);
            this.Controls.Add(this.txtShift);
            this.Controls.Add(this.lblCreator);
            this.Controls.Add(this.txtCreator);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.txtDate);

            this.Controls.Add(this.lblCategoria);
            this.Controls.Add(this.cboCategoria);

            this.Controls.Add(this.lblEquipamento);
            this.Controls.Add(this.cboEquipamento);

            this.Controls.Add(this.lblLocal);
            this.Controls.Add(this.cboLocal);

            this.Controls.Add(this.chkLider);
            this.Controls.Add(this.chkOperador);

            this.Controls.Add(this.lblDescricao);
            this.Controls.Add(this.txtDescricao);

            this.Controls.Add(this.lblAnexoTitulo);
            this.Controls.Add(this.lblAnexo);
            this.Controls.Add(this.btnSelecionarAnexo);

            this.Controls.Add(this.btnSalvar);
            this.Controls.Add(this.btnCancelar);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormHikitsugui";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Registro de Hikitsugui / 引継ぎ登録";
            this.ResumeLayout(false);
            this.PerformLayout();
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

        private System.Windows.Forms.CheckBox chkLider;
        private System.Windows.Forms.CheckBox chkOperador;

        private System.Windows.Forms.Label lblDescricao;
        private System.Windows.Forms.TextBox txtDescricao;

        private System.Windows.Forms.Label lblAnexoTitulo;
        private System.Windows.Forms.Label lblAnexo;
        private System.Windows.Forms.Button btnSelecionarAnexo;

        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
