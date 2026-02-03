using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class HikitsuguiRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public HikitsuguiRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        // ---------------------------------------------------------
        // INSERT
        // ---------------------------------------------------------
        public int Add(Hikitsugui h)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO Hikitsugui
                (Date, ShiftId, CreatorCodigoFJ, CategoryId, EquipmentId, LocalId,
                 ForLeaders, ForOperators, Description, AttachmentPath)
                VALUES
                (@date, @shift, @creator, @category, @equip, @local,
                 @leaders, @operators, @desc, @attach);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@date", h.Date);
            cmd.Parameters.AddWithValue("@shift", h.ShiftId);
            cmd.Parameters.AddWithValue("@creator", h.CreatorCodigoFJ);
            cmd.Parameters.AddWithValue("@category", h.CategoryId);
            cmd.Parameters.AddWithValue("@equip", (object?)h.EquipmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@local", (object?)h.LocalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@leaders", h.ForLeaders ? 1 : 0);
            cmd.Parameters.AddWithValue("@operators", h.ForOperators ? 1 : 0);
            cmd.Parameters.AddWithValue("@desc", h.Description);
            cmd.Parameters.AddWithValue("@attach", (object?)h.AttachmentPath ?? DBNull.Value);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        // ---------------------------------------------------------
        // UPDATE AttachmentPath
        // ---------------------------------------------------------
        public void UpdateAttachmentPath(int id, string path)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                UPDATE Hikitsugui
                SET AttachmentPath = @path
                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        // ---------------------------------------------------------
        // GET BY ID
        // ---------------------------------------------------------
        public Hikitsugui? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, Date, ShiftId, CreatorCodigoFJ, CategoryId,
                       EquipmentId, LocalId, ForLeaders, ForOperators,
                       Description, AttachmentPath
                FROM Hikitsugui
                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new Hikitsugui
            {
                Id = reader.GetInt32(0),
                Date = reader.GetDateTime(1),
                ShiftId = reader.GetInt32(2),
                CreatorCodigoFJ = reader.GetString(3),
                CategoryId = reader.GetInt32(4),
                EquipmentId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                LocalId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                ForLeaders = reader.GetInt32(7) == 1,
                ForOperators = reader.GetInt32(8) == 1,
                Description = reader.GetString(9),
                AttachmentPath = reader.IsDBNull(10) ? null : reader.GetString(10)
            };
        }

        // ---------------------------------------------------------
        // GET ALL
        // ---------------------------------------------------------
        public List<Hikitsugui> GetAll()
        {
            var list = new List<Hikitsugui>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, Date, ShiftId, CreatorCodigoFJ, CategoryId,
                       EquipmentId, LocalId, ForLeaders, ForOperators,
                       Description, AttachmentPath
                FROM Hikitsugui
                ORDER BY Date DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Hikitsugui
                {
                    Id = reader.GetInt32(0),
                    Date = reader.GetDateTime(1),
                    ShiftId = reader.GetInt32(2),
                    CreatorCodigoFJ = reader.GetString(3),
                    CategoryId = reader.GetInt32(4),
                    EquipmentId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                    LocalId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                    ForLeaders = reader.GetInt32(7) == 1,
                    ForOperators = reader.GetInt32(8) == 1,
                    Description = reader.GetString(9),
                    AttachmentPath = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }

            return list;
        }

        // ---------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------
        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "DELETE FROM Hikitsugui WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
