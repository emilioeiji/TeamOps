using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLHikitsuguiCreate : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _currentUser;
        private readonly Operator _currentOperator;

        public HTMLHikitsuguiCreate(
            SqliteConnectionFactory factory,
            User currentUser,
            Operator currentOperator)
        {
            InitializeComponent();

            _factory = factory;
            _currentUser = currentUser;
            _currentOperator = currentOperator;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewCreate.EnsureCoreWebView2Async(null);

            var core = webViewCreate.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "hikitsugui-create"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);
            if (msg == null) return;

            switch (msg.action)
            {
                case "load":
                    LoadInitialData();
                    break;

                case "save":
                    SaveHikitsugui(msg);
                    break;

                case "cancel":
                    Close();
                    break;
            }
        }

        private void LoadInitialData()
        {
            using var conn = _factory.CreateOpenConnection();

            var shifts = conn.Query(File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "select_shifts.sql")
            ));

            var categories = conn.Query(File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "select_categories.sql")
            ));

            var equipments = conn.Query(File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "select_equipments.sql")
            ));

            var locals = conn.Query(File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "select_locals.sql")
            ));

            var sectors = conn.Query(File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "select_sectors.sql")
            ));

            var json = JsonSerializer.Serialize(new
            {
                type = "filters",
                shifts,
                categories,
                equipments,
                locals,
                sectors,
                shiftId = _currentOperator.ShiftId,
                creatorCodigoFJ = _currentOperator.CodigoFJ,
                creatorName = _currentOperator.NameRomanji,
                now = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });

            webViewCreate.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void SaveHikitsugui(JsRequest msg)
        {
            using var conn = _factory.CreateOpenConnection();

            // 1) INSERT PRINCIPAL
            var sqlInsert = File.ReadAllText(
                Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "insert_hikitsugui.sql")
            );

            int newId = conn.ExecuteScalar<int>(sqlInsert, new
            {
                date = DateTime.Now,
                shiftId = msg.shiftId,
                creator = _currentOperator.CodigoFJ,
                categoryId = msg.reasonId,
                equipmentId = msg.equipId == 0 ? (int?)null : msg.equipId,
                localId = msg.localId == 0 ? (int?)null : msg.localId,
                sectorId = msg.sectorId == 0 ? (int?)null : msg.sectorId,
                forLeaders = msg.publico == "lider" ? 1 : 0,
                forOperators = msg.publico == "operador" ? 1 : 0,
                forMaSv = msg.publico == "masv" ? 1 : 0,
                description = msg.text
            });

            // 2) SALVAR ANEXOS
            if (msg.attachments != null)
            {
                var sqlAttach = File.ReadAllText(
                    Path.Combine(Application.StartupPath, "Sql", "Hikitsugui", "insert_hikitsugui_attachment.sql")
                );

                string baseFolder = Path.Combine(
                    Application.StartupPath,
                    "Attachments",
                    "Hikitsugui",
                    newId.ToString()
                );

                Directory.CreateDirectory(baseFolder);

                foreach (var file in msg.attachments)
                {
                    string fileName = file.fileName;
                    string fullPath = Path.Combine(baseFolder, fileName);

                    File.WriteAllBytes(fullPath, Convert.FromBase64String(file.base64));

                    conn.Execute(sqlAttach, new
                    {
                        hikitsuguiId = newId,
                        fileName,
                        filePath = fullPath
                    });
                }
            }

            // 3) RETORNO PARA O JS
            webViewCreate.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(new { type = "saved", id = newId })
            );
        }
    }
}
