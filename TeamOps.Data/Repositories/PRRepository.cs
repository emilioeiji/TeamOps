using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class PRRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public PRRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(PR pr)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO PR
                (SetorId, CategoriaId, PrioridadeId, Titulo, NomeArquivo,
                 DataEmissao, DataRetornoHiru, DataRetornoYakin, AutorCodigoFJ)
                VALUES
                (@setor, @cat, @prio, @titulo, @arquivo,
                 @emissao, @hiru, @yakin, @autor);

                SELECT last_insert_rowid();
            ";

            cmd.Parameters.AddWithValue("@setor", pr.SetorId);
            cmd.Parameters.AddWithValue("@cat", pr.CategoriaId);
            cmd.Parameters.AddWithValue("@prio", pr.PrioridadeId);
            cmd.Parameters.AddWithValue("@titulo", pr.Titulo);
            cmd.Parameters.AddWithValue("@arquivo", pr.NomeArquivo);
            cmd.Parameters.AddWithValue("@emissao", pr.DataEmissao.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@hiru",
                pr.DataRetornoHiru == null ? DBNull.Value : pr.DataRetornoHiru.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@yakin",
                pr.DataRetornoYakin == null ? DBNull.Value : pr.DataRetornoYakin.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@autor", pr.AutorCodigoFJ);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<PR> GetAll()
        {
            var list = new List<PR>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT * FROM PR ORDER BY Id DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PR
                {
                    Id = reader.GetInt32(0),
                    SetorId = reader.GetInt32(1),
                    CategoriaId = reader.GetInt32(2),
                    PrioridadeId = reader.GetInt32(3),
                    Titulo = reader.GetString(4),
                    NomeArquivo = reader.GetString(5),
                    DataEmissao = DateTime.Parse(reader.GetString(6)),
                    DataRetornoHiru = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    DataRetornoYakin = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                    AutorCodigoFJ = reader.GetString(9),
                    CreatedAt = DateTime.Parse(reader.GetString(10))
                });
            }

            return list;
        }
        public int GetLastId()
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IFNULL(MAX(Id), 0) FROM PR";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
