using Dapper;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Forms
{
    public partial class HTMLFormTasks : Form
    {
        private readonly SqliteConnectionFactory _factory;
        private readonly User _currentUser;
        private readonly Operator _currentOperator;

        private static readonly (string Value, string Label)[] TaskStatuses =
        {
            ("pending", "Pendente"),
            ("in_progress", "Em andamento"),
            ("blocked", "Bloqueada"),
            ("completed", "Concluida"),
            ("cancelled", "Cancelada")
        };

        public HTMLFormTasks(
            SqliteConnectionFactory factory,
            User currentUser,
            Operator currentOperator)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _factory = factory;
            _currentUser = currentUser;
            _currentOperator = currentOperator;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webViewTasks.EnsureCoreWebView2Async(null);

            var core = webViewTasks.CoreWebView2;

            core.Settings.IsWebMessageEnabled = true;
            core.Settings.AreDefaultScriptDialogsEnabled = true;
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;

            core.WebMessageReceived += WebMessageReceived;

            core.SetVirtualHostNameToFolderMapping(
                "app",
                Path.Combine(Application.StartupPath, "ui", "tasks"),
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

                case "load_details":
                    LoadTaskDetails(msg.id);
                    break;

                case "save":
                    SaveTask(msg);
                    break;

                case "update":
                    UpdateTask(msg);
                    break;

                case "change_status":
                    ChangeStatus(msg);
                    break;
            }
        }

        private void LoadInitialData()
        {
            using var conn = _factory.CreateOpenConnection();
            EnsureSchema(conn);

            PostJson(new
            {
                type = "init",
                currentOperatorName = _currentOperator.NameRomanji,
                currentUser = _currentUser.Name,
                statuses = TaskStatuses.Select(status => new
                {
                    value = status.Value,
                    label = status.Label
                }),
                shifts = conn.Query(
                    @"SELECT Id, NamePt
                      FROM Shifts
                      ORDER BY Id"
                ).ToList(),
                leaders = conn.Query(
                    @"SELECT
                          CodigoFJ,
                          NameRomanji,
                          ShiftId
                      FROM Operators
                      WHERE Status = 1
                        AND IsLeader = 1
                      ORDER BY ShiftId, NameRomanji"
                ).ToList(),
                rows = QueryTasks(conn).ToList()
            });
        }

        private void LoadTaskDetails(int taskId)
        {
            try
            {
                using var conn = _factory.CreateOpenConnection();
                EnsureSchema(conn);

                if (!TaskExists(conn, taskId))
                    throw new InvalidOperationException("Task nao encontrada.");

                PostJson(new
                {
                    type = "task_details",
                    id = taskId,
                    history = QueryTaskHistory(conn, taskId).ToList()
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

        private void SaveTask(JsRequest msg)
        {
            try
            {
                using var conn = _factory.CreateOpenConnection();
                EnsureSchema(conn);

                var description = NormalizeDescription(msg.description);
                var dueDate = ParseDueDate(msg.dueDate);
                var taskStatus = NormalizeTaskStatus(msg.taskStatus);
                var leaderCodigoFJ = NormalizeLeader(msg.leaderCodigoFJ);

                ValidatePayload(conn, description, dueDate, msg.shiftId, leaderCodigoFJ, taskStatus, false, 0);

                var now = DateTime.Now;
                var timestamps = CreateTimestampsForNewTask(taskStatus, now);

                using var tx = conn.BeginTransaction();

                var newId = conn.QuerySingle<int>(
                    @"
                        INSERT INTO Tasks
                        (
                            Description,
                            DueDate,
                            ShiftId,
                            AssigneeCodigoFJ,
                            Status,
                            CreatedByCodigoFJ,
                            CreatedAt,
                            UpdatedAt,
                            StartedAt,
                            CompletedAt,
                            CancelledAt
                        )
                        VALUES
                        (
                            @Description,
                            @DueDate,
                            @ShiftId,
                            @AssigneeCodigoFJ,
                            @Status,
                            @CreatedByCodigoFJ,
                            @CreatedAt,
                            @UpdatedAt,
                            @StartedAt,
                            @CompletedAt,
                            @CancelledAt
                        );
                        SELECT last_insert_rowid();",
                    new
                    {
                        Description = description,
                        DueDate = dueDate.ToString("yyyy-MM-dd"),
                        ShiftId = msg.shiftId,
                        AssigneeCodigoFJ = string.IsNullOrWhiteSpace(leaderCodigoFJ) ? null : leaderCodigoFJ,
                        Status = taskStatus,
                        CreatedByCodigoFJ = _currentOperator.CodigoFJ,
                        CreatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        StartedAt = timestamps.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CompletedAt = timestamps.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CancelledAt = timestamps.CancelledAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    tx
                );

                InsertTaskHistory(conn, tx, newId, null, taskStatus, _currentOperator.CodigoFJ, "Task criada.");
                tx.Commit();

                PostJson(new
                {
                    type = "saved",
                    message = "Task cadastrada com sucesso."
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

        private void UpdateTask(JsRequest msg)
        {
            try
            {
                using var conn = _factory.CreateOpenConnection();
                EnsureSchema(conn);

                var current = GetTaskState(conn, msg.id)
                    ?? throw new InvalidOperationException("Task nao encontrada para edicao.");

                var description = NormalizeDescription(msg.description);
                var dueDate = ParseDueDate(msg.dueDate);
                var taskStatus = NormalizeTaskStatus(msg.taskStatus);
                var leaderCodigoFJ = NormalizeLeader(msg.leaderCodigoFJ);

                ValidatePayload(conn, description, dueDate, msg.shiftId, leaderCodigoFJ, taskStatus, true, msg.id);

                var now = DateTime.Now;
                var timestamps = ResolveTimestampsForStatus(current, taskStatus, now);
                var statusChanged = !string.Equals(current.Status, taskStatus, StringComparison.OrdinalIgnoreCase);

                using var tx = conn.BeginTransaction();

                conn.Execute(
                    @"
                        UPDATE Tasks
                        SET
                            Description = @Description,
                            DueDate = @DueDate,
                            ShiftId = @ShiftId,
                            AssigneeCodigoFJ = @AssigneeCodigoFJ,
                            Status = @Status,
                            UpdatedAt = @UpdatedAt,
                            StartedAt = @StartedAt,
                            CompletedAt = @CompletedAt,
                            CancelledAt = @CancelledAt
                        WHERE Id = @Id;",
                    new
                    {
                        Id = msg.id,
                        Description = description,
                        DueDate = dueDate.ToString("yyyy-MM-dd"),
                        ShiftId = msg.shiftId,
                        AssigneeCodigoFJ = string.IsNullOrWhiteSpace(leaderCodigoFJ) ? null : leaderCodigoFJ,
                        Status = taskStatus,
                        UpdatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        StartedAt = timestamps.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CompletedAt = timestamps.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CancelledAt = timestamps.CancelledAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    tx
                );

                if (statusChanged)
                    InsertTaskHistory(conn, tx, msg.id, current.Status, taskStatus, _currentOperator.CodigoFJ, "Status alterado na edicao da task.");

                tx.Commit();

                PostJson(new
                {
                    type = "updated",
                    message = "Task atualizada com sucesso."
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

        private void ChangeStatus(JsRequest msg)
        {
            try
            {
                using var conn = _factory.CreateOpenConnection();
                EnsureSchema(conn);

                var current = GetTaskState(conn, msg.id)
                    ?? throw new InvalidOperationException("Task nao encontrada para atualizar status.");

                var taskStatus = NormalizeTaskStatus(msg.taskStatus);
                if (string.Equals(current.Status, taskStatus, StringComparison.OrdinalIgnoreCase))
                {
                    PostJson(new
                    {
                        type = "status_changed",
                        message = "A task ja estava com este status."
                    });
                    return;
                }

                var now = DateTime.Now;
                var timestamps = ResolveTimestampsForStatus(current, taskStatus, now);

                using var tx = conn.BeginTransaction();

                conn.Execute(
                    @"
                        UPDATE Tasks
                        SET
                            Status = @Status,
                            UpdatedAt = @UpdatedAt,
                            StartedAt = @StartedAt,
                            CompletedAt = @CompletedAt,
                            CancelledAt = @CancelledAt
                        WHERE Id = @Id;",
                    new
                    {
                        Id = msg.id,
                        Status = taskStatus,
                        UpdatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        StartedAt = timestamps.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CompletedAt = timestamps.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        CancelledAt = timestamps.CancelledAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    tx
                );

                InsertTaskHistory(conn, tx, msg.id, current.Status, taskStatus, _currentOperator.CodigoFJ, "Status alterado rapidamente.");
                tx.Commit();

                PostJson(new
                {
                    type = "status_changed",
                    message = taskStatus == "completed"
                        ? "Task finalizada com sucesso."
                        : "Status da task atualizado com sucesso."
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

        private void SendRows(System.Data.IDbConnection conn)
        {
            PostJson(new
            {
                type = "rows",
                data = QueryTasks(conn).ToList()
            });
        }

        private static string NormalizeDescription(string? description)
        {
            return (description ?? string.Empty).Trim();
        }

        private static string NormalizeLeader(string? leaderCodigoFJ)
        {
            return (leaderCodigoFJ ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static DateTime ParseDueDate(string? dueDate)
        {
            if (!DateTime.TryParse(dueDate, out var parsed))
                throw new InvalidOperationException("Informe uma data valida para a task.");

            return parsed.Date;
        }

        private static string NormalizeTaskStatus(string? taskStatus)
        {
            var normalized = (taskStatus ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalized))
                return "pending";

            if (!TaskStatuses.Any(status => status.Value == normalized))
                throw new InvalidOperationException("Selecione um status valido para a task.");

            return normalized;
        }

        private static string GetTaskStatusLabel(string taskStatus)
        {
            return TaskStatuses.FirstOrDefault(status => status.Value == taskStatus).Label ?? taskStatus;
        }

        private static void ValidatePayload(
            System.Data.IDbConnection conn,
            string description,
            DateTime dueDate,
            int shiftId,
            string leaderCodigoFJ,
            string taskStatus,
            bool isUpdate,
            int taskId)
        {
            if (isUpdate && taskId <= 0)
                throw new InvalidOperationException("Selecione uma task valida para editar.");

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException("Informe o que deve ser feito na task.");

            if (shiftId <= 0)
                throw new InvalidOperationException("Selecione o turno da task.");

            if (dueDate == default)
                throw new InvalidOperationException("Informe quando a task deve ser concluida.");

            if (!TaskStatuses.Any(status => status.Value == taskStatus))
                throw new InvalidOperationException("Status da task invalido.");

            if (!string.IsNullOrWhiteSpace(leaderCodigoFJ))
            {
                var isValidLeader = conn.ExecuteScalar<int>(
                    @"
                        SELECT COUNT(1)
                        FROM Operators
                        WHERE CodigoFJ = @codigoFJ
                          AND Status = 1
                          AND IsLeader = 1
                          AND ShiftId = @shiftId;",
                    new
                    {
                        codigoFJ = leaderCodigoFJ,
                        shiftId
                    }
                ) > 0;

                if (!isValidLeader)
                    throw new InvalidOperationException("O lider selecionado nao pertence ao turno informado.");
            }
        }

        private static bool TaskExists(System.Data.IDbConnection conn, int taskId)
        {
            return conn.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Tasks WHERE Id = @id;",
                new { id = taskId }
            ) > 0;
        }

        private static TaskStateRow? GetTaskState(System.Data.IDbConnection conn, int taskId)
        {
            return conn.QueryFirstOrDefault<TaskStateRow>(
                @"
                    SELECT
                        Id,
                        Status,
                        StartedAt,
                        CompletedAt,
                        CancelledAt
                    FROM Tasks
                    WHERE Id = @id;",
                new { id = taskId }
            );
        }

        private static TaskTimestamps CreateTimestampsForNewTask(string status, DateTime now)
        {
            DateTime? startedAt = null;
            DateTime? completedAt = null;
            DateTime? cancelledAt = null;

            if (status is "in_progress" or "blocked" or "completed")
                startedAt = now;

            if (status == "completed")
                completedAt = now;

            if (status == "cancelled")
                cancelledAt = now;

            return new TaskTimestamps(startedAt, completedAt, cancelledAt);
        }

        private static TaskTimestamps ResolveTimestampsForStatus(TaskStateRow current, string newStatus, DateTime now)
        {
            DateTime? startedAt = current.StartedAt;
            DateTime? completedAt = current.CompletedAt;
            DateTime? cancelledAt = current.CancelledAt;

            if (newStatus is "in_progress" or "blocked" or "completed")
                startedAt ??= now;

            if (newStatus == "completed")
            {
                completedAt ??= now;
                cancelledAt = null;
            }
            else if (newStatus == "cancelled")
            {
                cancelledAt = now;
                completedAt = null;
            }
            else
            {
                if (string.Equals(current.Status, "completed", StringComparison.OrdinalIgnoreCase))
                    completedAt = null;

                if (string.Equals(current.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                    cancelledAt = null;
            }

            return new TaskTimestamps(startedAt, completedAt, cancelledAt);
        }

        private static void InsertTaskHistory(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction tx,
            int taskId,
            string? previousStatus,
            string newStatus,
            string changedByCodigoFJ,
            string note)
        {
            conn.Execute(
                @"
                    INSERT INTO TaskStatusHistory
                    (
                        TaskId,
                        PreviousStatus,
                        NewStatus,
                        ChangedByCodigoFJ,
                        ChangedAt,
                        Note
                    )
                    VALUES
                    (
                        @TaskId,
                        @PreviousStatus,
                        @NewStatus,
                        @ChangedByCodigoFJ,
                        @ChangedAt,
                        @Note
                    );",
                new
                {
                    TaskId = taskId,
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus,
                    ChangedByCodigoFJ = changedByCodigoFJ,
                    ChangedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Note = note
                },
                tx
            );
        }

        private static IEnumerable<dynamic> QueryTasks(System.Data.IDbConnection conn)
        {
            const string sql = @"
                SELECT
                    t.Id AS id,
                    t.Description AS description,
                    substr(t.DueDate, 1, 10) AS dueDate,
                    t.ShiftId AS shiftId,
                    sh.NamePt AS shiftName,
                    COALESCE(t.AssigneeCodigoFJ, '') AS leaderCodigoFJ,
                    COALESCE(op.NameRomanji, '') AS leaderName,
                    t.Status AS taskStatus,
                    substr(t.CreatedAt, 1, 16) AS createdAt,
                    substr(t.UpdatedAt, 1, 16) AS updatedAt,
                    CASE WHEN t.StartedAt IS NULL THEN '' ELSE substr(t.StartedAt, 1, 16) END AS startedAt,
                    CASE WHEN t.CompletedAt IS NULL THEN '' ELSE substr(t.CompletedAt, 1, 16) END AS completedAt,
                    CASE WHEN t.CancelledAt IS NULL THEN '' ELSE substr(t.CancelledAt, 1, 16) END AS cancelledAt,
                    COALESCE(creator.NameRomanji, t.CreatedByCodigoFJ) AS createdByName,
                    t.CreatedByCodigoFJ AS createdByCodigoFJ
                FROM Tasks t
                LEFT JOIN Shifts sh ON sh.Id = t.ShiftId
                LEFT JOIN Operators op ON op.CodigoFJ = t.AssigneeCodigoFJ
                LEFT JOIN Operators creator ON creator.CodigoFJ = t.CreatedByCodigoFJ
                ORDER BY
                    CASE WHEN t.Status IN ('completed', 'cancelled') THEN 1 ELSE 0 END,
                    date(t.DueDate) ASC,
                    t.Id DESC;";

            return conn.Query(sql).Select(row => new
            {
                id = row.id,
                description = row.description,
                dueDate = row.dueDate,
                shiftId = row.shiftId,
                shiftName = row.shiftName,
                leaderCodigoFJ = row.leaderCodigoFJ,
                leaderName = row.leaderName,
                taskStatus = row.taskStatus,
                statusLabel = GetTaskStatusLabel((string)row.taskStatus),
                createdAt = row.createdAt,
                updatedAt = row.updatedAt,
                startedAt = row.startedAt,
                completedAt = row.completedAt,
                cancelledAt = row.cancelledAt,
                createdByName = row.createdByName,
                createdByCodigoFJ = row.createdByCodigoFJ
            });
        }

        private static IEnumerable<dynamic> QueryTaskHistory(System.Data.IDbConnection conn, int taskId)
        {
            const string sql = @"
                SELECT
                    h.Id AS id,
                    COALESCE(h.PreviousStatus, '') AS previousStatus,
                    h.NewStatus AS newStatus,
                    COALESCE(changer.NameRomanji, h.ChangedByCodigoFJ) AS changedByName,
                    substr(h.ChangedAt, 1, 16) AS changedAt,
                    COALESCE(h.Note, '') AS note
                FROM TaskStatusHistory h
                LEFT JOIN Operators changer ON changer.CodigoFJ = h.ChangedByCodigoFJ
                WHERE h.TaskId = @taskId
                ORDER BY h.Id DESC;";

            return conn.Query(sql, new { taskId }).Select(row => new
            {
                id = row.id,
                previousStatus = row.previousStatus,
                previousStatusLabel = string.IsNullOrWhiteSpace((string)row.previousStatus) ? string.Empty : GetTaskStatusLabel((string)row.previousStatus),
                newStatus = row.newStatus,
                newStatusLabel = GetTaskStatusLabel((string)row.newStatus),
                changedByName = row.changedByName,
                changedAt = row.changedAt,
                note = row.note
            });
        }

        private static void EnsureSchema(System.Data.IDbConnection conn)
        {
            conn.Execute(
                @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Description TEXT NOT NULL,
                        DueDate DATE NOT NULL,
                        ShiftId INTEGER NOT NULL,
                        AssigneeCodigoFJ TEXT,
                        Status TEXT NOT NULL,
                        CreatedByCodigoFJ TEXT NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        StartedAt DATETIME,
                        CompletedAt DATETIME,
                        CancelledAt DATETIME,
                        FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
                        FOREIGN KEY (AssigneeCodigoFJ) REFERENCES Operators(CodigoFJ),
                        FOREIGN KEY (CreatedByCodigoFJ) REFERENCES Operators(CodigoFJ)
                    );

                    CREATE TABLE IF NOT EXISTS TaskStatusHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskId INTEGER NOT NULL,
                        PreviousStatus TEXT,
                        NewStatus TEXT NOT NULL,
                        ChangedByCodigoFJ TEXT NOT NULL,
                        ChangedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        Note TEXT,
                        FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
                        FOREIGN KEY (ChangedByCodigoFJ) REFERENCES Operators(CodigoFJ)
                    );

                    CREATE INDEX IF NOT EXISTS IX_Tasks_Status_DueDate
                    ON Tasks(Status, DueDate);

                    CREATE INDEX IF NOT EXISTS IX_Tasks_Shift_Assignee
                    ON Tasks(ShiftId, AssigneeCodigoFJ);

                    CREATE INDEX IF NOT EXISTS IX_TaskStatusHistory_TaskId
                    ON TaskStatusHistory(TaskId, ChangedAt);"
            );
        }

        private void PostJson(object payload)
        {
            webViewTasks.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(payload)
            );
        }

        private sealed class TaskStateRow
        {
            public int Id { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime? CancelledAt { get; set; }
        }

        private sealed class TaskTimestamps
        {
            public TaskTimestamps(DateTime? startedAt, DateTime? completedAt, DateTime? cancelledAt)
            {
                StartedAt = startedAt;
                CompletedAt = completedAt;
                CancelledAt = cancelledAt;
            }

            public DateTime? StartedAt { get; }
            public DateTime? CompletedAt { get; }
            public DateTime? CancelledAt { get; }
        }
    }
}
