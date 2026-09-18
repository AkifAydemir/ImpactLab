namespace ImpactLab.Core.Continuum.Meshing.External;

public sealed record ExternalProcessMesherSettings(
    string ExecutablePath,
    string ArgumentsTemplate = "{input} {output}",
    TimeSpan Timeout = default,
    ExternalMesherFormat OutputFormat = ExternalMesherFormat.NeutralJson,
    bool KeepArtifactsOnSuccess = false
)
{
    public TimeSpan EffectiveTimeout => Timeout == default ? TimeSpan.FromMinutes(2) : Timeout;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ExecutablePath))
            throw new InvalidOperationException("External mesher executable path is required.");
        if (
            !ArgumentsTemplate.Contains("{input}", StringComparison.Ordinal)
            || !ArgumentsTemplate.Contains("{output}", StringComparison.Ordinal)
        )
            throw new InvalidOperationException(
                "ArgumentsTemplate must contain {input} and {output} placeholders."
            );
        if (EffectiveTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException("External mesher timeout must be positive.");
    }
}
