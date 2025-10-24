namespace TeamOps.UI.Forms
{
    partial class FormLocals
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvLocals;
        private System.Windows.Forms.Label lblNamePt;
        private System.Windows.Forms.TextBox txtNamePt;
        private System.Windows.Forms.Label lblNameJp;
        private System.Windows.Forms.TextBox txtNameJp;
        private System.Windows.Forms.Label lblSector;
        private System.Windows.Forms.ComboBox cmbSector;
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
            this.dgvLocals = new System.Windows.Forms.DataGridView();
            this.lblNamePt = new System.Windows.Forms.Label();
            this.txtNamePt = new System.Windows.Forms.TextBox();
            this.lblNameJp = new System.Windows.Forms.Label();
            this.txtNameJp = new System.Windows.Forms.TextBox();
            this.lblSector = new System.Windows.Forms.Label();
            this.cmbSector = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocals)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvLocals
            // 
            this.dgvLocals.AllowUserToAddRows = false;
            this.dgvLocals.AllowUserToDeleteRows = false;
            this.dgvLocals.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLocals.Location = new System.Drawing.Point(20, 20);
            this.dgvLocals.MultiSelect = false;
            this.dgvLocals.Name = "dgvLocals";
            this.dgvLocals.ReadOnly = true;
            this.dgvLocals.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLocals.Size = new System.Drawing.Size(500, 200);
            this.dgvLocals.TabIndex = 0;
            this.dgvLocals.SelectionChanged += new System.EventHandler(this.dgvLocals_SelectionChanged);
            // 
            // lblNamePt
            // 
            this.lblNamePt.Location = new System.Drawing.Point(20, 240);
            this.lblNamePt.Name = "lblNamePt";
            this.lblNamePt.Size = new System.Drawing.Size(80, 23);
            this.lblNamePt.Text = "Nome (PT):";
            // 
            // txtNamePt
            // 
            this.txtNamePt.Location = new System.Drawing.Point(110, 237);
            this.txtNamePt.Size = new System.Drawing.Size(200, 23);
            // 
            // lblNameJp
            // 
            this.lblNameJp.Location = new System.Drawing.Point(20, 280);
            this.lblNameJp.Name = "lblNameJp";
            this.lblNameJp.Size = new System.Drawing.Size(80, 23);
            this.lblNameJp.Text = "Nome (JP):";
            // 
            // txtNameJp
            // 
            this.txtNameJp.Location = new System.Drawing.Point(110, 277);
            this.txtNameJp.Size = new System.Drawing.Size(200, 23);
            // 
            // lblSector
            // 
            this.lblSector.Location = new System.Drawing.Point(20, 320);
            this.lblSector.Name = "lblSector";
            this.lblSector.Size = new System.Drawing.Size(80, 23);
            this.lblSector.Text = "Setor:";
            // 
            // cmbSector
            // 
            this.cmbSector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSector.Location = new System.Drawing.Point(110, 317);
            this.cmbSector.Size = new System.Drawing.Size(200, 23);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(350, 237);
            this.btnAdd.Size = new System.Drawing.Size(80, 30);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(350, 277);
            this.btnUpdate.Size = new System.Drawing.Size(80, 30);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(350, 317);
            this.btnDelete.Size = new System.Drawing.Size(80, 30);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // FormLocals
            // 
            this.ClientSize = new System.Drawing.Size(550, 380);
            this.Controls.Add(this.dgvLocals);
            this.Controls.Add(this.lblNamePt);
            this.Controls.Add(this.txtNamePt);
            this.Controls.Add(this.lblNameJp);
            this.Controls.Add(this.txtNameJp);
            this.Controls.Add(this.lblSector);
            this.Controls.Add(this.cmbSector);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Locals";
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocals)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
