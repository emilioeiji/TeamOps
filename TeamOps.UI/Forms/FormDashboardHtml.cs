// Project: TeamOps.UI
// File: Forms/FormDashboardHtml.cs

using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
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
            await webViewDashboard.EnsureCoreWebView2Async();

            webViewDashboard.CoreWebView2.WebMessageReceived += WebMessageReceived;

            string htmlPath = Path.Combine(Application.StartupPath, "ui", "dashboard", "index.html");
            webViewDashboard.CoreWebView2.Navigate(htmlPath);

            // 🔹 É AQUI que você coloca o PostWebMessageAsJson
            webViewDashboard.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                webViewDashboard.CoreWebView2.PostWebMessageAsJson($@"{{
                ""user"": ""{_user.Name}"",
                ""operatorName"": ""{_currentOperator.NameRomanji}"",
                ""accessLevel"": {_user.AccessLevel},
                ""date"": ""{DateTime.Now:dd/MM/yyyy HH:mm}""
            }}");

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
                    new FormOperators().ShowDialog();
                    break;

                case "open:atribuir":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDenied();
                    else
                        new FormAssignments().ShowDialog();
                    break;

                case "open:relatorios":
                    if (!HasAccess(AccessLevel.GL))
                        ShowAccessDenied();
                    else
                        new FormReports(
                            _currentOperator,
                            _currentShift,
                            new HikitsuguiRepository(_factory),
                            new HikitsuguiReadRepository(_factory),
                            new OperatorRepository(_factory),
                            _factory
                        ).ShowDialog();
                    break;

                case "open:followup":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormFollowUp().ShowDialog();
                    break;

                case "open:hikitsugui":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormHikitsugui(
                            _currentShift,
                            _currentOperator,
                            new HikitsuguiRepository(_factory),
                            new CategoryRepository(_factory),
                            new EquipmentRepository(_factory),
                            new LocalRepository(_factory),
                            new SectorRepository(_factory)
                        ).ShowDialog();
                    break;

                case "open:hikitsugui_read":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormHikitsuguiLeaderRead(
                            new HikitsuguiRepository(_factory),
                            new HikitsuguiReadRepository(_factory),
                            _user,
                            _currentOperator
                        ).ShowDialog();
                    break;

                case "open:sobradepeca":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormSobraDePeca().ShowDialog();
                    break;

                case "open:pr":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormPR(
                            new PRRepository(_factory),
                            new PRCategoriaRepository(_factory),
                            new PRPrioridadeRepository(_factory),
                            new SectorRepository(_factory),
                            new OperatorRepository(_factory),
                            _currentOperator
                        ).ShowDialog();
                    break;

                case "open:cl":
                    if (!HasAccess(AccessLevel.KL))
                        ShowAccessDenied();
                    else
                        new FormCL(
                            new CLRepository(_factory),
                            new CLCategoriaRepository(_factory),
                            new CLPrioridadeRepository(_factory),
                            new SectorRepository(_factory),
                            new OperatorRepository(_factory),
                            _currentOperator
                        ).ShowDialog();
                    break;

                case "open:yukyu":
                    BeginInvoke(new Action(() =>
                    {
                        using var form = new FormPaidLeaveTracking(
                            _currentOperator,
                            _currentShift,
                            _factory
                        );
                        form.ShowDialog();
                    }));
                    break;

                case "open:admin":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDenied();
                    else
                        new FormAdmin().ShowDialog();
                    break;

                case "open:accesscontrol":
                    if (!HasAccess(AccessLevel.Admin))
                        ShowAccessDenied();
                    else
                        new FormAccessControl().ShowDialog();
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
    }
}
