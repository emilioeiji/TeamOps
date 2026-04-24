// Project: TeamOps.UI
// File: Forms/FormDashboardHtml.cs

using Dapper;
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
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _user = user;

            var opRepo = new OperatorRepository(Program.ConnectionFactory);
            _currentOperator = opRepo.GetByCodigoFJ(_user.CodigoFJ!)
                               ?? throw new InvalidOperationException("Operador nao encontrado.");

            var shiftRepo = new ShiftRepository(Program.ConnectionFactory);
            _currentShift = shiftRepo.GetById(_currentOperator.ShiftId)
                            ?? throw new InvalidOperationException("Turno nao encontrado.");

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

            core.NavigationCompleted += (s, e) => SendDashboard();
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var json = JsonDocument.Parse(e.WebMessageAsJson);
            var root = json.RootElement;

            var action = ReadAction(root);
            var locale = ReadString(root, "locale");

            Console.WriteLine($"[Dashboard HTML] Mensagem recebida: {action}");

            switch (action)
            {
                case "set_locale":
                    Program.SetCurrentLocale(locale);
                    SendLocale();
                    break;

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
                        OpenDialog(() => new HTMLFormPresenceLayout(
                            _factory,
                            _currentShift.Id
                        ));
                    break;

                case "open:presence_dad":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormPresenceLayout(
                            _factory,
                            _currentShift.Id
                        ));
                    break;

                case "open:followup":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormFollowUp(
                            _factory,
                            _user,
                            _currentOperator
                        ));
                    break;

                case "open:tasks":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormTasks(
                            _factory,
                            _user,
                            _currentOperator
                        ));
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
                        OpenDialog(() => new HTMLFormAdmin());
                    break;

                case "open:accesscontrol":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDeniedAsync();
                    else
                        OpenDialog(() => new HTMLFormAccessControl());
                    break;
            }
        }

        private void SendDashboard()
        {
            PostJson(new
            {
                type = "init",
                user = _user.Name,
                locale = Program.CurrentLocale,
                operatorNamePt = _currentOperator.NameRomanji,
                operatorNameJp = string.IsNullOrWhiteSpace(_currentOperator.NameNihongo)
                    ? _currentOperator.NameRomanji
                    : _currentOperator.NameNihongo,
                accessLevel = (int)_user.AccessLevel,
                shiftNamePt = _currentShift.NamePt,
                shiftNameJp = string.IsNullOrWhiteSpace(_currentShift.NameJp)
                    ? _currentShift.NamePt
                    : _currentShift.NameJp,
                dateIso = DateTime.Now.ToString("O"),
                openTasksForShift = GetOpenTasksForCurrentShift()
            });
        }

        private void SendLocale()
        {
            PostJson(new
            {
                type = "locale_changed",
                locale = Program.CurrentLocale
            });
        }

        private int GetOpenTasksForCurrentShift()
        {
            using var conn = _factory.CreateOpenConnection();

            var tasksTableExists = conn.ExecuteScalar<int>(
                @"SELECT COUNT(1)
                  FROM sqlite_master
                  WHERE type = 'table'
                    AND name = 'Tasks'"
            ) > 0;

            if (!tasksTableExists)
                return 0;

            return conn.ExecuteScalar<int>(
                @"SELECT COUNT(1)
                  FROM Tasks
                  WHERE ShiftId = @ShiftId
                    AND Status NOT IN ('completed', 'cancelled')",
                new
                {
                    ShiftId = _currentShift.Id
                }
            );
        }

        private bool HasAccess(AccessLevel level)
        {
            return _user.AccessLevel >= level;
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show(
                "Acesso negado. Permissao insuficiente.",
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

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewDashboard.CoreWebView2 != null)
                        webViewDashboard.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewDashboard.CoreWebView2 != null)
                webViewDashboard.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string ReadAction(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? "";

            if (root.ValueKind == JsonValueKind.Object)
                return ReadString(root, "action");

            return "";
        }

        private static string ReadString(JsonElement root, string name)
        {
            return root.ValueKind == JsonValueKind.Object &&
                   root.TryGetProperty(name, out var prop)
                ? prop.GetString() ?? ""
                : "";
        }
    }
}
