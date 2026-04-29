using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Data.Db;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormAdmin : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly Dictionary<string, AdminEntityDefinition> _entities;

        public HTMLFormAdmin()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = L("Administracao", "\u7ba1\u7406");

            _factory = Program.ConnectionFactory;
            _entities = BuildEntities();

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewAdmin.EnsureCoreWebView2Async(null);

            var core = webViewAdmin.CoreWebView2;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "admin"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var json = JsonDocument.Parse(e.WebMessageAsJson);
                var root = json.RootElement;
                var action = ReadString(root, "action");

                switch (action)
                {
                    case "load":
                        LoadInitial();
                        break;

                    case "load_entity":
                        SendEntityRows(ReadString(root, "entity"));
                        break;

                    case "save":
                        SaveEntity(ReadString(root, "entity"), root);
                        break;

                    case "update":
                        UpdateEntity(ReadString(root, "entity"), ReadInt(root, "id"), root);
                        break;

                    case "delete":
                        DeleteEntity(ReadString(root, "entity"), ReadInt(root, "id"));
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
            PostJson(new
            {
                type = "init",
                data = new
                {
                    locale = Program.CurrentLocale,
                    activeEntity = "shift",
                    entities = _entities.Values.Select(entity => new
                    {
                        key = entity.Key,
                        group = entity.Group,
                        readOnly = entity.ReadOnly,
                        titlePt = entity.TitlePt,
                        titleJp = entity.TitleJp,
                        descriptionPt = entity.DescriptionPt,
                        descriptionJp = entity.DescriptionJp,
                        fields = entity.Fields.Select(field => new
                        {
                            key = field.Key,
                            labelPt = field.LabelPt,
                            labelJp = field.LabelJp,
                            type = field.Type,
                            required = field.Required,
                            lookupKey = field.LookupKey
                        }),
                        columns = entity.Columns.Select(column => new
                        {
                            key = column.Key,
                            jpKey = column.JpKey,
                            labelPt = column.LabelPt,
                            labelJp = column.LabelJp
                        })
                    }),
                    lookups = BuildLookups()
                }
            });

            SendEntityRows("shift");
        }

        private object BuildLookups()
        {
            using var conn = _factory.CreateOpenConnection();

            return new
            {
                sectors = conn.Query(
                    @"
                        SELECT
                            Id AS id,
                            COALESCE(NamePt, '') AS namePt,
                            COALESCE(NULLIF(NameJp, ''), NamePt, '') AS nameJp
                        FROM Sectors
                        ORDER BY NamePt;"
                ).ToList(),
                locals = conn.Query(
                    @"
                        SELECT
                            l.Id AS id,
                            COALESCE(l.NamePt, '') AS namePt,
                            COALESCE(NULLIF(l.NameJp, ''), l.NamePt, '') AS nameJp
                        FROM Locals l
                        ORDER BY l.SectorId, l.NamePt, l.Id;"
                ).ToList()
            };
        }

        private void SendEntityRows(string entityKey)
        {
            if (!_entities.TryGetValue(entityKey, out var entity))
                throw new InvalidOperationException(L("Item administrativo invalido.", "\u7121\u52b9\u306a\u7ba1\u7406\u9805\u76ee\u3067\u3059\u3002"));

            using var conn = _factory.CreateOpenConnection();
            var rows = entity.QueryRows(conn).ToList();

            PostJson(new
            {
                type = "entity_rows",
                data = new
                {
                    entity = entityKey,
                    rows,
                    lookups = BuildLookups()
                }
            });
        }

        private void SaveEntity(string entityKey, JsonElement root)
        {
            if (!_entities.TryGetValue(entityKey, out var entity))
                throw new InvalidOperationException(L("Item administrativo invalido.", "\u7121\u52b9\u306a\u7ba1\u7406\u9805\u76ee\u3067\u3059\u3002"));

            if (entity.ReadOnly)
                throw new InvalidOperationException(L("Este item e somente leitura.", "\u3053\u306e\u9805\u76ee\u306f\u95b2\u89a7\u306e\u307f\u3067\u3059\u3002"));

            var values = root.GetProperty("values");
            var parameters = entity.BuildParameters(values);

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(entity.InsertSql, parameters);

            PostJson(new
            {
                type = "saved",
                message = L("Cadastro salvo com sucesso.", "\u767b\u9332\u3092\u4fdd\u5b58\u3057\u307e\u3057\u305f\u3002")
            });

            SendEntityRows(entityKey);
        }

        private void UpdateEntity(string entityKey, int id, JsonElement root)
        {
            if (id <= 0)
                throw new InvalidOperationException(L("Selecione um registro valido para editar.", "\u7de8\u96c6\u5bfe\u8c61\u306e\u30ec\u30b3\u30fc\u30c9\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            if (!_entities.TryGetValue(entityKey, out var entity))
                throw new InvalidOperationException(L("Item administrativo invalido.", "\u7121\u52b9\u306a\u7ba1\u7406\u9805\u76ee\u3067\u3059\u3002"));

            if (entity.ReadOnly)
                throw new InvalidOperationException(L("Este item e somente leitura.", "\u3053\u306e\u9805\u76ee\u306f\u95b2\u89a7\u306e\u307f\u3067\u3059\u3002"));

            var values = root.GetProperty("values");
            var parameters = entity.BuildParameters(values);
            parameters.Add("@id", id);

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(entity.UpdateSql, parameters);

            PostJson(new
            {
                type = "updated",
                message = L("Cadastro atualizado com sucesso.", "\u767b\u9332\u3092\u66f4\u65b0\u3057\u307e\u3057\u305f\u3002")
            });

            SendEntityRows(entityKey);
        }

        private void DeleteEntity(string entityKey, int id)
        {
            if (id <= 0)
                throw new InvalidOperationException(L("Selecione um registro valido para excluir.", "\u524a\u9664\u5bfe\u8c61\u306e\u30ec\u30b3\u30fc\u30c9\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            if (!_entities.TryGetValue(entityKey, out var entity))
                throw new InvalidOperationException(L("Item administrativo invalido.", "\u7121\u52b9\u306a\u7ba1\u7406\u9805\u76ee\u3067\u3059\u3002"));

            if (entity.ReadOnly)
                throw new InvalidOperationException(L("Este item e somente leitura.", "\u3053\u306e\u9805\u76ee\u306f\u95b2\u89a7\u306e\u307f\u3067\u3059\u3002"));

            using var conn = _factory.CreateOpenConnection();
            conn.Execute(entity.DeleteSql, new { id });

            PostJson(new
            {
                type = "deleted",
                message = L("Cadastro excluido com sucesso.", "\u767b\u9332\u3092\u524a\u9664\u3057\u307e\u3057\u305f\u3002")
            });

            SendEntityRows(entityKey);
        }

        private Dictionary<string, AdminEntityDefinition> BuildEntities()
        {
            return new[]
            {
                CreateNameEntity(
                    "shift", "core",
                    "Turnos", "\u30b7\u30d5\u30c8",
                    "Base dos turnos usados no dashboard, tasks e filtros.",
                    "\u30c0\u30c3\u30b7\u30e5\u30dc\u30fc\u30c9\u3001tasks\u3001\u30d5\u30a3\u30eb\u30bf\u3067\u4f7f\u3046\u30b7\u30d5\u30c8\u30de\u30b9\u30bf\u3067\u3059\u3002",
                    "Shifts"),
                CreateNameEntity(
                    "group", "core",
                    "Grupos", "\u30b0\u30eb\u30fc\u30d7",
                    "Agrupamentos de lideranca e leitura de area.",
                    "\u30ea\u30fc\u30c0\u30fc\u30b0\u30eb\u30fc\u30d7\u3068\u30a8\u30ea\u30a2\u8aad\u307f\u53d6\u308a\u306e\u57fa\u790e\u30de\u30b9\u30bf\u3067\u3059\u3002",
                    "Groups"),
                CreateNameEntity(
                    "sector", "core",
                    "Setores", "\u30bb\u30af\u30bf\u30fc",
                    "Setores exibidos nas leituras, filtros e presencas.",
                    "\u8aad\u307f\u53d6\u308a\u3001\u30d5\u30a3\u30eb\u30bf\u3001\u51fa\u5e2d\u7ba1\u7406\u3067\u4f7f\u3046\u30bb\u30af\u30bf\u30fc\u3067\u3059\u3002",
                    "Sectors"),
                CreateLocalEntity(),
                CreateNameEntity(
                    "equipment", "production",
                    "Equipamentos", "\u8a2d\u5099",
                    "Equipamentos vinculados aos cadastros operacionais.",
                    "\u904b\u7528\u7cfb\u30d5\u30a9\u30fc\u30e0\u306b\u7d10\u4ed8\u304f\u8a2d\u5099\u30de\u30b9\u30bf\u3067\u3059\u3002",
                    "Equipments"),
                CreateMachineEntity(),
                CreateNameEntity(
                    "category", "production",
                    "Categorias", "\u30ab\u30c6\u30b4\u30ea",
                    "Categorias usadas no Hikitsugui e em outros registros.",
                    "Hikitsugui \u3068\u305d\u306e\u4ed6\u306e\u767b\u9332\u3067\u4f7f\u3046\u30ab\u30c6\u30b4\u30ea\u3067\u3059\u3002",
                    "Categories"),
                CreateNameEntity(
                    "follow_reason", "followup",
                    "Motivos Follow", "\u30d5\u30a9\u30ed\u30fc\u7406\u7531",
                    "Motivos padrao para os acompanhamentos.",
                    "\u30d5\u30a9\u30ed\u30fc\u767b\u9332\u3067\u4f7f\u3046\u7406\u7531\u30de\u30b9\u30bf\u3067\u3059\u3002",
                    "FollowUpReasons"),
                CreateNameEntity(
                    "follow_type", "followup",
                    "Tipos Follow", "\u30d5\u30a9\u30ed\u30fc\u7a2e\u5225",
                    "Tipos de acompanhamento para os formularios de follow.",
                    "\u30d5\u30a9\u30ed\u30fc\u30d5\u30a9\u30fc\u30e0\u3067\u4f7f\u3046\u7a2e\u5225\u30de\u30b9\u30bf\u3067\u3059\u3002",
                    "FollowUpTypes"),
                CreateShainEntity(),
                CreateSystemLogEntity()
            }.ToDictionary(item => item.Key, StringComparer.OrdinalIgnoreCase);
        }

        private static AdminEntityDefinition CreateNameEntity(
            string key,
            string group,
            string titlePt,
            string titleJp,
            string descriptionPt,
            string descriptionJp,
            string tableName)
        {
            return new AdminEntityDefinition
            {
                Key = key,
                Group = group,
                TitlePt = titlePt,
                TitleJp = titleJp,
                DescriptionPt = descriptionPt,
                DescriptionJp = descriptionJp,
                Fields =
                {
                    new AdminFieldDefinition("namePt", "Nome PT", "\u540d\u79f0 PT", "text", true),
                    new AdminFieldDefinition("nameJp", "Nome JP", "\u540d\u79f0 JP", "text", true)
                },
                Columns =
                {
                    new AdminColumnDefinition("id", null, "ID", "ID"),
                    new AdminColumnDefinition("namePt", "nameJp", "Nome", "\u540d\u79f0"),
                    new AdminColumnDefinition("nameJp", null, "Nome JP", "\u540d\u79f0 JP")
                },
                QueryRows = conn => conn.Query(
                    $@"
                        SELECT
                            Id AS id,
                            COALESCE(NamePt, '') AS namePt,
                            COALESCE(NameJp, '') AS nameJp
                        FROM {tableName}
                        ORDER BY Id;"
                ),
                InsertSql = $"INSERT INTO {tableName} (NamePt, NameJp) VALUES (@namePt, @nameJp);",
                UpdateSql = $"UPDATE {tableName} SET NamePt = @namePt, NameJp = @nameJp WHERE Id = @id;",
                DeleteSql = $"DELETE FROM {tableName} WHERE Id = @id;",
                BuildParameters = values =>
                {
                    var namePt = ReadString(values, "namePt").Trim();
                    var nameJp = ReadString(values, "nameJp").Trim();

                    if (string.IsNullOrWhiteSpace(namePt) || string.IsNullOrWhiteSpace(nameJp))
                        throw new InvalidOperationException(L("Preencha os dois campos de nome.", "\u4e21\u65b9\u306e\u540d\u79f0\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    var parameters = new DynamicParameters();
                    parameters.Add("@namePt", namePt);
                    parameters.Add("@nameJp", nameJp);
                    return parameters;
                }
            };
        }

        private static AdminEntityDefinition CreateLocalEntity()
        {
            return new AdminEntityDefinition
            {
                Key = "local",
                Group = "core",
                TitlePt = "Locais",
                TitleJp = "\u5834\u6240",
                DescriptionPt = "Locais vinculados aos setores e usados nos registros.",
                DescriptionJp = "\u30bb\u30af\u30bf\u30fc\u306b\u7d10\u4ed8\u304f\u5834\u6240\u30de\u30b9\u30bf\u3067\u3059\u3002",
                Fields =
                {
                    new AdminFieldDefinition("namePt", "Nome PT", "\u540d\u79f0 PT", "text", true),
                    new AdminFieldDefinition("nameJp", "Nome JP", "\u540d\u79f0 JP", "text", true),
                    new AdminFieldDefinition("sectorId", "Setor", "\u30bb\u30af\u30bf\u30fc", "select", true, "sectors")
                },
                Columns =
                {
                    new AdminColumnDefinition("id", null, "ID", "ID"),
                    new AdminColumnDefinition("namePt", "nameJp", "Nome", "\u540d\u79f0"),
                    new AdminColumnDefinition("sectorNamePt", "sectorNameJp", "Setor", "\u30bb\u30af\u30bf\u30fc")
                },
                QueryRows = conn => conn.Query(
                    @"
                        SELECT
                            l.Id AS id,
                            COALESCE(l.NamePt, '') AS namePt,
                            COALESCE(l.NameJp, '') AS nameJp,
                            l.SectorId AS sectorId,
                            COALESCE(s.NamePt, '') AS sectorNamePt,
                            COALESCE(NULLIF(s.NameJp, ''), s.NamePt, '') AS sectorNameJp
                        FROM Locals l
                        LEFT JOIN Sectors s ON s.Id = l.SectorId
                        ORDER BY l.Id;"
                ),
                InsertSql = "INSERT INTO Locals (NamePt, NameJp, SectorId) VALUES (@namePt, @nameJp, @sectorId);",
                UpdateSql = "UPDATE Locals SET NamePt = @namePt, NameJp = @nameJp, SectorId = @sectorId WHERE Id = @id;",
                DeleteSql = "DELETE FROM Locals WHERE Id = @id;",
                BuildParameters = values =>
                {
                    var namePt = ReadString(values, "namePt").Trim();
                    var nameJp = ReadString(values, "nameJp").Trim();
                    var sectorId = ReadInt(values, "sectorId");

                    if (string.IsNullOrWhiteSpace(namePt) || string.IsNullOrWhiteSpace(nameJp))
                        throw new InvalidOperationException(L("Preencha os dois campos de nome.", "\u4e21\u65b9\u306e\u540d\u79f0\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    if (sectorId <= 0)
                        throw new InvalidOperationException(L("Selecione um setor valido.", "\u6709\u52b9\u306a\u30bb\u30af\u30bf\u30fc\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    var parameters = new DynamicParameters();
                    parameters.Add("@namePt", namePt);
                    parameters.Add("@nameJp", nameJp);
                    parameters.Add("@sectorId", sectorId);
                    return parameters;
                }
            };
        }

        private static AdminEntityDefinition CreateMachineEntity()
        {
            return new AdminEntityDefinition
            {
                Key = "machine",
                Group = "production",
                TitlePt = "Maquinas",
                TitleJp = "\u6a5f\u68b0",
                DescriptionPt = "Cadastro das maquinas do monitor de producao com codigo, linha, setor e local.",
                DescriptionJp = "\u751f\u7523\u30e2\u30cb\u30bf\u30fc\u3067\u4f7f\u3046\u8a2d\u5099\u306e\u30b3\u30fc\u30c9\u3001\u30e9\u30a4\u30f3\u3001\u30bb\u30af\u30bf\u30fc\u3001\u5834\u6240\u3092\u7ba1\u7406\u3057\u307e\u3059\u3002",
                Fields =
                {
                    new AdminFieldDefinition("namePt", "Nome PT", "\u540d\u79f0 PT", "text", true),
                    new AdminFieldDefinition("nameJp", "Nome JP", "\u540d\u79f0 JP", "text", true),
                    new AdminFieldDefinition("machineCode", "Codigo da Maquina", "\u8a2d\u5099\u30b3\u30fc\u30c9", "text", true),
                    new AdminFieldDefinition("lineCode", "Linha", "\u30e9\u30a4\u30f3", "text", false),
                    new AdminFieldDefinition("sectorId", "Setor", "\u30bb\u30af\u30bf\u30fc", "select", false, "sectors"),
                    new AdminFieldDefinition("localId", "Local", "\u5834\u6240", "select", false, "locals")
                },
                Columns =
                {
                    new AdminColumnDefinition("id", null, "ID", "ID"),
                    new AdminColumnDefinition("machineCode", null, "Codigo", "\u30b3\u30fc\u30c9"),
                    new AdminColumnDefinition("namePt", "nameJp", "Nome", "\u540d\u79f0"),
                    new AdminColumnDefinition("lineCode", null, "Linha", "\u30e9\u30a4\u30f3"),
                    new AdminColumnDefinition("sectorNamePt", "sectorNameJp", "Setor", "\u30bb\u30af\u30bf\u30fc"),
                    new AdminColumnDefinition("localNamePt", "localNameJp", "Local", "\u5834\u6240")
                },
                QueryRows = conn => conn.Query(
                    @"
                        SELECT
                            m.Id AS id,
                            COALESCE(m.NamePt, '') AS namePt,
                            COALESCE(m.NameJp, '') AS nameJp,
                            COALESCE(m.MachineCode, '') AS machineCode,
                            COALESCE(m.LineCode, '') AS lineCode,
                            COALESCE(m.SectorId, 0) AS sectorId,
                            COALESCE(m.LocalId, 0) AS localId,
                            COALESCE(s.NamePt, '') AS sectorNamePt,
                            COALESCE(NULLIF(s.NameJp, ''), s.NamePt, '') AS sectorNameJp,
                            COALESCE(l.NamePt, '') AS localNamePt,
                            COALESCE(NULLIF(l.NameJp, ''), l.NamePt, '') AS localNameJp
                        FROM Machines m
                        LEFT JOIN Sectors s ON s.Id = m.SectorId
                        LEFT JOIN Locals l ON l.Id = m.LocalId
                        ORDER BY COALESCE(m.MachineCode, ''), m.Id;"
                ),
                InsertSql = @"
                    INSERT INTO Machines
                    (
                        NamePt,
                        NameJp,
                        MachineCode,
                        LineCode,
                        SectorId,
                        LocalId,
                        IsActive
                    )
                    VALUES
                    (
                        @namePt,
                        @nameJp,
                        @machineCode,
                        @lineCode,
                        @sectorId,
                        @localId,
                        1
                    );",
                UpdateSql = @"
                    UPDATE Machines
                    SET
                        NamePt = @namePt,
                        NameJp = @nameJp,
                        MachineCode = @machineCode,
                        LineCode = @lineCode,
                        SectorId = @sectorId,
                        LocalId = @localId
                    WHERE Id = @id;",
                DeleteSql = "DELETE FROM Machines WHERE Id = @id;",
                BuildParameters = values =>
                {
                    var namePt = ReadString(values, "namePt").Trim();
                    var nameJp = ReadString(values, "nameJp").Trim();
                    var machineCode = ReadString(values, "machineCode").Trim();
                    var lineCode = ReadString(values, "lineCode").Trim();
                    var sectorId = ReadInt(values, "sectorId");
                    var localId = ReadInt(values, "localId");

                    if (string.IsNullOrWhiteSpace(namePt) || string.IsNullOrWhiteSpace(nameJp))
                        throw new InvalidOperationException(L("Preencha os dois campos de nome.", "\u4e21\u65b9\u306e\u540d\u79f0\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    if (string.IsNullOrWhiteSpace(machineCode))
                        throw new InvalidOperationException(L("Informe o codigo da maquina.", "\u8a2d\u5099\u30b3\u30fc\u30c9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    if (localId > 0 && sectorId <= 0)
                        throw new InvalidOperationException(L("Ao definir um local, selecione tambem o setor.", "\u5834\u6240\u3092\u8a2d\u5b9a\u3059\u308b\u5834\u5408\u306f\u30bb\u30af\u30bf\u30fc\u3082\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    var parameters = new DynamicParameters();
                    parameters.Add("@namePt", namePt);
                    parameters.Add("@nameJp", nameJp);
                    parameters.Add("@machineCode", machineCode);
                    parameters.Add("@lineCode", string.IsNullOrWhiteSpace(lineCode) ? null : lineCode);
                    parameters.Add("@sectorId", sectorId > 0 ? sectorId : null);
                    parameters.Add("@localId", localId > 0 ? localId : null);
                    return parameters;
                }
            };
        }

        private static AdminEntityDefinition CreateShainEntity()
        {
            return new AdminEntityDefinition
            {
                Key = "shain",
                Group = "people",
                TitlePt = "Shain",
                TitleJp = "\u793e\u54e1",
                DescriptionPt = "Base simples de nomes romanji e nihongo para referencias internas.",
                DescriptionJp = "\u30ed\u30fc\u30de\u5b57\u540d\u3068\u65e5\u672c\u8a9e\u540d\u3092\u4fdd\u6301\u3059\u308b\u7c21\u6613\u30de\u30b9\u30bf\u3067\u3059\u3002",
                Fields =
                {
                    new AdminFieldDefinition("nameRomanji", "Nome Romanji", "\u30ed\u30fc\u30de\u5b57\u540d", "text", true),
                    new AdminFieldDefinition("nameNihongo", "Nome Nihongo", "\u65e5\u672c\u8a9e\u540d", "text", true)
                },
                Columns =
                {
                    new AdminColumnDefinition("id", null, "ID", "ID"),
                    new AdminColumnDefinition("nameRomanji", "nameNihongo", "Romanji", "\u30ed\u30fc\u30de\u5b57"),
                    new AdminColumnDefinition("nameNihongo", null, "Nihongo", "\u65e5\u672c\u8a9e")
                },
                QueryRows = conn => conn.Query(
                    @"
                        SELECT
                            Id AS id,
                            COALESCE(NameRomanji, '') AS nameRomanji,
                            COALESCE(NameNihongo, '') AS nameNihongo
                        FROM Shain
                        ORDER BY NameRomanji;"
                ),
                InsertSql = "INSERT INTO Shain (NameRomanji, NameNihongo) VALUES (@nameRomanji, @nameNihongo);",
                UpdateSql = "UPDATE Shain SET NameRomanji = @nameRomanji, NameNihongo = @nameNihongo WHERE Id = @id;",
                DeleteSql = "DELETE FROM Shain WHERE Id = @id;",
                BuildParameters = values =>
                {
                    var nameRomanji = ReadString(values, "nameRomanji").Trim();
                    var nameNihongo = ReadString(values, "nameNihongo").Trim();

                    if (string.IsNullOrWhiteSpace(nameRomanji) || string.IsNullOrWhiteSpace(nameNihongo))
                        throw new InvalidOperationException(L("Preencha os dois nomes do shain.", "\u793e\u54e1\u540d\u3092\u4e21\u65b9\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                    var parameters = new DynamicParameters();
                    parameters.Add("@nameRomanji", nameRomanji);
                    parameters.Add("@nameNihongo", nameNihongo);
                    return parameters;
                }
            };
        }

        private static AdminEntityDefinition CreateSystemLogEntity()
        {
            return new AdminEntityDefinition
            {
                Key = "system_log",
                Group = "misc",
                ReadOnly = true,
                TitlePt = "Log do Sistema",
                TitleJp = "\u30b7\u30b9\u30c6\u30e0\u30ed\u30b0",
                DescriptionPt = "Historico de eventos gravados pelo sistema, somente para consulta.",
                DescriptionJp = "\u30b7\u30b9\u30c6\u30e0\u304c\u8a18\u9332\u3057\u305f\u5c65\u6b74\u3092\u95b2\u89a7\u3059\u308b\u305f\u3081\u306e\u4e00\u89a7\u3067\u3059\u3002",
                Columns =
                {
                    new AdminColumnDefinition("timestamp", null, "Data/Hora", "\u65e5\u6642"),
                    new AdminColumnDefinition("userFJ", null, "FJ", "FJ"),
                    new AdminColumnDefinition("module", null, "Modulo", "\u30e2\u30b8\u30e5\u30fc\u30eb"),
                    new AdminColumnDefinition("action", null, "Acao", "\u64cd\u4f5c"),
                    new AdminColumnDefinition("targetId", null, "Target", "\u5bfe\u8c61"),
                    new AdminColumnDefinition("details", null, "Detalhes", "\u8a73\u7d30")
                },
                QueryRows = conn => conn.ExecuteScalar<int>(
                        @"SELECT COUNT(1)
                          FROM sqlite_master
                          WHERE type = 'table'
                            AND name = 'SystemLog';"
                    ) > 0
                        ? conn.Query(
                            @"
                                SELECT
                                    COALESCE(Timestamp, '') AS timestamp,
                                    COALESCE(UserFJ, '') AS userFJ,
                                    COALESCE(Module, '') AS module,
                                    COALESCE(Action, '') AS action,
                                    COALESCE(CAST(TargetId AS TEXT), '') AS targetId,
                                    COALESCE(Details, '') AS details
                                FROM SystemLog
                                ORDER BY Timestamp DESC, Id DESC
                                LIMIT 500;"
                        )
                        : Array.Empty<object>(),
                InsertSql = string.Empty,
                UpdateSql = string.Empty,
                DeleteSql = string.Empty,
                BuildParameters = _ => throw new InvalidOperationException(L("O log do sistema e somente leitura.", "\u30b7\u30b9\u30c6\u30e0\u30ed\u30b0\u306f\u95b2\u89a7\u306e\u307f\u3067\u3059\u3002"))
            };
        }

        private void PostJson(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            webViewAdmin.CoreWebView2.PostWebMessageAsJson(json);
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;
        }

        private static int ReadInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var prop))
                return 0;

            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetInt32(),
                JsonValueKind.String when int.TryParse(prop.GetString(), out var parsed) => parsed,
                _ => 0
            };
        }

        private static string L(string pt, string jp)
        {
            return string.Equals(Program.CurrentLocale, "ja-JP", StringComparison.OrdinalIgnoreCase)
                ? jp
                : pt;
        }

        private sealed class AdminEntityDefinition
        {
            public string Key { get; set; } = string.Empty;
            public string Group { get; set; } = string.Empty;
            public bool ReadOnly { get; set; }
            public string TitlePt { get; set; } = string.Empty;
            public string TitleJp { get; set; } = string.Empty;
            public string DescriptionPt { get; set; } = string.Empty;
            public string DescriptionJp { get; set; } = string.Empty;
            public List<AdminFieldDefinition> Fields { get; } = new();
            public List<AdminColumnDefinition> Columns { get; } = new();
            public Func<IDbConnection, IEnumerable<object>> QueryRows { get; set; } = _ => Array.Empty<object>();
            public string InsertSql { get; set; } = string.Empty;
            public string UpdateSql { get; set; } = string.Empty;
            public string DeleteSql { get; set; } = string.Empty;
            public Func<JsonElement, DynamicParameters> BuildParameters { get; set; } = _ => new DynamicParameters();
        }

        private sealed class AdminFieldDefinition
        {
            public AdminFieldDefinition(string key, string labelPt, string labelJp, string type, bool required, string? lookupKey = null)
            {
                Key = key;
                LabelPt = labelPt;
                LabelJp = labelJp;
                Type = type;
                Required = required;
                LookupKey = lookupKey;
            }

            public string Key { get; }
            public string LabelPt { get; }
            public string LabelJp { get; }
            public string Type { get; }
            public bool Required { get; }
            public string? LookupKey { get; }
        }

        private sealed class AdminColumnDefinition
        {
            public AdminColumnDefinition(string key, string? jpKey, string labelPt, string labelJp)
            {
                Key = key;
                JpKey = jpKey;
                LabelPt = labelPt;
                LabelJp = labelJp;
            }

            public string Key { get; }
            public string? JpKey { get; }
            public string LabelPt { get; }
            public string LabelJp { get; }
        }
    }
}
