using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
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
            Console.WriteLine("WEBVIEW MSG: " + e.WebMessageAsJson);

            var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);

            if (msg == null)
            {
                Console.WriteLine("ERRO: msg veio null");
                return;
            }

            Console.WriteLine("ACTION RECEBIDA: " + msg.action);

            switch (msg.action)
            {
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
                            accessLevel = _currentUser.AccessLevel
                        });

                        webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(jsonFilters);

                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial,
                            dtFinal,
                            publico = "todos",
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

                case "filter":
                    {
                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial = (string)msg.dtInicial,
                            dtFinal = (string)msg.dtFinal,
                            publico = (string)msg.publico,
                            shiftId = Convert.ToInt32(msg.shiftId),
                            operatorId = Convert.ToInt32(msg.operatorId),
                            reasonId = Convert.ToInt32(msg.reasonId),
                            typeId = 0,
                            equipId = Convert.ToInt32(msg.equipId),
                            sectorId = Convert.ToInt32(msg.sectorId),
                            codigoFJ = _currentLeader.CodigoFJ,
                            search = (string)msg.search,
                            accessLevel = _currentUser.AccessLevel
                        });
                        break;
                    }

                case "preview":
                    {
                        SendJsonFromSql("select_hikitsugui_by_id.sql", new { id = msg.id });
                        break;
                    }

                case "mark_read":
                    {
                        using var conn = _factory.CreateOpenConnection();

                        var id = Convert.ToInt32(msg.id);
                        EnsureRead(conn, id, _currentLeader.CodigoFJ);

                        SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                        {
                            dtInicial = (string)msg.dtInicial,
                            dtFinal = (string)msg.dtFinal,
                            publico = (string)msg.publico,
                            shiftId = Convert.ToInt32(msg.shiftId),
                            operatorId = Convert.ToInt32(msg.operatorId),
                            reasonId = Convert.ToInt32(msg.reasonId),
                            typeId = 0,
                            equipId = Convert.ToInt32(msg.equipId),
                            sectorId = Convert.ToInt32(msg.sectorId),
                            codigoFJ = _currentLeader.CodigoFJ,
                            search = (string)msg.search,
                            accessLevel = _currentUser.AccessLevel
                        });
                        break;
                    }

                case "load_for_edit":
                    {
                        using var conn = _factory.CreateOpenConnection();

                        var row = conn.QueryFirstOrDefault(
                            "SELECT * FROM Hikitsugui WHERE Id = @id",
                            new { id = msg.id });

                        var attachments = conn.Query(
                            "SELECT FileName, FilePath FROM HikitsuguiAttachments WHERE HikitsuguiId = @id",
                            new { id = msg.id });

                        var json = JsonSerializer.Serialize(new
                        {
                            type = "hikitsugui_edit",
                            data = new[] { row },
                            attachments
                        });

                        webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(json);
                        break;
                    }

                case "select_replies":
                    {
                        SendJsonFromSql("select_replies.sql", new { id = msg.id });
                        break;
                    }

                case "reply":
                    {
                        if (string.IsNullOrWhiteSpace(msg.text))
                        {
                            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                JsonSerializer.Serialize(new
                                {
                                    type = "error",
                                    message = "Digite uma resposta antes de salvar."
                                })
                            );
                            break;
                        }

                        ExecuteSql("insert_reply.sql", new
                        {
                            id = Convert.ToInt32(msg.id),
                            codigoFJ = _currentLeader.CodigoFJ,
                            text = (string)msg.text
                        });

                        SendJsonFromSql("select_replies.sql", new { id = msg.id });
                        break;
                    }

                case "select_attachments":
                    {
                        SendJsonFromSql("select_attachments.sql", new { id = msg.id });
                        break;
                    }

                case "open_attachment":
                    {
                        if (string.IsNullOrWhiteSpace(msg.path))
                        {
                            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                JsonSerializer.Serialize(new
                                {
                                    type = "error",
                                    message = "Caminho do anexo inválido."
                                })
                            );
                            break;
                        }

                        try
                        {
                            var ext = Path.GetExtension(msg.path).ToLowerInvariant();
                            var blockedExtensions = new[] { ".dll", ".exe", ".bin", ".sys" };

                            if (Array.Exists(blockedExtensions, x => x == ext))
                            {
                                webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                    JsonSerializer.Serialize(new
                                    {
                                        type = "error",
                                        message = "Este tipo de arquivo não pode ser aberto diretamente."
                                    })
                                );
                                break;
                            }

                            if (!File.Exists(msg.path))
                            {
                                webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                    JsonSerializer.Serialize(new
                                    {
                                        type = "error",
                                        message = "Arquivo não encontrado."
                                    })
                                );
                                break;
                            }

                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = msg.path,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("OPEN_ATTACHMENT ERROR: " + ex);

                            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                JsonSerializer.Serialize(new
                                {
                                    type = "error",
                                    message = ex.Message
                                })
                            );
                        }

                        break;
                    }

                case "delete_hikitsugui":
                    {
                        using var conn = _factory.CreateOpenConnection();
                        using var tran = conn.BeginTransaction();

                        try
                        {
                            conn.Execute(
                                "DELETE FROM HikitsuguiAttachments WHERE HikitsuguiId = @id",
                                new { id = msg.id },
                                tran
                            );

                            conn.Execute(
                                "DELETE FROM HikitsuguiReads WHERE HikitsuguiId = @id",
                                new { id = msg.id },
                                tran
                            );

                            conn.Execute(
                                "DELETE FROM HikitsuguiCorrections WHERE HikitsuguiId = @id",
                                new { id = msg.id },
                                tran
                            );

                            conn.Execute(
                                "DELETE FROM HikitsuguiResponses WHERE HikitsuguiId = @id",
                                new { id = msg.id },
                                tran
                            );

                            conn.Execute(
                                "DELETE FROM Hikitsugui WHERE Id = @id",
                                new { id = msg.id },
                                tran
                            );

                            tran.Commit();

                            try
                            {
                                var logRepo = new SystemLogRepository(_factory);
                                logRepo.Log(
                                    _currentLeader.CodigoFJ,
                                    "Hikitsugui",
                                    "Excluiu",
                                    msg.id
                                );
                            }
                            catch (Exception logEx)
                            {
                                Console.WriteLine("DELETE_HIKITSUGUI LOG ERROR: " + logEx);
                            }

                            SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                            {
                                dtInicial = (string)msg.dtInicial,
                                dtFinal = (string)msg.dtFinal,
                                publico = (string)msg.publico,
                                shiftId = Convert.ToInt32(msg.shiftId),
                                operatorId = Convert.ToInt32(msg.operatorId),
                                reasonId = Convert.ToInt32(msg.reasonId),
                                typeId = 0,
                                equipId = Convert.ToInt32(msg.equipId),
                                sectorId = Convert.ToInt32(msg.sectorId),
                                codigoFJ = _currentLeader.CodigoFJ,
                                search = (string)msg.search,
                                accessLevel = _currentUser.AccessLevel
                            });

                            break;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                tran.Rollback();
                            }
                            catch (Exception rollbackEx)
                            {
                                Console.WriteLine("DELETE_HIKITSUGUI ROLLBACK ERROR: " + rollbackEx);
                            }

                            Console.WriteLine("DELETE_HIKITSUGUI ERROR: " + ex);

                            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                JsonSerializer.Serialize(new
                                {
                                    type = "error",
                                    message = ex.Message
                                })
                            );

                            break;
                        }
                    }

                case "save_edit":
                    {
                        using var conn = _factory.CreateOpenConnection();
                        using var tran = conn.BeginTransaction();

                        try
                        {
                            var sqlUpdate = File.ReadAllText(
                                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "update_hikitsugui.sql")
                            );

                            var sqlInsertAttachment = File.ReadAllText(
                                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "insert_hikitsugui_attachment.sql")
                            );

                            int id = Convert.ToInt32(msg.id);

                            int? equipmentId = (msg.equipmentId == null || Convert.ToInt32(msg.equipmentId) == 0)
                                ? null
                                : Convert.ToInt32(msg.equipmentId);

                            int? localId = (msg.localId == null || Convert.ToInt32(msg.localId) == 0)
                                ? null
                                : Convert.ToInt32(msg.localId);

                            int? sectorId = (msg.sectorId == null || Convert.ToInt32(msg.sectorId) == 0)
                                ? null
                                : Convert.ToInt32(msg.sectorId);

                            conn.Execute(sqlUpdate, new
                            {
                                id,
                                categoryId = Convert.ToInt32(msg.categoryId),
                                equipmentId,
                                localId,
                                sectorId,
                                description = (string)msg.description
                            }, tran);

                            conn.Execute(
                                "DELETE FROM HikitsuguiAttachments WHERE HikitsuguiId = @id",
                                new { id },
                                tran
                            );

                            if (msg.existingAttachments != null)
                            {
                                foreach (var item in msg.existingAttachments)
                                {
                                    conn.Execute(sqlInsertAttachment, new
                                    {
                                        hikitsuguiId = id,
                                        fileName = item.FileName,
                                        filePath = item.FilePath
                                    }, tran);
                                }
                            }

                            if (msg.newAttachments != null)
                            {
                                foreach (var item in msg.newAttachments)
                                {
                                    var savedPath = SaveAttachment(id, item.fileName, item.base64);

                                    conn.Execute(sqlInsertAttachment, new
                                    {
                                        hikitsuguiId = id,
                                        fileName = item.fileName,
                                        filePath = savedPath
                                    }, tran);
                                }
                            }

                            tran.Commit();

                            try
                            {
                                var logRepo = new SystemLogRepository(_factory);
                                logRepo.Log(
                                    _currentLeader.CodigoFJ,
                                    "Hikitsugui",
                                    "Editou",
                                    id,
                                    $"Categoria={msg.categoryId}"
                                );
                            }
                            catch (Exception logEx)
                            {
                                Console.WriteLine("SAVE_EDIT LOG ERROR: " + logEx);
                            }

                            SendJsonFromSql("select_hikitsugui_for_leader.sql", new
                            {
                                dtInicial = (string)msg.dtInicial,
                                dtFinal = (string)msg.dtFinal,
                                publico = (string)msg.publico,
                                shiftId = Convert.ToInt32(msg.shiftId),
                                operatorId = Convert.ToInt32(msg.operatorId),
                                reasonId = Convert.ToInt32(msg.reasonId),
                                typeId = 0,
                                equipId = Convert.ToInt32(msg.equipId),
                                sectorId = Convert.ToInt32(msg.sectorIdFilter),
                                codigoFJ = _currentLeader.CodigoFJ,
                                search = (string)msg.search,
                                accessLevel = _currentUser.AccessLevel
                            });

                            break; // 👈 ESSENCIAL
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                tran.Rollback();
                            }
                            catch (Exception rollbackEx)
                            {
                                Console.WriteLine("ROLLBACK ERROR: " + rollbackEx);
                            }

                            Console.WriteLine("SAVE_EDIT ERROR: " + ex);

                            webViewHikitsugui.CoreWebView2.PostWebMessageAsJson(
                                JsonSerializer.Serialize(new
                                {
                                    type = "error",
                                    message = ex.Message
                                })
                            );

                            break;
                        }
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

                // Se a query não tem Description, não mexe em nada
                if (!dict.ContainsKey("Description"))
                    continue;

                if (dict["Description"] == null)
                {
                    dict["DescriptionHtml"] = "";
                    continue;
                }

                dict["DescriptionHtml"] = dict["Description"];
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

        private void EnsureRead(System.Data.IDbConnection conn, int id, string codigoFJ)
        {
            var alreadyRead = conn.ExecuteScalar<int>(
                @"SELECT COUNT(1)
                  FROM HikitsuguiReads
                  WHERE HikitsuguiId = @id
                    AND ReaderCodigoFJ = @codigoFJ;",
                new { id, codigoFJ });

            if (alreadyRead == 0)
            {
                conn.Execute(
                    @"INSERT INTO HikitsuguiReads
                      (HikitsuguiId, ReaderCodigoFJ, ReadAt)
                      VALUES
                      (@id, @codigoFJ, CURRENT_TIMESTAMP);",
                    new { id, codigoFJ });
            }
        }

        private string SaveAttachment(int hikitsuguiId, string fileName, string base64)
        {
            var root = ConfigurationManager.AppSettings["HikitsuguiAttachmentPath"];
            var dir = Path.Combine(root, hikitsuguiId.ToString());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, fileName);
            File.WriteAllBytes(filePath, Convert.FromBase64String(base64));

            return filePath;
        }

    }
}
