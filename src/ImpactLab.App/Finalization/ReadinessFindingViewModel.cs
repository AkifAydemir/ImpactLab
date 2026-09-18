using ImpactLab.Core.Finalization;

namespace ImpactLab.App.Finalization;

public sealed record ReadinessFindingViewModel(
    string Id,
    string Severity,
    string Area,
    string Message
)
{
    public static ReadinessFindingViewModel From(ReadinessFinding f) =>
        new(f.Id, f.Severity.ToString(), f.Area, f.Message);
}
