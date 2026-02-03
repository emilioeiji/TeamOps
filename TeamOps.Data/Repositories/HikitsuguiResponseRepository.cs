using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class HikitsuguiResponseRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public HikitsuguiResponseRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        // ---------------------------------------------------------
        // INSERT
        // ---------------------------------------------------------
        public int Add(HikitsuguiResponse r)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO HikitsuguiResponses
                (HikitsuguiId, Date, ResponderCodigoFJ, Message)
                VALUES
                (@hikitsuguiId, @date, @responder, @msg);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@hikitsuguiId", r.HikitsuguiId);
            cmd.Parameters.AddWithValue("@date", r.Date);
            cmd.Parameters.AddWithValue("@responder", r.ResponderCodigoFJ);
            cmd.Parameters.AddWithValue("@msg", r.Message);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        // ---------------------------------------------------------
        // GET ALL RESPONSES FOR A HIKITSUGUI
        // ---------------------------------------------------------
        public List<HikitsuguiResponse> GetByHikitsugui(int hikitsuguiId)
        {
            var list = new List<HikitsuguiResponse>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, HikitsuguiId, Date, ResponderCodigoFJ, Message
                FROM HikitsuguiResponses
                WHERE HikitsuguiId = @id
                ORDER BY Date ASC";

            cmd.Parameters.AddWithValue("@id", hikitsuguiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new HikitsuguiResponse
                {
                    Id = reader.GetInt32(0),
                    HikitsuguiId = reader.GetInt32(1),
                    Date = reader.GetDateTime(2),
                    ResponderCodigoFJ = reader.GetString(3),
                    Message = reader.GetString(4)
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

            cmd.CommandText = "DELETE FROM HikitsuguiResponses WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
