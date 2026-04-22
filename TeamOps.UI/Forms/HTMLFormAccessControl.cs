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
                    throw new InvalidOperationException("Ja existe um usuario com este login.");

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
                    message = "Usuario cadastrado com sucesso."
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
                    ?? throw new InvalidOperationException("Usuario nao encontrado para atualizacao.");

                existing.Name = (msg.name ?? string.Empty).Trim();
                existing.AccessLevel = ParseAccessLevel(msg.accessLevel);

                _userRepo.Update(existing);

                PostJson(new
                {
                    type = "updated",
                    message = "Usuario atualizado com sucesso."
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
                    throw new InvalidOperationException("Selecione um usuario para redefinir a senha.");

                if (string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Informe a nova senha.");

                if (password != confirmPassword)
                    throw new InvalidOperationException("A confirmacao da senha nao confere.");

                var existing = _userRepo.GetByLogin(login)
                    ?? throw new InvalidOperationException("Usuario nao encontrado para redefinicao de senha.");

                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                _userRepo.Update(existing);

                PostJson(new
                {
                    type = "password_reset",
                    message = "Senha redefinida com sucesso."
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
                throw new InvalidOperationException("Informe o login.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Informe a senha.");

            if (password != confirmPassword)
                throw new InvalidOperationException("A confirmacao da senha nao confere.");

            _ = ParseAccessLevel(msg.accessLevel);
        }

        private static void ValidateUpdatePayload(JsRequest msg, string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException("Selecione um usuario para editar.");

            _ = ParseAccessLevel(msg.accessLevel);
        }

        private static AccessLevel ParseAccessLevel(int rawValue)
        {
            if (!Enum.IsDefined(typeof(AccessLevel), rawValue))
                throw new InvalidOperationException("Selecione um nivel de acesso valido.");

            return (AccessLevel)rawValue;
        }

        private static string GetAccessLevelLabel(AccessLevel level)
        {
            return level switch
            {
                AccessLevel.Basic => "Basic",
                AccessLevel.KL => "KL",
                AccessLevel.GL => "GL",
                AccessLevel.Manager => "Manager",
                AccessLevel.Admin => "Admin",
                _ => level.ToString()
            };
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
