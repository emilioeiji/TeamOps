using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Web.WebView2.Core;
using System;
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
    public partial class HTMLFormOperators : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly OperatorRepository _opRepo;
        private readonly ShiftRepository _shiftRepo;
        private readonly GroupRepository _groupRepo;
        private readonly SectorRepository _sectorRepo;

        public HTMLFormOperators()
        {
            InitializeComponent();

            _factory = Program.ConnectionFactory;
            _opRepo = new OperatorRepository(_factory);
            _shiftRepo = new ShiftRepository(_factory);
            _groupRepo = new GroupRepository(_factory);
            _sectorRepo = new SectorRepository(_factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewOperators.EnsureCoreWebView2Async(null);

            var core = webViewOperators.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "operators"),
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
                    SaveOperator(msg);
                    break;

                case "update":
                    UpdateOperator(msg);
                    break;

                case "delete":
                    DeleteOperator(msg);
                    break;
            }
        }

        private void LoadInitialData()
        {
            using var conn = _factory.CreateOpenConnection();

            PostJson(new
            {
                type = "init",
                shifts = _shiftRepo.GetAll(),
                groups = _groupRepo.GetAll(),
                sectors = _sectorRepo.GetAll(),
                rows = QueryOperators(conn).ToList()
            });
        }

        private void SaveOperator(JsRequest msg)
        {
            try
            {
                var codigoFJ = NormalizeCodigoFJ(msg.codigoFJ);
                ValidatePayload(msg, codigoFJ, false);

                if (_opRepo.GetByCodigoFJ(codigoFJ) != null)
                    throw new InvalidOperationException("Ja existe um operador com este Codigo FJ.");

                _opRepo.Add(BuildOperator(msg, codigoFJ));

                PostJson(new
                {
                    type = "saved",
                    message = "Operador cadastrado com sucesso."
                });

                SendRows();
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

        private void UpdateOperator(JsRequest msg)
        {
            try
            {
                var codigoFJ = NormalizeCodigoFJ(msg.codigoFJ);
                ValidatePayload(msg, codigoFJ, true);

                if (_opRepo.GetByCodigoFJ(codigoFJ) == null)
                    throw new InvalidOperationException("Operador nao encontrado para atualizacao.");

                _opRepo.Update(BuildOperator(msg, codigoFJ));

                PostJson(new
                {
                    type = "updated",
                    message = "Operador atualizado com sucesso."
                });

                SendRows();
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

        private void DeleteOperator(JsRequest msg)
        {
            try
            {
                var codigoFJ = NormalizeCodigoFJ(msg.codigoFJ);
                if (string.IsNullOrWhiteSpace(codigoFJ))
                    throw new InvalidOperationException("Selecione um operador para excluir.");

                using var conn = _factory.CreateOpenConnection();

                if (_opRepo.GetByCodigoFJ(codigoFJ) == null)
                    throw new InvalidOperationException("Operador nao encontrado para exclusao.");

                if (HasOperatorDependencies(conn, codigoFJ, out var dependencyTable))
                    throw new InvalidOperationException(
                        $"Nao e possivel excluir este operador porque o Codigo FJ ja esta vinculado a registros em {dependencyTable}."
                    );

                _opRepo.Delete(codigoFJ);

                PostJson(new
                {
                    type = "deleted",
                    message = "Operador excluido com sucesso."
                });

                SendRows();
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

        private void SendRows()
        {
            using var conn = _factory.CreateOpenConnection();

            PostJson(new
            {
                type = "rows",
                data = QueryOperators(conn).ToList()
            });
        }

        private static string NormalizeCodigoFJ(string? codigoFJ)
        {
            return (codigoFJ ?? string.Empty).Trim().ToUpperInvariant();
        }

        private void ValidatePayload(JsRequest msg, string codigoFJ, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(codigoFJ))
                throw new InvalidOperationException("Informe o Codigo FJ.");

            if (string.IsNullOrWhiteSpace(msg.nameRomanji))
                throw new InvalidOperationException("Informe o nome Romanji.");

            if (string.IsNullOrWhiteSpace(msg.nameNihongo))
                throw new InvalidOperationException("Informe o nome Nihongo.");

            if (msg.shiftId <= 0)
                throw new InvalidOperationException("Selecione o turno.");

            if (msg.groupId <= 0)
                throw new InvalidOperationException("Selecione o grupo.");

            if (msg.sectorId <= 0)
                throw new InvalidOperationException("Selecione o setor.");

            if (!DateTime.TryParse(msg.startDate, out var startDate))
                throw new InvalidOperationException("Informe uma data de inicio valida.");

            if (msg.hasEndDate)
            {
                if (!DateTime.TryParse(msg.endDate, out var endDate))
                    throw new InvalidOperationException("Informe uma data de termino valida.");

                if (endDate < startDate)
                    throw new InvalidOperationException("A data de termino nao pode ser anterior a data de inicio.");
            }

            if (!string.IsNullOrWhiteSpace(msg.birthDate) && !DateTime.TryParse(msg.birthDate, out _))
                throw new InvalidOperationException("Informe uma data de nascimento valida.");

            if (isUpdate && string.IsNullOrWhiteSpace(codigoFJ))
                throw new InvalidOperationException("Codigo FJ invalido para atualizacao.");
        }

        private static Operator BuildOperator(JsRequest msg, string codigoFJ)
        {
            DateTime? endDate = null;
            if (msg.hasEndDate && DateTime.TryParse(msg.endDate, out var parsedEndDate))
                endDate = parsedEndDate;

            DateTime? birthDate = null;
            if (!string.IsNullOrWhiteSpace(msg.birthDate) && DateTime.TryParse(msg.birthDate, out var parsedBirthDate))
                birthDate = parsedBirthDate;

            return new Operator
            {
                CodigoFJ = codigoFJ,
                NameRomanji = msg.nameRomanji.Trim(),
                NameNihongo = msg.nameNihongo.Trim(),
                ShiftId = msg.shiftId,
                GroupId = msg.groupId,
                SectorId = msg.sectorId,
                StartDate = DateTime.Parse(msg.startDate),
                EndDate = endDate,
                Trainer = msg.trainer,
                Status = msg.status,
                IsLeader = msg.isLeader,
                Telefone = string.IsNullOrWhiteSpace(msg.phone) ? null : msg.phone.Trim(),
                Endereco = string.IsNullOrWhiteSpace(msg.address) ? null : msg.address.Trim(),
                Nascimento = birthDate
            };
        }

        private static System.Collections.Generic.IEnumerable<dynamic> QueryOperators(System.Data.IDbConnection conn)
        {
            const string sql = @"
                SELECT
                    o.CodigoFJ AS codigoFJ,
                    o.NameRomanji AS nameRomanji,
                    o.NameNihongo AS nameNihongo,
                    o.ShiftId AS shiftId,
                    o.GroupId AS groupId,
                    o.SectorId AS sectorId,
                    substr(o.StartDate, 1, 10) AS startDate,
                    CASE
                        WHEN o.EndDate IS NULL THEN ''
                        ELSE substr(o.EndDate, 1, 10)
                    END AS endDate,
                    CASE
                        WHEN o.Nascimento IS NULL THEN ''
                        ELSE substr(o.Nascimento, 1, 10)
                    END AS birthDate,
                    o.Trainer AS trainer,
                    o.Status AS status,
                    o.IsLeader AS isLeader,
                    COALESCE(o.Telefone, '') AS phone,
                    COALESCE(o.Endereco, '') AS address,
                    sh.NamePt AS shiftName,
                    g.NamePt AS groupName,
                    sc.NamePt AS sectorName
                FROM Operators o
                LEFT JOIN Shifts sh ON sh.Id = o.ShiftId
                LEFT JOIN Groups g ON g.Id = o.GroupId
                LEFT JOIN Sectors sc ON sc.Id = o.SectorId
                ORDER BY o.Status DESC, o.NameRomanji;";

            return conn.Query(sql);
        }

        private static bool HasOperatorDependencies(System.Data.IDbConnection conn, string codigoFJ, out string dependencyTable)
        {
            dependencyTable = string.Empty;

            if (conn is not SqliteConnection sqliteConn)
                return false;

            using var tablesCmd = sqliteConn.CreateCommand();
            tablesCmd.CommandText = @"
                SELECT name
                FROM sqlite_master
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name <> 'Operators'
                ORDER BY name;";

            using var tablesReader = tablesCmd.ExecuteReader();
            while (tablesReader.Read())
            {
                var tableName = tablesReader.GetString(0);

                using var fkCmd = sqliteConn.CreateCommand();
                fkCmd.CommandText = $"PRAGMA foreign_key_list(\"{tableName.Replace("\"", "\"\"")}\");";

                using var fkReader = fkCmd.ExecuteReader();
                while (fkReader.Read())
                {
                    var referencedTable = fkReader["table"]?.ToString();
                    var fromColumn = fkReader["from"]?.ToString();
                    var toColumn = fkReader["to"]?.ToString();

                    if (!string.Equals(referencedTable, "Operators", StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(toColumn, "CodigoFJ", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrWhiteSpace(fromColumn))
                    {
                        continue;
                    }

                    using var refCmd = sqliteConn.CreateCommand();
                    refCmd.CommandText = $@"
                        SELECT 1
                        FROM ""{tableName.Replace("\"", "\"\"")}""
                        WHERE ""{fromColumn.Replace("\"", "\"\"")}"" = @codigoFJ
                        LIMIT 1;";
                    refCmd.Parameters.AddWithValue("@codigoFJ", codigoFJ);

                    if (refCmd.ExecuteScalar() != null)
                    {
                        dependencyTable = tableName;
                        return true;
                    }
                }
            }

            return false;
        }

        private void PostJson(object payload)
        {
            webViewOperators.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(payload)
            );
        }
    }
}
