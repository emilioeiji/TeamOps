using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public sealed class OperatorPresenceRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public OperatorPresenceRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public void RegisterPresence(string codigoFJ, int sectorId, int localId, int shiftId, DateTime date)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO OperatorPresence 
                (CodigoFJ, SectorId, LocalId, ShiftId, Date)
                VALUES (@fj, @sector, @local, @shift, @date);
            ";

            cmd.Parameters.AddWithValue("@fj", codigoFJ);
            cmd.Parameters.AddWithValue("@sector", sectorId);
            cmd.Parameters.AddWithValue("@local", localId);
            cmd.Parameters.AddWithValue("@shift", shiftId);
            cmd.Parameters.AddWithValue("@date", date);

            cmd.ExecuteNonQuery();
        }

        public List<OperatorPresence> GetLatestByDateSectorShift(DateTime date, int sectorId, int shiftId)
        {
            var list = new List<OperatorPresence>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        SELECT 
            CodigoFJ,
            SectorId,
            LocalId,
            ShiftId,
            MAX(Date) AS LastPresence
        FROM OperatorPresence
        WHERE DATE(Date) = DATE(@date)
          AND SectorId = @sector
          AND ShiftId = @shift
        GROUP BY CodigoFJ;
    ";

            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@sector", sectorId);
            cmd.Parameters.AddWithValue("@shift", shiftId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OperatorPresence
                {
                    CodigoFJ = reader.GetString(0),
                    SectorId = reader.GetInt32(1),
                    LocalId = reader.GetInt32(2),
                    ShiftId = reader.GetInt32(3),
                    Date = reader.GetDateTime(4)
                });
            }

            return list;
        }

        public List<OperatorPresence> GetByDateSectorShift(DateTime date, int sectorId, int shiftId)
        {
            var list = new List<OperatorPresence>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT Id, CodigoFJ, SectorId, LocalId, ShiftId, Date
                FROM OperatorPresence
                WHERE DATE(Date) = DATE(@date)
                AND SectorId = @sector
                AND ShiftId = @shift
                ORDER BY Date DESC;
            ";

            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@sector", sectorId);
            cmd.Parameters.AddWithValue("@shift", shiftId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OperatorPresence
                {
                    Id = reader.GetInt32(0),
                    CodigoFJ = reader.GetString(1),
                    SectorId = reader.GetInt32(2),
                    LocalId = reader.GetInt32(3),
                    ShiftId = reader.GetInt32(4),
                    Date = reader.GetDateTime(5)
                });
            }

            return list;
        }
    }
}
