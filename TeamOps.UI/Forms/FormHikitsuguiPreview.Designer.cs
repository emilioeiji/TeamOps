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

        // Preview
        rtb.Dock = DockStyle.Top;
        rtb.ReadOnly = true;
        rtb.Height = 350;

        // Label
        lblAnexos.Text = "Anexos:";
        lblAnexos.Dock = DockStyle.Top;
        lblAnexos.Height = 25;
        lblAnexos.Padding = new Padding(5, 5, 0, 0);

        // ListBox
        lstAnexos.Dock = DockStyle.Fill;
        lstAnexos.DoubleClick += lstAnexos_DoubleClick;

        // Form
        ClientSize = new Size(600, 500);
        Controls.Add(lstAnexos);
        Controls.Add(lblAnexos);
        Controls.Add(rtb);
        Text = "Visualizar Hikitsugui";

        ResumeLayout(false);
    }
}

