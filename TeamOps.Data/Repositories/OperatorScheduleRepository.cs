using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamOps.Core.Entities;
using TeamOps.Data.Db;

namespace TeamOps.Data.Repositories
{
    public class OperatorScheduleRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public OperatorScheduleRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(OperatorSchedule schedule)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO OperatorSchedule
                (CodigoFJ, SectorId, LocalId, ShiftId, ScheduleDate)
                VALUES (@CodigoFJ, @SectorId, @LocalId, @ShiftId, @ScheduleDate);
            ";

            cmd.Parameters.AddWithValue("@CodigoFJ", schedule.CodigoFJ);
            cmd.Parameters.AddWithValue("@SectorId", schedule.SectorId);
            cmd.Parameters.AddWithValue("@LocalId", schedule.LocalId);
            cmd.Parameters.AddWithValue("@ShiftId", schedule.ShiftId);
            cmd.Parameters.AddWithValue("@ScheduleDate", schedule.ScheduleDate.ToString("yyyy-MM-dd"));

            cmd.ExecuteNonQuery();
        }

        public void DeleteByDateShift(DateTime date, int shiftId)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                DELETE FROM OperatorSchedule
                WHERE date(ScheduleDate) = date(@Date)
                AND ShiftId = @ShiftId;
            ";

            cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ShiftId", shiftId);

            cmd.ExecuteNonQuery();
        }

        public void DeleteByDateShiftSector(DateTime date, int shiftId, int sectorId)
        {
            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                DELETE FROM OperatorSchedule
                WHERE date(ScheduleDate) = date(@Date)
                  AND ShiftId = @ShiftId
                  AND SectorId = @SectorId;
            ";

            cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ShiftId", shiftId);
            cmd.Parameters.AddWithValue("@SectorId", sectorId);

            cmd.ExecuteNonQuery();
        }

        public List<OperatorSchedule> GetByDateShift(DateTime date, int shiftId)
        {
            var list = new List<OperatorSchedule>();

            using var conn = _factory.CreateOpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                    Id,
                    CodigoFJ,
                    SectorId,
                    LocalId,
                    ShiftId,
                    ScheduleDate,
                    ImportedAt
                FROM OperatorSchedule
                WHERE date(ScheduleDate) = date(@Date)
                AND ShiftId = @ShiftId;
            ";

            cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ShiftId", shiftId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OperatorSchedule
                {
                    Id = reader.GetInt32(0),
                    CodigoFJ = reader.GetString(1),
                    SectorId = reader.GetInt32(2),
                    LocalId = reader.GetInt32(3),
                    ShiftId = reader.GetInt32(4),
                    ScheduleDate = DateTime.Parse(reader.GetString(5)),
                    ImportedAt = DateTime.Parse(reader.GetString(6))
                });
            }

            return list;
        }
    }
}
