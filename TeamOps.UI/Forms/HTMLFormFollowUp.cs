using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormFollowUp : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _user;
        private readonly Operator _currentOperator;
        private readonly FollowUpRepository _followUpRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly OperatorRepository _operatorRepo;
        private readonly FollowUpReasonRepository _reasonRepo;
        private readonly FollowUpTypeRepository _typeRepo;
        private readonly LocalRepository _localRepo;
        private readonly EquipmentRepository _equipmentRepo;
        private readonly SectorRepository _sectorRepo;

        public HTMLFormFollowUp(
            SqliteConnectionFactory factory,
            User user,
            Operator currentOperator)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _factory = factory;
            _user = user;
            _currentOperator = currentOperator;

            _followUpRepo = new FollowUpRepository(_factory);
            _shiftRepo = new ShiftRepository(_factory);
            _operatorRepo = new OperatorRepository(_factory);
            _reasonRepo = new FollowUpReasonRepository(_factory);
            _typeRepo = new FollowUpTypeRepository(_factory);
            _localRepo = new LocalRepository(_factory);
            _equipmentRepo = new EquipmentRepository(_factory);
            _sectorRepo = new SectorRepository(_factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewFollowUp.EnsureCoreWebView2Async(null);

            var core = webViewFollowUp.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "follow-up"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.SetVirtualHostNameToFolderMapping(
                "assets",
                Path.Combine(Application.StartupPath, "Assets"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);
                if (msg == null)
                    return;

                switch (msg.action)
                {
                    case "load":
                        LoadInitial();
                        break;

                    case "save":
                        SaveFollowUp(msg);
                        break;

                    case "cancel":
                        BeginInvoke(new Action(Close));
                        break;
                }
            }
            catch (Exception ex)
            {
                PostJson(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void LoadInitial()
        {
            var shifts = _shiftRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();

            var sectors = _sectorRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();

            var reasons = _reasonRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();

            var types = _typeRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();

            var locals = _localRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt, sectorId = x.SectorId })
                .ToList();

            var equipments = _equipmentRepo.GetAll()
                .Select(x => new { id = x.Id, name = x.NamePt })
                .ToList();

            var operators = _operatorRepo.GetAll()
                .Where(x => x.Status)
                .Select(x => new
                {
                    codigoFJ = x.CodigoFJ,
                    nameRomanji = x.NameRomanji,
                    nameNihongo = x.NameNihongo,
                    shiftId = x.ShiftId,
                    sectorId = x.SectorId
                })
                .ToList();

            PostJson(new
            {
                type = "init",
                data = new
                {
                    header = new
                    {
                        title = "Cadastro de acompanhamento",
                        subtitle = "Registro rapido de erros, orientacoes e observacoes do operador."
                    },
                    lookups = new
                    {
                        shifts,
                        sectors,
                        reasons,
                        types,
                        locals,
                        equipments,
                        operators
                    },
                    defaults = new
                    {
                        date = DateTime.Today.ToString("yyyy-MM-dd"),
                        shiftId = _currentOperator.ShiftId,
                        sectorId = _currentOperator.SectorId,
                        executorCodigoFJ = _currentOperator.CodigoFJ,
                        executorName = _currentOperator.NameRomanji,
                        creatorName = _user.Name,
                        currentOperatorName = _currentOperator.NameRomanji,
                        currentOperatorCodigo = _currentOperator.CodigoFJ
                    },
                    logoUrl = "https://assets/logo_rodape.png"
                }
            });
        }

        private void SaveFollowUp(JsRequest msg)
        {
            var validationError = ValidatePayload(msg);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                PostJson(new
                {
                    type = "error",
                    message = validationError
                });
                return;
            }

            var now = DateTime.Now;
            var date = now;
            if (!string.IsNullOrWhiteSpace(msg.date) &&
                DateTime.TryParse(msg.date, out var parsedDate))
            {
                date = parsedDate.Date.Add(now.TimeOfDay);
            }

            var followUp = new FollowUp
            {
                Date = date,
                ShiftId = msg.shiftId,
                OperatorCodigoFJ = msg.operatorCodigoFJ.Trim(),
                ExecutorCodigoFJ = msg.executorCodigoFJ.Trim(),
                WitnessCodigoFJ = string.IsNullOrWhiteSpace(msg.witnessCodigoFJ)
                    ? null
                    : msg.witnessCodigoFJ.Trim(),
                ReasonId = msg.reasonId,
                TypeId = msg.typeId,
                LocalId = msg.localId,
                EquipmentId = msg.equipmentId,
                SectorId = msg.sectorId,
                Description = msg.description.Trim(),
                Guidance = msg.guidance.Trim()
            };

            var newId = _followUpRepo.Add(followUp);

            PostJson(new
            {
                type = "saved",
                data = new
                {
                    id = newId,
                    message = "Acompanhamento salvo com sucesso."
                }
            });
        }

        private static string ValidatePayload(JsRequest msg)
        {
            if (msg.shiftId <= 0)
                return "Selecione o turno.";

            if (msg.sectorId <= 0)
                return "Selecione o setor.";

            if (string.IsNullOrWhiteSpace(msg.operatorCodigoFJ))
                return "Selecione o operador.";

            if (string.IsNullOrWhiteSpace(msg.executorCodigoFJ))
                return "Selecione o executor.";

            if (msg.reasonId <= 0)
                return "Selecione o motivo.";

            if (msg.typeId <= 0)
                return "Selecione o tipo.";

            if (msg.localId <= 0)
                return "Selecione o local.";

            if (msg.equipmentId <= 0)
                return "Selecione o equipamento.";

            if (string.IsNullOrWhiteSpace(msg.description))
                return "Digite a descricao.";

            if (string.IsNullOrWhiteSpace(msg.guidance))
                return "Digite a orientacao.";

            return "";
        }

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webViewFollowUp.CoreWebView2 != null)
                        webViewFollowUp.CoreWebView2.PostWebMessageAsJson(json);
                }));
                return;
            }

            if (webViewFollowUp.CoreWebView2 != null)
                webViewFollowUp.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
