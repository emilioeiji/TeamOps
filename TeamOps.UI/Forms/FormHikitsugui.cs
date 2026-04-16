// Project: TeamOps.UI
// File: Forms/FormHikitsugui.cs

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;
using System.Configuration;

namespace TeamOps.UI.Forms
{
    public partial class FormHikitsugui : Form
    {
        private readonly Shift _currentShift;
        private readonly Operator _currentOperator;

        private readonly HikitsuguiRepository _hikitsuguiRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly EquipmentRepository _equipmentRepository;
        private readonly LocalRepository _localRepository;
        private readonly SectorRepository _sectorRepository;
        private readonly HikitsuguiAttachmentRepository _attachmentRepository;

        private List<string> _selectedAttachmentPaths = new();

        public FormHikitsugui(
            Shift currentShift,
            Operator currentOperator,
            HikitsuguiRepository hikitsuguiRepository,
            CategoryRepository categoryRepository,
            EquipmentRepository equipmentRepository,
            LocalRepository localRepository,
            SectorRepository sectorRepository)
        {
            InitializeComponent();

            _currentShift = currentShift;
            _currentOperator = currentOperator;
            _hikitsuguiRepository = hikitsuguiRepository;
            _categoryRepository = categoryRepository;
            _equipmentRepository = equipmentRepository;
            _localRepository = localRepository;
            _sectorRepository = sectorRepository;

            // 🔹 Novo: repositório de anexos
            _attachmentRepository = new HikitsuguiAttachmentRepository(Program.ConnectionFactory);

            Load += FormHikitsugui_Load;
        }

        private void FormHikitsugui_Load(object? sender, EventArgs e)
        {
            CarregarCamposFixos();
            CarregarCategorias();
            CarregarEquipamentos();
            CarregarLocais();
            CarregarSectors();
            ConfigurarEventos();
            ConfigurarListaDeAnexos();
        }

        private void ConfigurarListaDeAnexos()
        {
            lstAnexos.DoubleClick += (s, ev) =>
            {
                if (lstAnexos.SelectedIndex < 0)
                    return;

                string fileName = lstAnexos.SelectedItem.ToString();
                string? fullPath = _selectedAttachmentPaths
                    .FirstOrDefault(x => Path.GetFileName(x) == fileName);

                if (fullPath != null)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
            };
        }

        private void ConfigurarEventos()
        {
            btnSelecionarAnexo.Click += btnSelecionarAnexo_Click;
            btnSalvar.Click += btnSalvar_Click;
            btnCancelar.Click += btnCancelar_Click;

            btnBold.Click += btnBold_Click;
            btnItalic.Click += btnItalic_Click;
            btnUnderline.Click += btnUnderline_Click;
            btnBullet.Click += btnBullet_Click;
            btnNumbered.Click += btnNumbered_Click;
        }

        private void CarregarCamposFixos()
        {
            txtShift.Text = _currentShift.NamePt;
            txtCreator.Text = _currentOperator.CodigoFJ;
            txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        private void CarregarCategorias()
        {
            var list = _categoryRepository.GetAll();
            cboCategoria.DataSource = list;
            cboCategoria.DisplayMember = "NamePt";
            cboCategoria.ValueMember = "Id";
            cboCategoria.SelectedIndex = -1;
        }

        private void CarregarEquipamentos()
        {
            var list = _equipmentRepository.GetAll();
            cboEquipamento.DataSource = list;
            cboEquipamento.DisplayMember = "NamePt";
            cboEquipamento.ValueMember = "Id";
            cboEquipamento.SelectedIndex = -1;
        }

        private void CarregarLocais()
        {
            var list = _localRepository.GetAll();
            cboLocal.DataSource = list;
            cboLocal.DisplayMember = "NamePt";
            cboLocal.ValueMember = "Id";
            cboLocal.SelectedIndex = -1;
        }

        private void CarregarSectors()
        {
            var list = _sectorRepository.GetAll();
            cboSector.DataSource = list;
            cboSector.DisplayMember = "NamePt";
            cboSector.ValueMember = "Id";
            cboSector.SelectedIndex = -1;
        }

        // ---------------------------------------------------------
        // SELEÇÃO DE ANEXOS (AGORA MULTIPLOS)
        // ---------------------------------------------------------
        private void btnSelecionarAnexo_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Selecionar anexos / 添付ファイル選択",
                Filter = "Todos os arquivos (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedAttachmentPaths = ofd.FileNames.ToList();
                lblAnexo.Text = $"{_selectedAttachmentPaths.Count} arquivo(s) selecionado(s)";

                lstAnexos.Items.Clear();
                foreach (var file in _selectedAttachmentPaths)
                    lstAnexos.Items.Add(Path.GetFileName(file));
            }
        }

        // ---------------------------------------------------------
        // SALVAR
        // ---------------------------------------------------------
        private void btnSalvar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;
            
            AjustarFonteAntesDeSalvar();

            txtDescricao.Text = txtDescricao.Text.Normalize();

