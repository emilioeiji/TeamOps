using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class HikitsuguiAttachmentRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public HikitsuguiAttachmentRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(HikitsuguiAttachment a)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            INSERT INTO HikitsuguiAttachments (HikitsuguiId, FileName, FilePath)
            VALUES (@id, @name, @path)";

            cmd.Parameters.AddWithValue("@id", a.HikitsuguiId);
            cmd.Parameters.AddWithValue("@name", a.FileName);
            cmd.Parameters.AddWithValue("@path", a.FilePath);

            cmd.ExecuteNonQuery();
        }

        public List<HikitsuguiAttachment> GetByHikitsugui(int hikitsuguiId)
        {
            var list = new List<HikitsuguiAttachment>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            SELECT Id, HikitsuguiId, FileName, FilePath, CreatedAt
            FROM HikitsuguiAttachments
            WHERE HikitsuguiId = @id";

            cmd.Parameters.AddWithValue("@id", hikitsuguiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new HikitsuguiAttachment
                {
                    Id = reader.GetInt32(0),
                    HikitsuguiId = reader.GetInt32(1),
                    FileName = reader.GetString(2),
                    FilePath = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return list;
        }
    }
}