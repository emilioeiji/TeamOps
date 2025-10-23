namespace TeamOps.UI.Forms
{
    partial class FormAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnShifts;
        private System.Windows.Forms.Button btnGroups;
        private System.Windows.Forms.Button btnSectors;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnShifts = new System.Windows.Forms.Button();
            this.btnGroups = new System.Windows.Forms.Button();
            this.btnSectors = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnShifts
            // 
            this.btnShifts.Text = "Manage Shifts";
            this.btnShifts.Location = new System.Drawing.Point(50, 30);
            this.btnShifts.Size = new System.Drawing.Size(200, 50);
            this.btnShifts.Click += new System.EventHandler(this.btnShifts_Click);
            // 
            // btnGroups
            // 
            this.btnGroups.Text = "Manage Groups";
            this.btnGroups.Location = new System.Drawing.Point(50, 100);
            this.btnGroups.Size = new System.Drawing.Size(200, 50);
            this.btnGroups.Click += new System.EventHandler(this.btnGroups_Click);
            // 
            // btnSectors
            // 
            this.btnSectors.Text = "Manage Sectors";
            this.btnSectors.Location = new System.Drawing.Point(50, 170);
            this.btnSectors.Size = new System.Drawing.Size(200, 50);
            this.btnSectors.Click += new System.EventHandler(this.btnSectors_Click);
            // 
            // FormAdmin
            // 
            this.ClientSize = new System.Drawing.Size(320, 260);
            this.Controls.Add(this.btnShifts);
            this.Controls.Add(this.btnGroups);
            this.Controls.Add(this.btnSectors);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Admin Panel";
            this.ResumeLayout(false);
        }
    }
}
