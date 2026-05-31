using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dapper;
using TeamOps.Data.Db;
using TeamOps.UI.Forms.Models;

namespace TeamOps.UI.Services
{
    public sealed class GBareruCapacityForecastService
    {
        private const int GBareruSectorId = 1;
        private const string EciiCode = "ECII";
        private const string BunkatsuCode = "BUNKATSU";
        private const string DcsCode = "DCS";

        private readonly SqliteConnectionFactory _factory;

        public GBareruCapacityForecastService(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public GBareruCapacityForecastDto BuildForecast(ProductionDashboardFilter filter, ProductionDashboardDto dashboard)
        {
            var watch = Stopwatch.StartNew();
            var result = new GBareruCapacityForecastDto();

            if (filter.SectorId > 0 && filter.SectorId != GBareruSectorId)
            {
                result.Message = "Previsao disponivel apenas para G-Bareru.";
                result.CalculationMs = watch.ElapsedMilliseconds;
                return result;
            }

            var gBareruAreas = dashboard.Areas
                .Where(area => area.SectorId == GBareruSectorId)
                .Where(area => filter.LocalId <= 0 || area.LocalId == filter.LocalId)
                .ToList();

            if (gBareruAreas.Count == 0)
            {
                result.Message = "Sem area G-Bareru no filtro atual.";
                result.CalculationMs = watch.ElapsedMilliseconds;
                return result;
            }

            var times = LoadProcedureTimes(filter.LocalId);
            if (!times.TryGetValue(EciiCode, out var ecii)
                || !times.TryGetValue(BunkatsuCode, out var bunkatsu)
                || !times.TryGetValue(DcsCode, out var dcs)
                || ecii <= 0
                || bunkatsu <= 0
                || dcs <= 0)
            {
                result.Message = "Sem dados suficientes: configure os tempos ECII, Bunkatsu e DCS.";
                result.CalculationMs = watch.ElapsedMilliseconds;
                return result;
            }

            result.EciiMinutes = ecii;
            result.BunkatsuMinutes = bunkatsu;
            result.DcsMinutes = dcs;
            result.Block1Minutes = ecii + bunkatsu;
            result.Block2Minutes = dcs;

            foreach (var area in gBareruAreas)
            {
                var areaForecast = BuildAreaForecast(area, ecii, bunkatsu, dcs);
                result.Areas.Add(areaForecast);
            }

            var availableAreas = result.Areas
                .Where(area => string.IsNullOrWhiteSpace(area.Message))
                .ToList();

            if (availableAreas.Count == 0)
            {
                result.Message = string.Join(" | ", result.Areas.Select(area => area.Message).Distinct());
                result.CalculationMs = watch.ElapsedMilliseconds;
                return result;
            }

            var weightedMinutes = availableAreas.Sum(area =>
            {
                var source = gBareruAreas.FirstOrDefault(item => item.LocalId == area.LocalId);
                return ResolveAvailableMinutes(source);
            });

            result.IsAvailable = true;
            result.PeopleCount = availableAreas.Sum(area => area.PeopleCount);
            result.AvailableMinutes = Math.Round(weightedMinutes, 1);
            result.ForecastCapacity = Math.Round(availableAreas.Sum(area => area.ForecastCapacity), 1);
            result.ForecastKadouritsuPercent = Math.Round(availableAreas.Average(area => area.ForecastKadouritsuPercent), 1);
            result.RealKadouritsuPercent = Math.Round(availableAreas.Average(area => area.RealKadouritsuPercent), 1);
            result.DifferencePercent = Math.Round(result.RealKadouritsuPercent - result.ForecastKadouritsuPercent, 1);

            var first = availableAreas[0];
            result.CycleMode = availableAreas.Select(area => area.CycleMode).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1
                ? first.CycleMode
                : "mixed";
            result.CycleMinutes = Math.Round(availableAreas.Average(area => area.CycleMinutes), 1);
            result.Bottleneck = ResolveBottleneck(result.Block1Minutes, result.Block2Minutes);
            result.CalculationMs = watch.ElapsedMilliseconds;
            return result;
        }

        private Dictionary<string, double> LoadProcedureTimes(int localId)
        {
            using var conn = _factory.CreateOpenConnection();
            ProductionSchemaMigrator.Ensure(conn);

            var rows = conn.Query<ProcedureTimeRow>(
                @"
                    SELECT
                        upper(trim(ProcedureCode)) AS ProcedureCode,
                        StandardMinutes,
                        COALESCE(LocalId, 0) AS LocalId
                    FROM ProductionProcedureTimes
                    WHERE SectorId = @SectorId
                      AND COALESCE(IsActive, 1) = 1
                      AND upper(trim(ProcedureCode)) IN @Codes
                      AND (COALESCE(LocalId, 0) = 0 OR COALESCE(LocalId, 0) = @LocalId)
                    ORDER BY COALESCE(LocalId, 0) DESC;",
                new
                {
                    SectorId = GBareruSectorId,
                    LocalId = localId,
                    Codes = new[] { EciiCode, BunkatsuCode, DcsCode }
                })
                .ToList();

            return rows
                .GroupBy(row => row.ProcedureCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().StandardMinutes, StringComparer.OrdinalIgnoreCase);
        }

        private static GBareruCapacityAreaForecastDto BuildAreaForecast(
            ProductionAreaSummaryDto area,
            double ecii,
            double bunkatsu,
            double dcs)
        {
            var peopleCount = area.ScheduledOperatorsPt
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var forecast = new GBareruCapacityAreaForecastDto
            {
                LocalId = area.LocalId,
                LocalNamePt = area.LocalNamePt,
                LocalNameJp = area.LocalNameJp,
                PeopleCount = peopleCount,
                RealKadouritsuPercent = area.ProductionPercent
            };

            if (peopleCount <= 0)
            {
                forecast.Message = "Sem alocacao Haidai.";
                return forecast;
            }

            if (peopleCount > 2)
            {
                forecast.Message = "Mais de 2 pessoas: regra nao assumida automaticamente.";
                return forecast;
            }

            var block1 = ecii + bunkatsu;
            var block2 = dcs;
            var totalWork = block1 + block2;
            var cycle = peopleCount == 1
                ? totalWork
                : Math.Max(block1, block2);
            var available = ResolveAvailableMinutes(area);

            forecast.CycleMode = peopleCount == 1 ? "1 pessoa" : "2 pessoas";
            forecast.CycleMinutes = Math.Round(cycle, 1);
            forecast.ForecastCapacity = cycle <= 0 ? 0 : Math.Round(available / cycle, 1);
            forecast.ForecastKadouritsuPercent = cycle <= 0
                ? 0
                : Math.Round(Math.Min(100d, (totalWork / (cycle * peopleCount)) * 100d), 1);

            return forecast;
        }

        private static double ResolveAvailableMinutes(ProductionAreaSummaryDto? area)
        {
            if (area == null)
            {
                return 0;
            }

            return area.MachineCount <= 0
                ? Math.Round(area.TotalMinutes, 1)
                : Math.Round(area.TotalMinutes / area.MachineCount, 1);
        }

        private static string ResolveBottleneck(double block1, double block2)
        {
            if (Math.Abs(block1 - block2) < 0.01)
            {
                return "Balanceado";
            }

            return block1 > block2 ? "ECII+Bunkatsu" : "DCS";
        }

        private sealed class ProcedureTimeRow
        {
            public string ProcedureCode { get; set; } = string.Empty;
            public double StandardMinutes { get; set; }
            public int LocalId { get; set; }
        }
    }
}
