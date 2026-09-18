using System.Diagnostics;

namespace ImpactLab.Core.Geometry.Exchange;

public sealed class ExternalCadConverter
{
    public async Task<string> ConvertAsync(CadExchangeRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.ConverterExecutable))
            throw new InvalidOperationException("No external CAD converter configured.");
        var psi = new ProcessStartInfo(r.ConverterExecutable)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("--input");
        psi.ArgumentList.Add(Path.GetFullPath(r.SourcePath));
        psi.ArgumentList.Add("--output");
        psi.ArgumentList.Add(Path.GetFullPath(r.OutputMeshPath));
        psi.ArgumentList.Add("--chord");
        psi.ArgumentList.Add(
            r.ChordToleranceMeters.ToString(System.Globalization.CultureInfo.InvariantCulture)
        );
        using var p =
            Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start CAD converter.");
        await p.WaitForExitAsync(ct);
        if (p.ExitCode != 0)
            throw new InvalidOperationException(await p.StandardError.ReadToEndAsync(ct));
        if (!File.Exists(r.OutputMeshPath))
            throw new IOException("CAD converter did not produce output mesh.");
        return r.OutputMeshPath;
    }
}
