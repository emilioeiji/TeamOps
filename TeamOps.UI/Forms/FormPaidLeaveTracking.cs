using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Dapper;
using Microsoft.Web.WebView2.Core;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.UI.Forms
{
    public partial class FormPaidLeaveTracking : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly Operator _currentOperator;

        public FormPaidLeaveTracking(Operator op, Shift shift, SqliteConnectionFactory factory)
        {
            InitializeComponent();

            _factory = factory;
            _currentOperator = op;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewPaidLeave.EnsureCoreWebView2Async(null);

            webViewPaidLeave.CoreWebView2.WebMessageReceived += WebMessageReceived;

            var htmlPath = Path.Combine(
                Application.StartupPath,
                "ui", "paidleave", "index.html");

            webViewPaidLeave.Source = new Uri(htmlPath);
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);
            if (msg == null) return;

            switch (msg.action)
            {
                case "paidleave_load":
                    SendJsonFromSql("select_all.sql");
                    SendJsonFromSql("select_operators.sql");
                    break;

                case "toggle_todoke":
                    ExecuteSql("insert_todoke.sql", new
                    {
                        AcompYukyuId = msg.id,
                        TakenBy = _currentOperator.CodigoFJ,
                        TakenAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    SendJsonFromSql("select_all.sql");
                    break;

                case "toggle_folha":
                    ExecuteSql("insert_folha.sql", new
                    {
                        AcompYukyuId = msg.id,
                        TakenBy = _currentOperator.CodigoFJ,
                        TakenAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    SendJsonFromSql("select_all.sql");
                    break;

                case "get_todoke_info":
                    SendJsonFromSql("get_todoke_info.sql", new { AcompYukyuId = msg.id });
                    break;

                case "get_folha_info":
                    SendJsonFromSql("get_folha_info.sql", new { AcompYukyuId = msg.id });
                    break;

                case "add_request":
                    ExecuteSql("insert_acomp.sql", new
                    {
                        OperatorCodigoFJ = msg.opCodigoFJ,
                        RequestDate = msg.reqDate,
                        AuthorizedByCodigoFJ = _currentOperator.CodigoFJ,
                        Notes = msg.notes
                    });

                    SendJsonFromSql("select_all.sql");
                    break;
            }
        }

        private void SendJsonFromSql(string sqlFile, object? param = null)
        {
            var sqlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Sql", "paidleave", sqlFile);

            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            var rows = conn.Query(sql, param); // dynamic

            var json = JsonSerializer.Serialize(new
            {
                type = Path.GetFileNameWithoutExtension(sqlFile),
                data = rows
            });

            webViewPaidLeave.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void ExecuteSql(string sqlFile, object param)
        {
            var sqlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Sql", "paidleave", sqlFile);

            var sql = File.ReadAllText(sqlPath);

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(sql, param);
        }
    }

    public class JsRequest
    {
        public string action { get; set; } = "";
        public int id { get; set; }

        public string opCodigoFJ { get; set; }
        public string reqDate { get; set; }
        public string notes { get; set; }
    }
}
