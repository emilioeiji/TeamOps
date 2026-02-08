using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using static Dapper.SqlMapper;

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
        (Date, ShiftId, CreatorCodigoFJ, CategoryId, EquipmentId, LocalId, SectorId, ForLeaders, ForOperators, Description, AttachmentPath)
        VALUES
        (@Date, @ShiftId, @CreatorCodigoFJ, @CategoryId, @EquipmentId, @LocalId, @SectorId, @ForLeaders, @ForOperators, @Description, @AttachmentPath);

        SELECT last_insert_rowid();
    ";

            cmd.Parameters.AddWithValue("@Date", h.Date);
            cmd.Parameters.AddWithValue("@ShiftId", h.ShiftId);
            cmd.Parameters.AddWithValue("@CreatorCodigoFJ", h.CreatorCodigoFJ);
            cmd.Parameters.AddWithValue("@CategoryId", h.CategoryId);
            cmd.Parameters.AddWithValue("@EquipmentId", (object?)h.EquipmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LocalId", (object?)h.LocalId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SectorId", (object?)h.SectorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ForLeaders", h.ForLeaders ? 1 : 0);
            cmd.Parameters.AddWithValue("@ForOperators", h.ForOperators ? 1 : 0);
            cmd.Parameters.AddWithValue("@Description", h.Description ?? "");
            cmd.Parameters.AddWithValue("@AttachmentPath", (object?)h.AttachmentPath ?? DBNull.Value);

            object? result = cmd.ExecuteScalar();

            return Convert.ToInt32(result);
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
                       EquipmentId, LocalId, SectorId, ForLeaders, ForOperators,
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
                SectorId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                ForLeaders = reader.GetInt32(8) == 1,
                ForOperators = reader.GetInt32(9) == 1,
                Description = reader.GetString(10),
                AttachmentPath = reader.IsDBNull(11) ? null : reader.GetString(11)
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
        // GET FOR LEADER (Filtro por data + ForLeaders/ForOperators)
        // ---------------------------------------------------------
        public List<Hikitsugui> GetForLeader(DateTime start, DateTime end)
        {
            var list = new List<Hikitsugui>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                    h.Id,
                    h.Date,
                    h.ShiftId,
                    h.CreatorCodigoFJ,
                    h.CategoryId,
                    h.EquipmentId,
                    h.LocalId,
                    h.SectorId,
                    h.ForLeaders,
                    h.ForOperators,
                    h.Description,
                    h.AttachmentPath,
                    c.NamePt AS CategoryName,
                    s.NamePt AS SectorName
                FROM Hikitsugui h
                LEFT JOIN Categories c ON c.Id = h.CategoryId
                LEFT JOIN Sectors s ON s.Id = h.SectorId
                WHERE h.Date >= @start
                  AND h.Date <  @end
                  AND (h.ForLeaders = 1 OR h.ForOperators = 1)
                ORDER BY h.Date DESC";

            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

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
                    SectorId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    SectorName = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    ForLeaders = reader.GetInt32(7) == 1,
                    ForOperators = reader.GetInt32(8) == 1,
                    Description = reader.GetString(9),
                    AttachmentPath = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CategoryName = reader.IsDBNull(11) ? "" : reader.GetString(11)
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
