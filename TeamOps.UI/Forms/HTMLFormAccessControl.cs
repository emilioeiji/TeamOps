using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Common;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormAccessControl : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly UserRepository _userRepo;

        public HTMLFormAccessControl()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = L("Controle de acesso", "\u30a2\u30af\u30bb\u30b9\u7ba1\u7406");

            _factory = Program.ConnectionFactory;
            _userRepo = new UserRepository(_factory);

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewAccessControl.EnsureCoreWebView2Async(null);

            var core = webViewAccessControl.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "access-control"),
                CoreWebView2HostResourceAccessKind.Allow
            );

            core.Navigate("https://app/index.html");
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = JsonSerializer.Deserialize<JsRequest>(e.WebMessageAsJson);
            if (msg == null)
                return;

            switch (msg.action)
            {
                case "load":
                    LoadInitialData();
                    break;

                case "create":
                    CreateUser(msg);
                    break;

                case "update":
                    UpdateUser(msg);
                    break;

                case "reset_password":
                    ResetPassword(msg);
                    break;
            }
        }

        private void LoadInitialData()
        {
            using var conn = _factory.CreateOpenConnection();

            PostJson(new
            {
                type = "init",
                locale = Program.CurrentLocale,
                accessLevels = Enum.GetValues(typeof(AccessLevel))
                    .Cast<AccessLevel>()
                    .Select(level => new
                    {
                        value = (int)level,
                        label = GetAccessLevelLabel(level)
                    }),
                rows = QueryUsers(conn).ToList()
            });
        }

        private void CreateUser(JsRequest msg)
        {
            try
            {
                var login = NormalizeLogin(msg.login);
                var password = msg.password?.Trim() ?? string.Empty;
                var confirmPassword = msg.confirmPassword?.Trim() ?? string.Empty;

                ValidateCreatePayload(msg, login, password, confirmPassword);

                if (_userRepo.GetByLogin(login) != null)
                    throw new InvalidOperationException(L("Ja existe um usuario com este login.", "\u3053\u306e\u30ed\u30b0\u30a4\u30f3\u306e\u30e6\u30fc\u30b6\u30fc\u306f\u65e2\u306b\u5b58\u5728\u3057\u307e\u3059\u3002"));

                var level = ParseAccessLevel(msg.accessLevel);

                _userRepo.Add(new User
                {
                    Login = login,
                    CodigoFJ = login,
                    Name = (msg.name ?? string.Empty).Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    AccessLevel = level,
                    CreatedAt = DateTime.Now
                });

                PostJson(new
                {
                    type = "created",
                    message = L("Usuario cadastrado com sucesso.", "\u30e6\u30fc\u30b6\u30fc\u3092\u767b\u9332\u3057\u307e\u3057\u305f\u3002")
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

        private void UpdateUser(JsRequest msg)
        {
            try
            {
                var login = NormalizeLogin(msg.login);
                ValidateUpdatePayload(msg, login);

                var existing = _userRepo.GetByLogin(login)
                    ?? throw new InvalidOperationException(L("Usuario nao encontrado para atualizacao.", "\u66f4\u65b0\u5bfe\u8c61\u306e\u30e6\u30fc\u30b6\u30fc\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093\u3002"));

                existing.Name = (msg.name ?? string.Empty).Trim();
                existing.AccessLevel = ParseAccessLevel(msg.accessLevel);

                _userRepo.Update(existing);

                PostJson(new
                {
                    type = "updated",
                    message = L("Usuario atualizado com sucesso.", "\u30e6\u30fc\u30b6\u30fc\u3092\u66f4\u65b0\u3057\u307e\u3057\u305f\u3002")
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

        private void ResetPassword(JsRequest msg)
        {
            try
            {
                var login = NormalizeLogin(msg.login);
                var password = msg.password?.Trim() ?? string.Empty;
                var confirmPassword = msg.confirmPassword?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(login))
                    throw new InvalidOperationException(L("Selecione um usuario para redefinir a senha.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u3092\u518d\u8a2d\u5b9a\u3059\u308b\u30e6\u30fc\u30b6\u30fc\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException(L("Informe a nova senha.", "\u65b0\u3057\u3044\u30d1\u30b9\u30ef\u30fc\u30c9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

                if (password != confirmPassword)
                    throw new InvalidOperationException(L("A confirmacao da senha nao confere.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u78ba\u8a8d\u304c\u4e00\u81f4\u3057\u307e\u305b\u3093\u3002"));

                var existing = _userRepo.GetByLogin(login)
                    ?? throw new InvalidOperationException(L("Usuario nao encontrado para redefinicao de senha.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u518d\u8a2d\u5b9a\u5bfe\u8c61\u306e\u30e6\u30fc\u30b6\u30fc\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093\u3002"));

                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                _userRepo.Update(existing);

                PostJson(new
                {
                    type = "password_reset",
                    message = L("Senha redefinida com sucesso.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u3092\u518d\u8a2d\u5b9a\u3057\u307e\u3057\u305f\u3002")
                });
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
                data = QueryUsers(conn).ToList()
            });
        }

        private static string NormalizeLogin(string? login)
        {
            return (login ?? string.Empty).Trim();
        }

        private static void ValidateCreatePayload(
            JsRequest msg,
            string login,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException(L("Informe o login.", "\u30ed\u30b0\u30a4\u30f3\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(L("Informe a senha.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u3092\u5165\u529b\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            if (password != confirmPassword)
                throw new InvalidOperationException(L("A confirmacao da senha nao confere.", "\u30d1\u30b9\u30ef\u30fc\u30c9\u78ba\u8a8d\u304c\u4e00\u81f4\u3057\u307e\u305b\u3093\u3002"));

            _ = ParseAccessLevel(msg.accessLevel);
        }

        private static void ValidateUpdatePayload(JsRequest msg, string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException(L("Selecione um usuario para editar.", "\u7de8\u96c6\u3059\u308b\u30e6\u30fc\u30b6\u30fc\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            _ = ParseAccessLevel(msg.accessLevel);
        }

        private static AccessLevel ParseAccessLevel(int rawValue)
        {
            if (!Enum.IsDefined(typeof(AccessLevel), rawValue))
                throw new InvalidOperationException(L("Selecione um nivel de acesso valido.", "\u6709\u52b9\u306a\u30a2\u30af\u30bb\u30b9\u30ec\u30d9\u30eb\u3092\u9078\u629e\u3057\u3066\u304f\u3060\u3055\u3044\u3002"));

            return (AccessLevel)rawValue;
        }

        private static string GetAccessLevelLabel(AccessLevel level)
        {
            return level switch
            {
                AccessLevel.Basic => L("Basico", "\u57fa\u672c"),
                AccessLevel.KL => "KL",
                AccessLevel.GL => "GL",
                AccessLevel.Manager => L("Gerente", "\u30de\u30cd\u30fc\u30b8\u30e3\u30fc"),
                AccessLevel.Admin => L("Admin", "\u7ba1\u7406\u8005"),
                _ => level.ToString()
            };
        }

        private static string L(string pt, string jp)
        {
            return string.Equals(Program.CurrentLocale, "ja-JP", StringComparison.OrdinalIgnoreCase)
                ? jp
                : pt;
        }

        private static System.Collections.Generic.IEnumerable<object> QueryUsers(System.Data.IDbConnection conn)
        {
            const string sql = @"
                SELECT
                    Id,
                    Login,
                    COALESCE(CodigoFJ, '') AS CodigoFJ,
                    COALESCE(Name, '') AS Name,
                    AccessLevel,
                    strftime('%Y-%m-%d %H:%M', CreatedAt) AS CreatedAt
                FROM Users
                ORDER BY
                    AccessLevel DESC,
                    CASE
                        WHEN COALESCE(Name, '') = '' THEN Login
                        ELSE Name
                    END COLLATE NOCASE,
                    Login COLLATE NOCASE;";

            return conn.Query<UserListRow>(sql)
                .Select(row => new
                {
                    id = row.Id,
                    login = row.Login,
                    codigoFJ = row.CodigoFJ,
                    name = row.Name,
                    accessLevel = row.AccessLevel,
                    accessLabel = GetAccessLevelLabel((AccessLevel)row.AccessLevel),
                    createdAt = row.CreatedAt
                });
        }

        private void PostJson(object payload)
        {
            webViewAccessControl.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(payload)
            );
        }

        private sealed class UserListRow
        {
            public int Id { get; set; }
            public string Login { get; set; } = string.Empty;
            public string CodigoFJ { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int AccessLevel { get; set; }
            public string CreatedAt { get; set; } = string.Empty;
        }
    }
}
