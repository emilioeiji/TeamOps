using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

partial class FormHikitsuguiPreview
{
    private RichTextBox rtb;

    private void InitializeComponent()
    {
        rtb = new RichTextBox();
        SuspendLayout();

        rtb.Dock = DockStyle.Fill;
        rtb.ReadOnly = true;

        ClientSize = new Size(600, 500);
        Controls.Add(rtb);
        Text = "Visualizar Hikitsugui";

        ResumeLayout(false);
    }
}
