namespace TeamOps.UI.Forms
{
    partial class FormFollowSingleReport
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblShift;
        private System.Windows.Forms.Label lblExecutor;
        private System.Windows.Forms.Label lblWitness;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.Label lblEquipment;
        private System.Windows.Forms.Label lblSector;

        private System.Windows.Forms.Label lblDescTitle;
        private System.Windows.Forms.RichTextBox rtbDescription;

        private System.Windows.Forms.Label lblGuideTitle;
        private System.Windows.Forms.RichTextBox rtbGuidance;

        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnPdf;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            panelInfo = new Panel();
            lblDate = new Label();
            lblShift = new Label();
            lblExecutor = new Label();
            lblWitness = new Label();
            lblReason = new Label();
            lblType = new Label();
            lblLocal = new Label();
            lblEquipment = new Label();
            lblSector = new Label();
            lblDescTitle = new Label();
            rtbDescription = new RichTextBox();
            lblGuideTitle = new Label();
            rtbGuidance = new RichTextBox();
            btnPrint = new Button();
            btnPdf = new Button();
            panelInfo.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Yu Gothic UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(10);
            lblTitle.Size = new Size(900, 70);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "FollowUp";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelInfo
            // 
            panelInfo.BackColor = Color.WhiteSmoke;
            panelInfo.Controls.Add(lblDate);
            panelInfo.Controls.Add(lblShift);
            panelInfo.Controls.Add(lblExecutor);
            panelInfo.Controls.Add(lblWitness);
            panelInfo.Controls.Add(lblReason);
            panelInfo.Controls.Add(lblType);
            panelInfo.Controls.Add(lblLocal);
            panelInfo.Controls.Add(lblEquipment);
            panelInfo.Controls.Add(lblSector);
            panelInfo.Location = new Point(0, 120);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(900, 250);
            panelInfo.TabIndex = 4;
            // 
            // lblDate
            // 
            lblDate.Location = new Point(0, 0);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(100, 23);
            lblDate.TabIndex = 0;
            // 
            // lblShift
            // 
            lblShift.Location = new Point(0, 0);
            lblShift.Name = "lblShift";
            lblShift.Size = new Size(100, 23);
            lblShift.TabIndex = 1;
            // 
            // lblExecutor
            // 
            lblExecutor.Location = new Point(0, 0);
            lblExecutor.Name = "lblExecutor";
            lblExecutor.Size = new Size(100, 23);
            lblExecutor.TabIndex = 2;
            // 
            // lblWitness
            // 
            lblWitness.Location = new Point(0, 0);
            lblWitness.Name = "lblWitness";
            lblWitness.Size = new Size(100, 23);
            lblWitness.TabIndex = 3;
            // 
            // lblReason
            // 
            lblReason.Location = new Point(0, 0);
            lblReason.Name = "lblReason";
            lblReason.Size = new Size(100, 23);
            lblReason.TabIndex = 4;
            // 
            // lblType
            // 
            lblType.Location = new Point(0, 0);
            lblType.Name = "lblType";
            lblType.Size = new Size(100, 23);
            lblType.TabIndex = 5;
            // 
            // lblLocal
            // 
            lblLocal.Location = new Point(0, 0);
            lblLocal.Name = "lblLocal";
            lblLocal.Size = new Size(100, 23);
            lblLocal.TabIndex = 6;
            // 
            // lblEquipment
            // 
            lblEquipment.Location = new Point(0, 0);
            lblEquipment.Name = "lblEquipment";
            lblEquipment.Size = new Size(100, 23);
            lblEquipment.TabIndex = 7;
            // 
            // lblSector
            // 
            lblSector.Location = new Point(0, 0);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(100, 23);
            lblSector.TabIndex = 8;
            // 
            // lblDescTitle
            // 
            lblDescTitle.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold);
            lblDescTitle.Location = new Point(10, 380);
            lblDescTitle.Name = "lblDescTitle";
            lblDescTitle.Size = new Size(300, 30);
            lblDescTitle.TabIndex = 3;
            lblDescTitle.Text = "Descrição / 説明";
            // 
            // rtbDescription
            // 
            rtbDescription.Font = new Font("Yu Gothic UI", 11F);
            rtbDescription.Location = new Point(10, 410);
            rtbDescription.Name = "rtbDescription";
            rtbDescription.ReadOnly = true;
            rtbDescription.Size = new Size(860, 200);
            rtbDescription.TabIndex = 2;
            rtbDescription.Text = "";
            // 
            // lblGuideTitle
            // 
            lblGuideTitle.Font = new Font("Yu Gothic UI", 12F, FontStyle.Bold);
            lblGuideTitle.Location = new Point(10, 620);
            lblGuideTitle.Name = "lblGuideTitle";
            lblGuideTitle.Size = new Size(300, 30);
            lblGuideTitle.TabIndex = 1;
            lblGuideTitle.Text = "Orientação / 指示";
            // 
            // rtbGuidance
            // 
            rtbGuidance.Font = new Font("Yu Gothic UI", 11F);
            rtbGuidance.Location = new Point(10, 650);
            rtbGuidance.Name = "rtbGuidance";
            rtbGuidance.ReadOnly = true;
            rtbGuidance.Size = new Size(860, 200);
            rtbGuidance.TabIndex = 0;
            rtbGuidance.Text = "";
            // 
            // btnPrint
            // 
            btnPrint.Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold);
            btnPrint.Location = new Point(10, 75);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(120, 35);
            btnPrint.TabIndex = 6;
            btnPrint.Text = "Imprimir";
            btnPrint.Click += btnPrint_Click;
            // 
            // btnPdf
            // 
            btnPdf.Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold);
            btnPdf.Location = new Point(140, 75);
            btnPdf.Name = "btnPdf";
            btnPdf.Size = new Size(120, 35);
            btnPdf.TabIndex = 5;
            btnPdf.Text = "Salvar PDF";
            btnPdf.Click += btnPdf_Click;
            // 
            // FormFollowSingleReport
            // 
            ClientSize = new Size(900, 900);
            Controls.Add(rtbGuidance);
            Controls.Add(lblGuideTitle);
            Controls.Add(rtbDescription);
            Controls.Add(lblDescTitle);
            Controls.Add(panelInfo);
            Controls.Add(btnPdf);
            Controls.Add(btnPrint);
            Controls.Add(lblTitle);
            Name = "FormFollowSingleReport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Relatório de FollowUp";
            panelInfo.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void ConfigureInfoLabel(System.Windows.Forms.Label lbl, string text)
        {
            lbl.AutoSize = true;
            lbl.Font = new System.Drawing.Font("Yu Gothic UI", 10F);
            lbl.Text = text;
        }

        #endregion
    }
}
