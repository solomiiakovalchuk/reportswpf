using Reports.Core.Aggregation;
using Reports.Core.Domain;

namespace Reports.Core.Abstractions;

public interface IAuthService
{
    AppUser? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    Task<bool> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default);
    Task ResetPasswordAsync(string username, string newPassword, CancellationToken ct = default);
    void Logout();
}

public interface IReportRenderer
{
    ReportFormat Format { get; }

    Task<string> RenderAsync(ReportContent content, string outputPath, CancellationToken ct = default);
}

public sealed class RawResponseRow
{
    public required string RowKey { get; init; }

    public DateTime SubmittedAt { get; init; }

    public DateTime? Date { get; init; }

    public string OperatorEmail { get; init; } = string.Empty;
    public string SettlementName { get; init; } = string.Empty;
    public string UavTypeName { get; init; } = string.Empty;
    public string TargetTypeName { get; init; } = string.Empty;
    public int UavCount { get; init; }

    public IReadOnlyList<string> MunitionNames { get; init; } = Array.Empty<string>();

    public string StatusMarker { get; init; } = string.Empty;
}

public sealed class RawDirectories
{
    public IReadOnlyList<(string Short, string Full)> Uav { get; init; } = Array.Empty<(string, string)>();
    public IReadOnlyList<(string Short, string Full)> Targets { get; init; } = Array.Empty<(string, string)>();
    public IReadOnlyList<(string Short, string Full)> Munitions { get; init; } = Array.Empty<(string, string)>();
    public IReadOnlyList<(string Short, string Region)> Settlements { get; init; } = Array.Empty<(string, string)>();
    public IReadOnlyList<Person> People { get; init; } = Array.Empty<Person>();
}

public interface IResponseSource
{
    string Title { get; }

    Task<RawDirectories> LoadDirectoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RawResponseRow>> LoadResponsesAsync(CancellationToken ct = default);
}
