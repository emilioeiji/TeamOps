#pragma warning disable IDE0290
// Project: TeamOps.UI
// File: Forms/FormHikitsugui.cs

using System;
using System.IO;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Repositories;
using TeamOps.Data.Db;

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

        private string? _selectedAttachmentPath;

        public FormHikitsugui(
    Shift currentShift,
    Operator currentOperator,
    HikitsuguiRepository hikitsuguiRepository,
    CategoryRepository categoryRepository,
    EquipmentRepository equipmentRepository,
    LocalRepository localRepository)
        {
            InitializeComponent();

            _currentShift = currentShift;
            _currentOperator = currentOperator;
            _hikitsuguiRepository = hikitsuguiRepository;
            _categoryRepository = categoryRepository;
            _equipmentRepository = equipmentRepository;
            _localRepository = localRepository;

            Load += FormHikitsugui_Load;
        }


        private void FormHikitsugui_Load(object? sender, EventArgs e)
        {
            CarregarCamposFixos();
            CarregarCategorias();
            CarregarEquipamentos();
            CarregarLocais();
            ConfigurarEventos();
        }

        private void ConfigurarEventos()
        {
            btnSelecionarAnexo.Click += btnSelecionarAnexo_Click;
            btnSalvar.Click += btnSalvar_Click;
            btnCancelar.Click += btnCancelar_Click;
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
            cboCategoria.DisplayMember = "NamePt"; // somente português
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

        private void btnSelecionarAnexo_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Selecionar anexo / 添付ファイル選択",
                Filter = "Todos os arquivos (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedAttachmentPath = ofd.FileName;
                lblAnexo.Text = Path.GetFileName(ofd.FileName);
            }
        }

        private void btnSalvar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            var entity = MontarEntidade();

            // 1) Salva sem anexo para obter o ID
            int newId = _hikitsuguiRepository.Add(entity);
            entity.Id = newId;

            // 2) Se houver anexo, salva na pasta e atualiza
            if (!string.IsNullOrWhiteSpace(_selectedAttachmentPath))
            {
                string caminho = SalvarAnexo(newId, _selectedAttachmentPath);
                entity.AttachmentPath = caminho;

                string safePath = entity.AttachmentPath ?? "";
                _hikitsuguiRepository.UpdateAttachmentPath(entity.Id, safePath);
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

        private bool ValidarCampos()
        {
            if (cboCategoria.SelectedIndex < 0)
            {
                MessageBox.Show("Selecione uma categoria.", "Aviso",
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
                ForLeaders = chkLider.Checked,
                ForOperators = chkOperador.Checked,
                Description = txtDescricao.Text.Trim(),
                AttachmentPath = null
            };
        }

        private string SalvarAnexo(int id, string origem)
        {
            string pastaBase = @"C:\TeamOps\Anexo\Hikitsugui";
            string pasta = Path.Combine(pastaBase, id.ToString());

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            string destino = Path.Combine(pasta, Path.GetFileName(origem));
            File.Copy(origem, destino, true);

            return destino;
        }
    }
}
