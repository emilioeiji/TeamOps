using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class CLRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public CLRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(CL cl)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO CL
                (SetorId, CategoriaId, PrioridadeId, Titulo, NomeArquivo,
                 DataEmissao, DataRetornoHiru, DataRetornoYakin, AutorCodigoFJ)
                VALUES
                (@setor, @cat, @prio, @titulo, @arquivo,
                 @emissao, @hiru, @yakin, @autor);

                SELECT last_insert_rowid();
            ";

            cmd.Parameters.AddWithValue("@setor", cl.SetorId);
            cmd.Parameters.AddWithValue("@cat", cl.CategoriaId);
            cmd.Parameters.AddWithValue("@prio", cl.PrioridadeId);
            cmd.Parameters.AddWithValue("@titulo", cl.Titulo);
            cmd.Parameters.AddWithValue("@arquivo", cl.NomeArquivo);
            cmd.Parameters.AddWithValue("@emissao", cl.DataEmissao.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@hiru",
                cl.DataRetornoHiru == null ? DBNull.Value : cl.DataRetornoHiru.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@yakin",
                cl.DataRetornoYakin == null ? DBNull.Value : cl.DataRetornoYakin.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@autor", cl.AutorCodigoFJ);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<CL> GetAll()
        {
            var list = new List<CL>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT * FROM CL ORDER BY Id DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CL
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
            cmd.CommandText = "SELECT IFNULL(MAX(Id), 0) FROM CL";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
