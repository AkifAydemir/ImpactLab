namespace ImpactLab.Core.Runs;

public sealed class RunArchiveIndex
{
    public int FormatVersion { get; set; } = 1;
    public List<ArchivedRunRecord> Runs { get; set; } = [];
}
