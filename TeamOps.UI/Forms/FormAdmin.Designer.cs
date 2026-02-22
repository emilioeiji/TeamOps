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
        private System.Windows.Forms.Button btnMachines;
        private System.Windows.Forms.Button btnFollowUpReasons;
        private System.Windows.Forms.Button btnFollowUpTypes;
        private System.Windows.Forms.Button btnCategories;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnShifts = new Button();
            btnGroups = new Button();
            btnSectors = new Button();
            btnLocals = new Button();
            btnEquipments = new Button();
            btnMachines = new Button();
            btnFollowUpReasons = new Button();
            btnFollowUpTypes = new Button();
            btnCategories = new Button();
            SuspendLayout();
            // 
            // btnShifts
            // 
            btnShifts.Location = new Point(50, 30);
            btnShifts.Name = "btnShifts";
            btnShifts.Size = new Size(200, 50);
            btnShifts.TabIndex = 0;
            btnShifts.Text = "Manage Shifts";
            btnShifts.Click += btnShifts_Click;
            // 
            // btnGroups
            // 
            btnGroups.Location = new Point(50, 100);
            btnGroups.Name = "btnGroups";
            btnGroups.Size = new Size(200, 50);
            btnGroups.TabIndex = 1;
            btnGroups.Text = "Manage Groups";
            btnGroups.Click += btnGroups_Click;
            // 
            // btnSectors
            // 
            btnSectors.Location = new Point(50, 170);
            btnSectors.Name = "btnSectors";
            btnSectors.Size = new Size(200, 50);
            btnSectors.TabIndex = 2;
            btnSectors.Text = "Manage Sectors";
            btnSectors.Click += btnSectors_Click;
            // 
            // btnLocals
            // 
            btnLocals.Location = new Point(50, 240);
            btnLocals.Name = "btnLocals";
            btnLocals.Size = new Size(200, 50);
            btnLocals.TabIndex = 3;
            btnLocals.Text = "Manage Locals";
            btnLocals.Click += btnLocals_Click;
            // 
            // btnEquipments
            // 
            btnEquipments.Location = new Point(50, 310);
            btnEquipments.Name = "btnEquipments";
            btnEquipments.Size = new Size(200, 50);
            btnEquipments.TabIndex = 4;
            btnEquipments.Text = "Manage Equipments";
            btnEquipments.Click += btnEquipments_Click;
            // 
            // btnMachiness
            // 
            btnMachines.Location = new Point(50, 380);
            btnMachines.Name = "btnMachines";
            btnMachines.Size = new Size(200, 50);
            btnMachines.TabIndex = 4;
            btnMachines.Text = "Manage Machines";
            btnMachines.Click += btnMachines_Click;
            // 
            // btnFollowUpReasons
            // 
            btnFollowUpReasons.Location = new Point(50, 450);
            btnFollowUpReasons.Name = "btnFollowUpReasons";
            btnFollowUpReasons.Size = new Size(200, 50);
            btnFollowUpReasons.TabIndex = 5;
            btnFollowUpReasons.Text = "Manage FollowUp Reasons";
            btnFollowUpReasons.Click += btnFollowUpReasons_Click;
            // 
            // btnFollowUpTypes
            // 
            btnFollowUpTypes.Location = new Point(50, 520);
            btnFollowUpTypes.Name = "btnFollowUpTypes";
            btnFollowUpTypes.Size = new Size(200, 50);
            btnFollowUpTypes.TabIndex = 6;
            btnFollowUpTypes.Text = "Manage FollowUp Types";
            btnFollowUpTypes.Click += btnFollowUpTypes_Click;
            // 
            // btnCategories
            // 
            btnCategories.Location = new Point(50, 590);
            btnCategories.Name = "btnCategories";
            btnCategories.Size = new Size(200, 50);
            btnCategories.TabIndex = 7;
            btnCategories.Text = "Manage Categories";
            btnCategories.Click += btnCategories_Click;
            // 
            // FormAdmin
            // 
            ClientSize = new Size(320, 717);
            Controls.Add(btnShifts);
            Controls.Add(btnGroups);
            Controls.Add(btnSectors);
            Controls.Add(btnLocals);
            Controls.Add(btnEquipments);
            Controls.Add(btnMachines);
            Controls.Add(btnFollowUpReasons);
            Controls.Add(btnFollowUpTypes);
            Controls.Add(btnCategories);
            Name = "FormAdmin";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Admin Panel";
            ResumeLayout(false);
        }
    }
}
