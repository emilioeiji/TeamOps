using System.Diagnostics;
using System.Windows.Forms;
using TeamOps.OperatorApp;
using static System.Net.Mime.MediaTypeNames;

partial class FormHikitsuguiPreview
{
    private RichTextBox rtb;
    private Label lblAnexos;
    private ListBox lstAnexos;

    private void InitializeComponent()
    {
        rtb = new RichTextBox();
        lblAnexos = new Label();
        lstAnexos = new ListBox();
        SuspendLayout();
        // 
        // rtb
        // 
        rtb.Dock = DockStyle.Top;
        rtb.Font = new System.Drawing.Font("Yu Gothic UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 128);
        rtb.Location = new Point(0, 0);
        rtb.Name = "rtb";
        rtb.ReadOnly = true;
        rtb.Size = new Size(1265, 827);
        rtb.TabIndex = 2;
        rtb.Text = "";
        // 
        // lblAnexos
        // 
        lblAnexos.Dock = DockStyle.Top;
        lblAnexos.Location = new Point(0, 827);
        lblAnexos.Name = "lblAnexos";
        lblAnexos.Padding = new Padding(5, 5, 0, 0);
        lblAnexos.Size = new Size(1265, 25);
        lblAnexos.TabIndex = 1;
        lblAnexos.Text = "Anexos:";
        // 
        // lstAnexos
        // 
        lstAnexos.Dock = DockStyle.Fill;
        lstAnexos.Location = new Point(0, 852);
        lstAnexos.Name = "lstAnexos";
        lstAnexos.Size = new Size(1265, 95);
        lstAnexos.TabIndex = 0;
        lstAnexos.DoubleClick += lstAnexos_DoubleClick;
        // 
        // FormHikitsuguiPreview
        // 
        ClientSize = new Size(1265, 947);
        Controls.Add(lstAnexos);
        Controls.Add(lblAnexos);
        Controls.Add(rtb);
        Name = "FormHikitsuguiPreview";
        Text = "Visualizar Hikitsugui";
        ResumeLayout(false);
    }
}

