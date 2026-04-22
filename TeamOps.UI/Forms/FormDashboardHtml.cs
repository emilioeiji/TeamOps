// Project: TeamOps.UI
// File: Forms/FormDashboardHtml.cs

using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;

namespace TeamOps.UI.Forms
{
    public partial class FormDashboardHtml : Form
    {
        private readonly User _user;
        private readonly Operator _currentOperator;
        private readonly Shift _currentShift;
        private readonly SqliteConnectionFactory _factory;

        public FormDashboardHtml(User user)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _user = user;

            // Carrega Operator
            var opRepo = new OperatorRepository(Program.ConnectionFactory);
            _currentOperator = opRepo.GetByCodigoFJ(_user.CodigoFJ!)
                               ?? throw new InvalidOperationException("Operador não encontrado.");

            // Carrega Shift
            var shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _currentShift = shiftRepo.GetById(_currentOperator.ShiftId)
                            ?? throw new InvalidOperationException("Turno não encontrado.");

            _factory = Program.ConnectionFactory;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewDashboard.EnsureCoreWebView2Async(null);

            var core = webViewDashboard.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "dashboard"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");

            core.NavigationCompleted += (s, e) =>
            {
                var payload = JsonSerializer.Serialize(new
                {
                    type = "init",
                    user = _user.Name,
                    operatorName = _currentOperator.NameRomanji,
                    accessLevel = (int)_user.AccessLevel,
                    shiftName = _currentShift.NamePt,
                    date = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                });

                core.PostWebMessageAsJson(payload);

                _ready = true;
            };
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            Console.WriteLine($"[Dashboard HTML] Mensagem recebida: {message}");

            switch (message)
            {
                case "open:operadores":
                    OpenDialog(() => new HTMLFormOperators());
                    break;

                case "open:atribuir":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormAssignments());
                    break;

                case "open:relatorios":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormReports(
                            _currentOperator,
                            _currentShift,
                            new HikitsuguiRepository(_factory),
                            new HikitsuguiReadRepository(_factory),
                            new OperatorRepository(_factory),
                            _factory
                        ));
                    break;

                case "open:presence_gbareru":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormPresenceLayout(
                            1,
                            "G-Bareru",
                            _factory
                        ));
                    break;

                case "open:presence_dad":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormPresenceLayout(
                            2,
                            "DAD",
                            _factory
                        ));
                    break;

                case "open:followup":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormFollowUp());
                    break;

                case "open:hikitsugui":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLHikitsuguiCreate(
                            _factory,
                            _user,
                            _currentOperator
                        ));
                    break;

                case "open:hikitsugui_read":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLHikitsuguiLeaderRead(
                            _factory,
                            _user,
                            _currentOperator
                        ));
                    break;

                case "open:sobradepeca":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormSobraDePeca(
                            _factory,
                            _currentOperator
                        ));
                    break;

                case "open:pr":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormPR(
                            new PRRepository(_factory),
                            new PRCategoriaRepository(_factory),
                            new PRPrioridadeRepository(_factory),
                            new SectorRepository(_factory),
                            new OperatorRepository(_factory),
                            _currentOperator
                        ));
                    break;

                case "open:cl":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormCL(
                            new CLRepository(_factory),
                            new CLCategoriaRepository(_factory),
                            new CLPrioridadeRepository(_factory),
                            new SectorRepository(_factory),
                            new OperatorRepository(_factory),
                            _currentOperator
                        ));
                    break;

                case "open:yukyu":
                    OpenDialog(() => new FormPaidLeaveTracking(
                        _currentOperator,
                        _currentShift,
                        _factory
                    ));
                    break;

                case "open:admin":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new FormAdmin());
                    break;

                case "open:accesscontrol":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormAccessControl());
                    break;
            }
        }

        private bool _ready = false;

        private bool HasAccess(AccessLevel level)
        {
            return _user.AccessLevel >= level;
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show(
                "Acesso negado. Permissão insuficiente.",
                "Acesso Negado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private void ShowAccessDeniedAsync()
        {
            BeginInvoke(new Action(ShowAccessDenied));
        }

        private void OpenDialog(Func<Form> factory)
        {
            BeginInvoke(new Action(() =>
            {
                using var form = factory();
                form.ShowDialog(this);
            }));
        }
    }
}
