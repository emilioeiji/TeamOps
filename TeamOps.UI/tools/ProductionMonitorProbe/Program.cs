using Dapper;
using TeamOps.Config;
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Services;
using TeamOps.UI.Forms.Models;

var settings = new DbSettings();
var factory = new SqliteConnectionFactory(settings);
var machineRepository = new ProductionMachineRepository(factory);
var eventRepository = new ProductionEventRepository(factory);
var importer = new ProductionFileImporter(factory, machineRepository, eventRepository);
var analytics = new ProductionAnalyticsService(factory);

var command = args.Length > 0
    ? args[0].Trim().ToLowerInvariant()
    : "demo";

switch (command)
{
    case "import":
        RunImport();
        break;

    case "dashboard":
        ShowDashboards();
        break;

    case "demo":
    default:
        RunImport();
        Console.WriteLine();
        ShowDashboards();
        break;
}

void RunImport()
{
    var result = importer.ImportLatest();

    Console.WriteLine("=== IMPORT RESULT ===");
    Console.WriteLine($"FilesRead={result.FilesRead}");
    Console.WriteLine($"LinesRead={result.LinesRead}");
    Console.WriteLine($"Imported={result.Imported}");
    Console.WriteLine($"Ignored={result.Ignored}");
    Console.WriteLine($"MachinesCreated={result.MachinesCreated}");
    Console.WriteLine($"BatchExecuted={result.BatchExecuted}");
    Console.WriteLine($"BatchMessage={result.BatchMessage}");

    if (result.Errors.Count > 0)
    {
        Console.WriteLine("Errors:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($" - {error}");
        }
    }
}

void ShowDashboards()
{
    using var conn = factory.CreateOpenConnection();
    ProductionSchemaMigrator.Ensure(conn);

    var shifts = conn.Query<ShiftRow>(
        @"
            SELECT
                Id,
                COALESCE(NamePt, '') AS NamePt,
                COALESCE(NULLIF(NameJp, ''), NamePt, '') AS NameJp
            FROM Shifts
            ORDER BY Id;"
    ).ToList();

    var dayShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "hiru", "day", "dia"));
    var nightShift = shifts.FirstOrDefault(shift => MatchesShift(shift, "yakin", "night", "noite", "夜"));

    if (dayShift != null)
    {
        ShowDashboard("HIRUKIN", new DateTime(2026, 4, 29), dayShift.Id);
    }

    if (nightShift != null)
    {
        Console.WriteLine();
        ShowDashboard("YAKIN", new DateTime(2026, 4, 28), nightShift.Id);
    }

    if (dayShift == null && nightShift == null)
    {
        var fallbackShift = shifts.FirstOrDefault();
        if (fallbackShift == null)
        {
            Console.WriteLine("Nenhum turno encontrado para o probe.");
            return;
        }

        ShowDashboard("DEFAULT", new DateTime(2026, 4, 29), fallbackShift.Id);
    }
}

void ShowDashboard(string label, DateTime date, int shiftId)
{
    var dashboard = analytics.BuildDashboard(new ProductionDashboardFilter
    {
        Date = date,
        ShiftId = shiftId,
        MachineCode = "E01"
    });

    Console.WriteLine($"=== DASHBOARD {label} ===");
    Console.WriteLine($"Date={date:yyyy-MM-dd}");
    Console.WriteLine($"Period={dashboard.Period.Start:yyyy-MM-dd HH:mm:ss} -> {dashboard.Period.End:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"Kadouritsu={dashboard.ProductionPercent:F1}");
    Console.WriteLine($"MachinesRunning={dashboard.MachinesRunning}");
    Console.WriteLine($"MachinesStopped={dashboard.MachinesStopped}");
    Console.WriteLine($"ErrorMinutes={dashboard.ErrorMinutes:F1}");
    Console.WriteLine($"InactiveMinutes={dashboard.InactiveMinutes:F1}");
    Console.WriteLine($"Areas={dashboard.Areas.Count}");
    Console.WriteLine($"OperatorRanking={dashboard.OperatorRanking.Count}");

    foreach (var machine in dashboard.Machines)
    {
        Console.WriteLine($"Machine={machine.MachineCode} Status={machine.StatusText} Kadouritsu={machine.ProductionPercent:F1}% Running={machine.RunningMinutes:F1} Stop={machine.StoppedMinutes:F1} Error={machine.ErrorMinutes:F1} Inactive={machine.InactiveMinutes:F1} Updated={machine.LastUpdate:yyyy-MM-dd HH:mm:ss}");
    }
}

static bool MatchesShift(ShiftRow shift, params string[] keywords)
{
    var values = new[] { shift.NamePt, shift.NameJp }
        .Where(value => !string.IsNullOrWhiteSpace(value));

    foreach (var value in values)
    {
        foreach (var keyword in keywords)
        {
            if (value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
    }

    return false;
}

sealed class ShiftRow
{
    public int Id { get; set; }
    public string NamePt { get; set; } = string.Empty;
    public string NameJp { get; set; } = string.Empty;
}
