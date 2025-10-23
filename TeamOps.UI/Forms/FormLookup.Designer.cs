namespace TeamOps.UI.Forms
{
    partial class FormLookup<T>
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvLookup;
        private System.Windows.Forms.Label lblNamePt;
        private System.Windows.Forms.TextBox txtNamePt;
        private System.Windows.Forms.Label lblNameJp;
        private System.Windows.Forms.TextBox txtNameJp;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvLookup = new System.Windows.Forms.DataGridView();
            this.lblNamePt = new System.Windows.Forms.Label();
            this.txtNamePt = new System.Windows.Forms.TextBox();
            this.lblNameJp = new System.Windows.Forms.Label();
            this.txtNameJp = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvLookup)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvLookup
            // 
            this.dgvLookup.AllowUserToAddRows = false;
            this.dgvLookup.AllowUserToDeleteRows = false;
            this.dgvLookup.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLookup.Location = new System.Drawing.Point(20, 20);
            this.dgvLookup.MultiSelect = false;
            this.dgvLookup.ReadOnly = true;
            this.dgvLookup.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLookup.Size = new System.Drawing.Size(400, 200);
            this.dgvLookup.SelectionChanged += new System.EventHandler(this.dgvLookup_SelectionChanged);
            // 
            // lblNamePt
            // 
            this.lblNamePt.Text = "Nome PT:";
            this.lblNamePt.Location = new System.Drawing.Point(20, 240);
            this.lblNamePt.AutoSize = true;
            // 
            // txtNamePt
            // 
            this.txtNamePt.Location = new System.Drawing.Point(100, 237);
            this.txtNamePt.Width = 200;
            // 
            // lblNameJp
            // 
            this.lblNameJp.Text = "Nome JP:";
            this.lblNameJp.Location = new System.Drawing.Point(20, 280);
            this.lblNameJp.AutoSize = true;
            // 
            // txtNameJp
            // 
            this.txtNameJp.Location = new System.Drawing.Point(100, 277);
            this.txtNameJp.Width = 200;
            // 
            // btnAdd
            // 
            this.btnAdd.Text = "Adicionar";
            this.btnAdd.Location = new System.Drawing.Point(320, 237);
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Text = "Atualizar";
            this.btnUpdate.Location = new System.Drawing.Point(320, 277);
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Text = "Excluir";
            this.btnDelete.Location = new System.Drawing.Point(320, 317);
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // FormLookup
            // 
            this.ClientSize = new System.Drawing.Size(460, 370);
            this.Controls.Add(this.dgvLookup);
            this.Controls.Add(this.lblNamePt);
            this.Controls.Add(this.txtNamePt);
            this.Controls.Add(this.lblNameJp);
            this.Controls.Add(this.txtNameJp);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Lookup";
            ((System.ComponentModel.ISupportInitialize)(this.dgvLookup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
