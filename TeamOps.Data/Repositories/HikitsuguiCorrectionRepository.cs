using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class HikitsuguiCorrectionRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public HikitsuguiCorrectionRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        // ---------------------------------------------------------
        // INSERT
        // ---------------------------------------------------------
        public int Add(HikitsuguiCorrection c)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO HikitsuguiCorrections
                (HikitsuguiId, Date, CorrectorCodigoFJ, Correction)
                VALUES
                (@hikitsuguiId, @date, @corrector, @correction);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@hikitsuguiId", c.HikitsuguiId);
            cmd.Parameters.AddWithValue("@date", c.Date);
            cmd.Parameters.AddWithValue("@corrector", c.CorrectorCodigoFJ);
            cmd.Parameters.AddWithValue("@correction", c.Correction);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        // ---------------------------------------------------------
        // GET ALL CORRECTIONS FOR A HIKITSUGUI
        // ---------------------------------------------------------
        public List<HikitsuguiCorrection> GetByHikitsugui(int hikitsuguiId)
        {
            var list = new List<HikitsuguiCorrection>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, HikitsuguiId, Date, CorrectorCodigoFJ, Correction
                FROM HikitsuguiCorrections
                WHERE HikitsuguiId = @id
                ORDER BY Date ASC";

            cmd.Parameters.AddWithValue("@id", hikitsuguiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new HikitsuguiCorrection
                {
                    Id = reader.GetInt32(0),
                    HikitsuguiId = reader.GetInt32(1),
                    Date = reader.GetDateTime(2),
                    CorrectorCodigoFJ = reader.GetString(3),
                    Correction = reader.GetString(4)
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

            cmd.CommandText = "DELETE FROM HikitsuguiCorrections WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