            var entity = MontarEntidade();

            // 1) Salva o Hikitsugui sem anexos para obter o ID
            int newId = _hikitsuguiRepository.Add(entity);
            entity.Id = newId;

            // 2) Salva cada anexo individualmente
            foreach (var file in _selectedAttachmentPaths)
            {
                string destino = SalvarAnexo(newId, file);

                _attachmentRepository.Add(new HikitsuguiAttachment
                {
                    HikitsuguiId = newId,
                    FileName = Path.GetFileName(file),
                    FilePath = destino
                });
            }

            MessageBox.Show(
                "Hikitsugui registrado com sucesso.\n引継ぎが正常に登録されました。",
                "Sucesso / 完了",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ---------------------------------------------------------
        // BOTOES
        // ---------------------------------------------------------
        private void btnBold_Click(object sender, EventArgs e)
        {
            if (txtDescricao.SelectionFont != null)
            {
                var current = txtDescricao.SelectionFont;
                var newStyle = current.Style ^ FontStyle.Bold;
                txtDescricao.SelectionFont = new Font(current, newStyle);
            }
        }
        private void btnItalic_Click(object sender, EventArgs e)
        {
            if (txtDescricao.SelectionFont != null)
            {
                var current = txtDescricao.SelectionFont;
                var newStyle = current.Style ^ FontStyle.Italic;
                txtDescricao.SelectionFont = new Font(current, newStyle);
            }
        }
        private void btnUnderline_Click(object sender, EventArgs e)
        {
            if (txtDescricao.SelectionFont != null)
            {
                var current = txtDescricao.SelectionFont;
                var newStyle = current.Style ^ FontStyle.Underline;
                txtDescricao.SelectionFont = new Font(current, newStyle);
            }
        }
        private void btnBullet_Click(object sender, EventArgs e)
        {
            int start = txtDescricao.SelectionStart;
            int line = txtDescricao.GetLineFromCharIndex(start);
            int lineStart = txtDescricao.GetFirstCharIndexFromLine(line);

            txtDescricao.SelectionStart = lineStart;
            txtDescricao.SelectionLength = 0;

            txtDescricao.SelectedText = "• ";
        }
        private void btnNumbered_Click(object sender, EventArgs e)
        {
            int start = txtDescricao.SelectionStart;
            int line = txtDescricao.GetLineFromCharIndex(start);
            int lineStart = txtDescricao.GetFirstCharIndexFromLine(line);

            txtDescricao.SelectionStart = lineStart;
            txtDescricao.SelectionLength = 0;

            txtDescricao.SelectedText = $"{line + 1}. ";
        }


        // ---------------------------------------------------------
        // VALIDAÇÃO
        // ---------------------------------------------------------
        private bool ValidarCampos()
        {
            if (cboCategoria.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione uma categoria.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cboSector.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione um setor.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescricao.Text))
            {
                MessageBox.Show("Informe a descrição.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ---------------------------------------------------------
        // MONTAR ENTIDADE
        // ---------------------------------------------------------
        private Hikitsugui MontarEntidade()
        {
            return new Hikitsugui
            {
                Date = DateTime.Now,
                ShiftId = _currentShift.Id,
                CreatorCodigoFJ = _currentOperator.CodigoFJ,
                CategoryId = (int)cboCategoria.SelectedValue,
                EquipmentId = cboEquipamento.SelectedIndex >= 0 ? (int?)cboEquipamento.SelectedValue : null,
                LocalId = cboLocal.SelectedIndex >= 0 ? (int?)cboLocal.SelectedValue : null,
                SectorId = cboSector?.SelectedValue as int?,
                ForMaSv = chkForMaSv.Checked,
                ForLeaders = chkLider.Checked,
                ForOperators = chkOperador.Checked,
                Description = txtDescricao?.Rtf ?? "",
                AttachmentPath = null // agora sempre null
            };
        }

        private void AjustarFonteAntesDeSalvar()
        {
            txtDescricao.SuspendLayout();

            for (int i = 0; i < txtDescricao.TextLength; i++)
            {
                txtDescricao.Select(i, 1);

                var f = txtDescricao.SelectionFont;
                if (f != null)
                {
                    txtDescricao.SelectionFont = new Font(
                        f.FontFamily,
                        18,             // tamanho desejado
                        f.Style         // preserva negrito/itálico/sublinhado
                    );
                }
            }

            txtDescricao.Select(0, 0);
            txtDescricao.ResumeLayout();
        }

        // ---------------------------------------------------------
        // SALVAR ARQUIVOS
        // ---------------------------------------------------------
        private string SalvarAnexo(int id, string origem)
        {
            string pastaBase = ConfigurationManager.AppSettings["HikitsuguiAttachmentPath"];

            string pasta = Path.Combine(pastaBase, id.ToString());

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            string destino = Path.Combine(pasta, Path.GetFileName(origem));
            File.Copy(origem, destino, true);

            return destino;
        }
    }
}
