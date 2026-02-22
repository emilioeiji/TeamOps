using System.Diagnostics;
using System.Windows.Forms;
using TeamOps.UI;
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
        rtb.Location = new Point(0, 0);
        rtb.Name = "rtb";
        rtb.ReadOnly = true;
        rtb.Size = new Size(1585, 896);
        rtb.TabIndex = 2;
        rtb.Text = "";
        // 
        // lblAnexos
        // 
        lblAnexos.Dock = DockStyle.Top;
        lblAnexos.Location = new Point(0, 896);
        lblAnexos.Name = "lblAnexos";
        lblAnexos.Padding = new Padding(5, 5, 0, 0);
        lblAnexos.Size = new Size(1585, 25);
        lblAnexos.TabIndex = 1;
        lblAnexos.Text = "Anexos:";
        // 
        // lstAnexos
        // 
        lstAnexos.Dock = DockStyle.Fill;
        lstAnexos.Location = new Point(0, 921);
        lstAnexos.Name = "lstAnexos";
        lstAnexos.Size = new Size(1585, 97);
        lstAnexos.TabIndex = 0;
        lstAnexos.DoubleClick += lstAnexos_DoubleClick;
        // 
        // FormHikitsuguiPreview
        // 
        ClientSize = new Size(1585, 1018);
        Controls.Add(lstAnexos);
        Controls.Add(lblAnexos);
        Controls.Add(rtb);
        Name = "FormHikitsuguiPreview";
        Text = "Visualizar Hikitsugui";
        ResumeLayout(false);
    }
}

