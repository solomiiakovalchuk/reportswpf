namespace Reports.Core.Services;

public sealed class AppSettings
{
    public string Classification { get; set; } = "ВІДКРИТА ІНФОРМАЦІЯ";

    public string Addressee { get; set; } = "Командиру\nвійськової частини А0000";

    public string Resolution { get; set; } =
        "НС А та ППО\nПідготувати документи на списання встановленим порядком";

    public string ResolutionRank { get; set; } = "полковник";
    public string ResolutionName { get; set; } = "І. ПРІЗВИЩЕНКО";

    public string SubunitText { get; set; } =
        "взводом перехоплювачів безпілотних літальних апаратів зенітної батареї військової частини А0000";

    public string OrderDefault { get; set; } = "командира військової частини від 01.01.2026 №1т";

    public string DefaultSignerEmail { get; set; } = string.Empty;

    public string SpreadsheetId { get; set; } = string.Empty;

    public string ResponsesSheet { get; set; } = "Відповіді";

    public string DirectoriesSheet { get; set; } = "Довідники";
    public string PeopleSheet { get; set; } = "Люди";

    public string OutputFolder { get; set; } = string.Empty;

    // Google account that is allowed to reset the admin password
    public string AdminGoogleEmail { get; set; } = string.Empty;

    public string FontName { get; set; } = "Times New Roman";
    public int FontSize { get; set; } = 14;
}
