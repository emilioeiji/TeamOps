using System.Diagnostics;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;
using TeamOps.OperatorApp;

public partial class FormHikitsuguiPreview : Form
{
    private readonly Hikitsugui _hik;
    private readonly HikitsuguiAttachmentRepository _attachRepo;
    private List<HikitsuguiAttachment> _cachedAttachments = new();

    public FormHikitsuguiPreview(Hikitsugui hik)
    {
        InitializeComponent();

        _hik = hik;
        _attachRepo = new HikitsuguiAttachmentRepository(Program.ConnectionFactory);

        if (IsRtf(hik.Description))
            rtb.Rtf = hik.Description;
        else
            rtb.Text = hik.Description;

        // AjustarFontePreview();
        CarregarAnexos();
    }
    private void CarregarAnexos()
    {
        lstAnexos.Items.Clear();

        var anexos = _attachRepo.GetByHikitsugui(_hik.Id);

        _cachedAttachments = anexos;

        foreach (var a in anexos)
            lstAnexos.Items.Add(a.FileName);
    }
    private void lstAnexos_DoubleClick(object? sender, EventArgs e)
    {
        if (lstAnexos.SelectedItem == null)
            return;

        var fileName = lstAnexos.SelectedItem.ToString();

        var anex = _cachedAttachments
            .Find(a => a.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        if (anex == null)
            return;

        var ext = Path.GetExtension(anex.FilePath).ToLower();

        // Extensões que NÃO devem ser abertas
        var naoAbriveis = new[] { ".dll", ".exe", ".bin", ".sys" };

        if (naoAbriveis.Contains(ext))
        {
            MessageBox.Show("Este tipo de arquivo não pode ser aberto diretamente.");
            return;
        }

        if (File.Exists(anex.FilePath))
        {
            Process.Start(new ProcessStartInfo(anex.FilePath)
            {
                UseShellExecute = true
            });
        }
        else
        {
            MessageBox.Show("Arquivo não encontrado.");
        }
    }

    private bool IsRtf(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.TrimStart().StartsWith(@"{\rtf");
    }

    private void AjustarFontePreview()
    {
        // Evita flicker e mantém formatação
        rtb.SuspendLayout();

        rtb.SelectAll();
        rtb.SelectionFont = new Font("Yu Gothic UI", 18);
        rtb.DeselectAll();

        rtb.ResumeLayout();
    }
}
