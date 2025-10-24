namespace TeamOps.UI.Forms
{
    partial class FormAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnShifts;
        private System.Windows.Forms.Button btnGroups;
        private System.Windows.Forms.Button btnSectors;
        private System.Windows.Forms.Button btnLocals;
        private System.Windows.Forms.Button btnEquipments;
        private System.Windows.Forms.Button btnFollowUpReasons;
        private System.Windows.Forms.Button btnFollowUpTypes;

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
            this.btnLocals = new System.Windows.Forms.Button();
            this.btnEquipments = new System.Windows.Forms.Button();
            this.btnFollowUpReasons = new System.Windows.Forms.Button();
            this.btnFollowUpTypes = new System.Windows.Forms.Button();
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
            // btnLocals
            // 
            this.btnLocals.Text = "Manage Locals";
            this.btnLocals.Location = new System.Drawing.Point(50, 240);
            this.btnLocals.Size = new System.Drawing.Size(200, 50);
            this.btnLocals.Click += new System.EventHandler(this.btnLocals_Click);
            // 
            // btnEquipments
            // 
            this.btnEquipments.Text = "Manage Equipments";
            this.btnEquipments.Location = new System.Drawing.Point(50, 310);
            this.btnEquipments.Size = new System.Drawing.Size(200, 50);
            this.btnEquipments.Click += new System.EventHandler(this.btnEquipments_Click);
            // 
            // btnFollowUpReasons
            // 
            this.btnFollowUpReasons.Text = "Manage FollowUp Reasons";
            this.btnFollowUpReasons.Location = new System.Drawing.Point(50, 380);
            this.btnFollowUpReasons.Size = new System.Drawing.Size(200, 50);
            this.btnFollowUpReasons.Click += new System.EventHandler(this.btnFollowUpReasons_Click);
            // 
            // btnFollowUpTypes
            // 
            this.btnFollowUpTypes.Text = "Manage FollowUp Types";
            this.btnFollowUpTypes.Location = new System.Drawing.Point(50, 450);
            this.btnFollowUpTypes.Size = new System.Drawing.Size(200, 50);
            this.btnFollowUpTypes.Click += new System.EventHandler(this.btnFollowUpTypes_Click);
            // 
            // FormAdmin
            // 
            this.ClientSize = new System.Drawing.Size(320, 540);
            this.Controls.Add(this.btnShifts);
            this.Controls.Add(this.btnGroups);
            this.Controls.Add(this.btnSectors);
            this.Controls.Add(this.btnLocals);
            this.Controls.Add(this.btnEquipments);
            this.Controls.Add(this.btnFollowUpReasons);
            this.Controls.Add(this.btnFollowUpTypes);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Admin Panel";
            this.ResumeLayout(false);
        }
    }
}
