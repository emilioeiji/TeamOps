// Project: TeamOps.Data
// File: Repositories/GroupLeaderRepository.cs
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;
using BCrypt.Net;

namespace TeamOps.Data.Repositories
{
    public sealed class GroupLeaderRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public GroupLeaderRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(GroupLeader gl, string plainPassword)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO GroupLeaders (Name, Login, PasswordHash)
                VALUES (@n, @l, @p);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@n", gl.Name);
            cmd.Parameters.AddWithValue("@l", gl.Login);
            cmd.Parameters.AddWithValue("@p", hash);
            var id = (long)cmd.ExecuteScalar()!;
            return (int)id;
        }

        public GroupLeader? GetByLogin(string login)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Login, PasswordHash, CreatedAt FROM GroupLeaders WHERE Login = @l";
            cmd.Parameters.AddWithValue("@l", login);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new GroupLeader
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Login = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        public bool Authenticate(string login, string plainPassword, out GroupLeader? user)
        {
            user = GetByLogin(login);
            if (user is null) return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        }

        public List<GroupLeader> GetAll()
        {
            var list = new List<GroupLeader>();
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Login, PasswordHash, CreatedAt FROM GroupLeaders ORDER BY Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new GroupLeader
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Login = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }
            return list;
        }

        public void UpdateBasicInfo(GroupLeader gl)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE GroupLeaders
                SET Name = @n, Login = @l
                WHERE Id = @id
            ";
            cmd.Parameters.AddWithValue("@n", gl.Name);
            cmd.Parameters.AddWithValue("@l", gl.Login);
            cmd.Parameters.AddWithValue("@id", gl.Id);
            cmd.ExecuteNonQuery();
        }

        public void UpdatePassword(int id, string newPlainPassword)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword);

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE GroupLeaders
                SET PasswordHash = @p
                WHERE Id = @id
            ";
            cmd.Parameters.AddWithValue("@p", hash);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM GroupLeaders WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public GroupLeader? GetByCodigoFJ(string codigoFJ)
        {
            // aqui CodigoFJ == Login
            return GetByLogin(codigoFJ);
        }
    }
}
