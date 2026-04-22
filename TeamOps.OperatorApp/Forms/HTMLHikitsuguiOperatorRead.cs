using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamOps.Data.Repositories;

namespace TeamOps.OperatorApp.Forms
{
    public partial class HTMLHikitsuguiOperatorRead : Form
    {
        private readonly OperatorRepository _operatorRepo;
        private readonly LocalRepository _localRepo;
        private readonly HikitsuguiRepository _hikRepo;
        private readonly HikitsuguiReadRepository _readRepo;
        private readonly OperatorPresenceRepository _presenceRepo;
        private readonly HikitsuguiAttachmentRepository _attachRepo;

        public HTMLHikitsuguiOperatorRead()
        {
            InitializeComponent();
            ApplyBranding();

            _operatorRepo = new OperatorRepository(Program.ConnectionFactory);
            _localRepo = new LocalRepository(Program.ConnectionFactory);
            _hikRepo = new HikitsuguiRepository(Program.ConnectionFactory);
            _readRepo = new HikitsuguiReadRepository(Program.ConnectionFactory);
            _presenceRepo = new OperatorPresenceRepository(Program.ConnectionFactory);
            _attachRepo = new HikitsuguiAttachmentRepository(Program.ConnectionFactory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);

            var core = webView.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "assets",
                Path.Combine(Application.StartupPath, "Assets"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "hikitsugui-operator-read"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void ApplyBranding()
        {
            string iconPath = Path.Combine(Application.StartupPath, "Assets", "HikitsuguiOperatorRead.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // AGORA FUNCIONA: JsonElement
            var msg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(e.WebMessageAsJson);
            if (msg == null) return;

            string action = msg["action"].GetString();

            switch (action)
            {
                // ============================================
                // LOAD OPERATOR
                // ============================================
                case "load_operator":
                    {
                        string fj = msg["fj"].GetString();

                        Task.Run(() =>
                        {
                            var op = _operatorRepo.GetByCodigoFJ(fj);
                            if (op == null)
                            {
                                SendJson("operator_not_found", null);
                                return;
                            }

                            var locals = _localRepo.GetBySector(op.SectorId);

                            SendJson("operator_loaded", new
                            {
                                op.CodigoFJ,
                                op.NameRomanji,
                                op.SectorId,
                                op.ShiftId,
                                locals
                            });
                        });
                        break;
                    }

                // ============================================
                // REGISTER PRESENCE
                // ============================================
                case "register_presence":
                    {
                        Task.Run(() =>
                        {
                            string fj = msg["fj"].GetString();
                            int localId = msg["localId"].GetInt32();

                            var op = _operatorRepo.GetByCodigoFJ(fj);
                            if (op == null) return;

                            // MAPEAMENTO DE SETOR (SEM QUEBRAR OUTROS APPS)
                            int sector = op.SectorId;
                            if (sector < 1 || sector > 3)
                                sector = 3;

                            DateTime now = DateTime.Now;
                            DateTime dataRegistro = now;

                            if (op.ShiftId == 2)
                            {
                                if (now.TimeOfDay >= TimeSpan.Zero &&
                                    now.TimeOfDay <= new TimeSpan(8, 35, 0))
                                {
                                    dataRegistro = now.AddDays(-1);
                                }
                            }

                            _presenceRepo.RegisterPresence(
                                op.CodigoFJ,
                                sector,
                                localId,
                                op.ShiftId,
                                dataRegistro
                            );
                        });
                        break;
                    }

                // ============================================
                // FILTER
                // ============================================
                case "filter":
                    {
                        Task.Run(() =>
                        {
                            string fj = msg["fj"].GetString();
                            var op = _operatorRepo.GetByCodigoFJ(fj);
                            if (op == null) return;

                            DateTime dtIni = DateTime.Parse(msg["dtInicial"].GetString());
                            DateTime dtFim = DateTime.Parse(msg["dtFinal"].GetString()).AddDays(1);

                            int localId = msg["localId"].GetInt32();

                            // MAPEAMENTO DE SETOR
                            int sector = op.SectorId;
                            if (sector < 1 || sector > 3)
                                sector = 3;

                            var lista = _hikRepo.GetForOperator(dtIni, dtFim, sector, localId);
                            ApplyReadStatus(lista, fj);

                            SendJson("hikitsugui_list", lista);
                        });
                        break;
                    }

                // ============================================
                // PREVIEW
                // ============================================
                case "preview":
                    {
                        Task.Run(() =>
                        {
                            int id = msg["id"].GetInt32();
                            string fj = msg.ContainsKey("fj") ? msg["fj"].GetString() : "";

                            var h = _hikRepo.GetById(id);
                            if (h == null) return;

                            if (!string.IsNullOrEmpty(fj))
                            {
                                EnsureRead(id, fj);
                                SendJson("read_status", new { id, lido = true });
                            }

                            SendJson("hikitsugui_preview", h);

                            var anexos = _attachRepo.GetByHikitsugui(id);
                            SendJson("attachments", anexos);
                        });
                        break;
                    }

                // ============================================
                // OPEN ATTACHMENT
                // ============================================
                case "open_attachment":
                    {
                        string path = msg["path"].GetString();
                        if (File.Exists(path))
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = true
                            });
                        }
                        break;
                    }

                case "has_read":
                    {
                        Task.Run(() =>
                        {
                            int id = msg["id"].GetInt32();
                            string fj = msg["fj"].GetString();

                            bool lido = _readRepo.HasRead(id, fj);

                            SendJson("read_status", new { id, lido });
                        });
                        break;
                    }

                case "mark_read":
                    {
                        Task.Run(() =>
                        {
                            int id = msg["id"].GetInt32();
                            string fj = msg["fj"].GetString();

                            EnsureRead(id, fj);

                            SendJson("read_status", new { id, lido = true });
                        });
                        break;
                    }
            }
        }

        private void ApplyReadStatus(List<TeamOps.Core.Entities.HikitsuguiListItem> lista, string fj)
        {
            if (lista.Count == 0 || string.IsNullOrWhiteSpace(fj))
                return;

            using var conn = Program.ConnectionFactory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            var ids = lista.Select(x => x.Id).Distinct().ToList();
            var placeholders = new List<string>(ids.Count);

            cmd.CommandText = "SELECT HikitsuguiId FROM HikitsuguiReads WHERE ReaderCodigoFJ = @fj AND HikitsuguiId IN (";
            cmd.Parameters.AddWithValue("@fj", fj);

            for (int i = 0; i < ids.Count; i++)
            {
                var name = $"@id{i}";
                placeholders.Add(name);
                cmd.Parameters.AddWithValue(name, ids[i]);
            }

            cmd.CommandText += string.Join(", ", placeholders) + ")";

            var readIds = new HashSet<int>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                readIds.Add(reader.GetInt32(0));
            }

            foreach (var item in lista)
            {
                item.IsRead = readIds.Contains(item.Id);
            }
        }

        private void EnsureRead(int id, string fj)
        {
            if (!_readRepo.HasRead(id, fj))
            {
                _readRepo.MarkAsRead(id, fj);
            }
        }

        private void SendJson(string type, object? data)
        {
            var json = JsonSerializer.Serialize(new { type, data });

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    if (webView.CoreWebView2 != null)
                        webView.CoreWebView2.PostWebMessageAsJson(json);
                }));

                return;
            }

            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
