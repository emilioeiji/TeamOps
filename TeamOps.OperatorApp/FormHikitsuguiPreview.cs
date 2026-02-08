public partial class FormHikitsuguiPreview : Form
{
    public FormHikitsuguiPreview(string content)
    {
        InitializeComponent();

        if (IsRtf(content))
            rtb.Rtf = content;
        else
            rtb.Text = content;
    }

    private bool IsRtf(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.TrimStart().StartsWith(@"{\rtf");
    }
}
