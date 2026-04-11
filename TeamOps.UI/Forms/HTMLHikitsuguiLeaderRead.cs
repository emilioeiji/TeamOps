using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLHikitsuguiLeaderRead : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _currentUser;          // ← TEM ACCESS LEVEL
        private readonly Operator _currentLeader;    // ← TEM CODIGOFJ

        public HTMLHikitsuguiLeaderRead(
            SqliteConnectionFactory factory,
            User currentUser,
            Operator currentLeader)
        {
            InitializeComponent();

            _factory = factory;
            _currentUser = currentUser;
            _currentLeader = currentLeader;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewHikitsugui.EnsureCoreWebView2Async(null);

            var core = webViewHikitsugui.CoreWebView2;

            core.Settings.AreHostObjectsAllowed = true;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            var htmlPath = Path.Combine(
                Application.StartupPath,
                "ui", "hikitsugui-leader-read", "index.html");

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "hikitsugui-leader-read"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");

        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            
            var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);
            if (msg == null) return;
            Console.WriteLine("ACTION RECEBIDA: " + msg.action);

            switch (msg.action)
            {
                // ============================================================
                // LOAD INICIAL
                // ============================================================
                case "load":
                    {
                        using var conn = _factory.CreateOpenConnection();

                        var shifts = conn.Query("SELECT Id, NamePt FROM Shifts ORDER BY Id;");
                        var operators = conn.Query("SELECT CodigoFJ, NameRomanji FROM Operators ORDER BY NameRomanji;");
                        var categories = conn.Query("SELECT Id, NamePt FROM Categories ORDER BY NamePt;");
                        var equipments = conn.Query("SELECT Id, NamePt FROM Equipments ORDER BY NamePt;");
                        var locals = conn.Query("SELECT Id, NamePt FROM Locals ORDER BY NamePt;");
                        var sectors = conn.Query("SELECT Id, NamePt FROM Sectors ORDER BY NamePt;");

                        var dtInicial = DateTime.Today.AddDays(-31).ToString("yyyy-MM-dd");
                        var dtFinal = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

                        // Envia filtros + ACCESS LEVEL
                        var jsonFilters = JsonSerializer.Serialize(new
                        {
                            type = "filters",
                            shifts,
                            operators,
                            categories,
                            equipments,
                            locals,
                            sectors,
                            dtInicial,
                            dtFinal,
                            accessLevel = _currentUser.AccessLevel   // ← ADICIONADO
                        });

                        webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(jsonFilters);

                        // Envia tabela inicial
                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial,
                            dtFinal,
                            publico =
                                _currentUser.AccessLevel >= AccessLevel.GL ? "masv" :
                                _currentUser.AccessLevel == AccessLevel.KL ? "lider" :
                                "operador",
                        shiftId = 0,
                            operatorId = 0,
                            reasonId = 0,
                            typeId = 0,
                            equipId = 0,
                            sectorId = 0,
                            codigoFJ = _currentLeader.CodigoFJ,
                            search = "",
                            accessLevel = _currentUser.AccessLevel
                        });

                        break;
                    }

                // ============================================================
                // FILTRO
                // ============================================================
                case "filter":
                    {
                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial = msg.dtInicial,
                            dtFinal = msg.dtFinal,
                            publico =
                                msg.publico == "todos"
                                    ? (_currentUser.AccessLevel >= AccessLevel.GL ? "masv"
                                      : _currentUser.AccessLevel == AccessLevel.KL ? "lider"
                                      : "operador")
                                    : msg.publico,
                            shiftId = msg.shiftId,
                            operatorId = msg.operatorId,
                            reasonId = msg.reasonId,
                            typeId = 0,
                            equipId = msg.equipId,
                            sectorId = msg.sectorId,
                            codigoFJ = _currentLeader.CodigoFJ,
                            search = msg.search,
                            accessLevel = _currentUser.AccessLevel
                        });
                        break;
                    }

                // ============================================================
                // PREVIEW
                // ============================================================
                case "preview":
                    SendJsonFromSql("select_hikitsugui_by_id.sql", new { id = msg.id });
                    break;

                // ============================================================
                // INSERT REPLY
                // ============================================================
                case "select_replies":
                    {
                        SendJsonFromSql("select_replies.sql", new { id = msg.id });
                        break;
                    }

                case "reply":
                    {
                        ExecuteSql("insert_reply.sql", new
                        {
                            id = msg.id,
                            text = msg.text,
                            codigoFJ = _currentLeader.CodigoFJ
                        });

                        SendJsonFromSql("select_replies.sql", new { id = msg.id });
                        break;
                    }

                // ============================================================
                // DELETE REPLY
                // ============================================================
                case "delete_reply":
                    {
                        ExecuteSql("delete_reply.sql", new { id = msg.parentId });
                        SendJsonFromSql("select_replies.sql", new { id = msg.parentId });
                        break;
                    }

                // ============================================================
                // MARK READ
                // ============================================================
                case "mark_read":
                    {
                        ExecuteSql("insert_read.sql", new
                        {
                            id = msg.id,
                            codigoFJ = _currentLeader.CodigoFJ
                        });

                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial = msg.dtInicial,
                            dtFinal = msg.dtFinal,
                            publico =
                                msg.publico == "todos"
                                    ? (_currentUser.AccessLevel >= AccessLevel.GL ? "masv"
                                      : _currentUser.AccessLevel == AccessLevel.KL ? "lider"
                                      : "operador")
                                    : msg.publico,
                            shiftId = msg.shiftId,
                            operatorId = msg.operatorId,
                            reasonId = msg.reasonId,
                            typeId = 0,
                            equipId = msg.equipId,
                            sectorId = msg.sectorId,
                            codigoFJ = _currentLeader.CodigoFJ,
                            search = msg.search,
                            accessLevel = _currentUser.AccessLevel
                        });

                        break;
                    }
            }
        }

        // ============================================================
        // EXECUTAR SQL E ENVIAR JSON
        // ============================================================
        private void SendJsonFromSql(string sqlFile, object? param = null)
        {
            var sqlPath = Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", sqlFile);
            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            var rows = conn.Query(sql, param).ToList();

            foreach (var row in rows)
            {
                var dict = row as IDictionary<string, object>;

                if (!dict.ContainsKey("Description") || dict["Description"] == null)
                {
                    dict["DescriptionHtml"] = "";
                    continue;
                }

                string rtf = dict["Description"].ToString();

                // Usa o conversor SEGURO
                dict["DescriptionHtml"] = dict["Description"]; // envia RTF puro

            }

            var json = JsonSerializer.Serialize(new
            {
                type = Path.GetFileNameWithoutExtension(sqlFile).Replace("select_", ""),
                data = rows
            });

            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void ExecuteSql(string sqlFile, object param)
        {
            var sqlPath = Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", sqlFile);
            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(sql, param);
        }
    }
}
