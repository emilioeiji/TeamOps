// Project: TeamOps.Data
// File: Repositories/CategoryRepository.cs

using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class CategoryRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public CategoryRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(Category c)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Categories (NamePt, NameJp)
                VALUES (@pt, @jp);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@pt", c.NamePt);
            cmd.Parameters.AddWithValue("@jp", c.NameJp);
            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<Category> GetAll()
        {
            var list = new List<Category>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Categories ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                });
            }
            return list;
        }

        public Category? GetById(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, NamePt, NameJp FROM Categories WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Category
                {
                    Id = reader.GetInt32(0),
                    NamePt = reader.GetString(1),
                    NameJp = reader.GetString(2)
                };
            }
            return null;
        }

        public void Update(Category c)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Categories
                SET NamePt = @pt, NameJp = @jp
                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@pt", c.NamePt);
            cmd.Parameters.AddWithValue("@jp", c.NameJp);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Categories WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
