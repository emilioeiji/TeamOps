// Project: TeamOps.UI
// File: Forms/FormAdmin.cs
using System;
using System.Windows.Forms;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;

namespace TeamOps.UI.Forms
{
    public partial class FormAdmin : Form
    {
        public FormAdmin()
        {
            InitializeComponent();
        }

        private void btnShifts_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<Shift>(
                new ShiftRepository(Program.ConnectionFactory), "Shifts");
            form.ShowDialog();
        }

        private void btnGroups_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<Group>(
                new GroupRepository(Program.ConnectionFactory), "Groups");
            form.ShowDialog();
        }

        private void btnSectors_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<Sector>(
                new SectorRepository(Program.ConnectionFactory), "Sectors");
            form.ShowDialog();
        }
        private void btnLocals_Click(object sender, EventArgs e)
        {
            var form = new FormLocals();
            form.ShowDialog();
        }

        private void btnEquipments_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<Equipment>(
                new EquipmentRepository(Program.ConnectionFactory), "Equipments");
            form.ShowDialog();
        }

        private void btnFollowUpReasons_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<FollowUpReason>(
                new FollowUpReasonRepository(Program.ConnectionFactory), "FollowUp Reasons");
            form.ShowDialog();
        }

        private void btnFollowUpTypes_Click(object sender, EventArgs e)
        {
            var form = new FormLookup<FollowUpType>(
                new FollowUpTypeRepository(Program.ConnectionFactory), "FollowUp Types");
            form.ShowDialog();
        }

    }
}
