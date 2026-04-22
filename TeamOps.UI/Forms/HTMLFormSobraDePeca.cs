using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormSobraDePeca : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly Operator _currentOperator;

        public HTMLFormSobraDePeca(
            SqliteConnectionFactory factory,
            Operator currentOperator)
        {
            InitializeComponent();

            _factory = factory;
            _currentOperator = currentOperator;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewSobraDePeca.EnsureCoreWebView2Async(null);

            var core = webViewSobraDePeca.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "sobra-de-peca"),
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
                    SaveSobraDePeca(msg);
                    break;
            }
        }

        private void LoadInitialData()
        {
            using var conn = _factory.CreateOpenConnection();

            var shiftName = conn.QueryFirstOrDefault<string>(
                "SELECT NamePt FROM Shifts WHERE Id = @id",
                new { id = _currentOperator.ShiftId }
            ) ?? "";

            var operators = conn.Query(
                @"SELECT CodigoFJ, NameRomanji
                  FROM Operators
                  WHERE Status = 1 AND ShiftId = @shiftId
                  ORDER BY NameRomanji",
                new { shiftId = _currentOperator.ShiftId }
            ).ToList();

            var machines = conn.Query(
                @"SELECT Id, NamePt
                  FROM Machines
                  ORDER BY NamePt"
            ).ToList();

            var shains = conn.Query(
                @"SELECT Id, NameRomanji
                  FROM Shain
                  ORDER BY NameRomanji"
            ).ToList();

            var rows = QueryRecentRows(conn).ToList();

            PostJson(new
            {
                type = "init",
                shiftId = _currentOperator.ShiftId,
                shiftName,
                leaderName = _currentOperator.NameRomanji,
                today = DateTime.Today.ToString("yyyy-MM-dd"),
                operators,
                machines,
                shains,
                rows
            });
        }

        private void SaveSobraDePeca(JsRequest msg)
        {
            try
            {
                ValidatePayload(msg);

                using var conn = _factory.CreateOpenConnection();

                const string sql = @"
                    INSERT INTO SobraDePeca
                    (Data, TurnoId, Lote, OperadorId, Tanjuu, PesoGramas, Quantidade, MachineId, ShainId, Observacao, Lider, CreatedAt, Item)
                    VALUES
                    (@Data, @TurnoId, @Lote, @OperadorId, @Tanjuu, @PesoGramas, @Quantidade, @MachineId, @ShainId, @Observacao, @Lider, @CreatedAt, @Item);
                    SELECT last_insert_rowid();";

                var data = DateTime.Parse(msg.date);
                var createdAt = DateTime.Now;

                var newId = conn.ExecuteScalar<int>(sql, new
                {
                    Data = data.ToString("yyyy-MM-dd"),
                    TurnoId = _currentOperator.ShiftId,
                    Lote = msg.lote.Trim(),
                    OperadorId = msg.opCodigoFJ,
                    Tanjuu = msg.tanjuu,
                    PesoGramas = msg.pesoGramas,
                    Quantidade = msg.quantidade,
                    MachineId = msg.machineId,
                    ShainId = msg.shainId,
                    Observacao = string.IsNullOrWhiteSpace(msg.observacao) ? null : msg.observacao.Trim(),
                    Lider = _currentOperator.NameRomanji,
                    CreatedAt = createdAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Item = msg.item.Trim().ToUpperInvariant()
                });

                PostJson(new
                {
                    type = "saved",
                    id = newId,
                    message = "Sobra de peça registrada com sucesso."
                });

                SendRows(conn);
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

        private void ValidatePayload(JsRequest msg)
        {
            if (string.IsNullOrWhiteSpace(msg.date))
                throw new InvalidOperationException("Informe a data.");

            if (!DateTime.TryParse(msg.date, out _))
                throw new InvalidOperationException("Informe uma data válida.");

            if (string.IsNullOrWhiteSpace(msg.lote))
                throw new InvalidOperationException("Informe o lote.");

            if (string.IsNullOrWhiteSpace(msg.opCodigoFJ))
                throw new InvalidOperationException("Selecione um operador.");

            if (msg.tanjuu <= 0)
                throw new InvalidOperationException("Informe um tanjuu válido.");

            if (msg.pesoGramas <= 0)
                throw new InvalidOperationException("Informe um peso válido.");

            if (msg.quantidade <= 0)
                throw new InvalidOperationException("Quantidade inválida.");

            if (msg.machineId <= 0)
                throw new InvalidOperationException("Selecione uma máquina.");

            if (msg.shainId <= 0)
                throw new InvalidOperationException("Selecione um shain.");

            if (string.IsNullOrWhiteSpace(msg.item))
                throw new InvalidOperationException("Informe o item.");
        }

        private void SendRows(System.Data.IDbConnection conn)
        {
            PostJson(new
            {
                type = "rows",
                data = QueryRecentRows(conn).ToList()
            });
        }

        private static System.Collections.Generic.IEnumerable<dynamic> QueryRecentRows(System.Data.IDbConnection conn)
        {
            const string sql = @"
                SELECT
                    s.Id AS id,
                    substr(s.Data, 1, 10) AS data,
                    sh.NamePt AS shiftName,
                    s.Lote AS lote,
                    o.NameRomanji AS operatorName,
                    s.Tanjuu AS tanjuu,
                    s.PesoGramas AS pesoGramas,
                    s.Quantidade AS quantidade,
                    m.NamePt AS machineName,
                    sa.NameRomanji AS shainName,
                    COALESCE(s.Observacao, '') AS observacao,
                    s.Lider AS lider,
                    substr(s.CreatedAt, 1, 16) AS createdAt,
                    s.Item AS item
                FROM SobraDePeca s
                LEFT JOIN Shifts sh ON sh.Id = s.TurnoId
                LEFT JOIN Operators o ON o.CodigoFJ = s.OperadorId
                LEFT JOIN Machines m ON m.Id = s.MachineId
                LEFT JOIN Shain sa ON sa.Id = s.ShainId
                ORDER BY s.CreatedAt DESC, s.Id DESC
                LIMIT 100;";

            return conn.Query(sql);
        }

        private void PostJson(object payload)
        {
            webViewSobraDePeca.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(payload)
            );
        }
    }
}
