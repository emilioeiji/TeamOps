using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.Configuration;
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
                    Console.WriteLine("QTD ANEXOS RECEBIDOS: " + (msg.attachments?.Count ?? 0));
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

            var shifts = conn.Query(
                @"SELECT
                      Id,
                      COALESCE(NamePt, '') AS NamePt,
                      COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                  FROM Shifts
                  ORDER BY Id;"
            );

            var categories = conn.Query(
                @"SELECT
                      Id,
                      COALESCE(NamePt, '') AS NamePt,
                      COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                  FROM Categories
                  ORDER BY NamePt;"
            );

            var equipments = conn.Query(
                @"SELECT
                      Id,
                      COALESCE(NamePt, '') AS NamePt,
                      COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                  FROM Equipments
                  ORDER BY NamePt;"
            );

            var locals = conn.Query(
                @"SELECT
                      Id,
                      COALESCE(NamePt, '') AS NamePt,
                      COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                  FROM Locals
                  ORDER BY NamePt;"
            );

            var sectors = conn.Query(
                @"SELECT
                      Id,
                      COALESCE(NamePt, '') AS NamePt,
                      COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
                  FROM Sectors
                  ORDER BY NamePt;"
            );

            var json = JsonSerializer.Serialize(new
            {
                type = "filters",
                locale = Program.CurrentLocale,
                shifts,
                categories,
                equipments,
                locals,
                sectors,
                shiftId = _currentOperator.ShiftId,
                creatorCodigoFJ = _currentOperator.CodigoFJ,
                creatorNamePt = _currentOperator.NameRomanji,
                creatorNameJp = string.IsNullOrWhiteSpace(_currentOperator.NameNihongo)
                    ? _currentOperator.NameRomanji
                    : _currentOperator.NameNihongo,
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
                    ConfigurationManager.AppSettings["HikitsuguiAttachmentPath"],
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
