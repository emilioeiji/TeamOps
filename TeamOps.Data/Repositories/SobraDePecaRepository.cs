using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class SobraDePecaRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public SobraDePecaRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Add(SobraDePeca s)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO SobraDePeca 
                (Data, TurnoId, Lote, OperadorId, Tanjuu, PesoGramas, Quantidade, MachineId, ShainId, Observacao, Lider, CreatedAt, Item)
                VALUES 
                (@data, @turno, @lote, @op, @tanjuu, @peso, @qtd, @equipId, @shain, @obs, @lider, @created, @item);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("@data", s.Data.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@turno", s.TurnoId);
            cmd.Parameters.AddWithValue("@lote", s.Lote);
            cmd.Parameters.AddWithValue("@op", s.OperadorId);
            cmd.Parameters.AddWithValue("@tanjuu", s.Tanjuu);
            cmd.Parameters.AddWithValue("@peso", s.PesoGramas);
            cmd.Parameters.AddWithValue("@qtd", s.Quantidade);
            cmd.Parameters.AddWithValue("@equipId", s.MachineId);
            cmd.Parameters.AddWithValue("@shain", s.ShainId);
            cmd.Parameters.AddWithValue("@obs", (object?)s.Observacao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lider", s.Lider);
            cmd.Parameters.AddWithValue("@created", s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@item", s.Item);

            return (int)(long)cmd.ExecuteScalar()!;
        }

        public List<SobraDePeca> GetAll()
        {
            var list = new List<SobraDePeca>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, Data, TurnoId, Lote, OperadorId, Tanjuu, PesoGramas, Quantidade, MachineId, ShainId, Observacao, Lider, CreatedAt, Item
                FROM SobraDePeca
                ORDER BY Id DESC
                LIMIT 100";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SobraDePeca
                {
                    Id = reader.GetInt32(0),
                    Data = reader.GetDateTime(1),
                    TurnoId = reader.GetInt32(2),
                    Lote = reader.GetString(3),
                    OperadorId = reader.GetString(4),
                    Tanjuu = reader.GetDecimal(5),
                    PesoGramas = reader.GetDecimal(6),
                    Quantidade = reader.GetDecimal(7),
                    MachineId = reader.GetInt32(8),
                    ShainId = reader.GetInt32(9),
                    Observacao = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Lider = reader.GetString(11),
                    CreatedAt = reader.GetDateTime(12),
                    Item = reader.GetString(13)
                });
            }

            return list;
        }
    }
}